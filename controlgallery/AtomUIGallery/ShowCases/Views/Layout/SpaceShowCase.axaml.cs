using AtomUI;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Interactivity;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class SpaceShowCase : ReactiveUserControl<SpaceViewModel>
{
    public SpaceShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is SpaceViewModel vm)
            {
                vm.SizeType = CustomizableSizeType.Small;
            }
        });
        InitializeComponent();
    }

    // private void HandleSizeTypeChanged(object sender, RoutedEventArgs e)
    // {
    //     if (sender is RadioButton radioButton)
    //     {
    //         if (radioButton.IsChecked == true)
    //         {
    //             var sizeType = (CustomizableSizeType)radioButton.Tag!;
    //             if (DataContext is SpaceViewModel vm)
    //             {
    //                 vm.SizeType = sizeType;
    //             }
    //
    //             if (CustomSizeSlider != null)
    //             {
    //                 if (sizeType == CustomizableSizeType.Custom)
    //                 {
    //                     CustomSizeSlider.IsVisible = true;
    //                 }
    //                 else
    //                 {
    //                     CustomSizeSlider.IsVisible = false;
    //                 }
    //             }
    //         }
    //     }
    // }
}
