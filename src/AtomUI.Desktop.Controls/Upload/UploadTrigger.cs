using AtomUI.Theme.Resources;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class UploadTrigger : ContentControl, IMotionAwareControl,
    IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<UploadSourceKind> SourceKindProperty =
        AvaloniaProperty.Register<UploadTrigger, UploadSourceKind>(nameof(SourceKind));

    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        Upload.ListTypeProperty.AddOwner<UploadTrigger>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadTrigger>();

    public UploadSourceKind SourceKind
    {
        get => GetValue(SourceKindProperty);
        set => SetValue(SourceKindProperty, value);
    }

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

    private CompositeDisposable? _ownerBindings;

    public UploadTrigger()
    {
        AddHandler(PointerReleasedEvent, HandlePointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureOwnerBindings();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ClearOwnerBindings();
        base.OnDetachedFromVisualTree(e);
    }

    #region 实现 IThemeInheritanceSource 接口

    // AXAML cannot read AvaloniaObject's protected, potentially non-logical inheritance parent.
    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var owner = this.FindAncestorOfType<Upload>();
        if (owner is null)
        {
            return;
        }

        e.Handled = true;
        var task = SourceKind == UploadSourceKind.Directories
            ? owner.SelectDirectoriesAsync()
            : owner.SelectFilesAsync();
        if (!task.IsCompletedSuccessfully)
        {
            _ = ObserveSelectionOperationAsync(task);
        }
    }

    private static async Task ObserveSelectionOperationAsync(Task operation)
    {
        try
        {
            await operation.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Upload picker processing failed: {ex.Message}");
        }
    }

    private void ConfigureOwnerBindings()
    {
        ClearOwnerBindings();
        var owner = this.FindAncestorOfType<Upload>();
        if (owner is null)
        {
            return;
        }

        // TriggerContent is a logical content child, outside the owner ControlTheme template route.
        // Relay these behavior inputs at template priority and release them on detach.
        _ownerBindings = new CompositeDisposable
        {
            BindUtils.RelayBind(owner, Upload.ListTypeProperty, this, ListTypeProperty, priority: BindingPriority.Template),
            BindUtils.RelayBind(
                owner,
                Upload.IsMotionEnabledProperty,
                this,
                IsMotionEnabledProperty,
                priority: BindingPriority.Template)
        };
    }

    private void ClearOwnerBindings()
    {
        _ownerBindings?.Dispose();
        _ownerBindings = null;
    }

}
