using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// ComboBox and its registered ComboBoxItem. The item reads its own identity for globals and the ComboBox Own Tokens for
/// its colours and metrics (they are the ComboBox's design values), so the ComboBox rows open the drop-down and the
/// ComboBoxItem rows target an item inside it.
/// </summary>
public partial class SelectFamilyEffectTests
{
    public static IEnumerable<object[]> ComboBoxRows() => RowsOf("ComboBox");
    public static IEnumerable<object[]> ComboBoxItemRows() => RowsOf("ComboBoxItem");

    [Theory]
    [MemberData(nameof(ComboBoxRows))]
    public void ComboBox_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(ComboBoxSetup(token), ComboBoxTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(ComboBoxItemRows))]
    public void ComboBoxItem_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var b = new SetupBuilder { Popup = true };
        b.Init.Add(static c => ((ComboBox)c).SelectedIndex = 1);
        var setup = b.Build(static () => CreateComboBox(4), static root => root);
        Run(setup with
            {
                Options = setup.Options with
                {
                    // The item lives in the drop-down; compare the selected item, whose background shows the radius.
                    Target = static root => TopLevel.GetTopLevel(root)!.GetVisualDescendants().OfType<ComboBoxItem>()
                                                    .First(static item => item.IsSelected),
                    Region = null
                }
            },
            ComboBoxItemTokens.Identity, token);
    }

    private static ComboBox CreateComboBox(int count)
    {
        var comboBox = new ComboBox { Width = 240 };
        for (var index = 0; index < count; index++)
        {
            comboBox.Items.Add(new ComboBoxItem { Content = $"Item {index}", IsEnabled = index != count - 1 });
        }

        return comboBox;
    }

    private static EffectSetup ComboBoxSetup(string token)
    {
        var b     = new SetupBuilder();
        var count = 4;
        var size  = SizeOf(token);
        if (size != CustomizableSizeType.Middle)
        {
            b.Init.Add(c => ((ComboBox)c).SizeType = size);
        }

        void Selected() => b.Init.Add(static c => ((ComboBox)c).SelectedIndex = 1);
        void Status(InputControlStatus status) => b.Init.Add(c => ((ComboBox)c).Status = status);
        void Variant(InputControlStyleVariant variant) => b.Init.Add(c => ((ComboBox)c).StyleVariant = variant);
        void LeftAddOn() => b.Init.Add(static c => ((ComboBox)c).LeftAddOn = "http://");
        void Open() => b.Popup = true;
        void CandidateItem() => b.Arrange.Add(static c =>
            TopLevel.GetTopLevel(c)!.GetVisualDescendants().OfType<ComboBoxItem>()
                    .First(static item => !item.IsSelected && item.IsEnabled).IsCandidateSelected = true);
        void Handle(string pseudoClass)
        {
            if (pseudoClass.Length > 0)
            {
                b.OnPart<IconButton>("PART_OpenIndicatorButton", pseudoClass);
            }
            // The indicator is a few pixels of a wide surface; compare the handle that draws it.
            b.Region = static c => c.GetVisualDescendants().OfType<ComboBoxHandle>().First();
        }

        if (!FrameState(b, token, Status, Variant, LeftAddOn, Selected))
        {
            switch (token)
            {
                // ---- drop-down ----
                case "ItemBgColor" or "ItemColor" or "ItemMargin" or "ItemPadding" or "PopupContentPadding"
                    or "ColorBgElevated" or "ColorTextSecondary" or "UniformlyPaddingSM" or "UniformlyPaddingXXS"
                    or "BorderRadiusLG" or "ItemDisabledColor":
                    Open();
                    break;
                case "ItemSelectedBgColor" or "ItemSelectedColor" or "ControlItemBgActive" or "ControlItemColorActive":
                    Selected();
                    Open();
                    break;
                case "ItemHoverBgColor" or "ItemHoverColor" or "ColorBgTextHover":
                    Open();
                    CandidateItem();
                    break;
                case "ControlHeight":
                    // The item height only bounds the drop-down height (item height x page size).
                    count = 24;
                    Open();
                    break;
                case "Padding":
                    // The empty indicator padding shows in a drop-down without items.
                    count = 0;
                    Open();
                    break;

                // ---- handle ----
                case "HandleHoverColor":
                    Handle(":pointerover");
                    break;
                case "ColorPrimaryActive":
                    Handle(":pressed");
                    break;
                case "IconSizeSM":
                    Handle(string.Empty);
                    break;

                // ---- content ----
                case "ColorText" or "FontSize" or "FontSizeLG" or "FontSizeSM":
                    Selected();
                    break;
                case "ColorErrorText":
                    Variant(InputControlStyleVariant.Borderless);
                    Status(InputControlStatus.Error);
                    Selected();
                    break;
                case "ColorWarningText":
                    Variant(InputControlStyleVariant.Filled);
                    Status(InputControlStatus.Warning);
                    Selected();
                    break;
                case "ColorTextPlaceholder":
                    b.Init.Add(static c => ((ComboBox)c).PlaceholderText = "Placeholder");
                    break;
                case "SelectionBackground" or "SelectionForeground":
                    b.Init.Add(static c =>
                    {
                        ((ComboBox)c).IsEditable = true;
                        ((ComboBox)c).Text       = "Editable text";
                    });
                    b.Arrange.Add(static c => c.GetVisualDescendants().OfType<AvaloniaTextBox>().First().SelectAll());
                    break;
                case "SpacingXXS":
                    // The surface spaces the prefix from the content.
                    b.Init.Add(static c => ((ComboBox)c).ContentLeftAddOn = "$");
                    Selected();
                    break;
                case "IconSize":
                    // The clear entry reserves an IconSize slot beside the arrow while clearing is allowed; an auto-width
                    // ComboBox grows with the slot.
                    b.Init.Add(static c =>
                    {
                        var comboBox = (ComboBox)c;
                        comboBox.IsAllowClear        = true;
                        comboBox.Width               = double.NaN;
                        comboBox.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                    });
                    Selected();
                    break;
                case "ColorFillSecondary":
                    Variant(InputControlStyleVariant.Filled);
                    b.Hover();
                    break;
                case "InputContentMargin":
                    b.Init.Add(static c => ((ComboBox)c).SizeType = CustomizableSizeType.Small);
                    Selected();
                    break;
                case "SpacingXS":
                    b.Init.Add(static c => ((ComboBox)c).ContentRightAddOn = "kg");
                    break;
                case "ColorTextQuaternary":
                    b.Init.Add(static c => ((ComboBox)c).ContentRightAddOn = "kg");
                    break;
            }
        }

        var itemCount = count;
        return b.Build(() => CreateComboBox(itemCount), static root => root.GetSelfAndVisualDescendants().OfType<ComboBox>().First());
    }
}
