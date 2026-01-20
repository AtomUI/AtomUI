using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[Flags]
public enum TextBlockHighlightStrategy
{
    HighlightedMatch = 0x01,
    HighlightedWhole = 0x02,
    BoldedMatch = 0x04,
    HideUnMatched = 0x8,
    All = HighlightedMatch | BoldedMatch | HideUnMatched
}

public class HighlightableTextBlock : TextBlock
{
    #region 公共属性定义
    public static readonly StyledProperty<TextBlockHighlightStrategy> HighlightStrategyProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, TextBlockHighlightStrategy>(nameof(HighlightStrategy), TextBlockHighlightStrategy.All);
    
    public static readonly StyledProperty<string?> HighlightWordsProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, string?>(nameof(HighlightWords));
    
    public static readonly StyledProperty<IBrush?> HighlightForegroundProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, IBrush?>(nameof(HighlightForeground));
    
    public static readonly StyledProperty<IBrush?> HighlightBackgroundProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, IBrush?>(nameof(HighlightBackground));
    
    public TextBlockHighlightStrategy HighlightStrategy
    {
        get => GetValue(HighlightStrategyProperty);
        set => SetValue(HighlightStrategyProperty, value);
    }
    
    public string? HighlightWords
    {
        get => GetValue(HighlightWordsProperty);
        set => SetValue(HighlightWordsProperty, value);
    }
    
    public IBrush? HighlightForeground
    {
        get => GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }
    
    public IBrush? HighlightBackground
    {
        get => GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }
    #endregion

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == HighlightStrategyProperty ||
            change.Property == HighlightWordsProperty ||
            change.Property == HighlightBackgroundProperty ||
            change.Property == HighlightForegroundProperty ||
            change.Property == TextProperty)
        {
            NotifyBuildFilterHighlightRuns();
        }
    }

    protected virtual void NotifyBuildFilterHighlightRuns()
    {
        if (!string.IsNullOrEmpty(HighlightWords))
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return;
            }
            Debug.Assert(Text != null);
            var highlightRanges = CalculateHighlightRanges(HighlightWords, Text);
            var runs            = new InlineCollection();
            for (var i = 0; i < Text.Length; i++)
            {
                var c   =  Text[i];
                var run = new Run($"{c}");
                
                if (HighlightStrategy.HasFlag(TextBlockHighlightStrategy.HighlightedMatch))
                {
                    if (IsNeedHighlight(i, highlightRanges))
                    {
                        run.Foreground = HighlightForeground;
                        run.Background = HighlightBackground;
                        if (HighlightStrategy.HasFlag(TextBlockHighlightStrategy.BoldedMatch))
                        {
                            run.FontWeight = FontWeight.Bold;
                        }
                    }
                }
                else if (HighlightStrategy.HasFlag(TextBlockHighlightStrategy.HighlightedWhole))
                {
                    run.Foreground = HighlightForeground;
                    run.Background = HighlightBackground;
                    if (HighlightStrategy.HasFlag(TextBlockHighlightStrategy.BoldedMatch))
                    {
                        run.FontWeight = FontWeight.Bold;
                    }
                }
            
                runs.Add(run);
            }
        
            Inlines = runs;
        }
        else
        {
            Inlines = null;
        }
    }
    
    protected bool IsNeedHighlight(int pos, in List<(int, int)> ranges)
    {
        foreach (var range in ranges)
        {
            if (pos >= range.Item1 && pos < range.Item2)
            {
                return true;
            }
        }

        return false;
    }
    
    protected List<(int, int)> CalculateHighlightRanges(string highlightWords, string text)
    {
        var ranges          = new List<(int, int)>();
        int currentIndex    = 0;
        var highlightLength = highlightWords.Length;
        
        while (true)
        {
            int foundIndex = text.IndexOf(highlightWords, currentIndex, StringComparison.OrdinalIgnoreCase);
            if (foundIndex == -1) // 如果没有找到，退出循环
            {
                break;
            }
                
            currentIndex = foundIndex + highlightLength;
            ranges.Add((foundIndex, currentIndex));
        }
        return ranges;
    }
}