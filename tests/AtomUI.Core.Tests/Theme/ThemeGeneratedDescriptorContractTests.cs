using System.Globalization;
using AtomUI.Generated.AtomUICore;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ThemeGeneratedDescriptorContractTests
{

    [Fact]
    public void Descriptor_Preserves_Identity_And_Uses_Direct_Factory()
    {
        var descriptor = Control("Button", "Height");

        descriptor.Identity.ShouldBe(new ControlTokenIdentity("AtomUI", "Button"));
        descriptor.CreateBuilder().ShouldBeOfType<SchemaControlToken>();
    }

    [Fact]
    public void Token_Descriptor_Uses_Typed_Delegates_For_Parse_Access_And_Projection()
    {
        var descriptor = NumberToken("Scale", 0, TokenStage.Seed);
        var token = new SchemaDesignToken();

        descriptor.Parse("1.5").ShouldBe(1.5);
        descriptor.SetValue(token, 2.5);

        token.Scale.ShouldBe(2.5);
        descriptor.GetValue(token).ShouldBe(2.5);
        descriptor.ProjectResourceValue(token).ShouldBe(2.5);
        descriptor.ResourceKey.ShouldBe(SchemaResourceKey.Scale);
    }

    [Fact]
    public void Generated_Global_Descriptors_Read_Write_Format_And_Project_Their_Own_Property()
    {
        var snapshot = ThemeTestSnapshotFactory.Compile();
        var descriptors = GeneratedThemeSchema.GetGlobalTokens();
        descriptors.Count.ShouldBeGreaterThan(200);

        foreach (var descriptor in descriptors)
        {
            var property = typeof(DesignToken).GetProperty(descriptor.Name).ShouldNotBeNull(descriptor.Name);
            property.PropertyType.ShouldBe(descriptor.ValueType, descriptor.Name);
            var compiled = snapshot.GlobalTokenValues.GetValue(descriptor.Slot);
            var token = new DesignToken();

            descriptor.SetValue(token, compiled);

            property.GetValue(token).ShouldBe(compiled, descriptor.Name);
            descriptor.GetValue(token).ShouldBe(compiled, descriptor.Name);
            var text = descriptor.Format(compiled);
            text.ShouldBe(InvokeShared(typeof(ThemeTokenValueFormatter), "Format", descriptor.ValueType, compiled), descriptor.Name);
            Outcome(() => descriptor.Parse(text)).ShouldBe(
                Outcome(() => InvokeShared(typeof(ThemeTokenValueParser), "Parse", descriptor.ValueType, text)), descriptor.Name);
            var projected = descriptor.ProjectResourceValue(token);
            if (compiled is Color color)
            {
                projected.ShouldBeOfType<ImmutableSolidColorBrush>(descriptor.Name).Color.ShouldBe(color, descriptor.Name);
            }
            else
            {
                projected.ShouldBe(compiled, descriptor.Name);
            }
        }
    }

    [Fact]
    public void Generated_Descriptor_Dispatches_Its_Slot_To_The_Shared_Accessor_And_Value_Codec()
    {
        var first = TokenDescriptor.CreateGenerated(
            "First", 0, TokenStage.Seed, TokenValueCodec<double>.Instance, SchemaResourceKey.Scale, PairAccessor.Instance);
        var second = TokenDescriptor.CreateGenerated(
            "Second", 1, TokenStage.Seed, TokenValueCodec<Color>.Instance, "Second", PairAccessor.Instance);
        var token = new PairToken();

        first.ValueType.ShouldBe(typeof(double));
        second.ValueType.ShouldBe(typeof(Color));
        first.Parse("1.5").ShouldBe(1.5);
        second.Format(Color.Parse("#1677ff")).ShouldBe(ThemeTokenValueFormatter.Format(Color.Parse("#1677ff")));
        first.SetValue(token, 2.5);
        second.SetValue(token, Color.Parse("#ff0000"));

        token.First.ShouldBe(2.5);
        token.Second.ShouldBe(Color.Parse("#ff0000"));
        first.GetValue(token).ShouldBe(2.5);
        second.GetValue(token).ShouldBe(Color.Parse("#ff0000"));
        first.ProjectResourceValue(token).ShouldBe(2.5);
        second.ProjectResourceValue(token).ShouldBeOfType<ImmutableSolidColorBrush>().Color.ShouldBe(Color.Parse("#ff0000"));
        first.ResourceKey.ShouldBe(SchemaResourceKey.Scale);
        Should.Throw<ArgumentNullException>(() => first.GetValue(null!));
    }

    [Fact]
    public void Registry_Assigns_Deterministic_Control_Slots_And_Shares_Global_Schema()
    {
        var global = NumberToken("Scale", 0, TokenStage.Seed);
        var input = Control("Input", "InputValue");
        var button = Control("Button", "Height");

        var registry = new ThemeSchemaRegistry(
            [global],
            [input, button],
            Array.Empty<ThemeAlgorithmDescriptor>());

        registry.Controls.Select(static descriptor => descriptor.Identity.Id).ShouldBe(["Button", "Input"]);
        registry.Controls.Select(static descriptor => descriptor.Slot).ShouldBe([0, 1]);
        registry.Controls.ShouldAllBe(descriptor =>
            descriptor.ControlType == ThemeTestControlTypes.For("AtomUI", descriptor.Identity.Id));
        registry.TryGetControl(new ControlTokenIdentity("AtomUI", "Button"), out var boundButton).ShouldBeTrue();
        boundButton.ShouldNotBeSameAs(button);

        var builder = boundButton!.CreateBuilder().ShouldBeOfType<SchemaControlToken>();
        boundButton.Evaluate(builder, ThemeAppearance.Dark);
        builder.EvaluatedAppearance.ShouldBe(ThemeAppearance.Dark);
    }

    [Fact]
    public void Algorithm_Descriptor_Uses_Revisioned_Independent_Factory_And_Exact_Evaluate_Contract()
    {
        var descriptor = new ThemeAlgorithmDescriptor(
            ThemeAlgorithm.Default,
            revision: 3,
            ThemeAppearanceEffect.Light,
            static () => new SchemaAlgorithm());
        var effectiveSeed = new DesignToken();
        var previousMap = new DesignToken();
        var nextMap = new DesignToken();

        var algorithm = descriptor.Create().ShouldBeOfType<SchemaAlgorithm>();
        algorithm.Evaluate(effectiveSeed, previousMap, nextMap);

        descriptor.Revision.ShouldBe(3);
        descriptor.Algorithm.ShouldBe(ThemeAlgorithm.Default);
        algorithm.EffectiveSeed.ShouldBeSameAs(effectiveSeed);
        algorithm.PreviousMap.ShouldBeSameAs(previousMap);
        algorithm.NextMap.ShouldBeSameAs(nextMap);
        Should.Throw<ArgumentOutOfRangeException>(() => new ThemeAlgorithmDescriptor(
            ThemeAlgorithm.Default,
            revision: 0,
            ThemeAppearanceEffect.Preserve,
            static () => new SchemaAlgorithm()));
    }

    [Fact]
    public void Algorithm_Attribute_Uses_A_Defined_Enum_Identity()
    {
        var attribute = new ThemeAlgorithmAttribute(
            ThemeAlgorithm.Dark,
            revision: 2,
            ThemeAppearanceEffect.Dark);

        attribute.Algorithm.ShouldBe(ThemeAlgorithm.Dark);
        Should.Throw<ArgumentOutOfRangeException>(() => new ThemeAlgorithmAttribute(
            (ThemeAlgorithm)999,
            revision: 1,
            ThemeAppearanceEffect.Preserve));
    }

    [Fact]
    public void Shared_Parser_And_Resource_Projector_Are_Invariant_And_Immutable()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            ThemeTokenValueParser.Parse<double>("1.5").ShouldBe(1.5);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }

        var color = Color.Parse("#1677ff");
        ThemeResourceValue.Project(color)
                          .ShouldBeOfType<ImmutableSolidColorBrush>()
                          .Color
                          .ShouldBe(color);
    }

    private static object? InvokeShared(Type helper, string method, Type valueType, object? argument) =>
        helper.GetMethod(method)!.MakeGenericMethod(valueType).Invoke(null, [argument]);

    // Compares either the produced value or the type of the thrown exception.
    private static object? Outcome(Func<object?> action)
    {
        try
        {
            return action();
        }
        catch (Exception exception)
        {
            return (exception is System.Reflection.TargetInvocationException { InnerException: { } inner } ? inner : exception).GetType();
        }
    }

    private static TokenDescriptor NumberToken(string name, int slot, TokenStage stage)
    {
        return new TokenDescriptor(
            name,
            slot,
            stage,
            typeof(double),
            SchemaResourceKey.Scale,
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static token => ((SchemaDesignToken)token).Scale,
            static (token, value) => ((SchemaDesignToken)token).Scale = (double)value!,
            static token => ThemeResourceValue.Project(((SchemaDesignToken)token).Scale));
    }

    private static ControlTokenDescriptor Control(string id, string tokenName)
    {
        var ownToken = new TokenDescriptor(
            tokenName,
            0,
            TokenStage.Control,
            typeof(double),
            $"{id}.{tokenName}",
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static token => ((SchemaControlToken)token).Value,
            static (token, value) => ((SchemaControlToken)token).Value = (double)value!,
            static token => ThemeResourceValue.Project(((SchemaControlToken)token).Value));
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", id),
            new ControlTokenIdentity("AtomUI", id),
            [ownToken],
            static () => new SchemaControlToken(),
            static (token, appearance) => ((SchemaControlToken)token).EvaluatedAppearance = appearance);
    }

    private enum SchemaResourceKey
    {
        Scale
    }

    private sealed class SchemaDesignToken : AbstractDesignToken
    {
        public double Scale { get; set; }
    }

    private sealed class PairToken : AbstractDesignToken
    {
        public double First { get; set; }
        public Color Second { get; set; }
    }

    private sealed class PairAccessor : TokenValueAccessor
    {
        public static readonly PairAccessor Instance = new();

        public override object? GetValue(AbstractDesignToken token, int slot) => slot switch
        {
            0 => ((PairToken)token).First,
            1 => ((PairToken)token).Second,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };

        public override void SetValue(AbstractDesignToken token, int slot, object? value)
        {
            switch (slot)
            {
                case 0: ((PairToken)token).First = (double)value!; break;
                case 1: ((PairToken)token).Second = (Color)value!; break;
                default: throw new ArgumentOutOfRangeException(nameof(slot));
            }
        }

        public override object? ProjectResourceValue(AbstractDesignToken token, int slot) => slot switch
        {
            0 => ThemeResourceValue.Project(((PairToken)token).First),
            1 => ThemeResourceValue.Project(((PairToken)token).Second),
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };
    }

    private sealed class SchemaControlToken : AbstractControlDesignToken
    {
        public double Value { get; set; }
        public ThemeAppearance EvaluatedAppearance { get; set; }
    }

    private sealed class SchemaControl : Control
    {
    }

    private sealed class SchemaAlgorithm : IThemeAlgorithm
    {
        public DesignToken? EffectiveSeed { get; private set; }
        public DesignToken? PreviousMap { get; private set; }
        public DesignToken? NextMap { get; private set; }

        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
            EffectiveSeed = effectiveSeed;
            PreviousMap   = previousMap;
            NextMap       = nextMap;
        }
    }
}
