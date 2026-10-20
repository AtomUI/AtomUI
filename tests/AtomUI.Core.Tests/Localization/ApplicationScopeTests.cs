using AtomUI.Localization;
using AtomUI.Registration;
using AtomUI.Theme;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Localization;

public class ApplicationScopeTests
{
    [Fact]
    public void Scope_Mounts_One_Stable_Provider_And_Exposes_Exact_Service_Instances()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            using var scope = CreateScope(application);

            scope.InitializeApplication();

            application.Styles.Resources.MergedDictionaries
                       .Count(provider => ReferenceEquals(provider, scope.ResourceProvider))
                       .ShouldBe(1);
            scope.ResourceProvider.Owner.ShouldBeSameAs(application);
            ApplicationScopeRegistry.Get(application).ShouldBeSameAs(scope);
            scope.LanguageManager.ShouldBeSameAs(scope.LocalizationHost.LanguageManager);
            scope.Localizer.ShouldBeSameAs(scope.LocalizationHost.Localizer);

            application.Resources = new ResourceDictionary { ["ApplicationOverride"] = "preserved" };
            application.TryGetResource(LanguageResourceKeys.FlowDirection, null, out var direction).ShouldBeTrue();
            direction.ShouldBe(FlowDirection.LeftToRight);
        });
    }

    [Fact]
    public void Scope_Projects_FlowDirection_Through_The_Same_Language_Provider()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            using var scope = CreateScope(
                application,
                builder => builder.UseLanguages(
                    LanguageTags.ArSA,
                    [LanguageTags.ArSA, LanguageTags.EnUS]));
            scope.InitializeApplication();
            var provider = scope.ResourceProvider;
            var window = new Window
            {
                Content = new Border()
            };
            window.Show();

            window.FlowDirection.ShouldBe(FlowDirection.RightToLeft);
            scope.LanguageManager.ChangeLanguage(LanguageTags.EnUS);

            scope.ResourceProvider.ShouldBeSameAs(provider);
            window.FlowDirection.ShouldBe(FlowDirection.LeftToRight);
            window.Close();
        });
    }

    [Fact]
    public void Scope_Rejects_Duplicate_Initialization_For_The_Same_Application()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            using var first = CreateScope(application);
            using var second = CreateScope(application);
            first.InitializeApplication();

            var exception = Should.Throw<InvalidOperationException>(() => second.InitializeApplication());

            exception.Message.ShouldContain("already initialized");
            application.Styles.Resources.MergedDictionaries
                       .Count(provider => ReferenceEquals(provider, second.ResourceProvider))
                       .ShouldBe(0);
        });
    }

    [Fact]
    public void Dispose_Unmounts_Application_Resources_Styles_And_Services()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var scope = CreateScope(application);
            scope.InitializeApplication();
            var provider = scope.ResourceProvider;
            var flowDirectionStyle = scope.FlowDirectionStyle;

            scope.Dispose();

            application.Styles.Resources.MergedDictionaries.Contains(provider).ShouldBeFalse();
            application.Styles.Contains(flowDirectionStyle).ShouldBeFalse();
            ApplicationScopeRegistry.Get(application).ShouldBeNull();
            Should.Throw<InvalidOperationException>(() =>
                scope.LanguageManager.ChangeLanguage(LanguageTags.EnUS));
        });
    }

    [Fact]
    public void Owned_Services_Attach_In_Registration_Order_And_Cleanup_In_Reverse_Order()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var log = new List<string>();
            var first = new TrackingOwnedService("first", log);
            var second = new TrackingOwnedService("second", log);
            var scope = CreateScope(application, ownedServices: [first, second]);

            scope.InitializeApplication();
            scope.Dispose();

            log.ShouldBe([
                "attach:first",
                "attach:second",
                "detach:second",
                "detach:first",
                "dispose:second",
                "dispose:first"
            ]);
            ApplicationScopeRegistry.Get(application).ShouldBeNull();
        });
    }

    [Fact]
    public void Owned_Service_Attach_Failure_Rolls_Back_Only_Successfully_Attached_Services()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var log = new List<string>();
            var first = new TrackingOwnedService("first", log);
            var second = new TrackingOwnedService("second", log) { ThrowOnAttach = true };
            var third = new TrackingOwnedService("third", log);
            var scope = CreateScope(application, ownedServices: [first, second, third]);

            Should.Throw<InvalidOperationException>(() => scope.InitializeApplication())
                  .Message.ShouldContain("attach:second");

            log.ShouldBe([
                "attach:first",
                "attach:second",
                "detach:first"
            ]);
            ApplicationScopeRegistry.Get(application).ShouldBeNull();
            application.Styles.Resources.MergedDictionaries.Contains(scope.ResourceProvider).ShouldBeFalse();
            application.Styles.Contains(scope.FlowDirectionStyle).ShouldBeFalse();

            scope.Dispose();
            log.ShouldBe([
                "attach:first",
                "attach:second",
                "detach:first",
                "dispose:third",
                "dispose:second",
                "dispose:first"
            ]);
        });
    }

    [Fact]
    public void Dispose_Continues_After_Detach_And_Dispose_Failures()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var log = new List<string>();
            var first = new TrackingOwnedService("first", log) { ThrowOnDetach = true };
            var second = new TrackingOwnedService("second", log) { ThrowOnDispose = true };
            var scope = CreateScope(application, ownedServices: [first, second]);
            scope.InitializeApplication();

            var exception = Should.Throw<AggregateException>(scope.Dispose);

            exception.InnerExceptions.Count.ShouldBe(2);
            log.ShouldBe([
                "attach:first",
                "attach:second",
                "detach:second",
                "detach:first",
                "dispose:second",
                "dispose:first"
            ]);
            ApplicationScopeRegistry.Get(application).ShouldBeNull();
            application.Styles.Resources.MergedDictionaries.Contains(scope.ResourceProvider).ShouldBeFalse();
            application.Styles.Contains(scope.FlowDirectionStyle).ShouldBeFalse();
        });
    }

    [Fact]
    public void Initialize_Aggregates_Attach_And_Rollback_Failures()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var log = new List<string>();
            var first = new TrackingOwnedService("first", log) { ThrowOnDetach = true };
            var second = new TrackingOwnedService("second", log) { ThrowOnAttach = true };
            var scope = CreateScope(application, ownedServices: [first, second]);

            var exception = Should.Throw<AggregateException>(() => scope.InitializeApplication());

            exception.InnerExceptions.Count.ShouldBe(2);
            log.ShouldBe([
                "attach:first",
                "attach:second",
                "detach:first"
            ]);
            ApplicationScopeRegistry.Get(application).ShouldBeNull();
            scope.Dispose();
        });
    }

    [Fact]
    public void Owned_Service_Factory_Failure_Disposes_Previously_Created_Services_In_Reverse_Order()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var log = new List<string>();
            var first = new TrackingOwnedService("first", log);
            var second = new TrackingOwnedService("second", log);

            var exception = Should.Throw<InvalidOperationException>(() => ApplicationStartup.AttachWith(application, builder =>
            {
                var atomBuilder = (AtomUIBuilder)builder;
                atomBuilder.AddOwnedService("first", _ => first);
                atomBuilder.AddOwnedService("second", _ => second);
                atomBuilder.AddOwnedService(
                    "failure",
                    _ => throw new InvalidOperationException("factory:failure"));
            }));

            exception.Message.ShouldBe("factory:failure");
            log.ShouldBe([
                "dispose:second",
                "dispose:first"
            ]);
            ApplicationScopeRegistry.Get(application).ShouldBeNull();
        });
    }

    private static ApplicationScope CreateScope(
        Application application,
        Action<IAtomUIBuilder>? configure = null,
        IReadOnlyList<IAtomUIOwnedService>? ownedServices = null)
    {
        var builder = new AtomUIBuilder(application.GetType());
        configure?.Invoke(builder);
        return new ApplicationScope(
            application,
            builder.ThemeManagerBuilder.Build(),
            builder.LocalizationBuilder.Build(),
            ownedServices ?? [],
            new StartupProcess());
    }

    private static void RegisterSemanticPackage(IAtomUIBuilder builder, ControlTokenIdentity identity)
    {
        ControlRegistrationRuntime.RegisterPackage(
            builder,
            "Deferred.Package",
            static () => new DeferredProvider("Deferred.Package"),
            collection => collection.AddSemanticControl(new ControlSemanticDescriptor(
                typeof(Button),
                identity,
                [new SemanticPartDescriptor(
                    "root",
                    "root",
                    null,
                    typeof(Button),
                    SemanticPartCardinality.Single,
                    SemanticPartCustomization.Root,
                    null,
                    false,
                    null,
                    false)])));
    }

    private sealed class FailingResolver : IThemeDefinitionResolver
    {
        public string Id => nameof(FailingResolver);
        public bool SupportsReload => false;

        public ThemeDefinitionResolveResult Resolve(ThemeDefinitionResolveContext context)
        {
            return new ThemeDefinitionResolveResult(
                [],
                [new ThemeDiagnostic(
                    "TEST-STAGE-TWO",
                    ThemeDiagnosticSeverity.Error,
                    Id,
                    "$",
                    "Stage 2 preparation failure")]);
        }
    }

    private sealed class DeferredProvider : ControlThemesProvider
    {
        internal DeferredProvider(string id)
        {
            Id = id;
        }
    }

    private sealed class TestSubscriberException : Exception;

    private sealed class TrackingOwnedService : IAtomUIOwnedService
    {
        private readonly string _name;
        private readonly ICollection<string> _log;

        internal TrackingOwnedService(string name, ICollection<string> log)
        {
            _name = name;
            _log = log;
        }

        internal bool ThrowOnAttach { get; init; }

        internal bool ThrowOnDetach { get; init; }

        internal bool ThrowOnDispose { get; init; }

        public void Attach(Application application)
        {
            _log.Add($"attach:{_name}");
            if (ThrowOnAttach)
            {
                throw new InvalidOperationException($"attach:{_name}");
            }
        }

        public void Detach(Application application)
        {
            _log.Add($"detach:{_name}");
            if (ThrowOnDetach)
            {
                throw new InvalidOperationException($"detach:{_name}");
            }
        }

        public void Dispose()
        {
            _log.Add($"dispose:{_name}");
            if (ThrowOnDispose)
            {
                throw new InvalidOperationException($"dispose:{_name}");
            }
        }
    }
}
