using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class Empty : AbstractEmpty
{
    // Description is object-typed. Strings keep rendering through the TextBlock part; any other object
    // is shown through a content presenter, so string descriptions render exactly as before.
    internal static readonly DirectProperty<Empty, string?> DescriptionTextProperty =
        AvaloniaProperty.RegisterDirect<Empty, string?>(nameof(DescriptionText), o => o.DescriptionText);

    internal static readonly DirectProperty<Empty, object?> DescriptionContentProperty =
        AvaloniaProperty.RegisterDirect<Empty, object?>(nameof(DescriptionContent), o => o.DescriptionContent);

    internal static readonly DirectProperty<Empty, bool> IsDescriptionTextVisibleProperty =
        AvaloniaProperty.RegisterDirect<Empty, bool>(nameof(IsDescriptionTextVisible), o => o.IsDescriptionTextVisible);

    internal static readonly DirectProperty<Empty, bool> IsDescriptionContentVisibleProperty =
        AvaloniaProperty.RegisterDirect<Empty, bool>(nameof(IsDescriptionContentVisible), o => o.IsDescriptionContentVisible);

    private string? _descriptionText;
    private object? _descriptionContent;
    private bool _isDescriptionTextVisible = true;
    private bool _isDescriptionContentVisible;

    internal string? DescriptionText => _descriptionText;

    internal object? DescriptionContent => _descriptionContent;

    internal bool IsDescriptionTextVisible => _isDescriptionTextVisible;

    internal bool IsDescriptionContentVisible => _isDescriptionContentVisible;

    public Empty()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DescriptionProperty ||
            change.Property == IsDescriptionVisibleProperty)
        {
            UpdateDescriptionParts();
        }
    }

    private void UpdateDescriptionParts()
    {
        var description = Description;
        var content = description is null or string ? null : description;
        var isVisible = IsDescriptionVisible;
        SetAndRaise(DescriptionTextProperty, ref _descriptionText, description as string);
        SetAndRaise(DescriptionContentProperty, ref _descriptionContent, content);
        SetAndRaise(IsDescriptionTextVisibleProperty, ref _isDescriptionTextVisible, isVisible && content is null);
        SetAndRaise(IsDescriptionContentVisibleProperty, ref _isDescriptionContentVisible, isVisible && content is not null);
    }
}
