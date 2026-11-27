using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Selects how a <see cref="HighlightableTextBlock" /> marks the parts of its text that match its highlight
/// words. The values can be combined; <see cref="HighlightedMatch" /> takes precedence over
/// <see cref="HighlightedWhole" />.
/// </summary>
[Flags]
public enum TextBlockHighlightStrategy
{
    /// <summary>
    /// Gives each occurrence of the highlight words in the text its own highlighted run.
    /// </summary>
    HighlightedMatch = 0x01,
    /// <summary>
    /// Highlights the whole text as one run when it contains the highlight words. Ignored when
    /// <see cref="HighlightedMatch" /> is also set.
    /// </summary>
    HighlightedWhole = 0x02,
    /// <summary>
    /// Also renders the highlighted runs in bold. Has no effect unless <see cref="HighlightedMatch" /> or
    /// <see cref="HighlightedWhole" /> is set.
    /// </summary>
    BoldedMatch = 0x04,
    /// <summary>
    /// Has no effect: <see cref="HighlightableTextBlock" /> never reads this flag, and text that does not match
    /// is always shown.
    /// </summary>
    HideUnMatched = 0x8,
    /// <summary>
    /// Combines <see cref="HighlightedMatch" />, <see cref="BoldedMatch" /> and <see cref="HideUnMatched" />.
    /// This is the default of <see cref="HighlightableTextBlock.HighlightStrategy" />.
    /// </summary>
    All = HighlightedMatch | BoldedMatch | HideUnMatched
}

/// <summary>
/// A <see cref="TextBlock" /> that highlights the occurrences of <see cref="HighlightWords" /> in its text,
/// for example to show why an item matched a filter. The highlighting works by replacing the inherited
/// <c>Inlines</c>, so <c>Inlines</c> set directly are lost whenever the highlighting is rebuilt.
/// </summary>
public class HighlightableTextBlock : TextBlock
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="HighlightStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<TextBlockHighlightStrategy> HighlightStrategyProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, TextBlockHighlightStrategy>(nameof(HighlightStrategy), TextBlockHighlightStrategy.All);
    
    /// <summary>
    /// Identifies the <see cref="HighlightWords" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> HighlightWordsProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, string?>(nameof(HighlightWords));
    
    /// <summary>
    /// Identifies the <see cref="HighlightForeground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HighlightForegroundProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, IBrush?>(nameof(HighlightForeground));
    
    /// <summary>
    /// Identifies the <see cref="HighlightBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> HighlightBackgroundProperty =
        AvaloniaProperty.Register<HighlightableTextBlock, IBrush?>(nameof(HighlightBackground));
    
    /// <summary>
    /// Gets or sets which highlighting is applied to the text. The default is
    /// <see cref="TextBlockHighlightStrategy.All" />. Changing the value rebuilds the highlighting; a value
    /// that sets neither <see cref="TextBlockHighlightStrategy.HighlightedMatch" /> nor
    /// <see cref="TextBlockHighlightStrategy.HighlightedWhole" /> leaves the text unstyled.
    /// </summary>
    public TextBlockHighlightStrategy HighlightStrategy
    {
        get => GetValue(HighlightStrategyProperty);
        set => SetValue(HighlightStrategyProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the text to highlight in <c>Text</c>. The whole string is one search term, matched
    /// against the text ignoring case; it is not split into words. When <see langword="null" /> or empty, the
    /// inherited <c>Inlines</c> are cleared and the text is shown plain.
    /// </summary>
    public string? HighlightWords
    {
        get => GetValue(HighlightWordsProperty);
        set => SetValue(HighlightWordsProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the foreground of the highlighted runs. When <see langword="null" />, the runs use the
    /// foreground of the text block.
    /// </summary>
    public IBrush? HighlightForeground
    {
        get => GetValue(HighlightForegroundProperty);
        set => SetValue(HighlightForegroundProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the background of the highlighted runs. When <see langword="null" />, no background is
    /// painted behind them.
    /// </summary>
    public IBrush? HighlightBackground
    {
        get => GetValue(HighlightBackgroundProperty);
        set => SetValue(HighlightBackgroundProperty, value);
    }
    #endregion

    /// <summary>
    /// Rebuilds the highlighting when <see cref="HighlightStrategy" />, <see cref="HighlightWords" />,
    /// <see cref="HighlightForeground" />, <see cref="HighlightBackground" /> or the inherited <c>Text</c>
    /// changes. The base method is called first; call it from an override.
    /// </summary>
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

    /// <summary>
    /// Rebuilds the inherited <c>Inlines</c> from <c>Text</c>, <see cref="HighlightWords" />,
    /// <see cref="HighlightStrategy" /> and the two highlight brushes. Clears <c>Inlines</c> when
    /// <see cref="HighlightWords" /> is <see langword="null" /> or empty, and leaves them untouched while
    /// <c>Text</c> is null or white space. Call the base method from an override to keep the highlighting up
    /// to date.
    /// </summary>
    protected virtual void NotifyBuildFilterHighlightRuns()
    {
        if (string.IsNullOrEmpty(HighlightWords))
        {
            Inlines = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        Debug.Assert(Text != null);

        var strategy        = HighlightStrategy;
        var highlightMatch  = (strategy & TextBlockHighlightStrategy.HighlightedMatch) == TextBlockHighlightStrategy.HighlightedMatch;
        var highlightWhole  = !highlightMatch && (strategy & TextBlockHighlightStrategy.HighlightedWhole) == TextBlockHighlightStrategy.HighlightedWhole;
        var bold            = (strategy & TextBlockHighlightStrategy.BoldedMatch) == TextBlockHighlightStrategy.BoldedMatch;

        // HideUnMatched is not read here and was not read by the implementation this one replaced, so unmatched
        // text is always shown.
        if (highlightWhole)
        {
            var single = new InlineCollection { CreateRun(Text, isHighlighted: true, bold) };
            Inlines = single;
            return;
        }

        var firstHighlightIndex = Text.IndexOf(HighlightWords, StringComparison.OrdinalIgnoreCase);
        if (firstHighlightIndex == -1)
        {
            var plainRuns = new InlineCollection();
            plainRuns.Add(CreateRun(Text, isHighlighted: false, bold: false));
            Inlines = plainRuns;
            return;
        }

        var highlightRanges = CalculateHighlightRanges(HighlightWords, Text, firstHighlightIndex);
        var runs            = new InlineCollection();

        var cursor = 0;
        foreach (var (start, end) in highlightRanges)
        {
            if (start > cursor)
            {
                runs.Add(CreateRun(Text.Substring(cursor, start - cursor), isHighlighted: false, bold: false));
            }
            runs.Add(CreateRun(Text.Substring(start, end - start), isHighlighted: highlightMatch, bold: highlightMatch && bold));
            cursor = end;
        }
        if (cursor < Text.Length)
        {
            runs.Add(CreateRun(Text.Substring(cursor), isHighlighted: false, bold: false));
        }

        Inlines = runs;
    }

    private Run CreateRun(string segment, bool isHighlighted, bool bold)
    {
        var run = new Run(segment);
        if (isHighlighted)
        {
            run.Foreground = HighlightForeground;
            run.Background = HighlightBackground;
            if (bold)
            {
                run.FontWeight = FontWeight.Bold;
            }
        }
        return run;
    }
    
    /// <summary>
    /// Returns whether the character position <paramref name="pos" /> lies inside one of the half-open
    /// character ranges in <paramref name="ranges" />. This class does not call it.
    /// </summary>
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
    
    /// <summary>
    /// Returns the half-open character ranges (start index, exclusive end index) of the occurrences of
    /// <paramref name="highlightWords" /> in <paramref name="text" />, in order of appearance. Matching ignores
    /// case, occurrences do not overlap, and each range has the length of <paramref name="highlightWords" />.
    /// </summary>
    /// <param name="highlightWords">
    /// The text to find.
    /// </param>
    /// <param name="text">
    /// The text to search.
    /// </param>
    /// <param name="firstIndex">
    /// The index of the first occurrence. It is added as a range without being checked; the search for further
    /// occurrences starts after it. Pass -1 to get an empty list.
    /// </param>
    protected List<(int, int)> CalculateHighlightRanges(string highlightWords, string text, int firstIndex = 0)
    {
        var ranges          = new List<(int, int)>(1);
        var highlightLength = highlightWords.Length;
        var foundIndex      = firstIndex;
        
        while (true)
        {
            if (foundIndex == -1) // Nothing found, exit the loop.
            {
                break;
            }
                
            var currentIndex = foundIndex + highlightLength;
            ranges.Add((foundIndex, currentIndex));
            foundIndex = text.IndexOf(highlightWords, currentIndex, StringComparison.OrdinalIgnoreCase);
        }
        return ranges;
    }
}
