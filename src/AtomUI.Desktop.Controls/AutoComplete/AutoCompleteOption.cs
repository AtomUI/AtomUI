using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A suggestion of an <see cref="AbstractAutoComplete" />: a record that implements
/// <see cref="IAutoCompleteOption" /> for items sources that need no option type of their own.
/// </summary>
public record AutoCompleteOption : IAutoCompleteOption
{
    /// <summary>
    /// The text of the row in the suggestion list; the default option template renders it. When
    /// <see cref="Content" /> is <see langword="null" />, it is also the text written into the input when the
    /// option is chosen.
    /// </summary>
    public object? Header { get; set; }
    /// <inheritdoc />
    public string? Group { get; set; }

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// The text written into the input when the option is chosen. When it is <see langword="null" />,
    /// <see cref="Header" /> is used, and then <see cref="ItemKey" />.
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// Whether the option is marked as selected. The default is <see langword="false" />.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// The key of the option, or <see langword="null" /> when it has none. It is the last fallback for the text
    /// written into the input when the option is chosen, after <see cref="Content" /> and <see cref="Header" />.
    /// </summary>
    public EntityKey? ItemKey { get; init; }
}
