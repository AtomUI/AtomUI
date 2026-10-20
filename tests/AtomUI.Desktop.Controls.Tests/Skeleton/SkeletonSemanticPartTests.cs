using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISkeleton = AtomUI.Desktop.Controls.Skeleton;

namespace AtomUI.Desktop.Controls.Tests.Skeleton;

public class SkeletonSemanticPartTests
{
    private static readonly Type[] ElementOwners =
    [
        typeof(SkeletonAvatar),
        typeof(SkeletonButton),
        typeof(SkeletonInput),
        typeof(SkeletonImage),
        typeof(SkeletonNode)
    ];

    static SkeletonSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptors_Expose_The_Approved_Skeleton_Parts()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();

        manager.SemanticParts.TryGetControl(typeof(AtomUISkeleton), out var skeletonDescriptor).ShouldBeTrue();
        skeletonDescriptor.ShouldNotBeNull();
        skeletonDescriptor.Parts.Select(static part => part.Name)
                        .ShouldBe(["root", "avatar", "header", "paragraph", "section", "title"]);

        AssertRoot(skeletonDescriptor, typeof(AtomUISkeleton));
        AssertPart(skeletonDescriptor, "header", "semantic-header", typeof(DockPanel), SemanticPartCardinality.Single);
        AssertPart(skeletonDescriptor, "section", "semantic-section", typeof(StackPanel), SemanticPartCardinality.Single);
        AssertPart(skeletonDescriptor, "avatar", "semantic-avatar", typeof(SkeletonAvatar), SemanticPartCardinality.Single);
        AssertPart(skeletonDescriptor, "title", "semantic-title", typeof(SkeletonTitle), SemanticPartCardinality.Single);
        AssertPart(skeletonDescriptor, "paragraph", "semantic-paragraph", typeof(SkeletonParagraph), SemanticPartCardinality.Single);

        foreach (var ownerType in ElementOwners)
        {
            manager.SemanticParts.TryGetControl(ownerType, out var descriptor).ShouldBeTrue();
            descriptor.ShouldNotBeNull();
            descriptor.Parts.Select(static part => part.Name).ShouldBe(["root", "content"]);
            AssertRoot(descriptor, ownerType);
            AssertPart(descriptor, "content", "semantic-content", typeof(Border), SemanticPartCardinality.Multiple);
        }
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/Skeleton/Themes/AbstractSkeletonTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonAvatarTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonButtonTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonInputTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonImageTheme.axaml")]
    [InlineData("src/AtomUI.Desktop.Controls/Skeleton/Themes/SkeletonNodeTheme.axaml")]
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
    public void Generated_Semantic_Styles_Apply_To_The_Skeleton_Template_Targets()
    {
        var manager = Application.Current.ShouldNotBeNull().GetThemeManager().ShouldNotBeNull();
        manager.SemanticParts.TryGetControl(typeof(AtomUISkeleton), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();

        var skeleton = new AtomUISkeleton
        {
            IsLoading = true,
            IsShowAvatar = true,
            ParagraphRows = 2
        };
        skeleton.Classes.Add("semantic-owner");
        var ownerStyle = new Style(selector => selector.OfType<AtomUISkeleton>().Class("semantic-owner"));
        ownerStyle.Setters.Add(new Setter(Control.TagProperty, "root"));
        foreach (var part in descriptor.Parts.Where(static part => part.Name != "root"))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull()).ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }
        skeleton.Styles.Add(ownerStyle);

        using var window = Show(skeleton);
        skeleton.Tag.ShouldBe("root");
        FindSemanticControl<DockPanel>(skeleton, "semantic-header").Tag.ShouldBe("header");
        FindSemanticControl<StackPanel>(skeleton, "semantic-section").Tag.ShouldBe("section");
        FindSemanticControl<SkeletonAvatar>(skeleton, "semantic-avatar").Tag.ShouldBe("avatar");
        FindSemanticControl<SkeletonTitle>(skeleton, "semantic-title").Tag.ShouldBe("title");
        FindSemanticControl<SkeletonParagraph>(skeleton, "semantic-paragraph").Tag.ShouldBe("paragraph");
    }

    [Fact]
    public void Root_Style_Projects_The_Skeleton_Surface()
    {
        var background = new SolidColorBrush(Colors.White);
        var borderBrush = new SolidColorBrush(Colors.DodgerBlue);
        var padding = new Thickness(12);
        var borderThickness = new Thickness(2);
        var cornerRadius = new CornerRadius(10);
        var skeleton = new AtomUISkeleton
        {
            IsLoading = true,
            IsShowTitle = true,
            IsShowParagraph = false,
            Background = background,
            BorderBrush = borderBrush,
            BorderThickness = borderThickness,
            CornerRadius = cornerRadius,
            Padding = padding
        };

        using var window = Show(skeleton);
        var root = skeleton.GetVisualDescendants()
                           .OfType<Border>()
                           .Single(static border => border.Name == "PART_RootLayout");
        root.Background.ShouldBeSameAs(background);
        root.BorderBrush.ShouldBeSameAs(borderBrush);
        root.BorderThickness.ShouldBe(borderThickness);
        root.CornerRadius.ShouldBe(cornerRadius);
        root.Padding.ShouldBe(padding);
    }

    [Fact]
    public void Skeleton_Default_Root_Does_Not_Paint_The_Placeholder_Background()
    {
        var skeleton = new AtomUISkeleton
        {
            IsLoading = true,
            IsShowAvatar = true,
            ParagraphRows = 2
        };

        using var window = Show(skeleton);
        var root = skeleton.GetVisualDescendants()
                           .OfType<Border>()
                           .Single(static border => border.Name == "PART_RootLayout");
        root.Background.ShouldNotBeNull()
            .ShouldBeAssignableTo<ISolidColorBrush>()
            .Color.ShouldBe(Colors.Transparent);
    }

    [Fact]
    public void Paragraph_Background_Reaches_Runtime_Skeleton_Lines()
    {
        var background = new SolidColorBrush(Color.Parse("#80E5F3FE"));
        var updatedBackground = new SolidColorBrush(Color.Parse("#40D9F7BE"));
        var paragraph = new SkeletonParagraph
        {
            Rows = 3,
            Background = background
        };

        using var window = Show(paragraph);
        var lines = paragraph.GetVisualDescendants().OfType<SkeletonLine>().ToArray();
        lines.Length.ShouldBe(3);
        lines.All(static line => line.Background is not null).ShouldBeTrue();
        foreach (var line in lines)
        {
            line.Background.ShouldBeSameAs(background);
        }

        paragraph.Background = updatedBackground;
        Dispatcher.UIThread.RunJobs();

        foreach (var line in lines)
        {
            line.Background.ShouldBeSameAs(updatedBackground);
        }
    }

    [Fact]
    public void Active_Content_Alternatives_Preserve_Semantic_Marker_Identity()
    {
        var avatar = new SkeletonAvatar();
        using var window = Show(avatar);
        var contentLayers = avatar.GetVisualDescendants()
                                  .OfType<Border>()
                                  .Where(static border => border.Classes.Contains("semantic-content"))
                                  .ToArray();
        contentLayers.Length.ShouldBe(2);
        contentLayers.Count(static border => border.IsVisible).ShouldBe(1);

        avatar.IsActive = true;
        Dispatcher.UIThread.RunJobs();

        avatar.GetVisualDescendants()
              .OfType<Border>()
              .Where(static border => border.Classes.Contains("semantic-content"))
              .ShouldBe(contentLayers);
        contentLayers.Count(static border => border.IsVisible).ShouldBe(1);
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
