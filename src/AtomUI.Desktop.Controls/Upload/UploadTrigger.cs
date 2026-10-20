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

public class UploadTrigger : ContentControl, IMotionAwareControl
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ListTypeProperty)
        {
            var radius = Math.Max(Width, Height);
            if (double.IsNaN(radius))
            {
                radius = Math.Min(DesiredSize.Width, DesiredSize.Height);
            }
            ConfigureEffectiveCornerRadius(radius);
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        ConfigureEffectiveCornerRadius(Math.Max(e.NewSize.Width, e.NewSize.Height));
    }

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

    private void ConfigureEffectiveCornerRadius(double cornerRadius)
    {
        if (ListType == UploadListType.PictureCircle)
        {
            SetCurrentValue(CornerRadiusProperty, new CornerRadius(cornerRadius));
        }
    }
}
