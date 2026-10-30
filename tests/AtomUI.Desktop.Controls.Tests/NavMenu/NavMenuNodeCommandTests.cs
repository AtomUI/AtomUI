using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuNodeCommandTests
{
    static NavMenuNodeCommandTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Prepared_Container_Receives_Node_Command_And_Explicit_Parameter()
    {
        var command = new TrackingCommand();
        var parameter = new object();
        var node = new NavMenuNode
        {
            Header           = "Command item",
            ItemKey          = "command-item",
            Command          = command,
            CommandParameter = parameter
        };

        ShowInWindow(node, (_, item) =>
        {
            item.Command.ShouldBeSameAs(command);
            item.CommandParameter.ShouldBeSameAs(parameter);
        });
    }

    [Fact]
    public void Prepared_Container_Preserves_Null_CommandParameter_When_ItemKey_Is_Set()
    {
        var node = new NavMenuNode
        {
            Header  = "Command item",
            ItemKey = "command-item",
            Command = new TrackingCommand()
        };

        ShowInWindow(node, (_, item) => item.CommandParameter.ShouldBeNull());
    }

    [Fact]
    public void Realized_Container_Tracks_Node_Command_And_Parameter_Changes()
    {
        var oldCommand = new TrackingCommand();
        var newCommand = new TrackingCommand();
        var oldParameter = new object();
        var newParameter = new object();
        var node = new NavMenuNode
        {
            Header           = "Command item",
            Command          = oldCommand,
            CommandParameter = oldParameter
        };

        ShowInWindow(node, (_, item) =>
        {
            node.Command = newCommand;
            node.CommandParameter = newParameter;
            Dispatcher.UIThread.RunJobs();

            item.Command.ShouldBeSameAs(newCommand);
            item.CommandParameter.ShouldBeSameAs(newParameter);
        });
    }

    [Fact]
    public void Realized_Container_Tracks_Observable_Custom_Node_Command_Changes()
    {
        var oldCommand = new TrackingCommand();
        var newCommand = new TrackingCommand();
        var oldParameter = new object();
        var newParameter = new object();
        var node = new ObservableNavMenuNode
        {
            Header           = "Command item",
            Command          = oldCommand,
            CommandParameter = oldParameter
        };

        ShowInWindow(node, (_, item) =>
        {
            node.Command = newCommand;
            node.CommandParameter = newParameter;
            Dispatcher.UIThread.RunJobs();

            item.Command.ShouldBeSameAs(newCommand);
            item.CommandParameter.ShouldBeSameAs(newParameter);
        });
    }

    [Fact]
    public void Prepared_Container_Receives_NonObservable_Custom_Node_Command_Values()
    {
        var command = new TrackingCommand();
        var parameter = new object();
        var node = new PlainNavMenuNode
        {
            Header           = "Command item",
            Command          = command,
            CommandParameter = parameter
        };

        ShowInWindow(node, (_, item) =>
        {
            item.Command.ShouldBeSameAs(command);
            item.CommandParameter.ShouldBeSameAs(parameter);
        });
    }

    [Fact]
    public void Legacy_Custom_Node_Uses_Null_Command_Defaults()
    {
        INavMenuNode node = new LegacyNavMenuNode
        {
            Header  = "Legacy item",
            ItemKey = "legacy-item"
        };

        node.Command.ShouldBeNull();
        node.CommandParameter.ShouldBeNull();

        ShowInWindow(node, (_, item) =>
        {
            item.Command.ShouldBeNull();
            item.CommandParameter.ShouldBeNull();
        });
    }

    [Fact]
    public void Pointer_Submission_Executes_Node_Command_Once()
    {
        var command = new TrackingCommand();
        var parameter = new object();
        var node = new NavMenuNode
        {
            Header           = "Command item",
            Command          = command,
            CommandParameter = parameter
        };

        ShowInWindow(node, (window, item) =>
        {
            item.ItemHeader.ShouldNotBeNull();
            var header = item.ItemHeader;
            var point = header.TranslatePoint(
                new Point(header.Bounds.Width / 2, header.Bounds.Height / 2),
                window);
            point.ShouldNotBeNull();

            window.MouseMove(point.Value);
            window.MouseDown(point.Value, MouseButton.Left);
            window.MouseUp(point.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            command.ExecuteCount.ShouldBe(1);
            command.LastParameter.ShouldBeSameAs(parameter);
        });
    }

    [Fact]
    public void Pointer_Submission_With_ReactiveCommand_Coalesces_Transient_CanExecute_Changes()
    {
        NavMenuItem? rootItem = null;
        NavMenuItem? parentItem = null;
        NavMenuItem? siblingItem = null;
        NavMenuItem? leafItem = null;
        bool? siblingEnabledDuringExecution = null;
        bool? leafEnabledDuringExecution = null;
        bool? rootSelectedPathDuringExecution = null;
        bool? parentSelectedPathDuringExecution = null;
        var command = ReactiveCommand.Create<string>(_ =>
        {
            siblingEnabledDuringExecution      = siblingItem!.IsEffectivelyEnabled;
            leafEnabledDuringExecution         = leafItem!.IsEffectivelyEnabled;
            rootSelectedPathDuringExecution    = rootItem!.IsInSelectedPath;
            parentSelectedPathDuringExecution  = parentItem!.IsInSelectedPath;
        }, outputScheduler: AvaloniaScheduler.Instance);
        var sibling = new NavMenuNode
        {
            Header           = "Option 1",
            ItemKey          = "option-1",
            Command          = command,
            CommandParameter = "option-1"
        };
        var leaf = new NavMenuNode
        {
            Header           = "Option 2",
            ItemKey          = "option-2",
            Command          = command,
            CommandParameter = "option-2"
        };
        var parent = new NavMenuNode
        {
            Header  = "Item 1",
            ItemKey = "item-1"
        };
        parent.Children.Add(sibling);
        parent.Children.Add(leaf);
        var root = new NavMenuNode
        {
            Header  = "Navigation Three - Submenu",
            ItemKey = "navigation-three"
        };
        root.Children.Add(parent);
        var menu = CreateMenu(new[] { root });

        ShowInWindow(menu, (window, realizedRootItem) =>
        {
            rootItem = realizedRootItem;
            rootItem.Open();
            Dispatcher.UIThread.RunJobs();

            parentItem = rootItem.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            parentItem.Open();
            Dispatcher.UIThread.RunJobs();

            leafItem = parentItem.ContainerFromItem(leaf).ShouldBeOfType<NavMenuItem>();
            siblingItem = parentItem.ContainerFromItem(sibling).ShouldBeOfType<NavMenuItem>();
            var header = leafItem.ItemHeader.ShouldNotBeNull();
            var point = header.TranslatePoint(
                new Point(header.Bounds.Width / 2, header.Bounds.Height / 2),
                window).ShouldNotBeNull();

            window.MouseMove(point);
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            siblingEnabledDuringExecution.ShouldBe(true);
            leafEnabledDuringExecution.ShouldBe(true);
            rootSelectedPathDuringExecution.ShouldBe(true);
            parentSelectedPathDuringExecution.ShouldBe(true);
            siblingItem.IsEffectivelyEnabled.ShouldBeTrue();
            leafItem.IsEffectivelyEnabled.ShouldBeTrue();
            rootItem.IsInSelectedPath.ShouldBeTrue();
            parentItem.IsInSelectedPath.ShouldBeTrue();
        });
    }

    [Fact]
    public void Keyboard_Submission_Executes_Node_Command_Once()
    {
        var command = new TrackingCommand();
        var parameter = new object();
        var node = new NavMenuNode
        {
            Header           = "Command item",
            Command          = command,
            CommandParameter = parameter
        };

        ShowInWindow(node, (window, _) =>
        {
            var menu = window.Content.ShouldBeOfType<AtomUI.Desktop.Controls.NavMenu>();
            menu.Focus(NavigationMethod.Tab).ShouldBeTrue();
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            command.ExecuteCount.ShouldBe(1);
            command.LastParameter.ShouldBeSameAs(parameter);
        });
    }

    [Fact]
    public void Node_Command_CanExecute_Controls_Realized_Container_Effective_Enabled_State()
    {
        var command = new TrackingCommand
        {
            CanExecuteValue = false
        };
        var node = new NavMenuNode
        {
            Header  = "Command item",
            Command = command
        };

        ShowInWindow(node, (_, item) =>
        {
            item.IsEffectivelyEnabled.ShouldBeFalse();

            command.CanExecuteValue = true;
            command.RaiseCanExecuteChanged();
            Dispatcher.UIThread.RunJobs();

            item.IsEffectivelyEnabled.ShouldBeTrue();
            node.IsEnabled.ShouldBeTrue();
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reopening_Inline_Submenu_Refreshes_Command_State_Through_Groups(bool grouped)
    {
        var command = new TrackingCommand();
        var child = new NavMenuNode { Header = "Child", Command = command };
        var parent = new NavMenuNode { Header = "Parent" };
        var group = new NavMenuGroup { Header = "Group" };
        group.Entries.Add(child);
        if (grouped)
        {
            parent.Entries.Add(group);
        }
        else
        {
            group.Entries.Remove(child);
            parent.Entries.Add(child);
        }

        ShowInWindow(parent, (window, parentItem) =>
        {
            parentItem.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var childOwner = grouped
                ? parentItem.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>()
                : (ItemsControl)parentItem;
            var childItem = childOwner.ContainerFromItem(child).ShouldBeOfType<NavMenuItem>();
            childItem.IsEffectivelyEnabled.ShouldBeTrue();

            foreach (var enabled in new[] { false, true })
            {
                parentItem.IsSubMenuOpen = false;
                Dispatcher.UIThread.RunJobs();
                command.CanExecuteValue = enabled;
                command.RaiseCanExecuteChanged();
                Dispatcher.UIThread.RunJobs();
                parentItem.IsSubMenuOpen = true;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                childItem.IsEffectivelyEnabled.ShouldBe(enabled);
            }
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Commandless_Child_Activation_Does_Not_Execute_Ancestor_Command(bool keyboard)
    {
        var command = new TrackingCommand();
        var child = new NavMenuNode { Header = "Child" };
        var parent = new NavMenuNode { Header = "Parent", Command = command };
        parent.Entries.Add(child);
        ShowInWindow(parent, (window, parentItem) =>
        {
            parentItem.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var childItem = parentItem.ContainerFromItem(child).ShouldBeOfType<NavMenuItem>();
            var header = childItem.ItemHeader.ShouldNotBeNull();
            if (keyboard)
            {
                var menu = window.Content.ShouldBeOfType<AtomUI.Desktop.Controls.NavMenu>();
                menu.Focus(NavigationMethod.Tab).ShouldBeTrue();
                window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
                parentItem.IsKeyboardActive.ShouldBeTrue();
                window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
                childItem.IsKeyboardActive.ShouldBeTrue();
                menu.SelectedItem.ShouldBeNull();
                command.ExecuteCount.ShouldBe(0);
                window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            }
            else
            {
                var point = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), window)
                                  .ShouldNotBeNull();
                window.MouseMove(point);
                window.MouseDown(point, MouseButton.Left);
                window.MouseUp(point, MouseButton.Left);
            }

            Dispatcher.UIThread.RunJobs();
            window.Content.ShouldBeOfType<AtomUI.Desktop.Controls.NavMenu>().SelectedItem.ShouldBeSameAs(child);
            command.ExecuteCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Replacing_Node_Command_Unsubscribes_Old_Command_And_Subscribes_New_Command()
    {
        var oldCommand = new TrackingCommand();
        var newCommand = new TrackingCommand();
        var node = new NavMenuNode
        {
            Header  = "Command item",
            Command = oldCommand
        };

        ShowInWindow(node, (_, item) =>
        {
            oldCommand.SubscriberCount.ShouldBe(1);

            node.Command = newCommand;
            Dispatcher.UIThread.RunJobs();

            item.Command.ShouldBeSameAs(newCommand);
            oldCommand.SubscriberCount.ShouldBe(0);
            newCommand.SubscriberCount.ShouldBe(1);
        });

        newCommand.SubscriberCount.ShouldBe(0);
    }

    [Fact]
    public void Detaching_And_Reattaching_Menu_Does_Not_Duplicate_Command_Subscriptions()
    {
        var command = new TrackingCommand();
        var node = new NavMenuNode
        {
            Header  = "Command item",
            Command = command
        };
        var menu = CreateMenu(new[] { node });
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var item = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            command.SubscriberCount.ShouldBe(1);

            command.CanExecuteValue = false;
            command.RaiseCanExecuteChanged();
            window.Content = null;
            command.CanExecuteValue = true;
            Dispatcher.UIThread.RunJobs();
            command.SubscriberCount.ShouldBe(0);
            item.IsEffectivelyEnabled.ShouldBeTrue(
                "detaching the container must cancel a queued CanExecute update before it can write stale state.");

            window.Content = menu;
            Dispatcher.UIThread.RunJobs();
            command.SubscriberCount.ShouldBe(1);
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        command.SubscriberCount.ShouldBe(0);
    }

    [Fact]
    public void Removing_Node_Clears_Container_Command_And_Releases_Subscriptions()
    {
        var command = new TrackingCommand();
        var parameter = new object();
        var node = new NavMenuNode
        {
            Header           = "Command item",
            Command          = command,
            CommandParameter = parameter
        };
        var nodes = new AvaloniaList<INavMenuNode> { node };
        var menu = CreateMenu(nodes);

        ShowInWindow(menu, (_, item) =>
        {
            command.SubscriberCount.ShouldBe(1);

            nodes.Remove(node);
            RunDispatcherJobsUntil(() => menu.ContainerFromItem(node) is null);

            item.Command.ShouldBeNull();
            item.CommandParameter.ShouldBeNull();
            command.SubscriberCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Clearing_ItemsSource_Releases_All_Node_Command_Subscriptions()
    {
        var firstCommand = new TrackingCommand();
        var secondCommand = new TrackingCommand();
        var firstNode = new NavMenuNode
        {
            Header  = "First item",
            Command = firstCommand
        };
        var secondNode = new NavMenuNode
        {
            Header  = "Second item",
            Command = secondCommand
        };
        var nodes = new AvaloniaList<INavMenuNode> { firstNode, secondNode };
        var menu = CreateMenu(nodes);

        ShowInWindow(menu, (_, firstItem) =>
        {
            var secondItem = menu.ContainerFromItem(secondNode).ShouldBeOfType<NavMenuItem>();
            firstCommand.SubscriberCount.ShouldBe(1);
            secondCommand.SubscriberCount.ShouldBe(1);

            nodes.Clear();
            RunDispatcherJobsUntil(() => menu.ContainerFromItem(firstNode) is null &&
                                               menu.ContainerFromItem(secondNode) is null);

            firstItem.Command.ShouldBeNull();
            secondItem.Command.ShouldBeNull();
            firstCommand.SubscriberCount.ShouldBe(0);
            secondCommand.SubscriberCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Replacing_ItemsSource_Releases_Old_Node_Command_Subscription()
    {
        var oldCommand = new TrackingCommand();
        var newCommand = new TrackingCommand();
        var oldNode = new NavMenuNode
        {
            Header  = "Old item",
            Command = oldCommand
        };
        var newNode = new NavMenuNode
        {
            Header  = "New item",
            Command = newCommand
        };
        var menu = CreateMenu(new[] { oldNode });

        ShowInWindow(menu, (_, oldItem) =>
        {
            oldCommand.SubscriberCount.ShouldBe(1);

            menu.ItemsSource = new[] { newNode };
            RunDispatcherJobsUntil(() => menu.ContainerFromItem(newNode) is NavMenuItem);

            oldItem.Command.ShouldBeNull();
            oldCommand.SubscriberCount.ShouldBe(0);
            newCommand.SubscriberCount.ShouldBe(1);
        });

        newCommand.SubscriberCount.ShouldBe(0);
    }

    [Fact]
    public void Rebinding_The_Same_Container_Releases_Previous_Node_Command_Bindings()
    {
        var oldCommand = new TrackingCommand();
        var newCommand = new TrackingCommand();
        var oldNode = new NavMenuNode
        {
            Header  = "Old item",
            Command = oldCommand
        };
        var newNode = new NavMenuNode
        {
            Header  = "New item",
            Command = newCommand
        };
        var item = new NavMenuItem();

        var oldBindings = item.ResetNodeBindingDisposables();
        NavMenuItemContainerBinder.BindNode(item, oldNode, oldBindings);
        item.Command.ShouldBeSameAs(oldCommand);

        var newBindings = item.ResetNodeBindingDisposables();
        item.Command.ShouldBeNull();
        NavMenuItemContainerBinder.BindNode(item, newNode, newBindings);
        item.Command.ShouldBeSameAs(newCommand);

        item.ClearNodeBindingDisposables();
        item.Command.ShouldBeNull();
    }

    [Fact]
    public void Reapplying_Menu_Template_Does_Not_Duplicate_Command_Subscriptions()
    {
        var command = new TrackingCommand();
        var node = new NavMenuNode
        {
            Header  = "Command item",
            Command = command
        };

        ShowInWindow(node, (window, oldItem) =>
        {
            var menu = window.Content.ShouldBeOfType<AtomUI.Desktop.Controls.NavMenu>();
            var template = menu.Template;
            template.ShouldNotBeNull();
            command.SubscriberCount.ShouldBe(1);

            menu.Template = null;
            menu.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            command.SubscriberCount.ShouldBeLessThan(2);

            menu.Template = template;
            menu.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            command.SubscriberCount.ShouldBe(1);
            var currentItem = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            if (!ReferenceEquals(currentItem, oldItem))
            {
                oldItem.Command.ShouldBeNull();
            }
        });
    }

    [Fact]
    public void Removing_Custom_Node_Releases_PropertyChanged_Subscriptions()
    {
        var node = new ObservableNavMenuNode
        {
            Header  = "Command item",
            Command = new TrackingCommand()
        };
        var nodes = new AvaloniaList<INavMenuNode> { node };
        var menu = CreateMenu(nodes);

        ShowInWindow(menu, (_, item) =>
        {
            var realizedSubscriberCount = node.PropertyChangedSubscriberCount;
            realizedSubscriberCount.ShouldBeGreaterThan(1);

            nodes.Remove(node);
            RunDispatcherJobsUntil(() => menu.ContainerFromItem(node) is null);

            node.PropertyChangedSubscriberCount.ShouldBeLessThan(realizedSubscriberCount);
            item.Command.ShouldBeNull();

            node.Command = new TrackingCommand();
            node.CommandParameter = new object();
            Dispatcher.UIThread.RunJobs();

            item.Command.ShouldBeNull();
            item.CommandParameter.ShouldBeNull();
        });
    }

    [Fact]
    public void Removed_Observable_Custom_Node_Can_Be_Collected()
    {
        var nodeReference = CreateReleasedCustomNodeReference();

        CollectGarbage();

        nodeReference.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Removed_Node_Command_And_Owner_Graph_Can_Be_Collected()
    {
        var references = CreateReleasedCommandGraph();

        CollectGarbage();

        references.Node.IsAlive.ShouldBeFalse();
        references.Command.IsAlive.ShouldBeFalse();
        references.Owner.IsAlive.ShouldBeFalse();
    }

    private static void ShowInWindow(INavMenuNode node, Action<AvaloniaWindow, NavMenuItem> assertion)
    {
        var menu = CreateMenu(new[] { node });
        ShowInWindow(menu, assertion);
    }

    private static AtomUI.Desktop.Controls.NavMenu CreateMenu(IEnumerable<INavMenuNode> nodes)
    {
        return new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false,
            ItemsSource     = nodes
        };
    }

    private static void ShowInWindow(
        AtomUI.Desktop.Controls.NavMenu menu,
        Action<AvaloniaWindow, NavMenuItem> assertion)
    {
        var node = menu.ItemsSource!.Cast<INavMenuNode>().First();

        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var item = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            assertion(window, item);
        }
        finally
        {
            window.Close();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ReleasedCommandGraphReferences CreateReleasedCommandGraph()
    {
        CommandOwner? owner = new CommandOwner();
        NavMenuNode? node = new NavMenuNode
        {
            Header  = "Command item",
            Command = owner.Command
        };
        AvaloniaList<INavMenuNode>? nodes = new AvaloniaList<INavMenuNode> { node };
        AtomUI.Desktop.Controls.NavMenu? menu = CreateMenu(nodes);
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var command = owner.Command;
            command.SubscriberCount.ShouldBe(1);
            nodes.Remove(node);
            RunDispatcherJobsUntil(() => menu.ContainerFromItem(node) is null);
            command.SubscriberCount.ShouldBe(0);

            var references = new ReleasedCommandGraphReferences(
                new WeakReference(node),
                new WeakReference(command),
                new WeakReference(owner));
            menu.ItemsSource = null;
            window.Content = null;
            owner = null;
            node = null;
            nodes = null;
            menu = null;
            return references;
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateReleasedCustomNodeReference()
    {
        ObservableNavMenuNode? node = new ObservableNavMenuNode
        {
            Header  = "Command item",
            Command = new TrackingCommand()
        };
        AvaloniaList<INavMenuNode>? nodes = new AvaloniaList<INavMenuNode> { node };
        AtomUI.Desktop.Controls.NavMenu? menu = CreateMenu(nodes);
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            nodes.Remove(node);
            RunDispatcherJobsUntil(() => menu.ContainerFromItem(node) is null);

            var nodeReference = new WeakReference(node);
            menu.ItemsSource = null;
            window.Content = null;
            node = null;
            nodes = null;
            menu = null;
            return nodeReference;
        }
        finally
        {
            window.Content = null;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void RunDispatcherJobsUntil(Func<bool> condition, int maxPasses = 128)
    {
        for (var i = 0; i < maxPasses; i++)
        {
            if (condition())
            {
                return;
            }

            Dispatcher.UIThread.RunJobs();
        }

        condition().ShouldBeTrue("The expected NavMenu container state was not reached before the dispatcher pass limit.");
    }

    private static void CollectGarbage()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Dispatcher.UIThread.RunJobs();
    }

    private sealed record ReleasedCommandGraphReferences(
        WeakReference Node,
        WeakReference Command,
        WeakReference Owner);

    private sealed class CommandOwner
    {
        public CommandOwner()
        {
            Command = new TrackingCommand(this);
        }

        public TrackingCommand Command { get; }
    }

    private sealed class TrackingCommand : ICommand
    {
        private readonly object? _retainedState;
        private EventHandler? _canExecuteChanged;

        public TrackingCommand(object? retainedState = null)
        {
            _retainedState = retainedState;
        }

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public int SubscriberCount => _canExecuteChanged?.GetInvocationList().Length ?? 0;

        public bool CanExecuteValue { get; set; } = true;

        public event EventHandler? CanExecuteChanged
        {
            add
            {
                _canExecuteChanged += value;
            }
            remove
            {
                _canExecuteChanged -= value;
            }
        }

        public bool CanExecute(object? parameter) => CanExecuteValue;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }

        public void RaiseCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private sealed class ObservableNavMenuNode : INavMenuNode, INotifyPropertyChanged
    {
        private ICommand? _command;
        private object? _commandParameter;
        private PropertyChangedEventHandler? _propertyChanged;

        public object? Header { get; set; }

        public IDataTemplate? HeaderTemplate { get; set; }

        public EntityKey? ItemKey { get; set; }

        public PathIcon? Icon { get; set; }

        public bool IsEnabled { get; set; } = true;

        public ICommand? Command
        {
            get => _command;
            set => SetField(ref _command, value);
        }

        public object? CommandParameter
        {
            get => _commandParameter;
            set => SetField(ref _commandParameter, value);
        }

        public ITreeNode<INavMenuNode>? ParentNode { get; private set; }

        public IEnumerable<INavMenuNode> Children { get; } = [];

        public int PropertyChangedSubscriberCount { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                _propertyChanged += value;
                PropertyChangedSubscriberCount++;
            }
            remove
            {
                _propertyChanged -= value;
                PropertyChangedSubscriberCount--;
            }
        }

        public void UpdateParentNode(INavMenuNode? parentNode)
        {
            ParentNode = parentNode;
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private sealed class PlainNavMenuNode : INavMenuNode
    {
        public object? Header { get; set; }

        public IDataTemplate? HeaderTemplate { get; set; }

        public EntityKey? ItemKey { get; set; }

        public PathIcon? Icon { get; set; }

        public bool IsEnabled { get; set; } = true;

        public ICommand? Command { get; set; }

        public object? CommandParameter { get; set; }

        public ITreeNode<INavMenuNode>? ParentNode { get; private set; }

        public IEnumerable<INavMenuNode> Children { get; } = [];

        public void UpdateParentNode(INavMenuNode? parentNode)
        {
            ParentNode = parentNode;
        }
    }

    private sealed class LegacyNavMenuNode : INavMenuNode
    {
        public object? Header { get; set; }

        public IDataTemplate? HeaderTemplate { get; set; }

        public EntityKey? ItemKey { get; set; }

        public PathIcon? Icon { get; set; }

        public bool IsEnabled { get; set; } = true;

        public ITreeNode<INavMenuNode>? ParentNode { get; private set; }

        public IEnumerable<INavMenuNode> Children { get; } = [];

        public void UpdateParentNode(INavMenuNode? parentNode)
        {
            ParentNode = parentNode;
        }
    }
}
