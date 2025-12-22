using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Platform.Storage;

namespace AtomUI.Desktop.Controls;

public class UploadDefaultDropArea : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> DropIconProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, PathIcon?>(nameof(DropIcon));
    
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, object?>(nameof(Header));
    
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, IDataTemplate?>(nameof(HeaderTemplate));
    
    public static readonly StyledProperty<object?> SubHeaderProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, object?>(nameof(SubHeader));
    
    public static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, IDataTemplate?>(nameof(SubHeaderTemplate));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadDefaultDropArea>();
    
    public PathIcon? DropIcon
    {
        get => GetValue(DropIconProperty);
        set => SetValue(DropIconProperty, value);
    }
    
    [DependsOn(nameof(HeaderTemplate))]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    
    [DependsOn(nameof(SubHeaderTemplate))]
    public object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }
    
    public IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<UploadFilesDroppedEventArgs> FilesDroppedEvent =
        RoutedEvent.Register<UploadDefaultDropArea, UploadFilesDroppedEventArgs>(nameof(FilesDropped), RoutingStrategies.Bubble);
    
    public event EventHandler<UploadFilesDroppedEventArgs>? FilesDropped
    {
        add => AddHandler(FilesDroppedEvent, value);
        remove => RemoveHandler(FilesDroppedEvent, value);
    }

    #endregion

    static UploadDefaultDropArea()
    {
        DragDrop.DropEvent.AddClassHandler<UploadDefaultDropArea>((area, args) =>
        {
            area.HandleDrop(args);
        });
   
    }

    private void HandleDrop(DragEventArgs e)
    {
        var files = new List<IStorageFile>();
        foreach (var item in e.DataTransfer.Items)
        {
            var raw = item.TryGetRaw(DataFormat.File);
            if (raw is IStorageFile file)
            {
                files.Add(file);
            }
        }
        RaiseEvent(new UploadFilesDroppedEventArgs(files)
        {
            Source = this,
            RoutedEvent = FilesDroppedEvent,
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (DropIcon == null)
        {
            SetCurrentValue(DropIconProperty, new InboxOutlined());
        }
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BorderBrushProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
    
}