using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUI.Desktop.Controls.Primitives.Themes;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NumericUpDown;

public class NumericUpDownShiftConductionTests
{
    static NumericUpDownShiftConductionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void DecoratedBox_Shift_Properties_Drive_The_Template_Transforms()
    {
        var decoratedBox = new ButtonSpinnerDecoratedBox
        {
            Width = 200,
            Height = 32,
            IsMotionEnabled = false,
            ContentLeftAddOn = "$",
            ContentRightAddOn = "kg"
        };

        var window = new AvaloniaWindow { Width = 320, Height = 120, Content = decoratedBox };
        window.Show();
        decoratedBox.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var leftAddOn = decoratedBox.GetVisualDescendants()
                                        .OfType<AddOnContentPresenter>()
                                        .Single(p => p.Name == AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart);
            var rightAddOn = decoratedBox.GetVisualDescendants()
                                         .OfType<AddOnContentPresenter>()
                                         .Single(p => p.Name == AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart);
            var handle = decoratedBox.GetVisualDescendants()
                                     .OfType<ContentPresenter>()
                                     .Single(p => p.Name == "PART_SpinnerHandle");

            ((leftAddOn.RenderTransform as TranslateTransform)!).X.ShouldBe(decoratedBox.ContentLeftShift, 0.5);
            ((rightAddOn.RenderTransform as TranslateTransform)!).X.ShouldBe(decoratedBox.ContentRightShift, 0.5);
            ((handle.RenderTransform as TranslateTransform)!).X.ShouldBe(decoratedBox.HandleOffset, 0.5);

            decoratedBox.ContentLeftShift = 21d;
            decoratedBox.ContentRightShift = -40d;
            decoratedBox.HandleOffset = 12d;
            Dispatcher.UIThread.RunJobs();

            ((leftAddOn.RenderTransform as TranslateTransform)!).X.ShouldBe(21d, 0.5);
            ((rightAddOn.RenderTransform as TranslateTransform)!).X.ShouldBe(-40d, 0.5);
            ((handle.RenderTransform as TranslateTransform)!).X.ShouldBe(12d, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NumericUpDownMode.Spinner)]
    [InlineData(NumericUpDownMode.Input)]
    public void InnerLeftContent_Prefix_Renders_In_The_Input_Segment(NumericUpDownMode mode)
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Mode = mode,
            Minimum = 1,
            Maximum = 100,
            Value = 10m,
            InnerLeftContent = "$",
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
            var inner = numericUpDown.GetVisualDescendants()
                                     .OfType<ContentPresenter>()
                                     .Where(p => p.Name == "PART_InnerLeftContentPresenter")
                                     .ToList();
            var dump = string.Join(" ;; ", inner.Select(p =>
            {
                var chain = new System.Text.StringBuilder();
                foreach (var a in p.GetSelfAndVisualAncestors())
                {
                    chain.Append(a.GetType().Name);
                    if (a.Classes.Count > 0)
                    {
                        chain.Append("(").Append(string.Join("+", a.Classes)).Append(")");
                    }
                    chain.Append(" > ");
                    if (a is Avalonia.Controls.Window)
                    {
                        break;
                    }
                }
                return chain.ToString();
            }));
            inner.Count.ShouldBe(1, dump);
            inner[0].Content.ShouldBe("$");
            inner[0].Bounds.Width.ShouldBeGreaterThan(0d, "InnerLeftContent 应该在输入区渲染出前缀");
        }
        finally
        {
            window.Close();
        }
    }
}
