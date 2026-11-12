using AtomUI.Controls.Commons;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class Empty : AbstractEmpty
{
    // Description is object-typed and shown by one content presenter, as Ant Design wraps any description in one
    // node. A null description is presented as empty text so the node keeps the layout of an empty text line.
    internal static readonly DirectProperty<Empty, object?> EffectiveDescriptionProperty =
        AvaloniaProperty.RegisterDirect<Empty, object?>(nameof(EffectiveDescription), o => o.EffectiveDescription);

    private object? _effectiveDescription = string.Empty;

    internal object? EffectiveDescription => _effectiveDescription;

    public Empty()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DescriptionProperty)
        {
            SetAndRaise(EffectiveDescriptionProperty, ref _effectiveDescription, Description ?? string.Empty);
        }
    }
}
