using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class AutoCompleteViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static TreeNodeKey ID = "AutoComplete";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string UrlPathSegment { get; } = ID.ToString();
    
    private ICompleteOptionsAsyncLoader? _basicOptionsAsyncLoader;
    
    public ICompleteOptionsAsyncLoader? BasicOptionsAsyncLoader
    {
        get => _basicOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _basicOptionsAsyncLoader, value);
    }
    
    private ICompleteOptionsAsyncLoader? _customLabelOptionsAsyncLoader;
    
    public ICompleteOptionsAsyncLoader? CustomLabelOptionsAsyncLoader
    {
        get => _customLabelOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _customLabelOptionsAsyncLoader, value);
    }
    
    private ICompleteOptionsAsyncLoader? _searchEditOptionsAsyncLoader;
    
    public ICompleteOptionsAsyncLoader? SearchEditOptionsAsyncLoader
    {
        get => _searchEditOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _searchEditOptionsAsyncLoader, value);
    }
    
    private List<IAutoCompleteOption> _filterCaseOptions = [];
    
    public List<IAutoCompleteOption> FilterCaseOptions
    {
        get => _filterCaseOptions;
        set => this.RaiseAndSetIfChanged(ref _filterCaseOptions, value);
    }
    
    public AutoCompleteViewModel(IScreen screen)
    {
        HostScreen = screen;
        Activator  = new ViewModelActivator();
    }
}

public class BasicOptionsAsyncLoader : ICompleteOptionsAsyncLoader
{
    public async Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        List<IAutoCompleteOption> data = [];
        if (!string.IsNullOrWhiteSpace(context))
        {
            {
                var value = context;
                data.Add(new AutoCompleteOption()
                {
                    Header = value.Replace("\r\n", " ")
                                  .Replace("\n", " "),
                    Value  = value,
                });
            }
            {
                var value = string.Concat(Enumerable.Repeat(context, 2));
                data.Add(new AutoCompleteOption()
                {
                    Header = value.Replace("\r\n", " ")
                                  .Replace("\n", " "),
                    Value =  value,
                });
            }
            {
                var value = string.Concat(Enumerable.Repeat(context, 3));
                data.Add(new AutoCompleteOption()
                {
                    Header = value.Replace("\r\n", " ")
                                  .Replace("\n", " "),
                    Value  = value,
                });
            }
        }
        return new CompleteOptionsLoadResult()
        {
            StatusCode = RpcStatusCode.Success,
            Data = data
        };
    }
}

public class CustomLabelOptionsAsyncLoader : ICompleteOptionsAsyncLoader
{
    protected List<string> Suffixes = new ();

    public CustomLabelOptionsAsyncLoader()
    {
        Suffixes.Add("gmail.com");
        Suffixes.Add("163.com");
        Suffixes.Add("qq.com");
    }
    
    public async Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        List<IAutoCompleteOption> data = [];

        if (context != null && !string.IsNullOrWhiteSpace(context) && !context.Contains('@'))
        {
            foreach (var suffix in Suffixes)
            {
                var value = $"{context}@{suffix}";
                data.Add(new AutoCompleteOption()
                {
                    Header = value,
                    Value  = value,
                });
            }
        }
        
        return new CompleteOptionsLoadResult()
        {
            Data = data
        };
    }
}

public class SearchEditOptionsAsyncLoader : ICompleteOptionsAsyncLoader
{
    public async Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        List<IAutoCompleteOption> data   = [];
        var                       random = new Random();

        if (!string.IsNullOrWhiteSpace(context))
        {
            var count = random.Next(5);
        
            for (var i = 0; i < count; i++)
            {
                var newValue = $"{context}{i}";
                var value    = $"Found {context} on {newValue}";
                data.Add(new CustomAutoCompleteOption()
                {
                    Header      = value,
                    Value       = newValue,
                    ResultCount = random.Next(100, 200)
                });
            }
        }
        
        return new CompleteOptionsLoadResult()
        {
            Data = data
        };
    }
}

public class CustomAutoCompleteOption : AutoCompleteOption
{
    public int ResultCount { get; init; }
}