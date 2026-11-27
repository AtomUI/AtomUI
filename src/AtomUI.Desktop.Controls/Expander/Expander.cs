using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaExpander = Avalonia.Controls.Expander;

/// <summary>
/// Element of an <see cref="Expander" /> that toggles its expanded state and takes keyboard focus.
/// </summary>
public enum ExpanderTriggerType
{
    /// <summary>
    /// The whole header toggles the expander and is the keyboard stop of the control.
    /// </summary>
    Header,
    /// <summary>
    /// Only the expand icon button toggles the expander; the button is the keyboard stop instead of the control.
    /// </summary>
    Icon
}

/// <summary>
/// Side of the expander header that holds the expand icon.
/// </summary>
public enum ExpanderIconPosition
{
    /// <summary>
    /// Before the title.
    /// </summary>
    Start,
    /// <summary>
    /// After the title and the add-on content.
    /// </summary>
    End
}

/// <summary>
/// A panel with a header that shows or hides the content in the direction of
/// <see cref="Avalonia.Controls.Expander.ExpandDirection" />. The element that toggles the expander is
/// selected with <see cref="TriggerType" />.
/// </summary>
[PseudoClasses(
    ExpanderPseudoClass.Expanded,
    ExpanderPseudoClass.ExpandUp, 
    ExpanderPseudoClass.ExpandDown,
    ExpanderPseudoClass.ExpandLeft,
    ExpanderPseudoClass.ExpandRight)]
public partial class Expander : AvaloniaExpander, IMotionAwareControl, IFocusOutlineShapeProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Expander>();

    /// <summary>
    /// Identifies the <see cref="IsShowExpandIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowExpandIconProperty =
        AvaloniaProperty.Register<Expander, bool>(nameof(IsShowExpandIcon), true);

    /// <summary>
    /// Identifies the <see cref="ExpandIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> ExpandIconProperty =
        AvaloniaProperty.Register<Expander, PathIcon?>(nameof(ExpandIcon));

    /// <summary>
    /// Identifies the <see cref="AddOnContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> AddOnContentProperty =
        AvaloniaProperty.Register<Expander, object?>(nameof(AddOnContent));

    /// <summary>
    /// Identifies the <see cref="AddOnContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> AddOnContentTemplateProperty =
        AvaloniaProperty.Register<Expander, IDataTemplate?>(nameof(AddOnContentTemplate));

    /// <summary>
    /// Identifies the <see cref="IsGhostStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsGhostStyleProperty =
        AvaloniaProperty.Register<Expander, bool>(nameof(IsGhostStyle));

    /// <summary>
    /// Identifies the <see cref="IsBorderless" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<Expander, bool>(nameof(IsBorderless));

    /// <summary>
    /// Identifies the <see cref="TriggerType" /> property.
    /// </summary>
    public static readonly StyledProperty<ExpanderTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<Expander, ExpanderTriggerType>(nameof(TriggerType));

    /// <summary>
    /// Identifies the <see cref="ExpandIconPosition" /> property.
    /// </summary>
    public static readonly StyledProperty<ExpanderIconPosition> ExpandIconPositionProperty =
        AvaloniaProperty.Register<Expander, ExpanderIconPosition>(nameof(ExpandIconPosition));
    
    /// <summary>
    /// Identifies the <see cref="HeaderPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness?> HeaderPaddingProperty =
        AvaloniaProperty.Register<Expander, Thickness?>(nameof(HeaderPadding));
    
    /// <summary>
    /// Identifies the <see cref="ContentPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<Thickness?> ContentPaddingProperty =
        AvaloniaProperty.Register<Expander, Thickness?>(nameof(ContentPadding));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Expander>();
    
    /// <summary>
    /// Size preset of the expander. It selects the padding of the header and of the content and the font size
    /// and line height of the header title. <see cref="CustomizableSizeType.Custom" /> uses the values of
    /// <see cref="CustomizableSizeType.Middle" />, and <see cref="CustomizableSizeType.Small" /> its font. An
    /// explicit <see cref="HeaderPadding" /> or <see cref="ContentPadding" /> replaces the padding of the preset.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Value for showing the expand icon in the header. While <see langword="false" />, the icon is hidden. When
    /// <see cref="TriggerType" /> is <see cref="ExpanderTriggerType.Icon" />, the button is the only trigger, so
    /// the hidden icon leaves setting <see cref="Avalonia.Controls.Expander.IsExpanded" /> as the only way to
    /// expand or collapse.
    /// </summary>
    public bool IsShowExpandIcon
    {
        get => GetValue(IsShowExpandIconProperty);
        set => SetValue(IsShowExpandIconProperty, value);
    }

    /// <summary>
    /// Icon shown by the expand icon button. <see langword="null" /> gives the built-in default, a right-pointing
    /// arrow. The button turns a quarter turn while the expander is expanded.
    /// </summary>
    public PathIcon? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }

    /// <summary>
    /// Extra content in the header, placed after the title. With <see cref="ExpanderIconPosition.End" /> as
    /// icon position, the expand icon sits to its right.
    /// </summary>
    public object? AddOnContent
    {
        get => GetValue(AddOnContentProperty);
        set => SetValue(AddOnContentProperty, value);
    }

    /// <summary>
    /// Template used to present <see cref="AddOnContent" />. When <see langword="null" />, the presenter shows a
    /// control as it is and any other value as text.
    /// </summary>
    public IDataTemplate? AddOnContentTemplate
    {
        get => GetValue(AddOnContentTemplateProperty);
        set => SetValue(AddOnContentTemplateProperty, value);
    }

    /// <summary>
    /// Value for the ghost look: header and content paint no background, and the frame border and the divider
    /// between header and content are not drawn. It takes precedence over <see cref="IsBorderless" />.
    /// </summary>
    public bool IsGhostStyle
    {
        get => GetValue(IsGhostStyleProperty);
        set => SetValue(IsGhostStyleProperty, value);
    }

    /// <summary>
    /// Value for the look without a border: the frame border and the divider between header and content are not
    /// drawn, and the content paints the header background instead of the content background.
    /// </summary>
    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }

    /// <summary>
    /// Element that toggles the expanded state and takes keyboard focus. With
    /// <see cref="ExpanderTriggerType.Header" /> it is the whole header: a left mouse press anywhere on it
    /// toggles, and Enter and Space without modifiers toggle while the expander has focus. A click on the
    /// content gives the expander focus but keeps those keys off until the header is pressed again or the
    /// keyboard or code moves the focus to it. With <see cref="ExpanderTriggerType.Icon" /> it is the expand
    /// icon button alone, and the expander itself is not a keyboard stop.
    /// </summary>
    public ExpanderTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    /// <summary>
    /// Side of the header that holds the expand icon. When <see cref="HeaderPadding" /> is set, the margin
    /// between the icon and the title is taken from that padding instead of the theme spacing.
    /// </summary>
    public ExpanderIconPosition ExpandIconPosition
    {
        get => GetValue(ExpandIconPositionProperty);
        set => SetValue(ExpandIconPositionProperty, value);
    }
    
    /// <summary>
    /// Padding inside the header, around the expand icon, the title and the add-on content. When
    /// <see langword="null" />, the padding of the <see cref="SizeType" /> preset is used. A value replaces it
    /// for every size and also sets the spacing between the expand icon and the title.
    /// </summary>
    public Thickness? HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    /// <summary>
    /// Padding inside the content area, between the body frame and the expanded content. When
    /// <see langword="null" />, the padding of the <see cref="SizeType" /> preset is used.
    /// </summary>
    public Thickness? ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    
    /// <summary>
    /// Value for the motion of the expander, set on the control and not inherited. While
    /// <see langword="false" />, the content appears and disappears without the expand and collapse motion.
    /// The value is also passed to the expand icon button. The theme takes it from the
    /// <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<Thickness> ContentBorderThicknessProperty =
        AvaloniaProperty.Register<Expander, Thickness>(nameof(ContentBorderThickness));
    
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<Expander>();
    
    internal static readonly DirectProperty<Expander, Thickness> EffectiveBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Expander, Thickness>(nameof(EffectiveBorderThickness),
            o => o.EffectiveBorderThickness,
            (o, v) => o.EffectiveBorderThickness = v);

    internal static readonly DirectProperty<Expander, Thickness> EffectiveExpandButtonMarginProperty =
        AvaloniaProperty.RegisterDirect<Expander, Thickness>(nameof(EffectiveExpandButtonMargin),
            o => o.EffectiveExpandButtonMargin,
            (o, v) => o.EffectiveExpandButtonMargin = v);

    internal Thickness ContentBorderThickness
    {
        get => GetValue(ContentBorderThicknessProperty);
        set => SetValue(ContentBorderThicknessProperty, value);
    }

    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    private Thickness _effectiveBorderThickness;

    internal Thickness EffectiveBorderThickness
    {
        get => _effectiveBorderThickness;
        set => SetAndRaise(EffectiveBorderThicknessProperty, ref _effectiveBorderThickness, value);
    }

    private Thickness _effectiveExpandButtonMargin;

    internal Thickness EffectiveExpandButtonMargin
    {
        get => _effectiveExpandButtonMargin;
        set => SetAndRaise(EffectiveExpandButtonMarginProperty, ref _effectiveExpandButtonMargin, value);
    }

    #endregion

    private Control? _headerDecorator;
    private IconButton? _expandButton;
    private DashedBorder? _frame;
    // Avalonia focuses the nearest focusable ancestor of a pressed element, so a press on plain content gives
    // the Expander focus; Ant Design only toggles from the header, so toggle keys stay off until focus comes
    // from the keyboard or a header press.
    private bool _isToggleKeySuppressed;

    static Expander()
    {
        // The header is the keyboard stop (the template marks it as the focus target).
        FocusableProperty.OverrideDefaultValue<Expander>(true);
    }

    public Expander()
    {
        AddHandler(PointerPressedEvent, HandlePreviewPointerPressed, RoutingStrategies.Tunnel, true);
    }
    
    /// <summary>
    /// Called when the control template is applied. It finds the header, the expand icon button and the frame
    /// in the template, subscribes to the click of the icon button and refreshes the derived state: the frame
    /// border, the content divider, the icon button margin, the default icon and the pseudo-classes. An override
    /// must call the base member, or the expander does not toggle by pointer.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_expandButton is not null)
        {
            _expandButton.Click -= HandleExpandButtonClicked;
        }

        _headerDecorator = e.NameScope.Find<Control>("PART_HeaderDecorator");
        _expandButton    = e.NameScope.Find<IconButton>("PART_ExpandButton");
        _frame           = e.NameScope.Find<DashedBorder>("PART_Frame");


        if (_expandButton is not null)
        {
            _expandButton.Click += HandleExpandButtonClicked;
        }

        SetupEffectiveBorderThickness();
        ConfigureContentBorderThickness();
        UpdateEffectiveExpandButtonMargin();
        SetupDefaultIcon();
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Called when the value of a property of the control changes. It refreshes the state derived from the
    /// changed property: the default expand icon, the frame border, the content divider, the margin of the
    /// expand icon button and the custom-padding pseudo-classes. An override must call the base member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == ExpandIconProperty)
            {
                SetupDefaultIcon();
            }
        }

        if (change.Property == BorderThicknessProperty ||
            change.Property == IsGhostStyleProperty ||
            change.Property == IsBorderlessProperty ||
            change.Property == ExpandDirectionProperty)
        {
            SetupEffectiveBorderThickness();
            ConfigureContentBorderThickness();
        }

        if (change.Property == HeaderPaddingProperty ||
            change.Property == ExpandIconPositionProperty)
        {
            UpdateEffectiveExpandButtonMargin();
        }

        if (change.Property == ContentPaddingProperty ||
            change.Property == HeaderPaddingProperty)
        {
            UpdatePseudoClasses();
        }
    }

    /// <summary>
    /// Called when a pointer press bubbles up to the control. When <see cref="TriggerType" /> is
    /// <see cref="ExpanderTriggerType.Header" />, a left mouse press inside the header toggles the expanded
    /// state and marks the press as handled; every other press is left unhandled.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!CanToggleFromHeaderPointer(e))
        {
            return;
        }

        ToggleExpanded();
        e.Handled = true;
    }

    /// <summary>
    /// Called when the control or a descendant receives focus. When the control itself receives focus in a way
    /// other than a pointer press, it re-enables the Enter and Space toggle keys that a click on the content
    /// had switched off.
    /// </summary>
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, this) && e.NavigationMethod != NavigationMethod.Pointer)
        {
            _isToggleKeySuppressed = false;
        }
    }

    /// <summary>
    /// Called when a key press bubbles up to the control. When <see cref="TriggerType" /> is
    /// <see cref="ExpanderTriggerType.Header" />, Enter or Space without modifiers toggles the expanded state
    /// and marks the key as handled, as long as the event comes from the control itself and the keys are not
    /// switched off by a click on the content. Every other key press is left unhandled.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled ||
            _isToggleKeySuppressed ||
            !ReferenceEquals(e.Source, this) ||
            TriggerType != ExpanderTriggerType.Header ||
            e.KeyModifiers != KeyModifiers.None ||
            e.Key is not (Key.Enter or Key.Space))
        {
            return;
        }

        ToggleExpanded();
        e.Handled = true;
    }

    // The header has no radius of its own; PART_Frame rounds and clips it. Report the frame corners the
    // header touches, in the header's own space (left / right headers are rotated 90 degrees clockwise).
    CornerRadius IFocusOutlineShapeProvider.FocusOutlineCornerRadius
    {
        get
        {
            var frame = _frame?.CornerRadius ?? default;
            return (ExpandDirection, IsExpanded) switch
            {
                (ExpandDirection.Down, true) => new CornerRadius(frame.TopLeft, frame.TopRight, 0, 0),
                (ExpandDirection.Up, true) => new CornerRadius(0, 0, frame.BottomRight, frame.BottomLeft),
                (ExpandDirection.Left, true) => new CornerRadius(frame.TopRight, frame.BottomRight, 0, 0),
                (ExpandDirection.Right, true) => new CornerRadius(0, 0, frame.BottomLeft, frame.TopLeft),
                (ExpandDirection.Left or ExpandDirection.Right, false) =>
                    new CornerRadius(frame.TopRight, frame.BottomRight, frame.BottomLeft, frame.TopLeft),
                _ => frame
            };
        }
    }

    private void HandleExpandButtonClicked(object? sender, RoutedEventArgs args)
    {
        ToggleExpanded();
        args.Handled = true;
    }

    // Runs in the tunnel phase, before the focus manager focuses the Expander for a content press.
    private void HandlePreviewPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isToggleKeySuppressed = !IsPointerInHeader(e);
        }
    }

    private bool CanToggleFromHeaderPointer(PointerPressedEventArgs e)
    {
        return TriggerType == ExpanderTriggerType.Header &&
               e.Pointer.Type == PointerType.Mouse &&
               e.GetCurrentPoint(this).Properties.IsLeftButtonPressed &&
               IsPointerInHeader(e);
    }

    private bool IsPointerInHeader(PointerEventArgs e)
    {
        return _headerDecorator is not null &&
               new Rect(_headerDecorator.Bounds.Size).Contains(e.GetPosition(_headerDecorator));
    }

    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }

    private void SetupDefaultIcon()
    {
        if (ExpandIcon is null)
        {
            ClearValue(ExpandIconProperty);
            SetValue(ExpandIconProperty, new RightOutlined(), BindingPriority.Template);
        }

        Debug.Assert(ExpandIcon is not null);
    }

    private void ConfigureContentBorderThickness()
    {
        if (IsBorderless || IsGhostStyle)
        {
            ContentBorderThickness = default;
            return;
        }

        var line = BorderThickness.Bottom;
        ContentBorderThickness = ExpandDirection switch
        {
            ExpandDirection.Down => new Thickness(0, line, 0, 0),
            ExpandDirection.Up => new Thickness(0, 0, 0, line),
            ExpandDirection.Left => new Thickness(0, 0, line, 0),
            ExpandDirection.Right => new Thickness(line, 0, 0, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(ExpandDirection), ExpandDirection,
                "Invalid value for ExpandDirection")
        };
    }

    private void SetupEffectiveBorderThickness()
    {
        if (IsBorderless || IsGhostStyle)
        {
            EffectiveBorderThickness = default;
        }
        else
        {
            EffectiveBorderThickness = BorderThickness;
        }
    }

    private void UpdateEffectiveExpandButtonMargin()
    {
        if (HeaderPadding is not { } headerPadding)
        {
            EffectiveExpandButtonMargin = default;
            return;
        }

        EffectiveExpandButtonMargin = ExpandIconPosition == ExpanderIconPosition.Start
            ? new Thickness(0, 0, headerPadding.Left, 0)
            : new Thickness(headerPadding.Right, 0, 0, 0);
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ExpanderPseudoClass.CustomHeaderPadding, HeaderPadding != null);
        PseudoClasses.Set(ExpanderPseudoClass.CustomContentPadding, ContentPadding != null);
    }
}
