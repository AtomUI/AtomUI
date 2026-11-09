using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the layout / container family (docs/engineering/development/theme-effect-tests.md): Card (with
/// CardActionButton, CardMetaContent and the Token-owner transparent CardGridItem / CardTabsContent; the identity-less
/// CardActionPanel reads the Card's Tokens), Collapse (CollapseItem is Token-owner transparent), Expander, Splitter (the
/// identity-less handle and drag bar read the Splitter's Tokens), SplitView, Separator, Space (with CompactSpaceAddOn),
/// Masonry and GroupBox. Every Active Own Token and every consumed global Token of each member visibly changes the member when
/// overridden through <c>Controls[member]</c>; the rows come from the generated consumption map.
/// </summary>
public partial class LayoutFamilyEffectTests
{
    private static IEnumerable<object[]> MapRows(string controlId) => EffectTestData.For("AtomUI", controlId).ToArray();

    private static void Pseudo(Control control, string pseudoClass) =>
        ((IPseudoClasses)control.Classes).Add(pseudoClass);

    private static void Run(
        Func<Control> create,
        Action<Control>? arrange,
        ControlTokenIdentity identity,
        string token,
        EffectOptions? options = null)
    {
        TokenEffectHarness.AssertHasEffect(create, identity, token, arrange, options: options);
    }

    private static void WithProvider(Control content, Action<ThemeConfigProvider> body) =>
        FeedbackFamilyEffectTests.WithProvider(content, body);

    private static ThemeConfig WithControlToken(ControlTokenIdentity identity, string token, string value) =>
        FeedbackFamilyEffectTests.WithControlToken(identity, token, value);

    private static void WaitFor(Func<bool> condition) => FeedbackFamilyEffectTests.WaitFor(condition);

    /// <summary>The first visual descendant of <paramref name="root"/> of type T, named <paramref name="name"/> if given.</summary>
    private static T Descendant<T>(Visual root, string? name = null)
        where T : Control =>
        root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => name is null || control.Name == name)
            .ShouldNotBeNull($"No {typeof(T).Name}{(name is null ? string.Empty : " named " + name)} under {root.GetType().Name}.");

    // ---- Card -----------------------------------------------------------------------------------------------------------

    /// <summary>A titled card with an extra, a text body and an action row of two icon actions and one text action.</summary>
    internal static Card CreateCard(
        SizeType size = SizeType.Middle,
        CardStyleVariant variant = CardStyleVariant.Outlined,
        bool inner = false,
        bool hoverable = false,
        object? content = null)
    {
        var card = new Card
        {
            Width        = 360,
            Header       = "Card title",
            Extra        = "More",
            Content      = content ?? "Card content",
            SizeType     = size,
            StyleVariant = variant,
            IsInnerMode  = inner,
            IsHoverable  = hoverable
        };
        card.Actions.Add(new CardActionButton { Icon = new SettingOutlined() });
        card.Actions.Add(new CardActionButton { Icon = new EditOutlined() });
        card.Actions.Add(new AvaloniaTextBlock { Text = "Share", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
        return card;
    }

    private static CardGridContent CreateGrid(bool hoverable = true)
    {
        var grid = new CardGridContent
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            RowDefinitions    = new RowDefinitions("Auto,Auto"),
            IsHoverable       = hoverable
        };
        grid.Items.Add(new CardGridItem { Row = 0, Column = 0, Content = "Grid 1" });
        grid.Items.Add(new CardGridItem { Row = 0, Column = 1, Content = "Grid 2" });
        grid.Items.Add(new CardGridItem { Row = 1, Column = 0, Content = "Grid 3" });
        grid.Items.Add(new CardGridItem { Row = 1, Column = 1, Content = "Grid 4" });
        return grid;
    }

    private static CardTabsContent CreateTabs()
    {
        var tabs = new CardTabsContent();
        tabs.Items.Add(new TabItem { Header = "Tab 1", Content = "Content of tab 1" });
        tabs.Items.Add(new TabItem { Header = "Tab 2", Content = "Content of tab 2" });
        return tabs;
    }

    private static Control ActionsOf(Control card) => Descendant<CardActionPanel>(card, "PART_ActionPanel");

    private static Control HeaderOf(Control card) => Descendant<Control>(card, "HeaderFrame");

    public static IEnumerable<object[]> CardRows() => MapRows("Card");

    [Theory]
    [MemberData(nameof(CardRows))]
    public void Card_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = CardScenario(token);
        Run(scenario.Create, scenario.Arrange, CardTokens.Identity, token, scenario.Options);
    }

    private sealed record Scenario(Func<Control> Create, Action<Control>? Arrange = null, EffectOptions? Options = null);

    private static Scenario CardScenario(string token)
    {
        switch (token)
        {
            // The large size: header height / padding / font, body padding.
            case "HeaderHeightLG" or "HeaderPaddingLG" or "HeaderFontSizeLG" or "BodyPaddingLG" or "FontHeightLG" or
                "UniformlyPaddingLG" or "PaddingLG" or "FontSizeLG" or "UniformlyPadding":
                return new Scenario(static () => CreateCard(SizeType.Large));
            // The small size.
            case "HeaderHeightSM" or "HeaderPaddingSM" or "HeaderFontSizeSM" or "BodyPaddingSM" or "FontHeightSM" or "PaddingXS":
                return new Scenario(static () => CreateCard(SizeType.Small));
            // The header background is transparent by default: perturb to an opaque colour.
            case "HeaderBg":
                return new Scenario(static () => CreateCard(), null,
                    new EffectOptions { Perturb = static _ => Colors.Red, Region = HeaderOf });
            case "ExtraColor" or "ColorText":
                return new Scenario(static () => CreateCard(), null,
                    new EffectOptions { Region = static card => Descendant<ContentPresenter>(card, "HeaderExtra") });
            // The action row: background, icon size, text action colour / size, min height, separators.
            case "ActionsBg" or "CardActionsIconSize" or "ColorTextDescription" or "ControlHeightLG":
                return new Scenario(static () => CreateCard(), null, new EffectOptions { Region = ActionsOf });
            case "ColorPrimary":
                return new Scenario(static () => CreateCard(), static card => Pseudo(ActionsOf(card), ":pointerover"),
                    new EffectOptions { Region = ActionsOf });
            // A hoverable card shows the card shadow on hover.
            case "CardShadows":
                return new Scenario(static () => CreateCard(hoverable: true), static card => Pseudo(card, ":pointerover"));
            // The resting shadow of an outlined card is transparent by default: perturb to a visible shadow.
            case "RestingShadows":
                return new Scenario(static () => CreateCard(), null,
                    new EffectOptions { Perturb = static _ => BoxShadows.Parse(VisibleRestingShadows) });
            // A borderless card rests on the tertiary shadow.
            case "BoxShadowsTertiary":
                return new Scenario(static () => CreateCard(variant: CardStyleVariant.Borderless));
            // An inner card fills its header with the alter fill.
            case "ColorFillAlter":
                return new Scenario(static () => CreateCard(inner: true), null, new EffectOptions { Region = HeaderOf });
            // Grid items draw their edges with the grid shadows (LineWidth wide, ColorBorderSecondary).
            case "CardGridItemShadows" or "LineWidth":
                return new Scenario(static () => CreateCard(content: CreateGrid(hoverable: false)));
            // The tabs content: the tab bar edge padding of the middle size.
            case "UniformlyPaddingSM":
                return new Scenario(static () => CreateCard(content: CreateTabs()));
            // The rounded corners: on a grey surface the card's container fill shows the corner shape.
            case "BorderRadiusLG":
                return new Scenario(
                    static () => new Avalonia.Controls.Border { Background = Brushes.LightGray, Padding = new Thickness(8), Child = CreateCard() },
                    null,
                    new EffectOptions
                    {
                        Target  = static host => Descendant<Card>(host),
                        Perturb = static _ => new CornerRadius(24)
                    });
            default:
                return new Scenario(static () => CreateCard());
        }
    }

    public static IEnumerable<object[]> CardActionButtonRows() => MapRows("CardActionButton");

    [Theory]
    [MemberData(nameof(CardActionButtonRows))]
    public void CardActionButton_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => CreateCard(),
            token == "ColorPrimary" ? static button => Pseudo(button, ":pointerover") : null,
            CardActionButtonTokens.Identity, token,
            new EffectOptions { Target = static card => Descendant<CardActionButton>(card) });
    }

    public static IEnumerable<object[]> CardMetaContentRows() => MapRows("CardMetaContent");

    [Theory]
    [MemberData(nameof(CardMetaContentRows))]
    public void CardMetaContent_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(static () => new CardMetaContent
            {
                Width   = 320,
                Avatar  = new Avatar { Text = "A" },
                Header  = "Card title",
                Content = "This is the description"
            }, null, CardMetaContentTokens.Identity, token);
    }

    private const string VisibleRestingShadows = "0 1 3 0 #40000000, 0 1 2 -1 #40000000, 0 4 12 2 #30000000";

    [Fact]
    public void Card_Resting_Shadow_Yields_To_Borderless_And_Hover_Shadows()
    {
        // RestingShadows styles the resting outlined card only: a borderless card keeps BoxShadowsTertiary and a hovered
        // hoverable card swaps to CardShadows (with no border).
        var outlined   = CreateCard();
        var borderless = CreateCard(variant: CardStyleVariant.Borderless);
        var hoverable  = CreateCard(hoverable: true);
        var root = new StackPanel { Spacing = 8, Children = { outlined, borderless, hoverable } };
        WithProvider(root, provider =>
        {
            // Motion off: the shadow transition would otherwise still be running when the values are read.
            provider.Config = IsolationConfig(control: (CardTokens.Identity, "RestingShadows", VisibleRestingShadows));
            var expected = BoxShadows.Parse(VisibleRestingShadows);
            WaitFor(() => outlined.BoxShadow == expected);
            outlined.BoxShadow.ShouldBe(expected);
            hoverable.BoxShadow.ShouldBe(expected);
            borderless.BoxShadow.ShouldBe(
                ControlTokenAccessor.Capture(borderless).GetEffectiveGlobal<BoxShadows>(SharedTokenKind.BoxShadowsTertiary));

            Pseudo(hoverable, ":pointerover");
            Avalonia.Threading.Dispatcher.UIThread.RunJobs();
            WaitFor(() => hoverable.BoxShadow != expected);
            hoverable.BoxShadow.ShouldBe(ControlTokenAccessor.Capture(hoverable).GetOwn<BoxShadows>(CardTokenKind.CardShadows));
            hoverable.BorderThickness.ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Card_Grid_Item_Hover_Shadow_Follows_The_Card()
    {
        // CardGridItem is Token-owner transparent: its hover shadow is the Card's CardShadows.
        TokenEffectHarness.AssertHasEffect(static () => CreateCard(content: CreateGrid()), CardTokens.Identity, "CardShadows",
            static card => Pseudo(Descendant<CardGridItem>(card), ":pointerover"),
            options: new EffectOptions { Region = static card => Descendant<CardGridItem>(card) });
    }

    [Fact]
    public void Card_Tabs_Content_Padding_Follows_The_Card()
    {
        // CardTabsContent is Token-owner transparent: the tab content padding is the Card's BodyPadding.
        TokenEffectHarness.AssertHasEffect(static () => CreateCard(content: CreateTabs()), CardTokens.Identity, "BodyPadding",
            options: new EffectOptions { Region = static card => Descendant<CardTabsContent>(card) });
    }
}
