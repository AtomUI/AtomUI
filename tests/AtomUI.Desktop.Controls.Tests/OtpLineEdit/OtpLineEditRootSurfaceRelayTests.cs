using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIOtpLineEdit = AtomUI.Desktop.Controls.OtpLineEdit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.OtpLineEdit;

/// <summary>
/// OtpLineEdit draws each cell as its own <c>InputControlFrame</c>. <c>CellBorderBrush</c> is the
/// cell-specific API; the generic root <c>BorderBrush</c> must also reach the cells, matching the
/// input-family relay, with the cell-specific value taking precedence when both are set.
/// </summary>
public class OtpLineEditRootSurfaceRelayTests
{
    static OtpLineEditRootSurfaceRelayTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Root_BorderBrush_Reaches_Every_Cell()
    {
        var control = CreateOtpLineEdit(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var cells        = FindCells(control);
            var customBorder = new SolidColorBrush(Color.Parse("#f759ab"));

            control.BorderBrush = customBorder;
            Dispatcher.UIThread.RunJobs();

            cells.ShouldNotBeEmpty();
            foreach (var cell in cells)
            {
                GetSolidBrushColor(cell.BorderBrush).ShouldBe(customBorder.Color,
                    "The root BorderBrush must reach every OTP cell frame.");
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Cell_BorderBrush_Takes_Precedence_Over_Root_BorderBrush()
    {
        var control = CreateOtpLineEdit(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var cells        = FindCells(control);
            var rootBorder   = new SolidColorBrush(Color.Parse("#f759ab"));
            var cellBorder   = new SolidColorBrush(Color.Parse("#1677ff"));

            control.BorderBrush     = rootBorder;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(cells[0].BorderBrush).ShouldBe(rootBorder.Color);

            control.CellBorderBrush = cellBorder;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(cells[0].BorderBrush).ShouldBe(cellBorder.Color,
                "CellBorderBrush is the cell-specific override and must win over the generic root BorderBrush.");

            control.CellBorderBrush = null;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(cells[0].BorderBrush).ShouldBe(rootBorder.Color,
                "Clearing CellBorderBrush must fall back to the root BorderBrush, not the theme rest state.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Clearing_The_Root_BorderBrush_Restores_The_Cell_State_Machine()
    {
        var control = CreateOtpLineEdit(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var cells      = FindCells(control);
            var restBorder = GetThemeResource<IBrush>(SharedTokenKind.ColorBorder);
            var custom     = new SolidColorBrush(Color.Parse("#f759ab"));

            control.BorderBrush = custom;
            Dispatcher.UIThread.RunJobs();
            GetSolidBrushColor(cells[0].BorderBrush).ShouldBe(custom.Color);

            control.BorderBrush = null;
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(cells[0].BorderBrush).ShouldBe(GetSolidBrushColor(restBorder),
                "Clearing the root BorderBrush must fall back to the cell theme rest state.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Uncustomized_Cells_Keep_The_Cell_Theme_Rest_State()
    {
        var control = CreateOtpLineEdit(out var window);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            control.BorderBrush.ShouldBeNull(
                "The OTP theme must not set a dead owner-level BorderBrush default; the cell theme owns the rest state.");
            GetSolidBrushColor(FindCells(control)[0].BorderBrush)
                .ShouldBe(GetSolidBrushColor(GetThemeResource<IBrush>(SharedTokenKind.ColorBorder)));
        }
        finally
        {
            window.Close();
        }
    }

    private static AtomUIOtpLineEdit CreateOtpLineEdit(out AvaloniaWindow window)
    {
        var control = new AtomUIOtpLineEdit
        {
            Width           = 320,
            Length          = 4,
            IsMotionEnabled = false
        };

        window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 160,
            Content = control
        };
        return control;
    }

    private static IReadOnlyList<InputControlFrame> FindCells(Control control)
    {
        return control.GetVisualDescendants()
                      .OfType<InputControlFrame>()
                      .ToArray();
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static Color GetSolidBrushColor(IBrush? brush)
    {
        brush.ShouldNotBeNull();
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        return ((ISolidColorBrush)brush!).Color;
    }
}
