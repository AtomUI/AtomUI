using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUITimePicker = AtomUI.Desktop.Controls.TimePicker;

namespace AtomUI.Desktop.Controls.Tests.TimePickers;

public class TimePickerInputWidthStabilityTests
{
    public TimePickerInputWidthStabilityTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Input_Keeps_The_Same_Width_Before_And_After_Selection_With_Explicit_Width()
    {
        var picker = CreateExamplePicker();
        picker.Width = 300;
        RunStability(picker);
    }

    [Fact]
    public void Input_Keeps_The_Same_Width_Before_And_After_Selection_With_Stretch_Alignment()
    {
        var picker = CreateExamplePicker();
        picker.HorizontalAlignment = HorizontalAlignment.Stretch;
        RunStability(picker);
    }

    [Fact]
    public void Owner_Keeps_The_Same_Width_Before_And_After_Selection_Without_Explicit_Width()
    {
        var picker = CreateExamplePicker();
        RunStability(picker);
    }

    private static void RunStability(AtomUITimePicker picker)
    {
        var window = new AvaloniaWindow { Width = 900, Height = 200, Content = picker };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            picker.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var beforeOwner = picker.Bounds.Width;
            var beforeInput = InputWidth(picker);
            beforeInput.ShouldBeGreaterThan(0);

            picker.SelectedTime = new TimeSpan(12, 2, 0);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            picker.Bounds.Width.ShouldBe(beforeOwner,
                "owner width must not change between the placeholder and the selected states.");
            InputWidth(picker).ShouldBe(beforeInput,
                "input width must not change between the placeholder and the selected states.");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static double InputWidth(AtomUITimePicker picker)
    {
        return picker.GetVisualDescendants()
                     .OfType<Control>()
                     .First(control => control.Classes.Contains("semantic-input"))
                     .Bounds.Width;
    }

    private static AtomUITimePicker CreateExamplePicker()
    {
        return new AtomUITimePicker
        {
            IsNeedConfirm = false,
            IsShowNow = true,
            IsMotionEnabled = false,
            ContentLeftAddOn = "Object",
            PlaceholderText = "Object"
        };
    }
}
