using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class DenseThemeCompilerTests
{
    [Fact]
    public void Default_Configs_Are_Cached_By_Their_Immutable_Owner()
    {
        var registry = CreateRegistry();
        var definition = CreateInput(registry).Definition;

        var firstLibraryDefaults = ThemeCompiler.CreateLibraryDefaults(registry);
        var secondLibraryDefaults = ThemeCompiler.CreateLibraryDefaults(registry);
        var firstDefinitionDefaults = ThemeCompiler.CreateDefinitionDefaults(definition, registry);
        var secondDefinitionDefaults = ThemeCompiler.CreateDefinitionDefaults(definition, registry);

        secondLibraryDefaults.ShouldBeSameAs(firstLibraryDefaults);
        secondDefinitionDefaults.ShouldBeSameAs(firstDefinitionDefaults);
    }

    [Fact]
    public void Definition_Defaults_Carry_The_Definition_As_The_Value_Layer()
    {
        var button = Control<ButtonId>();
        var input = Control<InputId>();
        var registry = CreateRegistry([button, input]);
        registry.TryGetControl(button.Identity, out button).ShouldBeTrue();
        registry.TryGetControl(input.Identity, out input).ShouldBeTrue();
        var location = new ThemeSourceLocation("test", 1, 1, "/Theme");
        var definition = new PreparedThemeDefinition("TestTheme", "Test Theme", TestDesignLanguages.AntDesign,
            null, ThemeAppearance.Dark, null, new HashSet<string>(StringComparer.Ordinal),
            new ThemeSectionDefinition(Array.Empty<BoundTokenValue>(), Array.Empty<BoundTokenValue>(),
            [
                new ControlThemeDefinition(input, null, null, Array.Empty<BoundTokenValue>(), Array.Empty<BoundTokenValue>(), location),
                new ControlThemeDefinition(button, null, null, Array.Empty<BoundTokenValue>(), Array.Empty<BoundTokenValue>(), location)
            ]),
            Array.Empty<ThemeConditionDefinition>(), location);

        var defaults = ThemeCompiler.CreateDefinitionDefaults(definition, registry);

        defaults.Definition.ShouldBeSameAs(definition);
        defaults.Controls.ShouldBeEmpty();
        defaults.Language.ShouldBeSameAs(TestDesignLanguages.AntDesign);
        defaults.Axes.Appearance.ShouldBe(ThemeAppearance.Dark);
        definition.ControlIdentities.ShouldBe([button.Identity, input.Identity], ignoreOrder: true);
    }

    [Fact]
    public void Compile_Applies_Seed_Map_And_Alias_Overrides_In_Order()
    {
        var registry = CreateRegistry();
        var input = CreateInput(
            registry,
            globalTokens:
            [
                Token(registry, nameof(DesignToken.ColorPrimary), "#ff0000"),
                Token(registry, nameof(DesignToken.ColorPrimaryHover), "#010203"),
                Token(registry, nameof(DesignToken.ColorTextDisabled), "#040506")
            ]);

        var result = new ThemeCompiler().Compile(input);

        result.Success.ShouldBeTrue();
        GetGlobal<Color>(result.Snapshot!, registry, nameof(DesignToken.ColorPrimary))
            .ShouldBe(Color.Parse("#ff0000"));
        GetGlobal<Color>(result.Snapshot!, registry, nameof(DesignToken.ColorPrimaryHover))
            .ShouldBe(Color.Parse("#010203"));
        GetGlobal<Color>(result.Snapshot!, registry, nameof(DesignToken.ColorTextDisabled))
            .ShouldBe(Color.Parse("#040506"));
    }

    [Fact]
    public void Compile_Final_Normalization_Disables_Motion_And_Wave_Inputs()
    {
        var registry = CreateRegistry();
        var input = CreateInput(
            registry,
            globalTokens:
            [
                Token(registry, nameof(DesignToken.EnableMotion), "False"),
                Token(registry, nameof(DesignToken.MotionDurationFast), "00:00:01"),
                Token(registry, nameof(DesignToken.MotionDurationMid), "00:00:02"),
                Token(registry, nameof(DesignToken.MotionDurationSlow), "00:00:03"),
                Token(registry, nameof(DesignToken.MotionDurationVerySlow), "00:00:04"),
                Token(registry, nameof(DesignToken.EnableWaveSpirit), "False"),
                Token(registry, nameof(DesignToken.WaveAnimationRange), "24"),
                Token(registry, nameof(DesignToken.WaveStartOpacity), "0.8")
            ]);

        var result = new ThemeCompiler().Compile(input);

        result.Success.ShouldBeTrue();
        var snapshot = result.Snapshot!;
        GetGlobal<bool>(snapshot, registry, nameof(DesignToken.EnableMotion)).ShouldBeFalse();
        GetGlobal<TimeSpan>(snapshot, registry, nameof(DesignToken.MotionDurationFast)).ShouldBe(TimeSpan.Zero);
        GetGlobal<TimeSpan>(snapshot, registry, nameof(DesignToken.MotionDurationMid)).ShouldBe(TimeSpan.Zero);
        GetGlobal<TimeSpan>(snapshot, registry, nameof(DesignToken.MotionDurationSlow)).ShouldBe(TimeSpan.Zero);
        GetGlobal<TimeSpan>(snapshot, registry, nameof(DesignToken.MotionDurationVerySlow)).ShouldBe(TimeSpan.Zero);
        GetGlobal<bool>(snapshot, registry, nameof(DesignToken.EnableWaveSpirit)).ShouldBeFalse();
        GetGlobal<double>(snapshot, registry, nameof(DesignToken.WaveAnimationRange)).ShouldBe(0);
        GetGlobal<double>(snapshot, registry, nameof(DesignToken.WaveStartOpacity)).ShouldBe(0);
    }

    [Fact]
    public void Compile_Control_Map_Override_Applies_Directly_Without_Rederivation()
    {
        var control = Control<ButtonId>();
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        var config = ControlConfig(control.Identity, [Token(registry, nameof(DesignToken.ColorPrimaryHover), "#00b96b")]);

        var result = new ThemeCompiler().Compile(CreateInput(registry, controls: [config]));

        result.Success.ShouldBeTrue();
        var snapshot = result.Snapshot!;
        var controlSnapshot = snapshot.Controls[control.Slot];
        GetControlGlobal<Color>(
                controlSnapshot,
                snapshot,
                registry,
                nameof(DesignToken.ColorPrimaryHover))
            .ShouldBe(Color.Parse("#00b96b"));
        GetControlGlobal<Color>(
                controlSnapshot,
                snapshot,
                registry,
                nameof(DesignToken.ColorPrimaryActive))
            .ShouldBe(GetGlobal<Color>(snapshot, registry, nameof(DesignToken.ColorPrimaryActive)));
    }

    [Fact]
    public void Compile_Control_Without_Own_Tokens_Produces_Effective_Global_Delta_And_Empty_Own_State()
    {
        var control = new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", "Rating"),
            new ControlTokenIdentity("AtomUI", "Rating"));
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        var config = ControlConfig(control.Identity, [Token(registry, nameof(DesignToken.ColorPrimary), "#00b96b")]);

        var result = new ThemeCompiler().Compile(CreateInput(registry, controls: [config]));

        result.Success.ShouldBeTrue();
        var snapshot = result.Snapshot!;
        var controlSnapshot = snapshot.Controls[control.Slot];
        GetControlGlobal<Color>(
                controlSnapshot,
                snapshot,
                registry,
                nameof(DesignToken.ColorPrimary))
            .ShouldBe(Color.Parse("#00b96b"));
        controlSnapshot.ControlTokenValues.ShouldBeSameAs(TokenValueTable.Empty);
        controlSnapshot.ControlResources.ShouldBeEmpty();
    }

    [Fact]
    public void Compile_Control_Without_Own_Tokens_Or_Config_Does_Not_Reproject_Global_Resources()
    {
        var resourceProjections = 0;
        var globalToken = ProjectionCountingGlobalToken(() => resourceProjections++);
        var control = new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", "Rating"),
            new ControlTokenIdentity("AtomUI", "Rating"));
        var registry = new ThemeSchemaRegistry([globalToken], [control], [TestDesignLanguages.Probe()]);

        var result = new ThemeCompiler().Compile(CreateInput(registry));

        result.Success.ShouldBeTrue();
        resourceProjections.ShouldBe(1);
        // Controls 为按槽位惰性容器（不实现枚举），单槽位语义用 Count + 索引表达。
        var controls = result.Snapshot!.Controls;
        controls.Count.ShouldBe(1);
        var controlSnapshot = controls[0];
        controlSnapshot.EffectiveGlobalTokenDelta.ShouldBeEmpty();
        controlSnapshot.EffectiveGlobalResourceDelta.ShouldBeEmpty();
        controlSnapshot.ControlTokenValues.ShouldBeSameAs(TokenValueTable.Empty);
        controlSnapshot.ControlResources.ShouldBeEmpty();
    }

    [Fact]
    public void Compile_Control_Without_Global_Overrides_Does_Not_Reproject_Global_Resources()
    {
        var resourceProjections = 0;
        var globalToken = ProjectionCountingGlobalToken(() => resourceProjections++);
        var control = Control<ButtonId>();
        var registry = new ThemeSchemaRegistry([globalToken], [control], [TestDesignLanguages.Probe()], null,
            [ControlTokenRuleDescriptor.Create<ProbeLanguageToken, DenseHeightToken<ButtonId>, ProbeDenseTokenRule<DenseHeightToken<ButtonId>>>()]);

        var result = new ThemeCompiler().Compile(CreateInput(registry));

        result.Success.ShouldBeTrue();
        resourceProjections.ShouldBe(1);
        var controls = result.Snapshot!.Controls;
        controls.Count.ShouldBe(1);
        var controlSnapshot = controls[0];
        controlSnapshot.EffectiveGlobalTokenDelta.ShouldBeEmpty();
        controlSnapshot.EffectiveGlobalResourceDelta.ShouldBeEmpty();
        controlSnapshot.ControlTokenValues.Count.ShouldBe(1);
        controlSnapshot.ControlResources.Count.ShouldBe(1);
    }

    [Fact]
    public void Compile_Control_Seed_Override_Rederives_Map_And_Freezes_Own_Tokens_With_Appearance()
    {
        var control = Control<ButtonId>();
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        var config = ControlConfig(control.Identity, [Token(registry, nameof(DesignToken.ColorPrimary), "#00b96b")],
            [ControlToken(control, "48")]);

        var result = new ThemeCompiler().Compile(CreateInput(registry, controls: [config]));

        result.Success.ShouldBeTrue();
        var snapshot = result.Snapshot!;
        var controlSnapshot = snapshot.Controls[control.Slot];
        GetControlGlobal<Color>(
                controlSnapshot,
                snapshot,
                registry,
                nameof(DesignToken.ColorPrimaryHover))
            .ShouldNotBe(GetGlobal<Color>(snapshot, registry, nameof(DesignToken.ColorPrimaryHover)));
        controlSnapshot.ControlTokenValues.Get<double>(0).ShouldBe(48);
        controlSnapshot.Appearance.ShouldBe(ThemeAppearance.Light);
    }

    [Fact]
    public void Compile_Only_Replaces_The_Changed_Control_When_Global_Content_Is_Unchanged()
    {
        var button = Control<ButtonId>();
        var input = Control<InputId>();
        var registry = CreateRegistry([button, input]);
        registry.TryGetControl(button.Identity, out button).ShouldBeTrue();
        registry.TryGetControl(input.Identity, out input).ShouldBeTrue();
        var parent = new ThemeCompiler().Compile(CreateInput(registry)).Snapshot!;
        var buttonConfig = ControlConfig(button.Identity, [], [ControlToken(button, "48")]);

        var child = new ThemeCompiler().Compile(CreateInput(
            registry,
            controls: [buttonConfig],
            reusableParent: parent)).Snapshot!;

        child.GlobalTokenValues.ShouldBeSameAs(parent.GlobalTokenValues);
        child.GlobalResources.ShouldBeSameAs(parent.GlobalResources);
        child.PresetColorPalettes.ShouldBeSameAs(parent.PresetColorPalettes);
        child.Controls[button.Slot].ShouldNotBeSameAs(parent.Controls[button.Slot]);
        child.Controls[input.Slot].ShouldBeSameAs(parent.Controls[input.Slot]);
    }

    [Fact]
    public void Compile_Uses_The_ChangeSet_To_Recompute_Only_The_Changed_Control()
    {
        var buttonEvaluations = 0;
        var inputEvaluations = 0;
        var button = Control<ButtonId>(onEvaluate: () => buttonEvaluations++);
        var input = Control<InputId>(onEvaluate: () => inputEvaluations++);
        var registry = CreateRegistry([button, input]);
        registry.TryGetControl(button.Identity, out button).ShouldBeTrue();
        registry.TryGetControl(input.Identity, out input).ShouldBeTrue();
        var compiler = new ThemeCompiler();
        var parent = compiler.Compile(CreateInput(registry)).Snapshot!;
        var buttonConfig = ControlConfig(button.Identity, [], [ControlToken(button, "48")]);
        var changedConfig = new NormalizedThemeConfig(
            false,
            parent.EffectiveConfig.Axes,
            parent.EffectiveConfig.GlobalTokens,
            parent.EffectiveConfig.LanguageTokens,
            [buttonConfig],
            parent.EffectiveConfig.Definition);
        var changeSet = ThemeConfigMerger.Compare(parent.EffectiveConfig, changedConfig);

        var child = compiler.Compile(CreateInput(
            registry,
            controls: [buttonConfig],
            reusableParent: parent,
            changeSet: changeSet)).Snapshot!;

        child.Controls[button.Slot].ShouldNotBeSameAs(parent.Controls[button.Slot]);
        child.Controls[input.Slot].ShouldBeSameAs(parent.Controls[input.Slot]);

        // 惰性契约（runtime.md §7 控件槽位编译契约）：评估发生在首访而非编译期。
        // 首访后 button 被父子两次独立求值（2），input 复用父对象只求值一次（1）——
        // "changeSet 只重算变更控件"的不变量保持不变。
        buttonEvaluations.ShouldBe(2);
        inputEvaluations.ShouldBe(1);
    }

    [Fact]
    public void Compile_Reuses_A_WithoutOwnToken_Control_When_Global_Content_Changes()
    {
        var control = new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", "Rating"),
            new ControlTokenIdentity("AtomUI", "Rating"));
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        var parent = new ThemeCompiler().Compile(CreateInput(registry)).Snapshot!;
        var changed = new ThemeCompiler().Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ColorPrimary), "#00b96b")],
            reusableParent: parent)).Snapshot!;

        changed.Controls[control.Slot].ShouldBeSameAs(parent.Controls[control.Slot]);
    }

    [Fact]
    public void Compile_Recomputes_An_OwnToken_Control_When_Global_Content_Changes()
    {
        var control = Control<ButtonId>();
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        var parent = new ThemeCompiler().Compile(CreateInput(registry)).Snapshot!;

        var changed = new ThemeCompiler().Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ControlHeight), "48")],
            reusableParent: parent)).Snapshot!;

        changed.Controls[control.Slot].ShouldNotBeSameAs(parent.Controls[control.Slot]);
        changed.Controls[control.Slot].ControlTokenValues.Get<double>(0).ShouldBe(48);
    }

    [Fact]
    public void A_Child_Unit_Reuses_The_Controls_Whose_Read_Set_Misses_The_Changed_Alias_Token()
    {
        var heading = ReadControl<ListId>(static global => global.ColorTextHeading.A);
        var height = ReadControl<TagId>(static global => global.ControlHeight);
        var registry = CreateRegistry([heading, height]);
        registry.TryGetControl(heading.Identity, out heading).ShouldBeTrue();
        registry.TryGetControl(height.Identity, out height).ShouldBeTrue();
        var parent = new ThemeCompiler().Compile(CreateInput(registry)).Snapshot!;
        var parentHeading = parent.Controls[heading.Slot];
        var parentHeight = parent.Controls[height.Slot];

        // Spec 12.5: a local scope that overrides one Alias token re-derives only the controls that read it.
        var child = new ThemeCompiler().Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ColorTextHeading), "#80112233")],
            reusableParent: parent)).Snapshot!;

        child.Controls[height.Slot].ShouldBeSameAs(parentHeight);
        child.Controls[heading.Slot].ShouldNotBeSameAs(parentHeading);
        child.Controls[heading.Slot].ControlTokenValues.Get<double>(0).ShouldBe(0x80);
    }

    [Fact]
    public void A_Child_Unit_That_Changes_A_Seed_Re_Derives_Every_Control()
    {
        var height = ReadControl<TagId>(static global => global.ControlHeight);
        var registry = CreateRegistry([height]);
        registry.TryGetControl(height.Identity, out height).ShouldBeTrue();
        var parent = new ThemeCompiler().Compile(CreateInput(registry)).Snapshot!;
        var parentHeight = parent.Controls[height.Slot];

        var child = new ThemeCompiler().Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ColorPrimary), "#00b96b")],
            reusableParent: parent)).Snapshot!;

        child.Controls[height.Slot].ShouldNotBeSameAs(parentHeight);
        child.Controls[height.Slot].ControlTokenValues.Get<double>(0).ShouldBe(32);
    }

    [Fact]
    public void A_Control_That_Overrides_A_Global_Token_Is_Not_Reused_By_Read_Set()
    {
        var height = ReadControl<TagId>(static global => global.ControlHeight);
        var registry = CreateRegistry([height]);
        registry.TryGetControl(height.Identity, out height).ShouldBeTrue();
        var controls = new[] { ControlConfig(height.Identity, [Token(registry, nameof(DesignToken.ColorLink), "#ff0000")]) };
        var parent = new ThemeCompiler().Compile(CreateInput(registry, controls: controls)).Snapshot!;
        var parentHeight = parent.Controls[height.Slot];

        var child = new ThemeCompiler().Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ColorTextHeading), "#80112233")],
            controls: controls,
            reusableParent: parent)).Snapshot!;

        child.Controls[height.Slot].ShouldNotBeSameAs(parentHeight);
    }

    [Fact]
    public void Controls_That_Write_No_Global_Token_Give_The_Same_Values_With_A_Shared_View()
    {
        var first = ReadControl<ListId>(static global => global.ControlHeight + 1);
        var second = ReadControl<TagId>(static global => global.ControlHeight + 2);
        var registry = CreateRegistry([first, second]);
        registry.TryGetControl(first.Identity, out first).ShouldBeTrue();
        registry.TryGetControl(second.Identity, out second).ShouldBeTrue();

        var snapshot = new ThemeCompiler().Compile(CreateInput(registry)).Snapshot!;

        snapshot.Controls[first.Slot].ControlTokenValues.Get<double>(0).ShouldBe(33);
        snapshot.Controls[second.Slot].ControlTokenValues.Get<double>(0).ShouldBe(34);
        snapshot.Controls[first.Slot].EffectiveGlobalTokenDelta.ShouldBeEmpty();
        snapshot.Controls[second.Slot].EffectiveGlobalTokenDelta.ShouldBeEmpty();
    }

    [Fact]
    public void Compile_Keeps_The_Instance_Of_An_Unchanged_Resource_Across_Publishes_And_Replaces_A_Changed_One()
    {
        // Avalonia compares a republished property value with EqualityComparer<T>.Default; an unchanged Token that
        // keeps its instance across publishes short-circuits by reference and allocates nothing new.
        var control = Control<ButtonId>();
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        var compiler = new ThemeCompiler();
        var parent = compiler.Compile(CreateInput(registry)).Snapshot!;
        var parentButton = parent.Controls[control.Slot];
        var textKey = registry.GetSharedResourceKey(SharedTokenKind.ColorText);
        var primaryKey = registry.GetSharedResourceKey(SharedTokenKind.ColorPrimary);
        var heightKey = control.OwnTokens[0].ResourceKey;

        var changedPrimary = compiler.Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ColorPrimary), "#00b96b")],
            reusableParent: parent)).Snapshot!;

        changedPrimary.GlobalResources.ShouldNotBeSameAs(parent.GlobalResources);
        changedPrimary.GlobalResources[textKey].ShouldBeSameAs(parent.GlobalResources[textKey],
            "an unchanged global brush keeps its instance");
        changedPrimary.GlobalResources[primaryKey].ShouldNotBe(parent.GlobalResources[primaryKey],
            "a changed global brush gets a new value");
        changedPrimary.Controls[control.Slot].ShouldNotBeSameAs(parentButton);
        changedPrimary.Controls[control.Slot].ControlResources[heightKey].ShouldBeSameAs(
            parentButton.ControlResources[heightKey],
            "an unchanged Own value keeps its instance although the control was recompiled");

        var changedHeight = compiler.Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ControlHeight), "48")],
            reusableParent: changedPrimary)).Snapshot!;

        changedHeight.Controls[control.Slot].ControlResources[heightKey].ShouldBe(48d);
        changedHeight.Controls[control.Slot].ControlResources[heightKey]
                     .ShouldNotBeSameAs(parentButton.ControlResources[heightKey]);
    }

    [Fact]
    public void Compile_Keeps_The_Instance_Of_An_Unchanged_Effective_Global_Override_Across_Publishes()
    {
        var control = Control<ButtonId>();
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();
        var compiler = new ThemeCompiler();
        var borderKey = registry.GetSharedResourceKey(SharedTokenKind.ColorBorder);
        var buttonConfig = ControlConfig(control.Identity, [Token(registry, nameof(DesignToken.ColorBorder), "#ff0000")]);
        var parent = compiler.Compile(CreateInput(registry, controls: [buttonConfig])).Snapshot!;
        var parentDelta = parent.Controls[control.Slot].EffectiveGlobalResourceDelta;
        parentDelta.ShouldContainKey(borderKey);

        var changed = compiler.Compile(CreateInput(
            registry,
            globalTokens: [Token(registry, nameof(DesignToken.ColorPrimary), "#00b96b")],
            controls: [buttonConfig],
            reusableParent: parent)).Snapshot!;

        var changedDelta = changed.Controls[control.Slot].EffectiveGlobalResourceDelta;
        changedDelta.ShouldNotBeSameAs(parentDelta);
        changedDelta[borderKey].ShouldBeSameAs(parentDelta[borderKey],
            "an unchanged effective-global override keeps its instance across publishes");
    }

    [Fact]
    public void Compile_Preserves_Immutable_Control_Token_Values_Without_String_Round_Trip()
    {
        var transform = new ImmutableTransform(Matrix.CreateTranslation(4, 8));
        var control = ValueControl<BadgeId, ImmutableTransform>(transform);
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();

        var result = new ThemeCompiler().Compile(CreateInput(registry));

        result.Success.ShouldBeTrue();
        result.Snapshot!.Controls[control.Slot]
              .ControlTokenValues.Get<ImmutableTransform>(0)
              .ShouldBeSameAs(transform);
    }

    [Theory]
    [InlineData("token")]
    [InlineData("resource")]
    [InlineData("consumer")]
    public void Compile_Clones_Mutable_Spline_Easing_Control_Token_Values(string path)
    {
        var easing = new SplineEasing { X1 = 0.1, Y1 = 0.3, X2 = 0.6, Y2 = 0.8 };
        var control = ValueControl<SplitViewId, Easing>(easing);
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();

        var result = new ThemeCompiler().Compile(CreateInput(registry));

        result.Success.ShouldBeTrue();
        var snapshot = result.Snapshot!.Controls[control.Slot];
        var frozen = (path switch
        {
            "token" => snapshot.ControlTokenValues.GetValue(0),
            "resource" => snapshot.ControlResources[control.OwnTokens[0].ResourceKey],
            "consumer" => snapshot.ControlTokenValues.Get<Easing>(0),
            _ => throw new ArgumentOutOfRangeException(nameof(path))
        }).ShouldBeOfType<SplineEasing>();
        frozen.ShouldNotBeSameAs(easing);
        easing.Y1 = 0.9;
        frozen.X1.ShouldBe(0.1);
        frozen.Y1.ShouldBe(0.3);
        frozen.X2.ShouldBe(0.6);
        frozen.Y2.ShouldBe(0.8);
    }

    [Fact]
    public void Compile_Preserves_Concrete_SolidColorBrush_Resource_Type_With_An_Owned_Copy()
    {
        var brush = new SolidColorBrush(Colors.CornflowerBlue, 0.75);
        var control = ValueControl<WindowId, SolidColorBrush>(brush, ThemeResourceValue.Project);
        var registry = CreateRegistry([control]);
        registry.TryGetControl(control.Identity, out control).ShouldBeTrue();

        var result = new ThemeCompiler().Compile(CreateInput(registry));

        result.Success.ShouldBeTrue();
        var snapshot = result.Snapshot!.Controls[control.Slot];
        var frozen = snapshot.ControlTokenValues.Get<SolidColorBrush>(0);
        frozen.ShouldNotBeSameAs(brush);
        frozen.Color.ShouldBe(brush.Color);
        frozen.Opacity.ShouldBe(brush.Opacity);
        snapshot.ControlResources[control.OwnTokens[0].ResourceKey]
                .ShouldBeOfType<SolidColorBrush>()
                .ShouldNotBeSameAs(brush);
    }

    private static ThemeSchemaRegistry CreateRegistry()
    {
        return CreateRegistry(Array.Empty<ControlTokenDescriptor>());
    }

    private static ThemeSchemaRegistry CreateRegistry(IReadOnlyList<ControlTokenDescriptor> controls)
    {
        return new ThemeSchemaRegistry(
            GeneratedThemeSchema.GetGlobalTokens(),
            controls,
            [TestDesignLanguages.AntDesign],
            null,
            DenseTokenRules);
    }

    // Every test control's rule; the ones whose control a registry does not hold stay dormant.
    private static readonly ControlTokenRuleDescriptor[] DenseTokenRules =
    [
        ControlTokenRuleDescriptor.Create<AntDesignToken, DenseHeightToken<ButtonId>, DenseTokenRule<DenseHeightToken<ButtonId>>>(),
        ControlTokenRuleDescriptor.Create<AntDesignToken, DenseHeightToken<InputId>, DenseTokenRule<DenseHeightToken<InputId>>>(),
        ControlTokenRuleDescriptor.Create<AntDesignToken, DenseReadToken<ListId>, DenseTokenRule<DenseReadToken<ListId>>>(),
        ControlTokenRuleDescriptor.Create<AntDesignToken, DenseReadToken<TagId>, DenseTokenRule<DenseReadToken<TagId>>>(),
        ControlTokenRuleDescriptor.Create<AntDesignToken, DenseValueToken<BadgeId, ImmutableTransform>,
            DenseTokenRule<DenseValueToken<BadgeId, ImmutableTransform>>>(),
        ControlTokenRuleDescriptor.Create<AntDesignToken, DenseValueToken<SplitViewId, Easing>,
            DenseTokenRule<DenseValueToken<SplitViewId, Easing>>>(),
        ControlTokenRuleDescriptor.Create<AntDesignToken, DenseValueToken<WindowId, SolidColorBrush>,
            DenseTokenRule<DenseValueToken<WindowId, SolidColorBrush>>>()
    ];

    private static ThemeCompileInput CreateInput(
        ThemeSchemaRegistry registry,
        IReadOnlyList<NormalizedTokenValue>? globalTokens = null,
        IReadOnlyList<NormalizedControlThemeConfig>? controls = null,
        ThemeSnapshot? reusableParent = null,
        ThemeConfigChangeSet? changeSet = null)
    {
        var definition = ThemeTestSnapshotFactory.CreateLanguageDefinition(registry.Languages[0]);
        var defaults = ThemeCompiler.CreateDefinitionDefaults(definition, registry);
        var config = new NormalizedThemeConfig(
            false,
            defaults.Axes,
            globalTokens ?? Array.Empty<NormalizedTokenValue>(),
            Array.Empty<LanguageTokenValue>(),
            controls ?? Array.Empty<NormalizedControlThemeConfig>(),
            definition);
        return new ThemeCompileInput(
            definition,
            new ThemeDefinitionRevision("test", "1", "A1"),
            config,
            registry,
            reusableParent,
            changeSet);
    }

    private static NormalizedControlThemeConfig ControlConfig(
        ControlTokenIdentity identity,
        IReadOnlyList<NormalizedTokenValue> globalTokens,
        IReadOnlyList<NormalizedTokenValue>? ownTokens = null) =>
        new(identity, null, null, globalTokens, ownTokens ?? Array.Empty<NormalizedTokenValue>());

    private static NormalizedTokenValue Token(
        ThemeSchemaRegistry registry,
        string name,
        string value)
    {
        registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue();
        var parsed = descriptor!.Parse(value);
        return new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
    }

    private static T GetGlobal<T>(ThemeSnapshot snapshot, ThemeSchemaRegistry registry, string name)
    {
        registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue();
        return snapshot.GlobalTokenValues.Get<T>(descriptor!.Slot);
    }

    private static T GetControlGlobal<T>(
        ControlThemeSnapshot control,
        ThemeSnapshot snapshot,
        ThemeSchemaRegistry registry,
        string name)
    {
        registry.TryGetGlobalToken(name, out var descriptor).ShouldBeTrue();
        return control.GetEffectiveGlobalValue<T>(snapshot.GlobalTokenValues, descriptor!.Slot);
    }

    private static NormalizedTokenValue ControlToken(ControlTokenDescriptor control, string value)
    {
        var descriptor = control.OwnTokens.ShouldHaveSingleItem();
        var parsed = descriptor.Parse(value);
        return new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
    }

    private static ControlTokenDescriptor Control<TId>(Action? onEvaluate = null)
        where TId : IDenseId
    {
        var token = new TokenDescriptor(
            "Height",
            0,
            TokenStage.Control,
            typeof(double),
            $"{TId.Id}.Height",
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static builder => ((DenseHeightToken<TId>)builder).Height,
            static (builder, value) => ((DenseHeightToken<TId>)builder).Height = (double)value!,
            static builder => ((DenseHeightToken<TId>)builder).Height);
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", TId.Id),
            DenseHeightToken<TId>.ControlIdentity,
            [token],
            () => new DenseHeightToken<TId>(onEvaluate));
    }

    private static ControlTokenDescriptor ReadControl<TId>(Func<DesignToken, double> read)
        where TId : IDenseId
    {
        var token = new TokenDescriptor(
            "Height",
            0,
            TokenStage.Control,
            typeof(double),
            $"{TId.Id}.Height",
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static builder => ((DenseReadToken<TId>)builder).Height,
            static (builder, value) => ((DenseReadToken<TId>)builder).Height = (double)value!,
            static builder => ((DenseReadToken<TId>)builder).Height);
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", TId.Id),
            DenseReadToken<TId>.ControlIdentity,
            [token],
            () => new DenseReadToken<TId>(read));
    }

    private static ControlTokenDescriptor ValueControl<TId, TValue>(TValue value, Func<TValue?, object?>? project = null)
        where TId : IDenseId
        where TValue : class
    {
        var token = new TokenDescriptor(
            "Value",
            0,
            TokenStage.Control,
            typeof(TValue),
            $"{TId.Id}.Value",
            static text => ThemeTokenValueParser.Parse<TValue>(text),
            static stored => ThemeTokenValueFormatter.Format((TValue)stored!),
            static builder => ((DenseValueToken<TId, TValue>)builder).Value,
            static (builder, stored) => ((DenseValueToken<TId, TValue>)builder).Value = (TValue)stored!,
            project is null
                ? static builder => ((DenseValueToken<TId, TValue>)builder).Value
                : builder => project(((DenseValueToken<TId, TValue>)builder).Value));
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", TId.Id),
            DenseValueToken<TId, TValue>.ControlIdentity,
            [token],
            () => new DenseValueToken<TId, TValue>(value));
    }

    private static TokenDescriptor ProjectionCountingGlobalToken(Action onProject)
    {
        return new TokenDescriptor(
            nameof(DesignToken.ColorPrimary),
            0,
            TokenStage.Seed,
            typeof(Color),
            nameof(DesignToken.ColorPrimary),
            static value => ThemeTokenValueParser.Parse<Color>(value),
            static value => ThemeTokenValueFormatter.Format((Color)value!),
            static builder => ((DesignToken)builder).ColorPrimary,
            static (builder, value) => ((DesignToken)builder).ColorPrimary = (Color)value!,
            builder =>
            {
                onProject();
                return ((DesignToken)builder).ColorPrimary;
            });
    }

    public interface IDenseId
    {
        static abstract string Id { get; }
    }

    public sealed class ButtonId : IDenseId { public static string Id => "Button"; }
    public sealed class InputId : IDenseId { public static string Id => "Input"; }
    public sealed class BadgeId : IDenseId { public static string Id => "Badge"; }
    public sealed class SplitViewId : IDenseId { public static string Id => "SplitView"; }
    public sealed class WindowId : IDenseId { public static string Id => "Window"; }
    public sealed class ListId : IDenseId { public static string Id => "Select"; }
    public sealed class TagId : IDenseId { public static string Id => "Dialog"; }

    /// <summary>A test control token whose single value is read from the global view by a test-supplied function.</summary>
    public sealed class DenseReadToken<TId>(Func<DesignToken, double> read) : DenseToken, IControlToken where TId : IDenseId
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("AtomUI", TId.Id);

        public double Height
        {
            get => field;
            set
            {
                if (!IsFrozen(nameof(Height)))
                {
                    field = value;
                }
            }
        }

        internal override void Evaluate(DesignToken global, in DesignVariant variant) => Height = read(global);
    }

    /// <summary>A test control token whose view is computed by the token itself (it carries the injected test inputs).</summary>
    public abstract class DenseToken : AbstractControlDesignToken
    {
        internal abstract void Evaluate(DesignToken global, in DesignVariant variant);
    }

    public sealed class DenseHeightToken<TId>(Action? onEvaluate) : DenseToken, IControlToken where TId : IDenseId
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("AtomUI", TId.Id);

        // Written by hand the way the generator writes a token property: a frozen (explicit) value is kept.
        public double Height
        {
            get => field;
            set
            {
                if (!IsFrozen(nameof(Height)))
                {
                    field = value;
                }
            }
        }

        public ThemeAppearance Appearance { get; set; }

        internal override void Evaluate(DesignToken global, in DesignVariant variant)
        {
            onEvaluate?.Invoke();
            Appearance = variant.Appearance;
            Height     = global.ControlHeight;
        }
    }

    public sealed class DenseValueToken<TId, TValue>(TValue value) : DenseToken, IControlToken
        where TId : IDenseId
        where TValue : class
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("AtomUI", TId.Id);

        public TValue? Value { get; set; }

        internal override void Evaluate(DesignToken global, in DesignVariant variant) => Value = value;
    }

    public sealed class DenseTokenRule<TToken> : IControlTokenRule<AntDesignToken, TToken> where TToken : DenseToken
    {
        public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<TToken> target) =>
            target.Own.Evaluate(target.Global, context.Variant);
    }

    public sealed class ProbeDenseTokenRule<TToken> : IControlTokenRule<ProbeLanguageToken, TToken> where TToken : DenseToken
    {
        public void Apply(in TokenEvaluationContext<ProbeLanguageToken> context, ControlTokenTarget<TToken> target) =>
            target.Own.Evaluate(target.Global, context.Variant);
    }
}
