using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class StatisticShowCase : ReactiveUserControl<StatisticViewModel>
{
    public StatisticShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is StatisticViewModel viewModel)
            {
                viewModel.Deadline = DateTime.Now.Add(TimeSpan.FromSeconds(60 * 60 * 24 * 2 + 30));
                viewModel.Before   = DateTime.Now.Subtract(TimeSpan.FromSeconds(60 * 60 * 24 * 2 + 30));
                viewModel.TenSecondsLater   = DateTime.Now.AddSeconds(10);
            }
        });
        InitializeComponent();
    }
}