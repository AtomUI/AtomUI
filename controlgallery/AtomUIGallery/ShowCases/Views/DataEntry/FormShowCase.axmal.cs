using AtomUI.Desktop.Controls;
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace AtomUIGallery.ShowCases.Views;

public partial class FormShowCase : ReactiveUserControl<FormViewModel>
{
    public FormShowCase()
    {
        this.WhenActivated(disposables => { });
        InitializeComponent();
        ConfigureBasicForm();
    }

    private void ConfigureBasicForm()
    {
        var values = new FormValues();
        values.Add("remember", true);
        BasicForm.InitialValues = values;
    }
}