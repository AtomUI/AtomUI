using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIGrid = global::AtomUI.Desktop.Controls.DataGrid;
using AtomUIPixelAlignedBorder = AtomUI.Controls.Primitives.PixelAlignedBorder;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Theme;

public class DataGridSemanticPartTests
{
    private const string SectionClass       = "semantic-section";
    private const string TitleClass         = "semantic-title";
    private const string ContentClass       = "semantic-content";
    private const string HeaderWrapperClass = "semantic-header-wrapper";
    private const string HeaderCellClass    = "semantic-header-cell";
    private const string BodyWrapperClass   = "semantic-body-wrapper";
    private const string BodyRowClass       = "semantic-body-row";
    private const string BodyCellClass      = "semantic-body-cell";
    private const string FooterClass        = "semantic-footer";
    private const string PaginationRootClass = "semantic-pagination-root";
    private const string PaginationItemClass = "semantic-item";

    private static readonly DataGridFieldId ValueField = new("value");
    private static readonly DataGridFieldId GroupField = new("group");

    private sealed record Row(int Value, string Group);

    static DataGridSemanticPartTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Registered_Descriptor_Exposes_The_Expected_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIGrid), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(
                  [
                      "root",
                      "body.cell",
                      "body.row",
                      "body.wrapper",
                      "content",
                      "footer",
                      "header.cell",
                      "header.wrapper",
                      "pagination.item",
                      "pagination.root",
                      "section",
                      "title"
                  ]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"), typeof(AtomUIGrid));

        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "section"),
            SectionClass,
            "/template/ .semantic-section",
            typeof(Border),
            SemanticPartCardinality.Single,
            typeof(DataGridSectionStyle),
            runtimeCreated: false);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "header.wrapper"),
            HeaderWrapperClass,
            "/template/ .semantic-header-wrapper",
            typeof(Border),
            SemanticPartCardinality.Single,
            typeof(DataGridHeaderWrapperStyle),
            runtimeCreated: false);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "header.cell"),
            HeaderCellClass,
            ">> .semantic-header-cell",
            typeof(ContentControl),
            SemanticPartCardinality.Multiple,
            typeof(DataGridHeaderCellStyle),
            runtimeCreated: true);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "title"),
            TitleClass,
            "/template/ .semantic-title",
            typeof(AtomUIPixelAlignedBorder),
            SemanticPartCardinality.Single,
            typeof(DataGridTitleStyle),
            runtimeCreated: false);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "body.wrapper"),
            BodyWrapperClass,
            "/template/ .semantic-body-wrapper",
            typeof(DataGridRowsPresenter),
            SemanticPartCardinality.Single,
            typeof(DataGridBodyWrapperStyle),
            runtimeCreated: false);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "body.row"),
            BodyRowClass,
            ">> .semantic-body-row",
            typeof(TemplatedControl),
            SemanticPartCardinality.Multiple,
            typeof(DataGridBodyRowStyle),
            runtimeCreated: true);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "body.cell"),
            BodyCellClass,
            ">> .semantic-body-row >> .semantic-body-cell",
            typeof(DataGridCell),
            SemanticPartCardinality.Multiple,
            typeof(DataGridBodyCellStyle),
            runtimeCreated: true);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "footer"),
            FooterClass,
            "/template/ .semantic-footer",
            typeof(ContentPresenter),
            SemanticPartCardinality.Single,
            typeof(DataGridFooterStyle),
            runtimeCreated: false);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "content"),
            ContentClass,
            "/template/ .semantic-content",
            typeof(Grid),
            SemanticPartCardinality.Single,
            typeof(DataGridContentStyle),
            runtimeCreated: false);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "pagination.root"),
            PaginationRootClass,
            "/template/ .semantic-pagination-root",
            typeof(Pagination),
            SemanticPartCardinality.Multiple,
            typeof(DataGridPaginationRootStyle),
            runtimeCreated: false);
        AssertSelectorPart(
            descriptor.Parts.Single(static part => part.Name == "pagination.item"),
            PaginationItemClass,
            ">> .semantic-pagination-root >> .semantic-item",
            typeof(ContentControl),
            SemanticPartCardinality.Multiple,
            typeof(DataGridPaginationItemStyle),
            runtimeCreated: true);

        registry.TryGetControl(typeof(DataGridRow), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(DataGridCell), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(DataGridRowGroupHeader), out _).ShouldBeFalse();
    }

    [Fact]
    public void Built_In_Theme_Declares_Expected_Static_Semantic_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridTheme.axaml",
        [
            "semantic-body-wrapper:DataGridRowsPresenter",
            "semantic-content:Grid",
            "semantic-footer:ContentPresenter",
            "semantic-header-wrapper:Border",
            "semantic-pagination-root:Pagination",
            "semantic-pagination-root:Pagination",
            "semantic-section:Border",
            "semantic-title:PixelAlignedBorder"
        ]);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridRowTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridCellTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridColumnHeaderTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls.DataGrid/Themes/DataGridRowGroupHeaderTheme.axaml")]
    public void Built_In_Row_Cell_And_Header_Themes_Declare_No_Static_Semantic_Markers(string relativePath)
    {
        AssertThemeMarkers(relativePath, Array.Empty<string>());
    }

    [Fact]
    public void Realized_Grid_Marks_Headers_Rows_And_Cells()
    {
        var grid = LocalGrid(Rows(50), grouped: false, withTitleAndFooter: true);
        using var _ = Show(grid);

        var headers = grid.GetVisualDescendants().OfType<DataGridColumnHeader>().ToArray();
        headers.ShouldNotBeEmpty();
        headers.ShouldAllBe(static header => header.Classes.Contains(HeaderCellClass));

        var rows = grid.GetVisualDescendants().OfType<DataGridRow>().ToArray();
        rows.ShouldNotBeEmpty();
        rows.ShouldAllBe(static row => row.Classes.Contains(BodyRowClass));

        var cells = grid.GetVisualDescendants().OfType<DataGridCell>().ToArray();
        cells.ShouldNotBeEmpty();
        cells.ShouldAllBe(static cell => cell.Classes.Contains(BodyCellClass));

        grid.Classes.ShouldNotContain("semantic-root");
    }

    [Fact]
    public void Group_Headers_Carry_The_Body_Row_Marker()
    {
        var grid = LocalGrid(Rows(50), grouped: true, withTitleAndFooter: false);
        using var _ = Show(grid);

        var groupHeaders = grid.GetVisualDescendants().OfType<DataGridRowGroupHeader>().ToArray();
        groupHeaders.ShouldNotBeEmpty();
        groupHeaders.ShouldAllBe(static header => header.Classes.Contains(BodyRowClass));
    }

    [Fact]
    public void Template_Paginations_Carry_The_Root_Marker()
    {
        var grid = LocalGrid(Rows(50), grouped: false, withTitleAndFooter: false);
        grid.PageSize = 10;
        using var _ = Show(grid);

        var paginations = grid.GetTemplateDescendants().OfType<Pagination>().ToArray();
        paginations.Length.ShouldBe(2);
        paginations.ShouldAllBe(static pagination => pagination.Classes.Contains(PaginationRootClass));
    }

    [Fact]
    public void Recycled_Rows_And_Cells_Keep_Their_Semantic_Markers()
    {
        var grid = LocalGrid(Rows(500), grouped: false, withTitleAndFooter: false);
        using var _ = Show(grid);

        var firstViewportRows = grid.GetVisualDescendants().OfType<DataGridRow>().ToArray();
        firstViewportRows.ShouldNotBeEmpty();

        grid.RequestRangeViewport(300, 12, 1);
        Dispatcher.UIThread.RunJobs();

        var scrolledRows = grid.GetVisualDescendants().OfType<DataGridRow>().ToArray();
        scrolledRows.ShouldNotBeEmpty();
        scrolledRows.ShouldAllBe(static row => row.Classes.Contains(BodyRowClass));
        var scrolledCells = grid.GetVisualDescendants().OfType<DataGridCell>().ToArray();
        scrolledCells.ShouldNotBeEmpty();
        scrolledCells.ShouldAllBe(static cell => cell.Classes.Contains(BodyCellClass));
    }

    [Fact]
    public void Generated_Static_Styles_Apply_To_Template_Nodes()
    {
        var grid = LocalGrid(Rows(10), grouped: false, withTitleAndFooter: true);
        grid.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUIGrid>().Class("semantic-owner"));
        ownerStyle.Children.Add(new DataGridSectionStyle
        {
            Setters = { new Setter(Control.TagProperty, "section") }
        });
        ownerStyle.Children.Add(new DataGridTitleStyle
        {
            Setters = { new Setter(Control.TagProperty, "title") }
        });
        ownerStyle.Children.Add(new DataGridContentStyle
        {
            Setters = { new Setter(Control.TagProperty, "content") }
        });
        ownerStyle.Children.Add(new DataGridHeaderWrapperStyle
        {
            Setters = { new Setter(Control.TagProperty, "header.wrapper") }
        });
        ownerStyle.Children.Add(new DataGridBodyWrapperStyle
        {
            Setters = { new Setter(Control.TagProperty, "body.wrapper") }
        });
        ownerStyle.Children.Add(new DataGridFooterStyle
        {
            Setters = { new Setter(Control.TagProperty, "footer") }
        });
        grid.Styles.Add(ownerStyle);

        using var _ = Show(grid);

        TaggedSingle(grid, SectionClass).ShouldBe("section");
        TaggedSingle(grid, TitleClass).ShouldBe("title");
        TaggedSingle(grid, ContentClass).ShouldBe("content");
        TaggedSingle(grid, HeaderWrapperClass).ShouldBe("header.wrapper");
        TaggedSingle(grid, BodyWrapperClass).ShouldBe("body.wrapper");
        TaggedSingle(grid, FooterClass).ShouldBe("footer");
    }

    [Fact]
    public void Generated_Runtime_Styles_Apply_To_Headers_Rows_And_Cells()
    {
        var grid = LocalGrid(Rows(20), grouped: false, withTitleAndFooter: false);
        grid.Classes.Add("semantic-owner");

        var ownerStyle = new Style(selector => selector.OfType<AtomUIGrid>().Class("semantic-owner"));
        ownerStyle.Children.Add(new DataGridHeaderCellStyle
        {
            Setters = { new Setter(Control.TagProperty, "header.cell") }
        });
        ownerStyle.Children.Add(new DataGridBodyRowStyle
        {
            Setters = { new Setter(Control.TagProperty, "body.row") }
        });
        ownerStyle.Children.Add(new DataGridBodyCellStyle
        {
            Setters = { new Setter(Control.TagProperty, "body.cell") }
        });
        grid.Styles.Add(ownerStyle);

        using var _ = Show(grid);

        grid.GetVisualDescendants().OfType<DataGridColumnHeader>()
            .ShouldAllBe(static header => Equals(header.Tag, "header.cell"));
        grid.GetVisualDescendants().OfType<DataGridRow>()
            .ShouldAllBe(static row => Equals(row.Tag, "body.row"));
        grid.GetVisualDescendants().OfType<DataGridCell>()
            .ShouldAllBe(static cell => Equals(cell.Tag, "body.cell"));
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

    private static void AssertSelectorPart(
        SemanticPartDescriptor part,
        string selectorClass,
        string selectorRoute,
        Type contractType,
        SemanticPartCardinality cardinality,
        Type styleType,
        bool runtimeCreated)
    {
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBe(runtimeCreated);
        part.StyleType.ShouldBe(styleType);
        part.Since.ShouldBe("6.2.0");
    }

    private static IReadOnlyList<Row> Rows(int count)
    {
        var rows = new List<Row>(count);
        for (var i = 0; i < count; i++)
        {
            rows.Add(new Row(i, $"Group {i % 5}"));
        }
        return rows;
    }

    private static AtomUIGrid LocalGrid(IReadOnlyList<Row> rows, bool grouped, bool withTitleAndFooter)
    {
        var descriptor = DataGridLocalSourceDescriptor.For<Row>(
                static row => DataGridRowKey.FromInt64(row.Value + 1))
            .Field(ValueField, static row => row.Value)
            .Field(GroupField, static row => row.Group, canGroup: true);
        var source = DataGridLocalSource.Create(
            rows,
            descriptor,
            new DataGridLocalSourceOptions
            {
                PreferredRangeSize = 64,
                MaximumRangeSize = 64
            });

        var grid = new AtomUIGrid
        {
            AutoGenerateColumns = false,
            Query = grouped
                ? DataGridQuery.Empty.WithGroups(
                    [new DataGridGroup(GroupField, DataGridSortDirection.Ascending)])
                : DataGridQuery.Empty,
            ItemsSource = source,
            RowHeight = 32,
            Width = 460,
            Height = 200,
            IsMotionEnabled = false
        };
        if (withTitleAndFooter)
        {
            grid.Title = "Semantic title";
            grid.Footer = "Semantic footer";
        }
        grid.Columns.Add(new DataGridTextColumn
        {
            FieldId = ValueField,
            Binding = new Avalonia.Data.Binding(nameof(Row.Value))
        });
        return grid;
    }

    private static object? TaggedSingle(Control owner, string semanticClass)
    {
        // semantic-section/semantic-content 等类名在生态中被多控件复用（如内嵌 Spin 的
        // IndicatorLayout）；只统计 DataGrid 模板直接拥有的节点。
        var tagged = owner.GetVisualDescendants()
                          .OfType<Control>()
                          .Where(control => control.Classes.Contains(semanticClass) &&
                                            ReferenceEquals(control.TemplatedParent, owner))
                          .ToArray();
        tagged.Length.ShouldBe(1, $"expected exactly one '{semanticClass}' node templated by the owner");
        return tagged[0].Tag;
    }

    private static IDisposable Show(Control content)
    {
        var window = new Window
        {
            Width = 540,
            Height = 320,
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

    private sealed class WindowLifetime(Window window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
