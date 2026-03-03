using AtomUIGallery.ShowCases;
using AtomUIGallery.Workspace.Views;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace AtomUIGallery;

public partial class BaseGalleryApplication : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected WorkspaceWindow CreateWorkspaceWindow()
    {
        return new WorkspaceWindow();
    }

    public override void RegisterServices()
    {
        base.RegisterServices();
        ShowCaseRegister.Register();
    }
}
