using Acme.LocalizationComponent;
using Acme.LocalizationComponent.Localization;
using Acme.LocalizationConsumer.Localization;
using AtomUI;
using AtomUI.Localization;
using Avalonia;
using Avalonia.Headless;

namespace Acme.LocalizationConsumer;

public static class Program
{
    public static int Main()
    {
        AppBuilder.Configure<FixtureApplication>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .UseAtomUI(builder =>
                  {
                      builder.UseLanguages(
                          LanguageTags.EnUS,
                          [LanguageTags.EnUS, LanguageTags.JaJP]);
                      builder.UseLocalizationComponent();
                  })
                  .SetupWithoutStarting();

        var application = Application.Current ?? throw new InvalidOperationException(
            "The fixture application was not initialized.");
        var localizer = application.GetLocalizer() ?? throw new InvalidOperationException(
            "The fixture localizer was not registered.");
        var manager = application.GetLanguageManager() ?? throw new InvalidOperationException(
            "The fixture language manager was not registered.");

        if (localizer.Get(AppLangResourceKind.Title) != "Language pack fixture" ||
            localizer.Get(WelcomeLangResourceKind.Greeting) != "Welcome")
        {
            return 1;
        }

        manager.ChangeLanguage(LanguageTags.JaJP);
        if (localizer.Get(AppLangResourceKind.Title) != "言語パック フィクスチャ" ||
            localizer.Get(WelcomeLangResourceKind.Greeting) != "ようこそ" ||
            localizer.Format(WelcomeLangResourceKind.ItemCount, 3) != "3 件の項目があります。")
        {
            return 2;
        }

        Console.WriteLine(
            $"ja-JP:Welcome={localizer.Get(WelcomeLangResourceKind.Greeting)};" +
            $"ItemCount={localizer.Format(WelcomeLangResourceKind.ItemCount, 3)}");

        return 0;
    }
}

// 本工程引用 AtomUI 生成器：应用类声明为 partial，由生成器补上 RegisterServices 挂接。
public partial class FixtureApplication : Application;
