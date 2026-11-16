using System.Xml.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

/// <summary>
/// <c>ToolTipToken.ToolTipShadows</c>: the tooltip popup gets a shadow of its own, separate from the menu popup.
/// The default equals what the tooltip popup drew before (the popup token's <c>BoxShadowsSecondary</c>), so the
/// Ant Design rendering does not change.
/// </summary>
public class ToolTipShadowTests
{
    [AvaloniaFact]
    public async Task Tooltip_Shadow_Is_Unchanged_Under_AntDesign()
    {
        var manager = (AtomUI.Theme.ThemeManager)Application.Current!.GetThemeManager()!;
        await manager.ApplyThemeAsync(new AtomUI.Theme.ThemeRequest(
            AtomUI.Theme.IThemeManager.DEFAULT_THEME_ID, null, AtomUI.Theme.ThemeTransitionReason.UserRequest));
        var snapshot = manager.CurrentSnapshot!;

        Own<BoxShadows>(snapshot, "ToolTipShadows").ShouldBe(Global<BoxShadows>(snapshot, "BoxShadowsSecondary"));
    }

    [AvaloniaFact]
    public void ToolTipShadows_Drives_The_Tooltip_Popup_Shadow()
    {
        // The popup is a runtime-created cross-root node outside the ControlTheme's template chain, so the shadow
        // is relayed in code; the ControlTheme still owns the token read.
        var source = File.ReadAllText(FindRepoFile("src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs"));
        source.ShouldContain("_popup.Bind(Popup.PopupRootShadowProperty, this.GetObservable(ToolTipShadowsProperty))");
        source.ShouldContain("_popup.Bind(Popup.OverlayHostShadowProperty, this.GetObservable(ToolTipShadowsProperty))");

        var document = XDocument.Load(FindRepoFile("src/AtomUI.Desktop.Controls/Tooltip/Themes/ToolTipTheme.axaml"));
        document.ToString().ShouldContain("ToolTipTokenResource ToolTipShadows");
    }

    private static T Global<T>(AtomUI.Theme.Compilation.ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetGlobalToken(name, out var token).ShouldBeTrue(name);
        return snapshot.GlobalTokenValues.Get<T>(token!.Slot);
    }

    private static T Own<T>(AtomUI.Theme.Compilation.ThemeSnapshot snapshot, string name)
    {
        snapshot.Registry.TryGetControl(new AtomUI.Theme.Schema.ControlTokenIdentity("AtomUI", "ToolTip"), out var control)
                 .ShouldBeTrue();
        var token = control!.OwnTokens.Single(item => item.Name == name);
        return snapshot.Controls[control.Slot].ControlTokenValues.Get<T>(token.Slot);
    }

    private static string FindRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent!;
        }

        throw new InvalidOperationException($"Repository file '{relativePath}' was not found.");
    }
}
