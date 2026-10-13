using AtomUI.Localization;
using AtomUI.Theme;

namespace AtomUI;

/// <summary>主题和本地化统一提交载体。回调只在两者均已生效后发布。</summary>
internal sealed class StageTwoEvolutionPlan(
    ThemeManager themeManager,
    ThemeRegistrationEvolution theme,
    LanguageManager languageManager,
    LanguageEvolution language)
{
    internal void CommitAndPublish()
    {
        themeManager.ValidateRegistrationEvolution(theme);
        languageManager.ValidateEvolution(language);
        themeManager.CommitRegistrationEvolution(theme);
        languageManager.CommitEvolutionCore(language);
        themeManager.PublishRegistrationEvolution(theme);
        languageManager.PublishEvolvedResourcesChanged();
    }
}
