using System.Collections.Generic;
using AtomUI.Theme;
using AtomUI.Theme.Language;

namespace AtomUI.Theme.Language
{
    internal class LanguageProviderPool
    {
        internal static IList<LanguageProvider> GetLanguageProviders()
        {
            List<LanguageProvider> languageProviders = new List<LanguageProvider>();
            languageProviders.Add(new AtomUI.Desktop.Controls.DatePickerLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.DatePickerLang.zh_CN());
            languageProviders.Add(new AtomUI.Desktop.Controls.DialogLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.DialogLang.zh_CN());
            languageProviders.Add(new AtomUI.Desktop.Controls.ImagePreviewerLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.ImagePreviewerLang.zh_CN());
            languageProviders.Add(new AtomUI.Desktop.Controls.PaginationLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.PaginationLang.zh_CN());
            languageProviders.Add(new AtomUI.Desktop.Controls.PopupConfirmLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.PopupConfirmLang.zh_CN());
            languageProviders.Add(new AtomUI.Desktop.Controls.QRCodeLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.QRCodeLang.zh_CN());
            languageProviders.Add(new AtomUI.Desktop.Controls.TimePickerLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.TimePickerLang.zh_CN());
            languageProviders.Add(new AtomUI.Desktop.Controls.UploadLang.en_US());
            languageProviders.Add(new AtomUI.Desktop.Controls.UploadLang.zh_CN());
            return languageProviders;
        }
    }
}