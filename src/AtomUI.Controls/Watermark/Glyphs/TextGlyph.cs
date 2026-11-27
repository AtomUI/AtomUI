using System.Globalization;
using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AtomUI.Controls;

/// <summary>
/// A watermark glyph that draws <see cref="Text" /> with the font settings of this glyph. It is a non-visual
/// object without a template: <see cref="Render" /> paints the text and the watermark renderer tiles the glyph
/// at the size <see cref="GetDesiredSize" /> reports.
/// </summary>
public class TextGlyph : WatermarkGlyph
{
    /// <summary>
    /// The text to draw, or <see langword="null" /> to draw nothing and report an empty size, which keeps the
    /// watermark from tiling this glyph.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty
        .Register<TextGlyph, string?>(nameof(Text));

    /// <summary>
    /// Font size of <see cref="Text" />. The default is 16. Once a <see cref="Watermark" /> attaches the glyph
    /// to its target, the shared <c>FontSizeLG</c> token drives the property at style priority: a value set
    /// directly on the glyph wins, and clearing it returns to the token.
    /// </summary>
    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="FontSize" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty
        .Register<TextGlyph, double>(nameof(FontSize), 16);

    /// <summary>
    /// Brush that paints <see cref="Text" />. The default is black. Once a <see cref="Watermark" /> attaches
    /// the glyph to its target, the shared <c>ColorTextBase</c> token drives the property at style priority: a
    /// value set directly on the glyph wins, and clearing it returns to the token.
    /// </summary>
    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    // Preserve the unattached glyph's public metadata default. Attached watermark consumers use scoped
    // ColorTextBase below; this is not a failed-resource-lookup fallback.
#pragma warning disable ATOMUIGEN044
    /// <summary>
    /// Identifies the <see cref="Foreground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty
        .Register<TextGlyph, IBrush>(nameof(Foreground), Brushes.Black);
#pragma warning restore ATOMUIGEN044

    /// <summary>
    /// Font family used to shape <see cref="Text" />.
    /// </summary>
    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="FontFamily" /> property.
    /// </summary>
    public static readonly StyledProperty<FontFamily> FontFamilyProperty = AvaloniaProperty
        .Register<TextGlyph, FontFamily>(nameof(FontFamily), FontFamily.Default);

    /// <summary>
    /// Font weight used to shape <see cref="Text" />. The default is <c>Normal</c>.
    /// </summary>
    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="FontWeight" /> property.
    /// </summary>
    public static readonly StyledProperty<FontWeight> FontWeightProperty = AvaloniaProperty
        .Register<TextGlyph, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    /// <summary>
    /// Font style used to shape <see cref="Text" />. The default is <c>Normal</c>.
    /// </summary>
    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="FontStyle" /> property.
    /// </summary>
    public static readonly StyledProperty<FontStyle> FontStyleProperty = AvaloniaProperty
        .Register<TextGlyph, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    /// <summary>
    /// The formatted text that <see cref="Render" /> draws and <see cref="GetDesiredSize" /> measures, or
    /// <see langword="null" /> while <see cref="Text" /> is <see langword="null" />. A change of
    /// <see cref="Text" />, <see cref="FontFamily" />, <see cref="FontWeight" /> or <see cref="FontStyle" />
    /// replaces the instance; a change of <see cref="FontSize" /> or <see cref="Foreground" /> updates it in
    /// place.
    /// </summary>
    protected FormattedText? FormattedText { get; private set; }

    private CultureInfo? _textCulture;

    static TextGlyph()
    {
        TextProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontFamilyProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontWeightProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontStyleProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontSizeProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.UpdateFormatText());
        ForegroundProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.UpdateFormatText());
    }

    /// <summary>
    /// Rebuilds <see cref="FormattedText" /> after <see cref="Text" />, <see cref="FontFamily" />,
    /// <see cref="FontWeight" /> or <see cref="FontStyle" /> changed. The base implementation sets it to
    /// <see langword="null" /> while <see cref="Text" /> is <see langword="null" />, and otherwise creates a
    /// left-to-right, center-aligned text with the current typeface, size and brush for the current culture.
    /// </summary>
    /// <remarks>
    /// An override that does not call the base member must keep <see cref="FormattedText" /> current, because
    /// <see cref="Render" /> and <see cref="GetDesiredSize" /> read it.
    /// </remarks>
    protected virtual void RebuildFormatText()
    {
        if (Text == null)
        {
            FormattedText = null;
            return;
        }

        _textCulture = CultureInfo.CurrentCulture;
        FormattedText = new FormattedText(
            Text,
            _textCulture,
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

    /// <summary>
    /// Draws <see cref="FormattedText" /> with its top-left corner at the origin of the drawing context, or
    /// draws nothing while it is <see langword="null" />. The watermark renderer calls it for subclasses only
    /// and draws an exact <see cref="TextGlyph" /> from the text layout of <c>CreateTileTextLayout</c>.
    /// </summary>
    public override void Render(DrawingContext context)
    {
        if (FormattedText == null)
        {
            return;
        }

        context.DrawText(FormattedText, new Point());
    }

    /// <summary>
    /// Reports the size the glyph draws within: the layout size of <see cref="FormattedText" />, which excludes
    /// glyph overhang.
    /// </summary>
    /// <returns>
    /// The layout size, or an empty size while no text is formatted.
    /// </returns>
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

    internal TextLayout? CreateTileTextLayout()
    {
        if (Text is not { Length: > 0 } text || FormattedText == null)
        {
            return null;
        }

        var runProperties = new GenericTextRunProperties(new Typeface(FontFamily, FontStyle, FontWeight), FontSize,
            foregroundBrush: Foreground, cultureInfo: _textCulture);
        var paragraphProperties = new GenericTextParagraphProperties(runProperties, TextAlignment.Center,
            TextWrapping.WrapWithOverflow);
        return new TextLayout(new TileTextSource(text, runProperties), paragraphProperties);
    }

    // The layout size of FormattedText does not include glyph overhang. This method is used only for the exact
    // built-in TextGlyph; the Render of a custom subclass is not bound by this rect.
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

    private sealed class TileTextSource(string text, TextRunProperties properties) : ITextSource
    {
        public TextRun? GetTextRun(int textSourceIndex)
        {
            return textSourceIndex < text.Length ? new TextCharacters(text.AsMemory(textSourceIndex), properties) : null;
        }
    }
}
