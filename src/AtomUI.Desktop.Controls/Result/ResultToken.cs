using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own Tokens of the <see cref="Result" /> control: font sizes of the title and the subtitle, margins and padding
/// of its areas, and the size and colors of the status image.
/// </summary>
[ControlDesignToken]
public sealed partial class ResultToken : AbstractControlDesignToken
{

    /// <summary>
    /// Font size of the result title.
    /// </summary>
    public partial double HeaderFontSize { get; set; }

    /// <summary>
    /// Font size of the result subtitle.
    /// </summary>
    public partial double SubHeaderFontSize { get; set; }

    /// <summary>
    /// Size of the result status icon.
    /// </summary>
    public partial double StatusIconSize { get; set; }

    /// <summary>
    /// Margin around the extra content area.
    /// </summary>
    public partial Thickness ExtraMargin { get; set; }

    /// <summary>
    /// Width of the error-code illustration.
    /// </summary>
    public partial double ImageWidth { get; set; }
    /// <summary>
    /// Height of the error-code illustration.
    /// </summary>
    public partial double ImageHeight { get; set; }

    /// <summary>
    /// Color of the informational status icon.
    /// </summary>
    public partial Color ResultInfoIconColor { get; set; }
    /// <summary>
    /// Color of the success status icon.
    /// </summary>
    public partial Color ResultSuccessIconColor { get; set; }
    /// <summary>
    /// Color of the warning status icon.
    /// </summary>
    public partial Color ResultWarningIconColor { get; set; }
    /// <summary>
    /// Color of the error status icon.
    /// </summary>
    public partial Color ResultErrorIconColor { get; set; }
    /// <summary>
    /// Padding inside the result's outer frame.
    /// </summary>
    public partial Thickness FramePadding { get; set; }
    /// <summary>
    /// Padding inside the result's main content area.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }
    /// <summary>
    /// Margin around the result's main content area.
    /// </summary>
    public partial Thickness ContentMargin { get; set; }
    /// <summary>
    /// Margin around the result header.
    /// </summary>
    public partial Thickness HeaderMargin { get; set; }
    /// <summary>
    /// Margin around the status icon or illustration.
    /// </summary>
    public partial Thickness StatusImageMargin { get; set; }
    
}
