using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NumericUpDown;

public class NumericUpDownSpinnerLayoutTests
{
    static NumericUpDownSpinnerLayoutTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(NumericUpDownMode.Spinner)]
    [InlineData(NumericUpDownMode.Input)]
    public void Text_Input_Frame_Fills_The_Embedded_Text_Box(NumericUpDownMode mode)
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 150,
            Mode = mode,
            Minimum = 1,
            Maximum = 10,
            Value = 3m,
            IsMotionEnabled = false
        };

        var window = new AvaloniaWindow { Width = 320, Height = 120, Content = numericUpDown };
        window.Show();
        numericUpDown.ApplyTemplate();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var textBox = numericUpDown.GetVisualDescendants()
                                       .OfType<EmbeddedTextBox>()
                                       .Single();
            var frame = numericUpDown.GetVisualDescendants()
                                     .OfType<InputControlFrame>()
                                     .Single(control => control.Name == "PART_InputControlFrame");

            frame.Bounds.Height.ShouldBe(textBox.Bounds.Height, 0.5);
            var frameCenter = frame.TransformToVisual(textBox).ShouldNotBeNull()
                                       .Transform(new Point(0, 0)).Y + frame.Bounds.Height / 2;
            frameCenter.ShouldBe(textBox.Bounds.Height / 2, 0.5);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Suffix_Group_Slides_Left_With_The_Floating_Handle_Shift()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Mode = NumericUpDownMode.Input,
            Value = 3m,
            IsAllowClear = true,
            InnerRightContent = "kg",
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
            var suffix = numericUpDown.GetVisualDescendants()
                                      .OfType<StackPanel>()
                                      .Single(control => control.Classes.Contains("semantic-suffix"));
            var decoratedBox = numericUpDown.GetVisualDescendants()
                                            .OfType<ButtonSpinnerDecoratedBox>()
                                            .Single();
            var originX = suffix.TransformToVisual(numericUpDown).ShouldNotBeNull()
                                 .Transform(new Point(0, 0)).X;

            var editor = numericUpDown.GetVisualDescendants().OfType<EmbeddedTextBox>().Single();
            var clear = numericUpDown.GetVisualDescendants().OfType<InputClearIconButton>()
                                     .Single(control => ReferenceEquals(control.TemplatedParent, numericUpDown));
            var outerBounds = numericUpDown.Bounds;
            var hoverPoint = editor.TranslatePoint(new Point(10, editor.Bounds.Height / 2), window)
                                   .ShouldNotBeNull();
            window.MouseMove(hoverPoint);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var shiftedX = suffix.TransformToVisual(numericUpDown).ShouldNotBeNull()
                                  .Transform(new Point(0, 0)).X;
            (originX - shiftedX).ShouldBe(-decoratedBox.ContentRightShift, 0.5);
            var editorRight = editor.TranslatePoint(new Point(editor.Bounds.Width, 0), numericUpDown)
                                    .ShouldNotBeNull().X;
            editorRight.ShouldBeLessThanOrEqualTo(shiftedX + 0.5);
            var sharedShift = decoratedBox.ContentRightShift;
            foreach (var inset in new[] { 0.25, 0.5, 0.75, 1.25 })
            {
                decoratedBox.ContentRightShift = -inset;
                window.UpdateLayout();
                var animatedEditorRight = editor.TranslatePoint(new Point(editor.Bounds.Width, 0), numericUpDown)
                                                .ShouldNotBeNull().X;
                var animatedSuffixLeft = suffix.TranslatePoint(default, numericUpDown).ShouldNotBeNull().X;
                animatedEditorRight.ShouldBeLessThanOrEqualTo(animatedSuffixLeft + 0.01);
            }
            decoratedBox.ContentRightShift = sharedShift;
            window.UpdateLayout();

            foreach (var x in new[] { 1d, clear.Bounds.Width / 2, clear.Bounds.Width - 1d })
            {
                var point = clear.TranslatePoint(new Point(x, clear.Bounds.Height / 2), window)
                                 .ShouldNotBeNull();
                var hit = (window.InputHitTest(point) as Visual).ShouldNotBeNull();
                hit.GetSelfAndVisualAncestors().Contains(clear).ShouldBeTrue();
            }

            var center = clear.TranslatePoint(new Point(clear.Bounds.Width / 2, clear.Bounds.Height / 2), window)
                              .ShouldNotBeNull();
            window.MouseMove(center);
            window.MouseDown(center, MouseButton.Left);
            window.MouseUp(center, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            numericUpDown.Value.ShouldBeNull();
            string.IsNullOrEmpty(numericUpDown.Text).ShouldBeTrue();
            clear.IsEffectivelyVisible.ShouldBeFalse();
            numericUpDown.Bounds.ShouldBe(outerBounds);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Suffix_Group_Recomputes_Shift_When_Size_Changes_While_Hovered()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Mode = NumericUpDownMode.Input,
            Value = 3m,
            InnerRightContent = "kg",
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
            var suffix = numericUpDown.GetVisualDescendants()
                                      .OfType<StackPanel>()
                                      .Single(control => control.Classes.Contains("semantic-suffix"));
            var decoratedBox = numericUpDown.GetVisualDescendants()
                                            .OfType<ButtonSpinnerDecoratedBox>()
                                            .Single();
            decoratedBox.IsSpinnerContentHover = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var inputPanel = suffix.GetVisualParent().ShouldNotBeNull();
            var middleShift = suffix.TranslatePoint(new Point(suffix.Bounds.Width, 0), inputPanel)
                                    .ShouldNotBeNull().X + suffix.Margin.Right - inputPanel.Bounds.Width;

            numericUpDown.SizeType = CustomizableSizeType.Small;
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var smallShift = suffix.TranslatePoint(new Point(suffix.Bounds.Width, 0), inputPanel)
                                   .ShouldNotBeNull().X + suffix.Margin.Right - inputPanel.Bounds.Width;
            smallShift.ShouldNotBe(middleShift);
            smallShift.ShouldBe(decoratedBox.ContentRightShift, 0.5);
            var handle = numericUpDown.GetVisualDescendants().OfType<ContentPresenter>()
                                      .Single(control => control.Name == "PART_SpinnerHandle");
            var handleLeft = handle.TranslatePoint(default, inputPanel).ShouldNotBeNull().X;
            var suffixRight = suffix.TranslatePoint(new Point(suffix.Bounds.Width, 0), inputPanel)
                                    .ShouldNotBeNull().X;
            suffixRight.ShouldBeLessThanOrEqualTo(handleLeft);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Suffix_Group_Keeps_A_Visible_Gap_From_The_Spinner_Action_Divider()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width = 320,
            Mode = NumericUpDownMode.Spinner,
            Minimum = 1,
            Maximum = 100,
            Value = 10m,
            IsAllowClear = true,
            InnerLeftContent = "$",
            InnerRightContent = "kg",
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
            var suffix = numericUpDown.GetVisualDescendants()
                                      .OfType<StackPanel>()
                                      .Single(control => control.Classes.Contains("semantic-suffix"));
            var segmentFrames = numericUpDown.GetVisualDescendants()
                                             .OfType<Layoutable>()
                                             .Where(control => control.Name == "Frame")
                                             .Select(control => (Layout: control,
                                                 X: control.TransformToVisual(numericUpDown).ShouldNotBeNull()
                                                     .Transform(new Point(0, 0)).X))
                                             .ToArray();
            segmentFrames.ShouldNotBeEmpty();
            var increaseDividerX = segmentFrames.Max(static entry => entry.X);

            var suffixRight = suffix.TransformToVisual(numericUpDown).ShouldNotBeNull()
                                    .Transform(new Point(0, 0)).X + suffix.Bounds.Width;
            (increaseDividerX - suffixRight).ShouldBeGreaterThanOrEqualTo(6d);
        }
        finally
        {
            window.Close();
        }
    }
}
