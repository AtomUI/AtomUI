using System.Windows.Input;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A node of the entry tree of a <see cref="NavMenu" />. The menu generates one <see cref="NavMenuItem" /> for
/// each node and reads the header template, tooltip, danger state and command of the item from the node.
/// The members have default implementations for a node without a header template, tooltip, danger state or
/// command, so an implementation overrides only what it changes. <see cref="UpdateParentNode" /> is the
/// exception: its default throws a <c>NotImplementedException</c>, so an implementation must override it.
/// </summary>
public interface INavMenuNode : ITreeNode<INavMenuNode>, INavMenuEntry
{
    /// <summary>
    /// Template that the item generated for the node applies to its header content. When it is
    /// <see langword="null" />, the item presents the header itself, as a text block for a <c>string</c>.
    /// The default implementation returns <see langword="null" />.
    /// </summary>
    IDataTemplate? HeaderTemplate { get; }
    /// <summary>
    /// Tooltip of the item generated for the node. The menu shows it only for a top-level item without a sub-menu,
    /// while the menu is collapsed inline and its collapsed tooltips are enabled; <see langword="null" /> makes the
    /// menu show the header instead. The default implementation returns <see langword="null" />.
    /// </summary>
    object? Tooltip => null;
    /// <summary>
    /// Controls whether the item generated for the node shows its tooltip. <see langword="false" /> suppresses it
    /// even when <see cref="Tooltip" /> is set. The default implementation returns <see langword="true" />.
    /// </summary>
    bool IsTooltipEnabled => true;
    /// <summary>
    /// Marks the item generated for the node as a destructive action, which the header of the item then draws with
    /// its danger colors. The default implementation returns <see langword="false" />.
    /// </summary>
    bool IsDanger => false;
    /// <summary>
    /// Command that the item generated for the node runs when it is clicked, with <see cref="CommandParameter" />
    /// as its parameter. While the command cannot execute, the item is disabled.
    /// The default implementation returns <see langword="null" />.
    /// </summary>
    ICommand? Command => null;
    /// <summary>
    /// Parameter passed to <see cref="Command" /> when the item generated for the node runs it.
    /// The default implementation returns <see langword="null" />.
    /// </summary>
    object? CommandParameter => null;
    /// <summary>
    /// The entries of the sub-menu of the node: child nodes, groups and dividers. The menu builds the sub-menu of
    /// the item from it and traverses it to find items. The default implementation returns <c>Children</c>.
    /// </summary>
    IEnumerable<INavMenuEntry> Entries => Children;
    /// <summary>
    /// Records that the node is an entry of <paramref name="parentNode" />, or of a group that it owns. The owner
    /// calls it when it adds the node to its entries, and with <see langword="null" /> when it removes the node.
    /// The default implementation throws a <c>NotImplementedException</c>.
    /// </summary>
    /// <param name="parentNode">
    /// The node that owns this node from now on, or <see langword="null" /> when the node has no owner any more.
    /// </param>
    void UpdateParentNode(INavMenuNode? parentNode) => throw new NotImplementedException();
}

/// <summary>
/// The built-in <see cref="INavMenuNode" />: a data object that holds the values of one item of a
/// <see cref="NavMenu" /> entry tree together with the entries of its sub-menu. The menu forwards the values to
/// the <see cref="NavMenuItem" /> that it generates for the node.
/// </summary>
[GenerateScopedResourceHost]
public partial class NavMenuNode : AvaloniaObject, INavMenuNode
{
    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, IDataTemplate?>(
            nameof(HeaderTemplate),
            o => o.HeaderTemplate,
            (o, v) => o.HeaderTemplate = v);

    /// <summary>
    /// Identifies the <see cref="Tooltip" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, object?> TooltipProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, object?>(
            nameof(Tooltip),
            o => o.Tooltip,
            (o, v) => o.Tooltip = v);

    /// <summary>
    /// Identifies the <see cref="IsTooltipEnabled" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, bool> IsTooltipEnabledProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, bool>(
            nameof(IsTooltipEnabled),
            o => o.IsTooltipEnabled,
            (o, v) => o.IsTooltipEnabled = v,
            unsetValue: true);

    /// <summary>
    /// Identifies the <see cref="Command" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, ICommand?> CommandProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, ICommand?>(
            nameof(Command),
            o => o.Command,
            (o, v) => o.Command = v);

    /// <summary>
    /// Identifies the <see cref="CommandParameter" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, object?> CommandParameterProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, object?>(
            nameof(CommandParameter),
            o => o.CommandParameter,
            (o, v) => o.CommandParameter = v);
    
    /// <summary>
    /// Identifies the <see cref="ItemKey" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, EntityKey?> ItemKeyProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, EntityKey?>(
            nameof(ItemKey),
            o => o.ItemKey,
            (o, v) => o.ItemKey = v);
    
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, PathIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, PathIcon?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);
    
    /// <summary>
    /// Identifies the <see cref="IsEnabled" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, bool>(
            nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);

    /// <summary>
    /// Identifies the <see cref="IsDanger" /> property.
    /// </summary>
    public static readonly DirectProperty<NavMenuNode, bool> IsDangerProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, bool>(
            nameof(IsDanger),
            o => o.IsDanger,
            (o, v) => o.IsDanger = v);
        
    private object? _header;

    /// <summary>
    /// Content shown as the header of the item generated for this node. When <see cref="HeaderTemplate" /> is set,
    /// it builds the header from this content; otherwise the item shows a <c>string</c> as a text block.
    /// <see langword="null" /> shows no header.
    /// </summary>
    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    private IDataTemplate? _headerTemplate;

    /// <inheritdoc />
    public IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }

    private object? _tooltip;

    /// <inheritdoc />
    public object? Tooltip
    {
        get => _tooltip;
        set => SetAndRaise(TooltipProperty, ref _tooltip, value);
    }

    private bool _isTooltipEnabled = true;

    /// <inheritdoc />
    public bool IsTooltipEnabled
    {
        get => _isTooltipEnabled;
        set => SetAndRaise(IsTooltipEnabledProperty, ref _isTooltipEnabled, value);
    }

    private ICommand? _command;

    /// <inheritdoc />
    public ICommand? Command
    {
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
    }

    private object? _commandParameter;

    /// <inheritdoc />
    public object? CommandParameter
    {
        get => _commandParameter;
        set => SetAndRaise(CommandParameterProperty, ref _commandParameter, value);
    }
    
    private EntityKey? _itemKey;

    /// <summary>
    /// Key that identifies this node among the entries of its parent. The menu compares it with each segment of a
    /// <c>TreeNodePath</c>, such as <see cref="NavMenu.DefaultSelectedPath" />, to find the item of each level of
    /// the path; a node without a key matches no segment.
    /// </summary>
    public EntityKey? ItemKey
    {
        get => _itemKey;
        set => SetAndRaise(ItemKeyProperty, ref _itemKey, value);
    }
    
    private PathIcon? _icon;

    /// <summary>
    /// Icon shown in front of the header of the item generated for this node; <see langword="null" /> shows no
    /// icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }
    
    private bool _isEnabled = true;

    /// <summary>
    /// Controls whether the item generated for this node is enabled; <see langword="false" /> disables the item.
    /// The default is <see langword="true" />.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }

    private bool _isDanger;

    /// <summary>
    /// Whether the node is a danger (destructive) action, drawn with the menu's danger item colors.
    /// </summary>
    public bool IsDanger
    {
        get => _isDanger;
        set => SetAndRaise(IsDangerProperty, ref _isDanger, value);
    }
    
    /// <summary>
    /// The node that owns this node: the node whose entries contain it directly or inside a group. It is set by
    /// <see cref="UpdateParentNode" /> and is <see langword="null" /> while the node has no owner.
    /// </summary>
    public ITreeNode<INavMenuNode>? ParentNode { get; private set; }
    
    private readonly NavMenuEntryCollection _entries;
    private NavMenuNodeChildrenView? _children;
    private WeakReference<object>? _structuralOwnerReference;

    /// <summary>
    /// The entries of the sub-menu of this node: child nodes, groups and dividers. It is the XAML content of the
    /// node, so the child elements of the node are added to it. The collection rejects an entry that another
    /// owner already holds, and throws an <c>InvalidOperationException</c> for an entry that would make the entry
    /// tree cyclic. A node that is added, directly or inside a group, gets this node as its
    /// <see cref="ParentNode" />; a node that is removed loses its parent.
    /// </summary>
    [Content]
    public IList<INavMenuEntry> Entries
    {
        get => _entries;
        init => _entries.AddRange(value);
    }

    IEnumerable<INavMenuEntry> INavMenuNode.Entries => Entries;

    /// <summary>
    /// The child nodes of this node: the nodes of <see cref="Entries" />, including the nodes inside groups, as a
    /// view that follows changes of <see cref="Entries" />. Dividers are not part of it. In an object initializer,
    /// the listed nodes are appended to <see cref="Entries" />.
    /// </summary>
    public IList<INavMenuNode> Children
    {
        get => _children ??= new NavMenuNodeChildrenView(_entries);
        init
        {
            foreach (var child in value)
            {
                _entries.Add(child);
            }
        }
    }

    IEnumerable<INavMenuNode> ITreeNode<INavMenuNode>.Children => Children;
    
    public NavMenuNode()
    {
        _entries = new NavMenuEntryCollection(
            this,
            ValidateEntry,
            AttachEntry,
            DetachEntry);
    }
    
    /// <summary>
    /// Sets <see cref="ParentNode" /> to <paramref name="parentNode" />. The owners of this node call it when they
    /// add the node to their entries and when they remove it; application code does not need to call it.
    /// </summary>
    /// <param name="parentNode">
    /// The node that owns this node from now on, or <see langword="null" /> when the node has no owner any more.
    /// </param>
    public void UpdateParentNode(INavMenuNode? parentNode)
    {
        ParentNode = parentNode;
    }

    internal bool IsStructurallyOwnedBy(object owner)
    {
        return TryGetStructuralOwner(out var currentOwner) &&
               ReferenceEquals(currentOwner, owner);
    }

    internal bool TryGetStructuralOwner(out object? owner)
    {
        if (_structuralOwnerReference is not null &&
            _structuralOwnerReference.TryGetTarget(out var currentOwner))
        {
            owner = currentOwner;
            return true;
        }

        _structuralOwnerReference = null;
        owner = null;
        return false;
    }

    internal void EnsureCanAttachStructuralOwner(object owner)
    {
        if (!TryGetStructuralOwner(out var currentOwner))
        {
            return;
        }

        throw NavMenuEntryOwnership.CreateAlreadyAttachedException(this, owner, currentOwner!);
    }

    internal void AttachStructuralOwner(object owner)
    {
        _structuralOwnerReference = new WeakReference<object>(owner);
    }

    internal void DetachStructuralOwner(object owner)
    {
        if (IsStructurallyOwnedBy(owner))
        {
            _structuralOwnerReference = null;
        }
    }

    private void ValidateEntry(INavMenuEntry entry)
    {
        NavMenuEntryGraph.ValidateInsertion(this, entry);
    }

    private void AttachEntry(INavMenuEntry entry)
    {
        switch (entry)
        {
            case INavMenuNode node:
                node.UpdateParentNode(this);
                break;

            case NavMenuGroup group:
                group.UpdateSemanticParentNode(this);
                break;
        }
    }

    private void DetachEntry(INavMenuEntry entry)
    {
        switch (entry)
        {
            case INavMenuNode node when ReferenceEquals(node.ParentNode, this):
                node.UpdateParentNode(null);
                break;

            case NavMenuGroup group when ReferenceEquals(group.SemanticParentNode, this):
                group.UpdateSemanticParentNode(null);
                break;
        }
    }
}
