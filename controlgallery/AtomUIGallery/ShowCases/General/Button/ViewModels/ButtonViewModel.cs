using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Configuration;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Button;

public class ButtonViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static EntityKey ID = "Button";

    // The ghost examples use a contrasting surface in both appearances.
    public static ThemeConfig GhostDemoThemeConfig { get; } =
        new ThemeConfigBuilder()
            .WithControl(ButtonTokens.Identity,
                new ControlThemeConfigBuilder()
                    .WithToken(nameof(ButtonToken.DefaultGhostColor), "#FFFFFF")
                    .WithToken(nameof(ButtonToken.DefaultGhostBorderColor), "#FFFFFF")
                    .Build())
            .Build();

    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string? UrlPathSegment => ID.ToString();

    private CustomizableSizeType _buttonSizeType;
    private ButtonIconPlacement _buttonIconPlacement;

    public CustomizableSizeType ButtonSizeType
    {
        get => _buttonSizeType;
        set => this.RaiseAndSetIfChanged(ref _buttonSizeType, value);
    }

    public ButtonIconPlacement ButtonIconPlacement
    {
        get => _buttonIconPlacement;
        set => this.RaiseAndSetIfChanged(ref _buttonIconPlacement, value);
    }

    public ButtonViewModel(IScreen screen)
    {
        Activator  = new ViewModelActivator();
        HostScreen = screen;
    }

}
