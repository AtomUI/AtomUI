using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

internal class UploadList : ItemsControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        Upload.ListTypeProperty.AddOwner<UploadList>();
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadList>();

    public static readonly StyledProperty<double> ListMaxHeightProperty =
        Upload.ListMaxHeightProperty.AddOwner<UploadList>();

    public static readonly StyledProperty<ScrollBarVisibility> ListScrollBarVisibilityProperty =
        Upload.ListScrollBarVisibilityProperty.AddOwner<UploadList>();
    
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

    public double ListMaxHeight
    {
        get => GetValue(ListMaxHeightProperty);
        set => SetValue(ListMaxHeightProperty, value);
    }

    public ScrollBarVisibility ListScrollBarVisibility
    {
        get => GetValue(ListScrollBarVisibilityProperty);
        set => SetValue(ListScrollBarVisibilityProperty, value);
    }
    
    #endregion
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ListTypeProperty)
        {
            RefreshContainers();
        }
    }
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        AbstractUploadListItem listItem;
        if (ListType == UploadListType.Picture)
        {
            listItem = new UploadPictureListItem();
        }
        else if (ListType == UploadListType.PictureCard || ListType == UploadListType.PictureCircle)
        {
            listItem = new UploadPictureShapeListItem();
        }
        else
        {
            listItem = new UploadTextListItem();
        }

        listItem.Classes.Add(UploadSemanticParts.ItemClass);
        return listItem;
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
            if (item is UploadFileItem uploadFileItem)
            {
                listItem[!AbstractUploadListItem.TaskIdProperty]   = uploadFileItem[!UploadFileItem.IdProperty];
                listItem[!AbstractUploadListItem.FileNameProperty] = uploadFileItem[!UploadFileItem.NameProperty];
                listItem[!AbstractUploadListItem.ProgressProperty] = uploadFileItem[!UploadFileItem.ProgressProperty];
                listItem[!AbstractUploadListItem.IsImageFileProperty] =
                    uploadFileItem[!UploadFileItem.IsImageFileProperty];
                listItem[!AbstractUploadListItem.StatusProperty]       = uploadFileItem[!UploadFileItem.StatusProperty];
                listItem[!AbstractUploadListItem.ErrorMessageProperty] = uploadFileItem[!UploadFileItem.ErrorMessageProperty];
                listItem[!AbstractUploadListItem.FilePathProperty]     = uploadFileItem[!UploadFileItem.PathProperty];
            }
            listItem[!IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
            listItem[!ListTypeProperty]        = this[!ListTypeProperty];
            NotifyPrepareUploadListItem(listItem);
        }
    }

    protected override void ClearContainerForItemOverride(Control container)
    {
        if (container is AbstractUploadListItem listItem)
        {
            listItem.ClearValue(AbstractUploadListItem.TaskIdProperty);
            listItem.ClearValue(AbstractUploadListItem.FileNameProperty);
            listItem.ClearValue(AbstractUploadListItem.ProgressProperty);
            listItem.ClearValue(AbstractUploadListItem.IsImageFileProperty);
            listItem.ClearValue(AbstractUploadListItem.StatusProperty);
            listItem.ClearValue(AbstractUploadListItem.ErrorMessageProperty);
            listItem.ClearValue(AbstractUploadListItem.FilePathProperty);
            listItem.ClearValue(IsMotionEnabledProperty);
            listItem.ClearValue(ListTypeProperty);
        }

        base.ClearContainerForItemOverride(container);
    }

    protected virtual void NotifyPrepareUploadListItem(AbstractUploadListItem listItem)
    {
    }
}
