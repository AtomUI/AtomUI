using System.Diagnostics;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class FormShowCase : ReactiveUserControl<FormViewModel>
{
    private WindowMessageManager? _messageManager;
    public FormShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
        ConfigureBasicForm();
        ConfigureLayoutForm();
    }

    private void ConfigureBasicForm()
    {
        var values = new FormValues();
        values.Add("remember", true);
        BasicForm.InitialValues = values;
    }
    
    private void ConfigureLayoutForm()
    {
        LayoutCaseForm.PropertyChanged += (sender, args) =>
        {
            if (args.Property == Form.FormLayoutProperty)
            {
                if (args.NewValue is FormLayout layout)
                {
                    if (layout == FormLayout.Inline)
                    {
                        LayoutCaseForm.MinWidth            = 0;
                        LayoutCaseForm.HorizontalAlignment = HorizontalAlignment.Stretch;
                    }
                    else
                    {
                        LayoutCaseForm.MinWidth            = 600;
                        LayoutCaseForm.HorizontalAlignment = HorizontalAlignment.Left;
                    }
                }
            }
        };
    }
    
    public void HandleFormLayoutOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is FormViewModel vm)
        {
            if (args.CheckedOption.Tag is FormLayout formLayout)
            {
                vm.FormLayout = formLayout;
            }
        }
    }

    private void HandleFormStyleVariantChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (sender is Segmented segmented)
        {
            if (segmented.SelectedItem is SegmentedItem segmentedItem)
            {
                var styleVariant = segmentedItem.Tag as InputControlStyleVariant?;
                Debug.Assert(styleVariant != null);
                if (DataContext is FormViewModel vm)
                {
                    vm.FormStyleVariant = styleVariant.Value;
                }
            }
        }
    }
    
    private void HandleFormRequiredMarkChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is FormViewModel vm)
        {
            if (args.CheckedOption.Tag is FormRequiredMark requiredMark)
            {
                vm.FormRequiredMark = requiredMark;
            }
        }
    }
    
    private void HandleFormSizeTypeChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is FormViewModel vm)
        {
            if (args.CheckedOption.Tag is SizeType sizeType)
            {
                vm.FormSizeType = sizeType;
            }
        }
    }
    
    private void HandleFillClicked(object? sender, RoutedEventArgs args)
    {
        var formValues = new FormValues();
        formValues.Add("url", "https://taobao.com/");
        NoBlockRuleForm.SetFormValues(formValues);
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        _messageManager = new WindowMessageManager(topLevel)
        {
            MaxItems = 10
        };
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _messageManager = null;
    }

    private void HandleNoBlockFormSubmitted(object? sender, FormSubmittedEventArgs args)
    {
        _messageManager?.Show(new Message(
            type: MessageType.Success,
            content: "Submit success!"
        ));
    }
    
    private void HandleNoBlockFormValidated(object? sender, FormValidatedEventArgs args)
    {
        if (args.Result == FormValidateResult.Error)
        {
            _messageManager?.Show(new Message(
                type: MessageType.Error,
                content: "Submit failed!"
            ));
        }
    }

    private static int Form_GID = 3;
    
    private void HandleAddFormItem(object? sender, RoutedEventArgs args)
    {
        var id       = Form_GID++;
        var formItem = new FormItem();
        formItem.FieldName  = $"Passengers_{id}";
        formItem.LabelText  = $"passengers_{id}";
        formItem.Content    = new LineEdit();
        formItem.Validators = new List<IFormValidator>()
        {
            new FormStringNotEmptyValidator()
            {
                Message = "Please input passenger's name or delete this field!",
            }
        };
        var insertIndex = 0;
        for (var i = 0; i < DynamicForm.Items.Count; ++i)
        {
            var item = DynamicForm.Items[i];
            if (item is FormActionsItem)
            {
                insertIndex = i;
                break;
            }
        }
        
        insertIndex = Math.Max(0, insertIndex);
        DynamicForm.Items.Insert(insertIndex, formItem);
    }
    
    private void HandleAddFormItemAtHead(object? sender, RoutedEventArgs args)
    {
        var id       = Form_GID++;
        var formItem = new FormItem();
        formItem.FieldName = $"Passengers_{id}";
        formItem.LabelText = $"passengers_{id}";
        formItem.Content   = new LineEdit();
        formItem.Validators = new List<IFormValidator>()
        {
            new FormStringNotEmptyValidator()
            {
                Message = "Please input passenger's name or delete this field!",
            }
        };
        
        DynamicForm.Items.Insert(0, formItem);
    }
}

public class NoteFormItem : FormItem
{
    protected override void NotifyFormItemChanged(IFormItem formItem)
    {
        if (formItem.FieldName == "gender")
        {
            if (formItem.Content is Select select && select.Mode == SelectMode.Single)
            {
                if (formItem.GetItemValue() is ISelectOption selectOption)
                {
                    SetItemValue($"Hi, {selectOption.Value}!");
                }
            }
        }
    }
}

public class CustomizeGenderFormItem : FormItem
{
    protected override void NotifyFormItemChanged(IFormItem formItem)
    {
        if (formItem.FieldName == "gender")
        {
            if (formItem.Content is Select select && select.Mode == SelectMode.Single)
            {
                var option = formItem.GetItemValue() as ISelectOption;
                if (option?.Value?.ToString() == "other")
                {
                    IsVisible = true;
                }
                else
                {
                    IsVisible = false;
                }
            }
        }
    }
}

public class PriceValidator : AbstractFormValidator
{
    protected override async Task<bool> NotifyValidateAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var price = value as int?;
        return await Task.FromResult(price.HasValue && price.Value > 0);
    }
}