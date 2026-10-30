using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Controls.Presenters;
using Shouldly;
using Xunit;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NumericUpDown;

public class NumericUpDownSpinnerPrefixLayoutTests
{
    static NumericUpDownSpinnerPrefixLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    /// <summary>
    /// Spinner 模式的 prefix 宿主 presenter 位于 − 手柄与数字之间，并带有
    /// InputPadding 外边距；未设置 InnerLeftContent 时必须整体折叠，
    /// 否则空外边距会把居中的数字推向右侧。
    /// </summary>
    [Fact]
    public void Prefix_Host_Collapses_When_No_Prefix_Is_Set()
    {
        var numericUpDown = CreateSpinnerNumericUpDown(null);

        using var _ = ShowInWindow(numericUpDown);

        var host = FindPrefixHost(numericUpDown);
        host.ShouldNotBeNull();
        host.Bounds.Width.ShouldBe(0d, "未设置 prefix 时宿主 presenter 不应占位，否则居中数字会被推向右侧");
    }

    [Fact]
    public void Prefix_Host_Renders_When_Prefix_Is_Set()
    {
        var numericUpDown = CreateSpinnerNumericUpDown("$");

        using var _ = ShowInWindow(numericUpDown);

        var host = FindPrefixHost(numericUpDown);
        host.ShouldNotBeNull();
        host.IsVisible.ShouldBeTrue();
        host.Bounds.Width.ShouldBeGreaterThan(0d, "设置 prefix 后宿主 presenter 应渲染出前缀");
    }

    private static AtomUINumericUpDown CreateSpinnerNumericUpDown(string? innerLeftContent)
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Mode = NumericUpDownMode.Spinner,
            Minimum = 1,
            Maximum = 100,
            Value = 10m,
            IsMotionEnabled = false
        };
        if (innerLeftContent is not null)
        {
            numericUpDown.InnerLeftContent = innerLeftContent;
        }

        return numericUpDown;
    }

    private static IDisposable ShowInWindow(AtomUINumericUpDown numericUpDown)
    {
        var window = new AvaloniaWindow { Width = 480, Height = 120, Content = numericUpDown };
        window.Show();
        numericUpDown.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        return new WindowDisposable(window);
    }

    /// <summary>
    /// prefix 宿主是 Spinner 模板中包住 PART_InnerLeftContentPresenter 的
    /// ContentPresenter（后者由 NumericUpDown 模板无条件注入）。
    /// </summary>
    private static ContentPresenter? FindPrefixHost(AtomUINumericUpDown numericUpDown)
    {
        return numericUpDown.GetVisualDescendants()
                            .OfType<ContentPresenter>()
                            .SingleOrDefault(p => p.Content is AddOnContentPresenter addOn &&
                                                  addOn.Name == "PART_InnerLeftContentPresenter");
    }

    private sealed class WindowDisposable : IDisposable
    {
        private AvaloniaWindow? _window;

        public WindowDisposable(AvaloniaWindow window)
        {
            _window = window;
        }

        public void Dispose()
        {
            _window?.Close();
            _window = null;
        }
    }
}
