using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
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
using AtomUISizeType = AtomUI.SizeType;

namespace AtomUI.Desktop.Controls.Tests.Card;

public class CardBehaviorTests
{
    static CardBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void CardGridContent_ColumnDefinitions_Wrapper_Uses_Avalonia_Property()
    {
        var gridContent = new Desktop.Controls.CardGridContent();
        var columns     = new ColumnDefinitions("*,2*");

        gridContent.SetValue(Desktop.Controls.CardGridContent.ColumnDefinitionsProperty, columns);

        gridContent.ColumnDefinitions.ShouldBeSameAs(columns);
    }

    [Fact]
    public void CardGridContent_RowDefinitions_Wrapper_Uses_Avalonia_Property()
    {
        var gridContent = new Desktop.Controls.CardGridContent();
        var rows        = new RowDefinitions("Auto,*");

        gridContent.RowDefinitions = rows;

        gridContent.GetValue(Desktop.Controls.CardGridContent.RowDefinitionsProperty).ShouldBeSameAs(rows);
    }

    [Fact]
    public void Card_Content_Returns_To_Default_Layout_When_Grid_Content_Is_Replaced()
    {
        var card = new Desktop.Controls.Card
        {
            Width   = 320,
            Header  = "Title",
            Content = new Desktop.Controls.CardGridContent()
        };

        ShowInWindow(card, () =>
        {
            var body = FindCardContentBorder(card);
            body.Padding.ShouldBe(new Thickness(0));

            card.Content = new TextBlock { Text = "Plain content" };
            Dispatcher.UIThread.RunJobs();

            body.Padding.ShouldNotBe(new Thickness(0));
        });
    }

    [Fact]
    public void Card_Header_Frame_Keeps_Themed_Bottom_Separator()
    {
        var card = new Desktop.Controls.Card
        {
            Width   = 320,
            Header  = "Title",
            Content = new TextBlock { Text = "Plain content" }
        };

        ShowInWindow(card, () =>
        {
            var headerFrame = FindTemplatePixelAlignedBorder(card, "HeaderFrame");

            headerFrame.BorderBrush.ShouldNotBeNull();
            headerFrame.BorderThickness.Bottom.ShouldBeGreaterThan(0);
            headerFrame.MinHeight.ShouldBeGreaterThan(0);
            headerFrame.Padding.Left.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public void Card_Frame_Arranges_Header_And_Actions_Inside_The_Outer_Border()
    {
        var card = new Desktop.Controls.Card
        {
            Width           = 320,
            Header          = "Title",
            Content         = "Body",
            BorderThickness = new Thickness(1)
        };
        card.Actions.Add(new Desktop.Controls.CardActionButton());

        ShowInWindow(card, () =>
        {
            var frame       = FindTemplatePixelAlignedBorder(card, "Frame");
            var headerFrame = FindTemplatePixelAlignedBorder(card, "HeaderFrame");
            var actionPanel = card.GetVisualDescendants()
                                  .OfType<TemplatedControl>()
                                  .Single(control => control.Name == "PART_ActionPanel");

            frame.Child.ShouldNotBeNull();
            headerFrame.TranslatePoint(default, frame).ShouldBe(new Point(1, 1));
            var actionOffset = actionPanel.TranslatePoint(default, frame).ShouldNotBeNull();
            actionOffset.X.ShouldBe(1);
            headerFrame.Bounds.Width.ShouldBe(frame.Bounds.Width - 2);
            actionPanel.Bounds.Width.ShouldBe(frame.Bounds.Width - 2);
        });
    }

    [Fact]
    public void Owner_Scoped_Root_Style_Can_Restore_A_Borderless_Card_Border()
    {
        var card = new Desktop.Controls.Card
        {
            Width        = 320,
            Header       = "Title",
            Content      = "Body",
            StyleVariant = Desktop.Controls.CardStyleVariant.Borderless
        };
        card.Classes.Add("semantic-owner");
        card.Styles.Add(new Style(selector =>
            selector.OfType<Desktop.Controls.Card>().Class("semantic-owner"))
        {
            Setters =
            {
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(1))
            }
        });

        ShowInWindow(card, () =>
        {
            FindTemplatePixelAlignedBorder(card, "Frame")
                .BorderThickness.ShouldBe(new Thickness(1));
        });
    }

    [Fact]
    public void Borderless_Card_Without_An_Owner_Override_Remains_Borderless()
    {
        var card = new Desktop.Controls.Card
        {
            Width        = 320,
            Header       = "Title",
            Content      = "Body",
            StyleVariant = Desktop.Controls.CardStyleVariant.Borderless
        };

        ShowInWindow(card, () =>
        {
            FindTemplatePixelAlignedBorder(card, "Frame")
                .BorderThickness.ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Card_Releases_SizeType_Binding_From_Replaced_Special_Content()
    {
        var oldGridContent = new Desktop.Controls.CardGridContent();
        var card = new Desktop.Controls.Card
        {
            SizeType = AtomUISizeType.Small,
            Content  = oldGridContent
        };

        ShowInWindow(card, () =>
        {
            oldGridContent.SizeType.ShouldBe(AtomUISizeType.Small);

            card.Content = new TextBlock { Text = "Plain content" };
            Dispatcher.UIThread.RunJobs();

            card.SizeType = AtomUISizeType.Large;
            Dispatcher.UIThread.RunJobs();

            oldGridContent.SizeType.ShouldBe(AtomUISizeType.Middle);
        });
    }

    [Fact]
    public void CardGridContent_Disposes_Prepared_Container_State_When_Item_Is_Removed()
    {
        var disposeCount = 0;
        var gridContent  = new DisposableCardGridContent(() => disposeCount++);
        var gridItem     = new Desktop.Controls.CardGridItem();

        gridContent.PrepareForTest(gridItem, "first", 0);
        gridContent.ClearForTest(gridItem);

        disposeCount.ShouldBe(1);
    }

    [Fact]
    public void Skeleton_exists_only_while_loading()
    {
        var card = new Desktop.Controls.Card
        {
            Width   = 320,
            Header  = "Title",
            Content = new TextBlock { Text = "Plain content" }
        };

        ShowInWindow(card, () =>
        {
            FindSkeletons(card).ShouldBeEmpty();

            card.IsLoading = true;
            Dispatcher.UIThread.RunJobs();

            var skeleton = FindSkeletons(card).ShouldHaveSingleItem();
            skeleton.IsLoading.ShouldBeTrue();
            skeleton.IsActive.ShouldBeTrue();
            skeleton.ParagraphRows.ShouldBe(4);
            skeleton.IsShowTitle.ShouldBeFalse();
            skeleton.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Stretch);
            skeleton.VerticalAlignment.ShouldBe(Avalonia.Layout.VerticalAlignment.Stretch);
            FindCardContentBorder(card).GetVisualDescendants().ShouldContain(skeleton);

            card.IsLoading = false;
            Dispatcher.UIThread.RunJobs();

            FindSkeletons(card).ShouldBeEmpty();
        });
    }

    [Fact]
    public void Loading_toggle_restores_content()
    {
        var content = new TextBlock { Text = "Plain content" };
        var card = new Desktop.Controls.Card
        {
            Width     = 320,
            Header    = "Title",
            Content   = content,
            IsLoading = true
        };

        ShowInWindow(card, () =>
        {
            var presenter = FindContentPresenter(card);
            presenter.IsVisible.ShouldBeFalse();
            FindSkeletons(card).ShouldHaveSingleItem();

            card.IsLoading = false;
            Dispatcher.UIThread.RunJobs();
            presenter.IsVisible.ShouldBeTrue();
            content.IsEffectivelyVisible.ShouldBeTrue();
            content.GetVisualAncestors().ShouldContain(presenter);
            FindSkeletons(card).ShouldBeEmpty();

            card.IsLoading = true;
            Dispatcher.UIThread.RunJobs();
            presenter.IsVisible.ShouldBeFalse();
            FindSkeletons(card).ShouldHaveSingleItem();

            card.IsLoading = false;
            Dispatcher.UIThread.RunJobs();
            presenter.IsVisible.ShouldBeTrue();
            content.IsEffectivelyVisible.ShouldBeTrue();
            FindSkeletons(card).ShouldBeEmpty();
        });
    }

    [Fact]
    public void Actions_sync_and_visibility()
    {
        var card = new Desktop.Controls.Card
        {
            Width   = 320,
            Header  = "Title",
            Content = "Body"
        };
        var first  = new Desktop.Controls.CardActionButton();
        var second = new Desktop.Controls.CardActionButton();
        card.Actions.Add(first);

        ShowInWindow(card, () =>
        {
            var panel = FindActionPanel(card);
            panel.IsVisible.ShouldBeTrue();
            first.GetVisualAncestors().ShouldContain(panel);

            card.Actions.Add(second);
            Dispatcher.UIThread.RunJobs();
            second.GetVisualAncestors().ShouldContain(panel);

            card.Actions.Remove(first);
            card.Actions.Remove(second);
            Dispatcher.UIThread.RunJobs();
            panel.IsVisible.ShouldBeFalse();
            first.GetVisualParent().ShouldBeNull();

            card.Actions.Add(first);
            Dispatcher.UIThread.RunJobs();
            panel.IsVisible.ShouldBeTrue();
            first.GetVisualAncestors().ShouldContain(panel);
        });
    }

    [Fact]
    public void Headerless_follows_header_and_extra()
    {
        var card = new Desktop.Controls.Card { Width = 320, Content = "Body" };

        ShowInWindow(card, () =>
        {
            card.Classes.Contains(Desktop.Controls.CardPseudoClass.Headerless).ShouldBeTrue();

            card.Header = "Title";
            card.Classes.Contains(Desktop.Controls.CardPseudoClass.Headerless).ShouldBeFalse();

            card.Header = null;
            card.Classes.Contains(Desktop.Controls.CardPseudoClass.Headerless).ShouldBeTrue();

            card.Extra = "More";
            card.Classes.Contains(Desktop.Controls.CardPseudoClass.Headerless).ShouldBeFalse();

            card.Extra = null;
            card.Classes.Contains(Desktop.Controls.CardPseudoClass.Headerless).ShouldBeTrue();

            card.ExtraTemplate = new Avalonia.Controls.Templates.FuncDataTemplate<object>((_, _) => new TextBlock());
            card.Classes.Contains(Desktop.Controls.CardPseudoClass.Headerless).ShouldBeFalse();
        });
    }

    [Fact]
    public void Not_loading_card_has_no_skeleton_visuals()
    {
        var card = new Desktop.Controls.Card
        {
            Width   = 320,
            Header  = "Title",
            Content = new TextBlock { Text = "Plain content" }
        };

        ShowInWindow(card, () =>
        {
            card.GetVisualDescendants().OfType<AtomUI.Controls.AbstractSkeleton>().ShouldBeEmpty();
            card.GetVisualDescendants().OfType<Panel>()
                .Single(panel => panel.Name == "PART_LoadingHost")
                .Children.ShouldHaveSingleItem().ShouldBeOfType<ContentPresenter>();
            // Visual elements of a default card with a header and plain content: 16 before the skeleton became
            // on demand (Skeleton, its root layout and placeholder host), 14 since.
            card.GetVisualDescendants().Count().ShouldBeLessThanOrEqualTo(14);
        });
    }

    private static List<Desktop.Controls.Skeleton> FindSkeletons(Desktop.Controls.Card card)
    {
        return card.GetVisualDescendants().OfType<Desktop.Controls.Skeleton>().ToList();
    }

    private static ContentPresenter FindContentPresenter(Desktop.Controls.Card card)
    {
        var host = card.GetVisualDescendants()
                       .OfType<Panel>()
                       .Single(panel => panel.Name == "PART_LoadingHost");
        return host.Children.OfType<ContentPresenter>().Single();
    }

    private static TemplatedControl FindActionPanel(Desktop.Controls.Card card)
    {
        return card.GetVisualDescendants()
                   .OfType<TemplatedControl>()
                   .Single(control => control.Name == "PART_ActionPanel");
    }

    private static Border FindCardContentBorder(Desktop.Controls.Card card)
    {
        return FindTemplateBorder(card, "CardContent");
    }

    private static Border FindTemplateBorder(Desktop.Controls.Card card, string name)
    {
        return card.GetVisualDescendants()
                   .OfType<Border>()
                   .Single(border => border.Name == name);
    }

    private static PixelAlignedBorder FindTemplatePixelAlignedBorder(Desktop.Controls.Card card, string name)
    {
        return card.GetVisualDescendants()
                   .OfType<PixelAlignedBorder>()
                   .Single(border => border.Name == name && ReferenceEquals(border.TemplatedParent, card));
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 480,
            Height  = 320,
            Content = content
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
        }
    }

    private sealed class DisposableCardGridContent : Desktop.Controls.CardGridContent
    {
        private readonly Action _onDispose;

        public DisposableCardGridContent(Action onDispose)
        {
            _onDispose = onDispose;
        }

        protected override void PrepareCardGridItem(Desktop.Controls.CardGridItem cardGridItem,
                                                    object? item,
                                                    int index,
                                                    CompositeDisposable compositeDisposable)
        {
            compositeDisposable.Add(Disposable.Create(_onDispose));
        }

        public void PrepareForTest(Control container, object? item, int index)
        {
            PrepareContainerForItemOverride(container, item, index);
        }

        public void ClearForTest(Control container)
        {
            ClearContainerForItemOverride(container);
        }
    }
}
