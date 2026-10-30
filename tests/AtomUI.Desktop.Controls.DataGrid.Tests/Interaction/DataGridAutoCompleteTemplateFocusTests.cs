using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.DataGrid.Interaction;

public class DataGridAutoCompleteTemplateFocusTests
{
    static DataGridAutoCompleteTemplateFocusTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(AutoCompleteKind.Default)]
    [InlineData(AutoCompleteKind.SearchEdit)]
    [InlineData(AutoCompleteKind.TextArea)]
    public void AutoComplete_In_TemplateColumn_Keeps_Text_Input_Focus_After_Click(AutoCompleteKind kind)
    {
        var grid = CreateGrid(kind);
        var window = new Window
        {
            Width   = 520,
            Height  = 280,
            Content = grid
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var autoComplete = grid.GetVisualDescendants()
                                   .OfType<AbstractAutoComplete>()
                                   .Single();
            var textInput = FindTextInput(autoComplete);

            Click(textInput, window);
            Dispatcher.UIThread.RunJobs();

            textInput.IsFocused.ShouldBeTrue();
            autoComplete.IsKeyboardFocusWithin.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void AutoComplete_Outside_DataGrid_Keeps_Text_Input_Focus_After_Click()
    {
        var autoComplete = CreateAutoComplete(AutoCompleteKind.Default);
        var window = new Window
        {
            Width   = 320,
            Height  = 180,
            Content = autoComplete
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var textInput = autoComplete.GetVisualDescendants()
                                        .OfType<AbstractTextInput>()
                                        .Single();

            Click(textInput, window);
            Dispatcher.UIThread.RunJobs();

            textInput.IsFocused.ShouldBeTrue();
            autoComplete.IsKeyboardFocusWithin.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static global::AtomUI.Desktop.Controls.DataGrid CreateGrid(AutoCompleteKind kind)
    {
        var grid = new global::AtomUI.Desktop.Controls.DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = new TestDataGridSource<GridRow>(new List<GridRow>
            {
                new("John Brown", "London")
            }),
            Width  = 460,
            Height = 180
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header  = "Name",
            Binding = new Binding(nameof(GridRow.Name)),
            Width   = new DataGridLength(160)
        });
        grid.Columns.Add(new DataGridTemplateColumn
        {
            Header = "City",
            Width  = new DataGridLength(240),
            CellTemplate = new FuncDataTemplate<GridRow>((_, _) => CreateAutoComplete(kind))
        });

        return grid;
    }

    private static AbstractAutoComplete CreateAutoComplete(AutoCompleteKind kind)
    {
        AbstractAutoComplete autoComplete = kind switch
        {
            AutoCompleteKind.Default => new AutoComplete(),
            AutoCompleteKind.SearchEdit => new AutoCompleteSearchEdit(),
            AutoCompleteKind.TextArea => new AutoCompleteTextArea
            {
                Lines = 1
            },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };

        autoComplete.Width               = 180;
        autoComplete.HorizontalAlignment = HorizontalAlignment.Left;
        autoComplete.PlaceholderText     = "City";
        autoComplete.OptionsSource =
        [
            new AutoCompleteOption { Header = "London", Content = "London" },
            new AutoCompleteOption { Header = "New York", Content = "New York" },
            new AutoCompleteOption { Header = "Sydney", Content = "Sydney" }
        ];
        return autoComplete;
    }

    private static Control FindTextInput(AbstractAutoComplete autoComplete)
    {
        return autoComplete.GetVisualDescendants()
                           .OfType<AbstractTextInput>()
                           .Single();
    }

    private static void Click(Control control, Window window)
    {
        var point = control.TranslatePoint(
            new Point(Math.Min(12, Math.Max(1, control.Bounds.Width / 4)), control.Bounds.Height / 2),
            window);

        point.ShouldNotBeNull();
        window.MouseMove(point.Value);
        window.MouseDown(point.Value, MouseButton.Left);
        window.MouseUp(point.Value, MouseButton.Left);
    }

    public enum AutoCompleteKind
    {
        Default,
        SearchEdit,
        TextArea
    }

    private sealed record GridRow(string Name, string City);
}
