using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractColorPickerToken : AbstractControlDesignToken
{
    /// <summary>Opacity of the trigger color swatch when the picker is disabled.</summary>
    public partial double ColorBlockDisabledOpacity { get; set; }

    /// <summary>Inset outline that separates the trigger swatch from its surrounding surface.</summary>
    public partial BoxShadows ColorBlockInnerShadows { get; set; }

    /// <summary>Size of a tile in the checkerboard behind a translucent trigger swatch.</summary>
    public partial double TransparentBgSize { get; set; }

    /// <summary>Space between the trigger border and its swatch and optional label.</summary>
    public partial Thickness TriggerPadding { get; set; }

    /// <summary>Space around the optional label inside the trigger.</summary>
    public partial Thickness TriggerTextMargin { get; set; }
}
