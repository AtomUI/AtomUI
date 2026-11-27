using AtomUI.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Contract of an add-on decorated input box: the add-on blocks attached to its left and right, the prefix and
/// suffix inside the box, its size, style variant, status and extra content.
/// No type in this library implements the interface.
/// </summary>
public interface IAddOnDecoratedBox
{
    /// <summary>
    /// Content of the add-on block attached to the left of the box, outside the content frame.
    /// The block is shown only while this value is not <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn { get; set; }
    
    /// <summary>
    /// Data template that builds the visual for <see cref="LeftAddOn" />.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate { get; set; }

    /// <summary>
    /// Content of the add-on block attached to the right of the box, outside the content frame.
    /// The block is shown only while this value is not <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn{ get; set; }
    
    /// <summary>
    /// Data template that builds the visual for <see cref="RightAddOn" />.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate { get; set; }
    
    /// <summary>
    /// Content shown inside the box before the main content, as the prefix.
    /// </summary>
    [DependsOn(nameof(ContentLeftAddOnTemplate))]
    public object? ContentLeftAddOn{ get; set; }
    
    /// <summary>
    /// Data template that builds the visual for <see cref="ContentLeftAddOn" />.
    /// </summary>
    public IDataTemplate? ContentLeftAddOnTemplate { get; set; }

    /// <summary>
    /// Content shown inside the box after the main content, as the suffix.
    /// </summary>
    [DependsOn(nameof(ContentRightAddOnTemplate))]
    public object? ContentRightAddOn{ get; set; }
    
    /// <summary>
    /// Data template that builds the visual for <see cref="ContentRightAddOn" />.
    /// </summary>
    public IDataTemplate? ContentRightAddOnTemplate { get; set; }

    /// <summary>
    /// Size step of the box. Large, Middle and Small select the theme values of that step, such as the padding of
    /// the add-on blocks and the content frame and the line height of the content; Custom takes the Middle values.
    /// </summary>
    public CustomizableSizeType SizeType { get; set; }

    /// <summary>
    /// Visual style of the box, as one of the <see cref="InputControlStyleVariant" /> values.
    /// With Outlined and Filled the add-on blocks take the top and bottom border of the box and the border of their
    /// outer edge; with Underlined they are drawn without a border.
    /// </summary>
    public InputControlStyleVariant StyleVariant { get; set; }

    /// <summary>
    /// Validation state of the box, as one of the <see cref="InputControlStatus" /> values.
    /// Default paints no validation state; Warning and Error switch the frame to the warning or error colors.
    /// </summary>
    public InputControlStatus Status { get; set; }
    
    /// <summary>
    /// Extra content of the box, presented with <see cref="ExtraTemplate" />.
    /// </summary>
    [DependsOn(nameof(ExtraTemplate))]
    public object? Extra { get; set; }
    
    /// <summary>
    /// Data template that builds the visual for <see cref="Extra" />.
    /// </summary>
    public IDataTemplate? ExtraTemplate { get; set; }
}
