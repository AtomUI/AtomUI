using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIResult = AtomUI.Desktop.Controls.Result;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Feedback;

public class ResultIconContentTests
{
    static ResultIconContentTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static AvaloniaWindow Show(AtomUIResult result)
    {
        var window = new AvaloniaWindow { Width = 640, Height = 560, Content = result };
        window.Show();
        result.ApplyTemplate();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static ContentPresenter FindIconPresenter(AtomUIResult result)
    {
        return result.GetVisualDescendants().OfType<ContentPresenter>()
                     .Single(static presenter => presenter.Name == "PART_StatusIconPresenter");
    }

    private static Color Brush(Control? control)
    {
        return control.ShouldBeAssignableTo<AtomUI.Controls.Icon>()!.FillBrush.ShouldBeAssignableTo<ISolidColorBrush>()!.Color;
    }

    [Theory]
    [InlineData(ResultStatus.Info, typeof(ExclamationCircleFilled))]
    [InlineData(ResultStatus.Success, typeof(CheckCircleFilled))]
    [InlineData(ResultStatus.Warning, typeof(WarningFilled))]
    [InlineData(ResultStatus.Error, typeof(CloseCircleFilled))]
    [InlineData(ResultStatus.Waiting, typeof(ClockCircleFilled))]
    public void Status_shows_its_default_icon(ResultStatus status, Type icon)
    {
        var result = new AtomUIResult { Status = status, Header = "title" };
        var window = Show(result);
        try
        {
            var presenter = FindIconPresenter(result);
            presenter.IsVisible.ShouldBeTrue();
            presenter.Child.ShouldBeOfType(icon);
            presenter.Child!.Width.ShouldBe(result.StatusIconSize);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Waiting_icon_uses_the_success_colour()
    {
        var waiting = new AtomUIResult { Status = ResultStatus.Waiting };
        var success = new AtomUIResult { Status = ResultStatus.Success };
        var host = new StackPanel { Children = { waiting, success } };
        var window = new AvaloniaWindow { Width = 640, Height = 800, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var waitingIcon = FindIconPresenter(waiting).Child.ShouldNotBeNull();
            var successIcon = FindIconPresenter(success).Child.ShouldNotBeNull();
            Brush(waitingIcon).ShouldBe(Brush(successIcon));
            waiting.StatusIconBrush.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Object_icon_replaces_the_default_and_clearing_restores_it()
    {
        typeof(AtomUIResult).GetProperty(nameof(AtomUIResult.Icon))!.PropertyType.ShouldBe(typeof(object));

        var result = new AtomUIResult { Status = ResultStatus.Success };
        var window = Show(result);
        try
        {
            var presenter = FindIconPresenter(result);
            presenter.Child.ShouldBeOfType<CheckCircleFilled>();

            // A PathIcon, the pre-widening type, keeps working.
            var smile = new SmileOutlined();
            result.Icon = smile;
            Dispatcher.UIThread.RunJobs();
            presenter.Child.ShouldBeSameAs(smile);

            // Arbitrary content is hosted by the same presenter.
            result.Icon = "ok";
            Dispatcher.UIThread.RunJobs();
            presenter.Child.ShouldBeOfType<Avalonia.Controls.TextBlock>().Text.ShouldBe("ok");

            result.Icon = null;
            Dispatcher.UIThread.RunJobs();
            presenter.Child.ShouldBeOfType<CheckCircleFilled>();
        }
        finally
        {
            window.Close();
        }
    }
}
