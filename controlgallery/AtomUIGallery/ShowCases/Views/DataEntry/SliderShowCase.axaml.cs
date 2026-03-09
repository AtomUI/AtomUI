using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Media;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class SliderShowCase : ReactiveUserControl<SliderViewModel>
{
    public SliderShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is SliderViewModel vm)
            {
                var marks = new List<SliderMark>();
                marks.Add(new SliderMark("0°C", 0));
                marks.Add(new SliderMark("26°C", 26));
                marks.Add(new SliderMark("37°C", 37));
                marks.Add(new SliderMark("100°C", 100)
                {
                    LabelFontWeight = FontWeight.Bold,
                    LabelBrush      = new SolidColorBrush(Colors.Red)
                });
                vm.SliderMarks =  marks;
            }
        });
        InitializeComponent();
    }
}