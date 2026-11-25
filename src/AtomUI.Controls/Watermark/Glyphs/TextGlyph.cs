using System.Globalization;
using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;

namespace AtomUI.Controls;

public class TextGlyph : WatermarkGlyph
{
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty
        .Register<TextGlyph, string?>(nameof(Text));

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty
        .Register<TextGlyph, double>(nameof(FontSize), 16);

    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    // Preserve the unattached glyph's public metadata default. Attached watermark consumers use scoped
    // ColorTextBase below; this is not a failed-resource-lookup fallback.
#pragma warning disable ATOMUIGEN044
    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty
        .Register<TextGlyph, IBrush>(nameof(Foreground), Brushes.Black);
#pragma warning restore ATOMUIGEN044

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly StyledProperty<FontFamily> FontFamilyProperty = AvaloniaProperty
        .Register<TextGlyph, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public static readonly StyledProperty<FontWeight> FontWeightProperty = AvaloniaProperty
        .Register<TextGlyph, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public static readonly StyledProperty<FontStyle> FontStyleProperty = AvaloniaProperty
        .Register<TextGlyph, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    protected FormattedText? FormattedText { get; private set; }

    static TextGlyph()
    {
        TextProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontFamilyProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontWeightProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontStyleProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontSizeProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.UpdateFormatText());
        ForegroundProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.UpdateFormatText());
    }

    protected virtual void RebuildFormatText()
    {
        if (Text == null)
        {
            FormattedText = null;
            return;
        }

        FormattedText = new FormattedText(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight),
            FontSize,
            Foreground)
        {
            TextAlignment = TextAlignment.Center
        };
    }

    private void UpdateFormatText()
    {
        if (FormattedText == null)
        {
            return;
        }

        FormattedText.SetFontSize(FontSize);
        FormattedText.SetForegroundBrush(Foreground);
    }

    public override void Render(DrawingContext context)
    {
        if (FormattedText == null)
        {
            return;
        }

        context.DrawText(FormattedText, new Point());
    }

    public override Size GetDesiredSize()
    {
        return FormattedText == null ? new Size() : new Size(FormattedText.Width, FormattedText.Height);
    }

    #region 内部协作 API

    internal IDisposable CreateThemeDefaultBindings()
    {
        // A glyph is a non-Visual resource-hosted drawing object, not a ControlTheme target. The owning
        // WatermarkGlyph attachment disposes these bindings before its scoped host is released.
        return new CompositeDisposable(
            TokenResourceBinder.CreateGlobalTokenBinding(this, FontSizeProperty, SharedTokenKind.FontSizeLG,
                BindingPriority.Style),
            TokenResourceBinder.CreateGlobalTokenBinding(this, ForegroundProperty, SharedTokenKind.ColorTextBase,
                BindingPriority.Style));
    }

    // FormattedText 的布局尺寸不包含字形越界。此方法只用于确切的内置 TextGlyph，
    // 自定义子类的 Render 不受该边界约束。
    internal Rect GetDrawingBounds()
    {
        var bounds = new Rect(GetDesiredSize());
        if (FormattedText is not { } text || !double.IsFinite(text.Extent) || text.Extent <= 0)
        {
            return bounds;
        }

        var inkBounds = new Rect(text.OverhangLeading,
            text.Height + text.OverhangAfter - text.Extent,
            Math.Max(0, text.Width - text.OverhangTrailing - text.OverhangLeading), text.Extent);
        return bounds.Union(inkBounds);
    }

    #endregion
}
