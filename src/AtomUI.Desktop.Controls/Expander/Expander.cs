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

public enum ExpanderTriggerType
{
    Header,
    Icon
}

public enum ExpanderIconPosition
{
    Start,
    End
}

[PseudoClasses(
    ExpanderPseudoClass.Expanded,
    ExpanderPseudoClass.ExpandUp, 
    ExpanderPseudoClass.ExpandDown,
    ExpanderPseudoClass.ExpandLeft,
    ExpanderPseudoClass.ExpandRight)]
public partial class Expander : AvaloniaExpander, IMotionAwareControl, IFocusOutlineShapeProvider
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<Expander>();

    public static readonly StyledProperty<bool> IsShowExpandIconProperty =
        AvaloniaProperty.Register<Expander, bool>(nameof(IsShowExpandIcon), true);

    public static readonly StyledProperty<PathIcon?> ExpandIconProperty =
        AvaloniaProperty.Register<Expander, PathIcon?>(nameof(ExpandIcon));

    public static readonly StyledProperty<object?> AddOnContentProperty =
        AvaloniaProperty.Register<Expander, object?>(nameof(AddOnContent));

    public static readonly StyledProperty<IDataTemplate?> AddOnContentTemplateProperty =
        AvaloniaProperty.Register<Expander, IDataTemplate?>(nameof(AddOnContentTemplate));

    public static readonly StyledProperty<bool> IsGhostStyleProperty =
        AvaloniaProperty.Register<Expander, bool>(nameof(IsGhostStyle));

    public static readonly StyledProperty<bool> IsBorderlessProperty =
        AvaloniaProperty.Register<Expander, bool>(nameof(IsBorderless));

    public static readonly StyledProperty<ExpanderTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<Expander, ExpanderTriggerType>(nameof(TriggerType));

    public static readonly StyledProperty<ExpanderIconPosition> ExpandIconPositionProperty =
        AvaloniaProperty.Register<Expander, ExpanderIconPosition>(nameof(ExpandIconPosition));
    
    public static readonly StyledProperty<Thickness?> HeaderPaddingProperty =
        AvaloniaProperty.Register<Expander, Thickness?>(nameof(HeaderPadding));
    
    public static readonly StyledProperty<Thickness?> ContentPaddingProperty =
        AvaloniaProperty.Register<Expander, Thickness?>(nameof(ContentPadding));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Expander>();
    
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsShowExpandIcon
    {
        get => GetValue(IsShowExpandIconProperty);
        set => SetValue(IsShowExpandIconProperty, value);
    }

    public PathIcon? ExpandIcon
    {
        get => GetValue(ExpandIconProperty);
        set => SetValue(ExpandIconProperty, value);
    }

    public object? AddOnContent
    {
        get => GetValue(AddOnContentProperty);
        set => SetValue(AddOnContentProperty, value);
    }

    public IDataTemplate? AddOnContentTemplate
    {
        get => GetValue(AddOnContentTemplateProperty);
        set => SetValue(AddOnContentTemplateProperty, value);
    }

    public bool IsGhostStyle
    {
        get => GetValue(IsGhostStyleProperty);
        set => SetValue(IsGhostStyleProperty, value);
    }

    public bool IsBorderless
    {
        get => GetValue(IsBorderlessProperty);
        set => SetValue(IsBorderlessProperty, value);
    }

    public ExpanderTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    public ExpanderIconPosition ExpandIconPosition
    {
        get => GetValue(ExpandIconPositionProperty);
        set => SetValue(ExpandIconPositionProperty, value);
    }
    
    public Thickness? HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    public Thickness? ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    
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

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        if (ReferenceEquals(e.Source, this) && e.NavigationMethod != NavigationMethod.Pointer)
        {
            _isToggleKeySuppressed = false;
        }
    }

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
