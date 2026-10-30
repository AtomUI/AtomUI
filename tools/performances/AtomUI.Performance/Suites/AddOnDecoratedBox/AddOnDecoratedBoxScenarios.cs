using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Layout;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static IReadOnlyList<PerfScenario> CreateAddOnScenarios()
    {
        return
        [
            new PerfScenario("LineEdit.Default", _ => CreateLineEdit()),
            new PerfScenario("LineEdit.AllowClear", _ => CreateLineEdit(text: "clear text", isAllowClear: true)),
            new PerfScenario("LineEdit.Reveal", _ => CreateLineEdit(text: "secret", isEnableRevealButton: true, passwordChar: '*')),
            new PerfScenario("LineEdit.Count", _ => CreateLineEdit(text: "count text", isShowCount: true, maxLength: 100)),
            new PerfScenario("LineEdit.InnerRight", _ => CreateLineEdit(innerRightContent: new Avalonia.Controls.TextBlock { Text = "kg" })),
            new PerfScenario("LineEdit.FormFeedback", _ => CreateLineEdit(formFeedback: new FormValidateFeedback
            {
                ValidateStatus = FormValidateStatus.Error,
                ErrorFeedback  = new Avalonia.Controls.TextBlock { Text = "!" }
            })),
            new PerfScenario("LineEdit.OuterAddOns", _ => CreateLineEdit(
                leftAddOn: new Avalonia.Controls.TextBlock { Text = "http://" },
                rightAddOn: new Avalonia.Controls.TextBlock { Text = ".com" })),
            new PerfScenario("SearchEdit.Default", _ => new SearchEdit
            {
                Width = 260,
                Text = "search"
            }),
            new PerfScenario("DatePicker.Default", _ => new AtomUI.Desktop.Controls.DatePicker
            {
                Width = 260,
                PlaceholderText = "Select date"
            }),
            new PerfScenario("RangeDatePicker.Default", _ => new AtomUI.Desktop.Controls.RangeDatePicker
            {
                Width = 320,
                PlaceholderText = "Select range"
            }),
            new PerfScenario("Select.Default", _ => new Select
            {
                Width = 260,
                PlaceholderText = "Select"
            }),
            new PerfScenario("TreeSelect.Default", _ => new TreeSelect
            {
                Width = 260,
                PlaceholderText = "Tree select"
            }),
            new PerfScenario("Cascader.Default", _ => new Cascader
            {
                Width = 260,
                PlaceholderText = "Cascader"
            }),
            new PerfScenario("ButtonSpinner.Default", _ => new AtomUI.Desktop.Controls.ButtonSpinner
            {
                Width   = 160,
                Content = new Avalonia.Controls.TextBlock { Text = "100" }
            }),
            new PerfScenario("CompactSpace.LineEdit.Horizontal", _ => CreateCompactSpace(Orientation.Horizontal)),
            new PerfScenario("CompactSpace.LineEdit.Vertical", _ => CreateCompactSpace(Orientation.Vertical))
        ];
    }

    private static LineEdit CreateLineEdit(
        string? text = null,
        bool isAllowClear = false,
        bool isEnableRevealButton = false,
        bool isShowCount = false,
        int maxLength = 0,
        char passwordChar = '\0',
        object? innerRightContent = null,
        FormValidateFeedback? formFeedback = null,
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
            InnerRightContent    = innerRightContent,
            LeftAddOn            = leftAddOn,
            RightAddOn           = rightAddOn
        };
        if (formFeedback is not null)
        {
            // FormFeedback 变为私有 setter，通过 IFormItemFeedbackAware 设置（与测试用法一致）
            ((IFormItemFeedbackAware)lineEdit).SetFeedbackControl(formFeedback);
        }

        if (maxLength > 0)
        {
            lineEdit.MaxLength = maxLength;
        }

        return lineEdit;
    }

    private static CompactSpace CreateCompactSpace(Orientation orientation)
    {
        var compactSpace = new CompactSpace
        {
            Orientation = orientation
        };
        compactSpace.Children.Add(CreateLineEdit(text: "first", isAllowClear: true));
        compactSpace.Children.Add(CreateLineEdit(text: "middle"));
        compactSpace.Children.Add(CreateLineEdit(text: "last", isShowCount: true, maxLength: 50));
        return compactSpace;
    }
}
