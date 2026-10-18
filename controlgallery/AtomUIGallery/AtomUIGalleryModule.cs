using AtomUI.Icons.AntDesign;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Localization;
using AtomUI.Toolkits.GalleryBase.Routing;
using AtomUIGallery.Localization;
using AtomUIGallery.SourceCode;
using AtomUIGallery.ShowCases.Alert;
using AtomUIGallery.ShowCases.AutoComplete;
using AtomUIGallery.ShowCases.Avatar;
using AtomUIGallery.ShowCases.Badge;
using AtomUIGallery.ShowCases.BorderBeam;
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
using AtomUIGallery.ShowCases.Community;
using AtomUIGallery.ShowCases.ComboBox;
using AtomUIGallery.ShowCases.CustomizeTheme;
using AtomUIGallery.ShowCases.DataGrid;
using AtomUIGallery.ShowCases.DatePicker;
using AtomUIGallery.ShowCases.DateViewer;
using AtomUIGallery.ShowCases.Descriptions;
using AtomUIGallery.ShowCases.Drawer;
using AtomUIGallery.ShowCases.DropdownButton;
using AtomUIGallery.ShowCases.Empty;
using AtomUIGallery.ShowCases.Expander;
using AtomUIGallery.ShowCases.FlexPanel;
using AtomUIGallery.ShowCases.FloatButton;
using AtomUIGallery.ShowCases.Form;
using AtomUIGallery.ShowCases.Grid;
using AtomUIGallery.ShowCases.GroupBox;
using AtomUIGallery.ShowCases.Icon;
using AtomUIGallery.ShowCases.ImagePreviewer;
using AtomUIGallery.ShowCases.InfoFlyout;
using AtomUIGallery.ShowCases.LineEdit;
using AtomUIGallery.ShowCases.List;
using AtomUIGallery.ShowCases.Masonry;
using AtomUIGallery.ShowCases.Mentions;
using AtomUIGallery.ShowCases.Menu;
using AtomUIGallery.ShowCases.Message;
using AtomUIGallery.ShowCases.Modal;
using AtomUIGallery.ShowCases.Notification;
using AtomUIGallery.ShowCases.NumberUpDown;
using AtomUIGallery.ShowCases.Overview;
using AtomUIGallery.ShowCases.Pagination;
using AtomUIGallery.ShowCases.Palette;
using AtomUIGallery.ShowCases.PopupConfirm;
using AtomUIGallery.ShowCases.ProgressBar;
using AtomUIGallery.ShowCases.QRCode;
using AtomUIGallery.ShowCases.RadioButton;
using AtomUIGallery.ShowCases.Rate;
using AtomUIGallery.ShowCases.Result;
using AtomUIGallery.ShowCases.Segmented;
using AtomUIGallery.ShowCases.Select;
using AtomUIGallery.ShowCases.Separator;
using AtomUIGallery.ShowCases.Skeleton;
using AtomUIGallery.ShowCases.Slider;
using AtomUIGallery.ShowCases.Space;
using AtomUIGallery.ShowCases.Spin;
using AtomUIGallery.ShowCases.Splash;
using AtomUIGallery.ShowCases.SplitButton;
using AtomUIGallery.ShowCases.Splitter;
using AtomUIGallery.ShowCases.Statistic;
using AtomUIGallery.ShowCases.Steps;
using AtomUIGallery.ShowCases.TabControl;
using AtomUIGallery.ShowCases.TabStrip;
using AtomUIGallery.ShowCases.Tag;
using AtomUIGallery.ShowCases.TimePicker;
using AtomUIGallery.ShowCases.Timeline;
using AtomUIGallery.ShowCases.ToggleSwitch;
using AtomUIGallery.ShowCases.Tooltip;
using AtomUIGallery.ShowCases.Tour;
using AtomUIGallery.ShowCases.Transfer;
using AtomUIGallery.ShowCases.TreeSelect;
using AtomUIGallery.ShowCases.TreeView;
using AtomUIGallery.ShowCases.Upload;
using AtomUIGallery.ShowCases.Watermark;
using AtomUIGallery.ShowCases.Window;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery;

public static class AtomUIGalleryModule
{
    // Only fixed types and deferred factories are shared; application configuration stays application-owned.
    private static readonly Lazy<IReadOnlyList<GalleryRouteDescriptor>> RouteDescriptors = new(CreateRouteDescriptors);

    public static GalleryBaseConfiguration CreateConfiguration()
    {
        var options = new GalleryBaseOptions();
        Configure(options);
        return options.BuildConfiguration();
    }

    public static GalleryBaseConfiguration GetConfiguration()
    {
        return GalleryBaseConfigurationProvider.Current ?? CreateConfiguration();
    }

    public static void Configure(GalleryBaseOptions options)
    {
        ConfigureBranding(options.Branding);
        ConfigureNavigation(options.Navigation);
        foreach (var route in RouteDescriptors.Value)
        {
            options.Routes.Map(route);
        }
        ConfigureSourceCodeDisplay(options.SourceCodeDisplay);
    }

    public static void RegisterViews(DefaultViewLocator locator)
    {
        foreach (var route in RouteDescriptors.Value)
        {
            route.RegisterView(locator);
        }
    }

    private static IReadOnlyList<GalleryRouteDescriptor> CreateRouteDescriptors()
    {
        var routes = new GalleryRouteRegistry();
        ConfigureRoutes(routes);
        return routes.Routes;
    }

    private static void ConfigureBranding(GalleryBrandingOptions branding)
    {
        branding.AppName     = "AtomUI Gallery";
        branding.Logo        = "avares://AtomUIGallery/Assets/atomui-oss.svg";
        branding.VersionText = GalleryVersionInfo.DisplayVersion;
        branding.Links.Add(new GalleryLink("Website", "https://www.atomui.net", Icon(AntDesignIconKind.GlobalOutlined)));
        branding.Links.Add(new GalleryLink("Gitee", "https://gitee.com/chinware/AtomUI", Icon(AntDesignIconKind.GiteeOutlined)));
        branding.Links.Add(new GalleryLink("GitHub", "https://github.com/chinware/atomui", Icon(AntDesignIconKind.GithubOutlined)));
    }

    private static void ConfigureSourceCodeDisplay(GallerySourceCodeDisplayOptions sourceCodeDisplay)
    {
        sourceCodeDisplay.IsEnabled       = true;
        sourceCodeDisplay.SnippetProvider = new AtomUIGalleryShowCaseCodeSnippetProvider();
    }

    private static void ConfigureNavigation(AtomUI.Toolkits.GalleryBase.Navigation.GalleryNavigationBuilder navigation)
    {
        navigation.DefaultRoute = OverviewViewModel.ID;
        navigation.DefaultOpenKeys.Add("Components");

        navigation.AddPage(OverviewViewModel.ID, Nav(CaseNavigationLangResourceKind.Overview, "Overview"), Icon(AntDesignIconKind.HomeOutlined));
        navigation.AddPage(CommunityViewModel.ID, Nav(CaseNavigationLangResourceKind.Community, "Community"), Icon(AntDesignIconKind.TeamOutlined));

        var components = navigation.AddGroup("Components", Nav(CaseNavigationLangResourceKind.Components, "Components"), Icon(AntDesignIconKind.AppstoreOutlined));

        components.AddGroup("General", Nav(CaseNavigationLangResourceKind.General, "General"), Icon(AntDesignIconKind.ControlOutlined))
                  .AddPage(PaletteViewModel.ID, Nav(CaseNavigationLangResourceKind.General_Palette, "Palette"))
                  .AddPage(IconViewModel.ID, Nav(CaseNavigationLangResourceKind.General_Icons, "Icons"))
                  .AddPage(ButtonViewModel.ID, Nav(CaseNavigationLangResourceKind.General_Button, "Button"))
                  .AddPage(FloatButtonViewModel.ID, Nav(CaseNavigationLangResourceKind.General_FloatButton, "FloatButton"))
                  .AddPage(SplitButtonViewModel.ID, Nav(CaseNavigationLangResourceKind.General_SplitButton, "SplitButton"))
                  .AddPage(SeparatorViewModel.ID, Nav(CaseNavigationLangResourceKind.General_Separator, "Separator"))
                  .AddPage(CustomizeThemeViewModel.ID, Nav(CaseNavigationLangResourceKind.General_CustomizeTheme, "CustomizeTheme"))
                  .AddPage(WindowViewModel.ID, Nav(CaseNavigationLangResourceKind.General_Window, "Window"));

        components.AddGroup("Layout", Nav(CaseNavigationLangResourceKind.Layout, "Layout"), Icon(AntDesignIconKind.LayoutOutlined))
                  .AddPage(FlexPanelViewModel.ID, Nav(CaseNavigationLangResourceKind.Layout_FlexPanel, "FlexPanel"))
                  .AddPage(GridViewModel.ID, Nav(CaseNavigationLangResourceKind.Layout_Grid, "Grid"))
                  .AddPage(SpaceViewModel.ID, Nav(CaseNavigationLangResourceKind.Layout_Space, "Space"))
                  .AddPage(SplitterViewModel.ID, Nav(CaseNavigationLangResourceKind.Layout_Splitter, "Splitter"))
                  .AddPage(MasonryViewModel.ID, Nav(CaseNavigationLangResourceKind.Layout_Masonry, "Masonry"));

        components.AddGroup("Navigation", Nav(CaseNavigationLangResourceKind.Navigation, "Navigation"), Icon(AntDesignIconKind.MenuOutlined))
                  .AddPage(BreadcrumbViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_Breadcrumb, "Breadcrumb"))
                  .AddPage(ButtonSpinnerViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_ButtonSpinner, "ButtonSpinner"))
                  .AddPage(ComboBoxViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_ComboBox, "ComboBox"))
                  .AddPage(DropdownButtonViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_DropdownButton, "DropdownButton"))
                  .AddPage(MenuViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_Menu, "Menu"))
                  .AddPage(PaginationViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_Pagination, "Pagination"))
                  .AddPage(StepsViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_Steps, "Steps"))
                  .AddPage(TabControlViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_TabControl, "TabControl"))
                  .AddPage(TabStripViewModel.ID, Nav(CaseNavigationLangResourceKind.Navigation_TabStrip, "TabStrip"));

        components.AddGroup("DataEntry", Nav(CaseNavigationLangResourceKind.DataEntry, "Data Entry"), Icon(AntDesignIconKind.FormOutlined))
                  .AddPage(AutoCompleteViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_AutoComplete, "AutoComplete"))
                  .AddPage(CascaderViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Cascader, "Cascader"))
                  .AddPage(CheckBoxViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_CheckBox, "CheckBox"))
                  .AddPage(ColorPickerViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_ColorPicker, "ColorPicker"))
                  .AddPage(DatePickerViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_DatePicker, "DatePicker"))
                  .AddPage(TimePickerViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_TimePicker, "TimePicker"))
                  .AddPage(FormViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Form, "Form"))
                  .AddPage(LineEditViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_LineEdit, "LineEdit"))
                  .AddPage(MentionsViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Mentions, "Mentions"))
                  .AddPage(NumberUpDownViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_NumberUpDown, "NumberUpDown"))
                  .AddPage(RadioButtonViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_RadioButton, "RadioButton"))
                  .AddPage(RateViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Rate, "Rate"))
                  .AddPage(SelectViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Select, "Select"))
                  .AddPage(SliderViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Slider, "Slider"))
                  .AddPage(ToggleSwitchViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_ToggleSwitch, "ToggleSwitch"))
                  .AddPage(TreeSelectViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_TreeSelect, "TreeSelect"))
                  .AddPage(TransferViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Transfer, "Transfer"))
                  .AddPage(UploadViewModel.ID, Nav(CaseNavigationLangResourceKind.DataEntry_Upload, "Upload"));

        components.AddGroup("DataDisplay", Nav(CaseNavigationLangResourceKind.DataDisplay, "Data Display"), Icon(AntDesignIconKind.DatabaseOutlined))
                  .AddPage(AvatarViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Avatar, "Avatar"))
                  .AddPage(BadgeViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Badge, "Badge"))
                  .AddPage(CalendarViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Calendar, "Calendar"))
                  .AddPage(CardViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Card, "Card"))
                  .AddPage(CarouselViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Carousel, "Carousel"))
                  .AddPage(CollapseViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Collapse, "Collapse"))
                  .AddPage(DescriptionsViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Descriptions, "Descriptions"))
                  .AddPage(DataGridViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_DataGrid, "DataGrid"))
                  .AddPage(DateViewerViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_DateViewer, "DateViewer"))
                  .AddPage(ExpanderViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Expander, "Expander"))
                  .AddPage(EmptyViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Empty, "Empty"))
                  .AddPage(ImagePreviewerViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_ImagePreviewer, "ImagePreviewer"))
                  .AddPage(GroupBoxViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_GroupBox, "GroupBox"))
                  .AddPage(InfoFlyoutViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_InfoFlyout, "InfoFlyout"))
                  .AddPage(ListViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_List, "List"))
                  .AddPage(QRCodeViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_QRCode, "QRCode"))
                  .AddPage(SegmentedViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Segmented, "Segmented"))
                  .AddPage(StatisticViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Statistic, "Statistic"))
                  .AddPage(TagViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Tag, "Tag"))
                  .AddPage(TimelineViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Timeline, "Timeline"))
                  .AddPage(TreeViewViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_TreeView, "TreeView"))
                  .AddPage(TooltipViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Tooltip, "Tooltip"))
                  .AddPage(TourViewModel.ID, Nav(CaseNavigationLangResourceKind.DataDisplay_Tour, "Tour"));

        components.AddGroup("Feedback", Nav(CaseNavigationLangResourceKind.Feedback, "Feedback"), Icon(AntDesignIconKind.BellOutlined))
                  .AddPage(AlertViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Alert, "Alert"))
                  .AddPage(DrawerViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Drawer, "Drawer"))
                  .AddPage(MessageViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Message, "Message"))
                  .AddPage(ModalViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Modal, "Modal"))
                  .AddPage(NotificationViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Notification, "Notification"))
                  .AddPage(PopupConfirmViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_PopupConfirm, "PopupConfirm"))
                  .AddPage(ProgressBarViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_ProgressBar, "ProgressBar"))
                  .AddPage(ResultViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Result, "Result"))
                  .AddPage(SkeletonViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Skeleton, "Skeleton"))
                  .AddPage(SpinViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Spin, "Spin"))
                  .AddPage(WatermarkViewModel.ID, Nav(CaseNavigationLangResourceKind.Feedback_Watermark, "Watermark"));

        components.AddGroup("Other", Nav(CaseNavigationLangResourceKind.Other, "Other"), Icon(AntDesignIconKind.BlockOutlined))
                  .AddPage(BorderBeamViewModel.ID, Nav(CaseNavigationLangResourceKind.Other_BorderBeam, "BorderBeam"))
                  .AddPage(SplashViewModel.ID, Nav(CaseNavigationLangResourceKind.Other_Splash, "Splash"));
    }

    private static GalleryLocalizedText<CaseNavigationLangResourceKind> Nav(CaseNavigationLangResourceKind kind,
                                                                            string fallback)
    {
        return new GalleryLocalizedText<CaseNavigationLangResourceKind>(kind, fallback);
    }

    private static Func<PathIcon> Icon(AntDesignIconKind kind)
    {
        return () => (PathIcon)new AntDesignIconProvider(kind).ProvideValue(null!);
    }

    private static void ConfigureRoutes(GalleryRouteRegistry routes)
    {
        routes.Map(OverviewViewModel.ID, screen => new OverviewViewModel(screen), () => new OverviewPage());
        routes.Map(CommunityViewModel.ID, screen => new CommunityViewModel(screen), () => new CommunityPage());
        routes.Map(PaletteViewModel.ID, screen => new PaletteViewModel(screen), () => new PaletteShowCase());
        routes.Map(IconViewModel.ID, screen => new IconViewModel(screen), () => new IconShowCase());
        routes.Map(CustomizeThemeViewModel.ID, screen => new CustomizeThemeViewModel(screen), () => new CustomizeThemeShowCase());
        routes.Map(ButtonViewModel.ID, screen => new ButtonViewModel(screen), () => new ButtonShowCase());
        routes.Map(FloatButtonViewModel.ID, screen => new FloatButtonViewModel(screen), () => new FloatButtonShowCase());
        routes.Map(SplitButtonViewModel.ID, screen => new SplitButtonViewModel(screen), () => new SplitButtonShowCase());
        routes.Map(SeparatorViewModel.ID, screen => new SeparatorViewModel(screen), () => new SeparatorShowCase());
        routes.Map(SplitterViewModel.ID, screen => new SplitterViewModel(screen), () => new SplitterShowCase());
        routes.Map(GridViewModel.ID, screen => new GridViewModel(screen), () => new GridShowCase());
        routes.Map(BreadcrumbViewModel.ID, screen => new BreadcrumbViewModel(screen), () => new BreadcrumbShowCase());
        routes.Map(ButtonSpinnerViewModel.ID, screen => new ButtonSpinnerViewModel(screen), () => new ButtonSpinnerShowCase());
        routes.Map(ComboBoxViewModel.ID, screen => new ComboBoxViewModel(screen), () => new ComboBoxShowCase());
        routes.Map(DropdownButtonViewModel.ID, screen => new DropdownButtonViewModel(screen), () => new DropdownButtonShowCase());
        routes.Map(MessageViewModel.ID, screen => new MessageViewModel(screen), () => new MessageShowCase());
        routes.Map(ModalViewModel.ID, screen => new ModalViewModel(screen), () => new ModalShowCase());
        routes.Map(NotificationViewModel.ID, screen => new NotificationViewModel(screen), () => new NotificationShowCase());
        routes.Map(PopupConfirmViewModel.ID, screen => new PopupConfirmViewModel(screen), () => new PopupConfirmShowCase());
        routes.Map(DrawerViewModel.ID, screen => new DrawerViewModel(screen), () => new DrawerShowCase());
        routes.Map(AlertViewModel.ID, screen => new AlertViewModel(screen), () => new AlertShowCase());
        routes.Map(SpinViewModel.ID, screen => new SpinViewModel(screen), () => new SpinShowCase());
        routes.Map(FlexPanelViewModel.ID, screen => new FlexPanelViewModel(screen), () => new FlexPanelShowCase());
        routes.Map(CarouselViewModel.ID, screen => new CarouselViewModel(screen), () => new CarouselShowCase());
        routes.Map(AvatarViewModel.ID, screen => new AvatarViewModel(screen), () => new AvatarShowCase());
        routes.Map(CalendarViewModel.ID, screen => new CalendarViewModel(screen), () => new CalendarShowCase());
        routes.Map(SkeletonViewModel.ID, screen => new SkeletonViewModel(screen), () => new SkeletonShowCase());
        routes.Map(CardViewModel.ID, screen => new CardViewModel(screen), () => new CardShowCase());
        routes.Map(CollapseViewModel.ID, screen => new CollapseViewModel(screen), () => new CollapseShowCase());
        routes.Map(DescriptionsViewModel.ID, screen => new DescriptionsViewModel(screen), () => new DescriptionsShowCase());
        routes.Map(EmptyViewModel.ID, screen => new EmptyViewModel(screen), () => new EmptyShowCase());
        routes.Map(ExpanderViewModel.ID, screen => new ExpanderViewModel(screen), () => new ExpanderShowCase());
        routes.Map(ImagePreviewerViewModel.ID, screen => new ImagePreviewerViewModel(screen), () => new ImagePreviewerShowCase());
        routes.Map(CheckBoxViewModel.ID, screen => new CheckBoxViewModel(screen), () => new CheckBoxShowCase());
        routes.Map(ToggleSwitchViewModel.ID, screen => new ToggleSwitchViewModel(screen), () => new ToggleSwitchShowCase());
        routes.Map(RadioButtonViewModel.ID, screen => new RadioButtonViewModel(screen), () => new RadioButtonShowCase());
        routes.Map(RateViewModel.ID, screen => new RateViewModel(screen), () => new RateShowCase());
        routes.Map(SliderViewModel.ID, screen => new SliderViewModel(screen), () => new SliderShowCase());
        routes.Map(ColorPickerViewModel.ID, screen => new ColorPickerViewModel(screen), () => new ColorPickerShowCase());
        routes.Map(TagViewModel.ID, screen => new TagViewModel(screen), () => new TagShowCase());
        routes.Map(SegmentedViewModel.ID, screen => new SegmentedViewModel(screen), () => new SegmentedShowCase());
        routes.Map(GroupBoxViewModel.ID, screen => new GroupBoxViewModel(screen), () => new GroupBoxShowCase());
        routes.Map(ResultViewModel.ID, screen => new ResultViewModel(screen), () => new ResultShowCase());
        routes.Map(BadgeViewModel.ID, screen => new BadgeViewModel(screen), () => new BadgeShowCase());
        routes.Map(BorderBeamViewModel.ID, screen => new BorderBeamViewModel(screen), () => new BorderBeamShowCase());
        routes.Map(SplashViewModel.ID, screen => new SplashViewModel(screen), () => new SplashShowCase());
        routes.Map(WindowViewModel.ID, screen => new WindowViewModel(screen), () => new WindowShowCase());
        routes.Map(StatisticViewModel.ID, screen => new StatisticViewModel(screen), () => new StatisticShowCase());
        routes.Map(TimelineViewModel.ID, screen => new TimelineViewModel(screen), () => new TimelineShowCase());
        routes.Map(QRCodeViewModel.ID, screen => new QRCodeViewModel(screen), () => new QRCodeShowCase());
        routes.Map(LineEditViewModel.ID, screen => new LineEditViewModel(screen), () => new LineEditShowCase());
        routes.Map(AutoCompleteViewModel.ID, screen => new AutoCompleteViewModel(screen), () => new AutoCompleteShowCase());
        routes.Map(MentionsViewModel.ID, screen => new MentionsViewModel(screen), () => new MentionsShowCase());
        routes.Map(SelectViewModel.ID, screen => new SelectViewModel(screen), () => new SelectShowCase());
        routes.Map(CascaderViewModel.ID, screen => new CascaderViewModel(screen), () => new CascaderShowCase());
        routes.Map(TreeSelectViewModel.ID, screen => new TreeSelectViewModel(screen), () => new TreeSelectShowCase());
        routes.Map(TabControlViewModel.ID, screen => new TabControlViewModel(screen), () => new TabControlShowCase());
        routes.Map(TabStripViewModel.ID, screen => new TabStripViewModel(screen), () => new TabStripShowCase());
        routes.Map(StepsViewModel.ID, screen => new StepsViewModel(screen), () => new StepsShowCase());
        routes.Map(InfoFlyoutViewModel.ID, screen => new InfoFlyoutViewModel(screen), () => new InfoFlyoutShowCase());
        routes.Map(WatermarkViewModel.ID, screen => new WatermarkViewModel(screen), () => new WatermarkShowCase());
        routes.Map(ProgressBarViewModel.ID, screen => new ProgressBarViewModel(screen), () => new ProgressBarShowCase());
        routes.Map(PaginationViewModel.ID, screen => new PaginationViewModel(screen), () => new PaginationShowCase());
        routes.Map(MenuViewModel.ID, screen => new MenuViewModel(screen), () => new MenuShowCase());
        routes.Map(TimePickerViewModel.ID, screen => new TimePickerViewModel(screen), () => new TimePickerShowCase());
        routes.Map(DatePickerViewModel.ID, screen => new DatePickerViewModel(screen), () => new DatePickerShowCase());
        routes.Map(DateViewerViewModel.ID, screen => new DateViewerViewModel(screen), () => new DateViewerShowCase());
        routes.Map(UploadViewModel.ID, screen => new UploadViewModel(screen), () => new UploadShowCase());
        routes.Map(TransferViewModel.ID, screen => new TransferViewModel(screen), () => new TransferShowCase());
        routes.Map(FormViewModel.ID, screen => new FormViewModel(screen), () => new FormShowCase());
        routes.Map(SpaceViewModel.ID, screen => new SpaceViewModel(screen), () => new SpaceShowCase());
        routes.Map(NumberUpDownViewModel.ID, screen => new NumberUpDownViewModel(screen), () => new NumberUpDownShowCase());
        routes.Map(ListViewModel.ID, screen => new ListViewModel(screen), () => new ListShowCase());
        routes.Map(MasonryViewModel.ID, screen => new MasonryViewModel(screen), () => new MasonryShowCase());
        routes.Map(TreeViewViewModel.ID, screen => new TreeViewViewModel(screen), () => new TreeViewShowCase());
        routes.Map(TourViewModel.ID, screen => new TourViewModel(screen), () => new TourShowCase());
        routes.Map(DataGridViewModel.ID, screen => new DataGridViewModel(screen), () => new DataGridShowCase());
        routes.Map(TooltipViewModel.ID, screen => new TooltipViewModel(screen), () => new TooltipShowCase());
    }
}
