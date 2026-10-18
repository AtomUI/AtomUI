using System.Globalization;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

internal class RateCharacter : Control
{
    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<RateCharacter>();
    
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<RateCharacter>();
    
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<RateCharacter>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<RateCharacter>();
    
    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<RateCharacter>();
    
    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<RateCharacter>();
    
    public static readonly StyledProperty<char?> CharacterProperty =
        AvaloniaProperty.Register<RateCharacter, char?>(nameof(Character));
    
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
    
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }
    
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }
    
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
    
    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }
    
    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }
    
    public char? Character
    {
        get => GetValue(CharacterProperty);
        set => SetValue(CharacterProperty, value);
    }
    #endregion

    private FormattedText? _formattedText;
    private Size _textSize;

    static RateCharacter()
    {
        AffectsMeasure<RateCharacter>(CharacterProperty, FontSizeProperty, FontStyleProperty, FontWeightProperty);
        AffectsRender<RateCharacter>(BackgroundProperty, ForegroundProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CharacterProperty ||
            change.Property == FontSizeProperty ||
            change.Property == FontStyleProperty ||
            change.Property == FontWeightProperty ||
            change.Property == ForegroundProperty ||
            change.Property == FontFamilyProperty)
        {
            _formattedText = new FormattedText($"{Character}", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight), FontSize, Foreground);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _textSize = TextUtils.CalculateTextSize($"{Character}", FontSize, FontFamily, FontStyle, FontWeight);
        return _textSize;
    }

    public sealed override void Render(DrawingContext context)
    {
        if (_formattedText != null)
        {
            using var state = context.PushTextOptions(new TextOptions { TextRenderingMode = TextRenderingMode.Antialias });
            context.DrawRectangle(Background, null, new Rect(Bounds.Size));
            var offsetX = (Bounds.Width - _textSize.Width) / 2;
            var offsetY = (Bounds.Height - _textSize.Height) / 2 + 1;
            context.DrawText(_formattedText,  new Point(offsetX, offsetY));
        }
    }
}
