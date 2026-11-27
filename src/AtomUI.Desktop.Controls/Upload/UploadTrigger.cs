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
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The click area of an <see cref="Upload" />: a pointer release over it, even one a child has handled, makes the
/// Upload above it in the visual tree open its picker for files or folders, as <see cref="SourceKind" /> selects.
/// While attached, <see cref="ListType" /> and <see cref="IsMotionEnabled" /> follow that Upload.
/// </summary>
// Token-owner transparent part: a trigger belongs to one Upload and has no Own Tokens; its theme reads the Upload
// TokenResource, so Controls[Upload] reaches it and its transitions.
[TokenOwnerTransparent]
public class UploadTrigger : ContentControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SourceKind" /> property.
    /// </summary>
    public static readonly StyledProperty<UploadSourceKind> SourceKindProperty =
        AvaloniaProperty.Register<UploadTrigger, UploadSourceKind>(nameof(SourceKind));

    /// <summary>
    /// Identifies the <see cref="ListType" /> property.
    /// </summary>
    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        Upload.ListTypeProperty.AddOwner<UploadTrigger>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadTrigger>();

    /// <summary>
    /// Picker that a click opens on the owning <see cref="Upload" />: <see cref="UploadSourceKind.Files" /> (the
    /// default) adds the picked files; <see cref="UploadSourceKind.Directories" /> adds the files directly inside the
    /// picked folders.
    /// </summary>
    public UploadSourceKind SourceKind
    {
        get => GetValue(SourceKindProperty);
        set => SetValue(SourceKindProperty, value);
    }

    /// <summary>
    /// Form of the area, following the owning <see cref="Upload" /> while attached. Any value other than
    /// <see cref="UploadListType.PictureCard" /> and <see cref="UploadListType.PictureCircle" /> shows the content
    /// without a frame. Those two draw a dashed card of <c>PictureCardSize</c> with centered content, and
    /// <see cref="UploadListType.PictureCircle" /> rounds it with a corner radius that follows the larger of the
    /// trigger's width and height.
    /// </summary>
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }

    /// <summary>
    /// Enables the <c>BorderBrush</c> transition that animates the pointer-over border of the card forms. While
    /// attached, it follows the owning <see cref="Upload" />.
    /// </summary>
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

    /// <summary>
    /// Calls the base member, then binds <see cref="ListType" /> and <see cref="IsMotionEnabled" /> to the ancestor
    /// <see cref="Upload" />, if there is one. An override must call the base member to keep that relay.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureOwnerBindings();
    }

    /// <summary>
    /// Releases the bindings to the ancestor <see cref="Upload" /> before the base member runs. An override must call
    /// the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ClearOwnerBindings();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base member, then, when <see cref="ListType" /> changes, updates the corner radius of the
    /// <see cref="UploadListType.PictureCircle" /> form from the larger of <c>Width</c> and <c>Height</c>, or from the
    /// smaller desired dimension when either is not set.
    /// </summary>
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

    /// <summary>
    /// Calls the base member, then sets the corner radius of the <see cref="UploadListType.PictureCircle" /> form from
    /// the larger side of the new size.
    /// </summary>
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

        // The trigger is supplied as runtime Upload content and can be detached/reparented outside a ControlTemplate.
        // ControlTheme cannot express that owner lifecycle; ClearOwnerBindings releases both relays before rebinding/detach.
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
