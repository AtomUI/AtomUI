using AtomUI.Controls;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>Arrange table of Select: the state in which each of its Tokens shows.</summary>
public partial class SelectFamilyEffectTests
{
    private static EffectSetup SelectSetup(string token)
    {
        var b    = new SetupBuilder();
        var size = SizeOf(token);
        if (size != CustomizableSizeType.Middle)
        {
            b.Init.Add(c => ((Select)c).SizeType = size);
        }

        void Mode(SelectMode mode) => b.Init.Add(c => ((Select)c).Mode = mode);
        void Tags()
        {
            Mode(SelectMode.Multiple);
            b.Init.Add(static c => ((Select)c).SelectedOptions = [s_selectOptions[0], s_selectOptions[1]]);
        }
        void EmptyMultiple()
        {
            Mode(SelectMode.Multiple);
            b.Init.Add(static c => ((Select)c).PlaceholderText = "Placeholder");
        }
        void Selected() => b.Init.Add(static c => ((Select)c).SelectedOption = s_selectOptions[1]);
        void Status(InputControlStatus status) => b.Init.Add(c => ((Select)c).Status = status);
        void Variant(InputControlStyleVariant variant) => b.Init.Add(c => ((Select)c).StyleVariant = variant);
        void Open() => b.Popup = true;
        void ActiveCandidate() => b.Arrange.Add(static c =>
            Popup<SelectCandidateListItem>(c).IsCandidateSelected = true);
        void Clear()
        {
            Selected();
            b.Init.Add(static c => ((Select)c).IsAllowClear = true);
            b.Arrange.Add(static c => FrameOf(c).IsInnerBoxHover = true);
        }
        void LeftAddOn() => b.Init.Add(static c => ((Select)c).LeftAddOn = "http://");

        if (FrameState(b, token, Status, Variant, LeftAddOn, Selected))
        {
            return Build();
        }

        switch (token)
        {
            // ---- multiple mode content ----
            case "MultiModePadding" or "MultiModePaddingLG" or "MultiModePaddingSM" or "MultipleItemHeight"
                or "MultipleItemHeightLG" or "MultipleItemHeightSM" or "MultipleItemBg" or "ColorFillSecondary"
                or "SpacingXXS" or "ControlHeightLG" or "ControlHeightSM" or "UniformlyPaddingXXS" or "FontHeightLG":
                Tags();
                break;
            case "MultiModePrefixIndent" or "MultiModePrefixIndentLG" or "MultiModePrefixIndentSM":
                Tags();
                b.Init.Add(static c => ((Select)c).ContentLeftAddOn = "$");
                break;
            case "MultipleItemColorDisabled" or "MultipleSelectorBgDisabled":
                Tags();
                b.Disabled();
                break;
            case "SingleModePadding" or "SingleModePaddingLG" or "SingleModePaddingSM" or "ControlHeight" or "LineWidth"
                or "UniformlyPaddingSM" or "ControlPaddingHorizontalSM":
                EmptyMultiple();
                break;
            case "ControlPaddingHorizontal":
                // The large multiple-mode padding is the control horizontal padding.
                b.Init.Add(static c => ((Select)c).SizeType = CustomizableSizeType.Large);
                EmptyMultiple();
                break;
            case "FontHeight":
                // The vertical content padding is (ControlHeight - FontHeight) / 2, clamped at zero: it shows when the
                // font height decreases.
                EmptyMultiple();
                b.Perturb = static value => (double)value / 2;
                break;
            case "ColorTextDescription" or "SpacingXS":
                Tags();
                b.Init.Add(static c =>
                {
                    ((Select)c).MaxCount                = 3;
                    ((Select)c).IsShowMaxCountIndicator = true;
                });
                break;

            // ---- drop-down ----
            case "OptionFontSize" or "OptionHeight" or "OptionPadding" or "PopupContentPadding" or "ColorBgElevated":
                Open();
                break;
            case "OptionSelectedBg" or "OptionSelectedColor" or "OptionSelectedFontWeight" or "ControlItemBgActive" or "ControlItemColorActive"
                or "FontWeightStrong":
                Selected();
                Open();
                break;
            case "OptionActiveBg" or "ControlItemBgHover":
                Open();
                ActiveCandidate();
                break;

            // ---- content and suffix ----
            case "ColorText" or "FontSize" or "FontSizeLG":
                Selected();
                break;
            case "InputContentMargin":
                b.Init.Add(static c => ((Select)c).SizeType = CustomizableSizeType.Small);
                Selected();
                break;
            case "SelectionBackground" or "SelectionForeground":
                b.Init.Add(static c => ((Select)c).IsFilterEnabled = true);
                // The filter input is read-only until the drop-down opens; compare the input, not the drop-down.
                b.Popup  = true;
                b.Region = static c => c;
                SelectFilterText(b);
                break;
            case "ColorTextPlaceholder":
                b.Init.Add(static c => ((Select)c).PlaceholderText = "Placeholder");
                break;
            case "ColorTextQuaternary" or "IconSize":
                // The drop-down arrow is a few pixels of a wide surface; compare the handle that draws it.
                b.Region = static c => c.GetVisualDescendants().OfType<SelectHandle>().First();
                break;
            case "ColorTextTertiary":
                Clear();
                b.OnPart<InputClearIconButton>("PART_ClearButton", ":pointerover");
                break;
        }

        return Build();

        EffectSetup Build() => b.Build(
            static () => new Select { Width = 240, OptionsSource = s_selectOptions },
            static root => root.GetSelfAndVisualDescendants().OfType<Select>().First());
    }
}
