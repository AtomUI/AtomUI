using AtomUI.Theme.Language;

namespace AtomUI.Desktop.Controls.TransferLang;

[LanguageProvider(LanguageCode.zh_CN, TransferToken.ID)]
internal class zh_CN : LanguageProvider
{
    public const string Item = "项";
    public const string Items = "项";
    public const string SelectAll = "全选所有";
    public const string DeSelectAll = "取消全选";
    public const string RemoveAll = "删除所有";
    public const string ToggleSelectCurrentPage = "反选当页";
}