using System.Reactive;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DataLoad;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Mentions;

public class MentionsViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Mentions";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<IMentionOption>? _basicMentionOptions = [];

    public List<IMentionOption>? BasicMentionOptions
    {
        get => _basicMentionOptions;
        set => this.RaiseAndSetIfChanged(ref _basicMentionOptions, value);
    }

    private IMentionOptionsAsyncLoader? _mentionOptionAsyncLoader;

    public IMentionOptionsAsyncLoader? MentionOptionAsyncLoader
    {
        get => _mentionOptionAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _mentionOptionAsyncLoader, value);
    }

    private List<IMentionOption>? _semanticPreviewOptions;

    public List<IMentionOption>? SemanticPreviewOptions
    {
        get => _semanticPreviewOptions;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewOptions, value);
    }

    private List<IMentionOption>? _styleClassOptions;

    public List<IMentionOption>? StyleClassOptions
    {
        get => _styleClassOptions;
        set => this.RaiseAndSetIfChanged(ref _styleClassOptions, value);
    }

    private List<string>? _mentionTriggers = [];

    public List<string>? MentionTriggers
    {
        get => _mentionTriggers;
        set => this.RaiseAndSetIfChanged(ref _mentionTriggers, value);
    }

    private string? _boundValue = "@afc163";

    public string? BoundValue
    {
        get => _boundValue;
        set
        {
            if (_boundValue == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _boundValue, value);
            this.RaisePropertyChanged(nameof(BoundValueText));
        }
    }

    public string BoundValueText => string.IsNullOrEmpty(BoundValue) ? "-" : BoundValue;

    public ReactiveCommand<Unit, Unit> SetBoundValueCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundValueCommand { get; }

    public MentionsViewModel(IScreen screen)
    {
        HostScreen              = screen;
        SetBoundValueCommand    = ReactiveCommand.Create(SetBoundValue);
        ClearBoundValueCommand  = ReactiveCommand.Create(ClearBoundValue);
    }

    private void SetBoundValue()
    {
        BoundValue = "@zombieJ";
    }

    private void ClearBoundValue()
    {
        BoundValue = null;
    }

}

public class MentionOptionsAsyncLoader : IMentionOptionsAsyncLoader
{
    public async Task<MentionOptionsLoadResult> LoadAsync(string? context, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(600), token);
        List<IMentionOption>? options = null;
        if (!token.IsCancellationRequested)
        {
            var count = Random.Shared.Next(3, 8);
            var names = RandomUsernameGenerator.GenerateBatch(count);
            options = new List<IMentionOption>();
            foreach (var name in names)
            {
                options.Add(new MentionOption()
                {
                    Header = name,
                    Value  = name,
                });
            }
        }

        return new MentionOptionsLoadResult()
        {
            StatusCode = RpcStatusCode.Success,
            Data       = options
        };
    }
}

public class RandomUsernameGenerator
{
    private static readonly List<string> FirstNames =
    [
        "Alex", "Jordan", "Casey", "Riley", "Taylor", "Morgan", "Cameron",
        "Skye", "Dakota", "Phoenix", "Rowan", "Peyton", "Quinn", "Blake"
    ];

    private static readonly List<string> LastNames =
    [
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller",
        "Davis", "Rodriguez", "Martinez", "Lee", "Walker", "Hall", "Allen"
    ];

    private static readonly List<string> Adjectives =
    [
        "Swift", "Clever", "Brave", "Silent", "Lucky", "Fierce", "Gentle",
        "Witty", "Noble", "Calm", "Radiant", "Vivid", "Solar", "Lunar"
    ];

    private static readonly List<string> Nouns =
    [
        "Wolf", "Eagle", "Fox", "Lion", "Tiger", "Bear", "Hawk", "Falcon",
        "Storm", "River", "Stone", "Mountain", "Shadow", "Flame"
    ];

    private static readonly Random _random = new();

    public enum GenerationStyle
    {
        Simple,
        WithNumber,
        Creative,
        FullName,
        DotSeparated
    }

    public static string Generate(GenerationStyle style = GenerationStyle.WithNumber)
    {
        string firstName = FirstNames[_random.Next(FirstNames.Count)];
        string lastName = LastNames[_random.Next(LastNames.Count)];
        string adjective = Adjectives[_random.Next(Adjectives.Count)];
        string noun = Nouns[_random.Next(Nouns.Count)];
        int randomNumber = _random.Next(10, 9999);

        return style switch
        {
            GenerationStyle.Simple       => $"{firstName}{lastName}",
            GenerationStyle.WithNumber   => $"{firstName}{lastName}{randomNumber}",
            GenerationStyle.Creative     => $"{adjective}{noun}{randomNumber}",
            GenerationStyle.FullName     => $"{firstName}.{lastName}{randomNumber}",
            GenerationStyle.DotSeparated => $"{firstName}.{lastName}",
            _                            => $"{firstName}{lastName}{randomNumber}"
        };
    }

    public static List<string> GenerateBatch(int count, GenerationStyle style = GenerationStyle.WithNumber)
    {
        var usernames = new HashSet<string>();
        while (usernames.Count < count)
        {
            usernames.Add(Generate(style));
        }

        return usernames.ToList();
    }
}
