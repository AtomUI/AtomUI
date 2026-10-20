using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

public class DialogPopupControlFamilyTests
{
    static DialogPopupControlFamilyTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static TheoryData<string> OpenStateControlFamilies =>
        new()
        {
            "ComboBox",
            "Select",
            "Cascader",
            "TreeSelect",
            "AutoComplete",
            "AutoCompleteSearchEdit",
            "AutoCompleteTextArea",
            "Mentions"
        };

    public static TheoryData<string> PickerControlFamilies =>
        new()
        {
            "DatePicker",
            "TimePicker",
            "RangeDatePicker",
            "RangeTimePicker"
        };

    public static TheoryData<string> ColorPickerControlFamilies =>
        new()
        {
            "ColorPicker",
            "GradientColorPicker"
        };

    [Theory]
    [MemberData(nameof(OpenStateControlFamilies))]
    public void Open_State_Control_Family_In_Dialog_Creates_A_Real_Popup_Host(string family)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var scenario = CreateOpenStateScenario(family);
            using var host = DialogPopupTestHost.Open(scenario.Control);

            scenario.Open(host);
            DialogPopupTestHost.Pump();

            host.FindPopupHost().ShouldNotBeNull();
            scenario.IsOpen().ShouldBeTrue();

            host.LightDismiss();
            scenario.IsOpen().ShouldBeFalse();
            host.AssertNoPopupHosts();
            host.Presenter.Parent.ShouldBeSameAs(host.DialogLayer);
        });
    }

    [Theory]
    [MemberData(nameof(PickerControlFamilies))]
    public void Picker_Family_In_Dialog_Uses_The_Shared_InfoPicker_Popup_Path(string family)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var picker = CreatePicker(family);
            using var host = DialogPopupTestHost.Open(picker);
            var input = picker.GetVisualDescendants()
                              .OfType<TextBox>()
                              .First(control => control.Name == "PART_InfoInputBox");

            host.Click(input);

            host.FindPopupHost().ShouldNotBeNull();

            host.LightDismiss();
            host.AssertNoPopupHosts();
            host.Presenter.Parent.ShouldBeSameAs(host.DialogLayer);
        });
    }

    [Theory]
    [MemberData(nameof(ColorPickerControlFamilies))]
    public void ColorPicker_Family_In_Dialog_Uses_Its_Independent_Popup_Template(string family)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Control picker = family switch
            {
                "ColorPicker" => new AtomUI.Desktop.Controls.ColorPicker { Width = 220, IsMotionEnabled = false },
                "GradientColorPicker" => new AtomUI.Desktop.Controls.GradientColorPicker { Width = 220, IsMotionEnabled = false },
                _ => throw new ArgumentOutOfRangeException(nameof(family), family, null)
            };
            using var host = DialogPopupTestHost.Open(picker);

            host.Click(picker);

            host.FindPopupHost().ShouldNotBeNull();
            host.LightDismiss();
            host.AssertNoPopupHosts();
            host.Presenter.Parent.ShouldBeSameAs(host.DialogLayer);
        });
    }

    [Fact]
    public void Menu_Submenu_In_Dialog_Uses_The_Owning_Window_Popup_Layer()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var child = new AtomUI.Desktop.Controls.MenuItem { Header = "Child" };
            var parent = new AtomUI.Desktop.Controls.MenuItem
            {
                Header = "Parent",
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true,
                Items = { child }
            };
            var menu = new AtomUI.Desktop.Controls.Menu { Items = { parent } };
            using var host = DialogPopupTestHost.Open(menu);

            parent.IsSubMenuOpen = true;

            host.FindPopupHost().ShouldNotBeNull();
            parent.IsSubMenuOpen.ShouldBeTrue();
            host.ClickOutsidePopup();
            parent.IsSubMenuOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void NavMenu_Submenu_In_Dialog_Uses_The_Owning_Window_Popup_Layer()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var parentNode = new AtomUI.Desktop.Controls.NavMenuNode
            {
                Header = "Parent",
                Children =
                {
                    new AtomUI.Desktop.Controls.NavMenuNode { Header = "Child" }
                }
            };
            var menu = new AtomUI.Desktop.Controls.NavMenu
            {
                Width = 240,
                Mode = AtomUI.Desktop.Controls.NavMenuMode.Vertical,
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true,
                Items = { parentNode }
            };
            using var host = DialogPopupTestHost.Open(menu);
            var parent = menu.ContainerFromItem(parentNode)
                             .ShouldBeOfType<AtomUI.Desktop.Controls.NavMenuItem>();

            parent.IsSubMenuOpen = true;

            host.FindPopupHost().ShouldNotBeNull();
            parent.IsSubMenuOpen.ShouldBeTrue();
            host.ClickOutsidePopup();
            parent.IsSubMenuOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void Tour_In_Dialog_Uses_Its_Target_In_The_Owning_Window()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var target = new AtomUI.Desktop.Controls.Button
            {
                Width = 100,
                Height = 32,
                Content = "Tour target"
            };
            var tour = new AtomUI.Desktop.Controls.Tour
            {
                IsMotionEnabled = false,
                IsShowMask = false
            };
            tour.Steps.Add(new AtomUI.Desktop.Controls.TourStep
            {
                Target = target,
                Title = "Tour step",
                Content = "Tour content"
            });
            var content = new Avalonia.Controls.Grid { Children = { target, tour } };
            using var host = DialogPopupTestHost.Open(content);

            tour.IsOpen = true;

            host.FindPopupHost().ShouldNotBeNull();
            tour.IsOpen.ShouldBeTrue();
            TopLevel.GetTopLevel(target).ShouldBeSameAs(host.Window);

            tour.IsOpen = false;
            DialogPopupTestHost.Pump();
            tour.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void DropdownButton_In_Dialog_Opens_And_Invokes_Its_MenuFlyout()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var invoked = false;
            var menuItem = new AtomUI.Desktop.Controls.MenuItem { Header = "Action" };
            menuItem.Click += (_, _) => invoked = true;
            var flyout = new AtomUI.Desktop.Controls.MenuFlyout
            {
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true,
                Items = { menuItem }
            };
            var button = new AtomUI.Desktop.Controls.DropdownButton
            {
                Content = "Open",
                TriggerType = AtomUI.Desktop.Controls.FlyoutTriggerType.Click,
                DropdownFlyout = flyout,
                IsMotionEnabled = false
            };
            using var host = DialogPopupTestHost.Open(button);

            host.Click(button);
            var popupHost = host.FindPopupHost();
            TopLevel.GetTopLevel(popupHost).ShouldBeSameAs(host.Window);
            var popupItem = popupHost.GetVisualDescendants()
                                     .OfType<AtomUI.Desktop.Controls.MenuItem>()
                                     .Single(item => item.Header?.ToString() == "Action");

            host.Click(popupItem);
            invoked.ShouldBeTrue();
            flyout.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void SplitButton_In_Dialog_Opens_Its_Flyout_From_The_Secondary_Button()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var flyout = new AtomUI.Desktop.Controls.Flyout
            {
                Content = new TextBlock { Text = "Split content" },
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true
            };
            var splitButton = new AtomUI.Desktop.Controls.SplitButton
            {
                Content = "Split",
                TriggerType = AtomUI.Desktop.Controls.FlyoutTriggerType.Click,
                Flyout = flyout,
                IsMotionEnabled = false
            };
            using var host = DialogPopupTestHost.Open(splitButton);
            var secondaryButton = splitButton.GetVisualDescendants()
                                             .OfType<AtomUI.Desktop.Controls.Button>()
                                             .Single(button => button.Name == "PART_SecondaryButton");

            host.Click(secondaryButton);
            TopLevel.GetTopLevel(host.FindPopupHost()).ShouldBeSameAs(host.Window);
            flyout.IsOpen.ShouldBeTrue();

            host.LightDismiss();
            flyout.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void PopupConfirm_In_Dialog_Confirms_And_Closes_Its_Flyout()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var confirmed = false;
            var popupConfirm = new AtomUI.Desktop.Controls.PopupConfirm
            {
                Content = new AtomUI.Desktop.Controls.Button { Content = "Delete" },
                Title = "Confirm",
                OkText = "Confirm",
                CancelText = "Cancel",
                IsMotionEnabled = false,
                ShouldUseOverlayPopup = true
            };
            popupConfirm.Confirmed += (_, _) => confirmed = true;
            using var host = DialogPopupTestHost.Open(popupConfirm);

            popupConfirm.ShowFlyout(true);
            var popupHost = host.FindPopupHost();
            var okButton = popupHost.GetVisualDescendants()
                                    .OfType<AtomUI.Desktop.Controls.Button>()
                                    .Single(button => button.Name == "PART_OkButton");

            host.Click(okButton);
            confirmed.ShouldBeTrue();
            popupConfirm.Flyout!.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void AvatarGroup_Fold_In_Dialog_Opens_Its_Flyout()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var avatarGroup = new AtomUI.Desktop.Controls.AvatarGroup
            {
                MaxDisplayCount = 2,
                FoldAvatarFlyoutTriggerType = AtomUI.Desktop.Controls.FlyoutTriggerType.Click,
                IsMotionEnabled = false,
                Children =
                {
                    new AtomUI.Controls.Avatar { Text = "A" },
                    new AtomUI.Controls.Avatar { Text = "B" },
                    new AtomUI.Controls.Avatar { Text = "C" },
                    new AtomUI.Controls.Avatar { Text = "D" }
                }
            };
            using var host = DialogPopupTestHost.Open(avatarGroup);
            var foldHost = avatarGroup.GetVisualDescendants()
                                      .OfType<AtomUI.Desktop.Controls.FlyoutHost>()
                                      .Single();
            var foldAvatar = foldHost.Content.ShouldBeOfType<AtomUI.Controls.Avatar>();

            host.Click(foldAvatar);
            TopLevel.GetTopLevel(host.FindPopupHost()).ShouldBeSameAs(host.Window);
            foldHost.Flyout!.IsOpen.ShouldBeTrue();

            var foldFlyout = foldHost.Flyout;
            avatarGroup.MaxDisplayCount = null;
            host.WaitForClosed(() => foldFlyout.IsOpen);
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void Transfer_Dropdown_In_Dialog_Opens_Its_MenuFlyout()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var transfer = new AtomUI.Desktop.Controls.ListTransfer
            {
                Width = 360,
                Height = 220,
                PageSize = 2,
                IsMotionEnabled = false,
                ItemsSource = Enumerable.Range(1, 4)
                                        .Select(index => new ListItemData
                                        {
                                            ItemKey = $"item-{index}",
                                            Content = $"Item {index}"
                                        })
                                        .ToList()
            };
            using var host = DialogPopupTestHost.Open(transfer);
            var dropdown = transfer.GetVisualDescendants()
                                   .OfType<AtomUI.Desktop.Controls.TransferSelectDropdown>()
                                   .First();

            host.Invoke(dropdown);
            var menuFlyout = dropdown.Flyout.ShouldBeOfType<AtomUI.Desktop.Controls.MenuFlyout>();
            menuFlyout.IsOpen.ShouldBeTrue();
            TopLevel.GetTopLevel(host.FindPopupHost()).ShouldBeSameAs(host.Window);

            host.LightDismiss();
            menuFlyout.IsOpen.ShouldBeFalse();
            host.AssertNoPopupHosts();
        });
    }

    [Fact]
    public void TabControl_Overflow_In_Dialog_Opens_Its_MenuFlyout()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            var tabControl = new AtomUI.Desktop.Controls.TabControl
            {
                Width = 180,
                Height = 180,
                SelectedIndex = 0,
                IsMotionEnabled = false
            };
            foreach (var index in Enumerable.Range(1, 10))
            {
                tabControl.Items.Add(new AtomUI.Desktop.Controls.TabItem
                {
                    Header = $"Long tab {index}",
                    Content = $"Content {index}"
                });
            }

            using var host = DialogPopupTestHost.Open(tabControl);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            DialogPopupTestHost.Pump();
            var scrollViewer = tabControl.GetVisualDescendants()
                                         .OfType<AtomUI.Desktop.Controls.TabScrollViewer>()
                                         .Single();
            scrollViewer.Extent.Width.ShouldBeGreaterThan(scrollViewer.Viewport.Width);
            var menuIndicator = scrollViewer.GetVisualDescendants()
                                            .OfType<AtomUI.Desktop.Controls.IconButton>()
                                            .Single(button => button.Name == "PART_ScrollMenuIndicator");

            host.Click(menuIndicator);
            var popupHost = host.FindPopupHost();
            TopLevel.GetTopLevel(popupHost).ShouldBeSameAs(host.Window);

            host.LightDismiss();
            host.AssertNoPopupHosts();
        });
    }

    private static OpenStateScenario CreateOpenStateScenario(string family)
    {
        switch (family)
        {
            case "ComboBox":
            {
                var control = new AtomUI.Desktop.Controls.ComboBox
                {
                    Width = 240,
                    IsMotionEnabled = false,
                    Items =
                    {
                        new AtomUI.Desktop.Controls.ComboBoxItem { Content = "One" },
                        new AtomUI.Desktop.Controls.ComboBoxItem { Content = "Two" }
                    }
                };
                return new OpenStateScenario(control, _ => control.IsDropDownOpen = true, () => control.IsDropDownOpen);
            }
            case "Select":
            {
                var control = new AtomUI.Desktop.Controls.Select
                {
                    Width = 240,
                    IsMotionEnabled = false,
                    OptionsSource =
                    [
                        new AtomUI.Desktop.Controls.SelectOption { Header = "One" },
                        new AtomUI.Desktop.Controls.SelectOption { Header = "Two" }
                    ]
                };
                return new OpenStateScenario(control, _ => control.IsDropDownOpen = true, () => control.IsDropDownOpen);
            }
            case "Cascader":
            {
                var control = new AtomUI.Desktop.Controls.Cascader
                {
                    Width = 240,
                    IsMotionEnabled = false,
                    OptionsSource = new AtomUI.Desktop.Controls.ICascaderOption[]
                    {
                        new AtomUI.Desktop.Controls.CascaderOption { Header = "One", Value = "one", IsLeaf = true },
                        new AtomUI.Desktop.Controls.CascaderOption { Header = "Two", Value = "two", IsLeaf = true }
                    }
                };
                return new OpenStateScenario(control, _ => control.IsDropDownOpen = true, () => control.IsDropDownOpen);
            }
            case "TreeSelect":
            {
                var control = new AtomUI.Desktop.Controls.TreeSelect
                {
                    Width = 240,
                    IsMotionEnabled = false,
                    ItemsSource =
                    [
                        new AtomUI.Desktop.Controls.TreeItemNode { Header = "One", Value = "one", IsLeaf = true },
                        new AtomUI.Desktop.Controls.TreeItemNode { Header = "Two", Value = "two", IsLeaf = true }
                    ]
                };
                return new OpenStateScenario(control, _ => control.IsDropDownOpen = true, () => control.IsDropDownOpen);
            }
            case "AutoComplete":
            {
                var control = new AtomUI.Desktop.Controls.AutoComplete();
                return CreateAutoCompleteScenario(control);
            }
            case "AutoCompleteSearchEdit":
            {
                var control = new AtomUI.Desktop.Controls.AutoCompleteSearchEdit();
                return CreateAutoCompleteScenario(control);
            }
            case "AutoCompleteTextArea":
            {
                var control = new AtomUI.Desktop.Controls.AutoCompleteTextArea();
                return CreateAutoCompleteScenario(control);
            }
            case "Mentions":
            {
                var control = new AtomUI.Desktop.Controls.Mentions
                {
                    Width = 240,
                    IsMotionEnabled = false,
                    OptionsSource =
                    [
                        new AtomUI.Desktop.Controls.MentionOption { Header = "One", Value = "one" },
                        new AtomUI.Desktop.Controls.MentionOption { Header = "Two", Value = "two" }
                    ]
                };
                return new OpenStateScenario(control, _ => control.IsDropDownOpen = true, () => control.IsDropDownOpen);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(family), family, null);
        }
    }

    private static OpenStateScenario CreateAutoCompleteScenario(
        AtomUI.Desktop.Controls.AbstractAutoComplete control)
    {
        control.Width = 240;
        control.IsMotionEnabled = false;
        control.MinimumPrefixLength = 0;
        control.OptionsSource =
        [
            new AtomUI.Desktop.Controls.AutoCompleteOption { Header = "One", Content = "One" },
            new AtomUI.Desktop.Controls.AutoCompleteOption { Header = "Two", Content = "Two" }
        ];
        return new OpenStateScenario(
            control,
            _ => control.IsDropDownOpen = true,
            () => control.IsDropDownOpen);
    }

    private static InfoPickerInput CreatePicker(string family)
    {
        InfoPickerInput picker = family switch
        {
            "DatePicker" => new AtomUI.Desktop.Controls.DatePicker(),
            "TimePicker" => new AtomUI.Desktop.Controls.TimePicker(),
            "RangeDatePicker" => new AtomUI.Desktop.Controls.RangeDatePicker(),
            "RangeTimePicker" => new AtomUI.Desktop.Controls.RangeTimePicker(),
            _ => throw new ArgumentOutOfRangeException(nameof(family), family, null)
        };
        picker.Width = 240;
        picker.PlaceholderText = "Select a value";
        picker.IsMotionEnabled = false;
        return picker;
    }

    private sealed record OpenStateScenario(
        Control Control,
        Action<DialogPopupTestHost> Open,
        Func<bool> IsOpen);
}
