using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Definitions.Compiled;
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
    public void Abi_Mismatch_Throws()
    {
        Should.Throw<NotSupportedException>(() => new Abi1Program(2));
        Should.Throw<NotSupportedException>(() => new Abi2Program(1, []));
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
}
