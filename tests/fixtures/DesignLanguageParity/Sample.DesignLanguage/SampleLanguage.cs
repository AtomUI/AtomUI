using AtomUI;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Generated.SampleDesignLanguage;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace Sample.DesignLanguage;

/// <summary>
/// The language tokens of <see cref="SampleLanguage"/>. Deriving from <see cref="AntDesignToken"/> makes the sample a
/// derived language: every control token rule written for Ant Design applies to it unless a sample one overrides it.
/// </summary>
[LanguageDesignToken]
public partial class SampleToken : AntDesignToken
{
    /// <summary>The brand accent: a stronger primary used for highlighted surfaces.</summary>
    public partial Color Accent { get; set; }
}

/// <summary>
/// A brand language derived from Ant Design. It keeps Ant Design's axes and derivation and changes the primary seed;
/// it reuses the base language through its public contract, as any third-party language can.
/// </summary>
[DesignLanguage]
public sealed class SampleLanguage : DesignLanguage<SampleToken>
{
    /// <summary>The sample brand primary.</summary>
    public static Color BrandPrimary { get; } = Color.Parse("#7C3AED");

    private static readonly AntDesignLanguage s_base = new();

    private static readonly DesignLanguageAxes s_axes = new(
        [AntDesignLanguage.DefaultStyle],
        AntDesignLanguage.DefaultStyle,
        [DesignDensity.Default, DesignDensity.Compact],
        [DesignIdiom.Desktop],
        [GlobalTokenSchema.Get(nameof(DesignToken.ColorPrimary))]);

    public override string Id => "Sample";

    public override DesignLanguageAxes Axes => s_axes;

    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, SampleToken languageSeed)
    {
        s_base.DefineSeeds(variant, seed, languageSeed);
        seed.ColorPrimary = BrandPrimary;
    }

    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, SampleToken language)
    {
        s_base.DeriveLanguage(variant, seed, language);
        language.Accent = Darken(seed.ColorPrimary, variant.Appearance == ThemeAppearance.Dark ? 0.0 : 0.2);
    }

    public override void DeriveGlobal(in TokenEvaluationContext<SampleToken> context, DesignToken token)
    {
        s_base.DeriveGlobal(new TokenEvaluationContext<AntDesignToken>(context.Variant, context.Language), token);
    }

    private static Color Darken(Color color, double amount) =>
        Color.FromArgb(color.A,
            (byte)Math.Round(color.R * (1 - amount)),
            (byte)Math.Round(color.G * (1 - amount)),
            (byte)Math.Round(color.B * (1 - amount)));
}

public static class SampleLanguageBuilderExtensions
{
    /// <summary>
    /// Enables the sample language. <paramref name="configure"/> adds the sample value packages; a value package the
    /// sample does not provide falls back to the Ant Design one, added through <see cref="WithAntDesignValues"/>.
    /// </summary>
    public static IAtomUIBuilder UseSample(this IAtomUIBuilder builder,
        Action<DesignLanguageBuilder<SampleLanguage>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        DesignLanguageRegistration.Register(builder.Theme);
        configure?.Invoke(new DesignLanguageBuilder<SampleLanguage>(builder));
        return builder;
    }

    /// <summary>
    /// Registers Ant Design value packages for the sample language to inherit, without enabling Ant Design itself:
    /// <c>sample.WithAntDesignValues(ant =&gt; ant.AddDesktopControls())</c>.
    /// </summary>
    public static DesignLanguageBuilder<SampleLanguage> WithAntDesignValues(
        this DesignLanguageBuilder<SampleLanguage> language,
        Action<DesignLanguageBuilder<AntDesignLanguage>> configure)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(configure);
        configure(new DesignLanguageBuilder<AntDesignLanguage>(language.Root));
        return language;
    }
}
