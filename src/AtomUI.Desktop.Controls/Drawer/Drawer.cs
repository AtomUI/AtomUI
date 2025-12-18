using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class Drawer : Control,
                      IMotionAwareControl,
                      IControlSharedTokenResourcesHost,
                      ICustomizableSizeTypeAware
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Content));

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(ContentTemplate));

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

    public static readonly StyledProperty<bool> CloseWhenClickOnMaskProperty = 
        AvaloniaProperty.Register<Drawer, bool>(nameof(CloseWhenClickOnMask), true);

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

    public bool CloseWhenClickOnMask
    {
        get => GetValue(CloseWhenClickOnMaskProperty);
        set => SetValue(CloseWhenClickOnMaskProperty, value);
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
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => DrawerToken.ID;

    #endregion

    private DrawerContainer? _container;
    private CompositeDisposable? _relayBindingDisposables;
    private CompositeDisposable? _containerDisposables;
    
    static Drawer()
    {
        SizeTypeProperty.OverrideDefaultValue<Drawer>(CustomizableSizeType.Small);
    }

    public Drawer()
    {
        this.RegisterResources();
        this.ConfigureMotionBindingStyle();
        this.ConfigureInstanceStyles();
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
            _relayBindingDisposables.Add(Bind(OpenOnProperty, new Binding()
            {
                Priority = BindingPriority.Template,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor)
                {
                    AncestorType = typeof(TopLevel),
                }
            }));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _relayBindingDisposables?.Dispose();
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
            change.Property == DialogSizeProperty)
        {
            ConfigureEffectiveDialogSize();
        }
        
        if (change.Property == OpenOnProperty)
        {
            if (change.OldValue is Control oldOpenOn)
            {
                oldOpenOn.SizeChanged -= HandleOpenOnSizeChanged;
            }
            if (change.NewValue is Control newOpenOn)
            {
                newOpenOn.SizeChanged += HandleOpenOnSizeChanged;
                ScopeAwareAdornerLayer.SetAdornedElement(this, newOpenOn);
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
        var layer = ScopeAwareAdornerLayer.GetLayer(this);
        Debug.Assert(layer != null);
        NotifyBeforeOpen(layer);
        CreateDrawerContainer();
        Debug.Assert(_container != null);
        _container.Open(layer);
    }

    private void Close()
    {
        var layer = ScopeAwareAdornerLayer.GetLayer(this);
        Debug.Assert(layer != null);
        NotifyBeforeClose(layer);
        Debug.Assert(_container != null);
        _container.Close(layer);
    }

    private void CreateDrawerContainer()
    {
        if (_container == null)
        {
            _container = new DrawerContainer()
            {
                Drawer = new WeakReference<Drawer>(this)
            };
            _containerDisposables?.Dispose();
            _containerDisposables = new CompositeDisposable();
            _containerDisposables.Add(BindUtils.RelayBind(this, ContentProperty, _container, DrawerContainer.ContentProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, ContentTemplateProperty, _container, DrawerContainer.ContentTemplateProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, FooterProperty, _container, DrawerContainer.FooterProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, FooterTemplateProperty, _container, DrawerContainer.FooterTemplateProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, ExtraProperty, _container, DrawerContainer.ExtraProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, ExtraTemplateProperty, _container, DrawerContainer.ExtraTemplateProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, EffectiveDialogSizeProperty, _container, DrawerContainer.DialogSizeProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, PlacementProperty, _container, DrawerContainer.PlacementProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, TitleProperty, _container, DrawerContainer.TitleProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, IsShowMaskProperty, _container, DrawerContainer.IsShowMaskProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, IsShowCloseButtonProperty, _container, DrawerContainer.IsShowCloseButtonProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, _container, DrawerContainer.IsMotionEnabledProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, CloseWhenClickOnMaskProperty, _container,
                DrawerContainer.CloseWhenClickOnMaskProperty));
            _containerDisposables.Add(BindUtils.RelayBind(this, PushOffsetPercentProperty, _container,
                DrawerContainer.PushOffsetPercentProperty));
        }
    }

    protected internal virtual void NotifyBeforeOpen(ScopeAwareAdornerLayer layer)
    {
        var current = Parent;
        while (current != null && current.GetType() != typeof(ScopeAwareAdornerLayer))
        {
            if (current is DrawerContainer container)
            {
                if (container.Drawer != null && container.Drawer.TryGetTarget(out var drawer))
                {
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

    protected internal virtual void NotifyOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    protected internal virtual void NotifyClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
    
    private void ConfigureInstanceStyles()
    {
        var style = new Style();
        style.Add(PushOffsetPercentProperty, DrawerTokenKey.PushOffsetPercent);
        Styles.Add(style);
        
        var smallStyle = new Style(x => x.PropertyEquals(SizeTypeProperty, CustomizableSizeType.Small)); 
        smallStyle.Add(DialogSizeProperty, DrawerTokenKey.SmallSize);
        Styles.Add(smallStyle);
        
        var largeStyle = new Style(x => x.PropertyEquals(SizeTypeProperty, CustomizableSizeType.Large));
        largeStyle.Add(DialogSizeProperty, DrawerTokenKey.LargeSize);
        Styles.Add(largeStyle);
        
        var middleStyle = new Style(x => x.PropertyEquals(SizeTypeProperty, CustomizableSizeType.Middle)); 
        middleStyle.Add(DialogSizeProperty, DrawerTokenKey.MiddleSize);
        Styles.Add(middleStyle);
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
                var containerSize = OpenOn.Bounds.Size;
                if (Placement == DrawerPlacement.Top ||
                    Placement == DrawerPlacement.Bottom)
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