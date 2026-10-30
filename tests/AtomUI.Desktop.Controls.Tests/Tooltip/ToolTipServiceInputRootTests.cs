using System.Reflection;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

public class ToolTipServiceInputRootTests
{
    static ToolTipServiceInputRootTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pointer_Release_Can_Close_Window_Before_ToolTip_Service_Processes_Input()
    {
        var owner = new AvaloniaWindow { Width = 300, Height = 200 };
        var content = new Border { Background = Brushes.Red };
        var window = new AvaloniaWindow { Content = content, Width = 200, Height = 100 };
        owner.Show();
        window.Show(owner);
        Dispatcher.UIThread.RunJobs();
        var source = content.GetPresentationSource().ShouldNotBeNull();
        content.PointerReleased += (_, _) => window.Close();

        try
        {
            window.MouseMove(new Point(50, 50));
            window.MouseDown(new Point(50, 50), MouseButton.Left);
            Should.NotThrow(() => window.MouseUp(new Point(50, 50), MouseButton.Left));

            source.RootVisual.ShouldBeNull();
            window.IsVisible.ShouldBeFalse();
            owner.IsVisible.ShouldBeTrue();
            Should.NotThrow(() => owner.MouseMove(new Point(50, 50)));
        }
        finally
        {
            window.Close();
            owner.Close();
        }
    }

    [Fact]
    public void Overlay_ToolTip_Allows_Switching_Hosts_In_The_Owner_Window()
    {
        var first = new Border();
        var second = new Border();
        var window = new AtomUI.Desktop.Controls.Window
        {
            Content = new StackPanel { Children = { first, second } },
            Width = 300,
            Height = 200
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        using var inputManager = new TestInputManager();
        using var service = new ToolTipService(inputManager);
        try
        {
            ToolTip.SetTip(first, "first");
            ToolTip.SetTip(second, "second");
            ToolTip.SetIsUseOverlayHost(first, true);
            ToolTip.SetIsUseOverlayHost(second, true);
            ToolTip.SetShowDelay(first, 0);
            ToolTip.SetShowDelay(second, 0);
            var root = GetInputRoot(first);
            service.Update(root, first);
            Dispatcher.UIThread.RunJobs();

            var tip = first.GetValue(ToolTip.ToolTipProperty).ShouldNotBeNull();
            GetInputRoot(tip).ShouldBeSameAs(root);
            service.Update(root, tip);
            ToolTip.GetIsOpen(first).ShouldBeTrue("pointer movement within the overlay must keep it open");

            service.Update(root, second);
            ToolTip.GetIsOpen(first).ShouldBeFalse();
            ToolTip.GetIsOpen(second).ShouldBeTrue();
        }
        finally
        {
            ToolTip.SetIsOpen(first, false);
            ToolTip.SetIsOpen(second, false);
            window.Close();
        }
    }

    [Fact]
    public void Service_Does_Not_Retain_Detached_Host_With_Pending_Show_Timer()
    {
        using var inputManager = new TestInputManager();
        using var service = new ToolTipService(inputManager);
        var host = TrackAndDetachHost(service);
        Dispatcher.UIThread.RunJobs();

        for (var attempt = 0; attempt < 3; ++attempt)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        host.IsAlive.ShouldBeFalse("the global tooltip service must release a detached pending host");
        GC.KeepAlive(service);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference TrackAndDetachHost(ToolTipService service)
    {
        var host = new Border();
        var window = new AvaloniaWindow { Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            ToolTip.SetTip(host, "pending");
            ToolTip.SetShowDelay(host, 60000);
            service.Update(GetInputRoot(host), host);
            var root = GetInputRoot(host);
            window.Content = null;
            // A hit computed before detach must not restart tracking or the show timer.
            service.Update(root, host);
            return new WeakReference(host);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Native_ToolTip_Root_Transitions_And_Detach_Preserve_Open_State()
    {
        var host = new Border();
        var window = new AvaloniaWindow { Content = host };
        window.Show();
        SetHeadlessOverlayPopups(window, false);
        Dispatcher.UIThread.RunJobs();

        using var inputManager = new TestInputManager();
        using var mouseDevice = new MouseDevice();
        using var service = new ToolTipService(inputManager);
        try
        {
            ToolTip.SetTip(host, "tip");
            ToolTip.SetShowDelay(host, 0);
            service.Update(GetInputRoot(host), host);
            Dispatcher.UIThread.RunJobs();
            ToolTip.GetIsOpen(host).ShouldBeTrue();

            var tip = host.GetValue(ToolTip.ToolTipProperty).ShouldNotBeNull();
            var windowRoot = GetInputRoot(host);
            var tipRoot = GetInputRoot(tip);
            tipRoot.ShouldNotBeSameAs(windowRoot);

            const ulong timestamp = 42;
            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, windowRoot, RawPointerEventType.Move, timestamp, host));
            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, tipRoot, RawPointerEventType.Move, timestamp, tip));
            ToolTip.GetIsOpen(host).ShouldBeTrue(
                "moving from the window root into the native tooltip root must keep the tooltip open");

            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, windowRoot, RawPointerEventType.LeaveWindow, timestamp, null));
            ToolTip.GetIsOpen(host).ShouldBeTrue(
                "paired root events with the same platform timestamp describe one transition");

            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, windowRoot, RawPointerEventType.LeaveWindow, timestamp + 1, null));
            ToolTip.GetIsOpen(host).ShouldBeFalse();

            service.Update(windowRoot, host);
            Dispatcher.UIThread.RunJobs();
            var tipWindow = TopLevel.GetTopLevel(tip).ShouldNotBeNull();
            var point = tip.TranslatePoint(
                new Point(tip.Bounds.Width / 2, tip.Bounds.Height / 2), tipWindow).ShouldNotBeNull();
            tipWindow.MouseMove(point);
            tip.IsPointerOver.ShouldBeTrue();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            ToolTip.GetIsOpen(host).ShouldBeTrue("pointer exits during popup teardown must preserve desired IsOpen");

            window.Content = host;
            Dispatcher.UIThread.RunJobs();
            ToolTip.GetIsOpen(host).ShouldBeTrue();
            tip.Classes.Contains(ToolTipPseudoClass.Open).ShouldBeTrue();
        }
        finally
        {
            ToolTip.SetIsOpen(host, false);
            SetHeadlessOverlayPopups(window, true);
            window.Close();
        }
    }

    private static IInputRoot GetInputRoot(Visual visual) =>
        visual.GetPresentationSource().ShouldNotBeNull().ShouldBeAssignableTo<IInputRoot>();

    private static RawPointerEventArgs CreatePointerEvent(
        IInputDevice device,
        IInputRoot root,
        RawPointerEventType type,
        ulong timestamp,
        IInputElement? hit)
    {
        var args = new RawPointerEventArgs(device, timestamp, root, type, default(Point), default);
        typeof(RawPointerEventArgs).GetProperty(
                "InputHitTestResult",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(args, (hit, hit));
        return args;
    }

    private static void SetHeadlessOverlayPopups(AvaloniaWindow window, bool value)
    {
        var implementation = window.PlatformImpl.ShouldNotBeNull();
        var options = implementation.GetType().GetField(
                "_options",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(implementation)
            .ShouldNotBeNull();
        options.GetType().GetProperty("OverlayPopups")!.SetValue(options, value);
    }

    private sealed class TestInputManager : IInputManager, IDisposable
    {
        private readonly Subject<RawInputEventArgs> _preProcess = new();
        private readonly Subject<RawInputEventArgs> _process = new();
        private readonly Subject<RawInputEventArgs> _postProcess = new();

        public IObservable<RawInputEventArgs> PreProcess => _preProcess;
        public IObservable<RawInputEventArgs> Process => _process;
        public IObservable<RawInputEventArgs> PostProcess => _postProcess;

        public void ProcessInput(RawInputEventArgs e)
        {
            _preProcess.OnNext(e);
            _process.OnNext(e);
            _postProcess.OnNext(e);
        }

        public void Dispose()
        {
            _preProcess.Dispose();
            _process.Dispose();
            _postProcess.Dispose();
        }
    }

}
