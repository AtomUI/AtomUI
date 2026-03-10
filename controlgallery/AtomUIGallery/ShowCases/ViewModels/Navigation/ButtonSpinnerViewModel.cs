using AtomUI.Controls;
using Avalonia.Controls;
using ReactiveUI;
using ButtonSpinner = AtomUI.Desktop.Controls.ButtonSpinner;
using TextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace AtomUIGallery.ShowCases.ViewModels;

public class ButtonSpinnerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "ButtonSpinner";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public ButtonSpinnerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
    
    public void HandleSpin(object? sender, SpinEventArgs e)
    {
        if (sender is ButtonSpinner buttonSpinner)
        {
            if (buttonSpinner.Content is TextBlock textBlock)
            {
                var value = Array.IndexOf(_spinnerItems, textBlock.Text);
                if (e.Direction == SpinDirection.Increase)
                {
                    value++;
                }
                else
                {
                    value--;
                }

                if (value < 0)
                {
                    value = _spinnerItems.Length - 1;
                }
                else if (value >= _spinnerItems.Length)
                {
                    value = 0;
                }

                textBlock.Text = _spinnerItems[value];
            }
        }
    }

    private readonly string[] _spinnerItems =
    {
        "床前明月光",
        "疑是地上霜",
        "举头望明月",
        "低头思故乡"
    };
}