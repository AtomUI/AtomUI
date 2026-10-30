using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;

namespace AtomUI.Desktop.Controls.Tests.Switch;

public class ToggleSwitchSemanticPartTests
{
    static ToggleSwitchSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Approved_ToggleSwitch_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomToggleSwitch), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "content", "indicator"]);

        AssertRoot(descriptor, typeof(AtomToggleSwitch));
        AssertPart(descriptor, "content", "semantic-content", typeof(ContentPresenter), SemanticPartCardinality.Multiple);
        AssertPart(descriptor, "indicator", "semantic-indicator", typeof(TemplatedControl), SemanticPartCardinality.Single);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Switch/Themes/ToggleSwitchTheme.axaml")]
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
            GetRepoFile("src/AtomUI.Desktop.Controls/Switch/Themes/ToggleSwitchTheme.axaml"),
            LoadOptions.SetLineInfo);
        var selectors = document.Descendants()
                                .Where(static element => element.Name.LocalName == "Style")
                                .Attributes("Selector")
                                .Select(static attribute => attribute.Value)
                                .ToArray();
        selectors.ShouldAllBe(selector => !selector.Contains("semantic-", StringComparison.Ordinal));
    }

    [Fact]
    public void Generated_Semantic_Styles_Apply_To_The_ToggleSwitch_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomToggleSwitch), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var toggleSwitch = new AtomToggleSwitch
        {
            OnContent = "ON",
            OffContent = "OFF"
        };
        toggleSwitch.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomToggleSwitch>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        toggleSwitch.Styles.Add(ownerStyle);

        using var window = Show(toggleSwitch);
        toggleSwitch.Tag.ShouldBe("root");
        FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator").Tag.ShouldBe("indicator");
        foreach (var presenter in FindSemanticControls<ContentPresenter>(toggleSwitch, "semantic-content"))
        {
            presenter.Tag.ShouldBe("content");
        }
    }

    [Fact]
    public void Check_State_Changes_Preserve_Content_And_Indicator_Marker_Identity()
    {
        var toggleSwitch = new AtomToggleSwitch
        {
            OnContent = "ON",
            OffContent = "OFF"
        };

        using var window = Show(toggleSwitch);
        var indicator = FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator");
        var contents = FindSemanticControls<ContentPresenter>(toggleSwitch, "semantic-content");
        contents.Length.ShouldBe(2);

        toggleSwitch.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator").ShouldBeSameAs(indicator);
        FindSemanticControls<ContentPresenter>(toggleSwitch, "semantic-content").Length.ShouldBe(2);

        toggleSwitch.IsChecked = null;
        Dispatcher.UIThread.RunJobs();
        FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator").ShouldBeSameAs(indicator);
        FindSemanticControls<ContentPresenter>(toggleSwitch, "semantic-content").Length.ShouldBe(2);
    }

    [Fact]
    public void Null_Content_Keeps_Both_Content_Markers()
    {
        var toggleSwitch = new AtomToggleSwitch
        {
            OnContent = "ON",
            OffContent = "OFF"
        };

        using var window = Show(toggleSwitch);
        var contents = FindSemanticControls<ContentPresenter>(toggleSwitch, "semantic-content");
        contents.Length.ShouldBe(2);

        toggleSwitch.OnContent = null;
        toggleSwitch.OffContent = null;
        Dispatcher.UIThread.RunJobs();

        FindSemanticControls<ContentPresenter>(toggleSwitch, "semantic-content").Length.ShouldBe(2);
    }

    [Fact]
    public void Indicator_Fill_Is_Driven_By_Background_And_Stylable()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomToggleSwitch), out var descriptor).ShouldBeTrue();
        var indicatorPart = descriptor.ShouldNotBeNull()
                                      .Parts
                                      .Single(static part => part.Name == "indicator");

        var toggleSwitch = new AtomToggleSwitch
        {
            OnContent = "ON",
            OffContent = "OFF"
        };
        using var window = Show(toggleSwitch);

        // The handle fill must be the standard Background property so the indicator Semantic
        // Part matches the antd `indicator` handle contract; the built-in theme provides it.
        var indicator = FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator");
        indicator.Background.ShouldNotBeNull();

        var indicatorStyle = (Style)Activator.CreateInstance(indicatorPart.StyleType.ShouldNotBeNull())
            .ShouldNotBeNull();
        indicatorStyle.Setters.Add(new Setter(TemplatedControl.BackgroundProperty, Brushes.Red));
        var ownerStyle = new Style(selector => selector.OfType<AtomToggleSwitch>().Class("semantic-recolor"));
        ownerStyle.Children.Add(indicatorStyle);
        toggleSwitch.Styles.Add(ownerStyle);
        toggleSwitch.Classes.Add("semantic-recolor");
        Dispatcher.UIThread.RunJobs();

        var recolored = FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator");
        recolored.Background.ShouldNotBeNull()
                 .ShouldBeAssignableTo<ISolidColorBrush>()
                 .Color.ShouldBe(Colors.Red);
    }

    [Fact]
    public void Indicator_Effect_Can_Be_Overridden_By_Semantic_Styles()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomToggleSwitch), out var descriptor).ShouldBeTrue();
        var indicatorPart = descriptor.ShouldNotBeNull()
                                      .Parts
                                      .Single(static part => part.Name == "indicator");

        var toggleSwitch = new AtomToggleSwitch();
        using var window = Show(toggleSwitch);

        // The theme derives a shadow effect from the handle token; semantic indicator
        // styles must be able to replace it (antd's `indicator` shadow customization).
        var indicator = FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator");
        indicator.Effect.ShouldNotBeNull().ShouldBeAssignableTo<DropShadowEffect>();

        var customEffect = new DropShadowEffect
        {
            OffsetY    = 1,
            BlurRadius = 4,
            Color      = Color.Parse("#4D000000")
        };
        var indicatorStyle = (Style)Activator.CreateInstance(indicatorPart.StyleType.ShouldNotBeNull())
            .ShouldNotBeNull();
        indicatorStyle.Setters.Add(new Setter(Visual.EffectProperty, customEffect));
        var ownerStyle = new Style(selector => selector.OfType<AtomToggleSwitch>().Class("semantic-shadow"));
        ownerStyle.Children.Add(indicatorStyle);
        toggleSwitch.Styles.Add(ownerStyle);
        toggleSwitch.Classes.Add("semantic-shadow");
        Dispatcher.UIThread.RunJobs();

        FindSemanticControl<TemplatedControl>(toggleSwitch, "semantic-indicator")
            .Effect.ShouldBeSameAs(customEffect);
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

    private static T[] FindSemanticControls<T>(Control owner, string semanticClass)
        where T : Control
    {
        return owner.GetVisualDescendants()
                    .OfType<T>()
                    .Where(control => control.Classes.Contains(semanticClass))
                    .ToArray();
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
