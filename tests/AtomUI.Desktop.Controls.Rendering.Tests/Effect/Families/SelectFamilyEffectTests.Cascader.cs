using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Cascader and its registered drop-down view (CascaderView, CascaderViewItem). The view and its items keep their own
/// identities for globals; the Cascader Own Tokens they read (option colours and metrics, menu sizes) are the Cascader's
/// design values and are covered by the Cascader rows with the drop-down open. CascaderView is a public control, so its
/// rows and the item rows run on a standalone view.
/// </summary>
public partial class SelectFamilyEffectTests
{
    public static IEnumerable<object[]> CascaderRows() => RowsOf("Cascader");
    public static IEnumerable<object[]> CascaderViewRows() => RowsOf("CascaderView");
    public static IEnumerable<object[]> CascaderViewItemRows() => RowsOf("CascaderViewItem");

    [Theory]
    [MemberData(nameof(CascaderRows))]
    public void Cascader_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Run(CascaderSetup(token), CascaderTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(CascaderViewRows))]
    public void CascaderView_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        var empty = token == "Padding";
        TokenEffectHarness.AssertHasEffect(
            () => new CascaderView { OptionsSource = empty ? [] : CascaderOptions(3) },
            CascaderViewTokens.Identity, token);
    }

    [Theory]
    [MemberData(nameof(CascaderViewItemRows))]
    public void CascaderViewItem_Token_Has_Effect(string token, bool isOwn)
    {
        _ = isOwn;
        Func<CascaderViewItem, bool> pick = token switch
        {
            "ColorTextDisabled" => static item => !item.IsEnabled,
            "IconSize"          => static item => item.Icon is not null,
            _                   => static item => IsZhejiang(item)
        };
        Action<Control>? arrange = token is "BorderRadius"
            ? static item => ((CascaderViewItem)item).IsExpanded = true
            : null;
        TokenEffectHarness.AssertHasEffect(
            static () => new CascaderView { OptionsSource = CascaderOptions(3) },
            CascaderViewItemTokens.Identity, token, arrange,
            options: new EffectOptions
            {
                Target = root => root.GetVisualDescendants().OfType<CascaderViewItem>().First(pick)
            });
    }

    /// <summary>The item of the first root option: it has children, so it shows the expand icon.</summary>
    private static bool IsZhejiang(CascaderViewItem item) => item.Header is CascaderOption { Header: "Zhejiang" };

    private static CascaderOption[] CascaderOptions(int roots)
    {
        var options = new List<CascaderOption>
        {
            new() { Header = "Zhejiang", Children = { new CascaderOption { Header = "Hangzhou" } } },
            new() { Header = "Jiangsu", Children = { new CascaderOption { Header = "Nanjing" } } },
            new() { Header = "Beijing", Icon = new EnvironmentOutlined() },
            new() { Header = "Disabled", IsEnabled = false }
        };
        for (var index = options.Count; index < roots; index++)
        {
            options.Add(new CascaderOption { Header = $"City {index}" });
        }

        return options.ToArray();
    }

    private static EffectSetup CascaderSetup(string token)
    {
        var b     = new SetupBuilder();
        var roots = 4;
        var width = 240d;
        var size  = SizeOf(token);
        if (size != CustomizableSizeType.Middle)
        {
            b.Init.Add(c => ((Cascader)c).SizeType = size);
        }

        void Selected() => b.Init.Add(static c =>
        {
            var cascader = (Cascader)c;
            cascader.SelectedOption = ((CascaderOption[])cascader.OptionsSource!)[2];
        });
        void Status(InputControlStatus status) => b.Init.Add(c => ((Cascader)c).Status = status);
        void Variant(InputControlStyleVariant variant) => b.Init.Add(c => ((Cascader)c).StyleVariant = variant);
        void LeftAddOn() => b.Init.Add(static c => ((Cascader)c).LeftAddOn = "http://");
        void Open() => b.Popup = true;
        void Tags() => b.Init.Add(static c =>
        {
            var cascader = (Cascader)c;
            var options  = (CascaderOption[])cascader.OptionsSource!;
            cascader.IsMultiple      = true;
            cascader.SelectedOptions = [options[2], options[0].Children[0]];
        });
        void Item(Action<CascaderViewItem> step) => b.Arrange.Add(c =>
            step(TopLevel.GetTopLevel(c)!.GetVisualDescendants().OfType<CascaderViewItem>()
                         .First(IsZhejiang)));

        void EmptyMultiple() => b.Init.Add(static c =>
        {
            ((Cascader)c).IsMultiple = true;
            ((Cascader)c).PlaceholderText = "Placeholder";
        });
        var inputOwn = true;
        switch (token)
        {
            case "MultiModePadding" or "MultiModePaddingLG" or "MultiModePaddingSM" or "MultipleItemHeight"
                or "MultipleItemHeightLG" or "MultipleItemHeightSM" or "MultipleItemBg" or "ColorFillSecondary"
                or "UniformlyPaddingXXS" or "FontHeightLG" or "ControlHeightLG":
                Tags();
                break;
            case "MultiModePrefixIndent" or "MultiModePrefixIndentLG" or "MultiModePrefixIndentSM":
                Tags();
                b.Init.Add(static c => ((Cascader)c).ContentLeftAddOn = "$");
                break;
            case "MultipleItemColorDisabled" or "MultipleSelectorBgDisabled":
                Tags();
                b.Disabled();
                break;
            case "SingleModePadding" or "SingleModePaddingLG" or "SingleModePaddingSM" or "LineWidth"
                or "ControlPaddingHorizontalSM":
                EmptyMultiple();
                break;
            case "ControlPaddingHorizontal":
                b.Init.Add(static c => ((Cascader)c).SizeType = CustomizableSizeType.Large);
                EmptyMultiple();
                break;
            case "OptionFontSize":
                Tags();
                break;
            default:
                inputOwn = false;
                break;
        }

        if (!inputOwn && !FrameState(b, token, Status, Variant, LeftAddOn, Selected))
        {
            switch (token)
            {
                // ---- drop-down ----
                case "ControlItemWidth" or "HeaderHeight" or "ItemHeaderSpacing" or "MenuPadding" or "OptionPadding"
                    or "ColorBgElevated" or "ControlHeight" or "ControlHeightSM" or "PaddingXXS" or "UniformlyPaddingSM":
                    Open();
                    break;
                case "FontHeight":
                    // The vertical option padding is (ControlHeight - FontHeight) / 2: it shows when the font height
                    // decreases.
                    Open();
                    b.Perturb = static value => (double)value / 2;
                    break;
                case "DropdownHeight":
                    roots = 24;
                    Open();
                    break;
                case "OptionHoverBg" or "ControlItemBgHover":
                    Open();
                    Item(static item => item.IsCandidateSelected = true);
                    break;
                case "OptionSelectedBg" or "OptionSelectedColor" or "OptionSelectedFontWeight" or "ControlItemBgActive" or "ControlItemColorActive"
                    or "FontWeightStrong":
                    Open();
                    Item(static item => item.IsExpanded = true);
                    break;
                case "FilterHighlightColor":
                    Open();
                    // The view filters once it is loaded: the filter value is set with the drop-down open.
                    b.Init.Add(static c => ((Cascader)c).IsFilterEnabled = true);
                    b.Arrange.Add(static c => ((Cascader)c).FilterValue = "Hang");
                    break;
                case "ControlWidth":
                    // The default width applies to a Cascader without an explicit width.
                    width = double.NaN;
                    break;

                // ---- content ----
                case "SpacingXXS":
                    Tags();
                    break;
                case "ColorText" or "FontSize" or "FontSizeLG":
                    Selected();
                    break;
                case "InputContentMargin":
                    b.Init.Add(static c => ((Cascader)c).SizeType = CustomizableSizeType.Small);
                    Selected();
                    break;
                case "ColorFillSecondary":
                    Variant(InputControlStyleVariant.Filled);
                    b.Hover();
                    break;
                case "SelectionBackground" or "SelectionForeground":
                    b.Init.Add(static c => ((Cascader)c).IsFilterEnabled = true);
                    SelectFilterText(b);
                    break;
                case "ColorTextPlaceholder":
                    b.Init.Add(static c => ((Cascader)c).PlaceholderText = "Placeholder");
                    break;
                case "ColorTextDescription" or "SpacingXS":
                    Tags();
                    b.Init.Add(static c =>
                    {
                        ((Cascader)c).MaxCount                = 3;
                        ((Cascader)c).IsShowMaxCountIndicator = true;
                    });
                    break;
                case "ColorTextQuaternary" or "IconSize":
                    b.Region = static c => c.GetVisualDescendants().OfType<SelectHandle>().First();
                    break;
                case "ColorTextTertiary":
                    Selected();
                    b.Init.Add(static c => ((Cascader)c).IsAllowClear = true);
                    b.Hover();
                    b.OnPart<InputClearIconButton>("PART_ClearButton", ":pointerover");
                    break;
            }
        }

        var rootCount = roots;
        var cascaderWidth = width;
        return b.Build(
            () =>
            {
                var cascader = new Cascader { OptionsSource = CascaderOptions(rootCount) };
                if (!double.IsNaN(cascaderWidth))
                {
                    cascader.Width = cascaderWidth;
                }

                return cascader;
            },
            static root => root.GetSelfAndVisualDescendants().OfType<Cascader>().First());
    }
}
