using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUI.Controls;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AtomUIScrollViewer = AtomUI.Desktop.Controls.ScrollViewer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NumericUpDown;

public class NumericUpDownHandleTests
{
    static NumericUpDownHandleTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Disabled_NumericUpDown_Hides_Floatable_Spinner_Handle()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsEnabled       = false,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var spinnerHandle = numericUpDown.GetVisualDescendants()
                                             .OfType<ContentPresenter>()
                                             .Single(item => item.Name == "PART_SpinnerHandle");

            spinnerHandle.Opacity.ShouldBe(0.0);
        });
    }

    [Fact]
    public void Disabled_NumericUpDown_Uses_Disabled_Text_Foreground()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsEnabled       = false,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var scrollViewer = numericUpDown.GetVisualDescendants()
                                            .OfType<AtomUIScrollViewer>()
                                            .Single(item => item.Name == "ScrollViewer");

            BrushShouldHaveSameColor(
                scrollViewer.Foreground,
                GetThemeResource<IBrush>(SharedTokenKind.ColorTextDisabled));
        });
    }

    [Fact]
    public void Filled_NumericUpDown_Uses_Filled_Spinner_Handle_Background()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            StyleVariant    = InputControlStyleVariant.Filled,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var spinnerHandle = numericUpDown.GetVisualDescendants()
                                             .OfType<TemplatedControl>()
                                             .Single(item => item.GetType().Name == "ButtonSpinnerHandle");

            BrushShouldHaveSameColor(
                spinnerHandle.Background,
                GetThemeResource<IBrush>(ButtonSpinnerTokenKind.FilledHandleBg));
        });
    }

    [Fact]
    public void Default_Input_Mode_Uses_Floating_Handle_Template()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            numericUpDown.Mode.ShouldBe(NumericUpDownMode.Input);

            var spinnerHandle = numericUpDown.GetVisualDescendants()
                                             .OfType<TemplatedControl>()
                                             .Single(item => item.GetType().Name == "ButtonSpinnerHandle");

            var spinnerButtons = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Where(item => item.Name is "PART_DecreaseButton" or "PART_IncreaseButton")
                                              .ToList();

            spinnerButtons.Count.ShouldBe(2);
            spinnerButtons.All(item => item.GetVisualAncestors().Contains(spinnerHandle)).ShouldBeTrue();
        });
    }

    [Fact]
    public void Default_Input_Mode_Uses_Outer_Content_Padding_Only()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var textBox = numericUpDown.GetVisualDescendants()
                                       .OfType<TextBox>()
                                       .Single(item => item.Name == "PART_TextBox");
            var contentFrame = numericUpDown.GetVisualDescendants()
                                            .OfType<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>()
                                            .Single(item => item.Name == "PART_ContentFrame");

            contentFrame.Padding.Left.ShouldBeGreaterThan(0);
            textBox.ShouldBeAssignableTo<EmbeddedTextBox>();
            textBox.SizeType.ShouldBe(CustomizableSizeType.Custom);
            textBox.Padding.ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Spinner_Mode_Uses_Outer_Content_Padding_Only()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var textBox = numericUpDown.GetVisualDescendants()
                                       .OfType<TextBox>()
                                       .Single(item => item.Name == "PART_TextBox");
            var contentFrame = numericUpDown.GetVisualDescendants()
                                            .OfType<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>()
                                            .Single(item => item.Name == "PART_ContentFrame");

            contentFrame.Padding.ShouldBe(new Thickness(0));
            textBox.ShouldBeAssignableTo<EmbeddedTextBox>();
            textBox.SizeType.ShouldBe(CustomizableSizeType.Custom);
            textBox.Padding.ShouldBe(new Thickness(0));
        });
    }

    [Fact]
    public void Input_Mode_TextBox_Uses_Custom_SizeType_Instead_Of_LocalValue_Binding_Or_Custom_Padding_State()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/NumericUpDown/Themes/NumericUpDownTheme.axaml");

        source.ShouldContain("SizeType=\"Custom\"");
        source.ShouldContain("Padding=\"0\"");
        source.ShouldNotContain("IsCustomPadding");
        source.ShouldNotContain("Padding=\"{Binding Source=0");
    }

    [Fact]
    public void Input_Mode_Updates_ButtonSpinner_Visibility_From_ShowButtonSpinner()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width             = 160,
            Value             = 3,
            ShowButtonSpinner = false,
            IsMotionEnabled   = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var spinner = numericUpDown.GetVisualDescendants()
                                       .OfType<global::AtomUI.Desktop.Controls.ButtonSpinner>()
                                       .Single(item => item.Name == "PART_Spinner");

            spinner.IsButtonSpinnerVisible.ShouldBeFalse();

            numericUpDown.ShowButtonSpinner = true;
            Dispatcher.UIThread.RunJobs();

            spinner.IsButtonSpinnerVisible.ShouldBeTrue();
        });
    }

    [Fact]
    public void Spinner_Mode_Uses_Inline_Stepper_Template()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            numericUpDown.GetVisualDescendants()
                         .OfType<TemplatedControl>()
                         .Count(item => item.GetType().Name == "ButtonSpinnerHandle")
                         .ShouldBe(0);

            numericUpDown.GetVisualDescendants()
                         .OfType<IconButton>()
                         .Single(item => item.Name == "PART_DecreaseButton")
                         .ShouldNotBeNull();

            numericUpDown.GetVisualDescendants()
                         .OfType<IconButton>()
                         .Single(item => item.Name == "PART_IncreaseButton")
                         .ShouldNotBeNull();
        });
    }

    [Fact]
    public void Spinner_Mode_With_Hidden_Handle_Uses_Input_Template()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width             = 160,
            Value             = 3,
            Mode              = NumericUpDownMode.Spinner,
            ShowButtonSpinner = false,
            IsMotionEnabled   = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var spinnerHandle = numericUpDown.GetVisualDescendants()
                                             .OfType<TemplatedControl>()
                                             .Single(item => item.GetType().Name == "ButtonSpinnerHandle");
            var textBox = numericUpDown.GetVisualDescendants()
                                       .OfType<TextBox>()
                                       .Single(item => item.Name == "PART_TextBox");
            var spinnerButtons = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Where(item => item.Name is "PART_DecreaseButton" or "PART_IncreaseButton")
                                              .ToList();

            textBox.TextAlignment.ShouldBe(TextAlignment.Start);
            spinnerButtons.Count.ShouldBe(2);
            spinnerButtons.All(item => item.GetVisualAncestors().Contains(spinnerHandle)).ShouldBeTrue();

            numericUpDown.ShowButtonSpinner = true;
            Dispatcher.UIThread.RunJobs();

            numericUpDown.GetVisualDescendants()
                         .OfType<TemplatedControl>()
                         .Count(item => item.GetType().Name == "ButtonSpinnerHandle")
                         .ShouldBe(0);

            var decreaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_DecreaseButton");
            var increaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_IncreaseButton");

            decreaseButton.GetVisualParent().ShouldBeAssignableTo<PixelAlignedBorder>();
            increaseButton.GetVisualParent().ShouldBeAssignableTo<PixelAlignedBorder>();
        });
    }

    [Fact]
    public void Spinner_Mode_Buttons_Do_Not_Draw_Own_Rounded_Frame()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var decreaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_DecreaseButton");
            var increaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_IncreaseButton");

            decreaseButton.CornerRadius.ShouldBe(new CornerRadius(0));
            increaseButton.CornerRadius.ShouldBe(new CornerRadius(0));
            decreaseButton.BorderThickness.ShouldBe(new Thickness(0));
            increaseButton.BorderThickness.ShouldBe(new Thickness(0));

            var decreaseSegment = decreaseButton.GetVisualParent();
            var increaseSegment = increaseButton.GetVisualParent();

            decreaseSegment.ShouldBeAssignableTo<PixelAlignedBorder>();
            increaseSegment.ShouldBeAssignableTo<PixelAlignedBorder>();
            ((PixelAlignedBorder)decreaseSegment!).BorderThickness.ShouldBe(new Thickness(0, 0, 1, 0));
            ((PixelAlignedBorder)increaseSegment!).BorderThickness.ShouldBe(new Thickness(1, 0, 0, 0));
        });
    }

    [Fact]
    public void Spinner_Mode_Action_Segments_Are_Square_And_Use_Normal_Icon_Size()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 150,
            Value           = 3,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var iconSize  = GetThemeResource<double>(SharedTokenKind.IconSize);
            var lineWidth = GetThemeResource<double>(SharedTokenKind.LineWidth);

            var decreaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_DecreaseButton");
            var increaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_IncreaseButton");

            decreaseButton.Padding.ShouldBe(new Thickness(0));
            increaseButton.Padding.ShouldBe(new Thickness(0));
            decreaseButton.IconWidth.ShouldBe(iconSize);
            increaseButton.IconWidth.ShouldBe(iconSize);
            decreaseButton.IconHeight.ShouldBe(iconSize);
            increaseButton.IconHeight.ShouldBe(iconSize);

            foreach (var size in new[] { CustomizableSizeType.Small, CustomizableSizeType.Middle, CustomizableSizeType.Large })
            {
                numericUpDown.SizeType = size;
                ((AvaloniaWindow)TopLevel.GetTopLevel(numericUpDown)!).UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                ((AvaloniaWindow)TopLevel.GetTopLevel(numericUpDown)!).UpdateLayout();
                decreaseButton.Bounds.Width.ShouldBe(decreaseButton.Bounds.Height, 0.5);
                increaseButton.Bounds.Width.ShouldBe(increaseButton.Bounds.Height, 0.5);
                decreaseButton.GetVisualParent()!.Bounds.Width.ShouldBe(decreaseButton.Bounds.Width + lineWidth, 0.5);
                increaseButton.GetVisualParent()!.Bounds.Width.ShouldBe(increaseButton.Bounds.Width + lineWidth, 0.5);
            }

            numericUpDown.SizeType = CustomizableSizeType.Custom;
            numericUpDown.Height = 48;
            ((AvaloniaWindow)TopLevel.GetTopLevel(numericUpDown)!).UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            ((AvaloniaWindow)TopLevel.GetTopLevel(numericUpDown)!).UpdateLayout();
            decreaseButton.Bounds.Width.ShouldBe(decreaseButton.Bounds.Height, 0.5);
            increaseButton.Bounds.Width.ShouldBe(increaseButton.Bounds.Height, 0.5);
        });
    }

    [Fact]
    public void Spinner_Mode_Action_Buttons_Use_Handle_State_Visuals()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 150,
            Value           = 3,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var hoverBrush       = GetThemeResource<IBrush>(ButtonSpinnerTokenKind.HandleHoverColor);
            var activeBackground = GetThemeResource<IBrush>(ButtonSpinnerTokenKind.HandleActiveBg);
            var increaseButton   = numericUpDown.GetVisualDescendants()
                                                .OfType<IconButton>()
                                                .Single(item => item.Name == "PART_IncreaseButton");

            increaseButton.IsMotionEnabled = false;

            ((IPseudoClasses)increaseButton.Classes).Set(":pointerover", true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(increaseButton.IconBrush, hoverBrush);

            ((IPseudoClasses)increaseButton.Classes).Set(":pressed", true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(increaseButton.Background, activeBackground);
        });
    }

    [Fact]
    public void Spinner_Mode_Uses_Input_Control_Height_And_Centers_Action_Icons()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 150,
            Value           = 3,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var controlHeight  = GetThemeResource<double>(SharedTokenKind.ControlHeight);
            var iconSize       = GetThemeResource<double>(SharedTokenKind.IconSize);
            var lineWidth      = GetThemeResource<double>(SharedTokenKind.LineWidth);
            var expectedWidth  = controlHeight - lineWidth * 2;
            var increaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_IncreaseButton");
            var contentFrame = numericUpDown.GetVisualDescendants()
                                            .OfType<global::AtomUI.Desktop.Controls.AddOnDecoratedBoxContentFrame>()
                                            .Single(item => item.Name == "PART_ContentFrame");
            var iconPresenter = increaseButton.GetVisualDescendants()
                                              .OfType<IconPresenter>()
                                              .Single(item => item.Name == "IconPresenter");

            numericUpDown.Bounds.Height.ShouldBe(controlHeight, 0.5);
            contentFrame.Padding.ShouldBe(new Thickness(0));
            increaseButton.Bounds.Height.ShouldBe(controlHeight - lineWidth * 2, 0.5);
            increaseButton.Bounds.Width.ShouldBe(expectedWidth, 0.5);

            var buttonCenter = increaseButton.TranslatePoint(
                new Point(increaseButton.Bounds.Width / 2, increaseButton.Bounds.Height / 2),
                numericUpDown);
            var iconCenter = iconPresenter.TranslatePoint(
                new Point(iconPresenter.Bounds.Width / 2, iconPresenter.Bounds.Height / 2),
                numericUpDown);

            buttonCenter.ShouldNotBeNull();
            iconCenter.ShouldNotBeNull();
            iconCenter.Value.X.ShouldBe(buttonCenter.Value.X, 0.5);
            iconCenter.Value.Y.ShouldBe(buttonCenter.Value.Y, 0.5);
            iconCenter.Value.Y.ShouldBe(controlHeight / 2, 0.5);
        });
    }

    [Fact]
    public void Spinner_Mode_Updates_Button_Enabled_State_From_Min_Max()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Minimum         = 0,
            Maximum         = 3,
            Value           = 0,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var decreaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_DecreaseButton");
            var increaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_IncreaseButton");

            decreaseButton.IsEnabled.ShouldBeFalse();
            increaseButton.IsEnabled.ShouldBeTrue();

            numericUpDown.Value = 3;
            Dispatcher.UIThread.RunJobs();

            decreaseButton.IsEnabled.ShouldBeTrue();
            increaseButton.IsEnabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void Spinner_Mode_Button_Clicks_Change_Value()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Minimum         = 0,
            Maximum         = 10,
            Increment       = 1,
            Value           = 3,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var decreaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_DecreaseButton");
            var increaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_IncreaseButton");

            increaseButton.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, increaseButton));
            Dispatcher.UIThread.RunJobs();
            numericUpDown.Value.ShouldBe(4);

            decreaseButton.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, decreaseButton));
            Dispatcher.UIThread.RunJobs();
            numericUpDown.Value.ShouldBe(3);
        });
    }

    [Fact]
    public void Spinner_Mode_Respects_AllowSpin()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Minimum         = 0,
            Maximum         = 10,
            Value           = 3,
            AllowSpin       = false,
            Mode            = NumericUpDownMode.Spinner,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            var decreaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_DecreaseButton");
            var increaseButton = numericUpDown.GetVisualDescendants()
                                              .OfType<IconButton>()
                                              .Single(item => item.Name == "PART_IncreaseButton");

            decreaseButton.IsEnabled.ShouldBeFalse();
            increaseButton.IsEnabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void Mode_Runtime_Switch_Rebuilds_Template_Parts()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, () =>
        {
            numericUpDown.GetVisualDescendants()
                         .OfType<TemplatedControl>()
                         .Count(item => item.GetType().Name == "ButtonSpinnerHandle")
                         .ShouldBe(1);

            numericUpDown.Mode = NumericUpDownMode.Spinner;
            Dispatcher.UIThread.RunJobs();

            numericUpDown.GetVisualDescendants()
                         .OfType<TemplatedControl>()
                         .Count(item => item.GetType().Name == "ButtonSpinnerHandle")
                         .ShouldBe(0);

            numericUpDown.GetVisualDescendants()
                         .OfType<IconButton>()
                         .Single(item => item.Name == "PART_DecreaseButton")
                         .ShouldNotBeNull();
        });
    }

    [Fact]
    public void Floating_Handle_Render_Uses_Render_Scale_Aware_BorderThickness()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, window =>
        {
            window.SetRenderScaling(1.5);
            Dispatcher.UIThread.RunJobs();

            var spinnerHandle = numericUpDown.GetVisualDescendants()
                                             .OfType<TemplatedControl>()
                                             .Single(item => item.GetType().Name == "ButtonSpinnerHandle");
            var drawingGroup = RenderToDrawingGroup(spinnerHandle);
            var penThicknesses = EnumerateGeometryDrawings(drawingGroup)
                .Select(drawing => drawing.Pen?.Thickness)
                .Where(thickness => thickness is not null)
                .Select(thickness => thickness!.Value)
                .ToArray();

            penThicknesses.ShouldContain(thickness => Math.Abs(thickness - 2d / 3d) < 0.0001);
        });
    }

    [Fact]
    public void Floating_Handle_Render_Aligns_Left_Separator_To_Physical_Pixel()
    {
        var numericUpDown = new AtomUINumericUpDown
        {
            Width           = 160,
            Value           = 3,
            IsMotionEnabled = false
        };

        ShowInWindow(numericUpDown, window =>
        {
            window.SetRenderScaling(1.5);
            Dispatcher.UIThread.RunJobs();

            var spinnerHandle = numericUpDown.GetVisualDescendants()
                                             .OfType<TemplatedControl>()
                                             .Single(item => item.GetType().Name == "ButtonSpinnerHandle");
            var drawingGroup = RenderToDrawingGroup(spinnerHandle);
            var verticalLine = EnumerateStrokeDrawings(drawingGroup)
                .Single(drawing => drawing.Geometry?.Bounds is { Width: < 0.0001, Height: > 0 });

            verticalLine.Pen.ShouldNotBeNull();
            verticalLine.Pen!.Thickness.ShouldBe(2d / 3d, 0.0001);
            verticalLine.Geometry!.Bounds.X.ShouldBe(1d / 3d, 0.0001);
        });
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, IBrush? expected)
    {
        GetSolidBrushColor(actual).ShouldBe(GetSolidBrushColor(expected));
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
    }

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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        ShowInWindow(content, _ => assertion());
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 240,
            Height  = 120,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static DrawingGroup RenderToDrawingGroup(Control control)
    {
        var drawingGroup = new DrawingGroup();
        using (var context = drawingGroup.Open())
        {
            control.Render(context);
        }

        return drawingGroup;
    }

    private static IEnumerable<GeometryDrawing> EnumerateGeometryDrawings(Drawing drawing)
    {
        if (drawing is GeometryDrawing geometryDrawing)
        {
            yield return geometryDrawing;
        }
        else if (drawing is DrawingGroup drawingGroup)
        {
            foreach (var child in drawingGroup.Children.SelectMany(EnumerateGeometryDrawings))
            {
                yield return child;
            }
        }
    }

    private static IEnumerable<GeometryDrawing> EnumerateStrokeDrawings(Drawing drawing)
    {
        return EnumerateGeometryDrawings(drawing)
            .Where(drawing => drawing.Pen is not null);
    }
}
