using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>What a publish delta reports for global, owner-global and Own keys between two compiled snapshots.</summary>
public class ThemePublishDeltaTests
{
    private static readonly ControlTokenIdentity s_button = new("AtomUI", CompilerButtonToken.ID);

    [Fact]
    public void Global_Keys_Compare_By_Value_And_Unknown_Keys_Count_As_Changed()
    {
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry();
        var compiler = new ThemeCompiler();
        var previous = compiler.Compile(TypedThemeSnapshotCacheTests.CreateInput(registry)).Snapshot!;
        var current = compiler.Compile(TypedThemeSnapshotCacheTests.CreateInput(
            registry, globalTokens: [Token(registry, "ColorPrimary", "#00b96b")])).Snapshot!;

        var delta = ThemePublishDelta.Between(previous, current);

        delta.IsEverything.ShouldBeFalse();
        delta.IsGlobalChanged(registry.GetSharedResourceKey(SharedTokenKind.ColorPrimary)).ShouldBeTrue();
        delta.IsGlobalChanged(registry.GetSharedResourceKey(SharedTokenKind.ColorText)).ShouldBeFalse();
        delta.IsGlobalChanged(SharedTokenKind.ColorText).ShouldBeFalse("a freshly boxed kind is the same key");
        delta.IsChanged(SharedTokenKind.ColorPrimary).ShouldBeTrue();
        delta.IsChanged(new object()).ShouldBeTrue("a key the theme does not own is never skipped");
        ThemePublishDelta.Between(null, current).IsEverything.ShouldBeTrue();
        ThemePublishDelta.Nothing.IsChanged(SharedTokenKind.ColorPrimary).ShouldBeFalse();
        ThemePublishDelta.Everything.IsChanged(SharedTokenKind.ColorText).ShouldBeTrue();
    }

    [Fact]
    public void Owner_Keys_Follow_The_Root_Without_An_Override_And_Compare_Materialized_Values_With_One()
    {
        var button = ThemeCompilerTests.CreateCompilerButtonDescriptor();
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([button]);
        registry.TryGetControl(s_button, out button).ShouldBeTrue();
        var compiler = new ThemeCompiler();
        var borderKey = registry.GetSharedResourceKey(SharedTokenKind.ColorBorder);
        var heightKey = button!.OwnTokens[0].ResourceKey;
        var previous = compiler.Compile(TypedThemeSnapshotCacheTests.CreateInput(registry)).Snapshot!;
        var current = compiler.Compile(TypedThemeSnapshotCacheTests.CreateInput(
            registry, globalTokens: [Token(registry, "ColorPrimary", "#00b96b")])).Snapshot!;

        var delta = ThemePublishDelta.Between(previous, current);

        // Cold Own cells: unknown, so changed.
        delta.IsOwnChanged(button.Slot, heightKey).ShouldBeTrue();
        // Unconfigured owner: the effective ColorBorder is the root value, which did not change.
        delta.IsOwnerGlobalChanged(button.Slot, borderKey).ShouldBeFalse();
        delta.IsChanged(ControlTokenResourceKey.Global(s_button, SharedTokenKind.ColorBorder)).ShouldBeFalse();
        delta.IsChanged(ControlTokenResourceKey.Global(s_button, SharedTokenKind.ColorPrimary)).ShouldBeTrue();

        // Both cells materialized: Own values compare by value.
        _ = previous.Controls[button.Slot];
        _ = current.Controls[button.Slot];
        delta.IsOwnChanged(button.Slot, heightKey).ShouldBeFalse("Height follows ControlHeight, which did not change");
        delta.IsChanged(ControlTokenResourceKey.Own(s_button, heightKey)).ShouldBeFalse();
        delta.IsChanged(heightKey).ShouldBeFalse("the plain Own resource key resolves to the same slot");

        var taller = compiler.Compile(TypedThemeSnapshotCacheTests.CreateInput(
            registry, globalTokens: [Token(registry, "ControlHeight", "48")])).Snapshot!;
        _ = taller.Controls[button.Slot];
        ThemePublishDelta.Between(current, taller).IsOwnChanged(button.Slot, heightKey).ShouldBeTrue();

        // A configured owner whose override is unchanged: materialized values compare equal.
        var overridden = new ThemeConfigBuilder()
                         .WithControl(s_button, new ControlThemeConfigBuilder().WithToken("ColorBorder", "#ff0000").Build());
        var first = ThemeTestSnapshotFactory.Compile(overridden.Build(), ThemeCompilerTests.CreateCompilerButtonDescriptor());
        var second = ThemeTestSnapshotFactory.Compile(overridden.WithToken("ColorPrimary", "#00b96b").Build(),
            ThemeCompilerTests.CreateCompilerButtonDescriptor());
        first.Registry.TryGetControl(s_button, out var firstButton).ShouldBeTrue();
        _ = first.Controls[firstButton!.Slot];
        _ = second.Controls[firstButton.Slot];
        // Different registry instances: comparable only when the revision matches, which separate registries do not.
        ThemePublishDelta.Between(first, second).IsEverything.ShouldBe(first.RegistryRevision != second.RegistryRevision);
    }

    private static NormalizedTokenValue Token(ThemeSchemaRegistry registry, string name, string value)
    {
        registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue();
        var parsed = descriptor!.Parse(value);
        return new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
    }
}
