using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeTokenResolverTests
{
    [Fact]
    public void Capture_And_Dense_Reads_Use_One_Explicit_Owner_Context()
    {
        var snapshot = ThemeTokenResourceProviderTests.Compile();
        var manager = new ThemeManager(static () => true);
        var context = new ThemeContext(manager, snapshot, 0);
        var owner = new Border();
        owner.SetValue(ThemeScope.ContextProperty, context);
        var resolver = new ThemeTokenResolver();
        snapshot.Registry.TryGetGlobalToken(nameof(DesignToken.ColorPrimary), out var global).ShouldBeTrue();
        snapshot.Registry.TryGetControl(
            new AtomUI.Theme.Schema.ControlTokenIdentity("AtomUI", CompilerButtonToken.ID),
            out var control).ShouldBeTrue();

        var captured = resolver.Capture(owner);

        captured.ShouldBeSameAs(snapshot);
        resolver.GetGlobal<Avalonia.Media.Color>(captured, global!.Slot)
                .ShouldBe(snapshot.Global<Avalonia.Media.Color>(nameof(DesignToken.ColorPrimary)));
        resolver.GetControl<double>(captured, control!.Slot, 0).ShouldBe(32d);
    }

    [Fact]
    public void Subscription_Follows_Context_Replacement_And_Dispose_Releases_Notifications()
    {
        var first = ThemeTokenResourceProviderTests.Compile();
        var second = ThemeTokenResourceProviderTests.Compile(globalPrimary: "#00b96b");
        var manager = new ThemeManager(static () => true);
        var firstContext = new ThemeContext(manager, first, 0);
        var secondContext = new ThemeContext(manager, second, 1);
        var owner = new Border();
        owner.SetValue(ThemeScope.ContextProperty, firstContext);
        var resolver = new ThemeTokenResolver();
        var observed = new List<ThemeSnapshot>();

        var subscription = resolver.Subscribe(owner, observed.Add);
        firstContext.Publish();
        owner.SetValue(ThemeScope.ContextProperty, secondContext);
        firstContext.Publish();
        secondContext.Publish();
        subscription.Dispose();
        secondContext.Publish();

        observed.ShouldBe([first, second, second]);
    }

    [Fact]
    public void Subscription_Immediately_Observes_A_Context_Assigned_After_Subscribe()
    {
        var snapshot = ThemeTokenResourceProviderTests.Compile();
        var manager = new ThemeManager(static () => true);
        var context = new ThemeContext(manager, snapshot, 0);
        var owner = new Border();
        var resolver = new ThemeTokenResolver();
        var observed = new List<ThemeSnapshot>();

        using var subscription = resolver.Subscribe(owner, observed.Add);
        owner.SetValue(ThemeScope.ContextProperty, context);

        observed.ShouldBe([snapshot]);
    }

    [Fact]
    public void Missing_Context_Is_Rejected_Instead_Of_Falling_Back_To_A_Global_Manager()
    {
        var resolver = new ThemeTokenResolver();

        Should.Throw<InvalidOperationException>(() => resolver.Capture(new Border()));
    }

    [Fact]
    public void Control_Reads_Use_Control_Scoped_Global_Overrides_And_Preset_Palettes()
    {
        var snapshot = ThemeTokenResourceProviderTests.CompileCatalogControls();
        var resolver = new ThemeTokenResolver();
        var identity = new AtomUI.Theme.Schema.ControlTokenIdentity("First", CompilerButtonToken.ID);
        snapshot.Registry.TryGetGlobalToken(nameof(DesignToken.ColorPrimary), out var colorPrimary).ShouldBeTrue();

        var controlSlot = resolver.GetControlSlot(snapshot, identity);
        var effectivePrimary = resolver.GetEffectiveGlobal<Color>(
            snapshot,
            controlSlot,
            colorPrimary!.Slot);
        var palette = resolver.GetPresetPalette(snapshot, PresetPrimaryColor.Blue);

        effectivePrimary.ShouldBe(Color.Parse("#00b96b"));
        palette.ColorSequence.Count.ShouldBe(10);
    }

    [Fact]
    public void Control_Accessor_Uses_Exact_Owner_Type_And_One_Captured_Snapshot()
    {
        var snapshot = ThemeTokenResourceProviderTests.Compile(controlPrimary: "#00b96b");
        var manager = new ThemeManager(static () => true);
        var context = new ThemeContext(manager, snapshot, 0);
        var owner = new ButtonThemeTestControl();
        owner.SetValue(ThemeScope.ContextProperty, context);

        var accessor = ControlTokenAccessor.Capture(owner);

        accessor.GetEffectiveGlobal<Color>(SharedTokenKind.ColorPrimary)
                .ShouldBe(Color.Parse("#00b96b"));
        accessor.GetOwn<double>(CompilerButtonTokenKind.Height).ShouldBe(32d);

        var derived = new DerivedButtonThemeTestControl();
        derived.SetValue(ThemeScope.ContextProperty, context);
        Should.Throw<InvalidOperationException>(() => ControlTokenAccessor.Capture(derived));
    }

    [Fact]
    public void Control_Accessor_Can_Explicitly_Capture_A_Registered_Base_Control_Type()
    {
        var snapshot = ThemeTokenResourceProviderTests.Compile(controlPrimary: "#00b96b");
        var manager = new ThemeManager(static () => true);
        var context = new ThemeContext(manager, snapshot, 0);
        var owner = new DerivedButtonThemeTestControl();
        owner.SetValue(ThemeScope.ContextProperty, context);

        var accessor = ControlTokenAccessor.Capture<ButtonThemeTestControl>(owner);

        accessor.GetEffectiveGlobal<Color>(SharedTokenKind.ColorPrimary)
                .ShouldBe(Color.Parse("#00b96b"));
        accessor.GetOwn<double>(CompilerButtonTokenKind.Height).ShouldBe(32d);
    }
}
