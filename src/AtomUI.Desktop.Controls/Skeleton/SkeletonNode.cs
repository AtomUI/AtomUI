using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// An image placeholder that shows its <see cref="Content" /> in the center of the block instead of the image
/// icon.
/// </summary>
public partial class SkeletonNode : SkeletonImage
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<SkeletonNode>();

    /// <summary>
    /// Identifies the <see cref="ContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<SkeletonNode>();

    /// <summary>
    /// Content shown in the center of the placeholder instead of the image icon; XAML child elements are assigned to
    /// it. A control is shown as it is, anything else is built through <see cref="ContentTemplate" />. The default,
    /// <see langword="null" />, leaves the center empty.
    /// </summary>
    [Content]
    [DependsOn(nameof(ContentTemplate))]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Template that builds <see cref="Content" /> when it is not a control. <see langword="null" />, the default,
    /// leaves the content to the default rendering of the content presenter.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
    #endregion
}
