using System.Runtime.CompilerServices;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Injects the media breakpoint <see cref="ContainerQuery" /> items into the <see cref="ControlTheme" /> of
/// <see cref="Window" /> from code. The breakpoint values come from the alias tokens of the committed theme
/// snapshot (ScreenSMMin, ScreenSMMax and so on), so overriding them in the theme configuration changes the
/// breakpoints without editing literals in AXAML.
/// </summary>
/// <remarks>
/// <see cref="ThemeManager.ControlThemeResolved" /> adds the queries when the Window theme is resolved, and
/// <see cref="ThemeManager.ThemeChanged" /> updates their breakpoints in place after each committed theme switch.
/// The queries must be added from the largest breakpoint to the smallest (ExtraExtraExtraLarge to ExtraSmall),
/// in the order of <see cref="Rows" />: while a resize crosses breakpoints, Avalonia evaluates the activators in
/// declaration order, and only this order was measured to make the breakpoint change fire correctly in every
/// direction.
/// </remarks>
internal sealed class MediaBreakPointThemeBootstrapper : IDisposable
{
    // ContainerQuery.SetParent only accepts a ControlTheme as parent and never null, so a query cannot be removed
    // once it is in ControlTheme.Children. The queries are therefore cached after the first load, and a later
    // theme switch only updates their Query (the breakpoint thresholds) in place instead of adding or removing
    // nodes.
    private readonly ConditionalWeakTable<ControlTheme, ThemeQueries> _themeQueries = new();
    private readonly List<WeakReference<ThemeQueries>> _liveThemes = new();
    private ThemeManager? _themeManager;

    private static readonly (Func<ThemeSnapshot, double?> Min, Func<ThemeSnapshot, double?> Max, MediaBreakPoint Bp)[] Rows =
    {
        (t => Global(t, SharedTokenKind.ScreenXXXLMin), _ => null, MediaBreakPoint.ExtraExtraExtraLarge),
        (t => Global(t, SharedTokenKind.ScreenXXLMin), t => Global(t, SharedTokenKind.ScreenXXLMax), MediaBreakPoint.ExtraExtraLarge),
        (t => Global(t, SharedTokenKind.ScreenXLMin), t => Global(t, SharedTokenKind.ScreenXLMax), MediaBreakPoint.ExtraLarge),
        (t => Global(t, SharedTokenKind.ScreenLGMin), t => Global(t, SharedTokenKind.ScreenLGMax), MediaBreakPoint.Large),
        (t => Global(t, SharedTokenKind.ScreenMDMin), t => Global(t, SharedTokenKind.ScreenMDMax), MediaBreakPoint.Medium),
        (t => Global(t, SharedTokenKind.ScreenSMMin), t => Global(t, SharedTokenKind.ScreenSMMax), MediaBreakPoint.Small),
        (_ => null, t => Global(t, SharedTokenKind.ScreenSMMin) - 1, MediaBreakPoint.ExtraSmall),
    };

    private MediaBreakPointThemeBootstrapper(ThemeManager themeManager)
    {
        _themeManager = themeManager;
        themeManager.ThemeChanged  += HandleThemeChanged;
        themeManager.ControlThemeResolved += HandleControlThemeResolved;
    }

    public static IDisposable Attach(ThemeManager themeManager) => new MediaBreakPointThemeBootstrapper(themeManager);

    public void Dispose()
    {
        var manager = Interlocked.Exchange(ref _themeManager, null);
        if (manager is not null)
        {
            manager.ThemeChanged -= HandleThemeChanged;
            manager.ControlThemeResolved -= HandleControlThemeResolved;
        }
        _themeQueries.Clear();
        _liveThemes.Clear();
    }

    private void HandleControlThemeResolved(object key, ControlTheme theme)
    {
        if (key is not Type owner || owner != typeof(Window) || _themeManager?.CurrentSnapshot is not { } snapshot)
        {
            return;
        }
        if (_themeQueries.TryGetValue(theme, out var existing))
        {
            Update(existing, snapshot);
            return;
        }
        // StyleQuery is computed from the committed Token snapshot; AXAML cannot express
        // this runtime min/max query while retaining the exact ContainerQuery contract.
        var queries = Rows.Select(row => BuildContainerQuery(row.Min(snapshot), row.Max(snapshot), row.Bp)).ToArray();
        foreach (var query in queries)
        {
            theme.Children.Add(query);
        }
        var state = new ThemeQueries(queries, snapshot.GlobalTokenValues);
        _themeQueries.Add(theme, state);
        for (var index = _liveThemes.Count - 1; index >= 0; index--)
        {
            if (!_liveThemes[index].TryGetTarget(out _))
            {
                _liveThemes.RemoveAt(index);
            }
        }
        _liveThemes.Add(new WeakReference<ThemeQueries>(state));
    }

    private void HandleThemeChanged(object? sender, ThemeChangedEventArgs args)
    {
        if (sender is not ThemeManager tm)
        {
            return;
        }

        if (tm.CurrentSnapshot is { } snapshot)
        {
            for (var index = _liveThemes.Count - 1; index >= 0; index--)
            {
                if (_liveThemes[index].TryGetTarget(out var state))
                {
                    Update(state, snapshot);
                }
                else
                {
                    _liveThemes.RemoveAt(index);
                }
            }
        }
    }

    private static void Update(ThemeQueries state, ThemeSnapshot snapshot)
    {
        if (ReferenceEquals(state.Values, snapshot.GlobalTokenValues))
        {
            return;
        }

        for (var index = 0; index < Rows.Length; index++)
        {
            var (min, max, _) = Rows[index];
            state.Queries[index].Query = BuildStyleQuery(min(snapshot), max(snapshot));
        }
        state.Values = snapshot.GlobalTokenValues;
    }

    private sealed class ThemeQueries(ContainerQuery[] queries, TokenValueTable values)
    {
        internal ContainerQuery[] Queries { get; } = queries;
        internal TokenValueTable Values { get; set; } = values;
    }

    private static int Global(ThemeSnapshot snapshot, SharedTokenKind token)
    {
        return snapshot.GlobalTokenValues.Get<int>((int)token);
    }

    private static StyleQuery? BuildStyleQuery(double? min, double? max)
    {
        StyleQuery? query = null;
        if (min.HasValue)
        {
            query = query.Width(StyleQueryComparisonOperator.GreaterThanOrEquals, min.Value);
        }
        if (max.HasValue)
        {
            query = query.Width(StyleQueryComparisonOperator.LessThanOrEquals, max.Value);
        }
        return query;
    }

    private static ContainerQuery BuildContainerQuery(double? min, double? max, MediaBreakPoint bp)
    {
        var cq = new ContainerQuery
        {
            Name  = IMediaBreakAwareControl.GlobalQueryContainerName,
            Query = BuildStyleQuery(min, max)
        };

        var style = new Style(s => s.OfType<MediaBreakPointIndicator>().Name(MediaBreakPointIndicator.MediaQueryIndicatorName));
        style.Setters.Add(new Setter(MediaBreakPointIndicator.MediaBreakPointProperty, bp));
        cq.Children.Add(style);

        return cq;
    }
}
