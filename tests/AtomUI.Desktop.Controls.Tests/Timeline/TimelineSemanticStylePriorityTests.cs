using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomTimeline = AtomUI.Desktop.Controls.Timeline;
using AtomTimelineItem = AtomUI.Desktop.Controls.TimelineItem;

namespace AtomUI.Desktop.Controls.Tests.Timeline;

public class TimelineSemanticStylePriorityTests
{
    static TimelineSemanticStylePriorityTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Default_Alignment_Tracks_Mode_And_Direction_And_Returns_After_Style_Removal()
    {
        var view = new TimelineSemanticStylePriorityView();
        var window = new Avalonia.Controls.Window { Width = 640, Height = 320, Content = view };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var timeline = view.GetVisualDescendants().OfType<AtomTimeline>().Single();
            var title = timeline.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Classes.Contains("semantic-item-title"));
            var presenter = timeline.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(part => part.Classes.Contains("semantic-item-content"));
            var item = timeline.Items.OfType<AtomTimelineItem>().Single();
            item.IsLabelLayout = true;
            void Settle()
            {
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
            }

            Settle();
            title.TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Center);
            timeline.Classes.Remove("semantic-owner");
            Settle();
            title.TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Right);
            presenter.Child.ShouldBeOfType<Avalonia.Controls.TextBlock>().TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Left);
            timeline.Mode = AtomUI.Controls.TimelineMode.End;
            Settle();
            title.TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Left);
            presenter.Child.ShouldBeOfType<Avalonia.Controls.TextBlock>().TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Right);
            timeline.Mode = AtomUI.Controls.TimelineMode.Start;
            Settle();
            title.TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Right);
            presenter.Child.ShouldBeOfType<Avalonia.Controls.TextBlock>().TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Left);
            timeline.Orientation = Avalonia.Layout.Orientation.Horizontal;
            Settle();
            title.TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Start);
            presenter.Child.ShouldBeOfType<Avalonia.Controls.TextBlock>().TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Start);
            presenter.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Center);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Generated_Part_Styles_Override_Default_Alignment_In_Vertical_Label_Layout()
    {
        var view = new TimelineSemanticStylePriorityView();
        var window = new Avalonia.Controls.Window { Width = 640, Height = 320, Content = view };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var timeline = view.GetVisualDescendants().OfType<AtomTimeline>().Single();
            timeline.Items.OfType<AtomTimelineItem>().Single().IsLabelLayout = true;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            var header = timeline.GetVisualDescendants().OfType<StackPanel>()
                .Single(panel => panel.Classes.Contains("semantic-item-header"));
            var title = timeline.GetVisualDescendants().OfType<TextBlock>()
                .Single(text => text.Classes.Contains("semantic-item-title"));
            var content = timeline.GetVisualDescendants().OfType<ContentPresenter>()
                .Single(presenter => presenter.Classes.Contains("semantic-item-content"));

            header.Tag.ShouldBe("header");
            title.Tag.ShouldBe("title");
            content.Tag.ShouldBe("content");
            header.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Right);
            title.TextAlignment.ShouldBe(Avalonia.Media.TextAlignment.Center);
            content.HorizontalAlignment.ShouldBe(Avalonia.Layout.HorizontalAlignment.Right);
        }
        finally
        {
            window.Close();
        }
    }
}
