using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Select";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<SelectOption>? _randomOptions;

    public List<SelectOption>? RandomOptions
    {
        get => _randomOptions;
        set => this.RaiseAndSetIfChanged(ref _randomOptions, value);
    }
    
    private List<ISelectOption> _basicSelectedOptions = [];

    public List<ISelectOption> BasicSelectedOptions
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
    
    private List<SelectOption>? _maxTagCountOptions;

    public List<SelectOption>? MaxTagCountOptions
    {
        get => _maxTagCountOptions;
        set => this.RaiseAndSetIfChanged(ref _maxTagCountOptions, value);
    }
    
    private SizeType _selectSizeType;

    public SizeType SelectSizeType
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
        
    public SelectViewModel(IScreen screen)
    {
        HostScreen  = screen;
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
            Header = "Jack",
            Value = "jack"
        });
        options.Add(new SelectOption()
        {
            Header = "Lucy",
            Value  = "lucy"
        });
        options.Add(new SelectOption()
        {
            Header = "Yiminghe",
            Value  = "yiminghe"
        });
        options.Add(new SelectOption()
        {
            Header = "Disabled",
            Value  = "disabled",
            IsEnabled = false
        });
        return new SelectOptionsLoadResult()
        {
            Data = options,
            StatusCode = RpcStatusCode.Success
        };
    }
}