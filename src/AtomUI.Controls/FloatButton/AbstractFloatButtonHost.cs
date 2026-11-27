using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of controls that create a float button at runtime and show it in the scoped overlay layer of the
/// nearest layer host, instead of in their own visual tree. Each property declared on the host is forwarded to
/// the button, where it replaces what the theme of the button sets for the same property. The button exists only
/// while the host is attached to a visual tree.
/// </summary>
public abstract class AbstractFloatButtonHost : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        AbstractFloatButton.PlacementProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetX" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        AbstractFloatButton.FloatOffsetXProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetY" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        AbstractFloatButton.FloatOffsetYProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AbstractFloatButton.IconProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="Tooltip" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TooltipProperty =
        AbstractFloatButton.TooltipProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="TooltipColor" /> property.
    /// </summary>
    public static readonly StyledProperty<Color?> TooltipColorProperty =
        AbstractFloatButton.TooltipColorProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<AbstractFloatButtonHost, object?>(nameof(Description));
    
    /// <summary>
    /// Identifies the <see cref="DescriptionTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> DescriptionTemplateProperty =
        AvaloniaProperty.Register<AbstractFloatButtonHost, IDataTemplate?>(nameof(DescriptionTemplate));
    
    /// <summary>
    /// Identifies the <see cref="ButtonType" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        AbstractFloatButton.ButtonTypeProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        AbstractFloatButton.ShapeProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="Href" /> property.
    /// </summary>
    public static readonly StyledProperty<Uri?> HrefProperty =
        AbstractFloatButton.HrefProperty.AddOwner<AbstractFloatButtonHost>();

    /// <summary>
    /// Identifies the <see cref="Command" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        Avalonia.Controls.Button.CommandProperty.AddOwner<AbstractFloatButtonHost>();

    /// <summary>
    /// Identifies the <see cref="CommandParameter" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        Avalonia.Controls.Button.CommandParameterProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        AbstractFloatButton.IsMotionEnabledProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="IsBadgeEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBadgeEnabledProperty =
        AbstractFloatButton.IsBadgeEnabledProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="IsDotBadge" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDotBadgeProperty =
        AbstractFloatButton.IsDotBadgeProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="BadgeCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> BadgeCountProperty =
        AbstractFloatButton.BadgeCountProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="BadgeColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> BadgeColorProperty =
        AbstractFloatButton.BadgeColorProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Identifies the <see cref="BadgeOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> BadgeOffsetProperty =
        AbstractFloatButton.BadgeOffsetProperty.AddOwner<AbstractFloatButtonHost>();

    /// <summary>
    /// Identifies the <see cref="BadgeOverflowCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> BadgeOverflowCountProperty =
        AbstractFloatButton.BadgeOverflowCountProperty.AddOwner<AbstractFloatButtonHost>();
    
    /// <summary>
    /// Gets or sets the edge or corner of the scoped overlay layer that the hosted button is anchored to.
    /// The value is forwarded to the button. Unset means <see cref="FloatButtonPlacement.Top" />, and the themes of
    /// the built-in hosts place the button at <see cref="FloatButtonPlacement.BottomRight" />.
    /// </summary>
    public FloatButtonPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal offset, in device-independent pixels, of the hosted button from the position
    /// chosen by <see cref="Placement" />. A positive value moves the button right, or left when the placement is
    /// <see cref="FloatButtonPlacement.Right" />, <see cref="FloatButtonPlacement.TopRight" /> or
    /// <see cref="FloatButtonPlacement.BottomRight" />; a negative value moves it the other way. The value is
    /// forwarded to the button. When unset, the themes of the built-in hosts set it from the <c>FloatOffsetX</c>
    /// token of the float button.
    /// </summary>
    public double FloatOffsetX
    {
        get => GetValue(FloatOffsetXProperty);
        set => SetValue(FloatOffsetXProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset, in device-independent pixels, of the hosted button from the position
    /// chosen by <see cref="Placement" />. A positive value moves the button down, or up when the placement is
    /// <see cref="FloatButtonPlacement.Bottom" />, <see cref="FloatButtonPlacement.BottomLeft" /> or
    /// <see cref="FloatButtonPlacement.BottomRight" />; a negative value moves it the other way. The value is
    /// forwarded to the button. When unset, the themes of the built-in hosts set it from the <c>FloatOffsetY</c>
    /// token of the float button.
    /// </summary>
    public double FloatOffsetY
    {
        get => GetValue(FloatOffsetYProperty);
        set => SetValue(FloatOffsetYProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon shown in the center of the hosted button, above the description. The value is forwarded
    /// to the button. When the host has no icon while the button is created, the button installs the default icon of
    /// its type, such as <c>FileTextOutlined</c> on a float button that is circular or has no description and
    /// <c>VerticalAlignTopOutlined</c> on a back-top button. A later change to <see langword="null" /> leaves the
    /// button without an icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the text of the tooltip shown when the pointer rests on the hosted button.
    /// The value is forwarded to the button. <see langword="null" /> shows no tooltip.
    /// </summary>
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the background color of the tooltip of the hosted button. The value is forwarded to the button.
    /// <see langword="null" /> keeps the background of the theme.
    /// </summary>
    public Color? TooltipColor
    {
        get => GetValue(TooltipColorProperty);
        set => SetValue(TooltipColorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the description of the hosted button, which shows it under the icon while its shape is square.
    /// The value is forwarded to the button as its content. <see langword="null" /> shows no description.
    /// </summary>
    [DependsOn(nameof(DescriptionTemplate))]
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that presents <see cref="Description" /> on the hosted button. The value is
    /// forwarded to the button. <see langword="null" /> leaves the presentation to the theme, which shows a string
    /// as text.
    /// </summary>
    public IDataTemplate? DescriptionTemplate
    {
        get => GetValue(DescriptionTemplateProperty);
        set => SetValue(DescriptionTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the visual type of the hosted button, which selects the icon color and the background of the
    /// button in the normal, hovered and pressed states. The value is forwarded to the button. Unset means
    /// <see cref="FloatButtonType.Default" />.
    /// </summary>
    public FloatButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the outline of the hosted button. A circle is sized to the theme's float button size and shows
    /// only an icon. A square is a rounded rectangle; when the button has a description, it shows the description
    /// under the icon and grows in height to fit it. The value is forwarded to the button. Unset means
    /// <see cref="FloatButtonShape.Circle" />.
    /// </summary>
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a uniform resource identifier. The value is forwarded to the hosted button, which does not read
    /// it, so setting it has no effect on rendering or on the click behavior.
    /// </summary>
    public Uri? Href
    {
        get => GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <summary>
    /// Gets or sets the command that runs when the hosted button is clicked. The value is forwarded to the button.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter passed to <see cref="Command" /> when the hosted button is clicked. The value is
    /// forwarded to the button.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the hosted button animates. <see langword="true" /> transitions the background color
    /// when the pointer hovers or presses the button and animates the badge when it appears or disappears;
    /// <see langword="false" /> shows the end state at once. The value is forwarded to the button. When unset, the
    /// themes of the built-in hosts set it from their own <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether a badge is shown over the top-right corner of the hosted button. The value is forwarded
    /// to the button. <see langword="true" /> shows a count badge, or a dot badge when <see cref="IsDotBadge" /> is
    /// <see langword="true" />.
    /// </summary>
    public bool IsBadgeEnabled
    {
        get => GetValue(IsBadgeEnabledProperty);
        set => SetValue(IsBadgeEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the badge of the hosted button is a plain dot instead of a number.
    /// The value is forwarded to the button and applies only while <see cref="IsBadgeEnabled" /> is
    /// <see langword="true" />.
    /// </summary>
    public bool IsDotBadge
    {
        get => GetValue(IsDotBadgeProperty);
        set => SetValue(IsDotBadgeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the number shown by the count badge of the hosted button. The value is forwarded to the button
    /// and applies only while the badge is enabled and <see cref="IsDotBadge" /> is <see langword="false" />.
    /// </summary>
    public int BadgeCount
    {
        get => GetValue(BadgeCountProperty);
        set => SetValue(BadgeCountProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the color of the badge of the hosted button: a color string, a preset color name such as
    /// <c>blue</c>, or a status name such as <c>error</c>. Preset and status names follow the current theme. The
    /// value is forwarded to the button. When the value is unset or cannot be resolved, the theme's error color is
    /// used.
    /// </summary>
    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the offset of the badge from the top-right corner of the hosted button, in device-independent
    /// pixels. A positive value moves the badge right and down. The value is forwarded to the button. Unset means
    /// no offset, unless the theme of the host sets one; <c>FloatButtonHost</c> sets an offset tuned for the shape
    /// and for a dot or a count badge.
    /// </summary>
    public Point BadgeOffset
    {
        get => GetValue(BadgeOffsetProperty);
        set => SetValue(BadgeOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the number above which the count badge of the hosted button shows the overflow count followed by
    /// a plus sign instead of the actual count. The value is forwarded to the button. Values below zero are treated
    /// as zero. Unset means 99.
    /// </summary>
    public int BadgeOverflowCount
    {
        get => GetValue(BadgeOverflowCountProperty);
        set => SetValue(BadgeOverflowCountProperty, value);
    }
    #endregion

    private protected AbstractFloatButton? FloatButton;
    private protected CompositeDisposable? Disposables;
    private ScopeAwareOverlayLayer? _overlayLayer;
    private VisualLayerManager? _surfaceHost;

    /// <summary>
    /// Called when the host is attached to a visual tree. It resolves the scoped overlay layer of the nearest layer
    /// host, creates the button through <see cref="CreateFloatButton" /> when the host has none, and shows the
    /// button in that layer. Overrides must call the base implementation, otherwise no button is created.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PrepareScopedOverlayLayer();
    }

    /// <summary>
    /// Called when the host is detached from a visual tree. It removes the button from the scoped overlay layer and
    /// disposes its bindings, so that attaching the host again creates a new button. Overrides must call the base
    /// implementation, otherwise the button stays in the layer.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        CleanupScopedOverlayLayer();
    }

    private void PrepareScopedOverlayLayer()
    {
        _overlayLayer = ScopeAwareOverlayLayer.GetLayer(this);
        Disposables?.Dispose();
        Disposables =   new CompositeDisposable();
        FloatButton ??= CreateFloatButton(Disposables);
        if (_overlayLayer != null)
        {
            // The float button enters the T4 layer wrapped in its own surface host. The surface host is created in
            // code because the template of the host cannot reach that layer. FloatingSurfaceHost explains why a
            // ControlTheme is not viable.
            _surfaceHost = FloatingSurfaceHost.Attach(_overlayLayer, FloatButton, Disposables);
        }
    }

    private void CleanupScopedOverlayLayer()
    {
        if (FloatButton != null)
        {
            FloatingSurfaceHost.Detach(_overlayLayer, _surfaceHost);
            _surfaceHost = null;
            Disposables?.Dispose();
            Disposables = null;
            FloatButton = null;
        }

        _overlayLayer = null;
    }

    /// <summary>
    /// When implemented in a derived class, creates the button that the host shows in the scoped overlay layer.
    /// It is called when the host is attached to a visual tree, before the button is added to the layer. The
    /// implementation typically calls <see cref="BindFloatButtonProperties" /> and adds what else it creates to
    /// <paramref name="disposables" />.
    /// </summary>
    /// <param name="disposables">
    /// Receives the bindings and other disposables of the button. It is disposed when the host is detached.
    /// </param>
    protected abstract AbstractFloatButton CreateFloatButton(CompositeDisposable disposables);

    /// <summary>
    /// Forwards the properties declared on this class, and the data context, from the host to the given button:
    /// icon, tooltip and its color, type, shape, link, command and its parameter, description and its template,
    /// motion switch, placement, float offsets and the badge settings. Call it from <see cref="CreateFloatButton" />
    /// with the disposables received there; the bindings are disposed together with them. Properties declared by a
    /// derived host are not included.
    /// </summary>
    /// <param name="floatButton">
    /// The button created by <see cref="CreateFloatButton" /> that receives the values.
    /// </param>
    /// <param name="disposables">
    /// Receives the bindings. It is disposed when the host is detached from the visual tree.
    /// </param>
    protected void BindFloatButtonProperties(AbstractFloatButton floatButton, CompositeDisposable disposables)
    {
        disposables.Add(BindUtils.RelayBind(this, IconProperty, floatButton, AbstractFloatButton.IconProperty));
        disposables.Add(BindUtils.RelayBind(this, TooltipProperty, floatButton, AbstractFloatButton.TooltipProperty));
        disposables.Add(BindUtils.RelayBind(this, TooltipColorProperty, floatButton, AbstractFloatButton.TooltipColorProperty));
        disposables.Add(BindUtils.RelayBind(this, ButtonTypeProperty, floatButton, AbstractFloatButton.ButtonTypeProperty));
        disposables.Add(BindUtils.RelayBind(this, ShapeProperty, floatButton, AbstractFloatButton.ShapeProperty));
        disposables.Add(BindUtils.RelayBind(this, HrefProperty, floatButton, AbstractFloatButton.HrefProperty));
        disposables.Add(BindUtils.RelayBind(this, CommandProperty, floatButton, Avalonia.Controls.Button.CommandProperty));
        disposables.Add(BindUtils.RelayBind(this, CommandParameterProperty, floatButton, Avalonia.Controls.Button.CommandParameterProperty));
        disposables.Add(BindUtils.RelayBind(this, DataContextProperty, floatButton, DataContextProperty));
        disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, floatButton, AbstractFloatButton.IsMotionEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, PlacementProperty, floatButton, AbstractFloatButton.PlacementProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetXProperty, floatButton, AbstractFloatButton.FloatOffsetXProperty));
        disposables.Add(BindUtils.RelayBind(this, FloatOffsetYProperty, floatButton, AbstractFloatButton.FloatOffsetYProperty));
        disposables.Add(BindUtils.RelayBind(this, DescriptionProperty, floatButton, ContentControl.ContentProperty));
        disposables.Add(BindUtils.RelayBind(this, DescriptionTemplateProperty, floatButton, ContentControl.ContentTemplateProperty));
        disposables.Add(BindUtils.RelayBind(this, IsBadgeEnabledProperty, floatButton, AbstractFloatButton.IsBadgeEnabledProperty));
        disposables.Add(BindUtils.RelayBind(this, IsDotBadgeProperty, floatButton, AbstractFloatButton.IsDotBadgeProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeCountProperty, floatButton, AbstractFloatButton.BadgeCountProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeColorProperty, floatButton, AbstractFloatButton.BadgeColorProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeOffsetProperty, floatButton, AbstractFloatButton.BadgeOffsetProperty));
        disposables.Add(BindUtils.RelayBind(this, BadgeOverflowCountProperty, floatButton, AbstractFloatButton.BadgeOverflowCountProperty));
    }
}
