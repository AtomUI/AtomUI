using AtomUI.Theme.Algorithms;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUIGallery.Workspace;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Workspace;

public class GalleryBuiltInThemeTests
{
    static GalleryBuiltInThemeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData("PolarGreen")]
    [InlineData("SunsetOrange")]
    [InlineData("GoldenPurple")]
    [InlineData("Magenta")]
    public void Gallery_Catalog_No_Longer_Contains_The_Removed_Color_Presets(string themeId)
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();

        manager.AvailableThemes.Select(theme => theme.Id).ShouldNotContain(themeId);
    }

    [Fact]
    public void Gallery_Catalog_Keeps_DaybreakBlue_As_The_Seed_Based_Ant_Design_Language()
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();

        var theme = manager.AvailableThemes.Single(info => info.Id == "DaybreakBlue");
        theme.DesignLanguage.ShouldBe("AntDesign");
        theme.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
        theme.AccentColor.ShouldBe(Color.Parse("#1677FF"));
    }

    [Fact]
    public void Ant_Named_Swatches_Match_The_Removed_Presets()
    {
        GalleryBrandSwatches.AntNamed.Select(swatch => (swatch.Name, swatch.Color)).ShouldBe([
            ("Daybreak Blue", Color.Parse("#1677FF")),
            ("Polar Green", Color.Parse("#52C41A")),
            ("Sunset Orange", Color.Parse("#FA8C16")),
            ("Golden Purple", Color.Parse("#722ED1")),
            ("Magenta", Color.Parse("#EB2F96"))
        ]);
    }

    [Fact]
    public void Brand_Swatches_Show_Named_Ant_Colors_Only_For_Ant_Design()
    {
        var ant   = GalleryBrandSwatches.For("AntDesign", "Daybreak Blue", Color.Parse("#1677FF"));
        var other = GalleryBrandSwatches.For("PrimeNG.Aura", "PrimeNG Aura", Color.Parse("#10B981"));

        ant.Select(swatch => swatch.Name).ShouldContain("Polar Green");
        ant.Count(swatch => swatch.Color == Color.Parse("#1677FF")).ShouldBe(1);
        other.Select(swatch => swatch.Name).ShouldNotContain("Polar Green");
        GalleryBrandSwatches.Tailwind500.Count.ShouldBe(17);
        other.Count(swatch => swatch.Color == Color.Parse("#10B981")).ShouldBe(1);
    }

    [Theory]
    [InlineData("DaybreakBlue", "#1677FF")]
    public async Task Gallery_Theme_Id_Compiles_The_Expected_Brand_Colors(
        string themeId,
        string color)
    {
        var application = Application.Current.ShouldNotBeNull();
        var manager = application.GetThemeManager().ShouldNotBeNull();
        try
        {
            var result = await manager.ApplyThemeAsync(new ThemeRequest(
                themeId,
                null,
                ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);

            result.Status.ShouldBeOneOf(ThemeTransitionStatus.Committed, ThemeTransitionStatus.NoOp);
            manager.CurrentTheme!.ThemeId.ShouldBe(themeId);
            FindColorResource(application, SharedTokenKind.ColorPrimary).ShouldBe(Color.Parse(color));
            FindColorResource(application, SharedTokenKind.ColorLink).ShouldBe(Color.Parse(color));
            FindColorResource(application, SharedTokenKind.ColorInfo).ShouldBe(Color.Parse("#1677FF"));
        }
        finally
        {
            await manager.ApplyThemeAsync(new ThemeRequest(
                IThemeManager.DEFAULT_THEME_ID,
                null,
                ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);
        }
    }

    private static Color FindColorResource(Application application, SharedTokenKind kind)
    {
        application.TryFindResource(kind, out var resource).ShouldBeTrue();
        return resource switch
        {
            Color value => value,
            ISolidColorBrush brush => brush.Color,
            _ => throw new InvalidOperationException(
                $"{kind} resolved to unsupported type '{resource?.GetType().FullName ?? "null"}'.")
        };
    }
}
