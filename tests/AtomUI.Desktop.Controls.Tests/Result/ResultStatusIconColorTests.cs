using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIResult = AtomUI.Desktop.Controls.Result;
using AvaloniaWindow = Avalonia.Controls.Window;
using IconControl = AtomUI.Controls.Icon;

namespace AtomUI.Desktop.Controls.Tests.Feedback;

public class ResultStatusIconColorTests
{
    static ResultStatusIconColorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(ResultStatus.Info)]
    [InlineData(ResultStatus.Success)]
    [InlineData(ResultStatus.Warning)]
    [InlineData(ResultStatus.Error)]
    public void Status_Icon_Keeps_The_Status_Colour_After_A_Publish_That_Changes_The_Icon_Tokens(ResultStatus status)
    {
        var result   = new AtomUIResult { Status = status, Header = "Header" };
        var provider = new ThemeConfigProvider { Child = result, Config = new ThemeConfigBuilder().Build() };
        Show(provider, () =>
        {
            AssertStatusIcon(result);

            provider.Config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build();
            Dispatcher.UIThread.RunJobs();
            AssertStatusIcon(result);

            provider.Config = new ThemeConfigBuilder().WithToken("FontSize", "18").Build();
            Dispatcher.UIThread.RunJobs();
            AssertStatusIcon(result);
        });
    }

    [Fact]
    public void Status_Change_Moves_The_Status_Colour_And_Leaves_User_Icon_Brushes_Alone()
    {
        var result   = new AtomUIResult { Status = ResultStatus.Success, Header = "Header" };
        var provider = new ThemeConfigProvider { Child = result, Config = new ThemeConfigBuilder().Build() };
        Show(provider, () =>
        {
            AssertStatusIcon(result);

            result.Status = ResultStatus.Error;
            Dispatcher.UIThread.RunJobs();
            AssertStatusIcon(result);

            var custom = new AtomUI.Icons.AntDesign.SmileOutlined { FillBrush = Brushes.Purple, Width = 20 };
            result.Icon = custom;
            Dispatcher.UIThread.RunJobs();
            FindStatusIcon(result).ShouldBeSameAs(custom);

            provider.Config = new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build();
            Dispatcher.UIThread.RunJobs();
            custom.FillBrush.ShouldBeSameAs(Brushes.Purple);
            custom.Width.ShouldBe(20);
            ColorOf(custom.StrokeBrush).ShouldBe(ColorOf(result.StatusIconBrush));
        });
    }

    private static void AssertStatusIcon(AtomUIResult result)
    {
        var expected = ColorOf(result.StatusIconBrush.ShouldNotBeNull());
        var icon     = FindStatusIcon(result);
        ColorOf(icon.FillBrush).ShouldBe(expected);
        ColorOf(icon.StrokeBrush).ShouldBe(expected);
        ColorOf(icon.Foreground).ShouldBe(expected);
        icon.Width.ShouldBe(result.StatusIconSize);
        icon.Height.ShouldBe(result.StatusIconSize);
    }

    private static IconControl FindStatusIcon(AtomUIResult result)
    {
        var presenter = result.GetVisualDescendants()
                              .OfType<ContentPresenter>()
                              .Single(static presenter => presenter.Name == "PART_StatusIconPresenter");
        return presenter.Child.ShouldBeAssignableTo<IconControl>().ShouldNotBeNull();
    }

    private static Color ColorOf(IBrush? brush) => brush.ShouldBeAssignableTo<ISolidColorBrush>().ShouldNotBeNull().Color;

    private static void Show(Control content, Action assertion)
    {
        var window = new AvaloniaWindow { Width = 600, Height = 500, Content = content };
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
}
