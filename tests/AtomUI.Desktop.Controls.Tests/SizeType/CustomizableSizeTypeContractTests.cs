using System.Reflection;
using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Shouldly;
using Xunit;
using AtomUIAutoComplete = AtomUI.Desktop.Controls.AutoComplete;
using AtomUICascader = AtomUI.Desktop.Controls.Cascader;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AtomUIContextMenu = AtomUI.Desktop.Controls.ContextMenu;
using AtomUIForm = AtomUI.Desktop.Controls.Form;
using AtomUIListBox = AtomUI.Desktop.Controls.ListBox;
using AtomUIListBoxItem = AtomUI.Desktop.Controls.ListBoxItem;
using AtomUIListView = AtomUI.Desktop.Controls.ListView;
using AtomUIListViewItem = AtomUI.Desktop.Controls.ListViewItem;
using AtomUIMenu = AtomUI.Desktop.Controls.Menu;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using AtomUIPagination = AtomUI.Desktop.Controls.Pagination;
using AtomUISegmented = AtomUI.Desktop.Controls.Segmented;
using AtomUISpin = AtomUI.Desktop.Controls.Spin;
using AtomUISpinIndicator = AtomUI.Desktop.Controls.SpinIndicator;
using AtomUITextBox = AtomUI.Desktop.Controls.TextBox;

namespace AtomUI.Desktop.Controls.Tests.SizeType;

public class CustomizableSizeTypeContractTests
{
    private static IEnumerable<Type> TargetControlTypes()
    {
        yield return typeof(AbstractToggleSwitch);
        yield return typeof(ToggleSwitch);

        yield return typeof(AbstractOptionButtonGroup);
        yield return typeof(OptionButtonGroup);
        yield return typeof(AbstractOptionButton);
        yield return typeof(OptionButton);

        yield return typeof(AbstractSegmented);
        yield return typeof(AtomUISegmented);
        yield return typeof(AbstractSegmentedItem);
        yield return typeof(SegmentedItem);

        yield return typeof(AtomUITextBox);
        yield return typeof(LineEdit);
        yield return typeof(SearchEdit);
        yield return typeof(TextArea);
        yield return typeof(AtomUINumericUpDown);
        yield return typeof(AtomUIComboBox);
        yield return typeof(ComboBoxItem);
        yield return typeof(Mentions);

        yield return typeof(AddOnDecoratedBox);
        yield return typeof(CompactSpace);
        yield return typeof(CompactSpaceAddOn);
        yield return typeof(SizeTypeAwareIconPresenter);

        yield return typeof(AbstractAutoComplete);
        yield return typeof(CompactSpaceAwareAutoComplete);
        yield return typeof(AtomUIAutoComplete);
        yield return typeof(AutoCompleteSearchEdit);
        yield return typeof(AutoCompleteTextArea);

        yield return typeof(AbstractSelect);
        yield return typeof(Select);
        yield return typeof(AtomUICascader);
        yield return typeof(TreeSelect);
        yield return typeof(SelectResultOptionsBox);
        yield return typeof(SelectTag);
        yield return typeof(SelectTagAwareTextBox);

        yield return typeof(InfoPickerInput);
        yield return typeof(RangeInfoPickerInput);
        yield return typeof(DatePicker);
        yield return typeof(RangeDatePicker);
        yield return typeof(TimePicker);
        yield return typeof(RangeTimePicker);

        yield return typeof(AtomUIForm);
        yield return typeof(FormItem);
        yield return typeof(FormItemDecorator);

        yield return typeof(AbstractPagination);
        yield return typeof(AtomUIPagination);
        yield return typeof(SimplePagination);
        yield return typeof(PaginationNav);
        yield return typeof(PaginationNavItem);
        yield return typeof(QuickJumperBar);

        yield return typeof(ButtonSpinner);
        yield return typeof(Expander);
        yield return typeof(Collapse);
        yield return typeof(CollapseItem);

        yield return typeof(AtomUIMenu);
        yield return typeof(AtomUIContextMenu);
        yield return typeof(MenuFlyoutPresenter);
        yield return typeof(MenuItem);

        yield return typeof(AtomUIListView);
        yield return typeof(AtomUIListViewItem);
        yield return typeof(AtomUIListBox);
        yield return typeof(AtomUIListBoxItem);

        yield return typeof(AbstractTransfer);
        yield return typeof(ListTransfer);
        yield return typeof(TreeTransfer);
        yield return typeof(TransferItemDecorator);

        yield return typeof(AbstractSpin);
        yield return typeof(AtomUISpin);
        yield return typeof(AbstractSpinIndicator);
        yield return typeof(AtomUISpinIndicator);
    }

    [Fact]
    public void Target_Controls_Use_Customizable_SizeType_Contract()
    {
        foreach (var controlType in TargetControlTypes())
        {
            var property = controlType.GetProperty(
                "SizeType",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            property.ShouldNotBeNull($"{controlType.FullName} must expose SizeType.");
            property!.PropertyType.ShouldBe(typeof(CustomizableSizeType), $"{controlType.FullName}.SizeType should support Custom.");

            var field = controlType.GetField(
                "SizeTypeProperty",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            field.ShouldNotBeNull($"{controlType.FullName} must expose SizeTypeProperty.");
            field!.FieldType.ShouldBe(
                typeof(StyledProperty<CustomizableSizeType>),
                $"{controlType.FullName}.SizeTypeProperty should use CustomizableSizeType.");
        }
    }

    [Fact]
    public void ButtonSpinner_Implements_Customizable_SizeType_Aware()
    {
        typeof(ICustomizableSizeTypeAware)
            .IsAssignableFrom(typeof(ButtonSpinner))
            .ShouldBeTrue();
    }
}
