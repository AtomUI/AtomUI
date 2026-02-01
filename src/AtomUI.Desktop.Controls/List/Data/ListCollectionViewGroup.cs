using System.ComponentModel;
using Avalonia.Collections;

namespace AtomUI.Desktop.Controls.Data;

public abstract class ListCollectionViewGroup : INotifyPropertyChanged
{
    private int _itemCount;
    public object Key { get; }
    public int ItemCount => _itemCount;
    public IAvaloniaReadOnlyList<object> Items => ProtectedItems;
    protected AvaloniaList<object> ProtectedItems { get; }
    
    protected int ProtectedItemCount
    {
        get => _itemCount;
        set
        {
            _itemCount = value;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemCount)));
        }
    }
    
    internal abstract ListCollectionViewGroupInternal? Parent { get; }
    internal abstract IListGroupDescription? GroupBy { get; set; }

    protected ListCollectionViewGroup(object key)
    {
        Key            = key;
        ProtectedItems = new AvaloniaList<object>();
    }

    public abstract bool IsBottomLevel { get; }

    protected virtual event PropertyChangedEventHandler? PropertyChanged;

    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add =>  PropertyChanged += value;
        remove => PropertyChanged -= value;
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }
}