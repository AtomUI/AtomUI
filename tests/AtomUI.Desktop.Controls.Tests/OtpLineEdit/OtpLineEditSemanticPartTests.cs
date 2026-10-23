using System.Xml.Linq;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIOtpLineEdit = AtomUI.Desktop.Controls.OtpLineEdit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.OtpLineEdit;

public class OtpLineEditSemanticPartTests
{
    static OtpLineEditSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_The_Expected_Parts()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIOtpLineEdit), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(
            ["root", "cell", "cellList", "separator"]);

        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.SelectorClass.ShouldBeNull();
        root.ContractType.ShouldBe(typeof(AtomUIOtpLineEdit));
        root.StyleType.ShouldBeNull();

        var cellList = descriptor.Parts.Single(static part => part.Name == "cellList");
        cellList.SelectorClass.ShouldBe("semantic-cell-list");
        cellList.SelectorRoute.ShouldBe("/template/ .semantic-cell-list");
        cellList.ContractType.ShouldBe(typeof(ItemsControl));
        cellList.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        cellList.RuntimeCreated.ShouldBeFalse();
        cellList.Since.ShouldBe("6.2.0");
        cellList.StyleType.ShouldBe(typeof(AtomUI.Theme.Styling.OtpLineEditCellListStyle));

        var cell = descriptor.Parts.Single(static part => part.Name == "cell");
        cell.SelectorClass.ShouldBe("semantic-cell");
        cell.SelectorRoute.ShouldBe("/template/ .semantic-cell-list >> .semantic-scope-cell > .semantic-cell");
        cell.ContractType.ShouldBe(typeof(ContentControl));
        cell.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        cell.RuntimeCreated.ShouldBeTrue();
        cell.Since.ShouldBe("6.2.0");
        cell.StyleType.ShouldBe(typeof(AtomUI.Theme.Styling.OtpLineEditCellStyle));

        var separator = descriptor.Parts.Single(static part => part.Name == "separator");
        separator.SelectorClass.ShouldBe("semantic-separator");
        separator.SelectorRoute.ShouldBe("/template/ .semantic-cell-list >> .semantic-scope-cell > .semantic-separator");
        separator.ContractType.ShouldBe(typeof(Border));
        separator.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        separator.RuntimeCreated.ShouldBeTrue();
        separator.Since.ShouldBe("6.2.0");
        separator.StyleType.ShouldBe(typeof(AtomUI.Theme.Styling.OtpLineEditSeparatorStyle));

        registry.TryGetControl(typeof(OtpLineEditCell), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(OtpTextBox), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/OtpLineEdit/Themes/OtpLineEditTheme.axaml",
        new[]
        {
            "semantic-cell-list:ItemsControl",
            "semantic-cell:OtpLineEditCell",
            "semantic-scope-cell:StackPanel",
            "semantic-separator:Border"
        })]
    [InlineData("src/AtomUI.Desktop.Controls/OtpLineEdit/Themes/OtpLineEditCellTheme.axaml",
        new string[0])]
    [InlineData("src/AtomUI.Desktop.Controls/OtpLineEdit/Themes/OtpTextBoxTheme.axaml",
        new string[0])]
    public void Built_In_Themes_Declare_Expected_Static_Semantic_Markers(string relativePath, string[] expectedMarkers)
    {
        AssertThemeMarkers(relativePath, expectedMarkers);
    }

    [Fact]
    public void Realized_Cells_Carry_The_Cell_Part_Marker()
    {
        var view = new OtpLineEditSemanticStylesView();

        using var _ = Show(view);
        var otp = view.GetVisualDescendants().OfType<AtomUIOtpLineEdit>().Single();

        otp.Classes.ShouldNotContain("semantic-root");
        var cells = otp.GetVisualDescendants()
                       .OfType<OtpLineEditCell>()
                       .Where(static candidate => candidate.Classes.Contains("semantic-cell"))
                       .ToArray();
        cells.Length.ShouldBe(6);
        cells.ShouldAllBe(static cell => cell.IsAttachedToVisualTree());
        foreach (var cell in cells)
        {
            cell.BorderBrush.ShouldNotBeNull().ShouldBeAssignableTo<ISolidColorBrush>().Color.ShouldBe(Colors.Fuchsia);
            cell.Padding.ShouldBe(new Thickness(3));
        }

        var separators = otp.GetVisualDescendants().OfType<Border>()
                            .Where(static node => node.Classes.Contains("semantic-separator"))
                            .ToArray();
        separators.Length.ShouldBe(6);
        separators.ShouldAllBe(static node => node.Margin == new Thickness(9, 0, 0, 0));

        var fixture = XDocument.Load(GetRepoFile(
            "tests/AtomUI.Desktop.Controls.Tests/OtpLineEdit/OtpLineEditSemanticStylesView.axaml"));
        fixture.Descendants().Count(static node => node.Name.LocalName == "OtpLineEditCellStyle").ShouldBe(1);
        fixture.Descendants().Count(static node => node.Name.LocalName == "OtpLineEditSeparatorStyle").ShouldBe(1);
        fixture.Descendants().Attributes().ShouldNotContain(static attribute =>
            attribute.Name.LocalName == "Name" || attribute.Name.LocalName == "Loaded" ||
            attribute.Name.LocalName == "Unloaded");
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

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}

public partial class OtpLineEditSemanticStylesView : UserControl
{
    public OtpLineEditSemanticStylesView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
