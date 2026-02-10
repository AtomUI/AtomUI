using Avalonia;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public class AutoCompleteTextArea : AbstractAutoComplete
{
    #region 公共属性定义
    
    public static readonly StyledProperty<int> LinesProperty =
        TextArea.LinesProperty.AddOwner<AutoCompleteTextArea>();
    
    public static readonly StyledProperty<bool> IsAutoSizeProperty =
        TextArea.IsAutoSizeProperty.AddOwner<AutoCompleteTextArea>();
    
    public static readonly StyledProperty<bool> IsShowCountProperty =
        TextArea.IsShowCountProperty.AddOwner<AutoCompleteTextArea>();

    public static readonly StyledProperty<bool> IsResizableProperty =
        TextArea.IsResizableProperty.AddOwner<AutoCompleteTextArea>();
    
    public int Lines
    {
        get => GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }
    
    public bool IsAutoSize
    {
        get => GetValue(IsAutoSizeProperty);
        set => SetValue(IsAutoSizeProperty, value);
    }
    
    public bool IsShowCount
    {
        get => GetValue(IsShowCountProperty);
        set => SetValue(IsShowCountProperty, value);
    }
    
    public bool IsResizable
    {
        get => GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }
    
    #endregion

    private bool _ignoreCaretIndexChanged;

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
    
    private protected override void HandleCandidateListComplete(object? sender, RoutedEventArgs e)
    {
        _ignoreCaretIndexChanged = true;
        base.HandleCandidateListComplete(sender, e);
    }
}