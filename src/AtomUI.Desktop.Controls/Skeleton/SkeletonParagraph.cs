using AtomUI.Controls.Commons;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The paragraph placeholder of a skeleton: a vertical stack of <see cref="SkeletonLine" /> rows that all take
/// the background of the paragraph.
/// </summary>
public class SkeletonParagraph : AbstractSkeletonParagraph
{
    /// <summary>
    /// Creates a <see cref="SkeletonLine" /> whose background is bound to the background of the paragraph.
    /// </summary>
    protected override Control CreateLine()
    {
        var line = new SkeletonLine();
        // The lines draw the paragraph's background (the theme's GradientFromColor unless the paragraph is restyled).
        line[!BackgroundProperty] = this[!BackgroundProperty];
        return line;
    }

    /// <summary>
    /// Sets <c>LineWidth</c> of the line to <paramref name="width" />, or clears it when <paramref name="width" /> is
    /// <see langword="null" />. A line that is not a <see cref="SkeletonLine" /> is left unchanged.
    /// </summary>
    protected override void ApplyLineWidth(Control line, Dimension? width)
    {
        if (line is not SkeletonLine skeletonLine)
        {
            return;
        }

        if (width.HasValue)
        {
            skeletonLine.LineWidth = width.Value;
        }
        else
        {
            skeletonLine.ClearValue(SkeletonLine.LineWidthProperty);
        }
    }

    /// <summary>
    /// Copies <c>IsActive</c> and <c>IsRound</c> of the paragraph to the line. A line that is not a
    /// <see cref="SkeletonLine" /> is left unchanged.
    /// </summary>
    protected override void ApplyLineState(Control line)
    {
        if (line is SkeletonLine skeletonLine)
        {
            skeletonLine.IsActive = IsActive;
            skeletonLine.IsRound  = IsRound;
        }
    }
}
