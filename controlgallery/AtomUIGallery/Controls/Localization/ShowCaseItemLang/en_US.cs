using AtomUI.Theme.Language;
using AtomUIGallery.Controls;

namespace AtomUIGallery.Controls.Localization.ShowCaseItemLang;

[LanguageProvider(LanguageCode.en_US, ShowCaseItem.LanguageId, Constants.LanguageCatalog)]
internal class en_US : LanguageProvider
{
    public const string ToolTipShowHideCode = "Show/Hide Code";
    public const string ToolTipCopyCode = "Copy Code";
    public const string ToolTipOpenInNewWindow = "Open in New Window";
    public const string ToolTipOpenOnGitHub = "Open on GitHub";
    public const string CopySuccessMessage = "Code copied";
}
