using System.Xml.Linq;
using AtomUI.Controls;
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
using AtomRadioButton = AtomUI.Desktop.Controls.RadioButton;
using AtomRadioButtonGroup = AtomUI.Desktop.Controls.RadioButtonGroup;

namespace AtomUI.Desktop.Controls.Tests.RadioButton;

public class RadioButtonSemanticPartTests
{
    static RadioButtonSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Approved_RadioButton_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomRadioButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "icon", "label"]);

        AssertRoot(descriptor, typeof(AtomRadioButton));
        AssertPart(descriptor, "icon", "semantic-icon", typeof(TemplatedControl), SemanticPartCardinality.Single);
        AssertPart(descriptor, "label", "semantic-label", typeof(ContentPresenter), SemanticPartCardinality.Single);
    }

    [Fact]
    public void Non_Owner_Types_Do_Not_Hold_Semantic_Descriptors()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomRadioButtonGroup), out _).ShouldBeFalse();
        manager.SemanticParts.TryGetControl(typeof(OptionButton), out _).ShouldBeFalse();
        manager.SemanticParts.TryGetControl(typeof(OptionButtonGroup), out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/RadioButton/Themes/RadioButtonTheme.axaml")]
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
            GetRepoFile("src/AtomUI.Desktop.Controls/RadioButton/Themes/RadioButtonTheme.axaml"),
            LoadOptions.SetLineInfo);
        var selectors = document.Descendants()
                                .Where(static element => element.Name.LocalName == "Style")
                                .Attributes("Selector")
                                .Select(static attribute => attribute.Value)
                                .ToArray();
        selectors.ShouldAllBe(selector => !selector.Contains("semantic-", StringComparison.Ordinal));
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_RadioButton_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomRadioButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var radioButton = new AtomRadioButton
        {
            Content = "Radio"
        };
        radioButton.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomRadioButton>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        radioButton.Styles.Add(ownerStyle);

        using var window = Show(radioButton);
        radioButton.Tag.ShouldBe("root");
        FindSemanticControl<TemplatedControl>(radioButton, "semantic-icon").Tag.ShouldBe("icon");
        FindSemanticControl<ContentPresenter>(radioButton, "semantic-label").Tag.ShouldBe("label");
    }

    [Fact]
    public void Check_State_Changes_Preserve_Icon_And_Label_Marker_Identity()
    {
        var radioButton = new AtomRadioButton
        {
            Content = "Radio"
        };

        using var window = Show(radioButton);
        var icon = FindSemanticControl<TemplatedControl>(radioButton, "semantic-icon");
        var label = FindSemanticControl<ContentPresenter>(radioButton, "semantic-label");

        radioButton.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<TemplatedControl>(radioButton, "semantic-icon").ShouldBeSameAs(icon);
        FindSemanticControl<ContentPresenter>(radioButton, "semantic-label").ShouldBeSameAs(label);

        radioButton.IsChecked = null;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<TemplatedControl>(radioButton, "semantic-icon").ShouldBeSameAs(icon);
        FindSemanticControl<ContentPresenter>(radioButton, "semantic-label").ShouldBeSameAs(label);
    }

    [Fact]
    public void Null_Content_Keeps_The_Label_Marker()
    {
        var radioButton = new AtomRadioButton
        {
            Content = "Radio"
        };

        using var window = Show(radioButton);
        var label = FindSemanticControl<ContentPresenter>(radioButton, "semantic-label");
        label.IsEffectivelyVisible.ShouldBeTrue();

        radioButton.Content = null;
        Dispatcher.UIThread.RunJobs();

        FindSemanticControl<ContentPresenter>(radioButton, "semantic-label").ShouldBeSameAs(label);
        label.IsEffectivelyVisible.ShouldBeFalse();
    }

    [Fact]
    public void Group_Containers_Expose_The_Same_Descriptor_Without_Leaking_Markers()
    {
        var options = new[]
        {
            CreateOption("Apple"),
            CreateOption("Pear"),
            CreateOption("Orange")
        };
        var group = new AtomRadioButtonGroup
        {
            ItemsSource = options
        };

        using var window = Show(group);
        var containers = group.GetVisualDescendants()
                              .OfType<AtomRadioButton>()
                              .ToArray();
        containers.Length.ShouldBe(3);

        foreach (var container in containers)
        {
            FindSemanticControl<TemplatedControl>(container, "semantic-icon").ShouldNotBeNull();
            FindSemanticControl<ContentPresenter>(container, "semantic-label").ShouldNotBeNull();
        }

        group.CheckedItem = options[1];
        Dispatcher.UIThread.RunJobs();

        foreach (var container in group.GetVisualDescendants().OfType<AtomRadioButton>())
        {
            container.GetVisualDescendants()
                     .OfType<TemplatedControl>()
                     .Count(static control => control.Classes.Contains("semantic-icon"))
                     .ShouldBe(1);
            container.GetVisualDescendants()
                     .OfType<ContentPresenter>()
                     .Count(static control => control.Classes.Contains("semantic-label"))
                     .ShouldBe(1);
        }
    }

    private static RadioButtonOption CreateOption(string content)
    {
        return new RadioButtonOption
        {
            Content = content
        };
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
