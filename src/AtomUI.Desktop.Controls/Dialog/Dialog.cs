using System.ComponentModel;
using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class Dialog : TemplatedControl,
                              IMotionAwareControl,
                              IDialog,
                              ISemanticPartCrossRootProvider
{
    #region 公共属性定义
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<Dialog, string?>(nameof(Title));

    public static readonly StyledProperty<PathIcon?> TitleIconProperty =
        AvaloniaProperty.Register<Dialog, PathIcon?>(nameof(TitleIcon));

    public static readonly StyledProperty<object?> ContentProperty = ContentPresenter.ContentProperty.AddOwner<Dialog>();

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentPresenter.ContentTemplateProperty.AddOwner<Dialog>();

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Dialog, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsModalProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsModal), true);

    public static readonly StyledProperty<bool> IsResizableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsResizable));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsClosable), true);

    /// <summary>
    /// 是否允许 Overlay modal mask 外点发起关闭。仅 Overlay Host 有效；Window Host 没有 mask，
    /// 外点本来就不触发关闭。与 <see cref="IsClosable"/>（标题栏关闭入口）正交。
    /// </summary>
    public static readonly StyledProperty<bool> IsMaskClosableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsMaskClosable), true);

    public static readonly StyledProperty<bool> IsMaximizableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsMaximizable));

    public static readonly StyledProperty<bool> IsMinimizableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsMinimizable), true);

    public static readonly StyledProperty<bool> IsDragMovableProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsDragMovable), true);

    public static readonly StyledProperty<Control?> PlacementTargetProperty =
        AvaloniaProperty.Register<Dialog, Control?>(nameof(PlacementTarget));

    /// <summary>
    /// 可选：把 Overlay 宿主限定在指定作用域内。指定后 mask、Surface 尺寸与居中都以该作用域为边界
    /// （对齐上游 antd Modal 的内联/setContainer 语义，例如 Gallery 语义预览的"舞台内模态"）。
    /// 默认 <c>null</c> 保持既有行为：宿主解析到 owning TopLevel 的 OverlayLayer。
    /// 仅 Overlay 宿主有效，Window 宿主忽略；作用域内没有可用的 scope layer 时回退到默认 TopLevel 宿主。
    /// 作用域宿主没有 Window frame 契约，因此不参与 frame 内缩与 drawn chrome 抑制。
    /// </summary>
    public static readonly StyledProperty<Control?> OverlayScopeProperty =
        AvaloniaProperty.Register<Dialog, Control?>(nameof(OverlayScope));

    public static readonly StyledProperty<DialogHorizontalAnchor> HorizontalStartupLocationProperty =
        AvaloniaProperty.Register<Dialog, DialogHorizontalAnchor>(nameof(HorizontalStartupLocation),
            DialogHorizontalAnchor.Center);

    public static readonly StyledProperty<DialogVerticalAnchor> VerticalStartupLocationProperty =
        AvaloniaProperty.Register<Dialog, DialogVerticalAnchor>(nameof(VerticalStartupLocation),
            DialogVerticalAnchor.Center);

    public static readonly StyledProperty<Dimension?> HorizontalOffsetProperty =
        AvaloniaProperty.Register<Dialog, Dimension?>(nameof(HorizontalOffset));

    public static readonly StyledProperty<Dimension?> VerticalOffsetProperty =
        AvaloniaProperty.Register<Dialog, Dimension?>(nameof(VerticalOffset));

    public static readonly StyledProperty<bool> IsTopmostProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsTopmost));

    public static readonly StyledProperty<object?> ResultProperty =
        AvaloniaProperty.Register<Dialog, object?>(nameof(Result));

    public static readonly StyledProperty<DialogStandardButtons> StandardButtonsProperty =
        DialogButtonBox.StandardButtonsProperty.AddOwner<Dialog>();

    public static readonly StyledProperty<DialogStandardButton> DefaultStandardButtonProperty =
        DialogButtonBox.DefaultStandardButtonProperty.AddOwner<Dialog>();

    public static readonly StyledProperty<DialogStandardButton> EscapeStandardButtonProperty =
        DialogButtonBox.EscapeStandardButtonProperty.AddOwner<Dialog>();

    public static readonly StyledProperty<bool> IsFooterVisibleProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsFooterVisible), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Dialog>();

    public static readonly StyledProperty<DialogHostType> DialogHostTypeProperty =
        AvaloniaProperty.Register<Dialog, DialogHostType>(nameof(DialogHostType), DialogHostType.Overlay);

    public static readonly StyledProperty<bool> IsPinnedOpenProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsPinnedOpen));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsLoading));

    public static readonly StyledProperty<bool> IsConfirmLoadingProperty =
        AvaloniaProperty.Register<Dialog, bool>(nameof(IsConfirmLoading));

    // Dialog 仅持有会话状态，不直接参与可视布局。下面的 Host* 属性定义 Presenter Surface
    // 的尺寸约束。
    public static readonly StyledProperty<double> HostWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostWidth), double.NaN);

    public static readonly StyledProperty<double> HostHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostHeight), double.NaN);

    public static readonly StyledProperty<double> HostMinWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMinWidth), 0d);

    public static readonly StyledProperty<double> HostMinHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMinHeight), 0d);

    public static readonly StyledProperty<double> HostMaxWidthProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMaxWidth), double.PositiveInfinity);

    public static readonly StyledProperty<double> HostMaxHeightProperty =
        AvaloniaProperty.Register<Dialog, double>(nameof(HostMaxHeight), double.PositiveInfinity);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public PathIcon? TitleIcon
    {
        get => GetValue(TitleIconProperty);
        set => SetValue(TitleIconProperty, value);
    }

    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    public IAvaloniaDependencyResolver? DependencyResolver { get; set; }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }

    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public bool IsMaskClosable
    {
        get => GetValue(IsMaskClosableProperty);
        set => SetValue(IsMaskClosableProperty, value);
    }

    public bool IsMaximizable
    {
        get => GetValue(IsMaximizableProperty);
        set => SetValue(IsMaximizableProperty, value);
    }

    public bool IsMinimizable
    {
        get => GetValue(IsMinimizableProperty);
        set => SetValue(IsMinimizableProperty, value);
    }

    public bool IsDragMovable
    {
        get => GetValue(IsDragMovableProperty);
        set => SetValue(IsDragMovableProperty, value);
    }

    [ResolveByName]
    public Control? PlacementTarget
    {
        get => GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }

    public Control? OverlayScope
    {
        get => GetValue(OverlayScopeProperty);
        set => SetValue(OverlayScopeProperty, value);
    }

    public DialogHorizontalAnchor HorizontalStartupLocation
    {
        get => GetValue(HorizontalStartupLocationProperty);
        set => SetValue(HorizontalStartupLocationProperty, value);
    }

    public DialogVerticalAnchor VerticalStartupLocation
    {
        get => GetValue(VerticalStartupLocationProperty);
        set => SetValue(VerticalStartupLocationProperty, value);
    }

    public Dimension? HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public Dimension? VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public bool IsTopmost
    {
        get => GetValue(IsTopmostProperty);
        set => SetValue(IsTopmostProperty, value);
    }

    public object? Result
    {
        get => GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public DialogHostType DialogHostType
    {
        get => GetValue(DialogHostTypeProperty);
        set => SetValue(DialogHostTypeProperty, value);
    }

    public DialogStandardButtons StandardButtons
    {
        get => GetValue(StandardButtonsProperty);
        set => SetValue(StandardButtonsProperty, value);
    }

    public DialogStandardButton DefaultStandardButton
    {
        get => GetValue(DefaultStandardButtonProperty);
        set => SetValue(DefaultStandardButtonProperty, value);
    }

    public DialogStandardButton EscapeStandardButton
    {
        get => GetValue(EscapeStandardButtonProperty);
        set => SetValue(EscapeStandardButtonProperty, value);
    }

    public bool IsFooterVisible
    {
        get => GetValue(IsFooterVisibleProperty);
        set => SetValue(IsFooterVisibleProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// 钉住常开（语义预览用）：为 true 时用户发起的关闭入口（标题栏关闭按钮、Overlay 遮罩外点、Escape、
    /// Footer 按钮）不再发起关闭请求；外部代码直接设置 <see cref="IsOpen"/> 仍正常关闭。
    /// 仅 Overlay 宿主有效；Window 宿主由原生窗口关闭流程负责，不受此开关控制。
    /// </summary>
    public bool IsPinnedOpen
    {
        get => GetValue(IsPinnedOpenProperty);
        set => SetValue(IsPinnedOpenProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool IsConfirmLoading
    {
        get => GetValue(IsConfirmLoadingProperty);
        set => SetValue(IsConfirmLoadingProperty, value);
    }

    public double HostWidth
    {
        get => GetValue(HostWidthProperty);
        set => SetValue(HostWidthProperty, value);
    }

    public double HostHeight
    {
        get => GetValue(HostHeightProperty);
        set => SetValue(HostHeightProperty, value);
    }

    public double HostMinWidth
    {
        get => GetValue(HostMinWidthProperty);
        set => SetValue(HostMinWidthProperty, value);
    }

    public double HostMinHeight
    {
        get => GetValue(HostMinHeightProperty);
        set => SetValue(HostMinHeightProperty, value);
    }

    public double HostMaxWidth
    {
        get => GetValue(HostMaxWidthProperty);
        set => SetValue(HostMaxWidthProperty, value);
    }

    public double HostMaxHeight
    {
        get => GetValue(HostMaxHeightProperty);
        set => SetValue(HostMaxHeightProperty, value);
    }

    public AvaloniaList<DialogButton> CustomButtons { get; } = new();

    public Action<IReadOnlyList<DialogButton>>? ButtonsConfigure
    {
        get => _buttonsConfigure;
        set
        {
            _buttonsConfigure = value;
            if (_surfaceButtons is not null)
            {
                value?.Invoke(_surfaceButtons);
            }
        }
    }

    public Func<DialogClosingContext, ValueTask<bool>>? BeforeCloseAsync { get; set; }

    #endregion

    #region 公共事件定义

    public event EventHandler? Closed;
    public event EventHandler? Opened;
    public event EventHandler<CancelEventArgs>? Closing;
    public event EventHandler? Accepted;
    public event EventHandler? Rejected;
    public event EventHandler<DialogFinishedEventArgs>? Finished;
    public event EventHandler<DialogButtonClickedEventArgs>? ButtonClicked;

    #endregion

    #region ISemanticPartCrossRootProvider 实现

    public event EventHandler? CrossRootsChanged;

    public IReadOnlyList<Visual> GetCrossRoots()
    {
        var presenter = _session?.Presenter;
        if (presenter is Visual overlayRoot && overlayRoot.GetVisualParent() is not null)
        {
            return [overlayRoot];
        }

        // Window 宿主：跨根是原生 DialogWindow（独立 TopLevel）。语义预览据此在该窗口内解析并高亮部件
        // （Adorner 落在宿主窗口自己的 AdornerLayer，先例：ImagePreviewer 的 native dialog）。
        // 该上报只服务跨根解析；owner 作用域的样式级联仍不跨 TopLevel（见 semantic-part.md）。
        if (presenter is WindowDialogPresenter { HostWindow.IsVisible: true } windowPresenter)
        {
            return [windowPresenter.HostWindow];
        }

        return Array.Empty<Visual>();
    }

    internal void NotifyCrossRootsChanged()
    {
        CrossRootsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Dialog, double> OffsetXProperty =
        AvaloniaProperty.RegisterDirect<Dialog, double>(
            nameof(OffsetX),
            o => o.OffsetX,
            (o, v) => o.OffsetX = v);

    internal static readonly DirectProperty<Dialog, double> OffsetYProperty =
        AvaloniaProperty.RegisterDirect<Dialog, double>(
            nameof(OffsetY),
            o => o.OffsetY,
            (o, v) => o.OffsetY = v);

    internal static readonly DirectProperty<Dialog, bool> EffectiveMinimizableProperty =
        AvaloniaProperty.RegisterDirect<Dialog, bool>(
            nameof(EffectiveMinimizable),
            o => o.EffectiveMinimizable,
            (o, v) => o.EffectiveMinimizable = v);

    private double _offsetX;

    internal double OffsetX
    {
        get => _offsetX;
        set => SetAndRaise(OffsetXProperty, ref _offsetX, value);
    }

    private double _offsetY;

    internal double OffsetY
    {
        get => _offsetY;
        set => SetAndRaise(OffsetYProperty, ref _offsetY, value);
    }

    private bool _effectiveMinimizable;

    internal bool EffectiveMinimizable
    {
        get => _effectiveMinimizable;
        set => SetAndRaise(EffectiveMinimizableProperty, ref _effectiveMinimizable, value);
    }

    internal DialogMotionAnchorMode MotionAnchorMode { get; set; }

    internal bool UsesPlacementTargetAsMotionAnchor =>
        MotionAnchorMode switch
        {
            DialogMotionAnchorMode.ExplicitPlacementTarget => PlacementTarget is not null,
            DialogMotionAnchorMode.FallbackPlacementTarget => false,
            _ => PlacementTarget is not null
        };

    private protected IReadOnlyList<DialogButton> SurfaceButtons =>
        _surfaceButtons ?? Array.Empty<DialogButton>();

    #endregion

    #region 运行时字段

    private Action<IReadOnlyList<DialogButton>>? _buttonsConfigure;
    private IReadOnlyList<DialogButton>? _surfaceButtons;

    #endregion

    internal virtual object? GetSurfaceContent()
    {
        return Content;
    }

    internal virtual IDataTemplate? GetSurfaceContentTemplate()
    {
        return ContentTemplate;
    }

    internal virtual void ConfigureSurfaceButtons(IReadOnlyList<DialogButton> buttons)
    {
        _surfaceButtons = buttons;
        _buttonsConfigure?.Invoke(buttons);
    }

    internal virtual void ReleaseSurfaceButtons(IReadOnlyList<DialogButton> buttons)
    {
        if (ReferenceEquals(_surfaceButtons, buttons))
        {
            _surfaceButtons = null;
        }
    }
}
