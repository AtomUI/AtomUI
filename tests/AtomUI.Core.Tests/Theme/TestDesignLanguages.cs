using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Core.Tests.Theme;

/// <summary>The design languages the Core tests compile against.</summary>
internal static class TestDesignLanguages
{
    /// <summary>The Ant Design language as its package registers it (one shared descriptor, as in an application).</summary>
    internal static DesignLanguageDescriptor AntDesign { get; } = CaptureAntDesign();

    /// <summary>A minimal language whose derivations count their calls; see <see cref="ProbeLanguage"/>.</summary>
    internal static DesignLanguageDescriptor Probe(ProbeLanguage? language = null) =>
        new(language ?? new ProbeLanguage(), Array.Empty<TokenDescriptor>());

    /// <summary>
    /// Enables Ant Design on a theme manager builder the way <c>UseAntDesign()</c> does: the language and its compiled
    /// theme package (DaybreakBlue), plus the Ant Design values of the test controls.
    /// </summary>
    internal static ThemeManagerBuilder AddAntDesign(this ThemeManagerBuilder builder)
    {
        builder.AddDesignLanguage(AntDesign);
        AtomUI.Generated.AtomUIDesignLanguagesAntDesign.GeneratedThemePackageRegistration.Register(builder);
        // The Ant Design values of this project's hand-written test controls (dormant when a test does not use them).
        builder.AddControlDerivations(TestControlDerivations.All);
        return builder;
    }

    /// <summary>Starts AtomUI on <paramref name="application"/> with Ant Design enabled and the test's own configuration.</summary>
    internal static void AttachWithAntDesign(Avalonia.Application application, Action<IAtomUIBuilder> configure) =>
        AtomUI.ApplicationStartup.AttachWith(application, builder =>
        {
            AtomUI.DesignLanguages.AntDesign.AntDesignBuilderExtensions.UseAntDesign(builder);
            configure(builder);
        });

    private static DesignLanguageDescriptor CaptureAntDesign()
    {
        var builder = new ThemeManagerBuilder();
        AtomUI.Generated.AtomUIDesignLanguagesAntDesign.GeneratedDesignLanguageRegistration.Register(builder);
        return builder.Languages.Single();
    }
}

/// <summary>The language token of <see cref="ProbeLanguage"/>; it declares no tokens.</summary>
[LanguageDesignToken]
public partial class ProbeLanguageToken : LanguageDesignToken;

/// <summary>
/// A minimal design language for compiler tests: the seeds stay at their defaults, the global derivation writes
/// <c>ColorPrimaryHover</c> from <c>ColorPrimary</c> and <c>ControlHeight</c> by density, and every call is counted.
/// </summary>
public sealed class ProbeLanguage : DesignLanguage<ProbeLanguageToken>
{
    // Declared before the axes: static initializers run in textual order.
    public static DesignStyle ProbeStyle { get; } = new("Default");

    private static readonly DesignLanguageAxes s_axes = new(
        [ProbeStyle], ProbeStyle, [DesignDensity.Default, DesignDensity.Compact], [DesignIdiom.Desktop], []);

    public override string Id => "Probe";

    public override DesignLanguageAxes Axes => s_axes;

    public int DefineSeedsCalls { get; private set; }
    public int DeriveLanguageCalls { get; private set; }
    public int DeriveGlobalCalls { get; private set; }

    /// <summary>When set, <see cref="DeriveGlobal"/> throws.</summary>
    public bool FailGlobal { get; init; }

    public override void DefineSeeds(in DesignVariant variant, DesignToken seed, ProbeLanguageToken languageSeed) =>
        DefineSeedsCalls++;

    public override void DeriveLanguage(in DesignVariant variant, DesignToken seed, ProbeLanguageToken language) =>
        DeriveLanguageCalls++;

    public override void DeriveGlobal(in DerivationContext<ProbeLanguageToken> context, DesignToken token)
    {
        DeriveGlobalCalls++;
        if (FailGlobal)
        {
            throw new InvalidOperationException("Probe global derivation failed.");
        }

        token.ColorPrimaryHover = Color.FromArgb(0x80, token.ColorPrimary.R, token.ColorPrimary.G, token.ColorPrimary.B);
        token.ControlHeight = context.Variant.Density == DesignDensity.Compact ? 24 : 32;
        token.ColorBgBase = context.Variant.IsDark ? Colors.Black : Colors.White;
    }
}
