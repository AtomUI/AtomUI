using System.Reflection;
using AtomUI.Localization;
using Avalonia;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Localization;

public class AtomUIBuilderTests
{
    [Fact]
    public void Root_Builder_Exposes_Theme_And_Localization_As_Sibling_Subsystems()
    {
        var properties = typeof(IAtomUIBuilder)
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                         .OrderBy(static property => property.Name, StringComparer.Ordinal)
                         .ToArray();
        var builder = new AtomUIBuilder(typeof(Application));

        properties.Select(static property => property.Name)
                  .ShouldBe([nameof(IAtomUIBuilder.Localization), nameof(IAtomUIBuilder.Theme)]);
        builder.Theme.ShouldBeSameAs(builder.ThemeManagerBuilder);
        builder.Localization.ShouldBeSameAs(builder.LocalizationBuilder);
    }

    [Fact]
    public void UseLanguages_Forwards_Strongly_Typed_Configuration_And_Returns_The_Root_Builder()
    {
        var languages = new[]
        {
            LanguageTags.ZhCN,
            LanguageTags.EnUS,
            LanguageTags.ZhCN
        };
        var builder = new AtomUIBuilder(typeof(Application));

        var returned = builder.UseLanguages(LanguageTags.ZhCN, languages);
        languages[0] = LanguageTags.FrFR;
        using var runtime = builder.LocalizationBuilder.Build(static () => true);

        returned.ShouldBeSameAs(builder);
        runtime.LanguageManager.Current.CurrentLanguage.ShouldBe(LanguageTags.ZhCN);
        runtime.LanguageManager.SupportedLanguages.Select(static definition => definition.Tag)
               .ShouldBe([LanguageTags.ZhCN, LanguageTags.EnUS]);
    }

    [Fact]
    public void UseLanguages_Preserves_Runtime_Default_Membership_Validation()
    {
        var builder = new AtomUIBuilder(typeof(Application));
        builder.UseLanguages(LanguageTags.ZhCN, [LanguageTags.EnUS]);

        Should.Throw<LanguageConfigurationException>(() =>
            builder.LocalizationBuilder.Build(static () => true));
    }

    [Fact]
    public void Root_Theme_Extensions_Forward_To_The_Theme_SubBuilder_Fluently()
    {
        var builder = new AtomUIBuilder(typeof(Application));

        var returned = builder.WithApplicationId("Acme.App")
                              .WithInitialTheme("atomui-default")
                              .WithDefaultFontFamily("Inter");

        returned.ShouldBeSameAs(builder);
        builder.ThemeManagerBuilder.ApplicationId.ShouldBe("Acme.App");
        builder.ThemeManagerBuilder.InitialRequest.ThemeId.ShouldBe("atomui-default");
        builder.ThemeManagerBuilder.FontFamily.ShouldNotBeNull();
    }
}
