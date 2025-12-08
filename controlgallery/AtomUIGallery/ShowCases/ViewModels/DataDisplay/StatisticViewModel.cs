using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class StatisticViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Statistic";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public StatisticViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}