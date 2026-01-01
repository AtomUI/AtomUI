using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using ControlList = Avalonia.Controls.Controls;

public class Splitter : TemplatedControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Splitter, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<bool> IsLazyProperty =
        AvaloniaProperty.Register<Splitter, bool>(nameof(IsLazy));

    public static readonly StyledProperty<double> HandleSizeProperty =
        AvaloniaProperty.Register<Splitter, double>(nameof(HandleSize));

    public static readonly StyledProperty<IconTemplate?> CollapsePreviousIconProperty =
        AvaloniaProperty.Register<Splitter, IconTemplate?>(nameof(CollapsePreviousIcon));

    public static readonly StyledProperty<IconTemplate?> CollapseNextIconProperty =
        AvaloniaProperty.Register<Splitter, IconTemplate?>(nameof(CollapseNextIcon));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsLazy
    {
        get => GetValue(IsLazyProperty);
        set => SetValue(IsLazyProperty, value);
    }

    public double HandleSize
    {
        get => GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    public IconTemplate? CollapsePreviousIcon
    {
        get => GetValue(CollapsePreviousIconProperty);
        set => SetValue(CollapsePreviousIconProperty, value);
    }

    public IconTemplate? CollapseNextIcon
    {
        get => GetValue(CollapseNextIconProperty);
        set => SetValue(CollapseNextIconProperty, value);
    }
    
    [Content]
    public ControlList Children { get; } = new ();

    #endregion
    
    #region 布局项附加属性
    
    public static readonly AttachedProperty<Dimension?> SizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("Size",
            defaultBindingMode: BindingMode.TwoWay);
    
    public static readonly AttachedProperty<Dimension?> DefaultSizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("DefaultSize");
    
    public static readonly AttachedProperty<Dimension?> MinSizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("MinSize");
    
    public static readonly AttachedProperty<Dimension?> MaxSizeProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, Dimension?>("MaxSize");
    
    public static readonly AttachedProperty<bool> IsResizableProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, bool>("IsResizable", true);
    
    public static readonly AttachedProperty<SplitterPanelCollapsible?> CollapsibleProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, SplitterPanelCollapsible?>("Collapsible");
    
    public static readonly AttachedProperty<bool> IsCollapsedProperty =
        AvaloniaProperty.RegisterAttached<Splitter, Control, bool>("IsCollapsed", defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region 公共事件定义

    public event EventHandler<SplitterResizeEventArgs>? ResizeStarted;
    public event EventHandler<SplitterResizeEventArgs>? ResizeDelta;
    public event EventHandler<SplitterResizeEventArgs>? ResizeCompleted;

    #endregion
    
    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => SplitterToken.ID;

    #endregion
    
    private SplitterPanel? _splitterPanel;
    
    public Splitter()
    {
        Children.CollectionChanged += ChildrenChanged;
    }
    
    public static Dimension? GetSize(Control control)
    {
        return control.GetValue(SizeProperty);
    }
    
    public static void SetSize(Control control, Dimension? value)
    {
        control.SetValue(SizeProperty, value);
    }
    
    public static Dimension? GetDefaultSize(Control control)
    {
        return control.GetValue(DefaultSizeProperty);
    }
    
    public static void SetDefaultSize(Control control, Dimension? value)
    {
        control.SetValue(DefaultSizeProperty, value);
    }
    
    public static Dimension? GetMinSize(Control control)
    {
        return control.GetValue(MinSizeProperty);
    }
    
    public static void SetMinSize(Control control, Dimension? value)
    {
        control.SetValue(MinSizeProperty, value);
    }
    
    public static Dimension? GetMaxSize(Control control)
    {
        return control.GetValue(MaxSizeProperty);
    }
    
    public static void SetMaxSize(Control control, Dimension? value)
    {
        control.SetValue(MaxSizeProperty, value);
    }
    
    public static bool GetIsResizable(Control control)
    {
        return control.GetValue(IsResizableProperty);
    }
    
    public static void SetIsResizable(Control control, bool value)
    {
        control.SetValue(IsResizableProperty, value);
    }
    
    public static SplitterPanelCollapsible? GetCollapsible(Control control)
    {
        return control.GetValue(CollapsibleProperty);
    }
    
    public static void SetCollapsible(Control control, SplitterPanelCollapsible? value)
    {
        control.SetValue(CollapsibleProperty, value);
    }
    
    public static bool GetIsCollapsed(Control control)
    {
        return control.GetValue(IsCollapsedProperty);
    }
    
    public static void SetIsCollapsed(Control control, bool value)
    {
        control.SetValue(IsCollapsedProperty, value);
    }
    
    protected virtual void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _splitterPanel?.Children.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Control>().ToList());
                break;

            case NotifyCollectionChangedAction.Move:
                _splitterPanel?.Children.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                _splitterPanel?.Children.RemoveAll(e.OldItems!.OfType<Control>().ToList());
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _splitterPanel = e.NameScope.Find<SplitterPanel>(SplitterThemeConstants.SplitterPanelPart);
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
}