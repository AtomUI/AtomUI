using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Theme;

[Collection(ThemeConfigProviderTestCollection.Name)]
public class ThemeStartupTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Direct_Package_Collection_Failure_Poisons_Builder_Without_Partial_Staging(bool descriptorConflict)
    {
        HeadlessTestApp.Run(() =>
        {
            var builder = new ThemeManagerBuilder();
            var descriptor = ThemeCompilerTests.CreateCompilerButtonDescriptor();
            var first = new TestControlThemesProvider("Tests.Direct");
            builder.AddControlPackage(new(first.Id, [descriptor], [], [], first, []));
            var second = new TestControlThemesProvider(descriptorConflict ? "Tests.Conflict" : first.Id);
            Should.Throw<ThemeResourceRegisterException>(() => builder.AddControlPackage(new(second.Id, [descriptor], [], [], second, [])));
            builder.ControlPackages.ShouldHaveSingleItem().Id.ShouldBe(first.Id);
            Should.Throw<InvalidOperationException>(() => builder.Build());
            var third = new TestControlThemesProvider("Tests.AfterFailure");
            Should.Throw<InvalidOperationException>(() => builder.AddControlPackage(new(third.Id, [], [], [], third, [])));
            var independent = new ThemeManagerBuilder();
            independent.AddControlPackage(new(third.Id, [], [], [], third, []));
            using var manager = independent.Build();
        });
    }

    [Fact]
    public void Direct_Theme_Builder_Validates_Stale_Assets_Before_Build_And_Poisons_Failure()
    {
        var provider = new TestControlThemesProvider("Tests.Stale");
        var builder = new ThemeManagerBuilder();
        var asset = new ControlThemeAssetDescriptor("Stale", new Uri("avares://Tests/Stale.axaml"), [], [], [], 1);
        builder.AddControlPackage(new ControlPackageRegistration(provider.Id, [], [asset], [], provider, []));
        Should.Throw<ThemeSchemaException>(() => builder.Build());
        Should.Throw<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Direct_Theme_Builder_Freezes_Registration_After_Build()
    {
        var builder = new ThemeManagerBuilder();
        builder.Build();
        var provider = new TestControlThemesProvider("Tests.Late");
        Should.Throw<InvalidOperationException>(() => builder.AddControlPackage(new(provider.Id, [], [], [], provider, [])));
    }

    [Fact]
    public void Control_Package_Registration_Adds_Descriptors_Assets_And_Themes_Atomically()
    {
        HeadlessTestApp.Run(() =>
        {
            var descriptor = ThemeCompilerTests.CreateCompilerButtonDescriptor();
            var asset = ControlThemeAssetContractTests.Asset([new ControlThemeExportDescriptor(descriptor.ControlType, descriptor.ControlType)],
                [ControlTokenIdentity.ForControl(descriptor.ControlType, descriptor.Identity.Catalog, descriptor.Identity.Id)], globals: AtomUI.Generated.AtomUICore.GeneratedThemeSchema.GetGlobalTokens());
            var provider = new TestControlThemesProvider("Tests.Controls");
            var packageResource = new object();
            ((ResourceDictionary)provider.ControlThemes.Single())["PackageThemeResource"] = packageResource;
            var semanticDescriptor = new ControlSemanticDescriptor(
                descriptor.ControlType,
                descriptor.Identity,
                [
                    new SemanticPartDescriptor(
                        "root",
                        "root",
                        null,
                        descriptor.ControlType,
                        SemanticPartCardinality.Single,
                        SemanticPartCustomization.Root,
                        null,
                        false,
                        "6.0",
                        false)
                ]);
            var package = new ControlPackageRegistration(
                provider.Id,
                [descriptor],
                [asset],
                [semanticDescriptor],
                provider, []);
            var builder = new ThemeManagerBuilder();

            builder.AddControlPackage(package);
            var manager = builder.Build();
            manager.InitializeApplication(Application.Current!);

            manager.CurrentSnapshot!.Registry.Controls
                   .ShouldContain(control => control.Identity == descriptor.Identity);
            manager.SemanticParts.TryGetControl(descriptor.ControlType, out var registeredSemanticDescriptor)
                   .ShouldBeTrue();
            registeredSemanticDescriptor.ShouldBeSameAs(semanticDescriptor);
            manager.Resources.TryGetResource("PackageThemeResource", ThemeVariant.Light, out var resource).ShouldBeTrue();
            resource.ShouldBeSameAs(packageResource);
            Should.Throw<InvalidOperationException>(() => builder.AddControlPackage(package));
        });
    }

    [Fact]
    public void Control_Package_Can_Register_An_Empty_Semantic_Contract()
    {
        var descriptor = ThemeCompilerTests.CreateCompilerButtonDescriptor();
        var provider = new TestControlThemesProvider("Tests.EmptySemantics");
        var package = new ControlPackageRegistration(
            provider.Id,
            [descriptor],
            [], [], provider, []);
        var builder = new ThemeManagerBuilder();

        builder.AddControlPackage(package);
        var manager = builder.Build();

        package.SemanticControls.ShouldBeEmpty();
        manager.SemanticParts.Controls.ShouldBeEmpty();
    }

    [Fact]
    public void Control_Package_Rejects_Null_Semantic_Descriptors_With_A_Contract_Exception()
    {
        var descriptor = ThemeCompilerTests.CreateCompilerButtonDescriptor();
        var provider = new TestControlThemesProvider("Tests.InvalidSemanticControls");

        Should.Throw<ArgumentException>(() => new ControlPackageRegistration(
            provider.Id,
            [descriptor],
            Array.Empty<ControlThemeAssetDescriptor>(),
            [null!],
            provider, []));
    }

    [Fact]
    public void InitializeApplication_Mounts_Full_Resources_And_Root_Style_Before_The_First_Frame()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            var builder = new ThemeManagerBuilder();
            builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
            var manager = builder.Build();

            manager.InitializeApplication(application);

            manager.CurrentTheme.ShouldNotBeNull();
            manager.CurrentSnapshot.ShouldNotBeNull();
            manager.RootContext.Snapshot.ShouldBeSameAs(manager.CurrentSnapshot);
            manager.Resources.MergedDictionaries
                   .OfType<ThemeTokenResourceProvider>()
                   .ShouldHaveSingleItem()
                   .ShouldBeSameAs(manager.RootContext.ResourceProvider);
            application.Styles.Resources.MergedDictionaries.ShouldContain(manager.Resources);
            manager.Resources.Owner.ShouldBeSameAs(application);
            application.Styles.ShouldNotContain(manager);
            application.RequestedThemeVariant.ShouldBe(ThemeVariant.Light);
            manager.AvailableThemes.ShouldContain(theme =>
                theme.Id == IThemeManager.DEFAULT_THEME_ID && theme.IsDefault);
            var host = new Window();
            host.ApplyStyling();
            host.GetValue(ThemeScope.ContextProperty).ShouldBeSameAs(manager.RootContext);
            host.GetValue(ThemeStyleQualification.ActiveProperty)
                .ShouldBeSameAs(manager.RootContext.Styles.Current.Activation);
            host.Close();

            var frameworkValue = new object();
            manager.Resources["StartupThemeResource"] = frameworkValue;
            application.Styles.Add(new Style
            {
                Resources = new ResourceDictionary { ["StartupThemeResource"] = new object() }
            });
            application.TryGetResource("StartupThemeResource", null, out var actual).ShouldBeTrue();
            actual.ShouldBeSameAs(frameworkValue);
            application.Resources["StartupThemeResource"] = "application override";
            application.TryGetResource("StartupThemeResource", null, out actual).ShouldBeTrue();
            actual.ShouldBe("application override");
        });
    }

    [Fact]
    public async Task Runtime_Request_Preserves_Startup_Config_As_The_Lower_Layer()
    {
        await HeadlessTestApp.RunAsync(async () =>
        {
            var application = Application.Current!;
            var builder = new ThemeManagerBuilder();
            builder.WithInitialTheme(
                IThemeManager.DEFAULT_THEME_ID,
                new ThemeConfigBuilder()
                    .WithToken(nameof(DesignToken.BorderRadius), "12")
                    .Build());
            var manager = builder.Build();
            manager.InitializeApplication(application);

            var result = await manager.ApplyThemeAsync(
                new ThemeRequest(
                    IThemeManager.DEFAULT_THEME_ID,
                    new ThemeConfigBuilder()
                        .WithToken(nameof(DesignToken.ColorPrimary), "#00b96b")
                        .Build(),
                    ThemeTransitionReason.UserRequest),
                TestContext.Current.CancellationToken);

            result.Status.ShouldBe(ThemeTransitionStatus.Committed);
            var snapshot = manager.CurrentSnapshot.ShouldNotBeNull();
            snapshot.Global<CornerRadius>(nameof(DesignToken.BorderRadius))
                    .ShouldBe(new CornerRadius(12));
            snapshot.Global<Color>(nameof(DesignToken.ColorPrimary))
                    .ShouldBe(Color.Parse("#00b96b"));
        });
    }

    [Fact]
    public void Root_ThemeConfigProvider_Resolves_Context_Before_TopLevel_Styles_Are_Applied()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            ApplicationStartup.AttachWith(application, static _ => { });
            var manager = AvaloniaLocator.Current.GetService(typeof(ThemeManager))
                                         .ShouldBeOfType<ThemeManager>();
            var provider = new ThemeConfigProvider
            {
                Child = new Border()
            };
            var window = new Window
            {
                Content = provider
            };

            var providerContext = provider.GetValue(ThemeScope.ContextProperty).ShouldNotBeNull();
            providerContext.Manager.ShouldBeSameAs(manager);
            providerContext.RegistrationId.ShouldBeGreaterThan(0);
            window.GetValue(ThemeScope.ContextProperty)
                  .ShouldBeSameAs(manager.RootContext);

            window.Content = null;
            window.Close();
        });
    }

    [Fact]
    public void Root_ThemeContext_Is_Resolved_For_Ordinary_TopLevel_Content()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            ApplicationStartup.AttachWith(application, static _ => { });
            var manager = AvaloniaLocator.Current.GetService(typeof(ThemeManager))
                                         .ShouldBeOfType<ThemeManager>();
            var content = new Border();
            var window = new Window
            {
                Content = content
            };

            new ThemeTokenResolver().Capture(content)
                                    .ShouldBeSameAs(manager.RootContext.Snapshot);
            window.GetValue(ThemeScope.ContextProperty)
                  .ShouldBeSameAs(manager.RootContext);

            window.Content = null;
            window.Close();
        });
    }

    private sealed class TestControlThemesProvider : ControlThemesProvider
    {
        internal TestControlThemesProvider(string id)
        {
            Id = id;
            ControlThemes.Add(new ResourceDictionary());
        }
    }
}
