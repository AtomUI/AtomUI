using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUILineEdit = AtomUI.Desktop.Controls.LineEdit;
using AtomUISelect = AtomUI.Desktop.Controls.Select;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUIGallery.Tests.Theme.Performance;

/// <summary>
/// Explicit-run measurement of the design-language scenarios of spec 12.5 that need several languages enabled (the
/// Gallery host enables Ant Design, Shadcn and PrimeNG): language switching cold and cached, a cold compile under each
/// idiom, a control-level fixed axis or seed, and a local scope that overrides one Alias token. It is a measuring tool,
/// not a regression test: it is excluded from normal runs and rejects invalid samples. Each metric is one line prefixed with
/// <c>MEASURE </c>, also appended to the file named by <c>ATOMUI_MEASURE_OUTPUT</c>.
/// </summary>
/// <remarks>
/// <c>ATOMUI_MEASURE_ITERATIONS</c> (timed samples, default 15) and <c>ATOMUI_MEASURE_WARMUPS</c> (default 3). The cold
/// switch and cold scope samples drop the private <c>ThemeManager._themeSnapshotCache</c> by reflection before timing.
/// </remarks>
public class DesignLanguageScenarioMeasurement
{
    static DesignLanguageScenarioMeasurement()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact(Explicit = true)]
    public void Measure_Design_Language_Scenarios()
    {
        var iterations = ReadInt("ATOMUI_MEASURE_ITERATIONS", 15);
        var warmups    = ReadInt("ATOMUI_MEASURE_WARMUPS", 3);
        var output = new MeasureOutput(Environment.GetEnvironmentVariable("ATOMUI_MEASURE_OUTPUT"));
        var manager = (ThemeManager)Application.Current!.GetThemeManager()!;
        output.Write("config.iterations", iterations.ToString(CultureInfo.InvariantCulture));
        output.Write("config.warmups", warmups.ToString(CultureInfo.InvariantCulture));
        var initialTheme = manager.CurrentTheme!;
        var initialSelection = manager.CurrentSelection ?? ThemeSelection.Of(initialTheme.Language).WithTheme(initialTheme.ThemeId);

        var window = CreateHostWindow(out var host);
        window.Show();
        Pump();
        try
        {
            MeasureLanguageSwitch(manager, output, iterations, warmups);
            MeasureIdioms(manager, output, iterations, warmups);
            MeasureControlOverrides(manager, host, output, iterations, warmups);
            MeasureSingleAliasScope(manager, host, output, iterations, warmups);
        }
        finally
        {
            window.Close();
            if (manager.CurrentTheme?.Fingerprint != initialTheme.Fingerprint)
            {
                Apply(manager, initialSelection);
            }
        }
    }

    // Ant Design -> Shadcn -> PrimeNG -> Ant Design through the public manager path, with a shown window.
    private static void MeasureLanguageSwitch(ThemeManager manager, MeasureOutput output, int iterations, int warmups)
    {
        var languages = new (string Name, ThemeSelection Selection)[]
        {
            ("Shadcn", ThemeSelection.Of<ShadcnLanguage>()),
            ("PrimeNG", ThemeSelection.Of<PrimeNGLanguage>()),
            ("AntDesign", ThemeSelection.Of<AntDesignLanguage>().WithTheme(AntDesignThemes.DaybreakBlue))
        };
        var snapshots = new Dictionary<string, ThemeSnapshot>(StringComparer.Ordinal);
        double Sample(ThemeSelection selection, bool dropCache, out bool cacheHit)
        {
            var elapsed = Switch(manager, selection, dropCache);
            var snapshot = manager.RootContext.Snapshot;
            cacheHit = snapshots.TryGetValue(selection.Language, out var previous) && ReferenceEquals(previous, snapshot);
            snapshots[selection.Language] = snapshot;
            return elapsed;
        }
        foreach (var (mode, dropCache) in new[] { ("cold", true), ("cached", false) })
        {
            for (var index = 0; index < warmups; index++)
            {
                foreach (var (_, selection) in languages)
                {
                    Sample(selection, dropCache, out _);
                }
            }
            var samples = languages.ToDictionary(static entry => entry.Name, _ => new double[iterations]);
            var cacheHits = languages.ToDictionary(static entry => entry.Name, _ => 0);
            for (var index = 0; index < iterations; index++)
            {
                Settle();
                foreach (var (name, selection) in languages)
                {
                    samples[name][index] = Sample(selection, dropCache, out var cacheHit);
                    if (cacheHit)
                    {
                        cacheHits[name]++;
                    }
                }
            }
            foreach (var (name, values) in samples)
            {
                output.Write($"languageSwitch.{mode}.to{name}.medianMs", Format(Median(values)));
                output.Write($"languageSwitch.{mode}.to{name}.minMs", Format(values.Min()));
                output.Write($"languageSwitch.{mode}.to{name}.cacheHits", Format(cacheHits[name]));
                if (cacheHits[name] != (dropCache ? 0 : iterations))
                {
                    throw new InvalidOperationException($"Unexpected cache behavior switching to {name} in {mode} mode.");
                }
            }
        }
    }

    // A cold compile of the Ant Design default under the desktop and under the mobile idiom: a new compiler, no caches.
    private static void MeasureIdioms(ThemeManager manager, MeasureOutput output, int iterations, int warmups)
    {
        var root = manager.RootContext.Snapshot;
        foreach (var idiom in new[] { DesignIdiom.Desktop, DesignIdiom.Mobile })
        {
            var config = new ThemeConfigBuilder().WithIdiom(idiom).Build();
            for (var index = 0; index < warmups; index++)
            {
                ColdCompile(manager, root, config);
            }
            var times = new double[iterations];
            var allocations = new long[iterations];
            for (var index = 0; index < iterations; index++)
            {
                Settle();
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                var start = Stopwatch.GetTimestamp();
                var snapshot = ColdCompile(manager, root, config);
                times[index] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                allocations[index] = GC.GetAllocatedBytesForCurrentThread() - allocated;
                GC.KeepAlive(snapshot);
            }
            output.Write($"idiom.{idiom.Id}.coldMedianMs", Format(Median(times)));
            output.Write($"idiom.{idiom.Id}.coldAllocatedBytesMedian",
                MedianLong(allocations).ToString(CultureInfo.InvariantCulture));
        }
    }

    // A child scope whose config fixes Button's appearance, or overrides a seed for Button only: only Button gets its own
    // global view; every other control's cell must be the parent's (reference).
    private static void MeasureControlOverrides(ThemeManager manager, ContentControl host, MeasureOutput output,
                                                int iterations, int warmups)
    {
        var button = new ControlTokenIdentity("AtomUI", "Button");
        var cases = new (string Name, ControlThemeConfig Config)[]
        {
            ("fixedAppearance", new ControlThemeConfigBuilder().WithAppearance(ThemeAppearance.Dark).Build()),
            ("seed", new ControlThemeConfigBuilder().WithToken(nameof(DesignToken.ColorPrimary), "#722ED1").Build())
        };
        var root = manager.RootContext.Snapshot;
        var buttonSlot = root.Registry.Controls.Single(descriptor => descriptor.Identity == button).Slot;
        var primarySlot = root.Registry.GlobalTokens.Single(descriptor => descriptor.Name == nameof(DesignToken.ColorPrimary)).Slot;
        foreach (var (name, controlConfig) in cases)
        {
            var namePrefix = $"controlOverride.{name}";
            var config = new ThemeConfigBuilder().WithControl(button, controlConfig).Build();
            var snapshot = MeasureScope(manager, host, output, namePrefix, config, iterations, warmups);
            var recompiled = Enumerable.Range(0, root.Controls.Count)
                                       .Where(slot => !ReferenceEquals(snapshot.Controls[slot], root.Controls[slot]))
                                       .ToArray();
            output.Write($"{namePrefix}.controls", Format(root.Controls.Count));
            output.Write($"{namePrefix}.reusedControls", Format(root.Controls.Count - recompiled.Length));
            output.Write($"{namePrefix}.recompiledControls", Format(recompiled.Length));
            if (!ReferenceEquals(snapshot.GlobalTokenValues, root.GlobalTokenValues) ||
                recompiled is not [var slot] || slot != buttonSlot ||
                !snapshot.EffectiveConfig.TryGetControl(button, out var effective) || !effective.IsOwnEvaluationUnit)
            {
                throw new InvalidOperationException($"{namePrefix} must isolate the override to Button's own evaluation unit.");
            }

            var buttonSnapshot = snapshot.Controls[buttonSlot];
            var applied = name == "fixedAppearance"
                ? buttonSnapshot.Appearance == ThemeAppearance.Dark && root.Controls[buttonSlot].Appearance == ThemeAppearance.Light
                : buttonSnapshot.GetEffectiveGlobalValue<Color>(snapshot.GlobalTokenValues, primarySlot) == Color.Parse("#722ED1");
            if (!applied)
            {
                throw new InvalidOperationException($"{namePrefix} did not apply the requested Button override.");
            }
        }
    }

    // A child scope overriding one Alias token: only the controls whose read set contains it are recompiled.
    private static void MeasureSingleAliasScope(ThemeManager manager, ContentControl host, MeasureOutput output,
                                                int iterations, int warmups)
    {
        const string alias = nameof(DesignToken.ColorTextLabel);
        var config = new ThemeConfigBuilder().WithToken(alias, "#FF0000").Build();
        var root = manager.RootContext.Snapshot;
        var token = root.Registry.GlobalTokens.Single(descriptor => descriptor.Name == alias && descriptor.Stage == TokenStage.Alias);
        var child = MeasureScope(manager, host, output, "singleAlias", config, iterations, warmups);
        var changed = root.Registry.GlobalTokens.Where(descriptor =>
            !root.GlobalTokenValues.ValueEquals(descriptor.Slot, child.GlobalTokenValues.GetValue(descriptor.Slot)));
        if (!changed.SequenceEqual([token]) || child.GlobalTokenValues.Get<Color>(token.Slot) != Color.Parse("#FF0000"))
        {
            throw new InvalidOperationException("The single-Alias scenario must change only the selected Alias token.");
        }
        output.Write("singleAlias.changedGlobalToken", alias);

        var readers = 0;
        var withOwnTokens = 0;
        var recompiled = 0;
        var recompiledWithoutReadSet = 0;
        var replacedEmptyCells = 0;
        for (var control = 0; control < root.Controls.Count; control++)
        {
            var parentValue = root.Controls[control];
            var reused = ReferenceEquals(child.Controls[control], parentValue);
            if (!root.Registry.ControlHasOwnTokens(control))
            {
                if (!reused)
                {
                    replacedEmptyCells++;
                }
                continue;
            }

            withOwnTokens++;
            if (!reused)
            {
                recompiled++;
            }
            if (parentValue.GlobalReads is not { } reads)
            {
                if (!reused)
                {
                    recompiledWithoutReadSet++;
                }
                continue;
            }

            var readsAlias = reads.Contains(token.Slot);
            if (readsAlias)
            {
                readers++;
            }
            if (reused == readsAlias)
            {
                throw new InvalidOperationException($"Unexpected Alias read-set reuse for {root.Registry.GetControlIdentity(control)}.");
            }
        }
        output.Write("singleAlias.controls", Format(root.Controls.Count));
        output.Write("singleAlias.controlsWithOwnTokens", Format(withOwnTokens));
        output.Write("singleAlias.controlsReadingIt", Format(readers));
        output.Write("singleAlias.recompiledControls", Format(recompiled));
        output.Write("singleAlias.recompiledWithoutReadSet", Format(recompiledWithoutReadSet));
        output.Write("singleAlias.replacedEmptyCells", Format(replacedEmptyCells));
        if (readers == 0)
        {
            throw new InvalidOperationException("The single-Alias scenario must exercise at least one recorded reader.");
        }
    }

    private static ThemeSnapshot MeasureScope(ThemeManager manager, ContentControl host, MeasureOutput output, string name,
                                              ThemeConfig config, int iterations, int warmups)
    {
        var parent = manager.RootContext.Snapshot;
        MaterializeAll(parent);
        var times = new double[iterations];
        var allocations = new long[iterations];
        ThemeSnapshot? last = null;
        for (var index = -warmups; index < iterations; index++)
        {
            DropSnapshotCache(manager);
            Settle();
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            var start = Stopwatch.GetTimestamp();
            var provider = new ThemeConfigProvider { Config = config, Child = new Border() };
            host.Content = provider;
            Pump();
            var context = provider.GetValue(ThemeScope.ContextProperty) ??
                          throw new InvalidOperationException("Scope did not attach to the theme manager.");
            var snapshot = context.Snapshot;
            MaterializeAll(snapshot);
            var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            if (index >= 0)
            {
                times[index] = elapsed;
                allocations[index] = GC.GetAllocatedBytesForCurrentThread() - allocated;
            }
            host.Content = null;
            Pump();
            if (ReferenceEquals(snapshot, parent) || ReferenceEquals(snapshot, last))
            {
                throw new InvalidOperationException($"{name} did not compile a fresh scope snapshot.");
            }
            last = snapshot;
        }
        output.Write($"{name}.scopeMedianMs", Format(Median(times)));
        output.Write($"{name}.allocatedBytesMedian", Format(MedianLong(allocations)));
        return last!;
    }

    private static ThemeSnapshot ColdCompile(ThemeManager manager, ThemeSnapshot root, ThemeConfig config)
    {
        var registry = root.Registry;
        var runtime = ThemeConfigNormalizer.Normalize(
            ThemeStartupCompilation.AddDefaultFont(config, manager.FontFamily)!, registry);
        var runtimeDefault = ThemeConfigNormalizer.Normalize(
            ThemeStartupCompilation.AddDefaultFont(null, manager.FontFamily) ?? ThemeConfig.Empty, registry);
        var effective = ThemeStartupCompilation.MergeEffectiveConfig(
            ThemeCompiler.CreateDefinitionDefaults(root.Definition, registry), runtimeDefault.Config!, runtime.Config!);
        var result = new ThemeCompiler().Compile(new ThemeCompileInput(root.Definition, root.DefinitionRevision, effective,
            registry));
        if (!result.Success)
        {
            throw new InvalidOperationException("Cold compile failed.", result.Exception);
        }
        if (result.Snapshot!.Variant.Idiom != config.Idiom)
        {
            throw new InvalidOperationException("Cold compile did not resolve the requested idiom.");
        }
        MaterializeAll(result.Snapshot!);
        return result.Snapshot!;
    }

    private static double Switch(ThemeManager manager, ThemeSelection selection, bool dropCache)
    {
        if (dropCache)
        {
            DropSnapshotCache(manager);
        }
        Pump();
        var start = Stopwatch.GetTimestamp();
        Apply(manager, selection);
        var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        if (manager.CurrentTheme?.Language != selection.Language)
        {
            throw new InvalidOperationException($"The committed theme did not select {selection.Language}.");
        }
        return elapsed;
    }

    private static void Apply(ThemeManager manager, ThemeSelection selection)
    {
        var task = manager.ApplyAsync(selection);
        while (!task.IsCompleted)
        {
            Pump();
        }
        var status = task.GetAwaiter().GetResult().Status;
        if (status != ThemeTransitionStatus.Committed)
        {
            throw new InvalidOperationException($"Theme switch ended as {status}.");
        }
        Pump();
    }

    private static AtomUIWindow CreateHostWindow(out ContentControl host)
    {
        host = new ContentControl();
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(new AtomUIButton { Content = "Button" });
        panel.Children.Add(new AtomUILineEdit { Text = "LineEdit" });
        panel.Children.Add(new AtomUISelect());
        panel.Children.Add(host);
        return new AtomUIWindow { Width = 800, Height = 600, Content = panel };
    }

    private static void MaterializeAll(ThemeSnapshot snapshot)
    {
        for (var slot = 0; slot < snapshot.Controls.Count; slot++)
        {
            _ = snapshot.Controls[slot];
        }
    }

    private static void DropSnapshotCache(ThemeManager manager)
    {
        var field = typeof(ThemeManager).GetField("_themeSnapshotCache", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new InvalidOperationException("ThemeManager._themeSnapshotCache was not found.");
        (field.GetValue(manager) as IDisposable)?.Dispose();
        field.SetValue(manager, null);
    }

    private static void Pump() => Dispatcher.UIThread.RunJobs();

    private static void Settle()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private static double Median(double[] values)
    {
        var sorted = values.OrderBy(static value => value).ToArray();
        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }

    private static long MedianLong(long[] values)
    {
        var sorted = values.OrderBy(static value => value).ToArray();
        var middle = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
    }

    private static string Format(double value) => value.ToString("F4", CultureInfo.InvariantCulture);

    private static string Format(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static int ReadInt(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), NumberStyles.Integer, CultureInfo.InvariantCulture,
            out var value) && value > 0
            ? value
            : fallback;

    private sealed class MeasureOutput(string? filePath)
    {
        internal void Write(string metric, string value)
        {
            var line = $"MEASURE {metric}={value}";
            TestContext.Current.TestOutputHelper?.WriteLine(line);
            if (!string.IsNullOrEmpty(filePath))
            {
                File.AppendAllText(filePath, line + Environment.NewLine);
            }
        }
    }
}
