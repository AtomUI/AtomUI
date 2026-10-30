using System.Runtime.ExceptionServices;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

[GenerateScopedResourceHost]
public partial class NavMenuGroup : AvaloniaObject, INavMenuEntry
{
    public static readonly DirectProperty<NavMenuGroup, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<NavMenuGroup, object?>(
            nameof(Header),
            group => group.Header,
            (group, value) => group.Header = value);

    public static readonly DirectProperty<NavMenuGroup, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<NavMenuGroup, IDataTemplate?>(
            nameof(HeaderTemplate),
            group => group.HeaderTemplate,
            (group, value) => group.HeaderTemplate = value);

    private object? _header;
    private IDataTemplate? _headerTemplate;
    private readonly NavMenuEntryCollection _entries;
    private WeakReference<object>? _structuralOwnerReference;

    internal INavMenuNode? SemanticParentNode { get; private set; }

    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }

    [Content]
    public IList<INavMenuEntry> Entries
    {
        get => _entries;
        init => _entries.AddRange(value);
    }

    public NavMenuGroup()
    {
        _entries = new NavMenuEntryCollection(
            this,
            ValidateEntry,
            AttachEntry,
            DetachEntry);
    }

    internal void UpdateSemanticParentNode(INavMenuNode? parentNode)
    {
        SemanticParentNode = parentNode;
        List<Exception>? callbackExceptions = null;
        foreach (var entry in _entries)
        {
            try
            {
                AttachEntry(entry);
            }
            catch (Exception exception)
            {
                // The group has already changed parent. Complete every descendant projection,
                // including nested groups, before propagating custom node callback failures.
                (callbackExceptions ??= []).Add(exception);
            }
        }

        if (callbackExceptions is { Count: 1 })
        {
            ExceptionDispatchInfo.Capture(callbackExceptions[0]).Throw();
        }
        else if (callbackExceptions is not null)
        {
            throw new AggregateException(callbackExceptions);
        }
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
                node.UpdateParentNode(SemanticParentNode);
                break;

            case NavMenuGroup group:
                group.UpdateSemanticParentNode(SemanticParentNode);
                break;
        }
    }

    private void DetachEntry(INavMenuEntry entry)
    {
        switch (entry)
        {
            case INavMenuNode node when ReferenceEquals(node.ParentNode, SemanticParentNode):
                node.UpdateParentNode(null);
                break;

            case NavMenuGroup group when ReferenceEquals(group.SemanticParentNode, SemanticParentNode):
                group.UpdateSemanticParentNode(null);
                break;
        }
    }
}
