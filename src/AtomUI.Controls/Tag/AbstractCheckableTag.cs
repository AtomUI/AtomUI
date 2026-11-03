using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Controls.Commons;

public abstract class AbstractCheckableTag : ToggleButton,
                                             IMotionAwareControl,
                                             IFormItemAware, IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractCheckableTag, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCheckableTag>();

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    private EventHandler? _formValueChanged;

    static AbstractCheckableTag()
    {
        AffectsMeasure<AbstractCheckableTag>(IconProperty);
        IsCheckedProperty.OverrideMetadata<AbstractCheckableTag>(
            new StyledPropertyMetadata<bool?>(false, coerce: CoerceIsChecked));
        IsCheckedProperty.Changed.AddClassHandler<AbstractCheckableTag>(
            (tag, _) => tag.HandleIsCheckedChanged());
    }

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    #region 实现 FormItem 接口

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
