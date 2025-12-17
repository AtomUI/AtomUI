using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using AtomUI.Desktop.Controls;
using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
using RadioButton = Avalonia.Controls.RadioButton;

namespace AtomUIGallery.ShowCases.Views;

public partial class TimelineShowCase : ReactiveUserControl<TimelineViewModel>
{
    public TimelineShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
        
        ModeLeft.IsCheckedChanged += ModeChecked;
        
        ModeRight.IsCheckedChanged += ModeChecked;
        
        ModeAlternate.IsCheckedChanged += ModeChecked;
        
        ReverseButton.Click += ReverseButtonClick;
    }
    
    private void ReverseButtonClick(object? sender, RoutedEventArgs e)
    {
        ReverseTimeline.IsReverse = !ReverseTimeline.IsReverse;
    }
    
    private void ModeChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton)
        {
            if (radioButton == ModeLeft && ModeLeft.IsChecked.HasValue && ModeLeft.IsChecked.Value)
            {
                LabelTimeline.Mode = TimeLineMode.Left;
            }
            else if (radioButton == ModeRight && ModeRight.IsChecked.HasValue && ModeRight.IsChecked.Value)
            {
                LabelTimeline.Mode = TimeLineMode.Right;
            }
            else if (radioButton == ModeAlternate && ModeAlternate.IsChecked.HasValue && ModeAlternate.IsChecked.Value)
            {
                LabelTimeline.Mode = TimeLineMode.Alternate;
            }
        }
    }

}