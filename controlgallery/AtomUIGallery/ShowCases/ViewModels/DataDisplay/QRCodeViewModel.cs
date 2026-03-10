using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class QRCodeViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "QRCode";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();
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
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }

    private readonly ObservableAsPropertyHelper<int> _iconSize;

    public int IconSize => _iconSize.Value;

    private List<QRCodeEccLevel> _eccLevels = [];

    public List<QRCodeEccLevel> EccLevels
    {
        get => _eccLevels;
        set => this.RaiseAndSetIfChanged(ref _eccLevels, value);
    }

    public ReactiveCommand<Button, Unit> SmallerCommand { get; }
    public ReactiveCommand<Button, Unit> LargerCommand { get; }

    public QRCodeViewModel(IScreen screen)
    {
        HostScreen = screen;
        var smallerCanExecute = this.WhenAnyValue(x => x.Size, size => size > MinSize);
        var largerCanExecute  = this.WhenAnyValue(x => x.Size, size => size < MaxSize);
        SmallerCommand = ReactiveCommand.Create<Button>(_ => { Size -= 10; }, smallerCanExecute);
        LargerCommand  = ReactiveCommand.Create<Button>(_ => { Size += 10; }, largerCanExecute);
        EccLevels =
        [
            QRCodeEccLevel.L,
            QRCodeEccLevel.M,
            QRCodeEccLevel.Q,
            QRCodeEccLevel.H
        ];
        _iconSize = this.WhenAnyValue(x => x.Size, size => size / 4).ToProperty(this, x => x.IconSize);
    }
}