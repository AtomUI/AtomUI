using System.Globalization;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

public class TextGlyph : WatermarkGlyph
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty
        .Register<TextGlyph, string?>(nameof(Text));

    public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty
        .Register<TextGlyph, double>(nameof(FontSize), 16);

    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty
        .Register<TextGlyph, IBrush>(nameof(Foreground), Brushes.Black);

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    #endregion

    protected FormattedText? FormattedText { get; private set; }

    static TextGlyph()
    {
        TextProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        // SetFontSize changes run metrics but retains the paragraph's original
        // line-height floor. Rebuild when em size changes so shrinking also restores height.
        FontSizeProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        ForegroundProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.UpdateForeground());
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
            FlowDirection.RightToLeft,
            Typeface.Default,
            FontSize,
            Foreground)
        {
            TextAlignment = TextAlignment.Center
        };
    }

    private void UpdateForeground()
    {
        if (FormattedText == null)
        {
            return;
        }

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
