using AtomUI.Theme.Languages;
using Sample.Controls.Themes.AntDesign;
using Sample.DesignLanguage;

namespace Sample.Controls.Themes.Sample;

/// <summary>The sample language's view of <see cref="SampleBadgeToken"/>: the badge takes the brand accent.</summary>
[ControlDerivation]
internal sealed class SampleSampleBadgeDerivation : IControlDerivation<SampleToken, SampleBadgeToken>
{
    public void Derive(in DerivationContext<SampleToken> context, ControlView<SampleBadgeToken> view)
    {
        view.Token.BadgeBackground = context.Language.Accent;
        view.Token.BadgeSize       = view.Global.ControlHeight;
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
        AtomUI.Generated.SampleControlsThemesSample.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
