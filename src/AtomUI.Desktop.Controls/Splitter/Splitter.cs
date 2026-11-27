using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

/// <summary>
/// Lays its children out as resizable panels separated by draggable split bars, along the axis chosen by
/// <see cref="Orientation" />. The size, the size limits, the collapse settings and the collapse state of a panel
/// are attached properties that are set on the panel itself, for example <see cref="SizeProperty" /> and
/// <see cref="CollapsibleProperty" />.
/// </summary>
public partial class Splitter : TemplatedControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Splitter, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// Identifies the <see cref="IsLazy" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLazyProperty =
        AvaloniaProperty.Register<Splitter, bool>(nameof(IsLazy));

    /// <summary>
    /// Identifies the <see cref="HandleSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HandleSizeProperty =
        AvaloniaProperty.Register<Splitter, double>(nameof(HandleSize));

    /// <summary>
    /// Identifies the <see cref="LineThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> LineThicknessProperty =
        AvaloniaProperty.Register<Splitter, double>(nameof(LineThickness));

    /// <summary>
    /// Identifies the <see cref="LineCornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> LineCornerRadiusProperty =
        AvaloniaProperty.Register<Splitter, CornerRadius>(nameof(LineCornerRadius));

    /// <summary>
    /// Identifies the <see cref="BorderDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<Splitter, IReadOnlyList<double>?>(nameof(BorderDashArray));

    /// <summary>
    /// Identifies the <see cref="BorderDashOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<Splitter, double>(nameof(BorderDashOffset));

    /// <summary>
    /// Identifies the <see cref="CollapsePreviousIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> CollapsePreviousIconProperty =
        AvaloniaProperty.Register<Splitter, IconTemplate?>(nameof(CollapsePreviousIcon));

    /// <summary>
    /// Identifies the <see cref="CollapseNextIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<IconTemplate?> CollapseNextIconProperty =
        AvaloniaProperty.Register<Splitter, IconTemplate?>(nameof(CollapseNextIcon));

    /// <summary>
    /// Direction of the split bars, and so of the axis along which the panels are sized.
    /// <see cref="Avalonia.Layout.Orientation.Vertical" />, the default, places the panels side by side from left to
    /// right and sizes them by width; <see cref="Avalonia.Layout.Orientation.Horizontal" /> stacks them from top to
    /// bottom and sizes them by height.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Delays the resizing of the panels until a drag ends. With <see langword="true" /> only the dragged split bar
    /// moves while the pointer moves and the panels are resized once when the drag is completed. With
    /// <see langword="false" />, the default, the panels are resized on every drag delta.
    /// </summary>
    public bool IsLazy
    {
        get => GetValue(IsLazyProperty);
        set => SetValue(IsLazyProperty, value);
    }

    /// <summary>
    /// Length reserved along the split axis for the handle of each split bar, in pixels. The handle is measured with
    /// this length and with the full extent of the splitter across the split axis. The theme sets it from
    /// <see cref="SplitterToken.SplitBarHandleSize" />.
    /// </summary>
    public double HandleSize
    {
        get => GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    /// <summary>
    /// Thickness of the divider line and of the drag bar of every split bar, measured across the split axis: the
    /// width of a vertical drag bar and the height of a horizontal one. The theme sets it from
    /// <see cref="SplitterToken.HandleLineThickness" />.
    /// </summary>
    public double LineThickness
    {
        get => GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    /// <summary>
    /// Corner radius of the divider line and of the drag bar of every split bar. The theme sets it from the
    /// <c>BorderRadiusXS</c> token.
    /// </summary>
    public CornerRadius LineCornerRadius
    {
        get => GetValue(LineCornerRadiusProperty);
        set => SetValue(LineCornerRadiusProperty, value);
    }

    /// <summary>
    /// The dash pattern of the frame border stroke, as alternating dash and gap lengths in device independent units.
    /// <see langword="null" /> or an empty list paints a solid border. The pattern is visible only when
    /// <c>BorderBrush</c> and <c>BorderThickness</c> give the border a stroke.
    /// </summary>
    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    /// <summary>
    /// The distance along the frame border at which the dash pattern given by <see cref="BorderDashArray" /> starts,
    /// in the same units as the pattern. It has no effect on a solid border; the default is 0.
    /// </summary>
    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    /// <summary>
    /// Template of the icon on a split bar that collapses or expands the panel before the bar. The icon is shown only
    /// when that panel is collapsible. <see langword="null" />, the default, uses the built-in icon, which points
    /// toward that panel.
    /// </summary>
    public IconTemplate? CollapsePreviousIcon
    {
        get => GetValue(CollapsePreviousIconProperty);
        set => SetValue(CollapsePreviousIconProperty, value);
    }

    /// <summary>
    /// Template of the icon on a split bar that collapses or expands the panel after the bar. The icon is shown only
    /// when that panel is collapsible. <see langword="null" />, the default, uses the built-in icon, which points
    /// toward that panel.
    /// </summary>
    public IconTemplate? CollapseNextIcon
    {
        get => GetValue(CollapseNextIconProperty);
        set => SetValue(CollapseNextIconProperty, value);
    }
    
    /// <summary>
    /// The panels of the splitter, in the order in which they are arranged along the split axis. This is the content
    /// property, so the direct child elements of the splitter in XAML are added here.
    /// </summary>
    [Content]
    public ControlList Children { get; } = new ();

    #endregion
    
    #region 布局项附加属性
    
    /// <summary>
    /// Attached property for the size of a panel. It is set on the direct child elements of the splitter, in XAML as
    /// <c>Splitter.Size</c>, as a <see cref="Dimension" /> in pixels or as a percentage of the length available to
    /// the panels. <see langword="null" />, the default, leaves the size to <see cref="DefaultSizeProperty" />. The
    /// splitter also writes sizes into this property while it resizes the panels: in the unit of this property, or of
    /// <see cref="DefaultSizeProperty" /> when the size is not set, or in pixels. Bindings are two-way by default, so
    /// a bound source receives the current size.
    /// </summary>
    public static readonly AttachedProperty<Dimension?> SizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("Size",
            defaultBindingMode: BindingMode.TwoWay);
    
    /// <summary>
    /// Attached property for the size a panel is laid out with while <see cref="SizeProperty" /> is not set on it. It
    /// is set on the direct child elements of the splitter, in XAML as <c>Splitter.DefaultSize</c>, and takes the
    /// same <see cref="Dimension" /> as <see cref="SizeProperty" />. The splitter never writes into this property, so
    /// it stays the size a panel is expanded to when no earlier size is known. A panel with neither size takes its
    /// share of the length that the other panels leave.
    /// </summary>
    public static readonly AttachedProperty<Dimension?> DefaultSizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("DefaultSize");
    
    /// <summary>
    /// Attached property for the smallest length a panel keeps. It is set on the direct child elements of the
    /// splitter, in XAML as <c>Splitter.MinSize</c>, and takes the same <see cref="Dimension" /> as
    /// <see cref="SizeProperty" />. <see langword="null" />, the default, lets the panel shrink to zero. Dragging and
    /// expanding stop at this length; it does not keep a collapsed panel open.
    /// </summary>
    public static readonly AttachedProperty<Dimension?> MinSizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("MinSize");
    
    /// <summary>
    /// Attached property for the largest length a panel can take. It is set on the direct child elements of the
    /// splitter, in XAML as <c>Splitter.MaxSize</c>, and takes the same <see cref="Dimension" /> as
    /// <see cref="SizeProperty" />. <see langword="null" />, the default, leaves the length unbounded. Dragging,
    /// expanding and the distribution of spare length stop at it. A value below <see cref="MinSizeProperty" /> is
    /// treated as that minimum.
    /// </summary>
    public static readonly AttachedProperty<Dimension?> MaxSizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("MaxSize");
    
    /// <summary>
    /// Attached property that tells whether a panel takes part in resizing. It is set on the direct child elements of
    /// the splitter, in XAML as <c>Splitter.IsResizable</c>; the default is <see langword="true" />. The split bar
    /// between two panels can be dragged, and can collapse or expand one of them, only when both panels are resizable.
    /// </summary>
    public static readonly AttachedProperty<bool> IsResizableProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, bool>("IsResizable", true);
    
    /// <summary>
    /// Attached property with the collapse settings of a panel. It is set on the direct child elements of the
    /// splitter, in XAML as <c>Splitter.Collapsible</c>. <see langword="null" />, the default, keeps the panel from
    /// being collapsed or expanded from a split bar. A panel is collapsible only while
    /// <see cref="SplitterPanelCollapsible.IsEnabled" /> is <see langword="true" /> in its settings, and the split
    /// bars next to it show their collapse icons as <see cref="SplitterPanelCollapsible.ShowCollapsibleIcon" /> says.
    /// </summary>
    public static readonly AttachedProperty<SplitterPanelCollapsible?> CollapsibleProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, SplitterPanelCollapsible?>("Collapsible");
    
    /// <summary>
    /// Attached property that tells whether a panel is collapsed. It is set on the direct child elements of the
    /// splitter, in XAML as <c>Splitter.IsCollapsed</c>; the default is <see langword="false" />. A collapsed panel is
    /// laid out with no length, which the other panels take. Setting the property collapses or expands the panel. The
    /// splitter also writes the state that results from dragging or from a collapse button into this property.
    /// Bindings are two-way by default, so a bound source receives the current state.
    /// </summary>
    public static readonly AttachedProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, bool>("IsCollapsed", defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when a drag of a split bar starts and the drag is accepted, before the first delta is applied. The
    /// event data carries the index of the split bar and the panel sizes at that moment.
    /// </summary>
    public event EventHandler<SplitterResizeEventArgs>? ResizeStarted;
    /// <summary>
    /// Raised for every drag delta while a split bar is dragged, before the panels are resized. The event data
    /// carries the index of the split bar and the sizes the panels would have with that delta.
    /// </summary>
    public event EventHandler<SplitterResizeEventArgs>? ResizeDelta;
    /// <summary>
    /// Raised when a drag of a split bar ends, after the final panel sizes have been applied. The event data carries
    /// the index of the split bar and those sizes.
    /// </summary>
    public event EventHandler<SplitterResizeEventArgs>? ResizeCompleted;
    /// <summary>
    /// Raised when the drag bar of a split bar is double-clicked. The splitter itself does not react to it; the event
    /// data carries the index of the double-clicked split bar.
    /// </summary>
    public event EventHandler<SplitterDraggerDoubleClickedEventArgs>? DraggerDoubleClicked;

    #endregion
    
    private SplitterPanel? _splitterPanel;
    
    public Splitter()
    {
        Children.CollectionChanged += ChildrenChanged;
    }
    
    /// <summary>
    /// Gets the size attached to <paramref name="control" />, or <see langword="null" /> when none is attached.
    /// </summary>
    public static Dimension? GetSize(Control control)
    {
        return control.GetValue(SizeProperty);
    }
    
    /// <summary>
    /// Attaches <paramref name="value" /> as the size of <paramref name="control" />, or removes the size with
    /// <see langword="null" />.
    /// </summary>
    public static void SetSize(Control control, Dimension? value)
    {
        control.SetValue(SizeProperty, value);
    }
    
    /// <summary>
    /// Gets the start size attached to <paramref name="control" />, or <see langword="null" /> when none is attached.
    /// </summary>
    public static Dimension? GetDefaultSize(Control control)
    {
        return control.GetValue(DefaultSizeProperty);
    }
    
    /// <summary>
    /// Attaches <paramref name="value" /> as the size <paramref name="control" /> starts with, or removes it with
    /// <see langword="null" />.
    /// </summary>
    public static void SetDefaultSize(Control control, Dimension? value)
    {
        control.SetValue(DefaultSizeProperty, value);
    }
    
    /// <summary>
    /// Gets the smallest length attached to <paramref name="control" />, or <see langword="null" /> when none is
    /// attached.
    /// </summary>
    public static Dimension? GetMinSize(Control control)
    {
        return control.GetValue(MinSizeProperty);
    }
    
    /// <summary>
    /// Attaches <paramref name="value" /> as the smallest length of <paramref name="control" />, or removes the limit
    /// with <see langword="null" />.
    /// </summary>
    public static void SetMinSize(Control control, Dimension? value)
    {
        control.SetValue(MinSizeProperty, value);
    }
    
    /// <summary>
    /// Gets the largest length attached to <paramref name="control" />, or <see langword="null" /> when none is
    /// attached.
    /// </summary>
    public static Dimension? GetMaxSize(Control control)
    {
        return control.GetValue(MaxSizeProperty);
    }
    
    /// <summary>
    /// Attaches <paramref name="value" /> as the largest length of <paramref name="control" />, or removes the limit
    /// with <see langword="null" />.
    /// </summary>
    public static void SetMaxSize(Control control, Dimension? value)
    {
        control.SetValue(MaxSizeProperty, value);
    }
    
    /// <summary>
    /// Gets whether resizing is enabled for <paramref name="control" />.
    /// </summary>
    public static bool GetIsResizable(Control control)
    {
        return control.GetValue(IsResizableProperty);
    }
    
    /// <summary>
    /// Lets <paramref name="control" /> take part in resizing or not, following <paramref name="value" />.
    /// </summary>
    public static void SetIsResizable(Control control, bool value)
    {
        control.SetValue(IsResizableProperty, value);
    }
    
    /// <summary>
    /// Gets the collapse settings attached to <paramref name="control" />, or <see langword="null" /> when none are
    /// attached.
    /// </summary>
    public static SplitterPanelCollapsible? GetCollapsible(Control control)
    {
        return control.GetValue(CollapsibleProperty);
    }
    
    /// <summary>
    /// Attaches <paramref name="value" /> as the collapse settings of <paramref name="control" />. Passing
    /// <see langword="null" /> keeps the panel from being collapsed or expanded from a split bar.
    /// </summary>
    public static void SetCollapsible(Control control, SplitterPanelCollapsible? value)
    {
        control.SetValue(CollapsibleProperty, value);
    }
    
    /// <summary>
    /// Gets whether <paramref name="control" /> is collapsed.
    /// </summary>
    public static bool GetIsCollapsed(Control control)
    {
        return control.GetValue(IsCollapsedProperty);
    }
    
    /// <summary>
    /// Collapses <paramref name="control" /> or expands it, following <paramref name="value" />.
    /// </summary>
    public static void SetIsCollapsed(Control control, bool value)
    {
        control.SetValue(IsCollapsedProperty, value);
    }
    
    /// <summary>
    /// Called when <see cref="Children" /> reports a change. Repeats the change on the panels of the inner splitter
    /// panel. A <c>Reset</c> notification is not supported and throws <see cref="NotSupportedException" />. An
    /// override must call the base member, otherwise the splitter keeps the panels it showed before the change.
    /// </summary>
    protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _splitterPanel?.Children.InsertRange(e.NewStartingIndex, CopyControls(e.NewItems!));
                break;

            case NotifyCollectionChangedAction.Move:
                _splitterPanel?.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                _splitterPanel?.Children.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                break;

            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.OldItems!.Count; ++i)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    _splitterPanel?.Children[index] = child;
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException();
        }
    }

    private static Control[] CopyControls(System.Collections.IList items)
    {
        var controls = new Control[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            controls[i] = (Control)items[i]!;
        }

        return controls;
    }

    /// <summary>
    /// Called when the control template is applied. Locates the <c>PART_SplitterPanel</c> template part, makes this
    /// splitter its owner and adds every panel of <see cref="Children" /> to that part. An override that keeps this
    /// template must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _splitterPanel = e.NameScope.Find<SplitterPanel>("PART_SplitterPanel");
        if (_splitterPanel != null)
        {
            _splitterPanel.OwnerSplitter = this;
            foreach (var child in Children)
            {
                _splitterPanel?.Children.Add(child);
            }
        }
    }
    
    internal void RaiseResizeStarted(int handleIndex, IReadOnlyList<double> sizes)
    {
        ResizeStarted?.Invoke(this, new SplitterResizeEventArgs(handleIndex, sizes));
    }

    internal void RaiseResizeDelta(int handleIndex, IReadOnlyList<double> sizes)
    {
        ResizeDelta?.Invoke(this, new SplitterResizeEventArgs(handleIndex, sizes));
    }

    internal void RaiseResizeCompleted(int handleIndex, IReadOnlyList<double> sizes)
    {
        ResizeCompleted?.Invoke(this, new SplitterResizeEventArgs(handleIndex, sizes));
    }

    internal void RaiseDraggerDoubleClicked(int handleIndex)
    {
        DraggerDoubleClicked?.Invoke(this, new SplitterDraggerDoubleClickedEventArgs(handleIndex));
    }
}
