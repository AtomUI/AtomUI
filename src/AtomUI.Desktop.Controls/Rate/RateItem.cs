using System.Diagnostics;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Utilities;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

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
        Rate.IsAllowClearProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<bool> IsAllowHalfProperty =
        Rate.IsAllowHalfProperty.AddOwner<RateItem>();

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<object?> CharacterProperty =
        Rate.CharacterProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<IBrush?> StarColorProperty =
        Rate.StarColorProperty.AddOwner<RateItem>();
    
    public static readonly StyledProperty<IBrush?> StarBgColorProperty =
        Rate.StarBgColorProperty.AddOwner<RateItem>();
    
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

    internal VisualBrush? CharacterBgBrush
    {
        get => _characterBgBrush;
        set => SetAndRaise(CharacterBgBrushProperty, ref _characterBgBrush, value);
    }
    
    private VisualBrush? _characterBrush;

    internal VisualBrush? CharacterBrush
    {
        get => _characterBrush;
        set => SetAndRaise(CharacterBrushProperty, ref _characterBrush, value);
    }
    
    private Geometry? _starClip;

    internal Geometry? StarClip
    {
        get => _starClip;
        set => SetAndRaise(StarClipProperty, ref _starClip, value);
    }

    internal double HoverScale
    {
        get => GetValue(HoverScaleProperty);
        set => SetValue(HoverScaleProperty, value);
    }
    
    internal ITransform? HoverRenderTransform
    {
        get => GetValue(HoverRenderTransformProperty);
        set => SetValue(HoverRenderTransformProperty, value);
    }
    
    private bool _isFocusStartItem;

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
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }

    private void ConfigureCharacterBrushes()
    {
        SetCurrentValue(CharacterBgBrushProperty, BuildCharacterVisualBrush(StarBgColor));
        SetCurrentValue(CharacterBrushProperty, BuildCharacterVisualBrush(StarColor));
    }
    
    private VisualBrush BuildCharacterVisualBrush(IBrush? brush)
    {
        Control? charControl = null;
        if (Character is Icon iconChar)
        {
            var iconType = iconChar.GetType();
            var newIcon  = Activator.CreateInstance(iconType) as Icon;
            Debug.Assert(newIcon != null);
            charControl       = newIcon;
            newIcon.FillBrush = brush;
            newIcon.StrokeBrush = brush;
        }
        else if (Character is char character)
        {
            charControl = new RateCharacter()
            {
                Character = character,
                Foreground = brush,
                FontSize = double.IsNaN(Height) ? FontSize : Height,
            };
        }
        else if (Character is string str && str.Length >= 1)
        {
            charControl = new RateCharacter()
            {
                Character  = str.First(),
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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureCharacterBrushes();
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
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions = [
                    TransitionUtils.CreateTransition<TransformOperationsTransition>(RenderTransformProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
}