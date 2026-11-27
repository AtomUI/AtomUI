using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.AutoComplete;

public class AutoCompleteViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "AutoComplete";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private ICompleteOptionsAsyncLoader? _basicOptionsAsyncLoader;

    public ICompleteOptionsAsyncLoader? BasicOptionsAsyncLoader
    {
        get => _basicOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _basicOptionsAsyncLoader, value);
    }

    private ICompleteOptionsAsyncLoader? _customOptionsAsyncLoader;

    public ICompleteOptionsAsyncLoader? CustomOptionsAsyncLoader
    {
        get => _customOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _customOptionsAsyncLoader, value);
    }

    private ICompleteOptionsAsyncLoader? _customInputOptionsAsyncLoader;

    public ICompleteOptionsAsyncLoader? CustomInputOptionsAsyncLoader
    {
        get => _customInputOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _customInputOptionsAsyncLoader, value);
    }

    private ICompleteOptionsAsyncLoader? _uncertainCategoryOptionsAsyncLoader;

    public ICompleteOptionsAsyncLoader? UncertainCategoryOptionsAsyncLoader
    {
        get => _uncertainCategoryOptionsAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _uncertainCategoryOptionsAsyncLoader, value);
    }

    private List<IAutoCompleteOption>? _certainCategoryOptions;

    public List<IAutoCompleteOption>? CertainCategoryOptions
    {
        get => _certainCategoryOptions;
        set => this.RaiseAndSetIfChanged(ref _certainCategoryOptions, value);
    }

    private List<IAutoCompleteOption>? _filterCaseOptions;

    public List<IAutoCompleteOption>? FilterCaseOptions
    {
        get => _filterCaseOptions;
        set => this.RaiseAndSetIfChanged(ref _filterCaseOptions, value);
    }

    private List<IAutoCompleteOption>? _semanticPreviewOptions;

    public List<IAutoCompleteOption>? SemanticPreviewOptions
    {
        get => _semanticPreviewOptions;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewOptions, value);
    }

    private List<IAutoCompleteOption>? _styleClassOptions;

    public List<IAutoCompleteOption>? StyleClassOptions
    {
        get => _styleClassOptions;
        set => this.RaiseAndSetIfChanged(ref _styleClassOptions, value);
    }

    public AutoCompleteViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}

/// <summary>
/// Shared mockVal logic of the antd basic, status, variant, allowClear and custom demos:
/// the input text yields the three options [text, text repeated twice, text repeated three times].
/// </summary>
public class MockValOptionsAsyncLoader : ICompleteOptionsAsyncLoader
{
    public Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token)
    {
        List<IAutoCompleteOption> data = [];
        if (!string.IsNullOrWhiteSpace(context))
        {
            for (var repeat = 1; repeat <= 3; repeat++)
            {
                var value = string.Concat(Enumerable.Repeat(context, repeat));
                data.Add(new AutoCompleteOption()
                {
                    Header  = value.Replace("\r\n", " ").Replace("\n", " "),
                    Content = value
                });
            }
        }
        return Task.FromResult(new CompleteOptionsLoadResult()
        {
            StatusCode = RpcStatusCode.Success,
            Data       = data
        });
    }
}

/// <summary>
/// antd options demo: builds the value@domain email options from the input text.
/// </summary>
public class EmailOptionsAsyncLoader : ICompleteOptionsAsyncLoader
{
    private static readonly string[] Suffixes = ["gmail.com", "163.com", "qq.com"];

    public Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token)
    {
        List<IAutoCompleteOption> data = [];
        if (!string.IsNullOrWhiteSpace(context) && !context.Contains('@'))
        {
            foreach (var suffix in Suffixes)
            {
                var value = $"{context}@{suffix}";
                data.Add(new AutoCompleteOption()
                {
                    Header  = value,
                    Content = value
                });
            }
        }
        return Task.FromResult(new CompleteOptionsLoadResult()
        {
            Data = data
        });
    }
}

/// <summary>
/// antd uncertain-category demo: a random number of "Found {query} on {category}" entries,
/// each carrying a random result count.
/// </summary>
public class UncertainCategoryOptionsAsyncLoader : ICompleteOptionsAsyncLoader
{
    public Task<CompleteOptionsLoadResult> LoadAsync(string? context, CancellationToken token)
    {
        List<IAutoCompleteOption> data   = [];
        var                       random = new Random();

        if (!string.IsNullOrWhiteSpace(context))
        {
            var count = random.Next(5);
            for (var i = 0; i < count; i++)
            {
                var newValue = $"{context}{i}";
                data.Add(new CustomAutoCompleteOption()
                {
                    Header      = $"Found {context} on {newValue}",
                    Content     = newValue,
                    ResultCount = random.Next(100, 200)
                });
            }
        }

        return Task.FromResult(new CompleteOptionsLoadResult()
        {
            Data = data
        });
    }
}

public record CustomAutoCompleteOption : AutoCompleteOption
{
    public int ResultCount { get; set; }
}

/// <summary>
/// Grouped options of the antd certain-category demo: an item with IsGroupHeader true renders as the
/// category title row; every other item renders a title and a citation count.
/// </summary>
public record CategoryAutoCompleteOption : AutoCompleteOption
{
    public bool IsGroupHeader { get; init; }

    public bool IsGroupItem => !IsGroupHeader;

    public int ReferenceCount { get; init; }
}
