using AtomUI.Toolkits.GalleryBase.Infrastructure;
using System.Collections.ObjectModel;
using System.Reactive;
using AtomUI;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Segmented;

public class SegmentedViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Segmented";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private CustomizableSizeType _roundShapeSizeType = CustomizableSizeType.Middle;

    public CustomizableSizeType RoundShapeSizeType
    {
        get => _roundShapeSizeType;
        set => this.RaiseAndSetIfChanged(ref _roundShapeSizeType, value);
    }

    public ObservableCollection<string> DynamicOptions { get; } = new()
    {
        "Daily",
        "Weekly",
        "Monthly"
    };

    private bool _isDynamicOptionsLoaded;

    public bool IsDynamicOptionsLoaded
    {
        get => _isDynamicOptionsLoaded;
        private set => this.RaiseAndSetIfChanged(ref _isDynamicOptionsLoaded, value);
    }

    public ReactiveCommand<Unit, Unit> LoadMoreOptionsCommand { get; }

    public SegmentedViewModel(IScreen screen)
    {
        HostScreen = screen;
        LoadMoreOptionsCommand = GalleryCommands.Create(LoadMoreOptions);
    }

    private void LoadMoreOptions()
    {
        if (IsDynamicOptionsLoaded)
        {
            return;
        }

        DynamicOptions.Add("Quarterly");
        DynamicOptions.Add("Yearly");
        IsDynamicOptionsLoaded = true;
    }
}
