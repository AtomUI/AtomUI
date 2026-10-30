using System.Reflection;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases;
using AtomUIGallery.ShowCases.TabControl;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomScrollBar = AtomUI.Desktop.Controls.ScrollBar;
using TabStripPage = AtomUIGallery.ShowCases.TabStrip.TabStripShowCase;
using TabStripViewModel = AtomUIGallery.ShowCases.TabStrip.TabStripViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class TabOverflowSearchShowCaseTests
{
    static TabOverflowSearchShowCaseTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Shared_Search_Popup_Matches_Ant_Design_Interaction_Contract()
    {
        var markup = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/SearchableTabOverflowPopup.axaml");
        var code   = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/SearchableTabOverflowPopup.axaml.cs");

        markup.ShouldContain("Width=\"200\"");
        markup.ShouldContain("MaxHeight=\"300\"");
        markup.ShouldContain("VerticalScrollBarVisibility=\"Hidden\"");
        markup.ShouldNotContain("VerticalScrollBarVisibility=\"Auto\"");
        markup.ShouldNotContain("atom:ScrollViewer.IsLiteMode");
        markup.ShouldNotContain("atom:ScrollViewer.AllowAutoHide");
        CountOccurrences(markup, "CornerRadius=\"{atom:SharedTokenResource BorderRadiusLG}\"").ShouldBe(2);
        markup.ShouldContain("x:CompileBindings=\"True\"");
        markup.ShouldNotContain("x:CompileBindings=\"False\"");
        markup.ShouldNotContain("Focusable=\"True\"");
        markup.ShouldContain("Padding=\"12,8\"");
        markup.ShouldContain("SharedTokenResource ColorBorder}");
        markup.ShouldContain("SharedTokenResource BoxShadows}");
        markup.ShouldContain("MenuTokenResource ItemHeight}");
        markup.ShouldNotContain("MenuTokenResource ItemMargin}");
        markup.ShouldContain("MenuTokenResource MenuPopupContentPadding}");
        markup.ShouldContain("IsAllowClear=\"True\"");
        markup.ShouldContain("SearchOutlined");
        markup.ShouldContain("ItemsSource=\"{Binding FilteredItems,");
        code.ShouldContain("StringComparison.OrdinalIgnoreCase");
        code.ShouldContain("context.PropertyChanged += HandleContextPropertyChanged");
        code.ShouldContain("PropertyChanged -= HandleContextPropertyChanged");
        code.ShouldContain("_filteredItems.Clear()");
        code.ShouldContain("Key.Escape");
        code.ShouldContain("Key.Home");
        code.ShouldContain("Key.End");
    }

    [Fact]
    public void Search_Popup_Renders_Each_Overflow_Item_Header()
    {
        var context = CreateContext();
        Publish(context, [CreateItem("Alpha"), CreateItem("Beta")]);
        var popup = new SearchableTabOverflowPopup { Context = context };
        var window = new AvaloniaWindow { Content = popup };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.GetVisualDescendants()
                 .OfType<AvaloniaTextBlock>()
                 .Select(textBlock => textBlock.Text)
                 .Where(text => !string.IsNullOrEmpty(text))
                 .ShouldContain("Alpha");
            popup.GetVisualDescendants()
                 .OfType<AvaloniaTextBlock>()
                 .Select(textBlock => textBlock.Text)
                 .Where(text => !string.IsNullOrEmpty(text))
                 .ShouldContain("Beta");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Search_Popup_Scrolls_Without_A_Visible_Or_Reserved_Scrollbar_Gutter()
    {
        var context = CreateContext();
        Publish(context, Enumerable.Range(0, 20).Select(index => CreateItem($"Document {index:00}")).ToArray());
        var popup = new SearchableTabOverflowPopup { Context = context };
        var window = new AvaloniaWindow
        {
            Width = 240,
            Height = 500,
            Content = popup
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var scrollViewer = popup.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                                    .Single(item => item.MaxHeight == 300);
            var verticalScrollBar = scrollViewer.GetVisualDescendants()
                                                .OfType<AtomScrollBar>()
                                                .Single(item => item.Orientation == Orientation.Vertical);
            var firstItem = popup.GetVisualDescendants()
                                 .OfType<AtomButton>()
                                 .First(button => button.Tag is TabOverflowItem);
            var itemTransform = firstItem.TransformToVisual(scrollViewer).ShouldNotBeNull();
            var itemBounds = new Rect(firstItem.Bounds.Size).TransformToAABB(itemTransform);
            var leftGap = itemBounds.Left;
            var rightGap = scrollViewer.Bounds.Width - itemBounds.Right;

            leftGap.ShouldBe(rightGap, 0.5);
            scrollViewer.VerticalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Hidden);
            verticalScrollBar.IsVisible.ShouldBeFalse();
            scrollViewer.ScrollBarMaximum.Y.ShouldBeGreaterThan(0);

            var wheelPoint = firstItem.TranslatePoint(
                new Point(firstItem.Bounds.Width / 2, firstItem.Bounds.Height / 2),
                window);
            wheelPoint.ShouldNotBeNull();
            window.MouseMove(wheelPoint.Value);
            window.MouseWheel(wheelPoint.Value, new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            scrollViewer.Offset.Y.ShouldBeGreaterThan(0);

            scrollViewer.Offset = new Vector(0, scrollViewer.ScrollBarMaximum.Y);
            Dispatcher.UIThread.RunJobs();
            scrollViewer.Offset.Y.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Search_Popup_Exposes_Its_Surface_Corner_Radius_To_The_Popup_Host()
    {
        var popup = new SearchableTabOverflowPopup();
        var window = new AvaloniaWindow { Content = popup };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            popup.CornerRadius.TopLeft.ShouldBeGreaterThan(0);
            popup.CornerRadius.TopRight.ShouldBe(popup.CornerRadius.TopLeft);
            popup.CornerRadius.BottomRight.ShouldBe(popup.CornerRadius.TopLeft);
            popup.CornerRadius.BottomLeft.ShouldBe(popup.CornerRadius.TopLeft);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData("TabControl", "TabControl", "CardTabControl")]
    [InlineData("TabStrip", "TabStrip", "CardTabStrip")]
    public void Four_Tab_Owners_Demonstrate_The_Shared_Search_Popup(
        string showcase,
        string lineOwner,
        string cardOwner)
    {
        var source = ReadRepoFile(
            $"controlgallery/AtomUIGallery/ShowCases/Navigation/{showcase}/Views/{showcase}ShowCase.axaml");

        source.ShouldContain($"{showcase}OverflowPopupSearchTitle");
        source.ShouldContain("SourceKey=\"tab-");
        source.ShouldContain($"<atom:{lineOwner}.OverflowPopupTemplate>");
        source.ShouldContain($"<atom:{cardOwner}.OverflowPopupTemplate>");
        CountOccurrences(source, "<gallery:SearchableTabOverflowPopup").ShouldBe(2);
        CountOccurrences(source, "MaxWidth=\"720\"").ShouldBe(2);
        CountOccurrences(source, " Width=\"720\"").ShouldBe(0);
    }

    [Fact]
    public void Search_Popup_Releases_Context_Subscription_While_Detached_And_Refreshes_On_Reattach()
    {
        var context = CreateContext();
        Publish(context, [CreateItem("Alpha"), CreateItem("Beta")]);
        var popup = new SearchableTabOverflowPopup
        {
            Context = context,
            Query   = "ALP"
        };
        var window = new AvaloniaWindow { Content = popup };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            popup.FilteredItems.Select(item => item.Header).ShouldBe(["Alpha"]);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            popup.FilteredItems.ShouldBeEmpty();
            popup.Query.ShouldBe("ALP");

            Publish(context, [CreateItem("Gamma"), CreateItem("Alpine")]);
            popup.FilteredItems.ShouldBeEmpty();

            window.Content = popup;
            Dispatcher.UIThread.RunJobs();
            popup.FilteredItems.Select(item => item.Header).ShouldBe(["Alpine"]);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Search_Popup_Transfers_Focus_Only_After_Explicit_Directional_Input()
    {
        var context = CreateContext();
        Publish(context, [CreateItem("Alpha"), CreateItem("Beta")]);
        var popup = new SearchableTabOverflowPopup { Context = context };
        var window = new AvaloniaWindow { Content = popup };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var input = popup.FindControl<LineEdit>("PART_SearchInput").ShouldNotBeNull();
            input.Focus();
            Dispatcher.UIThread.RunJobs();

            var focusedInput = TopLevel.GetTopLevel(popup)!
                                       .FocusManager
                                       .GetFocusedElement()
                                       .ShouldBeAssignableTo<Interactive>()!;
            focusedInput.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Source = focusedInput,
                Key = Key.Down
            });
            Dispatcher.UIThread.RunJobs();

            var firstItem = popup.GetVisualDescendants()
                                 .OfType<AtomButton>()
                                 .First(button => button.Tag is TabOverflowItem);
            TopLevel.GetTopLevel(popup)!.FocusManager.GetFocusedElement().ShouldBeSameAs(firstItem);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void Overflow_Examples_Keep_Menu_Activators_Inside_A_Narrow_Masonry_Column(
        int exampleIndex)
    {
        var page = new TabControlShowCase
        {
            DataContext = new TabControlViewModel(new TestScreen())
        };
        var examples = page.FindControl<ShowCasePanel>("ExamplesContent").ShouldNotBeNull();
        var window = new AvaloniaWindow
        {
            Width   = 1400,
            Height  = 1800,
            Content = CreatePopupOverlayHost(page)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var overflowExample = examples.Children[exampleIndex].ShouldBeOfType<ShowCaseItem>();
            overflowExample.MaterializeDeferredContent();
            overflowExample.BringIntoView();
            Dispatcher.UIThread.RunJobs();

            AssertOverflowOwnersFit<BaseTabControl>(
                overflowExample,
                expectedOwnerCount: 2,
                assertSearchPopup: exampleIndex == 8);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    public void TabStrip_Overflow_Examples_Keep_Menu_Activators_Inside_A_Narrow_Masonry_Column(
        int exampleIndex)
    {
        var page = new TabStripPage
        {
            DataContext = new TabStripViewModel(new TestScreen())
        };
        var examples = page.FindControl<ShowCasePanel>("ExamplesContent").ShouldNotBeNull();
        var window = new AvaloniaWindow
        {
            Width   = 1400,
            Height  = 1800,
            Content = CreatePopupOverlayHost(page)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var overflowExample = examples.Children[exampleIndex].ShouldBeOfType<ShowCaseItem>();
            overflowExample.MaterializeDeferredContent();
            overflowExample.BringIntoView();
            Dispatcher.UIThread.RunJobs();

            AssertOverflowOwnersFit<BaseTabStrip>(
                overflowExample,
                expectedOwnerCount: exampleIndex == 7 ? 1 : 2,
                assertSearchPopup: exampleIndex == 8);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        for (var index = 0;;)
        {
            index = source.IndexOf(value, index, StringComparison.Ordinal);
            if (index < 0)
            {
                return count;
            }

            count++;
            index += value.Length;
        }
    }

    private static void AssertOverflowOwnersFit<TOwner>(
        ShowCaseItem overflowExample,
        int expectedOwnerCount,
        bool assertSearchPopup)
        where TOwner : Control
    {
        var owners = overflowExample.GetVisualDescendants().OfType<TOwner>().ToArray();
        owners.Length.ShouldBe(expectedOwnerCount);
        foreach (var owner in owners)
        {
            var ownerTransform = owner.TransformToVisual(overflowExample).ShouldNotBeNull();
            var ownerBounds = new Rect(owner.Bounds.Size).TransformToAABB(ownerTransform);
            ownerBounds.Right.ShouldBeLessThanOrEqualTo(overflowExample.Bounds.Width + 0.5);

            var indicator = owner.GetVisualDescendants()
                                 .OfType<IconButton>()
                                 .Single(button => button.Name == "PART_ScrollMenuIndicator");
            var viewer = indicator.GetVisualAncestors()
                                  .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                                  .First();
            indicator.IsVisible.ShouldBeTrue(
                $"owner={owner.Bounds}, example={overflowExample.Bounds}, viewer={viewer.Bounds}, " +
                $"extent={viewer.Extent}, viewport={viewer.Viewport}, desired={viewer.DesiredSize}");
            indicator.Bounds.Width.ShouldBeGreaterThan(0);

            var transform = indicator.TransformToVisual(owner).ShouldNotBeNull();
            var indicatorBounds = new Rect(indicator.Bounds.Size).TransformToAABB(transform);
            indicatorBounds.Right.ShouldBeLessThanOrEqualTo(owner.Bounds.Width + 0.5);

            if (assertSearchPopup)
            {
                indicator.Focus();
                TopLevel.GetTopLevel(owner)!.FocusManager.GetFocusedElement().ShouldBeSameAs(indicator);

                indicator.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, indicator));
                Dispatcher.UIThread.RunJobs();

                var popupRoot = viewer.GetType()
                                      .GetProperty(
                                          "OverflowPopupRoot",
                                          BindingFlags.Instance |
                                          BindingFlags.Public |
                                          BindingFlags.NonPublic)
                                      .ShouldNotBeNull()
                                      .GetValue(viewer);
                var searchablePopup = popupRoot.ShouldBeOfType<SearchableTabOverflowPopup>();
                searchablePopup.FindControl<LineEdit>("PART_SearchInput").ShouldNotBeNull();
                searchablePopup.GetVisualDescendants()
                               .OfType<AvaloniaTextBlock>()
                               .Any(textBlock => textBlock.Text?.StartsWith("Tab-", StringComparison.Ordinal) == true)
                               .ShouldBeTrue();

                var shadowContainer = searchablePopup.GetVisualAncestors()
                                                     .Single(visual => visual.GetType().Name == "ShadowsAwareContainer");
                var cornerRadius = shadowContainer.GetType()
                                                  .GetProperty("CornerRadius", BindingFlags.Instance | BindingFlags.Public)
                                                  .ShouldNotBeNull()
                                                  .GetValue(shadowContainer)
                                                  .ShouldBeOfType<CornerRadius>();
                cornerRadius.TopLeft.ShouldBeGreaterThan(0);
                cornerRadius.TopRight.ShouldBe(cornerRadius.TopLeft);
                cornerRadius.BottomRight.ShouldBe(cornerRadius.TopLeft);
                cornerRadius.BottomLeft.ShouldBe(cornerRadius.TopLeft);
                TopLevel.GetTopLevel(owner)!.FocusManager.GetFocusedElement().ShouldBeSameAs(indicator);
            }
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Expected repository file to exist: {relativePath}");
    }

    private static VisualLayerManager CreatePopupOverlayHost(Control content)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 1400,
            Height = 1800
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager { Child = overlayPanel };
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic).ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
        return visualLayerManager;
    }

    private static TabOverflowPopupContext CreateContext()
    {
        return (TabOverflowPopupContext)Activator.CreateInstance(
            typeof(TabOverflowPopupContext),
            nonPublic: true)!;
    }

    private static TabOverflowItem CreateItem(string header)
    {
        var constructor = typeof(TabOverflowItem).GetConstructors(
            BindingFlags.Instance | BindingFlags.NonPublic).Single();
        return constructor.Invoke([header, header, null, true, false, false])
            .ShouldBeAssignableTo<TabOverflowItem>();
    }

    private static void Publish(
        TabOverflowPopupContext context,
        IReadOnlyList<TabOverflowItem> items)
    {
        var method = typeof(TabOverflowPopupContext).GetMethod(
            "Publish",
            BindingFlags.Instance | BindingFlags.NonPublic).ShouldNotBeNull();
        method!.Invoke(context, [items, null]);
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
