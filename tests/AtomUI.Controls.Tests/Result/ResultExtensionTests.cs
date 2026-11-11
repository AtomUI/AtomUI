using AtomUI.Controls.Commons;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Controls.Tests.Result;

public class ResultExtensionTests
{
    static ResultExtensionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // Minimal templated subclass that exposes the resolved status icon.
    private class TestResult : AbstractResult
    {
        public TestResult()
        {
            Template = new FuncControlTemplate<TestResult>((_, _) => new Panel());
        }

        internal object? CurrentStatusIcon => StatusIcon;
    }

    // Overrides the default icon factory the way a Mobile subclass will.
    private sealed class FactoryResult : TestResult
    {
        internal static readonly object SuccessMarker = new();
        internal int FactoryCalls;

        private protected override object? CreateDefaultStatusIcon(ResultStatus status)
        {
            FactoryCalls++;
            return status == ResultStatus.Success ? SuccessMarker : base.CreateDefaultStatusIcon(status);
        }
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

    [Fact]
    public void Waiting_appended_without_renumbering()
    {
        ((int)ResultStatus.Info).ShouldBe(0);
        ((int)ResultStatus.Success).ShouldBe(1);
        ((int)ResultStatus.Error).ShouldBe(2);
        ((int)ResultStatus.Warning).ShouldBe(3);
        ((int)ResultStatus.ErrorCode404).ShouldBe(4);
        ((int)ResultStatus.ErrorCode403).ShouldBe(5);
        ((int)ResultStatus.ErrorCode500).ShouldBe(6);
        ((int)ResultStatus.Waiting).ShouldBe(7);
        Enum.GetValues<ResultStatus>().Length.ShouldBe(8);
    }

    [Fact]
    public void Default_icon_factory_is_overridable()
    {
        var result = new FactoryResult { Status = ResultStatus.Success };
        using var context = new WindowContext(result);

        result.CurrentStatusIcon.ShouldBeSameAs(FactoryResult.SuccessMarker);
        result.FactoryCalls.ShouldBeGreaterThan(0);

        // Statuses the override defers to the base for keep the built-in icons.
        result.Status = ResultStatus.Error;
        Dispatcher.UIThread.RunJobs();
        result.CurrentStatusIcon.ShouldBeOfType<CloseCircleFilled>();

        // A new instance per call: icons are controls and cannot be shared between parents.
        result.Status = ResultStatus.Success;
        result.Status = ResultStatus.Error;
        var first = result.CurrentStatusIcon;
        result.Status = ResultStatus.Success;
        result.Status = ResultStatus.Error;
        result.CurrentStatusIcon.ShouldNotBeSameAs(first);
    }

    [Fact]
    public void Object_icon_replaces_default_and_clearing_restores()
    {
        var result = new TestResult { Status = ResultStatus.Success };
        using var context = new WindowContext(result);
        result.CurrentStatusIcon.ShouldBeOfType<CheckCircleFilled>();

        result.Icon = "emoji";
        Dispatcher.UIThread.RunJobs();
        result.CurrentStatusIcon.ShouldBe("emoji");

        var custom = new Border();
        result.Icon = custom;
        Dispatcher.UIThread.RunJobs();
        result.CurrentStatusIcon.ShouldBeSameAs(custom);

        // The explicit icon survives a status change.
        result.Status = ResultStatus.Warning;
        Dispatcher.UIThread.RunJobs();
        result.CurrentStatusIcon.ShouldBeSameAs(custom);

        result.Icon = null;
        Dispatcher.UIThread.RunJobs();
        result.CurrentStatusIcon.ShouldBeOfType<WarningFilled>();

        AbstractResult.IconProperty.PropertyType.ShouldBe(typeof(object));
        typeof(AbstractResult).GetProperty(nameof(AbstractResult.Icon))!.PropertyType.ShouldBe(typeof(object));
    }

    [Fact]
    public void Desktop_icons_unchanged()
    {
        var expected = new (ResultStatus Status, Type Icon)[]
        {
            (ResultStatus.Info, typeof(ExclamationCircleFilled)),
            (ResultStatus.Success, typeof(CheckCircleFilled)),
            (ResultStatus.Warning, typeof(WarningFilled)),
            (ResultStatus.Error, typeof(CloseCircleFilled)),
            (ResultStatus.Waiting, typeof(ClockCircleFilled))
        };

        foreach (var (status, icon) in expected)
        {
            var result = new TestResult { Status = status };
            using var context = new WindowContext(result);
            result.CurrentStatusIcon.ShouldBeOfType(icon, status.ToString());
        }

        // Error-code statuses do not use the icon slot.
        foreach (var status in new[] { ResultStatus.ErrorCode403, ResultStatus.ErrorCode404, ResultStatus.ErrorCode500 })
        {
            var result = new TestResult { Status = status };
            using var context = new WindowContext(result);
            result.CurrentStatusIcon.ShouldBeNull(status.ToString());
        }
    }
}
