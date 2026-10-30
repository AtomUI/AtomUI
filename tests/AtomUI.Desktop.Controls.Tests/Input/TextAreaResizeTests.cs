using System.Reflection;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Input;

public class TextAreaResizeTests
{
    static TextAreaResizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Resize_Handle_Is_Hidden_When_IsResizable_Is_False()
    {
        var textArea = new AtomUI.Desktop.Controls.TextArea
        {
            Width           = 240,
            Height          = 120,
            IsResizable     = false,
            IsMotionEnabled = false
        };
        var window = CreateWindow(textArea);

        try
        {
            var resizeHandle = FindResizeHandle(textArea);
            resizeHandle.IsVisible.ShouldBeFalse(
                "IsResizable=false should remove the interactive resize handle from the template.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Resize_Handle_Is_Visible_When_IsResizable_Is_True()
    {
        var textArea = new AtomUI.Desktop.Controls.TextArea
        {
            Width           = 240,
            Height          = 120,
            IsResizable     = true,
            IsMotionEnabled = false
        };
        var window = CreateWindow(textArea);

        try
        {
            var resizeHandle = FindResizeHandle(textArea);
            resizeHandle.IsVisible.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Resize_Notifications_Do_Not_Change_Height_When_IsResizable_Is_False()
    {
        var textArea = new AtomUI.Desktop.Controls.TextArea
        {
            Width           = 240,
            Height          = 120,
            IsResizable     = false,
            IsMotionEnabled = false
        };
        var window = CreateWindow(textArea);

        try
        {
            var originalHeight = textArea.Height;

            InvokeInternal(textArea, "NotifyAboutToResize");
            InvokeInternal(textArea, "NotifyResizing", new Point(0, 40));
            InvokeInternal(textArea, "NotifyResizeCompleted");

            textArea.Height.ShouldBe(originalHeight);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Resize_Notifications_Change_Height_When_IsResizable_Is_True()
    {
        var textArea = new AtomUI.Desktop.Controls.TextArea
        {
            Width           = 240,
            Height          = 120,
            IsResizable     = true,
            IsMotionEnabled = false
        };
        var window = CreateWindow(textArea);

        try
        {
            InvokeInternal(textArea, "NotifyAboutToResize");
            InvokeInternal(textArea, "NotifyResizing", new Point(0, 40));
            InvokeInternal(textArea, "NotifyResizeCompleted");

            textArea.Height.ShouldBe(160);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inner_Left_Content_Uses_Token_Margin_Before_Text_Content()
    {
        var textArea = new AtomUI.Desktop.Controls.TextArea
        {
            Width            = 240,
            Height           = 80,
            InnerLeftContent = "L",
            Text             = "content",
            IsMotionEnabled  = false
        };
        var window = CreateWindow(textArea);

        try
        {
            var leftAddOn      = FindTemplatePart<ContentPresenter>(textArea, "PART_ContentLeftAddOn");
            var expectedMargin = GetThemeResource<Thickness>(TextAreaTokenKind.LeftInnerAddOnMargin);

            leftAddOn.Margin.ShouldBe(expectedMargin);
            leftAddOn.Margin.Right.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Content_Frame_Uses_AddOnDecoratedBox_Rendering_Frame()
    {
        var textArea = new AtomUI.Desktop.Controls.TextArea
        {
            Width           = 240,
            Height          = 80,
            Text            = "content",
            IsMotionEnabled = false
        };
        var window = CreateWindow(textArea);

        try
        {
            var contentFrame = FindTemplatePart<AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>(
                textArea,
                "PART_ContentFrame");

            contentFrame.UseLayoutRounding.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 260,
            Content = content
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        return window;
    }

    private static Control FindResizeHandle(Control textArea)
    {
        return textArea.GetVisualDescendants()
                       .OfType<Control>()
                       .Single(item => item.Name == "PART_ResizeHandle");
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : Control
    {
        var part = control.GetVisualDescendants()
                          .OfType<T>()
                          .SingleOrDefault(item => item.Name == name);
        part.ShouldNotBeNull();
        return part!;
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void InvokeInternal(object instance, string methodName, params object[] args)
    {
        var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        method.Invoke(instance, args);
    }
}
