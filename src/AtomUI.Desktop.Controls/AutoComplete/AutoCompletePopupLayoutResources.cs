using AtomUI.Desktop.Controls.DesignTokens;

namespace AtomUI.Desktop.Controls;

internal static class AutoCompletePopupLayoutResources
{
    public static CandidatePopupLayoutTokenResourceKeys AutoComplete { get; } =
        new(AutoCompleteTokenKind.OptionHeight, AutoCompleteTokenKind.PopupContentPadding);

    public static CandidatePopupLayoutTokenResourceKeys SearchEdit { get; } =
        new(AutoCompleteSearchEditTokenKind.OptionHeight, AutoCompleteSearchEditTokenKind.PopupContentPadding);

    public static CandidatePopupLayoutTokenResourceKeys TextArea { get; } =
        new(AutoCompleteTextAreaTokenKind.OptionHeight, AutoCompleteTextAreaTokenKind.PopupContentPadding);
}
