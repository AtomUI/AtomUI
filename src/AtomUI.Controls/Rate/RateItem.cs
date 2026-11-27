using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

internal enum RateItemSelectedState
{
    None,
    FullSelected,
    HalfSelected
}

internal class RateItem : TemplatedControl
{
    #region 公共属性定义
    public static readonly StyledProperty<RateItemSelectedState> SelectedStateProperty =
        AvaloniaProperty.Register<RateItem, RateItemSelectedState>(nameof(SelectedState), RateItemSelectedState.None);
    
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AbstractRate.IsAllowClearProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<bool> IsAllowHalfProperty =
        AbstractRate.IsAllowHalfProperty.AddOwner<RateItem>();

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<object?> CharacterProperty =
        AbstractRate.CharacterProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<IBrush?> StarColorProperty =
        AbstractRate.StarColorProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<IBrush?> StarBgColorProperty =
        AbstractRate.StarBgColorProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<RateItem>();
    
    public RateItemSelectedState SelectedState
    {
        get => GetValue(SelectedStateProperty);
        set => SetValue(SelectedStateProperty, value);
    }
    
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    public bool IsAllowHalf
    {
        get => GetValue(IsAllowHalfProperty);
        set => SetValue(IsAllowHalfProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public object? Character
    {
        get => GetValue(CharacterProperty);
        set => SetValue(CharacterProperty, value);
    }
    
    public IBrush? StarColor
    {
        get => GetValue(StarColorProperty);
        set => SetValue(StarColorProperty, value);
    }
    
    public IBrush? StarBgColor
    {
        get => GetValue(StarBgColorProperty);
        set => SetValue(StarBgColorProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly DirectProperty<RateItem, VisualBrush?> CharacterBgBrushProperty =
        AvaloniaProperty.RegisterDirect<RateItem, VisualBrush?>(
            nameof(CharacterBgBrush),
            o => o.CharacterBgBrush,
            (o, v) => o.CharacterBgBrush = v);
    
    internal static readonly DirectProperty<RateItem, VisualBrush?> CharacterBrushProperty =
        AvaloniaProperty.RegisterDirect<RateItem, VisualBrush?>(
            nameof(CharacterBrush),
            o => o.CharacterBrush,
            (o, v) => o.CharacterBrush = v);
    
    internal static readonly DirectProperty<RateItem, Geometry?> StarClipProperty =
        AvaloniaProperty.RegisterDirect<RateItem, Geometry?>(
            nameof(StarClip),
            o => o.StarClip,
            (o, v) => o.StarClip = v);
    
    internal static readonly DirectProperty<RateItem, bool> IsFocusStartItemProperty =
        AvaloniaProperty.RegisterDirect<RateItem, bool>(
            nameof(IsFocusStartItem),
            o => o.IsFocusStartItem,
            (o, v) => o.IsFocusStartItem = v);
    
    internal static readonly StyledProperty<double> HoverScaleProperty =
        AvaloniaProperty.Register<RateItem, double>(nameof(HoverScale));
    
    internal static readonly StyledProperty<ITransform?> HoverRenderTransformProperty =
        AvaloniaProperty.Register<RateItem, ITransform?>(nameof (HoverRenderTransform));
    
    private VisualBrush? _characterBgBrush;

    /// <summary>
    /// Brush that paints the character of the item in its unselected state: a <see cref="VisualBrush" /> whose
    /// visual is built from <see cref="Character" /> and filled with <see cref="StarBgColor" />. The item rebuilds
    /// it when <see cref="Character" />, <see cref="StarColor" />, <see cref="StarBgColor" />,
    /// <see cref="SizeType" /> or the font size changes while it is attached, and when the template is applied. The
    /// template fills the background rectangle of the item with it.
    /// </summary>
    internal VisualBrush? CharacterBgBrush
    {
        get => _characterBgBrush;
        set => SetAndRaise(CharacterBgBrushProperty, ref _characterBgBrush, value);
    }
    
    private VisualBrush? _characterBrush;

    /// <summary>
    /// Brush that paints the character of the item in its selected state: a <see cref="VisualBrush" /> whose visual
    /// is built from <see cref="Character" /> and filled with <see cref="StarColor" />. The item rebuilds it under
    /// the same conditions as <see cref="CharacterBgBrush" />. The theme fills the active rectangle of the item
    /// with it while <see cref="SelectedState" /> is not <see cref="RateItemSelectedState.None" />.
    /// </summary>
    internal VisualBrush? CharacterBrush
    {
        get => _characterBrush;
        set => SetAndRaise(CharacterBrushProperty, ref _characterBrush, value);
    }
    
    private Geometry? _starClip;

    /// <summary>
    /// Clip of the active rectangle of the item, the rectangle the theme fills with <see cref="CharacterBrush" />.
    /// While <see cref="IsAllowHalf" /> is <see langword="true" /> and <see cref="SelectedState" /> is
    /// <see cref="RateItemSelectedState.HalfSelected" />, the item sets it to the left half of the item, which shows
    /// half a selected character; otherwise it is <see langword="null" />. The item updates it when
    /// <see cref="IsAllowHalf" />, <see cref="SelectedState" />, the width, the height or the font size changes.
    /// </summary>
    internal Geometry? StarClip
    {
        get => _starClip;
        set => SetAndRaise(StarClipProperty, ref _starClip, value);
    }

    /// <summary>
    /// Scale factor of the character of the item while it is hovered or marked as the start of the keyboard
    /// stepping, <c>1</c> for no scaling. The item turns every change into the uniform scale of
    /// <see cref="HoverRenderTransform" />. The default is <c>0</c>, which would collapse the scaled item, so a
    /// theme has to set it; the built-in theme takes it from the <c>StarHoverScale</c> Token.
    /// </summary>
    internal double HoverScale
    {
        get => GetValue(HoverScaleProperty);
        set => SetValue(HoverScaleProperty, value);
    }
    
    /// <summary>
    /// Transform that the theme applies to the item while it is hovered or marked as the start of the keyboard
    /// stepping: the uniform scale built from <see cref="HoverScale" />, around the center of the item. The item
    /// replaces it whenever <see cref="HoverScale" /> changes; a value set directly lasts only until then.
    /// </summary>
    internal ITransform? HoverRenderTransform
    {
        get => GetValue(HoverRenderTransformProperty);
        set => SetValue(HoverRenderTransformProperty, value);
    }
    
    private bool _isFocusStartItem;

    /// <summary>
    /// Whether the rate marks the item as the start of its keyboard stepping: the rate sets it on the star that
    /// was pressed when the user changes the value with the Left and Right keys, and clears it when the rate
    /// loses focus. On the marked item the theme shows the dashed outline in <see cref="StarColor" /> and applies
    /// <see cref="HoverRenderTransform" />, as on a hovered item.
    /// </summary>
    internal bool IsFocusStartItem
    {
        get => _isFocusStartItem;
        set => SetAndRaise(IsFocusStartItemProperty, ref _isFocusStartItem, value);
    }
    
    #endregion

    static RateItem()
    {
        AffectsMeasure<RateItem>(SizeTypeProperty);
        AffectsRender<RateItem>(CharacterProperty, StarColorProperty, StarBgColorProperty);
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == CharacterProperty ||
                change.Property == StarColorProperty ||
                change.Property == StarBgColorProperty ||
                change.Property == SizeTypeProperty ||
                change.Property == FontSizeProperty)
            {
                ConfigureCharacterBrushes();
            }
        }

        if (change.Property == IsAllowHalfProperty ||
            change.Property == SelectedStateProperty ||
            change.Property == WidthProperty ||
            change.Property == HeightProperty ||
            change.Property == FontSizeProperty)
        {
            ConfigureStarClip();
        }

        if (change.Property == HoverScaleProperty)
        {
            var builder = TransformOperations.CreateBuilder(1);
            builder.AppendScale(HoverScale, HoverScale);
            SetCurrentValue(HoverRenderTransformProperty, builder.Build());
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureCharacterBrushes();
    }

    private void ConfigureCharacterBrushes()
    {
        SetCurrentValue(CharacterBgBrushProperty, BuildCharacterVisualBrush(StarBgColor));
        SetCurrentValue(CharacterBrushProperty, BuildCharacterVisualBrush(StarColor));
    }

    private void ConfigureStarClip()
    {
        if (IsAllowHalf)
        {
            if (SelectedState == RateItemSelectedState.HalfSelected)
            {
                var geometry = new RectangleGeometry(new Rect(0, 0, Width / 2, Height));
                SetCurrentValue(StarClipProperty, geometry);
            }
            else
            {
                SetCurrentValue(StarClipProperty, null);
            }
        }
        else
        {
            SetCurrentValue(StarClipProperty, null);
        }
    }

    private VisualBrush BuildCharacterVisualBrush(IBrush? brush)
    {
        Control? charControl = null;
        if (Character is Icon iconChar)
        {
            var newIcon = iconChar.CreateInstance();
            charControl = newIcon;
            newIcon.FillBrush   = brush;
            newIcon.StrokeBrush = brush;
        }
        else if (Character is char character)
        {
            charControl = new RateCharacter()
            {
                Character  = character,
                Foreground = brush,
                FontSize   = double.IsNaN(Height) ? FontSize : Height,
            };
        }
        else if (Character is string str && str.Length >= 1)
        {
            charControl = new RateCharacter()
            {
                Character  = str[0],
                Foreground = brush,
                FontSize   = double.IsNaN(Height) ? FontSize : Height,
            };
        }

        if (charControl != null)
        {
            charControl.Width  = Width;
            charControl.Height = Height;
        }
        charControl?.Measure(Size.Infinity);
        return new VisualBrush()
        {
            Visual = charControl,
        };
    }

}
