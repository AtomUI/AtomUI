using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls.DesignTokens;

[ControlDesignToken]
public abstract partial class AbstractSkeletonToken : AbstractControlDesignToken
{
    /// <summary>Start color of the loading gradient. Pair with <see cref="GradientToColor"/>.</summary>
    public partial Color GradientFromColor { get; set; }

    /// <summary>End color of the loading gradient. Pair with <see cref="GradientFromColor"/>.</summary>
    public partial Color GradientToColor { get; set; }

    /// <summary>Duration of one cycle of the animated skeleton highlight.</summary>
    public partial TimeSpan LoadingMotionDuration { get; set; }

    /// <summary>Corner radius of the skeleton placeholder shapes.</summary>
    public partial CornerRadius BlockRadius { get; set; }

    /// <summary>Height of the title placeholder bar.</summary>
    public partial double TitleHeight { get; set; }

    /// <summary>Height of each paragraph placeholder bar.</summary>
    public partial double ParagraphLineHeight { get; set; }
}
