using AtomUI.Theme.Language;

namespace AtomUI.Desktop.Controls.UploadLang;

[LanguageProvider(LanguageCode.en_US, UploadToken.ID)]
internal class en_US : LanguageProvider
{
    public const string Uploading = "Uploading...";
    public const string Pending = "Pending...";
    public const string DragUploadHead = "Click or drag file to this area to upload";
}