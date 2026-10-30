using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISpin = AtomUI.Desktop.Controls.Spin;
using AtomUISpinIndicator = AtomUI.Desktop.Controls.SpinIndicator;
using AtomUITextBlock = AtomUI.Desktop.Controls.TextBlock;
using Ellipse = Avalonia.Controls.Shapes.Ellipse;

namespace AtomUI.Desktop.Controls.Tests.Spin;

public class SpinSemanticPartTests
{
    static SpinSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Approved_Spin_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUISpin), out var spinDescriptor).ShouldBeTrue();
        spinDescriptor.ShouldNotBeNull();
        spinDescriptor.Parts.Select(static part => part.Name)
                      .ShouldBe(["root", "container", "description", "indicator", "mask", "section"]);

        AssertRoot(spinDescriptor, typeof(AtomUISpin));
        AssertPart(spinDescriptor, "container", "semantic-container", typeof(ContentPresenter), SemanticPartCardinality.Single);
        AssertPart(spinDescriptor, "mask", "semantic-mask", typeof(Border), SemanticPartCardinality.Single);
        AssertPart(spinDescriptor, "section", "semantic-section", typeof(StackPanel), SemanticPartCardinality.Single);
        AssertPart(spinDescriptor, "indicator", "semantic-indicator", typeof(AtomUISpinIndicator), SemanticPartCardinality.Single);
        AssertPart(spinDescriptor, "description", "semantic-description", typeof(TextBlock), SemanticPartCardinality.Single);

        manager.SemanticParts.TryGetControl(typeof(AtomUISpinIndicator), out var indicatorDescriptor).ShouldBeTrue();
        indicatorDescriptor.ShouldNotBeNull();
        indicatorDescriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "content"]);
        AssertRoot(indicatorDescriptor, typeof(AtomUISpinIndicator));
        AssertPart(indicatorDescriptor, "content", "semantic-content", typeof(Control), SemanticPartCardinality.Multiple);
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Spin/Themes/SpinTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Spin/Themes/SpinIndicatorTheme.axaml")]
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
                     "src/AtomUI.Desktop.Controls/Spin/Themes/SpinTheme.axaml",
                     "src/AtomUI.Desktop.Controls/Spin/Themes/SpinIndicatorTheme.axaml"
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
    public void Generated_Semantic_Styles_Apply_To_The_Spin_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUISpin), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var spin = new AtomUISpin
        {
            IsSpinning   = true,
            IsTipVisible = true,
            Tip          = "Loading...",
            Content      = new Border { Width = 120, Height = 80 }
        };
        spin.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISpin>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        spin.Styles.Add(ownerStyle);

        using var window = Show(spin);
        spin.Tag.ShouldBe("root");
        FindSemanticControl<ContentPresenter>(spin, "semantic-container").Tag.ShouldBe("container");
        FindSemanticControl<Border>(spin, "semantic-mask").Tag.ShouldBe("mask");
        FindSemanticControl<StackPanel>(spin, "semantic-section").Tag.ShouldBe("section");
        FindSemanticControl<AtomUISpinIndicator>(spin, "semantic-indicator").Tag.ShouldBe("indicator");
        FindSemanticControl<AtomUITextBlock>(spin, "semantic-description").Tag.ShouldBe("description");
    }

    [Fact]
    public void Generated_Content_Style_Applies_To_Both_SpinIndicator_Alternatives()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUISpinIndicator), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var indicator = new AtomUISpinIndicator();
        indicator.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISpinIndicator>().Class("semantic-owner"));
        var contentPart = descriptor.Parts.Single(static part => part.Name == "content");
        var contentStyle = (Style)Activator.CreateInstance(contentPart.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
        contentStyle.Setters.Add(new Setter(Control.TagProperty, "content"));
        ownerStyle.Children.Add(contentStyle);
        indicator.Styles.Add(ownerStyle);

        using var window = Show(indicator);
        var contentTargets = indicator.GetVisualDescendants()
                                      .OfType<Control>()
                                      .Where(static control => control.Classes.Contains("semantic-content"))
                                      .ToArray();
        contentTargets.Length.ShouldBe(2);
        contentTargets.ShouldAllBe(static target => target.Tag as string == "content");
        contentTargets.Count(static target => target.IsVisible).ShouldBe(1);
    }

    [Fact]
    public void Spinning_Toggle_Preserves_Overlay_Marker_Identity()
    {
        var spin = new AtomUISpin
        {
            IsSpinning   = true,
            IsTipVisible = true,
            Tip          = "Loading...",
            Content      = new Border { Width = 120, Height = 80 }
        };

        using var window = Show(spin);
        var section = FindSemanticControl<StackPanel>(spin, "semantic-section");
        var mask    = FindSemanticControl<Border>(spin, "semantic-mask");
        var overlay = spin.GetVisualDescendants()
                          .OfType<Panel>()
                          .Single(static panel => panel.Name == "MaskLayout");
        overlay.IsVisible.ShouldBeTrue();

        spin.IsSpinning = false;
        Dispatcher.UIThread.RunJobs();

        overlay.IsVisible.ShouldBeFalse();
        FindSemanticControl<StackPanel>(spin, "semantic-section").ShouldBeSameAs(section);
        FindSemanticControl<Border>(spin, "semantic-mask").ShouldBeSameAs(mask);

        spin.IsSpinning = true;
        Dispatcher.UIThread.RunJobs();

        overlay.IsVisible.ShouldBeTrue();
        FindSemanticControl<StackPanel>(spin, "semantic-section").ShouldBeSameAs(section);
        FindSemanticControl<Border>(spin, "semantic-mask").ShouldBeSameAs(mask);
    }

    [Fact]
    public void Tip_Visibility_Toggle_Preserves_Description_Marker()
    {
        var spin = new AtomUISpin
        {
            IsSpinning   = true,
            IsTipVisible = false,
            Tip          = "Loading...",
            Content      = new Border { Width = 120, Height = 80 }
        };

        using var window = Show(spin);
        var description = FindSemanticControl<AtomUITextBlock>(spin, "semantic-description");
        description.IsVisible.ShouldBeFalse();
        description.Text.ShouldBe("Loading...");

        spin.IsTipVisible = true;
        Dispatcher.UIThread.RunJobs();

        FindSemanticControl<AtomUITextBlock>(spin, "semantic-description").ShouldBeSameAs(description);
        description.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Custom_Indicator_Switch_Preserves_Content_Marker_Identity()
    {
        var indicator = new AtomUISpinIndicator();

        using var window = Show(indicator);
        var contentTargets = indicator.GetVisualDescendants()
                                      .OfType<Control>()
                                      .Where(static control => control.Classes.Contains("semantic-content"))
                                      .ToArray();
        contentTargets.Length.ShouldBe(2);
        var builtIn = contentTargets.Single(static target => target.IsVisible);
        builtIn.Name.ShouldBe("BuiltInIndicatorLayout");

        indicator.CustomIndicator = new Border { Width = 24, Height = 24 };
        Dispatcher.UIThread.RunJobs();

        var updatedTargets = indicator.GetVisualDescendants()
                                      .OfType<Control>()
                                      .Where(static control => control.Classes.Contains("semantic-content"))
                                      .ToArray();
        updatedTargets.ShouldBe(contentTargets);
        updatedTargets.Count(static target => target.IsVisible).ShouldBe(1);
        updatedTargets.Single(static target => target.IsVisible).Name.ShouldBe("PART_CustomIndicatorPresenter");

        indicator.CustomIndicator = null;
        Dispatcher.UIThread.RunJobs();

        indicator.GetVisualDescendants()
                 .OfType<Control>()
                 .Where(static control => control.Classes.Contains("semantic-content"))
                 .ShouldBe(contentTargets);
        builtIn.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Standalone_Spin_Without_Content_Keeps_Marker_Contract()
    {
        var spin = new AtomUISpin
        {
            IsSpinning   = true,
            IsTipVisible = true,
            Tip          = "Loading..."
        };

        using var window = Show(spin);
        FindSemanticControl<ContentPresenter>(spin, "semantic-container").ShouldNotBeNull();
        FindSemanticControl<Border>(spin, "semantic-mask").ShouldNotBeNull();
        FindSemanticControl<StackPanel>(spin, "semantic-section").ShouldNotBeNull();
        FindSemanticControl<AtomUISpinIndicator>(spin, "semantic-indicator").ShouldNotBeNull();
        FindSemanticControl<AtomUITextBlock>(spin, "semantic-description").ShouldNotBeNull();
    }

    [Fact]
    public void SizeType_Branches_Produce_Distinct_Indicator_Metrics()
    {
        var small = new AtomUISpinIndicator { SizeType = CustomizableSizeType.Small };
        var middle = new AtomUISpinIndicator { SizeType = CustomizableSizeType.Middle };
        var large = new AtomUISpinIndicator { SizeType = CustomizableSizeType.Large };
        var custom = new AtomUISpinIndicator { SizeType = CustomizableSizeType.Custom };
        var host = new StackPanel
        {
            Children = { small, middle, large, custom }
        };

        using var window = Show(host);
        small.IndicatorSize.ShouldBeLessThan(middle.IndicatorSize);
        middle.IndicatorSize.ShouldBeLessThan(large.IndicatorSize);
        small.DotSize.ShouldBeLessThan(middle.DotSize);
        middle.DotSize.ShouldBeLessThan(large.DotSize);
        custom.IndicatorSize.ShouldBe(middle.IndicatorSize);
        custom.DotSize.ShouldBe(middle.DotSize);

        custom.IndicatorSize = 44;
        custom.DotSize = 14;
        Dispatcher.UIThread.RunJobs();

        custom.IndicatorSize.ShouldBe(44);
        custom.DotSize.ShouldBe(14);
    }

    [Fact]
    public void Dot_Bg_Brush_Projects_To_All_Built_In_Dots()
    {
        var brush = new SolidColorBrush(Color.Parse("#00d4ff"));
        var indicator = new AtomUISpinIndicator
        {
            DotBgBrush = brush
        };

        using var window = Show(indicator);
        var dots = indicator.GetVisualDescendants()
                            .OfType<Ellipse>()
                            .Where(static dot => dot.Classes.Contains("SpinIndicatorDot"))
                            .ToArray();
        dots.Length.ShouldBe(4);
        foreach (var dot in dots)
        {
            dot.Fill.ShouldBeSameAs(brush);
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
