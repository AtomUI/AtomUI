using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Menu;

public class MenuInteractionHandlerTests
{
    static MenuInteractionHandlerTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Menu_Uses_AtomUI_Default_Handler()
    {
        new InspectableMenu().Handler
            .ShouldBeOfType<DefaultMenuInteractionHandler>();
    }

    [Fact]
    public void ContextMenu_Uses_AtomUI_Default_Handler()
    {
        new InspectableContextMenu().Handler
            .ShouldBeOfType<DefaultMenuInteractionHandler>();
    }

    [Fact]
    public void MenuFlyoutPresenter_Uses_AtomUI_Default_Handler()
    {
        new InspectableMenuFlyoutPresenter().Handler
            .ShouldBeOfType<DefaultMenuInteractionHandler>();
    }

    [Fact]
    public void MenuFlyoutPresenter_Preserves_Injected_Handler()
    {
        var handler = new TestCustomMenuInteractionHandler();

        new InspectableMenuFlyoutPresenter(handler).Handler.ShouldBeSameAs(handler);
    }

    [Theory]
    [InlineData(Key.Up, 0)]
    [InlineData(Key.Down, 2)]
    public void Keyboard_Vertical_Navigation_From_Open_Submenu_Does_Not_Open_Leaf_Submenu(
        Key key,
        int expectedSelectedIndex)
    {
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        var presenter = new MenuFlyoutPresenter(handler) { IsMotionEnabled = false };
        var previousLeaf = new MenuItem { Header = "Previous" };
        var openSubmenu = CreateSubMenu("Submenu");
        var nextLeaf = new MenuItem { Header = "Next" };
        presenter.Items.Add(previousLeaf);
        presenter.Items.Add(openSubmenu);
        presenter.Items.Add(nextLeaf);
        using var tree = new DirectMenuTree(presenter);
        presenter.SelectedIndex = 1;
        openSubmenu.IsSubMenuOpen = true;
        presenter.ContainerFromIndex(1).ShouldBeSameAs(openSubmenu);
        openSubmenu.Parent.ShouldBeSameAs(presenter);
        openSubmenu.IsSubMenuOpen.ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();
        var openSubmenuPopup = openSubmenu.GetVisualDescendants()
            .OfType<Popup>()
            .Single();
        openSubmenuPopup.IsOpen.ShouldBeTrue();

        var args = handler.PressKey(openSubmenu, key);

        args.Handled.ShouldBeTrue();
        presenter.SelectedIndex.ShouldBe(expectedSelectedIndex);
        var selectedLeaf = expectedSelectedIndex == 0 ? previousLeaf : nextLeaf;
        selectedLeaf.IsSubMenuOpen.ShouldBeFalse();
        Dispatcher.UIThread.RunJobs();
        openSubmenuPopup.IsOpen.ShouldBeFalse();
        selectedLeaf.GetVisualDescendants()
            .OfType<Popup>()
            .Single()
            .IsOpen.ShouldBeFalse();
    }

    [Fact]
    public void Menu_Close_Cancels_Pending_Open_Before_Early_Return()
    {
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        var menu = new InspectableMenu(handler) { IsMotionEnabled = false };
        var target = CreateSubMenu("Target");
        using var tree = new MenuTree(menu, false, target);

        handler.Enter(target);
        runner.Count.ShouldBe(1);
        handler.HasPendingOpen.ShouldBeTrue();
        menu.SetIsOpenForTest(false);
        menu.Close();
        handler.HasPendingOpen.ShouldBeFalse();
        runner.RunAll();

        target.IsSubMenuOpen.ShouldBeFalse();
    }

    [Fact]
    public void Menu_CloseImmediately_Cancels_Pending_Open_Before_Early_Return()
    {
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        var menu = new InspectableMenu(handler) { IsMotionEnabled = false };
        var target = CreateSubMenu("Target");
        using var tree = new MenuTree(menu, false, target);

        handler.Enter(target);
        runner.Count.ShouldBe(1);
        handler.HasPendingOpen.ShouldBeTrue();
        menu.SetIsOpenForTest(false);
        menu.CloseImmediatelyForTest();
        handler.HasPendingOpen.ShouldBeFalse();
        runner.RunAll();

        target.IsSubMenuOpen.ShouldBeFalse();
    }

    [Fact]
    public void MenuFlyoutPresenter_Close_Cancels_Pending_Open_Before_Popup_Lookup()
    {
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        var presenter = new InspectableMenuFlyoutPresenter(handler);
        var target = CreateSubMenu("Target");
        using var tree = new MenuTree(presenter, false, target);

        handler.Enter(target);
        runner.Count.ShouldBe(1);
        handler.HasPendingOpen.ShouldBeTrue();
        presenter.Close();
        handler.HasPendingOpen.ShouldBeFalse();
        runner.RunAll();

        target.IsSubMenuOpen.ShouldBeFalse();
    }

    [Fact]
    public void ContextMenu_Close_Cancels_Pending_Open_Before_Early_Return()
    {
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        var menu = new InspectableContextMenu(handler);
        var target = CreateSubMenu("Target");
        using var tree = new MenuTree(menu, false, target);

        handler.Enter(target);
        runner.Count.ShouldBe(1);
        handler.HasPendingOpen.ShouldBeTrue();
        menu.Close();
        handler.HasPendingOpen.ShouldBeFalse();
        runner.RunAll();

        target.IsSubMenuOpen.ShouldBeFalse();
    }

    [Fact]
    public void ContextMenu_PopupClosing_Cancels_Pending_Open_Before_Forwarding()
    {
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        var menu = new InspectableContextMenu(handler);
        var target = CreateSubMenu("Target");
        using var tree = new MenuTree(menu, false, target);

        handler.Enter(target);
        runner.Count.ShouldBe(1);
        handler.HasPendingOpen.ShouldBeTrue();
        menu.InvokePopupClosing();
        handler.HasPendingOpen.ShouldBeFalse();
        runner.RunAll();

        target.IsSubMenuOpen.ShouldBeFalse();
    }

    [Fact]
    public void Pending_Open_Does_Not_Run_After_Pointer_Leaves_Target()
    {
        using var tree = CreateNestedSubMenu();
        var target = tree.FirstTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(target);
        handler.Exit(target);

        runner.RunNext();

        target.IsSubMenuOpen.ShouldBeFalse();
        runner.RunAll();
    }

    [Fact]
    public void Pending_Close_Does_Not_Run_After_Pointer_Reenters_Target()
    {
        using var tree = CreateNestedSubMenu();
        var target = tree.FirstTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(target);
        runner.RunNext();
        target.IsSubMenuOpen.ShouldBeTrue();

        handler.Exit(target);
        handler.Enter(target);
        runner.RunNext();

        target.IsSubMenuOpen.ShouldBeTrue();
        runner.RunAll();
    }

    [Fact]
    public void Entering_Submenu_Descendant_Invalidates_Ancestor_Pending_Close()
    {
        var descendant = CreateSubMenu("Descendant");
        var target = new MenuItem
        {
            Header = "Target",
            Items =
            {
                descendant
            }
        };
        using var tree = new MenuTree(target);
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(target);
        runner.RunNext();
        target.IsSubMenuOpen.ShouldBeTrue();
        Dispatcher.UIThread.RunJobs();
        target.ContainerFromIndex(0).ShouldBeSameAs(descendant);

        handler.Exit(target);
        handler.Enter(descendant);
        runner.RunAll();

        target.IsSubMenuOpen.ShouldBeTrue();
    }

    [Fact]
    public void Entering_Sibling_Submenu_Does_Not_Invalidate_Pending_Close()
    {
        using var tree = CreateSiblingSubMenus();
        var firstTarget = tree.FirstTarget;
        var secondTarget = tree.SecondTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(firstTarget);
        runner.RunNext();
        firstTarget.IsSubMenuOpen.ShouldBeTrue();

        handler.Exit(firstTarget);
        handler.Enter(secondTarget);
        runner.RunAll();

        firstTarget.IsSubMenuOpen.ShouldBeFalse();
        secondTarget.IsSubMenuOpen.ShouldBeTrue();
    }

    [Fact]
    public void Keyboard_Right_Invalidates_Pending_Close()
    {
        using var tree = CreateNestedSubMenu();
        var target = tree.FirstTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(target);
        runner.RunNext();
        target.IsSubMenuOpen.ShouldBeTrue();

        handler.Exit(target);
        var args = handler.PressKey(target, Key.Right);
        runner.RunNext();

        args.Handled.ShouldBeTrue();
        target.IsSubMenuOpen.ShouldBeTrue();
        runner.RunAll();
    }

    [Fact]
    public void Unhandled_Key_Does_Not_Invalidate_Pending_Open()
    {
        using var tree = CreateNestedSubMenu();
        var target = tree.FirstTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(target);
        var args = handler.PressKey(target, Key.A);
        runner.RunAll();

        args.Handled.ShouldBeFalse();
        target.IsSubMenuOpen.ShouldBeTrue();
    }

    [Fact]
    public void Unhandled_Directional_Key_Does_Not_Invalidate_Pending_Open()
    {
        using var tree = CreateNestedSubMenu();
        var target = tree.FirstTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(target);
        var args = handler.PressKey(new Avalonia.Controls.TextBlock(), Key.Right);
        runner.RunAll();

        args.Handled.ShouldBeFalse();
        target.IsSubMenuOpen.ShouldBeTrue();
    }

    [Fact]
    public void Pointer_Press_Invalidates_Pending_Close()
    {
        using var tree = CreateNestedSubMenu();
        var target = tree.FirstTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(target);
        runner.RunNext();
        target.IsSubMenuOpen.ShouldBeTrue();

        handler.Exit(target);
        var args = handler.Press(target);
        runner.RunNext();

        args.Handled.ShouldBeTrue();
        target.IsSubMenuOpen.ShouldBeTrue();
        runner.RunAll();
    }

    [Fact]
    public void Only_Current_Submenu_Target_Can_Open()
    {
        using var tree = CreateSiblingSubMenus();
        var firstTarget = tree.FirstTarget;
        var secondTarget = tree.SecondTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(firstTarget);
        handler.Enter(secondTarget);
        runner.RunAll();

        firstTarget.IsSubMenuOpen.ShouldBeFalse();
        secondTarget.IsSubMenuOpen.ShouldBeTrue();
    }

    [Fact]
    public void Reentering_First_Target_Does_Not_Reactivate_Its_Old_Open_Callback()
    {
        using var tree = CreateSiblingSubMenus();
        var firstTarget = tree.FirstTarget;
        var secondTarget = tree.SecondTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);
        handler.AttachTo(tree.Menu);

        handler.Enter(firstTarget);
        handler.Enter(secondTarget);
        handler.Enter(firstTarget);

        runner.RunNext();
        firstTarget.IsSubMenuOpen.ShouldBeFalse();

        runner.RunNext();
        secondTarget.IsSubMenuOpen.ShouldBeFalse();

        runner.RunNext();
        firstTarget.IsSubMenuOpen.ShouldBeTrue();
    }

    [Fact]
    public void Pending_Open_Does_Not_Run_After_Handler_Detach()
    {
        using var tree = CreateNestedSubMenu();
        var target = tree.FirstTarget;
        var runner = new QueuedDelayRunner();
        using var handler = new TestMenuInteractionHandler(runner.Schedule);

        handler.AttachTo(tree.Menu);
        handler.Enter(target);
        handler.HasPendingOpen.ShouldBeTrue();
        handler.DetachFrom(tree.Menu);
        handler.HasPendingOpen.ShouldBeFalse();
        runner.RunAll();

        target.IsSubMenuOpen.ShouldBeFalse();
    }

    [Fact]
    public void Throwing_Delay_Runner_Cancels_Queued_Callback()
    {
        Action? queuedCallback = null;
        var expectedException = new InvalidOperationException("Schedule failed.");
        Action<Action, TimeSpan> runner = (callback, _) =>
        {
            queuedCallback = callback;
            throw expectedException;
        };
        var adapterMethod = typeof(DefaultMenuInteractionHandler).GetMethod(
            "AdaptDelayRun",
            BindingFlags.Static | BindingFlags.NonPublic);
        adapterMethod.ShouldNotBeNull();
        var adapter = adapterMethod.Invoke(null, [runner])
            .ShouldBeOfType<Func<Action, TimeSpan, IDisposable>>();
        var callbackCommittedState = false;

        var actualException = Should.Throw<InvalidOperationException>(
            () => adapter(() => callbackCommittedState = true, TimeSpan.Zero));
        queuedCallback.ShouldNotBeNull();
        queuedCallback();

        actualException.ShouldBeSameAs(expectedException);
        callbackCommittedState.ShouldBeFalse();
    }

    [Fact]
    public void Disposed_Adapted_Delay_Releases_Original_Callback()
    {
        var runner = new QueuedDelayRunner();
        var adapter = GetAdaptedDelayRunner(runner.Schedule);
        var capturedReference = ScheduleAndDisposeCallback(adapter);

        runner.Count.ShouldBe(1);
        ForceFullGc();

        capturedReference.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Synchronous_Delay_Runner_Invokes_Callback_Once()
    {
        var callbackCount = 0;
        var adapter = GetAdaptedDelayRunner((callback, _) =>
        {
            callback();
            callback();
        });

        using (adapter(() => ++callbackCount, TimeSpan.Zero))
        {
        }

        callbackCount.ShouldBe(1);
    }

    private static Func<Action, TimeSpan, IDisposable> GetAdaptedDelayRunner(
        Action<Action, TimeSpan> runner)
    {
        var adapterMethod = typeof(DefaultMenuInteractionHandler).GetMethod(
            "AdaptDelayRun",
            BindingFlags.Static | BindingFlags.NonPublic);
        adapterMethod.ShouldNotBeNull();
        return adapterMethod.Invoke(null, [runner])
            .ShouldBeOfType<Func<Action, TimeSpan, IDisposable>>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference ScheduleAndDisposeCallback(
        Func<Action, TimeSpan, IDisposable> adapter)
    {
        var capturedObject = new object();
        var capturedReference = new WeakReference(capturedObject);
        var callback = new Action(() => GC.KeepAlive(capturedObject));
        using (adapter(callback, TimeSpan.Zero))
        {
        }

        return capturedReference;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ForceFullGc()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
    }

    private static MenuTree CreateNestedSubMenu()
    {
        return new MenuTree(CreateSubMenu("Target"));
    }

    private static MenuTree CreateSiblingSubMenus()
    {
        return new MenuTree(CreateSubMenu("First"), CreateSubMenu("Second"));
    }

    private static MenuItem CreateSubMenu(string header)
    {
        return new MenuItem
        {
            Header = header,
            Items =
            {
                new MenuItem { Header = $"{header} Child" }
            }
        };
    }

    private static void SetInteractionHandler(MenuBase menu, IMenuInteractionHandler handler)
    {
        var interactionHandler = typeof(MenuBase).GetField(
            "<InteractionHandler>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        interactionHandler.ShouldNotBeNull();
        interactionHandler.SetValue(menu, handler);
    }

    private sealed class InspectableMenu : AtomUI.Desktop.Controls.Menu
    {
        public InspectableMenu()
        {
        }

        public InspectableMenu(IMenuInteractionHandler handler)
        {
            SetInteractionHandler(this, handler);
        }

        public IMenuInteractionHandler Handler => InteractionHandler;

        public void SetIsOpenForTest(bool isOpen)
        {
            IsOpen = isOpen;
        }

        public void CloseImmediatelyForTest()
        {
            var closeImmediately = typeof(AtomUI.Desktop.Controls.Menu).GetMethod(
                "CloseImmediately",
                BindingFlags.Instance | BindingFlags.NonPublic);
            closeImmediately.ShouldNotBeNull();
            closeImmediately.Invoke(this, null);
        }
    }

    private sealed class InspectableContextMenu : ContextMenu
    {
        public InspectableContextMenu()
        {
        }

        public InspectableContextMenu(IMenuInteractionHandler handler)
        {
            SetInteractionHandler(this, handler);
        }

        public IMenuInteractionHandler Handler => InteractionHandler;

        public void InvokePopupClosing()
        {
            var handlePopupClosing = typeof(ContextMenu).GetMethod(
                "HandlePopupClosing",
                BindingFlags.Instance | BindingFlags.NonPublic);
            handlePopupClosing.ShouldNotBeNull();
            handlePopupClosing.Invoke(this, [null, new CancelEventArgs()]);
        }
    }

    private sealed class InspectableMenuFlyoutPresenter : MenuFlyoutPresenter
    {
        public InspectableMenuFlyoutPresenter()
        {
        }

        public InspectableMenuFlyoutPresenter(IMenuInteractionHandler handler)
            : base(handler)
        {
        }

        public IMenuInteractionHandler Handler => InteractionHandler;
    }

    private sealed class TestCustomMenuInteractionHandler : IMenuInteractionHandler
    {
        public void Attach(MenuBase menu)
        {
        }

        public void Detach(MenuBase menu)
        {
        }
    }

    private sealed class TestMenuInteractionHandler : DefaultMenuInteractionHandler, IDisposable
    {
        private Avalonia.Controls.MenuBase? _attachedMenu;

        public TestMenuInteractionHandler(Action<Action, TimeSpan> delayRun)
            : base(false, null, delayRun)
        {
        }

        public bool HasPendingOpen
        {
            get
            {
                var pendingOpenTarget = typeof(DefaultMenuInteractionHandler).GetField(
                    "_pendingOpenTarget",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                pendingOpenTarget.ShouldNotBeNull();
                return pendingOpenTarget.GetValue(this) != null;
            }
        }

        public void AttachTo(Avalonia.Controls.MenuBase menu)
        {
            ((IMenuInteractionHandler)this).Attach(menu);
            _attachedMenu = menu;
        }

        public void DetachFrom(Avalonia.Controls.MenuBase menu)
        {
            ((IMenuInteractionHandler)this).Detach(menu);
            _attachedMenu = null;
        }

        public void Enter(MenuItem item)
        {
            PointerEntered(item, new RoutedEventArgs { Source = item });
        }

        public void Exit(MenuItem item)
        {
            PointerExited(item, new RoutedEventArgs { Source = item });
        }

        public KeyEventArgs PressKey(Avalonia.Controls.Control source, Key key)
        {
            var args = new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Source      = source,
                Key         = key
            };
            KeyDown(source, args);
            return args;
        }

        public PointerPressedEventArgs Press(MenuItem item)
        {
            var args = new PointerPressedEventArgs(
                item,
                new Avalonia.Input.Pointer(
                    Avalonia.Input.Pointer.GetNextFreeId(),
                    PointerType.Mouse,
                    true),
                item,
                default,
                0,
                new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
                KeyModifiers.None);
            PointerPressed(item, args);
            return args;
        }

        public void Dispose()
        {
            if (_attachedMenu is { } menu)
            {
                ((IMenuInteractionHandler)this).Detach(menu);
                _attachedMenu = null;
            }
        }
    }

    private sealed class MenuTree : IDisposable
    {
        private readonly Avalonia.Controls.Window _window;

        public MenuTree(params MenuItem[] targets)
            : this(new AtomUI.Desktop.Controls.Menu { IsMotionEnabled = false }, targets)
        {
        }

        public MenuTree(MenuBase menu, params MenuItem[] targets)
            : this(menu, true, targets)
        {
        }

        public MenuTree(MenuBase menu, bool verifyItemContainers, params MenuItem[] targets)
        {
            Targets = targets;
            Parent = new MenuItem { Header = "Parent" };
            foreach (var target in targets)
            {
                Parent.Items.Add(target);
            }

            Menu = menu;
            Menu.Items.Add(Parent);
            var overlayPanel = new ScopeAwareOverlayLayerPanel
            {
                Width = 320,
                Height = 240
            };
            overlayPanel.Children.Add(Menu);
            var visualLayerManager = new VisualLayerManager
            {
                Child = overlayPanel
            };
            EnablePopupOverlayLayer(visualLayerManager);
            _window = new Avalonia.Controls.Window
            {
                Width = 320,
                Height = 240,
                Content = visualLayerManager
            };

            _window.Show();
            Dispatcher.UIThread.RunJobs();
            Menu.ApplyTemplate();
            Parent.ApplyTemplate();
            Parent.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();

            if (!verifyItemContainers)
            {
                return;
            }

            for (var index = 0; index < targets.Length; index++)
            {
                Parent.ContainerFromIndex(index).ShouldBeSameAs(targets[index]);
            }
        }

        public MenuBase Menu { get; }

        public MenuItem Parent { get; }

        public MenuItem[] Targets { get; }

        public MenuItem FirstTarget => Targets[0];

        public MenuItem SecondTarget => Targets[1];

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
        {
            var property = typeof(VisualLayerManager).GetProperty(
                "EnablePopupOverlayLayer",
                BindingFlags.Instance | BindingFlags.NonPublic);

            property.ShouldNotBeNull();
            property.SetValue(visualLayerManager, true);
        }
    }

    private sealed class DirectMenuTree : IDisposable
    {
        private readonly AtomUI.Desktop.Controls.Window _window;

        public DirectMenuTree(MenuBase menu)
        {
            var visualLayerManager = new VisualLayerManager { Child = menu };
            var enablePopupOverlayLayer = typeof(VisualLayerManager).GetProperty(
                "EnablePopupOverlayLayer",
                BindingFlags.Instance | BindingFlags.NonPublic);
            enablePopupOverlayLayer.ShouldNotBeNull();
            enablePopupOverlayLayer.SetValue(visualLayerManager, true);
            _window = new AtomUI.Desktop.Controls.Window
            {
                Width = 320,
                Height = 240,
                Content = visualLayerManager
            };
            _window.Show();
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
                _window.UpdateLayout();
            }
        }

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class QueuedDelayRunner
    {
        private readonly Queue<Action> _callbacks = new();

        public int Count => _callbacks.Count;

        public void Schedule(Action callback, TimeSpan delay)
        {
            _callbacks.Enqueue(callback);
        }

        public void RunNext()
        {
            _callbacks.Dequeue().Invoke();
        }

        public void RunAll()
        {
            while (_callbacks.Count > 0)
            {
                RunNext();
            }
        }
    }
}
