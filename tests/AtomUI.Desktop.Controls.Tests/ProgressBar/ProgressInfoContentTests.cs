using AtomUI.Controls.Commons;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUICircleProgress = AtomUI.Desktop.Controls.CircleProgress;
using AtomUIDashboardProgress = AtomUI.Desktop.Controls.DashboardProgress;
using AtomUIProgressBar = AtomUI.Desktop.Controls.ProgressBar;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.ProgressBar;

public class ProgressInfoContentTests
{
    static ProgressInfoContentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Line_progress_shows_info_content_in_default_theme()
    {
        VerifyInfoContent(new AtomUIProgressBar { Width = 200, Value = 40 });
    }

    [Fact]
    public void Circle_progress_shows_info_content_in_default_theme()
    {
        VerifyInfoContent(new AtomUICircleProgress { Value = 40 });
    }

    [Fact]
    public void Dashboard_progress_shows_info_content_in_default_theme()
    {
        VerifyInfoContent(new AtomUIDashboardProgress { Value = 40 });
    }

    [Fact]
    public void Steps_progress_shows_info_content_in_default_theme()
    {
        VerifyInfoContent(new AtomUI.Desktop.Controls.StepsProgressBar { Steps = 5, Value = 40 });
        var steps = new AtomUI.Desktop.Controls.StepsProgressBar { Steps = 5, Value = 40 };
        ShowInWindow(steps, () =>
        {
            steps.ProgressInfoContent = new Border { Width = 30, Height = 10 };
            Dispatcher.UIThread.RunJobs();
            var presenter = Find(steps, "PART_ProgressInfoContentPresenter");
            presenter.IsVisible.ShouldBeTrue();
            presenter.Bounds.Width.ShouldBe(30);
            Find(steps, "PART_LayoutTransformControl").IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Info_content_shows_when_info_hidden_in_default_theme()
    {
        var progress = new AtomUIProgressBar { Width = 200, IsProgressInfoVisible = false };
        ShowInWindow(progress, () =>
        {
            Find(progress, "PART_ProgressInfoContentPresenter").IsVisible.ShouldBeFalse();
            progress.ProgressInfoContent = new Border { Width = 30, Height = 10 };
            Dispatcher.UIThread.RunJobs();
            Find(progress, "PART_ProgressIndicator").IsVisible.ShouldBeTrue();
            Find(progress, "PART_ProgressInfoContentPresenter").IsVisible.ShouldBeTrue();
        });
    }

    private static void VerifyInfoContent(AbstractProgressBar progress)
    {
        ShowInWindow(progress, () =>
        {
            Find(progress, "PART_ProgressInfoContentPresenter").IsVisible.ShouldBeFalse();
            Find(progress, "PART_LayoutTransformControl").IsVisible.ShouldBeTrue();

            progress.ProgressInfoContent = new Border { Width = 30, Height = 10 };
            Dispatcher.UIThread.RunJobs();

            Find(progress, "PART_ProgressInfoContentPresenter").IsVisible.ShouldBeTrue();
            Find(progress, "PART_LayoutTransformControl").IsVisible.ShouldBeFalse();
            Find(progress, "PART_SuccessCompletedIconPresenter").IsVisible.ShouldBeFalse();
            Find(progress, "PART_ExceptionCompletedIconPresenter").IsVisible.ShouldBeFalse();

            progress.ClearValue(AbstractProgressBar.ProgressInfoContentProperty);
            Dispatcher.UIThread.RunJobs();

            Find(progress, "PART_ProgressInfoContentPresenter").IsVisible.ShouldBeFalse();
        });
    }

    private static Control Find(Control root, string name)
    {
        return root.GetVisualDescendants().OfType<Control>().Single(c => c.Name == name);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 320, Height = 220, Content = content };
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
}
