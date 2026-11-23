using AtomUI.Theme;
using AtomUI.Theme.Languages;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>
/// The selection resolver (spec 7.8): a pure function with three rules and no fallback search — the language, theme and
/// axis values must be available; the theme's fixed axes win; the theme's locked seeds are not replaced.
/// </summary>
public class ThemeSelectionResolverTests
{
    private static readonly DesignStyle Nova = new("Nova");
    private static readonly DesignStyle Vega = new("Vega");

    private static readonly DesignLanguageInfo Shadcn = new("Shadcn", [Vega, Nova], Vega,
        [DesignDensity.Default, DesignDensity.Compact], [DesignIdiom.Desktop], ["ColorPrimary", "BaseColor"]);

    private static readonly DesignLanguageInfo Ant = new("AntDesign", [new DesignStyle("Default")], new DesignStyle("Default"),
        [DesignDensity.Default, DesignDensity.Compact], [DesignIdiom.Desktop], ["ColorPrimary"]);

    private static readonly IReadOnlyList<ThemeInfo> Themes =
    [
        new("$Shadcn", "Shadcn", "Shadcn") { IsLanguageDefault = true },
        new("$AntDesign", "AntDesign", "AntDesign") { IsLanguageDefault = true },
        new("DaybreakBlue", "Daybreak Blue", "AntDesign"),
        new("Midnight", "Midnight", "Shadcn")
        {
            Style = Nova, Appearance = ThemeAppearance.Dark, Density = DesignDensity.Compact,
            LockedSeeds = ["ColorPrimary"]
        }
    ];

    [Fact]
    public void Without_A_Theme_The_Language_Default_Entry_Is_Selected()
    {
        var result = Resolve(ThemeSelection.Of("Shadcn", Nova).WithAppearance(ThemeAppearance.Dark)
                                           .WithDensity(DesignDensity.Compact));

        result.Request.ThemeId.ShouldBe("$Shadcn");
        var config = result.Request.Config.ShouldNotBeNull();
        config.Language.ShouldBe("Shadcn");
        config.Style.ShouldBe(Nova);
        config.Appearance.ShouldBe(ThemeAppearance.Dark);
        config.Density.ShouldBe(DesignDensity.Compact);
        result.IgnoredAxes.ShouldBeEmpty();
        result.IgnoredSeeds.ShouldBeEmpty();
        result.Request.Reason.ShouldBe(ThemeTransitionReason.UserRequest);
    }

    [Fact]
    public void A_Named_Theme_Of_The_Language_Is_Selected()
    {
        var result = Resolve(ThemeSelection.Of("AntDesign").WithTheme("DaybreakBlue"));

        result.Request.ThemeId.ShouldBe("DaybreakBlue");
        result.Request.Config.ShouldNotBeNull().Language.ShouldBe("AntDesign");
    }

    [Fact]
    public void A_Language_That_Is_Not_Enabled_Fails()
    {
        Should.Throw<ThemeSelectionException>(() => Resolve(ThemeSelection.Of("PrimeNG")))
              .Message.ShouldContain("'PrimeNG' is not enabled");
    }

    [Fact]
    public void A_Theme_Of_Another_Language_Fails()
    {
        Should.Throw<ThemeSelectionException>(() => Resolve(ThemeSelection.Of("AntDesign").WithTheme("Midnight")))
              .Message.ShouldContain("belongs to 'Shadcn'");
    }

    [Fact]
    public void A_Theme_That_Is_Not_In_The_Catalog_Fails()
    {
        Should.Throw<ThemeSelectionException>(() => Resolve(ThemeSelection.Of("AntDesign").WithTheme("Missing")))
              .Message.ShouldContain("'Missing' is not in the catalog");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void An_Axis_Value_The_Language_Does_Not_Declare_Fails(bool style)
    {
        var selection = style
            ? ThemeSelection.Of("AntDesign", Nova)
            : ThemeSelection.Of("AntDesign").WithDensity(new DesignDensity("Spacious"));

        Should.Throw<ThemeSelectionException>(() => Resolve(selection));
    }

    [Fact]
    public void The_Axes_A_Theme_Fixes_Win_And_Are_Reported()
    {
        var result = Resolve(ThemeSelection.Of("Shadcn", Vega).WithTheme("Midnight")
                                           .WithAppearance(ThemeAppearance.Light).WithDensity(DesignDensity.Default));

        result.IgnoredAxes.ShouldBe(["Style", "Appearance", "Density"]);
        var config = result.Request.Config.ShouldNotBeNull();
        config.Style.ShouldBeNull();
        config.Appearance.ShouldBeNull();
        config.Density.ShouldBeNull();
    }

    [Fact]
    public void A_Selection_That_Agrees_With_The_Fixed_Axes_Is_Not_Reported()
    {
        var result = Resolve(ThemeSelection.Of("Shadcn", Nova).WithTheme("Midnight")
                                           .WithAppearance(ThemeAppearance.Dark).WithDensity(DesignDensity.Compact));

        result.IgnoredAxes.ShouldBeEmpty();
    }

    [Fact]
    public void Locked_Seeds_Are_Not_Replaced_And_Are_Reported()
    {
        var result = Resolve(ThemeSelection.Of("Shadcn").WithTheme("Midnight")
                                           .WithSeed("ColorPrimary", "#FF0000")
                                           .WithLanguageSeed("BaseColor", "#737373"));

        result.IgnoredSeeds.ShouldBe(["ColorPrimary"]);
        var config = result.Request.Config.ShouldNotBeNull();
        config.Tokens.ShouldNotContainKey("ColorPrimary");
        config.LanguageTokens["BaseColor"].ShouldBe("#737373");
    }

    [Fact]
    public void Language_Seeds_Are_Written_As_Language_Tokens()
    {
        var result = Resolve(ThemeSelection.Of("Shadcn").WithLanguageSeed("Radius", "0.5rem"));

        result.Request.Config.ShouldNotBeNull().LanguageTokens["Radius"].ShouldBe("0.5rem");
    }

    [Theory]
    [InlineData(ThemeAppearance.Light)]
    [InlineData(ThemeAppearance.Dark)]
    public void Following_The_System_Uses_The_System_Appearance(ThemeAppearance system)
    {
        var result = ThemeSelectionResolver.Resolve([Shadcn, Ant], Themes,
            ThemeSelection.Of("AntDesign").WithAppearance(ThemeAppearance.Dark).FollowSystemAppearance(), system);

        result.Request.Config.ShouldNotBeNull().Appearance.ShouldBe(system);
    }

    [Fact]
    public void The_Resolver_Is_Pure()
    {
        var selection = ThemeSelection.Of("Shadcn", Nova).WithSeed("ColorPrimary", "#FF0000");

        var first = Resolve(selection);
        var second = Resolve(selection);

        second.Request.ThemeId.ShouldBe(first.Request.ThemeId);
        second.Request.Config.ShouldNotBeNull().Tokens.ShouldBe(first.Request.Config.ShouldNotBeNull().Tokens);
        selection.Seeds.Count.ShouldBe(1);
    }

    private static ThemeSelectionResult Resolve(ThemeSelection selection) =>
        ThemeSelectionResolver.Resolve([Shadcn, Ant], Themes, selection);
}
