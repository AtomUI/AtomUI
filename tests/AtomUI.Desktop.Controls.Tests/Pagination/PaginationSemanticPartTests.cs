using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIPagination = AtomUI.Desktop.Controls.Pagination;
using AtomUISimplePagination = AtomUI.Desktop.Controls.SimplePagination;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Pagination;

public class PaginationSemanticPartTests
{
    private const string ItemClass = "semantic-item";

    static PaginationSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Expected_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIPagination), out var paginationDescriptor).ShouldBeTrue();
        paginationDescriptor.ShouldNotBeNull();
        paginationDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "item"]);

        AssertRoot(paginationDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUIPagination));
        AssertItem(paginationDescriptor.Parts.Single(static part => part.Name == "item"),
            "/template/ .semantic-scope-nav > .semantic-item",
            typeof(PaginationItemStyle),
            runtimeCreated: true);

        registry.TryGetControl(typeof(AtomUISimplePagination), out var simpleDescriptor).ShouldBeTrue();
        simpleDescriptor.ShouldNotBeNull();
        simpleDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "info", "item"]);

        AssertRoot(simpleDescriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUISimplePagination));
        AssertItem(simpleDescriptor.Parts.Single(static part => part.Name == "item"),
            "/template/ .semantic-item",
            typeof(SimplePaginationItemStyle),
            runtimeCreated: false);
        AssertInfo(simpleDescriptor.Parts.Single(static part => part.Name == "info"));

        registry.TryGetControl(typeof(AbstractPagination), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(PaginationNav), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(PaginationNavItem), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationTheme.axaml",
        new[] { "semantic-scope-nav:PaginationNav" })]
    [InlineData("src/AtomUI.Desktop.Controls/Pagination/Themes/SimplePaginationTheme.axaml",
        new[] { "semantic-info:TextBlock", "semantic-item:PaginationNavItem", "semantic-item:PaginationNavItem" })]
    public void Built_In_Themes_Declare_Expected_Static_Semantic_Markers(string relativePath, string[] expectedMarkers)
    {
        AssertThemeMarkers(relativePath, expectedMarkers);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationNavTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Pagination/Themes/PaginationNavItemTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Pagination/Themes/QuickJumperBarTheme.axaml")]
    public void Built_In_Themes_Declare_No_Static_Semantic_Markers(string relativePath)
    {
        AssertThemeMarkers(relativePath, Array.Empty<string>());
    }

    [Fact]
    public void Realized_Nav_Items_Are_Item_Parts_When_No_Jump_Item_Is_Visible()
    {
        var pagination = new AtomUIPagination
        {
            Total       = 50,
            CurrentPage = 1
        };

        using var window = Show(pagination);

        var visibleCells = GetVisibleNavItems(pagination);
        visibleCells.Length.ShouldBe(7);
        visibleCells.ShouldNotContain(static cell =>
            cell.PaginationItemType == PaginationItemType.JumpPrevious ||
            cell.PaginationItemType == PaginationItemType.JumpNext);
        visibleCells.ShouldAllBe(static cell => cell.Classes.Contains(ItemClass));
        pagination.Classes.ShouldNotContain("semantic-root");
    }

    [Fact]
    public void Jump_Cells_Are_Excluded_From_The_Item_Part()
    {
        var pagination = new AtomUIPagination
        {
            Total       = 500,
            CurrentPage = 6
        };

        using var window = Show(pagination);

        var visibleCells = GetVisibleNavItems(pagination);
        visibleCells.Length.ShouldBe(9);
        visibleCells.Count(static cell => cell.PaginationItemType is PaginationItemType.JumpPrevious or PaginationItemType.JumpNext).ShouldBe(2);
        visibleCells.Where(static cell => cell.PaginationItemType is not PaginationItemType.JumpPrevious and not PaginationItemType.JumpNext)
                    .ShouldAllBe(static cell => cell.Classes.Contains(ItemClass));
        visibleCells.Where(static cell => cell.PaginationItemType is PaginationItemType.JumpPrevious or PaginationItemType.JumpNext)
                    .ShouldAllBe(static cell => !cell.Classes.Contains(ItemClass));
    }

    [Fact]
    public void Page_Range_Change_Syncs_Item_Markers_Across_Jump_Transitions()
    {
        var pagination = new AtomUIPagination
        {
            Total       = 500,
            CurrentPage = 6
        };

        using var window = Show(pagination);

        var jumpCell = GetVisibleNavItems(pagination).First(
            static cell => cell.PaginationItemType == PaginationItemType.JumpPrevious);
        jumpCell.Classes.ShouldNotContain(ItemClass);

        pagination.CurrentPage = 1;
        Dispatcher.UIThread.RunJobs();

        jumpCell.PaginationItemType.ShouldBe(PaginationItemType.PageIndicator);
        jumpCell.Classes.ShouldContain(ItemClass);

        var visibleCells = GetVisibleNavItems(pagination);
        visibleCells.Length.ShouldBe(9);
        visibleCells.Count(static cell => cell.PaginationItemType == PaginationItemType.JumpNext).ShouldBe(1);
        visibleCells.Where(static cell => cell.PaginationItemType is not PaginationItemType.JumpPrevious and not PaginationItemType.JumpNext)
                    .ShouldAllBe(static cell => cell.Classes.Contains(ItemClass));
        visibleCells.Where(static cell => cell.PaginationItemType is PaginationItemType.JumpPrevious or PaginationItemType.JumpNext)
                    .ShouldAllBe(static cell => !cell.Classes.Contains(ItemClass));
    }

    [Fact]
    public void Generated_Item_Style_Applies_To_Page_Cells_But_Not_Jump_Cells()
    {
        var pagination = new AtomUIPagination
        {
            Total       = 500,
            CurrentPage = 6
        };
        pagination.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUIPagination>().Class("semantic-owner"));
        ownerStyle.Children.Add(new PaginationItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        pagination.Styles.Add(ownerStyle);

        using var window = Show(pagination);

        var markedItems = GetSemanticElements(pagination, ItemClass).OfType<Control>().ToArray();
        markedItems.Length.ShouldBe(7);
        markedItems.ShouldAllBe(static item => Equals(item.Tag, "item"));
        var visibleJumpCells = GetVisibleNavItems(pagination)
            .Where(static cell => cell.PaginationItemType is PaginationItemType.JumpPrevious or PaginationItemType.JumpNext)
            .ToArray();
        visibleJumpCells.ShouldNotBeEmpty();
        visibleJumpCells.ShouldAllBe(static cell => !cell.Classes.Contains(ItemClass));
        visibleJumpCells.ShouldAllBe(static cell => cell.Tag == null);
    }

    [Fact]
    public void Simple_Pagination_Prev_And_Next_Are_Item_Parts_And_Info_And_Jumper_Are_Excluded()
    {
        var pagination = new AtomUISimplePagination
        {
            Total = 50
        };

        using var window = Show(pagination);

        var cells = pagination.GetVisualDescendants().OfType<PaginationNavItem>().ToArray();
        cells.Length.ShouldBe(2);
        cells.ShouldAllBe(static cell => cell.Classes.Contains(ItemClass));

        var infoIndicator = pagination.GetVisualDescendants().Single(static control => control.Name == "PART_InfoIndicator");
        infoIndicator.Classes.ShouldNotContain(ItemClass);

        var quickJumper = pagination.GetVisualDescendants().OfType<QuickJumpEdit>().Single();
        quickJumper.Classes.ShouldNotContain(ItemClass);
    }

    [Fact]
    public void Generated_Simple_Item_Style_Applies_To_Prev_And_Next_Only()
    {
        var pagination = new AtomUISimplePagination
        {
            Total = 50
        };
        pagination.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUISimplePagination>().Class("semantic-owner"));
        ownerStyle.Children.Add(new SimplePaginationItemStyle
        {
            Setters = { new Setter(Control.TagProperty, "item") }
        });
        pagination.Styles.Add(ownerStyle);

        using var window = Show(pagination);

        var markedItems = GetSemanticElements(pagination, ItemClass).OfType<Control>().ToArray();
        markedItems.Length.ShouldBe(2);
        markedItems.ShouldAllBe(static item => Equals(item.Tag, "item"));
        var infoIndicator = pagination.GetVisualDescendants().Single(static control => control.Name == "PART_InfoIndicator");
        var infoControl = infoIndicator as Control;
        infoControl.ShouldNotBeNull();
        infoControl.Tag.ShouldBeNull();
    }

    [Fact]
    public void Generated_Info_Style_Applies_To_The_Info_Indicator_Only()
    {
        var pagination = new AtomUISimplePagination
        {
            Total = 50
        };
        pagination.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUISimplePagination>().Class("semantic-owner"));
        ownerStyle.Children.Add(new SimplePaginationInfoStyle
        {
            Setters = { new Setter(Control.TagProperty, "info") }
        });
        pagination.Styles.Add(ownerStyle);

        using var window = Show(pagination);

        var infoIndicator = pagination.GetVisualDescendants()
                                      .Single(static control => control.Name == "PART_InfoIndicator");
        infoIndicator.Classes.ShouldContain("semantic-info");
        var infoControl = infoIndicator as Control;
        infoControl.ShouldNotBeNull();
        infoControl.Tag.ShouldBe("info");

        GetSemanticElements(pagination, ItemClass).OfType<Control>().ShouldAllBe(static item => item.Tag == null);
        pagination.GetVisualDescendants().OfType<QuickJumpEdit>().Single().Tag.ShouldBeNull();
    }

    private static void AssertRoot(SemanticPartDescriptor part, Type ownerType)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(ownerType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBeNull();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertItem(
        SemanticPartDescriptor part,
        string selectorRoute,
        Type styleType,
        bool runtimeCreated)
    {
        part.Path.ShouldBe("item");
        part.SelectorClass.ShouldBe(ItemClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(typeof(ContentControl));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.StyleType.ShouldBe(styleType);
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertInfo(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("info");
        part.SelectorClass.ShouldBe("semantic-info");
        part.SelectorRoute.ShouldBe("/template/ .semantic-info");
        part.ContractType.ShouldBe(typeof(TextBlock));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBe(typeof(SimplePaginationInfoStyle));
        part.Since.ShouldBe("6.2.0");
    }

    private static PaginationNavItem[] GetVisibleNavItems(Control owner)
    {
        return owner.GetVisualDescendants()
                    .OfType<PaginationNavItem>()
                    .Where(static cell => cell.IsVisible)
                    .ToArray();
    }

    private static Control[] GetSemanticElements(Control owner, string semanticClass)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => control.Classes.Contains(semanticClass))
                    .ToArray();
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 260,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
    }

    private static void AssertThemeMarkers(string relativePath, string[] expectedMarkers)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe(expectedMarkers);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
