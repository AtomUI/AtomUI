using System.Reflection;
using AtomUI.Controls.Themes.AntDesign;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls.Themes.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Tests.Theme;

/// <summary>
/// Runs the Ant Design value package rule of one control token against a hand-built global token, so token
/// tests can check the derived values without compiling a whole theme. The rule classes are internal to their
/// value packages, so they are located by the public interface they implement.
/// </summary>
internal static class AntDesignTokenRuleRunner
{
    private static readonly Assembly[] s_valuePackages =
    [
        typeof(AntDesignDesktopControlsExtensions).Assembly,
        typeof(AntDesignCommonControlsExtensions).Assembly
    ];

    public static TToken CreateToken<TToken>(DesignToken global, ThemeAppearance appearance = ThemeAppearance.Light)
        where TToken : AbstractControlDesignToken, new()
    {
        var token = new TToken();
        Apply(token, global, appearance);
        return token;
    }

    public static void Apply<TToken>(TToken token, DesignToken global,
                                      ThemeAppearance appearance = ThemeAppearance.Light)
        where TToken : AbstractControlDesignToken
    {
        var rule = CreateRule<TToken>();
        var variant = new DesignVariant(AntDesignLanguage.DefaultStyle, appearance, DesignDensity.Default,
            DesignIdiom.Desktop);
        rule.Apply(new TokenEvaluationContext<AntDesignToken>(variant, new AntDesignToken()),
            new ControlTokenTarget<TToken>(token, global));
    }

    private static IControlTokenRule<AntDesignToken, TToken> CreateRule<TToken>()
        where TToken : AbstractControlDesignToken
    {
        var contract = typeof(IControlTokenRule<AntDesignToken, TToken>);
        var type = s_valuePackages.SelectMany(static assembly => assembly.GetTypes())
                                  .Single(candidate => !candidate.IsAbstract && contract.IsAssignableFrom(candidate));
        return (IControlTokenRule<AntDesignToken, TToken>)Activator.CreateInstance(type, nonPublic: true)!;
    }
}
