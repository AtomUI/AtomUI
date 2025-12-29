using AtomUIGallery.ShowCases.ViewModels;
using AtomUIGallery.ShowCases.Views;
using ReactiveUI;
using Splat;


namespace AtomUIGallery.ShowCases;

internal static class ShowCaseRegister
{
    public static void Register()
    {
        RegisterGeneralCases();
        RegisterLayoutCases();
        RegisterDataDisplayCases();
        RegisterDataEntryCases();
        RegisterFeedbackCases();
        RegisterNavigationCases();
    }

    private static void RegisterGeneralCases()
    {
        Locator.CurrentMutable.Register(() => new AboutUsPage(), typeof(IViewFor<AboutUsViewModel>));
        Locator.CurrentMutable.Register(() => new ButtonShowCase(), typeof(IViewFor<ButtonViewModel>));
        Locator.CurrentMutable.Register(() => new CustomizeThemeShowCase(), typeof(IViewFor<CustomizeThemeViewModel>));
        Locator.CurrentMutable.Register(() => new IconShowCase(), typeof(IViewFor<IconViewModel>));
        Locator.CurrentMutable.Register(() => new OsInfoPage(), typeof(IViewFor<OsInfoViewModel>));
        Locator.CurrentMutable.Register(() => new PaletteShowCase(), typeof(IViewFor<PaletteViewModel>));
        Locator.CurrentMutable.Register(() => new SeparatorShowCase(), typeof(IViewFor<SeparatorViewModel>));
        Locator.CurrentMutable.Register(() => new SplitButtonShowCase(), typeof(IViewFor<SplitButtonViewModel>));
    }
    
    private static void RegisterLayoutCases()
    {
        Locator.CurrentMutable.Register(() => new BoxPanelShowCase(), typeof(IViewFor<BoxPanelViewModel>));
        Locator.CurrentMutable.Register(() => new FlexPanelShowCase(), typeof(IViewFor<FlexPanelViewModel>));
        Locator.CurrentMutable.Register(() => new GridShowCase(), typeof(IViewFor<GridViewModel>));
    }
    
    private static void RegisterDataDisplayCases()
    {
        Locator.CurrentMutable.Register(() => new AvatarShowCase(), typeof(IViewFor<AvatarViewModel>));
        Locator.CurrentMutable.Register(() => new BadgeShowCase(), typeof(IViewFor<BadgeViewModel>));
        Locator.CurrentMutable.Register(() => new CalendarShowCase(), typeof(IViewFor<CalendarViewModel>));
        Locator.CurrentMutable.Register(() => new CollapseShowCase(), typeof(IViewFor<CollapseViewModel>));
        Locator.CurrentMutable.Register(() => new CardShowCase(), typeof(IViewFor<CardViewModel>));
        Locator.CurrentMutable.Register(() => new CarouselShowCase(), typeof(IViewFor<CarouselViewModel>));
        Locator.CurrentMutable.Register(() => new DataGridShowCase(), typeof(IViewFor<DataGridViewModel>));
        Locator.CurrentMutable.Register(() => new DescriptionsShowCase(), typeof(IViewFor<DescriptionsViewModel>));
        Locator.CurrentMutable.Register(() => new EmptyShowCase(), typeof(IViewFor<EmptyViewModel>));
        Locator.CurrentMutable.Register(() => new ImagePreviewerShowCase(), typeof(IViewFor<ImagePreviewerViewModel>));
        Locator.CurrentMutable.Register(() => new ExpanderShowCase(), typeof(IViewFor<ExpanderViewModel>));
        Locator.CurrentMutable.Register(() => new GroupBoxShowCase(), typeof(IViewFor<GroupBoxViewModel>));
        Locator.CurrentMutable.Register(() => new InfoFlyoutShowCase(), typeof(IViewFor<InfoFlyoutViewModel>));
        Locator.CurrentMutable.Register(() => new ListShowCase(), typeof(IViewFor<ListViewModel>));
        Locator.CurrentMutable.Register(() => new QRCodeShowCase(), typeof(IViewFor<QRCodeViewModel>));
        Locator.CurrentMutable.Register(() => new SegmentedShowCase(), typeof(IViewFor<SegmentedViewModel>));
        Locator.CurrentMutable.Register(() => new StatisticShowCase(), typeof(IViewFor<StatisticViewModel>));
        Locator.CurrentMutable.Register(() => new TagShowCase(), typeof(IViewFor<TagViewModel>));
        Locator.CurrentMutable.Register(() => new TimelineShowCase(), typeof(IViewFor<TimelineViewModel>));
        Locator.CurrentMutable.Register(() => new TooltipShowCase(), typeof(IViewFor<TooltipViewModel>));
        Locator.CurrentMutable.Register(() => new TreeViewShowCase(), typeof(IViewFor<TreeViewViewModel>));
    }
    
    private static void RegisterDataEntryCases()
    {
        Locator.CurrentMutable.Register(() => new CheckBoxShowCase(), typeof(IViewFor<CheckBoxViewModel>));
        Locator.CurrentMutable.Register(() => new ColorPickerShowCase(), typeof(IViewFor<ColorPickerViewModel>));
        Locator.CurrentMutable.Register(() => new DatePickerShowCase(), typeof(IViewFor<DatePickerViewModel>));
        Locator.CurrentMutable.Register(() => new LineEditShowCase(), typeof(IViewFor<LineEditViewModel>));
        Locator.CurrentMutable.Register(() => new NumberUpDownShowCase(), typeof(IViewFor<NumberUpDownViewModel>));
        Locator.CurrentMutable.Register(() => new RadioButtonShowCase(), typeof(IViewFor<RadioButtonViewModel>));
        Locator.CurrentMutable.Register(() => new RateShowCase(), typeof(IViewFor<RateViewModel>));
        Locator.CurrentMutable.Register(() => new SelectShowCase(), typeof(IViewFor<SelectViewModel>));
        Locator.CurrentMutable.Register(() => new SliderShowCase(), typeof(IViewFor<SliderViewModel>));
        Locator.CurrentMutable.Register(() => new TimePickerShowCase(), typeof(IViewFor<TimePickerViewModel>));
        Locator.CurrentMutable.Register(() => new ToggleSwitchShowCase(), typeof(IViewFor<ToggleSwitchViewModel>));
        Locator.CurrentMutable.Register(() => new UploadShowCase(), typeof(IViewFor<UploadViewModel>));
    }
    
    private static void RegisterFeedbackCases()
    {
        Locator.CurrentMutable.Register(() => new AlertShowCase(), typeof(IViewFor<AlertViewModel>));
        Locator.CurrentMutable.Register(() => new DrawerShowCase(), typeof(IViewFor<DrawerViewModel>));
        Locator.CurrentMutable.Register(() => new SpinShowCase(), typeof(IViewFor<SpinViewModel>));
        Locator.CurrentMutable.Register(() => new MessageShowCase(), typeof(IViewFor<MessageViewModel>));
        Locator.CurrentMutable.Register(() => new ModalShowCase(), typeof(IViewFor<ModalViewModel>));
        Locator.CurrentMutable.Register(() => new NotificationShowCase(), typeof(IViewFor<NotificationViewModel>));
        Locator.CurrentMutable.Register(() => new PopupConfirmShowCase(), typeof(IViewFor<PopupConfirmViewModel>));
        Locator.CurrentMutable.Register(() => new ProgressBarShowCase(), typeof(IViewFor<ProgressBarViewModel>));
        Locator.CurrentMutable.Register(() => new ResultShowCase(), typeof(IViewFor<ResultViewModel>));
        Locator.CurrentMutable.Register(() => new SkeletonShowCase(), typeof(IViewFor<SkeletonViewModel>));
        Locator.CurrentMutable.Register(() => new WatermarkShowCase(), typeof(IViewFor<WatermarkViewModel>));
    }
    
    private static void RegisterNavigationCases()
    {
        Locator.CurrentMutable.Register(() => new BreadcrumbShowCase(), typeof(IViewFor<BreadcrumbViewModel>));
        Locator.CurrentMutable.Register(() => new ButtonSpinnerShowCase(), typeof(IViewFor<ButtonSpinnerViewModel>));
        Locator.CurrentMutable.Register(() => new ComboBoxShowCase(), typeof(IViewFor<ComboBoxViewModel>));
        Locator.CurrentMutable.Register(() => new DropdownButtonShowCase(), typeof(IViewFor<DropdownButtonViewModel>));
        Locator.CurrentMutable.Register(() => new MenuShowCase(), typeof(IViewFor<MenuViewModel>));
        Locator.CurrentMutable.Register(() => new PaginationShowCase(), typeof(IViewFor<PaginationViewModel>));
        Locator.CurrentMutable.Register(() => new StepsShowCase(), typeof(IViewFor<StepsViewModel>));
        Locator.CurrentMutable.Register(() => new TabControlShowCase(), typeof(IViewFor<TabControlViewModel>));
    }
}
