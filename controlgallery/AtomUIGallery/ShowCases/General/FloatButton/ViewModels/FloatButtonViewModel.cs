using AtomUI.Toolkits.GalleryBase.Infrastructure;
using System.Reactive;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.FloatButton;

public class FloatButtonViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "FloatButton";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private int _commandClickCount;
    private string _lastCommandSource = "-";

    public int CommandClickCount
    {
        get => _commandClickCount;
        private set => this.RaiseAndSetIfChanged(ref _commandClickCount, value);
    }

    public string LastCommandSource
    {
        get => _lastCommandSource;
        private set => this.RaiseAndSetIfChanged(ref _lastCommandSource, value);
    }

    public ReactiveCommand<string, Unit> FloatButtonCommand { get; }
    
    private bool _isOpened;

    public bool IsOpened
    {
        get => _isOpened;
        set => this.RaiseAndSetIfChanged(ref _isOpened, value);
    }

    public FloatButtonViewModel(IScreen screen)
    {
        HostScreen         = screen;
        FloatButtonCommand = GalleryCommands.Create<string>(HandleFloatButtonCommand);
    }

    private void HandleFloatButtonCommand(string source)
    {
        LastCommandSource = source;
        CommandClickCount++;
    }
}
