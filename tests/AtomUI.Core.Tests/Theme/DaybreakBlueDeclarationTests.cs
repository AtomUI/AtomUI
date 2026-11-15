using AtomUI.Theme;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class DaybreakBlueDeclarationTests
{
    [Fact]
    public void DaybreakBlue_Declares_AntDesign_Seed_Normal_Light()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            var info = manager.AvailableThemes.Single(static theme => theme.Id == IThemeManager.DEFAULT_THEME_ID);
            info.DesignLanguage.ShouldBe("AntDesign");
            info.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
            info.Density.ShouldBe(ThemeDensity.Normal);
            info.Appearance.ShouldBe(ThemeAppearance.Light);
        });
    }
}
