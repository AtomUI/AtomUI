using System.ComponentModel;
using System.Runtime.CompilerServices;
using AtomUI.Controls;
using Avalonia.Controls.Templates;

namespace AtomUI.Desktop.Controls;

public sealed class OtpLineEditCellInfo : INotifyPropertyChanged
{
    public string? DisplayText
    {
        get => _displayText;
        internal set => SetField(ref _displayText, value);
    }

    public string? PlaceholderText
    {
        get => _placeholderText;
        internal set => SetField(ref _placeholderText, value);
    }

    public bool IsActive
    {
        get => _isActive;
        internal set => SetField(ref _isActive, value);
    }

    public bool IsInputTarget
    {
        get => _isInputTarget;
        internal set => SetField(ref _isInputTarget, value);
    }

    public bool IsMotionEnabled
    {
        get => _isMotionEnabled;
        internal set => SetField(ref _isMotionEnabled, value);
    }

    public InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        internal set => SetField(ref _effectiveStatus, value);
    }

    public CustomizableSizeType SizeType
    {
        get => _sizeType;
        internal set => SetField(ref _sizeType, value);
    }

    public InputControlStyleVariant StyleVariant
    {
        get => _styleVariant;
        internal set => SetField(ref _styleVariant, value);
    }

    public object? Separator
    {
        get => _separator;
        internal set => SetField(ref _separator, value);
    }

    public IDataTemplate? SeparatorTemplate
    {
        get => _separatorTemplate;
        internal set => SetField(ref _separatorTemplate, value);
    }

    public bool IsSeparatorVisible
    {
        get => _isSeparatorVisible;
        internal set => SetField(ref _isSeparatorVisible, value);
    }

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
