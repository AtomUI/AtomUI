using AtomUI.Theme.Language;

namespace AtomUI.Desktop.Controls.UploadLang;

[LanguageProvider(LanguageCode.zh_CN, UploadToken.ID)]
internal class zh_CN : LanguageProvider
{
    public const string Uploading = "上传中...";
    public const string Pending = "等待调度...";
    public const string DragUploadHead = "点击或拖动文件到此区域进行上传";
}