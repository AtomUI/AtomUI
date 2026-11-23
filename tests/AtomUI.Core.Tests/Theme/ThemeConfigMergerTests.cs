using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

/// <summary>Layering of scope configurations (spec 7.6, 7.7): axes and values of a layer win over the layer below.</summary>
public class ThemeConfigMergerTests
{
    [Fact]
    public void Merge_Inherits_Parent_And_Applies_Local_Precedence()
    {
        var schema = ThemeConfigTestSchema.Create();
        var defaults = Normalize(schema, false, b => b.WithLanguage("Probe"), ("Alpha", "1"));
        var parent = Normalize(schema, true, b => b.WithDensity(DesignDensity.Compact), ("Alpha", "2"), ("Beta", "3"));
        var local = Normalize(schema, true, b => b.WithAppearance(ThemeAppearance.Dark), ("Beta", "4"));

        var withParent = ThemeConfigMerger.Merge(defaults, null, parent).EffectiveConfig;
        var result = ThemeConfigMerger.Merge(defaults, withParent, local);

        result.EffectiveConfig.Language.ShouldNotBeNull().Id.ShouldBe("Probe");
        result.EffectiveConfig.Axes.Density.ShouldBe(DesignDensity.Compact);
        result.EffectiveConfig.Axes.Appearance.ShouldBe(ThemeAppearance.Dark);
        Value(result.EffectiveConfig, "Alpha").ShouldBe(2d);
        Value(result.EffectiveConfig, "Beta").ShouldBe(4d);
        result.ChangeSet.LanguageInputsChanged.ShouldBeTrue();
        result.ChangeSet.GlobalTokensChanged.ShouldBeTrue();
    }

    [Fact]
    public void Merge_Inherit_False_Keeps_The_Language_But_Restarts_Axes_And_Values_From_Defaults()
    {
        var schema = ThemeConfigTestSchema.Create();
        var defaults = Normalize(schema, false, null, ("Alpha", "1"));
        var parent = ThemeConfigMerger.Merge(defaults, null,
            Normalize(schema, true, b => b.WithLanguage("Probe").WithDensity(DesignDensity.Compact), ("Alpha", "2"),
                ("Beta", "3"))).EffectiveConfig;
        var local = Normalize(schema, false, null, ("Beta", "4"));

        var result = ThemeConfigMerger.Merge(defaults, parent, local);

        // Inherit = false keeps the parent's language (spec 7.7) and drops its density and values.
        result.EffectiveConfig.Language.ShouldNotBeNull().Id.ShouldBe("Probe");
        result.EffectiveConfig.Axes.Density.ShouldBeNull();
        Value(result.EffectiveConfig, "Alpha").ShouldBe(1d);
        Value(result.EffectiveConfig, "Beta").ShouldBe(4d);
        result.ChangeSet.LanguageInputsChanged.ShouldBeTrue();
        result.ChangeSet.GlobalTokensChanged.ShouldBeTrue();
    }

    [Fact]
    public void Merge_Values_Only_Leaves_The_Language_Inputs_Unchanged()
    {
        var schema = ThemeConfigTestSchema.Create();
        var defaults = Normalize(schema, false, b => b.WithLanguage("Probe"));
        var local = Normalize(schema, true, null, ("Beta", "4"));

        var result = ThemeConfigMerger.Merge(defaults, null, local);

        result.ChangeSet.LanguageInputsChanged.ShouldBeFalse();
        result.ChangeSet.GlobalTokensChanged.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(ThemeAppearance.Dark, null, ThemeAppearance.Dark)]
    [InlineData(null, ThemeAppearance.Light, ThemeAppearance.Light)]
    [InlineData(ThemeAppearance.Dark, ThemeAppearance.Light, ThemeAppearance.Light)]
    public void Merge_Resolves_The_Control_Axes_Layer_By_Layer(
        ThemeAppearance? parentAppearance,
        ThemeAppearance? localAppearance,
        ThemeAppearance? expected)
    {
        var schema = ThemeConfigTestSchema.Create();
        var defaults = Normalize(schema, false, b => b.WithLanguage("Probe"));
        var parent = ThemeConfigMerger.Merge(defaults, null,
            NormalizeControl(schema, true, parentAppearance, DesignDensity.Compact, "20")).EffectiveConfig;
        var local = NormalizeControl(schema, true, localAppearance, null, "32");

        var result = ThemeConfigMerger.Merge(defaults, parent, local);

        var button = result.EffectiveConfig.Controls.Single();
        button.Appearance.ShouldBe(expected);
        button.Density.ShouldBe(DesignDensity.Compact);
        button.OwnTokens.Single(item => item.Descriptor.Name == "Height").Value.ShouldBe(32d);
    }

    [Fact]
    public void Merge_Produces_Deterministic_Changed_Control_Order_And_NoOp_Result()
    {
        var schema = ThemeConfigTestSchema.Create();
        var defaults = Normalize(schema, false, b => b.WithLanguage("Probe"));
        var parent = NormalizeControl(schema, true, null, null, "20");
        var local = NormalizeControl(schema, true, null, null, "32");

        var changed = ThemeConfigMerger.Merge(defaults, parent, local);
        var noOpConfig = Normalize(new ThemeConfigBuilder().WithInherit(true).Build(), schema);
        var noOp = ThemeConfigMerger.Merge(defaults, changed.EffectiveConfig, noOpConfig);

        changed.ChangeSet.ChangedControls.ShouldBe([ThemeConfigTestSchema.ButtonIdentity]);
        changed.ChangeSet.IsEmpty.ShouldBeFalse();
        noOp.ChangeSet.IsEmpty.ShouldBeTrue();
        noOp.EffectiveConfig.ShouldBe(changed.EffectiveConfig);
        noOp.EffectiveConfig.ShouldBeSameAs(changed.EffectiveConfig);
        noOp.EffectiveConfig.Fingerprint.ShouldBe(changed.EffectiveConfig.Fingerprint);
    }

    [Fact]
    public void Merge_Drops_Lower_Language_Tokens_When_A_Layer_Switches_The_Language()
    {
        var schema = new ThemeSchemaRegistry(ThemeConfigTestSchema.Create().GlobalTokens, [],
            [TestDesignLanguages.Probe(),
                new DesignLanguageDescriptor(new ChainLanguage<ChildProbeToken>("Child"), Array.Empty<TokenDescriptor>())]);
        var defaults = Normalize(schema, false, b => b.WithLanguage("Probe").WithLanguageToken("Radius", "4"));
        var local = Normalize(schema, true, b => b.WithLanguage("Child").WithLanguageToken("Spacing", "8"));

        var result = ThemeConfigMerger.Merge(defaults, null, local);

        result.EffectiveConfig.LanguageTokens.Select(static token => token.Name).ShouldBe(["Spacing"]);
    }

    private static NormalizedThemeConfig Normalize(
        ThemeSchemaRegistry schema,
        bool inherit,
        Action<ThemeConfigBuilder>? axes,
        params (string Name, string Value)[] tokens)
    {
        var builder = new ThemeConfigBuilder().WithInherit(inherit);
        axes?.Invoke(builder);
        foreach (var token in tokens)
        {
            builder.WithToken(token.Name, token.Value);
        }

        return Normalize(builder.Build(), schema);
    }

    private static NormalizedThemeConfig NormalizeControl(
        ThemeSchemaRegistry schema,
        bool inherit,
        ThemeAppearance? appearance,
        DesignDensity? density,
        string height)
    {
        var controlBuilder = new ControlThemeConfigBuilder().WithToken("Height", height);
        if (appearance is { } value)
        {
            controlBuilder.WithAppearance(value);
        }
        if (density is { } controlDensity)
        {
            controlBuilder.WithDensity(controlDensity);
        }

        var config = new ThemeConfigBuilder()
                     .WithInherit(inherit)
                     .WithControl(ThemeConfigTestSchema.ButtonIdentity, controlBuilder.Build())
                     .Build();
        return Normalize(config, schema);
    }

    private static NormalizedThemeConfig Normalize(ThemeConfig config, ThemeSchemaRegistry schema)
    {
        return ThemeConfigNormalizer.Normalize(config, schema).Config.ShouldNotBeNull();
    }

    private static double Value(NormalizedThemeConfig config, string name)
    {
        return (double)config.GlobalTokens.Single(item => item.Descriptor.Name == name).Value!;
    }
}
