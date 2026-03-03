using AtomUI.Theme.Language;
using AtomUIGallery.Controls;

namespace AtomUIGallery.Controls.Localization.ShowCaseItemLang;

[LanguageProvider(LanguageCode.zh_CN, ShowCaseItem.LanguageId, Constants.LanguageCatalog)]
internal class zh_CN : LanguageProvider
{
    public const string ToolTipShowHideCode = "显示/隐藏代码";
    public const string ToolTipCopyCode = "复制代码";
    public const string ToolTipOpenInNewWindow = "在新窗口打开";
    public const string ToolTipOpenOnGitHub = "在 GitHub 上打开";
    public const string CopySuccessMessage = "代码已复制";
}
