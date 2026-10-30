using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Icon;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class IconShowCasePageTests
{

    [Fact]
    public void Icon_ShowCase_Click_Copies_Icon_Name_And_Shows_Success_Feedback()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new IconShowCase();

        ShowInWindow(page, window =>
        {
            var item = WaitForVisual<IconInfoItem>(page);

            item.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, item));

            WaitForClipboardText(item, item.IconName);
            var messageManager = WaitForVisual<WindowMessageManager>(window);
            messageManager.MaxItems.ShouldBe(1);
            var messageCard = WaitForVisual<MessageCard>(messageManager);
            messageCard.MessageType.ShouldBe(MessageType.Success);
            messageCard.Message.ShouldContain(item.IconName);

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            messageManager.GetVisualParent().ShouldBeNull();
        });
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = content
        };
        var window = new AvaloniaWindow
        {
            Width   = 1000,
            Height  = 700,
            Content = visualLayerManager
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static T WaitForVisual<T>(Visual root)
        where T : Visual
    {
        for (var i = 0; i < 40; i++)
        {
            Dispatcher.UIThread.RunJobs();
            var visual = root.GetVisualDescendants().OfType<T>().FirstOrDefault();
            if (visual is not null)
            {
                return visual;
            }

            Thread.Sleep(10);
        }

        return root.GetVisualDescendants().OfType<T>().First();
    }

    private static void WaitForClipboardText(Control control, string expectedText)
    {
        var clipboard = TopLevel.GetTopLevel(control)?.Clipboard;
        clipboard.ShouldNotBeNull();

        string? actualText = null;
        for (var i = 0; i < 40; i++)
        {
            Dispatcher.UIThread.RunJobs();
            using var dataTransfer = clipboard!.TryGetInProcessDataAsync().GetAwaiter().GetResult();
            actualText = dataTransfer is null
                ? null
                : dataTransfer.TryGetValueAsync(DataFormat.Text).GetAwaiter().GetResult();
            if (actualText == expectedText)
            {
                return;
            }

            Thread.Sleep(10);
        }

        actualText.ShouldBe(expectedText);
    }

}
