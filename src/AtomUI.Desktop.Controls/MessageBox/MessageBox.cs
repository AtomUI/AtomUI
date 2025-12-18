using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum MessageBoxOkButtonStyle
{
    Default,
    Primary
}

public class MessageBox : TemplatedControl, 
                          IMotionAwareControl, 
                          IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<MessageBox, string?>(nameof (Title));
    
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<MessageBox, PathIcon?>(nameof (Icon));
    
    public static readonly StyledProperty<object?> ContentProperty = Dialog.ContentProperty.AddOwner<MessageBox>();

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = Dialog.ContentTemplateProperty.AddOwner<MessageBox>();
    
    public static readonly StyledProperty<MessageBoxStyle> StyleProperty =
        AvaloniaProperty.Register<MessageBox, MessageBoxStyle>(nameof (Style));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<MessageBox>();
    
    public static readonly StyledProperty<DialogHostType> HostTypeProperty =
        AvaloniaProperty.Register<Dialog, DialogHostType>(nameof(HostType), DialogHostType.Overlay);
    
    public static readonly StyledProperty<MessageBoxOkButtonStyle> OkButtonStyleProperty =
        AvaloniaProperty.Register<Dialog, MessageBoxOkButtonStyle>(nameof(OkButtonStyle), MessageBoxOkButtonStyle.Primary);
    
    public static readonly StyledProperty<string?> OkButtonTextProperty = AvaloniaProperty.Register<MessageBox, string?>(nameof(OkButtonText));
    
    public static readonly StyledProperty<string?> CancelButtonTextProperty = AvaloniaProperty.Register<MessageBox, string?>(nameof(CancelButtonText));
    
    public static readonly StyledProperty<bool> IsLoadingProperty = AvaloniaProperty.Register<MessageBox, bool>(nameof(IsLoading));
    
    public static readonly StyledProperty<bool> IsConfirmLoadingProperty = AvaloniaProperty.Register<MessageBox, bool>(nameof(IsConfirmLoading));
    
    public static readonly StyledProperty<bool> IsLightDismissEnabledProperty =
        Dialog.IsLightDismissEnabledProperty.AddOwner<MessageBox>();
    
    public static readonly StyledProperty<bool> IsOpenProperty =
        Dialog.IsOpenProperty.AddOwner<MessageBox>();
    
    public static readonly StyledProperty<bool> IsModalProperty =
        Dialog.IsModalProperty.AddOwner<MessageBox>();
    
    public static readonly StyledProperty<bool> IsDragMovableProperty =
        Dialog.IsDragMovableProperty.AddOwner<MessageBox>();
    
    public static readonly StyledProperty<bool> IsCenterOnStartupProperty =
        AvaloniaProperty.Register<MessageBox, bool>(nameof(IsCenterOnStartup), true);
    
    public static readonly StyledProperty<Dimension?> HorizontalOffsetProperty =
        AvaloniaProperty.Register<MessageBox, Dimension?>(nameof(HorizontalOffset));
    
    public static readonly StyledProperty<Dimension?> VerticalOffsetProperty =
        AvaloniaProperty.Register<MessageBox, Dimension?>(nameof(VerticalOffset));
    
    public static readonly StyledProperty<Control?> PlacementTargetProperty =
        AvaloniaProperty.Register<MessageBox, Control?>(nameof(PlacementTarget));
    
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
    
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    
    public MessageBoxStyle Style
    {
        get => GetValue(StyleProperty);
        set => SetValue(StyleProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public DialogHostType HostType
    {
        get => GetValue(HostTypeProperty);
        set => SetValue(HostTypeProperty, value);
    }
    
    public MessageBoxOkButtonStyle OkButtonStyle
    {
        get => GetValue(OkButtonStyleProperty);
        set => SetValue(OkButtonStyleProperty, value);
    }
    
    public string? OkButtonText
    {
        get => GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }
    
    public string? CancelButtonText
    {
        get => GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    
    public bool IsConfirmLoading
    {
        get => GetValue(IsConfirmLoadingProperty);
        set => SetValue(IsConfirmLoadingProperty, value);
    }
    
    public bool IsLightDismissEnabled
    {
        get => GetValue(IsLightDismissEnabledProperty);
        set => SetValue(IsLightDismissEnabledProperty, value);
    }
    
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
    
    public bool IsModal
    {
        get => GetValue(IsModalProperty);
        set => SetValue(IsModalProperty, value);
    }
    
    public bool IsDragMovable
    {
        get => GetValue(IsDragMovableProperty);
        set => SetValue(IsDragMovableProperty, value);
    }
    
    public bool IsCenterOnStartup
    {
        get => GetValue(IsCenterOnStartupProperty);
        set => SetValue(IsCenterOnStartupProperty, value);
    }
    
    public Dimension? HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }
    
    public Dimension? VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }
    
    [ResolveByName]
    public Control? PlacementTarget
    {
        get => GetValue(PlacementTargetProperty);
        set => SetValue(PlacementTargetProperty, value);
    }
    
    private Action<IReadOnlyList<DialogButton>>? _buttonsConfigure;

    public Action<IReadOnlyList<DialogButton>>? ButtonsConfigure
    {
        get => _buttonsConfigure;
        set
        {
            _buttonsConfigure = value;
            if (_dialog != null)
            {
                _dialog.ButtonsConfigure = _buttonsConfigure;
            }
        }
    }
    
    public AvaloniaList<DialogButton> CustomButtons { get; } = new ();
    
    #endregion
    
    #region 公共事件定义
    public event EventHandler? Opened;
    public event EventHandler? Closed;
    public event EventHandler? Cancelled;
    public event EventHandler? Confirmed;
    #endregion
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => DialogToken.ID;
    
    #endregion
    
    private Dialog? _dialog;

    public MessageBox()
    {
        CustomButtons.CollectionChanged += new NotifyCollectionChangedEventHandler(HandleCustomButtonsChanged);
    }

    public object? Open()
    {
        if (_dialog == null)
        {
            return null;
        }
        return _dialog.Open();
    }

    public Task<object?>? OpenAsync()
    {
        if (_dialog == null)
        {
            return null;
        }
        return _dialog.OpenAsync();
    }

    public void Cancel()
    {
        _dialog?.Reject();
    }

    public void Confirm()
    {
        _dialog?.Accept();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == StyleProperty)
            {
                ConfigureIcon();
            }
            else if (change.Property == OkButtonStyleProperty)
            {
                ConfigureOkButton();
            }
            else if (change.Property == IsCenterOnStartupProperty)
            {
                ConfigurePositionOnStartup();
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _dialog = e.NameScope.Find<Dialog>(MessageBoxThemeConstants.DialogPart);
        if (_dialog != null)
        {
            _dialog.Opened           += HandleDialogOpened;
            _dialog.Closed           += HandleDialogClosed;
            _dialog.Rejected         += HandleDialogCancelled;
            _dialog.Accepted         += HandleDialogConfirmed;
            _dialog.ButtonsConfigure =  ButtonsConfigure;
            _dialog.CustomButtons.AddRange(CustomButtons);
        }

        ConfigureIcon();
        ConfigureOkButton();
        ConfigurePositionOnStartup();
    }
    
    private void HandleDialogOpened(object? sender, EventArgs e)
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    private void HandleDialogClosed(object? sender, EventArgs e)
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void HandleDialogCancelled(object? sender, EventArgs e)
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void HandleDialogConfirmed(object? sender, EventArgs e)
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    private void ConfigureIcon()
    {
        Debug.Assert(_dialog != null);
        if (Style == MessageBoxStyle.Information)
        {
            SetValue(IconProperty, new InfoCircleFilled(), BindingPriority.Template);
            _dialog.StandardButtons = DialogStandardButton.Ok;
        }
        else if (Style == MessageBoxStyle.Success)
        {
            SetValue(IconProperty, new CheckCircleFilled(), BindingPriority.Template);
            _dialog.StandardButtons = DialogStandardButton.Ok;
        }
        else if (Style == MessageBoxStyle.Error)
        {
            SetValue(IconProperty, new CloseCircleFilled(), BindingPriority.Template);
            _dialog.StandardButtons = DialogStandardButton.Ok;
        }
        else if (Style == MessageBoxStyle.Warning)
        {
            SetValue(IconProperty, new ExclamationCircleFilled(), BindingPriority.Template);
            _dialog.StandardButtons = DialogStandardButton.Ok;
        }
        else if (Style == MessageBoxStyle.Normal)
        {
            _dialog.StandardButtons = DialogStandardButton.Ok;
        }
        else if (Style == MessageBoxStyle.Confirm)
        {
            SetValue(IconProperty, new ExclamationCircleFilled(), BindingPriority.Template);
            _dialog.StandardButtons = DialogStandardButton.Ok | DialogStandardButton.Cancel;
        }
    }

    private void ConfigureOkButton()
    {
        Debug.Assert(_dialog != null);
        if (OkButtonStyle == MessageBoxOkButtonStyle.Primary)
        {
            _dialog.DefaultStandardButton = DialogStandardButton.Ok;
        }
        else
        {
            _dialog.DefaultStandardButton = DialogStandardButton.NoButton;
        }
    }

    private void ConfigurePositionOnStartup()
    {
        if (_dialog != null)
        {
            if (IsCenterOnStartup)
            {
                _dialog.HorizontalStartupLocation = DialogHorizontalAnchor.Center;
                _dialog.VerticalStartupLocation   = DialogVerticalAnchor.Center;
            }
            else
            {
                _dialog.HorizontalStartupLocation = DialogHorizontalAnchor.Custom;
                _dialog.VerticalStartupLocation   = DialogVerticalAnchor.Custom;
            }
        }
    }
    
    protected override Size MeasureCore(Size availableSize)
    {
        if (IsVisible)
        {
            ApplyStyling();
            ApplyTemplate();
        }

        return new Size();
    }
    
    private void HandleCustomButtonsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_dialog != null)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var newItems = e.NewItems!.OfType<DialogButton>();
                    _dialog.CustomButtons.AddRange(newItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var oldItems = e.OldItems!.OfType<DialogButton>();
                    _dialog.CustomButtons.RemoveAll(oldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
        }
    }
}