using Avalonia;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Lets a control adjust the custom placement of a popup. The popup calls it after it has applied its anchor,
/// gravity, offset and flip handling and has notified the flip result. It hands the same instance to the
/// positioner afterwards, so a change made to <paramref name="parameters" /> takes effect.
/// </summary>
/// <param name="parameters">
/// The placement to adjust. Its anchor, gravity and offset are already set for the requested placement.
/// </param>
/// <param name="shadowThickness">
/// The shadow thickness of the popup in the current hosting mode.
/// </param>
/// <param name="marginToAnchor">
/// The gap between the popup and its placement target.
/// </param>
/// <param name="isUseOverlayHost">
/// <see langword="true" /> when the popup is hosted in an overlay of its top level,
/// <see langword="false" /> when it is hosted in a separate popup root.
/// </param>
/// <param name="isHorizontalFlipped">
/// <see langword="true" /> when the popup is shown on the other side of the target along the X axis.
/// </param>
/// <param name="isVerticalFlipped">
/// <see langword="true" /> when the popup is shown on the other side of the target along the Y axis.
/// </param>
public delegate void CustomPlacementCallback(CustomPopupPlacement parameters,
                                             Thickness shadowThickness,
                                             double marginToAnchor,
                                             bool isUseOverlayHost, 
                                             bool isHorizontalFlipped, 
                                             bool isVerticalFlipped);