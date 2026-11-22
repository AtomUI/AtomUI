using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Token effect tests of the first half of the data family (docs/engineering/development/theme-effect-tests.md): ListBox and
/// ListView (their Token-owner transparent items), TreeView (its Token-owner transparent items; the identity-less node
/// headers and switchers read the tree's Tokens through <c>OwnerTokenResource</c>), the Skeleton placeholders, Avatar /
/// AvatarGroup, Timeline, Statistic, Carousel and the image previewers. Every Active Own Token and every consumed global Token
/// of each member visibly changes the member when overridden through <c>Controls[member]</c>.
/// </summary>
/// <remarks>
/// Items of a Token-owner transparent container read the host's Tokens: the consumption map attributes their theme reads to
/// the item identity (ListBoxItem, ListViewItem, TreeViewItem), and those rows run against the host identity here.
/// Identity-less shared parts are attributed to <c>shared-primitive</c>; their reads are hand lists, drift-guarded against
/// the theme AXAML (<see cref="Owner_Read_Lists_Match_The_Part_Themes"/>).
/// </remarks>
public partial class DataFamilyEffectTests
{
    /// <summary>One identity-less part theme and the global Tokens it reads from its owner.</summary>
    internal sealed record OwnerReadList(string Theme, string[] Tokens, params string[] Code);

    private static IEnumerable<object[]> MapRows(string controlId)
    {
        try
        {
            return EffectTestData.For("AtomUI", controlId).ToArray();
        }
        catch (ArgumentException)
        {
            return [];
        }
    }

    /// <summary>
    /// The member's map rows, plus the global rows of the transparent items it hosts and the owner reads of its parts.
    /// </summary>
    private static IEnumerable<object[]> HostRows(string controlId, string[] transparentItems, OwnerReadList[] parts)
    {
        var rows = MapRows(controlId).ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        var extra = transparentItems.SelectMany(static item => MapRows(item).Select(static row => (string)row[0]))
                                    .Concat(parts.SelectMany(static list => list.Tokens))
                                    .Distinct(StringComparer.Ordinal)
                                    .Order(StringComparer.Ordinal);
        foreach (var token in extra)
        {
            if (seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    private static void Run(
        Func<Control> create,
        Action<Control>? arrange,
        ControlTokenIdentity identity,
        string token,
        EffectOptions? options = null)
    {
        TokenEffectHarness.AssertHasEffect(create, identity, token, arrange, options: options);
    }

    private sealed record Scenario(Func<Control> Create, Action<Control>? Arrange = null, EffectOptions? Options = null);

    private static void Pseudo(Control control, string pseudoClass) =>
        ((IPseudoClasses)control.Classes).Add(pseudoClass);

    /// <summary>The first visual descendant of <paramref name="root"/> of type T, named <paramref name="name"/> if given.</summary>
    internal static T Descendant<T>(Visual root, string? name = null)
        where T : Control =>
        root.GetVisualDescendants().OfType<T>().FirstOrDefault(control => name is null || control.Name == name)
            .ShouldNotBeNull($"No {typeof(T).Name}{(name is null ? string.Empty : " named " + name)} under {root.GetType().Name}.");

    private static void WithProvider(Control content, Action<ThemeConfigProvider> body) =>
        FeedbackFamilyEffectTests.WithProvider(content, body);

    private static void WaitFor(Func<bool> condition) => FeedbackFamilyEffectTests.WaitFor(condition);

    private static ThemeConfig ControlConfig(ControlTokenIdentity identity, string token, string value) =>
        new ThemeConfigBuilder()
            .WithControl(identity, new ControlThemeConfigBuilder().WithToken(token, value).Build())
            .Build();

    // ---- ListBox --------------------------------------------------------------------------------------------------------

    /// <summary>A four-item list: the second item is selected on arrange, the last one disabled.</summary>
    internal static ListBox CreateListBox(
        CustomizableSizeType size = CustomizableSizeType.Middle,
        bool showIndicator = false,
        bool empty = false,
        bool longText = false)
    {
        var listBox = new ListBox
        {
            Width                   = 260,
            SizeType                = size,
            IsShowSelectedIndicator = showIndicator,
            ItemsSource = empty
                ? Array.Empty<ListItemData>()
                : new[]
                {
                    new ListItemData { Content = "Apple pie" },
                    new ListItemData { Content = "Banana split" },
                    new ListItemData { Content = "Cherry tart" },
                    new ListItemData { Content = "Durian cake", IsEnabled = false }
                }
        };
        if (longText)
        {
            listBox.ItemTemplate = TrimmedTextTemplate();
        }

        return listBox;
    }

    /// <summary>
    /// Item texts that run into the check mark, trimmed at the content edge: the gap before the check mark
    /// (SelectedIndicatorMargin) moves the trimming point.
    /// </summary>
    private static Avalonia.Controls.Templates.IDataTemplate TrimmedTextTemplate() =>
        new Avalonia.Controls.Templates.FuncDataTemplate<ListItemData>((data, _) => new Avalonia.Controls.TextBlock
        {
            Text         = data.Content + " with a long description that runs into the check mark",
            TextTrimming = TextTrimming.CharacterEllipsis
        });

    /// <summary>The list on a grey surface with a white fill, so the frame's rounded corners show.</summary>
    private static Control OnSurface(Control list)
    {
        ((TemplatedControl)list).Background = Brushes.White;
        return new Avalonia.Controls.Border { Background = Brushes.LightGray, Padding = new Thickness(8), Child = list };
    }

    private static Scenario RoundedList<T>(Func<T> create)
        where T : TemplatedControl =>
        new(() => OnSurface(create()), null, new EffectOptions
        {
            Target  = static host => Descendant<T>(host),
            Perturb = static _ => new CornerRadius(24)
        });

    private static ListBoxItem ListBoxItemAt(Control list, int index) =>
        ((ListBox)list).ContainerFromIndex(index).ShouldBeOfType<ListBoxItem>();

    public static IEnumerable<object[]> ListBoxRows() => HostRows("ListBox", ["ListBoxItem"], []);

    [Theory]
    [MemberData(nameof(ListBoxRows))]
    public void ListBox_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = ListBoxScenario(token);
        Run(scenario.Create, scenario.Arrange, ListBoxTokens.Identity, token, scenario.Options);
    }

    private static Scenario ListBoxScenario(string token)
    {
        switch (token)
        {
            // Hovered item: background and text.
            case "ItemHoverBgColor" or "ItemHoverColor" or "ColorBgTextHover":
                return new Scenario(static () => CreateListBox(), static list => Pseudo(ListBoxItemAt(list, 0), ":pointerover"));
            // Selected item: background, text and (with the indicator shown) the check mark, its size and margin.
            case "ItemSelectedBgColor" or "ItemSelectedColor" or "ControlItemBgActive" or "ControlItemColorActive" or "ColorText":
                return new Scenario(static () => CreateListBox(), static list => ((ListBox)list).SelectedIndex = 1);
            // The check mark of the selected item (compared on the mark: its colour fills a few pixels only).
            case "ColorPrimary" or "ItemSelectedIndicatorColor":
                return new Scenario(static () => CreateListBox(showIndicator: true), static list => ((ListBox)list).SelectedIndex = 1,
                    new EffectOptions { Region = static list => Descendant<IconTemplatePresenter>(ListBoxItemAt(list, 1), "SelectedIndicator") });
            case "IconSize":
                return new Scenario(static () => CreateListBox(showIndicator: true), static list => ((ListBox)list).SelectedIndex = 1,
                    new EffectOptions { Region = static list => ListBoxItemAt(list, 1) });
            case "SelectedIndicatorMargin" or "UniformlyMarginXXS":
                return new Scenario(static () => CreateListBox(showIndicator: true, longText: true),
                    static list => ((ListBox)list).SelectedIndex = 1,
                    new EffectOptions { Region = static list => ListBoxItemAt(list, 1) });
            // The disabled item's text.
            case "ItemDisabledColor" or "ColorTextDisabled":
                return new Scenario(static () => CreateListBox(), null,
                    new EffectOptions { Region = static list => ListBoxItemAt(list, 3) });
            // Filtering highlights the matched words.
            case "FilterHighlightColor" or "ColorError":
                return new Scenario(static () => CreateListBox(), static list => ((ListBox)list).FilterValue = "a");
            // The item background is transparent by default (ColorTransparent): an override paints it.
            case "ItemBgColor" or "ColorTransparent":
                return new Scenario(static () => CreateListBox());
            // Sizes: item heights and paddings, frame corner radii.
            case "ItemPaddingLG" or "UniformlyPadding" or "ControlHeightLG":
                return new Scenario(static () => CreateListBox(CustomizableSizeType.Large));
            case "ItemPaddingSM" or "UniformlyPaddingXS" or "ControlHeightSM":
                return new Scenario(static () => CreateListBox(CustomizableSizeType.Small));
            // The rounded frame corners show on a filled list on a grey surface.
            case "BorderRadiusLG":
                return RoundedList(static () => CreateListBox(CustomizableSizeType.Large));
            case "BorderRadiusSM":
                return RoundedList(static () => CreateListBox(CustomizableSizeType.Small));
            case "BorderRadius":
                return RoundedList(static () => CreateListBox());
            // The empty indicator's distance from the frame (Large uses PaddingXL, Middle Padding).
            case "PaddingXL":
                return new Scenario(static () => CreateListBox(CustomizableSizeType.Large, empty: true));
            case "Padding":
                return new Scenario(static () => CreateListBox(empty: true));
            default:
                // ColorSplit / BorderThickness (frame border and item split lines), ItemColor / ColorTextSecondary,
                // ItemPadding / UniformlyPaddingSM, ControlHeight, ContentPadding, ItemMargin, EnableMotion.
                return new Scenario(static () => CreateListBox());
        }
    }

    // ---- ListView -------------------------------------------------------------------------------------------------------

    internal static ListView CreateListView(
        CustomizableSizeType size = CustomizableSizeType.Middle,
        bool showIndicator = false,
        bool grouped = false,
        bool paged = false,
        bool empty = false,
        bool longText = false, bool transfer = false)
    {
        ListView listView = transfer ? new TransferListView() : new ListView();
        listView.Width = 260;
        listView.SizeType = size;
        listView.IsShowSelectedIndicator = showIndicator;
        listView.IsGroupEnabled = grouped;
        listView.ItemsSource = empty
                ? Array.Empty<ListItemData>()
                : new[]
                {
                    new ListItemData { Content = "Apple pie", Group = "Fruit" },
                    new ListItemData { Content = "Banana split", Group = "Fruit" },
                    new ListItemData { Content = "Cherry tart", Group = "Tart" },
                    new ListItemData { Content = "Durian cake", Group = "Tart", IsEnabled = false }
                };
        if (longText)
        {
            listView.ItemTemplate = TrimmedTextTemplate();
        }

        if (paged)
        {
            listView.PageSize         = 2;
            listView.BottomPagination = new SimplePagination();
        }

        return listView;
    }

    private static ListViewItem ListViewItemAt(Control list, int index) =>
        list.GetVisualDescendants().OfType<ListViewItem>().Where(static item => !item.IsGroupItem).ElementAt(index);

    private static ListViewItem GroupHeaderOf(Control list) =>
        list.GetVisualDescendants().OfType<ListViewItem>().First(static item => item.IsGroupItem);

    private const string ListViewThemes = "src/AtomUI.Desktop.Controls/ListView/Themes/";

    /// <summary>The group header (identity-less, created by ListView and its derived lists) reads the list's globals.</summary>
    internal static readonly OwnerReadList s_groupHeaderItem =
        new(ListViewThemes + "GroupHeaderItemTheme.axaml", ["ColorFillAlter", "FontWeightStrong"]);

    public static IEnumerable<object[]> ListViewRows() => HostRows("ListView", ["ListViewItem"], [s_groupHeaderItem]);

    [Theory]
    [MemberData(nameof(ListViewRows))]
    public void ListView_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var scenario = ListViewScenario(token);
        Run(scenario.Create, scenario.Arrange, ListViewTokens.Identity, token, scenario.Options);
    }

    internal static void AssertTransferListViewEffect(string token)
    {
        var scenario = ListViewScenario(token, transfer: true);
        Run(scenario.Create, scenario.Arrange, TransferListViewTokens.Identity, token, scenario.Options);
    }

    private static Scenario ListViewScenario(string token, bool transfer = false)
    {
        ListView CreateListView(CustomizableSizeType size = CustomizableSizeType.Middle, bool showIndicator = false,
            bool grouped = false, bool paged = false, bool longText = false, bool empty = false) =>
            DataFamilyEffectTests.CreateListView(size: size, showIndicator: showIndicator, grouped: grouped, paged: paged, empty: empty, longText: longText, transfer: transfer);
        switch (token)
        {
            case "ItemHoverBgColor" or "ItemHoverColor" or "ColorBgTextHover":
                return new Scenario(() => CreateListView(), static list => Pseudo(ListViewItemAt(list, 0), ":pointerover"));
            case "ItemSelectedBgColor" or "ItemSelectedColor" or "ControlItemBgActive" or "ControlItemColorActive" or "ColorText":
                return new Scenario(() => CreateListView(), static list => ((ListView)list).SelectedIndex = 1);
            case "ColorPrimary" or "ItemSelectedIndicatorColor":
                return new Scenario(() => CreateListView(showIndicator: true), static list => ((ListView)list).SelectedIndex = 1,
                    new EffectOptions { Region = static list => Descendant<IconTemplatePresenter>(ListViewItemAt(list, 1), "SelectedIndicator") });
            case "IconSize":
                return new Scenario(() => CreateListView(showIndicator: true), static list => ((ListView)list).SelectedIndex = 1,
                    new EffectOptions { Region = static list => ListViewItemAt(list, 1) });
            case "SelectedIndicatorMargin" or "UniformlyMarginXXS":
                return new Scenario(() => CreateListView(showIndicator: true, longText: true),
                    static list => ((ListView)list).SelectedIndex = 1,
                    new EffectOptions { Region = static list => ListViewItemAt(list, 1) });
            case "ItemDisabledColor" or "ColorTextDisabled":
                return new Scenario(() => CreateListView(), null,
                    new EffectOptions { Region = static list => ListViewItemAt(list, 3) });
            case "ItemBgColor" or "ColorTransparent":
                return new Scenario(() => CreateListView());
            // Group headers: colour, padding, background, weight; the middle size uses FontSizeSM, the large FontSize.
            case "GroupHeaderColor" or "ColorTextDescription" or "GroupHeaderPadding" or "ColorFillAlter" or "FontWeightStrong" or
                "FontSizeSM":
                return new Scenario(() => CreateListView(grouped: true), null,
                    new EffectOptions { Region = static list => GroupHeaderOf(list) });
            case "FontSize":
                return new Scenario(() => CreateListView(CustomizableSizeType.Large, grouped: true), null,
                    new EffectOptions { Region = static list => GroupHeaderOf(list) });
            // The pagination's distance from the items.
            case "PaginationMargin" or "UniformlyMarginXS":
                return new Scenario(() => CreateListView(paged: true));
            case "ItemPaddingLG" or "UniformlyPadding" or "ControlHeightLG":
                return new Scenario(() => CreateListView(CustomizableSizeType.Large));
            case "ItemPaddingSM" or "UniformlyPaddingXS" or "ControlHeightSM":
                return new Scenario(() => CreateListView(CustomizableSizeType.Small));
            case "BorderRadiusLG":
                return RoundedList(() => CreateListView(CustomizableSizeType.Large));
            case "BorderRadiusSM":
                return RoundedList(() => CreateListView(CustomizableSizeType.Small));
            case "BorderRadius":
                return RoundedList(() => CreateListView());
            case "PaddingXL":
                return new Scenario(() => CreateListView(CustomizableSizeType.Large, empty: true));
            case "Padding":
                return new Scenario(() => CreateListView(empty: true));
            default:
                return new Scenario(() => CreateListView());
        }
    }

    [Fact]
    public void List_Motion_Follows_The_List_Tokens()
    {
        // EnableMotion rows are read-backs: the lists and their items switch motion with the list's Token.
        var listBox  = CreateListBox();
        var listView = CreateListView();
        var root = new StackPanel { Spacing = 8, Children = { listBox, listView } };
        WithProvider(root, provider =>
        {
            var boxItem  = ListBoxItemAt(listBox, 0);
            var viewItem = ListViewItemAt(listView, 0);
            listBox.IsMotionEnabled.ShouldBeTrue();
            boxItem.Transitions.ShouldNotBeNull();
            provider.Config = new ThemeConfigBuilder()
                              .WithControl(ListBoxTokens.Identity, new ControlThemeConfigBuilder().WithToken("EnableMotion", "false").Build())
                              .WithControl(ListViewTokens.Identity, new ControlThemeConfigBuilder()
                                                                     .WithToken("MotionDurationMid", "00:00:00.4000000").Build())
                              .Build();
            WaitFor(() => !listBox.IsMotionEnabled);
            listBox.IsMotionEnabled.ShouldBeFalse();
            listView.IsMotionEnabled.ShouldBeTrue();
            // ListViewItem is Token-owner transparent: its TokenTransitions take the ListView's durations.
            viewItem.Transitions.ShouldNotBeNull();
            WaitFor(() => viewItem.Transitions!.OfType<Avalonia.Animation.ITransition>().Any());
            viewItem.Transitions!.OfType<Avalonia.Animation.TransitionBase>()
                    .ShouldAllBe(static transition => transition.Duration == TimeSpan.FromMilliseconds(400));
        });
    }
}
