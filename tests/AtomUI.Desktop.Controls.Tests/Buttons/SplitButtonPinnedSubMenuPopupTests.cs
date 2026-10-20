using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

// 回归：钉住弹层的 pin 语义只能作用在真正承载子菜单的弹层上。
// 叶子菜单项的 PART_Popup 若被置为 pinned，会自行强制打开成一个没有内容的空白弹层，
// 遗留在 overlay 层，表现为菜单上方一条带阴影的空白卡片。
public class SplitButtonPinnedSubMenuPopupTests
{
    static SplitButtonPinnedSubMenuPopupTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pinned_Menu_Only_Pins_Popups_That_Carry_A_SubMenu()
    {
        var splitButton = new SplitButton
        {
            Content = "Primary Style",
            IsPrimaryButtonType = true,
            IsMotionEnabled = false,
            IsPopupPinnedOpen = true,
            Flyout = new MenuFlyout
            {
                Items =
                {
                    new MenuItemGroup
                    {
                        Header = "Group title",
                        Items = { new MenuItem { Header = "1st menu item" } }
                    },
                    new MenuItem
                    {
                        Header = "SubMenu",
                        IsSubMenuOpen = true,
                        Items =
                        {
                            new MenuItemGroup
                            {
                                Header = "Group title",
                                Items = { new MenuItem { Header = "Option 1" }, new MenuItem { Header = "Option 2" } }
                            }
                        }
                    },
                    new MenuItem { Header = "Delete" }
                }
            }
        };

        ShowInWindow(splitButton, window =>
        {
            var pinnedFlags = window.GetVisualDescendants()
                                   .OfType<Popup>()
                                   .Select(static popup => (popup, pinned: GetPinned(popup)))
                                   .Where(static pair => pair.pinned)
                                   .Select(pair => GetOwnerHeader(pair.popup))
                                   .ToArray();

            // 只有带子菜单的项允许 pinned；叶子项（1st menu item / Option 1 / Option 2 / Delete）不得 pinned。
            pinnedFlags.ShouldNotContain("Delete");
            pinnedFlags.ShouldNotContain("1st menu item");
            pinnedFlags.ShouldNotContain("Option 1");
            pinnedFlags.ShouldNotContain("Option 2");

            // 每个弹层宿主都必须能解析到一个真正的 MenuFlyoutPresenter 内容（非空白壳）。
            // 空白壳的判据：宿主可见且尺寸大于 0，但其可视子树里既没有菜单项容器，
            // 也没有任何非空文本——只剩余圆角白底与阴影。
            var blankHosts = window.GetVisualDescendants()
                                   .OfType<OverlayPopupHost>()
                                   .Where(static host => host.IsVisible &&
                                                         host.Bounds.Width > 0 &&
                                                         host.Bounds.Height > 0)
                                   .Where(static host =>
                                   {
                                       var hasMenuItem = host.GetVisualDescendants()
                                                             .OfType<MenuItem>()
                                                             .Any();
                                       var hasText = host.GetVisualDescendants()
                                                         .OfType<TextBlock>()
                                                         .Any(static text => !string.IsNullOrEmpty(text.Text));
                                       return !hasMenuItem && !hasText;
                                   })
                                   .ToArray();
            blankHosts.ShouldBeEmpty(
                "a blank popup shell (rounded white card with shadow) must not be left in the overlay layer");
        });
    }

    private static bool GetPinned(Popup popup)
    {
        var flags = System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public;
        return (bool)(popup.GetType().GetProperty("IsPopupPinnedOpen", flags)?.GetValue(popup) ?? false);
    }

    private static string? GetOwnerHeader(Popup popup)
    {
        var item = popup.Parent as MenuItem ??
                   popup.GetVisualAncestors().OfType<MenuItem>().FirstOrDefault();
        return item?.Header?.ToString() ?? popup.GetType().Name;
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel { Width = 900, Height = 700 };
        overlayPanel.Children.Add(content);
        var visualLayerManager = new VisualLayerManager { Child = overlayPanel };
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);

        var window = new AvaloniaWindow { Width = 900, Height = 700, Content = visualLayerManager };
        try
        {
            window.Show();
            for (var i = 0; i < 6; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
