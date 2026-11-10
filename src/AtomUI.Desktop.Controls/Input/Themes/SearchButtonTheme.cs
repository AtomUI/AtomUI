using Avalonia.Markup.Xaml;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

internal class SearchButtonTheme : ControlTheme
{
    public SearchButtonTheme()
    {
        AvaloniaXamlLoader.Load(this);
        // This inline theme bypasses the Type-keyed theme resolver. TokenOwner is an internal attached property whose
        // accessors are unavailable to AXAML; the real nested public Button must keep its own palette and motion owner.
        Setters.Add(new Setter(ThemeScope.TokenOwnerProperty, ButtonTokens.Identity));
    }
}
