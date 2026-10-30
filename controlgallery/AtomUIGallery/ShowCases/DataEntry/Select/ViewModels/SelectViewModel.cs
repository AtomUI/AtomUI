using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Select;

public class SelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Select";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<SelectOption>? _randomOptions;

    public List<SelectOption>? RandomOptions
    {
        get => _randomOptions;
        set => this.RaiseAndSetIfChanged(ref _randomOptions, value);
    }

    private List<ISelectOption>? _basicSelectedOptions = [];

    public List<ISelectOption>? BasicSelectedOptions
    {
        get => _basicSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _basicSelectedOptions, value);
    }

    private List<ISelectOption>? _defaultSelectedOptions;

    public List<ISelectOption>? DefaultSelectedOptions
    {
        get => _defaultSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectedOptions, value);
    }

    private ISelectOption? _defaultSelectedOption;

    public ISelectOption? DefaultSelectedOption
    {
        get => _defaultSelectedOption;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectedOption, value);
    }

    private ISelectOption? _boundSelectedOption;

    public ISelectOption? BoundSelectedOption
    {
        get => _boundSelectedOption;
        set
        {
            this.RaiseAndSetIfChanged(ref _boundSelectedOption, value);
            this.RaisePropertyChanged(nameof(BoundSelectedOptionText));
        }
    }

    private IList<ISelectOption>? _boundSelectedOptions;
    private INotifyCollectionChanged? _boundSelectedOptionsCollectionChangedSource;

    public IList<ISelectOption>? BoundSelectedOptions
    {
        get => _boundSelectedOptions;
        set
        {
            if (ReferenceEquals(_boundSelectedOptions, value))
            {
                this.RaisePropertyChanged(nameof(BoundSelectedOptionsText));
                return;
            }

            if (_boundSelectedOptionsCollectionChangedSource != null)
            {
                _boundSelectedOptionsCollectionChangedSource.CollectionChanged -= HandleBoundSelectedOptionsCollectionChanged;
            }

            this.RaiseAndSetIfChanged(ref _boundSelectedOptions, value);

            _boundSelectedOptionsCollectionChangedSource = value as INotifyCollectionChanged;
            if (_boundSelectedOptionsCollectionChangedSource != null)
            {
                _boundSelectedOptionsCollectionChangedSource.CollectionChanged += HandleBoundSelectedOptionsCollectionChanged;
            }
            this.RaisePropertyChanged(nameof(BoundSelectedOptionsText));
        }
    }

    public string BoundSelectedOptionText => BoundSelectedOption?.Header?.ToString() ?? "-";

    public string BoundSelectedOptionsText => BoundSelectedOptions is { Count: > 0 }
        ? string.Join(", ", BoundSelectedOptions.Select(option => option.Header?.ToString()))
        : "-";

    private List<ISelectOption>? _singleLucyOptions;

    public List<ISelectOption>? SingleLucyOptions
    {
        get => _singleLucyOptions;
        set => this.RaiseAndSetIfChanged(ref _singleLucyOptions, value);
    }

    private List<ISelectOption>? _searchOptions;

    public List<ISelectOption>? SearchOptions
    {
        get => _searchOptions;
        set => this.RaiseAndSetIfChanged(ref _searchOptions, value);
    }

    private List<CustomOption>? _customCountryOptions;

    public List<CustomOption>? CustomCountryOptions
    {
        get => _customCountryOptions;
        set => this.RaiseAndSetIfChanged(ref _customCountryOptions, value);
    }

    private List<ISelectOption>? _groupedPersonOptions;

    public List<ISelectOption>? GroupedPersonOptions
    {
        get => _groupedPersonOptions;
        set => this.RaiseAndSetIfChanged(ref _groupedPersonOptions, value);
    }

    private List<ISelectOption>? _variantOptions;

    public List<ISelectOption>? VariantOptions
    {
        get => _variantOptions;
        set => this.RaiseAndSetIfChanged(ref _variantOptions, value);
    }

    private List<ISelectOption>? _hideSelectedOptions;

    public List<ISelectOption>? HideSelectedOptions
    {
        get => _hideSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _hideSelectedOptions, value);
    }

    private List<ISelectOption>? _maxCountLimitedOptions;

    public List<ISelectOption>? MaxCountLimitedOptions
    {
        get => _maxCountLimitedOptions;
        set => this.RaiseAndSetIfChanged(ref _maxCountLimitedOptions, value);
    }

    private List<ISelectOption>? _prefixSuffixOptions;

    public List<ISelectOption>? PrefixSuffixOptions
    {
        get => _prefixSuffixOptions;
        set => this.RaiseAndSetIfChanged(ref _prefixSuffixOptions, value);
    }

    private List<SelectOption>? _maxTagCountOptions;

    public List<SelectOption>? MaxTagCountOptions
    {
        get => _maxTagCountOptions;
        set => this.RaiseAndSetIfChanged(ref _maxTagCountOptions, value);
    }

    private CustomizableSizeType _selectSizeType = CustomizableSizeType.Middle;

    public CustomizableSizeType SelectSizeType
    {
        get => _selectSizeType;
        set => this.RaiseAndSetIfChanged(ref _selectSizeType, value);
    }

    private ISelectOptionsAsyncLoader? _selectOptionsAsyncLoader;

    public ISelectOptionsAsyncLoader? SelectOptionsAsyncLoader
    {
        get => _selectOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _selectOptionsAsyncLoader, value);
    }

    private List<ISelectOption>? _semanticPreviewOptions;

    public List<ISelectOption>? SemanticPreviewOptions
    {
        get => _semanticPreviewOptions;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewOptions, value);
    }

    private IList<ISelectOption>? _semanticPreviewSelectedOptions;

    public IList<ISelectOption>? SemanticPreviewSelectedOptions
    {
        get => _semanticPreviewSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewSelectedOptions, value);
    }

    private List<ISelectOption>? _styleClassOptions;

    public List<ISelectOption>? StyleClassOptions
    {
        get => _styleClassOptions;
        set => this.RaiseAndSetIfChanged(ref _styleClassOptions, value);
    }

    public SelectViewModel(IScreen screen)
    {
        HostScreen = screen;
        SetBoundSelectedOptionCommand  = ReactiveCommand.Create(SetBoundSelectedOption);
        ClearBoundSelectedOptionCommand = ReactiveCommand.Create(ClearBoundSelectedOption);
        SetBoundSelectedOptionsCommand = ReactiveCommand.Create(SetBoundSelectedOptions);
        ClearBoundSelectedOptionsCommand = ReactiveCommand.Create(ClearBoundSelectedOptions);
    }

    public ReactiveCommand<Unit, Unit> SetBoundSelectedOptionCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundSelectedOptionCommand { get; }

    public ReactiveCommand<Unit, Unit> SetBoundSelectedOptionsCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundSelectedOptionsCommand { get; }

    private void SetBoundSelectedOption()
    {
        if (BasicSelectedOptions is { Count: > 1 })
        {
            BoundSelectedOption = BasicSelectedOptions[1];
        }
    }

    private void ClearBoundSelectedOption()
    {
        BoundSelectedOption = null;
    }

    private void SetBoundSelectedOptions()
    {
        if (BasicSelectedOptions is not { Count: > 2 })
        {
            return;
        }

        if (BoundSelectedOptions is ObservableCollection<ISelectOption> collection)
        {
            collection.Clear();
            collection.Add(BasicSelectedOptions[0]);
            collection.Add(BasicSelectedOptions[2]);
        }
        else
        {
            BoundSelectedOptions = new ObservableCollection<ISelectOption>
            {
                BasicSelectedOptions[0],
                BasicSelectedOptions[2]
            };
        }
    }

    private void ClearBoundSelectedOptions()
    {
        if (BoundSelectedOptions is ObservableCollection<ISelectOption> collection)
        {
            collection.Clear();
        }
        else
        {
            BoundSelectedOptions = new ObservableCollection<ISelectOption>();
        }
    }

    private void HandleBoundSelectedOptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(BoundSelectedOptionsText));
    }

}

public class SelectOptionsAsyncLoader : ISelectOptionsAsyncLoader
{
    public async Task<SelectOptionsLoadResult> LoadAsync(object? context, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(600), token);
        var options = new List<ISelectOption>();
        options.Add(new SelectOption()
        {
            Header  = SelectShowCaseLanguage.Get(SelectShowCaseLangResourceKind.P2HeaderJack, "Jack"),
            Content = "jack"
        });
        options.Add(new SelectOption()
        {
            Header  = SelectShowCaseLanguage.Get(SelectShowCaseLangResourceKind.P2HeaderLucy, "Lucy"),
            Content = "lucy"
        });
        options.Add(new SelectOption()
        {
            Header  = SelectShowCaseLanguage.Get(SelectShowCaseLangResourceKind.P2HeaderYiminghe, "Yiminghe"),
            Content = "yiminghe"
        });
        options.Add(new SelectOption()
        {
            Header    = SelectShowCaseLanguage.Get(SelectShowCaseLangResourceKind.P2HeaderDisabled, "Disabled"),
            Content   = "disabled",
            IsEnabled = false
        });
        return new SelectOptionsLoadResult()
        {
            Data       = options,
            StatusCode = RpcStatusCode.Success
        };
    }
}
