using System.Reflection;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Resources;
using AtomUIGallery.ShowCases.Button;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using ButtonType = AtomUI.Desktop.Controls.ButtonType;
using AtomRibbonBadge = AtomUI.Desktop.Controls.RibbonBadge;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class ButtonShowCasePageTests
{

    [Fact]
    public void Button_ShowCase_Declares_A_Deferred_Semantic_Part_Preview()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/Button/Views/ButtonShowCase.axaml");

        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("<gallery:SemanticPartPreview");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Button}\"");
        source.ShouldContain("Name=\"SemanticPartDemoButton\"");
        CountOccurrences(source, "<gallery:SemanticPartDescription").ShouldBe(3);
        source.ShouldContain("Path=\"root\"");
        source.ShouldContain("Path=\"icon\"");
        source.ShouldContain("Path=\"content\"");
    }

    [Fact]
    public void Button_Semantic_Preview_Is_Materialized_Only_After_The_Tab_Is_Selected()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ButtonShowCase
        {
            DataContext = new ButtonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 800, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Button>()
                .ShouldNotContain(static button => button.Name == "SemanticPartDemoButton");

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().Count().ShouldBe(1);
            page.GetVisualDescendants()
                .OfType<AtomUI.Desktop.Controls.Button>()
                .Count(static button => button.Name == "SemanticPartDemoButton")
                .ShouldBe(1);
        });
    }

    [Fact]
    public void Button_Semantic_Part_Example_Matches_Object_And_Function_Contract()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/Button/Views/ButtonShowCase.axaml");
        var localization =
            ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/Button/Localization/en-US.xlf");
        var examples = ExtractButtonExampleItems(source);

        var item = ExtractShowCaseItemByTitle(
            examples,
            "ButtonShowCaseLangResource SemanticPartStyleTitle");

        item.ShouldContain("SourceKey=\"button-semantic-part\"");
        item.ShouldContain("Span=\"Full\"");
        item.ShouldContain("IsDeferredContentEnabled=\"True\"");
        item.ShouldContain("<gallery:ShowCaseItem.DeferredContentTemplate>");
        item.ShouldContain("ButtonShowCaseLangResource SemanticPartStyleDescription");
        item.ShouldContain("ButtonShowCaseLangResource P2ContentSemanticObject");
        item.ShouldContain("ButtonShowCaseLangResource P2ContentSemanticFunction");
        item.ShouldContain("Selector=\"atom|Button.semantic-part-demo\"");
        item.ShouldContain("<atom:ButtonContentStyle x:SetterTargetType=\"ContentPresenter\">");
        item.ShouldContain("Selector=\"atom|Button.semantic-part-demo.semantic-object\"");
        item.ShouldContain("Selector=\"atom|Button.semantic-part-demo[ButtonType=Primary]\"");
        item.ShouldContain("<atom:ButtonContentStyle x:SetterTargetType=\"ContentPresenter\">");
        item.ShouldContain("x:SetterTargetType=\"ContentPresenter\"");
        item.ShouldContain("<Setter Property=\"Background\" Value=\"#171717\" />");
        item.ShouldContain("Property=\"Foreground\"");
        item.ShouldContain("Value=\"{atom:SharedTokenResource ColorText}\"");
        item.ShouldContain("<Setter Property=\"Foreground\" Value=\"#FFFFFF\" />");
        CountOccurrences(item, "Classes=\"semantic-part-demo").ShouldBe(2);
        item.ShouldNotContain("Selector=\".semantic-content\"");
        item.ShouldNotContain("/template/ .semantic-content");
        localization.ShouldContain("<source>Custom Semantic Part styling</source>");
        localization.ShouldContain(
            "<source>Use owner-scoped selectors and Button state selectors to customize published Semantic Parts.</source>");
        localization.ShouldNotContain("Semantic DOM");
        localization.ShouldNotContain("semantic dom");
        localization.ShouldNotContain("classNames");
        localization.ShouldContain("<source>Object</source>");
        localization.ShouldContain("<source>Function</source>");
    }

    [Fact]
    public void Button_Semantic_Part_Example_Applies_Styles_To_Runtime_Parts()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ButtonShowCase
        {
            DataContext = new ButtonViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 1000, _ =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "button-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var buttons = page.GetVisualDescendants()
                              .OfType<AtomUI.Desktop.Controls.Button>()
                              .Where(static button => button.Classes.Contains("semantic-part-demo"))
                              .ToArray();
            buttons.Length.ShouldBe(2);

            var objectButton = buttons.Single(static button => button.Classes.Contains("semantic-object"));
            var functionButton = buttons.Single(static button => button.ButtonType == ButtonType.Primary);

            objectButton.Effect.ShouldBeOfType<DropShadowEffect>().BlurRadius.ShouldBe(2);
            objectButton.Effect.ShouldBeOfType<DropShadowEffect>().OffsetY.ShouldBe(1);
            functionButton.Background.ShouldBeAssignableTo<ISolidColorBrush>();
            ((ISolidColorBrush)functionButton.Background!).Color.ShouldBe(Color.Parse("#171717"));

            var objectContent = GetSemanticContent(objectButton);
            var functionContent = GetSemanticContent(functionButton);
            BrushShouldHaveSameColor(
                objectContent.Foreground,
                GetThemeResource<IBrush>(SharedTokenKind.ColorText));
            BrushShouldHaveSameColor(functionContent.Foreground, Brushes.White);

            SetPseudoClass(objectButton, ":pointerover", true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(
                objectContent.Foreground,
                GetThemeResource<IBrush>(SharedTokenKind.ColorText));

            SetPseudoClass(functionButton, ":pointerover", true);
            Dispatcher.UIThread.RunJobs();
            functionButton.IsMotionEnabled.ShouldBeTrue();
            functionButton.IsWaveSpiritEnabled.ShouldBeTrue();

            BrushShouldHaveSameColor(functionContent.Foreground, Brushes.White);
            BrushShouldHaveSameColor(
                functionButton.BorderBrush,
                GetThemeResource<IBrush>(SharedTokenKind.ColorBorder));

            SetPrivatePropertyValue(functionButton, "IsPressed", true);
            functionButton.IsPressed.ShouldBeTrue();
            SetPrivatePropertyValue(functionButton, "IsPressed", false);
            functionButton.IsPressed.ShouldBeFalse();
            Dispatcher.UIThread.RunJobs();

            var waveSpiritDecorator = GetPrivateFieldValue(functionButton, "_waveSpiritDecorator");
            var waveBrush = GetPublicPropertyValue<IBrush?>(waveSpiritDecorator, "WaveBrush");
            BrushShouldHaveSameColor(
                waveBrush,
                GetThemeResource<IBrush>(SharedTokenKind.ColorBorder));
        });
    }

    [Fact]
    public void Button_ShowCase_Version_Ribbon_Anchors_To_Real_ShowCaseItem_Right_Edge()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ButtonShowCase
        {
            DataContext = new ButtonViewModel(new TestScreen())
        };
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = page
        };

        ShowInWindow(visualLayerManager, 1440, 900, () =>
        {
            var item = page.GetVisualDescendants()
                           .OfType<ShowCaseItem>()
                           .Single(showCaseItem => showCaseItem.BadgeText == "v6.0.6");
            var ribbonBadge = item.GetVisualDescendants()
                                  .OfType<AtomRibbonBadge>()
                                  .Single();
            var label = visualLayerManager.GetVisualDescendants()
                                          .OfType<TextBlock>()
                                          .Single(textBlock => textBlock.Text == "v6.0.6");

            var labelRight = label.TranslatePoint(new Point(label.Bounds.Width, 0), visualLayerManager);
            var targetRight = ribbonBadge.TranslatePoint(new Point(ribbonBadge.Bounds.Width, 0), visualLayerManager);

            labelRight.ShouldNotBeNull();
            targetRight.ShouldNotBeNull();
            (labelRight.Value.X - targetRight.Value.X).ShouldBe(8, 1,
                "ShowCaseItem version RibbonBadge should match Ant Design's right:-badgeRibbonOffset placement.");
        });
    }

    private static string ExtractButtonExampleItems(string source)
    {
        const string firstItemMarker = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string ExtractShowCaseItemByTitle(string source, string titleResource)
    {
        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemCloseMarker = "</gallery:ShowCaseItem>";

        var titleIndex = source.IndexOf(titleResource, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemClose = source.IndexOf(itemCloseMarker, titleIndex, StringComparison.Ordinal);
        itemClose.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemClose + itemCloseMarker.Length)];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        ShowInWindow(content, width, height, _ => assertion());
    }

    private static void ShowInWindow(
        Control content,
        double width,
        double height,
        Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Content = content,
            Width   = width,
            Height  = height
        };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static ContentPresenter GetSemanticContent(Control button)
    {
        var content = button.GetVisualDescendants()
                            .OfType<ContentPresenter>()
                            .SingleOrDefault(static presenter => presenter.Classes.Contains("semantic-content"));
        content.ShouldNotBeNull();
        return content!;
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current.ShouldNotBeNull();
        application.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, IBrush? expected)
    {
        actual.ShouldNotBeNull();
        expected.ShouldNotBeNull();
        actual.ShouldBeAssignableTo<ISolidColorBrush>();
        expected.ShouldBeAssignableTo<ISolidColorBrush>();
        ((ISolidColorBrush)actual!).Color.ShouldBe(((ISolidColorBrush)expected!).Color);
    }

    private static object GetPrivateFieldValue(object target, string fieldName)
    {
        var field = target.GetType().GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        var value = field.GetValue(target);
        value.ShouldNotBeNull();
        return value;
    }

    private static T GetPublicPropertyValue<T>(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        property.ShouldNotBeNull();
        return (T)property.GetValue(target)!;
    }

    private static void SetPrivatePropertyValue<T>(object target, string propertyName, T value)
    {
        var property = typeof(Avalonia.Controls.Button).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);
        property.ShouldNotBeNull();
        var setter = property.GetSetMethod(true);
        setter.ShouldNotBeNull();
        setter.Invoke(target, [value]);
    }

    private static void SetPseudoClass(Control control, string pseudoClass, bool value)
    {
        ((IPseudoClasses)control.Classes).Set(pseudoClass, value);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
