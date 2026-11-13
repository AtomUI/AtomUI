using AtomUI.Theme.Algorithms;
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
            static () => throw new InvalidOperationException(),
            static (_, _) => throw new InvalidOperationException());
        var defaultAlgorithm = Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Preserve);

        var registry = new ThemeSchemaRegistry(
            [colorPrimary, borderRadius],
            [button],
            [defaultAlgorithm]);

        registry.GlobalTokens.ShouldBe([colorPrimary, borderRadius]);
        registry.Controls.ShouldHaveSingleItem().Identity.ShouldBe(buttonIdentity);
        registry.Algorithms.ShouldBe([defaultAlgorithm]);
        registry.TryGetGlobalToken("ColorPrimary", out var foundGlobal).ShouldBeTrue();
        foundGlobal.ShouldBeSameAs(colorPrimary);
        registry.TryGetControl(buttonIdentity, out var foundControl).ShouldBeTrue();
        foundControl.ShouldBeSameAs(registry.Controls[0]);
        foundControl.OwnTokens.ShouldBe([contentFontSize]);
        registry.TryGetAlgorithm(ThemeAlgorithm.Default, out var foundAlgorithm).ShouldBeTrue();
        foundAlgorithm.ShouldBeSameAs(defaultAlgorithm);
        foundAlgorithm.Algorithm.ShouldBe(ThemeAlgorithm.Default);
        registry.TryGetGlobalToken("colorprimary", out _).ShouldBeFalse();
        registry.TryGetControl(new ControlTokenIdentity("atomui", "Button"), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("global-name")]
    [InlineData("global-slot")]
    [InlineData("control-identity")]
    [InlineData("algorithm-id")]
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
        var defaultAlgorithm = Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Preserve);
        var algorithms = duplicate == "algorithm-id"
            ? new[] { defaultAlgorithm, Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Dark) }
            : new[] { defaultAlgorithm };

        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(globals, controls, algorithms));
    }

    [Fact]
    public void Registry_Indexes_Exact_Control_Type_And_Rejects_Duplicate_Type()
    {
        var button = Control("AtomUI", "Button");
        var input = Control("AtomUI", "Input");
        var registry = new ThemeSchemaRegistry(
            Array.Empty<TokenDescriptor>(),
            [button, input],
            Array.Empty<ThemeAlgorithmDescriptor>());

        registry.TryGetControl(typeof(ButtonThemeTestControl), out var found).ShouldBeTrue();
        found!.Identity.ShouldBe(button.Identity);
        registry.TryGetControl(typeof(DerivedButtonThemeTestControl), out _).ShouldBeFalse();

        var duplicateType = new ControlTokenDescriptor(
            typeof(ButtonThemeTestControl),
            new ControlTokenIdentity("AtomUI", "Other"));
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            Array.Empty<TokenDescriptor>(),
            [button, duplicateType],
            Array.Empty<ThemeAlgorithmDescriptor>()));
    }

    [Fact]
    public void Registry_Requires_Contiguous_Global_And_Control_Own_Slots()
    {
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            [Token("ColorPrimary", 1, TokenStage.Seed)],
            Array.Empty<ControlTokenDescriptor>(),
            [Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Preserve)]));

        var invalidControl = new ControlTokenDescriptor(
            typeof(ButtonThemeTestControl),
            new ControlTokenIdentity("AtomUI", "Button"),
            [Token("ContentFontSize", 1, TokenStage.Control)],
            static () => throw new InvalidOperationException(),
            static (_, _) => throw new InvalidOperationException());
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            [Token("ColorPrimary", 0, TokenStage.Seed)],
            [invalidControl],
            [Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Preserve)]));
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
            static () => throw new InvalidOperationException(),
            static (_, _) => throw new InvalidOperationException());

        new ThemeSchemaRegistry(
            [global],
            [unusedGlobal],
            Array.Empty<ThemeAlgorithmDescriptor>());
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry(
            [global],
            [collidingOwnToken],
            Array.Empty<ThemeAlgorithmDescriptor>()));
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
    public void Registry_Revision_Is_Order_Independent_And_Changes_With_Algorithm_Revision()
    {
        var token = Token("ColorPrimary", 0, TokenStage.Seed);
        var control = Control("AtomUI", "Button");
        var first = new ThemeSchemaRegistry(
            [token],
            [control],
            [Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Light, revision: 1)]);
        var reordered = new ThemeSchemaRegistry(
            [token],
            [control],
            [Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Light, revision: 1)]);
        var changed = new ThemeSchemaRegistry(
            [token],
            [control],
            [Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Light, revision: 2)]);

        reordered.Revision.ShouldBe(first.Revision);
        changed.Revision.ShouldNotBe(first.Revision);
    }

    [Fact]
    public void Registry_Revision_Is_Deterministic_For_Algorithm_Identities()
    {
        // No tokens or controls: their CLR assembly-qualified names would tie the pinned value
        // to framework and test assembly versions instead of to the algorithm identities.
        var first = new ThemeSchemaRegistry(
            Array.Empty<TokenDescriptor>(),
            Array.Empty<ControlTokenDescriptor>(),
            [
                Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Light),
                Algorithm(ThemeAlgorithm.Dark, ThemeAppearanceEffect.Dark),
                Algorithm(new ThemeAlgorithm("PrimeNG", "Aura"), ThemeAppearanceEffect.Light)
            ]);
        var reordered = new ThemeSchemaRegistry(
            Array.Empty<TokenDescriptor>(),
            Array.Empty<ControlTokenDescriptor>(),
            [
                Algorithm(new ThemeAlgorithm("PrimeNG", "Aura"), ThemeAppearanceEffect.Light),
                Algorithm(ThemeAlgorithm.Dark, ThemeAppearanceEffect.Dark),
                Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Light)
            ]);

        reordered.Revision.ShouldBe(first.Revision);
        first.Algorithms.Select(static descriptor => descriptor.Algorithm.ToString())
             .ShouldBe(["AtomUI:Dark", "AtomUI:Default", "PrimeNG:Aura"]);
        // Pins cross-process stability: the revision must never depend on string.GetHashCode().
        first.Revision.Value.ShouldBe(0x29D0F9BA9FF24C7FUL);
    }

    [Fact]
    public void Registry_Revision_Changes_With_Algorithm_Catalog_Or_Id()
    {
        var token = Token("ColorPrimary", 0, TokenStage.Seed);
        var control = Control("AtomUI", "Button");
        ThemeSchemaRevision Revision(ThemeAlgorithm algorithm) => new ThemeSchemaRegistry(
            [token],
            [control],
            [Algorithm(algorithm, ThemeAppearanceEffect.Light)]).Revision;

        var baseline = Revision(new ThemeAlgorithm("PrimeNG", "Aura"));

        Revision(new ThemeAlgorithm("PrimeNG", "Aura")).ShouldBe(baseline);
        Revision(new ThemeAlgorithm("PrimeVue", "Aura")).ShouldNotBe(baseline);
        Revision(new ThemeAlgorithm("PrimeNG", "Lara")).ShouldNotBe(baseline);
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

    private static ThemeAlgorithmDescriptor Algorithm(
        ThemeAlgorithm algorithm,
        ThemeAppearanceEffect effect,
        int revision = 1)
    {
        return new ThemeAlgorithmDescriptor(
            algorithm,
            revision,
            effect,
            static () => throw new InvalidOperationException());
    }
}
