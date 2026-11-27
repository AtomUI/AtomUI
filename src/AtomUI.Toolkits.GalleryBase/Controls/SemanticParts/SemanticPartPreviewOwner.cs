using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Declares one owner of a Semantic Part preview: an owner instance anchor plus an optional owner type.
/// When a single preview has to cover several public owners (for example the notice card and the list
/// host of Message), declare them in order on <see cref="SemanticPartPreview.SemanticOwners" />. The
/// preview merges the part lists of the descriptors in declaration order and resolves each highlight
/// against the owner the part belongs to.
/// </summary>
public class SemanticPartPreviewOwner : AvaloniaObject
{
    /// <summary>
    /// Identifies the <see cref="Owner" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> OwnerProperty =
        AvaloniaProperty.Register<SemanticPartPreviewOwner, Control?>(nameof(Owner));

    /// <summary>
    /// Identifies the <see cref="OwnerType" /> property.
    /// </summary>
    public static readonly StyledProperty<Type?> OwnerTypeProperty =
        AvaloniaProperty.Register<SemanticPartPreviewOwner, Type?>(nameof(OwnerType));

    /// <summary>
    /// The owner instance anchor. It supplies the owner type when <see cref="OwnerType" /> is not set,
    /// is used to verify compatibility, and locates every instance of that type in the preview content.
    /// An owner declaration must set it: the preview fails when it is <see langword="null" />.
    /// </summary>
    public Control? Owner
    {
        get => GetValue(OwnerProperty);
        set => SetValue(OwnerProperty, value);
    }

    /// <summary>
    /// The owner type contract. When it is <see langword="null" />, the exact runtime type of
    /// <see cref="Owner" /> is used; the type hierarchy is not searched through reflection.
    /// </summary>
    public Type? OwnerType
    {
        get => GetValue(OwnerTypeProperty);
        set => SetValue(OwnerTypeProperty, value);
    }
}
