using System.Xml.Linq;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Badge;

public class BadgeSemanticPartTests
{
    private const string IndicatorClass = "semantic-indicator";
    private const string ContentClass = "semantic-content";
    private const string TargetIndicatorSelectorRoute =
        "> .semantic-scope-indicator /template/ .semantic-indicator";
    private const string RibbonIndicatorSelectorRoute = "> .semantic-indicator";
    private const string RibbonContentSelectorRoute =
        "> .semantic-indicator /template/ .semantic-content";

    static BadgeSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(typeof(Desktop.Controls.CountBadge), 2)]
    [InlineData(typeof(Desktop.Controls.DotBadge), 2)]
    [InlineData(typeof(Desktop.Controls.RibbonBadge), 3)]
    public void Registered_Descriptor_Exposes_The_Approved_Parts(Type ownerType, int expectedPartCount)
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Count.ShouldBe(expectedPartCount);

        var root = descriptor.Parts.Single(static part => part.Name == "root");
        root.Path.ShouldBe("root");
        root.SelectorClass.ShouldBeNull();
        root.ContractType.ShouldBe(ownerType);
        root.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        root.Customization.ShouldBe(SemanticPartCustomization.Root);
        root.CrossVisualRoot.ShouldBeFalse();
        root.RuntimeCreated.ShouldBeFalse();

        var indicator = descriptor.Parts.Single(static part => part.Name == "indicator");
        indicator.Path.ShouldBe("indicator");
        indicator.SelectorClass.ShouldBe(IndicatorClass);
        indicator.ContractType.ShouldBe(typeof(Control));
        indicator.Cardinality.ShouldBe(SemanticPartCardinality.Optional);
        indicator.Customization.ShouldBe(SemanticPartCustomization.Selector);
        indicator.CrossVisualRoot.ShouldBe(ownerType != typeof(Desktop.Controls.RibbonBadge));
        indicator.RuntimeCreated.ShouldBeTrue();
        indicator.Since.ShouldBe("6.2.0");
        indicator.SelectorRoute.ShouldBe(
            ownerType == typeof(Desktop.Controls.RibbonBadge)
                ? RibbonIndicatorSelectorRoute
                : TargetIndicatorSelectorRoute);

        var content = descriptor.Parts.SingleOrDefault(static part => part.Name == "content");
        if (ownerType == typeof(Desktop.Controls.RibbonBadge))
        {
            content.ShouldNotBeNull();
            content.Path.ShouldBe("content");
            content.SelectorClass.ShouldBe(ContentClass);
            content.ContractType.ShouldBe(typeof(TextBlock));
            content.Cardinality.ShouldBe(SemanticPartCardinality.Optional);
            content.Customization.ShouldBe(SemanticPartCustomization.Selector);
            content.CrossVisualRoot.ShouldBeFalse();
            content.RuntimeCreated.ShouldBeTrue();
            content.Since.ShouldBe("6.2.0");
            content.SelectorRoute.ShouldBe(RibbonContentSelectorRoute);
        }
        else
        {
            content.ShouldBeNull();
        }
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Badge/Themes/CountBadgeAdornerTheme.axaml", 1, 0)]
    [InlineData("src/AtomUI.Desktop.Controls/Badge/Themes/DotBadgeAdornerTheme.axaml", 2, 0)]
    [InlineData("src/AtomUI.Desktop.Controls/Badge/Themes/RibbonBadgeAdornerTheme.axaml", 0, 1)]
    public void Built_In_Themes_Use_Only_Static_Semantic_Markers(
        string relativePath,
        int expectedIndicatorMarkers,
        int expectedContentMarkers)
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
                                           .Attributes()
                                           .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                               "Classes.semantic-",
                                               StringComparison.Ordinal))
                                           .ToArray();

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static attribute =>
            string.Equals(attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Count(static attribute =>
            attribute.Name.LocalName == $"Classes.{IndicatorClass}").ShouldBe(expectedIndicatorMarkers);
        classPropertyMarkers.Count(static attribute =>
            attribute.Name.LocalName == $"Classes.{ContentClass}").ShouldBe(expectedContentMarkers);
        classPropertyMarkers.ShouldAllBe(static attribute =>
            attribute.Name.LocalName == "Classes.semantic-indicator" ||
            attribute.Name.LocalName == "Classes.semantic-content");
    }

    [Theory]
    [InlineData("count")]
    [InlineData("dot")]
    public void Target_Mode_Preserves_Badge_As_Logical_And_Style_Owner(string badgeKind)
    {
        var badge = CreateTargetBadge(badgeKind);
        badge.Classes.Add("semantic-owner");
        badge.Styles.Add(new Style(selector =>
            selector.OfType(badge.GetType())
                    .Class("semantic-owner")
                    .Child()
                    .Class("semantic-scope-indicator")
                    .Template()
                    .Class(IndicatorClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "owner-style")
            }
        });

        using var context = ShowInAdornerHost(badge);
        var adorner = FindNativeAdorner(badge);
        var indicator = adorner.GetVisualDescendants()
                               .OfType<Control>()
                               .Single(control => control.Classes.Contains(IndicatorClass));

        ((ILogical)adorner).LogicalParent.ShouldBeSameAs(badge);
        adorner.Classes.ShouldContain("semantic-scope-indicator");
        indicator.Tag.ShouldBe("owner-style");
    }

    [Theory]
    [InlineData("count")]
    [InlineData("dot")]
    public void Target_Mode_Route_Does_Not_Style_A_Nested_Badge_Indicator(string badgeKind)
    {
        var nestedBadge = CreateTargetBadge(badgeKind);
        Control outerBadge = badgeKind switch
        {
            "count" => new Desktop.Controls.CountBadge
            {
                Count = 8,
                IsMotionEnabled = false,
                DecoratedTarget = nestedBadge
            },
            "dot" => new Desktop.Controls.DotBadge
            {
                Status = AtomUI.Controls.Commons.DotBadgeStatus.Success,
                IsMotionEnabled = false,
                DecoratedTarget = nestedBadge
            },
            _ => throw new ArgumentOutOfRangeException(nameof(badgeKind), badgeKind, null)
        };
        outerBadge.Classes.Add("semantic-owner");
        outerBadge.Styles.Add(new Style(selector =>
            selector.OfType(outerBadge.GetType())
                    .Class("semantic-owner")
                    .Child()
                    .Class("semantic-scope-indicator")
                    .Template()
                    .Class(IndicatorClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "outer-indicator")
            }
        });

        using var context = ShowInAdornerHost(outerBadge, width: 180, height: 140);
        var outerIndicator = FindNativeAdorner(outerBadge)
            .GetVisualDescendants()
            .OfType<Control>()
            .Single(control => control.Classes.Contains(IndicatorClass));
        var nestedIndicator = FindNativeAdorner(nestedBadge)
            .GetVisualDescendants()
            .OfType<Control>()
            .Single(control => control.Classes.Contains(IndicatorClass));

        outerIndicator.Tag.ShouldBe("outer-indicator");
        nestedIndicator.Tag.ShouldBeNull();
    }

    [Fact]
    public void Ribbon_Routes_Match_Only_The_Owner_Indicator_And_Content()
    {
        var nested = new Desktop.Controls.RibbonBadge
        {
            Text = "Nested",
            DecoratedTarget = new Border { Width = 80, Height = 40 }
        };
        var badge = new Desktop.Controls.RibbonBadge
        {
            Text = "Outer",
            DecoratedTarget = nested
        };
        badge.Classes.Add("semantic-owner");
        badge.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.RibbonBadge>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(IndicatorClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "outer-indicator")
            }
        });
        badge.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.RibbonBadge>()
                    .Class("semantic-owner")
                    .Child()
                    .Class(IndicatorClass)
                    .Template()
                    .Class(ContentClass))
        {
            Setters =
            {
                new Setter(Control.TagProperty, "outer-content")
            }
        });

        using var context = ShowInAdornerHost(badge, width: 240, height: 160);
        var outerIndicator = badge.GetVisualChildren()
                                  .OfType<Control>()
                                  .Single(control => control.Classes.Contains(IndicatorClass));
        var outerContent = outerIndicator.GetVisualDescendants()
                                         .OfType<TextBlock>()
                                         .Single(control => control.Classes.Contains(ContentClass));
        var nestedIndicator = nested.GetVisualChildren()
                                    .OfType<Control>()
                                    .Single(control => control.Classes.Contains(IndicatorClass));
        var nestedContent = nestedIndicator.GetVisualDescendants()
                                           .OfType<TextBlock>()
                                           .Single(control => control.Classes.Contains(ContentClass));

        outerIndicator.Tag.ShouldBe("outer-indicator");
        outerContent.Tag.ShouldBe("outer-content");
        nestedIndicator.Tag.ShouldBeNull();
        nestedContent.Tag.ShouldBeNull();
    }

    [Theory]
    [InlineData("count")]
    [InlineData("dot")]
    public void Target_Mode_Detach_Clears_Adorner_Owner_And_Association(string badgeKind)
    {
        var badge = CreateTargetBadge(badgeKind);
        using var context = ShowInAdornerHost(badge);
        var adorner = FindNativeAdorner(badge);

        HideBadge(badge);
        Dispatcher.UIThread.RunJobs();

        ((ILogical)adorner).LogicalParent.ShouldBeNull();
        AdornerLayer.GetAdornedElement(adorner).ShouldBeNull();
        context.AdornerLayer.Children.ShouldNotContain(adorner);
    }

    [Fact]
    public void Ribbon_Runtime_Indicator_And_Content_Expose_Approved_Markers()
    {
        var badge = new Desktop.Controls.RibbonBadge
        {
            Text = "Semantic Ribbon",
            DecoratedTarget = new Border
            {
                Width = 160,
                Height = 80
            }
        };
        using var context = ShowInAdornerHost(badge, width: 240, height: 160);

        var indicator = badge.GetVisualDescendants()
                             .OfType<Control>()
                             .Single(control => control.Classes.Contains(IndicatorClass));
        var content = indicator.GetVisualDescendants()
                               .OfType<TextBlock>()
                               .Single(control => control.Classes.Contains(ContentClass));

        indicator.GetType().Name.ShouldBe("RibbonBadgeAdorner");
        content.Text.ShouldBe("Semantic Ribbon");
        badge.Classes.ShouldNotContain("semantic-root");
    }

    private static Control CreateTargetBadge(string badgeKind)
    {
        var target = new Border
        {
            Width = 48,
            Height = 48
        };

        return badgeKind switch
        {
            "count" => new Desktop.Controls.CountBadge
            {
                Count = 5,
                IsMotionEnabled = false,
                DecoratedTarget = target
            },
            "dot" => new Desktop.Controls.DotBadge
            {
                Status = AtomUI.Controls.Commons.DotBadgeStatus.Success,
                IsMotionEnabled = false,
                DecoratedTarget = target
            },
            _ => throw new ArgumentOutOfRangeException(nameof(badgeKind), badgeKind, null)
        };
    }

    private static Control FindNativeAdorner(Control badge)
    {
        var adornerLayer = AdornerLayer.GetAdornerLayer(badge).ShouldNotBeNull();
        return adornerLayer.Children.Single(child =>
            ReferenceEquals(AdornerLayer.GetAdornedElement(child), badge));
    }

    private static WindowContext ShowInAdornerHost(Control control, double width = 120, double height = 100)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = control
        };
        var window = new AvaloniaWindow
        {
            Width = width,
            Height = height,
            Content = visualLayerManager
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowContext(window, AdornerLayer.GetAdornerLayer(control).ShouldNotBeNull());
    }

    private static void HideBadge(Control badge)
    {
        switch (badge)
        {
            case Desktop.Controls.CountBadge countBadge:
                countBadge.BadgeIsVisible = false;
                break;
            case Desktop.Controls.DotBadge dotBadge:
                dotBadge.BadgeIsVisible = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(badge), badge.GetType(), null);
        }
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

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        public WindowContext(AvaloniaWindow window, AdornerLayer adornerLayer)
        {
            _window = window;
            AdornerLayer = adornerLayer;
        }

        public AdornerLayer AdornerLayer { get; }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
