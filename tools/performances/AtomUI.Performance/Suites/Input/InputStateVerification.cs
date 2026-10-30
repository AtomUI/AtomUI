using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AtomTextBox = AtomUI.Desktop.Controls.TextBox;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunInputStateVerification()
    {
        var failures = new List<string>();
        VerifyInputSelectionTokenMapping(failures);
        VerifyTextBoxRuntimeSlots(failures);
        VerifyTextAreaRuntimeSlots(failures);
        VerifySearchEditRuntimeSlots(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Input state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Input state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }

        return false;
    }

    private static void VerifyInputSelectionTokenMapping(ICollection<string> failures)
    {
        // 主题算法 API 演进：Calculate(token) → Evaluate(effectiveSeed, previousMap, nextMap)。
        // 与 ThemeCompiler 一致，seed 与 map 使用独立实例（Dark 算法在 previousMap 为 null 时
        // 会先执行 Default 算法并写回 nextMap，若 seed 与 map 同实例会污染 Dark 的基色读取）。
        var defaultToken = new DesignToken();
        new DefaultThemeVariantCalculator().Evaluate(new DesignToken(), null, defaultToken);

        Expect(defaultToken.SelectionBackground == defaultToken.ColorPrimary,
            $"Default SelectionBackground should use ColorPrimary ({defaultToken.ColorPrimary}), actual {defaultToken.SelectionBackground}.",
            failures);
        Expect(defaultToken.SelectionForeground == defaultToken.ColorWhite,
            $"Default SelectionForeground should use ColorWhite ({defaultToken.ColorWhite}), actual {defaultToken.SelectionForeground}.",
            failures);

        var darkToken = new DesignToken();
        new DarkThemeVariantCalculator().Evaluate(new DesignToken(), null, darkToken);

        Expect(darkToken.SelectionBackground == darkToken.ColorPrimary,
            $"Dark SelectionBackground should use ColorPrimary ({darkToken.ColorPrimary}), actual {darkToken.SelectionBackground}.",
            failures);
        Expect(darkToken.SelectionForeground == darkToken.ColorWhite,
            $"Dark SelectionForeground should use ColorWhite ({darkToken.ColorWhite}), actual {darkToken.SelectionForeground}.",
            failures);

        VerifyInputSelectionBrushes(new AtomTextBox
        {
            Width = 260,
            Text  = "selection"
        }, defaultToken.ColorPrimary, Colors.White, "TextBox", failures);
        VerifyInputSelectionBrushes(new LineEdit
        {
            Width = 260,
            Text  = "selection"
        }, defaultToken.ColorPrimary, Colors.White, "LineEdit", failures);
        VerifyInputSelectionBrushes(new SearchEdit
        {
            Width = 260,
            Text  = "selection"
        }, defaultToken.ColorPrimary, Colors.White, "SearchEdit", failures);
        VerifyInputSelectionBrushes(new TextArea
        {
            Width = 260,
            Text  = "selection"
        }, defaultToken.ColorPrimary, Colors.White, "TextArea", failures);
        VerifyInputTextPresenterInvalidatesSelectionTextLayout(failures);
    }

    private static void VerifyInputSelectionBrushes(Avalonia.Controls.TextBox textBox,
                                                    Color expectedSelectionBackground,
                                                    Color expectedSelectionForeground,
                                                    string label,
                                                    ICollection<string> failures)
    {
        using var realized = RealizeControl(textBox);
        RefreshLayout(realized.Window);

        Expect(GetSolidBrushColor(textBox.SelectionBrush) == expectedSelectionBackground,
            $"{label} SelectionBrush should use the Ant Design primary selection token.",
            failures);
        Expect(GetSolidBrushColor(textBox.SelectionForegroundBrush) == expectedSelectionForeground,
            $"{label} SelectionForegroundBrush should use the Ant Design selected-text foreground token.",
            failures);

        var presenter = FindVisualByName<TextPresenter>(textBox, "PART_TextPresenter");
        Expect(presenter != null,
            $"{label} should realize PART_TextPresenter for text selection rendering.",
            failures);
        Expect(presenter is InputTextPresenter,
            $"{label} should use InputTextPresenter to clear Avalonia 12 text run cache for selected text.",
            failures);
        Expect(GetSolidBrushColor(presenter?.SelectionBrush) == expectedSelectionBackground,
            $"{label} PART_TextPresenter SelectionBrush should use the Ant Design primary selection token.",
            failures);
        Expect(GetSolidBrushColor(presenter?.SelectionForegroundBrush) == expectedSelectionForeground,
            $"{label} PART_TextPresenter SelectionForegroundBrush should use the Ant Design selected-text foreground.",
            failures);
    }

    private static void VerifyInputTextPresenterInvalidatesSelectionTextLayout(ICollection<string> failures)
    {
        var presenter = new CountingInputTextPresenter
        {
            Text                     = "selection",
            SelectionForegroundBrush = Brush(Colors.Black),
            ShowSelectionHighlight   = true
        };
        using var realized = RealizeControl(presenter);
        RefreshLayout(realized.Window);

        presenter.ResetTextLayoutInvalidations();
        presenter.SetCurrentValue(TextPresenter.SelectionForegroundBrushProperty, Brush(Colors.White));
        Expect(presenter.TextLayoutInvalidations > 0,
            "InputTextPresenter should fully invalidate text layout when SelectionForegroundBrush changes.",
            failures);

        presenter.ResetTextLayoutInvalidations();
        presenter.SetCurrentValue(TextPresenter.SelectionStartProperty, 1);
        Expect(presenter.TextLayoutInvalidations > 0,
            "InputTextPresenter should fully invalidate text layout when SelectionStart changes.",
            failures);

        presenter.ResetTextLayoutInvalidations();
        presenter.SetCurrentValue(TextPresenter.SelectionEndProperty, 8);
        Expect(presenter.TextLayoutInvalidations > 0,
            "InputTextPresenter should fully invalidate text layout when SelectionEnd changes.",
            failures);

        presenter.ResetTextLayoutInvalidations();
        presenter.SetCurrentValue(TextPresenter.ShowSelectionHighlightProperty, false);
        Expect(presenter.TextLayoutInvalidations > 0,
            "InputTextPresenter should fully invalidate text layout when ShowSelectionHighlight changes.",
            failures);
    }

    private static void VerifyTextBoxRuntimeSlots(ICollection<string> failures)
    {
        var textBox = new AtomTextBox
        {
            Width = 260,
            Text  = "abc"
        };
        using var realized = RealizeControl(textBox);

        Expect(FindVisualByName<InputClearIconButton>(textBox, "PART_ClearButton") == null,
            "TextBox default should not create PART_ClearButton.", failures);
        Expect(FindVisualByName<RevealButton>(textBox, "PART_RevealButton") == null,
            "TextBox default should not create PART_RevealButton.", failures);
        Expect(FindVisualByName<ContentPresenter>(textBox, "FormFeedBack") == null,
            "TextBox default should not create FormFeedBack presenter.", failures);
        Expect(FindVisualByName<ContentPresenter>(textBox, "PART_RightAddOn") == null,
            "TextBox default should not create PART_RightAddOn presenter.", failures);
        Expect(FindVisualByName<Avalonia.Controls.TextBlock>(textBox, "TextCountIndicator") == null,
            "TextBox default should not create TextCountIndicator.", failures);

        textBox.SetCurrentValue(AtomTextBox.IsAllowClearProperty, true);
        RefreshLayout(realized.Window);
        var clearButton = FindVisualByName<InputClearIconButton>(textBox, "PART_ClearButton");
        Expect(clearButton != null, "TextBox should create PART_ClearButton when clear is visible.", failures);
        Expect(clearButton?.Icon is CloseCircleFilled,
            "TextBox clear button should use the default clear icon when ClearIcon is unset.", failures);
        var customClearIcon = new InfoCircleOutlined();
        textBox.SetCurrentValue(AtomTextBox.ClearIconProperty, customClearIcon);
        RefreshLayout(realized.Window);
        Expect(ReferenceEquals(clearButton?.Icon, customClearIcon),
            "TextBox clear button should sync a custom ClearIcon.", failures);
        textBox.SetCurrentValue(AtomTextBox.ClearIconProperty, null);
        RefreshLayout(realized.Window);
        Expect(clearButton?.Icon is CloseCircleFilled,
            "TextBox clear button should restore the default clear icon after ClearIcon is cleared.", failures);
        clearButton?.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, clearButton));
        RefreshLayout(realized.Window);
        Expect(string.IsNullOrEmpty(textBox.Text), "TextBox clear button should clear Text.", failures);
        Expect(FindVisualByName<InputClearIconButton>(textBox, "PART_ClearButton") == null,
            "TextBox should remove PART_ClearButton after text is cleared.", failures);
        Expect(clearButton?.GetVisualParent() == null,
            "Removed TextBox clear button should not keep a visual parent.", failures);

        textBox.SetCurrentValue(Avalonia.Controls.TextBox.TextProperty, "secret");
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.PasswordCharProperty, '*');
        textBox.SetCurrentValue(AtomTextBox.IsEnableRevealButtonProperty, true);
        RefreshLayout(realized.Window);
        var revealButton = FindVisualByName<RevealButton>(textBox, "PART_RevealButton");
        Expect(revealButton != null, "TextBox should create PART_RevealButton when reveal is enabled.", failures);
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.RevealPasswordProperty, true);
        RefreshLayout(realized.Window);
        Expect(revealButton?.IsChecked == true,
            "TextBox RevealPassword should sync to RevealButton.", failures);
        revealButton?.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
        RefreshLayout(realized.Window);
        Expect(!textBox.RevealPassword,
            "TextBox RevealButton IsChecked should sync back to RevealPassword.", failures);
        textBox.SetCurrentValue(AtomTextBox.IsEnableRevealButtonProperty, false);
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<RevealButton>(textBox, "PART_RevealButton") == null,
            "TextBox should remove PART_RevealButton when reveal is disabled.", failures);
        Expect(revealButton?.GetVisualParent() == null,
            "Removed TextBox reveal button should not keep a visual parent.", failures);

        textBox.SetCurrentValue(Avalonia.Controls.TextBox.TextProperty, "count");
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.MaxLengthProperty, 20);
        textBox.SetCurrentValue(AtomTextBox.IsShowCountProperty, true);
        RefreshLayout(realized.Window);
        var countIndicator = FindVisualByName<Avalonia.Controls.TextBlock>(textBox, "TextCountIndicator");
        Expect(countIndicator?.Text == "5 / 20",
            $"TextBox count indicator should show '5 / 20', actual '{countIndicator?.Text}'.", failures);
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.MaxLengthProperty, 30);
        RefreshLayout(realized.Window);
        Expect(countIndicator?.Text == "5 / 30",
            $"TextBox count indicator should update after MaxLength changes, actual '{countIndicator?.Text}'.", failures);
        textBox.SetCurrentValue(AtomTextBox.IsShowCountProperty, false);
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<Avalonia.Controls.TextBlock>(textBox, "TextCountIndicator") == null,
            "TextBox should remove TextCountIndicator when count is disabled.", failures);
        Expect(countIndicator?.GetVisualParent() == null,
            "Removed TextBox count indicator should not keep a visual parent.", failures);

        var leftContent = new Avalonia.Controls.TextBlock { Text = "left" };
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.InnerLeftContentProperty, leftContent);
        RefreshLayout(realized.Window);
        var leftPresenter = FindVisualByName<ContentPresenter>(textBox, "PART_LeftAddOn");
        Expect(ReferenceEquals(leftPresenter?.Content, leftContent),
            "TextBox left addon presenter should hold InnerLeftContent.", failures);
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.InnerLeftContentProperty, null);
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<ContentPresenter>(textBox, "PART_LeftAddOn") == null,
            "TextBox should remove left addon presenter after clearing InnerLeftContent.", failures);

        var rightContent = new Avalonia.Controls.TextBlock { Text = "right" };
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.InnerRightContentProperty, rightContent);
        RefreshLayout(realized.Window);
        var rightPresenter = FindVisualByName<ContentPresenter>(textBox, "PART_RightAddOn");
        Expect(ReferenceEquals(rightPresenter?.Content, rightContent),
            "TextBox right addon presenter should hold InnerRightContent.", failures);
        textBox.SetCurrentValue(Avalonia.Controls.TextBox.InnerRightContentProperty, null);
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<ContentPresenter>(textBox, "PART_RightAddOn") == null,
            "TextBox should remove right addon presenter after clearing InnerRightContent.", failures);
    }

    private static void VerifyTextAreaRuntimeSlots(ICollection<string> failures)
    {
        var textArea = new TextArea
        {
            Width = 260,
            Text  = "abc"
        };
        using var realized = RealizeControl(textArea);

        Expect(FindVisualByName<ResizeHandle>(textArea, "PART_ResizeHandle") == null,
            "TextArea default should not create PART_ResizeHandle.", failures);
        Expect(FindVisualByName<Avalonia.Controls.TextBlock>(textArea, "TextCountIndicator") == null,
            "TextArea default should not create TextCountIndicator.", failures);

        textArea.SetCurrentValue(TextArea.IsResizableProperty, true);
        RefreshLayout(realized.Window);
        var resizeHandle = FindVisualByName<ResizeHandle>(textArea, "PART_ResizeHandle");
        Expect(resizeHandle != null, "TextArea should create PART_ResizeHandle when IsResizable is true.", failures);
        textArea.SetCurrentValue(TextArea.IsResizableProperty, false);
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<ResizeHandle>(textArea, "PART_ResizeHandle") == null,
            "TextArea should remove PART_ResizeHandle when IsResizable is false.", failures);
        Expect(resizeHandle?.GetVisualParent() == null,
            "Removed TextArea resize handle should not keep a visual parent.", failures);

        textArea.SetCurrentValue(Avalonia.Controls.TextBox.MaxLengthProperty, 20);
        textArea.SetCurrentValue(TextArea.IsShowCountProperty, true);
        RefreshLayout(realized.Window);
        var countIndicator = FindVisualByName<Avalonia.Controls.TextBlock>(textArea, "TextCountIndicator");
        Expect(countIndicator?.Text == "3 / 20",
            $"TextArea count indicator should show '3 / 20', actual '{countIndicator?.Text}'.", failures);
        textArea.SetCurrentValue(Avalonia.Controls.TextBox.MaxLengthProperty, 30);
        RefreshLayout(realized.Window);
        Expect(countIndicator?.Text == "3 / 30",
            $"TextArea count indicator should update after MaxLength changes, actual '{countIndicator?.Text}'.", failures);
        textArea.SetCurrentValue(TextArea.IsShowCountProperty, false);
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<Avalonia.Controls.TextBlock>(textArea, "TextCountIndicator") == null,
            "TextArea should remove TextCountIndicator when count is disabled.", failures);
        Expect(countIndicator?.GetVisualParent() == null,
            "Removed TextArea count indicator should not keep a visual parent.", failures);

        textArea.SetCurrentValue(Avalonia.Controls.TextBox.TextProperty, "clear");
        textArea.SetCurrentValue(TextArea.IsAllowClearProperty, true);
        RefreshLayout(realized.Window);
        var clearButton = FindVisualByName<InputClearIconButton>(textArea, "PART_ClearButton");
        Expect(clearButton?.Icon is CloseCircleFilled,
            "TextArea clear button should use the default clear icon when ClearIcon is unset.", failures);
        var customClearIcon = new InfoCircleOutlined();
        textArea.SetCurrentValue(TextArea.ClearIconProperty, customClearIcon);
        RefreshLayout(realized.Window);
        Expect(ReferenceEquals(clearButton?.Icon, customClearIcon),
            "TextArea clear button should sync a custom ClearIcon.", failures);
        textArea.SetCurrentValue(TextArea.ClearIconProperty, null);
        RefreshLayout(realized.Window);
        Expect(clearButton?.Icon is CloseCircleFilled,
            "TextArea clear button should restore the default clear icon after ClearIcon is cleared.", failures);
    }

    private static void VerifySearchEditRuntimeSlots(ICollection<string> failures)
    {
        var searchEdit = new SearchEdit
        {
            Width = 260,
            Text  = "search"
        };
        using var realized = RealizeControl(searchEdit);

        // SearchButton 控件类型已移除：搜索按钮现在是 SearchEditDecoratedBox 模板中的 atom:Button#PART_RightAddOn
        Expect(FindVisualByName<Button>(searchEdit, "PART_RightAddOn") != null,
            "SearchEdit should keep its search button.", failures);
        Expect(FindVisualByName<InputClearIconButton>(searchEdit, "PART_ClearButton") == null,
            "SearchEdit default should not create a clear button.", failures);

        var leftAddOn = new Avalonia.Controls.TextBlock { Text = "https://" };
        searchEdit.SetCurrentValue(LineEdit.LeftAddOnProperty, leftAddOn);
        RefreshLayout(realized.Window);
        var leftPresenter = FindVisualByName<ContentPresenter>(searchEdit, "PART_LeftAddOn");
        Expect(ReferenceEquals(leftPresenter?.Content, leftAddOn),
            "SearchEdit left addon presenter should hold LeftAddOn.", failures);
        searchEdit.SetCurrentValue(LineEdit.LeftAddOnProperty, null);
        RefreshLayout(realized.Window);
        Expect(leftPresenter?.Content == null,
            "SearchEdit static left addon presenter should clear Content after clearing LeftAddOn.", failures);

        searchEdit.SetCurrentValue(AtomTextBox.IsAllowClearProperty, true);
        RefreshLayout(realized.Window);
        var contentRightPresenter = FindVisualByName<ContentPresenter>(searchEdit, "PART_ContentRightAddOn");
        Expect(contentRightPresenter != null,
            "SearchEdit should create content-right presenter when clear accessory is visible.", failures);
        var clearButton = FindVisualByName<InputClearIconButton>(searchEdit, "PART_ClearButton");
        Expect(clearButton != null,
            "SearchEdit should create clear button in dynamic content-right presenter.", failures);
        Expect(clearButton?.Icon is CloseCircleFilled,
            "SearchEdit clear button should use the default clear icon when ClearIcon is unset.", failures);
        var customClearIcon = new InfoCircleOutlined();
        searchEdit.SetCurrentValue(AtomTextBox.ClearIconProperty, customClearIcon);
        RefreshLayout(realized.Window);
        Expect(ReferenceEquals(clearButton?.Icon, customClearIcon),
            "SearchEdit clear button should sync a custom ClearIcon.", failures);
        searchEdit.SetCurrentValue(AtomTextBox.ClearIconProperty, null);
        RefreshLayout(realized.Window);
        Expect(clearButton?.Icon is CloseCircleFilled,
            "SearchEdit clear button should restore the default clear icon after ClearIcon is cleared.", failures);
        searchEdit.SetCurrentValue(AtomTextBox.IsAllowClearProperty, false);
        RefreshLayout(realized.Window);
        Expect(FindVisualByName<InputClearIconButton>(searchEdit, "PART_ClearButton") == null,
            "SearchEdit should remove clear button after clear accessory is disabled.", failures);
        Expect(contentRightPresenter?.GetVisualParent() != null,
            "SearchEdit keeps the static content-right presenter in the template.", failures);
    }
}
