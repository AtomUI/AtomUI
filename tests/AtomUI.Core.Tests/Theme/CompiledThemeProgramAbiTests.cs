using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Definitions.Compiled;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>The compiled theme program ABI 4 (spec 7.12): language, fixed axes, locked seeds and accent color.</summary>
public class CompiledThemeProgramAbiTests
{
    [Fact]
    public void Abi4_Program_Publishes_The_Language_Fixed_Axes_Locked_Seeds_And_Accent()
    {
        var program = new Abi4Program(4, "Default", 1, "Compact", ["ColorPrimary"], Color.Parse("#722ED1"));

        program.Language.ShouldBe("AntDesign");
        program.Style.ShouldBe(new DesignStyle("Default"));
        program.Appearance.ShouldBe(ThemeAppearance.Dark);
        program.Density.ShouldBe(DesignDensity.Compact);
        program.LockedSeeds.ShouldBe(["ColorPrimary"]);
        program.AccentColor.ShouldBe(Color.Parse("#722ED1"));
    }

    [Fact]
    public void Abi4_Program_Leaves_The_Axes_It_Does_Not_Fix_Open()
    {
        var program = new Abi4Program(4, null, -1, null, [], null);

        program.Style.ShouldBeNull();
        program.Appearance.ShouldBeNull();
        program.Density.ShouldBeNull();
        program.LockedSeeds.ShouldBeEmpty();
        program.AccentColor.ShouldBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void Another_Abi_Is_Rejected_With_A_Rebuild_Hint(int abi)
    {
        Should.Throw<NotSupportedException>(() => new Abi4Program(abi, null, -1, null, [], null))
              .Message.ShouldContain("rebuild the package");
    }

    [Fact]
    public void Invalid_Program_Metadata_Is_Rejected()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Abi4Program(4, null, 2, null, [], null));
        Should.Throw<ArgumentNullException>(() => new Abi4Program(4, null, -1, null, null!, null));
    }

    [Fact]
    public void Bind_Checks_The_Program_Against_The_Enabled_Language()
    {
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(), [], [TestDesignLanguages.AntDesign]);

        var definition = new Abi4Program(4, "Default", 0, "Compact", ["ColorPrimary"], null).Bind(registry);
        definition.Language.ShouldBeSameAs(TestDesignLanguages.AntDesign);
        definition.Appearance.ShouldBe(ThemeAppearance.Light);
        definition.LockedSeeds.ShouldBe(["ColorPrimary"]);

        Should.Throw<ThemeSchemaException>(() => new Abi4Program(4, "Nova", -1, null, [], null).Bind(registry))
              .Message.ShouldContain("style 'Nova'");
        Should.Throw<ThemeSchemaException>(() => new Abi4Program(4, null, -1, "Spacious", [], null).Bind(registry))
              .Message.ShouldContain("density 'Spacious'");
        Should.Throw<ThemeSchemaException>(() => new Abi4Program(4, null, -1, null, ["BorderRadius"], null).Bind(registry))
              .Message.ShouldContain("not an adjustable seed");
        Should.Throw<ThemeSchemaException>(() => new Abi4Program(4, null, -1, null, [], null, "Shadcn").Bind(registry))
              .Message.ShouldContain("'Shadcn', which is not enabled");
    }

    [Fact]
    public void ThemeInfo_LockedSeeds_Are_Copied_Ordered_And_Reject_Null()
    {
        string[] source = ["ColorPrimary", "BaseColor"];
        var info = new ThemeInfo("T", "T", "Shadcn") { LockedSeeds = source };
        source[0] = "Changed";

        info.LockedSeeds.ShouldBe(["BaseColor", "ColorPrimary"]);
        Should.Throw<ArgumentNullException>(() => new ThemeInfo("T", "T", "Shadcn") { LockedSeeds = null! });
    }

    private sealed class Abi4Program(
        int abi, string? style, int appearance, string? density, string[] lockedSeeds, Color? accent,
        string language = "AntDesign")
        : CompiledThemeProgram(abi, "Themes/T.xml", "rev", "digest", "program", "T", "T", language, style, appearance,
            density, lockedSeeds, accent)
    {
        protected override void Describe(CompiledThemeBindingBuilder builder) => builder.BeginSection(0, null, -1, null, null);
        protected override void WriteStaticDefaults(CompiledThemeValueWriter writer) { }
    }
}
