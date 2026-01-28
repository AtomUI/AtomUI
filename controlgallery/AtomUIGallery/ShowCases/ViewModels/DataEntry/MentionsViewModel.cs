using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DataLoad;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class MentionsViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Mentions";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
        
    private List<IMentionOption> _basicMentionOptions = [];
    
    public List<IMentionOption> BasicMentionOptions
    {
        get => _basicMentionOptions;
        set => this.RaiseAndSetIfChanged(ref _basicMentionOptions, value);
    }
    
    private MentionOptionsAsyncLoader? _mentionOptionAsyncLoader;
    
    public MentionOptionsAsyncLoader? MentionOptionAsyncLoader
    {
        get => _mentionOptionAsyncLoader;
        set => this.RaiseAndSetIfChanged(ref _mentionOptionAsyncLoader, value);
    }
    
    private List<string> _mentionTriggers = [];
    
    public List<string> MentionTriggers
    {
        get => _mentionTriggers;
        set => this.RaiseAndSetIfChanged(ref _mentionTriggers, value);
    }

    public MentionsViewModel(IScreen screen)
    {
        HostScreen = screen;
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
            Data = options
        };
    }
}

public class RandomUsernameGenerator
{
    private static readonly List<string> FirstNames = new List<string>
    {
        "Alex", "Jordan", "Casey", "Riley", "Taylor", "Morgan", "Cameron",
        "Skye", "Dakota", "Phoenix", "Rowan", "Peyton", "Quinn", "Blake"
    };
    
    private static readonly List<string> LastNames = new List<string>
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller",
        "Davis", "Rodriguez", "Martinez", "Lee", "Walker", "Hall", "Allen"
    };
    
    private static readonly List<string> Adjectives = new List<string>
    {
        "Swift", "Clever", "Brave", "Silent", "Lucky", "Fierce", "Gentle",
        "Witty", "Noble", "Calm", "Radiant", "Vivid", "Solar", "Lunar"
    };
    
    private static readonly List<string> Nouns = new List<string>
    {
        "Wolf", "Eagle", "Fox", "Lion", "Tiger", "Bear", "Hawk", "Falcon",
        "Storm", "River", "Stone", "Mountain", "Shadow", "Flame"
    };
    
    private static readonly Random _random = new Random();
    
    public enum GenerationStyle
    {
        Simple,      // 如: AlexJohnson
        WithNumber,  // 如: AlexJohnson42
        Creative,    // 如: SwiftWolf123
        FullName,    // 如: Alex.Johnson2024
        DotSeparated // 如: Alex.Johnson
    }
    
    public static string Generate(GenerationStyle style = GenerationStyle.WithNumber)
    {
        string firstName = FirstNames[_random.Next(FirstNames.Count)];
        string lastName = LastNames[_random.Next(LastNames.Count)];
        string adjective = Adjectives[_random.Next(Adjectives.Count)];
        string noun = Nouns[_random.Next(Nouns.Count)];
        int randomNumber = _random.Next(10, 9999); // 生成10-9999之间的数字
        
        return style switch
        {
            GenerationStyle.Simple => $"{firstName}{lastName}",
            GenerationStyle.WithNumber => $"{firstName}{lastName}{randomNumber}",
            GenerationStyle.Creative => $"{adjective}{noun}{randomNumber}",
            GenerationStyle.FullName => $"{firstName}.{lastName}{randomNumber}",
            GenerationStyle.DotSeparated => $"{firstName}.{lastName}",
            _ => $"{firstName}{lastName}{randomNumber}"
        };
    }
    
    public static List<string> GenerateBatch(int count, GenerationStyle style = GenerationStyle.WithNumber)
    {
        var usernames = new HashSet<string>(); // 使用HashSet自动去重
        while (usernames.Count < count)
        {
            usernames.Add(Generate(style));
        }
        return usernames.ToList();
    }
}