using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ControlOwnTokenExplicitOverrideTests
{
    private const string ControlId = "ExplicitTest";
    private static readonly ControlTokenIdentity s_identity = new("AtomUI", ControlId);

    [Fact]
    public void Explicit_Own_Override_Is_Visible_To_Default_Calculation()
    {
        var snapshot = Compile(Config(("Base", "7")), alwaysWriteDerived: false, out _);

        snapshot.Control<double>(s_identity, 1).ShouldBe(14);
    }

    [Fact]
    public void Explicit_Own_Override_Wins_Over_Evaluator_Write()
    {
        var snapshot = Compile(Config(("Derived", "5")), alwaysWriteDerived: true, out _);

        snapshot.Control<double>(s_identity, 1).ShouldBe(5);
    }

    [Fact]
    public void Explicit_Zero_Counts_As_Explicit()
    {
        var snapshot = Compile(Config(("Derived", "0")), alwaysWriteDerived: false, out _);

        snapshot.Control<double>(s_identity, 1).ShouldBe(0);
    }

    [Fact]
    public void Unset_Field_Is_Not_Explicit()
    {
        var snapshot = Compile(null, alwaysWriteDerived: false, out var builders);

        // Control snapshots compile on first access, which is also when the builder is created.
        snapshot.Control<double>(s_identity, 1).ShouldBe(20);
        builders.ShouldHaveSingleItem().IsExplicitSlot(1).ShouldBeFalse();
    }

    [Fact]
    public void Only_Overridden_Fields_Are_Marked_Explicit()
    {
        var snapshot = Compile(Config(("Base", "7")), alwaysWriteDerived: false, out var builders);

        snapshot.Control<double>(s_identity, 0).ShouldBe(7);
        var builder = builders.ShouldHaveSingleItem();
        builder.IsExplicitSlot(0).ShouldBeTrue();
        builder.IsExplicitSlot(1).ShouldBeFalse();
    }

    [Fact]
    public void Explicit_Bits_Grow_Beyond_The_Initial_Word_And_Can_Be_Reset()
    {
        var token = new ExplicitTestToken();

        token.IsExplicitSlot(0).ShouldBeFalse();
        token.IsExplicitSlot(200).ShouldBeFalse();
        token.IsExplicitSlot(-1).ShouldBeFalse();

        token.MarkExplicit(0);
        token.MarkExplicit(63);
        token.MarkExplicit(64);
        token.MarkExplicit(200);

        token.IsExplicitSlot(0).ShouldBeTrue();
        token.IsExplicitSlot(63).ShouldBeTrue();
        token.IsExplicitSlot(64).ShouldBeTrue();
        token.IsExplicitSlot(200).ShouldBeTrue();
        token.IsExplicitSlot(1).ShouldBeFalse();
        token.IsExplicitSlot(65).ShouldBeFalse();
        token.IsExplicitSlot(1000).ShouldBeFalse();

        token.ResetExplicit();

        token.IsExplicitSlot(0).ShouldBeFalse();
        token.IsExplicitSlot(200).ShouldBeFalse();
        Should.Throw<ArgumentOutOfRangeException>(() => token.MarkExplicit(-1));
    }

    private static ThemeConfig Config(params (string Name, string Value)[] tokens)
    {
        var control = new ControlThemeConfigBuilder();
        foreach (var (name, value) in tokens)
        {
            control.WithToken(name, value);
        }

        return new ThemeConfigBuilder().WithControl(s_identity, control.Build()).Build();
    }

    private static ThemeSnapshot Compile(
        ThemeConfig? config,
        bool alwaysWriteDerived,
        out List<ExplicitTestToken> builders)
    {
        var created = new List<ExplicitTestToken>();
        builders = created;
        return ThemeTestSnapshotFactory.Compile(config, Descriptor(alwaysWriteDerived, created));
    }

    private static ControlTokenDescriptor Descriptor(bool alwaysWriteDerived, List<ExplicitTestToken> created)
    {
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", ControlId),
            s_identity,
            [
                OwnToken(nameof(ExplicitTestToken.Base), 0,
                    static token => ((ExplicitTestToken)token).Base,
                    static (token, value) => ((ExplicitTestToken)token).Base = (double)value!),
                OwnToken(nameof(ExplicitTestToken.Derived), 1,
                    static token => ((ExplicitTestToken)token).Derived,
                    static (token, value) => ((ExplicitTestToken)token).Derived = (double)value!)
            ],
            () =>
            {
                var token = new ExplicitTestToken { AlwaysWriteDerived = alwaysWriteDerived };
                created.Add(token);
                return token;
            },
            static (token, _) => ((ExplicitTestToken)token).Evaluate());
    }

    private static TokenDescriptor OwnToken(
        string name,
        int slot,
        Func<AbstractDesignToken, object?> getter,
        Action<AbstractDesignToken, object?> setter)
    {
        return new TokenDescriptor(
            name,
            slot,
            TokenStage.Control,
            typeof(double),
            $"{ControlId}.{name}",
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            getter,
            setter,
            getter);
    }

    private sealed class ExplicitTestToken : AbstractControlDesignToken
    {
        public double Base { get; set; }
        public double Derived { get; set; }
        public bool AlwaysWriteDerived { get; init; }

        internal void Evaluate()
        {
            if (!IsExplicit(ExplicitTestTokenKind.Base))
            {
                Base = 10;
            }

            if (AlwaysWriteDerived)
            {
                // Simulates an evaluator that ignores the explicit bit: the compiler must still let the override win.
                Derived = 99;
            }
            else if (!IsExplicit(ExplicitTestTokenKind.Derived))
            {
                Derived = Base * 2;
            }
        }
    }

    private enum ExplicitTestTokenKind
    {
        Base,
        Derived
    }
}
