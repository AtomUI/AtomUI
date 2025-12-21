using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

internal class UploadShapeList : UploadList
{
    #region 公共属性定义
    public static readonly StyledProperty<object?> TriggerContentProperty =
        AvaloniaProperty.Register<UploadShapeList, object?>(nameof(TriggerContent));
    
    public static readonly StyledProperty<IDataTemplate?> TriggerContentTemplateProperty =
        AvaloniaProperty.Register<UploadShapeList, IDataTemplate?>(nameof(TriggerContentTemplate));
    
    public static readonly StyledProperty<bool> IsShowUploadListProperty =
        Upload.IsShowUploadListProperty.AddOwner<UploadShapeList>();

    public static readonly StyledProperty<bool> IsShowUploadTriggerProperty =
        Upload.IsShowUploadTriggerProperty.AddOwner<UploadShapeList>();
    
    [DependsOn(nameof(TriggerContentTemplate))]
    public object? TriggerContent
    {
        get => GetValue(TriggerContentProperty);
        set => SetValue(TriggerContentProperty, value);
    }

    public IDataTemplate? TriggerContentTemplate
    {
        get => GetValue(TriggerContentTemplateProperty);
        set => SetValue(TriggerContentTemplateProperty, value);
    }
        
    public bool IsShowUploadList
    {
        get => GetValue(IsShowUploadListProperty);
        set => SetValue(IsShowUploadListProperty, value);
    }
    
    public bool IsShowUploadTrigger
    {
        get => GetValue(IsShowUploadTriggerProperty);
        set => SetValue(IsShowUploadTriggerProperty, value);
    }

    #endregion
    
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        if (ListType == UploadListType.PictureCard || ListType == UploadListType.PictureCircle)
        {
            if (item is UploadTaskInfo uploadTaskInfo && uploadTaskInfo.IsPictureTriggerTask)
            {
                return new UploadTriggerContent();
            }
        }
        return base.CreateContainerForItemOverride(item, index, recycleKey);
    }

    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is UploadTriggerContent listItem)
        {
            var disposables = new CompositeDisposable(6);
            disposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, listItem, UploadTriggerContent.IsMotionEnabledProperty));
            disposables.Add(BindUtils.RelayBind(this, IsShowUploadTriggerProperty, listItem, IsVisibleProperty));
            disposables.Add(BindUtils.RelayBind(this, ListTypeProperty, listItem, UploadTriggerContent.ListTypeProperty));
            disposables.Add(BindUtils.RelayBind(this, TriggerContentProperty, listItem, UploadTriggerContent.ContentProperty));
            disposables.Add(BindUtils.RelayBind(this, TriggerContentTemplateProperty, listItem, UploadTriggerContent.ContentTemplateProperty));
            if (_itemsBindingDisposables.TryGetValue(listItem, out var oldDisposables))
            {
                oldDisposables.Dispose();
                _itemsBindingDisposables.Remove(listItem);
            }
            _itemsBindingDisposables.Add(listItem, disposables);
        }
    }
    
    protected override void NotifyPrepareUploadListItem(AbstractUploadListItem listItem, CompositeDisposable disposables)
    {
        disposables.Add(BindUtils.RelayBind(this, IsShowUploadListProperty, listItem, IsVisibleProperty));
    }
}