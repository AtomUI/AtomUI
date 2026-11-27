using System.ComponentModel;
using System.Runtime.CompilerServices;
using AtomUI.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One item of the cell list of an <see cref="OtpLineEdit" /> and the data context of the template that draws
/// one character cell together with the separator after it.
/// </summary>
/// <remarks>
/// The setters are internal. The <see cref="OtpLineEdit" /> assigns the values of every cell whenever its state
/// changes, and each changed value raises <see cref="PropertyChanged" />.
/// </remarks>
public sealed class OtpLineEditCellInfo : INotifyPropertyChanged
{
    /// <summary>
    /// Text shown in the cell. It is the mask character when the input is masked, otherwise the typed character,
    /// and <see langword="null" /> for a cell that holds no character.
    /// </summary>
    public string? DisplayText
    {
        get => _displayText;
        internal set => SetField(ref _displayText, value);
    }

    /// <summary>
    /// Text shown while the cell holds no character. When <see cref="OtpLineEdit.PlaceholderText" /> has exactly
    /// one character per cell, this is the character at the index of the cell; otherwise it is the whole
    /// placeholder, and it is <see langword="null" /> when no placeholder is set.
    /// </summary>
    public string? PlaceholderText
    {
        get => _placeholderText;
        internal set => SetField(ref _placeholderText, value);
    }

    /// <summary>
    /// Whether this cell is the active cell. It is <see langword="true" /> for the cell at the active index while
    /// the input has the keyboard focus and is enabled; the cell then shows the caret and the active frame.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        internal set => SetField(ref _isActive, value);
    }

    /// <summary>
    /// Whether this cell is the one the user edits. It is <see langword="true" /> for the cell at the active index
    /// while the input is enabled and can be edited; the cell then shows the text input cursor instead of the
    /// arrow pointer.
    /// </summary>
    public bool IsInputTarget
    {
        get => _isInputTarget;
        internal set => SetField(ref _isInputTarget, value);
    }

    /// <summary>
    /// Whether the cell plays its motion. It follows <see cref="OtpLineEdit.IsMotionEnabled" /> of the input.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => _isMotionEnabled;
        internal set => SetField(ref _isMotionEnabled, value);
    }

    /// <summary>
    /// Status that the frame of the cell is painted with. Every cell receives the effective status of the input,
    /// so an error or a warning colors all cells alike.
    /// </summary>
    public InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        internal set => SetField(ref _effectiveStatus, value);
    }

    /// <summary>
    /// Size of the cell. It follows <see cref="OtpLineEdit.SizeType" /> of the input.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => _sizeType;
        internal set => SetField(ref _sizeType, value);
    }

    /// <summary>
    /// Style variant of the frame of the cell. It follows <see cref="OtpLineEdit.StyleVariant" /> of the input.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => _styleVariant;
        internal set => SetField(ref _styleVariant, value);
    }

    /// <summary>
    /// Content of the separator after this cell. Without a <see cref="SeparatorTemplate" /> it is the
    /// <see cref="OtpLineEdit.Separator" /> of the input; with one it is an
    /// <see cref="OtpLineEditSeparatorContext" />. It is <see langword="null" /> when no separator follows this cell.
    /// </summary>
    public object? Separator
    {
        get => _separator;
        internal set => SetField(ref _separator, value);
    }

    /// <summary>
    /// Template that builds the separator after this cell. It follows
    /// <see cref="OtpLineEdit.SeparatorTemplate" /> of the input; without a template the separator content is
    /// shown directly.
    /// </summary>
    public IDataTemplate? SeparatorTemplate
    {
        get => _separatorTemplate;
        internal set => SetField(ref _separatorTemplate, value);
    }

    /// <summary>
    /// Whether a separator is drawn after this cell. It is <see langword="true" /> when the input has a
    /// <see cref="OtpLineEdit.Separator" /> and this cell is not the last one.
    /// </summary>
    public bool IsSeparatorVisible
    {
        get => _isSeparatorVisible;
        internal set => SetField(ref _isSeparatorVisible, value);
    }

    /// <summary>
    /// Raised after the value of one of the properties of this item changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    internal bool JoinsPreviousCell
    {
        get => _joinsPreviousCell;
        set => SetField(ref _joinsPreviousCell, value);
    }

    internal bool JoinsNextCell
    {
        get => _joinsNextCell;
        set => SetField(ref _joinsNextCell, value);
    }

    internal int FocusZIndex
    {
        get => _focusZIndex;
        set => SetField(ref _focusZIndex, value);
    }

    private string? _displayText;
    private string? _placeholderText;
    private bool _isActive;
    private bool _isInputTarget;
    private bool _isMotionEnabled;
    private InputControlStatus _effectiveStatus;
    private CustomizableSizeType _sizeType;
    private InputControlStyleVariant _styleVariant;
    private object? _separator;
    private IDataTemplate? _separatorTemplate;
    private bool _isSeparatorVisible;
    private bool _joinsPreviousCell;
    private bool _joinsNextCell;
    private int _focusZIndex;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
