using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Provides the current tab overflow snapshot and session-validated actions to a custom popup template.
/// </summary>
public sealed class TabOverflowPopupContext : INotifyPropertyChanged
{
    private IReadOnlyList<TabOverflowItem> _items = Array.Empty<TabOverflowItem>();
    private TabOverflowItem? _selectedItem;
    private WeakReference<ITabOverflowPopupActionTarget>? _actionTarget;
    private long _sessionId;

    internal TabOverflowPopupContext()
    {
    }

    /// <summary>
    /// Gets the immutable snapshot of tabs that are not fully visible in the current viewport.
    /// </summary>
    public IReadOnlyList<TabOverflowItem> Items => _items;

    /// <summary>
    /// Gets the selected overflow item, or <see langword="null"/> when the selected tab is fully visible.
    /// </summary>
    public TabOverflowItem? SelectedItem => _selectedItem;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Attempts to activate a tab from the current overflow session.
    /// </summary>
    public bool TryActivate(TabOverflowItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return TryGetActionTarget(out var target) && target.TryActivate(_sessionId, item);
    }

    /// <summary>
    /// Attempts to close a tab through its owning control's closing contract.
    /// </summary>
    public bool TryClose(TabOverflowItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return TryGetActionTarget(out var target) && target.TryClose(_sessionId, item);
    }

    /// <summary>
    /// Requests ordinary dismissal of the current overflow popup.
    /// </summary>
    public void Dismiss()
    {
        if (TryGetActionTarget(out var target))
        {
            target.Dismiss(_sessionId);
        }
    }

    internal void OpenSession(
        long sessionId,
        IReadOnlyList<TabOverflowItem> items,
        TabOverflowItem? selectedItem,
        ITabOverflowPopupActionTarget actionTarget)
    {
        _sessionId = sessionId;
        _actionTarget = new WeakReference<ITabOverflowPopupActionTarget>(actionTarget);
        Publish(items, selectedItem);
    }

    internal void Publish(IReadOnlyList<TabOverflowItem> items, TabOverflowItem? selectedItem)
    {
        ArgumentNullException.ThrowIfNull(items);
        var itemsChanged = !ReferenceEquals(_items, items);
        var selectedItemChanged = !ReferenceEquals(_selectedItem, selectedItem);
        _items = items;
        _selectedItem = selectedItem;

        if (itemsChanged)
        {
            OnPropertyChanged(nameof(Items));
        }
        if (selectedItemChanged)
        {
            OnPropertyChanged(nameof(SelectedItem));
        }
    }

    internal void CloseSession()
    {
        _sessionId = 0;
        _actionTarget = null;
        Publish(Array.Empty<TabOverflowItem>(), null);
    }

    private bool TryGetActionTarget(out ITabOverflowPopupActionTarget target)
    {
        target = null!;
        if (_sessionId == 0 ||
            _actionTarget is null ||
            !_actionTarget.TryGetTarget(out var currentTarget) ||
            currentTarget is null)
        {
            return false;
        }

        target = currentTarget;
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal interface ITabOverflowPopupActionTarget
{
    bool TryActivate(long sessionId, TabOverflowItem item);
    bool TryClose(long sessionId, TabOverflowItem item);
    void Dismiss(long sessionId);
}
