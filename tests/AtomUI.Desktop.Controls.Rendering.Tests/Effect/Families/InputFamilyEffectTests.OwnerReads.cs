using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Global Tokens that the identity-less primitive themes of the input family read through <c>OwnerTokenResource</c>.
/// The consumption map attributes them to <c>shared-primitive</c> (or to the identity theme they are written in), so every
/// host lists the primitives it carries and runs their reads as rows of its own; the drift guard keeps the hand lists equal
/// to the theme AXAML.
/// </summary>
public partial class InputFamilyEffectTests
{
    private const string Primitives = "src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/Themes/";
    private const string Input = "src/AtomUI.Desktop.Controls/Input/Themes/";
    private const string Spinner = "src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/";
    private const string Numeric = "src/AtomUI.Desktop.Controls/NumericUpDown/Themes/";
    private const string Complete = "src/AtomUI.Desktop.Controls/AutoComplete/Themes/";

    /// <summary>
    /// One primitive theme and the global Tokens it reads from its owner, in the theme AXAML or, for theme variables the
    /// primitive computes, in its code.
    /// </summary>
    internal sealed record OwnerReadList(string Theme, string[] Tokens, params string[] Code);

    internal static readonly OwnerReadList s_inputControlFrame = new(Primitives + "InputControlFrameTheme.axaml",
    [
        "BorderRadius", "BorderRadiusLG", "BorderRadiusSM", "BorderThickness", "ColorBgContainer", "ColorBgContainerDisabled",
        "ColorBorder", "ColorError", "ColorErrorBg", "ColorErrorBgHover", "ColorErrorBorderHover", "ColorFillSecondary",
        "ColorFillTertiary", "ColorPrimary", "ColorPrimaryHover", "ColorText", "ColorTextDisabled", "ColorWarning",
        "ColorWarningBg", "ColorWarningBgHover", "ColorWarningBorderHover", "EnableMotion", "InputActiveShadow",
        "InputErrorActiveShadow", "InputWarningActiveShadow"
    ], "src/AtomUI.Desktop.Controls/Primitives/InputControlFrame.cs");

    internal static readonly OwnerReadList s_addOnDecoratedBox = new(Primitives + "AddOnDecoratedBoxTheme.axaml",
    [
        "ColorBorder", "ColorError", "ColorErrorText", "ColorFillAlter", "ColorText", "ColorTextDisabled",
        "ColorTextQuaternary", "ColorWarning", "ColorWarningText", "FontHeight", "FontHeightLG", "FontHeightSM",
        "InputContentMargin", "InputPadding", "InputPaddingLG", "InputPaddingSM", "SpacingXXS"
    ], "src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/AddOnDecoratedBox.cs");

    internal static readonly OwnerReadList s_clearButton = new(Input + "InputClearIconButtonTheme.axaml",
        ["ColorText", "ColorTextQuaternary", "ColorTextTertiary", "IconSize"]);

    internal static readonly OwnerReadList s_revealButton = new(Input + "RevealButtonTheme.axaml",
        ["ColorIcon", "ColorIconHover", "ColorText", "ColorTextDisabled", "IconSize"]);

    internal static readonly OwnerReadList s_textAreaDecoratedBox = new(Input + "TextAreaDecoratedBoxTheme.axaml",
        ["InputPadding", "InputPaddingLG", "InputPaddingSM"]);

    internal static readonly OwnerReadList s_resizeHandle = new(Input + "ResizeHandleTheme.axaml", ["LineWidth"],
        "src/AtomUI.Desktop.Controls/Input/ResizeHandle.cs");

    /// <summary>TextBoxTheme reads its globals from the owner: the theme is also the base of identity-less embedded editors.</summary>
    internal static readonly OwnerReadList s_textBox = new(Input + "TextBoxTheme.axaml",
    [
        "BorderRadius", "BorderRadiusLG", "BorderRadiusSM", "BorderThickness", "ColorText", "ColorTextPlaceholder",
        "EnableMotion", "FontHeight", "FontHeightLG", "FontHeightSM", "FontSize", "FontSizeLG", "FontSizeSM",
        "SelectionBackground", "SelectionForeground", "SpacingXXS", "UniformlyPaddingXXS"
    ]);

    internal static readonly OwnerReadList s_buttonSpinnerDecoratedBox = new(Spinner + "ButtonSpinnerDecoratedBoxTheme.axaml",
    [
        "ControlHeight", "ControlHeightLG", "ControlHeightSM", "InputPadding", "InputPaddingLG", "InputPaddingSM", "SpacingXXS"
    ]);

    internal static readonly OwnerReadList s_buttonSpinnerHandle = new(Spinner + "ButtonSpinnerHandleTheme.axaml",
        ["BorderThickness", "ColorBgContainerDisabled", "ColorBorder"]);

    /// <summary>ButtonSpinnerTheme reads its globals from the owner: it is also the base of NumericUpDown's spinner.</summary>
    internal static readonly OwnerReadList s_buttonSpinner = new(Spinner + "ButtonSpinnerTheme.axaml",
        ["BorderRadius", "BorderRadiusLG", "BorderRadiusSM", "EnableMotion"]);

    internal static readonly OwnerReadList s_sizeTypeAwareIcon = new(Spinner + "SizeTypeAwareIconPresenter.axaml",
        ["IconSize", "IconSizeLG", "IconSizeSM"]);

    /// <summary>NumericUpDownSpinnerTheme has no owner reads: its -/+ buttons are registered IconButtons.</summary>
    internal static readonly OwnerReadList s_numericUpDownSpinner = new(Numeric + "NumericUpDownSpinnerTheme.axaml", []);

    internal static readonly OwnerReadList s_abstractAutoComplete = new(Complete + "AbstractAutoCompleteTheme.axaml",
        ["ColorBgElevated", "ColorTextPlaceholder", "EnableMotion"]);

    private static readonly OwnerReadList[] s_allLists =
    [
        s_inputControlFrame, s_addOnDecoratedBox, s_clearButton, s_revealButton, s_textAreaDecoratedBox, s_resizeHandle, s_textBox,
        s_buttonSpinnerDecoratedBox, s_buttonSpinnerHandle, s_buttonSpinner, s_sizeTypeAwareIcon, s_numericUpDownSpinner,
        s_abstractAutoComplete
    ];

    /// <summary>The primitives each host carries, and the reads of them the host does not render (with the reason).</summary>
    internal static readonly Dictionary<string, (OwnerReadList[] Lists, Dictionary<string, string> NotRendered)> s_hosts =
        new(StringComparer.Ordinal)
        {
            ["TextBox"] = ([s_inputControlFrame, s_textBox, s_clearButton, s_revealButton], new(StringComparer.Ordinal)),
            ["LineEdit"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_clearButton, s_revealButton], new(StringComparer.Ordinal)),
            ["SearchEdit"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_clearButton, s_revealButton], new(StringComparer.Ordinal)
            {
                ["SpacingXXS"] = "SearchEdit's frame lays out prefix and input in a plain DockPanel (LineEdit renders it)"
            }),
            ["TextArea"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_textAreaDecoratedBox, s_resizeHandle, s_clearButton],
                new(StringComparer.Ordinal)
                {
                    ["ColorFillAlter"] = "TextArea has no outer add-ons (LineEdit renders the add-on background)",
                    ["SpacingXXS"]     = "TextArea's frame lays out prefix and input in a plain DockPanel (LineEdit renders it)"
                }),
            ["ButtonSpinner"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_buttonSpinnerDecoratedBox, s_buttonSpinnerHandle,
                s_sizeTypeAwareIcon], new(StringComparer.Ordinal)),
            ["NumericUpDown"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_buttonSpinnerDecoratedBox, s_buttonSpinnerHandle,
                s_buttonSpinner, s_numericUpDownSpinner, s_clearButton, s_textBox], new(StringComparer.Ordinal)
            {
                ["FontSizeLG"] = "the embedded editor is SizeType=Custom and keeps the middle preset (TextBox renders it)",
                ["FontSizeSM"] = "the embedded editor is SizeType=Custom and keeps the middle preset (TextBox renders it)"
            }),
            ["AutoComplete"] = ([s_abstractAutoComplete], new(StringComparer.Ordinal)),
            ["AutoCompleteSearchEdit"] = ([s_abstractAutoComplete], new(StringComparer.Ordinal)),
            ["AutoCompleteTextArea"] = ([s_abstractAutoComplete], new(StringComparer.Ordinal))
        };

    /// <summary>Consumption-map rows of the host plus the rendered owner reads of the primitives it carries.</summary>
    private static IEnumerable<object[]> RowsOf(string controlId)
    {
        var (lists, notRendered) = s_hosts[controlId];
        var rows = MapRows(controlId).ToList();
        var seen = new HashSet<string>(rows.Select(static row => (string)row[0]), StringComparer.Ordinal);
        foreach (var token in lists.SelectMany(static list => list.Tokens).Distinct(StringComparer.Ordinal)
                                   .Order(StringComparer.Ordinal))
        {
            if (!notRendered.ContainsKey(token) && seen.Add(token))
            {
                rows.Add([token, false]);
            }
        }

        return rows;
    }

    private static IEnumerable<object[]> MapRows(string controlId)
    {
        try
        {
            return EffectTestData.For("AtomUI", controlId).ToArray();
        }
        catch (ArgumentException)
        {
            // Controls whose theme reads only through primitives have no rows of their own in the map.
            return [];
        }
    }

    [Fact]
    public void Owner_Read_Lists_Match_The_Primitive_Themes()
    {
        // Drift guard: a new owner read in a primitive theme or theme-variable code must get an effect row through its hosts.
        foreach (var list in s_allLists)
        {
            ThemeOwnerReads.Read(list.Theme).Union(ThemeOwnerReads.ReadCode(list.Code)).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal), list.Theme);
        }
    }

    [Fact]
    public void Every_Owner_Read_Is_Rendered_By_At_Least_One_Host()
    {
        foreach (var list in s_allLists)
        {
            foreach (var token in list.Tokens)
            {
                s_hosts.Values.Any(host => host.Lists.Contains(list) && !host.NotRendered.ContainsKey(token))
                       .ShouldBeTrue($"{list.Theme} reads '{token}' but no host renders it.");
            }
        }
    }
}
