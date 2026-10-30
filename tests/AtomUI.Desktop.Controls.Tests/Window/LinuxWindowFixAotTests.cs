using System.Reflection;
using Avalonia.Controls;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class LinuxWindowFixAotTests
{
    [Fact]
    public void Linux_Window_And_TitleBar_Popup_Fixes_Do_Not_Add_Aot_Unsafe_Code()
    {
        var source = string.Join(
            Environment.NewLine,
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Window.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/WindowChromeManager.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/AbstractLinuxWindowChromeManager.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/X11WindowChromeManager.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Window/Chrome/WaylandWindowChromeManager.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/DesktopAppBuilderExtensions.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Popup/DetachedTitleBarPopupSupport.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Menu/Menu.cs")),
            File.ReadAllText(GetRepoFile("src/AtomUI.Desktop.Controls/Menu/MenuItem.cs")));

        foreach (var unsafePattern in s_aotUnsafePatterns)
        {
            source.ShouldNotContain(unsafePattern);
        }
    }

    [Fact]
    public void Managed_Resize_Grip_Reflection_Boundary_Is_Trimming_Safe()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Desktop.Controls/Window/Utils/WindowDrawnDecorationsReflectionExtensions.cs"));

        source.ShouldContain("DynamicDependency");
        source.ShouldContain("typeof(TopLevel).GetField(");
        source.ShouldContain("\"_topLevelHost\"");
        source.ShouldNotContain("DynamicallyAccessedMemberTypes.NonPublicFields, typeof(TopLevel)");
        source.ShouldContain("DynamicDependency(\"_decorations\"");
        source.ShouldContain("\"Avalonia.Controls.TopLevelHost\"");
        source.ShouldContain("\"Avalonia.Controls.Chrome.ResizeGripLayer\"");
        source.ShouldContain("DynamicallyAccessedMemberTypes.NonPublicProperties");
        source.ShouldContain("\"GripThickness\"");
        source.ShouldNotContain("Assembly.GetTypes");
        source.ShouldNotContain("Activator.CreateInstance");
        source.ShouldNotContain("RequiresUnreferencedCode");
        source.ShouldNotContain("RequiresDynamicCode");
        source.ShouldNotContain("UnconditionalSuppressMessage");
    }

    [Fact]
    public void Avalonia_Managed_Resize_Grip_Internal_Contract_Is_Available()
    {
        var topLevelHostField = typeof(TopLevel).GetField(
            "_topLevelHost",
            BindingFlags.Instance | BindingFlags.NonPublic);
        topLevelHostField.ShouldNotBeNull();

        var decorationsField = topLevelHostField.FieldType.GetField(
            "_decorations",
            BindingFlags.Instance | BindingFlags.NonPublic);
        decorationsField.ShouldNotBeNull();
        decorationsField.FieldType.FullName.ShouldBe("Avalonia.Controls.Chrome.WindowDrawnDecorations");

        var resizeGripsField = topLevelHostField.FieldType.GetField(
            "_resizeGrips",
            BindingFlags.Instance | BindingFlags.NonPublic);
        resizeGripsField.ShouldNotBeNull();
        resizeGripsField.FieldType.GetProperty(
            "GripThickness",
            BindingFlags.Instance | BindingFlags.NonPublic).ShouldNotBeNull();
    }

    [Fact]
    public void Wayland_Input_Region_Reflection_Boundary_Is_Trimming_Safe()
    {
        var source = File.ReadAllText(GetRepoFile(
            "src/AtomUI.Native/Linux/WaylandWindowReflectionExtensions.cs"));

        source.ShouldContain("DynamicDependency");
        source.ShouldContain("\"Avalonia.Wayland.WindowImpl\"");
        source.ShouldContain("\"_surfaceProxy\"");
        source.ShouldContain("\"Avalonia.Wayland.Server.Persistent.WXdgTopLevelProxy\"");
        source.ShouldContain("ProxyTargetFieldInfo");
        source.ShouldContain("\"_target\"");
        source.ShouldContain("\"Avalonia.Wayland.Server.Persistent.WSurface\"");
        source.ShouldContain("\"WlSurface\"");
        source.ShouldContain("\"Globals\"");
        source.ShouldContain("\"Avalonia.Wayland.Server.Transient.WaylandGlobals\"");
        source.ShouldContain("\"WlCompositor\"");
        source.ShouldNotContain("Assembly.GetTypes");
        source.ShouldNotContain("Activator.CreateInstance");
        source.ShouldNotContain("RequiresUnreferencedCode");
        source.ShouldNotContain("RequiresDynamicCode");
        source.ShouldNotContain("UnconditionalSuppressMessage");
    }

    [Fact]
    public void Avalonia_Wayland_Input_Region_Internal_Contract_Is_Available()
    {
        var waylandAssembly = Assembly.Load("Avalonia.Wayland");
        var windowImplType = waylandAssembly.GetType("Avalonia.Wayland.WindowImpl");
        var proxyType = waylandAssembly.GetType(
            "Avalonia.Wayland.Server.Persistent.WXdgTopLevelProxy");
        var surfaceType = waylandAssembly.GetType(
            "Avalonia.Wayland.Server.Persistent.WSurface");
        var globalsType = waylandAssembly.GetType(
            "Avalonia.Wayland.Server.Transient.WaylandGlobals");

        windowImplType.ShouldNotBeNull();
        windowImplType.GetField("_surfaceProxy", BindingFlags.Instance | BindingFlags.NonPublic)
                      .ShouldNotBeNull();
        proxyType.ShouldNotBeNull();
        proxyType.GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic)
                 .ShouldNotBeNull();
        surfaceType.ShouldNotBeNull();
        surfaceType.GetProperty("WlSurface", BindingFlags.Instance | BindingFlags.Public)
                   .ShouldNotBeNull();
        surfaceType.GetProperty("Globals", BindingFlags.Instance | BindingFlags.Public)
                   .ShouldNotBeNull();
        globalsType.ShouldNotBeNull();
        globalsType.GetProperty("WlCompositor", BindingFlags.Instance | BindingFlags.Public)
                   .ShouldNotBeNull();
    }

    private static readonly string[] s_aotUnsafePatterns =
    [
        "using System.Reflection",
        "Assembly.GetTypes",
        "GetCustomAttribute",
        "Activator.CreateInstance",
        "Expression.Compile",
        "MakeGenericType",
        "new Binding(",
        "ReflectionBinding",
        "WhenAnyValue",
        "ToProperty",
        "OneWayBind",
        "BindCommand",
        "ReactiveUserControl",
        "RequiresUnreferencedCode",
        "RequiresDynamicCode",
        "UnconditionalSuppressMessage"
    ];

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }
}
