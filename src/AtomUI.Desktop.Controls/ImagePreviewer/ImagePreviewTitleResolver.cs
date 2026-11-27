namespace AtomUI.Desktop.Controls;

/// <summary>
/// Derives the title of the native preview window from the item that is currently shown. It is consulted only
/// when the <c>Title</c> of the window is blank and the item has no title of its own.
/// </summary>
public interface IImagePreviewTitleResolver
{
    /// <summary>
    /// Returns the title for the item described by <paramref name="context" />, or <see langword="null" /> when the
    /// item has none. The preview window treats a blank result like <see langword="null" />.
    /// </summary>
    /// <param name="context">
    /// The item that is currently shown, with its position among the items of the preview.
    /// </param>
    string? ResolveTitle(in ImagePreviewTitleResolveContext context);
}

/// <summary>
/// Describes the item that a title is derived for: <c>Item</c> is the item being shown, <c>CurrentIndex</c> is
/// its zero-based position among the <c>Count</c> items of the preview.
/// </summary>
public readonly record struct ImagePreviewTitleResolveContext(
    ImagePreviewItem Item,
    int CurrentIndex,
    int Count);

/// <summary>
/// The default <see cref="IImagePreviewTitleResolver" />. It holds no state, so one instance serves every preview.
/// </summary>
public sealed class DefaultImagePreviewTitleResolver : IImagePreviewTitleResolver
{
    /// <summary>
    /// The single shared instance, used as the default <c>PreviewTitleResolver</c> of a preview.
    /// </summary>
    public static DefaultImagePreviewTitleResolver Instance { get; } = new();

    /// <summary>
    /// Returns the display name of the item's source, or <see langword="null" /> when that name is null, empty or
    /// white space.
    /// </summary>
    /// <param name="context">
    /// The item being shown. Only its <c>Item</c> is read; the position and the count are ignored.
    /// </param>
    public string? ResolveTitle(in ImagePreviewTitleResolveContext context)
    {
        return string.IsNullOrWhiteSpace(context.Item.Source.DisplayName)
            ? null
            : context.Item.Source.DisplayName;
    }
}
