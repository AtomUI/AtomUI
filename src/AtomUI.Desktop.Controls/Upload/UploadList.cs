using System.Collections.Specialized;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class UploadList : ItemsControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        Upload.ListTypeProperty.AddOwner<UploadList>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadList>();
    
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion
    
    private readonly Dictionary<AbstractUploadListItem, CompositeDisposable> _itemsBindingDisposables = new();
    
    public UploadList()
    {
        LogicalChildren.CollectionChanged += HandleCollectionChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ListTypeProperty)
        {
            RefreshContainers();
        }
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Count > 0)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is AbstractUploadListItem listItem)
                    {
                        if (_itemsBindingDisposables.TryGetValue(listItem, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(listItem);
                        }
                    }
                }
            }
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (ListType == UploadListType.Picture)
        {
            return new UploadPictureListItem();
        }
        if (ListType == UploadListType.PictureCard)
        {
            return new UploadPictureCardListItem();
        }
        if (ListType == UploadListType.PictureCircle)
        {
            return new UploadPictureCircleListItem();
        }
        return new UploadTextListItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<AbstractUploadListItem>(item, out recycleKey);
    }
    
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is AbstractUploadListItem listItem)
        {
            var disposables = new CompositeDisposable(8);
            if (item != null && item is UploadTaskInfo uploadTaskInfo)
            {
                disposables.Add(BindUtils.RelayBind(uploadTaskInfo, UploadTaskInfo.TaskIdProperty, listItem, AbstractUploadListItem.TaskIdProperty));
                disposables.Add(BindUtils.RelayBind(uploadTaskInfo, UploadTaskInfo.FileNameProperty, listItem, AbstractUploadListItem.FileNameProperty));
                disposables.Add(BindUtils.RelayBind(uploadTaskInfo, UploadTaskInfo.ProgressProperty, listItem, AbstractUploadListItem.ProgressProperty));
                disposables.Add(BindUtils.RelayBind(uploadTaskInfo, UploadTaskInfo.IsImageFileProperty, listItem, AbstractUploadListItem.IsImageFileProperty));
                disposables.Add(BindUtils.RelayBind(uploadTaskInfo, UploadTaskInfo.StatusProperty, listItem, AbstractUploadListItem.StatusProperty));
                disposables.Add(BindUtils.RelayBind(uploadTaskInfo, UploadTaskInfo.ErrorMessageProperty, listItem, AbstractUploadListItem.ErrorMessageProperty));
                disposables.Add(BindUtils.RelayBind(uploadTaskInfo, UploadTaskInfo.FilePathProperty, listItem, AbstractUploadListItem.FilePathProperty));
            }
            
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, listItem, AbstractUploadListItem.IsMotionEnabledProperty));
            
            if (_itemsBindingDisposables.TryGetValue(listItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(listItem);
            }
            _itemsBindingDisposables.Add(listItem, disposables);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(container), "The container type is incorrect, it must be type AbstractUploadListItem.");
        }
    }
}