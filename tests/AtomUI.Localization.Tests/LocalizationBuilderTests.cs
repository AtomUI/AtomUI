using System.Globalization;
using Shouldly;
using Xunit;

namespace AtomUI.Localization.Tests;

public class LocalizationBuilderTests
{
    [Fact]
    public void Build_Defaults_To_English_As_The_Only_Supported_Language()
    {
        var builder = CreateBuilderWithEnglishCatalog();

        using var runtime = builder.Build(static () => true);

        runtime.LanguageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.EnUS);
        runtime.LanguageManager.Current.Revision.ShouldBe(0);
        runtime.LanguageManager.SupportedLanguages.Select(static definition => definition.Tag)
               .ShouldBe([LanguageTags.EnUS]);
        runtime.Snapshots.Keys.ShouldBe([LanguageTags.EnUS]);
    }

    [Fact]
    public void Build_Defensively_Copies_And_DeDuplicates_Supported_Languages()
    {
        var languages = new[]
        {
            LanguageTags.ZhCN,
            LanguageTags.EnUS,
            LanguageTags.ZhCN
        };
        var builder = CreateBuilderWithEnglishCatalog();
        builder.AddTranslationBundle(CreateBundle(LanguageTags.ZhCN, "中文"));
        builder.ConfigureLanguages(LanguageTags.ZhCN, languages);

        languages[0] = LanguageTags.FrFR;
        using var runtime = builder.Build(static () => true);

        runtime.LanguageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.ZhCN);
        runtime.LanguageManager.SupportedLanguages.Select(static definition => definition.Tag)
               .ShouldBe([LanguageTags.ZhCN, LanguageTags.EnUS]);
        runtime.Snapshots.Keys.ShouldBe([LanguageTags.ZhCN]);
        runtime.LanguageManager.ChangeLanguage(LanguageTags.EnUS);
        runtime.Snapshots.Keys.ShouldBe([LanguageTags.ZhCN, LanguageTags.EnUS], ignoreOrder: true);
    }

    [Fact]
    public void Build_Requires_Default_Language_To_Be_Supported()
    {
        var builder = CreateBuilderWithEnglishCatalog();
        builder.ConfigureLanguages(LanguageTags.ZhCN, [LanguageTags.EnUS]);

        var exception = Should.Throw<LanguageConfigurationException>(() =>
            builder.Build(static () => true));

        exception.Message.ShouldContain("zh-CN");
        exception.Message.ShouldContain("supported");
    }

    [Fact]
    public void Build_Rejects_An_Empty_Supported_Language_Set()
    {
        var builder = CreateBuilderWithEnglishCatalog();
        builder.ConfigureLanguages(LanguageTags.EnUS, []);

        Should.Throw<LanguageConfigurationException>(() => builder.Build(static () => true))
              .Message.ShouldContain("At least one");
    }

    [Fact]
    public void Build_Rejects_Default_Language_Tag_In_Supported_Set()
    {
        var builder = CreateBuilderWithEnglishCatalog();
        builder.ConfigureLanguages(LanguageTags.EnUS, [LanguageTags.EnUS, default]);

        Should.Throw<LanguageConfigurationException>(() => builder.Build(static () => true))
              .Message.ShouldContain("valid language tag");
    }

    [Fact]
    public void Build_Uses_Generated_Standard_Language_Definitions()
    {
        var builder = CreateBuilderWithEnglishCatalog();
        builder.AddTranslationBundle(CreateBundle(LanguageTags.ArSA, "العربية"));
        builder.ConfigureLanguages(LanguageTags.ArSA, [LanguageTags.ArSA]);

        using var runtime = builder.Build(static () => true);
        var definition = runtime.LanguageManager.SupportedLanguages.ShouldHaveSingleItem();

        definition.Tag.ShouldBe(LanguageTags.ArSA);
        definition.FormattingCulture.Name.ShouldBe("ar-SA");
        definition.NativeName.ShouldBe("العربية (المملكة العربية السعودية)");
        definition.TextDirection.ShouldBe(LanguageTextDirection.RightToLeft);
    }

    [Fact]
    public void Build_Requires_An_Explicit_Definition_For_Unknown_Private_Tag()
    {
        var privateTag = LanguageTag.Parse("en-x-acme");
        var builder = CreateBuilderWithEnglishCatalog();
        builder.ConfigureLanguages(privateTag, [privateTag]);

        var exception = Should.Throw<LanguageConfigurationException>(() =>
            builder.Build(static () => true));

        exception.Message.ShouldContain("en-x-acme");
        exception.Message.ShouldContain(nameof(LanguageDefinition));
    }

    [Fact]
    public void Build_Uses_An_Explicit_Definition_For_A_Private_Tag()
    {
        var privateTag = LanguageTag.Parse("en-x-acme");
        var builder = CreateBuilderWithEnglishCatalog();
        builder.AddLanguageDefinition(new LanguageDefinition(
            privateTag,
            CultureInfo.GetCultureInfo("en-US"),
            "Acme English",
            LanguageTextDirection.LeftToRight));
        builder.ConfigureLanguages(privateTag, [privateTag]);

        using var runtime = builder.Build(static () => true);
        var definition = runtime.LanguageManager.SupportedLanguages.ShouldHaveSingleItem();

        definition.Tag.ShouldBe(privateTag);
        definition.NativeName.ShouldBe("Acme English");
        runtime.Localizer.Get(BuilderResourceKind.Value).ShouldBe("English");
    }

    [Fact]
    public void Build_Rejects_Duplicate_Explicit_Language_Definitions()
    {
        var builder = CreateBuilderWithEnglishCatalog();
        builder.AddLanguageDefinition(new LanguageDefinition(
            LanguageTags.EnUS,
            CultureInfo.GetCultureInfo("en-US"),
            "First",
            LanguageTextDirection.LeftToRight));
        builder.AddLanguageDefinition(new LanguageDefinition(
            LanguageTags.EnUS,
            CultureInfo.GetCultureInfo("en-GB"),
            "Second",
            LanguageTextDirection.RightToLeft));

        var exception = Should.Throw<LanguageConfigurationException>(() =>
            builder.Build(static () => true));

        exception.Message.ShouldContain("en-US");
        exception.Message.ShouldContain("more than once");
    }

    [Fact]
    public void Build_Validates_Manual_Catalog_Coverage_For_Every_Supported_Language()
    {
        var builder = CreateBuilderWithEnglishCatalog();
        builder.ConfigureLanguages(
            LanguageTags.EnUS,
            [LanguageTags.EnUS, LanguageTags.ZhCN]);

        var exception = Should.Throw<LanguageCoverageException>(() =>
            builder.Build(static () => true));

        exception.Message.ShouldContain("zh-CN");
        exception.Message.ShouldContain("Acme:Acme.BuilderResourceKind");
    }

    [Fact]
    public void Build_Composes_One_Shared_Runtime_And_Is_Idempotent()
    {
        var builder = CreateBuilderWithEnglishCatalog();

        using var runtime = builder.Build(static () => true);
        var second = builder.Build(static () => false);

        second.ShouldBeSameAs(runtime);
        runtime.ResourceProvider.TryGetResource(BuilderResourceKind.Value, null, out var resource).ShouldBeTrue();
        resource.ShouldBe(runtime.Localizer.Get(BuilderResourceKind.Value));
        runtime.LanguageManager.ShouldBeAssignableTo<ILanguageManager>();
        runtime.Localizer.ShouldBeAssignableTo<ILocalizer>();
    }

    [Fact]
    public void Build_Freezes_All_Further_Registrations()
    {
        var builder = CreateBuilderWithEnglishCatalog();
        using var runtime = builder.Build(static () => true);

        Should.Throw<InvalidOperationException>(() =>
            builder.AddTranslationBundle(CreateBundle(LanguageTags.ZhCN, "中文")));
        Should.Throw<InvalidOperationException>(() =>
            builder.AddLanguageDefinition(new LanguageDefinition(
                LanguageTags.ZhCN,
                CultureInfo.GetCultureInfo("zh-CN"),
                "简体中文",
                LanguageTextDirection.LeftToRight)));
        Should.Throw<InvalidOperationException>(() =>
            builder.ConfigureLanguages(LanguageTags.EnUS, [LanguageTags.EnUS]));
    }

    private static LocalizationBuilder CreateBuilderWithEnglishCatalog()
    {
        var builder = new LocalizationBuilder();
        builder.AddCatalog(new LanguageCatalogDescriptor<BuilderResourceKind>(
            "Acme:Acme.BuilderResourceKind",
            [new LanguageCatalogUnitDescriptor("Value")],
            static key => key == BuilderResourceKind.Value ? 0 : -1));
        builder.AddTranslationBundle(CreateBundle(LanguageTags.EnUS, "English"));
        return builder;
    }

    private static TranslationBundleDescriptor CreateBundle(
        LanguageTag language,
        string value)
    {
        return new TranslationBundleDescriptor(
            "Acme:Acme.BuilderResourceKind",
            language,
            TranslationSourceKind.ModuleBuiltIn,
            "Acme",
            [value]);
    }

    private enum BuilderResourceKind
    {
        Value
    }
}
