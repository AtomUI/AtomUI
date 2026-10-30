using System.Xml.Linq;
using AtomUI.Controls.Commons;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISeparator = AtomUI.Desktop.Controls.Separator;
using AtomUIVerticalSeparator = AtomUI.Desktop.Controls.VerticalSeparator;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace AtomUI.Desktop.Controls.Tests.Separator;

public class SeparatorSemanticPartTests
{
    static SeparatorSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Approved_Separator_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUISeparator), out var separatorDescriptor).ShouldBeTrue();
        separatorDescriptor.ShouldNotBeNull();
        separatorDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "content", "rail"]);

        AssertRoot(separatorDescriptor, typeof(AtomUISeparator));
        AssertPart(separatorDescriptor, "rail", "semantic-rail", typeof(SeparatorRail), SemanticPartCardinality.Multiple);
        AssertPart(separatorDescriptor, "content", "semantic-content", typeof(TextBlock), SemanticPartCardinality.Single);

        // VerticalSeparator reuses the Separator theme and descriptor; it does not register its own.
        manager.SemanticParts.TryGetControl(typeof(AtomUIVerticalSeparator), out var verticalDescriptor).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Separator/Themes/SeparatorTheme.axaml")]
    public void Built_In_Themes_Use_Only_Static_Semantic_Markers(string relativePath)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var literalMarkers = document.Descendants()
                                     .Attributes("Classes")
                                     .Where(static attribute => attribute.Value.Split(
                                         (char[]?)null,
                                         StringSplitOptions.RemoveEmptyEntries)
                                         .Any(static value => value.StartsWith("semantic-", StringComparison.Ordinal)))
                                     .ToArray();
        var propertyMarkers = document.Descendants()
                                      .Attributes()
                                      .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                          "Classes.semantic-",
                                          StringComparison.Ordinal))
                                      .ToArray();

        literalMarkers.ShouldBeEmpty();
        propertyMarkers.ShouldNotBeEmpty();
        propertyMarkers.ShouldAllBe(static attribute =>
            string.Equals(attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        propertyMarkers.ShouldAllBe(static attribute =>
            !string.Equals(attribute.Value, "false", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Default_Themes_Do_Not_Consume_Semantic_Selectors()
    {
        var document = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls/Separator/Themes/SeparatorTheme.axaml"),
            LoadOptions.SetLineInfo);
        var selectors = document.Descendants()
                                .Where(static element => element.Name.LocalName == "Style")
                                .Attributes("Selector")
                                .Select(static attribute => attribute.Value)
                                .ToArray();
        selectors.ShouldAllBe(selector => !selector.Contains("semantic-", StringComparison.Ordinal));
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_Separator_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUISeparator), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var separator = new AtomUISeparator
        {
            Title = "Title"
        };
        separator.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISeparator>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        separator.Styles.Add(ownerStyle);

        using var window = Show(separator);
        separator.Tag.ShouldBe("root");
        FindSemanticControl<AtomUITextBlock>(separator, "semantic-content").Tag.ShouldBe("content");
    }

    [Fact]
    public void Title_Change_Preserves_Content_Marker_Identity_And_Projects_Text()
    {
        var separator = new AtomUISeparator
        {
            Title = "Title"
        };

        using var window = Show(separator);
        var content = FindSemanticControl<AtomUITextBlock>(separator, "semantic-content");
        content.Text.ShouldBe("Title");

        separator.Title = "Restored";
        Dispatcher.UIThread.RunJobs();

        FindSemanticControl<AtomUITextBlock>(separator, "semantic-content").ShouldBeSameAs(content);
        content.Text.ShouldBe("Restored");
    }

    [Fact]
    public void Vertical_Separator_Resolves_To_The_Separator_Descriptor_Contract()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUISeparator), out var separatorDescriptor).ShouldBeTrue();
        separatorDescriptor.ShouldNotBeNull();

        var vertical = new AtomUIVerticalSeparator();
        using var window = Show(vertical);
        FindSemanticControl<AtomUITextBlock>(vertical, "semantic-content").ShouldNotBeNull();
    }

    private static void AssertRoot(ControlSemanticDescriptor descriptor, Type ownerType)
    {
        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.SelectorRoute.ShouldBeNull();
        root.ContractType.ShouldBe(ownerType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.Since.ShouldBe("6.2.0");
    }

    private static void AssertPart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        Type contractType,
        SemanticPartCardinality cardinality)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(cardinality);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.StyleType.ShouldNotBeNull();
        part.Since.ShouldBe("6.2.0");
    }

    private static T FindSemanticControl<T>(Control owner, string semanticClass)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Single(control => control.Classes.Contains(semanticClass));
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 260,
            Content = content
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
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

        throw new FileNotFoundException($"Unable to locate repository file '{relativePath}'.");
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
