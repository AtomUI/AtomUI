using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.SelectControl;

public class SelectBehaviorTests
{
    static SelectBehaviorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pinned_Open_Request_Opens_Select_And_Its_Template_Popup()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [new SelectOption { Header = "Jack", Content = "jack" }]
        };

        ShowInWindow(select, () =>
        {
            select.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();

            var popup = GetVisualDescendant<Popup>(select, "PART_Popup");
            select.IsDropDownOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_Select_Rejects_Business_Close_Request()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [new SelectOption { Header = "Jack", Content = "jack" }]
        };

        ShowInWindow(select, () =>
        {
            select.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = GetVisualDescendant<Popup>(select, "PART_Popup");

            select.IsDropDownOpen = false;
            Dispatcher.UIThread.RunJobs();

            select.IsDropDownOpen.ShouldBeTrue();
            popup.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Pinned_Select_Detach_Closes_Popup_And_Reattach_Reopens_It()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [new SelectOption { Header = "Jack", Content = "jack" }]
        };
        var window = CreateWindow(select);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var visualLayerManager = window.Content.ShouldBeOfType<VisualLayerManager>();
            var overlayPanel = visualLayerManager.Child.ShouldBeOfType<ScopeAwareOverlayLayerPanel>();

            select.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var popup = GetVisualDescendant<Popup>(select, "PART_Popup");
            popup.IsOpen.ShouldBeTrue();

            overlayPanel.Children.Remove(select);
            Dispatcher.UIThread.RunJobs();

            select.IsPopupPinnedOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeFalse();
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldBeEmpty();

            overlayPanel.Children.Add(select);
            Dispatcher.UIThread.RunJobs();

            select.IsDropDownOpen.ShouldBeTrue();
            popup.IsOpen.ShouldBeTrue(
                $"selectAttached={select.IsAttachedToVisualTree()}, popupAttached={popup.IsAttachedToVisualTree()}, " +
                $"popupPinned={popup.IsPopupPinnedOpen}, child={popup.Child is not null}, " +
                $"placementTarget={popup.PlacementTarget?.GetType().Name ?? "null"}, " +
                $"suspended={GetPrivateField<bool>(popup, "_isPinnedOpenSuspended")}, " +
                $"trackedTarget={GetPrivateField<Control?>(popup, "_pinnedOpenTrackingTarget")?.GetType().Name ?? "null"}");
            window.GetVisualDescendants().OfType<OverlayPopupHost>().ShouldHaveSingleItem();
        }
        finally
        {
            select.IsPopupPinnedOpen = false;
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Pinned_Select_Template_Replacement_Closes_Old_Popup_And_Opens_New_Popup()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [new SelectOption { Header = "Jack", Content = "jack" }]
        };

        ShowInWindow(select, () =>
        {
            select.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            var oldPopup = GetVisualDescendant<Popup>(select, "PART_Popup");
            var template = select.Template;
            template.ShouldNotBeNull();

            select.Template = null;
            select.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            oldPopup.IsOpen.ShouldBeFalse();

            select.Template = template;
            select.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            var newPopup = GetVisualDescendant<Popup>(select, "PART_Popup");
            newPopup.ShouldNotBeSameAs(oldPopup);
            select.IsDropDownOpen.ShouldBeTrue();
            newPopup.IsPopupPinnedOpen.ShouldBeTrue();
            newPopup.IsOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void OptionsSource_Replacement_Preserves_Single_Selection_By_Content()
    {
        var firstLucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var select = new Desktop.Controls.Select
        {
            OptionsSource  = [new SelectOption { Header = "Jack", Content = "jack" }, firstLucy],
            SelectedOption = firstLucy
        };
        var secondLucy = new SelectOption
        {
            Header  = "Lucy Updated",
            Content = "lucy"
        };

        select.OptionsSource =
        [
            new SelectOption { Header = "Jack Updated", Content = "jack" },
            secondLucy
        ];

        select.SelectedOption.ShouldBeSameAs(secondLucy);
    }

    [Fact]
    public void OptionsSource_Replacement_Refreshes_Localized_Multiple_Tag_Layout()
    {
        var firstOption = new SelectOption
        {
            Header  = "A",
            Content = "a"
        };
        var secondOption = new SelectOption
        {
            Header  = "B",
            Content = "b"
        };
        var select = new Desktop.Controls.Select
        {
            Width              = 240,
            Mode               = SelectMode.Multiple,
            IsFilterEnabled    = true,
            IsResponsiveTagMode = true,
            MaxTagCount        = 1,
            OptionsSource      = [firstOption, secondOption],
            SelectedOptions    = [firstOption, secondOption]
        };
        var firstLocalizedOption = new SelectOption
        {
            Header  = "A much longer localized option",
            Content = "a"
        };
        var secondLocalizedOption = new SelectOption
        {
            Header  = "B much longer localized option",
            Content = "b"
        };
        var firstLocalizedHeader = (string)firstLocalizedOption.Header!;

        ShowInWindow(select, () =>
        {
            var selectedOptionsBox = GetVisualDescendant<SelectResultOptionsBox>(select, "SelectedOptionsBox");
            var maxCountPanel = GetVisualDescendant<SelectMaxTagAwarePanel>(select, "PART_MaxCountAwarePanel");

            select.OptionsSource = [firstLocalizedOption, secondLocalizedOption];
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldBe([firstLocalizedOption, secondLocalizedOption]);
            selectedOptionsBox.GetVisualDescendants()
                              .OfType<SelectTag>()
                              .ShouldContain(tag => tag.Text == firstLocalizedHeader);

            var infoTag = maxCountPanel.Children
                                        .OfType<SelectRemainInfoTag>()
                                        .Single();
            var searchTextBox = GetTagsSearchTextBox(select);

            infoTag.IsVisible.ShouldBeTrue();
            infoTag.Bounds.Right.ShouldBeLessThanOrEqualTo(maxCountPanel.Bounds.Right + 0.001);
            searchTextBox.Bounds.X.ShouldBeGreaterThanOrEqualTo(infoTag.Bounds.Right - 0.001);
            searchTextBox.Bounds.Right.ShouldBeLessThanOrEqualTo(maxCountPanel.Bounds.Right + 0.001);
        });
    }

    [Fact]
    public void DefaultValues_Selects_Single_Option_On_Load()
    {
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var select = new Desktop.Controls.Select
        {
            OptionsSource =
            [
                new SelectOption { Header = "Jack", Content = "jack" },
                lucy
            ],
            DefaultValues = ["lucy"]
        };

        ShowInWindow(select, () => select.SelectedOption.ShouldBeSameAs(lucy));
    }

    [Fact]
    public void SelectedOption_DefaultBinding_Updates_ViewModel_When_Control_Value_Changes()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var viewModel = new SelectBindingViewModel
        {
            SelectedOption = jack
        };
        var select = new Desktop.Controls.Select
        {
            OptionsSource = [jack, lucy]
        };
        select.Bind(
            Desktop.Controls.Select.SelectedOptionProperty,
            new Binding(nameof(SelectBindingViewModel.SelectedOption))
            {
                Source = viewModel
            });

        ShowInWindow(select, () =>
        {
            select.SelectedOption.ShouldBeSameAs(jack);

            select.SelectedOption = lucy;
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedOption.ShouldBeSameAs(lucy);
        });
    }

    [Fact]
    public void SelectedOptions_DefaultBinding_Updates_ViewModel_When_Control_Value_Changes()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var viewModel = new SelectBindingViewModel();
        var select = new Desktop.Controls.Select
        {
            Mode          = SelectMode.Multiple,
            OptionsSource = [jack, lucy]
        };
        select.Bind(
            Desktop.Controls.Select.SelectedOptionsProperty,
            new Binding(nameof(SelectBindingViewModel.SelectedOptions))
            {
                Source = viewModel
            });

        ShowInWindow(select, () =>
        {
            select.SelectedOptions = [jack, lucy];
            Dispatcher.UIThread.RunJobs();

            viewModel.SelectedOptions.ShouldNotBeNull();
            viewModel.SelectedOptions.ShouldBe([jack, lucy]);
        });
    }

    [Fact]
    public void SelectedOptions_ObservableCollection_Mutation_Refreshes_Result_And_Candidate_Selection()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var viewModel = new SelectBindingViewModel
        {
            SelectedOptions = new ObservableCollection<ISelectOption>
            {
                jack
            }
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Multiple,
            IsFilterEnabled = false,
            OptionsSource   = [jack, lucy]
        };
        select.Bind(
            Desktop.Controls.Select.SelectedOptionsProperty,
            new Binding(nameof(SelectBindingViewModel.SelectedOptions))
            {
                Source = viewModel
            });

        ShowInWindow(select, () =>
        {
            select.GetVisualDescendants()
                  .OfType<SelectTag>()
                  .Single(tag => tag.Text == "Jack");

            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            candidateList.SelectedItems.ShouldNotBeNull();
            candidateList.SelectedItems.Cast<ISelectOption>().ShouldContain(jack);

            viewModel.SelectedOptions!.Add(lucy);
            Dispatcher.UIThread.RunJobs();

            select.GetVisualDescendants()
                  .OfType<SelectTag>()
                  .Single(tag => tag.Text == "Lucy");
            candidateList.SelectedItems.Cast<ISelectOption>().ShouldContain(lucy);

            viewModel.SelectedOptions.Remove(jack);
            Dispatcher.UIThread.RunJobs();

            select.GetVisualDescendants()
                  .OfType<SelectTag>()
                  .ShouldNotContain(tag => tag.Text == "Jack");
            candidateList.SelectedItems.Cast<ISelectOption>().ShouldNotContain(jack);
        });
    }

    [Fact]
    public void Form_Value_Uses_Mode_Specific_Selection_Property()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var select = new Desktop.Controls.Select();
        var formItem = (IFormItemAware)select;

        formItem.SetFormValue(jack);
        formItem.GetFormValue().ShouldBeSameAs(jack);
        select.SelectedOption.ShouldBeSameAs(jack);

        select.Mode = SelectMode.Multiple;
        IList<ISelectOption> selectedOptions = [jack, lucy];

        formItem.SetFormValue(selectedOptions);
        formItem.GetFormValue().ShouldBeSameAs(selectedOptions);
        select.SelectedOptions.ShouldBeSameAs(selectedOptions);

        formItem.ClearFormValue();

        select.SelectedOptions.ShouldBeNull();
    }

    [Fact]
    public void Template_Binds_Right_AddOn_Count_And_Handle_State_From_Axaml()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var rightAddOn  = new TextBlock { Text = "extra" };
        var suffixIcon  = new DownOutlined();
        var loadingIcon = new LoadingOutlined();
        var select = new Desktop.Controls.Select
        {
            Width                   = 240,
            Mode                    = SelectMode.Multiple,
            IsFilterEnabled         = true,
            IsAllowClear            = true,
            IsMotionEnabled         = false,
            MaxCount                = 3,
            IsShowMaxCountIndicator = true,
            SelectedOptions         = [jack, lucy],
            ContentRightAddOn       = rightAddOn,
            SuffixIcon              = suffixIcon,
            SuffixLoadingIcon       = loadingIcon
        };

        ShowInWindow(select, () =>
        {
            var indicator = GetVisualDescendant<SelectMaxCountIndicator>(select, "PART_SelectMaxCountIndicator");
            indicator.MaxCount.ShouldBe(3);
            indicator.SelectedCount.ShouldBe(2);
            indicator.IsVisible.ShouldBeTrue();

            var contentPresenter = GetVisualDescendant<ContentPresenter>(select, "PART_ContentRightAddOnPresenter");
            contentPresenter.Content.ShouldBeSameAs(rightAddOn);
            contentPresenter.IsVisible.ShouldBeTrue();

            var handle = GetVisualDescendant<SelectHandle>(select, "PART_SelectHandle");
            handle.OpenIndicator.ShouldBeSameAs(suffixIcon);
            handle.LoadingIcon.ShouldBeSameAs(loadingIcon);
            handle.IsFilterEnabled.ShouldBeTrue();
            handle.IsMotionEnabled.ShouldBeFalse();
            handle.IsAllowClear.ShouldBeTrue();
            handle.IsSelectionEmpty.ShouldBeFalse();
        });
    }

    [Fact]
    public void Template_Relays_AddOnDecoratedBox_Input_State_To_Handle()
    {
        var select = new Desktop.Controls.Select
        {
            Width = 240
        };

        ShowInWindow(select, () =>
        {
            var addOnBox = GetVisualDescendant<AddOnDecoratedBox>(
                select,
                AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var handle = GetVisualDescendant<SelectHandle>(select, "PART_SelectHandle");

            addOnBox.IsInnerBoxHover = true;
            addOnBox.IsInnerBoxPressed = true;

            handle.IsInputHover.ShouldBeTrue();
            handle.IsInputPressed.ShouldBeTrue();
        });
    }

    [Fact]
    public void Custom_Size_Single_Selected_Text_Is_Vertically_Centered()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Height          = 38,
            FontSize        = 15,
            SizeType        = CustomizableSizeType.Custom,
            IsAllowClear    = true,
            OptionsSource   = [jack],
            SelectedOption  = jack,
            PlaceholderText = "Please select"
        };

        ShowInWindow(select, () =>
        {
            var addOnBox = GetVisualDescendant<AddOnDecoratedBox>(
                select,
                AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var textPresenter = GetVisualDescendant<TextPresenter>(select, "PART_TextPresenter");

            var textTop = textPresenter.TranslatePoint(default, addOnBox);
            textTop.ShouldNotBeNull();

            var textCenterY = textTop.Value.Y + textPresenter.Bounds.Height / 2;
            var boxCenterY  = addOnBox.Bounds.Height / 2;

            Math.Abs(textCenterY - boxCenterY).ShouldBeLessThanOrEqualTo(1.0);
        });
    }

    [Fact]
    public void Single_Filter_Input_Uses_Outer_Content_Padding_Only()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsFilterEnabled = true,
            PlaceholderText = "Please select"
        };

        ShowInWindow(select, () =>
        {
            var addOnBox = GetVisualDescendant<AddOnDecoratedBox>(
                select,
                AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var searchTextBox = GetVisualDescendant<SelectFilterTextBox>(select, "PART_SingleFilterInput");

            addOnBox.ContentFrame.ShouldNotBeNull();
            addOnBox.ContentFrame!.Padding.Left.ShouldBeGreaterThan(0);
            AssertEmbeddedSelectFilterTextBox(searchTextBox);
        });
    }

    [Theory]
    [InlineData(SelectMode.Multiple)]
    [InlineData(SelectMode.Tags)]
    public void Multiple_And_Tags_Search_Input_Uses_Outer_Content_Padding_Only(SelectMode mode)
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = mode,
            IsFilterEnabled = true,
            SelectedOptions = [jack]
        };

        ShowInWindow(select, () =>
        {
            var searchTextBox = GetTagsSearchTextBox(select);

            AssertEmbeddedSelectFilterTextBox(searchTextBox);
        });
    }

    [Theory]
    [InlineData(SelectMode.Multiple)]
    [InlineData(SelectMode.Tags)]
    public void Multiple_And_Tags_Filtering_Keeps_Selected_Options_Hidden_By_Filter(SelectMode mode)
    {
        var apple  = new SelectOption { Header = "Apple", Content  = "apple" };
        var banana = new SelectOption { Header = "Banana", Content = "banana" };
        var cherry = new SelectOption { Header = "Cherry", Content = "cherry" };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = mode,
            IsFilterEnabled = true,
            IsMotionEnabled = false,
            Filter          = new StringContainsFilter(),
            OptionsSource   = [apple, banana, cherry],
            SelectedOptions = [apple, cherry]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var searchTextBox = GetTagsSearchTextBox(select);
            searchTextBox.Focus();
            searchTextBox.Text = "ban";
            Dispatcher.UIThread.RunJobs();

            select.FilterValue.ShouldBe("ban");
            select.SelectedOptions.ShouldNotBeNull();
            select.SelectedOptions.ShouldBe([apple, cherry], ignoreOrder: true);

            var candidateList = GetCandidateList(select);
            RaiseCandidateKeyDown(candidateList, Key.Down);
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(banana);

            RaiseCandidateKeyDown(candidateList, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldBe([apple, banana, cherry], ignoreOrder: true);
        });
    }

    [Theory]
    [InlineData(SelectMode.Multiple)]
    [InlineData(SelectMode.Tags)]
    public void Multiple_And_Tags_Placeholder_Hides_While_Search_Preedit_Text_Is_Rendered(SelectMode mode)
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = mode,
            IsFilterEnabled = true,
            PlaceholderText = "Please select"
        };

        ShowInWindow(select, () =>
        {
            var placeholder   = GetVisualDescendant<Avalonia.Controls.TextBlock>(select, "PlaceholderText");
            var searchTextBox = GetTagsSearchTextBox(select);
            var presenter     = GetVisualDescendant<TextPresenter>(searchTextBox, "PART_TextPresenter");

            placeholder.IsVisible.ShouldBeTrue();

            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, "测");
            Dispatcher.UIThread.RunJobs();

            placeholder.IsVisible.ShouldBeFalse(
                "IME preedit text is rendered by the Select search TextPresenter before Text is committed, so the outer placeholder must not remain over it.");

            presenter.SetCurrentValue(TextPresenter.PreeditTextProperty, string.Empty);
            Dispatcher.UIThread.RunJobs();

            placeholder.IsVisible.ShouldBeTrue(
                "The placeholder should return when search preedit text is cleared and the selection is still empty.");
        });
    }

    [Fact]
    public void Custom_Size_Multiple_Tag_Uses_Input_Content_Height()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Height          = 38,
            Mode            = SelectMode.Multiple,
            SizeType        = CustomizableSizeType.Custom,
            IsFilterEnabled = false,
            SelectedOptions = [jack, lucy]
        };

        ShowInWindow(select, () =>
        {
            var addOnBox = GetVisualDescendant<AddOnDecoratedBox>(
                select,
                AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var tag = select.GetVisualDescendants()
                            .OfType<SelectTag>()
                            .First(item => item.Text == "Jack");

            addOnBox.ContentFrame.ShouldNotBeNull();
            var contentPadding = addOnBox.ContentFrame!.Padding;
            var border         = addOnBox.InnerBoxBorderThickness;
            var expectedHeight = select.Height -
                                 contentPadding.Top -
                                 contentPadding.Bottom -
                                 border.Top -
                                 border.Bottom;

            tag.Bounds.Height.ShouldBe(expectedHeight, 0.5);
        });
    }

    [Fact]
    public void Custom_Size_Multiple_Tag_Keeps_Minimum_Height_And_Uses_Small_Vertical_Padding_When_Input_Is_Low()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Height          = 28,
            Mode            = SelectMode.Multiple,
            SizeType        = CustomizableSizeType.Custom,
            IsFilterEnabled = false,
            SelectedOptions = [jack]
        };

        ShowInWindow(select, () =>
        {
            var addOnBox = GetVisualDescendant<AddOnDecoratedBox>(
                select,
                AddOnDecoratedBox.AddOnDecoratedBoxPart);
            var tag = select.GetVisualDescendants()
                            .OfType<SelectTag>()
                            .Single(item => item.Text == "Jack");
            var minimumTagHeight = GetThemeResource<double>(SelectTokenKind.MultipleItemHeight);
            var smallPadding     = GetThemeResource<Thickness>(SelectTokenKind.MultiModePaddingSM);

            addOnBox.ContentFrame.ShouldNotBeNull();
            addOnBox.ContentFrame!.Padding.Top.ShouldBe(smallPadding.Top);
            addOnBox.ContentFrame.Padding.Bottom.ShouldBe(smallPadding.Bottom);
            tag.Bounds.Height.ShouldBe(minimumTagHeight, 0.5);
        });
    }

    [Fact]
    public void Committing_Current_Single_Selection_Does_Not_Suppress_Next_External_Selection_Sync()
    {
        var jack = new SelectOption
        {
            Header  = "Jack",
            Content = "jack"
        };
        var lucy = new SelectOption
        {
            Header  = "Lucy",
            Content = "lucy"
        };
        var select = new Desktop.Controls.Select
        {
            Width          = 240,
            OptionsSource  = [jack, lucy],
            SelectedOption = jack
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            candidateList.SelectedItem.ShouldBeSameAs(jack);

            candidateList.RaiseEvent(new RoutedEventArgs(SelectCandidateList.CommitEvent));
            Dispatcher.UIThread.RunJobs();

            select.IsDropDownOpen.ShouldBeFalse();

            select.SelectedOption = lucy;

            candidateList.SelectedItem.ShouldBeSameAs(lucy);
        });
    }

    [Fact]
    public void Pointer_Move_And_Keyboard_Navigation_Share_One_Active_Candidate()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var lucy = new SelectOption { Header = "Lucy", Content = "lucy" };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [jack, lucy]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            var jackContainer = GetCandidateContainer(candidateList, 0);
            var lucyContainer = GetCandidateContainer(candidateList, 1);

            RaisePointerMoved(jackContainer);
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedIndex.ShouldBe(0);
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);
            jackContainer.IsCandidateSelected.ShouldBeTrue();
            lucyContainer.IsCandidateSelected.ShouldBeFalse();
            select.SelectedOption.ShouldBeNull();

            RaiseCandidateKeyDown(candidateList, Key.Down);
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedIndex.ShouldBe(1);
            candidateList.CandidateSelectedItem.ShouldBeSameAs(lucy);
            jackContainer.IsCandidateSelected.ShouldBeFalse();
            lucyContainer.IsCandidateSelected.ShouldBeTrue();
            select.SelectedOption.ShouldBeNull();

            RaisePointerMoved(jackContainer);
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedIndex.ShouldBe(0);
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);
            jackContainer.IsCandidateSelected.ShouldBeTrue();
            lucyContainer.IsCandidateSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Candidate_Direct_Properties_Keep_Index_Item_And_Projection_Consistent()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var lucy = new SelectOption { Header = "Lucy", Content = "lucy" };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [jack, lucy]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            var jackContainer = GetCandidateContainer(candidateList, 0);
            var lucyContainer = GetCandidateContainer(candidateList, 1);

            candidateList.CandidateSelectedItem = lucy;

            candidateList.CandidateSelectedIndex.ShouldBe(1);
            candidateList.CandidateSelectedItem.ShouldBeSameAs(lucy);
            jackContainer.IsCandidateSelected.ShouldBeFalse();
            lucyContainer.IsCandidateSelected.ShouldBeTrue();

            candidateList.CandidateSelectedIndex = 0;

            candidateList.CandidateSelectedIndex.ShouldBe(0);
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);
            jackContainer.IsCandidateSelected.ShouldBeTrue();
            lucyContainer.IsCandidateSelected.ShouldBeFalse();

            candidateList.CandidateSelectedItem = new SelectOption();

            candidateList.CandidateSelectedIndex.ShouldBe(-1);
            candidateList.CandidateSelectedItem.ShouldBeNull();
            jackContainer.IsCandidateSelected.ShouldBeFalse();
            lucyContainer.IsCandidateSelected.ShouldBeFalse();
        });
    }

    [Fact]
    public void Pointer_Move_Then_Enter_Commits_The_Pointer_Candidate()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var lucy = new SelectOption { Header = "Lucy", Content = "lucy" };
        var select = new Desktop.Controls.Select
        {
            Width         = 240,
            OptionsSource = [jack, lucy]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            RaiseCandidateKeyDown(candidateList, Key.Down);
            RaisePointerMoved(GetCandidateContainer(candidateList, 1));
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedItem.ShouldBeSameAs(lucy);

            RaiseCandidateKeyDown(candidateList, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            select.SelectedOption.ShouldBeSameAs(lucy);
            select.IsDropDownOpen.ShouldBeFalse();
        });
    }

    [Fact]
    public void PointerOver_Does_Not_Preserve_A_Second_Candidate_Highlight()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var lucy = new SelectOption { Header = "Lucy", Content = "lucy" };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [jack, lucy]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            var jackContainer = GetCandidateContainer(candidateList, 0);
            var lucyContainer = GetCandidateContainer(candidateList, 1);
            var defaultBg     = GetThemeResource<IBrush>(ListViewTokenKind.ItemBgColor);
            var activeBg      = GetThemeResource<IBrush>(SelectTokenKind.OptionActiveBg);

            RaisePointerMoved(jackContainer);
            ((IPseudoClasses)jackContainer.Classes).Set(":pointerover", true);
            Dispatcher.UIThread.RunJobs();

            jackContainer.Background.ShouldBe(activeBg);

            RaiseCandidateKeyDown(candidateList, Key.Down);
            Dispatcher.UIThread.RunJobs();

            jackContainer.IsCandidateSelected.ShouldBeFalse();
            jackContainer.Background.ShouldBe(defaultBg);
            lucyContainer.IsCandidateSelected.ShouldBeTrue();
            lucyContainer.Background.ShouldBe(activeBg);
        });
    }

    [Fact]
    public void Committed_Selection_Visual_Overrides_Active_Candidate_Visual()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            IsMotionEnabled = false,
            OptionsSource   = [jack],
            SelectedOption  = jack
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            var container     = GetCandidateContainer(candidateList, 0);
            var selectedBg    = GetThemeResource<IBrush>(SelectTokenKind.OptionSelectedBg);

            RaisePointerMoved(container);
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);
            container.IsCandidateSelected.ShouldBeTrue();
            container.IsSelected.ShouldBeTrue();
            container.Background.ShouldBe(selectedBg);
        });
    }

    [Fact]
    public void Popup_Close_Filter_And_MaxCount_Invalidate_Active_Candidate()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var lucy = new SelectOption { Header = "Lucy", Content = "lucy" };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Multiple,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            MaxCount        = 2,
            OptionsSource   = [jack, lucy],
            SelectedOptions = [jack]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            RaisePointerMoved(GetCandidateContainer(candidateList, 1));
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(lucy);

            select.MaxCount = 1;
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedIndex.ShouldBe(-1);
            candidateList.CandidateSelectedItem.ShouldBeNull();

            select.MaxCount = 2;
            RaisePointerMoved(GetCandidateContainer(candidateList, 1));
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(lucy);

            select.FilterValue = "jack";
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedIndex.ShouldBe(-1);
            candidateList.CandidateSelectedItem.ShouldBeNull();

            select.FilterValue = null;
            Dispatcher.UIThread.RunJobs();
            RaisePointerMoved(GetCandidateContainer(candidateList, 0));
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);

            var popup = GetVisualDescendant<Popup>(select, "PART_Popup");
            var popupClosed = false;
            void OnPopupClosed(object? sender, EventArgs args)
            {
                popupClosed = true;
            }

            popup.Closed += OnPopupClosed;
            try
            {
                select.IsDropDownOpen = false;
                // Candidate cleanup follows the physical close, after default leave motion.
                var timeout = Stopwatch.StartNew();
                while (!popupClosed && timeout.Elapsed < TimeSpan.FromSeconds(2))
                {
                    AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
                    Dispatcher.UIThread.RunJobs();
                    Thread.Sleep(5);
                }

                popupClosed.ShouldBeTrue();
                candidateList.CandidateSelectedIndex.ShouldBe(-1);
                candidateList.CandidateSelectedItem.ShouldBeNull();
            }
            finally
            {
                popup.Closed -= OnPopupClosed;
            }
        });
    }

    [Fact]
    public void Candidate_Projection_Is_Restored_After_Container_Context_Reset()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var select = new Desktop.Controls.Select
        {
            Width         = 240,
            IsMotionEnabled = false,
            OptionsSource = [jack]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            var container = GetCandidateContainer(candidateList, 0);
            candidateList.TrySetCandidateItemSelected(jack).ShouldBeTrue();
            container.IsCandidateSelected.ShouldBeTrue();

            InvokeCandidateListHook(
                candidateList,
                "NotifyClearContainerForVirtualizingContext",
                container);
            container.IsCandidateSelected.ShouldBeFalse();

            InvokeCandidateListHook(
                candidateList,
                "NotifyRestoreDefaultContext",
                container,
                jack);
            container.IsCandidateSelected.ShouldBeTrue();
        });
    }

    [Fact]
    public void Navigation_Skips_Disabled_Hidden_And_Group_Candidates()
    {
        var disabled = new SelectOption
        {
            Header    = "Disabled",
            Content   = "disabled",
            Group     = "A",
            IsEnabled = false
        };
        var selected = new SelectOption
        {
            Header  = "Selected",
            Content = "selected",
            Group   = "A"
        };
        var available = new SelectOption
        {
            Header  = "Available",
            Content = "available",
            Group   = "B"
        };
        var select = new Desktop.Controls.Select
        {
            Width                   = 240,
            Mode                    = SelectMode.Multiple,
            IsGroupEnabled          = true,
            GroupPropertySelector   = value => (value as ISelectOption)?.Group,
            IsHideSelectedOptions   = true,
            OptionsSource           = [disabled, selected, available],
            SelectedOptions         = [selected]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            RaiseCandidateKeyDown(candidateList, Key.Down);
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedItem.ShouldBeSameAs(available);

            var groupContainer = candidateList.GetVisualDescendants()
                                              .OfType<SelectCandidateListItem>()
                                              .First(item => item.IsGroupItem);
            RaisePointerMoved(groupContainer);
            Dispatcher.UIThread.RunJobs();

            candidateList.CandidateSelectedItem.ShouldBeSameAs(available);
        });
    }

    [Fact]
    public void Multiple_Mode_Enter_Toggles_Selection_And_Keeps_Active_Candidate()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var select = new Desktop.Controls.Select
        {
            Width         = 240,
            Mode          = SelectMode.Multiple,
            OptionsSource = [jack]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            RaisePointerMoved(GetCandidateContainer(candidateList, 0));
            RaiseCandidateKeyDown(candidateList, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldNotBeNull();
            select.SelectedOptions.ShouldContain(jack);
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);
            select.IsDropDownOpen.ShouldBeTrue();

            RaiseCandidateKeyDown(candidateList, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldNotContain(jack);
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);
        });
    }

    [Fact]
    public void Tags_Mode_With_OptionsSource_Creates_Runtime_Option_Without_Mutating_User_Source()
    {
        var optionsSource = new List<ISelectOption>
        {
            new SelectOption { Header = "Jack", Content = "jack" },
            new SelectOption { Header = "Lucy", Content = "lucy" }
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Tags,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            OptionsSource   = optionsSource
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var searchTextBox = GetTagsSearchTextBox(select);

            searchTextBox.Text = "aaa";
            Dispatcher.UIThread.RunJobs();

            var candidateList = GetCandidateList(select);
            candidateList.TotalItemCount.ShouldBe(1);
            var dynamicOption = candidateList.Items
                                             .OfType<ISelectOption>()
                                             .Single(option => option.IsDynamicAdded);

            dynamicOption.Header.ShouldBe("aaa");
            dynamicOption.Content.ShouldBe("aaa");
            optionsSource.Count.ShouldBe(2);
        });
    }

    [Fact]
    public void Tags_Mode_Enter_From_Search_Input_Adds_Runtime_Candidate()
    {
        var optionsSource = new List<ISelectOption>
        {
            new SelectOption { Header = "Jack", Content = "jack" }
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Tags,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            OptionsSource   = optionsSource
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var searchTextBox = GetTagsSearchTextBox(select);
            searchTextBox.Text = "ssssss";
            Dispatcher.UIThread.RunJobs();

            RaiseSearchInputKeyDown(searchTextBox, Key.Enter);
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldNotBeNull();
            select.SelectedOptions.Single().Header.ShouldBe("ssssss");
            select.SelectedOptions.Single().IsDynamicAdded.ShouldBeTrue();
            optionsSource.Count.ShouldBe(1);
            select.IsDropDownOpen.ShouldBeTrue();
        });
    }

    [Fact]
    public void Tags_Mode_Escape_From_Search_Input_Closes_Dropdown_Without_Adding_Runtime_Candidate()
    {
        var optionsSource = new List<ISelectOption>
        {
            new SelectOption { Header = "Jack", Content = "jack" }
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Tags,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            OptionsSource   = optionsSource
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var searchTextBox = GetTagsSearchTextBox(select);
            searchTextBox.Text = "ssssss";
            Dispatcher.UIThread.RunJobs();

            RaiseSearchInputKeyDown(searchTextBox, Key.Escape);
            Dispatcher.UIThread.RunJobs();

            select.IsDropDownOpen.ShouldBeFalse();
            select.SelectedOptions.ShouldBeNull();
            optionsSource.Count.ShouldBe(1);
        });
    }

    [Fact]
    public void Tags_Mode_Arrow_Keys_From_Search_Input_Navigate_Candidates()
    {
        var jack = new SelectOption { Header = "Jack", Content = "jack" };
        var lucy = new SelectOption { Header = "Lucy", Content = "lucy" };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Tags,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            OptionsSource   = [jack, lucy]
        };

        ShowInWindow(select, () =>
        {
            select.IsDropDownOpen = true;
            Dispatcher.UIThread.RunJobs();

            var searchTextBox = GetTagsSearchTextBox(select);
            var candidateList = GetCandidateList(select);

            RaiseSearchInputKeyDown(searchTextBox, Key.Down);
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);

            RaiseSearchInputKeyDown(searchTextBox, Key.Down);
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(lucy);

            RaiseSearchInputKeyDown(searchTextBox, Key.Up);
            Dispatcher.UIThread.RunJobs();
            candidateList.CandidateSelectedItem.ShouldBeSameAs(jack);
        });
    }

    [Fact]
    public void Tags_Mode_Preserves_Selected_Runtime_Option_When_OptionsSource_Replaced_Without_Formal_Match()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Tags,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            OptionsSource   = [new SelectOption { Header = "Jack", Content = "jack" }]
        };

        ShowInWindow(select, () =>
        {
            var dynamicOption = CreateRuntimeTagOption(select, "aaa");

            select.SelectedOptions.ShouldNotBeNull();
            select.SelectedOptions.Single().ShouldBeSameAs(dynamicOption);

            select.OptionsSource = [new SelectOption { Header = "Lucy", Content = "lucy" }];
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldNotBeNull();
            select.SelectedOptions.Single().ShouldBeSameAs(dynamicOption);
        });
    }

    [Fact]
    public void Tags_Mode_Remaps_Selected_Runtime_Option_When_OptionsSource_Adds_Formal_Match()
    {
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Tags,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            OptionsSource   = [new SelectOption { Header = "Jack", Content = "jack" }]
        };
        var formalOption = new SelectOption
        {
            Header  = "AAA formal",
            Content = "aaa"
        };

        ShowInWindow(select, () =>
        {
            var dynamicOption = CreateRuntimeTagOption(select, "aaa");
            dynamicOption.IsDynamicAdded.ShouldBeTrue();

            select.OptionsSource = [formalOption];
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldNotBeNull();
            select.SelectedOptions.Single().ShouldBeSameAs(formalOption);
            select.SelectedOptions.Single().IsDynamicAdded.ShouldBeFalse();
        });
    }

    [Fact]
    public void Tags_Mode_Clearing_Runtime_Option_Does_Not_Mutate_User_Source()
    {
        var optionsSource = new List<ISelectOption>
        {
            new SelectOption { Header = "Jack", Content = "jack" }
        };
        var select = new Desktop.Controls.Select
        {
            Width           = 240,
            Mode            = SelectMode.Tags,
            IsFilterEnabled = true,
            Filter          = ValueFilterFactory.BuildFilter(ValueFilterMode.Contains),
            OptionsSource   = optionsSource
        };

        ShowInWindow(select, () =>
        {
            CreateRuntimeTagOption(select, "aaa");

            select.ClearValue();
            Dispatcher.UIThread.RunJobs();

            select.SelectedOptions.ShouldBeNull();
            optionsSource.Count.ShouldBe(1);
        });
    }

    [Fact]
    public void Detaching_Select_Cancels_Pending_Async_Options_Load()
    {
        var loader = new PendingSelectOptionsLoader();
        var select = new Desktop.Controls.Select
        {
            Width         = 240,
            OptionsLoader = loader
        };
        var window = CreateWindow(select);
        var cancellationObserved = false;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            InvokeHandleOpenDropRequest(select);

            loader.Started.Wait(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken).ShouldBeTrue();

            window.Close();
            Dispatcher.UIThread.RunJobs();

            cancellationObserved =
                loader.Cancelled.Wait(TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken);
        }
        finally
        {
            loader.Complete();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        cancellationObserved.ShouldBeTrue();
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

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (T)field!.GetValue(target)!;
    }

    private static T GetThemeResource<T>(object key)
    {
        var application = Application.Current;
        application.ShouldNotBeNull();
        application!.TryGetResource(key, application.ActualThemeVariant, out var value).ShouldBeTrue();
        value.ShouldBeAssignableTo<T>();
        return (T)value!;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = CreateWindow(content);

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

    private static AvaloniaWindow CreateWindow(Control content)
    {
        var overlayPanel = new ScopeAwareOverlayLayerPanel
        {
            Width  = 420,
            Height = 320
        };
        overlayPanel.Children.Add(content);

        var visualLayerManager = new VisualLayerManager
        {
            Child = overlayPanel
        };
        EnablePopupOverlayLayer(visualLayerManager);

        var window = new AvaloniaWindow
        {
            Width   = 420,
            Height  = 320,
            Content = visualLayerManager
        };

        return window;
    }

    private static SelectCandidateList GetCandidateList(Desktop.Controls.Select select)
    {
        var field = typeof(Desktop.Controls.Select).GetField(
            "_candidateList",
            BindingFlags.Instance | BindingFlags.NonPublic);

        field.ShouldNotBeNull();
        var candidateList = field.GetValue(select) as SelectCandidateList;
        candidateList.ShouldNotBeNull();
        return candidateList;
    }

    private static SelectCandidateListItem GetCandidateContainer(SelectCandidateList candidateList, int viewIndex)
    {
        var container = candidateList.ContainerFromIndex(viewIndex) as SelectCandidateListItem;
        container.ShouldNotBeNull();
        return container;
    }

    private static void RaisePointerMoved(Control source)
    {
        source.RaiseEvent(new PointerEventArgs(
            InputElement.PointerMovedEvent,
            source,
            new Avalonia.Input.Pointer(Avalonia.Input.Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            new Point(1, 1),
            0,
            new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other),
            KeyModifiers.None));
    }

    private static void RaiseCandidateKeyDown(SelectCandidateList candidateList, Key key)
    {
        candidateList.HandleKeyDown(new KeyEventArgs
        {
            RoutedEvent  = InputElement.KeyDownEvent,
            Source       = candidateList,
            Key          = key,
            PhysicalKey  = key switch
            {
                Key.Enter => PhysicalKey.Enter,
                Key.Up    => PhysicalKey.ArrowUp,
                Key.Down  => PhysicalKey.ArrowDown,
                _         => PhysicalKey.None
            },
            KeyModifiers = KeyModifiers.None
        });
    }

    private static void InvokeCandidateListHook(
        SelectCandidateList candidateList,
        string methodName,
        params object?[] arguments)
    {
        var method = typeof(SelectCandidateList).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        method.ShouldNotBeNull();
        method!.Invoke(candidateList, arguments);
    }

    private static ISelectOption CreateRuntimeTagOption(Desktop.Controls.Select select, string text)
    {
        select.IsDropDownOpen = true;
        Dispatcher.UIThread.RunJobs();

        var searchTextBox = GetTagsSearchTextBox(select);
        searchTextBox.Text = text;
        Dispatcher.UIThread.RunJobs();

        var candidateList = GetCandidateList(select);
        var dynamicOption = candidateList.Items
                                         .OfType<ISelectOption>()
                                         .Single(option => option.IsDynamicAdded);
        candidateList.CandidateSelectedItem = dynamicOption;
        candidateList.HandleKeyDown(new KeyEventArgs
        {
            RoutedEvent  = InputElement.KeyDownEvent,
            Source       = candidateList,
            Key          = Key.Enter,
            PhysicalKey  = PhysicalKey.Enter,
            KeyModifiers = KeyModifiers.None
        });
        Dispatcher.UIThread.RunJobs();

        return dynamicOption;
    }

    private static void AssertEmbeddedSelectFilterTextBox(SelectFilterTextBox searchTextBox)
    {
        searchTextBox.ShouldBeAssignableTo<EmbeddedTextBox>();
        searchTextBox.SizeType.ShouldBe(CustomizableSizeType.Custom);
        searchTextBox.Padding.ShouldBe(new Thickness(0));
        searchTextBox.BorderThickness.ShouldBe(new Thickness(0));
        searchTextBox.IsCustomFontSize.ShouldBeTrue();
    }

    private static SelectFilterTextBox GetTagsSearchTextBox(Desktop.Controls.Select select)
    {
        var textBox = select.GetVisualDescendants()
                            .OfType<SelectFilterTextBox>()
                            .SingleOrDefault(item => item.Name != "PART_SingleFilterInput");
        textBox.ShouldNotBeNull();
        return textBox;
    }

    private static void RaiseSearchInputKeyDown(SelectFilterTextBox searchTextBox, Key key)
    {
        searchTextBox.RaiseEvent(new KeyEventArgs
        {
            RoutedEvent  = InputElement.KeyDownEvent,
            Source       = searchTextBox,
            Key          = key,
            PhysicalKey  = key switch
            {
                Key.Enter  => PhysicalKey.Enter,
                Key.Escape => PhysicalKey.Escape,
                Key.Up     => PhysicalKey.ArrowUp,
                Key.Down   => PhysicalKey.ArrowDown,
                _          => PhysicalKey.None
            },
            KeyModifiers = KeyModifiers.None
        });
    }

    private static void InvokeHandleOpenDropRequest(Desktop.Controls.Select select)
    {
        var method = typeof(Desktop.Controls.Select).GetMethod(
            "HandleOpenDropRequest",
            BindingFlags.Instance | BindingFlags.NonPublic);

        method.ShouldNotBeNull();
        method.Invoke(select, null);
    }

    private static void EnablePopupOverlayLayer(VisualLayerManager visualLayerManager)
    {
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }

    private sealed class PendingSelectOptionsLoader : ISelectOptionsAsyncLoader
    {
        private readonly TaskCompletionSource<SelectOptionsLoadResult> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ManualResetEventSlim Started { get; } = new();
        public ManualResetEventSlim Cancelled { get; } = new();

        public Task<SelectOptionsLoadResult> LoadAsync(object? context, CancellationToken token)
        {
            Started.Set();
            token.Register(static state => ((ManualResetEventSlim)state!).Set(), Cancelled);
            return _completion.Task;
        }

        public void Complete()
        {
            _completion.TrySetResult(new SelectOptionsLoadResult
            {
                Data = []
            });
        }
    }

    private sealed class SelectBindingViewModel : INotifyPropertyChanged
    {
        private ISelectOption? _selectedOption;
        private IList<ISelectOption>? _selectedOptions;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ISelectOption? SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (ReferenceEquals(_selectedOption, value))
                {
                    return;
                }
                _selectedOption = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedOption)));
            }
        }

        public IList<ISelectOption>? SelectedOptions
        {
            get => _selectedOptions;
            set
            {
                if (ReferenceEquals(_selectedOptions, value))
                {
                    return;
                }
                _selectedOptions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedOptions)));
            }
        }
    }
}
