using System.Diagnostics;
using System.Text;
using AtomUI.Theme;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Testing;
using Avalonia.Threading;

namespace AtomUI.DesignLanguages.Shadcn.Tests.TokenValues;

/// <summary>
/// Writes the compiled token values of every shadcn variant under every seed case the pre-refactor Gallery dumped
/// (<c>tools/performances/AtomUI.GalleryPerformance/TokenDumpProbe.cs</c>), one file per variant and case in the
/// <see cref="ThemeSnapshotDump"/> format, named like the baseline: <c>Shadcn.&lt;Style&gt;.&lt;Light|Dark&gt;.&lt;Normal|Compact&gt;[@case].tokens.txt</c>.
/// </summary>
internal static class ShadcnTokenDumps
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(60);

    // shadcn Base Colors: the 500 stop of each Tailwind v4.3.5 neutral family.
    private static readonly (string Name, string Color)[] s_baseColors =
    [
        ("neutral", "#737373"), ("stone", "#79716B"), ("zinc", "#71717B"), ("mauve", "#79697B"),
        ("olive", "#7C7C67"), ("mist", "#67787C"), ("taupe", "#7C6D67")
    ];

    // shadcn Themes: the 700 stop (400 for yellow) of the Tailwind v4.3.5 family.
    private static readonly (string Name, string Color)[] s_themes =
    [
        ("blue", "#1447E6"), ("rose", "#C70036"), ("green", "#008236"), ("violet", "#7008E7"), ("yellow", "#FDC700")
    ];

    /// <summary>Every variant and seed case with the request selection the Gallery sends for it.</summary>
    public static IEnumerable<(string FileName, ThemeSelection Selection)> Cases()
    {
        foreach (var style in ShadcnStyles.All)
        {
            foreach (var appearance in new[] { ThemeAppearance.Light, ThemeAppearance.Dark })
            {
                foreach (var compact in new[] { false, true })
                {
                    var key = $"Shadcn.{style.Id}.{appearance}.{(compact ? "Compact" : "Normal")}";
                    var selection = ThemeSelection.Of("Shadcn", style).WithAppearance(appearance);
                    if (compact)
                    {
                        selection = selection.WithDensity(DesignDensity.Compact);
                    }

                    yield return ($"{key}.tokens.txt", selection);
                    foreach (var (name, color) in s_baseColors)
                    {
                        yield return ($"{key}@base-{name}.tokens.txt",
                            selection.WithLanguageSeed(nameof(ShadcnToken.BaseColor), color));
                    }

                    foreach (var (name, color) in s_themes)
                    {
                        yield return ($"{key}@theme-{name}.tokens.txt",
                            selection.WithSeed(nameof(SharedTokenKind.ColorPrimary), color));
                    }
                }
            }
        }
    }

    /// <summary>Applies each case to <paramref name="manager"/> and writes its dump; returns the files written.</summary>
    public static int WriteAll(ThemeManager manager, string directory)
    {
        Directory.CreateDirectory(directory);
        var written = 0;
        foreach (var (fileName, selection) in Cases())
        {
            File.WriteAllText(Path.Combine(directory, fileName), Dump(manager, selection), Encoding.UTF8);
            written++;
        }

        return written;
    }

    /// <summary>The dump of one selection applied to <paramref name="manager"/>.</summary>
    public static string Dump(ThemeManager manager, ThemeSelection selection)
    {
        var request = ThemeSelectionResolver.Resolve(manager.Languages, manager.Themes, selection).Request;
        var result = WaitFor(manager.ApplyThemeAsync(request));
        if (result.Status is not (ThemeTransitionStatus.Committed or ThemeTransitionStatus.NoOp))
        {
            throw new InvalidOperationException(
                $"request for '{request.ThemeId}' ended as {result.Status}: " +
                string.Join(" ", result.Diagnostics.Select(static diagnostic => diagnostic.Message)));
        }

        var snapshot = manager.CurrentSnapshot ?? throw new InvalidOperationException("no current snapshot after the request.");
        return ThemeSnapshotDump.Dump(snapshot);
    }

    private static T WaitFor<T>(Task<T> task)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!task.IsCompleted)
        {
            if (stopwatch.Elapsed > s_timeout)
            {
                throw new TimeoutException($"Timed out after {s_timeout.TotalSeconds:0}s.");
            }

            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        return task.GetAwaiter().GetResult();
    }
}
