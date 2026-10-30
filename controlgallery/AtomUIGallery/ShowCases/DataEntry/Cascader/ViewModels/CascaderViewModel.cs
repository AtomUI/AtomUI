using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DataLoad;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Cascader;

public class CascaderViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Cascader";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<ICascaderOption>? _basicCascaderViewNodes = [];

    public List<ICascaderOption>? BasicCascaderViewNodes
    {
        get => _basicCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCascaderViewNodes, value);
    }

    private TreeNodePath? _defaultSelectOptionPath;

    public TreeNodePath? DefaultSelectOptionPath
    {
        get => _defaultSelectOptionPath;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectOptionPath, value);
    }

    private ICascaderOption? _boundSelectedCascaderOption;

    public ICascaderOption? BoundSelectedCascaderOption
    {
        get => _boundSelectedCascaderOption;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundSelectedCascaderOption, value);
            this.RaisePropertyChanged(nameof(BoundSelectedCascaderOptionText));
        }
    }

    private IList<ICascaderOption>? _boundSelectedCascaderOptions;
    private INotifyCollectionChanged? _boundSelectedCascaderOptionsCollectionChangedSource;

    public IList<ICascaderOption>? BoundSelectedCascaderOptions
    {
        get => _boundSelectedCascaderOptions;
        set
        {
            if (ReferenceEquals(_boundSelectedCascaderOptions, value))
            {
                this.RaisePropertyChanged(nameof(BoundSelectedCascaderOptionsText));
                return;
            }

            if (_boundSelectedCascaderOptionsCollectionChangedSource != null)
            {
                _boundSelectedCascaderOptionsCollectionChangedSource.CollectionChanged -= HandleBoundSelectedCascaderOptionsCollectionChanged;
            }

            this.RaiseAndSetIfChanged(ref _boundSelectedCascaderOptions, value);

            _boundSelectedCascaderOptionsCollectionChangedSource = value as INotifyCollectionChanged;
            if (_boundSelectedCascaderOptionsCollectionChangedSource != null)
            {
                _boundSelectedCascaderOptionsCollectionChangedSource.CollectionChanged += HandleBoundSelectedCascaderOptionsCollectionChanged;
            }
            this.RaisePropertyChanged(nameof(BoundSelectedCascaderOptionsText));
        }
    }

    public string BoundSelectedCascaderOptionText => FormatCascaderOption(BoundSelectedCascaderOption);

    public string BoundSelectedCascaderOptionsText => FormatCascaderOptions(BoundSelectedCascaderOptions);

    private List<ICascaderOption>? _basicCheckableCascaderViewNodes = [];

    public List<ICascaderOption>? BasicCheckableCascaderViewNodes
    {
        get => _basicCheckableCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCheckableCascaderViewNodes, value);
    }

    private List<ICascaderOption>? _hoverCascaderNodes = [];

    public List<ICascaderOption>? HoverCascaderNodes
    {
        get => _hoverCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _hoverCascaderNodes, value);
    }

    private List<ICascaderOption>? _disabledCascaderNodes = [];

    public List<ICascaderOption>? DisabledCascaderNodes
    {
        get => _disabledCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _disabledCascaderNodes, value);
    }

    private List<ICascaderOption>? _selectParentCascaderNodes = [];

    public List<ICascaderOption>? SelectParentCascaderNodes
    {
        get => _selectParentCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _selectParentCascaderNodes, value);
    }

    private List<ICascaderOption>? _multipleSelectCascaderNodes = [];

    public List<ICascaderOption>? MultipleSelectCascaderNodes
    {
        get => _multipleSelectCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _multipleSelectCascaderNodes, value);
    }

    private List<ICascaderOption>? _checkStrategyShowParentCascaderNodes = [];

    public List<ICascaderOption>? CheckStrategyShowParentCascaderNodes
    {
        get => _checkStrategyShowParentCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _checkStrategyShowParentCascaderNodes, value);
    }

    private List<ICascaderOption>? _checkStrategyShowAllCascaderNodes = [];

    public List<ICascaderOption>? CheckStrategyShowAllCascaderNodes
    {
        get => _checkStrategyShowAllCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _checkStrategyShowAllCascaderNodes, value);
    }

    private List<ICascaderOption>? _prefixAndSuffixCascaderNodes = [];

    public List<ICascaderOption>? PrefixAndSuffixCascaderNodes
    {
        get => _prefixAndSuffixCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _prefixAndSuffixCascaderNodes, value);
    }

    private SelectPopupPlacement _placement;

    public SelectPopupPlacement Placement
    {
        get => _placement;
        set => this.RaiseAndSetIfChanged(ref _placement, value);
    }

    private List<ICascaderOption>? _placementCascaderNodes = [];

    public List<ICascaderOption>? PlacementCascaderNodes
    {
        get => _placementCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _placementCascaderNodes, value);
    }

    private List<ICascaderOption>? _searchCascaderNodes = [];

    public List<ICascaderOption>? SearchCascaderNodes
    {
        get => _searchCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _searchCascaderNodes, value);
    }

    private List<ICascaderOption>? _sizeCascaderNodes = [];

    public List<ICascaderOption>? SizeCascaderNodes
    {
        get => _sizeCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _sizeCascaderNodes, value);
    }

    private List<ICascaderOption>? _asyncLoadCascaderViewNodes = [];

    public List<ICascaderOption>? AsyncLoadCascaderViewNodes
    {
        get => _asyncLoadCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadCascaderViewNodes, value);
    }

    private List<ICascaderOption>? _searchCascaderViewNodes = [];

    public List<ICascaderOption>? SearchCascaderViewNodes
    {
        get => _searchCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _searchCascaderViewNodes, value);
    }

    private CascaderItemDataLoader? _asyncCascaderNodeLoader;

    public CascaderItemDataLoader? AsyncCascaderNodeLoader
    {
        get => _asyncCascaderNodeLoader;
        set => this.RaiseAndSetIfChanged(ref _asyncCascaderNodeLoader, value);
    }

    private List<ICascaderOption>? _defaultExpandCascaderViewNodes = [];

    public List<ICascaderOption>? DefaultExpandCascaderViewNodes
    {
        get => _defaultExpandCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _defaultExpandCascaderViewNodes, value);
    }

    private TreeNodePath? _defaultExpandPath;

    public TreeNodePath? DefaultExpandPath
    {
        get => _defaultExpandPath;
        set => this.RaiseAndSetIfChanged(ref _defaultExpandPath, value);
    }

    private List<ICascaderOption>? _semanticPreviewOptions;

    public List<ICascaderOption>? SemanticPreviewOptions
    {
        get => _semanticPreviewOptions;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewOptions, value);
    }

    private IList<ICascaderOption>? _semanticPreviewSelectedOptions;

    public IList<ICascaderOption>? SemanticPreviewSelectedOptions
    {
        get => _semanticPreviewSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewSelectedOptions, value);
    }

    private List<ICascaderOption>? _styleClassOptions;

    public List<ICascaderOption>? StyleClassOptions
    {
        get => _styleClassOptions;
        set => this.RaiseAndSetIfChanged(ref _styleClassOptions, value);
    }

    public CascaderViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public void ResetBoundCascaderSelection()
    {
        if (!TryGetSelectionBindingOptions(out var firstOption, out var secondOption))
        {
            BoundSelectedCascaderOption  = null;
            BoundSelectedCascaderOptions = null;
            return;
        }

        BoundSelectedCascaderOption  = secondOption;
        BoundSelectedCascaderOptions = new ObservableCollection<ICascaderOption> { firstOption, secondOption };
    }

    public void SelectFirstBoundCascaderOption()
    {
        if (!TryGetSelectionBindingOptions(out var firstOption, out _))
        {
            return;
        }

        BoundSelectedCascaderOption = firstOption;
    }

    public void SelectSecondBoundCascaderOption()
    {
        if (!TryGetSelectionBindingOptions(out _, out var secondOption))
        {
            return;
        }

        BoundSelectedCascaderOption = secondOption;
    }

    public void ClearBoundCascaderOptionSelection()
    {
        BoundSelectedCascaderOption = null;
    }

    public void SelectFirstBoundCascaderOptions()
    {
        if (!TryGetSelectionBindingOptions(out var firstOption, out _))
        {
            return;
        }

        SetBoundSelectedCascaderOptions(firstOption);
    }

    public void SelectSecondBoundCascaderOptions()
    {
        if (!TryGetSelectionBindingOptions(out _, out var secondOption))
        {
            return;
        }

        SetBoundSelectedCascaderOptions(secondOption);
    }

    public void SelectBothBoundCascaderOptions()
    {
        if (!TryGetSelectionBindingOptions(out var firstOption, out var secondOption))
        {
            return;
        }

        SetBoundSelectedCascaderOptions(firstOption, secondOption);
    }

    public void ClearBoundCascaderOptionsSelection()
    {
        SetBoundSelectedCascaderOptions();
    }

    private void HandleBoundSelectedCascaderOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(BoundSelectedCascaderOptionsText));
    }

    private void SetBoundSelectedCascaderOptions(params ICascaderOption[] options)
    {
        if (BoundSelectedCascaderOptions is ObservableCollection<ICascaderOption> selectedOptions)
        {
            selectedOptions.Clear();
            foreach (var option in options)
            {
                selectedOptions.Add(option);
            }
            return;
        }

        BoundSelectedCascaderOptions = new ObservableCollection<ICascaderOption>(options);
    }

    private bool TryGetSelectionBindingOptions(out ICascaderOption firstOption, out ICascaderOption secondOption)
    {
        firstOption  = null!;
        secondOption = null!;

        if (BasicCascaderViewNodes is not { Count: > 0 } nodes ||
            !TryGetChildOption(nodes[0], 0, out var firstLevelOption) ||
            !TryGetChildOption(firstLevelOption, 0, out firstOption) ||
            !TryGetChildOption(firstLevelOption, 1, out secondOption))
        {
            return false;
        }

        return true;
    }

    private static bool TryGetChildOption(ICascaderOption parentOption, int index, out ICascaderOption option)
    {
        var currentIndex = 0;
        foreach (var childOption in parentOption.Children)
        {
            if (currentIndex == index)
            {
                option = childOption;
                return true;
            }

            currentIndex++;
        }

        option = null!;
        return false;
    }

    private static string FormatCascaderOption(ICascaderOption? option)
    {
        return option?.Header?.ToString() ?? "-";
    }

    private static string FormatCascaderOptions(IEnumerable<ICascaderOption>? options)
    {
        if (options == null)
        {
            return "-";
        }

        var headers = new List<string>();
        foreach (var option in options)
        {
            headers.Add(FormatCascaderOption(option));
        }
        return headers.Count == 0 ? "-" : string.Join(", ", headers);
    }
}

public class CascaderItemDataLoader : ICascaderItemDataLoader
{
    public async Task<CascaderItemLoadResult> LoadAsync(ICascaderOption targetCascaderItem, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(600), token);
        var children = new List<CascaderOption>();
        children.AddRange(
        [
            new CascaderOption()
            {
                Header = CascaderShowCaseLanguage.FormatDynamicOption(targetCascaderItem, 1),
                IsLeaf = true
            },
            new CascaderOption()
            {
                Header = CascaderShowCaseLanguage.FormatDynamicOption(targetCascaderItem, 2),
                IsLeaf = true
            }
        ]);
        return new CascaderItemLoadResult()
        {
            IsSuccess = true,
            Data      = children
        };
    }
}
