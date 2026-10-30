using AtomUI.Desktop.Controls.Internal.DateViewer;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = AtomUI.Desktop.Controls.Window;
using AtomUIButton = AtomUI.Desktop.Controls.Button;

namespace AtomUI.Desktop.Controls.Tests.DatePickers;

public class DatePickerViewerBehaviorTests
{
    static DatePickerViewerBehaviorTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(DatePickerMode.Date, DateViewerCellType.Date)]
    [InlineData(DatePickerMode.Week, DateViewerCellType.Date)]
    [InlineData(DatePickerMode.Month, DateViewerCellType.Month)]
    [InlineData(DatePickerMode.Quarter, DateViewerCellType.Quarter)]
    [InlineData(DatePickerMode.Year, DateViewerCellType.Year)]
    public void Picker_Uses_Shared_Viewer_For_Each_Selection_Unit_And_Commits_Only_On_Confirm(
        DatePickerMode pickerMode,
        DateViewerCellType cellType)
    {
        var picker = new DatePicker
        {
            PickerMode = pickerMode,
            PickerDisplayDate = new DateTime(2026, 7, 15),
            IsNeedConfirm = true
        };

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<DatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<DateViewer>().Single();
            if (pickerMode == DatePickerMode.Week)
            {
                viewer.PanelSession.Input.FirstDayOfWeek.ShouldBe(DayOfWeek.Sunday);
                var hoveredDate = new DateTime(2026, 7, 15);
                viewer.PanelSession.Apply(new DatePanelAction.Hover(hoveredDate));
                Drain();
                var weekRow = viewer.GetVisualDescendants().OfType<DateViewerCell>()
                    .Where(candidate => candidate.Model!.Row == 2).ToArray();
                weekRow.Length.ShouldBe(8);
                foreach (var weekCell in weekRow)
                {
                    weekCell.GetVisualDescendants().OfType<Avalonia.Controls.Border>()
                        .Single(border => border.Name == "PART_WeekSelection").IsVisible.ShouldBeTrue();
                }
            }
            viewer.Bounds.Width.ShouldBe(288, 0.5);
            viewer.Bounds.Height.ShouldBe(pickerMode == DatePickerMode.Quarter ? 86 : 301, 0.5);
            var cell = viewer.GetVisualDescendants()
                             .OfType<DateViewerCell>()
                             .First(candidate => candidate.Model is
                                 {
                                     IsFocusable: true,
                                     IsInView: true
                                 } model && model.Kind == cellType);
            var frame = cell.GetVisualDescendants().OfType<Avalonia.Controls.Border>()
                .Single(border => border.Name == "PART_ValueFrame");
            if (pickerMode is DatePickerMode.Month or DatePickerMode.Quarter or DatePickerMode.Year)
            {
                frame.Bounds.Width.ShouldBe(60, 0.5);
                frame.Padding.Left.ShouldBe(8);
                frame.Padding.Right.ShouldBe(8);
            }
            frame.TranslatePoint(new Avalonia.Point(0, frame.Bounds.Height / 2), cell)!.Value.Y
                .ShouldBe(cell.Bounds.Height / 2, 0.5);

            viewer.Value.ShouldBeNull("a hosted viewer reports intent and the picker remains the value owner");
            cell.Activate();
            Drain();

            picker.SelectedDateTime.ShouldBeNull();
            var candidate = presenter.SelectedDateTime.ShouldNotBeNull();
            viewer.Value.ShouldBeNull();

            Confirm(presenter);

            picker.SelectedDateTime.ShouldBe(candidate);
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Picker_Open_Uses_Selected_Value_Before_Display_Anchor_Without_Transferring_Value_Ownership()
    {
        var selected = new DateTime(2026, 2, 11, 14, 25, 0);
        var picker = new DatePicker
        {
            PickerDisplayDate = new DateTime(2027, 10, 20),
            SelectedDateTime = selected
        };

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<DatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<DateViewer>().Single();

            viewer.PanelSession.Input.DisplayDate.ShouldBe(selected.Date);
            viewer.PanelSession.Input.SelectedDate.ShouldBe(selected);
            viewer.Value.ShouldBeNull();
            picker.SelectedDateTime.ShouldBe(selected);
        });
    }

    [Fact]
    public void Picker_Open_Normalizes_And_Clamps_An_Empty_Display_Anchor_To_Its_Bounds()
    {
        var picker = new DatePicker
        {
            PickerMode = DatePickerMode.Month,
            PickerDisplayDate = new DateTime(2026, 5, 20),
            MinDate = new DateTime(2026, 7, 15),
            MaxDate = new DateTime(2026, 10, 31)
        };

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<DatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<DateViewer>().Single();

            viewer.PanelSession.Input.DisplayDate.ShouldBe(new DateTime(2026, 7, 1));
            viewer.PanelSession.Input.SelectedDate.ShouldBeNull();
            viewer.Value.ShouldBeNull();
            picker.SelectedDateTime.ShouldBeNull();
        });
    }

    [Fact]
    public void Range_End_Open_Uses_The_Start_Dual_Panel_Period_And_Keeps_The_Host_As_Value_Owner()
    {
        var start = new DateTime(2026, 1, 12);
        var end = new DateTime(2026, 2, 11);
        var picker = new RangeDatePicker
        {
            PickerDisplayDate = new DateTime(2027, 9, 23),
            RangeStartSelectedDate = start,
            RangeEndSelectedDate = end
        };
        picker.RangeActivatedPart = RangeActivatedPart.End;

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<RangeDatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<RangeDateViewer>().Single();

            viewer.PanelSession.Input.DisplayDate.ShouldBe(start);
            viewer.PanelSession.Input.Range.ShouldBe(new DateViewerRange(start, end));
            viewer.PanelSession.Input.PanelCount.ShouldBe(2);
            viewer.Bounds.Width.ShouldBe(596, 0.5);
            viewer.Bounds.Height.ShouldBe(301, 0.5);
            var panels = viewer.GetVisualDescendants().OfType<DatePanel>().ToArray();
            panels[0].Bounds.Width.ShouldBe(288, 0.5);
            panels[1].Bounds.X.ShouldBe(308, 0.5);
            presenter.GetVisualDescendants().OfType<DateViewerHeader>()
                     .ShouldAllBe(header => header.HeaderTemplate != null);
            viewer.Value.ShouldBeNull();
        });
    }

    [Fact]
    public void Range_End_Open_Keeps_The_Start_Period_As_The_Left_Dual_Panel_When_The_Range_Is_In_One_Month()
    {
        var start = new DateTime(2026, 7, 6);
        var end = new DateTime(2026, 7, 12);
        var picker = new RangeDatePicker
        {
            PickerDisplayDate = new DateTime(2027, 9, 23),
            RangeStartSelectedDate = start,
            RangeEndSelectedDate = end
        };
        picker.RangeActivatedPart = RangeActivatedPart.End;

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<RangeDatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<RangeDateViewer>().Single();

            viewer.PanelSession.Input.DisplayDate.ShouldBe(start);
            viewer.PanelSession.Models[0].Anchor.ShouldBe(new DateTime(2026, 7, 1));
            viewer.PanelSession.Models[1].Anchor.ShouldBe(new DateTime(2026, 8, 1));
            viewer.PanelSession.Input.Range.ShouldBe(new DateViewerRange(start, end));
            viewer.PanelSession.Input.ActiveRangePart.ShouldBe(DateRangeActivePart.End);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Range_Partial_Confirmation_Survives_Reopen_And_Final_Confirmation_Orders_Dates(bool showTime)
    {
        var picker = new RangeDatePicker
        {
            PickerDisplayDate = new DateTime(2026, 7, 15),
            IsNeedConfirm = true,
            IsShowTime = showTime
        };
        picker.RangeActivatedPart = RangeActivatedPart.Start;

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<RangeDatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<RangeDateViewer>().Single();

            Activate(viewer, new DateTime(2026, 7, 25));
            picker.RangeStartSelectedDate.ShouldBeNull();
            Confirm(presenter);

            picker.IsPickerOpen.ShouldBeTrue();
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 7, 25));
            picker.RangeEndSelectedDate.ShouldBeNull();
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.End);
            viewer.Value.ShouldBeNull();

            picker.IsPickerOpen = false;
            Drain();
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 7, 25));
            picker.RangeEndSelectedDate.ShouldBeNull();
            picker.SecondaryText.ShouldBe(string.Empty);

            picker.IsPickerOpen = true;
            Drain();
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.End);
            viewer.PanelSession.Input.Range.ShouldBe(new DateViewerRange(new DateTime(2026, 7, 25), null));

            Activate(viewer, new DateTime(2026, 7, 20));
            var window = TopLevel.GetTopLevel(picker).ShouldBeAssignableTo<AvaloniaWindow>()!;
            ClickConfirm(window, presenter);

            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 7, 20));
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 7, 25));
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void End_First_Time_Range_Discards_Unconfirmed_Preview_But_Preserves_Confirmed_End_Across_Reopen()
    {
        var picker = new RangeDatePicker
        {
            PickerDisplayDate = new DateTime(2026, 10, 4),
            IsNeedConfirm = true,
            IsShowTime = true
        };
        picker.RangeActivatedPart = RangeActivatedPart.End;

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();
            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<RangeDatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<RangeDateViewer>().Single();

            Activate(viewer, new DateTime(2026, 10, 16));
            Hover(viewer, new DateTime(2026, 10, 31));
            (picker.SecondaryText ?? string.Empty).ShouldContain("2026-10-31");
            picker.IsPickerOpen = false;
            Drain();
            picker.RangeEndSelectedDate.ShouldBeNull();
            picker.SecondaryText.ShouldBe(string.Empty);

            picker.RangeActivatedPart = RangeActivatedPart.End;
            picker.IsPickerOpen = true;
            Drain();
            Activate(viewer, new DateTime(2026, 10, 16));
            Confirm(presenter);
            picker.RangeStartSelectedDate.ShouldBeNull();
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 10, 16));
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.Start);

            picker.IsPickerOpen = false;
            Drain();
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 10, 16));
            (picker.SecondaryText ?? string.Empty).ShouldContain("2026-10-16");

            picker.IsPickerOpen = true;
            Drain();
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.Start);
            Activate(viewer, new DateTime(2026, 10, 12));
            var window = TopLevel.GetTopLevel(picker).ShouldBeAssignableTo<AvaloniaWindow>()!;
            ClickConfirm(window, presenter);

            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 10, 12));
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 10, 16));
            picker.IsPickerOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Complete_Range_Reopen_Rotates_The_Active_End_And_Previews_The_Next_Start()
    {
        var picker = new RangeDatePicker
        {
            RangeStartSelectedDate = new DateTime(2026, 10, 12),
            RangeEndSelectedDate = new DateTime(2026, 11, 23)
        };
        picker.RangeActivatedPart = RangeActivatedPart.End;

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<RangeDatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<RangeDateViewer>().Single();

            Activate(viewer, new DateTime(2026, 11, 16));

            picker.IsPickerOpen.ShouldBeTrue();
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 10, 12));
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 11, 16));
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.Start);
            viewer.PanelSession.Input.ActiveRangePart.ShouldBe(DateRangeActivePart.Start);

            Hover(viewer, new DateTime(2026, 10, 13));

            picker.Text.ShouldBe("2026-10-13");
            picker.SecondaryText.ShouldBe("2026-11-16");
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 10, 12));
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 11, 16));
        });
    }

    [Fact]
    public void Bound_Range_Partial_End_Confirmation_Preserves_The_Opposite_Start_Value()
    {
        var viewModel = new RangeBindingViewModel
        {
            Start = new DateTime(2026, 10, 8),
            End = new DateTime(2026, 11, 26)
        };
        var picker = new RangeDatePicker
        {
            DataContext = viewModel
        };
        picker.Bind(RangeDatePicker.RangeStartSelectedDateProperty, new Binding(nameof(RangeBindingViewModel.Start)));
        picker.Bind(RangeDatePicker.RangeEndSelectedDateProperty, new Binding(nameof(RangeBindingViewModel.End)));
        picker.RangeActivatedPart = RangeActivatedPart.End;

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<RangeDatePickerPresenter>()!;
            var viewer = presenter.GetVisualDescendants().OfType<RangeDateViewer>().Single();

            Hover(viewer, new DateTime(2026, 11, 18));
            Activate(viewer, new DateTime(2026, 11, 18));

            picker.IsPickerOpen.ShouldBeTrue();
            picker.Text.ShouldBe("2026-10-08");
            picker.SecondaryText.ShouldBe("2026-11-18");
            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 10, 8));
            picker.RangeEndSelectedDate.ShouldBe(new DateTime(2026, 11, 18));
            viewModel.Start.ShouldBe(new DateTime(2026, 10, 8));
            viewModel.End.ShouldBe(new DateTime(2026, 11, 18));
            picker.RangeActivatedPart.ShouldBe(RangeActivatedPart.Start);
            viewer.PanelSession.Input.Range.ShouldBe(new DateViewerRange(new DateTime(2026, 10, 8), new DateTime(2026, 11, 18)));

            var startCell = Cell(viewer, new DateTime(2026, 10, 8));
            var endCell = Cell(viewer, new DateTime(2026, 11, 18));
            startCell.Model!.IsSelected.ShouldBeTrue();
            startCell.Model.IsVisualEndpoint.ShouldBeTrue();
            startCell.Model.IsRangeStart.ShouldBeTrue();
            endCell.Model!.IsSelected.ShouldBeTrue();
            endCell.Model.IsVisualEndpoint.ShouldBeTrue();
            endCell.Model.IsRangeEnd.ShouldBeTrue();
            startCell.Model.HasRangePreview.ShouldBeFalse();
            endCell.Model.HasRangePreview.ShouldBeFalse();
        });
    }

    [Fact]
    public void Activating_The_Only_Filled_Range_Endpoint_Does_Not_Clear_Its_Visible_Text()
    {
        var picker = new RangeDatePicker
        {
            RangeStartSelectedDate = new DateTime(2026, 10, 8)
        };

        ShowPicker(picker, () =>
        {
            var startInput = picker.GetVisualDescendants()
                                   .OfType<TextBox>()
                                   .Single(candidate => candidate.Name == "PART_InfoInputBox");

            startInput.Text.ShouldBe("2026-10-08");

            picker.RangeActivatedPart = RangeActivatedPart.Start;
            Drain();

            picker.RangeStartSelectedDate.ShouldBe(new DateTime(2026, 10, 8));
            picker.Text.ShouldBe("2026-10-08");
            startInput.Text.ShouldBe("2026-10-08");
        });
    }

    [Fact]
    public void Timed_Range_Uses_One_Date_Panel_And_NonTimed_Range_Uses_Two()
    {
        foreach (var showTime in new[] { true, false })
        {
            var picker = new RangeDatePicker
            {
                IsShowTime = showTime,
                PickerDisplayDate = new DateTime(2026, 7, 15)
            };

            ShowPicker(picker, () =>
            {
                picker.IsPickerOpen = true;
                Drain();

                var presenter = picker.PickerPresenter.ShouldBeAssignableTo<RangeDatePickerPresenter>()!;
                var viewer = presenter.GetVisualDescendants().OfType<RangeDateViewer>().Single();

                viewer.PanelSession.Input.PanelCount.ShouldBe(showTime ? 1 : 2);
                viewer.Bounds.Width.ShouldBe(showTime ? 288 : 596, 0.5);
                viewer.Bounds.Height.ShouldBe(301, 0.5);
                presenter.GetVisualDescendants()
                         .OfType<DateViewerCell>()
                         .Count()
                         .ShouldBe(showTime ? 42 : 84);
                presenter.GetVisualDescendants().OfType<TimeView>().Single().IsVisible.ShouldBe(showTime);
                viewer.Value.ShouldBeNull();
            });
        }
    }

    [Fact]
    public void Today_And_Now_Actions_Are_Disabled_When_The_Current_Date_Is_Out_Of_Range()
    {
        var picker = new DatePicker
        {
            IsShowTime = true,
            IsShowNow = true,
            MinDate = DateTime.Today.AddDays(1),
            MaxDate = DateTime.Today.AddDays(10)
        };

        ShowPicker(picker, () =>
        {
            picker.IsPickerOpen = true;
            Drain();

            var presenter = picker.PickerPresenter.ShouldBeAssignableTo<DatePickerPresenter>()!;
            var buttons = presenter.GetVisualDescendants().OfType<AtomUIButton>().ToArray();

            buttons.Single(button => button.Name == "PART_TodayButton").IsEnabled.ShouldBeFalse();
            buttons.Single(button => button.Name == "PART_NowButton").IsEnabled.ShouldBeFalse();
            picker.SelectedDateTime.ShouldBeNull();
        });
    }

    private static void Activate(RangeDateViewer viewer, DateTime value)
    {
        viewer.GetVisualDescendants()
              .OfType<DateViewerCell>()
              .Single(cell => cell.Model?.Value == value)
              .Activate();
        Drain();
    }

    private static void Hover(RangeDateViewer viewer, DateTime value)
    {
        viewer.PanelSession.Apply(new DatePanelAction.Hover(value));
        Drain();
    }

    private static DateViewerCell Cell(RangeDateViewer viewer, DateTime value) =>
        viewer.GetVisualDescendants()
              .OfType<DateViewerCell>()
              .Single(cell => cell.Model?.Value == value);

    private static void Confirm(DatePickerPresenter presenter)
    {
        var button = presenter.GetVisualDescendants()
                              .OfType<AtomUIButton>()
                              .Single(candidate => candidate.Name == "PART_ConfirmButton");
        button.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent));
        Drain();
    }

    private static void ClickConfirm(AvaloniaWindow window, DatePickerPresenter presenter)
    {
        var button = presenter.GetVisualDescendants().OfType<AtomUIButton>()
            .Single(candidate => candidate.Name == "PART_ConfirmButton");
        var point = button.TranslatePoint(
            new Avalonia.Point(button.Bounds.Width / 2, button.Bounds.Height / 2), window).ShouldNotBeNull();
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Drain();
    }

    private static void ShowPicker(Control picker, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 960,
            Height = 800,
            Content = picker
        };

        try
        {
            window.Show();
            Drain();
            assertion();
        }
        finally
        {
            window.Close();
            Drain();
        }
    }

    private static void Drain() => Dispatcher.UIThread.RunJobs();

    private sealed class RangeBindingViewModel
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }
}
