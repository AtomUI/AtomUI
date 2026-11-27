using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A panel that opens at an edge of the area given by <see cref="OpenOn" /> and shows a title, extra content,
/// body content and an optional footer. Set <see cref="IsOpen" /> to show or hide it.
/// The mask and the panel are drawn by a container that is created when the drawer opens and is hosted in a
/// presentation layer outside the visual tree of this control. The container takes its state from the properties
/// of this control.
/// </summary>
public partial class Drawer : Control,
                             IMotionAwareControl,
                             ICustomizableSizeTypeAware,
                             ISemanticPartCrossRootProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Content));

    /// <summary>
    /// Identifies the <see cref="ContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Identifies the <see cref="ContentPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<Drawer, Thickness>(nameof(ContentPadding));

    /// <summary>
    /// Identifies the <see cref="IsOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty = 
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsOpen), false, false, BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<DrawerPlacement> PlacementProperty = 
        AvaloniaProperty.Register<Drawer, DrawerPlacement>(nameof(Placement), DrawerPlacement.Right);

    /// <summary>
    /// Identifies the <see cref="OpenOn" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> OpenOnProperty =
        AvaloniaProperty.Register<Drawer, Control?>(nameof(OpenOn));

    /// <summary>
    /// Identifies the <see cref="IsShowMask" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowMaskProperty = 
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsShowMask), true);

    /// <summary>
    /// Identifies the <see cref="IsShowCloseButton" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowCloseButtonProperty = 
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsShowCloseButton), true);

    /// <summary>
    /// Identifies the <see cref="IsCloseOnMaskClick" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCloseOnMaskClickProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsCloseOnMaskClick), true);

    /// <summary>
    /// Identifies the <see cref="IsPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPinnedOpenProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsPinnedOpen));

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty = 
        AvaloniaProperty.Register<Drawer, string>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="Footer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Footer));

    /// <summary>
    /// Identifies the <see cref="FooterTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(FooterTemplate));

    /// <summary>
    /// Identifies the <see cref="Extra" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ExtraProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(Extra));

    /// <summary>
    /// Identifies the <see cref="ExtraTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(ExtraTemplate));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Drawer>();

    /// <summary>
    /// Identifies the <see cref="DialogSize" /> property.
    /// </summary>
    public static readonly StyledProperty<Dimension> DialogSizeProperty =
        AvaloniaProperty.Register<Drawer, Dimension>(nameof(DialogSize));

    /// <summary>
    /// Identifies the <see cref="PushOffsetPercent" /> property.
    /// </summary>
    public static readonly StyledProperty<double> PushOffsetPercentProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(PushOffsetPercent));

    /// <summary>
    /// Identifies the <see cref="EdgeBorderColor" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> EdgeBorderColorProperty =
        AvaloniaProperty.Register<Drawer, IBrush?>(nameof(EdgeBorderColor));

    /// <summary>
    /// Identifies the <see cref="EdgeBorderThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> EdgeBorderThicknessProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(EdgeBorderThickness));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Drawer>();
    
    /// <summary>
    /// The body content shown below the header. XAML children of the drawer become this value.
    /// Use <see cref="ContentTemplate" /> to build the visuals of a value that is not a control.
    /// </summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="Content" />. When it is <see langword="null" />, the content
    /// presenter falls back to its default presentation of the value.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Insets the body (<see cref="Content" />) from the inner edge of the panel. The header and the footer are
    /// not affected; they use their own tokens.
    /// The theme sets the value from <see cref="DrawerToken.ContentPadding" />; an explicit value replaces it.
    /// </summary>
    public Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    /// <summary>
    /// Whether the drawer is shown. Setting it opens or closes the drawer; the change takes effect when the control
    /// is attached, and a drawer that is set open beforehand opens on attachment.
    /// The property binds two-way by default, so a mask click or the close button that closes the drawer also updates
    /// the bound source.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// The edge of the host area the drawer opens at. A Left or Right drawer stretches over the full height and uses
    /// <see cref="DialogSize" /> as its width; a Top or Bottom drawer stretches over the full width and uses it as
    /// its height. The value can be changed while the drawer is open.
    /// </summary>
    public DrawerPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// The control whose area the drawer opens in. The presentation layer that hosts the mask and the panel is
    /// resolved from it, and a percentage <see cref="DialogSize" /> is resolved against its visible size.
    /// A <see cref="TopLevel" /> opens the drawer in that TopLevel's own presentation layer; another control scopes
    /// the drawer to that control. The value can be changed while the drawer is open.
    /// Without a value, a drawer nested in another drawer follows the parent drawer's value, and any other drawer
    /// opens on its nearest <see cref="TopLevel" /> ancestor.
    /// </summary>
    public Control? OpenOn
    {
        get => GetValue(OpenOnProperty);
        set => SetValue(OpenOnProperty, value);
    }

    /// <summary>
    /// Whether the mask that dims the host area is shown while the drawer is open.
    /// </summary>
    public bool IsShowMask
    {
        get => GetValue(IsShowMaskProperty);
        set => SetValue(IsShowMaskProperty, value);
    }

    /// <summary>
    /// Whether the close button is shown at the start of the header.
    /// </summary>
    public bool IsShowCloseButton
    {
        get => GetValue(IsShowCloseButtonProperty);
        set => SetValue(IsShowCloseButtonProperty, value);
    }

    /// <summary>
    /// Whether a click on the mask closes the drawer by setting <see cref="IsOpen" /> to <see langword="false" />.
    /// It has no effect while <see cref="IsPinnedOpen" /> is <see langword="true" />.
    /// </summary>
    public bool IsCloseOnMaskClick
    {
        get => GetValue(IsCloseOnMaskClickProperty);
        set => SetValue(IsCloseOnMaskClickProperty, value);
    }

    /// <summary>
    /// Keeps the drawer open for as long as it is <see langword="true" />: a mask click or the close button then no
    /// longer sets <see cref="IsOpen" /> to <see langword="false" />. Setting <see cref="IsOpen" /> to
    /// <see langword="false" /> from code still closes the drawer.
    /// It is meant for semantic previews, which need the drawer to stay open. This matches the always-open state of
    /// the drawers in the semantic demos of upstream Ant Design.
    /// </summary>
    public bool IsPinnedOpen
    {
        get => GetValue(IsPinnedOpenProperty);
        set => SetValue(IsPinnedOpenProperty, value);
    }

    /// <summary>
    /// The text shown in the header of the drawer.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The footer content shown at the bottom of the panel, below a separator. The footer and its separator are
    /// shown only while this value is set. Use <see cref="FooterTemplate" /> to build the visuals.
    /// </summary>
    [DependsOn(nameof(FooterTemplate))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="Footer" />.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    /// <summary>
    /// The extra content at the end of the header, shown only while this value is set. Use
    /// <see cref="ExtraTemplate" /> to build its visuals.
    /// </summary>
    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra
    {
        get => GetValue(ExtraProperty);
        set => SetValue(ExtraProperty, value);
    }

    /// <summary>
    /// The template used to present <see cref="Extra" />.
    /// </summary>
    public IDataTemplate? ExtraTemplate
    {
        get => GetValue(ExtraTemplateProperty);
        set => SetValue(ExtraTemplateProperty, value);
    }

    /// <summary>
    /// The size preset that selects the drawer size token bound to <see cref="DialogSize" />. The default is
    /// <see cref="CustomizableSizeType.Small" />. With <see cref="CustomizableSizeType.Custom" /> the small token is
    /// still used, so set <see cref="DialogSize" /> explicitly for a custom size.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// The size of the panel measured from its anchored edge: the width of a Left or Right drawer, the height of a
    /// Top or Bottom one. An absolute value is used as it is. A percentage is resolved against the visible size of
    /// <see cref="OpenOn" /> and follows when <see cref="OpenOn" /> resizes; while <see cref="OpenOn" /> is
    /// <see langword="null" /> the percentage is not resolved and the previous size stays in use.
    /// The theme binds the value to the size token selected by <see cref="SizeType" />. A local value takes
    /// precedence over the binding.
    /// </summary>
    public Dimension DialogSize
    {
        get => GetValue(DialogSizeProperty);
        set => SetValue(DialogSizeProperty, value);
    }

    /// <summary>
    /// The share of this drawer's own width (Left and Right) or height (Top and Bottom) by which it is pushed aside
    /// when a drawer nested in its content opens over it. 0 leaves it in place.
    /// The theme binds the value to <see cref="DrawerToken.PushOffsetPercent" />; a local value takes precedence.
    /// </summary>
    public double PushOffsetPercent
    {
        get => GetValue(PushOffsetPercentProperty);
        set => SetValue(PushOffsetPercentProperty, value);
    }

    /// <summary>
    /// The color of the border drawn on the side of the panel that touches the edge of the host area.
    /// The theme binds the value to <see cref="DrawerToken.EdgeBorderColor" />, which is transparent by default, so
    /// the Ant Design rendering draws no border. A local value takes precedence over the binding.
    /// </summary>
    public IBrush? EdgeBorderColor
    {
        get => GetValue(EdgeBorderColorProperty);
        set => SetValue(EdgeBorderColorProperty, value);
    }

    /// <summary>
    /// The thickness of the border drawn on the side of the panel that touches the edge of the host area.
    /// The theme binds the value to <see cref="DrawerToken.EdgeBorderThickness" />, which is 0 by default, so no
    /// border is drawn. A local value takes precedence over the binding.
    /// </summary>
    public double EdgeBorderThickness
    {
        get => GetValue(EdgeBorderThicknessProperty);
        set => SetValue(EdgeBorderThicknessProperty, value);
    }

    /// <summary>
    /// Whether the mask and the panel animate when the drawer opens and closes.
    /// A drawer nested in another drawer follows the parent drawer's value unless it sets its own.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the drawer has finished opening, after the open motion has completed.
    /// </summary>
    public event EventHandler? Opened;
    /// <summary>
    /// Raised when the drawer has finished closing, after the close motion has completed and the drawer container
    /// has left its presentation layer. <see cref="IsOpen" /> is already <see langword="false" />,
    /// and <see cref="CrossRootsChanged" /> follows right after.
    /// </summary>
    public event EventHandler? Closed;

    #endregion

    #region ISemanticPartCrossRootProvider 实现

    /// <summary>
    /// Raised when the set of visuals returned by <see cref="GetCrossRoots" /> changes: once the drawer
    /// container has been opened, after the drawer is closed, and when the drawer container is about to be
    /// released.
    /// </summary>
    public event EventHandler? CrossRootsChanged;

    /// <summary>
    /// Returns the container that holds the mask and the panel while it is hosted in a presentation layer.
    /// The container lives outside the visual tree of this control.
    /// </summary>
    /// <returns>
    /// A list containing the drawer container, or an empty list when the drawer has no hosted container.
    /// </returns>
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
    private OwnerTokenPropertyBindings? _drawerTokens;
    private Control? _openOnSizeChangedTarget;
    private int _visualTreeVersion;
    
    static Drawer()
    {
        SizeTypeProperty.OverrideDefaultValue<Drawer>(CustomizableSizeType.Small);
    }

    /// <summary>
    /// Returns the drawer that owns the drawer container surrounding the given element.
    /// </summary>
    /// <param name="element">
    /// A visual inside a drawer, for example part of its content.
    /// </param>
    /// <returns>
    /// The owning drawer, or <see langword="null" /> when no ancestor of the element is a drawer container, or
    /// when the owning drawer has already been collected.
    /// </returns>
    public static Drawer? GetDrawer(Visual element)
    {
        var container = element.FindAncestorOfType<DrawerContainer>();
        if (container?.Drawer != null && container.Drawer.TryGetTarget(out var drawer))
        {
            return drawer;
        }

        return null;
    }

    /// <summary>
    /// Called when the control is attached to the visual tree. Creates the motion binding and the token bindings,
    /// binds <see cref="OpenOn" /> and <see cref="IsMotionEnabled" /> to a parent drawer when this drawer is nested,
    /// and otherwise binds <see cref="OpenOn" /> to the enclosing <see cref="TopLevel" />. Opens the drawer when
    /// <see cref="IsOpen" /> is already <see langword="true" />. Overrides must call the base implementation.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        UpdateDrawerTokenBindings();
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

    /// <summary>
    /// Called when the control is detached from the visual tree. Disposes the motion binding, the token bindings,
    /// the bindings to a parent drawer or the enclosing <see cref="TopLevel" /> and the size subscription, then
    /// posts the release of the drawer container. The container is kept when the control is attached again before
    /// that callback runs. Overrides must call the base implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = null;
        _drawerTokens?.Dispose();
        _drawerTokens = null;
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
        while (current != null && current is not ScopeAwareAdornerLayer)
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

    /// <summary>
    /// Routes the drawer to the presentation layer that hosts it. A drawer nested in another drawer shares the
    /// layer of its parent. When <c>OpenOn is TopLevel</c> the drawer goes to that TopLevel's T7
    /// <see cref="TopLevelPresentationLayer" />. In every other case, a local element or <see langword="null" />,
    /// it goes to the T5 <see cref="ContentPresentationLayer" /> of the scoped host.
    /// </summary>
    private ScopeAwareAdornerLayer? ResolvePresentationLayer(Control? openOn)
    {
        // Resolve again from the parent drawer's current OpenOn instead of reading the visual parent of the parent
        // container. When a parent drawer and its child move at the same time, the child may be notified before the
        // parent container has moved, and the result must be the parent drawer's target layer.
        if (FindParentDrawer() is { } parentDrawer &&
            parentDrawer.ResolvePresentationLayer(parentDrawer.OpenOn) is { } parentLayer)
        {
            return parentLayer;
        }

        if (openOn is TopLevel topLevel &&
            TopLevelPresentationLayer.GetLayer(topLevel) is { } topLevelLayer)
        {
            return topLevelLayer;
        }

        // When OpenOn lies inside this drawer's own container, resolving from the anchor lands in the container's own
        // surface host, and the container would be moved into a layer inside itself, which closes a cycle in the visual
        // tree. Resolve from outside the container instead: the scoped host the container is currently in.
        var anchor = openOn ?? (Control)this;
        if (_container is { } container && container.IsVisualAncestorOf(anchor))
        {
            anchor = container;
        }

        return ContentPresentationLayer.GetLayer(anchor);
    }

    /// <summary>
    /// Called when a property of the control changes. Opens or closes the drawer when <see cref="IsOpen" /> changes
    /// while the control is attached. Keeps the effective size, the size subscription and the size token in step
    /// with <see cref="OpenOn" />, <see cref="DialogSize" />, <see cref="Placement" /> and
    /// <see cref="SizeType" />, and moves an open drawer when <see cref="OpenOn" /> changes. Overrides must call
    /// the base implementation.
    /// </summary>
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
            UpdateDialogSizeTokenBinding();
        }
        
        if (change.Property == OpenOnProperty)
        {
            var newOpenOn = change.NewValue as Control;
            ScopeAwareAdornerLayer.SetAdornedElement(this, newOpenOn);
            if (IsOpen && _container is not null)
            {
                _container.UpdateOpenOn(
                    newOpenOn,
                    ResolvePresentationLayer(newOpenOn));
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
        var layer = ResolvePresentationLayer(OpenOn);
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
                    ResolvePresentationLayer(OpenOn);
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

    /// <summary>
    /// Called while the drawer opens, before the drawer container is created. The default implementation copies the
    /// <c>DataContext</c> of the nearest drawer that contains this one and tells every containing drawer that this
    /// drawer is about to open. An override that skips the base implementation leaves them uninformed.
    /// </summary>
    /// <param name="layer">
    /// The presentation layer the drawer is about to open in. The default implementation does not use it.
    /// </param>
    protected internal virtual void NotifyBeforeOpen(ScopeAwareAdornerLayer layer)
    {
        Drawer? firstParentDrawer = null;
        var current = Parent;
        while (current != null && current is not ScopeAwareAdornerLayer)
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

    /// <summary>
    /// Called while the drawer closes, before the drawer container is closed. The default implementation tells
    /// every drawer that contains this one that this drawer is about to close. An override that skips the base
    /// implementation leaves them uninformed.
    /// </summary>
    /// <param name="layer">
    /// The presentation layer the drawer is about to close in. The default implementation does not use it.
    /// </param>
    protected virtual void NotifyBeforeClose(ScopeAwareAdornerLayer layer)
    {
        var current = Parent;
        while (current != null && current is not ScopeAwareAdornerLayer)
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
        while (current != null && current is not ScopeAwareAdornerLayer)
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

    /// <summary>
    /// Called by the drawer container when the drawer has finished opening. Raises <see cref="Opened" />. Call
    /// the base implementation to raise the event.
    /// </summary>
    protected internal virtual void NotifyOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called by the drawer container when the drawer has finished closing and the container has left its
    /// presentation layer. Raises <see cref="Closed" /> and then <see cref="CrossRootsChanged" />. Call the
    /// base implementation to raise both events.
    /// </summary>
    protected internal virtual void NotifyClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
        RaiseCrossRootsChanged();
    }
    
    private void UpdateDrawerTokenBindings()
    {
        _drawerTokens?.Dispose();
        // The runtime overlay consumes these public values from the Drawer. Keep a single theme read group
        // on the Drawer so the placement scope and Template priority do not depend on where the overlay is attached.
        var tokens = new OwnerTokenPropertyBindings(this, 4, BindingPriority.Template);
        _drawerTokens = tokens;
        var ownerType = GetType();
        try
        {
            tokens.Bind(0, PushOffsetPercentProperty, ControlTokenResourceKey.Own(ownerType, DrawerTokenKind.PushOffsetPercent));
            tokens.Bind(1, EdgeBorderColorProperty, ControlTokenResourceKey.Own(ownerType, DrawerTokenKind.EdgeBorderColor));
            tokens.Bind(2, EdgeBorderThicknessProperty, ControlTokenResourceKey.Own(ownerType, DrawerTokenKind.EdgeBorderThickness));
            UpdateDialogSizeTokenBinding();
        }
        catch
        {
            _drawerTokens = null;
            tokens.Dispose();
            throw;
        }
    }

    private void UpdateDialogSizeTokenBinding()
    {
        var tokenKind = SizeType switch
        {
            CustomizableSizeType.Small  => DrawerTokenKind.SmallSize,
            CustomizableSizeType.Middle => DrawerTokenKind.MiddleSize,
            CustomizableSizeType.Large  => DrawerTokenKind.LargeSize,
            _                           => DrawerTokenKind.SmallSize
        };
        _drawerTokens?.Bind(3, DialogSizeProperty, ControlTokenResourceKey.Own(GetType(), tokenKind));
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
