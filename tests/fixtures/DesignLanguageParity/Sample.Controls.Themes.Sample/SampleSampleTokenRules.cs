using AtomUI.Generated.SampleControlsThemesSample;
using AtomUI.Theme.Languages;
using Sample.Controls.Themes.AntDesign;
using Sample.DesignLanguage;

namespace Sample.Controls.Themes.Sample;

/// <summary>The sample language's view of <see cref="SampleBadgeToken"/>: the badge takes the brand accent.</summary>
[ControlTokenRule]
internal sealed class SampleSampleBadgeTokenRule : IControlTokenRule<SampleToken, SampleBadgeToken>
{
    public void Apply(in TokenEvaluationContext<SampleToken> context, ControlTokenTarget<SampleBadgeToken> target)
    {
        target.Own.BadgeBackground = context.Language.Accent;
        target.Own.BadgeSize       = target.Global.ControlHeight;
    }
}

public static class SampleSampleControlsExtensions
{
    /// <summary>
    /// Adds the sample language's values of the sample controls, and the Ant Design ones it inherits for the controls it
    /// does not override.
    /// </summary>
    public static DesignLanguageBuilder<SampleLanguage> AddSampleControls(this DesignLanguageBuilder<SampleLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        language.WithAntDesignValues(static ant => ant.AddSampleControls());
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
