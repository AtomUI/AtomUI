using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace Sample.Controls.Themes.AntDesign;

/// <summary>The Ant Design view of <see cref="SampleBadgeToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignSampleBadgeDerivation : IControlDerivation<AntDesignToken, SampleBadgeToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<SampleBadgeToken> view)
    {
        view.Token.BadgeBackground = view.Global.ColorPrimary;
        view.Token.BadgeSize       = view.Global.ControlHeightSM;
    }
}

/// <summary>The Ant Design view of <see cref="SampleTagToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignSampleTagDerivation : IControlDerivation<AntDesignToken, SampleTagToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<SampleTagToken> view)
    {
        view.Token.TagBackground = view.Global.ColorFillSecondary;
    }
}

public static class AntDesignSampleControlsExtensions
{
    /// <summary>Adds the Ant Design values of the sample controls; the application enables them with <c>UseSampleControls()</c>.</summary>
    public static DesignLanguageBuilder<AntDesignLanguage> AddSampleControls(this DesignLanguageBuilder<AntDesignLanguage> language)
    {
        ArgumentNullException.ThrowIfNull(language);
        AtomUI.Generated.SampleControlsThemesAntDesign.GeneratedControlDerivationRegistration.Register(language.Theme);
        return language;
    }
}
