using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

public class DatePickerBehaviorTests
{
    static DatePickerBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pinned_Open_Request_Opens_DatePicker_And_Its_Template_Popup()
    {
        var datePicker = new Desktop.Controls.DatePicker
        {
            Width           = 240,
            IsMotionEnabled = false
        };

        ShowInWindow(datePicker, () =>
        {
            datePicker.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popup = datePicker.GetVisualDescendants()
                                  .OfType<Popup>()
                                  .Single(item => item.Name == "PART_Popup");
            datePicker.IsPickerOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_Open_Request_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It()
    {
        var datePicker = new Desktop.Controls.DatePicker
        {
            Width             = 240,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true
        };

        ShowInWindow(datePicker, () =>
        {
            var popup = datePicker.GetVisualDescendants()
                                  .OfType<Popup>()
                                  .Single(item => item.Name == "PART_Popup");

            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            datePicker.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen.ShouldBeFalse();
            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_DatePicker_Rejects_ClosePickerFlyout_Request()
    {
        var datePicker = new Desktop.Controls.DatePicker
        {
            Width           = 240,
            IsMotionEnabled = false
        };

        ShowInWindow(datePicker, () =>
        {
            datePicker.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = datePicker.GetVisualDescendants()
                                  .OfType<Popup>()
                                  .Single(item => item.Name == "PART_Popup");

            datePicker.ClosePickerFlyout();
            Dispatcher.UIThread.RunJobs();

            datePicker.IsPickerOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_DatePicker_Close_Request_Does_Not_Publish_A_Transient_Closed_State()
    {
        var datePicker = new Desktop.Controls.DatePicker
        {
            Width           = 240,
            IsMotionEnabled = false
        };

        ShowInWindow(datePicker, () =>
        {
            datePicker.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var pickerOpenChangeCount = 0;
            datePicker.PropertyChanged += (_, change) =>
            {
                if (change.Property.Name == "IsPickerOpen")
                {
                    ++pickerOpenChangeCount;
                }
            };

            datePicker.ClosePickerFlyout();
            Dispatcher.UIThread.RunJobs();

            datePicker.IsPickerOpen.ShouldBeTrue();
            pickerOpenChangeCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Unpinning_A_Pending_DatePicker_Request_Clears_The_Business_Open_State()
    {
        var datePicker = new Desktop.Controls.DatePicker
        {
            Width           = 240,
            IsMotionEnabled = false
        };

        datePicker.IsPopupPinnedOpen = true;
        datePicker.IsPickerOpen.ShouldBeTrue();

        datePicker.IsPopupPinnedOpen = false;

        datePicker.IsPickerOpen.ShouldBeFalse();
    }

    [Fact]
    public void Pinned_DatePicker_Detach_Cleans_Up_And_Reattach_Reopens()
    {
        var datePicker = new Desktop.Controls.DatePicker
        {
            Width           = 240,
            IsMotionEnabled = false
        };
        var panel = new Avalonia.Controls.Grid();
        panel.Children.Add(datePicker);
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = panel
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            datePicker.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = datePicker.GetVisualDescendants()
                                  .OfType<Popup>()
                                  .Single(item => item.Name == "PART_Popup");
            popup.IsOpen.ShouldBeTrue();
            datePicker.PickerPresenter.ShouldNotBeNull();

            panel.Children.Remove(datePicker);
            Dispatcher.UIThread.RunJobs();

            datePicker.IsPopupPinnedOpen.ShouldBeTrue();
            datePicker.IsPickerOpen.ShouldBeFalse();
            datePicker.PickerPresenter.ShouldBeNull();
            popup.IsOpen.ShouldBeFalse();

            panel.Children.Add(datePicker);
            Dispatcher.UIThread.RunJobs();

            datePicker.IsPickerOpen.ShouldBeTrue();
            datePicker.PickerPresenter.ShouldNotBeNull();
            popup.IsOpen.ShouldBeTrue();
        }
        finally
        {
            datePicker.IsPopupPinnedOpen = false;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void SelectedDateTime_Is_TwoWay_And_DataValidation_Enabled()
    {
        var metadata = Desktop.Controls.DatePicker.SelectedDateTimeProperty.GetMetadata(
            typeof(Desktop.Controls.DatePicker));

        metadata.DefaultBindingMode.ShouldBe(BindingMode.TwoWay);
        metadata.EnableDataValidation.ShouldBe(true);
    }

    [Fact]
    public void Date_Boundary_Properties_Default_To_Null_And_Are_Shared_With_RangeDatePicker()
    {
        var datePicker  = new Desktop.Controls.DatePicker();
        var rangePicker = new Desktop.Controls.RangeDatePicker();

        datePicker.MinDate.ShouldBeNull();
        datePicker.MaxDate.ShouldBeNull();
        rangePicker.MinDate.ShouldBeNull();
        rangePicker.MaxDate.ShouldBeNull();
        Desktop.Controls.RangeDatePicker.MinDateProperty.ShouldBeSameAs(
            Desktop.Controls.DatePicker.MinDateProperty);
        Desktop.Controls.RangeDatePicker.MaxDateProperty.ShouldBeSameAs(
            Desktop.Controls.DatePicker.MaxDateProperty);
    }

    [Fact]
    public void SelectedDateTime_DefaultBindingMode_Updates_ViewModel()
    {
        var initialDate = new DateTime(2026, 7, 5);
        var updatedDate = new DateTime(2026, 7, 6);
        var viewModel = new DatePickerBindingViewModel
        {
            SelectedDateTime = initialDate
        };
        var datePicker = new Desktop.Controls.DatePicker
        {
            Width = 240
        };
        datePicker.Bind(
            Desktop.Controls.DatePicker.SelectedDateTimeProperty,
            new Binding(nameof(DatePickerBindingViewModel.SelectedDateTime))
            {
                Source = viewModel
            });

        ShowInWindow(datePicker, () =>
        {
            datePicker.SelectedDateTime.ShouldBe(initialDate);

            datePicker.SelectedDateTime = updatedDate;
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedDateTime.ShouldBe(updatedDate);
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class DatePickerBindingViewModel : INotifyPropertyChanged
    {
        private DateTime? _selectedDateTime;

        public DateTime? SelectedDateTime
        {
            get => _selectedDateTime;
            set
            {
                if (_selectedDateTime == value)
                {
                    return;
                }

                _selectedDateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDateTime)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
