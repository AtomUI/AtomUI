using AtomUI.Theme;
using AtomUI.Theme.Resources;
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

    [Fact]
    public void Gallery_Registers_The_Five_Product_Themes_In_Display_Order()
    {
        var manager = Application.Current!.GetThemeManager().ShouldNotBeNull();

        manager.AvailableThemes.ShouldBe([
            new ThemeInfo("DaybreakBlue", "Daybreak Blue", ThemeAppearance.Light, true, Color.Parse("#1677FF")),
            new ThemeInfo("PolarGreen", "Polar Green", ThemeAppearance.Light, false, Color.Parse("#52C41A")),
            new ThemeInfo("SunsetOrange", "Sunset Orange", ThemeAppearance.Light, false, Color.Parse("#FA8C16")),
            new ThemeInfo("GoldenPurple", "Golden Purple", ThemeAppearance.Light, false, Color.Parse("#722ED1")),
            new ThemeInfo("Magenta", "Magenta", ThemeAppearance.Light, false, Color.Parse("#EB2F96"))
        ]);
    }

    [Theory]
    [InlineData("DaybreakBlue", "#1677FF")]
    [InlineData("PolarGreen", "#52C41A")]
    [InlineData("SunsetOrange", "#FA8C16")]
    [InlineData("GoldenPurple", "#722ED1")]
    [InlineData("Magenta", "#EB2F96")]
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
