using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase.Localization;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Toolkits.GalleryBase.Tests.Localization;

public class GalleryBaseCatalogTests
{
    static GalleryBaseCatalogTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void GalleryLocalizedText_Resolves_From_Current_Localizer()
    {
        var application = Application.Current.ShouldNotBeNull();
        var languageManager = application.GetLanguageManager().ShouldNotBeNull();
        try
        {
            languageManager.ChangeLanguage(LanguageTags.ZhCN);
            var localizedText = new GalleryLocalizedText<GalleryShowCaseHeaderLangResourceKind>(
                GalleryShowCaseHeaderLangResourceKind.NamespaceLabel,
                "fallback");

            localizedText.Resolve().ShouldBe("命名空间");
        }
        finally
        {
            languageManager.ChangeLanguage(LanguageTags.EnUS);
        }
    }

}
