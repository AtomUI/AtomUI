using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Form;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIButton = AtomUI.Desktop.Controls.Button;
using AtomUIForm = AtomUI.Desktop.Controls.Form;

namespace AtomUIGallery.Tests.ShowCases;

public class FormShowCaseSemanticTabTests
{
    [Fact]
    public void Form_Semantic_Tab_Materializes_The_Preview_And_Styling_Example()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new FormShowCase
        {
            DataContext = new FormViewModel(new TestScreen())
        };

        ShowInWindow(page, 1280, 900, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "FormItemSemanticPreview");
            preview.SemanticOwner.ShouldBeOfType<FormItem>();

            // The demo form must enter the failed state with two error messages.
            // The item owns 3 semantic-help-item nodes: 2 runtime message items
            // plus the (empty) static HelpText node from the template.
            var passwordItem = page.GetVisualDescendants()
                                   .OfType<FormItem>()
                                   .Single(static item => item.Name == "PasswordSemanticItem");
            var helpItems = passwordItem.GetVisualDescendants()
                                        .OfType<Avalonia.Controls.TextBlock>()
                                        .Where(static block => block.Classes.Contains("semantic-help-item"))
                                        .ToArray();
            helpItems.Length.ShouldBe(3);
            helpItems.Count(static block => !string.IsNullOrEmpty(block.Text)).ShouldBe(2);
        });
    }

    [Fact]
    public void Form_Semantic_Styling_Cards_Stretch_Equally_Below_The_Max_Width()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new FormShowCase
        {
            DataContext = new FormViewModel(new TestScreen())
        };

        ShowInWindow(page, 1400, 900, () =>
        {
            // The styling demo is the last deferred ShowCaseItem of Examples.
            var stylingItem = page.GetVisualDescendants()
                                  .OfType<ShowCaseItem>()
                                  .Last(static item => item.IsDeferredContentEnabled);
            stylingItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var cards = page.GetVisualDescendants()
                            .OfType<AtomUIForm>()
                            .Where(static form => form.Classes.Contains("semantic-card"))
                            .ToArray();
            cards.Length.ShouldBe(2);

            // Both cards stretch to the shared MaxWidth=800 cap instead of
            // hugging their content width: equal widths near the cap.
            cards[0].Bounds.Width.ShouldBe(cards[1].Bounds.Width, 0.5);
            cards[0].Bounds.Width.ShouldBeInRange(700, 800.5);

            // The 4*/20* star grid over equal card widths aligns the label
            // columns: both content areas start at the same x offset.
            var contentX = cards.Select(static card => card.GetVisualDescendants()
                                                           .OfType<ContentPresenter>()
                                                           .First(static cp => cp.Classes.Contains("semantic-content"))
                                                           .Bounds.X)
                                .ToArray();
            contentX[0].ShouldBe(contentX[1], 0.5);
        });
    }

    [Fact]
    public void Form_Semantic_Styling_Part_Styles_Do_Not_Leak_Into_Button_Templates()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new FormShowCase
        {
            DataContext = new FormViewModel(new TestScreen())
        };

        ShowInWindow(page, 1400, 900, () =>
        {
            var stylingItem = page.GetVisualDescendants()
                                  .OfType<ShowCaseItem>()
                                  .Last(static item => item.IsDeferredContentEnabled);
            stylingItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var cards = page.GetVisualDescendants()
                            .OfType<AtomUIForm>()
                            .Where(static form => form.Classes.Contains("semantic-card"))
                            .ToArray();
            cards.Length.ShouldBe(2);

            foreach (var card in cards)
            {
                // The FormItem-level content part keeps the +12 start padding…
                var itemContent = card.GetVisualDescendants()
                                      .OfType<ContentPresenter>()
                                      .Where(static cp => cp.Classes.Contains("semantic-content"))
                                      .Where(static cp => TemplateOwner(cp) is FormItem)
                                      .ToArray();
                itemContent.Length.ShouldBe(3);
                itemContent.ShouldAllBe(cp => cp.Padding.Left == 12);

                // …while the Submit/reset button template presenters stay at
                // the default padding — semantic-content is a cross-control
                // part marker, so the style must not reach nested templates.
                var buttonContent = card.GetVisualDescendants()
                                        .OfType<ContentPresenter>()
                                        .Where(static cp => TemplateOwner(cp) is AtomUIButton)
                                        .ToArray();
                buttonContent.Length.ShouldBe(2);
                buttonContent.ShouldAllBe(cp => cp.Padding == new Thickness(0));
            }
        });
    }

    private static TemplatedControl? TemplateOwner(Control element)
    {
        return element.GetVisualAncestors().OfType<TemplatedControl>().FirstOrDefault();
    }

    private static void ShowInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child = content
        };

        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width   = width,
            Height  = height
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
