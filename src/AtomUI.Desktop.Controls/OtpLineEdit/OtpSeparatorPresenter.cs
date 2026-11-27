using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Presenter of a separator. It moves the ink center of any text glyph onto the center of its own line box.
/// By font design, the ink of glyphs such as the asterisk lies in the upper half of the line box and to the
/// left of the advance, while glyphs such as the hyphen already lie near the center. At display time the
/// presenter measures the ink box from the glyph metrics of the current font and applies the opposite
/// translation, so every character, font and font size centers itself without manual calibration per
/// character. Non-text content from a custom SeparatorTemplate is not adjusted and must center itself.
/// </summary>
internal sealed class OtpSeparatorPresenter : ContentControl
{
    public double ComputedInkOffsetX { get; private set; }

    public double ComputedInkOffsetY { get; private set; }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty ||
            change.Property == FontSizeProperty ||
            change.Property == FontWeightProperty ||
            change.Property == FontStyleProperty ||
            change.Property == FontFamilyProperty ||
            change.Property == FontStretchProperty)
        {
            UpdateInkTransform();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateInkTransform();
    }

    private void UpdateInkTransform()
    {
        ComputedInkOffsetX = 0;
        ComputedInkOffsetY = 0;
        RenderTransform    = null;

        if (Content is not string text || string.IsNullOrEmpty(text))
        {
            return;
        }

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        if (!FontManager.Current.TryGetGlyphTypeface(typeface, out var glyphTypeface))
        {
            return;
        }

        var designEmHeight = glyphTypeface.Metrics.DesignEmHeight;
        if (designEmHeight == 0)
        {
            return;
        }

        var memory = text.AsMemory();
        using var shaped = TextShaper.Current.ShapeText(
            memory,
            new TextShaperOptions(glyphTypeface, FontSize, 0, CultureInfo.CurrentCulture));
        if (shaped.Length == 0)
        {
            return;
        }

        using var glyphRun = new GlyphRun(glyphTypeface, FontSize, memory, shaped, null, 0);

        // With several characters, shift each glyph along its advance and take the union of the ink boxes.
        var penX       = 0d;
        var inkLeft    = double.MaxValue;
        var inkRight   = double.MinValue;
        var inkTop     = double.MinValue;
        var inkBottom  = double.MaxValue;
        var scale      = FontSize / designEmHeight;
        var hasInk     = false;

        for (var i = 0; i < shaped.Length; i++)
        {
            var glyph = shaped[i];
            if (!glyphTypeface.TryGetGlyphMetrics(glyph.GlyphIndex, out var metrics) ||
                metrics.Width * scale <= 0 ||
                metrics.Height * scale <= 0)
            {
                penX += glyph.GlyphAdvance;
                continue;
            }

            var left  = penX + metrics.XBearing * scale;
            var right = left + metrics.Width * scale;
            var top   = metrics.YBearing * scale;
            var bottom = top - metrics.Height * scale;

            inkLeft   = Math.Min(inkLeft, left);
            inkRight  = Math.Max(inkRight, right);
            inkTop    = Math.Max(inkTop, top);
            inkBottom = Math.Min(inkBottom, bottom);
            hasInk    = true;
            penX      += glyph.GlyphAdvance;
        }

        if (!hasInk)
        {
            return;
        }

        var advance = penX;
        if (advance <= 0)
        {
            return;
        }

        var baseline = glyphRun.Metrics.Baseline;
        var lineBox  = glyphRun.Metrics.Height;
        if (baseline <= 0 || lineBox <= 0)
        {
            return;
        }

        var inkCenterX            = (inkLeft + inkRight) / 2;
        var offsetX               = advance / 2 - inkCenterX;
        var inkCenterAboveBaseline = (inkTop + inkBottom) / 2;
        var boxCenterAboveBaseline = baseline - lineBox / 2;
        var offsetY                = inkCenterAboveBaseline - boxCenterAboveBaseline;

        if (offsetX == 0 && offsetY == 0)
        {
            return;
        }

        ComputedInkOffsetX = offsetX;
        ComputedInkOffsetY = offsetY;
        RenderTransform    = new TranslateTransform(offsetX, offsetY);
    }
}
