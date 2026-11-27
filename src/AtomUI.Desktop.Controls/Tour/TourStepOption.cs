using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data of one step of a <see cref="Tour" />: the control the step points at, the content of the step card, and
/// the options that replace the values of the Tour while this step is current.
/// <see cref="TourStep" /> implements it as the control that shows a step. Any other implementation can be an
/// item of <see cref="Tour.StepsSource" /> instead. The Tour reads <see cref="Target" /> and the option
/// properties from the item itself. It never reads <see cref="CloseIcon" />, <see cref="Cover" />,
/// <see cref="Title" />, <see cref="TitleTemplate" />, <see cref="Description" /> or
/// <see cref="DescriptionTemplate" /> from an item: the item is shown as the content of a
/// <see cref="TourStep" /> container, so these values matter only to the template that presents it.
/// An option that is <see langword="null" /> uses the value of the Tour. When the Tour is initialized, and
/// whenever its steps change, it copies its value into every option that is still <see langword="null" />, and
/// the option then holds that copy instead of following the Tour. <see cref="MaskColor" /> is never copied.
/// </summary>
public interface ITourStepOption
{
    /// <summary>
    /// Control this step points at while it is current: the popup is placed next to it and the mask leaves it
    /// uncovered. <see langword="null" /> shows the step as a centered popup without an arrow, whatever
    /// <see cref="Placement" /> and <see cref="IsArrowVisible" /> are set to.
    /// </summary>
    Control? Target { get; set; }
    /// <summary>
    /// Overrides <see cref="Tour.IsArrowVisible" /> for this step: whether the popup shows an arrow while this step
    /// is current. <see langword="null" /> uses the value of the Tour.
    /// A step without a <see cref="Target" /> never shows an arrow.
    /// </summary>
    bool? IsArrowVisible { get; set; }
    /// <summary>
    /// Per-step value of <see cref="Tour.IsPointAtCenter" />.
    /// <see langword="null" /> uses the value of the Tour. No code evaluates the value of a single step at present.
    /// </summary>
    bool? IsPointAtCenter { get; set; }
    /// <summary>
    /// Icon shown by the close button in the header of the step; <see langword="null" /> shows the button without an
    /// icon.
    /// </summary>
    PathIcon? CloseIcon { get; set; }
    /// <summary>
    /// Content shown at the top of the body of the step, above the description; <see langword="null" /> shows no
    /// cover.
    /// </summary>
    object? Cover { get; set; }
    /// <summary>
    /// Content shown in the header of the step, to the left of the close button; <see langword="null" /> shows no
    /// title.
    /// </summary>
    object? Title { get; set; }
    /// <summary>
    /// Template that builds the title from <see cref="Title" />.
    /// <see langword="null" /> presents the title without an explicit template.
    /// </summary>
    IDataTemplate? TitleTemplate { get; set; }
    /// <summary>
    /// Content shown in the body of the step, below the cover; <see langword="null" /> shows no description.
    /// </summary>
    object? Description { get; set; }
    /// <summary>
    /// Template that builds the description from <see cref="Description" />.
    /// <see langword="null" /> presents the description without an explicit template.
    /// </summary>
    IDataTemplate? DescriptionTemplate { get; set; }
    /// <summary>
    /// Overrides <see cref="Tour.Placement" /> for this step: the position of the popup relative to
    /// <see cref="Target" /> while this step is current. <see langword="null" /> uses the value of the Tour.
    /// A step without a target is centered, whatever this value is.
    /// </summary>
    TourPlacementMode? Placement { get; set; }
    /// <summary>
    /// Overrides <see cref="Tour.StyleType" /> for this step: the visual style of the popup while this step is
    /// current. <see langword="null" /> uses the value of the Tour.
    /// </summary>
    TourStyleType? StyleType { get; set; }
    /// <summary>
    /// Overrides <see cref="Tour.IsShowMask" /> for this step: whether the mask is shown while this step is
    /// current. <see langword="null" /> uses the value of the Tour.
    /// </summary>
    bool? IsShowMask { get; set; }
    /// <summary>
    /// Overrides <see cref="Tour.MaskColor" /> for this step: the color of the mask while this step is current.
    /// <see langword="null" /> uses the value of the Tour, which the theme binds to the
    /// <c>ColorBgMask</c> token. The Tour does not copy its value into this option, so it keeps following the
    /// Tour while it is <see langword="null" />.
    /// </summary>
    IBrush? MaskColor { get; set; }
    /// <summary>
    /// Per-step value of <see cref="Tour.IsScrollIntoView" />.
    /// <see langword="null" /> uses the value of the Tour. No code evaluates the value of a single step at present.
    /// </summary>
    bool? IsScrollIntoView { get; set; }
}

/// <summary>
/// Plain implementation of <see cref="ITourStepOption" /> that only holds the values of a step. Use it for steps
/// that are created in code, as items of <see cref="Tour.StepsSource" />.
/// </summary>
public class TourStepOption : ITourStepOption
{
    /// <inheritdoc />
    public Control? Target { get; set; }
    /// <inheritdoc />
    public bool? IsArrowVisible { get; set; }
    /// <inheritdoc />
    public bool? IsPointAtCenter { get; set; }
    /// <inheritdoc />
    public PathIcon? CloseIcon { get; set; }
    /// <inheritdoc />
    public object? Cover { get; set; }
    /// <inheritdoc />
    public object? Title { get; set; }
    /// <inheritdoc />
    public IDataTemplate? TitleTemplate { get; set; }
    /// <inheritdoc />
    public object? Description { get; set; }
    /// <inheritdoc />
    public IDataTemplate? DescriptionTemplate { get; set; }
    /// <inheritdoc />
    public TourPlacementMode? Placement { get; set; }
    /// <inheritdoc />
    public TourStyleType? StyleType { get; set; }
    /// <inheritdoc />
    public bool? IsShowMask { get; set; }
    /// <inheritdoc />
    public IBrush? MaskColor { get; set; }
    /// <inheritdoc />
    public bool? IsScrollIntoView { get; set; }
}