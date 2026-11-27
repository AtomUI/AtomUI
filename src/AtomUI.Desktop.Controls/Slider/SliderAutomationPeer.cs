using Avalonia.Automation.Peers;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Exposes a <see cref="Slider" /> to UI Automation as a slider control.
/// </summary>
public class SliderAutomationPeer : RangeBaseAutomationPeer
{
    /// <summary>
    /// Creates the peer for a slider.
    /// </summary>
    /// <param name="owner">
    /// The slider to expose to UI Automation.
    /// </param>
    public SliderAutomationPeer(Slider owner) : base(owner)
    {
    }

    /// <summary>
    /// Returns <c>Slider</c> as the class name reported to UI Automation clients.
    /// </summary>
    protected override string GetClassNameCore()
    {
        return "Slider";
    }

    /// <summary>
    /// Reports the UI Automation control type <see cref="AutomationControlType.Slider" />.
    /// </summary>
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Slider;
    }
}