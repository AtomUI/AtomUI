using Avalonia;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The <see cref="AbstractAutoComplete" /> whose text box is a multi-line <see cref="TextArea" />. The
/// suggestion popup opens next to the line that holds the caret.
/// </summary>
public partial class AutoCompleteTextArea : AbstractAutoComplete
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="Lines" /> property.
    /// </summary>
    public static readonly StyledProperty<int> LinesProperty =
        TextArea.LinesProperty.AddOwner<AutoCompleteTextArea>();
    
    /// <summary>
    /// Identifies the <see cref="IsAutoSize" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoSizeProperty =
        TextArea.IsAutoSizeProperty.AddOwner<AutoCompleteTextArea>();
    
    /// <summary>
    /// Identifies the <see cref="IsShowCount" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowCountProperty =
        TextArea.IsShowCountProperty.AddOwner<AutoCompleteTextArea>();

    /// <summary>
    /// Identifies the <see cref="IsResizable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsResizableProperty =
        TextArea.IsResizableProperty.AddOwner<AutoCompleteTextArea>();
    
    /// <summary>
    /// The number of text rows the input is high for; longer text scrolls inside it. The default is 2. The value
    /// is passed to the inner <see cref="TextArea" />. It sets the height only while <see cref="IsAutoSize" /> is
    /// false, the value is greater than 0 and the inner <see cref="TextArea" /> has no <c>Height</c>.
    /// </summary>
    public int Lines
    {
        get => GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }
    
    /// <summary>
    /// <see langword="true" /> makes the input grow and shrink with its text; <see cref="Lines" /> then no longer
    /// sets the height. The default is <see langword="false" />. The value is passed to the inner
    /// <see cref="TextArea" />.
    /// </summary>
    public bool IsAutoSize
    {
        get => GetValue(IsAutoSizeProperty);
        set => SetValue(IsAutoSizeProperty, value);
    }
    
    /// <summary>
    /// <see langword="true" /> shows a counter for the length of the entered text. The default is
    /// <see langword="false" />. The value is passed to the inner <see cref="TextArea" />.
    /// </summary>
    public bool IsShowCount
    {
        get => GetValue(IsShowCountProperty);
        set => SetValue(IsShowCountProperty, value);
    }
    
    /// <summary>
    /// <see langword="true" /> shows a resize handle on the input with which the user drags its height. The
    /// default is <see langword="false" />. The value is passed to the inner <see cref="TextArea" />.
    /// </summary>
    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }
    
    #endregion

    private bool _ignoreCaretIndexChanged;

    /// <summary>
    /// Called when a property of the control changes. After the base implementation has run, it moves the
    /// suggestion popup to the line that holds the caret when <see cref="AbstractAutoComplete.CaretIndex" /> or
    /// <see cref="AbstractAutoComplete.Placement" /> changes. The caret change that follows the choice of a
    /// candidate is skipped once.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CaretIndexProperty || change.Property == PlacementProperty)
        {
            if (_ignoreCaretIndexChanged)
            {
                _ignoreCaretIndexChanged = false;
                return;
            }
            ConfigurePopupOffset();
        }
    }

    private void ConfigurePopupOffset()
    {
        if (TextInputBox is AutoCompleteTextAreaBox textBox)
        {
            var caretBounds = textBox.GetCaretBounds(this);
            if (_popup != null)
            {
                if (Placement == AutoCompletePlacementMode.Bottom)
                {
                    _popup.VerticalOffset = Math.Min(-(DesiredSize.Height - caretBounds.Y) + caretBounds.Height, 0);
                }
                else
                {
                    _popup.VerticalOffset = caretBounds.Y;
                }
            }
        }
    }
    
    /// <summary>
    /// Commits the chosen candidate through the base method. Before that it marks the next
    /// <see cref="AbstractAutoComplete.CaretIndex" /> or <see cref="AbstractAutoComplete.Placement" /> change, so
    /// that <see cref="OnPropertyChanged" /> does not move the suggestion popup for it.
    /// </summary>
    private protected override void HandleCandidateListComplete(object? sender, RoutedEventArgs e)
    {
        _ignoreCaretIndexChanged = true;
        base.HandleCandidateListComplete(sender, e);
    }
}