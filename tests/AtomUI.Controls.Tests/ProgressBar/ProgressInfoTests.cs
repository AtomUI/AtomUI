using AtomUI.Controls.Commons;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Controls.Tests.ProgressBar;

public class ProgressInfoTests
{
    static ProgressInfoTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // 与 Desktop 默认模板同构的信息区：百分比标签、两个状态图标、内容呈现器。
    private static Canvas BuildInfoArea(INameScope scope)
    {
        var label = new Label { Name = AbstractProgressBar.PercentageLabelPart };
        var labelHost = new LayoutTransformControl
        {
            Name = AbstractProgressBar.LayoutTransformControlPart,
            Child = label
        };
        var exceptionIcon = new IconPresenter { Name = AbstractProgressBar.ExceptionCompletedIconPresenterPart };
        var successIcon = new IconPresenter { Name = AbstractProgressBar.SuccessCompletedIconPresenterPart };
        var presenter = new ContentPresenter { Name = AbstractProgressBar.ProgressInfoContentPresenterPart };
        scope.Register(labelHost.Name!, labelHost);
        scope.Register(label.Name!, label);
        scope.Register(exceptionIcon.Name!, exceptionIcon);
        scope.Register(successIcon.Name!, successIcon);
        scope.Register(presenter.Name!, presenter);
        var canvas = new Canvas { Name = AbstractProgressBar.ProgressIndicatorPart };
        scope.Register(canvas.Name!, canvas);
        canvas.Children.Add(labelHost);
        canvas.Children.Add(exceptionIcon);
        canvas.Children.Add(successIcon);
        canvas.Children.Add(presenter);
        return canvas;
    }

    private sealed class TestLine : AbstractGeneralProgressBar
    {
        public TestLine()
        {
            Template = new FuncControlTemplate<TestLine>((_, scope) =>
            {
                var panel = new LineProgressPanel { Name = ProgressBodyPart };
                scope.Register(panel.Name!, panel);
                panel.Children.Add(BuildInfoArea(scope));
                return panel;
            });
        }

        internal Size ExtraInfoSize => _extraInfoSize;
        internal Size CalculatedExtraInfoSize => CalculateExtraInfoSize(FontSize);
        internal Control LabelHost => (Control)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == LayoutTransformControlPart);
        internal Label Label => (Label)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == PercentageLabelPart);
        internal Control ExceptionIcon =>
            (Control)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == ExceptionCompletedIconPresenterPart);
        internal Control SuccessIcon =>
            (Control)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == SuccessCompletedIconPresenterPart);
        internal ContentPresenter Presenter =>
            (ContentPresenter)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == ProgressInfoContentPresenterPart);
    }

    private sealed class HiddenInfoLine : AbstractGeneralProgressBar
    {
        static HiddenInfoLine()
        {
            IsProgressInfoVisibleProperty.OverrideDefaultValue<HiddenInfoLine>(false);
        }

        public HiddenInfoLine()
        {
            Template = new FuncControlTemplate<HiddenInfoLine>((_, scope) =>
            {
                var panel = new LineProgressPanel { Name = ProgressBodyPart };
                scope.Register(panel.Name!, panel);
                panel.Children.Add(new Border { Name = ProgressRailPart });
                panel.Children.Add(BuildInfoArea(scope));
                return panel;
            });
        }

        internal Control Rail => (Control)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == ProgressRailPart);
    }

    private sealed class TestCircle : AbstractGeneralCircleProgress
    {
        public TestCircle()
        {
            Template = new FuncControlTemplate<TestCircle>((_, scope) =>
            {
                var panel = new CircleProgressPanel();
                panel.Children.Add(BuildInfoArea(scope));
                return panel;
            });
        }

        internal Label Label => (Label)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == PercentageLabelPart);
        internal Control LabelHost => (Control)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == LayoutTransformControlPart);
        internal Control SuccessIcon =>
            (Control)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == SuccessCompletedIconPresenterPart);
        internal ContentPresenter Presenter =>
            (ContentPresenter)this.GetVisualDescendants().OfType<Control>().Single(c => c.Name == ProgressInfoContentPresenterPart);
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        internal WindowContext(Control content)
        {
            _window = new AvaloniaWindow { Width = 400, Height = 300, Content = content };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void Flush()
    {
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public void Info_content_replaces_text_and_status_icons()
    {
        var line = new TestLine { Value = 100, Status = ProgressStatus.Success };
        using var _ = new WindowContext(line);

        line.LabelHost.IsVisible.ShouldBeTrue();
        line.Presenter.IsVisible.ShouldBeFalse();

        line.ProgressInfoContent = "uploading";
        Flush();

        line.Presenter.IsVisible.ShouldBeTrue();
        line.Presenter.Content.ShouldBe("uploading");
        line.LabelHost.IsVisible.ShouldBeFalse();
        line.ExceptionIcon.IsVisible.ShouldBeFalse();
        line.SuccessIcon.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Turning_info_on_at_runtime_fills_the_text()
    {
        var line = new TestLine { Value = 40, Width = 200, IsProgressInfoVisible = false };
        using var _ = new WindowContext(line);
        line.Label.Content.ShouldBeNull();

        line.IsProgressInfoVisible = true;
        Flush();

        line.IsProgressInfoPresented.ShouldBeTrue();
        line.Label.Content.ShouldBe("40%");
    }

    [AvaloniaFact]
    public void Turning_info_on_at_runtime_rechecks_the_circle_fit()
    {
        var circle = new TestCircle { Width = 100, Height = 100, Value = 40, IsProgressInfoVisible = false };
        using var _ = new WindowContext(circle);

        circle.IsProgressInfoVisible = true;
        Flush();

        circle.IsProgressInfoPresented.ShouldBeTrue();
        circle.Label.Content.ShouldBe("40%");
        circle.IsPercentLabelVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void Leaving_exception_status_refreshes_the_text()
    {
        var line = new TestLine { Value = 40, Width = 200 };
        using var _ = new WindowContext(line);
        line.Status = ProgressStatus.Exception;
        Flush();
        line.Value = 70;
        Flush();
        line.Label.Content.ShouldBe("40%");

        line.Status = ProgressStatus.Normal;
        Flush();

        line.Label.Content.ShouldBe("70%");
    }

    [AvaloniaFact]
    public void Info_content_in_circle_replaces_text_and_status_icons()
    {
        var circle = new TestCircle { Width = 100, Height = 100, Value = 100 };
        using var _ = new WindowContext(circle);

        circle.ProgressInfoContent = new Border { Width = 20, Height = 10 };
        Flush();

        circle.Presenter.IsVisible.ShouldBeTrue();
        circle.LabelHost.IsVisible.ShouldBeFalse();
        circle.SuccessIcon.IsVisible.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Info_content_shows_when_info_hidden()
    {
        var line = new TestLine { IsProgressInfoVisible = false, Width = 200 };
        using var _ = new WindowContext(line);

        line.IsProgressInfoPresented.ShouldBeFalse();

        line.ProgressInfoContent = new Border { Width = 40, Height = 12 };
        Flush();

        line.IsProgressInfoPresented.ShouldBeTrue();
        line.Presenter.IsVisible.ShouldBeTrue();
        line.ExtraInfoSize.ShouldBe(new Size(40, 12));
    }

    [AvaloniaFact]
    public void Clearing_info_content_restores_text()
    {
        var line = new TestLine { Value = 40, Width = 200 };
        using var _ = new WindowContext(line);
        var textSize = line.ExtraInfoSize;

        line.ProgressInfoContent = new Border { Width = 60, Height = 12 };
        Flush();
        line.ExtraInfoSize.ShouldBe(new Size(60, 12));

        line.ClearValue(AbstractProgressBar.ProgressInfoContentProperty);
        Flush();

        line.Presenter.IsVisible.ShouldBeFalse();
        line.LabelHost.IsVisible.ShouldBeTrue();
        line.Label.Content.ShouldBe("40%");
        line.ExtraInfoSize.ShouldBe(textSize);
    }

    [AvaloniaFact]
    public void Extra_info_size_uses_percentage()
    {
        var line = new TestLine { Minimum = 0, Maximum = 200, Value = 100 };
        using var _ = new WindowContext(line);

        var expected = TextUtils.CalculateTextSize("50%", line.FontSize, line.FontFamily);
        line.CalculatedExtraInfoSize.ShouldBe(expected);
        line.ExtraInfoSize.ShouldBe(expected);
    }

    [AvaloniaFact]
    public void Local_font_size_survives_circle_resize()
    {
        var circle = new TestCircle { Width = 60, Height = 60, FontSize = 20 };
        using var _ = new WindowContext(circle);

        circle.FontSize.ShouldBe(20);

        circle.Width = 200;
        circle.Height = 200;
        Flush();

        circle.FontSize.ShouldBe(20);
    }

    [AvaloniaFact]
    public void Auto_font_size_still_applies_without_local_value()
    {
        var circle = new TestCircle { Width = 100, Height = 100 };
        using var _ = new WindowContext(circle);

        circle.FontSize.ShouldBe(100 * 0.15 + 6);
        // 主题样式值让位于自动字号
        circle.Styles.Add(new Style(x => x.OfType<TestCircle>())
        {
            Setters = { new Setter(TemplatedControl.FontSizeProperty, 99d) }
        });
        Flush();
        circle.FontSize.ShouldBe(100 * 0.15 + 6);

        circle.Width = 200;
        circle.Height = 200;
        Flush();

        circle.FontSize.ShouldBe(200 * 0.15 + 6);
    }

    [AvaloniaFact]
    public void Desktop_defaults_unchanged()
    {
        var line = new TestLine { Value = 50, Width = 200 };
        using var _ = new WindowContext(line);

        line.ProgressInfoContent.ShouldBeNull();
        line.ProgressInfoContentTemplate.ShouldBeNull();
        line.IsProgressInfoVisible.ShouldBeTrue();
        line.IsProgressInfoPresented.ShouldBeTrue();
        line.LabelHost.IsVisible.ShouldBeTrue();
        line.Presenter.IsVisible.ShouldBeFalse();
        line.Label.Content.ShouldBe("50%");
        var expected = TextUtils.CalculateTextSize("50%", line.FontSize, line.FontFamily);
        line.ExtraInfoSize.ShouldBe(expected);

        line.IsProgressInfoVisible = false;
        line.IsProgressInfoPresented.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void Default_hidden_info_is_not_presented_and_reserves_no_space()
    {
        var line = new HiddenInfoLine { Width = 300, Height = 20 };
        using var _ = new WindowContext(line);

        line.IsProgressInfoVisible.ShouldBeFalse();
        line.IsProgressInfoPresented.ShouldBeFalse();
        line.Rail.Bounds.Width.ShouldBe(300, 0.5);
    }

    [AvaloniaFact]
    public void Text_format_and_font_size_changes_remeasure_the_info_area()
    {
        var line = new TestLine { Value = 50, Width = 300 };
        using var _ = new WindowContext(line);
        var before = line.ExtraInfoSize;

        line.ProgressTextFormat = "{0:0} percent done";
        Flush();
        var afterFormat = line.ExtraInfoSize;
        afterFormat.Width.ShouldBeGreaterThan(before.Width);
        afterFormat.ShouldBe(TextUtils.CalculateTextSize("50 percent done", line.FontSize, line.FontFamily));
        line.MinWidth.ShouldBe(afterFormat.Width, 0.5);

        line.FontSize = line.FontSize * 2;
        Flush();
        line.ExtraInfoSize.Height.ShouldBeGreaterThan(afterFormat.Height);
        line.ExtraInfoSize.ShouldBe(TextUtils.CalculateTextSize("50 percent done", line.FontSize, line.FontFamily));
    }
}
