using AtomUI.Generated.AtomUICore;
using AtomUI.Registration;
using AtomUI.Startup.Data;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ControlTokenAlgorithmRegistryTests
{
    private static readonly ThemeAlgorithm Flat = new("Test", "Flat");
    private static readonly ThemeAlgorithm Rounded = new("Test", "Rounded");

    [Fact]
    public void Registered_Control_Algorithm_Is_Found_By_Algorithm_And_Control()
    {
        var descriptor = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 3);

        descriptor.Algorithm.ShouldBe(Flat);
        descriptor.Control.ShouldBe(ProbeToken.ControlIdentity);
        descriptor.Revision.ShouldBe(3);

        var registry = Registry([descriptor]);

        registry.ControlAlgorithms.ShouldBe([descriptor]);
        registry.TryGetControlAlgorithm(Flat, ProbeToken.ControlIdentity, out var found).ShouldBeTrue();
        found.ShouldBeSameAs(descriptor);
        registry.TryGetControlAlgorithm(Rounded, ProbeToken.ControlIdentity, out _).ShouldBeFalse();
        registry.TryGetControlAlgorithm(Flat, OtherProbeToken.ControlIdentity, out _).ShouldBeFalse();

        // The descriptor holds one algorithm instance and dispatches both stages to it with the typed token.
        var token = new ProbeToken();
        var global = new DesignToken();
        var context = new ThemeAlgorithmContext(ThemeAppearance.Dark, [Flat]);
        found.EvaluateGlobal(in context, global);
        found.EvaluateOwn(in context, global, token);
        found.EvaluateOwn(in context, global, token);
        global.ColorPrimary.ShouldBe(Colors.Red);
        token.Calls.ShouldBe(["Own:Dark", "Own:Dark"]);
        token.Instances.Distinct().ShouldHaveSingleItem();
        Should.Throw<ArgumentException>(() => found.EvaluateOwn(in context, global, new OtherProbeToken()));

        // The builder channel carries the same instance into the startup registry.
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddControlTokenAlgorithms([descriptor]).ShouldBeSameAs(builder);
            var manager = builder.Build();

            manager.InitializeApplication(Application.Current!);

            manager.CurrentSnapshot!.Registry.TryGetControlAlgorithm(Flat, ProbeToken.ControlIdentity, out var registered)
                   .ShouldBeTrue();
            registered.ShouldBeSameAs(descriptor);
        });
    }

    [Fact]
    public void Duplicate_Control_Algorithm_Is_Rejected()
    {
        var first = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 1);
        var second = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 2);

        Should.Throw<ThemeSchemaException>(() => Registry([first, second]))
              .Message.ShouldBe("Duplicate control algorithm 'Test:Flat' for control 'Test:Probe'.");
        // The same control under another algorithm, and another control under the same algorithm, are distinct.
        Should.NotThrow(() => Registry([
            first,
            ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Rounded, 1),
            ControlTokenAlgorithmDescriptor.Create<OtherProbeToken, OtherProbeAlgorithm>(Flat, 1)
        ]));

        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder(Application.Current!.GetType());
            builder.AddControlTokenAlgorithms([first]);
            builder.AddControlTokenAlgorithms([second]);

            Should.Throw<ThemeSchemaException>(() => builder.Build())
                  .Message.ShouldBe("Duplicate control algorithm 'Test:Flat' for control 'Test:Probe'.");
        });
    }

    [Fact]
    public void Control_Algorithm_For_An_Unregistered_Control_Is_Kept_But_Not_Resolvable_For_Other_Controls()
    {
        // Neither the algorithm 'Test:Flat' nor the control 'Test:Probe' is registered in this registry.
        var descriptor = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 1);
        var registry = Registry([descriptor]);

        registry.TryGetAlgorithm(Flat, out _).ShouldBeFalse();
        registry.TryGetControl(ProbeToken.ControlIdentity, out _).ShouldBeFalse();
        registry.ControlAlgorithms.ShouldBe([descriptor]);
        registry.TryGetControlAlgorithm(Flat, ProbeToken.ControlIdentity, out var kept).ShouldBeTrue();
        kept.ShouldBeSameAs(descriptor);
        registry.TryGetControlAlgorithm(Flat, OtherProbeToken.ControlIdentity, out _).ShouldBeFalse();
        registry.TryGetControlAlgorithm(Flat, new ControlTokenIdentity("AtomUI", "Button"), out _).ShouldBeFalse();
    }

    [Fact]
    public void Registry_Revision_Changes_With_Control_Algorithm_Revision_And_Is_Order_Independent()
    {
        var probe = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 1);
        var other = ControlTokenAlgorithmDescriptor.Create<OtherProbeToken, OtherProbeAlgorithm>(Rounded, 1);
        var probeRevision2 = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 2);
        var probeUnderRounded = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Rounded, 1);

        var first = Registry([probe, other]);
        var reordered = Registry([other, probe]);

        reordered.Revision.ShouldBe(first.Revision);
        reordered.ControlAlgorithms.ShouldBe([probe, other]);
        Registry([probeRevision2, other]).Revision.ShouldNotBe(first.Revision);
        Registry([probeUnderRounded, other]).Revision.ShouldNotBe(first.Revision);
        Registry([probe]).Revision.ShouldNotBe(first.Revision);
        // Pins cross-process stability: identities are folded through the deterministic fingerprint builder.
        first.Revision.Value.ShouldBe(0x3C27BB46DFCE54C8UL);

        // With no control algorithm registered the revision is bit-for-bit the existing one on every path.
        Registry(null).Revision.Value.ShouldBe(0x29D0F9BA9FF24C7FUL);
        Registry([]).Revision.Value.ShouldBe(0x29D0F9BA9FF24C7FUL);
        Registry([probe]).Revision.ShouldNotBe(Registry([]).Revision);

        var globals = GeneratedThemeSchema.GetGlobalTokens();
        var core = GeneratedThemeSchema.GetAlgorithms();
        var noControls = Array.Empty<ControlTokenDescriptor>();
        new ThemeSchemaRegistry(globals, noControls, core, null, GeneratedThemeSchema.GlobalSchemaFingerprint, [], [])
            .Revision.ShouldBe(new ThemeSchemaRegistry(globals, noControls, core, null,
                GeneratedThemeSchema.GlobalSchemaFingerprint, []).Revision);
        new ThemeSchemaRegistry(globals, noControls, core, null, GeneratedThemeSchema.GlobalSchemaFingerprint, [], [probe])
            .Revision.ShouldNotBe(new ThemeSchemaRegistry(globals, noControls, core, null,
                GeneratedThemeSchema.GlobalSchemaFingerprint, []).Revision);
        var stamped = StartupPackageIndex.Create([], new StartupImage(StartupManifestCodec.Version, 0x1234UL, [], []));
        new ThemeSchemaRegistry(globals, stamped, core, []).Revision.ShouldBe(new ThemeSchemaRevision(0x1234UL));
        new ThemeSchemaRegistry(globals, stamped, core, [probe, other]).Revision
            .ShouldBe(new ThemeSchemaRegistry(globals, stamped, core, [other, probe]).Revision);
        new ThemeSchemaRegistry(globals, stamped, core, [probe]).Revision.ShouldNotBe(new ThemeSchemaRevision(0x1234UL));
        new ThemeSchemaRegistry(globals, stamped, core, [probeRevision2]).Revision
            .ShouldNotBe(new ThemeSchemaRegistry(globals, stamped, core, [probe]).Revision);
    }

    [Fact]
    public void Append_Accepts_A_Control_Algorithm_For_A_Newly_Added_Algorithm()
    {
        HeadlessTestApp.Run(() =>
        {
            using var scope = CreateScopeWithProbeControl();
            var flatProbe = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 1);

            // 'Test:Probe' is already registered; 'Test:Flat' arrives with the same append.
            scope.LoadPackages(builder => builder.Theme.AddThemeAlgorithms([PackageAlgorithm(Flat)])
                                                 .AddControlTokenAlgorithms([flatProbe]));

            var registry = scope.ThemeManager.CurrentSnapshot!.Registry;
            registry.TryGetAlgorithm(Flat, out _).ShouldBeTrue();
            registry.TryGetControlAlgorithm(Flat, ProbeToken.ControlIdentity, out var accepted).ShouldBeTrue();
            accepted.ShouldBeSameAs(flatProbe);

            // A committed control stage is carried forward by later appends, not validated again as new.
            var roundedProbe = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Rounded, 1);
            scope.LoadPackages(builder => builder.Theme.AddThemeAlgorithms([PackageAlgorithm(Rounded)])
                                                 .AddControlTokenAlgorithms([roundedProbe]));

            registry = scope.ThemeManager.CurrentSnapshot!.Registry;
            registry.ControlAlgorithms.ShouldBe([flatProbe, roundedProbe], ignoreOrder: true);
            registry.TryGetControlAlgorithm(Flat, ProbeToken.ControlIdentity, out var carried).ShouldBeTrue();
            carried.ShouldBeSameAs(flatProbe);
        });
    }

    [Fact]
    public void Append_Accepts_A_Control_Algorithm_For_A_Newly_Added_Control()
    {
        HeadlessTestApp.Run(() =>
        {
            using var scope = CreateScopeWithProbeControl();
            var defaultOther = ControlTokenAlgorithmDescriptor.Create<OtherProbeToken, OtherProbeAlgorithm>(
                ThemeAlgorithm.Default, 1);

            // 'AtomUI:Default' is already registered; 'Test:Other' arrives with the same append.
            scope.LoadPackages(builder =>
            {
                builder.Theme.AddControlPackage(ControlPackage("Tests.OtherProbe", OtherProbeToken.ControlIdentity,
                    typeof(OtherProbeControl)));
                builder.Theme.AddControlTokenAlgorithms([defaultOther]);
            });

            var registry = scope.ThemeManager.CurrentSnapshot!.Registry;
            registry.TryGetControl(OtherProbeToken.ControlIdentity, out _).ShouldBeTrue();
            registry.TryGetControlAlgorithm(ThemeAlgorithm.Default, OtherProbeToken.ControlIdentity, out var accepted)
                    .ShouldBeTrue();
            accepted.ShouldBeSameAs(defaultOther);
        });
    }

    [Fact]
    public void Append_Rejects_A_Control_Algorithm_For_An_Existing_Algorithm_And_Existing_Control()
    {
        HeadlessTestApp.Run(() =>
        {
            using var scope = CreateScopeWithProbeControl();
            var defaultProbe = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(ThemeAlgorithm.Default, 1);

            // An append that carries nothing but the control stage still reaches validation.
            Should.Throw<ThemeLoadException>(() =>
                      scope.LoadPackages(builder => builder.Theme.AddControlTokenAlgorithms([defaultProbe])))
                  .Message.ShouldBe("Package append cannot add a control algorithm for the already registered " +
                                    "algorithm 'AtomUI:Default' and control 'Test:Probe'.");
            scope.ThemeManager.CurrentSnapshot!.Registry
                 .TryGetControlAlgorithm(ThemeAlgorithm.Default, ProbeToken.ControlIdentity, out _).ShouldBeFalse();
        });
    }

    [Fact]
    public void Append_Accepts_A_Dormant_Control_Algorithm_Whose_Algorithm_Or_Control_Is_Not_Registered()
    {
        HeadlessTestApp.Run(() =>
        {
            // Like at startup, a stage whose algorithm or control is registered nowhere cannot be called, so it
            // cannot change committed values; it is kept until a later append registers the missing side.
            using var scope = CreateScopeWithProbeControl();
            var unregisteredAlgorithm = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 1);
            var unregisteredControl = ControlTokenAlgorithmDescriptor.Create<OtherProbeToken, OtherProbeAlgorithm>(
                ThemeAlgorithm.Default, 1);

            scope.LoadPackages(builder => builder.Theme.AddControlTokenAlgorithms([unregisteredAlgorithm, unregisteredControl]));

            var registry = scope.ThemeManager.CurrentSnapshot!.Registry;
            registry.TryGetAlgorithm(Flat, out _).ShouldBeFalse();
            registry.TryGetControl(OtherProbeToken.ControlIdentity, out _).ShouldBeFalse();
            registry.ControlAlgorithms.ShouldBe([unregisteredAlgorithm, unregisteredControl], ignoreOrder: true);

            // The later append that registers the missing control is the one that makes the stage callable.
            scope.LoadPackages(builder => builder.Theme.AddControlPackage(ControlPackage("Tests.OtherProbe",
                OtherProbeToken.ControlIdentity, typeof(OtherProbeControl))));
            scope.ThemeManager.CurrentSnapshot!.Registry
                 .TryGetControlAlgorithm(ThemeAlgorithm.Default, OtherProbeToken.ControlIdentity, out var kept)
                 .ShouldBeTrue();
            kept.ShouldBeSameAs(unregisteredControl);
        });
    }

    [Fact]
    public void Rejected_Append_Leaves_The_Committed_Registry_And_Snapshot_Untouched()
    {
        HeadlessTestApp.Run(() =>
        {
            using var scope = CreateScopeWithProbeControl();
            var manager = scope.ThemeManager;
            var snapshot = manager.CurrentSnapshot!;
            var registry = snapshot.Registry;
            var controlAlgorithms = registry.ControlAlgorithms;

            // The acceptable parts of the append (a new algorithm with its stage) do not rescue the rejected one.
            Should.Throw<ThemeLoadException>(() => scope.LoadPackages(builder => builder.Theme
                .AddThemeAlgorithms([PackageAlgorithm(Flat)])
                .AddControlTokenAlgorithms([
                    ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 1),
                    ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(ThemeAlgorithm.Dark, 1)
                ])));

            manager.CurrentSnapshot.ShouldBeSameAs(snapshot);
            manager.CurrentSnapshot!.Registry.ShouldBeSameAs(registry);
            registry.ControlAlgorithms.ShouldBeSameAs(controlAlgorithms);
            registry.TryGetAlgorithm(Flat, out _).ShouldBeFalse();
            registry.TryGetControlAlgorithm(Flat, ProbeToken.ControlIdentity, out _).ShouldBeFalse();

            // The manager is idle again: the same append without the rejected stage succeeds.
            var flatProbe = ControlTokenAlgorithmDescriptor.Create<ProbeToken, ProbeAlgorithm>(Flat, 1);
            scope.LoadPackages(builder => builder.Theme.AddThemeAlgorithms([PackageAlgorithm(Flat)])
                                                 .AddControlTokenAlgorithms([flatProbe]));
            manager.CurrentSnapshot!.Registry.TryGetControlAlgorithm(Flat, ProbeToken.ControlIdentity, out _)
                   .ShouldBeTrue();
        });
    }

    private static ApplicationScope CreateScopeWithProbeControl()
    {
        var application = Application.Current!;
        var builder = new AtomUIBuilder(application.GetType());
        builder.Theme.AddControlPackage(ControlPackage("Tests.Probe", ProbeToken.ControlIdentity, typeof(ProbeControl)));
        var scope = new ApplicationScope(application, builder.ThemeManagerBuilder.Build(),
            builder.LocalizationBuilder.Build(), [], new StartupProcess());
        scope.InitializeApplication();
        scope.ReleaseStartupInputs();
        scope.ThemeManager.CurrentSnapshot!.Registry.TryGetControl(ProbeToken.ControlIdentity, out _).ShouldBeTrue();
        return scope;
    }

    private static ControlPackageRegistration ControlPackage(string id, ControlTokenIdentity control, Type controlType)
    {
        var provider = new ProbeControlThemesProvider(id);
        return new ControlPackageRegistration(id, [new ControlTokenDescriptor(controlType, control)], [], [], provider, []);
    }

    private static ThemeAlgorithmDescriptor PackageAlgorithm(ThemeAlgorithm algorithm) =>
        new(algorithm, 1, ThemeAppearanceEffect.Light, static () => new NoOpThemeAlgorithm());

    private sealed class NoOpThemeAlgorithm : IThemeAlgorithm
    {
        public void Evaluate(DesignToken effectiveSeed, DesignToken? previousMap, DesignToken nextMap)
        {
        }
    }

    private sealed class ProbeControlThemesProvider : ControlThemesProvider
    {
        internal ProbeControlThemesProvider(string id)
        {
            Id = id;
        }
    }

    private sealed class ProbeControl : Control
    {
    }

    private sealed class OtherProbeControl : Control
    {
    }

    // Same inputs as ThemeSchemaRegistryTests.Registry_Revision_Is_Deterministic_For_Algorithm_Identities, whose
    // pinned revision a registry without control algorithms must keep.
    private static ThemeSchemaRegistry Registry(IEnumerable<ControlTokenAlgorithmDescriptor>? controlAlgorithms) =>
        new(Array.Empty<TokenDescriptor>(),
            Array.Empty<ControlTokenDescriptor>(),
            [
                Algorithm(ThemeAlgorithm.Default, ThemeAppearanceEffect.Light),
                Algorithm(ThemeAlgorithm.Dark, ThemeAppearanceEffect.Dark),
                Algorithm(new ThemeAlgorithm("PrimeNG", "Aura"), ThemeAppearanceEffect.Light)
            ],
            null,
            controlAlgorithms);

    private static ThemeAlgorithmDescriptor Algorithm(ThemeAlgorithm algorithm, ThemeAppearanceEffect effect) =>
        new(algorithm, 1, effect, static () => throw new InvalidOperationException());

    public sealed class ProbeToken : AbstractControlDesignToken, IControlToken
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("Test", "Probe");

        internal List<string> Calls { get; } = [];
        internal List<object> Instances { get; } = [];
    }

    public sealed class OtherProbeToken : AbstractControlDesignToken, IControlToken
    {
        public static ControlTokenIdentity ControlIdentity { get; } = new("Test", "Other");
    }

    public sealed class ProbeAlgorithm : IControlTokenAlgorithm<ProbeToken>
    {
        public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
        {
            controlGlobal.ColorPrimary = context.Appearance == ThemeAppearance.Dark ? Colors.Red : Colors.Blue;
        }

        public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, ProbeToken token)
        {
            token.Calls.Add("Own:" + context.Appearance);
            token.Instances.Add(this);
        }
    }

    public sealed class OtherProbeAlgorithm : IControlTokenAlgorithm<OtherProbeToken>
    {
        public void EvaluateGlobal(in ThemeAlgorithmContext context, DesignToken controlGlobal)
        {
        }

        public void EvaluateOwn(in ThemeAlgorithmContext context, DesignToken effectiveGlobal, OtherProbeToken token)
        {
        }
    }
}
