using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIAutoComplete = AtomUI.Desktop.Controls.AutoComplete;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.AutoComplete;

public class AutoCompletePinnedPopupTests
{
    static AutoCompletePinnedPopupTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pinned_Popup_Suppresses_Light_Dismiss_Before_The_Popup_Opens()
    {
        var autoComplete = new AtomUIAutoComplete
        {
            Width = 320,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            PlaceholderText = "input here",
            OptionsSource = new[]
            {
                new AutoCompleteOption { Header = "Alpha", Content = "Alpha" },
                new AutoCompleteOption { Header = "Alpine", Content = "Alpine" }
            }
        };

        ShowInWindow(autoComplete, window =>
        {
            var popup = autoComplete.GetVisualDescendants()
                                    .OfType<Popup>()
                                    .Single();
            popup.IsOpen.ShouldBeTrue();
            // Avalonia 只在弹层打开瞬间读取 IsLightDismissEnabled 创建遮罩层；
            // 钉住的弹层忽略 light-dismiss 关闭请求，必须在打开前抑制遮罩。
            popup.IsLightDismissEnabled.ShouldBeFalse();

            // placeholder 部件高亮必须紧贴文字（Ant Design 语义结构为内联元素）：
            // HorizontalContentAlignment 默认 Stretch，占位元素经转换器按 Left
            // 收缩，不得撑满内容区。
            var placeholder = autoComplete.GetVisualDescendants()
                                          .OfType<TextBlock>()
                                          .Single(textBlock =>
                                              textBlock.Classes.Contains("semantic-placeholder"));
            placeholder.IsVisible.ShouldBeTrue();
            placeholder.Bounds.Width
                       .ShouldBeLessThan(autoComplete.Bounds.Width * 0.5);

            // popup.listItem 部件的标记类在候选项容器创建时注入（与 ListBox 的
            // semantic-item 相同模式），解析器按"契约类型+标记类"命中候选项本体。
            var textBox = autoComplete.GetVisualDescendants()
                                      .OfType<AbstractTextInput>()
                                      .Single();
            textBox.Text = "Al";
            textBox.CaretIndex = 2;

            CandidateListItem[] items = [];
            for (var i = 0; i < 20 && items.Length < 2; i++)
            {
                Dispatcher.UIThread.RunJobs();
                items = window.GetVisualDescendants()
                              .OfType<CandidateListItem>()
                              .ToArray();
            }
            items.Length.ShouldBeGreaterThanOrEqualTo(2);
            items.ShouldAllBe(item =>
                item.Classes.Contains(AtomUI.Desktop.Controls.Primitives.CandidateList.PopupListItemClass));
        });
    }

    [Fact]
    public void Unpin_Restores_The_Template_Light_Dismiss_Default()
    {
        var autoComplete = new AtomUIAutoComplete
        {
            Width = 320,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true
        };

        ShowInWindow(autoComplete, _ =>
        {
            var popup = autoComplete.GetVisualDescendants()
                                    .OfType<Popup>()
                                    .Single();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            autoComplete.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width = 480,
            Height = 240
        };
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width = 480,
            Height = 240,
            Content = visualLayerManager
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
