using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A masonry (waterfall) layout control that organizes children of uneven heights into columns
/// using stable-column assignments by default, with classic shortest-column reflow available
/// through <see cref="LayoutStrategy"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Masonry"/> derives from <see cref="ItemsControl"/> and supports two content styles:
/// placing arbitrary <see cref="Control"/> children directly (the child itself acts as its container,
/// no extra visual layer), or binding a data collection via <see cref="ItemsControl.ItemsSource"/>
/// (the base class generates <c>ContentPresenter</c> containers). Container generation, collection
/// change synchronization and container recycling are provided by the base class.
/// </para>
/// <para>
/// The layout engine is the internal <c>MasonryPanel</c> assembled as the <c>ItemsPanel</c> by
/// the control theme; it is not exposed to developers.
/// </para>
/// </remarks>
public partial class Masonry : ItemsControl
{
    #region 公共属性定义

    /// <summary>
    /// Defines the <see cref="ColumnCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int> ColumnCountProperty =
        AvaloniaProperty.Register<Masonry, int>(nameof(ColumnCount));

    /// <summary>
    /// Defines the <see cref="ColumnInfo"/> property.
    /// </summary>
    public static readonly StyledProperty<ResponsiveInt?> ColumnInfoProperty =
        AvaloniaProperty.Register<Masonry, ResponsiveInt?>(nameof(ColumnInfo));

    /// <summary>
    /// Defines the <see cref="MinColumnWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinColumnWidthProperty =
        AvaloniaProperty.Register<Masonry, double>(nameof(MinColumnWidth), 320d);

    /// <summary>
    /// Defines the <see cref="MaxColumnCount"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxColumnCountProperty =
        AvaloniaProperty.Register<Masonry, int>(nameof(MaxColumnCount), 4);

    /// <summary>
    /// Defines the <see cref="ColumnGap"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnGapProperty =
        AvaloniaProperty.Register<Masonry, double>(nameof(ColumnGap), 16d);

    /// <summary>
    /// Defines the <see cref="RowGap"/> property.
    /// </summary>
    public static readonly StyledProperty<double> RowGapProperty =
        AvaloniaProperty.Register<Masonry, double>(nameof(RowGap), 16d);

    /// <summary>
    /// Defines the <see cref="Gutter"/> property.
    /// </summary>
    public static readonly StyledProperty<ResponsiveGutter?> GutterProperty =
        AvaloniaProperty.Register<Masonry, ResponsiveGutter?>(nameof(Gutter));

    /// <summary>
    /// Defines the <see cref="LayoutStrategy"/> property.
    /// </summary>
    public static readonly StyledProperty<MasonryLayoutStrategy> LayoutStrategyProperty =
        AvaloniaProperty.Register<Masonry, MasonryLayoutStrategy>(
            nameof(LayoutStrategy),
            MasonryLayoutStrategy.StableColumns);

    /// <summary>
    /// Defines the attached <c>Masonry.Column</c> property, which pins a child to a specific
    /// column. A <c>null</c> value (the default) delegates automatic placement to
    /// <see cref="LayoutStrategy"/>.
    /// </summary>
    public static readonly AttachedProperty<int?> ColumnProperty =
        AvaloniaProperty.RegisterAttached<Masonry, Control, int?>("Column");

    /// <summary>
    /// Defines the attached <c>Masonry.Span</c> property, which controls how a child occupies
    /// columns. Defaults to <see cref="MasonryItemSpan.Auto"/>.
    /// </summary>
    public static readonly AttachedProperty<MasonryItemSpan> SpanProperty =
        AvaloniaProperty.RegisterAttached<Masonry, Control, MasonryItemSpan>("Span", MasonryItemSpan.Auto);

    /// <summary>
    /// Gets or sets the fixed number of columns. When greater than zero, Masonry uses a fixed
    /// column count. When less than or equal to zero, Masonry computes the effective column count
    /// from <see cref="MinColumnWidth"/>, <see cref="MaxColumnCount"/> and the available width.
    /// Defaults to <c>0</c> (container adaptive).
    /// </summary>
    public int ColumnCount
    {
        get => GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the responsive column count. When the current breakpoint is configured,
    /// this value takes precedence over <see cref="ColumnCount"/>.
    /// </summary>
    public ResponsiveInt? ColumnInfo
    {
        get => GetValue(ColumnInfoProperty);
        set => SetValue(ColumnInfoProperty, value);
    }

    /// <summary>
    /// Gets or sets the target minimum width of a single column used by the container-adaptive
    /// column count computation. Defaults to <c>320</c>.
    /// </summary>
    public double MinColumnWidth
    {
        get => GetValue(MinColumnWidthProperty);
        set => SetValue(MinColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the upper bound for the adaptive column count to prevent runaway column
    /// counts on wide screens. Defaults to <c>4</c>.
    /// </summary>
    public int MaxColumnCount
    {
        get => GetValue(MaxColumnCountProperty);
        set => SetValue(MaxColumnCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal gap between columns. Negative, <see cref="double.NaN"/>
    /// and infinite values are normalized to a valid non-negative value in the effective state.
    /// Defaults to <c>16</c>.
    /// </summary>
    public double ColumnGap
    {
        get => GetValue(ColumnGapProperty);
        set => SetValue(ColumnGapProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical gap between rows. Negative, <see cref="double.NaN"/>
    /// and infinite values are normalized to a valid non-negative value in the effective state.
    /// Defaults to <c>16</c>.
    /// </summary>
    public double RowGap
    {
        get => GetValue(RowGapProperty);
        set => SetValue(RowGapProperty, value);
    }

    /// <summary>
    /// Gets or sets the responsive horizontal and vertical gutter. When the current breakpoint is
    /// configured, this value takes precedence over <see cref="ColumnGap"/> and <see cref="RowGap"/>.
    /// </summary>
    public ResponsiveGutter? Gutter
    {
        get => GetValue(GutterProperty);
        set => SetValue(GutterProperty, value);
    }

    /// <summary>
    /// Gets or sets the strategy used to assign automatic items to columns. The default
    /// <see cref="MasonryLayoutStrategy.StableColumns"/> keeps existing items in their columns
    /// while the effective column count is unchanged. <see cref="MasonryLayoutStrategy.Reflow"/>
    /// recomputes automatic assignments from the current shortest column on each layout calculation.
    /// </summary>
    public MasonryLayoutStrategy LayoutStrategy
    {
        get => GetValue(LayoutStrategyProperty);
        set => SetValue(LayoutStrategyProperty, value);
    }

    /// <summary>Gets the value of the attached <c>Masonry.Column</c> property.</summary>
    public static int? GetColumn(Control element) => element.GetValue(ColumnProperty);

    /// <summary>Sets the value of the attached <c>Masonry.Column</c> property.</summary>
    public static void SetColumn(Control element, int? value) => element.SetValue(ColumnProperty, value);

    /// <summary>Gets the value of the attached <c>Masonry.Span</c> property.</summary>
    public static MasonryItemSpan GetSpan(Control element) => element.GetValue(SpanProperty);

    /// <summary>Sets the value of the attached <c>Masonry.Span</c> property.</summary>
    public static void SetSpan(Control element, MasonryItemSpan value) => element.SetValue(SpanProperty, value);

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the effective column assignment of children changes. The event only notifies
    /// column assignment changes, not pixel-level arrange rectangles, and is dispatched outside
    /// of the Avalonia layout pass to prevent re-entrancy.
    /// </summary>
    public event EventHandler<MasonryLayoutChangedEventArgs>? LayoutChanged;

    #endregion

    #region 内部属性定义

    /// <summary>
    /// Defines the internal <see cref="MotionDuration"/> property. Item appear fade and position
    /// glide duration, fed from the <c>MotionDurationSlow</c> token by the control theme.
    /// Zero (global motion disabled) degrades all item motions to instantaneous.
    /// </summary>
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<Masonry>();

    /// <summary>
    /// Defines the internal <see cref="LeaveMotionDuration"/> property. Removed item fade-out
    /// duration, fed from the <c>MotionDurationFast</c> token by the control theme.
    /// </summary>
    internal static readonly StyledProperty<TimeSpan> LeaveMotionDurationProperty =
        AvaloniaProperty.Register<Masonry, TimeSpan>(nameof(LeaveMotionDuration), TimeSpan.FromMilliseconds(100));

    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    internal TimeSpan LeaveMotionDuration
    {
        get => GetValue(LeaveMotionDurationProperty);
        set => SetValue(LeaveMotionDurationProperty, value);
    }

    #endregion

    static Masonry()
    {
        // Layout properties live on Masonry; the internal MasonryPanel reads them via
        // RelativeSource binding assembled by the control theme. A change here must trigger
        // a re-measure of MasonryPanel (the ItemsPanel).
        AffectsMeasure<Masonry>(
            ColumnCountProperty,
            ColumnInfoProperty,
            MinColumnWidthProperty,
            MaxColumnCountProperty,
            ColumnGapProperty,
            RowGapProperty,
            GutterProperty,
            LayoutStrategyProperty);
        ColumnProperty.Changed.AddClassHandler<Control>(HandleItemLayoutPropertyChanged);
        SpanProperty.Changed.AddClassHandler<Control>(HandleItemLayoutPropertyChanged);
    }

    #region 离场动效（ghost 层托管）

    // antd motionEaseOut == CubicEaseOut（与 MasonryPanel.MotionEasing 一致）
    private static readonly CubicEaseOut MotionEasing = new();
    private Canvas? _motionGhostLayer;
    private readonly List<Border> _motionGhostHosts = new();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _motionGhostLayer = e.NameScope.Find<Canvas>("PART_MotionGhostLayer");
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClearMotionGhosts();
    }

    /// <summary>面板移除子项后调用：将被移除容器移入 ghost 层在原位置淡出。</summary>
    internal void NotifyItemsRemoved(MasonryPanel source, IReadOnlyList<MasonryPanel.MasonryRemovedItem> removedItems)
    {
        if (_motionGhostLayer is null)
        {
            return;
        }
        foreach (var removed in removedItems)
        {
            HostMotionGhost(source, removed);
        }
    }

    private void HostMotionGhost(MasonryPanel source, MasonryPanel.MasonryRemovedItem removed)
    {
        var duration = LeaveMotionDuration;
        if (duration <= TimeSpan.Zero)
        {
            return; // 禁用动效：与上游一致直接消失
        }
        var layer = _motionGhostLayer!;
        var origin = source.TranslatePoint(default, layer).GetValueOrDefault();
        var isRtl  = FlowDirection == FlowDirection.RightToLeft;
        var x      = isRtl ? layer.Bounds.Width - removed.Rect.Right : removed.Rect.X;
        var host   = new Border
        {
            Width  = removed.Rect.Width,
            Height = removed.Rect.Height,
            IsHitTestVisible = false,
            Child  = removed.Container
        };
        Canvas.SetLeft(host, origin.X + x);
        Canvas.SetTop(host, origin.Y + removed.Rect.Y);
        layer.Children.Add(host);
        _motionGhostHosts.Add(host);
        _ = FadeOutGhostAsync(host, duration);
    }

    private async Task FadeOutGhostAsync(Border host, TimeSpan duration)
    {
        try
        {
            var animation = new Animation
            {
                Duration = duration,
                Easing   = MotionEasing,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0d), Setters = { new Setter(Visual.OpacityProperty, 1d) } },
                    new KeyFrame { Cue = new Cue(1d), Setters = { new Setter(Visual.OpacityProperty, 0d) } },
                }
            };
            await animation.RunAsync(host);
        }
        catch (OperationCanceledException) { }
        ReleaseMotionGhost(host);
    }

    /// <summary>容器重加入面板前由面板调用：立即归还容器归属，避免双视觉父级。</summary>
    internal bool TryReleaseMotionGhost(Control container)
    {
        for (var i = 0; i < _motionGhostHosts.Count; i++)
        {
            if (ReferenceEquals(_motionGhostHosts[i].Child, container))
            {
                ReleaseMotionGhost(_motionGhostHosts[i]);
                return true;
            }
        }
        return false;
    }

    private void ReleaseMotionGhost(Border host)
    {
        host.Child = null; // 归还容器归属（用户直接子元素场景容器归用户所有）
        _motionGhostHosts.Remove(host);
        if (host.Parent is Canvas layer)
        {
            layer.Children.Remove(host);
        }
    }

    private void ClearMotionGhosts()
    {
        foreach (var host in _motionGhostHosts.ToArray())
        {
            host.Child = null;
            if (host.Parent is Canvas layer)
            {
                layer.Children.Remove(host);
            }
        }
        _motionGhostHosts.Clear();
    }

    #endregion

    /// <summary>
    /// Called by the internal layout engine after a layout pass produced a new effective column
    /// assignment. Dispatched outside the layout pass to avoid re-entrancy.
    /// </summary>
    internal void NotifyLayoutChanged(IReadOnlyList<MasonryItemLayout> items)
    {
        Dispatcher.Post(() => LayoutChanged?.Invoke(this, new MasonryLayoutChangedEventArgs(items)));
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (!container.Classes.Contains(MasonrySemanticParts.ItemClass))
        {
            container.Classes.Add(MasonrySemanticParts.ItemClass);
        }
    }

    private static void HandleItemLayoutPropertyChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        if (control.GetVisualParent() is MasonryPanel panel)
        {
            panel.InvalidateStableAssignments();
        }
    }

}
