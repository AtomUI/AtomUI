using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Shouldly;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the navigation family (docs/engineering/development/theme-effect-tests.md): NavMenu (with its
/// items, headers and submenu popups), Menu (with MenuItem, MenuItemGroup, MenuSeparator, ContextMenu and the
/// MenuFlyoutPresenter of drop-down buttons), the tab controls, Breadcrumb, Pagination and Tour. Every Active Own Token and
/// every consumed global Token of each member visibly changes the member when overridden through <c>Controls[member]</c>;
/// the rows come from the generated consumption map. Submenus open in Popups, which are Token-owner transparent, so
/// popup rows open them through the overlay popup layer and compare the popup frame.
/// </summary>
public partial class NavigationFamilyEffectTests
{
    private static IEnumerable<object[]> MapRows(string controlId) => EffectTestData.For("AtomUI", controlId).ToArray();

    private static void Pseudo(Control control, string pseudoClass) =>
        ((IPseudoClasses)control.Classes).Add(pseudoClass);

    private static Control OverlayLayerHost(Control content, double width = 640, double height = 480) =>
        FeedbackFamilyEffectTests.OverlayLayerHost(content, width, height);

    private static T WaitForVisual<T>(Func<T?> find, string description)
        where T : Control =>
        FeedbackFamilyEffectTests.WaitForVisual(find, description);

    private static void WaitFor(Func<bool> condition) => FeedbackFamilyEffectTests.WaitFor(condition);

    private static void WithProvider(Control content, Action<ThemeConfigProvider> body) =>
        FeedbackFamilyEffectTests.WithProvider(content, body);

    private static ThemeConfig WithControlToken(ControlTokenIdentity identity, string token, string value) =>
        FeedbackFamilyEffectTests.WithControlToken(identity, token, value);

    /// <summary>The panel of an <see cref="OverlayLayerHost"/>: the area popups opened from its content paint in.</summary>
    private static Control LayerPanelOf(Control anyInWindow) =>
        TopLevel.GetTopLevel(anyInWindow)!.GetVisualDescendants().OfType<Panel>()
                .First(static panel => panel.Width > 0 && panel.Parent is VisualLayerManager);

    private static void Run(
        Func<Control> create,
        Action<Control>? arrange,
        ControlTokenIdentity identity,
        string token,
        EffectOptions? options = null)
    {
        TokenEffectHarness.AssertHasEffect(create, identity, token, arrange, options: options);
    }

    /// <summary>The first visual descendant of <paramref name="root"/> of type T, named <paramref name="name"/> if given.</summary>
    private static T Descendant<T>(Visual root, string? name = null)
        where T : Control =>
        root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => name is null || control.Name == name)
            .ShouldNotBeNull($"No {typeof(T).Name}{(name is null ? string.Empty : " named " + name)} under {root.GetType().Name}.");
}
