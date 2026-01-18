using AtomUI.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Threading;
using ReactiveUI;

namespace AtomUIGallery.Workspace.ViewModels;

public class CaseNavigationViewModel : ReactiveObject
{
    private Dictionary<TreeNodeKey, Func<IRoutableViewModel>> _showCaseViewModelFactories;
    private TreeNodeKey? _currentShowCase;
    private DispatcherTimer _dispatcherTimer;

    public IScreen HostScreen { get; }

    public CaseNavigationViewModel(IScreen hostScreen)
    {
        _showCaseViewModelFactories = new Dictionary<TreeNodeKey, Func<IRoutableViewModel>>();
        HostScreen                  = hostScreen;
        RegisterShowCaseViewModels();
        _dispatcherTimer      =  new DispatcherTimer();
        _dispatcherTimer.Tick += RandomNavigateToTimerHandler;
    }

    private void RegisterShowCaseViewModels()
    {
        RegisterGeneralViewModels();
        RegisterLayoutViewModels();
        RegisterDataDisplayViewModels();
        RegisterDataEntryViewModels();
        RegisterFeedbackViewModels();
        RegisterNavigationViewModels();
    }

    private void RegisterGeneralViewModels()
    {
        _showCaseViewModelFactories.Add(AboutUsViewModel.ID, () => new AboutUsViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ButtonViewModel.ID, () => new ButtonViewModel(HostScreen));
        _showCaseViewModelFactories.Add(IconViewModel.ID, () => new IconViewModel(HostScreen));
        _showCaseViewModelFactories.Add(OsInfoViewModel.ID, () => new OsInfoViewModel(HostScreen));
        _showCaseViewModelFactories.Add(PaletteViewModel.ID, () => new PaletteViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SeparatorViewModel.ID, () => new SeparatorViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SplitButtonViewModel.ID, () => new SplitButtonViewModel(HostScreen));
        _showCaseViewModelFactories.Add(CustomizeThemeViewModel.ID, () => new CustomizeThemeViewModel(HostScreen));
    }
    
    private void RegisterLayoutViewModels()
    {
        _showCaseViewModelFactories.Add(BoxPanelViewModel.ID, () => new BoxPanelViewModel(HostScreen));
        _showCaseViewModelFactories.Add(FlexPanelViewModel.ID, () => new FlexPanelViewModel(HostScreen));
        _showCaseViewModelFactories.Add(GridViewModel.ID, () => new GridViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SplitterViewModel.ID, () => new SplitterViewModel(HostScreen));
    }

    private void RegisterDataDisplayViewModels()
    {
        _showCaseViewModelFactories.Add(AvatarViewModel.ID, () => new AvatarViewModel(HostScreen));
        _showCaseViewModelFactories.Add(BadgeViewModel.ID, () => new BadgeViewModel(HostScreen));
        _showCaseViewModelFactories.Add(CalendarViewModel.ID, () => new CalendarViewModel(HostScreen));
        _showCaseViewModelFactories.Add(CardViewModel.ID, () => new CardViewModel(HostScreen));
        _showCaseViewModelFactories.Add(CarouselViewModel.ID, () => new CarouselViewModel(HostScreen));
        _showCaseViewModelFactories.Add(CollapseViewModel.ID, () => new CollapseViewModel(HostScreen));
        _showCaseViewModelFactories.Add(DescriptionsViewModel.ID, () => new DescriptionsViewModel(HostScreen));
        _showCaseViewModelFactories.Add(DataGridViewModel.ID, () => new DataGridViewModel(HostScreen));
        _showCaseViewModelFactories.Add(EmptyViewModel.ID, () => new EmptyViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ImagePreviewerViewModel.ID, () => new ImagePreviewerViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ExpanderViewModel.ID, () => new ExpanderViewModel(HostScreen));
        _showCaseViewModelFactories.Add(GroupBoxViewModel.ID, () => new GroupBoxViewModel(HostScreen));
        _showCaseViewModelFactories.Add(InfoFlyoutViewModel.ID, () => new InfoFlyoutViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ListViewModel.ID, () => new ListViewModel(HostScreen));
        _showCaseViewModelFactories.Add(QRCodeViewModel.ID, () => new QRCodeViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SegmentedViewModel.ID, () => new SegmentedViewModel(HostScreen));
        _showCaseViewModelFactories.Add(StatisticViewModel.ID, () => new StatisticViewModel(HostScreen));
        _showCaseViewModelFactories.Add(TagViewModel.ID, () => new TagViewModel(HostScreen));
        _showCaseViewModelFactories.Add(TimelineViewModel.ID, () => new TimelineViewModel(HostScreen));
        _showCaseViewModelFactories.Add(TooltipViewModel.ID, () => new TooltipViewModel(HostScreen));
        _showCaseViewModelFactories.Add(TreeViewViewModel.ID, () => new TreeViewViewModel(HostScreen));
    }

    private void RegisterDataEntryViewModels()
    {
        _showCaseViewModelFactories.Add(CascaderViewModel.ID, () => new CascaderViewModel(HostScreen));
        _showCaseViewModelFactories.Add(CheckBoxViewModel.ID, () => new CheckBoxViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ColorPickerViewModel.ID, () => new ColorPickerViewModel(HostScreen));
        _showCaseViewModelFactories.Add(DatePickerViewModel.ID, () => new DatePickerViewModel(HostScreen));
        _showCaseViewModelFactories.Add(LineEditViewModel.ID, () => new LineEditViewModel(HostScreen));
        _showCaseViewModelFactories.Add(MentionsViewModel.ID, () => new MentionsViewModel(HostScreen));
        _showCaseViewModelFactories.Add(NumberUpDownViewModel.ID, () => new NumberUpDownViewModel(HostScreen));
        _showCaseViewModelFactories.Add(RadioButtonViewModel.ID, () => new RadioButtonViewModel(HostScreen));
        _showCaseViewModelFactories.Add(RateViewModel.ID, () => new RateViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SelectViewModel.ID, () => new SelectViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SliderViewModel.ID, () => new SliderViewModel(HostScreen));
        _showCaseViewModelFactories.Add(TimePickerViewModel.ID, () => new TimePickerViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ToggleSwitchViewModel.ID, () => new ToggleSwitchViewModel(HostScreen));
        _showCaseViewModelFactories.Add(TreeSelectViewModel.ID, () => new TreeSelectViewModel(HostScreen));
        _showCaseViewModelFactories.Add(UploadViewModel.ID, () => new UploadViewModel(HostScreen));
    }

    private void RegisterFeedbackViewModels()
    {
        _showCaseViewModelFactories.Add(AlertViewModel.ID, () => new AlertViewModel(HostScreen));
        _showCaseViewModelFactories.Add(DrawerViewModel.ID, () => new DrawerViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SpinViewModel.ID, () => new SpinViewModel(HostScreen));
        _showCaseViewModelFactories.Add(MessageViewModel.ID, () => new MessageViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ModalViewModel.ID, () => new ModalViewModel(HostScreen));
        _showCaseViewModelFactories.Add(NotificationViewModel.ID, () => new NotificationViewModel(HostScreen));
        _showCaseViewModelFactories.Add(PopupConfirmViewModel.ID, () => new PopupConfirmViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ResultViewModel.ID, () => new ResultViewModel(HostScreen));
        _showCaseViewModelFactories.Add(SkeletonViewModel.ID, () => new SkeletonViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ProgressBarViewModel.ID, () => new ProgressBarViewModel(HostScreen));
        _showCaseViewModelFactories.Add(WatermarkViewModel.ID, () => new WatermarkViewModel(HostScreen));
    }

    private void RegisterNavigationViewModels()
    {
        _showCaseViewModelFactories.Add(ButtonSpinnerViewModel.ID, () => new ButtonSpinnerViewModel(HostScreen));
        _showCaseViewModelFactories.Add(BreadcrumbViewModel.ID, () => new BreadcrumbViewModel(HostScreen));
        _showCaseViewModelFactories.Add(ComboBoxViewModel.ID, () => new ComboBoxViewModel(HostScreen));
        _showCaseViewModelFactories.Add(DropdownButtonViewModel.ID, () => new DropdownButtonViewModel(HostScreen));
        _showCaseViewModelFactories.Add(MenuViewModel.ID, () => new MenuViewModel(HostScreen));
        _showCaseViewModelFactories.Add(PaginationViewModel.ID, () => new PaginationViewModel(HostScreen));
        _showCaseViewModelFactories.Add(StepsViewModel.ID, () => new StepsViewModel(HostScreen));
        _showCaseViewModelFactories.Add(TabControlViewModel.ID, () => new TabControlViewModel(HostScreen));
    }

    public void NavigateTo(TreeNodeKey showCaseId)
    {
        if (_currentShowCase is not null && _currentShowCase == showCaseId)
        {
            return;
        }

        _currentShowCase = showCaseId;

        if (!_showCaseViewModelFactories.TryGetValue(showCaseId,  out var viewModelFactory))
        {
            throw new NotSupportedException($"unknown showcase id {showCaseId}");
        }

        var viewModel = viewModelFactory();
        HostScreen.Router.Navigate.Execute(viewModel);
    }
    
    private static int _currentShowCaseIdx = 0;

    private void RandomNavigateToTimerHandler(object? sender, EventArgs e)
    {
        var    caseIds      = _showCaseViewModelFactories.Keys.ToList();
        // Random random       = new Random();
        // var    nextKeyIndex = random.Next(caseIds.Count);
        var id = caseIds[_currentShowCaseIdx++ % caseIds.Count];
        NavigateTo(id);
    }

    public void TestNavigatePages(TimeSpan interval)
    {
        _dispatcherTimer.Stop();
        _dispatcherTimer.Interval = interval;
        _dispatcherTimer.Start();
    }

    public void StopTestNavigatePages()
    {
        _dispatcherTimer.Stop();
    }
}
