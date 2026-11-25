using AtomUI.Generated.SampleControlsThemesAntDesign;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace Sample.Controls.Themes.AntDesign;

/// <summary>The Ant Design view of <see cref="SampleBadgeToken"/>.</summary>
[ControlTokenRule]
internal sealed class AntDesignSampleBadgeTokenRule : IControlTokenRule<AntDesignToken, SampleBadgeToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SampleBadgeToken> target)
    {
        target.Own.BadgeBackground = target.Global.ColorPrimary;
        target.Own.BadgeSize       = target.Global.ControlHeightSM;
    }
}

/// <summary>The Ant Design view of <see cref="SampleTagToken"/>.</summary>
[ControlTokenRule]
internal sealed class AntDesignSampleTagTokenRule : IControlTokenRule<AntDesignToken, SampleTagToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SampleTagToken> target)
    {
        target.Own.TagBackground = target.Global.ColorFillSecondary;
    }
}

public static class AntDesignSampleControlsExtensions
{
    /// <summary>Adds the Ant Design values of the sample controls; the application enables them with <c>UseSampleControls()</c>.</summary>
    public static DesignLanguageBuilder<AntDesignLanguage> AddSampleControls(this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        GeneratedControlTokenRuleRegistration.Register(language.Theme);
        return language;
    }
}
