using System.Diagnostics;
using System.Globalization;
using System.Text;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Threading;
using AtomUI.DataGridPerformanceSupport;

namespace AtomUI.DataGridPerformance;

internal static class Program
{
    private const int DefaultCount = 60;

    [STAThread]
    public static int Main(string[] args)
    {
        var options = Options.Parse(args);
        AppBuilder.Configure<PerformanceApplication>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .UseAtomUI(builder =>
                  {
                      builder.UseDesktopControls();
                      builder.UseDesktopDataGrid();
                  })
                  .SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime());

        if (options.VerifyStates)
        {
            return DataGridStateVerifier.Run() ? 0 : 1;
        }

        var scenarios = CreateScenarios(options.Scenario);
        foreach (var scenario in scenarios)
        {
            RunWarmup(scenario, options.Warmup);
        }

        var results = scenarios.Select(scenario => Measure(scenario, options.Count)).ToArray();
        var markdown = RenderMarkdown(results, options.Count, options.Warmup);
        Console.WriteLine(markdown);

        if (!string.IsNullOrWhiteSpace(options.MarkdownPath))
        {
            var fullPath = Path.GetFullPath(options.MarkdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, markdown, new UTF8Encoding(false));
            Console.WriteLine($"Wrote markdown baseline: {fullPath}");
        }

        return 0;
    }

    private static IReadOnlyList<Scenario> CreateScenarios(string? requested)
    {
        Scenario[] all =
        [
            new("DataGrid.Basic.8x4", () => Wrap(CreateBasicGrid(8, 4))),
            new("DataGrid.Virtualized.1000x8", () => Wrap(CreateBasicGrid(1_000, 8))),
            new("DataGrid.RowDetails.1000", () => Wrap(CreateRowDetailsGrid())),
            new("DataGrid.GalleryShape", CreateGalleryShape)
        ];

        if (string.IsNullOrWhiteSpace(requested))
        {
            return all;
        }

        var selected = all.Where(scenario =>
            scenario.Name.Equals(requested, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (selected.Length == 0)
        {
            throw new ArgumentException($"Unknown scenario '{requested}'.", nameof(requested));
        }
        return selected;
    }

    private static void RunWarmup(Scenario scenario, int count)
    {
        for (var i = 0; i < count; i++)
        {
            RealizeAndClose(scenario.Create());
        }
    }

    private static Result Measure(Scenario scenario, int count)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var elapsed = new double[count];
        var uiThreadAllocated = new long[count];
        var processAllocated = new long[count];
        var processCreateAllocated = new long[count];
        var processRealizeAllocated = new long[count];
        var processReadyAllocated = new long[count];
        var processCloseAllocated = new long[count];
        for (var i = 0; i < count; i++)
        {
            var beforeUiThreadBytes = GC.GetAllocatedBytesForCurrentThread();
            var beforeProcessBytes = GC.GetTotalAllocatedBytes(precise: true);
            var start = Stopwatch.GetTimestamp();
            var instance = scenario.Create();
            var afterCreateBytes = GC.GetTotalAllocatedBytes(precise: true);
            var realization = RealizeAndClose(instance);
            elapsed[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            uiThreadAllocated[i] = GC.GetAllocatedBytesForCurrentThread() - beforeUiThreadBytes;
            var afterProcessBytes = GC.GetTotalAllocatedBytes(precise: true);
            processAllocated[i] = afterProcessBytes - beforeProcessBytes;
            processCreateAllocated[i] = afterCreateBytes - beforeProcessBytes;
            processRealizeAllocated[i] = afterProcessBytes - afterCreateBytes;
            processReadyAllocated[i] = realization.ReadyBytes;
            processCloseAllocated[i] = realization.CloseBytes;
        }

        Array.Sort(elapsed);
        Array.Sort(uiThreadAllocated);
        Array.Sort(processAllocated);
        Array.Sort(processCreateAllocated);
        Array.Sort(processRealizeAllocated);
        Array.Sort(processReadyAllocated);
        Array.Sort(processCloseAllocated);
        return new Result(
            scenario.Name,
            elapsed.Average(),
            Percentile(elapsed, 0.50),
            Percentile(elapsed, 0.95),
            uiThreadAllocated.Average(),
            Percentile(uiThreadAllocated, 0.50),
            Percentile(uiThreadAllocated, 0.95),
            processAllocated.Average(),
            Percentile(processAllocated, 0.50),
            Percentile(processAllocated, 0.95),
            processCreateAllocated.Average(),
            processRealizeAllocated.Average(),
            processReadyAllocated.Average(),
            processCloseAllocated.Average());
    }

    private static RealizationAllocation RealizeAndClose(ScenarioInstance instance)
    {
        var beforeReadyBytes = GC.GetTotalAllocatedBytes(precise: true);
        var window = new Avalonia.Controls.Window
        {
            Width = 900,
            Height = 640,
            Content = instance.Root,
            ShowInTaskbar = false
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        WaitForSettledDataGrids(instance.Grids);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        WaitForSettledDataGrids(instance.Grids);
        var afterReadyBytes = GC.GetTotalAllocatedBytes(precise: true);
        window.Close();
        Dispatcher.UIThread.RunJobs();
        var afterCloseBytes = GC.GetTotalAllocatedBytes(precise: true);
        return new RealizationAllocation(
            afterReadyBytes - beforeReadyBytes,
            afterCloseBytes - afterReadyBytes);
    }

    private static void WaitForSettledDataGrids(IReadOnlyList<DataGrid> grids)
    {
        var timeout = Stopwatch.StartNew();
        while (!AreSettled(grids))
        {
            Dispatcher.UIThread.RunJobs();
            if (timeout.Elapsed > TimeSpan.FromSeconds(10))
            {
                throw new TimeoutException("DataGrid did not reach a terminal load state.");
            }
            Thread.Yield();
        }
        for (var index = 0; index < grids.Count; index++)
        {
            if (grids[index].LoadState == DataGridLoadState.Error)
            {
                throw new InvalidOperationException(
                    "DataGrid failed during performance realization.",
                    grids[index].LoadError);
            }
        }

        static bool AreSettled(IReadOnlyList<DataGrid> candidates)
        {
            for (var index = 0; index < candidates.Count; index++)
            {
                var grid = candidates[index];
                if (grid.LoadState is DataGridLoadState.Idle or
                        DataGridLoadState.Loading or
                        DataGridLoadState.Refreshing ||
                    grid.HasPendingRangeViewport ||
                    grid.RangeActiveRequestCount != 0 ||
                    grid.RangeInFlightBlockCount != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }

    private static double Percentile(double[] sorted, double percentile)
    {
        var index = (int)Math.Ceiling(sorted.Length * percentile) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
    }

    private static long Percentile(long[] sorted, double percentile)
    {
        var index = (int)Math.Ceiling(sorted.Length * percentile) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
    }

    private static string RenderMarkdown(IEnumerable<Result> results, int count, int warmup)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# DataGrid control performance");
        builder.AppendLine();
        builder.AppendLine($"Iterations: {count}; warmup: {warmup}; operation: create + first layout + close.");
        builder.AppendLine();
        builder.AppendLine("| Scenario | Mean ms | Median ms | P95 ms | Mean UI-thread bytes | Median UI-thread bytes | P95 UI-thread bytes | Mean process bytes | Median process bytes | P95 process bytes | Mean create bytes | Mean realize + close bytes | Mean ready bytes | Mean close bytes |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var result in results)
        {
            builder.AppendLine(CultureInfo.InvariantCulture,
                $"| {result.Name} | {result.MeanMilliseconds:F3} | {result.MedianMilliseconds:F3} | {result.P95Milliseconds:F3} | {result.MeanUiThreadAllocatedBytes:F0} | {result.MedianUiThreadAllocatedBytes} | {result.P95UiThreadAllocatedBytes} | {result.MeanProcessAllocatedBytes:F0} | {result.MedianProcessAllocatedBytes} | {result.P95ProcessAllocatedBytes} | {result.MeanProcessCreateAllocatedBytes:F0} | {result.MeanProcessRealizeAllocatedBytes:F0} | {result.MeanProcessReadyAllocatedBytes:F0} | {result.MeanProcessCloseAllocatedBytes:F0} |");
        }
        return builder.ToString();
    }

    private static DataGrid CreateBasicGrid(int rowCount, int columnCount)
    {
        var grid = CreateGridShell(CreateRows(rowCount));
        for (var i = 0; i < columnCount; i++)
        {
            var field = GetField(i);
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = $"Column {i + 1}",
                FieldId = field.Id,
                Binding = new Binding(field.Path)
            });
        }
        return grid;
    }

    private static DataGrid CreateRowDetailsGrid()
    {
        var grid = CreateGridShell(CreateRows(1_000));
        grid.Columns.Add(new DataGridDetailExpanderColumn());
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Name",
            FieldId = NameField,
            Binding = new Binding(nameof(Row.Name))
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Age",
            FieldId = AgeField,
            Binding = new Binding(nameof(Row.Age))
        });
        grid.RowDetailsTemplate = new FuncDataTemplate<Row>((row, _) => new Avalonia.Controls.TextBlock
        {
            Text = row?.Address,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });
        return grid;
    }

    private static ScenarioInstance CreateGalleryShape()
    {
        var panel = new StackPanel { Spacing = 8 };
        var basic = CreateBasicGrid(8, 4);
        var wide = CreateBasicGrid(20, 8);
        var details = CreateRowDetailsGrid();
        panel.Children.Add(basic);
        panel.Children.Add(wide);
        panel.Children.Add(details);
        return new ScenarioInstance(panel, [basic, wide, details]);
    }

    private static DataGrid CreateGridShell(IReadOnlyList<Row> rows)
    {
        var source = DataGridLocalSource.Create(rows, RowDescriptor);
        var grid = new DataGrid
        {
            Width = 720,
            Height = 220,
            ItemsSource = source
        };
        return grid;
    }

    private static Row[] CreateRows(int count)
    {
        var rows = new Row[count];
        for (var index = 0; index < count; index++)
        {
            rows[index] = new Row
            {
                Id = index,
                Name = index % 2 == 0 ? "Joe" : "Jim",
                Age = 20 + index % 30,
                Address = index % 3 == 0 ? "London" : "New York",
                Score = 60 + index % 40
            };
        }
        return rows;
    }

    private static (DataGridFieldId Id, string Path) GetField(int index) => (index % 4) switch
    {
        0 => (NameField, nameof(Row.Name)),
        1 => (AgeField, nameof(Row.Age)),
        2 => (AddressField, nameof(Row.Address)),
        _ => (ScoreField, nameof(Row.Score))
    };

    private static readonly DataGridFieldId NameField = new("name");
    private static readonly DataGridFieldId AgeField = new("age");
    private static readonly DataGridFieldId AddressField = new("address");
    private static readonly DataGridFieldId ScoreField = new("score");

    private static readonly DataGridLocalSourceDescriptor<Row> RowDescriptor =
        DataGridLocalSourceDescriptor.For<Row>(static row => DataGridRowKey.FromInt64(row.Id))
            .Field(NameField, static row => row.Name ?? string.Empty, StringComparer.Ordinal)
            .Field(AgeField, static row => row.Age)
            .Field(AddressField, static row => row.Address ?? string.Empty, StringComparer.Ordinal)
            .Field(ScoreField, static row => row.Score);

    private static ScenarioInstance Wrap(DataGrid grid) => new(grid, [grid]);

    private sealed record Scenario(string Name, Func<ScenarioInstance> Create);

    private sealed record ScenarioInstance(Control Root, IReadOnlyList<DataGrid> Grids);

    private readonly record struct RealizationAllocation(long ReadyBytes, long CloseBytes);

    private sealed record Result(
        string Name,
        double MeanMilliseconds,
        double MedianMilliseconds,
        double P95Milliseconds,
        double MeanUiThreadAllocatedBytes,
        long MedianUiThreadAllocatedBytes,
        long P95UiThreadAllocatedBytes,
        double MeanProcessAllocatedBytes,
        long MedianProcessAllocatedBytes,
        long P95ProcessAllocatedBytes,
        double MeanProcessCreateAllocatedBytes,
        double MeanProcessRealizeAllocatedBytes,
        double MeanProcessReadyAllocatedBytes,
        double MeanProcessCloseAllocatedBytes);

    private sealed class Row
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public int Age { get; init; }
        public string? Address { get; init; }
        public int Score { get; init; }
    }

    private sealed class Options
    {
        public int Count { get; private init; } = DefaultCount;
        public int Warmup { get; private init; } = 5;
        public string? MarkdownPath { get; private init; }
        public string? Scenario { get; private init; }
        public bool VerifyStates { get; private init; }

        public static Options Parse(IReadOnlyList<string> args)
        {
            var count = DefaultCount;
            var warmup = 5;
            string? markdown = null;
            string? scenario = null;
            var verifyStates = false;
            for (var i = 0; i < args.Count; i++)
            {
                switch (args[i])
                {
                    case "--count":
                        count = int.Parse(args[++i], CultureInfo.InvariantCulture);
                        break;
                    case "--warmup":
                        warmup = int.Parse(args[++i], CultureInfo.InvariantCulture);
                        break;
                    case "--markdown":
                        markdown = args[++i];
                        break;
                    case "--scenario":
                        scenario = args[++i];
                        break;
                    case "--verify-states":
                        verifyStates = true;
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument '{args[i]}'.");
                }
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (warmup < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(warmup));
            }
            return new Options
            {
                Count = count,
                Warmup = warmup,
                MarkdownPath = markdown,
                Scenario = scenario,
                VerifyStates = verifyStates
            };
        }
    }
}

public sealed class PerformanceApplication : Application
{
    // 本工程不引用 AtomUI 生成器，按公开契约手写挂接。
    public override void RegisterServices()
    {
        base.RegisterServices();
        AtomUIStartup.Attach(this);
    }
}
