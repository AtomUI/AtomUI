using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Xunit;
using AtomUIAlert = AtomUI.Desktop.Controls.Alert;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUICard = AtomUI.Desktop.Controls.Card;
using AtomUICheckBox = AtomUI.Desktop.Controls.CheckBox;
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AtomUIPagination = AtomUI.Desktop.Controls.Pagination;
using AtomUIRadioButton = AtomUI.Desktop.Controls.RadioButton;
using AtomUISelect = AtomUI.Desktop.Controls.Select;
using AtomUISlider = AtomUI.Desktop.Controls.Slider;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using AtomUIToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Theme.Performance;

/// <summary>
/// Explicit-run measurement of default theme compilation and light/dark switching. It is a measuring
/// tool for the performance baseline, not a regression test: it has no assertions and is excluded from
/// normal runs. Each metric is written as one line prefixed with <c>MEASURE </c> to the test output and,
/// when <c>ATOMUI_MEASURE_OUTPUT</c> names a file, appended to that file.
/// </summary>
/// <remarks>
/// Optional environment variables: <c>ATOMUI_MEASURE_ITERATIONS</c> (timed samples, default 15),
/// <c>ATOMUI_MEASURE_WARMUPS</c> (untimed warm-ups, default 3), <c>ATOMUI_MEASURE_COMPILE_ITERATIONS</c>
/// (timed cold compiles per chain, default the iteration count), <c>ATOMUI_MEASURE_COMPILE_WARMUPS</c>
/// (untimed cold compiles per chain, default the warm-up count), <c>ATOMUI_MEASURE_LABEL</c> (free text
/// echoed on every line). The file only uses APIs that also exist before the theme algorithm strategy
/// work, so the same source compiles against the pre-change tree for comparisons.
/// Cold samples build the effective theme config (and so its cache key) from scratch; process-wide memos keyed by
/// the schema registry or the theme definition stay warm, as they do in a running application. The uncached switch
/// and the fresh definition defaults reach private members (<c>ThemeManager._themeSnapshotCache</c>,
/// <c>ThemeCompiler.CreateDefinitionDefaultsCore</c>) by reflection; renaming either breaks this tool.
/// </remarks>
public class ThemeCompileMeasurement
{
    [Fact(Explicit = true)]
    public async Task Measure_Default_Theme_Compile_And_Switch()
    {
        var iterations = ReadInt("ATOMUI_MEASURE_ITERATIONS", 15);
        var warmups = ReadInt("ATOMUI_MEASURE_WARMUPS", 3);
        var compileIterations = ReadInt("ATOMUI_MEASURE_COMPILE_ITERATIONS", iterations);
        var compileWarmups = ReadInt("ATOMUI_MEASURE_COMPILE_WARMUPS", warmups);
        var output = new MeasureOutput(Environment.GetEnvironmentVariable("ATOMUI_MEASURE_OUTPUT"),
            Environment.GetEnvironmentVariable("ATOMUI_MEASURE_LABEL") ?? string.Empty);

        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        var rootSnapshot = manager.RootContext.Snapshot;
        var definition = rootSnapshot.Definition;
        var definitionRevision = rootSnapshot.DefinitionRevision;
        var registry = rootSnapshot.Registry;
        output.Write("config.iterations", iterations.ToString(CultureInfo.InvariantCulture));
        output.Write("config.warmups", warmups.ToString(CultureInfo.InvariantCulture));
        output.Write("config.compileIterations", compileIterations.ToString(CultureInfo.InvariantCulture));
        output.Write("config.compileWarmups", compileWarmups.ToString(CultureInfo.InvariantCulture));
        output.Write("config.themeId", rootSnapshot.ThemeId);
        output.Write("schema.globalSlots", registry.GlobalTokens.Count.ToString(CultureInfo.InvariantCulture));
        output.Write("schema.controlSlots", registry.Controls.Count.ToString(CultureInfo.InvariantCulture));

        // Same layering as ThemeStartupCompilation.CompileInitial: definition defaults, then the
        // application layer (no initial config in the test app: font only), then the runtime request.
        // The three gated chains build their definition defaults from scratch for every sample, so every
        // sample compiles a NormalizedThemeConfig object (and therefore a config cache key and Control config
        // objects) that no earlier sample has seen. "defaultMemoised" uses the process-wide memoised
        // definition defaults: with an empty runtime layer the merge returns that memoised object, so its
        // config cache key is built once and then reused. It is reported for reference and is not gated.
        var chains = new (string Name, ThemeConfig? Config, bool Memoised)[]
        {
            ("default", null, false),
            ("dark", new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Dark).Build(), false),
            ("compact", new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Compact).Build(), false),
            ("defaultMemoised", null, true)
        };

        // ThemeCompiler.CreateDefinitionDefaults memoises per (definition, registry); the private core
        // builds the same content as a new object.
        var createDefaultsCore = typeof(ThemeCompiler).GetMethod(
                                     "CreateDefinitionDefaultsCore", BindingFlags.Static | BindingFlags.NonPublic)
                                 ?? throw new InvalidOperationException(
                                     "ThemeCompiler.CreateDefinitionDefaultsCore was not found.");

        ThemeCompileInput PrepareInput(ThemeConfig? config, bool memoised)
        {
            var runtimeDefault = ThemeConfigNormalizer.Normalize(
                ThemeStartupCompilation.AddDefaultFont(null, manager.FontFamily) ?? ThemeConfig.Empty, registry);
            var runtime = config is null
                ? runtimeDefault
                : ThemeConfigNormalizer.Normalize(
                    ThemeStartupCompilation.AddDefaultFont(config, manager.FontFamily)!, registry);
            var defaults = memoised
                ? ThemeCompiler.CreateDefinitionDefaults(definition, registry)
                : (NormalizedThemeConfig)createDefaultsCore.Invoke(null, [definition, registry])!;
            var effective = ThemeStartupCompilation.MergeEffectiveConfig(
                defaults, runtimeDefault.Config!, runtime.Config!);
            // No reusable parent and no change set: nothing can be reused from an earlier snapshot.
            return new ThemeCompileInput(definition, definitionRevision, effective, registry);
        }

        // A new compiler, no snapshot cache and no Control compilation cache.
        static ThemeSnapshot CompileInput(ThemeCompileInput input)
        {
            var result = new ThemeCompiler().Compile(input);
            if (!result.Success)
            {
                throw new InvalidOperationException("Cold compile failed.", result.Exception);
            }
            return result.Snapshot!;
        }

        ThemeSnapshot ColdCompile(ThemeConfig? config, bool memoised) => CompileInput(PrepareInput(config, memoised));

        static void MaterializeAll(ThemeSnapshot snapshot)
        {
            for (var slot = 0; slot < snapshot.Controls.Count; slot++)
            {
                _ = snapshot.Controls[slot];
            }
        }

        // First-ever compile in this process (includes JIT); informational only.
        var firstWatch = Stopwatch.StartNew();
        MaterializeAll(ColdCompile(null, false));
        firstWatch.Stop();
        output.Write("compile.default.firstEverMs", Format(firstWatch.Elapsed.TotalMilliseconds));

        var sanity = ColdCompile(null, false);
        output.Write("sanity.defaultFingerprintEqualsManagerRoot",
            sanity.ContentFingerprint.Equals(rootSnapshot.ContentFingerprint) ? "true" : "false");

        foreach (var (name, config, memoised) in chains)
        {
            // Freshness check: two consecutive samples must not share the effective config object or its
            // cache key (fresh chains), while their content must equal the memoised path's.
            var first = PrepareInput(config, memoised).EffectiveConfig;
            var second = PrepareInput(config, memoised).EffectiveConfig;
            var reference = PrepareInput(config, true).EffectiveConfig;
            output.Write($"sanity.{name}.configObjectReusedAcrossSamples",
                ReferenceEquals(first, second) ? "true" : "false");
            output.Write($"sanity.{name}.configKeyReusedAcrossSamples",
                ReferenceEquals(NormalizedThemeConfigCacheKey.Create(first), NormalizedThemeConfigCacheKey.Create(second))
                    ? "true"
                    : "false");
            output.Write($"sanity.{name}.configContentEqualsMemoised", first.Equals(reference) ? "true" : "false");

            // Token instance count of one cold compile, derived from the compiler's construction rules:
            // the global builder chain creates one seed plus one copy per algorithm; every lazily compiled
            // Control creates one effective global copy (plus one per algorithm for Global/Custom modes) and
            // one Control builder when it has own tokens.
            var probe = ColdCompile(config, memoised);
            var algorithmCount = probe.EffectiveConfig.Algorithms.Count;
            long designTokens = 1 + algorithmCount;
            long controlTokens = 0;
            long lazyControls = 0;
            for (var slot = 0; slot < probe.Controls.Count; slot++)
            {
                if (probe.Controls.GetCell(slot).MaterializedValue is not null)
                {
                    continue;
                }
                lazyControls++;
                var descriptor = registry.Controls[slot];
                designTokens++;
                if (probe.EffectiveConfig.TryGetControl(descriptor.Identity, out var controlConfig))
                {
                    designTokens += controlConfig.AlgorithmMode switch
                    {
                        ControlAlgorithmMode.Global => algorithmCount,
                        ControlAlgorithmMode.Custom => controlConfig.Algorithms.Count,
                        _                           => 0
                    };
                }
                if (descriptor.HasOwnTokens)
                {
                    controlTokens++;
                }
            }
            MaterializeAll(probe);
            output.Write($"compile.{name}.lazyControls", lazyControls.ToString(CultureInfo.InvariantCulture));
            output.Write($"compile.{name}.designTokenInstances", designTokens.ToString(CultureInfo.InvariantCulture));
            output.Write($"compile.{name}.controlTokenInstances", controlTokens.ToString(CultureInfo.InvariantCulture));
            output.Write($"compile.{name}.tokenInstances",
                (designTokens + controlTokens).ToString(CultureInfo.InvariantCulture));
            output.Write($"hash.{name}", ComputeValueHash(probe, registry));

            for (var index = 0; index < compileWarmups; index++)
            {
                MaterializeAll(ColdCompile(config, memoised));
            }

            // One sample: the effective config is prepared first (normalize + merge, reported separately as
            // "prepare"), then the cold compile proper is measured: ThemeCompiler.Compile plus the
            // materialization of every Control snapshot.
            var times = new double[compileIterations];
            var allocations = new long[compileIterations];
            var prepareTimes = new double[compileIterations];
            var prepareAllocations = new long[compileIterations];
            for (var index = 0; index < compileIterations; index++)
            {
                Settle();
                var allocated0 = GC.GetAllocatedBytesForCurrentThread();
                var time0 = Stopwatch.GetTimestamp();
                var input = PrepareInput(config, memoised);
                var time1 = Stopwatch.GetTimestamp();
                var allocated1 = GC.GetAllocatedBytesForCurrentThread();
                var snapshot = CompileInput(input);
                MaterializeAll(snapshot);
                var time2 = Stopwatch.GetTimestamp();
                var allocated2 = GC.GetAllocatedBytesForCurrentThread();
                prepareTimes[index] = Stopwatch.GetElapsedTime(time0, time1).TotalMilliseconds;
                times[index] = Stopwatch.GetElapsedTime(time1, time2).TotalMilliseconds;
                prepareAllocations[index] = allocated1 - allocated0;
                allocations[index] = allocated2 - allocated1;
                GC.KeepAlive(snapshot);
            }
            output.Write($"compile.{name}.coldMedianMs", Format(Median(times)));
            output.Write($"compile.{name}.coldP25Ms", Format(Percentile(times, 0.25)));
            output.Write($"compile.{name}.coldMinMs", Format(times.Min()));
            output.Write($"compile.{name}.coldMaxMs", Format(times.Max()));
            output.Write($"compile.{name}.coldAllocatedBytesMedian",
                MedianLong(allocations).ToString(CultureInfo.InvariantCulture));
            output.Write($"compile.{name}.coldAllocatedBytesMin",
                allocations.Min().ToString(CultureInfo.InvariantCulture));
            output.Write($"compile.{name}.coldAllocatedBytesMax",
                allocations.Max().ToString(CultureInfo.InvariantCulture));
            output.Write($"prepare.{name}.medianMs", Format(Median(prepareTimes)));
            output.Write($"prepare.{name}.allocatedBytesMedian",
                MedianLong(prepareAllocations).ToString(CultureInfo.InvariantCulture));
            output.Write($"compile.{name}.coldSamplesMs", string.Join(',', times.Select(Format)));
        }

        // Light <-> dark switch through the public manager path, with a shown AtomUI window hosting a few
        // common controls so the switch also re-materializes their Control snapshots and republishes resources.
        // "cached": the manager's snapshot cache is kept, as repeated toggles in an app see it.
        // "uncached": the manager's snapshot cache is dropped before every switch, so each switch compiles
        // (the first switch an app makes to a theme it has not shown yet).
        var cancellation = TestContext.Current.CancellationToken;
        var lightRequest = new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, null, ThemeTransitionReason.UserRequest);
        var darkRequest = new ThemeRequest(IThemeManager.DEFAULT_THEME_ID, chains[1].Config,
            ThemeTransitionReason.UserRequest);
        var window = CreateHostWindow();
        var statuses = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            async Task<double> SwitchAsync(ThemeRequest request, bool dropCache)
            {
                if (dropCache)
                {
                    DropSnapshotCache(manager);
                }
                Dispatcher.UIThread.RunJobs();
                var start = Stopwatch.GetTimestamp();
                var result = await manager.ApplyThemeAsync(request, cancellation);
                Dispatcher.UIThread.RunJobs();
                var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                statuses.Add(result.Status.ToString());
                return elapsed;
            }

            foreach (var (mode, dropCache) in new[] { ("cached", false), ("uncached", true) })
            {
                for (var index = 0; index < warmups; index++)
                {
                    await SwitchAsync(darkRequest, dropCache);
                    await SwitchAsync(lightRequest, dropCache);
                }

                var toDark = new double[iterations];
                var toLight = new double[iterations];
                var roundTrips = new double[iterations];
                for (var index = 0; index < iterations; index++)
                {
                    Settle();
                    toDark[index] = await SwitchAsync(darkRequest, dropCache);
                    toLight[index] = await SwitchAsync(lightRequest, dropCache);
                    roundTrips[index] = toDark[index] + toLight[index];
                }
                output.Write($"switch.{mode}.roundTripMedianMs", Format(Median(roundTrips)));
                output.Write($"switch.{mode}.roundTripP25Ms", Format(Percentile(roundTrips, 0.25)));
                output.Write($"switch.{mode}.roundTripMinMs", Format(roundTrips.Min()));
                output.Write($"switch.{mode}.toDarkMedianMs", Format(Median(toDark)));
                output.Write($"switch.{mode}.toLightMedianMs", Format(Median(toLight)));
                output.Write($"switch.{mode}.roundTripSamplesMs", string.Join(',', roundTrips.Select(Format)));
            }
            output.Write("switch.statuses", string.Join('|', statuses.OrderBy(s => s, StringComparer.Ordinal)));
        }
        finally
        {
            window.Close();
            await manager.ApplyThemeAsync(lightRequest, cancellation);
        }
    }

    private static AtomUIWindow CreateHostWindow()
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(new AtomUIButton { Content = "Button" });
        panel.Children.Add(new AtomUILineEdit { Text = "LineEdit" });
        panel.Children.Add(new AtomUICheckBox { Content = "CheckBox" });
        panel.Children.Add(new AtomUIRadioButton { Content = "RadioButton" });
        panel.Children.Add(new AtomUIToggleSwitch());
        panel.Children.Add(new AtomUISlider { Value = 30 });
        panel.Children.Add(new AtomUISelect());
        panel.Children.Add(new AtomUIAlert());
        panel.Children.Add(new AtomUIPagination { Total = 100 });
        panel.Children.Add(new AtomUICard { Content = new AtomUITextBlock { Text = "Card" } });
        return new AtomUIWindow { Width = 800, Height = 600, Content = panel };
    }

    // The manager creates a new snapshot cache on demand; dropping it makes the next switch compile.
    private static void DropSnapshotCache(ThemeManager manager)
    {
        var field = typeof(ThemeManager).GetField("_themeSnapshotCache", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("ThemeManager._themeSnapshotCache was not found.");
        (field.GetValue(manager) as IDisposable)?.Dispose();
        field.SetValue(manager, null);
    }

    /// <summary>
    /// SHA-256 over the formatted value of every global slot and every Control own slot. Global tokens are
    /// ordered by name, Controls by identity and own tokens by name (all ordinal), so the hash does not depend
    /// on slot numbering and stays comparable across trees.
    /// </summary>
    private static string ComputeValueHash(ThemeSnapshot snapshot, ThemeSchemaRegistry registry)
    {
        var text = new StringBuilder();
        foreach (var descriptor in registry.GlobalTokens.OrderBy(d => d.Name, StringComparer.Ordinal))
        {
            text.Append("G|").Append(descriptor.Name).Append('=')
                .Append(descriptor.Format(snapshot.GlobalTokenValues.GetValue(descriptor.Slot))).Append('\n');
        }

        var controls = Enumerable.Range(0, registry.Controls.Count)
                                 .Select(slot => (Slot: slot, Descriptor: registry.Controls[slot]))
                                 .OrderBy(entry => entry.Descriptor.Identity.ToString(), StringComparer.Ordinal);
        foreach (var (slot, descriptor) in controls)
        {
            if (descriptor.OwnTokens.Count == 0)
            {
                continue;
            }
            var values = snapshot.Controls[slot].ControlTokenValues;
            var identity = descriptor.Identity.ToString();
            foreach (var token in descriptor.OwnTokens.OrderBy(d => d.Name, StringComparer.Ordinal))
            {
                text.Append("C|").Append(identity).Append('|').Append(token.Name).Append('=')
                    .Append(token.Format(values.GetValue(token.Slot))).Append('\n');
            }
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())));
    }

    private static void Settle()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static double Median(double[] values)
    {
        var sorted = values.OrderBy(value => value).ToArray();
        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }

    // Nearest-rank percentile.
    private static double Percentile(double[] values, double fraction)
    {
        var sorted = values.OrderBy(value => value).ToArray();
        var rank = (int)Math.Ceiling(fraction * sorted.Length);
        return sorted[Math.Clamp(rank - 1, 0, sorted.Length - 1)];
    }

    // Same rule as Median: the mean of the two middle values for even-length input (rounded down).
    private static long MedianLong(long[] values)
    {
        var sorted = values.OrderBy(value => value).ToArray();
        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }

    private static string Format(double value) => value.ToString("F4", CultureInfo.InvariantCulture);

    private static int ReadInt(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), NumberStyles.Integer, CultureInfo.InvariantCulture,
            out var value) && value > 0
            ? value
            : fallback;

    private sealed class MeasureOutput(string? filePath, string label)
    {
        internal void Write(string metric, string value)
        {
            var line = label.Length == 0 ? $"MEASURE {metric}={value}" : $"MEASURE [{label}] {metric}={value}";
            TestContext.Current.TestOutputHelper?.WriteLine(line);
            if (!string.IsNullOrEmpty(filePath))
            {
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
        }
    }
}
