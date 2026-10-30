using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public partial class AutoComplete : CompactSpaceAwareAutoComplete
{
    private AutoCompleteLineEditBox? _lineEditBox;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _lineEditBox = e.NameScope.Find<AutoCompleteLineEditBox>(AutoCompleteThemeConstants.TextBoxPart);
    }

    protected override double GetBorderThicknessForCompactSpace()
    {
        if (_lineEditBox is ICompactSpaceAware compactSpaceAware)
        {
            return compactSpaceAware.GetBorderThickness();
        }
        return 0.0d;
    }
}