using System.ComponentModel;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.FloatButton;

public class FloatButtonHostOverlayTests
{
    static FloatButtonHostOverlayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void FloatButtonHost_Removes_Overlay_Button_When_Detached()
    {
        var host = new AtomUI.Desktop.Controls.FloatButtonHost();
        var window = CreateWindow(host, out var overlayPanel);

        try
        {
            var overlayLayer = ScopeAwareOverlayLayer.FindLayer(overlayPanel);

            overlayLayer.ShouldNotBeNull();
            overlayLayer.Children.Count.ShouldBe(1);

            overlayPanel.Children.Remove(host);
            Dispatcher.UIThread.RunJobs();

            overlayLayer.Children.Count.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonGroupHost_Removes_Overlay_Group_When_Detached()
    {
        var host = new FloatButtonGroupHost();
        var window = CreateWindow(host, out var overlayPanel);

        try
        {
            var overlayLayer = ScopeAwareOverlayLayer.FindLayer(overlayPanel);

            overlayLayer.ShouldNotBeNull();
            overlayLayer.Children.Count.ShouldBe(1);

            overlayPanel.Children.Remove(host);
            Dispatcher.UIThread.RunJobs();

            overlayLayer.Children.Count.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonGroupHost_Keeps_Initially_Closed_Menu_Hidden()
    {
        var host = new FloatButtonGroupHost
        {
            Trigger = FloatButtonGroupTrigger.Click
        };
        host.Children.Add(new AtomUI.Desktop.Controls.FloatButton());

        var window = CreateWindow(host, out var overlayPanel);

        try
        {
            var overlayLayer = ScopeAwareOverlayLayer.FindLayer(overlayPanel);
            overlayLayer.ShouldNotBeNull();

            var group = overlayLayer.GetVisualDescendants()
                                    .OfType<FloatButtonGroup>()
                                    .Single();
            var motionActor = group.GetVisualDescendants()
                                   .OfType<Border>()
                                   .Single(control => control.Name == "PART_MenuSurface");

            group.IsOpen.ShouldBeFalse();
            motionActor.IsVisible.ShouldBeFalse();

            Thread.Sleep(500);
            Dispatcher.UIThread.RunJobs();

            group.IsOpen.ShouldBeFalse();
            motionActor.IsVisible.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonGroupHost_Click_Trigger_Reopens_After_Shared_Bound_State_Closes()
    {
        var viewModel = new OpenStateViewModel
        {
            IsOpen = true
        };
        var hoverHost = CreateBoundGroupHost(viewModel, FloatButtonGroupTrigger.Hover, FloatButtonShape.Square);
        hoverHost.FloatOffsetX = 80;
        var clickHost = CreateBoundGroupHost(viewModel, FloatButtonGroupTrigger.Click, FloatButtonShape.Circle);
        var panel = new Panel();
        panel.Children.Add(hoverHost);
        panel.Children.Add(clickHost);

        var window = CreateWindow(panel, out var overlayPanel);

        try
        {
            var overlayLayer = ScopeAwareOverlayLayer.FindLayer(overlayPanel);
            overlayLayer.ShouldNotBeNull();

            var clickGroup = overlayLayer.GetVisualDescendants()
                                         .OfType<FloatButtonGroup>()
                                         .Single(group => group.Trigger == FloatButtonGroupTrigger.Click);

            viewModel.IsOpen = false;
            Dispatcher.UIThread.RunJobs();

            clickGroup.IsOpen.ShouldBeFalse();

            var triggerButton = clickGroup.GetVisualDescendants()
                                          .OfType<AtomUI.Desktop.Controls.FloatButton>()
                                          .Single(button => button.GetVisualParent() is Canvas);

            Click(triggerButton, window);
            Dispatcher.UIThread.RunJobs();

            viewModel.IsOpen.ShouldBeTrue();
            clickHost.IsOpen.ShouldBeTrue();
            clickGroup.IsOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonGroupHost_Does_Not_Cancel_External_Switch_Open_When_Closed()
    {
        var viewModel = new OpenStateViewModel
        {
            IsOpen = false
        };
        var toggleSwitch = new AtomUI.Desktop.Controls.ToggleSwitch();
        toggleSwitch.Bind(
            Avalonia.Controls.Primitives.ToggleButton.IsCheckedProperty,
            new Binding(nameof(OpenStateViewModel.IsOpen))
            {
                Source = viewModel
            });
        var hoverHost = CreateBoundGroupHost(viewModel, FloatButtonGroupTrigger.Hover, FloatButtonShape.Square);
        hoverHost.FloatOffsetX = 80;
        var clickHost = CreateBoundGroupHost(viewModel, FloatButtonGroupTrigger.Click, FloatButtonShape.Circle);
        var panel = new Panel();
        panel.Children.Add(toggleSwitch);
        panel.Children.Add(hoverHost);
        panel.Children.Add(clickHost);

        var window = CreateWindow(panel, out _);

        try
        {
            Dispatcher.UIThread.RunJobs();

            clickHost.IsOpen.ShouldBeFalse();

            Click(toggleSwitch, window);
            Dispatcher.UIThread.RunJobs();

            viewModel.IsOpen.ShouldBeTrue();
            toggleSwitch.IsChecked.ShouldBe(true);
            hoverHost.IsOpen.ShouldBeTrue();
            clickHost.IsOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonHost_Forwards_Command_To_Overlay_Button()
    {
        var parameter = new object();
        var command   = new RecordingCommand();
        var host = new AtomUI.Desktop.Controls.FloatButtonHost
        {
            Command          = command,
            CommandParameter = parameter
        };
        var window = CreateWindow(host, out var overlayPanel);

        try
        {
            var overlayButton = GetOverlayFloatButton<AtomUI.Desktop.Controls.FloatButton>(overlayPanel);

            Click(overlayButton, window);
            Dispatcher.UIThread.RunJobs();

            command.ExecuteCount.ShouldBe(1);
            command.LastParameter.ShouldBeSameAs(parameter);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonHost_Disables_Overlay_Button_When_Command_Cannot_Execute()
    {
        var command = new RecordingCommand
        {
            CanExecuteValue = false
        };
        var host = new AtomUI.Desktop.Controls.FloatButtonHost
        {
            Command = command
        };
        var window = CreateWindow(host, out var overlayPanel);

        try
        {
            var overlayButton = GetOverlayFloatButton<AtomUI.Desktop.Controls.FloatButton>(overlayPanel);

            overlayButton.IsEffectivelyEnabled.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void BackTopFloatButtonHost_Forwards_Command_To_Overlay_Button()
    {
        var parameter = new object();
        var command   = new RecordingCommand();
        var host = new BackTopFloatButtonHost
        {
            Command          = command,
            CommandParameter = parameter
        };
        var window = CreateWindow(host, out var overlayPanel);

        try
        {
            var overlayButton = GetOverlayFloatButton<BackTopFloatButton>(overlayPanel);

            Click(overlayButton, window);
            Dispatcher.UIThread.RunJobs();

            command.ExecuteCount.ShouldBe(1);
            command.LastParameter.ShouldBeSameAs(parameter);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonHost_In_LiteScrollViewer_Renders_Overlay_Above_ScrollBars()
    {
        var host = new AtomUI.Desktop.Controls.FloatButtonHost
        {
            IsBadgeEnabled = true,
            BadgeCount     = 99
        };
        var content = new Panel
        {
            Height = 500
        };
        content.Children.Add(host);
        var scrollViewer = new AtomUI.Desktop.Controls.ScrollViewer
        {
            Width                       = 320,
            Height                      = 240,
            IsLiteMode                  = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            Content                     = content
        };
        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = scrollViewer
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var overlayLayer = scrollViewer.GetVisualDescendants()
                                           .OfType<ScopeAwareOverlayLayer>()
                                           .Single(layer => layer.Children.OfType<AtomUI.Desktop.Controls.FloatButton>().Any());
            var verticalScrollBar = scrollViewer.GetVisualDescendants()
                                                .OfType<AtomUI.Desktop.Controls.ScrollBar>()
                                                .Single(scrollBar => scrollBar.Name == "PART_VerticalScrollBar");
            var scrollLayout = verticalScrollBar.GetVisualParent() as Avalonia.Controls.Grid;
            scrollLayout.ShouldNotBeNull();

            overlayLayer.GetVisualParent().ShouldBeSameAs(scrollLayout.GetVisualParent());
            overlayLayer.ZIndex.ShouldBeGreaterThan(scrollLayout.ZIndex);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonGroupHost_Child_CommandBinding_Uses_Host_DataContext()
    {
        var viewModel = new CommandBindingViewModel();
        var child     = CreateBoundChildButton();
        var host = new FloatButtonGroupHost
        {
            DataContext = viewModel
        };
        host.Children.Add(child);

        var window = CreateWindow(host, out _);

        try
        {
            Dispatcher.UIThread.RunJobs();

            child.Command.ShouldBeSameAs(viewModel.ChildCommand);
            child.CommandParameter.ShouldBeSameAs(viewModel.Parameter);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonGroupHost_Does_Not_Overwrite_Child_Local_DataContext()
    {
        var hostViewModel  = new CommandBindingViewModel();
        var childViewModel = new CommandBindingViewModel();
        var child = CreateBoundChildButton();
        child.DataContext = childViewModel;
        var host = new FloatButtonGroupHost
        {
            DataContext = hostViewModel
        };
        host.Children.Add(child);

        var window = CreateWindow(host, out _);

        try
        {
            Dispatcher.UIThread.RunJobs();

            child.Command.ShouldBeSameAs(childViewModel.ChildCommand);
            child.CommandParameter.ShouldBeSameAs(childViewModel.Parameter);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void FloatButtonGroupHost_Child_Added_After_Attach_Uses_Host_DataContext()
    {
        var viewModel = new CommandBindingViewModel();
        var host = new FloatButtonGroupHost
        {
            DataContext = viewModel
        };
        var window = CreateWindow(host, out _);

        try
        {
            var child = CreateBoundChildButton();

            host.Children.Add(child);
            Dispatcher.UIThread.RunJobs();

            child.Command.ShouldBeSameAs(viewModel.ChildCommand);
            child.CommandParameter.ShouldBeSameAs(viewModel.Parameter);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Controlled_Direct_Group_Keeps_Menu_Open_On_Trigger_Click()
    {
        var group = new FloatButtonGroup
        {
            Trigger         = FloatButtonGroupTrigger.Click,
            IsOpen          = true,
            IsMotionEnabled = false,
            Children =
            {
                new AtomUI.Desktop.Controls.FloatButton(),
                new AtomUI.Desktop.Controls.FloatButton()
            }
        };

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = group
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var trigger = group.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.FloatButton>()
                               .Single(button => button.Classes.Contains("semantic-trigger"));

            Click(trigger, window);
            Dispatcher.UIThread.RunJobs();

            group.IsOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void FloatButtonHost_Preserves_Position_After_Detach_And_Reattach()
    {
        var host = new AtomUI.Desktop.Controls.FloatButtonHost();
        var content = new Panel
        {
            Height = 500
        };
        content.Children.Add(host);
        var scrollViewer = new AtomUI.Desktop.Controls.ScrollViewer
        {
            Width  = 320,
            Height = 240,
            Content = content
        };
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 320,
            Height = 240
        };
        overlayPanel.Children.Add(scrollViewer);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = overlayPanel
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var overlayLayer = scrollViewer.GetVisualDescendants()
                                           .OfType<ScopeAwareOverlayLayer>()
                                           .Single(layer => layer.Children.OfType<AtomUI.Desktop.Controls.FloatButton>().Any());
            var button = overlayLayer.Children.OfType<AtomUI.Desktop.Controls.FloatButton>().Single();
            var beforeX = Canvas.GetLeft(button);
            var beforeY = Canvas.GetTop(button);

            overlayPanel.Children.Remove(scrollViewer);
            Dispatcher.UIThread.RunJobs();

            overlayPanel.Children.Add(scrollViewer);
            Dispatcher.UIThread.RunJobs();

            var reattachedLayer = scrollViewer.GetVisualDescendants()
                                              .OfType<ScopeAwareOverlayLayer>()
                                              .Single(layer => layer.Children.OfType<AtomUI.Desktop.Controls.FloatButton>().Any());
            var reattachedButton = reattachedLayer.Children.OfType<AtomUI.Desktop.Controls.FloatButton>().Single();

            Canvas.GetLeft(reattachedButton).ShouldBe(beforeX);
            Canvas.GetTop(reattachedButton).ShouldBe(beforeY);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Uncontrolled_Direct_Group_Toggles_Open_On_Trigger_Click()
    {
        var group = new FloatButtonGroup
        {
            Trigger         = FloatButtonGroupTrigger.Click,
            IsMotionEnabled = false,
            Children =
            {
                new AtomUI.Desktop.Controls.FloatButton(),
                new AtomUI.Desktop.Controls.FloatButton()
            }
        };

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = group
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            group.IsOpen.ShouldBeFalse();

            var trigger = group.GetVisualDescendants()
                               .OfType<AtomUI.Desktop.Controls.FloatButton>()
                               .Single(button => button.Classes.Contains("semantic-trigger"));

            Click(trigger, window);
            Dispatcher.UIThread.RunJobs();
            group.IsOpen.ShouldBeTrue();

            Click(trigger, window);
            Dispatcher.UIThread.RunJobs();
            group.IsOpen.ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Avalonia.Controls.Window CreateWindow(Control host, out ScopeAwareOverlayLayerPanel overlayPanel)
    {
        overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 320,
            Height = 240
        };
        overlayPanel.Children.Add(host);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = overlayPanel
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static TFloatButton GetOverlayFloatButton<TFloatButton>(ScopeAwareOverlayLayerPanel overlayPanel)
        where TFloatButton : AtomUI.Controls.Commons.AbstractFloatButton
    {
        var overlayLayer = ScopeAwareOverlayLayer.FindLayer(overlayPanel);
        overlayLayer.ShouldNotBeNull();
        return overlayLayer.GetVisualDescendants()
                           .OfType<TFloatButton>()
                           .Single();
    }

    private static AtomUI.Desktop.Controls.FloatButton CreateBoundChildButton()
    {
        var child = new AtomUI.Desktop.Controls.FloatButton();
        child.Bind(
            Avalonia.Controls.Button.CommandProperty,
            new Binding(nameof(CommandBindingViewModel.ChildCommand)));
        child.Bind(
            Avalonia.Controls.Button.CommandParameterProperty,
            new Binding(nameof(CommandBindingViewModel.Parameter)));
        return child;
    }

    private static FloatButtonGroupHost CreateBoundGroupHost(
        OpenStateViewModel viewModel,
        FloatButtonGroupTrigger trigger,
        FloatButtonShape shape)
    {
        var host = new FloatButtonGroupHost
        {
            Trigger         = trigger,
            Shape           = shape,
            IsMotionEnabled = false
        };
        host.Bind(
            FloatButtonGroupHost.IsOpenProperty,
            new Binding(nameof(OpenStateViewModel.IsOpen))
            {
                Source = viewModel
            });
        host.Children.Add(new AtomUI.Desktop.Controls.FloatButton());
        host.Children.Add(new AtomUI.Desktop.Controls.FloatButton());
        return host;
    }

    private static void Click(Control control, Avalonia.Controls.Window window)
    {
        var point = control.TranslatePoint(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), window);
        point.ShouldNotBeNull();
        window.MouseMove(point.Value);
        window.MouseDown(point.Value, MouseButton.Left);
        window.MouseUp(point.Value, MouseButton.Left);
    }

    private sealed class OpenStateViewModel : INotifyPropertyChanged
    {
        private bool _isOpen;

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value)
                {
                    return;
                }

                _isOpen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOpen)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class CommandBindingViewModel
    {
        public RecordingCommand ChildCommand { get; } = new();

        public object Parameter { get; } = new();
    }

    private sealed class RecordingCommand : ICommand
    {
        public bool CanExecuteValue { get; set; } = true;

        public int ExecuteCount { get; private set; }

        public object? LastParameter { get; private set; }

        public bool CanExecute(object? parameter)
        {
            return CanExecuteValue;
        }

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            LastParameter = parameter;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
