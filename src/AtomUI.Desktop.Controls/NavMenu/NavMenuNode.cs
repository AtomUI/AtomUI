using System.Windows.Input;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public interface INavMenuNode : ITreeNode<INavMenuNode>, INavMenuEntry
{
    IDataTemplate? HeaderTemplate { get; }
    object? Tooltip => null;
    bool IsTooltipEnabled => true;
    bool IsDanger => false;
    ICommand? Command => null;
    object? CommandParameter => null;
    IEnumerable<INavMenuEntry> Entries => Children;
    void UpdateParentNode(INavMenuNode? parentNode) => throw new NotImplementedException();
}

[GenerateScopedResourceHost]
public partial class NavMenuNode : AvaloniaObject, INavMenuNode
{
    public static readonly DirectProperty<NavMenuNode, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    public static readonly DirectProperty<NavMenuNode, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, IDataTemplate?>(
            nameof(HeaderTemplate),
            o => o.HeaderTemplate,
            (o, v) => o.HeaderTemplate = v);

    public static readonly DirectProperty<NavMenuNode, object?> TooltipProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, object?>(
            nameof(Tooltip),
            o => o.Tooltip,
            (o, v) => o.Tooltip = v);

    public static readonly DirectProperty<NavMenuNode, bool> IsTooltipEnabledProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, bool>(
            nameof(IsTooltipEnabled),
            o => o.IsTooltipEnabled,
            (o, v) => o.IsTooltipEnabled = v,
            unsetValue: true);

    public static readonly DirectProperty<NavMenuNode, ICommand?> CommandProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, ICommand?>(
            nameof(Command),
            o => o.Command,
            (o, v) => o.Command = v);

    public static readonly DirectProperty<NavMenuNode, object?> CommandParameterProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, object?>(
            nameof(CommandParameter),
            o => o.CommandParameter,
            (o, v) => o.CommandParameter = v);
    
    public static readonly DirectProperty<NavMenuNode, EntityKey?> ItemKeyProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, EntityKey?>(
            nameof(ItemKey),
            o => o.ItemKey,
            (o, v) => o.ItemKey = v);
    
    public static readonly DirectProperty<NavMenuNode, PathIcon?> IconProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, PathIcon?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v);
    
    public static readonly DirectProperty<NavMenuNode, bool> IsEnabledProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, bool>(
            nameof(IsEnabled),
            o => o.IsEnabled,
            (o, v) => o.IsEnabled = v);

    public static readonly DirectProperty<NavMenuNode, bool> IsDangerProperty =
        AvaloniaProperty.RegisterDirect<NavMenuNode, bool>(
            nameof(IsDanger),
            o => o.IsDanger,
            (o, v) => o.IsDanger = v);
        
    private object? _header;

    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    private IDataTemplate? _headerTemplate;

    public IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }

    private object? _tooltip;

    public object? Tooltip
    {
        get => _tooltip;
        set => SetAndRaise(TooltipProperty, ref _tooltip, value);
    }

    private bool _isTooltipEnabled = true;

    public bool IsTooltipEnabled
    {
        get => _isTooltipEnabled;
        set => SetAndRaise(IsTooltipEnabledProperty, ref _isTooltipEnabled, value);
    }

    private ICommand? _command;

    public ICommand? Command
    {
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
    }

    private object? _commandParameter;

    public object? CommandParameter
    {
        get => _commandParameter;
        set => SetAndRaise(CommandParameterProperty, ref _commandParameter, value);
    }
    
    private EntityKey? _itemKey;

    public EntityKey? ItemKey
    {
        get => _itemKey;
        set => SetAndRaise(ItemKeyProperty, ref _itemKey, value);
    }
    
    private PathIcon? _icon;

    public PathIcon? Icon
    {
        get => _icon;
        set => SetAndRaise(IconProperty, ref _icon, value);
    }
    
    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetAndRaise(IsEnabledProperty, ref _isEnabled, value);
    }

    private bool _isDanger;

    /// <summary>
    /// Whether the node is a danger (destructive) action, drawn with the menu's danger item colours.
    /// </summary>
    public bool IsDanger
    {
        get => _isDanger;
        set => SetAndRaise(IsDangerProperty, ref _isDanger, value);
    }
    
    public ITreeNode<INavMenuNode>? ParentNode { get; private set; }
    
    private readonly NavMenuEntryCollection _entries;
    private NavMenuNodeChildrenView? _children;
    private WeakReference<object>? _structuralOwnerReference;

    [Content]
    public IList<INavMenuEntry> Entries
    {
        get => _entries;
        init => _entries.AddRange(value);
    }

    IEnumerable<INavMenuEntry> INavMenuNode.Entries => Entries;

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
