using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NumericUpDown;

public class NumericUpDownAddonIconTests
{
    static NumericUpDownAddonIconTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Right_AddOn_PathIcon_Renders_Inside_The_SizeType_Aware_Presenter()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 400,
            Value = 3m,
            RightAddOn = new SettingOutlined(),
            IsMotionEnabled = false
        };

        var window = new AvaloniaWindow { Width = 480, Height = 120, Content = numericUpDown };
        window.Show();
        numericUpDown.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var wrapper = numericUpDown.GetVisualDescendants()
                                       .OfType<SizeTypeAwareIconPresenter>()
                                       .Single();
            wrapper.GetVisualDescendants()
                   .OfType<IconPresenter>()
                   .ShouldNotBeEmpty();
            wrapper.Bounds.Height.ShouldBeGreaterThanOrEqualTo(10d);
            wrapper.Bounds.Width.ShouldBeGreaterThanOrEqualTo(10d);
        }
        finally
        {
            window.Close();
        }
    }
}
