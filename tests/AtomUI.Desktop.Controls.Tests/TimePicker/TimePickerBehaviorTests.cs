using Avalonia.Input.Raw;
using Avalonia.Input;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TimePickers;

public class TimePickerBehaviorTests
{
    static TimePickerBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void SelectedTime_Is_TwoWay_And_DataValidation_Enabled()
    {
        var metadata = TimePicker.SelectedTimeProperty.GetMetadata(typeof(TimePicker));

        metadata.DefaultBindingMode.ShouldBe(BindingMode.TwoWay);
        metadata.EnableDataValidation.ShouldBe(true);
    }

    [Fact]
    public void SelectedTime_DefaultBindingMode_Updates_ViewModel()
    {
        var initialTime = new TimeSpan(10, 9, 20);
        var updatedTime = new TimeSpan(12, 12, 20);
        var viewModel = new TimePickerBindingViewModel
        {
            SelectedTime = initialTime
        };
        var timePicker = new TimePicker
        {
            Width = 240
        };
        timePicker.Bind(
            TimePicker.SelectedTimeProperty,
            new Binding(nameof(TimePickerBindingViewModel.SelectedTime))
            {
                Source = viewModel
            });

        ShowInWindow(timePicker, () =>
        {
            timePicker.SelectedTime.ShouldBe(initialTime);

            timePicker.SelectedTime = updatedTime;
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedTime.ShouldBe(updatedTime);
        });
    }

    [Fact]
    public void Pinned_Open_Request_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It()
    {
        var timePicker = new TimePicker
        {
            Width             = 240,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true
        };

        ShowInWindow(timePicker, () =>
        {
            var popup = timePicker.GetVisualDescendants()
                                  .OfType<Popup>()
                                  .Single(item => item.Name == "PART_Popup");

            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            var view = popup.Child.ShouldNotBeNull().GetVisualDescendants().OfType<TimeView>().Single();
            var selector = view.GetVisualDescendants().OfType<DateTimePickerPanel>().First();
            var owner = TopLevel.GetTopLevel(view).ShouldNotBeNull();
            var viewport = selector.GetVisualAncestors().OfType<Avalonia.Controls.Presenters.ScrollContentPresenter>().First();
            var point = viewport.TranslatePoint(new Point(viewport.Bounds.Width / 2, viewport.Bounds.Height / 2), owner).ShouldNotBeNull();
            var other = new AvaloniaWindow { Width = 640, Height = 600, Content = new Border { Background = Brushes.Blue } };
            other.Show();
            try
            {
                other.MouseMove(point);
                view.IsPointerInSelector.ShouldBeFalse();
                owner.MouseMove(point);
                view.IsPointerInSelector.ShouldBeTrue();
                using var device = new MouseDevice();
                var root = view.GetPresentationSource().ShouldNotBeNull().ShouldBeAssignableTo<IInputRoot>();
                var manager = (IInputManager)AvaloniaLocator.CurrentMutable.GetService(typeof(IInputManager))!;
                manager.ProcessInput(new RawPointerEventArgs(device, 42, root, RawPointerEventType.LeaveWindow, new Point(-1, -1), default));
                view.IsPointerInSelector.ShouldBeFalse();
            }
            finally
            {
                other.Close();
            }

            timePicker.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen.ShouldBeFalse();
            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void Range_Pinned_Open_Request_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It()
    {
        var rangePicker = new RangeTimePicker
        {
            Width             = 420,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true
        };

        ShowInWindow(rangePicker, () =>
        {
            var popup = rangePicker.GetVisualDescendants()
                                   .OfType<Popup>()
                                   .Single(item => item.Name == "PART_Popup");

            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            rangePicker.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen.ShouldBeFalse();
            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            EnableOverlayLayer = true,
            Child = content
        };
        EnablePopupOverlayLayer(visualLayerManager);
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }

    private sealed class TimePickerBindingViewModel : INotifyPropertyChanged
    {
        private TimeSpan? _selectedTime;

        public TimeSpan? SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (_selectedTime == value)
                {
                    return;
                }

                _selectedTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTime)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
