using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of tags that toggle between a checked and an unchecked state and report that state to the form item
/// that hosts them. A tag is never in the indeterminate state: <see cref="ToggleButton.IsChecked" /> defaults
/// to <see langword="false" />, and <see langword="null" /> is coerced to <see langword="false" />.
/// </summary>
public abstract class AbstractCheckableTag : ToggleButton,
                                             IMotionAwareControl,
                                             IFormItemAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractCheckableTag, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckableTag>();

    /// <summary>
    /// The icon shown before the content, or <see langword="null" /> to show no icon. The theme sizes it and
    /// paints it with the foreground brush.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Enables the theme's motion: with <see langword="true" /> the appearance animates on state changes, and with
    /// <see langword="false" /> it switches immediately. The theme sets the value from the
    /// <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    static AbstractCheckableTag()
    {
        AffectsMeasure<AbstractCheckableTag>(IconProperty);
        IsCheckedProperty.OverrideMetadata<AbstractCheckableTag>(
            new StyledPropertyMetadata<bool?>(false, coerce: CoerceIsChecked));
        IsCheckedProperty.Changed.AddClassHandler<AbstractCheckableTag>(
            (tag, _) => tag.HandleIsCheckedChanged());
    }

    #region 实现 FormItem 接口

    private EventHandler? _formValueChanged;

    event EventHandler IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value)
    {
        SetCurrentValue(IsCheckedProperty, value is true);
    }

    object IFormItemAware.GetFormValue()
    {
        return IsChecked == true;
    }

    void IFormItemAware.ClearFormValue()
    {
        SetCurrentValue(IsCheckedProperty, false);
    }

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    private static bool? CoerceIsChecked(AvaloniaObject _, bool? value)
    {
        return value ?? false;
    }

    private void HandleIsCheckedChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
