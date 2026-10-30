using System.Xml.Linq;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

public class ButtonSemanticPartTests
{
    private const string IconClass = "semantic-icon";
    private const string ContentClass = "semantic-content";

    static ButtonSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_Root_Icon_And_Content()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUIButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(["root", "content", "icon"]);

        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.ContractType.ShouldBe(typeof(AtomUIButton));
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);

        var icon = descriptor.Parts.Single(static part => part.Name == "icon");
        icon.Path.ShouldBe("icon");
        icon.SelectorClass.ShouldBe(IconClass);
        icon.ContractType.ShouldBe(typeof(Control));
        icon.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        icon.Customization.ShouldBe(SemanticPartCustomization.Selector);
        icon.Since.ShouldBe("6.2.0");

        var content = descriptor.Parts.Single(static part => part.Name == "content");
        content.Path.ShouldBe("content");
        content.SelectorClass.ShouldBe(ContentClass);
        content.ContractType.ShouldBe(typeof(ContentPresenter));
        content.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        content.Customization.ShouldBe(SemanticPartCustomization.Selector);
        content.Since.ShouldBe("6.2.0");
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.axaml")]
    public void Every_Built_In_Template_Implements_The_Same_Marker_Contract(string relativePath)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var templates = document.Descendants()
                                .Where(static element => element.Name.LocalName == "ControlTemplate")
                                .ToArray();
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
                                           .Attributes()
                                           .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                               "Classes.semantic-",
                                               StringComparison.Ordinal))
                                           .ToArray();

        templates.Length.ShouldBe(3);
        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.Length.ShouldBe(9);
        classPropertyMarkers.ShouldAllBe(static attribute =>
            string.Equals(attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        foreach (var template in templates)
        {
            FindMarkedElements(template, IconClass).Count.ShouldBe(2);
            var contents = FindMarkedElements(template, ContentClass);
            contents.Count.ShouldBe(1);
            contents[0].Name.LocalName.ShouldBe(nameof(ContentPresenter));
            FindMarkedElements(template, "semantic-root").ShouldBeEmpty();
        }
    }

    [Theory]
    [InlineData("application")]
    [InlineData("local")]
    [InlineData("owner")]
    public void Native_Avalonia_Selector_Scopes_Match_Semantic_Markers(string scope)
    {
        var ownerClass = $"semantic-scope-{scope}";
        var button = new AtomUIButton
        {
            Content = "Semantic Button"
        };
        button.Classes.Add(ownerClass);
        var window = new AvaloniaWindow
        {
            Width = 320,
            Height = 120,
            Content = button
        };
        var iconStyle = CreateMarkerStyle(IconClass, "icon-style", ownerClass);
        var contentStyle = CreateMarkerStyle(ContentClass, "content-style", ownerClass);
        AddStyles(scope, button, window, iconStyle, contentStyle);

        try
        {
            window.Show();
            button.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var semanticControls = button.GetVisualDescendants()
                                         .OfType<Control>()
                                         .Where(static control => control.Classes.Any(
                                             className => className.StartsWith("semantic-", StringComparison.Ordinal)))
                                         .ToArray();
            var icons = semanticControls.Where(static control => control.Classes.Contains(IconClass)).ToArray();
            var content = semanticControls.Single(static control => control.Classes.Contains(ContentClass));

            icons.Length.ShouldBe(2);
            icons.ShouldAllBe(static icon => Equals(icon.Tag, "icon-style"));
            content.ShouldBeOfType<ContentPresenter>();
            content.Tag.ShouldBe("content-style");
            button.Classes.Contains("semantic-root").ShouldBeFalse();
        }
        finally
        {
            if (scope == "application")
            {
                Application.Current!.Styles.Remove(iconStyle);
                Application.Current.Styles.Remove(contentStyle);
            }
            window.Close();
        }
    }

    [Fact]
    public void Semantic_Style_Overrides_TemplateBinding_And_LocalValue_Overrides_Semantic_Style()
    {
        var button = new AtomUIButton
        {
            Content = "Template Content"
        };
        button.Styles.Add(new Style(selector =>
            selector.OfType<AtomUIButton>().Template().Class(ContentClass))
        {
            Setters =
            {
                new Setter(ContentPresenter.ContentProperty, "Semantic Style Content")
            }
        });
        var window = new AvaloniaWindow
        {
            Width = 320,
            Height = 120,
            Content = button
        };

        try
        {
            window.Show();
            button.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var content = button.GetVisualDescendants()
                                .OfType<ContentPresenter>()
                                .Single(static presenter => presenter.Classes.Contains(ContentClass));
            content.Content.ShouldBe("Semantic Style Content");

            content.SetValue(ContentPresenter.ContentProperty, "Local Content");
            content.Content.ShouldBe("Local Content");

            content.ClearValue(ContentPresenter.ContentProperty);
            Dispatcher.UIThread.RunJobs();
            content.Content.ShouldBe("Semantic Style Content");
        }
        finally
        {
            window.Close();
        }
    }

    private static Style CreateMarkerStyle(string marker, object value, string ownerClass)
    {
        return new Style(selector =>
            selector.OfType<AtomUIButton>().Class(ownerClass).Template().Class(marker))
        {
            Setters =
            {
                new Setter(Control.TagProperty, value)
            }
        };
    }

    private static void AddStyles(
        string scope,
        AtomUIButton button,
        AvaloniaWindow window,
        params Style[] styles)
    {
        var target = scope switch
        {
            "application" => Application.Current!.Styles,
            "local" => window.Styles,
            "owner" => button.Styles,
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null)
        };
        foreach (var style in styles)
        {
            target.Add(style);
        }
    }

    private static List<XElement> FindMarkedElements(XElement template, string marker)
    {
        return template.Descendants()
                       .Where(element => HasMarker(element, marker))
                       .ToList();
    }

    private static bool HasMarker(XElement element, string marker)
    {
        if (((string?)element.Attribute("Classes"))
            ?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Contains(marker, StringComparer.Ordinal) == true)
        {
            return true;
        }

        var classProperty = element.Attribute($"Classes.{marker}");
        return classProperty is not null &&
               bool.TryParse(classProperty.Value, out var isEnabled) &&
               isEnabled;
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
}
