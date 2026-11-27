using System.Collections.Specialized;
using AtomUI.Controls;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// One option in the data of a cascader. It extends <see cref="ITreeNode{ICascaderOption}" /> with a writable
/// header, a value, and the check, expand and leaf state that the cascader view reads and writes while the
/// option is shown.
/// </summary>
public interface ICascaderOption : ITreeNode<ICascaderOption>
{
    /// <summary>
    /// The text that the option shows in its column. The view renders its string form.
    /// </summary>
    new object? Header { get; set; }
    /// <summary>
    /// The check state of the option: <see langword="true" /> for checked, <see langword="false" /> for unchecked
    /// and <see langword="null" /> when only some of its children are checked. The view writes it when the user
    /// toggles a check box and when the state of the children changes.
    /// </summary>
    bool? IsChecked { get; set; }
    /// <summary>
    /// Whether the check box of the option is enabled. While it is disabled the option cannot be checked, and a
    /// parent that is checked does not check it. The check box also requires the option to be enabled.
    /// </summary>
    bool IsCheckBoxEnabled { get; set; }
    /// <summary>
    /// Whether the children of the option are shown in the next column. The view updates it when the user expands
    /// or collapses the option.
    /// </summary>
    bool IsExpanded { get; set; }
    /// <summary>
    /// Marks the option as the end of a selection path. Such an option counts as a leaf even when it has children,
    /// so it can be selected while the view does not allow selecting parents, and its children are not loaded on
    /// demand.
    /// </summary>
    bool IsLeaf { get; set; }
    /// <summary>
    /// The value of the option. A segment of an option path, such as the <c>DefaultSelectOptionPath</c> of a
    /// cascader, matches an option by this value when the option has no <see cref="IItemKey.ItemKey" />.
    /// </summary>
    object? Value { get; set; }
    
    /// <summary>
    /// Sets the parent of the option. Passing <see langword="null" /> detaches the option.
    /// </summary>
    /// <param name="parentNode">
    /// The new parent, or <see langword="null" /> to detach the option.
    /// </param>
    /// <remarks>
    /// <see cref="CascaderOption" /> calls it for every option added to or removed from its children, and the
    /// cascader view calls it for children that were loaded on demand. The default implementation throws
    /// <see cref="NotImplementedException" />, so an option type must supply its own.
    /// </remarks>
    void UpdateParentNode(ICascaderOption? parentNode)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Provides queries on <see cref="ICascaderOption" /> that decide whether an option counts as a leaf.
/// </summary>
public static class ICascaderOptionExtensions
{
    /// <summary>
    /// Decides whether an option counts as a leaf when it is selected. It does when
    /// <see cref="ICascaderOption.IsLeaf" /> is <see langword="true" /> or when the option has no children.
    /// </summary>
    /// <param name="option">
    /// The option to check.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the option counts as a leaf; otherwise <see langword="false" />.
    /// </returns>
    public static bool IsEffectiveLeaf(this ICascaderOption option)
    {
        return option.IsLeaf || !option.HasChildren();
    }

    internal static bool HasChildren(this ICascaderOption option)
    {
        return option.Children.HasChildren();
    }

    internal static bool HasChildren(this IEnumerable<ICascaderOption> children)
    {
        if (children is ICollection<ICascaderOption> collection)
        {
            return collection.Count > 0;
        }

        if (children is IReadOnlyCollection<ICascaderOption> readOnlyCollection)
        {
            return readOnlyCollection.Count > 0;
        }

        foreach (var _ in children)
        {
            return true;
        }
        return false;
    }
}

/// <summary>
/// The built-in option type of a cascader: a header, an optional value and child options. In a multiple
/// selection, the tag of a selected option shows the header text.
/// </summary>
public record CascaderOption : ICascaderOption, ISelectTagTextProvider
{
    /// <summary>
    /// The parent of the option, or <see langword="null" /> for a root option or for one that was removed from the
    /// children of its parent. Only <see cref="UpdateParentNode" /> changes it.
    /// </summary>
    public ITreeNode<ICascaderOption>? ParentNode { get; private set; }
    
    /// <summary>
    /// The key of the option. A segment of an option path, such as the <c>DefaultSelectOptionPath</c> of a
    /// cascader, matches an option by this key first, and by <see cref="Value" /> when the key is
    /// <see langword="null" />. The default is <see langword="null" />.
    /// </summary>
    public EntityKey? ItemKey { get; init; }
    /// <summary>
    /// The text of the option, see <see cref="ICascaderOption.Header" />. In a multiple selection it is also the
    /// text of the tag of a selected option.
    /// </summary>
    public object? Header { get; set; }
    /// <summary>
    /// The icon shown before the text of the option. The default is <see langword="null" />, which shows no icon.
    /// </summary>
    public PathIcon? Icon { get; set; }
    /// <summary>
    /// Whether the option can be selected and checked. An option is selectable only while it and all of its
    /// ancestors are enabled. The default is <see langword="true" />.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    /// <summary>
    /// The check state of the option, see <see cref="ICascaderOption.IsChecked" />. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool? IsChecked { get; set; } = false;
    /// <summary>
    /// Marks the option as the end of a selection path, see <see cref="ICascaderOption.IsLeaf" />. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool IsLeaf { get; set; } = false;
    /// <summary>
    /// Whether the children of the option are shown in the next column, see
    /// <see cref="ICascaderOption.IsExpanded" />. The default is <see langword="false" />.
    /// </summary>
    public bool IsExpanded { get; set; }
    /// <summary>
    /// Whether the check box of the option is enabled, see <see cref="ICascaderOption.IsCheckBoxEnabled" />.
    /// The default is <see langword="true" />.
    /// </summary>
    public bool IsCheckBoxEnabled { get; set; } = true;
    /// <inheritdoc />
    public object? Value { get; set; }
    
    string? ISelectTagTextProvider.TagText => Header?.ToString();
    private readonly AvaloniaList<ICascaderOption> _children = [];
    
    /// <summary>
    /// The child options. The getter returns the live list, which the cascader view shows in the next column, and
    /// adding or removing an option there updates its <see cref="ParentNode" />.
    /// </summary>
    /// <remarks>
    /// The <c>init</c> accessor appends the given options to the list instead of replacing its content.
    /// </remarks>
    [Content]
    public IList<ICascaderOption> Children
    {
        get => _children;
        init => _children.AddRange(value);
    }

    IEnumerable<ICascaderOption> ITreeNode<ICascaderOption>.Children => Children;
    
    /// <summary>
    /// Sets <see cref="ParentNode" /> to <paramref name="parentNode" />, which detaches the option when it is
    /// <see langword="null" />.
    /// </summary>
    /// <param name="parentNode">
    /// The new parent, or <see langword="null" /> to detach the option.
    /// </param>
    public void UpdateParentNode(ICascaderOption? parentNode)
    {
        ParentNode = parentNode;
    }

    public CascaderOption()
    {
        _children.CollectionChanged += HandleCollectionChanged;
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            if (e.NewItems != null)
            {
                foreach (var child in e.NewItems)
                {
                    if (child is ICascaderOption option)
                    {
                        option.UpdateParentNode(this);
                    }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var child in e.OldItems)
                {
                    if (child is ICascaderOption option)
                    {
                        option.UpdateParentNode(null);
                    }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var child in Children)
            {
                child.UpdateParentNode(this);
            }
        }
    }
}
