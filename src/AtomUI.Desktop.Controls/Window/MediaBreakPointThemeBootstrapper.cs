using System.Runtime.CompilerServices;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 在 Window 的 ControlTheme 里程序化注入媒体断点 ContainerQuery。
///
/// 断点数值来自当前 Theme 的 Alias Token (ScreenSMMin/ScreenSMMax 等),使得开发者
/// 通过主题配置文件覆盖 Alias Token 就能改断点,不需要动 axaml 里的字面量。
/// 订阅 ThemeManager.ThemeChanged,每次 committed theme 切换后重建 CQ。
///
/// 注意:CQ 必须按从大到小的顺序注入 (ExtraExtraExtraLarge → ExtraSmall)。
/// Avalonia 在跨断点 resize 过程中 activator 的评估顺序与声明顺序相关,
/// 经实测只有这个顺序下所有方向的断点切换都能正确 fire。
/// </summary>
internal sealed class MediaBreakPointThemeBootstrapper : IDisposable
{
    // 注意:ContainerQuery.SetParent 只允许 ControlTheme 作为 parent,不接受 null,
    // 意味着一旦加入 ControlTheme.Children 就无法 Remove。因此首次加载后缓存 CQ,
    // 后续主题切换只就地更新 Query (断点阈值),不再增删节点。
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
