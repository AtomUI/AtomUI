using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ControlTokenIdentityOwnershipTests
{
    private static ControlTokenIdentity Raw => new("Acme", "Button");
    private static ControlTokenIdentity Typed(Type owner) => ControlTokenIdentity.ForControl(owner, "Acme", "Button");

    [Fact]
    public void Typed_Identity_Has_String_Equality_And_Does_Not_Serialize_Owner()
    {
        var typed = Typed(typeof(Button));
        typed.ShouldBe(Raw);
        typed.GetHashCode().ShouldBe(Raw.GetHashCode());
        (typed == Raw).ShouldBeTrue();
        (typed != Raw).ShouldBeFalse();
        System.Text.Json.JsonSerializer.Serialize(typed).ShouldBe("{\"Catalog\":\"Acme\",\"Id\":\"Button\"}");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Builder_Preserves_Typed_Owner_In_Both_Replacement_Orders(bool typedFirst)
    {
        var builder = new ThemeConfigBuilder();
        var config = new ControlThemeConfigBuilder().Build();
        builder.WithControl(typedFirst ? Typed(typeof(Button)) : Raw, config);
        builder.WithControl(typedFirst ? Raw : Typed(typeof(Button)), config);
        var canonical = builder.Build().Controls.Keys.Single();
        canonical.OwnerType.ShouldBe(typeof(Button));
        Should.Throw<ThemeSchemaException>(() => builder.WithControl(Typed(typeof(TextBox)), config));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Registry_Lookup_And_Normalization_Reject_Wrong_Owner(bool typedFirst)
    {
        var registry = new ThemeSchemaRegistry([], [new ControlTokenDescriptor(typeof(Button), Raw)], []);
        registry.TryGetControl(Typed(typeof(TextBox)), out _).ShouldBeFalse();
        var config = new ControlThemeConfigBuilder().Build();
        var input = new ThemeConfigBuilder()
            .WithControl(typedFirst ? Typed(typeof(TextBox)) : Raw, config)
            .WithControl(typedFirst ? Raw : Typed(typeof(TextBox)), config)
            .Build();
        ThemeConfigNormalizer.Normalize(input, registry).Success.ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Merge_Preserves_Owner_Even_When_Values_Are_Equal(bool typedFirst)
    {
        var parent = Normalized(typedFirst ? Typed(typeof(Button)) : Raw);
        var current = Normalized(typedFirst ? Raw : Typed(typeof(Button)));
        var result = ThemeConfigMerger.Merge(NormalizedThemeConfig.CreateCanonical(false, true, [], [], []), parent, current);
        result.EffectiveConfig.Controls.Single().Identity.OwnerType.ShouldBe(typeof(Button));
        Should.Throw<ThemeSchemaException>(() => ThemeConfigMerger.Merge(
            NormalizedThemeConfig.CreateCanonical(false, true, [], [], []), result.EffectiveConfig, Normalized(Typed(typeof(TextBox)))));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Conflicting_Typed_Owners_Fail_In_Either_Order(bool reverse)
    {
        var first = Typed(reverse ? typeof(TextBox) : typeof(Button));
        var second = Typed(reverse ? typeof(Button) : typeof(TextBox));
        var builder = new ThemeConfigBuilder().WithControl(first, new ControlThemeConfigBuilder().Build());
        Should.Throw<ThemeSchemaException>(() => builder.WithControl(second, new ControlThemeConfigBuilder().Build()));
        Should.Throw<ThemeSchemaException>(() => ThemeConfigMerger.Merge(
            NormalizedThemeConfig.CreateCanonical(false, true, [], [], []), Normalized(first), Normalized(second)));
        Should.Throw<ThemeSchemaException>(() => new ThemeSchemaRegistry([], [new ControlTokenDescriptor(typeof(Button), Typed(typeof(TextBox)))], []));
    }

    [Fact]
    public void Schema_Revision_Uses_Full_Defining_Assembly_Identity()
    {
        var firstType = TypeInAssemblyVersion(1);
        var secondType = TypeInAssemblyVersion(2);
        firstType.FullName.ShouldBe(secondType.FullName);
        var first = new ThemeSchemaRegistry([], [new ControlTokenDescriptor(firstType, Raw)], []);
        var second = new ThemeSchemaRegistry([], [new ControlTokenDescriptor(secondType, Raw)], []);
        first.Revision.ShouldNotBe(second.Revision);
    }

    private static Type TypeInAssemblyVersion(int major)
    {
        var assembly = new System.Reflection.AssemblyName("OwnerIdentityTests") { Version = new Version(major, 0, 0, 0) };
        return System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(assembly, System.Reflection.Emit.AssemblyBuilderAccess.Run)
            .DefineDynamicModule("OwnerIdentityTests").DefineType("Acme.Button", System.Reflection.TypeAttributes.Public, typeof(Control)).CreateType()!;
    }

    private static NormalizedThemeConfig Normalized(ControlTokenIdentity identity) =>
        new(true, false, [], [], [new NormalizedControlThemeConfig(identity, ControlAlgorithmMode.Disabled, [], [], [])]);
}
