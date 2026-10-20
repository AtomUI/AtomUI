using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaGrid = Avalonia.Controls.Grid;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.OtpLineEdit;

public class OtpLineEditBehaviorTests
{
    static OtpLineEditBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Length_Defaults_To_Six()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        otpLineEdit.Length.ShouldBe(6);
    }

    [Fact]
    public void Length_Clamps_To_One_And_Truncates_Text()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text   = "123456",
            Length = 0
        };

        otpLineEdit.Length.ShouldBe(1);
        otpLineEdit.Text.ShouldBe("1");
    }

    [Fact]
    public void Text_Uses_Formatter_InputMode_And_Length_Order()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Length    = 4,
            InputMode = OtpLineEditInputMode.Numeric,
            Formatter = value => value.Replace(" ", string.Empty)
        };

        otpLineEdit.Text = " 1 a 2 3 4 5 ";

        otpLineEdit.Text.ShouldBe("1234");
    }

    [Fact]
    public void Clear_Resets_Text()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "123456"
        };

        otpLineEdit.Clear();

        otpLineEdit.Text.ShouldBeNull();
    }

    [Fact]
    public void Completed_Fires_When_Text_Transitions_To_Filled()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Length = 3
        };
        var completedValues = new List<string?>();
        otpLineEdit.Completed += (_, args) => completedValues.Add(args.Text);

        otpLineEdit.Text = "12";
        otpLineEdit.Text = "123";
        otpLineEdit.Text = "123";
        otpLineEdit.Text = "12";
        otpLineEdit.Text = "123";

        completedValues.ShouldBe(["123", "123"]);
    }

    [Fact]
    public void Text_Changes_Update_Filled_And_Empty_PseudoClasses()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Length = 3
        };

        otpLineEdit.Classes.Contains(":empty").ShouldBeTrue();
        otpLineEdit.Classes.Contains(":filled").ShouldBeFalse();

        otpLineEdit.Text = "123";

        otpLineEdit.Classes.Contains(":empty").ShouldBeFalse();
        otpLineEdit.Classes.Contains(":filled").ShouldBeTrue();

        otpLineEdit.Clear();

        otpLineEdit.Classes.Contains(":empty").ShouldBeTrue();
        otpLineEdit.Classes.Contains(":filled").ShouldBeFalse();
    }

    [Fact]
    public void Text_Input_Appends_From_First_Empty_Cell()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        RaiseTextInput(otpLineEdit, "1");
        RaiseTextInput(otpLineEdit, "234567");

        otpLineEdit.Text.ShouldBe("123456");
    }

    [Fact]
    public void Backspace_Removes_Previous_Filled_Cell()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "123"
        };

        RaiseKeyDown(otpLineEdit, Key.Back);

        otpLineEdit.Text.ShouldBe("12");
    }

    [Fact]
    public void Backspace_From_Last_Filled_Cell_Moves_Active_Cell_To_Previous_Filled_Cell()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "654321"
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            RaiseKeyDown(otpLineEdit, Key.Back);
            Dispatcher.UIThread.RunJobs();

            otpLineEdit.Text.ShouldBe("65432");

            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToList();
            cells.Count(cell => cell.IsActive).ShouldBe(1);
            cells[5].IsActive.ShouldBeTrue("deleting the last filled cell keeps the caret on the cleared cell.");
            cells[4].IsActive.ShouldBeFalse();
            CaretOf(cells[5]).IsVisible.ShouldBeTrue();
            CaretOf(cells[4]).IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Delete_Removes_Active_Cell_After_Left_Navigation()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "123"
        };

        RaiseKeyDown(otpLineEdit, Key.Left);
        RaiseKeyDown(otpLineEdit, Key.Delete);

        otpLineEdit.Text.ShouldBe("12");
    }

    [Fact]
    public void Template_Generates_Cells_From_Length_And_Text()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Length = 4,
            Text   = "12"
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToList();

            cells.Count.ShouldBe(4);
            cells.Select(cell => cell.DisplayText)
                 .ToArray()
                 .ShouldBe(["1", "2", null, null]);
        });
    }

    [Fact]
    public void Template_Uses_OtpTextBox_As_Cell_Text_Host()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            var textBox = otpLineEdit.GetVisualDescendants()
                                     .OfType<AvaloniaTextBox>()
                                     .First(item => item.Name == "PART_TextBox");

            textBox.GetType().Name.ShouldBe("OtpTextBox");
        });
    }

    [Fact]
    public void OtpTextBox_Template_Does_Not_Use_Grid_Panels()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            var textBox = otpLineEdit.GetVisualDescendants()
                                     .OfType<AvaloniaTextBox>()
                                     .First(item => item.Name == "PART_TextBox");
            var templateGrids = textBox.GetVisualDescendants()
                                       .OfType<AvaloniaGrid>()
                                       .Where(grid => ReferenceEquals(grid.TemplatedParent, textBox))
                                       .ToList();

            templateGrids.ShouldBeEmpty("the OTP text host template should avoid heavyweight Grid panels for simple overlay/centering.");
        });
    }

    [Fact]
    public void Template_Renders_Cells_Without_Outer_AddOn_Decorated_Box()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.GetVisualDescendants()
                       .OfType<AddOnDecoratedBox>()
                       .ShouldBeEmpty();
        });
    }

    [Fact]
    public void Disabled_Cells_Use_Disabled_Input_Surface()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            IsEnabled = false
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var disabledBackground = GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainerDisabled);
            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToList();

            cells.ShouldAllBe(cell => !cell.IsEffectivelyEnabled);
            foreach (var cell in cells)
            {
                var frame = cell.GetVisualDescendants()
                                .OfType<PixelAlignedBorder>()
                                .Single(item => item.Name == "PART_Frame");

                BrushShouldHaveSameColor(frame.Background, disabledBackground);
            }
        });
    }

    [Fact]
    public void StyleVariants_Use_LineEdit_Input_Surface_Visuals()
    {
        AssertCellSurface(
            InputControlStyleVariant.Outlined,
            GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainer),
            GetThemeResource<IBrush>(SharedTokenKind.ColorBorder),
            new Thickness(1),
            GetThemeResource<CornerRadius>(SharedTokenKind.BorderRadius));

        AssertCellSurface(
            InputControlStyleVariant.Filled,
            GetThemeResource<IBrush>(SharedTokenKind.ColorFillTertiary),
            Brushes.Transparent,
            new Thickness(1),
            GetThemeResource<CornerRadius>(SharedTokenKind.BorderRadius));

        AssertCellSurface(
            InputControlStyleVariant.Borderless,
            Brushes.Transparent,
            Brushes.Transparent,
            new Thickness(0),
            GetThemeResource<CornerRadius>(SharedTokenKind.BorderRadius));

        AssertCellSurface(
            InputControlStyleVariant.Underlined,
            GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainer),
            GetThemeResource<IBrush>(SharedTokenKind.ColorBorder),
            new Thickness(0, 0, 0, 1),
            new CornerRadius(0));
    }

    [Fact]
    public void Status_Visuals_Follow_LineEdit_Variant_Priority()
    {
        AssertCellSurface(
            InputControlStyleVariant.Filled,
            GetThemeResource<IBrush>(SharedTokenKind.ColorErrorBg),
            Brushes.Transparent,
            new Thickness(1),
            GetThemeResource<CornerRadius>(SharedTokenKind.BorderRadius),
            InputControlStatus.Error,
            GetThemeResource<IBrush>(SharedTokenKind.ColorErrorText));

        AssertCellSurface(
            InputControlStyleVariant.Underlined,
            GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainer),
            GetThemeResource<IBrush>(SharedTokenKind.ColorWarning),
            new Thickness(0, 0, 0, 1),
            new CornerRadius(0),
            InputControlStatus.Warning);
    }

    [Fact]
    public void Filled_Cell_Only_Shows_Active_Border_When_Focused()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            StyleVariant     = InputControlStyleVariant.Filled,
            IsMotionEnabled  = false
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var activeBorder = GetThemeResource<IBrush>(SharedTokenKind.ColorPrimary);
            var activeBg     = GetThemeResource<IBrush>(SharedTokenKind.ColorBgContainer);
            var activeCell   = otpLineEdit.GetVisualDescendants()
                                          .OfType<OtpLineEditCell>()
                                          .Single(cell => cell.IsActive);
            var frame = activeCell.GetVisualDescendants()
                                  .OfType<PixelAlignedBorder>()
                                  .Single(item => item.Name == "PART_Frame");

            activeCell.Classes.Contains(":cell-active").ShouldBeTrue();
            BrushShouldHaveSameColor(activeCell.Background, activeBg);
            BrushShouldHaveSameColor(frame.Background, activeBg);
            BrushShouldHaveSameColor(frame.BorderBrush, activeBorder);
        });
    }

    [Fact]
    public void Filled_Cell_Hover_Keeps_Transparent_Border()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            StyleVariant    = InputControlStyleVariant.Filled,
            IsMotionEnabled = false
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var hoverBackground = GetThemeResource<IBrush>(SharedTokenKind.ColorFillSecondary);
            var cell            = otpLineEdit.GetVisualDescendants()
                                             .OfType<OtpLineEditCell>()
                                             .First();
            var frame = cell.GetVisualDescendants()
                            .OfType<PixelAlignedBorder>()
                            .Single(item => item.Name == "PART_Frame");

            SetPseudoClass(cell, StdPseudoClass.PointerOver, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(frame.Background, hoverBackground);
            BrushShouldHaveSameColor(frame.BorderBrush, Brushes.Transparent);
        });
    }

    [Fact]
    public void Cell_Pressed_PseudoClass_Is_Set_By_Pointer_Press()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            var cell = otpLineEdit.GetVisualDescendants()
                                  .OfType<OtpLineEditCell>()
                                  .First();

            RaisePointerPressed(cell);

            cell.Classes.Contains(StdPseudoClass.Pressed)
                .ShouldBeTrue("OTP cells must expose the standard pressed pseudo-class so theme pressed visuals are reachable.");
        });
    }

    [Fact]
    public void Cell_Pressed_Surface_Uses_Active_Border_Color_After_Hover()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            IsMotionEnabled = false
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var normalBorder = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);
            var hoverBorder  = GetThemeResource<IBrush>(SharedTokenKind.ColorPrimaryHover);
            var activeBorder = GetThemeResource<IBrush>(SharedTokenKind.ColorPrimary);
            var cell         = otpLineEdit.GetVisualDescendants()
                                          .OfType<OtpLineEditCell>()
                                          .First();
            var frame = cell.GetVisualDescendants()
                            .OfType<PixelAlignedBorder>()
                            .Single(item => item.Name == "PART_Frame");

            BrushShouldHaveSameColor(frame.BorderBrush, normalBorder);

            SetPseudoClass(cell, StdPseudoClass.PointerOver, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(frame.BorderBrush, hoverBorder);

            SetPseudoClass(cell, StdPseudoClass.Pressed, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(frame.BorderBrush, activeBorder);
        });
    }

    [Fact]
    public void Cell_Pressed_Status_Surface_Does_Not_Fall_Back_To_Primary_Color()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            IsMotionEnabled = false,
            Status          = InputControlStatus.Error
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var errorBorder = GetThemeResource<IBrush>(SharedTokenKind.ColorError);
            var cell        = otpLineEdit.GetVisualDescendants()
                                         .OfType<OtpLineEditCell>()
                                         .First();
            var frame = cell.GetVisualDescendants()
                            .OfType<PixelAlignedBorder>()
                            .Single(item => item.Name == "PART_Frame");

            SetPseudoClass(cell, StdPseudoClass.PointerOver, true);
            SetPseudoClass(cell, StdPseudoClass.Pressed, true);
            Dispatcher.UIThread.RunJobs();

            BrushShouldHaveSameColor(frame.BorderBrush, errorBorder);
        });
    }

    [Fact]
    public void Cell_Surface_Transitions_Follow_Owner_Motion_Setting()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            IsMotionEnabled = true
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var cell = otpLineEdit.GetVisualDescendants()
                                  .OfType<OtpLineEditCell>()
                                  .First();
            var transitionProperties = cell.Transitions.ShouldNotBeNull()
                                           .OfType<SolidColorBrushTransition>()
                                           .Select(transition => transition.Property)
                                           .ToList();

            transitionProperties.ShouldContain(OtpLineEditCell.BorderBrushProperty);
            transitionProperties.ShouldContain(OtpLineEditCell.BackgroundProperty);
        });
    }

    [Fact]
    public void Cells_Are_Not_Active_While_OtpLineEdit_Is_Not_Focused()
    {
        var otherFocusTarget = new Button();
        var otpLineEdit      = new AtomUI.Desktop.Controls.OtpLineEdit();
        var content          = new StackPanel
        {
            Children =
            {
                otherFocusTarget,
                otpLineEdit
            }
        };

        ShowInWindow(content, () =>
        {
            otherFocusTarget.Focus();
            Dispatcher.UIThread.RunJobs();

            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToList();
            cells.ShouldAllBe(cell => !cell.IsActive);
        });
    }

    [Fact]
    public void Active_Cell_Uses_Lightweight_TextBox_Caret_Host()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var activeCell = otpLineEdit.GetVisualDescendants()
                                        .OfType<OtpLineEditCell>()
                                        .Single(cell => cell.IsActive);
            var textBox = activeCell.GetVisualDescendants()
                                    .OfType<AvaloniaTextBox>()
                                    .SingleOrDefault(item => item.Name == "PART_TextBox");

            textBox.ShouldNotBeNull("the active OTP cell needs a real TextBox/TextPresenter so Avalonia can render a caret.");
            textBox.ShouldNotBeAssignableTo<AtomUI.Desktop.Controls.TextBox>(
                "the OTP cell already owns its frame; nesting a full AtomUI TextBox would duplicate input chrome.");
            textBox.IsReadOnly.ShouldBeTrue();
            textBox.Focusable.ShouldBeFalse("the root OtpLineEdit owns keyboard focus; cell TextBox is only a caret host.");
            textBox.IsFocused.ShouldBeFalse();
            TopLevel.GetTopLevel(otpLineEdit)?.FocusManager?.GetFocusedElement()
                    .ShouldBe(otpLineEdit);

            var textPresenter = textBox.GetVisualDescendants()
                                       .OfType<TextPresenter>()
                                       .SingleOrDefault(item => item.Name == "PART_TextPresenter");

            textPresenter.ShouldNotBeNull("a TextBox without a TextPresenter can hold text but cannot render a caret.");
            textPresenter.CaretBrush.ShouldNotBeNull();
            activeCell.GetVisualDescendants()
                      .OfType<TextBlock>()
                      .Any(item => item.Name == "PART_Text")
                      .ShouldBeFalse("cell text should be hosted by TextBox instead of the old TextBlock presenter.");
        });
    }

    [Fact]
    public void Active_Cell_Caret_Host_Shows_Ibeam_Mouse_Cursor()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "654321"
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var activeTextBox = GetActiveCellTextBox(otpLineEdit);
            var textPresenter = activeTextBox
                .GetVisualDescendants()
                .OfType<TextPresenter>()
                .Single(item => item.Name == "PART_TextPresenter");
            var visualChain = string.Join(
                " -> ",
                textPresenter.GetVisualAncestors().Select(item => $"{item.GetType().Name}:{(item as Control)?.Name}"));
            var scrollViewer = textPresenter.GetVisualAncestors()
                                            .OfType<AvaloniaScrollViewer>()
                                            .SingleOrDefault();

            scrollViewer.ShouldNotBeNull(visualChain);
            activeTextBox.Cursor.ShouldNotBeNull();
            activeTextBox.Cursor.ToString().ShouldContain("Ibeam");
            scrollViewer.Cursor.ShouldNotBeNull();
            scrollViewer.Cursor.ToString().ShouldContain("Ibeam");
        });
    }

    [Fact]
    public void Current_Input_Target_Cell_Shows_Ibeam_Mouse_Cursor_Before_Focus()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "12"
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.IsKeyboardFocusWithin.ShouldBeFalse();

            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToList();
            var inputTargetTextBox = cells[2].GetVisualDescendants()
                                             .OfType<AvaloniaTextBox>()
                                             .Single(item => item.Name == "PART_TextBox");
            var inputTargetScrollViewer = inputTargetTextBox.GetVisualDescendants()
                                                            .OfType<AvaloniaScrollViewer>()
                                                            .Single(item => item.Name == "PART_ScrollViewer");
            var filledTextBox = cells[0].GetVisualDescendants()
                                        .OfType<AvaloniaTextBox>()
                                        .Single(item => item.Name == "PART_TextBox");

            inputTargetTextBox.Cursor.ShouldNotBeNull();
            inputTargetTextBox.Cursor.ToString().ShouldContain("Ibeam");
            inputTargetScrollViewer.Cursor.ShouldNotBeNull();
            inputTargetScrollViewer.Cursor.ToString().ShouldContain("Ibeam");
            filledTextBox.Cursor.ShouldNotBeNull();
            filledTextBox.Cursor.ToString().ShouldContain("Arrow");
        });
    }

    [Fact]
    public void Inactive_Cell_Caret_Host_Keeps_Arrow_Mouse_Cursor()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "654321"
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var inactiveCell = otpLineEdit.GetVisualDescendants()
                                          .OfType<OtpLineEditCell>()
                                          .First(cell => !cell.IsActive);
            var inactiveTextBox = inactiveCell.GetVisualDescendants()
                                              .OfType<AvaloniaTextBox>()
                                              .Single(item => item.Name == "PART_TextBox");
            var scrollViewer = inactiveTextBox.GetVisualDescendants()
                                              .OfType<AvaloniaScrollViewer>()
                                              .Single(item => item.Name == "PART_ScrollViewer");

            inactiveTextBox.Cursor.ShouldNotBeNull();
            inactiveTextBox.Cursor.ToString().ShouldContain("Arrow");
            scrollViewer.Cursor.ShouldNotBeNull();
            scrollViewer.Cursor.ToString().ShouldContain("Arrow");
        });
    }

    [Fact]
    public void Text_Input_To_Last_Cell_Keeps_Caret_After_Display_Text()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            foreach (var text in "123244".Select(value => value.ToString()))
            {
                RaiseTextInput(GetActiveCellTextBox(otpLineEdit), text);
                Dispatcher.UIThread.RunJobs();
            }

            otpLineEdit.Text.ShouldBe("123244");

            var textBox = GetActiveCellTextBox(otpLineEdit);
            textBox.Text.ShouldBe("4");
            textBox.CaretIndex.ShouldBe(1);
            textBox.SelectionStart.ShouldBe(1);
            textBox.SelectionEnd.ShouldBe(1);

            var textPresenter = textBox.GetVisualDescendants()
                                       .OfType<TextPresenter>()
                                       .Single(item => item.Name == "PART_TextPresenter");
            textPresenter.CaretIndex.ShouldBe(1);
            textPresenter.SelectionStart.ShouldBe(1);
            textPresenter.SelectionEnd.ShouldBe(1);
            textPresenter.TextLayout.HitTestTextPosition(1).Position.X.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public void Cell_TextBox_Bubbled_Input_Routes_To_OtpLineEdit()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var textBox = GetActiveCellTextBox(otpLineEdit);

            RaiseTextInput(textBox, "1");
            Dispatcher.UIThread.RunJobs();
            otpLineEdit.Text.ShouldBe("1");

            textBox = GetActiveCellTextBox(otpLineEdit);

            RaiseKeyDown(textBox, Key.Back);
            Dispatcher.UIThread.RunJobs();
            otpLineEdit.Text.ShouldBeNull();
        });
    }

    [Fact]
    public void Text_Input_Keeps_Owner_Focused_And_Only_One_Active_Cell_TextBox()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit();

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();
            AssertSingleActiveCellTextBoxHost(otpLineEdit);

            foreach (var text in "22332".Select(value => value.ToString()))
            {
                var textBox = GetActiveCellTextBox(otpLineEdit);
                TopLevel.GetTopLevel(otpLineEdit)?.FocusManager?.GetFocusedElement()
                        .ShouldBe(otpLineEdit);

                RaiseTextInput(textBox, text);
                Dispatcher.UIThread.RunJobs();

                AssertSingleActiveCellTextBoxHost(otpLineEdit);
            }
        });
    }

    [Fact]
    public void Active_Cell_Caret_Does_Not_Bubble_BringIntoView_To_Parent()
    {
        var otpLineEdit           = new AtomUI.Desktop.Controls.OtpLineEdit();
        var bringIntoViewRequests = 0;
        var content               = new StackPanel
        {
            Children =
            {
                otpLineEdit
            }
        };
        content.AddHandler(
            Control.RequestBringIntoViewEvent,
            (_, _) => bringIntoViewRequests++);

        ShowInWindow(content, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var textPresenter = GetActiveCellTextBox(otpLineEdit)
                .GetVisualDescendants()
                .OfType<TextPresenter>()
                .Single(item => item.Name == "PART_TextPresenter");
            textPresenter.BringIntoView();

            bringIntoViewRequests.ShouldBe(0);
        });
    }

    [Fact]
    public void Active_Empty_Cell_Hides_Placeholder_To_Avoid_Caret_Overlap()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            PlaceholderText = "000000"
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToList();
            var activeCellTextBox = cells.Single(cell => cell.IsActive)
                                         .GetVisualDescendants()
                                         .OfType<AvaloniaTextBox>()
                                         .Single(item => item.Name == "PART_TextBox");
            var inactiveCellTextBox = cells.First(cell => !cell.IsActive)
                                           .GetVisualDescendants()
                                           .OfType<AvaloniaTextBox>()
                                           .Single(item => item.Name == "PART_TextBox");

            activeCellTextBox.PlaceholderText.ShouldBeNull();
            inactiveCellTextBox.PlaceholderText.ShouldBe("0");
        });
    }

    [Fact]
    public void Masked_Cells_Do_Not_Change_Text()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text     = "12",
            IsMasked = true,
            MaskChar = '*'
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .Take(2)
                                   .ToList();

            cells.Select(cell => cell.DisplayText)
                 .ToArray()
                 .ShouldBe(["*", "*"]);
            otpLineEdit.Text.ShouldBe("12");
        });
    }

    [Fact]
    public void Focused_Active_Cell_Shows_Caret_And_Others_Hide()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Length = 3
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            Dispatcher.UIThread.RunJobs();

            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToArray();
            cells.Length.ShouldBe(3);
            CaretOf(cells[0]).IsVisible.ShouldBeTrue();
            CaretOf(cells[1]).IsVisible.ShouldBeFalse();
            CaretOf(cells[2]).IsVisible.ShouldBeFalse();

            otpLineEdit.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();
            foreach (var cell in cells)
            {
                CaretOf(cell).IsVisible.ShouldBeFalse();
            }
        });
    }

    [Fact]
    public void Backspace_Keeps_Caret_On_The_Cleared_Cell()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Length = 3
        };

        ShowInWindow(otpLineEdit, () =>
        {
            otpLineEdit.Focus();
            otpLineEdit.Text = "123";
            Dispatcher.UIThread.RunJobs();

            otpLineEdit.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.Back
            });
            Dispatcher.UIThread.RunJobs();

            otpLineEdit.Text.ShouldBe("23");
            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToArray();
            CaretOf(cells[0]).IsVisible.ShouldBeTrue("caret must stay on the cell that was just cleared");
            CaretOf(cells[1]).IsVisible.ShouldBeFalse();
            CaretOf(cells[2]).IsVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Occupied_Active_Cell_Shows_Caret_After_The_Character()
    {
        var cell = new OtpLineEditCell
        {
            IsActive = true,
            DisplayText = "7"
        };

        ShowInWindow(cell, () =>
        {
            var caret = CaretOf(cell);
            caret.IsVisible.ShouldBeTrue();
            var translate = caret.RenderTransform.ShouldBeOfType<TranslateTransform>();
            translate.X.ShouldBe(cell.FontSize * 0.30, 0.001);

            cell.DisplayText = null;
            Dispatcher.UIThread.RunJobs();
            CaretOf(cell).IsVisible.ShouldBeTrue();
        });
    }

    private static Rectangle CaretOf(OtpLineEditCell cell)
    {
        return cell.GetVisualDescendants()
                   .OfType<Rectangle>()
                   .Single(rectangle => rectangle.Name == "PART_Caret");
    }

    [Fact]
    public void Separators_Render_Between_Cells_With_Visible_Content()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Separator = "/"
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var separatorPresenters = otpLineEdit.GetVisualDescendants()
                                                 .OfType<ContentPresenter>()
                                                 .Where(presenter => Equals(presenter.Content, "/"))
                                                 .ToList();

            separatorPresenters.Count.ShouldBe(5);
            separatorPresenters.ShouldAllBe(presenter => presenter.IsVisible);
            separatorPresenters.ShouldAllBe(
                presenter => presenter.Bounds.Width > 0,
                "separator presenters must reserve visible width between OTP cells.");
        });
    }

    [Fact]
    public void SeparatorTemplate_Renders_Context_Content_Between_Cells()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Separator         = "—",
            SeparatorTemplate = new FuncDataTemplate<OtpLineEditSeparatorContext>(
                (context, _) => new TextBlock
                {
                    Text = $"{context?.Content}{context?.SeparatorIndex}"
                })
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var separators = otpLineEdit.GetVisualDescendants()
                                        .OfType<TextBlock>()
                                        .Where(textBlock => textBlock.Text?.StartsWith('—') == true)
                                        .ToList();

            separators.Select(textBlock => textBlock.Text)
                      .ToArray()
                      .ShouldBe(["—1", "—2", "—3", "—4", "—5"]);
            separators.ShouldAllBe(textBlock => textBlock.IsVisible);
            separators.ShouldAllBe(
                textBlock => textBlock.Bounds.Width > 0,
                "separator templates must render visible content between OTP cells.");
        });
    }

    [Fact]
    public void Clear_Button_Clears_Text()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text         = "12",
            IsAllowClear = true
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var clearButton = otpLineEdit.GetVisualDescendants()
                                         .OfType<InputClearIconButton>()
                                         .Single(item => item.Name == "PART_ClearButton");

            clearButton.IsVisible.ShouldBeTrue();
            clearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, clearButton));
            Dispatcher.UIThread.RunJobs();

            otpLineEdit.Text.ShouldBeNull();
        });
    }

    [Fact]
    public void Native_Validation_Error_Projects_To_Cells()
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            Text = "12"
        };
        var validationError = new InvalidOperationException("native");

        ShowInWindow(otpLineEdit, () =>
        {
            DataValidationErrors.SetError(otpLineEdit, validationError);
            Dispatcher.UIThread.RunJobs();

            var cells = otpLineEdit.GetVisualDescendants()
                                   .OfType<OtpLineEditCell>()
                                   .ToList();

            cells.ShouldAllBe(cell => cell.EffectiveStatus == InputControlStatus.Error);
        });
    }

    private static void RaiseTextInput(InputElement target, string text)
    {
        target.RaiseEvent(new TextInputEventArgs
        {
            RoutedEvent = InputElement.TextInputEvent,
            Source      = target,
            Text        = text
        });
    }

    private static void RaisePointerPressed(Control source)
    {
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
    }

    private static void SetPseudoClass(Control control, string pseudoClass, bool value)
    {
        ((IPseudoClasses)control.Classes).Set(pseudoClass, value);
    }

    private static AvaloniaTextBox GetActiveCellTextBox(AtomUI.Desktop.Controls.OtpLineEdit otpLineEdit)
    {
        var activeCell = otpLineEdit.GetVisualDescendants()
                                    .OfType<OtpLineEditCell>()
                                    .Single(cell => cell.IsActive);

        return activeCell.GetVisualDescendants()
                         .OfType<AvaloniaTextBox>()
                         .Single(item => item.Name == "PART_TextBox");
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void AssertCellSurface(
        InputControlStyleVariant styleVariant,
        IBrush? expectedBackground,
        IBrush? expectedBorderBrush,
        Thickness expectedBorderThickness,
        CornerRadius expectedCornerRadius,
        InputControlStatus status = InputControlStatus.Default,
        IBrush? expectedForeground = null)
    {
        var otpLineEdit = new AtomUI.Desktop.Controls.OtpLineEdit
        {
            StyleVariant = styleVariant,
            Status       = status
        };

        ShowInWindow(otpLineEdit, () =>
        {
            var cell = otpLineEdit.GetVisualDescendants()
                                  .OfType<OtpLineEditCell>()
                                  .First();
            var frame = cell.GetVisualDescendants()
                            .OfType<PixelAlignedBorder>()
                            .Single(item => item.Name == "PART_Frame");

            BrushShouldHaveSameColor(frame.Background, expectedBackground);
            BrushShouldHaveSameColor(frame.BorderBrush, expectedBorderBrush);
            frame.BorderThickness.ShouldBe(expectedBorderThickness);
            frame.CornerRadius.ShouldBe(expectedCornerRadius);

            if (expectedForeground is not null)
            {
                var textBox = cell.GetVisualDescendants()
                                  .OfType<AvaloniaTextBox>()
                                  .Single(item => item.Name == "PART_TextBox");

                BrushShouldHaveSameColor(textBox.Foreground, expectedForeground);
            }
        });
    }

    private static void BrushShouldHaveSameColor(IBrush? actual, IBrush? expected)
    {
        GetSolidBrushColor(actual).ShouldBe(GetSolidBrushColor(expected));
    }

    private static Color? GetSolidBrushColor(IBrush? brush)
    {
        return brush switch
        {
            ISolidColorBrush solidColorBrush => solidColorBrush.Color,
            _ => null
        };
    }

    private static void AssertSingleActiveCellTextBoxHost(AtomUI.Desktop.Controls.OtpLineEdit otpLineEdit)
    {
        var cells = otpLineEdit.GetVisualDescendants()
                               .OfType<OtpLineEditCell>()
                               .ToList();
        var textBoxes = cells.Select(cell => cell.GetVisualDescendants()
                                                 .OfType<AvaloniaTextBox>()
                                                 .Single(item => item.Name == "PART_TextBox"))
                             .ToList();
        var activeCell    = cells.Single(cell => cell.IsActive);
        var activeTextBox = activeCell.GetVisualDescendants()
                                      .OfType<AvaloniaTextBox>()
                                      .Single(item => item.Name == "PART_TextBox");

        textBoxes.Count.ShouldBe(otpLineEdit.Length);
        cells.Count(cell => cell.IsActive).ShouldBe(1);
        textBoxes.ShouldAllBe(textBox => !textBox.Focusable);
        textBoxes.ShouldAllBe(textBox => !textBox.IsFocused);
        TopLevel.GetTopLevel(otpLineEdit)?.FocusManager?.GetFocusedElement()
                .ShouldBe(otpLineEdit);

        foreach (var inactiveTextBox in textBoxes.Where(textBox => !ReferenceEquals(textBox, activeTextBox)))
        {
            inactiveTextBox.SelectionStart.ShouldBe(inactiveTextBox.SelectionEnd);
        }
    }

    private static void RaiseKeyDown(InputElement target, Key key)
    {
        target.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent = InputElement.KeyDownEvent,
            Source      = target,
            Key         = key,
            PhysicalKey = key switch
            {
                Key.Back   => PhysicalKey.Backspace,
                Key.Delete => PhysicalKey.Delete,
                Key.Left   => PhysicalKey.ArrowLeft,
                _          => PhysicalKey.None
            }
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 160,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }
}
