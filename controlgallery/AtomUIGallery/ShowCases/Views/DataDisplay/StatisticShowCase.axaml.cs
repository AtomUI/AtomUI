using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class StatisticShowCase : ReactiveUserControl<StatisticViewModel>
{
    public StatisticShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
    }
}