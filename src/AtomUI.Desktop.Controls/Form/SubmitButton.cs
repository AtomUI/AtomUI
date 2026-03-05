using AtomUI.Data;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public class SubmitButton : Button
{
    #region 公共属性定义

    public static readonly StyledProperty<bool> IsWatchValidateResultProperty =
        AvaloniaProperty.Register<SubmitButton, bool>(nameof(IsWatchValidateResult), false);
    
    public bool IsWatchValidateResult
    {
        get => GetValue(IsWatchValidateResultProperty);
        set => SetValue(IsWatchValidateResultProperty, value);
    }
    #endregion
    
    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> SubmitEvent =
        RoutedEvent.Register<SubmitButton, RoutedEventArgs>(nameof(Submit), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Submit
    {
        add => AddHandler(SubmitEvent, value);
        remove => RemoveHandler(SubmitEvent, value);
    }
    #endregion
    
    private Form? _form;
    private IDisposable? _subscription;

    protected override void OnClick()
    {
        base.OnClick();
        RaiseEvent(new RoutedEventArgs(SubmitEvent));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _form = this.FindAncestorOfType<Form>();
        ConfigureWatchValidate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsWatchValidateResultProperty)
        {
            ConfigureWatchValidate();
        }
    }
    
    private void ConfigureWatchValidate()
    {
        _subscription?.Dispose();
        if (IsWatchValidateResult)
        {
            if (_form != null)
            {
                _subscription = Form.IsFormValidProperty.Changed.Subscribe((args) =>
                {
                    IsEnabled = args.NewValue == true;
                });
                IsEnabled = _form.IsFormValid == true;
            }
        }
    }
}