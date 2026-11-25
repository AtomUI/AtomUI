using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;

namespace AtomUI.Desktop.Controls;

internal static class SelectPopupLayoutResources
{
    public static CandidatePopupLayoutTokenResourceKeys Select { get; } =
        new(SelectTokenKind.OptionHeight, SelectTokenKind.PopupContentPadding);

    public static CandidatePopupLayoutTokenResourceKeys TreeSelect { get; } =
        new(ControlTokenResourceKey.Global(TreeSelectTokens.Identity, SharedTokenKind.ControlHeight),
            TreeSelectTokenKind.PopupContentPadding);

    public static CandidatePopupLayoutTokenResourceKeys Cascader { get; } =
        new(ControlTokenResourceKey.Global(CascaderTokens.Identity, SharedTokenKind.ControlHeight),
            DefaultMaxHeight: CascaderTokenKind.DropdownHeight);
}
