using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Primitives.Themes;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AtomTextBox = AtomUI.Desktop.Controls.TextBox;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunAccessoryLifecycleVerification()
    {
        var failures = new List<string>();
        VerifyLineEditAccessoryLifecycle(failures);
        VerifyTextAreaAccessoryLifecycle(failures);
        VerifySearchEditAccessoryLifecycle(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Accessory lifecycle verification passed.");
            return true;
        }

        Console.Error.WriteLine("Accessory lifecycle verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyLineEditAccessoryLifecycle(ICollection<string> failures)
    {
        var lineEdit = CreateLineEdit(text: "abc");
        using var realized = RealizeControl(lineEdit);
        var decoratedBox = GetAddOnDecoratedBox(lineEdit, failures, "LineEdit");
        if (decoratedBox == null)
        {
            return;
        }

        Expect(decoratedBox.ContentRightAddOn == null, "LineEdit default should not materialize ContentRightAddOn.", failures);

        lineEdit.SetCurrentValue(AtomTextBox.IsAllowClearProperty, true);
        RefreshLayout(realized.Window);
        var clearHost = ExpectHost(decoratedBox, failures, "LineEdit clear");
        var clearButton = clearHost?.Children.OfType<InputClearIconButton>().SingleOrDefault();
        Expect(clearButton != null, "LineEdit clear should create InputClearIconButton.", failures);
        clearButton?.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, clearButton));
        RefreshLayout(realized.Window);
        Expect(string.IsNullOrEmpty(lineEdit.Text), "LineEdit clear button should clear Text.", failures);
        Expect(decoratedBox.ContentRightAddOn == null, "LineEdit clear host should be removed after text is cleared.", failures);
        Expect(clearHost?.Children.Count == 0, "LineEdit clear host should clear children after detach.", failures);
        Expect(clearHost?.GetVisualParent() == null, "LineEdit detached clear host should leave the visual tree.", failures);
        lineEdit.SetCurrentValue(AtomTextBox.IsAllowClearProperty, false);
        RefreshLayout(realized.Window);

        lineEdit.SetCurrentValue(Avalonia.Controls.TextBox.TextProperty, "secret");
        lineEdit.SetCurrentValue(Avalonia.Controls.TextBox.PasswordCharProperty, '*');
        lineEdit.SetCurrentValue(AtomTextBox.IsEnableRevealButtonProperty, true);
        RefreshLayout(realized.Window);
        var revealHost = ExpectHost(decoratedBox, failures, "LineEdit reveal");
        var revealButton = revealHost?.Children.OfType<RevealButton>().SingleOrDefault();
        Expect(revealButton != null, "LineEdit reveal should create RevealButton.", failures);
        lineEdit.SetCurrentValue(Avalonia.Controls.TextBox.RevealPasswordProperty, true);
        RefreshLayout(realized.Window);
        Expect(revealButton?.IsChecked == true, "LineEdit RevealPassword should sync to RevealButton.", failures);
        revealButton?.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
        RefreshLayout(realized.Window);
        Expect(!lineEdit.RevealPassword, "RevealButton should sync IsChecked back to LineEdit.RevealPassword.", failures);
        lineEdit.SetCurrentValue(AtomTextBox.IsEnableRevealButtonProperty, false);
        RefreshLayout(realized.Window);
        Expect(decoratedBox.ContentRightAddOn == null, "LineEdit reveal host should be removed after disabling reveal.", failures);
        Expect(revealHost?.Children.Count == 0, "LineEdit reveal host should clear children after detach.", failures);

        lineEdit.SetCurrentValue(Avalonia.Controls.TextBox.TextProperty, "abc");
        lineEdit.SetCurrentValue(Avalonia.Controls.TextBox.MaxLengthProperty, 10);
        lineEdit.SetCurrentValue(AtomTextBox.IsShowCountProperty, true);
        RefreshLayout(realized.Window);
        var countHost = ExpectHost(decoratedBox, failures, "LineEdit count");
        var countText = countHost?.Children.OfType<Avalonia.Controls.TextBlock>()
                                 .SingleOrDefault(textBlock => textBlock.Name == "TextCountIndicator");
        Expect(countText?.Text == "3 / 10", $"LineEdit count text should be '3 / 10', actual '{countText?.Text}'.", failures);
        lineEdit.SetCurrentValue(AtomTextBox.IsShowCountProperty, false);
        RefreshLayout(realized.Window);
        Expect(decoratedBox.ContentRightAddOn == null, "LineEdit count host should be removed after hiding count.", failures);
        Expect(countHost?.Children.Count == 0, "LineEdit count host should clear children after detach.", failures);

        var innerRightContent = new Avalonia.Controls.TextBlock { Text = "kg" };
        lineEdit.SetCurrentValue(Avalonia.Controls.TextBox.InnerRightContentProperty, innerRightContent);
        RefreshLayout(realized.Window);
        var innerRightHost = ExpectHost(decoratedBox, failures, "LineEdit inner right");
        var innerRightPresenter = innerRightHost?.Children.OfType<ContentPresenter>()
                                               .SingleOrDefault(presenter => presenter.Name == "InnerRightContentPresenter");
        Expect(ReferenceEquals(innerRightPresenter?.Content, innerRightContent), "LineEdit inner right presenter should hold InnerRightContent.", failures);
        lineEdit.SetCurrentValue(Avalonia.Controls.TextBox.InnerRightContentProperty, null);
        RefreshLayout(realized.Window);
        Expect(decoratedBox.ContentRightAddOn == null, "LineEdit inner right host should be removed after clearing content.", failures);
        Expect(innerRightHost?.Children.Count == 0, "LineEdit inner right host should clear children after detach.", failures);
        Expect(innerRightPresenter?.Content == null, "LineEdit inner right presenter should clear Content after detach.", failures);

        var feedback = new FormValidateFeedback
        {
            ErrorFeedback = new Avalonia.Controls.TextBlock { Text = "!" }
        };
        lineEdit.FormFeedback = feedback;
        feedback.SetCurrentValue(FormValidateFeedback.ValidateStatusProperty, FormValidateStatus.Error);
        RefreshLayout(realized.Window);
        var feedbackHost = ExpectHost(decoratedBox, failures, "LineEdit form feedback");
        var feedbackPresenter = feedbackHost?.Children.OfType<ContentPresenter>()
                                           .SingleOrDefault(presenter => presenter.Name == "FormFeedBack");
        Expect(ReferenceEquals(feedbackPresenter?.Content, feedback), "LineEdit form feedback presenter should hold FormFeedback.", failures);
        feedback.SetCurrentValue(FormValidateFeedback.ValidateStatusProperty, FormValidateStatus.Default);
        RefreshLayout(realized.Window);
        Expect(decoratedBox.ContentRightAddOn == null, "LineEdit form feedback host should be removed when feedback status returns to default.", failures);
        Expect(feedbackHost?.Children.Count == 0, "LineEdit feedback host should clear children after detach.", failures);
        Expect(feedbackPresenter?.Content == null, "LineEdit feedback presenter should clear Content after detach.", failures);
    }

    private static void VerifyTextAreaAccessoryLifecycle(ICollection<string> failures)
    {
        var textArea = new TextArea
        {
            Text  = "abc",
            Width = 260
        };
        using var realized = RealizeControl(textArea);
        var decoratedBox = GetAddOnDecoratedBox(textArea, failures, "TextArea");
        if (decoratedBox == null)
        {
            return;
        }

        Expect(decoratedBox.ContentRightAddOn == null, "TextArea default should not materialize ContentRightAddOn.", failures);

        textArea.SetCurrentValue(TextArea.IsAllowClearProperty, true);
        RefreshLayout(realized.Window);
        var clearHost = ExpectTextAreaHost(decoratedBox, failures, "TextArea clear");
        var clearButton = clearHost?.Children.OfType<InputClearIconButton>().SingleOrDefault();
        Expect(clearButton != null, "TextArea clear should create InputClearIconButton.", failures);
        clearButton?.RaiseEvent(new RoutedEventArgs(Avalonia.Controls.Button.ClickEvent, clearButton));
        RefreshLayout(realized.Window);
        Expect(string.IsNullOrEmpty(textArea.Text), "TextArea clear button should clear Text.", failures);
        Expect(decoratedBox.ContentRightAddOn == null, "TextArea clear host should be removed after text is cleared.", failures);
        Expect(clearHost?.Children.Count == 0, "TextArea clear host should clear children after detach.", failures);
        Expect(clearHost?.GetVisualParent() == null, "TextArea detached clear host should leave the visual tree.", failures);

        var innerRightContent = new Avalonia.Controls.TextBlock { Text = "rows" };
        textArea.SetCurrentValue(Avalonia.Controls.TextBox.InnerRightContentProperty, innerRightContent);
        RefreshLayout(realized.Window);
        var innerRightHost = ExpectTextAreaHost(decoratedBox, failures, "TextArea inner right");
        var innerRightPresenter = innerRightHost?.Children.OfType<ContentPresenter>()
                                               .SingleOrDefault(presenter => presenter.Name == "PART_InnerRightContentPresenter");
        Expect(ReferenceEquals(innerRightPresenter?.Content, innerRightContent), "TextArea inner right presenter should hold InnerRightContent.", failures);
        textArea.SetCurrentValue(Avalonia.Controls.TextBox.InnerRightContentProperty, null);
        RefreshLayout(realized.Window);
        Expect(decoratedBox.ContentRightAddOn == null, "TextArea inner right host should be removed after clearing content.", failures);
        Expect(innerRightHost?.Children.Count == 0, "TextArea inner right host should clear children after detach.", failures);
        Expect(innerRightPresenter?.Content == null, "TextArea inner right presenter should clear Content after detach.", failures);
    }

    private static void VerifySearchEditAccessoryLifecycle(ICollection<string> failures)
    {
        var searchEdit = new SearchEdit
        {
            Text        = "search",
            Width       = 260,
            IsShowCount = true,
            MaxLength   = 30
        };
        using var realized = RealizeControl(searchEdit);
        var decoratedBox = GetAddOnDecoratedBox(searchEdit, failures, "SearchEdit");
        if (decoratedBox == null)
        {
            return;
        }

        Expect(decoratedBox.ContentRightAddOn == null, "SearchEdit count should not materialize LineEdit count accessory.", failures);

        searchEdit.SetCurrentValue(AtomTextBox.IsAllowClearProperty, true);
        RefreshLayout(realized.Window);
        var clearHost = ExpectHost(decoratedBox, failures, "SearchEdit clear");
        Expect(clearHost?.Children.OfType<InputClearIconButton>().SingleOrDefault() != null,
            "SearchEdit clear should still create InputClearIconButton.", failures);

        searchEdit.SetCurrentValue(AtomTextBox.IsAllowClearProperty, false);
        RefreshLayout(realized.Window);
        Expect(decoratedBox.ContentRightAddOn == null, "SearchEdit clear host should be removed after disabling clear.", failures);
        Expect(clearHost?.Children.Count == 0, "SearchEdit clear host should clear children after detach.", failures);
    }

    private static AddOnDecoratedBox? GetAddOnDecoratedBox(Control root, ICollection<string> failures, string label)
    {
        var decoratedBoxes = root.GetSelfAndVisualDescendants()
                                 .OfType<AddOnDecoratedBox>()
                                 .ToList();
        if (decoratedBoxes.Count != 1)
        {
            failures.Add($"{label} should have exactly one AddOnDecoratedBox, actual {decoratedBoxes.Count}.");
            return null;
        }

        return decoratedBoxes[0];
    }

    private static LineEditAccessoryHost? ExpectHost(AddOnDecoratedBox decoratedBox,
                                                     ICollection<string> failures,
                                                     string label)
    {
        if (decoratedBox.ContentRightAddOn is LineEditAccessoryHost host)
        {
            return host;
        }

        failures.Add($"{label} should materialize LineEditAccessoryHost.");
        return null;
    }

    private static TextAreaAccessoryHost? ExpectTextAreaHost(AddOnDecoratedBox decoratedBox,
                                                             ICollection<string> failures,
                                                             string label)
    {
        if (decoratedBox.ContentRightAddOn is TextAreaAccessoryHost host)
        {
            return host;
        }

        failures.Add($"{label} should materialize TextAreaAccessoryHost.");
        return null;
    }
}
