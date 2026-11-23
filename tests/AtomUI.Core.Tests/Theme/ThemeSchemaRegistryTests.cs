using AtomUI.Generated.AtomUICore;
using AtomUI.Registration;
using AtomUI.Startup.Data;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeSchemaRegistryTests
{
    [Fact]
    public void Registry_Indexes_Descriptors_By_Ordinal_Identity_And_Dense_Slot()
    {
        var colorPrimary = Token("ColorPrimary", 0, TokenStage.Seed);
        var borderRadius = Token("BorderRadius", 1, TokenStage.Map);
        var contentFontSize = Token("ContentFontSize", 0, TokenStage.Control);
        var buttonIdentity = new ControlTokenIdentity("AtomUI", "Button");
        var button = new ControlTokenDescriptor(
            typeof(ButtonThemeTestControl),
            buttonIdentity,
            [contentFontSize],
            static () => throw new InvalidOperationException());
        var probe = TestDesignLanguages.Probe();

        var registry = new ThemeSchemaRegistry(
            [colorPrimary, borderRadius],
            [button],
            [probe],
            null,
            [ControlDerivationDescriptor.Create<ProbeLanguageToken, ThemeConfigTestSchema.TestControlToken,
                ThemeConfigTestSchema.TestControlDerivation>()]);

        registry.GlobalTokens.ShouldBe([colorPrimary, borderRadius]);
        registry.Controls.ShouldHaveSingleItem().Identity.ShouldBe(buttonIdentity);
        registry.Languages.ShouldBe([probe]);
        registry.TryGetGlobalToken("ColorPrimary", out var foundGlobal).ShouldBeTrue();
        foundGlobal.ShouldBeSameAs(colorPrimary);
        registry.TryGetControl(buttonIdentity, out var foundControl).ShouldBeTrue();
        foundControl.ShouldBeSameAs(registry.Controls[0]);
        foundControl.OwnTokens.ShouldBe([contentFontSize]);
        registry.TryGetLanguage("Probe", out var foundLanguage).ShouldBeTrue();
        foundLanguage.ShouldBeSameAs(probe);
        registry.TryGetLanguage("probe", out _).ShouldBeFalse();
        registry.TryGetGlobalToken("colorprimary", out _).ShouldBeFalse();
        registry.TryGetControl(new ControlTokenIdentity("atomui", "Button"), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("global-name")]
    [InlineData("global-slot")]
    [InlineData("control-identity")]
    [InlineData("language-id")]
    public void Registry_Rejects_Duplicate_Descriptors(string duplicate)
    {
        var global0 = Token("ColorPrimary", 0, TokenStage.Seed);
        var globals = duplicate switch
        {
            "global-name" => new[] { global0, Token("ColorPrimary", 1, TokenStage.Map) },
            "global-slot" => new[] { global0, Token("BorderRadius", 0, TokenStage.Map) },
            _             => new[] { global0 }
        };
        var button = Control("AtomUI", "Button");
        var controls = duplicate switch
        {
            "control-identity" => new[] { button, Control("AtomUI", "Button") },
            _                  => new[] { button }
        };
        var languages = duplicate == "language-id"
            ? new[] { TestDesignLanguages.Probe(), TestDesignLanguages.Probe() }
            : new[] { TestDesignLanguages.Probe() };

        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(globals, controls, languages));
    }

    [Fact]
    public void Registry_Indexes_Exact_Control_Type_And_Rejects_Duplicate_Type()
    {
        var button = Control("AtomUI", "Button");
        var input = Control("AtomUI", "Input");
        var registry = new ThemeSchemaRegistry(
            Array.Empty<TokenDescriptor>(),
            [button, input],
            Array.Empty<DesignLanguageDescriptor>());

        registry.TryGetControl(typeof(ButtonThemeTestControl), out var found).ShouldBeTrue();
        found!.Identity.ShouldBe(button.Identity);
        registry.TryGetControl(typeof(DerivedButtonThemeTestControl), out _).ShouldBeFalse();

        var duplicateType = new ControlTokenDescriptor(
            typeof(ButtonThemeTestControl),
            new ControlTokenIdentity("AtomUI", "Other"));
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            Array.Empty<TokenDescriptor>(),
            [button, duplicateType],
            Array.Empty<DesignLanguageDescriptor>()));
    }

    [Fact]
    public void Registry_Requires_Contiguous_Global_And_Control_Own_Slots()
    {
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            [Token("ColorPrimary", 1, TokenStage.Seed)],
            Array.Empty<ControlTokenDescriptor>(),
            Array.Empty<DesignLanguageDescriptor>()));

        var invalidControl = new ControlTokenDescriptor(
            typeof(ButtonThemeTestControl),
            new ControlTokenIdentity("AtomUI", "Button"),
            [Token("ContentFontSize", 1, TokenStage.Control)],
            static () => throw new InvalidOperationException());
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            [Token("ColorPrimary", 0, TokenStage.Seed)],
            [invalidControl],
            Array.Empty<DesignLanguageDescriptor>()));
    }

    [Fact]
    public void Registry_Allows_Unused_Global_Tokens_And_Rejects_Global_Own_Name_Collisions()
    {
        var global = Token("ColorPrimary", 0, TokenStage.Seed);
        var unusedGlobal = new ControlTokenDescriptor(
            typeof(ButtonThemeTestControl),
            new ControlTokenIdentity("AtomUI", "Button"));
        var collidingOwnToken = new ControlTokenDescriptor(
            typeof(ButtonThemeTestControl),
            new ControlTokenIdentity("AtomUI", "Button"),
            [Token("ColorPrimary", 0, TokenStage.Control)],
            static () => throw new InvalidOperationException());

        new ThemeSchemaRegistry(
            [global],
            [unusedGlobal],
            Array.Empty<DesignLanguageDescriptor>());
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            [global],
            [collidingOwnToken],
            Array.Empty<DesignLanguageDescriptor>()));
    }

    [Theory]
    [InlineData("", "Button")]
    [InlineData("AtomUI", "")]
    [InlineData("Atom UI", "Button")]
    [InlineData("AtomUI", "Button:Primary")]
    public void Control_Identity_Requires_Valid_Explicit_Identifiers(string catalog, string id)
    {
        Should.Throw<ArgumentException>(() => new ControlTokenIdentity(catalog, id));
    }

    [Fact]
    public void Registry_Revision_Is_Order_Independent_And_Changes_With_The_Language_Set()
    {
        var token = Token("ColorPrimary", 0, TokenStage.Seed);
        var control = Control("AtomUI", "Button");
        var probe = TestDesignLanguages.Probe();
        var child = new DesignLanguageDescriptor(new ChainLanguage<ChildProbeToken>("Child"), Array.Empty<TokenDescriptor>());
        var first = new ThemeSchemaRegistry([token], [control], [probe, child]);
        var reordered = new ThemeSchemaRegistry([token], [control], [child, probe]);
        var fewer = new ThemeSchemaRegistry([token], [control], [probe]);
        var none = new ThemeSchemaRegistry([token], [control], Array.Empty<DesignLanguageDescriptor>());

        reordered.Revision.ShouldBe(first.Revision);
        first.Languages.Select(static language => language.Id).ShouldBe(["Child", "Probe"]);
        fewer.Revision.ShouldNotBe(first.Revision);
        none.Revision.ShouldNotBe(fewer.Revision);
    }

    [Fact]
    public void Registry_Revision_Changes_With_The_Derivations()
    {
        var probe = TestDesignLanguages.Probe();
        var globals = GeneratedThemeSchema.GetGlobalTokens();
        var without = new ThemeSchemaRegistry(globals, Array.Empty<ControlTokenDescriptor>(), [probe], null, []);
        var with = new ThemeSchemaRegistry(globals, Array.Empty<ControlTokenDescriptor>(), [probe], null,
            [ControlDerivationDescriptor.Create<ProbeLanguageToken, ViewProbeToken, ViewProbeDerivation>()]);

        with.Revision.ShouldNotBe(without.Revision);
    }

    [Fact]
    public void Registry_Revision_Is_Deterministic_Without_Languages()
    {
        // No tokens, controls or languages: their CLR names and module versions would tie the pinned value to framework
        // and test assembly builds.
        var first = new ThemeSchemaRegistry(
            Array.Empty<TokenDescriptor>(),
            Array.Empty<ControlTokenDescriptor>(),
            Array.Empty<DesignLanguageDescriptor>());

        // Pins cross-process stability: the revision must never depend on string.GetHashCode().
        first.Revision.Value.ShouldBe(9354609568656401157UL);
    }

    [Fact]
    public void Registry_Revision_Changes_When_A_Package_Language_Is_Added()
    {
        var globals = GeneratedThemeSchema.GetGlobalTokens();
        DesignLanguageDescriptor[] none = [];
        DesignLanguageDescriptor[] withPackage = [TestDesignLanguages.Probe()];

        new ThemeSchemaRegistry(globals, Array.Empty<ControlTokenDescriptor>(), withPackage).Revision
            .ShouldNotBe(new ThemeSchemaRegistry(globals, Array.Empty<ControlTokenDescriptor>(), none).Revision);
        var index = StartupPackageIndex.Create([]);
        new ThemeSchemaRegistry(globals, index, withPackage, null).Revision
            .ShouldNotBe(new ThemeSchemaRegistry(globals, index, none, null).Revision);
        var stamped = StartupPackageIndex.Create([], new StartupImage(StartupImageFormat.Id, 0x1234UL, [], []));
        new ThemeSchemaRegistry(globals, stamped, withPackage, null).Revision
            .ShouldNotBe(new ThemeSchemaRegistry(globals, stamped, none, null).Revision);
        // A registry without languages keeps the compiled stamp.
        new ThemeSchemaRegistry(globals, stamped, none, null).Revision.ShouldBe(new ThemeSchemaRevision(0x1234UL));
    }

    [Fact]
    public void ExtendsGlobalSchemaOf_Accepts_Added_Languages_And_Rejects_Replaced_Ones()
    {
        var token = Token("ColorPrimary", 0, TokenStage.Seed);
        var probe = TestDesignLanguages.Probe();
        var child = new DesignLanguageDescriptor(new ChainLanguage<ChildProbeToken>("Child"), Array.Empty<TokenDescriptor>());
        var previous = new ThemeSchemaRegistry([token], Array.Empty<ControlTokenDescriptor>(), [probe]);

        var added = new ThemeSchemaRegistry([token], Array.Empty<ControlTokenDescriptor>(), [probe, child]);
        var replaced = new ThemeSchemaRegistry([token], Array.Empty<ControlTokenDescriptor>(), [TestDesignLanguages.Probe()]);
        var otherGlobals = new ThemeSchemaRegistry([Token("ColorPrimary", 0, TokenStage.Seed)],
            Array.Empty<ControlTokenDescriptor>(), [probe]);

        previous.ExtendsGlobalSchemaOf(previous).ShouldBeTrue();
        added.ExtendsGlobalSchemaOf(previous).ShouldBeTrue();
        previous.ExtendsGlobalSchemaOf(added).ShouldBeFalse();
        replaced.ExtendsGlobalSchemaOf(previous).ShouldBeFalse();
        otherGlobals.ExtendsGlobalSchemaOf(previous).ShouldBeFalse();
    }

    [Fact]
    public void Package_Append_With_A_New_Language_Reuses_The_Committed_Global_Snapshot()
    {
        var previousRegistry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(),
            Array.Empty<ControlTokenDescriptor>(), [TestDesignLanguages.AntDesign]);
        var previous = CompileDefault(previousRegistry);
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(),
            Array.Empty<ControlTokenDescriptor>(), [TestDesignLanguages.AntDesign, TestDesignLanguages.Probe()]);

        var result = new ThemeCompiler().CompilePackageAppend(new ThemeCompileInput(previous.Definition,
            previous.DefinitionRevision, previous.EffectiveConfig, registry, previous));

        result.Exception.ShouldBeNull();
        result.Success.ShouldBeTrue();
        result.Snapshot!.GlobalTokenValues.ShouldBeSameAs(previous.GlobalTokenValues);
        result.Snapshot.Registry.TryGetLanguage("Probe", out _).ShouldBeTrue();
    }

    [Fact]
    public void Package_Append_That_Replaces_A_Committed_Language_Descriptor_Is_Rejected()
    {
        var probe = TestDesignLanguages.Probe();
        var previousRegistry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(),
            Array.Empty<ControlTokenDescriptor>(), [probe]);
        var previous = CompileDefault(previousRegistry);
        var registry = new ThemeSchemaRegistry(GeneratedThemeSchema.GetGlobalTokens(),
            Array.Empty<ControlTokenDescriptor>(), [TestDesignLanguages.Probe()]);

        var result = new ThemeCompiler().CompilePackageAppend(new ThemeCompileInput(previous.Definition,
            previous.DefinitionRevision, previous.EffectiveConfig, registry, previous));

        result.Success.ShouldBeFalse();
        result.Exception.ShouldBeOfType<InvalidOperationException>()
              .Message.ShouldBe("Package append cannot change committed global theme inputs.");
    }

    private static ThemeSnapshot CompileDefault(ThemeSchemaRegistry registry)
    {
        var definition = ThemeTestSnapshotFactory.CreateLanguageDefinition(registry.Languages[0]);
        var config = ThemeCompiler.CreateDefinitionDefaults(definition, registry);
        var result = new ThemeCompiler().Compile(new ThemeCompileInput(definition,
            new ThemeDefinitionRevision("test", "1", "A1"), config, registry));
        result.Success.ShouldBeTrue();
        return result.Snapshot!;
    }

    private static TokenDescriptor Token(string name, int slot, TokenStage stage)
    {
        return new TokenDescriptor(
            name,
            slot,
            stage,
            typeof(string),
            name,
            static value => value,
            static value => ThemeTokenValueFormatter.Format((string)value!),
            static _ => throw new InvalidOperationException(),
            static (_, _) => throw new InvalidOperationException(),
            static _ => throw new InvalidOperationException());
    }

    private static ControlTokenDescriptor Control(string catalog, string id)
    {
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For(catalog, id),
            new ControlTokenIdentity(catalog, id));
    }
}
