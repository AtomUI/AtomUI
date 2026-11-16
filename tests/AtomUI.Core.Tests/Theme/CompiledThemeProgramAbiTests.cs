using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Definitions.Compiled;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class CompiledThemeProgramAbiTests
{
    [Fact]
    public void Abi1_Program_Loads_With_Default_Metadata()
    {
        var program = new Abi1Program(1);

        program.PublishedInfo.DesignLanguage.ShouldBeNull();
        program.PublishedInfo.Density.ShouldBe(ThemeDensity.Normal);
        program.PublishedInfo.BrandColor.ShouldBe(ThemeBrandColorSupport.Fixed);
        program.PublishedInfo.Algorithms.ShouldBeEmpty();
    }

    [Fact]
    public void Abi2_Program_Publishes_Metadata_And_Algorithms()
    {
        var program = new Abi2Program(2, ["PrimeNG:Aura", "AtomUI:Compact"]);

        var info = program.PublishedInfo;
        info.DesignLanguage.ShouldBe("PrimeNG.Aura");
        info.Density.ShouldBe(ThemeDensity.Compact);
        info.BrandColor.ShouldBe(ThemeBrandColorSupport.Seed);
        info.Algorithms.ShouldBe([new ThemeAlgorithm("PrimeNG", "Aura"), new ThemeAlgorithm("AtomUI", "Compact")]);
    }

    [Fact]
    public void Abi3_Program_Publishes_NeutralColor_And_NeutralAccentColor()
    {
        var program = new Abi3Program(3, ThemeNeutralColorSupport.Seed, Color.Parse("#71717B"));

        program.PublishedInfo.NeutralColor.ShouldBe(ThemeNeutralColorSupport.Seed);
        program.PublishedInfo.NeutralAccentColor.ShouldBe(Color.Parse("#71717B"));
    }

    [Fact]
    public void Abi2_Program_Keeps_Loading_With_Neutral_Defaults()
    {
        var program = new Abi2Program(2, ["PrimeNG:Aura"]);

        program.PublishedInfo.NeutralColor.ShouldBe(ThemeNeutralColorSupport.Fixed);
        program.PublishedInfo.NeutralAccentColor.ShouldBeNull();
    }

    [Fact]
    public void Abi2_Program_Rejects_Null_Algorithms()
    {
        Should.Throw<ArgumentNullException>(() => new Abi2Program(2, null!));
        Should.Throw<ArgumentNullException>(() => new Abi2Program(2, ["PrimeNG:Aura", null!]));
    }

    [Fact]
    public void Published_Algorithms_Cannot_Be_Mutated_Through_A_Cast()
    {
        var program = new Abi2Program(2, ["PrimeNG:Aura"]);

        program.PublishedInfo.Algorithms.ShouldNotBeAssignableTo<ThemeAlgorithm[]>();
        ((IList<ThemeAlgorithm>)program.PublishedInfo.Algorithms).IsReadOnly.ShouldBeTrue();
    }

    [Fact]
    public void ThemeInfo_Algorithms_Copies_And_Rejects_Null()
    {
        ThemeAlgorithm[] source = [ThemeAlgorithm.Default];
        var info = new ThemeInfo("T", "T", ThemeAppearance.Light, false) { Algorithms = source };
        source[0] = ThemeAlgorithm.Dark;

        info.Algorithms.ShouldBe([ThemeAlgorithm.Default]);
        Should.Throw<ArgumentNullException>(() => new ThemeInfo("T", "T", ThemeAppearance.Light, false) { Algorithms = null! });
    }

    [Fact]
    public void Abi_Mismatch_Throws()
    {
        Should.Throw<NotSupportedException>(() => new Abi1Program(2));
        Should.Throw<NotSupportedException>(() => new Abi2Program(1, []));
        Should.Throw<NotSupportedException>(() => new Abi3Program(2, ThemeNeutralColorSupport.Fixed, null));
    }

    private sealed class Abi1Program : CompiledThemeProgram
    {
        public Abi1Program(int abi) : base(abi, "Themes/T.xml", "rev", "digest", "program", "T", "T", ThemeAppearance.Light, false, null) { }
        public ThemeInfo PublishedInfo => Info;
        protected override void Describe(CompiledThemeBindingBuilder builder) { }
        protected override void WriteStaticDefaults(CompiledThemeValueWriter writer) { }
    }

    private sealed class Abi2Program : CompiledThemeProgram
    {
        public Abi2Program(int abi, string[] algorithms) : base(abi, "Themes/T.xml", "rev", "digest", "program", "T", "T",
            ThemeAppearance.Light, false, null, "PrimeNG.Aura", ThemeDensity.Compact, ThemeBrandColorSupport.Seed, algorithms) { }
        public ThemeInfo PublishedInfo => Info;
        protected override void Describe(CompiledThemeBindingBuilder builder) { }
        protected override void WriteStaticDefaults(CompiledThemeValueWriter writer) { }
    }

    private sealed class Abi3Program : CompiledThemeProgram
    {
        public Abi3Program(int abi, ThemeNeutralColorSupport neutralColor, Color? neutralAccent) : base(abi, "Themes/T.xml", "rev", "digest", "program", "T", "T",
            ThemeAppearance.Light, false, null, "PrimeNG.Aura", ThemeDensity.Normal, ThemeBrandColorSupport.Seed, neutralColor, neutralAccent, []) { }
        public ThemeInfo PublishedInfo => Info;
        protected override void Describe(CompiledThemeBindingBuilder builder) { }
        protected override void WriteStaticDefaults(CompiledThemeValueWriter writer) { }
    }
}
