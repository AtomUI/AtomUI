using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateInputScenarios()
    {
        return
        [
            new PerfScenario("TextBox.Default", _ => CreateTextBox()),
            new PerfScenario("TextBox.AllowClear.Empty", _ => CreateTextBox(isAllowClear: true)),
            new PerfScenario("TextBox.AllowClear.Text", _ => CreateTextBox(text: "clear text", isAllowClear: true)),
            new PerfScenario("TextBox.Reveal", _ => CreateTextBox(text: "secret", isEnableRevealButton: true, passwordChar: '*')),
            new PerfScenario("TextBox.Count", _ => CreateTextBox(text: "count text", isShowCount: true, maxLength: 100)),
            new PerfScenario("TextBox.InnerRight", _ => CreateTextBox(innerRightContent: new Avalonia.Controls.TextBlock { Text = "kg" })),
            new PerfScenario("LineEdit.Default", _ => CreateInputLineEdit()),
            new PerfScenario("LineEdit.AllowClear", _ => CreateInputLineEdit(text: "clear text", isAllowClear: true)),
            new PerfScenario("LineEdit.Reveal", _ => CreateInputLineEdit(text: "secret", isEnableRevealButton: true, passwordChar: '*')),
            new PerfScenario("LineEdit.Count", _ => CreateInputLineEdit(text: "count text", isShowCount: true, maxLength: 100)),
            new PerfScenario("LineEdit.InnerLeftRight", _ => CreateInputLineEdit(
                innerLeftContent: new UserOutlined(),
                innerRightContent: new InfoCircleOutlined())),
            new PerfScenario("LineEdit.OuterAddOns", _ => CreateInputLineEdit(
                leftAddOn: new Avalonia.Controls.TextBlock { Text = "http://" },
                rightAddOn: new Avalonia.Controls.TextBlock { Text = ".com" })),
            new PerfScenario("SearchEdit.Default", _ => CreateSearchEdit()),
            new PerfScenario("SearchEdit.TextButton", _ => CreateSearchEdit(searchButtonText: "Search")),
            new PerfScenario("SearchEdit.Primary", _ => CreateSearchEdit(searchButtonText: "Search", searchButtonStyle: SearchButtonType.Primary)),
            new PerfScenario("SearchEdit.Loading", _ => CreateSearchEdit(searchButtonStyle: SearchButtonType.Primary, isOperating: true)),
            new PerfScenario("SearchEdit.AllowClear", _ => CreateSearchEdit(text: "search", isAllowClear: true)),
            new PerfScenario("SearchEdit.InnerRight", _ => CreateSearchEdit(innerRightContent: new AudioOutlined { Width = 16, Height = 16 })),
            new PerfScenario("SearchEdit.LeftAddOn", _ => CreateSearchEdit(leftAddOn: new Avalonia.Controls.TextBlock { Text = "https://" })),
            new PerfScenario("TextArea.Default", _ => CreateTextArea()),
            new PerfScenario("TextArea.Lines4", _ => CreateTextArea(lines: 4)),
            new PerfScenario("TextArea.AutoSize", _ => CreateTextArea(isAutoSize: true, minLines: 3, maxLines: 6)),
            new PerfScenario("TextArea.Count", _ => CreateTextArea(text: "count text", isShowCount: true, maxLength: 100)),
            new PerfScenario("TextArea.Resizable", _ => CreateTextArea(isResizable: true)),
            new PerfScenario("TextArea.AllowClear", _ => CreateTextArea(text: "clear text", isAllowClear: true)),
            new PerfScenario("TextArea.Status", _ => CreateTextArea(isShowCount: true, maxLength: 100, status: InputControlStatus.Error)),
            new PerfScenario("Input.GalleryShape", _ => CreateInputGalleryShape())
        ];
    }

    private static AtomUI.Desktop.Controls.TextBox CreateTextBox(
        string? text = null,
        bool isAllowClear = false,
        bool isEnableRevealButton = false,
        bool isShowCount = false,
        int maxLength = 0,
        char passwordChar = '\0',
        object? innerRightContent = null)
    {
        var textBox = new AtomUI.Desktop.Controls.TextBox
        {
            Width                = 260,
            Text                 = text,
            IsAllowClear         = isAllowClear,
            IsEnableRevealButton = isEnableRevealButton,
            IsShowCount          = isShowCount,
            PasswordChar         = passwordChar,
            InnerRightContent    = innerRightContent
        };

        if (maxLength > 0)
        {
            textBox.MaxLength = maxLength;
        }

        return textBox;
    }

    private static LineEdit CreateInputLineEdit(
        string? text = null,
        bool isAllowClear = false,
        bool isEnableRevealButton = false,
        bool isShowCount = false,
        int maxLength = 0,
        char passwordChar = '\0',
        object? innerLeftContent = null,
        object? innerRightContent = null,
        object? leftAddOn = null,
        object? rightAddOn = null)
    {
        var lineEdit = new LineEdit
        {
            Width                = 260,
            Text                 = text,
            IsAllowClear         = isAllowClear,
            IsEnableRevealButton = isEnableRevealButton,
            IsShowCount          = isShowCount,
            PasswordChar         = passwordChar,
            InnerLeftContent     = innerLeftContent,
            InnerRightContent    = innerRightContent,
            LeftAddOn            = leftAddOn,
            RightAddOn           = rightAddOn
        };

        if (maxLength > 0)
        {
            lineEdit.MaxLength = maxLength;
        }

        return lineEdit;
    }

    private static SearchEdit CreateSearchEdit(
        string? text = null,
        bool isAllowClear = false,
        SearchButtonType searchButtonStyle = SearchButtonType.Default,
        string? searchButtonText = null,
        bool isOperating = false,
        object? innerRightContent = null,
        object? leftAddOn = null)
    {
        return new SearchEdit
        {
            Width             = 260,
            Text              = text,
            IsAllowClear      = isAllowClear,
            SearchButtonStyle = searchButtonStyle,
            SearchButtonText  = searchButtonText,
            IsOperating       = isOperating,
            InnerRightContent = innerRightContent,
            LeftAddOn         = leftAddOn
        };
    }

    private static TextArea CreateTextArea(
        string? text = null,
        int lines = 2,
        bool isAutoSize = false,
        int minLines = 1,
        int maxLines = 0,
        bool isShowCount = false,
        int maxLength = 0,
        bool isResizable = false,
        bool isAllowClear = false,
        InputControlStatus status = InputControlStatus.Default)
    {
        var textArea = new TextArea
        {
            Width        = 260,
            Text         = text,
            Lines        = lines,
            IsAutoSize   = isAutoSize,
            MinLines     = minLines,
            MaxLines     = maxLines,
            IsShowCount  = isShowCount,
            IsResizable  = isResizable,
            IsAllowClear = isAllowClear,
            Status       = status
        };

        if (maxLength > 0)
        {
            textArea.MaxLength = maxLength;
        }

        return textArea;
    }

    private static Control CreateInputGalleryShape()
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing     = 8
        };

        for (var i = 0; i < 52; i++)
        {
            panel.Children.Add(CreateInputLineEdit(
                text: i % 4 == 0 ? "mysite" : null,
                isAllowClear: i % 13 == 0,
                isEnableRevealButton: i % 17 == 0,
                isShowCount: i % 19 == 0,
                maxLength: 100,
                passwordChar: i % 17 == 0 ? '*' : '\0',
                innerLeftContent: i % 5 == 0 ? new UserOutlined() : null,
                innerRightContent: i % 7 == 0 ? new InfoCircleOutlined() : null,
                leftAddOn: i % 6 == 0 ? new Avalonia.Controls.TextBlock { Text = "http://" } : null,
                rightAddOn: i % 6 == 0 ? new Avalonia.Controls.TextBlock { Text = ".com" } : null));
        }

        for (var i = 0; i < 30; i++)
        {
            panel.Children.Add(CreateSearchEdit(
                text: i % 5 == 0 ? "search" : null,
                isAllowClear: i % 9 == 0,
                searchButtonStyle: i % 3 == 0 ? SearchButtonType.Primary : SearchButtonType.Default,
                searchButtonText: i % 2 == 0 ? "Search" : null,
                isOperating: i % 11 == 0,
                innerRightContent: i % 10 == 0 ? new AudioOutlined { Width = 16, Height = 16 } : null,
                leftAddOn: i % 4 == 0 ? new Avalonia.Controls.TextBlock { Text = "https://" } : null));
        }

        for (var i = 0; i < 11; i++)
        {
            panel.Children.Add(CreateTextArea(
                text: i % 3 == 0 ? "textarea" : null,
                lines: i % 4 == 0 ? 4 : 2,
                isAutoSize: i % 5 == 0,
                minLines: 1,
                maxLines: 6,
                isShowCount: i % 2 == 0,
                maxLength: 100,
                isResizable: i % 3 == 0,
                isAllowClear: i % 7 == 0,
                status: i % 5 == 0 ? InputControlStatus.Error : InputControlStatus.Default));
        }

        return panel;
    }
}
