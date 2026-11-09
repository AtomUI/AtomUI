using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class SkeletonParagraph : AbstractSkeletonParagraph
{
    protected override Control CreateLine()
    {
        var line = new SkeletonLine();
        // The lines draw the paragraph's background (the theme's GradientFromColor unless the paragraph is restyled).
        line[!BackgroundProperty] = this[!BackgroundProperty];
        return line;
    }

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

    protected override void ApplyLineState(Control line)
    {
        if (line is SkeletonLine skeletonLine)
        {
            skeletonLine.IsActive = IsActive;
            skeletonLine.IsRound  = IsRound;
        }
    }
}
