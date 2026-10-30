using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
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
using AtomUIFloatButton = AtomUI.Desktop.Controls.FloatButton;
using AtomUIBackTopFloatButton = AtomUI.Desktop.Controls.BackTopFloatButton;
using AtomUIFloatButtonGroup = AtomUI.Desktop.Controls.FloatButtonGroup;
using AtomUIFloatButtonHost = AtomUI.Desktop.Controls.FloatButtonHost;

namespace AtomUI.Desktop.Controls.Tests.FloatButton;

public class FloatButtonSemanticPartTests
{
    static FloatButtonSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Approved_FloatButton_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        foreach (var ownerType in new[] { typeof(AtomUIFloatButton), typeof(AtomUIBackTopFloatButton) })
        {
            manager.SemanticParts.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
            descriptor.ShouldNotBeNull();
            descriptor.Parts.Select(static part => part.Name)
                      .ShouldBe(["root", "content", "icon"]);
            AssertRoot(descriptor, ownerType);
            AssertPart(descriptor, "icon", "semantic-icon", typeof(IconPresenter), SemanticPartCardinality.Single);
            AssertPart(descriptor, "content", "semantic-content", typeof(ContentPresenter), SemanticPartCardinality.Optional);
        }

        manager.SemanticParts.TryGetControl(typeof(AtomUIFloatButtonGroup), out var groupDescriptor).ShouldBeTrue();
        groupDescriptor.ShouldNotBeNull();
        groupDescriptor.Parts.Select(static part => part.Name)
                       .ShouldBe(["root", "list", "trigger"]);
        AssertRoot(groupDescriptor, typeof(AtomUIFloatButtonGroup));
        AssertPart(groupDescriptor, "trigger", "semantic-trigger", typeof(AtomUIFloatButton), SemanticPartCardinality.Optional);
        AssertPart(groupDescriptor, "list", "semantic-list", typeof(TemplatedControl), SemanticPartCardinality.Single);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/FloatButton/Themes/BackTopFloatButtonTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonGroupTheme.axaml")]
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
        foreach (var relativePath in new[]
                 {
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/AbstractFloatButtonTheme.axaml",
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonTheme.axaml",
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/BackTopFloatButtonTheme.axaml",
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/BackTopFloatButtonHostTheme.axaml",
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonGroupTheme.axaml",
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonGroupHostTheme.axaml",
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonHostTheme.axaml",
                     "src/AtomUI.Desktop.Controls/FloatButton/Themes/FloatButtonItemsControlTheme.axaml"
                 })
        {
            var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
            var selectors = document.Descendants()
                                    .Where(static element => element.Name.LocalName == "Style")
                                    .Attributes("Selector")
                                    .Select(static attribute => attribute.Value)
                                    .ToArray();
            selectors.ShouldAllBe(selector => !selector.Contains("semantic-", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_FloatButton_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUIFloatButton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var button = new AtomUIFloatButton
        {
            Shape   = FloatButtonShape.Square,
            Content = "Docs"
        };
        button.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUIFloatButton>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        button.Styles.Add(ownerStyle);

        using var window = Show(button);
        button.Tag.ShouldBe("root");
        FindSemanticControl<IconPresenter>(button, "semantic-icon").Tag.ShouldBe("icon");
        FindSemanticControl<ContentPresenter>(button, "semantic-content").Tag.ShouldBe("content");
    }

    [Fact]
    public void Circle_Template_Exposes_Only_The_Icon_Marker()
    {
        var button = new AtomUIFloatButton
        {
            Shape = FloatButtonShape.Circle
        };

        using var window = Show(button);
        FindSemanticControl<IconPresenter>(button, "semantic-icon").ShouldNotBeNull();
        button.GetVisualDescendants()
              .OfType<Control>()
              .Any(static control => control.Classes.Contains("semantic-content"))
              .ShouldBeFalse();
    }

    [Fact]
    public void Shape_Switch_Reprovides_The_Same_Marker_Contract()
    {
        var button = new AtomUIFloatButton
        {
            Shape   = FloatButtonShape.Square,
            Content = "Docs"
        };

        using var window = Show(button);
        FindSemanticControl<ContentPresenter>(button, "semantic-content").ShouldNotBeNull();

        button.Shape = FloatButtonShape.Circle;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<IconPresenter>(button, "semantic-icon").ShouldNotBeNull();
        button.GetVisualDescendants()
              .OfType<Control>()
              .Any(static control => control.Classes.Contains("semantic-content"))
              .ShouldBeFalse();

        button.Shape = FloatButtonShape.Square;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<IconPresenter>(button, "semantic-icon").ShouldNotBeNull();
        FindSemanticControl<ContentPresenter>(button, "semantic-content").ShouldNotBeNull();
    }

    [Fact]
    public void BackTop_Template_Targets_Match_The_FloatButton_Contract()
    {
        var backTop = new AtomUIBackTopFloatButton
        {
            Shape   = FloatButtonShape.Square,
            Content = "Top"
        };

        using var window = Show(backTop);
        FindSemanticControl<IconPresenter>(backTop, "semantic-icon").ShouldNotBeNull();
        FindSemanticControl<ContentPresenter>(backTop, "semantic-content").ShouldNotBeNull();
    }

    [Fact]
    public void Default_Group_Exposes_Only_The_List_Marker()
    {
        var group = new AtomUIFloatButtonGroup
        {
            Trigger = FloatButtonGroupTrigger.Default,
            Children =
            {
                new AtomUIFloatButton(),
                new AtomUIFloatButton()
            }
        };

        using var window = Show(group);
        group.GetVisualDescendants()
             .OfType<Control>()
             .Single(static control => control.Classes.Contains("semantic-list"))
             .ShouldNotBeNull();
        group.GetVisualDescendants()
             .OfType<Control>()
             .Any(static control => control.Classes.Contains("semantic-trigger"))
             .ShouldBeFalse();
    }

    [Fact]
    public void Trigger_Group_Open_Close_Reopen_Preserves_Marker_Identity()
    {
        var group = new AtomUIFloatButtonGroup
        {
            Trigger         = FloatButtonGroupTrigger.Click,
            IsMotionEnabled = false,
            Children =
            {
                new AtomUIFloatButton(),
                new AtomUIFloatButton()
            }
        };

        using var window = Show(group);
        var trigger = group.GetVisualDescendants()
                           .OfType<AtomUIFloatButton>()
                           .Single(static control => control.Classes.Contains("semantic-trigger"));

        group.IsOpen = true;
        Dispatcher.UIThread.RunJobs();
        var list = group.GetVisualDescendants()
                        .OfType<Control>()
                        .Single(static control => control.Classes.Contains("semantic-list"));
        list.IsEffectivelyVisible.ShouldBeTrue();

        group.IsOpen = false;
        Dispatcher.UIThread.RunJobs();
        list.IsEffectivelyVisible.ShouldBeFalse();

        group.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        group.GetVisualDescendants()
             .OfType<AtomUIFloatButton>()
             .Single(static control => control.Classes.Contains("semantic-trigger"))
             .ShouldBeSameAs(trigger);
        group.GetVisualDescendants()
             .OfType<Control>()
             .Single(static control => control.Classes.Contains("semantic-list"))
             .ShouldBeSameAs(list);
        list.IsEffectivelyVisible.ShouldBeTrue();
    }

    [Fact]
    public void Group_Children_Add_And_Remove_Stay_In_Sync_With_The_Items_Layout()
    {
        var first  = new AtomUIFloatButton();
        var second = new AtomUIFloatButton();
        var group = new AtomUIFloatButtonGroup
        {
            Trigger = FloatButtonGroupTrigger.Default,
            Children =
            {
                first
            }
        };

        using var window = Show(group);
        var itemsLayout = group.GetVisualDescendants()
                               .OfType<StackPanel>()
                               .Single(static panel => panel.Name == "PART_ItemsLayout");
        itemsLayout.Children.ShouldBe([first]);

        group.Children.Add(second);
        Dispatcher.UIThread.RunJobs();
        itemsLayout.Children.ShouldBe([first, second]);

        group.Children.Remove(first);
        Dispatcher.UIThread.RunJobs();
        itemsLayout.Children.ShouldBe([second]);
    }

    [Fact]
    public void Host_Created_Overlay_Button_Carries_The_Same_Markers()
    {
        var host = new AtomUIFloatButtonHost
        {
            Shape       = FloatButtonShape.Square,
            Description = "Docs"
        };
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 480,
            Height = 320
        };
        overlayPanel.Children.Add(host);
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 320,
            Content = overlayPanel
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var overlayLayer = ScopeAwareOverlayLayer.FindLayer(overlayPanel).ShouldNotBeNull();
            var overlayButton = overlayLayer.Children.OfType<AtomUIFloatButton>().Single();
            FindSemanticControl<IconPresenter>(overlayButton, "semantic-icon").ShouldNotBeNull();
            FindSemanticControl<ContentPresenter>(overlayButton, "semantic-content").ShouldNotBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
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
            Height = 320,
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
