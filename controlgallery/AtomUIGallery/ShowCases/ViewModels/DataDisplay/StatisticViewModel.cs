using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class StatisticViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Statistic";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

        
    private DateTime _deadline;
    
    public DateTime Deadline
    {
        get => _deadline;
        set => this.RaiseAndSetIfChanged(ref _deadline, value);
    }
    
    private DateTime _tenSecondsLater;
    
    public DateTime TenSecondsLater
    {
        get => _tenSecondsLater;
        set => this.RaiseAndSetIfChanged(ref _tenSecondsLater, value);
    }
    
    private DateTime _before;
    
    public DateTime Before
    {
        get => _before;
        set => this.RaiseAndSetIfChanged(ref _before, value);
    }
    
    public StatisticViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}