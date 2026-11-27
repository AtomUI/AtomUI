namespace AtomUI.Controls;

/// <summary>
/// Supplies the content and the state of the radio button that a group creates for a data item.
/// </summary>
/// <remarks>
/// An <c>AbstractRadioButtonGroup</c> reads the values when it prepares the radio button container for the item;
/// later changes are not picked up.
/// </remarks>
public interface IRadioButtonOption
{
    /// <summary>
    /// Gets whether the radio button accepts input.
    /// </summary>
    bool IsEnabled { get; }
    /// <summary>
    /// Gets the text of the radio button. A group without an <c>ItemTemplate</c> uses it as the content of the radio
    /// button and applies no content template.
    /// </summary>
    object? Content { get; }
    /// <summary>
    /// Gets whether the radio button is checked when the group prepares its container.
    /// </summary>
    bool IsChecked { get; }
}

/// <summary>
/// Mutable option that can be used as a data item of a radio button group.
/// </summary>
public class RadioButtonOption : IRadioButtonOption
{
    /// <summary>
    /// Gets or sets whether the radio button accepts input. The default is <see langword="true" />.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    /// <summary>
    /// Gets or sets the text of the radio button. The default is <see langword="null" />.
    /// </summary>
    public object? Content { get; set; }
    /// <summary>
    /// Gets or sets whether the radio button is checked. The default is <see langword="false" />.
    /// </summary>
    public bool IsChecked { get; set; } = false;
}