using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A text input with an add-on on each side of its text area. <see cref="LeftAddOn" /> and
/// <see cref="RightAddOn" /> sit outside the border of the text area, while the add-ons built from
/// <see cref="InnerLeftContentTemplate" /> and <see cref="InnerRightContentTemplate" /> sit inside it.
/// </summary>
public partial class LineEdit : AbstractTextInput
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<LineEdit>();

    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<LineEdit>();

    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<LineEdit>();

    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<LineEdit>();

    /// <summary>
    /// Identifies the <see cref="InnerLeftContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<LineEdit, IDataTemplate?>(nameof(InnerLeftContentTemplate));

    /// <summary>
    /// Identifies the <see cref="InnerRightContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<LineEdit, IDataTemplate?>(nameof(InnerRightContentTemplate));

    /// <summary>
    /// The leading add-on: content shown to the left of the text area, outside its border.
    /// </summary>
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    /// <summary>
    /// The template that builds <see cref="LeftAddOn" />. A <see langword="null" /> template shows the content itself.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// The trailing add-on: content shown to the right of the text area, outside its border.
    /// </summary>
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }

    /// <summary>
    /// The template that builds <see cref="RightAddOn" />. A <see langword="null" /> template shows the content itself.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Template of the add-on shown inside the input frame, before the text.
    /// </summary>
    /// <remarks>
    /// A <see langword="null" /> template shows no add-on.
    /// </remarks>
    public IDataTemplate? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }

    /// <summary>
    /// Template of the add-on shown inside the input frame, after the text. It sits in the trailing accessory panel,
    /// after the clear button and before the character counter.
    /// </summary>
    /// <remarks>
    /// A <see langword="null" /> template shows no add-on.
    /// </remarks>
    public IDataTemplate? InnerRightContentTemplate
    {
        get => GetValue(InnerRightContentTemplateProperty);
        set => SetValue(InnerRightContentTemplateProperty, value);
    }

    #endregion

    #region 内部协作 API

    /// <summary>
    /// The decorated box of the applied template, or <see langword="null" /> until the template provides one.
    /// </summary>
    internal AddOnDecoratedBox? DecoratedBox => _addOnDecoratedBox;

    #endregion

    private AddOnDecoratedBox? _addOnDecoratedBox;

    public LineEdit()
    {
    }

    /// <summary>
    /// Calls the base implementation, then finds the decorated box of the template and stores it for
    /// <see cref="DecoratedBox" /> and <see cref="GetBorderThicknessForCompactSpace" />. An override must call the
    /// base implementation.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _addOnDecoratedBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
    }

    /// <summary>
    /// Overrides the base hook that exposes the styled property of the add-on template after the text: returns
    /// <see cref="InnerRightContentTemplateProperty" /> instead of <see langword="null" />.
    /// </summary>
    protected override AvaloniaProperty? InnerRightContentTemplatePropertyForBinding =>
        InnerRightContentTemplateProperty;

    /// <summary>
    /// Returns the border width reported to a compact space: the left width of the inner border of the template's
    /// decorated box when the control lies in a compact space and its style variant is
    /// <see cref="InputControlStyleVariant.Outlined" />, and 0 otherwise. In that variant the visible border is the
    /// inner border of the decorated box, not the control's own border.
    /// </summary>
    protected override double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace ||
            _addOnDecoratedBox is null ||
            _addOnDecoratedBox.StyleVariant != InputControlStyleVariant.Outlined)
        {
            return 0.0;
        }

        return _addOnDecoratedBox.InnerBoxBorderThickness.Left;
    }
}
