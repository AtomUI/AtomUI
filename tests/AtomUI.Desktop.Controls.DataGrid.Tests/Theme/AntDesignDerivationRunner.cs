using System.Reflection;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls.Themes.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

/// <summary>
/// Runs the Ant Design value package derivation of one control token against a hand-built global token, so token
/// tests can check the derived values without compiling a whole theme. The derivation classes are internal to their
/// value packages, so they are located by the public interface they implement.
/// </summary>
internal static class AntDesignDerivationRunner
{
    private static readonly Assembly[] s_valuePackages =
    [
        typeof(AntDesignDataGridExtensions).Assembly,
        typeof(AntDesignDesktopControlsExtensions).Assembly
    ];

    public static TToken Derive<TToken>(DesignToken global, ThemeAppearance appearance = ThemeAppearance.Light)
        where TToken : AbstractControlDesignToken, new()
    {
        var token = new TToken();
        Derive(token, global, appearance);
        return token;
    }

    public static void Derive<TToken>(TToken token, DesignToken global,
                                      ThemeAppearance appearance = ThemeAppearance.Light)
        where TToken : AbstractControlDesignToken
    {
        var derivation = CreateDerivation<TToken>();
        var variant = new DesignVariant(AntDesignLanguage.DefaultStyle, appearance, DesignDensity.Default,
            DesignIdiom.Desktop);
        derivation.Derive(new DerivationContext<AntDesignToken>(variant, new AntDesignToken()),
            new ControlView<TToken>(token, global));
    }

    private static IControlDerivation<AntDesignToken, TToken> CreateDerivation<TToken>()
        where TToken : AbstractControlDesignToken
    {
        var contract = typeof(IControlDerivation<AntDesignToken, TToken>);
        var type = s_valuePackages.SelectMany(static assembly => assembly.GetTypes())
                                  .Single(candidate => !candidate.IsAbstract && contract.IsAssignableFrom(candidate));
        return (IControlDerivation<AntDesignToken, TToken>)Activator.CreateInstance(type, nonPublic: true)!;
    }
}
