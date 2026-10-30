using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
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
