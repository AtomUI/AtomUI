using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public partial class Drawer : Control,
                             IMotionAwareControl,
                             ICustomizableSizeTypeAware,
                             ISemanticPartCrossRootProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(ContentTemplate));

    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<Drawer, Thickness>(nameof(ContentPadding));

    public static readonly StyledProperty<bool> IsOpenProperty = 
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsOpen), false, false, BindingMode.TwoWay);

    public static readonly StyledProperty<DrawerPlacement> PlacementProperty = 
        AvaloniaProperty.Register<Drawer, DrawerPlacement>(nameof(Placement), DrawerPlacement.Right);

    public static readonly StyledProperty<Control?> OpenOnProperty =
        AvaloniaProperty.Register<Drawer, Control?>(nameof(OpenOn));

    public static readonly StyledProperty<bool> IsShowMaskProperty = 
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsShowMask), true);

    public static readonly StyledProperty<bool> IsShowCloseButtonProperty = 
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsShowCloseButton), true);

    public static readonly StyledProperty<bool> IsCloseOnMaskClickProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsCloseOnMaskClick), true);

    public static readonly StyledProperty<bool> IsPinnedOpenProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsPinnedOpen));

    public static readonly StyledProperty<string> TitleProperty = 
        AvaloniaProperty.Register<Drawer, string>(nameof(Title));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Extra));

    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(ExtraTemplate));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Drawer>();

    public static readonly StyledProperty<Dimension> DialogSizeProperty =
        AvaloniaProperty.Register<Drawer, Dimension>(nameof(DialogSize));

    public static readonly StyledProperty<double> PushOffsetPercentProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(PushOffsetPercent));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Drawer>();
    
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

    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public DrawerPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public Control? OpenOn
    {
        get => GetValue(OpenOnProperty);
        set => SetValue(OpenOnProperty, value);
    }

    public bool IsShowMask
    {
        get => GetValue(IsShowMaskProperty);
        set => SetValue(IsShowMaskProperty, value);
    }

    public bool IsShowCloseButton
    {
        get => GetValue(IsShowCloseButtonProperty);
        set => SetValue(IsShowCloseButtonProperty, value);
    }

    public bool IsCloseOnMaskClick
    {
        get => GetValue(IsCloseOnMaskClickProperty);
        set => SetValue(IsCloseOnMaskClickProperty, value);
    }

    /// <summary>
    /// 钉住常开（语义预览用）：为 true 时遮罩点击与关闭按钮不再将 <see cref="IsOpen"/> 置 false；
    /// 外部代码直接设置 <see cref="IsOpen"/> 仍正常关闭。对齐上游 antd 语义演示的受控常开行为。
    /// </summary>
    public bool IsPinnedOpen
    {
        get => GetValue(IsPinnedOpenProperty);
        set => SetValue(IsPinnedOpenProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    [DependsOn(nameof(FooterTemplate))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public Dimension DialogSize
    {
        get => GetValue(DialogSizeProperty);
        set => SetValue(DialogSizeProperty, value);
    }

    public double PushOffsetPercent
    {
        get => GetValue(PushOffsetPercentProperty);
        set => SetValue(PushOffsetPercentProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler? Opened;
    public event EventHandler? Closed;

    #endregion

    #region ISemanticPartCrossRootProvider 实现

    public event EventHandler? CrossRootsChanged;

    public IReadOnlyList<Visual> GetCrossRoots()
    {
        if (_container is not null && _container.GetVisualParent() is not null)
        {
            return [_container];
        }

        return Array.Empty<Visual>();
    }

    private void RaiseCrossRootsChanged()
    {
        CrossRootsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region 内部属性定义
    
    internal static readonly DirectProperty<Drawer, double> EffectiveDialogSizeProperty =
        AvaloniaProperty.RegisterDirect<Drawer, double>(nameof(EffectiveDialogSize),
            o => o.EffectiveDialogSize,
            (o, v) => o.EffectiveDialogSize = v);
    
    private double _effectiveDialogSize;

    internal double EffectiveDialogSize
    {
        get => _effectiveDialogSize;
        set => SetAndRaise(EffectiveDialogSizeProperty, ref _effectiveDialogSize, value);
    }

    #endregion

    private DrawerContainer? _container;
    private CompositeDisposable? _relayBindingDisposables;
    private IDisposable? _motionBinding;
    private IDisposable? _pushOffsetBinding;
    private IDisposable? _dialogSizeBinding;
    private Control? _openOnSizeChangedTarget;
    private int _visualTreeVersion;
    
    static Drawer()
    {
        SizeTypeProperty.OverrideDefaultValue<Drawer>(CustomizableSizeType.Small);
    }

    public static Drawer? GetDrawer(Visual element)
    {
        var container = element.FindAncestorOfType<DrawerContainer>();
        if (container?.Drawer != null && container.Drawer.TryGetTarget(out var drawer))
        {
            return drawer;
        }

        return null;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        _pushOffsetBinding?.Dispose();
        _pushOffsetBinding = TokenResourceBinder.CreateControlTokenBinding(
            this,
            PushOffsetPercentProperty,
            DrawerTokenKind.PushOffsetPercent);
        ApplyDialogSizeTokenBinding();
        _visualTreeVersion++;
        var parentDrawer = FindParentDrawer();
        _relayBindingDisposables?.Dispose();
        _relayBindingDisposables = new CompositeDisposable();
        if (parentDrawer != null)
        {
            _relayBindingDisposables.Add(BindUtils.RelayBind(parentDrawer, OpenOnProperty, this, OpenOnProperty, BindingMode.Default,
                BindingPriority.Template));
            _relayBindingDisposables.Add(BindUtils.RelayBind(parentDrawer, IsMotionEnabledProperty, this, IsMotionEnabledProperty,
                BindingMode.Default, BindingPriority.Template));
        }
        else
        {
            _relayBindingDisposables.Add(BindUtils.BindVisualAncestor(
                this,
                this,
                OpenOnProperty,
                typeof(TopLevel),
                priority: BindingPriority.Template));
        }

        if (IsOpen)
        {
            HandleIsOpenChanged();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = null;
        _pushOffsetBinding?.Dispose();
        _pushOffsetBinding = null;
        _dialogSizeBinding?.Dispose();
        _dialogSizeBinding = null;
        var visualTreeVersion = ++_visualTreeVersion;
        _relayBindingDisposables?.Dispose();
        _relayBindingDisposables = null;
        DetachOpenOnSizeChanged();
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (visualTreeVersion == _visualTreeVersion && !this.IsAttachedToVisualTree())
            {
                ReleaseDrawerContainer();
            }
        });
    }

    private Drawer? FindParentDrawer()
    {
        Drawer? target  = null;
        var     current = Parent;
        while (current != null && current.GetType() != typeof(ScopeAwareAdornerLayer))
        {
            if (current is DrawerContainer container)
            {
                if (container.Drawer != null && container.Drawer.TryGetTarget(out var drawer))
                {
                    target = drawer;
                }
            }

            if (target != null)
            {
                break;
            }

            current = current.Parent;
        }

        return target;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsOpenProperty)
            {
                HandleIsOpenChanged();
            }
        }

        if (change.Property == OpenOnProperty ||
            change.Property == DialogSizeProperty ||
            change.Property == PlacementProperty)
        {
            ConfigureEffectiveDialogSize();
            ConfigureOpenOnSizeChangedSubscription();
        }

        if (change.Property == PlacementProperty && IsOpen)
        {
            NotifyParentDrawersChildPlacementChanged();
        }

        if (change.Property == SizeTypeProperty)
        {
            ApplyDialogSizeTokenBinding();
        }
        
        if (change.Property == OpenOnProperty)
        {
            var newOpenOn = change.NewValue as Control;
            ScopeAwareAdornerLayer.SetAdornedElement(this, newOpenOn);
            if (IsOpen && _container is not null)
            {
                _container.UpdateOpenOn(
                    newOpenOn,
                    ScopeAwareAdornerLayer.GetLayer(newOpenOn ?? this));
            }
        }
    }

    private void HandleOpenOnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        ConfigureEffectiveDialogSize();
    }

    private void HandleIsOpenChanged()
    {
        if (IsOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    private void Open()
    {
        var layer = ScopeAwareAdornerLayer.GetLayer(OpenOn ?? this);
        Debug.Assert(layer != null);
        ConfigureEffectiveDialogSize();
        ConfigureOpenOnSizeChangedSubscription();
        NotifyBeforeOpen(layer);
        CreateDrawerContainer();
        Debug.Assert(_container != null);
        _container.Open(layer);
        RaiseCrossRootsChanged();
    }

    private void Close()
    {
        DetachOpenOnSizeChanged();
        var layer = _container?.GetVisualParent<ScopeAwareAdornerLayer>() ??
                    ScopeAwareAdornerLayer.GetLayer(OpenOn ?? this);
        Debug.Assert(layer != null);
        _container?.CloseActiveChildDrawer();
        NotifyBeforeClose(layer);
        _container?.Close(layer);
    }

    private void CreateDrawerContainer()
    {
        if (_container == null)
        {
            _container = new DrawerContainer()
            {
                Drawer = new WeakReference<Drawer>(this)
            };
            _container.BindToDrawer(this);
        }
    }

    private void ReleaseDrawerContainer()
    {
        var container = _container;
        _container = null;
        Exception? notificationFailure = null;
        try
        {
            RaiseCrossRootsChanged();
        }
        catch (Exception exception)
        {
            notificationFailure = exception;
        }
        try
        {
            container?.Release();
        }
        catch (Exception releaseFailure) when (notificationFailure is not null)
        {
            throw new AggregateException(notificationFailure, releaseFailure);
        }
        if (notificationFailure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(notificationFailure).Throw();
        }
    }

    private void ConfigureOpenOnSizeChangedSubscription()
    {
        if (!IsOpen || !DialogSize.IsPercentage || OpenOn is null)
        {
            DetachOpenOnSizeChanged();
            return;
        }

        if (ReferenceEquals(_openOnSizeChangedTarget, OpenOn))
        {
            return;
        }

        DetachOpenOnSizeChanged();
        _openOnSizeChangedTarget = OpenOn;
        _openOnSizeChangedTarget.SizeChanged += HandleOpenOnSizeChanged;
    }

    private void DetachOpenOnSizeChanged()
    {
        if (_openOnSizeChangedTarget is null)
        {
            return;
        }

        _openOnSizeChangedTarget.SizeChanged -= HandleOpenOnSizeChanged;
        _openOnSizeChangedTarget = null;
    }

    protected internal virtual void NotifyBeforeOpen(ScopeAwareAdornerLayer layer)
    {
        Drawer? firstParentDrawer = null;
        var current = Parent;
        while (current != null && current.GetType() != typeof(ScopeAwareAdornerLayer))
        {
            if (current is DrawerContainer container)
            {
                if (container.Drawer != null && container.Drawer.TryGetTarget(out var drawer))
                {
                    if (firstParentDrawer == null)
                    {
                        firstParentDrawer = drawer;
                        DataContext       =  firstParentDrawer.DataContext;
                    }
                    drawer.NotifyChildDrawerAboutToOpen(this);
                }
            }

            current = current.Parent;
        }
    }

    internal void NotifyChildDrawerAboutToOpen(Drawer childDrawer)
    {
        _container?.NotifyChildDrawerAboutToOpen(childDrawer);
    }

    internal void NotifyChildDrawerAboutToClose(Drawer childDrawer)
    {
        _container?.NotifyChildDrawerAboutToClose(childDrawer);
    }

    internal void NotifyChildDrawerPlacementChanged(Drawer childDrawer)
    {
        _container?.NotifyChildDrawerPlacementChanged(childDrawer);
    }

    protected virtual void NotifyBeforeClose(ScopeAwareAdornerLayer layer)
    {
        var current = Parent;
        while (current != null && current.GetType() != typeof(ScopeAwareAdornerLayer))
        {
            if (current is DrawerContainer container)
            {
                if (container.Drawer != null && container.Drawer.TryGetTarget(out var drawer))
                {
                    drawer.NotifyChildDrawerAboutToClose(this);
                }
            }

            current = current.Parent;
        }
    }

    private void NotifyParentDrawersChildPlacementChanged()
    {
        var current = Parent;
        while (current != null && current.GetType() != typeof(ScopeAwareAdornerLayer))
        {
            if (current is DrawerContainer container)
            {
                if (container.Drawer != null && container.Drawer.TryGetTarget(out var drawer))
                {
                    drawer.NotifyChildDrawerPlacementChanged(this);
                }
            }

            current = current.Parent;
        }
    }

    protected internal virtual void NotifyOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    protected internal virtual void NotifyClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
        RaiseCrossRootsChanged();
    }
    
    private void ApplyDialogSizeTokenBinding()
    {
        _dialogSizeBinding?.Dispose();
        _dialogSizeBinding = null;
        if (!this.IsAttachedToVisualTree())
        {
            return;
        }

        var tokenKind = SizeType switch
        {
            CustomizableSizeType.Small  => DrawerTokenKind.SmallSize,
            CustomizableSizeType.Middle => DrawerTokenKind.MiddleSize,
            CustomizableSizeType.Large  => DrawerTokenKind.LargeSize,
            _                           => DrawerTokenKind.SmallSize
        };
        _dialogSizeBinding = TokenResourceBinder.CreateControlTokenBinding(this, DialogSizeProperty, tokenKind);
    }

    private void ConfigureEffectiveDialogSize()
    {
        if (DialogSize.IsAbsolute)
        {
            SetCurrentValue(EffectiveDialogSizeProperty, DialogSize.Value);
        }
        else if (DialogSize.IsPercentage)
        {
            if (OpenOn != null)
            {
                var containerSize = TopLevelMarginBinder.GetVisibleFrameSize(OpenOn);
                if (Placement == DrawerPlacement.Top || Placement == DrawerPlacement.Bottom)
                {
                    SetCurrentValue(EffectiveDialogSizeProperty, DialogSize.Resolve(containerSize.Height));
                }
                else
                {
                    SetCurrentValue(EffectiveDialogSizeProperty, DialogSize.Resolve(containerSize.Width));
                }
            }
        }
    }
}
