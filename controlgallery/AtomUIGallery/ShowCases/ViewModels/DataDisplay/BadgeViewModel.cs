using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class BadgeViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public ViewModelActivator Activator { get; }

    public static EntityKey ID = "Badge";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private double _dynamicBadgeCount = 5;

    public double DynamicBadgeCount
    {
        get => _dynamicBadgeCount;
        set => this.RaiseAndSetIfChanged(ref _dynamicBadgeCount, value);
    }

    private bool _dynamicDotBadgeVisible = true;

    public bool DynamicDotBadgeVisible
    {
        get => _dynamicDotBadgeVisible;
        set => this.RaiseAndSetIfChanged(ref _dynamicDotBadgeVisible, value);
    }

    private bool _standaloneSwitchChecked;

    public bool StandaloneSwitchChecked
    {
        get => _standaloneSwitchChecked;
        set => this.RaiseAndSetIfChanged(ref _standaloneSwitchChecked, value);
    }

    private double _standaloneBadgeCount1;

    public double StandaloneBadgeCount1
    {
        get => _standaloneBadgeCount1;
        set => this.RaiseAndSetIfChanged(ref _standaloneBadgeCount1, value);
    }

    private double _standaloneBadgeCount2;

    public double StandaloneBadgeCount2
    {
        get => _standaloneBadgeCount2;
        set => this.RaiseAndSetIfChanged(ref _standaloneBadgeCount2, value);
    }

    private double _standaloneBadgeCount3;

    public double StandaloneBadgeCount3
    {
        get => _standaloneBadgeCount3;
        set => this.RaiseAndSetIfChanged(ref _standaloneBadgeCount3, value);
    }

    public BadgeViewModel(IScreen screen)
    {
        Activator = new ViewModelActivator();
        this.WhenActivated((CompositeDisposable disposables) =>
        {
            this.WhenAnyValue(vm => vm.StandaloneSwitchChecked)
                .Subscribe(HandleStandaloneSwitchChecked)
                .DisposeWith(disposables);
        });
        HostScreen = screen;
    }

    private void HandleStandaloneSwitchChecked(bool value)
    {
        if (value)
        {
            StandaloneBadgeCount1 = 11;
            StandaloneBadgeCount2 = 25;
            StandaloneBadgeCount3 = 109;
        }
        else
        {
            StandaloneBadgeCount1 = 0;
            StandaloneBadgeCount2 = 0;
            StandaloneBadgeCount3 = 0;
        }
    }

    public void AddDynamicBadgeCount()
    {
        DynamicBadgeCount += 1;
    }

    public void SubDynamicBadgeCount()
    {
        var value = DynamicBadgeCount;
        value             -= 1;
        value             =  Math.Max(value, 0);
        DynamicBadgeCount =  value;
    }

    public void RandomDynamicBadgeCount()
    {
        var random = new Random();
        DynamicBadgeCount = random.Next(0, 110);
    }
}