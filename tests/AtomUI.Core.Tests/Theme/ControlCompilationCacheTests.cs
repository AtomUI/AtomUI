using AtomUI.Theme.Languages;
using AtomUI.DesignLanguages.AntDesign;
using System.Reflection;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using AtomUI.Theme.DesignTokens;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

public class ControlCompilationCacheTests
{
    [Fact]
    public void Control_Keys_Share_The_Precomputed_Global_Key()
    {
        var button = CreateControl<DenseThemeCompilerTests.ButtonId>();
        var input = CreateControl<DenseThemeCompilerTests.InputId>();
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([button, input], derivations: Derivations);
        registry.TryGetControl(button.Identity, out button).ShouldBeTrue();
        registry.TryGetControl(input.Identity, out input).ShouldBeTrue();
        var compileInput = TypedThemeSnapshotCacheTests.CreateInput(registry);
        var globalKey = GlobalCompilationCacheKey.Create(compileInput);

        var buttonKey = ControlCompilationCacheKey.Create(
            globalKey,
            button,
            null,
            registry.FindDerivation(TestDesignLanguages.AntDesign, button.Slot));
        var inputKey = ControlCompilationCacheKey.Create(
            globalKey,
            input,
            null,
            registry.FindDerivation(TestDesignLanguages.AntDesign, input.Slot));

        var field = typeof(ControlCompilationCacheKey).GetField(
            "_global",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.GetValue(buttonKey).ShouldBeSameAs(globalKey);
        field.GetValue(inputKey).ShouldBeSameAs(globalKey);
    }

    [Fact]
    public async Task Different_Snapshots_Reuse_Only_Equivalent_Control_Compilations()
    {
        var button = CreateControl<DenseThemeCompilerTests.ButtonId>();
        var input = CreateControl<DenseThemeCompilerTests.InputId>();
        var registry = TypedThemeSnapshotCacheTests.CreateRegistry([button, input], derivations: Derivations);
        registry.TryGetControl(button.Identity, out button).ShouldBeTrue();
        registry.TryGetControl(input.Identity, out input).ShouldBeTrue();
        var buttonConfig = new NormalizedControlThemeConfig(
            button.Identity,
            null,
            null,
            Array.Empty<NormalizedTokenValue>(),
            [CreateControlToken(button, "48")]);
        var cache = new ThemeSnapshotCache();
        var compiler = new ThemeCompiler();

        var baseline = await TypedThemeSnapshotCacheTests.CompileAsync(cache,
            TypedThemeSnapshotCacheTests.CreateInput(registry),
            compiler);
        var changed = await TypedThemeSnapshotCacheTests.CompileAsync(cache,
            TypedThemeSnapshotCacheTests.CreateInput(registry, controls: [buttonConfig]),
            compiler);

        baseline.Success.ShouldBeTrue();
        changed.Success.ShouldBeTrue();
        changed.Snapshot!.Controls[button.Slot]
               .ShouldNotBeSameAs(baseline.Snapshot!.Controls[button.Slot]);
        changed.Snapshot.Controls[input.Slot]
               .ShouldBeSameAs(baseline.Snapshot.Controls[input.Slot]);
        cache.ControlCount.ShouldBe(3);
        cache.ControlRetainedBytes.ShouldBeGreaterThan(0);
    }

    private static NormalizedTokenValue CreateControlToken(
        ControlTokenDescriptor control,
        string value)
    {
        var descriptor = control.OwnTokens.ShouldHaveSingleItem();
        var parsed = descriptor.Parse(value);
        return new NormalizedTokenValue(descriptor, parsed, descriptor.Format(parsed));
    }

    private static readonly ControlDerivationDescriptor[] Derivations =
    [
        ControlDerivationDescriptor.Create<AntDesignToken, CacheControlToken<DenseThemeCompilerTests.ButtonId>,
            CacheDerivation<DenseThemeCompilerTests.ButtonId>>(),
        ControlDerivationDescriptor.Create<AntDesignToken, CacheControlToken<DenseThemeCompilerTests.InputId>,
            CacheDerivation<DenseThemeCompilerTests.InputId>>()
    ];

    private static ControlTokenDescriptor CreateControl<TId>() where TId : DenseThemeCompilerTests.IDenseId
    {
        var token = new TokenDescriptor(
            "Height",
            0,
            TokenStage.Control,
            typeof(double),
            $"{TId.Id}.Height",
            static value => ThemeTokenValueParser.Parse<double>(value),
            static value => ThemeTokenValueFormatter.Format((double)value!),
            static builder => ((CacheControlToken<TId>)builder).Height,
            static (builder, value) => ((CacheControlToken<TId>)builder).Height = (double)value!,
            static builder => ((CacheControlToken<TId>)builder).Height);
        return new ControlTokenDescriptor(
            ThemeTestControlTypes.For("AtomUI", TId.Id),
            CacheControlToken<TId>.ControlIdentity,
            [token],
            static () => new CacheControlToken<TId>());
    }

    public sealed class CacheControlToken<TId> : AbstractControlDesignToken, IControlToken
        where TId : DenseThemeCompilerTests.IDenseId
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("AtomUI", TId.Id);

        public double Height { get; set; }
    }

    public sealed class CacheDerivation<TId> : IControlDerivation<AntDesignToken, CacheControlToken<TId>>
        where TId : DenseThemeCompilerTests.IDenseId
    {
        public void Derive(in DerivationContext<AntDesignToken> context, ControlView<CacheControlToken<TId>> view) =>
            view.Token.Height = 40;
    }
}
