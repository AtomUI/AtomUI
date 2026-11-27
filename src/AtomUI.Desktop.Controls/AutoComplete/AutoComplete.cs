using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The single-line <see cref="AbstractAutoComplete" />: an input whose text box is a line edit and that lists
/// matching suggestions in a popup while the user types.
/// </summary>
public partial class AutoComplete : CompactSpaceAwareAutoComplete
{
    private AutoCompleteLineEditBox? _lineEditBox;

    /// <summary>
    /// Finds the line edit of the template after the base implementation has run, so that
    /// <see cref="GetBorderThicknessForCompactSpace" /> can read its border width.
    /// An override must call the base implementation, which binds the text box and the suggestion list parts.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _lineEditBox = e.NameScope.Find<AutoCompleteLineEditBox>(AutoCompleteThemeConstants.TextBoxPart);
    }

    /// <summary>
    /// Returns the border width of the line edit, or 0 when the template has no line edit that provides one.
    /// A compact <c>Space</c> reads this value from the control.
    /// </summary>
    protected override double GetBorderThicknessForCompactSpace()
    {
        if (_lineEditBox is ICompactSpaceAware compactSpaceAware)
        {
            return compactSpaceAware.GetBorderThickness();
        }
        return 0.0d;
    }
}