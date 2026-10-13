using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Definitions;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class ThemeInitialModeTests
{
    [Fact]
    public void Builder_Initial_Theme_Keeps_Theme_Id_And_Dark_Algorithm_Config()
    {
        var builder = new TestThemeManagerBuilder();
        var config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build();

        builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID, config);

        builder.ThemeId.ShouldBe(IThemeManager.DEFAULT_THEME_ID);
        builder.InitialConfig.ShouldBeSameAs(config);
        builder.InitialConfig!.Algorithms.ShouldBe([ThemeAlgorithm.Default, ThemeAlgorithm.Dark]);
    }

    [Fact]
    public void Builder_Initial_Theme_Preserves_Explicit_Algorithm_Order()
    {
        var builder = new TestThemeManagerBuilder();
        var config = new ThemeConfigBuilder()
                     .WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Compact, ThemeAlgorithm.Dark)
                     .Build();

        builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID, config);

        builder.InitialConfig!.Algorithms.ShouldBe([
            ThemeAlgorithm.Default,
            ThemeAlgorithm.Compact,
            ThemeAlgorithm.Dark
        ]);
    }

    [Fact]
    public void Builder_Initial_Theme_Allows_Definition_Defaults_When_Config_Is_Null()
    {
        var builder = new TestThemeManagerBuilder();

        builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);

        builder.ThemeId.ShouldBe(IThemeManager.DEFAULT_THEME_ID);
        builder.InitialConfig.ShouldBeNull();
    }

    private sealed class TestThemeManagerBuilder : IThemeManagerBuilder
    {
        public IList<ControlPackageRegistration> ControlPackages { get; } = new List<ControlPackageRegistration>();
        public IList<Action<IThemeManager>> Initializers { get; } = new List<Action<IThemeManager>>();
        public string ThemeId { get; private set; } = IThemeManager.DEFAULT_THEME_ID;
        public ThemeConfig? InitialConfig { get; private set; }
        public ThemeRequest? FollowSystemLight { get; private set; }
        public ThemeRequest? FollowSystemDark { get; private set; }

        public void AddThemeDefinitionResolver(IThemeDefinitionResolver resolver)
        {
        }

        public void AddCompiledThemePackage(AtomUI.Theme.Definitions.Compiled.CompiledThemePackage package)
        {
        }

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
            ThemeId      = themeId;
            InitialConfig = config;
        }

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
