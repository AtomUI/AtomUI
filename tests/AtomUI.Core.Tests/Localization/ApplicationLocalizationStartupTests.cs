using AtomUI.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Localization;

public class ApplicationLocalizationStartupTests
{
    private enum DeferredResourceKind
    {
        Value
    }

    [Fact]
    public void UseAtomUI_Publishes_Default_Application_Services()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;

            application.GetLanguageManager().ShouldBeNull();
            application.GetLocalizer().ShouldBeNull();
            ApplicationStartup.AttachWith(application, static _ => { });

            var scope = ApplicationScopeRegistry.Get(application).ShouldNotBeNull();
            application.GetLanguageManager().ShouldBeSameAs(scope.LanguageManager);
            application.GetLocalizer().ShouldBeSameAs(scope.Localizer);
            application.GetThemeManager().ShouldBeSameAs(scope.ThemeManager);
            scope.LanguageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.EnUS);
            scope.LanguageManager.Current.Revision.ShouldBe(0);
        });
    }

    [Fact]
    public void UseAtomUI_Uses_Strongly_Typed_Language_Configuration_Before_First_Window()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;

            ApplicationStartup.AttachWith(application, builder =>
            {
                builder.Localization.AddTranslationBundle(new TranslationBundleDescriptor(
                    "AtomUI.Core.Tests:AtomUI.Core.Tests.Localization.GeneratedStartupLangResourceKind",
                    LanguageTags.ArSA,
                    TranslationSourceKind.ApplicationOverride,
                    "AtomUI.Core.Tests",
                    ["العربية", "{0} عناصر"]));
                builder.UseLanguages(
                    LanguageTags.ArSA,
                    [LanguageTags.ArSA, LanguageTags.EnUS]);
            });
            var window = new Window();
            window.Show();

            application.GetLanguageManager()!.Current.CurrentLanguage.ShouldBe(LanguageTags.ArSA);
            window.FlowDirection.ShouldBe(FlowDirection.RightToLeft);
            window.Close();
        });
    }

    [Fact]
    public void UseAtomUI_Invokes_Generated_Bootstrap_Before_User_Configuration()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = (TestApplication)Application.Current!;
            var configured = false;
            application.ShouldBeAssignableTo<IGeneratedApplicationLanguageBootstrap>();
            ApplicationStartup.AttachWith(application, builder =>
            {
                configured = true;
                builder.UseLanguages(
                    LanguageTags.EnUS,
                    [LanguageTags.EnUS, LanguageTags.ZhCN]);
            });

            configured.ShouldBeTrue();
            application.GetLocalizer()!
                       .Get(GeneratedStartupLangResourceKind.Value)
                       .ShouldBe("English");
            application.GetLocalizer()!
                       .Format(GeneratedStartupLangResourceKind.ItemCount, 3)
                       .ShouldBe("3 items");
        });
    }

    [Fact]
    public void Application_Dynamic_Enum_Resources_Refresh_After_Committed_Change()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = (TestApplication)Application.Current!;
            ApplicationStartup.AttachWith(application, builder => builder.UseLanguages(
                LanguageTags.EnUS,
                [LanguageTags.EnUS, LanguageTags.ZhCN]));

            var text = new TextBlock();
            var extension = new GeneratedStartupLangResourceExtension(
                GeneratedStartupLangResourceKind.Value);
            var dynamicResource = extension.ProvideValue(
                    new ProvideValueServiceProvider(text, TextBlock.TextProperty))
                .ShouldBeOfType<DynamicResourceExtension>();
            dynamicResource.ResourceKey.ShouldBe(GeneratedStartupLangResourceKind.Value);
            text.Bind(TextBlock.TextProperty, dynamicResource);
            var window = new Window
            {
                Content = text
            };
            window.Show();

            text.Text.ShouldBe("English");
            application.GetLanguageManager()!.ChangeLanguage(LanguageTags.ZhCN);
            text.Text.ShouldBe("中文");
            application.GetLocalizer()!
                       .Format(GeneratedStartupLangResourceKind.ItemCount, 3)
                       .ShouldBe("3 项");
            window.Close();
        });
    }

    [Fact]
    public void UseAtomUI_Rejects_Duplicate_Initialization()
    {
        HeadlessTestApp.Run(() =>
        {
            var application = Application.Current!;
            ApplicationStartup.AttachWith(application, static _ => { });

            var exception = Should.Throw<InvalidOperationException>(
                () => ApplicationStartup.AttachWith(application, static _ => { }));

            exception.Message.ShouldContain("already initialized");
        });
    }

    private sealed class ProvideValueServiceProvider(
        object targetObject,
        object targetProperty) : IServiceProvider, IProvideValueTarget
    {
        public object TargetObject { get; } = targetObject;

        public object TargetProperty { get; } = targetProperty;

        public object? GetService(Type serviceType)
        {
            return serviceType == typeof(IProvideValueTarget) ? this : null;
        }
    }

    private static TranslationBundleDescriptor CreateDeferredBundle(
        LanguageCatalogDescriptor catalog,
        LanguageTag language,
        string value)
    {
        return new TranslationBundleDescriptor(
            catalog.CatalogId,
            language,
            TranslationSourceKind.ModuleBuiltIn,
            "AtomUI.Core.Tests",
            [value]);
    }
}
