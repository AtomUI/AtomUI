using AtomUI.Toolkits.GalleryBase.Infrastructure;
using System.Reactive;
using System.Reactive.Subjects;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.QRCode;

public class QRCodeViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "QRCode";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();
    private const double MinSize = 48;
    private const double MaxSize = 300;

    private string _qrCodeInput = "https://atomui.net";

    public string QRCodeInput
    {
        get => _qrCodeInput;
        set => this.RaiseAndSetIfChanged(ref _qrCodeInput, value);
    }

    private int _size = 160;

    public int Size
    {
        get => _size;
        set
        {
            if (_size == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _size, value);
            this.RaisePropertyChanged(nameof(IconSize));
            UpdateSizeCommandState();
        }
    }

    private readonly BehaviorSubject<bool> _smallerCanExecute;
    private readonly BehaviorSubject<bool> _largerCanExecute;

    public int IconSize => Size / 4;

    private IList<QRCodeEccLevel>? _eccLevels;

    public IList<QRCodeEccLevel>? EccLevels
    {
        get => _eccLevels;
        set => this.RaiseAndSetIfChanged(ref _eccLevels, value);
    }

    private QRCodeEccLevel _selectedEccLevel = QRCodeEccLevel.M;

    public QRCodeEccLevel SelectedEccLevel
    {
        get => _selectedEccLevel;
        set => this.RaiseAndSetIfChanged(ref _selectedEccLevel, value);
    }

    public ReactiveCommand<Unit, Unit> SmallerCommand { get; }
    public ReactiveCommand<Unit, Unit> LargerCommand { get; }

    public QRCodeViewModel(IScreen screen)
    {
        HostScreen = screen;
        _smallerCanExecute = new BehaviorSubject<bool>(CanDecreaseSize());
        _largerCanExecute  = new BehaviorSubject<bool>(CanIncreaseSize());
        SmallerCommand     = GalleryCommands.Create(() => { Size -= 10; }, _smallerCanExecute);
        LargerCommand      = GalleryCommands.Create(() => { Size += 10; }, _largerCanExecute);
    }

    private bool CanDecreaseSize()
    {
        return Size > MinSize;
    }

    private bool CanIncreaseSize()
    {
        return Size < MaxSize;
    }

    private void UpdateSizeCommandState()
    {
        _smallerCanExecute.OnNext(CanDecreaseSize());
        _largerCanExecute.OnNext(CanIncreaseSize());
    }
}
