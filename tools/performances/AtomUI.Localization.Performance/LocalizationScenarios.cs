using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Avalonia.Controls;

namespace AtomUI.Localization.Performance;

internal static class LocalizationScenarios
{
    private const int MemoryRetentionSampleCount = 16;

    private static readonly LanguageTag[] s_languagePool =
    [
        LanguageTags.EnUS,
        LanguageTags.ZhCN,
        LanguageTags.ZhTW,
        LanguageTags.JaJP,
        LanguageTags.ArSA,
        LanguageTags.FrFR
    ];

    internal static int Run(int count, string? markdownPath, bool verifyStates)
    {
        if (verifyStates && !LocalizationStateVerification.Run())
        {
            return 1;
        }

        var startup = MeasureStartupBuild();
        var hotPath = MeasureGetHotPath(Math.Max(1_000, count * 1_000));
        var switching = MeasureLanguageSwitching(Math.Max(10, count * 10));
        var memory = MeasureSnapshotMemory();

        Console.WriteLine("Localization performance");
        Console.WriteLine("------------------------");
        Console.WriteLine($"Snapshot startup build: {startup.Elapsed.TotalMilliseconds:0.00} ms, " +
                          $"{startup.AllocatedBytes / 1024.0:0.0} KiB allocated " +
                          $"({startup.CatalogCount} catalogs, {startup.UnitCount} units, {startup.LanguageCount} languages)");
        Console.WriteLine($"ILocalizer.Get hot path: {hotPath.Elapsed.TotalMilliseconds:0.00} ms, " +
                          $"{hotPath.AllocatedBytes / 1024.0:0.0} KiB for {hotPath.OperationCount:N0} calls " +
                          $"({hotPath.Elapsed.TotalNanoseconds() / hotPath.OperationCount:0.0} ns/call)");
        Console.WriteLine($"Language switch commit: {switching.Elapsed.TotalMilliseconds:0.00} ms, " +
                          $"{switching.AllocatedBytes / 1024.0:0.0} KiB for {switching.OperationCount:N0} changes, " +
                          $"{switching.ResourceNotificationCount:N0} resource notifications, " +
                          $"{switching.LanguageEventCount:N0} language events");
        Console.WriteLine();
        Console.WriteLine("Snapshot memory matrix");
        Console.WriteLine("----------------------");
        Console.WriteLine("Catalogs Units Languages RetainedKiB LogicalTextKiB SnapshotSlots");
        foreach (var point in memory)
        {
            Console.WriteLine($"{point.CatalogCount,7} {point.UnitCount,5} {point.LanguageCount,9} " +
                              $"{point.RetainedBytes / 1024.0,12:0.0} {point.LogicalTextBytes / 1024.0,14:0.0} {point.SnapshotSlotCount,13}");
        }

        if (!string.IsNullOrWhiteSpace(markdownPath))
        {
            var fullPath = Path.GetFullPath(markdownPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, RenderMarkdown(startup, hotPath, switching, memory),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine();
            Console.WriteLine($"Wrote markdown baseline: {fullPath}");
        }

        return 0;
    }

    internal static LocalizationRuntime BuildRuntime(
        int catalogCount,
        int unitCount,
        int languageCount)
    {
        return BuildRuntime(CreateFixture(catalogCount, unitCount, languageCount));
    }

    private static LocalizationFixture CreateFixture(
        int catalogCount,
        int unitCount,
        int languageCount)
    {
        if (catalogCount is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(catalogCount), catalogCount, "Use 1..8 catalogs.");
        }
        if (unitCount is < 1 or > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(unitCount), unitCount, "Use 1..128 units.");
        }
        if (languageCount is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(languageCount), languageCount, "Use 1..6 languages.");
        }

        var languages = s_languagePool[..languageCount];
        var catalogs = new LanguageCatalogDescriptor[catalogCount];
        var bundles = new List<TranslationBundleDescriptor>(catalogCount * languageCount);
        for (var catalogIndex = 0; catalogIndex < catalogCount; catalogIndex++)
        {
            var catalogId = $"Perf:AtomUI.Performance.LocalizationCatalog{catalogIndex}";
            var units = Enumerable.Range(0, unitCount)
                                   .Select(static unitIndex =>
                                       new LanguageCatalogUnitDescriptor(unitIndex + 1, $"Unit{unitIndex}"))
                                   .ToArray();
            catalogs[catalogIndex] = CreateCatalog(catalogIndex, catalogId, units);

            foreach (var language in languages)
            {
                var values = Enumerable.Range(0, unitCount)
                                        .Select(unitIndex =>
                                            $"{language.Value}:{catalogIndex}:{unitIndex}")
                                        .Cast<string?>()
                                        .ToArray();
                bundles.Add(new TranslationBundleDescriptor(
                    catalogId,
                    1,
                    language,
                    TranslationSourceKind.ModuleBuiltIn,
                    $"LocalizationPerf.Catalog{catalogIndex}.{language.Value}",
                    values));
            }
        }

        return new LocalizationFixture(catalogs, bundles, languages);
    }

    private static LocalizationRuntime BuildRuntime(LocalizationFixture fixture)
    {
        var builder = new LocalizationBuilder();
        foreach (var catalog in fixture.Catalogs)
        {
            builder.AddCatalog(catalog);
        }
        foreach (var bundle in fixture.Bundles)
        {
            builder.AddTranslationBundle(bundle);
        }
        builder.ConfigureLanguages(fixture.Languages[0], fixture.Languages);
        return builder.Build(static () => true);
    }

    private static LocalizationStartupResult MeasureStartupBuild()
    {
        var fixture = CreateFixture(catalogCount: 4, unitCount: 32, languageCount: 4);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        using var runtime = BuildRuntime(fixture);
        stopwatch.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        GC.KeepAlive(runtime);
        return new LocalizationStartupResult(stopwatch.Elapsed, allocated, 4, 32, 4);
    }

    private static LocalizationHotPathResult MeasureGetHotPath(int operationCount)
    {
        using var runtime = BuildRuntime(catalogCount: 4, unitCount: 32, languageCount: 4);
        ILocalizer localizer = runtime.Localizer;
        _ = localizer.Get(Catalog0ResourceKind.Value);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        var checksum = 0;
        for (var index = 0; index < operationCount; index++)
        {
            checksum += localizer.Get(Catalog0ResourceKind.Value).Length;
        }
        stopwatch.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (checksum == 0)
        {
            throw new InvalidOperationException("The localization hot path returned an empty value.");
        }
        return new LocalizationHotPathResult(stopwatch.Elapsed, allocated, operationCount);
    }

    private static LocalizationSwitchResult MeasureLanguageSwitching(int operationCount)
    {
        using var runtime = BuildRuntime(catalogCount: 4, unitCount: 32, languageCount: 4);
        var host = new Border();
        host.Resources.MergedDictionaries.Add(runtime.ResourceProvider);
        var resourceNotifications = 0;
        var languageEvents = 0;
        ((IResourceHost)host).ResourcesChanged += (_, _) => resourceNotifications++;
        runtime.LanguageManager.LanguageChanged += (_, _) => languageEvents++;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        for (var index = 0; index < operationCount; index++)
        {
            var language = s_languagePool[(index % 3) + 1];
            runtime.LanguageManager.ChangeLanguage(language);
        }
        stopwatch.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        return new LocalizationSwitchResult(
            stopwatch.Elapsed,
            allocated,
            operationCount,
            resourceNotifications,
            languageEvents);
    }

    private static IReadOnlyList<LocalizationMemoryPoint> MeasureSnapshotMemory()
    {
        var matrix = new (int Catalogs, int Units, int Languages)[]
        {
            (1, 8, 2),
            (4, 8, 2),
            (8, 8, 2),
            (1, 32, 2),
            (1, 64, 2),
            (1, 8, 4),
            (1, 8, 6),
            (4, 32, 4),
            (8, 64, 6)
        };
        var points = new List<LocalizationMemoryPoint>(matrix.Length);
        foreach (var dimensions in matrix)
        {
            points.Add(MeasureSnapshotMemoryPoint(dimensions));
        }

        return points;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static LocalizationMemoryPoint MeasureSnapshotMemoryPoint(
        (int Catalogs, int Units, int Languages) dimensions)
    {
        var runtimes = new LocalizationRuntime[MemoryRetentionSampleCount];
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var before = GC.GetTotalMemory(forceFullCollection: true);
        try
        {
            for (var index = 0; index < runtimes.Length; index++)
            {
                runtimes[index] = BuildRuntime(
                    dimensions.Catalogs,
                    dimensions.Units,
                    dimensions.Languages);
            }

            var after = GC.GetTotalMemory(forceFullCollection: true);
            var logicalTextBytes = CountSnapshotTextBytes(runtimes[0]);
            var snapshotSlotCount =
                (long)runtimes[0].Snapshots.Count * dimensions.Catalogs * dimensions.Units;
            GC.KeepAlive(runtimes);
            return new LocalizationMemoryPoint(
                dimensions.Catalogs,
                dimensions.Units,
                dimensions.Languages,
                Math.Max(0, (after - before) / MemoryRetentionSampleCount),
                logicalTextBytes,
                snapshotSlotCount);
        }
        finally
        {
            foreach (var runtime in runtimes)
            {
                runtime?.Dispose();
            }
        }
    }

    private static long CountSnapshotTextBytes(LocalizationRuntime runtime)
    {
        long total = 0;
        for (var snapshotIndex = 0; snapshotIndex < runtime.Snapshots.Count; snapshotIndex++)
        {
            var snapshot = runtime.Snapshots.Values.ElementAt(snapshotIndex);
            for (var catalogSlot = 0; catalogSlot < runtime.Registry.Catalogs.Count; catalogSlot++)
            {
                var catalog = runtime.Registry.Catalogs[catalogSlot];
                for (var unitSlot = 0; unitSlot < catalog.Units.Count; unitSlot++)
                {
                    total += snapshot.GetEntry(catalogSlot, unitSlot).Text.Length * sizeof(char);
                }
            }
        }

        return total;
    }

    private static LanguageCatalogDescriptor CreateCatalog(
        int catalogIndex,
        string catalogId,
        IReadOnlyList<LanguageCatalogUnitDescriptor> units)
    {
        return catalogIndex switch
        {
            0 => new LanguageCatalogDescriptor<Catalog0ResourceKind>(catalogId, 1, units, static _ => 0),
            1 => new LanguageCatalogDescriptor<Catalog1ResourceKind>(catalogId, 1, units, static _ => 0),
            2 => new LanguageCatalogDescriptor<Catalog2ResourceKind>(catalogId, 1, units, static _ => 0),
            3 => new LanguageCatalogDescriptor<Catalog3ResourceKind>(catalogId, 1, units, static _ => 0),
            4 => new LanguageCatalogDescriptor<Catalog4ResourceKind>(catalogId, 1, units, static _ => 0),
            5 => new LanguageCatalogDescriptor<Catalog5ResourceKind>(catalogId, 1, units, static _ => 0),
            6 => new LanguageCatalogDescriptor<Catalog6ResourceKind>(catalogId, 1, units, static _ => 0),
            7 => new LanguageCatalogDescriptor<Catalog7ResourceKind>(catalogId, 1, units, static _ => 0),
            _ => throw new ArgumentOutOfRangeException(nameof(catalogIndex))
        };
    }

    private static string RenderMarkdown(
        LocalizationStartupResult startup,
        LocalizationHotPathResult hotPath,
        LocalizationSwitchResult switching,
        IReadOnlyList<LocalizationMemoryPoint> memory)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Localization Baseline");
        builder.AppendLine();
        builder.AppendLine($"- Date: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine("- Runner: `tools/performances/AtomUI.Performance`");
        builder.AppendLine("- Runtime input: generated Catalog descriptors and compiled translation strings; no XLIFF parsing");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value |");
        builder.AppendLine("| --- | ---: |");
        builder.AppendLine($"| Snapshot startup build (ms) | {startup.Elapsed.TotalMilliseconds:0.00} |");
        builder.AppendLine($"| Snapshot startup allocation (KiB) | {startup.AllocatedBytes / 1024.0:0.0} |");
        builder.AppendLine($"| ILocalizer.Get calls | {hotPath.OperationCount} |");
        builder.AppendLine($"| ILocalizer.Get (ns/call) | {hotPath.Elapsed.TotalNanoseconds() / hotPath.OperationCount:0.0} |");
        builder.AppendLine($"| ILocalizer.Get allocation (KiB) | {hotPath.AllocatedBytes / 1024.0:0.0} |");
        builder.AppendLine($"| Language switch changes | {switching.OperationCount} |");
        builder.AppendLine($"| Language switch (ns/change) | {switching.Elapsed.TotalNanoseconds() / switching.OperationCount:0.0} |");
        builder.AppendLine($"| Language switch allocation (KiB) | {switching.AllocatedBytes / 1024.0:0.0} |");
        builder.AppendLine($"| Resource notifications | {switching.ResourceNotificationCount} |");
        builder.AppendLine($"| Language events | {switching.LanguageEventCount} |");
        builder.AppendLine();
        builder.AppendLine("## Snapshot Memory");
        builder.AppendLine();
        builder.AppendLine("| Catalogs | Units/Catalog | Languages | Retained KiB | Logical text KiB | Snapshot slots |");
        builder.AppendLine("| ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var point in memory)
        {
            builder.AppendLine($"| {point.CatalogCount} | {point.UnitCount} | {point.LanguageCount} | " +
                               $"{point.RetainedBytes / 1024.0:0.0} | {point.LogicalTextBytes / 1024.0:0.0} | {point.SnapshotSlotCount} |");
        }
        builder.AppendLine();
        builder.AppendLine("Notes:");
        builder.AppendLine();
        builder.AppendLine("- Measurements run in a deterministic in-process runtime with UI-thread checks disabled only for the benchmark harness.");
        builder.AppendLine($"- Retained memory is a coarse process GC measurement averaged across {MemoryRetentionSampleCount} live runtimes; logical text bytes and slot counts show the expected scaling signal.");
        return builder.ToString();
    }

    private static double TotalNanoseconds(this TimeSpan timeSpan)
    {
        return timeSpan.TotalMilliseconds * 1_000_000;
    }

    private sealed record LocalizationStartupResult(
        TimeSpan Elapsed,
        long AllocatedBytes,
        int CatalogCount,
        int UnitCount,
        int LanguageCount);

    private sealed record LocalizationHotPathResult(TimeSpan Elapsed, long AllocatedBytes, int OperationCount);

    private sealed record LocalizationSwitchResult(
        TimeSpan Elapsed,
        long AllocatedBytes,
        int OperationCount,
        int ResourceNotificationCount,
        int LanguageEventCount);

    internal sealed record LocalizationMemoryPoint(
        int CatalogCount,
        int UnitCount,
        int LanguageCount,
        long RetainedBytes,
        long LogicalTextBytes,
        long SnapshotSlotCount);

    private sealed record LocalizationFixture(
        IReadOnlyList<LanguageCatalogDescriptor> Catalogs,
        IReadOnlyList<TranslationBundleDescriptor> Bundles,
        IReadOnlyList<LanguageTag> Languages);

    internal enum Catalog0ResourceKind { Value }
    private enum Catalog1ResourceKind { Value }
    private enum Catalog2ResourceKind { Value }
    private enum Catalog3ResourceKind { Value }
    private enum Catalog4ResourceKind { Value }
    private enum Catalog5ResourceKind { Value }
    private enum Catalog6ResourceKind { Value }
    private enum Catalog7ResourceKind { Value }
}
