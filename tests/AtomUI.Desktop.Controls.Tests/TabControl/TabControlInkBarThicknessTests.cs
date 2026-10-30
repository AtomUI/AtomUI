using System.ComponentModel;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUITabControl = AtomUI.Desktop.Controls.TabControl;
using AtomUITabItem    = AtomUI.Desktop.Controls.TabItem;
using AtomUITabStrip   = AtomUI.Desktop.Controls.TabStrip;
using AtomUITabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AvaloniaWindow   = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabControlInkBarThicknessTests
{
    static TabControlInkBarThicknessTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void InkBar_Thickness_Defaults_To_LineWidthBold()
    {
        var tabControl = new AtomUITabControl();
        using (Show(tabControl))
        {
            // LineWidthBold 由 lineWidth(1) + 1 计算而来
            tabControl.SelectedIndicatorThickness.ShouldBe(2d);
        }
    }

    [Fact]
    public void InkBar_Thickness_Token_Override_Applies_To_Control()
    {
        var tabControl = new AtomUITabControl();
        tabControl.Resources[TabControlTokenKind.InkBarThickness] = 4.0;
        using (Show(tabControl))
        {
            tabControl.SelectedIndicatorThickness.ShouldBe(4d);
        }
    }

    [Fact]
    public void InkBar_Thickness_Token_Override_Sizes_Selected_Indicator()
    {
        var tabControl = new AtomUITabControl { IsMotionEnabled = true };
        tabControl.Resources[TabControlTokenKind.InkBarThickness] = 4.0;
        tabControl.Items.Add(new AtomUITabItem { Header = "Tab 1" });
        tabControl.Items.Add(new AtomUITabItem { Header = "Tab 2" });
        tabControl.SelectedIndex = 1;
        using (Show(tabControl))
        {
            var indicator = tabControl.GetVisualDescendants()
                                      .OfType<Border>()
                                      .Single(static candidate => candidate.Name == "PART_SelectedItemIndicator");
            indicator.Height.ShouldBe(4d);
            tabControl.SelectedIndex.ShouldBe(1);
            var selected = tabControl.ContainerFromIndex(1).ShouldNotBeNull();
            indicator.TranslatePoint(default, tabControl)!.Value.X.ShouldBe(selected.TranslatePoint(default, tabControl)!.Value.X, 1e-6);
        }
    }

    [Fact]
    public void TabStrip_InkBar_Thickness_Token_Override_Applies()
    {
        var tabStrip = new AtomUITabStrip { IsMotionEnabled = true };
        tabStrip.Items.Add(new AtomUITabStripItem { Content = "First" });
        tabStrip.Items.Add(new AtomUITabStripItem { Content = "Second" });
        tabStrip.SelectedIndex = 1;
        tabStrip.Resources[TabControlTokenKind.InkBarThickness] = 4.0;
        using (Show(tabStrip))
        {
            tabStrip.SelectedIndicatorThickness.ShouldBe(4d);
            var indicator = tabStrip.GetVisualDescendants().OfType<Border>()
                .Single(candidate => candidate.Name == "PART_SelectedItemIndicator");
            indicator.Height.ShouldBe(4d);
            tabStrip.SelectedIndex.ShouldBe(1);
            var selected = tabStrip.ContainerFromIndex(1).ShouldNotBeNull();
            indicator.TranslatePoint(default, tabStrip)!.Value.X.ShouldBe(selected.TranslatePoint(default, tabStrip)!.Value.X, 1e-6);
        }
    }

    private static IDisposable Show(Control content)
    {
        var window = new AvaloniaWindow { Width = 640, Height = 260, Content = content };
        ((ISupportInitialize)content).BeginInit();
        ((ISupportInitialize)content).EndInit();
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new WindowLifetime(window);
    }

    private sealed class WindowLifetime(AvaloniaWindow window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
