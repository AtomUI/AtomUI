using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class ThemeInitialModeTests
{
    [Fact]
    public void Builder_Initial_Theme_Keeps_Theme_Id_And_Dark_Appearance_Config()
    {
        var builder = new TestThemeManagerBuilder();
        var config  = new ThemeConfigBuilder().WithAppearance(ThemeAppearance.Dark).Build();

        builder.WithInitialTheme(AntDesignThemes.DaybreakBlue, config);

        builder.ThemeId.ShouldBe(AntDesignThemes.DaybreakBlue);
        builder.InitialConfig.ShouldBeSameAs(config);
        builder.InitialConfig!.Appearance.ShouldBe(ThemeAppearance.Dark);
        builder.InitialConfig.Density.ShouldBeNull();
    }

    [Fact]
    public void Builder_Initial_Theme_Preserves_Explicit_Density_And_Appearance()
    {
        var builder = new TestThemeManagerBuilder();
        var config = new ThemeConfigBuilder()
                     .WithDensity(DesignDensity.Compact)
                     .WithAppearance(ThemeAppearance.Dark)
                     .Build();

        builder.WithInitialTheme(AntDesignThemes.DaybreakBlue, config);

        builder.InitialConfig!.Density.ShouldBe(DesignDensity.Compact);
        builder.InitialConfig.Appearance.ShouldBe(ThemeAppearance.Dark);
    }

    [Fact]
    public void Builder_Initial_Theme_Allows_Definition_Defaults_When_Config_Is_Null()
    {
        var builder = new TestThemeManagerBuilder();

        builder.WithInitialTheme(AntDesignThemes.DaybreakBlue);

        builder.ThemeId.ShouldBe(AntDesignThemes.DaybreakBlue);
        builder.InitialConfig.ShouldBeNull();
    }

    [Fact]
    public void Builder_Initial_Selection_Keeps_Appearance_And_Density()
    {
        var builder = new TestThemeManagerBuilder();
        var selection = ThemeSelection.Of("AntDesign")
                                      .WithAppearance(ThemeAppearance.Dark)
                                      .WithDensity(DesignDensity.Compact);

        builder.WithInitialTheme(selection);

        builder.InitialSelection.ShouldBeSameAs(selection);
        builder.InitialSelection!.Language.ShouldBe("AntDesign");
        builder.InitialSelection.Appearance.ShouldBe(ThemeAppearance.Dark);
        builder.InitialSelection.Density.ShouldBe(DesignDensity.Compact);
    }

    private sealed class TestThemeManagerBuilder : IThemeManagerBuilder
    {
        public IList<ControlPackageRegistration> ControlPackages { get; } = new List<ControlPackageRegistration>();
        public IList<Action<IThemeManager>> Initializers { get; } = new List<Action<IThemeManager>>();
        public string ThemeId { get; private set; } = AntDesignThemes.DaybreakBlue;
        public ThemeConfig? InitialConfig { get; private set; }
        public ThemeSelection? InitialSelection { get; private set; }
        public ThemeRequest? FollowSystemLight { get; private set; }
        public ThemeRequest? FollowSystemDark { get; private set; }

        public void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver)
        {
        }

        public void AddCompiledThemePackage(AtomUI.Theme.Definitions.Compiled.CompiledThemePackage package)
        {
        }

        public IThemeManagerBuilder AddDesignLanguage(DesignLanguageDescriptor language) => this;

        public IThemeManagerBuilder AddControlTokenRules(IEnumerable<ControlTokenRuleDescriptor> derivations) => this;

        public void AddControlPackage(ControlPackageRegistration package)
        {
            ControlPackages.Add(package);
        }

        public void AddInitializer(Action<IThemeManager> initializer)
        {
            Initializers.Add(initializer);
        }

        public void WithInitialTheme(string themeId, ThemeConfig? config = null)
        {
            ThemeId       = themeId;
            InitialConfig = config;
        }

        public void WithInitialTheme(ThemeSelection selection)
        {
            InitialSelection = selection;
        }

        public void WithInitialDesignLanguage<TLanguage>() where TLanguage : DesignLanguage =>
            throw new NotSupportedException();

        public void SetDefaultTheme<TLanguage>(string themeId) where TLanguage : DesignLanguage =>
            throw new NotSupportedException();

        public void SelectValuePackage<TLanguage>(ControlValuePackage package) where TLanguage : DesignLanguage =>
            throw new NotSupportedException();

        public void WithFollowSystemThemes(ThemeRequest light, ThemeRequest dark)
        {
            FollowSystemLight = light;
            FollowSystemDark  = dark;
        }

        public void WithApplicationId(string applicationId)
        {
        }

        public void UseUserThemeDirectory()
        {
        }

        public void UseUserThemeDirectory(string directory)
        {
        }

        public void WithDefaultFontFamily(FontFamily fontFamily)
        {
        }

        public void WithDefaultFontFamily(string fontFamily)
        {
        }
    }
}
