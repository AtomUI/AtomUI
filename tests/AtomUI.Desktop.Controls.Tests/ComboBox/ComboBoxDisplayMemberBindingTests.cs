using System.Reflection;
using System.Text.RegularExpressions;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaWindow = Avalonia.Controls.Window;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AtomUIComboBoxItem = AtomUI.Desktop.Controls.ComboBoxItem;
using AtomUITextBox = AtomUI.Desktop.Controls.TextBox;

namespace AtomUI.Desktop.Controls.Tests.ComboBox;

public class ComboBoxDisplayMemberBindingTests
{
    static ComboBoxDisplayMemberBindingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pinned_Open_Request_Opens_ComboBox_And_Its_Template_Popup()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsMotionEnabled = false,
            ItemsSource     = new[] { "Alpha", "Beta" }
        };

        ShowInWindow(comboBox, () =>
        {
            comboBox.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popup = GetVisualDescendant<Popup>(comboBox, "PART_Popup");
            comboBox.IsDropDownOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_Open_Request_Suppresses_Light_Dismiss_Before_First_Open_And_Unpin_Restores_It()
    {
        var comboBox = new AtomUIComboBox
        {
            Width             = 200,
            IsMotionEnabled   = false,
            IsPopupPinnedOpen = true,
            ItemsSource       = new[] { "Alpha", "Beta" }
        };

        ShowInWindow(comboBox, () =>
        {
            var popup = GetVisualDescendant<Popup>(comboBox, "PART_Popup");

            comboBox.IsDropDownOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
            popup.IsLightDismissEnabled.ShouldBeFalse();

            comboBox.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();

            popup.IsPopupPinnedOpen.ShouldBeFalse();
            popup.IsLightDismissEnabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_ComboBox_Rejects_Business_Close_Request()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsMotionEnabled = false,
            ItemsSource     = new[] { "Alpha", "Beta" }
        };

        ShowInWindow(comboBox, () =>
        {
            comboBox.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = GetVisualDescendant<Popup>(comboBox, "PART_Popup");

            comboBox.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_ComboBox_Detach_Closes_And_Reattach_Reopens_It()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsMotionEnabled = false,
            ItemsSource     = new[] { "Alpha", "Beta" }
        };

        ShowInWindow(comboBox, window =>
        {
            var visualLayerManager = window.Content.ShouldBeOfType<VisualLayerManager>();
            var overlayPanel = visualLayerManager.Child.ShouldBeOfType<ScopeAwareOverlayLayerPanel>();
            comboBox.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = GetVisualDescendant<Popup>(comboBox, "PART_Popup");
            popup.IsOpen.ShouldBeTrue();

            overlayPanel.Children.Remove(comboBox);
            Dispatcher.UIThread.RunJobs();

            comboBox.IsPopupPinnedOpen.ShouldBeTrue();
            comboBox.IsDropDownOpen.ShouldBeFalse();
            popup.IsOpen.ShouldBeFalse();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();

            overlayPanel.Children.Add(comboBox);
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void DisplayMemberBinding_DropDown_Item_Content_Is_Vertically_Centered()
    {
        var comboBox = new TestComboBox
        {
            Width                = 200,
            ItemsSource          = new List<DataItem> { new("Avalonia") },
            DisplayMemberBinding = new Binding(nameof(DataItem.Name)),
            OptionFontSize       = 17,
            IsMotionEnabled      = false
        };
        var item      = new DataItem("Avalonia");
        var container = new AtomUIComboBoxItem();

        ShowInWindow(container, () =>
        {
            comboBox.PrepareContainerForTest(container, item, 0);
            container.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            container.Content.ShouldBe(item);
            container.ContentTemplate.ShouldNotBeNull();
            container.FontSize.ShouldBe(17);
            container.VerticalContentAlignment.ShouldBe(
                VerticalAlignment.Center,
                "DisplayMemberBinding uses Avalonia's generated item template, so the ComboBoxItem itself must center presented content.");
        });
    }

    [Fact]
    public void Selected_Content_Presenter_Uses_SelectionBoxItemTemplate()
    {
        var selectedTemplate = new FuncDataTemplate<DataItem>((_, _) => new TextBlock { Text = "Selected value" });
        var comboBox = new AtomUIComboBox
        {
            Width                    = 200,
            ItemsSource              = new List<DataItem> { new("Avalonia") },
            SelectedIndex            = 0,
            SelectionBoxItemTemplate = selectedTemplate,
            IsMotionEnabled          = false
        };

        ShowInWindow(comboBox, () =>
        {
            var presenter = comboBox.GetVisualDescendants()
                                    .OfType<ContentPresenter>()
                                    .Single(x => x.Name == "SelectedContentPresenter");

            presenter.ContentTemplate.ShouldBeSameAs(
                selectedTemplate,
                "ComboBox must preserve Avalonia's SelectionBoxItemTemplate semantics instead of hard-wiring selected display to ItemTemplate.");
        });
    }

    [Fact]
    public void Template_Binds_Right_AddOn_Feedback_And_Handle_State_From_Axaml()
    {
        var rightAddOn = new TextBlock { Text = "extra" };
        var feedback   = new FormValidateFeedback();
        var comboBox = new AtomUIComboBox
        {
            Width             = 200,
            ContentRightAddOn = rightAddOn,
            IsMotionEnabled   = false,
            IsEnabled         = false
        };

        ((IFormItemFeedbackAware)comboBox).SetFeedbackControl(feedback);
        feedback.ValidateStatus = FormValidateStatus.Error;

        ShowInWindow(comboBox, () =>
        {
            var contentPresenter = GetVisualDescendant<ContentPresenter>(
                comboBox,
                "PART_ContentRightAddOnPresenter");
            contentPresenter.Content.ShouldBeSameAs(rightAddOn);
            contentPresenter.IsVisible.ShouldBeTrue();

            var formFeedback = GetVisualDescendant<ContentPresenter>(comboBox, "PART_FormFeedBack");
            formFeedback.Content.ShouldBeSameAs(feedback);
            formFeedback.IsVisible.ShouldBeTrue();

            var handle = GetVisualDescendant<ComboBoxHandle>(comboBox, "PART_ComboBoxHandle");
            handle.IsMotionEnabled.ShouldBeFalse();
            handle.IsEnabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void Template_Keeps_NonFilter_ItemsPresenter_As_Direct_ScrollViewer_Content()
    {
        var source = ReadRepoFile("src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml");

        Regex.IsMatch(
                 source,
                 """
                 <atom:ScrollViewer\b[^>]*>\s*<ItemsPresenter\s+Name="PART_ItemsPresenter"
                 """,
                 RegexOptions.CultureInvariant)
             .ShouldBeTrue(
                 "The non-filter popup path must keep PART_ItemsPresenter as the direct ScrollViewer content.");
        source.ShouldNotContain("""
                                <Panel>
                                    <ItemsPresenter
                                        Name="PART_ItemsPresenter"
""");
        source.ShouldContain("OverlayDismissEventPassThrough=\"True\"");
    }

    [Fact]
    public void NonEditable_DropDown_Presents_Direct_ComboBoxItem_Containers()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Gamma" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popupItems = window.GetVisualDescendants()
                                   .OfType<AtomUIComboBoxItem>()
                                   .Where(item => item.Content is string)
                                   .Select(item => item.Content)
                                   .ToList();

            popupItems.ShouldBe(
            [
                "Alpha",
                "Beta",
                "Gamma"
            ]);

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            popupFrame.Bounds.Height.ShouldBeGreaterThan(
                96,
                "The opened popup must be tall enough to show direct item rows instead of being clipped to the content padding.");
        });
    }

    [Fact]
    public void Editable_Template_Provides_TextBox_And_Binds_Text()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha") }
        };

        ShowInWindow(comboBox, () =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.IsVisible.ShouldBeTrue();

            comboBox.Text = "Al";
            Dispatcher.UIThread.RunJobs();
            textBox.Text.ShouldBe("Al");

            textBox.Text = "Alpha";
            Dispatcher.UIThread.RunJobs();
            comboBox.Text.ShouldBe("Alpha");
        });
    }

    [Fact]
    public void Editable_Template_Uses_Lightweight_TextBox_With_TextPresenter()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha") }
        };

        ShowInWindow(comboBox, () =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.GetVisualAncestors()
                   .OfType<ContentPresenter>()
                   .Any(presenter => presenter.Name == "PART_ContentPresenter")
                   .ShouldBeTrue(
                       "The editable TextBox must live inside AddOnDecoratedBox's content presenter, because the content frame owns ComboBox input chrome and spacing.");
            textBox.ShouldNotBeAssignableTo<AtomUITextBox>(
                "The ComboBox content frame owns the input chrome; the editable field must not nest a full AtomUI TextBox frame inside it.");

            textBox.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var textPresenter = textBox.GetVisualDescendants()
                                       .OfType<TextPresenter>()
                                       .SingleOrDefault(presenter => presenter.Name == "PART_TextPresenter");
            textPresenter.ShouldNotBeNull(
                "A focused editable ComboBox without a TextPresenter can accept Text changes but cannot render a caret.");
            textPresenter.Bounds.Height.ShouldBeGreaterThan(0);
            textPresenter.CaretBrush.ShouldNotBeNull();

            textBox.GetVisualDescendants()
                   .OfType<Border>()
                   .Any(border => border.Name == "InnerBoxDecorator")
                   .ShouldBeFalse(
                       "The editable field should only contribute text input visuals; AddOnDecoratedBox already renders the ComboBox frame.");
        });
    }

    [Fact]
    public void Editable_Placeholder_Hides_While_Ime_Preedit_Text_Is_Rendered()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            PlaceholderText = "请选择",
            ItemsSource     = new List<DataItem> { new("Alpha") }
        };

        ShowInWindow(comboBox, () =>
        {
            var textBox     = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            var placeholder = GetVisualDescendant<TextBlock>(textBox, "Placeholder");
            var presenter   = GetVisualDescendant<TextPresenter>(textBox, "PART_TextPresenter");

            placeholder.IsVisible.ShouldBeTrue();

            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, "测");
            Dispatcher.UIThread.RunJobs();

            placeholder.IsVisible.ShouldBeFalse(
                "IME preedit text is rendered by the editable TextBox presenter before Text is committed, so the placeholder must not remain over it.");

            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, string.Empty);
            Dispatcher.UIThread.RunJobs();

            placeholder.IsVisible.ShouldBeTrue(
                "The editable ComboBox placeholder should return when IME preedit text is cleared and the text is still empty.");
        });
    }

    [Fact]
    public void Editable_Filter_Click_Focuses_TextBox_And_Accepts_Text_Input()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            var clickPoint = textBox.TranslatePoint(
                new Point(textBox.Bounds.Width / 2, textBox.Bounds.Height / 2),
                window);

            clickPoint.ShouldNotBeNull();

            window.MouseMove(clickPoint.Value);
            window.MouseDown(clickPoint.Value, MouseButton.Left);
            window.MouseUp(clickPoint.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            textBox.IsFocused.ShouldBeTrue(
                "Editable ComboBox must route pointer focus to PART_EditableTextBox so text input reaches the editable field.");

            window.KeyTextInput("A");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("A");
            comboBox.FilterValue.ShouldBe("A");
        });
    }

    [Fact]
    public void Editable_Filter_Surface_Click_Focuses_TextBox_And_Accepts_Text_Input()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            var clickPoint = comboBox.TranslatePoint(
                new Point(16, comboBox.Bounds.Height / 2),
                window);

            clickPoint.ShouldNotBeNull();

            window.MouseMove(clickPoint.Value);
            window.MouseDown(clickPoint.Value, MouseButton.Left);
            window.MouseUp(clickPoint.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            textBox.IsFocused.ShouldBeTrue(
                "Clicking the editable ComboBox input surface must focus PART_EditableTextBox, not only the exact TextBox visual bounds.");

            window.KeyTextInput("A");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("A");
            comboBox.FilterValue.ShouldBe("A");
        });
    }

    [Fact]
    public void Editable_Filter_Handle_Click_Focuses_TextBox_And_Accepts_Text_Input()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            var handle  = GetVisualDescendant<ComboBoxHandle>(comboBox, "PART_ComboBoxHandle");
            var clickPoint = handle.TranslatePoint(
                new Point(handle.Bounds.Width / 2, handle.Bounds.Height / 2),
                window);

            clickPoint.ShouldNotBeNull();

            window.MouseMove(clickPoint.Value);
            window.MouseDown(clickPoint.Value, MouseButton.Left);
            window.MouseUp(clickPoint.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeTrue();
            textBox.IsFocused.ShouldBeTrue(
                "Opening an editable ComboBox from the drop-down handle must still leave keyboard input on PART_EditableTextBox.");

            window.KeyTextInput("A");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("A");
            comboBox.FilterValue.ShouldBe("A");
        });
    }

    [Fact]
    public void Editable_Filter_Handle_Click_With_Selected_Item_Keeps_TextBox_Focused()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });
        comboBox.SelectedIndex = 0;

        ShowInWindow(comboBox, window =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            var handle  = GetVisualDescendant<ComboBoxHandle>(comboBox, "PART_ComboBoxHandle");
            var clickPoint = handle.TranslatePoint(
                new Point(handle.Bounds.Width / 2, handle.Bounds.Height / 2),
                window);

            clickPoint.ShouldNotBeNull();

            window.MouseMove(clickPoint.Value);
            window.MouseDown(clickPoint.Value, MouseButton.Left);
            window.MouseUp(clickPoint.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeTrue();
            textBox.IsFocused.ShouldBeTrue(
                "Avalonia ComboBox focuses the selected drop-down item when the popup opens; AtomUI editable ComboBox must restore focus to the input box.");

            window.KeyTextInput("B");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldEndWith("B");
            comboBox.FilterValue?.ToString().ShouldEndWith("B");
        });
    }

    [Fact]
    public void Editable_Filter_Popup_Passes_Overlay_Input_Through_To_Input_Surface()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, () =>
        {
            var decoratedBox = GetVisualDescendant<AddOnDecoratedBox>(comboBox, AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var popup        = GetVisualDescendant<Popup>(comboBox, "PART_Popup");

            popup.OverlayInputPassThroughElement.ShouldBeSameAs(
                decoratedBox,
                "Editable filtering ComboBox uses an overlay popup, so the overlay must pass pointer input through to the whole input surface.");
        });
    }

    [Fact]
    public void Editable_Filter_Open_Popup_Input_Surface_Click_Passes_Through_And_Accepts_Text()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var clickPoint = comboBox.TranslatePoint(
                new Point(16, comboBox.Bounds.Height / 2),
                window);

            clickPoint.ShouldNotBeNull();

            window.MouseMove(clickPoint.Value);
            window.MouseDown(clickPoint.Value, MouseButton.Left);
            window.MouseUp(clickPoint.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            textBox.IsFocused.ShouldBeTrue(
                "When the overlay popup is open, pointer input over the ComboBox input surface must pass through the overlay and focus PART_EditableTextBox.");

            window.KeyTextInput("A");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("A");
            comboBox.FilterValue.ShouldBe("A");
        });
    }

    [Fact]
    public void Editable_Filter_Popup_Opened_Focuses_TextBox()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");

            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            textBox.IsFocused.ShouldBeTrue(
                "Editable filtering ComboBox must follow Select's popup lifecycle and focus the text input after the popup has opened.");

            window.KeyTextInput("A");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("A");
            comboBox.FilterValue.ShouldBe("A");
        });
    }

    [Fact]
    public void Editable_Filter_Popup_Opened_Does_Not_Select_All_Input_Text()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta"), new("Gamma") }
        };

        ShowInWindow(comboBox, () =>
        {
            comboBox.Text           = "Gamma";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.IsFocused.ShouldBeTrue();
            textBox.SelectionStart.ShouldBe(
                textBox.Text?.Length ?? 0,
                "Opening the editable filtering popup must not preserve Avalonia ComboBox's SelectAll focus behavior.");
            textBox.SelectionEnd.ShouldBe(textBox.SelectionStart);
            textBox.CaretIndex.ShouldBe(textBox.SelectionStart);
        });
    }

    [Fact]
    public void Editable_Filter_Decorated_Box_Focus_Routes_Text_Input_To_TextBox()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            var decoratedBox = GetVisualDescendant<AddOnDecoratedBox>(comboBox, AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var textBox      = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");

            decoratedBox.Focus(NavigationMethod.Pointer);
            Dispatcher.UIThread.RunJobs();

            textBox.IsFocused.ShouldBeTrue(
                "When the editable ComboBox input surface receives focus, keyboard input must be routed to PART_EditableTextBox.");

            window.KeyTextInput("A");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("A");
            comboBox.FilterValue.ShouldBe("A");
        });
    }

    [Fact]
    public void Editable_Filter_Text_Input_Opens_DropDown()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");

            textBox.Focus(NavigationMethod.Pointer);
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeFalse();

            window.KeyTextInput("A");
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("A");
            comboBox.FilterValue.ShouldBe("A");
            comboBox.IsDropDownOpen.ShouldBeTrue(
                "Editable filtering ComboBox should open suggestions when the focused input receives text.");
        });
    }

    [Fact]
    public void Editable_Filter_Backspace_Opens_DropDown_With_Filtered_Candidates()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta"), new("Gamma") }
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text = "Gamma";
            Dispatcher.UIThread.RunJobs();

            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.Focus(NavigationMethod.Pointer);
            textBox.CaretIndex     = textBox.Text?.Length ?? 0;
            textBox.SelectionStart = textBox.CaretIndex;
            textBox.SelectionEnd   = textBox.CaretIndex;
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeFalse();

            PressKey(window, Key.Back, PhysicalKey.Backspace);
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("Gamm");
            comboBox.FilterValue.ShouldBe("Gamm");
            comboBox.IsDropDownOpen.ShouldBeTrue(
                "Backspace is a user text edit in editable filtering mode, so the candidate popup should reopen with the new filter text.");

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var visibleRows = popupFrame.GetVisualDescendants()
                                        .OfType<AtomUIComboBoxItem>()
                                        .Where(item => item.IsVisible && item.Content is DataItem)
                                        .Select(item => ((DataItem)item.Content!).Name)
                                        .ToList();
            visibleRows.ShouldBe(["Gamma"]);
        });
    }

    [Fact]
    public void Editable_Filter_Backspace_To_Empty_Closes_DropDown()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "A";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.Focus(NavigationMethod.Pointer);
            textBox.CaretIndex     = textBox.Text?.Length ?? 0;
            textBox.SelectionStart = textBox.CaretIndex;
            textBox.SelectionEnd   = textBox.CaretIndex;
            Dispatcher.UIThread.RunJobs();

            PressKey(window, Key.Back, PhysicalKey.Backspace);
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBeEmpty();
            comboBox.FilterValue.ShouldBeNull();
            comboBox.IsDropDownOpen.ShouldBeFalse(
                "Deleting the editable filtering text to empty should close the popup instead of showing an unfiltered list implicitly.");
        });
    }

    [Fact]
    public void Editable_Filter_Programmatic_Text_Does_Not_Open_DropDown()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Beta") }
        };

        ShowInWindow(comboBox, () =>
        {
            comboBox.Text = "Al";
            Dispatcher.UIThread.RunJobs();

            comboBox.FilterValue.ShouldBe("Al");
            comboBox.IsDropDownOpen.ShouldBeFalse(
                "Only focused editable input changes should open the candidate popup; programmatic Text updates should not.");
        });
    }

    [Fact]
    public void Editable_Filter_Uses_Filtered_Visible_Items_Without_Replacing_Items()
    {
        var items = new List<DataItem>
        {
            new("Alpha"),
            new("Alpine"),
            new("Beta")
        };
        var comboBox = new AtomUIComboBox
        {
            Width               = 200,
            IsEditable          = true,
            IsFilterEnabled     = true,
            IsMotionEnabled     = false,
            ItemsSource         = items,
            FilterValueSelector = item => item is DataItem dataItem ? dataItem.Name : item?.ToString()
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            comboBox.Items.Count.ShouldBe(3);
            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var visibleRows = popupFrame.GetVisualDescendants()
                                    .OfType<AtomUIComboBoxItem>()
                                    .Where(item => item.IsVisible && item.Content is DataItem)
                                    .Select(item => ((DataItem)item.Content!).Name)
                                    .ToList();

            visibleRows.ShouldBe(["Alpha", "Alpine"]);
        });
    }

    [Fact]
    public void Editable_Filter_Uses_DisplayMemberBinding_When_FilterValueSelector_Is_Not_Set()
    {
        var comboBox = new AtomUIComboBox
        {
            Width                = 200,
            IsEditable           = true,
            IsFilterEnabled      = true,
            IsMotionEnabled      = false,
            DisplayMemberBinding = new Binding(nameof(DataItem.Name)),
            ItemsSource = new List<DataItem>
            {
                new DataItem("Alpha"),
                new DataItem("Alpine"),
                new DataItem("Beta")
            }
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "pine";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var visibleRows = popupFrame.GetVisualDescendants()
                                    .OfType<AtomUIComboBoxItem>()
                                    .Where(item => item.IsVisible && item.Content is DataItem)
                                    .Select(item => ((DataItem)item.Content!).Name)
                                    .ToList();

            visibleRows.ShouldBe(["Alpine"]);
        });
    }

    [Fact]
    public void Editable_Filter_With_ItemTemplate_Does_Not_Set_Candidate_DisplayMemberBinding()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            ItemsSource     = new List<DataItem> { new("Alpha"), new("Alpine") },
            ItemTemplate    = new FuncDataTemplate<DataItem>((item, _) => new TextBlock { Text = item?.Name })
        };

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var visibleRows = popupFrame.GetVisualDescendants()
                                    .OfType<AtomUIComboBoxItem>()
                                    .Where(item => item.IsVisible && item.Content is DataItem)
                                    .Select(item => ((DataItem)item.Content!).Name)
                                    .ToList();

            visibleRows.ShouldBe(["Alpha", "Alpine"]);
        });
    }

    [Fact]
    public void Editable_Filter_DropDown_Presents_Candidate_Item_Rows()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            comboBox.FilterValue.ShouldBe("Al");
            ((AtomUIComboBoxItem)comboBox.Items[2]!).IsVisible.ShouldBeFalse();
            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var candidateRows = popupFrame.GetVisualDescendants()
                                      .OfType<AtomUIComboBoxItem>()
                                      .Where(item => item.IsVisible && item.Content is string)
                                      .Select(item => item.Content)
                                      .ToList();

            candidateRows.ShouldBe(
            [
                "Alpha",
                "Alpine"
            ]);

            popupFrame.Bounds.Height.ShouldBeGreaterThan(
                48,
                "The editable filter popup must render candidate rows instead of collapsing to an empty strip.");
        });
    }

    [Fact]
    public void Editable_Filter_No_Match_Shows_Empty_Indicator()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "NoMatch";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            popupFrame.GetVisualDescendants()
                      .OfType<AtomUIComboBoxItem>()
                      .Where(item => item.IsVisible && item.Content is string)
                      .ShouldBeEmpty();

            var emptyIndicator = GetVisualDescendant<Control>(popupFrame, "PART_EmptyIndicator");
            emptyIndicator.IsVisible.ShouldBeTrue(
                "When editable filtering removes every candidate, the popup should show the standard empty state instead of a blank panel.");
            emptyIndicator.GetVisualDescendants()
                          .OfType<Empty>()
                          .SingleOrDefault()
                          .ShouldNotBeNull();
        });
    }

    [Fact]
    public void Editable_Filter_Candidate_Selection_Maps_Back_To_Source_ComboBoxItem()
    {
        var firstItem  = new AtomUIComboBoxItem { Content = "Alpha" };
        var secondItem = new AtomUIComboBoxItem { Content = "Alpine" };
        var thirdItem  = new AtomUIComboBoxItem { Content = "Beta" };
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(firstItem);
        comboBox.Items.Add(secondItem);
        comboBox.Items.Add(thirdItem);

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text = "Alpine";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var candidateRow = popupFrame.GetVisualDescendants()
                                     .OfType<AtomUIComboBoxItem>()
                                     .Single(item => item.IsVisible && item.Content == secondItem.Content);
            comboBox.UpdateSelectionFromEvent(candidateRow, new RoutedEventArgs());
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedItem.ShouldBeSameAs(secondItem);
            comboBox.SelectedIndex.ShouldBe(1);
        });
    }

    [Fact]
    public void Editable_Filter_Clicking_Candidate_Closes_DropDown()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Gamma" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Delta" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var candidateRow = popupFrame.GetVisualDescendants()
                                     .OfType<AtomUIComboBoxItem>()
                                     .Single(item => item.IsVisible && item.Content?.ToString() == "Gamma");

            ClickControl(window, candidateRow);
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(3);
            comboBox.Text.ShouldBe("Gamma");
            comboBox.IsDropDownOpen.ShouldBeFalse(
                "Selecting a candidate from the editable filtering popup must commit the option and close the popup.");
        });
    }

    [Fact]
    public void Editable_Filter_Clicking_Candidate_Does_Not_Transiently_Select_All_Text()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Gamma" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "G";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var textBox   = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            var textLength = textBox.Text?.Length ?? 0;
            textBox.SelectionStart = textLength;
            textBox.SelectionEnd   = textLength;
            textBox.CaretIndex     = textLength;

            var observedFullSelection = false;
            void TrackSelection()
            {
                var currentTextLength = textBox.Text?.Length ?? 0;
                if (currentTextLength > 0 &&
                    textBox.SelectionStart == 0 &&
                    textBox.SelectionEnd == currentTextLength)
                {
                    observedFullSelection = true;
                }
            }

            using var selectionStartSubscription = textBox.GetObservable(AvaloniaTextBox.SelectionStartProperty)
                                                          .Subscribe(_ => TrackSelection());
            using var selectionEndSubscription = textBox.GetObservable(AvaloniaTextBox.SelectionEndProperty)
                                                        .Subscribe(_ => TrackSelection());

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var candidateRow = popupFrame.GetVisualDescendants()
                                         .OfType<AtomUIComboBoxItem>()
                                         .Single(item => item.IsVisible &&
                                                         item.Content?.ToString() == "Gamma");

            ClickControl(window, candidateRow);
            Dispatcher.UIThread.RunJobs();

            comboBox.Text.ShouldBe("Gamma");
            comboBox.IsDropDownOpen.ShouldBeFalse();
            observedFullSelection.ShouldBeFalse(
                "Committing a popup candidate must not run Avalonia ComboBox's editable SelectAll focus behavior and then collapse it afterward.");
        });
    }

    [Fact]
    public void Editable_Filter_Clicking_Current_Filtered_Candidate_Closes_DropDown()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Gamma" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Gamma";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(3);

            var popupFrame = GetVisualDescendant<Border>(window, "PopupFrame");
            var candidateRow = popupFrame.GetVisualDescendants()
                                     .OfType<AtomUIComboBoxItem>()
                                     .Single(item => item.IsVisible && item.Content?.ToString() == "Gamma");

            ClickControl(window, candidateRow);
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(3);
            comboBox.Text.ShouldBe("Gamma");
            comboBox.IsDropDownOpen.ShouldBeFalse(
                "Clicking the currently selected filtered candidate must still close the popup even when SelectedItem does not change.");
        });
    }

    [Fact]
    public void Editable_Filter_Down_Key_Activates_Candidate_Without_Changing_Selection()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.Focus(NavigationMethod.Pointer);
            Dispatcher.UIThread.RunJobs();

            PressKey(window, Key.Down, PhysicalKey.ArrowDown);
            Dispatcher.UIThread.RunJobs();

            var visibleRows = GetVisibleStringComboBoxItems(window);
            visibleRows.Select(item => item.Content).ShouldBe(["Alpha", "Alpine"]);
            IsCandidateSelected(visibleRows[0]).ShouldBeTrue(
                "Down should move the keyboard candidate highlight to the first visible option.");
            IsCandidateSelected(visibleRows[1]).ShouldBeFalse();
            comboBox.SelectedIndex.ShouldBe(-1,
                "Moving the keyboard candidate must not commit the real ComboBox selection.");

            PressKey(window, Key.Down, PhysicalKey.ArrowDown);
            Dispatcher.UIThread.RunJobs();

            IsCandidateSelected(visibleRows[0]).ShouldBeFalse();
            IsCandidateSelected(visibleRows[1]).ShouldBeTrue(
                "Repeated Down should move the candidate highlight without committing selection.");
            comboBox.SelectedIndex.ShouldBe(-1);
        });
    }

    [Fact]
    public void Pointer_And_Keyboard_Navigation_Share_One_Active_Candidate()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var visibleRows = GetVisibleStringComboBoxItems(window);
            MovePointerTo(window, visibleRows[0]);

            IsCandidateSelected(visibleRows[0]).ShouldBeTrue();
            IsCandidateSelected(visibleRows[1]).ShouldBeFalse();

            PressKey(window, Key.Down, PhysicalKey.ArrowDown);
            Dispatcher.UIThread.RunJobs();

            visibleRows[0].IsPointerOver.ShouldBeTrue(
                "keyboard navigation must replace the candidate even while the pointer remains over the old row.");
            IsCandidateSelected(visibleRows[0]).ShouldBeFalse();
            IsCandidateSelected(visibleRows[1]).ShouldBeTrue();
            comboBox.SelectedIndex.ShouldBe(-1);
        });
    }

    [Fact]
    public void Pointer_Move_Then_Enter_Commits_The_Pointer_Candidate()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var visibleRows = GetVisibleStringComboBoxItems(window);
            PressKey(window, Key.Down, PhysicalKey.ArrowDown);
            MovePointerTo(window, visibleRows[1]);
            Dispatcher.UIThread.RunJobs();

            IsCandidateSelected(visibleRows[0]).ShouldBeFalse();
            IsCandidateSelected(visibleRows[1]).ShouldBeTrue();

            PressKey(window, Key.Enter, PhysicalKey.Enter);
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(1);
            comboBox.Text.ShouldBe("Alpine");
            comboBox.IsDropDownOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void Editable_Filter_Enter_Key_Commits_Active_Candidate_And_Closes_DropDown()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.Focus(NavigationMethod.Pointer);
            Dispatcher.UIThread.RunJobs();

            PressKey(window, Key.Down, PhysicalKey.ArrowDown);
            PressKey(window, Key.Enter, PhysicalKey.Enter);
            Dispatcher.UIThread.RunJobs();

            comboBox.SelectedIndex.ShouldBe(0);
            comboBox.Text.ShouldBe("Alpha");
            comboBox.IsDropDownOpen.ShouldBeFalse(
                "Enter should commit the active keyboard candidate and close the popup.");
        });
    }

    [Fact]
    public void Editable_Filter_Up_Key_Activates_Last_Visible_Candidate()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.Focus(NavigationMethod.Pointer);
            Dispatcher.UIThread.RunJobs();

            PressKey(window, Key.Up, PhysicalKey.ArrowUp);
            Dispatcher.UIThread.RunJobs();

            var visibleRows = GetVisibleStringComboBoxItems(window);
            visibleRows.Select(item => item.Content).ShouldBe(["Alpha", "Alpine"]);
            IsCandidateSelected(visibleRows[0]).ShouldBeFalse();
            IsCandidateSelected(visibleRows[1]).ShouldBeTrue(
                "Up should move the keyboard candidate highlight to the last visible option when no candidate is active.");
            comboBox.SelectedIndex.ShouldBe(-1);
        });
    }

    [Fact]
    public void Editable_Filter_Escape_Key_Clears_Candidate_And_Closes_DropDown()
    {
        var comboBox = new AtomUIComboBox
        {
            Width           = 200,
            IsEditable      = true,
            IsFilterEnabled = true,
            IsMotionEnabled = false
        };
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpha" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Alpine" });
        comboBox.Items.Add(new AtomUIComboBoxItem { Content = "Beta" });

        ShowInWindow(comboBox, window =>
        {
            comboBox.Text           = "Al";
            comboBox.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var textBox = GetVisualDescendant<AvaloniaTextBox>(comboBox, "PART_EditableTextBox");
            textBox.Focus(NavigationMethod.Pointer);
            Dispatcher.UIThread.RunJobs();

            PressKey(window, Key.Down, PhysicalKey.ArrowDown);
            Dispatcher.UIThread.RunJobs();

            var visibleRows = GetVisibleStringComboBoxItems(window);
            IsCandidateSelected(visibleRows[0]).ShouldBeTrue();

            PressKey(window, Key.Escape, PhysicalKey.Escape);
            Dispatcher.UIThread.RunJobs();

            comboBox.IsDropDownOpen.ShouldBeFalse("Escape should close the open candidate popup.");
            comboBox.SelectedIndex.ShouldBe(-1, "Escape should not commit the active keyboard candidate.");
            visibleRows.ForEach(row => IsCandidateSelected(row).ShouldBeFalse());
        });
    }

    private static T GetVisualDescendant<T>(Control control, string name)
        where T : Control
    {
        var descendant = control.GetVisualDescendants()
                                .OfType<T>()
                                .SingleOrDefault(x => x.Name == name);
        descendant.ShouldNotBeNull();
        return descendant;
    }

    private static void ClickControl(AvaloniaWindow window, Control control)
    {
        var clickPoint = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);
        clickPoint.ShouldNotBeNull();

        window.MouseMove(clickPoint.Value);
        window.MouseDown(clickPoint.Value, MouseButton.Left);
        window.MouseUp(clickPoint.Value, MouseButton.Left);
    }

    private static void MovePointerTo(AvaloniaWindow window, Control control)
    {
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);
        point.ShouldNotBeNull();

        window.MouseMove(point.Value);
        Dispatcher.UIThread.RunJobs();
    }

    private static List<AtomUIComboBoxItem> GetVisibleStringComboBoxItems(Control root)
    {
        return root.GetVisualDescendants()
                   .OfType<AtomUIComboBoxItem>()
                   .Where(item => item.IsVisible && item.Content is string)
                   .ToList();
    }

    private static bool IsCandidateSelected(AtomUIComboBoxItem item)
    {
        var property = typeof(AtomUIComboBoxItem).GetProperty(
            "IsCandidateSelected",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.ShouldNotBeNull("ComboBoxItem should expose an internal keyboard candidate state.");
        return (bool)property.GetValue(item)!;
    }

    private static void PressKey(AvaloniaWindow window, Key key, PhysicalKey physicalKey)
    {
        window.KeyPress(key, RawInputModifiers.None, physicalKey, null);
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        ShowInWindow(content, _ => assertion());
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 220,
            Content = CreatePopupOverlayHost(content)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            content.Measure(Size.Infinity);
            content.Arrange(new Rect(content.DesiredSize));
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static VisualLayerManager CreatePopupOverlayHost(Control content)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 360,
            Height = 220
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);
        return visualLayerManager;
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
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

    private sealed record DataItem(string Name);

    private sealed class TestComboBox : AtomUIComboBox
    {
        public void PrepareContainerForTest(Control container, object? item, int index)
        {
            PrepareContainerForItemOverride(container, item, index);
        }
    }
}
