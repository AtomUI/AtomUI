using AtomUI.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The default drop area shown inside an upload trigger: a dashed frame with an icon, a heading and a
/// sub-heading, which tells the user to click or drag files into the area.
/// </summary>
// Token-owner transparent part: the default drag area has no Own Tokens; its theme reads the Upload TokenResource.
[TokenOwnerTransparent]
public class UploadDefaultDropArea : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="DropIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> DropIconProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, PathIcon?>(nameof(DropIcon));
    
    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, object?>(nameof(Header));
    
    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, IDataTemplate?>(nameof(HeaderTemplate));
    
    /// <summary>
    /// Identifies the <see cref="SubHeader" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> SubHeaderProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, object?>(nameof(SubHeader));
    
    /// <summary>
    /// Identifies the <see cref="SubHeaderTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> SubHeaderTemplateProperty =
        AvaloniaProperty.Register<UploadDefaultDropArea, IDataTemplate?>(nameof(SubHeaderTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<UploadDefaultDropArea>();
    
    /// <summary>
    /// Icon shown above the heading. When it is <see langword="null" /> as the template is applied,
    /// <c>InboxOutlined</c> is set as the current value, so a value assigned later replaces it.
    /// </summary>
    public PathIcon? DropIcon
    {
        get => GetValue(DropIconProperty);
        set => SetValue(DropIconProperty, value);
    }
    
    /// <summary>
    /// Heading shown below the icon. The theme sets the localized prompt <c>DragUploadHead</c>, which reads
    /// "Click or drag file to this area to upload" in English.
    /// </summary>
    [DependsOn(nameof(HeaderTemplate))]
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    /// <summary>
    /// Template used to present <see cref="Header" />. <see langword="null" /> uses the default presentation,
    /// which shows a string as a text block.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }
    
    /// <summary>
    /// Secondary line shown below the heading, in the smaller muted description style. The theme sets no default,
    /// so the area shows nothing until a value is set.
    /// </summary>
    [DependsOn(nameof(SubHeaderTemplate))]
    public object? SubHeader
    {
        get => GetValue(SubHeaderProperty);
        set => SetValue(SubHeaderProperty, value);
    }
    
    /// <summary>
    /// Template used to present <see cref="SubHeader" />. <see langword="null" /> uses the default presentation,
    /// which shows a string as a text block.
    /// </summary>
    public IDataTemplate? SubHeaderTemplate
    {
        get => GetValue(SubHeaderTemplateProperty);
        set => SetValue(SubHeaderTemplateProperty, value);
    }
    
    /// <summary>
    /// Enables the theme transitions of the drop area when <see langword="true" />, such as the animated border
    /// color on pointer over. The theme's default is the global <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    /// <summary>
    /// Applies <c>InboxOutlined</c> as the drop icon when <see cref="DropIcon" /> is <see langword="null" />.
    /// An override must call the base member to keep this default.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (DropIcon == null)
        {
            SetCurrentValue(DropIconProperty, new InboxOutlined());
        }
    }

}
