using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

public class TextBlock : AvaloniaTextBlock, IFormItemAware
{
    static TextBlock()
    {
        FontStyleProperty.OverrideDefaultValue<TextBlock>(FontStyle.Normal);
        TextProperty.Changed.AddClassHandler<TextBlock>((block, args) => block.NotifyFormValueChanged(args.NewValue as string));
    }

    public TextBlock()
    {
        var styles = new Style();
        styles.Add(ClipToBoundsProperty, false);
        Styles.Add(styles);
    }
    
    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as string);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    
    protected virtual void NotifyFormValueChanged(string? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(string? value)
    {
        SetCurrentValue(TextProperty, value);
    }

    protected virtual string? NotifyGetFormValue()
    {
        return Text;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(TextProperty, null);
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }
    #endregion
}