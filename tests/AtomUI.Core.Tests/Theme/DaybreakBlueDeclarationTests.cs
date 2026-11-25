using AtomUI.DesignLanguages.AntDesign;
using Avalonia.Media;
using AtomUI.Theme;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class DaybreakBlueDeclarationTests
{
    [Fact]
    public void DaybreakBlue_Belongs_To_AntDesign_And_Fixes_No_Axis()
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType()).AddAntDesign();
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);
            var info = manager.Themes.Single(static theme => theme.Id == AntDesignThemes.DaybreakBlue);
            // The official theme follows every variant of its language and leaves the brand color adjustable.
            info.Language.ShouldBe("AntDesign");
            info.Style.ShouldBeNull();
            info.Appearance.ShouldBeNull();
            info.Density.ShouldBeNull();
            info.LockedSeeds.ShouldBeEmpty();
            info.AccentColor.ShouldBe(Color.Parse("#1677FF"));
        });
    }
}
