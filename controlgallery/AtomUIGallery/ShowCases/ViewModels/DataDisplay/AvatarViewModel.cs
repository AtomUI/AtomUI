using System.Reactive.Disposables;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class AvatarViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static EntityKey ID = "Avatar";
    public ViewModelActivator Activator { get; }
    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private string? _avatarText;

    public string? AvatarText
    {
        get => _avatarText;
        set => this.RaiseAndSetIfChanged(ref _avatarText, value);
    }

    private double? _avatarGap;

    public double? AvatarGap
    {
        get => _avatarGap;
        set => this.RaiseAndSetIfChanged(ref _avatarGap, value);
    }

    private string? _avatarBackground;

    public string? AvatarBackground
    {
        get => _avatarBackground;
        set => this.RaiseAndSetIfChanged(ref _avatarBackground, value);
    }

    private int _textCurrentIndex = 0;
    private int _gapCurrentIndex = 0;

    private List<string> _userList;
    private List<string> _colorList;
    private List<double> _gapList;

    public AvatarViewModel(IScreen screen)
    {
        Activator  = new ViewModelActivator();
        HostScreen = screen;
        _userList  = ["U", "Lucy", "Tom", "Edward"];
        _colorList = ["#f56a00", "#7265e6", "#ffbf00", "#00a2ae"];
        _gapList   = [4, 3, 2, 1];
        this.WhenActivated((CompositeDisposable disposables) =>
        {
            SetupAvatarText();
            SetupAvatarGap();
        });
    }

    private void SetupAvatarText()
    {
        var index = (_textCurrentIndex++) % 4;
        AvatarText       = _userList[index];
        AvatarBackground = _colorList[index];
    }

    private void SetupAvatarGap()
    {
        var index = (_gapCurrentIndex++) % 4;
        AvatarGap = _gapList[index];
    }

    public void HandleChangeUserClicked(object? sender, EventArgs e)
    {
        SetupAvatarText();
    }

    public void HandleChangeGapClicked(object? sender, EventArgs e)
    {
        SetupAvatarGap();
    }
}