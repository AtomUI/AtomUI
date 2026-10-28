using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUIGallery.Desktop;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases;
using AtomUIGallery.ShowCases.Alert;
using AtomUIGallery.ShowCases.AutoComplete;
using AtomUIGallery.ShowCases.Avatar;
using AtomUIGallery.ShowCases.Badge;
using AtomUIGallery.ShowCases.Breadcrumb;
using AtomUIGallery.ShowCases.Button;
using AtomUIGallery.ShowCases.ButtonSpinner;
using AtomUIGallery.ShowCases.Calendar;
using AtomUIGallery.ShowCases.Card;
using AtomUIGallery.ShowCases.Carousel;
using AtomUIGallery.ShowCases.Cascader;
using AtomUIGallery.ShowCases.CheckBox;
using AtomUIGallery.ShowCases.Collapse;
using AtomUIGallery.ShowCases.ColorPicker;
using AtomUIGallery.ShowCases.ComboBox;
using AtomUIGallery.ShowCases.DataGrid;
using AtomUIGallery.ShowCases.DatePicker;
using AtomUIGallery.ShowCases.Descriptions;
using AtomUIGallery.ShowCases.Drawer;
using AtomUIGallery.ShowCases.DropdownButton;
using AtomUIGallery.ShowCases.Empty;
using AtomUIGallery.ShowCases.Expander;
using AtomUIGallery.ShowCases.FloatButton;
using AtomUIGallery.ShowCases.Form;
using AtomUIGallery.ShowCases.GroupBox;
using AtomUIGallery.ShowCases.Icon;
using AtomUIGallery.ShowCases.ImagePreviewer;
using AtomUIGallery.ShowCases.InfoFlyout;
using AtomUIGallery.ShowCases.LineEdit;
using AtomUIGallery.ShowCases.List;
using AtomUIGallery.ShowCases.Mentions;
using AtomUIGallery.ShowCases.Menu;
using AtomUIGallery.ShowCases.Message;
using AtomUIGallery.ShowCases.Modal;
using AtomUIGallery.ShowCases.Notification;
using AtomUIGallery.ShowCases.NumberUpDown;
using AtomUIGallery.ShowCases.Overview;
using AtomUIGallery.ShowCases.Pagination;
using AtomUIGallery.ShowCases.PopupConfirm;
using AtomUIGallery.ShowCases.ProgressBar;
using AtomUIGallery.ShowCases.QRCode;
using AtomUIGallery.ShowCases.RadioButton;
using AtomUIGallery.ShowCases.Rate;
using AtomUIGallery.ShowCases.Result;
using AtomUIGallery.ShowCases.Segmented;
using AtomUIGallery.ShowCases.Select;
using AtomUIGallery.ShowCases.Skeleton;
using AtomUIGallery.ShowCases.Slider;
using AtomUIGallery.ShowCases.Space;
using AtomUIGallery.ShowCases.Spin;
using AtomUIGallery.ShowCases.SplitButton;
using AtomUIGallery.ShowCases.Splitter;
using AtomUIGallery.ShowCases.Statistic;
using AtomUIGallery.ShowCases.Steps;
using AtomUIGallery.ShowCases.TabControl;
using AtomUIGallery.ShowCases.TimePicker;
using AtomUIGallery.ShowCases.Timeline;
using AtomUIGallery.ShowCases.ToggleSwitch;
using AtomUIGallery.ShowCases.Tour;
using AtomUIGallery.ShowCases.Transfer;
using AtomUIGallery.ShowCases.TreeSelect;
using AtomUIGallery.ShowCases.TreeView;
using AtomUIGallery.ShowCases.Upload;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUI.GalleryPerformance;

internal static class Program
{
    private static readonly Size WindowSize = new(1300, 900);
    private static readonly Rect WindowBounds = new(0, 0, WindowSize.Width, WindowSize.Height);
#if DEBUG
    private const string BuildConfiguration = "Debug";
#else
    private const string BuildConfiguration = "Release";
#endif
    private const string ColdChildSamplePrefix = "__ATOMUI_COLD_SAMPLE__";
    private static readonly ShowCaseSpec Overview = new(
        "OverviewPage",
        OverviewViewModel.ID,
        "AtomUIGallery.ShowCases.Overview.OverviewPage",
        "controlgallery/AtomUIGallery/ShowCases/General/Overview/Views/OverviewPage.axaml",
        stats => stats.VisualCount > 0);
    private static readonly IReadOnlyDictionary<string, ShowCaseSpec> ShowCases =
        new Dictionary<string, ShowCaseSpec>(StringComparer.OrdinalIgnoreCase)
        {
            ["lineedit"] = new(
                "LineEditShowCase",
                LineEditViewModel.ID,
                "AtomUIGallery.ShowCases.Views.LineEditShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/LineEditShowCase.axaml",
                stats => stats.LineEditCount > 0),
            ["icon"] = new(
                "IconShowCase",
                IconViewModel.ID,
                "AtomUIGallery.ShowCases.Views.IconShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/General/IconShowCase.axaml",
                stats => stats.IconCount > 0),
            ["avatar"] = new(
                "AvatarShowCase",
                AvatarViewModel.ID,
                "AtomUIGallery.ShowCases.Views.AvatarShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/AvatarShowCase.axaml",
                stats => stats.AvatarCount > 0),
            ["badge"] = new(
                "BadgeShowCase",
                BadgeViewModel.ID,
                "AtomUIGallery.ShowCases.Views.BadgeShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/BadgeShowCase.axaml",
                stats => stats.CountBadgeCount > 0 || stats.DotBadgeCount > 0 || stats.RibbonBadgeCount > 0),
            ["infoflyout"] = new(
                "InfoFlyoutShowCase",
                InfoFlyoutViewModel.ID,
                "AtomUIGallery.ShowCases.Views.InfoFlyoutShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/InfoFlyoutShowCase.axaml",
                stats => stats.FlyoutHostCount > 0),
            ["card"] = new(
                "CardShowCase",
                CardViewModel.ID,
                "AtomUIGallery.ShowCases.Views.CardShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/CardShowCase.axaml",
                stats => stats.CardCount > 0),
            ["groupbox"] = new(
                "GroupBoxShowCase",
                GroupBoxViewModel.ID,
                "AtomUIGallery.ShowCases.Views.GroupBoxShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/GroupBoxShowCase.axaml",
                stats => stats.GroupBoxCount >= 9),
            ["imagepreviewer"] = new(
                "ImagePreviewerShowCase",
                ImagePreviewerViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ImagePreviewerShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/ImagePreviewerShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 5 && stats.ImageCount + stats.SvgCount >= 5),
            ["collapse"] = new(
                "CollapseShowCase",
                CollapseViewModel.ID,
                "AtomUIGallery.ShowCases.Views.CollapseShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/CollapseShowCase.axaml",
                stats => stats.CollapseCount > 0),
            ["expander"] = new(
                "ExpanderShowCase",
                ExpanderViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ExpanderShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/ExpanderShowCase.axaml",
                stats => stats.VisualCount > 0),
            ["empty"] = new(
                "EmptyShowCase",
                EmptyViewModel.ID,
                "AtomUIGallery.ShowCases.Views.EmptyShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/EmptyShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 4 && stats.SvgCount >= 6),
            ["descriptions"] = new(
                "DescriptionsShowCase",
                DescriptionsViewModel.ID,
                "AtomUIGallery.ShowCases.Views.DescriptionsShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/DescriptionsShowCase.axaml",
                stats => stats.DescriptionsCount > 0),
            ["tour"] = new(
                "TourShowCase",
                TourViewModel.ID,
                "AtomUIGallery.ShowCases.Views.TourShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/TourShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 7 && stats.ButtonCount >= 20 && stats.VisualCount > 0),
            ["statistic"] = new(
                "StatisticShowCase",
                StatisticViewModel.ID,
                "AtomUIGallery.ShowCases.Views.StatisticShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/StatisticShowCase.axaml",
                stats => stats.StatisticCount >= 9 &&
                         stats.TimerStatisticCount >= 6 &&
                         stats.StatisticCountUpCount >= 2),
            ["timeline"] = new(
                "TimelineShowCase",
                TimelineViewModel.ID,
                "AtomUIGallery.ShowCases.Views.TimelineShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/TimelineShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 6 &&
                         stats.ButtonCount >= 1 &&
                         stats.RadioButtonCount >= 3),
            ["list"] = new(
                "ListShowCase",
                ListViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ListShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/ListShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 11 && stats.VisualCount > 0),
            ["modal"] = new(
                "ModalShowCase",
                ModalViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ModalShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Feedback/ModalShowCase.axaml",
                stats => stats.DialogCount > 0 || stats.MessageBoxCount > 0),
            ["message"] = new(
                "MessageShowCase",
                MessageViewModel.ID,
                "AtomUIGallery.ShowCases.Message.MessageShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 4 && stats.ButtonCount >= 7),
            ["notification"] = new(
                "NotificationShowCase",
                NotificationViewModel.ID,
                "AtomUIGallery.ShowCases.Notification.NotificationShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 6 && stats.ButtonCount >= 14),
            ["alert"] = new(
                "AlertShowCase",
                AlertViewModel.ID,
                "AtomUIGallery.ShowCases.Views.AlertShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Feedback/AlertShowCase.axaml",
                stats => stats.AlertCount > 0),
            ["skeleton"] = new(
                "SkeletonShowCase",
                SkeletonViewModel.ID,
                "AtomUIGallery.ShowCases.Views.SkeletonShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Feedback/SkeletonShowCase.axaml",
                stats => stats.SkeletonCount >= 4 && stats.SkeletonLineCount >= 6),
            ["spin"] = new(
                "SpinShowCase",
                SpinViewModel.ID,
                "AtomUIGallery.ShowCases.Views.SpinShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Feedback/SpinShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 5 && stats.AlertCount >= 3),
            ["popupconfirm"] = new(
                "PopupConfirmShowCase",
                PopupConfirmViewModel.ID,
                "AtomUIGallery.ShowCases.Views.PopupConfirmShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Feedback/PopupConfirmShowCase.axaml",
                stats => stats.FlyoutHostCount > 0),
            ["result"] = new(
                "ResultShowCase",
                ResultViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ResultShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Feedback/ResultShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 8 && stats.SvgCount >= 3 && stats.ButtonCount >= 10),
            ["progressbar"] = new(
                "ProgressBarShowCase",
                ProgressBarViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ProgressBarShowCase",
                "controlgallery/AtomUIGallery/ShowCases/ProgressBarShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 18 && stats.VisualCount > 0),
            ["qrcode"] = new(
                "QRCodeShowCase",
                QRCodeViewModel.ID,
                "AtomUIGallery.ShowCases.Views.QRCodeShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/QRCodeShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 8 && stats.VisualCount > 0),
            ["segmented"] = new(
                "SegmentedShowCase",
                SegmentedViewModel.ID,
                typeof(SegmentedShowCase).FullName!,
                "controlgallery/AtomUIGallery/ShowCases/DataDisplay/Segmented/Views/SegmentedShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 6 && stats.IconPresenterCount >= 10),
            ["drawer"] = new(
                "DrawerShowCase",
                DrawerViewModel.ID,
                "AtomUIGallery.ShowCases.Views.DrawerShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Feedback/DrawerShowCase.axaml",
                stats => stats.DrawerCount > 0),
            ["carousel"] = new(
                "CarouselShowCase",
                CarouselViewModel.ID,
                "AtomUIGallery.ShowCases.Views.CarouselShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/CarouselShowCase.axaml",
                stats => stats.CarouselCount > 0),
            ["calendar"] = new(
                "CalendarShowCase",
                CalendarViewModel.ID,
                "AtomUIGallery.ShowCases.Views.CalendarShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/CalendarShowCase.axaml",
                stats => stats.CalendarCount > 0 &&
                         stats.CalendarItemCount > 0 &&
                         stats.CalendarDayButtonCount >= 42),
            ["button"] = new(
                "ButtonShowCase",
                ButtonViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ButtonShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/General/ButtonShowCase.axaml",
                stats => stats.ButtonCount > 0),
            ["floatbutton"] = new(
                "FloatButtonShowCase",
                FloatButtonViewModel.ID,
                "AtomUIGallery.ShowCases.Views.FloatButtonShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/General/FloatButtonShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 10 && stats.IconPresenterCount > 0),
            ["dropdownbutton"] = new(
                "DropdownButtonShowCase",
                DropdownButtonViewModel.ID,
                "AtomUIGallery.ShowCases.Views.DropdownButtonShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Navigation/DropdownButtonShowCase.axaml",
                stats => stats.ButtonCount > 0),
            ["splitbutton"] = new(
                "SplitButtonShowCase",
                SplitButtonViewModel.ID,
                "AtomUIGallery.ShowCases.Views.SplitButtonShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/General/SplitButtonShowCase.axaml",
                stats => stats.ButtonCount > 0),
            ["buttonspinner"] = new(
                "ButtonSpinnerShowCase",
                ButtonSpinnerViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ButtonSpinnerShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Navigation/ButtonSpinnerShowCase.axaml",
                stats => stats.ButtonSpinnerCount > 0),
            ["breadcrumb"] = new(
                "BreadcrumbShowCase",
                BreadcrumbViewModel.ID,
                "AtomUIGallery.ShowCases.Views.BreadcrumbShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Navigation/BreadcrumbShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 6 && stats.IconPresenterCount >= 21),
            ["tabcontrol"] = new(
                "TabControlShowCase",
                TabControlViewModel.ID,
                typeof(TabControlShowCase).FullName!,
                "controlgallery/AtomUIGallery/ShowCases/Navigation/TabControl/Views/TabControlShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 10 && stats.VisualCount > 0),
            ["steps"] = new(
                "StepsShowCase",
                StepsViewModel.ID,
                "AtomUIGallery.ShowCases.Views.StepsShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Navigation/StepsShowCase.axaml",
                stats => stats.StepsCount >= 34 &&
                         stats.StepsItemCount >= 115 &&
                         stats.StepsItemIndicatorCount >= 115),
            ["combobox"] = new(
                "ComboBoxShowCase",
                ComboBoxViewModel.ID,
                "AtomUIGallery.ShowCases.ComboBox.ComboBoxShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox/Views/ComboBoxShowCase.axaml",
                stats => stats.ComboBoxCount > 0),
            ["pagination"] = new(
                "PaginationShowCase",
                PaginationViewModel.ID,
                "AtomUIGallery.ShowCases.Views.PaginationShowCase",
                "controlgallery/AtomUIGallery/ShowCases/PaginationShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 7 && stats.VisualCount > 0),
            ["space"] = new(
                "SpaceShowCase",
                SpaceViewModel.ID,
                "AtomUIGallery.ShowCases.Views.SpaceShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Layout/SpaceShowCase.axaml",
                stats => stats.SpaceCount > 0 || stats.CompactSpaceCount > 0),
            ["splitter"] = new(
                "SplitterShowCase",
                SplitterViewModel.ID,
                "AtomUIGallery.ShowCases.Views.SplitterShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Layout/SplitterShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 7 && stats.VisualCount > 0),
            ["select"] = new(
                "SelectShowCase",
                SelectViewModel.ID,
                "AtomUIGallery.ShowCases.Views.SelectShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/SelectShowCase.axaml",
                stats => stats.SelectCount > 0),
            ["numberupdown"] = new(
                "NumberUpDownShowCase",
                NumberUpDownViewModel.ID,
                "AtomUIGallery.ShowCases.Views.NumberUpDownShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/NumberUpDownShowCase.axaml",
                stats => stats.ButtonSpinnerCount >= 30 && stats.AddOnDecoratedBoxCount >= 30),
            ["autocomplete"] = new(
                "AutoCompleteShowCase",
                AutoCompleteViewModel.ID,
                "AtomUIGallery.ShowCases.Views.AutoCompleteShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/AutoCompleteShowCase.axaml",
                stats => stats.AutoCompleteCount > 0),
            ["mentions"] = new(
                "MentionsShowCase",
                MentionsViewModel.ID,
                "AtomUIGallery.ShowCases.Views.MentionsShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/MentionsShowCase.axaml",
                stats => stats.MentionsCount > 0),
            ["datepicker"] = new(
                "DatePickerShowCase",
                DatePickerViewModel.ID,
                "AtomUIGallery.ShowCases.DatePicker.DatePickerShowCase",
                "controlgallery/AtomUIGallery/ShowCases/DataEntry/DatePicker/Views/DatePickerShowCase.axaml",
                stats => stats.DatePickerCount > 0 || stats.RangeDatePickerCount > 0),
            ["timepicker"] = new(
                "TimePickerShowCase",
                TimePickerViewModel.ID,
                "AtomUIGallery.ShowCases.Views.TimePickerShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/TimePickerShowCase.axaml",
                stats => stats.InfoPickerInputCount > 0),
            ["colorpicker"] = new(
                "ColorPickerShowCase",
                ColorPickerViewModel.ID,
                "AtomUIGallery.ShowCases.Views.ColorPickerShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/ColorPickerShowCase.axaml",
                stats => stats.ColorPickerCount >= 20 && stats.GradientColorPickerCount >= 3),
            ["form"] = new(
                "FormShowCase",
                FormViewModel.ID,
                "AtomUIGallery.ShowCases.Views.FormShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/FormShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 2 && stats.LineEditCount >= 3),
            ["treeselect"] = new(
                "TreeSelectShowCase",
                TreeSelectViewModel.ID,
                "AtomUIGallery.ShowCases.Views.TreeSelectShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/TreeSelectShowCase.axaml",
                stats => stats.TreeSelectCount > 0),
            ["cascader"] = new(
                "CascaderShowCase",
                CascaderViewModel.ID,
                "AtomUIGallery.ShowCases.Views.CascaderShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/CascaderShowCase.axaml",
                stats => stats.CascaderCount > 0),
            ["checkbox"] = new(
                "CheckBoxShowCase",
                CheckBoxViewModel.ID,
                "AtomUIGallery.ShowCases.Views.CheckBoxShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/CheckBoxShowCase.axaml",
                stats => stats.CheckBoxCount > 0),
            ["radiobutton"] = new(
                "RadioButtonShowCase",
                RadioButtonViewModel.ID,
                "AtomUIGallery.ShowCases.Views.RadioButtonShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/RadioButtonShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 8 && stats.RadioButtonCount > 0),
            ["toggleswitch"] = new(
                "ToggleSwitchShowCase",
                ToggleSwitchViewModel.ID,
                typeof(ToggleSwitchShowCase).FullName!,
                "controlgallery/AtomUIGallery/ShowCases/DataEntry/ToggleSwitch/Views/ToggleSwitchShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 5 && stats.ButtonCount >= 2 && stats.VisualCount > 0),
            ["rate"] = new(
                "RateShowCase",
                RateViewModel.ID,
                "AtomUIGallery.ShowCases.Views.RateShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/RateShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 6 && stats.VisualCount > 0),
            ["slider"] = new(
                "SliderShowCase",
                SliderViewModel.ID,
                "AtomUIGallery.ShowCases.Views.SliderShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/SliderShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 4 && stats.VisualCount > 0),
            ["datagrid"] = new(
                "DataGridShowCase",
                DataGridViewModel.ID,
                "AtomUIGallery.ShowCases.Views.DataGridShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/DataGridShowCase.axaml",
                stats => stats.VisualCount > 0),
            ["treeview"] = new(
                "TreeViewShowCase",
                TreeViewViewModel.ID,
                "AtomUIGallery.ShowCases.Views.TreeViewShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataDisplay/TreeViewShowCase.axaml",
                stats => stats.VisualCount > 0),
            ["transfer"] = new(
                "TransferShowCase",
                TransferViewModel.ID,
                "AtomUIGallery.ShowCases.Views.TransferShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/TransferShowCase.axaml",
                stats => stats.VisualCount > 0),
            ["upload"] = new(
                "UploadShowCase",
                UploadViewModel.ID,
                "AtomUIGallery.ShowCases.Views.UploadShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/DataEntry/UploadShowCase.axaml",
                stats => stats.ShowCaseItemCount >= 10 && stats.VisualCount > 0),
            ["menu"] = new(
                "MenuShowCase",
                MenuViewModel.ID,
                "AtomUIGallery.ShowCases.Views.MenuShowCase",
                "controlgallery/AtomUIGallery/ShowCases/Views/Navigation/MenuShowCase.axaml",
                stats => stats.MenuItemCount > 0 &&
                         stats.NavMenuItemHeaderCount >= 42 &&
                         stats.MotionTargetCount >= 30)
        };

    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Contains("--native-motion", StringComparer.Ordinal))
        {
            return NativeMotionProbe.Run(args);
        }

        var options = PerfOptions.Parse(args);
        if (options.FeedbackStack)
        {
            try
            {
                SetupAvalonia(out var feedbackLifetime);
                feedbackLifetime.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var feedbackOutput = FeedbackStackPerformanceProbe.Run(
                    options.Iterations,
                    options.Warmup,
                    options.Label);
                Console.WriteLine(feedbackOutput);
                WriteMarkdownOutput(feedbackOutput, options);
                feedbackLifetime.Shutdown();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        if (!ShowCases.TryGetValue(options.ShowCase, out var showCase))
        {
            Console.Error.WriteLine($"Unknown showcase '{options.ShowCase}'. Available: {string.Join(", ", ShowCases.Keys)}.");
            return 1;
        }

        try
        {
            if (options.SpaceItems)
            {
                SetupAvalonia(out _);
                var itemOutput = RunSpaceShowCaseItemBreakdown(options);
                Console.WriteLine(itemOutput);
                WriteMarkdownOutput(itemOutput, options);
                return 0;
            }

            if (options.ColdChild)
            {
                return RunColdChild(options, showCase);
            }

            var coldRuns = !options.TraceNavigation && options.ColdIterations > 1
                ? RunColdIterations(options)
                : null;

            SetupAvalonia(out var lifetime);

            if (lifetime.MainWindow is not WorkspaceWindow window)
            {
                Console.Error.WriteLine("Gallery workspace window was not created.");
                return 1;
            }

            window.ShowInTaskbar = false;
            window.Width         = WindowSize.Width;
            window.Height        = WindowSize.Height;
            window.Show();

            WaitForRoute(window, Overview, options.Timeout);

            if (options.ResizeTrace)
            {
                TriggerNavigation(window, showCase);
                var route = WaitForRoute(window, showCase, options.Timeout);
                var resizeResult = GalleryResizeProbe.Run(
                    window,
                    route,
                    new GalleryResizeProbeOptions(
                        1300,
                        1728,
                        1,
                        options.Warmup,
                        options.Iterations,
                        options.ResizeForceMaterialized,
                        options.Label));
                var resizeOutput = GalleryResizeProbe.RenderMarkdown(resizeResult);
                Console.WriteLine(resizeOutput);
                WriteMarkdownOutput(resizeOutput, options);
                window.Close();
                Dispatcher.UIThread.RunJobs();
                return 0;
            }

            if (options.TraceNavigation)
            {
                var traceOutput = RunNavigationTrace(window, options, showCase);
                Console.WriteLine(traceOutput);
                WriteMarkdownOutput(traceOutput, options);
                window.Close();
                Dispatcher.UIThread.RunJobs();
                return 0;
            }

            if (coldRuns is null)
            {
                coldRuns =
                [
                    MeasureNavigation(window, 0, "Cold", options, showCase)
                ];
                NavigateToOverview(window, options);
            }
            else
            {
                _ = MeasureNavigation(window, 0, "Priming", options, showCase);
                NavigateToOverview(window, options);
            }

            for (var i = 0; i < options.Warmup; i++)
            {
                _ = MeasureNavigation(window, i + 1, "Warmup", options, showCase);
                NavigateToOverview(window, options);
            }

            var samples = new List<NavigationSample>(options.Iterations);
            for (var i = 0; i < options.Iterations; i++)
            {
                samples.Add(MeasureNavigation(window, i + 1, "Measured", options, showCase));
                NavigateToOverview(window, options);
            }

            var result = NavigationResult.Create(options.Label, coldRuns, samples);
            var output = RenderResult(result, options, showCase);
            Console.WriteLine(output);

            if (!string.IsNullOrWhiteSpace(options.MarkdownOutputPath))
            {
                WriteMarkdownOutput(output, options);
            }

            window.Close();
            Dispatcher.UIThread.RunJobs();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void SetupAvalonia(out ClassicDesktopStyleApplicationLifetime lifetime)
    {
        lifetime = new ClassicDesktopStyleApplicationLifetime
        {
            Args = []
        };

        AppBuilder.Configure<GalleryApplication>()
                  .UseReactiveUI(build =>
                      build.ConfigureViewLocator(locator => new ShowCaseViewModule().RegisterViews(locator)))
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .WithAtomUIDefaultOptions()
                  .UseAtomUI(GalleryApplication.Configure)
                  .SetupWithLifetime(lifetime);
        // The public startup flow schedules the main window after framework setup.
        Dispatcher.UIThread.RunJobs();
    }

    private static int RunColdChild(PerfOptions options, ShowCaseSpec showCase)
    {
        SetupAvalonia(out var lifetime);
        if (lifetime.MainWindow is not WorkspaceWindow window)
        {
            Console.Error.WriteLine("Gallery workspace window was not created.");
            return 1;
        }

        window.ShowInTaskbar = false;
        window.Width         = WindowSize.Width;
        window.Height        = WindowSize.Height;
        window.Show();

        WaitForRoute(window, Overview, options.Timeout);
        var sample = MeasureNavigation(window, options.ColdChildIteration, "Cold", options, showCase);
        var dto    = ColdChildSampleDto.FromSample(sample);
        Console.WriteLine(ColdChildSamplePrefix + JsonSerializer.Serialize(dto));

        window.Close();
        Dispatcher.UIThread.RunJobs();
        return 0;
    }

    private static IReadOnlyList<NavigationSample> RunColdIterations(PerfOptions options)
    {
        var samples = new List<NavigationSample>(options.ColdIterations);
        for (var i = 0; i < options.ColdIterations; i++)
        {
            samples.Add(RunColdIterationProcess(options, i + 1));
        }
        return samples;
    }

    private static NavigationSample RunColdIterationProcess(PerfOptions options, int iteration)
    {
        var assemblyPath = Assembly.GetEntryAssembly()?.Location;
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            throw new InvalidOperationException("Unable to resolve GalleryPerformance assembly path.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName               = ResolveDotnetHostPath(),
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            WorkingDirectory       = Environment.CurrentDirectory
        };
        startInfo.ArgumentList.Add(assemblyPath);
        startInfo.ArgumentList.Add("--cold-child");
        startInfo.ArgumentList.Add("--cold-child-iteration");
        startInfo.ArgumentList.Add(iteration.ToString(CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--showcase");
        startInfo.ArgumentList.Add(options.ShowCase);
        startInfo.ArgumentList.Add("--timeout-ms");
        startInfo.ArgumentList.Add(((int)options.Timeout.TotalMilliseconds).ToString(CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--label");
        startInfo.ArgumentList.Add(options.Label);
        if (options.SpaceRemoveItem is { } spaceRemoveItem)
        {
            startInfo.ArgumentList.Add("--space-remove-item");
            startInfo.ArgumentList.Add(spaceRemoveItem.ToString(CultureInfo.InvariantCulture));
        }

        using var process = Process.Start(startInfo)
                            ?? throw new InvalidOperationException("Unable to start cold navigation child process.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Cold navigation child process failed with exit code {process.ExitCode}.{Environment.NewLine}{stderr}{Environment.NewLine}{stdout}");
        }

        var sampleLine = stdout.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                               .FirstOrDefault(line => line.StartsWith(ColdChildSamplePrefix, StringComparison.Ordinal));
        if (sampleLine is null)
        {
            throw new InvalidOperationException(
                $"Cold navigation child process did not emit a sample line.{Environment.NewLine}{stderr}{Environment.NewLine}{stdout}");
        }

        var json = sampleLine[ColdChildSamplePrefix.Length..];
        var dto  = JsonSerializer.Deserialize<ColdChildSampleDto>(json)
                   ?? throw new InvalidOperationException("Unable to parse cold navigation child sample.");
        return dto.ToSample();
    }

    private static string ResolveDotnetHostPath()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } dotnetHostPath)
        {
            return dotnetHostPath;
        }
        if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is { Length: > 0 } dotnetRoot)
        {
            return Path.Combine(dotnetRoot, "dotnet");
        }
        return "dotnet";
    }

    private static string RunNavigationTrace(WorkspaceWindow window, PerfOptions options, ShowCaseSpec showCase)
    {
        var samples = new List<NavigationTraceSample>
        {
            MeasureNavigationTrace(window, 0, "Cold", options, showCase)
        };
        NavigateToOverview(window, options);
        samples.Add(MeasureNavigationTrace(window, 1, "Second", options, showCase));

        var builder = new StringBuilder();
        builder.AppendLine($"# {showCase.Label} navigation trace - {options.Label}");
        builder.AppendLine();
        builder.AppendLine($"- Timestamp: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine($"- Configuration: {BuildConfiguration}, headless, {WindowSize.Width:0}x{WindowSize.Height:0} window");
        builder.AppendLine("- Measurement: Overview route settled -> trigger navigation -> route visual tree and layout stable");
        builder.AppendLine();
        builder.AppendLine("| Phase | Trigger | Total ms | Trigger ms | First found ms | First ready ms | Stable ms | Pump count | Pump total ms | Max pump ms | Stats count | Stats total ms | Scan total ms | Alloc KB | Visuals | Alert | MarqueeLabel | AddOnDecoratedBox | CompactSpace | CompactSpaceItem | LineEdit | Button | ButtonSpinner | Select | AutoComplete | AC popup fields | AC candidate fields | CandidateList visuals | TreeSelect | Cascader | CheckBox | CheckBoxGroup | CheckBoxIndicator | Collapse | CollapseItem | Collapse content motion | Collapse expand button |");
        builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var sample in samples)
        {
            builder.AppendLine(
                $"| {sample.Phase} | {sample.Trigger} | {FormatMs(sample.Total)} | {FormatMs(sample.TriggerElapsed)} | {FormatOptionalMs(sample.FirstFoundElapsed)} | {FormatOptionalMs(sample.FirstReadyElapsed)} | {FormatMs(sample.StableElapsed)} | {sample.PumpCount} | {FormatMs(sample.PumpTotal)} | {FormatMs(sample.MaxPump)} | {sample.StatsCount} | {FormatMs(sample.StatsTotal)} | {FormatMs(sample.ScanTotal)} | {FormatKb(sample.AllocatedBytes)} | {sample.Stats.VisualCount} | {sample.Stats.AlertCount} | {sample.Stats.MarqueeLabelCount} | {sample.Stats.AddOnDecoratedBoxCount} | {sample.Stats.CompactSpaceCount} | {sample.Stats.CompactSpaceItemCount} | {sample.Stats.LineEditCount} | {sample.Stats.ButtonCount} | {sample.Stats.ButtonSpinnerCount} | {sample.Stats.SelectCount} | {sample.Stats.AutoCompleteCount} | {sample.Stats.AutoCompletePopupFieldCount} | {sample.Stats.AutoCompleteCandidateListFieldCount} | {sample.Stats.CandidateListCount} | {sample.Stats.TreeSelectCount} | {sample.Stats.CascaderCount} | {sample.Stats.CheckBoxCount} | {sample.Stats.CheckBoxGroupCount} | {sample.Stats.CheckBoxIndicatorCount} | {sample.Stats.CollapseCount} | {sample.Stats.CollapseItemCount} | {sample.Stats.CollapseContentViewportCount} | {sample.Stats.CollapseExpandButtonCount} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Shape Events");
        builder.AppendLine();
        foreach (var sample in samples)
        {
            builder.AppendLine($"### {sample.Phase}");
            builder.AppendLine();
            foreach (var item in sample.Events)
            {
                builder.AppendLine($"- {item}");
            }
            builder.AppendLine();
        }
        return builder.ToString();
    }

    private static NavigationTraceSample MeasureNavigationTrace(WorkspaceWindow window,
                                                               int iteration,
                                                               string phase,
                                                               PerfOptions options,
                                                               ShowCaseSpec showCase)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var totalStopwatch  = Stopwatch.StartNew();
        var trigger         = TriggerNavigation(window, showCase);
        ApplyNavigationVariant(window, options);
        var triggerElapsed  = totalStopwatch.Elapsed;
        var trace           = WaitForRouteTrace(window, showCase, options.Timeout, totalStopwatch);
        totalStopwatch.Stop();

        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        return new NavigationTraceSample(
            iteration,
            phase,
            trigger,
            totalStopwatch.Elapsed,
            triggerElapsed,
            trace.FirstFoundElapsed,
            trace.FirstReadyElapsed,
            trace.StableElapsed,
            trace.PumpCount,
            trace.PumpTotal,
            trace.MaxPump,
            trace.StatsCount,
            trace.StatsTotal,
            trace.ScanTotal,
            allocatedBytes,
            trace.Stats,
            trace.Events);
    }

    private static void WriteMarkdownOutput(string output, PerfOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.MarkdownOutputPath))
        {
            return;
        }

        var fullPath = Path.GetFullPath(options.MarkdownOutputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        Console.WriteLine();
        Console.WriteLine($"Wrote markdown result: {fullPath}");
    }

    private static NavigationSample MeasureNavigation(WorkspaceWindow window,
                                                       int iteration,
                                                       string phase,
                                                       PerfOptions options,
                                                       ShowCaseSpec showCase)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch       = Stopwatch.StartNew();
        var trigger         = TriggerNavigation(window, showCase);
        ApplyNavigationVariant(window, options);
        var route           = WaitForRoute(window, showCase, options.Timeout);
        stopwatch.Stop();

        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        var stats          = RouteStats.Collect(route);
        return new NavigationSample(iteration, phase, trigger, stopwatch.Elapsed, allocatedBytes, stats);
    }

    private static string TriggerNavigation(WorkspaceWindow window, ShowCaseSpec showCase)
    {
        var navMenu = window.GetSelfAndVisualDescendants().OfType<NavMenu>().FirstOrDefault();
        var navItem = window.GetSelfAndVisualDescendants()
                            .OfType<INavMenuItem>()
                            .Concat(window.GetSelfAndLogicalDescendants().OfType<INavMenuItem>())
                            .FirstOrDefault(item => item.ItemKey.HasValue &&
                                                    item.ItemKey.Value == showCase.Key);

        if (navMenu is not null && navItem is not null)
        {
            navMenu.RaiseEvent(new NavMenuItemClickEventArgs(NavMenu.NavMenuItemClickEvent, navItem));
            return "NavMenuItemClick";
        }

        ExecuteNavigateCommand(window, showCase.Key);
        return "NavigateToCommand";
    }

    private static void ApplyNavigationVariant(WorkspaceWindow window, PerfOptions options)
    {
        if (options.SpaceRemoveItem is not { } itemNumber)
        {
            return;
        }

        var route = window.GetSelfAndVisualDescendants()
                          .OfType<SpaceShowCase>()
                          .FirstOrDefault() ??
                    window.GetSelfAndLogicalDescendants()
                          .OfType<SpaceShowCase>()
                          .FirstOrDefault();
        if (route is null)
        {
            return;
        }

        var panel = GetSpaceShowCasePanel(route);
        var items = panel.Children.OfType<ShowCaseItem>().Where(item => !item.IsFake).ToList();
        var index = itemNumber - 1;
        if (index < 0 || index >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(options.SpaceRemoveItem),
                $"SpaceShowCase item {itemNumber} is out of range. Count={items.Count}.");
        }
        panel.Children.Remove(items[index]);
    }

    private static void NavigateToOverview(WorkspaceWindow window, PerfOptions options)
    {
        ExecuteNavigateCommand(window, OverviewViewModel.ID);
        WaitForRoute(window, Overview, options.Timeout);
    }

    private static void ExecuteNavigateCommand(WorkspaceWindow window, EntityKey key)
    {
        var error = default(Exception);
        using var subscription = window.ViewModel!.CaseNavigation.NavigateToCommand
                                      .Execute(key)
                                      .Subscribe(_ => { }, ex => error = ex);
        if (error is not null)
        {
            throw error;
        }
    }

    private static Control WaitForRoute(WorkspaceWindow window, ShowCaseSpec showCase, TimeSpan timeout)
    {
        var stopwatch          = Stopwatch.StartNew();
        var stableLayoutPasses = 0;
        var previousStats      = default(RouteStats);
        Control? route         = null;

        while (stopwatch.Elapsed < timeout)
        {
            PumpLayout(window);

            route = window.GetSelfAndVisualDescendants()
                          .OfType<Control>()
                          .FirstOrDefault(control => control.GetType().FullName == showCase.RouteTypeName);

            if (route is not null && route.IsVisible && route.Bounds.Width > 0 && route.Bounds.Height > 0)
            {
                var currentStats = RouteStats.Collect(route);
                if (currentStats.IsDisplayReady(showCase) &&
                    previousStats is not null &&
                    currentStats.HasSameShape(previousStats))
                {
                    stableLayoutPasses++;
                    if (stableLayoutPasses >= 2)
                    {
                        return route;
                    }
                }
                else
                {
                    stableLayoutPasses = 0;
                    previousStats      = currentStats;
                }
            }
        }

        var routeLabel = route is null
            ? "route was not found"
            : $"route was found but did not stabilize, bounds={route.Bounds}";
        throw new TimeoutException($"Timed out waiting for {showCase.RouteTypeName}: {routeLabel}.");
    }

    private static NavigationTraceData WaitForRouteTrace(WorkspaceWindow window,
                                                         ShowCaseSpec showCase,
                                                         TimeSpan timeout,
                                                         Stopwatch totalStopwatch)
    {
        var stableLayoutPasses = 0;
        var previousStats      = default(RouteStats);
        var events             = new List<string>();
        var pumpCount          = 0;
        var statsCount         = 0;
        var pumpTotal          = TimeSpan.Zero;
        var maxPump            = TimeSpan.Zero;
        var statsTotal         = TimeSpan.Zero;
        var scanTotal          = TimeSpan.Zero;
        var firstFoundElapsed  = default(TimeSpan?);
        var firstReadyElapsed  = default(TimeSpan?);
        var latestStats        = default(RouteStats);

        while (totalStopwatch.Elapsed < timeout)
        {
            var pumpStopwatch = Stopwatch.StartNew();
            PumpLayout(window);
            pumpStopwatch.Stop();
            pumpCount++;
            pumpTotal += pumpStopwatch.Elapsed;
            if (pumpStopwatch.Elapsed > maxPump)
            {
                maxPump = pumpStopwatch.Elapsed;
            }

            var scanStopwatch = Stopwatch.StartNew();
            var route = window.GetSelfAndVisualDescendants()
                              .OfType<Control>()
                              .FirstOrDefault(control => control.GetType().FullName == showCase.RouteTypeName);
            scanStopwatch.Stop();
            scanTotal += scanStopwatch.Elapsed;

            if (route is null)
            {
                continue;
            }

            if (firstFoundElapsed is null)
            {
                firstFoundElapsed = totalStopwatch.Elapsed;
                events.Add($"+{FormatMs(firstFoundElapsed.Value)} first route found");
            }

            if (!route.IsVisible || route.Bounds.Width <= 0 || route.Bounds.Height <= 0)
            {
                continue;
            }

            var statsStopwatch = Stopwatch.StartNew();
            var currentStats   = RouteStats.Collect(route);
            statsStopwatch.Stop();
            statsCount++;
            statsTotal += statsStopwatch.Elapsed;
            latestStats = currentStats;

            if (currentStats.IsDisplayReady(showCase) && firstReadyElapsed is null)
            {
                firstReadyElapsed = totalStopwatch.Elapsed;
                events.Add($"+{FormatMs(firstReadyElapsed.Value)} first display-ready shape: {DescribeStats(currentStats)}");
            }

            if (previousStats is null || !currentStats.HasSameShape(previousStats))
            {
                events.Add($"+{FormatMs(totalStopwatch.Elapsed)} shape changed: {DescribeStats(currentStats)}");
                stableLayoutPasses = 0;
                previousStats      = currentStats;
                continue;
            }

            if (currentStats.IsDisplayReady(showCase))
            {
                stableLayoutPasses++;
                events.Add($"+{FormatMs(totalStopwatch.Elapsed)} stable pass {stableLayoutPasses}: {DescribeStats(currentStats)}");
                if (stableLayoutPasses >= 2)
                {
                    return new NavigationTraceData(
                        firstFoundElapsed,
                        firstReadyElapsed,
                        totalStopwatch.Elapsed,
                        pumpCount,
                        pumpTotal,
                        maxPump,
                        statsCount,
                        statsTotal,
                        scanTotal,
                        currentStats,
                        events);
                }
            }
        }

        throw new TimeoutException(
            $"Timed out waiting for {showCase.RouteTypeName}: latest={DescribeStats(latestStats)}.");
    }

    private static void PumpLayout(Avalonia.Controls.Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.Measure(WindowSize);
        window.Arrange(WindowBounds);
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static string RunSpaceShowCaseItemBreakdown(PerfOptions options)
    {
        var itemInfos = GetSpaceShowCaseItemInfos();
        var samples = new List<SpaceShowCaseItemSample>();
        foreach (var itemInfo in itemInfos)
        {
            for (var i = 0; i < options.Warmup; i++)
            {
                _ = MeasureSpaceShowCaseItem(itemInfo, i + 1, "Warmup", options);
            }

            for (var i = 0; i < options.Iterations; i++)
            {
                samples.Add(MeasureSpaceShowCaseItem(itemInfo, i + 1, "Measured", options));
            }
        }

        return RenderSpaceShowCaseItemBreakdown(samples, options);
    }

    private static IReadOnlyList<SpaceShowCaseItemInfo> GetSpaceShowCaseItemInfos()
    {
        var view  = CreateSpaceShowCase();
        var panel = GetSpaceShowCasePanel(view);
        return panel.Children
                    .OfType<ShowCaseItem>()
                    .Where(item => !item.IsFake)
                    .Select((item, index) => new SpaceShowCaseItemInfo(
                        index,
                        string.IsNullOrWhiteSpace(item.Title) ? $"Item {index + 1}" : item.Title,
                        string.IsNullOrWhiteSpace(item.Description) ? string.Empty : item.Description))
                    .ToList();
    }

    private static SpaceShowCaseItemSample MeasureSpaceShowCaseItem(SpaceShowCaseItemInfo itemInfo,
                                                                    int iteration,
                                                                    string phase,
                                                                    PerfOptions options)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var view   = CreateSpaceShowCase();
        var panel  = GetSpaceShowCasePanel(view);
        var items  = panel.Children.OfType<ShowCaseItem>().Where(item => !item.IsFake).ToList();
        var target = items[itemInfo.Index];
        for (var i = panel.Children.Count - 1; i >= 0; i--)
        {
            if (!ReferenceEquals(panel.Children[i], target))
            {
                panel.Children.RemoveAt(i);
            }
        }
        if (options.SpaceItemsWithoutTreeCascader ||
            options.SpaceItemsWithoutTreeCascaderSelect)
        {
            RemoveSpaceItemVariantControls(target, options.SpaceItemsWithoutTreeCascaderSelect);
        }

        var window = new Avalonia.Controls.Window
        {
            Width         = WindowSize.Width,
            Height        = WindowSize.Height,
            Content       = view,
            ShowInTaskbar = false
        };

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch       = Stopwatch.StartNew();
        window.Show();
        WaitForStableControl(window, target, options.Timeout);
        stopwatch.Stop();

        var allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        var stats          = RouteStats.Collect(target);
        window.Close();
        Dispatcher.UIThread.RunJobs();

        return new SpaceShowCaseItemSample(itemInfo, iteration, phase, stopwatch.Elapsed, allocatedBytes, stats);
    }

    private static void RemoveSpaceItemVariantControls(Control root, bool includeSelect)
    {
        RemoveMatchingChildren(root, control =>
        {
            var typeName = control.GetType().FullName;
            return typeName is "AtomUI.Desktop.Controls.TreeSelect" or
                               "AtomUI.Desktop.Controls.Cascader" ||
                   includeSelect && typeName == "AtomUI.Desktop.Controls.Select";
        });
    }

    private static void RemoveMatchingChildren(Control control, Func<Control, bool> shouldRemove)
    {
        if (control is Panel panel)
        {
            for (var i = panel.Children.Count - 1; i >= 0; i--)
            {
                if (panel.Children[i] is Control child)
                {
                    if (shouldRemove(child))
                    {
                        panel.Children.RemoveAt(i);
                    }
                    else
                    {
                        RemoveMatchingChildren(child, shouldRemove);
                    }
                }
            }
        }

        if (control is Space space)
        {
            for (var i = space.Children.Count - 1; i >= 0; i--)
            {
                var child = space.Children[i];
                if (shouldRemove(child))
                {
                    space.Children.RemoveAt(i);
                }
                else
                {
                    RemoveMatchingChildren(child, shouldRemove);
                }
            }
        }

        if (control is CompactSpace compactSpace)
        {
            for (var i = compactSpace.Children.Count - 1; i >= 0; i--)
            {
                var child = compactSpace.Children[i];
                if (shouldRemove(child))
                {
                    compactSpace.Children.RemoveAt(i);
                }
                else
                {
                    RemoveMatchingChildren(child, shouldRemove);
                }
            }
        }

        if (control is ContentControl contentControl &&
            contentControl.Content is Control content)
        {
            if (shouldRemove(content))
            {
                contentControl.Content = null;
            }
            else
            {
                RemoveMatchingChildren(content, shouldRemove);
            }
        }
    }

    private static SpaceShowCase CreateSpaceShowCase()
    {
        var viewModel = new SpaceViewModel(new ProbeScreen())
        {
            SizeType           = CustomizableSizeType.Small,
            CustomSpacingValue = 24
        };
        return new SpaceShowCase
        {
            DataContext = viewModel
        };
    }

    private static ShowCasePanel GetSpaceShowCasePanel(SpaceShowCase view)
    {
        if (view.Content is ShowCasePanel panel)
        {
            return panel;
        }
        throw new InvalidOperationException("SpaceShowCase root content is not ShowCasePanel.");
    }

    private static void WaitForStableControl(Avalonia.Controls.Window window, Control target, TimeSpan timeout)
    {
        var stopwatch          = Stopwatch.StartNew();
        var stableLayoutPasses = 0;
        var previousStats      = default(RouteStats);

        while (stopwatch.Elapsed < timeout)
        {
            PumpLayout(window);

            if (target.IsVisible && target.Bounds.Width > 0 && target.Bounds.Height > 0)
            {
                var currentStats = RouteStats.Collect(target);
                if (previousStats is not null &&
                    currentStats.HasSameShape(previousStats))
                {
                    stableLayoutPasses++;
                    if (stableLayoutPasses >= 2)
                    {
                        return;
                    }
                }
                else
                {
                    stableLayoutPasses = 0;
                    previousStats      = currentStats;
                }
            }
        }

        throw new TimeoutException($"Timed out waiting for SpaceShowCase item layout: {target}.");
    }

    private static string RenderSpaceShowCaseItemBreakdown(IReadOnlyList<SpaceShowCaseItemSample> samples,
                                                           PerfOptions options)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# SpaceShowCase item performance breakdown");
        builder.AppendLine();
        builder.AppendLine($"- Timestamp: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine($"- Configuration: {BuildConfiguration}, headless, {WindowSize.Width.ToString(CultureInfo.InvariantCulture)}x{WindowSize.Height.ToString(CultureInfo.InvariantCulture)} window");
        builder.AppendLine("- Measurement: construct the real SpaceShowCase, keep one real ShowCaseItem before ShowCasePanel template/layout, then time attach/template/layout until stable.");
        if (options.SpaceItemsWithoutTreeCascaderSelect)
        {
            builder.AppendLine("- Variant: `TreeSelect`, `Cascader`, and exact `Select` controls are removed from each item content tree before attach/layout.");
        }
        else if (options.SpaceItemsWithoutTreeCascader)
        {
            builder.AppendLine("- Variant: `TreeSelect` and `Cascader` controls are removed from each item content tree before attach/layout.");
        }
        builder.AppendLine($"- Warmup per item: {options.Warmup}, measured iterations per item: {options.Iterations}, timeout: {options.Timeout.TotalSeconds.ToString("0.#", CultureInfo.InvariantCulture)}s");
        builder.AppendLine();
        builder.AppendLine("| # | Title | Description | Mean ms | Median ms | P95 ms | Min ms | Max ms | Alloc KB mean | Visuals | Logical | Space | CompactSpace | CompactSpaceItem | LineEdit total | LineEdit direct | SearchEdit | TextArea | Button | Select | TreeSelect | Cascader | Menu | MenuItem | AddOnDecoratedBox |");
        builder.AppendLine("| ---: | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var group in samples.GroupBy(sample => sample.Item).OrderBy(group => group.Key.Index))
        {
            var ordered = group.Select(sample => sample.Elapsed.TotalMilliseconds).Order().ToArray();
            var stats   = group.Last().Stats;
            builder.AppendLine(string.Join(" | ",
                "| " + (group.Key.Index + 1).ToString(CultureInfo.InvariantCulture),
                EscapeCell(group.Key.Title),
                EscapeCell(group.Key.Description),
                Format(ordered.Average()),
                Format(Percentile(ordered, 0.50)),
                Format(Percentile(ordered, 0.95)),
                Format(ordered.First()),
                Format(ordered.Last()),
                Format(group.Average(sample => sample.AllocatedBytes / 1024.0)),
                stats.VisualCount.ToString(CultureInfo.InvariantCulture),
                stats.LogicalCount.ToString(CultureInfo.InvariantCulture),
                stats.SpaceCount.ToString(CultureInfo.InvariantCulture),
                stats.CompactSpaceCount.ToString(CultureInfo.InvariantCulture),
                stats.CompactSpaceItemCount.ToString(CultureInfo.InvariantCulture),
                stats.LineEditCount.ToString(CultureInfo.InvariantCulture),
                stats.LineEditDirectCount.ToString(CultureInfo.InvariantCulture),
                stats.SearchEditCount.ToString(CultureInfo.InvariantCulture),
                stats.TextAreaCount.ToString(CultureInfo.InvariantCulture),
                stats.ButtonCount.ToString(CultureInfo.InvariantCulture),
                stats.SelectCount.ToString(CultureInfo.InvariantCulture),
                stats.TreeSelectCount.ToString(CultureInfo.InvariantCulture),
                stats.CascaderCount.ToString(CultureInfo.InvariantCulture),
                stats.MenuCount.ToString(CultureInfo.InvariantCulture),
                stats.MenuItemCount.ToString(CultureInfo.InvariantCulture),
                stats.AddOnDecoratedBoxCount.ToString(CultureInfo.InvariantCulture) + " |"));
        }

        builder.AppendLine();
        builder.AppendLine("## Samples");
        builder.AppendLine();
        builder.AppendLine("| # | Title | Iteration | Elapsed ms | Alloc KB | Visuals | Logical | Space | CompactSpace | CompactSpaceItem | LineEdit total | SearchEdit | Button | Select | TreeSelect | Cascader | AddOnDecoratedBox |");
        builder.AppendLine("| ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var sample in samples.OrderBy(sample => sample.Item.Index).ThenBy(sample => sample.Iteration))
        {
            builder.AppendLine(string.Join(" | ",
                "| " + (sample.Item.Index + 1).ToString(CultureInfo.InvariantCulture),
                EscapeCell(sample.Item.Title),
                sample.Iteration.ToString(CultureInfo.InvariantCulture),
                Format(sample.Elapsed.TotalMilliseconds),
                Format(sample.AllocatedBytes / 1024.0),
                sample.Stats.VisualCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.LogicalCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.SpaceCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.CompactSpaceCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.CompactSpaceItemCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.LineEditCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.SearchEditCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.ButtonCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.SelectCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.TreeSelectCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.CascaderCount.ToString(CultureInfo.InvariantCulture),
                sample.Stats.AddOnDecoratedBoxCount.ToString(CultureInfo.InvariantCulture) + " |"));
        }
        return builder.ToString();
    }

    private static string EscapeCell(string value)
    {
        return value.Replace("|", "\\|", StringComparison.Ordinal)
                    .ReplaceLineEndings(" ");
    }

    private static string RenderResult(NavigationResult result, PerfOptions options, ShowCaseSpec showCase)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {showCase.Label} navigation performance - {result.Label}");
        builder.AppendLine();
        builder.AppendLine($"- Timestamp: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}");
        builder.AppendLine($"- Configuration: {BuildConfiguration}, headless, {WindowSize.Width.ToString(CultureInfo.InvariantCulture)}x{WindowSize.Height.ToString(CultureInfo.InvariantCulture)} window");
        builder.AppendLine($"- Measurement: Overview route settled -> trigger {showCase.Label} navigation -> route visual tree and layout stable");
        builder.AppendLine($"- Route type: `{showCase.RouteTypeName}`");
        builder.AppendLine($"- XAML source: `{Path.GetFullPath(showCase.XamlPath)}`");
        builder.AppendLine($"- Cold first navigation samples: {result.ColdRuns.Count}");
        builder.AppendLine($"- Warmup: {options.Warmup}, measured iterations: {options.Iterations}, timeout: {options.Timeout.TotalSeconds.ToString("0.#", CultureInfo.InvariantCulture)}s");
        builder.AppendLine();
        builder.AppendLine("## Gallery source shape");
        builder.AppendLine();
        builder.AppendLine(SourceXamlStats.Read(showCase.XamlPath).RenderMarkdown());
        builder.AppendLine();
        builder.AppendLine("| Set | Trigger | Mean ms | Median ms | P95 ms | Min ms | Max ms | Alloc KB mean | Visuals | Logical | Space | CompactSpace | CompactSpaceItem | Icon | IconPresenter | PathIcon | Avatar | AvatarGroup | Image | Svg | TextBlock | FlyoutHost | CountBadge | DotBadge | RibbonBadge | CountBadgeAdorner | DotBadgeAdorner | RibbonBadgeAdorner | DotBadgeIndicator | MotionTarget | Label | Alert | MarqueeLabel | LineEdit total | LineEdit direct | SearchEdit | TextArea | Button | IconButton | ToggleIconButton | ButtonSpinner | ButtonSpinnerBox | ButtonSpinnerHandle | ButtonSpinnerContentPanel | Card | CardActionPanel | CardActionButton | CardMetaContent | CardGridContent | CardGridItem | CardTabsContent | Collapse | CollapseItem | Collapse content motion | Collapse expand button | Collapse addon presenter | Carousel | CarouselPage | CarouselPagination | CarouselIndicator | CarouselNavButton | CarouselLayoutTransform | CarouselProgressBorder | CarouselPageTransition | CarouselTimer | CarouselIndicatorAnimation | Skeleton | SkeletonLine | Select | ComboBox | ComboBoxItem | ComboBoxHandle | ComboBoxHost | DatePicker | RangeDatePicker | InfoPicker | PickerHost | DatePickerPresenter | RangePickerPresenter | DateCalendar | DateCalendarItem | DateDayButton | DateCalendarButton | TimeView | DateTimePanel | AutoComplete | AC popup fields | AC candidate fields | CandidateList visuals | TreeSelect | Cascader | Menu | MenuItem | NavMenuHeader | ShowCaseItem | IconGallery | IconInfoItem | AddOnDecoratedBox | CheckBox | CheckBoxGroup | CheckBoxIndicator | CheckBox checked mark | CheckBox tristate mark | RadioButton | RadioButtonGroup | RadioIndicator | WaveSpiritDecorator | Descriptions | DescriptionDefaultItem | DescriptionBorderedItemLabel | DescriptionBorderedItemContent | Dialog | MessageBox | OverlayDialogPresenter | DialogWindow | DialogSurface | DialogButtonBox | DialogButton | DialogCaptionButton | OverlayDialogMask | OverlayDialogResizer | MessageBoxContent | Drawer | GroupBox | GroupBoxHeaderIcon |");
        builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        builder.AppendLine(RenderSampleRow("Cold first navigation", result.ColdRuns));
        builder.AppendLine(RenderSampleRow("Repeated navigation", result.Samples));
        builder.AppendLine();
        builder.AppendLine("## Samples");
        builder.AppendLine();
        builder.AppendLine("| Iteration | Phase | Trigger | Elapsed ms | Alloc KB | Visuals | Logical | Space | CompactSpace | CompactSpaceItem | Icon | IconPresenter | PathIcon | Avatar | AvatarGroup | Image | Svg | TextBlock | FlyoutHost | CountBadge | DotBadge | RibbonBadge | CountBadgeAdorner | DotBadgeAdorner | RibbonBadgeAdorner | DotBadgeIndicator | MotionTarget | Label | Alert | MarqueeLabel | LineEdit total | LineEdit direct | SearchEdit | TextArea | Button | IconButton | ToggleIconButton | ButtonSpinner | ButtonSpinnerBox | ButtonSpinnerHandle | ButtonSpinnerContentPanel | Card | CardActionPanel | CardActionButton | CardMetaContent | CardGridContent | CardGridItem | CardTabsContent | Collapse | CollapseItem | Collapse content motion | Collapse expand button | Collapse addon presenter | Carousel | CarouselPage | CarouselPagination | CarouselIndicator | CarouselNavButton | CarouselLayoutTransform | CarouselProgressBorder | CarouselPageTransition | CarouselTimer | CarouselIndicatorAnimation | Skeleton | SkeletonLine | Select | ComboBox | ComboBoxItem | ComboBoxHandle | ComboBoxHost | DatePicker | RangeDatePicker | InfoPicker | PickerHost | DatePickerPresenter | RangePickerPresenter | DateCalendar | DateCalendarItem | DateDayButton | DateCalendarButton | TimeView | DateTimePanel | AutoComplete | AC popup fields | AC candidate fields | CandidateList visuals | TreeSelect | Cascader | Menu | MenuItem | NavMenuHeader | ShowCaseItem | IconGallery | IconInfoItem | AddOnDecoratedBox | CheckBox | CheckBoxGroup | CheckBoxIndicator | CheckBox checked mark | CheckBox tristate mark | RadioButton | RadioButtonGroup | RadioIndicator | WaveSpiritDecorator | Descriptions | DescriptionDefaultItem | DescriptionBorderedItemLabel | DescriptionBorderedItemContent | Dialog | MessageBox | OverlayDialogPresenter | DialogWindow | DialogSurface | DialogButtonBox | DialogButton | DialogCaptionButton | OverlayDialogMask | OverlayDialogResizer | MessageBoxContent | Drawer | GroupBox | GroupBoxHeaderIcon |");
        builder.AppendLine("| ---: | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var sample in result.ColdRuns)
        {
            builder.AppendLine(RenderSample(sample));
        }
        foreach (var sample in result.Samples)
        {
            builder.AppendLine(RenderSample(sample));
        }
        return builder.ToString();
    }

    private static string RenderSampleRow(string label, IReadOnlyList<NavigationSample> samples)
    {
        var ordered = samples.Select(sample => sample.Elapsed.TotalMilliseconds).Order().ToArray();
        var mean    = ordered.Average();
        var median  = Percentile(ordered, 0.50);
        var p95     = Percentile(ordered, 0.95);
        var min     = ordered.First();
        var max     = ordered.Last();
        var allocKb = samples.Average(sample => sample.AllocatedBytes / 1024.0);
        var stats   = samples.Last().Stats;
        var trigger = string.Join(", ", samples.Select(sample => sample.Trigger).Distinct());

        return string.Join(" | ",
            "| " + label,
            trigger,
            Format(mean),
            Format(median),
            Format(p95),
            Format(min),
            Format(max),
            Format(allocKb),
            stats.VisualCount.ToString(CultureInfo.InvariantCulture),
            stats.LogicalCount.ToString(CultureInfo.InvariantCulture),
            stats.SpaceCount.ToString(CultureInfo.InvariantCulture),
            stats.CompactSpaceCount.ToString(CultureInfo.InvariantCulture),
            stats.CompactSpaceItemCount.ToString(CultureInfo.InvariantCulture),
            stats.IconCount.ToString(CultureInfo.InvariantCulture),
            stats.IconPresenterCount.ToString(CultureInfo.InvariantCulture),
            stats.PathIconCount.ToString(CultureInfo.InvariantCulture),
            stats.AvatarCount.ToString(CultureInfo.InvariantCulture),
            stats.AvatarGroupCount.ToString(CultureInfo.InvariantCulture),
            stats.ImageCount.ToString(CultureInfo.InvariantCulture),
            stats.SvgCount.ToString(CultureInfo.InvariantCulture),
            stats.TextBlockCount.ToString(CultureInfo.InvariantCulture),
            stats.FlyoutHostCount.ToString(CultureInfo.InvariantCulture),
            stats.CountBadgeCount.ToString(CultureInfo.InvariantCulture),
            stats.DotBadgeCount.ToString(CultureInfo.InvariantCulture),
            stats.RibbonBadgeCount.ToString(CultureInfo.InvariantCulture),
            stats.CountBadgeAdornerCount.ToString(CultureInfo.InvariantCulture),
            stats.DotBadgeAdornerCount.ToString(CultureInfo.InvariantCulture),
            stats.RibbonBadgeAdornerCount.ToString(CultureInfo.InvariantCulture),
            stats.DotBadgeIndicatorCount.ToString(CultureInfo.InvariantCulture),
            stats.MotionTargetCount.ToString(CultureInfo.InvariantCulture),
            stats.LabelCount.ToString(CultureInfo.InvariantCulture),
            stats.AlertCount.ToString(CultureInfo.InvariantCulture),
            stats.MarqueeLabelCount.ToString(CultureInfo.InvariantCulture),
            stats.LineEditCount.ToString(CultureInfo.InvariantCulture),
            stats.LineEditDirectCount.ToString(CultureInfo.InvariantCulture),
            stats.SearchEditCount.ToString(CultureInfo.InvariantCulture),
            stats.TextAreaCount.ToString(CultureInfo.InvariantCulture),
            stats.ButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.IconButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.ToggleIconButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.ButtonSpinnerCount.ToString(CultureInfo.InvariantCulture),
            stats.ButtonSpinnerDecoratedBoxCount.ToString(CultureInfo.InvariantCulture),
            stats.ButtonSpinnerHandleCount.ToString(CultureInfo.InvariantCulture),
            stats.ButtonSpinnerContentPanelCount.ToString(CultureInfo.InvariantCulture),
            stats.CardCount.ToString(CultureInfo.InvariantCulture),
            stats.CardActionPanelCount.ToString(CultureInfo.InvariantCulture),
            stats.CardActionButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.CardMetaContentCount.ToString(CultureInfo.InvariantCulture),
            stats.CardGridContentCount.ToString(CultureInfo.InvariantCulture),
            stats.CardGridItemCount.ToString(CultureInfo.InvariantCulture),
            stats.CardTabsContentCount.ToString(CultureInfo.InvariantCulture),
            stats.CollapseCount.ToString(CultureInfo.InvariantCulture),
            stats.CollapseItemCount.ToString(CultureInfo.InvariantCulture),
            stats.CollapseContentViewportCount.ToString(CultureInfo.InvariantCulture),
            stats.CollapseExpandButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.CollapseAddOnPresenterCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselPageCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselPaginationCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselPageIndicatorCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselNavButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselLayoutTransformCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselProgressBorderCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselPageTransitionFieldCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselAutoPlayTimerFieldCount.ToString(CultureInfo.InvariantCulture),
            stats.CarouselIndicatorAnimationFieldCount.ToString(CultureInfo.InvariantCulture),
            stats.SkeletonCount.ToString(CultureInfo.InvariantCulture),
            stats.SkeletonLineCount.ToString(CultureInfo.InvariantCulture),
            stats.SelectCount.ToString(CultureInfo.InvariantCulture),
            stats.ComboBoxCount.ToString(CultureInfo.InvariantCulture),
            stats.ComboBoxItemCount.ToString(CultureInfo.InvariantCulture),
            stats.ComboBoxHandleCount.ToString(CultureInfo.InvariantCulture),
            stats.ComboBoxAccessoryHostCount.ToString(CultureInfo.InvariantCulture),
            stats.DatePickerCount.ToString(CultureInfo.InvariantCulture),
            stats.RangeDatePickerCount.ToString(CultureInfo.InvariantCulture),
            stats.InfoPickerInputCount.ToString(CultureInfo.InvariantCulture),
            stats.PickerAccessoryHostCount.ToString(CultureInfo.InvariantCulture),
            stats.DatePickerPresenterCount.ToString(CultureInfo.InvariantCulture),
            stats.RangeDatePickerPresenterCount.ToString(CultureInfo.InvariantCulture),
            stats.DatePickerCalendarCount.ToString(CultureInfo.InvariantCulture),
            stats.DatePickerCalendarItemCount.ToString(CultureInfo.InvariantCulture),
            stats.DatePickerCalendarDayButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.DatePickerCalendarButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.TimeViewCount.ToString(CultureInfo.InvariantCulture),
            stats.DateTimePickerPanelCount.ToString(CultureInfo.InvariantCulture),
            stats.AutoCompleteCount.ToString(CultureInfo.InvariantCulture),
            stats.AutoCompletePopupFieldCount.ToString(CultureInfo.InvariantCulture),
            stats.AutoCompleteCandidateListFieldCount.ToString(CultureInfo.InvariantCulture),
            stats.CandidateListCount.ToString(CultureInfo.InvariantCulture),
            stats.TreeSelectCount.ToString(CultureInfo.InvariantCulture),
            stats.CascaderCount.ToString(CultureInfo.InvariantCulture),
            stats.MenuCount.ToString(CultureInfo.InvariantCulture),
            stats.MenuItemCount.ToString(CultureInfo.InvariantCulture),
            stats.NavMenuItemHeaderCount.ToString(CultureInfo.InvariantCulture),
            stats.ShowCaseItemCount.ToString(CultureInfo.InvariantCulture),
            stats.IconGalleryCount.ToString(CultureInfo.InvariantCulture),
            stats.IconInfoItemCount.ToString(CultureInfo.InvariantCulture),
            stats.AddOnDecoratedBoxCount.ToString(CultureInfo.InvariantCulture),
            stats.CheckBoxCount.ToString(CultureInfo.InvariantCulture),
            stats.CheckBoxGroupCount.ToString(CultureInfo.InvariantCulture),
            stats.CheckBoxIndicatorCount.ToString(CultureInfo.InvariantCulture),
            stats.CheckBoxCheckedMarkCount.ToString(CultureInfo.InvariantCulture),
            stats.CheckBoxTristateMarkCount.ToString(CultureInfo.InvariantCulture),
            stats.RadioButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.RadioButtonGroupCount.ToString(CultureInfo.InvariantCulture),
            stats.RadioIndicatorCount.ToString(CultureInfo.InvariantCulture),
            stats.WaveSpiritDecoratorCount.ToString(CultureInfo.InvariantCulture),
            stats.DescriptionsCount.ToString(CultureInfo.InvariantCulture),
            stats.DescriptionDefaultItemCount.ToString(CultureInfo.InvariantCulture),
            stats.DescriptionBorderedItemLabelCount.ToString(CultureInfo.InvariantCulture),
            stats.DescriptionBorderedItemContentCount.ToString(CultureInfo.InvariantCulture),
            stats.DialogCount.ToString(CultureInfo.InvariantCulture),
            stats.MessageBoxCount.ToString(CultureInfo.InvariantCulture),
            stats.OverlayDialogPresenterCount.ToString(CultureInfo.InvariantCulture),
            stats.DialogWindowCount.ToString(CultureInfo.InvariantCulture),
            stats.DialogSurfaceCount.ToString(CultureInfo.InvariantCulture),
            stats.DialogButtonBoxCount.ToString(CultureInfo.InvariantCulture),
            stats.DialogButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.DialogCaptionButtonCount.ToString(CultureInfo.InvariantCulture),
            stats.OverlayDialogMaskCount.ToString(CultureInfo.InvariantCulture),
            stats.OverlayDialogResizerCount.ToString(CultureInfo.InvariantCulture),
            stats.MessageBoxContentCount.ToString(CultureInfo.InvariantCulture),
            stats.DrawerCount.ToString(CultureInfo.InvariantCulture),
            stats.GroupBoxCount.ToString(CultureInfo.InvariantCulture),
            stats.GroupBoxHeaderIconPresenterCount.ToString(CultureInfo.InvariantCulture) + " |");
    }

    private static string RenderSample(NavigationSample sample)
    {
        return string.Join(" | ",
            "| " + sample.Iteration.ToString(CultureInfo.InvariantCulture),
            sample.Phase,
            sample.Trigger,
            Format(sample.Elapsed.TotalMilliseconds),
            Format(sample.AllocatedBytes / 1024.0),
            sample.Stats.VisualCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.LogicalCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.SpaceCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CompactSpaceCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CompactSpaceItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.IconCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.IconPresenterCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.PathIconCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.AvatarCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.AvatarGroupCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ImageCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.SvgCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.TextBlockCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.FlyoutHostCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CountBadgeCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DotBadgeCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.RibbonBadgeCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CountBadgeAdornerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DotBadgeAdornerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.RibbonBadgeAdornerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DotBadgeIndicatorCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.MotionTargetCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.LabelCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.AlertCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.MarqueeLabelCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.LineEditCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.LineEditDirectCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.SearchEditCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.TextAreaCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.IconButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ToggleIconButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ButtonSpinnerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ButtonSpinnerDecoratedBoxCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ButtonSpinnerHandleCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ButtonSpinnerContentPanelCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CardCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CardActionPanelCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CardActionButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CardMetaContentCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CardGridContentCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CardGridItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CardTabsContentCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CollapseCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CollapseItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CollapseContentViewportCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CollapseExpandButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CollapseAddOnPresenterCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselPageCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselPaginationCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselPageIndicatorCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselNavButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselLayoutTransformCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselProgressBorderCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselPageTransitionFieldCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselAutoPlayTimerFieldCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CarouselIndicatorAnimationFieldCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.SkeletonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.SkeletonLineCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.SelectCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ComboBoxCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ComboBoxItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ComboBoxHandleCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ComboBoxAccessoryHostCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DatePickerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.RangeDatePickerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.InfoPickerInputCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.PickerAccessoryHostCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DatePickerPresenterCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.RangeDatePickerPresenterCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DatePickerCalendarCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DatePickerCalendarItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DatePickerCalendarDayButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DatePickerCalendarButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.TimeViewCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DateTimePickerPanelCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.AutoCompleteCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.AutoCompletePopupFieldCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.AutoCompleteCandidateListFieldCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CandidateListCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.TreeSelectCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CascaderCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.MenuCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.MenuItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.NavMenuItemHeaderCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.ShowCaseItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.IconGalleryCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.IconInfoItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.AddOnDecoratedBoxCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CheckBoxCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CheckBoxGroupCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CheckBoxIndicatorCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CheckBoxCheckedMarkCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.CheckBoxTristateMarkCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.RadioButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.RadioButtonGroupCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.RadioIndicatorCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.WaveSpiritDecoratorCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DescriptionsCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DescriptionDefaultItemCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DescriptionBorderedItemLabelCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DescriptionBorderedItemContentCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DialogCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.MessageBoxCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.OverlayDialogPresenterCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DialogWindowCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DialogSurfaceCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DialogButtonBoxCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DialogButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DialogCaptionButtonCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.OverlayDialogMaskCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.OverlayDialogResizerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.MessageBoxContentCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.DrawerCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.GroupBoxCount.ToString(CultureInfo.InvariantCulture),
            sample.Stats.GroupBoxHeaderIconPresenterCount.ToString(CultureInfo.InvariantCulture) + " |");
    }

    private static double Percentile(IReadOnlyList<double> ordered, double percentile)
    {
        if (ordered.Count == 0)
        {
            return 0;
        }
        if (ordered.Count == 1)
        {
            return ordered[0];
        }

        var index = (ordered.Count - 1) * percentile;
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        if (lower == upper)
        {
            return ordered[lower];
        }

        var weight = index - lower;
        return ordered[lower] * (1 - weight) + ordered[upper] * weight;
    }

    private static string Format(double value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FormatMs(TimeSpan value)
    {
        return value.TotalMilliseconds.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FormatOptionalMs(TimeSpan? value)
    {
        return value.HasValue ? FormatMs(value.Value) : string.Empty;
    }

    private static string FormatKb(long bytes)
    {
        return (bytes / 1024.0).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string DescribeStats(RouteStats? stats)
    {
        if (stats is null)
        {
            return "none";
        }

        return $"visuals={stats.VisualCount}, logical={stats.LogicalCount}, space={stats.SpaceCount}, compactSpace={stats.CompactSpaceCount}, compactItems={stats.CompactSpaceItemCount}, avatar={stats.AvatarCount}, avatarGroup={stats.AvatarGroupCount}, badge={stats.CountBadgeCount + stats.DotBadgeCount + stats.RibbonBadgeCount}, badgeAdorner={stats.CountBadgeAdornerCount + stats.DotBadgeAdornerCount + stats.RibbonBadgeAdornerCount}, flyoutHost={stats.FlyoutHostCount}, alert={stats.AlertCount}, marqueeLabel={stats.MarqueeLabelCount}, lineEdit={stats.LineEditCount}, button={stats.ButtonCount}, buttonSpinner={stats.ButtonSpinnerCount}, card={stats.CardCount}, cardActionPanel={stats.CardActionPanelCount}, groupBox={stats.GroupBoxCount}, groupBoxHeaderIcon={stats.GroupBoxHeaderIconPresenterCount}, collapse={stats.CollapseCount}, collapseItem={stats.CollapseItemCount}, collapseMotion={stats.CollapseContentViewportCount}, collapseExpandButton={stats.CollapseExpandButtonCount}, carousel={stats.CarouselCount}, carouselIndicator={stats.CarouselPageIndicatorCount}, carouselNavButton={stats.CarouselNavButtonCount}, carouselProgressBorder={stats.CarouselProgressBorderCount}, skeleton={stats.SkeletonCount}, skeletonLine={stats.SkeletonLineCount}, statistic={stats.StatisticCount}, timerStatistic={stats.TimerStatisticCount}, statisticCountUp={stats.StatisticCountUpCount}, steps={stats.StepsCount}, stepsItem={stats.StepsItemCount}, stepsIndicator={stats.StepsItemIndicatorCount}, calendar={stats.CalendarCount}, calendarItem={stats.CalendarItemCount}, calendarDayButton={stats.CalendarDayButtonCount}, calendarButton={stats.CalendarButtonCount}, select={stats.SelectCount}, comboBox={stats.ComboBoxCount}, comboBoxItem={stats.ComboBoxItemCount}, comboBoxHandle={stats.ComboBoxHandleCount}, comboBoxHost={stats.ComboBoxAccessoryHostCount}, colorPicker={stats.ColorPickerCount}, gradientColorPicker={stats.GradientColorPickerCount}, datePicker={stats.DatePickerCount}, rangeDatePicker={stats.RangeDatePickerCount}, datePickerPresenter={stats.DatePickerPresenterCount}, datePickerCalendar={stats.DatePickerCalendarCount}, timeView={stats.TimeViewCount}, autoComplete={stats.AutoCompleteCount}, autoCompletePopupFields={stats.AutoCompletePopupFieldCount}, autoCompleteCandidateFields={stats.AutoCompleteCandidateListFieldCount}, mentions={stats.MentionsCount}, mentionTextArea={stats.MentionTextAreaCount}, mentionsPopupFields={stats.MentionsPopupFieldCount}, mentionsCandidateFields={stats.MentionsCandidateListFieldCount}, candidateListVisuals={stats.CandidateListCount}, treeSelect={stats.TreeSelectCount}, cascader={stats.CascaderCount}, checkBox={stats.CheckBoxCount}, checkBoxGroup={stats.CheckBoxGroupCount}, checkBoxIndicator={stats.CheckBoxIndicatorCount}, radioButton={stats.RadioButtonCount}, radioButtonGroup={stats.RadioButtonGroupCount}, radioIndicator={stats.RadioIndicatorCount}, wave={stats.WaveSpiritDecoratorCount}, descriptions={stats.DescriptionsCount}, descriptionDefaultItem={stats.DescriptionDefaultItemCount}, descriptionBorderedLabel={stats.DescriptionBorderedItemLabelCount}, descriptionBorderedContent={stats.DescriptionBorderedItemContentCount}, drawer={stats.DrawerCount}, dialog={stats.DialogCount}, messageBox={stats.MessageBoxCount}, overlayPresenter={stats.OverlayDialogPresenterCount}, dialogWindow={stats.DialogWindowCount}, dialogSurface={stats.DialogSurfaceCount}, dialogButtonBox={stats.DialogButtonBoxCount}, dialogButton={stats.DialogButtonCount}, captionButton={stats.DialogCaptionButtonCount}, dialogMask={stats.OverlayDialogMaskCount}, dialogResizer={stats.OverlayDialogResizerCount}, messageBoxContent={stats.MessageBoxContentCount}, addOnDecoratedBox={stats.AddOnDecoratedBoxCount}";
    }
}

internal sealed record PerfOptions(
    int Iterations,
    int Warmup,
    int ColdIterations,
    bool ColdChild,
    int ColdChildIteration,
    string Label,
    string ShowCase,
    string? MarkdownOutputPath,
    TimeSpan Timeout,
    bool SpaceItems,
    bool SpaceItemsWithoutTreeCascader,
    bool SpaceItemsWithoutTreeCascaderSelect,
    bool TraceNavigation,
    int? SpaceRemoveItem,
    bool ResizeTrace,
    bool ResizeForceMaterialized,
    bool FeedbackStack)
{
    public static PerfOptions Parse(string[] args)
    {
        var iterations = 20;
        var warmup     = 3;
        var coldIterations = 1;
        var coldChild = false;
        var coldChildIteration = 0;
        var label      = "current";
        var showCase   = "lineedit";
        var markdown   = default(string);
        var timeout    = TimeSpan.FromSeconds(10);
        var spaceItems = false;
        var spaceItemsWithoutTreeCascader = false;
        var spaceItemsWithoutTreeCascaderSelect = false;
        var traceNavigation = false;
        var spaceRemoveItem = default(int?);
        var resizeTrace = false;
        var resizeForceMaterialized = false;
        var feedbackStack = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--iterations" when i + 1 < args.Length &&
                                         int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIterations):
                    iterations = parsedIterations;
                    i++;
                    break;
                case "--warmup" when i + 1 < args.Length &&
                                     int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedWarmup):
                    warmup = parsedWarmup;
                    i++;
                    break;
                case "--cold-iterations" when i + 1 < args.Length &&
                                              int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedColdIterations):
                    coldIterations = parsedColdIterations;
                    i++;
                    break;
                case "--cold-child":
                    coldChild = true;
                    break;
                case "--cold-child-iteration" when i + 1 < args.Length &&
                                                   int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedColdChildIteration):
                    coldChildIteration = parsedColdChildIteration;
                    i++;
                    break;
                case "--label" when i + 1 < args.Length:
                    label = args[i + 1];
                    i++;
                    break;
                case "--showcase" when i + 1 < args.Length:
                    showCase = args[i + 1];
                    i++;
                    break;
                case "--markdown" when i + 1 < args.Length:
                    markdown = args[i + 1];
                    i++;
                    break;
                case "--timeout-ms" when i + 1 < args.Length &&
                                         int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedTimeout):
                    timeout = TimeSpan.FromMilliseconds(parsedTimeout);
                    i++;
                    break;
                case "--space-items":
                    showCase   = "space";
                    spaceItems = true;
                    break;
                case "--space-items-without-tree-cascader":
                    showCase                      = "space";
                    spaceItems                    = true;
                    spaceItemsWithoutTreeCascader = true;
                    break;
                case "--space-items-without-tree-cascader-select":
                    showCase                            = "space";
                    spaceItems                          = true;
                    spaceItemsWithoutTreeCascaderSelect = true;
                    break;
                case "--trace-navigation":
                    traceNavigation = true;
                    break;
                case "--resize-trace":
                    resizeTrace = true;
                    break;
                case "--resize-force-materialized":
                    resizeForceMaterialized = true;
                    break;
                case "--feedback-stack":
                    feedbackStack = true;
                    break;
                case "--space-remove-item" when i + 1 < args.Length &&
                                                int.TryParse(args[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedItem):
                    showCase        = "space";
                    spaceRemoveItem = parsedItem;
                    i++;
                    break;
            }
        }

        return new PerfOptions(
            Math.Max(1, iterations),
            Math.Max(0, warmup),
            Math.Max(1, coldIterations),
            coldChild,
            Math.Max(0, coldChildIteration),
            label,
            showCase,
            markdown,
            timeout,
            spaceItems,
            spaceItemsWithoutTreeCascader,
            spaceItemsWithoutTreeCascaderSelect,
            traceNavigation,
            spaceRemoveItem,
            resizeTrace,
            resizeForceMaterialized,
            feedbackStack);
    }
}

internal sealed class ProbeScreen : IScreen
{
    public RoutingState Router { get; } = new();
}

internal sealed record SpaceShowCaseItemInfo(int Index, string Title, string Description);

internal sealed record SpaceShowCaseItemSample(
    SpaceShowCaseItemInfo Item,
    int Iteration,
    string Phase,
    TimeSpan Elapsed,
    long AllocatedBytes,
    RouteStats Stats);

internal sealed record ShowCaseSpec(
    string Label,
    EntityKey Key,
    string RouteTypeName,
    string XamlPath,
    Func<RouteStats, bool> IsReady);

internal sealed record NavigationResult(
    string Label,
    IReadOnlyList<NavigationSample> ColdRuns,
    IReadOnlyList<NavigationSample> Samples)
{
    public static NavigationResult Create(string label,
                                          IReadOnlyList<NavigationSample> coldRuns,
                                          IReadOnlyList<NavigationSample> samples)
    {
        return new NavigationResult(label, coldRuns, samples);
    }
}

internal sealed record NavigationSample(
    int Iteration,
    string Phase,
    string Trigger,
    TimeSpan Elapsed,
    long AllocatedBytes,
    RouteStats Stats);

internal sealed class ColdChildSampleDto
{
    public int Iteration { get; set; }
    public string Phase { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public double ElapsedMs { get; set; }
    public long AllocatedBytes { get; set; }
    public int[] Stats { get; set; } = [];

    public static ColdChildSampleDto FromSample(NavigationSample sample)
    {
        return new ColdChildSampleDto
        {
            Iteration      = sample.Iteration,
            Phase          = sample.Phase,
            Trigger        = sample.Trigger,
            ElapsedMs      = sample.Elapsed.TotalMilliseconds,
            AllocatedBytes = sample.AllocatedBytes,
            Stats          = RouteStatsSerializer.ToArray(sample.Stats)
        };
    }

    public NavigationSample ToSample()
    {
        return new NavigationSample(
            Iteration,
            Phase,
            Trigger,
            TimeSpan.FromMilliseconds(ElapsedMs),
            AllocatedBytes,
            RouteStatsSerializer.FromArray(Stats));
    }
}

internal static class RouteStatsSerializer
{
    private static readonly ConstructorInfo Constructor =
        typeof(RouteStats).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                          .OrderByDescending(constructor => constructor.GetParameters().Length)
                          .First();

    private static readonly PropertyInfo[] Properties =
        Constructor.GetParameters()
                   .Select(parameter => typeof(RouteStats).GetProperty(ToPropertyName(parameter.Name!))
                                        ?? throw new InvalidOperationException($"RouteStats property was not found for parameter '{parameter.Name}'."))
                   .ToArray();

    public static int[] ToArray(RouteStats stats)
    {
        return Properties.Select(property => (int)property.GetValue(stats)!).ToArray();
    }

    public static RouteStats FromArray(IReadOnlyList<int> values)
    {
        if (values.Count != Properties.Length)
        {
            throw new InvalidOperationException($"RouteStats value count mismatch. Expected {Properties.Length}, actual {values.Count}.");
        }
        var args = values.Select(value => (object)value).ToArray();
        return (RouteStats)Constructor.Invoke(args);
    }

    private static string ToPropertyName(string parameterName)
    {
        return char.ToUpperInvariant(parameterName[0]) + parameterName[1..];
    }
}

internal sealed record NavigationTraceData(
    TimeSpan? FirstFoundElapsed,
    TimeSpan? FirstReadyElapsed,
    TimeSpan StableElapsed,
    int PumpCount,
    TimeSpan PumpTotal,
    TimeSpan MaxPump,
    int StatsCount,
    TimeSpan StatsTotal,
    TimeSpan ScanTotal,
    RouteStats Stats,
    IReadOnlyList<string> Events);

internal sealed record NavigationTraceSample(
    int Iteration,
    string Phase,
    string Trigger,
    TimeSpan Total,
    TimeSpan TriggerElapsed,
    TimeSpan? FirstFoundElapsed,
    TimeSpan? FirstReadyElapsed,
    TimeSpan StableElapsed,
    int PumpCount,
    TimeSpan PumpTotal,
    TimeSpan MaxPump,
    int StatsCount,
    TimeSpan StatsTotal,
    TimeSpan ScanTotal,
    long AllocatedBytes,
    RouteStats Stats,
    IReadOnlyList<string> Events);

internal sealed record RouteStats(
    int VisualCount,
    int LogicalCount,
    int SpaceCount,
    int CompactSpaceCount,
    int CompactSpaceItemCount,
    int IconCount,
    int IconPresenterCount,
    int PathIconCount,
    int AvatarCount,
    int AvatarGroupCount,
    int ImageCount,
    int SvgCount,
    int TextBlockCount,
    int FlyoutHostCount,
    int CountBadgeCount,
    int DotBadgeCount,
    int RibbonBadgeCount,
    int CountBadgeAdornerCount,
    int DotBadgeAdornerCount,
    int RibbonBadgeAdornerCount,
    int DotBadgeIndicatorCount,
    int MotionTargetCount,
    int LabelCount,
    int AlertCount,
    int MarqueeLabelCount,
    int LineEditCount,
    int LineEditDirectCount,
    int SearchEditCount,
    int TextAreaCount,
    int ShowCaseItemCount,
    int IconGalleryCount,
    int IconInfoItemCount,
    int AddOnDecoratedBoxCount,
    int ButtonCount,
    int IconButtonCount,
    int ToggleIconButtonCount,
    int ButtonSpinnerCount,
    int ButtonSpinnerDecoratedBoxCount,
    int ButtonSpinnerHandleCount,
    int ButtonSpinnerContentPanelCount,
    int CardCount,
    int CardActionPanelCount,
    int CardActionButtonCount,
    int CardMetaContentCount,
    int CardGridContentCount,
    int CardGridItemCount,
    int CardTabsContentCount,
    int CollapseCount,
    int CollapseItemCount,
    int CollapseContentViewportCount,
    int CollapseExpandButtonCount,
    int CollapseAddOnPresenterCount,
    int CarouselCount,
    int CarouselPageCount,
    int CarouselPaginationCount,
    int CarouselPageIndicatorCount,
    int CarouselNavButtonCount,
    int CarouselLayoutTransformCount,
    int CarouselProgressBorderCount,
    int CarouselPageTransitionFieldCount,
    int CarouselAutoPlayTimerFieldCount,
    int CarouselIndicatorAnimationFieldCount,
    int SkeletonCount,
    int SkeletonLineCount,
    int StatisticCount,
    int TimerStatisticCount,
    int StatisticCountUpCount,
    int StepsCount,
    int StepsItemCount,
    int StepsItemIndicatorCount,
    int CalendarCount,
    int CalendarItemCount,
    int CalendarDayButtonCount,
    int CalendarButtonCount,
    int SelectCount,
    int ComboBoxCount,
    int ComboBoxItemCount,
    int ComboBoxHandleCount,
    int ComboBoxAccessoryHostCount,
    int ColorPickerCount,
    int GradientColorPickerCount,
    int DatePickerCount,
    int RangeDatePickerCount,
    int InfoPickerInputCount,
    int PickerAccessoryHostCount,
    int DatePickerPresenterCount,
    int RangeDatePickerPresenterCount,
    int DatePickerCalendarCount,
    int DatePickerCalendarItemCount,
    int DatePickerCalendarDayButtonCount,
    int DatePickerCalendarButtonCount,
    int TimeViewCount,
    int DateTimePickerPanelCount,
    int AutoCompleteCount,
    int AutoCompleteSearchEditCount,
    int AutoCompleteTextAreaCount,
    int AutoCompletePopupFieldCount,
    int AutoCompleteCandidateListFieldCount,
    int MentionsCount,
    int MentionTextAreaCount,
    int MentionsPopupFieldCount,
    int MentionsCandidateListFieldCount,
    int CandidateListCount,
    int TreeSelectCount,
    int CascaderCount,
    int MenuCount,
    int MenuItemCount,
    int NavMenuItemHeaderCount,
    int CheckBoxCount,
    int CheckBoxGroupCount,
    int CheckBoxIndicatorCount,
    int CheckBoxCheckedMarkCount,
    int CheckBoxTristateMarkCount,
    int RadioButtonCount,
    int RadioButtonGroupCount,
    int RadioIndicatorCount,
    int WaveSpiritDecoratorCount,
    int DescriptionsCount,
    int DescriptionDefaultItemCount,
    int DescriptionBorderedItemLabelCount,
    int DescriptionBorderedItemContentCount,
    int DrawerCount,
    int DialogCount,
    int MessageBoxCount,
    int OverlayDialogPresenterCount,
    int DialogWindowCount,
    int DialogSurfaceCount,
    int DialogButtonBoxCount,
    int DialogButtonCount,
    int DialogCaptionButtonCount,
    int OverlayDialogMaskCount,
    int OverlayDialogResizerCount,
    int MessageBoxContentCount,
    int GroupBoxCount,
    int GroupBoxHeaderIconPresenterCount)
{
    public bool IsDisplayReady(ShowCaseSpec showCase)
    {
        return VisualCount > 0 && showCase.IsReady(this);
    }

    public static RouteStats Collect(Control root)
    {
        var visuals                 = root.GetSelfAndVisualDescendants().ToList();
        var spaceCount              = 0;
        var compactSpaceCount       = 0;
        var compactSpaceItemCount   = 0;
        var iconCount               = 0;
        var iconPresenterCount      = 0;
        var pathIconCount           = 0;
        var avatarCount             = 0;
        var avatarGroupCount        = 0;
        var imageCount              = 0;
        var svgCount                = 0;
        var textBlockCount          = 0;
        var flyoutHostCount         = 0;
        var countBadgeCount         = 0;
        var dotBadgeCount           = 0;
        var ribbonBadgeCount        = 0;
        var countBadgeAdornerCount  = 0;
        var dotBadgeAdornerCount    = 0;
        var ribbonBadgeAdornerCount = 0;
        var dotBadgeIndicatorCount  = 0;
        var motionTargetCount        = 0;
        var labelCount              = 0;
        var alertCount              = 0;
        var marqueeLabelCount       = 0;
        var lineEditCount           = 0;
        var lineEditDirectCount     = 0;
        var searchEditCount         = 0;
        var textAreaCount           = 0;
        var showCaseItemCount       = 0;
        var iconGalleryCount        = 0;
        var iconInfoItemCount       = 0;
        var addOnDecoratedBoxCount  = 0;
        var buttonCount             = 0;
        var iconButtonCount         = 0;
        var toggleIconButtonCount   = 0;
        var buttonSpinnerCount      = 0;
        var buttonSpinnerDecoratedBoxCount = 0;
        var buttonSpinnerHandleCount       = 0;
        var buttonSpinnerContentPanelCount = 0;
        var cardCount                = 0;
        var cardActionPanelCount     = 0;
        var cardActionButtonCount    = 0;
        var cardMetaContentCount     = 0;
        var cardGridContentCount     = 0;
        var cardGridItemCount        = 0;
        var cardTabsContentCount     = 0;
        var collapseCount            = 0;
        var collapseItemCount        = 0;
        var collapseContentViewportCount = 0;
        var collapseExpandButtonCount       = 0;
        var collapseAddOnPresenterCount     = 0;
        var carouselCount            = 0;
        var carouselPageCount        = 0;
        var carouselPaginationCount  = 0;
        var carouselPageIndicatorCount = 0;
        var carouselNavButtonCount   = 0;
        var carouselLayoutTransformCount = 0;
        var carouselProgressBorderCount = 0;
        var carouselPageTransitionFieldCount = 0;
        var carouselAutoPlayTimerFieldCount = 0;
        var carouselIndicatorAnimationFieldCount = 0;
        var skeletonCount            = 0;
        var skeletonLineCount        = 0;
        var statisticCount           = 0;
        var timerStatisticCount      = 0;
        var statisticCountUpCount    = 0;
        var stepsCount               = 0;
        var stepsItemCount           = 0;
        var stepsItemIndicatorCount  = 0;
        var calendarCount            = 0;
        var calendarItemCount        = 0;
        var calendarDayButtonCount   = 0;
        var calendarButtonCount      = 0;
        var selectCount             = 0;
        var comboBoxCount           = 0;
        var comboBoxItemCount       = 0;
        var comboBoxHandleCount     = 0;
        var comboBoxAccessoryHostCount = 0;
        var colorPickerCount        = 0;
        var gradientColorPickerCount = 0;
        var datePickerCount         = 0;
        var rangeDatePickerCount    = 0;
        var infoPickerInputCount    = 0;
        var pickerAccessoryHostCount = 0;
        var datePickerPresenterCount = 0;
        var rangeDatePickerPresenterCount = 0;
        var datePickerCalendarCount = 0;
        var datePickerCalendarItemCount = 0;
        var datePickerCalendarDayButtonCount = 0;
        var datePickerCalendarButtonCount = 0;
        var timeViewCount           = 0;
        var dateTimePickerPanelCount = 0;
        var autoCompleteCount       = 0;
        var autoCompleteSearchEditCount = 0;
        var autoCompleteTextAreaCount   = 0;
        var autoCompletePopupFieldCount = 0;
        var autoCompleteCandidateListFieldCount = 0;
        var mentionsCount          = 0;
        var mentionTextAreaCount   = 0;
        var mentionsPopupFieldCount = 0;
        var mentionsCandidateListFieldCount = 0;
        var candidateListCount      = 0;
        var treeSelectCount         = 0;
        var cascaderCount           = 0;
        var menuCount               = 0;
        var menuItemCount           = 0;
        var navMenuItemHeaderCount  = 0;
        var checkBoxCount           = 0;
        var checkBoxGroupCount      = 0;
        var checkBoxIndicatorCount  = 0;
        var checkBoxCheckedMarkCount  = 0;
        var checkBoxTristateMarkCount = 0;
        var radioButtonCount = 0;
        var radioButtonGroupCount = 0;
        var radioIndicatorCount = 0;
        var waveSpiritDecoratorCount = 0;
        var descriptionsCount = 0;
        var descriptionDefaultItemCount = 0;
        var descriptionBorderedItemLabelCount = 0;
        var descriptionBorderedItemContentCount = 0;
        var drawerCount = 0;
        var dialogCount = 0;
        var messageBoxCount = 0;
        var overlayDialogPresenterCount = 0;
        var dialogWindowCount = 0;
        var dialogSurfaceCount = 0;
        var dialogButtonBoxCount = 0;
        var dialogButtonCount = 0;
        var dialogCaptionButtonCount = 0;
        var overlayDialogMaskCount = 0;
        var overlayDialogResizerCount = 0;
        var messageBoxContentCount = 0;
        var groupBoxCount = 0;
        var groupBoxHeaderIconPresenterCount = 0;

        foreach (var visual in visuals)
        {
            var type = visual.GetType();
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Space"))
            {
                spaceCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CompactSpace"))
            {
                compactSpaceCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CompactSpaceItem"))
            {
                compactSpaceItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Controls.Icon"))
            {
                iconCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Controls.IconPresenter"))
            {
                iconPresenterCount++;
            }
            if (visual is PathIcon)
            {
                pathIconCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Avatar"))
            {
                avatarCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AvatarGroup"))
            {
                avatarGroupCount++;
            }
            if (visual is Image)
            {
                imageCount++;
            }
            if (IsTypeOrDerived(type, "Avalonia.Svg.Svg"))
            {
                svgCount++;
            }
            if (type.Name == "TextBlock")
            {
                textBlockCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.FlyoutHost"))
            {
                flyoutHostCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CountBadge"))
            {
                countBadgeCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DotBadge"))
            {
                dotBadgeCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RibbonBadge"))
            {
                ribbonBadgeCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CountBadgeAdorner"))
            {
                countBadgeAdornerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DotBadgeAdorner"))
            {
                dotBadgeAdornerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RibbonBadgeAdorner"))
            {
                ribbonBadgeAdornerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Controls.Commons.DotBadgeIndicator"))
            {
                dotBadgeIndicatorCount++;
            }
            if (visual is Control motionTarget &&
                    (IsTypeOrDerived(type, "AtomUI.Controls.Primitives.ContentViewport") ||
                     motionTarget.Name is "PART_SurfaceMotionRoot" or "PART_Surface" or "PART_MenuSurface" or
                         "PART_VisibilityRoot" or "PART_PopupSurface" or "PART_IndicatorSurface" ||
                     (motionTarget.Name is "Frame" or "PART_Frame" &&
                      motionTarget.GetVisualParent()?.GetType().Name is "MessageCard" or "NotificationCard")))
            {
                motionTargetCount++;
            }
            if (visual is Label)
            {
                labelCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Alert"))
            {
                alertCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.MarqueeLabel"))
            {
                marqueeLabelCount++;
            }
            if (type.FullName == "AtomUI.Desktop.Controls.LineEdit")
            {
                lineEditDirectCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SearchEdit"))
            {
                searchEditCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.LineEdit"))
            {
                lineEditCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.TextArea"))
            {
                textAreaCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Toolkits.GalleryBase.Controls.ShowCaseItem"))
            {
                showCaseItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Toolkits.GalleryBase.Controls.IconGallery"))
            {
                iconGalleryCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Toolkits.GalleryBase.Controls.IconInfoItem"))
            {
                iconInfoItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AddOnDecoratedBox"))
            {
                addOnDecoratedBoxCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Button"))
            {
                buttonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.IconButton"))
            {
                iconButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ToggleIconButton"))
            {
                toggleIconButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinner"))
            {
                buttonSpinnerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinnerDecoratedBox"))
            {
                buttonSpinnerDecoratedBoxCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinnerHandle"))
            {
                buttonSpinnerHandleCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ButtonSpinnerContentPanel"))
            {
                buttonSpinnerContentPanelCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Card"))
            {
                cardCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardActionPanel"))
            {
                cardActionPanelCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardActionButton"))
            {
                cardActionButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardMetaContent"))
            {
                cardMetaContentCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardGridContent"))
            {
                cardGridContentCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardGridItem"))
            {
                cardGridItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CardTabsContent"))
            {
                cardTabsContentCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.GroupBox"))
            {
                groupBoxCount++;
            }
            if (visual is IconPresenter { Name: "PART_HeaderIconPresenter" } groupBoxIconPresenter &&
                groupBoxIconPresenter.GetVisualAncestors().Any(ancestor =>
                    IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.GroupBox")))
            {
                groupBoxHeaderIconPresenterCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Collapse"))
            {
                collapseCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CollapseItem"))
            {
                collapseItemCount++;
            }
            if (visual is Control { Name: "PART_ContentViewport" } contentViewport &&
                contentViewport.GetVisualAncestors().Any(ancestor =>
                    IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CollapseItem")))
            {
                collapseContentViewportCount++;
            }
            if (visual is Control { Name: "PART_ExpandButton" } expandButton &&
                expandButton.GetVisualAncestors().Any(ancestor =>
                    IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CollapseItem")))
            {
                collapseExpandButtonCount++;
            }
            if (visual is ContentPresenter { Name: "PART_AddOnContentPresenter" } addOnPresenter &&
                addOnPresenter.GetVisualAncestors().Any(ancestor =>
                    IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CollapseItem")))
            {
                collapseAddOnPresenterCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Carousel"))
            {
                carouselCount++;
                if (visual is AtomUI.Desktop.Controls.Carousel carousel && carousel.PageTransition is not null)
                {
                    carouselPageTransitionFieldCount++;
                }
                if (HasFieldValue(visual, "AtomUI.Desktop.Controls.Carousel", "_autoPlayTimer"))
                {
                    carouselAutoPlayTimerFieldCount++;
                }
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselPage"))
            {
                carouselPageCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselPagination"))
            {
                carouselPaginationCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselPageIndicator"))
            {
                carouselPageIndicatorCount++;
                if (HasFieldValue(visual, "AtomUI.Desktop.Controls.CarouselPageIndicator", "_animation"))
                {
                    carouselIndicatorAnimationFieldCount++;
                }
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CarouselNavButton"))
            {
                carouselNavButtonCount++;
            }
            if (visual is LayoutTransformControl { Name: "PaginationLayoutTransform" })
            {
                carouselLayoutTransformCount++;
            }
            if (visual is Border { Name: "Progress" } progressBorder &&
                progressBorder.GetVisualAncestors().Any(ancestor =>
                    IsTypeOrDerived(ancestor.GetType(), "AtomUI.Desktop.Controls.CarouselPageIndicator")))
            {
                carouselProgressBorderCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Skeleton"))
            {
                skeletonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.SkeletonLine"))
            {
                skeletonLineCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Statistic"))
            {
                statisticCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.TimerStatistic"))
            {
                timerStatisticCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.StatisticCountUp"))
            {
                statisticCountUpCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Steps"))
            {
                stepsCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.StepsItem"))
            {
                stepsItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.StepsItemIndicator"))
            {
                stepsItemIndicatorCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Calendar"))
            {
                calendarCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CalendarItem"))
            {
                calendarItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.BaseCalendarDayButton"))
            {
                calendarDayButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.BaseCalendarButton"))
            {
                calendarButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Select"))
            {
                selectCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBox"))
            {
                comboBoxCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBoxItem"))
            {
                comboBoxItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBoxHandle"))
            {
                comboBoxHandleCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ComboBoxAccessoryHost"))
            {
                comboBoxAccessoryHostCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.ColorPicker"))
            {
                colorPickerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.GradientColorPicker"))
            {
                gradientColorPickerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DatePicker"))
            {
                datePickerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RangeDatePicker"))
            {
                rangeDatePickerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Primitives.InfoPickerInput"))
            {
                infoPickerInputCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Primitives.PickerAccessoryHost"))
            {
                pickerAccessoryHostCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DatePickerPresenter"))
            {
                datePickerPresenterCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RangeDatePickerPresenter"))
            {
                rangeDatePickerPresenterCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CalendarView.Calendar"))
            {
                datePickerCalendarCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CalendarView.CalendarItem"))
            {
                datePickerCalendarItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CalendarView.CalendarDayButton"))
            {
                datePickerCalendarDayButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CalendarView.CalendarButton"))
            {
                datePickerCalendarButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.TimeView"))
            {
                timeViewCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DateTimePickerPanel"))
            {
                dateTimePickerPanelCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AbstractAutoComplete"))
            {
                autoCompleteCount++;
                if (HasFieldValue(visual, "AtomUI.Desktop.Controls.AbstractAutoComplete", "_popup"))
                {
                    autoCompletePopupFieldCount++;
                }
                if (HasFieldValue(visual, "AtomUI.Desktop.Controls.AbstractAutoComplete", "_candidateList"))
                {
                    autoCompleteCandidateListFieldCount++;
                }
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AutoCompleteSearchEdit"))
            {
                autoCompleteSearchEditCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.AutoCompleteTextArea"))
            {
                autoCompleteTextAreaCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Mentions"))
            {
                mentionsCount++;
                if (HasFieldValue(visual, "AtomUI.Desktop.Controls.Mentions", "_popup"))
                {
                    mentionsPopupFieldCount++;
                }
                if (HasFieldValue(visual, "AtomUI.Desktop.Controls.Mentions", "_candidateList"))
                {
                    mentionsCandidateListFieldCount++;
                }
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.MentionTextArea"))
            {
                mentionTextAreaCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Primitives.CandidateList"))
            {
                candidateListCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.TreeSelect"))
            {
                treeSelectCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Cascader"))
            {
                cascaderCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Menu"))
            {
                menuCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.MenuItem"))
            {
                menuItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.BaseNavMenuItemHeader"))
            {
                navMenuItemHeaderCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CheckBox"))
            {
                checkBoxCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.CheckBoxGroup"))
            {
                checkBoxGroupCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Controls.CheckBoxIndicator"))
            {
                checkBoxIndicatorCount++;
            }
            if (visual is Control { Name: "CheckedMark" })
            {
                checkBoxCheckedMarkCount++;
            }
            if (visual is Control { Name: "TristateMark" })
            {
                checkBoxTristateMarkCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RadioButton"))
            {
                radioButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.RadioButtonGroup"))
            {
                radioButtonGroupCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Controls.Commons.RadioIndicator"))
            {
                radioIndicatorCount++;
            }
            if (type.Name == "WaveSpiritDecorator")
            {
                waveSpiritDecoratorCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Descriptions"))
            {
                descriptionsCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DescriptionDefaultItem"))
            {
                descriptionDefaultItemCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DescriptionBorderedItemLabel"))
            {
                descriptionBorderedItemLabelCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DescriptionBorderedItemContent"))
            {
                descriptionBorderedItemContentCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Drawer"))
            {
                drawerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.Dialog"))
            {
                dialogCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.MessageBox"))
            {
                messageBoxCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.OverlayDialogPresenter"))
            {
                overlayDialogPresenterCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogWindow"))
            {
                dialogWindowCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogSurface"))
            {
                dialogSurfaceCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogButtonBox"))
            {
                dialogButtonBoxCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogButton"))
            {
                dialogButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.DialogCaptionButton"))
            {
                dialogCaptionButtonCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.OverlayDialogMask"))
            {
                overlayDialogMaskCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.OverlayDialogResizer"))
            {
                overlayDialogResizerCount++;
            }
            if (IsTypeOrDerived(type, "AtomUI.Desktop.Controls.MessageBoxContent"))
            {
                messageBoxContentCount++;
            }
        }

        return new RouteStats(
            visuals.Count,
            root.GetSelfAndLogicalDescendants().Count(),
            spaceCount,
            compactSpaceCount,
            compactSpaceItemCount,
            iconCount,
            iconPresenterCount,
            pathIconCount,
            avatarCount,
            avatarGroupCount,
            imageCount,
            svgCount,
            textBlockCount,
            flyoutHostCount,
            countBadgeCount,
            dotBadgeCount,
            ribbonBadgeCount,
            countBadgeAdornerCount,
            dotBadgeAdornerCount,
            ribbonBadgeAdornerCount,
            dotBadgeIndicatorCount,
            motionTargetCount,
            labelCount,
            alertCount,
            marqueeLabelCount,
            lineEditCount,
            lineEditDirectCount,
            searchEditCount,
            textAreaCount,
            showCaseItemCount,
            iconGalleryCount,
            iconInfoItemCount,
            addOnDecoratedBoxCount,
            buttonCount,
            iconButtonCount,
            toggleIconButtonCount,
            buttonSpinnerCount,
            buttonSpinnerDecoratedBoxCount,
            buttonSpinnerHandleCount,
            buttonSpinnerContentPanelCount,
            cardCount,
            cardActionPanelCount,
            cardActionButtonCount,
            cardMetaContentCount,
            cardGridContentCount,
            cardGridItemCount,
            cardTabsContentCount,
            collapseCount,
            collapseItemCount,
            collapseContentViewportCount,
            collapseExpandButtonCount,
            collapseAddOnPresenterCount,
            carouselCount,
            carouselPageCount,
            carouselPaginationCount,
            carouselPageIndicatorCount,
            carouselNavButtonCount,
            carouselLayoutTransformCount,
            carouselProgressBorderCount,
            carouselPageTransitionFieldCount,
            carouselAutoPlayTimerFieldCount,
            carouselIndicatorAnimationFieldCount,
            skeletonCount,
            skeletonLineCount,
            statisticCount,
            timerStatisticCount,
            statisticCountUpCount,
            stepsCount,
            stepsItemCount,
            stepsItemIndicatorCount,
            calendarCount,
            calendarItemCount,
            calendarDayButtonCount,
            calendarButtonCount,
            selectCount,
            comboBoxCount,
            comboBoxItemCount,
            comboBoxHandleCount,
            comboBoxAccessoryHostCount,
            colorPickerCount,
            gradientColorPickerCount,
            datePickerCount,
            rangeDatePickerCount,
            infoPickerInputCount,
            pickerAccessoryHostCount,
            datePickerPresenterCount,
            rangeDatePickerPresenterCount,
            datePickerCalendarCount,
            datePickerCalendarItemCount,
            datePickerCalendarDayButtonCount,
            datePickerCalendarButtonCount,
            timeViewCount,
            dateTimePickerPanelCount,
            autoCompleteCount,
            autoCompleteSearchEditCount,
            autoCompleteTextAreaCount,
            autoCompletePopupFieldCount,
            autoCompleteCandidateListFieldCount,
            mentionsCount,
            mentionTextAreaCount,
            mentionsPopupFieldCount,
            mentionsCandidateListFieldCount,
            candidateListCount,
            treeSelectCount,
            cascaderCount,
            menuCount,
            menuItemCount,
            navMenuItemHeaderCount,
            checkBoxCount,
            checkBoxGroupCount,
            checkBoxIndicatorCount,
            checkBoxCheckedMarkCount,
            checkBoxTristateMarkCount,
            radioButtonCount,
            radioButtonGroupCount,
            radioIndicatorCount,
            waveSpiritDecoratorCount,
            descriptionsCount,
            descriptionDefaultItemCount,
            descriptionBorderedItemLabelCount,
            descriptionBorderedItemContentCount,
            drawerCount,
            dialogCount,
            messageBoxCount,
            overlayDialogPresenterCount,
            dialogWindowCount,
            dialogSurfaceCount,
            dialogButtonBoxCount,
            dialogButtonCount,
            dialogCaptionButtonCount,
            overlayDialogMaskCount,
            overlayDialogResizerCount,
            messageBoxContentCount,
            groupBoxCount,
            groupBoxHeaderIconPresenterCount);
    }

    public bool HasSameShape(RouteStats other)
    {
        return VisualCount == other.VisualCount &&
               LogicalCount == other.LogicalCount &&
               SpaceCount == other.SpaceCount &&
               CompactSpaceCount == other.CompactSpaceCount &&
               CompactSpaceItemCount == other.CompactSpaceItemCount &&
               IconCount == other.IconCount &&
               IconPresenterCount == other.IconPresenterCount &&
               PathIconCount == other.PathIconCount &&
               AvatarCount == other.AvatarCount &&
               AvatarGroupCount == other.AvatarGroupCount &&
               ImageCount == other.ImageCount &&
               SvgCount == other.SvgCount &&
               TextBlockCount == other.TextBlockCount &&
               FlyoutHostCount == other.FlyoutHostCount &&
               CountBadgeCount == other.CountBadgeCount &&
               DotBadgeCount == other.DotBadgeCount &&
               RibbonBadgeCount == other.RibbonBadgeCount &&
               CountBadgeAdornerCount == other.CountBadgeAdornerCount &&
               DotBadgeAdornerCount == other.DotBadgeAdornerCount &&
               RibbonBadgeAdornerCount == other.RibbonBadgeAdornerCount &&
               DotBadgeIndicatorCount == other.DotBadgeIndicatorCount &&
               MotionTargetCount == other.MotionTargetCount &&
               LabelCount == other.LabelCount &&
               AlertCount == other.AlertCount &&
               MarqueeLabelCount == other.MarqueeLabelCount &&
               LineEditCount == other.LineEditCount &&
               LineEditDirectCount == other.LineEditDirectCount &&
               SearchEditCount == other.SearchEditCount &&
               TextAreaCount == other.TextAreaCount &&
               ShowCaseItemCount == other.ShowCaseItemCount &&
               IconGalleryCount == other.IconGalleryCount &&
               IconInfoItemCount == other.IconInfoItemCount &&
               AddOnDecoratedBoxCount == other.AddOnDecoratedBoxCount &&
               ButtonCount == other.ButtonCount &&
               IconButtonCount == other.IconButtonCount &&
               ToggleIconButtonCount == other.ToggleIconButtonCount &&
               ButtonSpinnerCount == other.ButtonSpinnerCount &&
               ButtonSpinnerDecoratedBoxCount == other.ButtonSpinnerDecoratedBoxCount &&
               ButtonSpinnerHandleCount == other.ButtonSpinnerHandleCount &&
               ButtonSpinnerContentPanelCount == other.ButtonSpinnerContentPanelCount &&
               CardCount == other.CardCount &&
               CardActionPanelCount == other.CardActionPanelCount &&
               CardActionButtonCount == other.CardActionButtonCount &&
               CardMetaContentCount == other.CardMetaContentCount &&
               CardGridContentCount == other.CardGridContentCount &&
               CardGridItemCount == other.CardGridItemCount &&
               CardTabsContentCount == other.CardTabsContentCount &&
               CollapseCount == other.CollapseCount &&
               CollapseItemCount == other.CollapseItemCount &&
               CollapseContentViewportCount == other.CollapseContentViewportCount &&
               CollapseExpandButtonCount == other.CollapseExpandButtonCount &&
               CollapseAddOnPresenterCount == other.CollapseAddOnPresenterCount &&
               CarouselCount == other.CarouselCount &&
               CarouselPageCount == other.CarouselPageCount &&
               CarouselPaginationCount == other.CarouselPaginationCount &&
               CarouselPageIndicatorCount == other.CarouselPageIndicatorCount &&
               CarouselNavButtonCount == other.CarouselNavButtonCount &&
               CarouselLayoutTransformCount == other.CarouselLayoutTransformCount &&
               CarouselProgressBorderCount == other.CarouselProgressBorderCount &&
               CarouselPageTransitionFieldCount == other.CarouselPageTransitionFieldCount &&
               CarouselAutoPlayTimerFieldCount == other.CarouselAutoPlayTimerFieldCount &&
               CarouselIndicatorAnimationFieldCount == other.CarouselIndicatorAnimationFieldCount &&
               SkeletonCount == other.SkeletonCount &&
               SkeletonLineCount == other.SkeletonLineCount &&
               StatisticCount == other.StatisticCount &&
               TimerStatisticCount == other.TimerStatisticCount &&
               StatisticCountUpCount == other.StatisticCountUpCount &&
               StepsCount == other.StepsCount &&
               StepsItemCount == other.StepsItemCount &&
               StepsItemIndicatorCount == other.StepsItemIndicatorCount &&
               CalendarCount == other.CalendarCount &&
               CalendarItemCount == other.CalendarItemCount &&
               CalendarDayButtonCount == other.CalendarDayButtonCount &&
               CalendarButtonCount == other.CalendarButtonCount &&
               SelectCount == other.SelectCount &&
               ComboBoxCount == other.ComboBoxCount &&
               ComboBoxItemCount == other.ComboBoxItemCount &&
               ComboBoxHandleCount == other.ComboBoxHandleCount &&
               ComboBoxAccessoryHostCount == other.ComboBoxAccessoryHostCount &&
               ColorPickerCount == other.ColorPickerCount &&
               GradientColorPickerCount == other.GradientColorPickerCount &&
               DatePickerCount == other.DatePickerCount &&
               RangeDatePickerCount == other.RangeDatePickerCount &&
               InfoPickerInputCount == other.InfoPickerInputCount &&
               PickerAccessoryHostCount == other.PickerAccessoryHostCount &&
               DatePickerPresenterCount == other.DatePickerPresenterCount &&
               RangeDatePickerPresenterCount == other.RangeDatePickerPresenterCount &&
               DatePickerCalendarCount == other.DatePickerCalendarCount &&
               DatePickerCalendarItemCount == other.DatePickerCalendarItemCount &&
               DatePickerCalendarDayButtonCount == other.DatePickerCalendarDayButtonCount &&
               DatePickerCalendarButtonCount == other.DatePickerCalendarButtonCount &&
               TimeViewCount == other.TimeViewCount &&
               DateTimePickerPanelCount == other.DateTimePickerPanelCount &&
               AutoCompleteCount == other.AutoCompleteCount &&
               AutoCompleteSearchEditCount == other.AutoCompleteSearchEditCount &&
               AutoCompleteTextAreaCount == other.AutoCompleteTextAreaCount &&
               AutoCompletePopupFieldCount == other.AutoCompletePopupFieldCount &&
               AutoCompleteCandidateListFieldCount == other.AutoCompleteCandidateListFieldCount &&
               MentionsCount == other.MentionsCount &&
               MentionTextAreaCount == other.MentionTextAreaCount &&
               MentionsPopupFieldCount == other.MentionsPopupFieldCount &&
               MentionsCandidateListFieldCount == other.MentionsCandidateListFieldCount &&
               CandidateListCount == other.CandidateListCount &&
               TreeSelectCount == other.TreeSelectCount &&
               CascaderCount == other.CascaderCount &&
               MenuCount == other.MenuCount &&
               MenuItemCount == other.MenuItemCount &&
               NavMenuItemHeaderCount == other.NavMenuItemHeaderCount &&
               CheckBoxCount == other.CheckBoxCount &&
               CheckBoxGroupCount == other.CheckBoxGroupCount &&
               CheckBoxIndicatorCount == other.CheckBoxIndicatorCount &&
               CheckBoxCheckedMarkCount == other.CheckBoxCheckedMarkCount &&
               CheckBoxTristateMarkCount == other.CheckBoxTristateMarkCount &&
               RadioButtonCount == other.RadioButtonCount &&
               RadioButtonGroupCount == other.RadioButtonGroupCount &&
               RadioIndicatorCount == other.RadioIndicatorCount &&
               WaveSpiritDecoratorCount == other.WaveSpiritDecoratorCount &&
               DescriptionsCount == other.DescriptionsCount &&
               DescriptionDefaultItemCount == other.DescriptionDefaultItemCount &&
               DescriptionBorderedItemLabelCount == other.DescriptionBorderedItemLabelCount &&
               DescriptionBorderedItemContentCount == other.DescriptionBorderedItemContentCount &&
               DrawerCount == other.DrawerCount &&
               DialogCount == other.DialogCount &&
               MessageBoxCount == other.MessageBoxCount &&
               OverlayDialogPresenterCount == other.OverlayDialogPresenterCount &&
               DialogWindowCount == other.DialogWindowCount &&
               DialogSurfaceCount == other.DialogSurfaceCount &&
               DialogButtonBoxCount == other.DialogButtonBoxCount &&
               DialogButtonCount == other.DialogButtonCount &&
               DialogCaptionButtonCount == other.DialogCaptionButtonCount &&
               OverlayDialogMaskCount == other.OverlayDialogMaskCount &&
               OverlayDialogResizerCount == other.OverlayDialogResizerCount &&
               MessageBoxContentCount == other.MessageBoxContentCount &&
               GroupBoxCount == other.GroupBoxCount &&
               GroupBoxHeaderIconPresenterCount == other.GroupBoxHeaderIconPresenterCount;
    }

    private static bool IsTypeOrDerived(Type type, string fullName)
    {
        while (true)
        {
            if (type.FullName == fullName)
            {
                return true;
            }

            if (type.BaseType is null)
            {
                return false;
            }

            type = type.BaseType;
        }
    }

    private static bool HasFieldValue(object target, string declaringTypeName, string fieldName)
    {
        var type = target.GetType();
        while (type is not null)
        {
            if (type.FullName == declaringTypeName)
            {
                var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                return field?.GetValue(target) is not null;
            }
            type = type.BaseType;
        }
        return false;
    }
}

internal sealed record SourceXamlStats(
    string SourcePath,
    bool IsAvailable,
    int AntDesignIconProviderCount,
    int SpaceCount,
    int CompactSpaceCount,
    int CompactSpaceFillerCount,
    int CompactSpaceAddOnCount,
    int IconPresenterCount,
    int IconGalleryCount,
    int AvatarCount,
    int AvatarGroupCount,
    int CountBadgeCount,
    int DotBadgeCount,
    int RibbonBadgeCount,
    int CardCount,
    int CardActionButtonCount,
    int CardGridContentCount,
    int CardGridItemCount,
    int CardMetaContentCount,
    int CardTabsContentCount,
    int CollapseCount,
    int CollapseItemCount,
    int CarouselCount,
    int CarouselPageCount,
    int LineEditDirectCount,
    int SearchEditCount,
    int TextAreaCount,
    int MentionsCount,
    int ButtonCount,
    int ButtonSpinnerCount,
    int ToggleIconButtonCount,
    int DatePickerCount,
    int RangeDatePickerCount,
    int SelectCount,
    int ComboBoxCount,
    int ComboBoxItemCount,
    int TreeSelectCount,
    int CascaderCount,
    int MenuCount,
    int MenuItemCount,
    int CheckBoxCount,
    int CheckBoxGroupCount,
    int RadioButtonCount,
    int RadioButtonGroupCount,
    int DescriptionsCount,
    int DescriptionItemCount,
    int DrawerCount,
    int AlertCount,
    int MessageMarqueeEnabledCount,
    int ShowCaseItemCount,
    int GroupBoxCount,
    int GroupBoxHeaderIconPresenterCount)
{
    private const string AtomNamespace = "https://atomui.net";
    private const string GalleryNamespace = "https://atomui.net/oss-controls/gallery";

    public static SourceXamlStats Read(string relativePath)
    {
        var sourcePath = Path.GetFullPath(relativePath);
        if (!File.Exists(sourcePath))
        {
            return new SourceXamlStats(sourcePath, false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        var text     = File.ReadAllText(sourcePath);
        var document = XDocument.Load(sourcePath, LoadOptions.None);
        return new SourceXamlStats(
            sourcePath,
            true,
            CountText(text, "AntDesignIconProvider"),
            CountElements(document, AtomNamespace, "Space"),
            CountElements(document, AtomNamespace, "CompactSpace"),
            CountElements(document, AtomNamespace, "CompactSpaceFiller"),
            CountElements(document, AtomNamespace, "CompactSpaceAddOn"),
            CountElements(document, AtomNamespace, "IconPresenter"),
            CountElements(document, GalleryNamespace, "IconGallery"),
            CountElements(document, AtomNamespace, "Avatar"),
            CountElements(document, AtomNamespace, "AvatarGroup"),
            CountElements(document, AtomNamespace, "CountBadge"),
            CountElements(document, AtomNamespace, "DotBadge"),
            CountElements(document, AtomNamespace, "RibbonBadge"),
            CountElements(document, AtomNamespace, "Card"),
            CountElements(document, AtomNamespace, "CardActionButton"),
            CountElements(document, AtomNamespace, "CardGridContent"),
            CountElements(document, AtomNamespace, "CardGridItem"),
            CountElements(document, AtomNamespace, "CardMetaContent"),
            CountElements(document, AtomNamespace, "CardTabsContent"),
            CountElements(document, AtomNamespace, "Collapse"),
            CountElements(document, AtomNamespace, "CollapseItem"),
            CountElements(document, AtomNamespace, "Carousel"),
            CountElements(document, AtomNamespace, "CarouselPage"),
            CountElements(document, AtomNamespace, "LineEdit"),
            CountElements(document, AtomNamespace, "SearchEdit"),
            CountElements(document, AtomNamespace, "TextArea"),
            CountElements(document, AtomNamespace, "Mentions"),
            CountElements(document, AtomNamespace, "Button"),
            CountElements(document, AtomNamespace, "ButtonSpinner"),
            CountElements(document, AtomNamespace, "ToggleIconButton"),
            CountElements(document, AtomNamespace, "DatePicker"),
            CountElements(document, AtomNamespace, "RangeDatePicker"),
            CountElements(document, AtomNamespace, "Select"),
            CountElements(document, AtomNamespace, "ComboBox"),
            CountElements(document, AtomNamespace, "ComboBoxItem"),
            CountElements(document, AtomNamespace, "TreeSelect"),
            CountElements(document, AtomNamespace, "Cascader"),
            CountElements(document, AtomNamespace, "Menu"),
            CountElements(document, AtomNamespace, "MenuItem"),
            CountElements(document, AtomNamespace, "CheckBox"),
            CountElements(document, AtomNamespace, "CheckBoxGroup"),
            CountElements(document, AtomNamespace, "RadioButton"),
            CountElements(document, AtomNamespace, "RadioButtonGroup"),
            CountElements(document, AtomNamespace, "Descriptions"),
            CountElements(document, AtomNamespace, "DescriptionItem"),
            CountElements(document, AtomNamespace, "Drawer"),
            CountElements(document, AtomNamespace, "Alert"),
            CountText(text, "IsMessageMarqueeEnabled"),
            CountElements(document, GalleryNamespace, "ShowCaseItem"),
            CountElements(document, AtomNamespace, "GroupBox"),
            CountGroupBoxHeaderIcons(document));
    }

    public string RenderMarkdown()
    {
        if (!IsAvailable)
        {
            return $"`{SourcePath}` was not found.";
        }

        var lineEditTotal = LineEditDirectCount + SearchEditCount;
        var builder       = new StringBuilder();
        builder.AppendLine("| Source | AntDesignIconProvider | Space | CompactSpace | CompactSpaceFiller | CompactSpaceAddOn | IconPresenter | IconGallery | Avatar | AvatarGroup | CountBadge | DotBadge | RibbonBadge | Card | CardActionButton | CardGridContent | CardGridItem | CardMetaContent | CardTabsContent | Collapse | CollapseItem | Carousel | CarouselPage | LineEdit direct | SearchEdit | LineEdit total | TextArea | Mentions | Button | ButtonSpinner | ToggleIconButton | DatePicker | RangeDatePicker | Select | ComboBox | ComboBoxItem | TreeSelect | Cascader | Menu | MenuItem | CheckBox | CheckBoxGroup | RadioButton | RadioButtonGroup | Descriptions | DescriptionItem | Drawer | Alert | IsMessageMarqueeEnabled | ShowCaseItem | GroupBox | GroupBoxHeaderIcon |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        builder.Append("| `");
        builder.Append(SourcePath);
        builder.Append("` | ");
        builder.Append(AntDesignIconProviderCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(SpaceCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CompactSpaceCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CompactSpaceFillerCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CompactSpaceAddOnCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(IconPresenterCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(IconGalleryCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(AvatarCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(AvatarGroupCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CountBadgeCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(DotBadgeCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(RibbonBadgeCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CardCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CardActionButtonCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CardGridContentCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CardGridItemCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CardMetaContentCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CardTabsContentCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CollapseCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CollapseItemCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CarouselCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CarouselPageCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(LineEditDirectCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(SearchEditCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(lineEditTotal.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(TextAreaCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(MentionsCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(ButtonCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(ButtonSpinnerCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(ToggleIconButtonCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(DatePickerCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(RangeDatePickerCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(SelectCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(ComboBoxCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(ComboBoxItemCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(TreeSelectCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CascaderCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(MenuCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(MenuItemCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CheckBoxCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(CheckBoxGroupCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(RadioButtonCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(RadioButtonGroupCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(DescriptionsCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(DescriptionItemCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(DrawerCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(AlertCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(MessageMarqueeEnabledCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(ShowCaseItemCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(GroupBoxCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" | ");
        builder.Append(GroupBoxHeaderIconPresenterCount.ToString(CultureInfo.InvariantCulture));
        builder.Append(" |");
        return builder.ToString();
    }

    private static int CountElements(XDocument document, string ns, string localName)
    {
        return document.Descendants()
                       .Count(element => element.Name.NamespaceName == ns &&
                                         element.Name.LocalName == localName);
    }

    private static int CountText(string source, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    private static int CountGroupBoxHeaderIcons(XDocument document)
    {
        return document.Descendants()
                       .Count(element => element.Name.NamespaceName == AtomNamespace &&
                                         element.Name.LocalName == "GroupBox" &&
                                         element.Attributes().Any(attribute =>
                                             attribute.Name.LocalName == "HeaderIcon"));
    }
}
