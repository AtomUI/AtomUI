using Avalonia.Automation.Peers;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Exposes a <see cref="SliderThumb" /> to UI Automation as a thumb that is not a content element.
/// </summary>
public class SliderThumbAutomationPeer : ControlAutomationPeer
{
    /// <summary>
    /// Creates the peer for a slider thumb.
    /// </summary>
    /// <param name="owner">
    /// The thumb to expose to UI Automation.
    /// </param>
    public SliderThumbAutomationPeer(SliderThumb owner) : base(owner)
    {
    }

    /// <summary>
    /// Reports the UI Automation control type <see cref="AutomationControlType.Thumb" />.
    /// </summary>
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Thumb;
    }

    /// <summary>
    /// Returns <see langword="false" />, so the thumb is not reported as a content element.
    /// </summary>
    protected override bool IsContentElementCore()
    {
        return false;
    }
}