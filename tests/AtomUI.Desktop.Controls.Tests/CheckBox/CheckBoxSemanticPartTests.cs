using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomCheckBox = AtomUI.Desktop.Controls.CheckBox;

namespace AtomUI.Desktop.Controls.Tests.CheckBox;

public class CheckBoxSemanticPartTests
{
    static CheckBoxSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Approved_CheckBox_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomCheckBox), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "icon", "label"]);

        AssertRoot(descriptor, typeof(AtomCheckBox));
        AssertPart(descriptor, "icon", "semantic-icon", typeof(TemplatedControl), SemanticPartCardinality.Single);
        AssertPart(descriptor, "label", "semantic-label", typeof(ContentPresenter), SemanticPartCardinality.Single);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/CheckBox/Themes/CheckBoxTheme.axaml")]
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
            GetRepoFile("src/AtomUI.Desktop.Controls/CheckBox/Themes/CheckBoxTheme.axaml"),
            LoadOptions.SetLineInfo);
        var selectors = document.Descendants()
                                .Where(static element => element.Name.LocalName == "Style")
                                .Attributes("Selector")
                                .Select(static attribute => attribute.Value)
                                .ToArray();
        selectors.ShouldAllBe(selector => !selector.Contains("semantic-", StringComparison.Ordinal));
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_CheckBox_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomCheckBox), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var checkBox = new AtomCheckBox
        {
            Content = "Checkbox"
        };
        checkBox.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomCheckBox>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        checkBox.Styles.Add(ownerStyle);

        using var window = Show(checkBox);
        checkBox.Tag.ShouldBe("root");
        FindSemanticControl<TemplatedControl>(checkBox, "semantic-icon").Tag.ShouldBe("icon");
        FindSemanticControl<ContentPresenter>(checkBox, "semantic-label").Tag.ShouldBe("label");
    }

    [Fact]
    public void Check_State_Changes_Preserve_Icon_And_Label_Marker_Identity()
    {
        var checkBox = new AtomCheckBox
        {
            Content = "Checkbox"
        };

        using var window = Show(checkBox);
        var icon = FindSemanticControl<TemplatedControl>(checkBox, "semantic-icon");
        var label = FindSemanticControl<ContentPresenter>(checkBox, "semantic-label");

        checkBox.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<TemplatedControl>(checkBox, "semantic-icon").ShouldBeSameAs(icon);

        checkBox.IsChecked = null;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<TemplatedControl>(checkBox, "semantic-icon").ShouldBeSameAs(icon);
        FindSemanticControl<ContentPresenter>(checkBox, "semantic-label").ShouldBeSameAs(label);
    }

    [Fact]
    public void Null_Content_Keeps_The_Label_Marker()
    {
        var checkBox = new AtomCheckBox
        {
            Content = "Checkbox"
        };

        using var window = Show(checkBox);
        var label = FindSemanticControl<ContentPresenter>(checkBox, "semantic-label");
        label.IsEffectivelyVisible.ShouldBeTrue();

        checkBox.Content = null;
        Dispatcher.UIThread.RunJobs();

        FindSemanticControl<ContentPresenter>(checkBox, "semantic-label").ShouldBeSameAs(label);
        label.IsEffectivelyVisible.ShouldBeFalse();
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
