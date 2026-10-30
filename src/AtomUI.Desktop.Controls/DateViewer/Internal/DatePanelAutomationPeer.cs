using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

internal sealed class DatePanelAutomationPeer(DatePanel owner) : ControlAutomationPeer(owner), ISelectionProvider
{
    public bool CanSelectMultiple => owner.Session?.Input.IsRangeSelection == true;
    public bool IsSelectionRequired => owner.Session?.Input.IsSelectionRequired == true;
    protected override string GetClassNameCore() => "DatePanel";
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Table;
    public IReadOnlyList<AutomationPeer> GetSelection() => owner.GetRealizedCells()
        .Where(cell => cell.Model?.IsSelected == true)
        .Select(ControlAutomationPeer.CreatePeerForElement).OfType<AutomationPeer>().ToArray();
}
