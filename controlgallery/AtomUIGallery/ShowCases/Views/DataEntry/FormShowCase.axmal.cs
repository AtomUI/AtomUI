using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class FormShowCase : ReactiveUserControl<FormViewModel>
{
    public FormShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
        // ConfigureBasicForm();
        // ConfigureLayoutForm();
    }

    // private void ConfigureBasicForm()
    // {
    //     var values = new FormValues();
    //     values.Add("remember", true);
    //     BasicForm.InitialValues = values;
    // }
    //
    // private void ConfigureLayoutForm()
    // {
    //     LayoutCaseForm.PropertyChanged += (sender, args) =>
    //     {
    //         if (args.Property == Form.FormLayoutProperty)
    //         {
    //             if (args.NewValue is FormLayout layout)
    //             {
    //                 if (layout == FormLayout.Inline)
    //                 {
    //                     LayoutCaseForm.MinWidth            = 0;
    //                     LayoutCaseForm.HorizontalAlignment = HorizontalAlignment.Stretch;
    //                 }
    //                 else
    //                 {
    //                     LayoutCaseForm.MinWidth            = 600;
    //                     LayoutCaseForm.HorizontalAlignment = HorizontalAlignment.Left;
    //                 }
    //             }
    //         }
    //     };
    // }
    
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
