using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.VisualTree;
using Avalonia;
using AtomUI.Desktop.Controls.Localization;

namespace AtomUI.Desktop.Controls.Internal.DateViewer;

internal sealed class DateViewerCellAutomationPeer(DateViewerCell owner) : ControlAutomationPeer(owner), ISelectionItemProvider
{
    public bool IsSelected => owner.Model?.IsSelected == true;
    public ISelectionProvider? SelectionContainer => owner.GetVisualAncestors().OfType<DatePanel>().FirstOrDefault() is { } panel
        ? ControlAutomationPeer.CreatePeerForElement(panel) as ISelectionProvider : null;
    protected override string GetClassNameCore() => "DateViewerCell";
    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ListItem;
    protected override string? GetNameCore()
    {
        if (owner.Model is not { Value: { } value } model)
        {
            return string.Empty;
        }

        if (owner.Session is { AutomationNameFactory: { } factory } session)
        {
            return factory(session, model) ?? string.Empty;
        }

        var culture = owner.Session?.Input.Culture;
        return model.Kind switch
        {
            DateViewerCellType.Date => value.ToString("D", culture),
            DateViewerCellType.Month => value.ToString("Y", culture),
            DateViewerCellType.Quarter => $"{value.ToString("yyyy", culture)} {model.DisplayText}",
            DateViewerCellType.Year => value.ToString("yyyy", culture),
            DateViewerCellType.Week => $"{GetWeekLabel()} {model.DisplayText}",
            _ => string.Empty
        };
    }

    private static string GetWeekLabel() => Application.Current is { } application
        ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)?.Get(DateViewerLangResourceKind.Week) ?? "Week"
        : "Week";
    public void Select() => owner.Activate();
    public void AddToSelection() => owner.Activate();
    public void RemoveFromSelection() => throw new InvalidOperationException("Clear or change the selection through the date viewer owner.");
}
