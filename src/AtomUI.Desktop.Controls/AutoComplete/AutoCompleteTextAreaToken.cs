using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of <see cref="AutoCompleteTextArea" />. The class declares no token of its own: every value
/// is the shared one from <see cref="AbstractAutoCompleteToken" />, which styles the suggestion options and
/// the popup.
/// </summary>
[ControlDesignToken]
public sealed partial class AutoCompleteTextAreaToken : AbstractAutoCompleteToken
{
}
