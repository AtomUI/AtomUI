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
    
    protected readonly Dictionary<object, CompositeDisposable> _itemsBindingDisposables = new();
    
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
                    if (item != null)
                    {
                        if (_itemsBindingDisposables.TryGetValue(item, out var disposable))
                        {
                            disposable.Dispose();
                            _itemsBindingDisposables.Remove(item);
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
        if (ListType == UploadListType.PictureCard || ListType == UploadListType.PictureCircle)
        {
            return new UploadPictureShapeListItem()
            {
                IsCircle = ListType == UploadListType.PictureCircle
            };
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
            NotifyPrepareUploadListItem(listItem, disposables);
            if (_itemsBindingDisposables.TryGetValue(listItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(listItem);
            }
            _itemsBindingDisposables.Add(listItem, disposables);
        }
    }

    protected virtual void NotifyPrepareUploadListItem(AbstractUploadListItem listItem, CompositeDisposable disposables)
    {
    }
}