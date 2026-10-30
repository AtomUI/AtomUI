using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal class DescriptionBorderedItemLabel : DescriptionBorderedCell
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find("ContentPresenter") is ContentPresenter presenter)
        {
            presenter.Classes.Add(DescriptionsSemanticParts.LabelClass);
        }
    }
}
