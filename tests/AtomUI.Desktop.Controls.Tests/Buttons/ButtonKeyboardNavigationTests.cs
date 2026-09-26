using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Buttons;

public class ButtonKeyboardNavigationTests
{
    static ButtonKeyboardNavigationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Tab_Should_Move_Focus_To_AtomButton()
    {
        TabShouldMoveFocusToAtomButton(new AvaloniaWindow());
    }

    [Fact]
    public void Tab_Should_Move_Focus_To_AtomButton_In_AtomWindow()
    {
        TabShouldMoveFocusToAtomButton(new AtomWindow());
    }

    [Fact]
    public void Fresh_AtomWindow_Tab_From_No_Focus_Should_Reach_Button()
    {
        var button1 = new AtomButton { Content = "One", Width = 80 };
        var panel   = new StackPanel();
        panel.Children.Add(button1);

        var window = new AtomWindow
        {
            Width   = 360,
            Height  = 220,
            Content = panel
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var focusManager = TopLevel.GetTopLevel(panel)!.FocusManager;
            focusManager.GetFocusedElement().ShouldNotBeSameAs(button1);

            for (var i = 0; i < 8; i++)
            {
                PressTab(window);
                if (ReferenceEquals(focusManager.GetFocusedElement(), button1))
                {
                    break;
                }
            }

            focusManager.GetFocusedElement().ShouldBeSameAs(button1);
        }
        finally
        {
            window.Close();
        }
    }

    private static void TabShouldMoveFocusToAtomButton(AvaloniaWindow window)
    {
        var textBox  = new TextBox { Width = 120 };
        var button1  = new AtomButton { Content = "One", Width = 80 };
        var button2  = new AtomButton { Content = "Two", Width = 80 };

        var panel = new StackPanel();
        panel.Children.Add(textBox);
        panel.Children.Add(button1);
        panel.Children.Add(button2);

        window.Width   = 360;
        window.Height  = 220;
        window.Content = panel;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var focusManager = TopLevel.GetTopLevel(panel)!.FocusManager;

            // 起点：焦点在 TextBox
            textBox.Focus().ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            PressTab(window);
            focusManager.GetFocusedElement().ShouldBeSameAs(button1);

            PressTab(window);
            focusManager.GetFocusedElement().ShouldBeSameAs(button2);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Focused_AtomButton_Enter_Should_Click()
    {
        var clicked  = 0;
        var button   = new AtomButton { Content = "Go", Width = 80 };
        button.Click += (_, _) => clicked++;

        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 220,
            Content = button
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            button.Focus().ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();

            clicked.ShouldBe(1);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Tab_Focused_Button_Should_Show_FocusVisual()
    {
        AssertFocusVisualVisible(new AtomButton { Content = "Go", Width = 80 },
            "ButtonTheme.axaml");
    }

    [Fact]
    public void Tab_Focused_Dashed_Button_Should_Show_FocusVisual()
    {
        AssertFocusVisualVisible(new AtomButton { Content = "Go", Width = 80, ButtonType = ButtonType.Dashed },
            "ButtonTheme.axaml");
    }

    [Fact]
    public void Tab_Focused_Dashed_Variant_Button_Should_Show_FocusVisual()
    {
        AssertFocusVisualVisible(new AtomButton { Content = "Go", Width = 80, Variant = ButtonVariant.Dashed },
            "ButtonTheme.axaml");
    }

    [Fact]
    public void Tab_Focused_DropdownButton_Should_Show_FocusVisual()
    {
        AssertFocusVisualVisible(new DropdownButton { Content = "Go", Width = 120 },
            "DropdownButtonTheme.axaml / DropdownButtonBaseTheme.axaml");
    }

    private static void AssertFocusVisualVisible(Control button, string themeFile)
    {
        var window = new AtomWindow
        {
            Width   = 360,
            Height  = 220,
            Content = button
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            button.Focus(NavigationMethod.Tab).ShouldBeTrue();
            Dispatcher.UIThread.RunJobs();

            // :focus-visible 伪类应在键盘导航后置位
            button.Classes.Contains(":focus-visible").ShouldBeTrue();

            // 主题声明了 ^:focus-visible /template/ Border#FocusVisual 样式，
            // 模板必须存在该元素且在键盘焦点下可见，锁定 #487 的回归形态
            var focusVisual = button.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(c => c.Name == "FocusVisual");
            focusVisual.ShouldNotBeNull($"{themeFile} :focus-visible 样式指向 Border#FocusVisual，模板必须包含该元素");
            focusVisual.IsVisible.ShouldBeTrue();
            focusVisual.BorderThickness.ShouldNotBe(new Thickness(0));
            focusVisual.BorderBrush.ShouldNotBeNull("ColorFocusBorder 令牌应解析出焦点环画刷");
        }
        finally
        {
            window.Close();
        }
    }

    private static void PressTab(AvaloniaWindow window)
    {
        window.KeyPress(Key.Tab, RawInputModifiers.None, PhysicalKey.Tab, null);
        Dispatcher.UIThread.RunJobs();
    }
}
