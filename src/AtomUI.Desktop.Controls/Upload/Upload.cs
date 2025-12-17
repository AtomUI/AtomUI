using AtomUI.Controls;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public class Upload : TemplatedControl
{
    #region 公共属性定义
    public static readonly StyledProperty<IReadOnlyList<string>?> AcceptsProperty =
        AvaloniaProperty.Register<Upload, IReadOnlyList<string>?>(nameof(Accepts));
    
    public static readonly StyledProperty<string?> ActionUrlProperty =
        AvaloniaProperty.Register<Upload, string?>(nameof(ActionUrl));
    
    public static readonly StyledProperty<Func<string?, CancellationToken, Task<bool>>?> BeforeUploadProperty =
        AvaloniaProperty.Register<Upload, Func<string?, CancellationToken, Task<bool>>?>(
            nameof(BeforeUpload));
    
    public static readonly StyledProperty<object?> ExtraContextProperty =
        AvaloniaProperty.Register<Upload, object?>(
            nameof(ExtraContext));
    
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<Upload, int>(
            nameof(MaxCount), int.MaxValue);
    
    public static readonly StyledProperty<bool> IsUploadDirectoryEnabledProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsUploadDirectoryEnabled));
    
    public static readonly StyledProperty<UploadListType> ListTypeProperty =
        AvaloniaProperty.Register<Upload, UploadListType>(nameof(ListType));
    
    public static readonly StyledProperty<Func<UploadFileInfo, UploadListType, Control>?> IconRenderProperty =
        AvaloniaProperty.Register<Upload, Func<UploadFileInfo, UploadListType, Control>?>(nameof(IconRender));
    
    public static readonly StyledProperty<bool> IsMultipleEnabledProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsMultipleEnabled));
    
    public static readonly StyledProperty<bool> IsOpenFileDialogOnClickProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsOpenFileDialogOnClick), true);
    
    public static readonly StyledProperty<bool> IsPastableProperty =
        AvaloniaProperty.Register<Upload, bool>(nameof(IsPastable));
    
    public IReadOnlyList<string>? Accepts
    {
        get => GetValue(AcceptsProperty);
        set => SetValue(AcceptsProperty, value);
    }
    
    public string? ActionUrl
    {
        get => GetValue(ActionUrlProperty);
        set => SetValue(ActionUrlProperty, value);
    }
    
    public Func<string?, CancellationToken, Task<bool>>? BeforeUpload
    {
        get => GetValue(BeforeUploadProperty);
        set => SetValue(BeforeUploadProperty, value);
    }
    
    public object? ExtraContext
    {
        get => GetValue(ExtraContextProperty);
        set => SetValue(ExtraContextProperty, value);
    }
    
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    
    public bool IsUploadDirectoryEnabled
    {
        get => GetValue(IsUploadDirectoryEnabledProperty);
        set => SetValue(IsUploadDirectoryEnabledProperty, value);
    }
    
    public UploadListType ListType
    {
        get => GetValue(ListTypeProperty);
        set => SetValue(ListTypeProperty, value);
    }
    
    public Func<UploadFileInfo, UploadListType, Control>? IconRender
    {
        get => GetValue(IconRenderProperty);
        set => SetValue(IconRenderProperty, value);
    }
    
    public bool IsMultipleEnabled
    {
        get => GetValue(IsMultipleEnabledProperty);
        set => SetValue(IsMultipleEnabledProperty, value);
    }
    
    public bool IsOpenFileDialogOnClick
    {
        get => GetValue(IsOpenFileDialogOnClickProperty);
        set => SetValue(IsOpenFileDialogOnClickProperty, value);
    }
    
    public bool IsPastable
    {
        get => GetValue(IsPastableProperty);
        set => SetValue(IsPastableProperty, value);
    }
    
    public AvaloniaList<UploadFileInfo> FileList { get; } = new ();
    
    #endregion

    #region 公共回调函数定义

    public Func<UploadFileInfo, bool>? ImageTypePredicate { get; set; }
    public Action<UploadFileInfo>? FilePreviewHandler { get; set; }
    public Action<UploadFileInfo>? FileDropHandler { get; set; }
    public Action<UploadFileInfo, Task<bool>>? FileRemovedHandler { get; set; }
    
    #endregion
}