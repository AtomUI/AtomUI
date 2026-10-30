using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 分隔符内容呈现器：把任意文本字形的墨迹中心平移到自身行盒中心。
/// 星号等字形的墨迹绘制在行盒上半区、偏 advance 左侧，连字符等则接近
/// 行盒中心，这由字体设计决定；本呈现器在显示时按当前字体的 glyph
/// metrics 实测墨迹盒并生成反向平移，任意字符、字体和字号都自动居中，
/// 不再需要逐字符人工校准。非文本内容（自定义 SeparatorTemplate）不做
/// 处理，居中由内容自行负责。
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

        // 多字符时沿 advance 逐字形平移并取墨迹并集。
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
