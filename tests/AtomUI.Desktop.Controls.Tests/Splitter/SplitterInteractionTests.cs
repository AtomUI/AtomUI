using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUISplitter = AtomUI.Desktop.Controls.Splitter;

namespace AtomUI.Desktop.Controls.Tests.Splitter;

public class SplitterInteractionTests
{
    static SplitterInteractionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Dragger_Double_Click_Raises_Public_Event_With_Handle_Index_After_Release()
    {
        var splitter = CreateSplitter(3);
        var window = new AvaloniaWindow
        {
            Width   = 600,
            Height  = 320,
            Content = splitter
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var handle = splitter.GetVisualDescendants()
                                 .OfType<SplitterHandle>()
                                 .Single(item => item.HandleIndex == 1);
            var dragBar = handle.GetVisualDescendants()
                                .OfType<SplitterDragBar>()
                                .Single();

            var eventCount = 0;
            var handleIndex = -1;
            splitter.DraggerDoubleClicked += (_, e) =>
            {
                eventCount++;
                handleIndex = e.HandleIndex;
            };

            var pointer = RaiseLeftPress(dragBar, 2);

            eventCount.ShouldBe(0);

            RaiseLeftRelease(dragBar, pointer);

            eventCount.ShouldBe(1);
            handleIndex.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Dragger_Double_Click_Does_Not_Start_A_Second_Drag()
    {
        var splitter = CreateSplitter(2);
        var window = new AvaloniaWindow
        {
            Width   = 600,
            Height  = 320,
            Content = splitter
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var dragBar = splitter.GetVisualDescendants()
                                  .OfType<SplitterDragBar>()
                                  .Single();

            var resizeStartedCount = 0;
            var doubleClickedCount = 0;
            splitter.ResizeStarted         += (_, _) => resizeStartedCount++;
            splitter.DraggerDoubleClicked += (_, _) => doubleClickedCount++;

            var firstPointer = RaiseLeftPress(dragBar, 1);
            RaiseLeftRelease(dragBar, firstPointer);

            resizeStartedCount.ShouldBe(1);
            doubleClickedCount.ShouldBe(0);

            var secondPointer = RaiseLeftPress(dragBar, 2);

            resizeStartedCount.ShouldBe(1);
            doubleClickedCount.ShouldBe(0);

            RaiseLeftRelease(dragBar, secondPointer);

            resizeStartedCount.ShouldBe(1);
            doubleClickedCount.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Dragger_Double_Click_Remains_Available_When_Dragging_Is_Disabled()
    {
        var splitter = CreateSplitter(2);
        AtomUISplitter.SetIsResizable(splitter.Children[1], false);
        var window = new AvaloniaWindow
        {
            Width   = 600,
            Height  = 320,
            Content = splitter
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var dragBar = splitter.GetVisualDescendants()
                                  .OfType<SplitterDragBar>()
                                  .Single();
            dragBar.IsDragEnabled.ShouldBeFalse();

            var eventCount = 0;
            splitter.DraggerDoubleClicked += (_, _) => eventCount++;

            var pointer = RaiseLeftPress(dragBar, 2);
            RaiseLeftRelease(dragBar, pointer);

            eventCount.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Pointer_Capture_Loss_Cancels_Pending_Dragger_Double_Click()
    {
        var splitter = CreateSplitter(2);
        var window = new AvaloniaWindow
        {
            Width   = 600,
            Height  = 320,
            Content = splitter
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var dragBar = splitter.GetVisualDescendants()
                                  .OfType<SplitterDragBar>()
                                  .Single();
            var eventCount = 0;
            splitter.DraggerDoubleClicked += (_, _) => eventCount++;

            var pointer = RaiseLeftPress(dragBar, 2);
            dragBar.RaiseEvent(new PointerCaptureLostEventArgs(dragBar, pointer));
            RaiseLeftRelease(dragBar, pointer);

            eventCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Rebuilding_Handles_Detaches_Old_Dragger_Double_Click_Subscription()
    {
        var splitter = CreateSplitter(2);
        var window = new AvaloniaWindow
        {
            Width   = 600,
            Height  = 320,
            Content = splitter
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var oldDragBar = splitter.GetVisualDescendants()
                                     .OfType<SplitterDragBar>()
                                     .Single();
            var eventCount = 0;
            splitter.DraggerDoubleClicked += (_, _) => eventCount++;

            splitter.Children.Add(new Border());
            Dispatcher.UIThread.RunJobs();

            var oldPointer = RaiseLeftPress(oldDragBar, 2);
            RaiseLeftRelease(oldDragBar, oldPointer);

            eventCount.ShouldBe(0);

            var newDragBar = splitter.GetVisualDescendants()
                                     .OfType<SplitterHandle>()
                                     .Single(item => item.HandleIndex == 0)
                                     .GetVisualDescendants()
                                     .OfType<SplitterDragBar>()
                                     .Single();
            var newPointer = RaiseLeftPress(newDragBar, 2);
            RaiseLeftRelease(newDragBar, newPointer);

            eventCount.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Nested_Splitter_Dragger_Double_Click_Does_Not_Notify_Outer_Splitter()
    {
        var innerSplitter = CreateSplitter(2);
        var outerSplitter = new AtomUISplitter
        {
            Width       = 600,
            Height      = 280,
            Orientation = Orientation.Vertical,
            Children =
            {
                innerSplitter,
                new Border()
            }
        };
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 360,
            Content = outerSplitter
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var innerEventCount = 0;
            var outerEventCount = 0;
            innerSplitter.DraggerDoubleClicked += (_, _) => innerEventCount++;
            outerSplitter.DraggerDoubleClicked += (_, _) => outerEventCount++;

            var dragBar = innerSplitter.GetVisualDescendants()
                                       .OfType<SplitterDragBar>()
                                       .Single();
            var pointer = RaiseLeftPress(dragBar, 2);
            RaiseLeftRelease(dragBar, pointer);

            innerEventCount.ShouldBe(1);
            outerEventCount.ShouldBe(0);
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUISplitter CreateSplitter(int panelCount)
    {
        var splitter = new AtomUISplitter
        {
            Width       = 560,
            Height      = 240,
            Orientation = Orientation.Vertical
        };

        for (var i = 0; i < panelCount; i++)
        {
            splitter.Children.Add(new Border());
        }

        return splitter;
    }

    private static Pointer RaiseLeftPress(Control source, int clickCount)
    {
        var pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            pointer,
            source,
            default,
            0,
            new PointerPointProperties(
                RawInputModifiers.LeftMouseButton,
                PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None,
            clickCount));
        return pointer;
    }

    private static void RaiseLeftRelease(Control source, Pointer pointer)
    {
        source.RaiseEvent(new PointerReleasedEventArgs(
            source,
            pointer,
            source,
            default,
            1,
            new PointerPointProperties(
                RawInputModifiers.None,
                PointerUpdateKind.LeftButtonReleased),
            KeyModifiers.None,
            MouseButton.Left));
    }
}
