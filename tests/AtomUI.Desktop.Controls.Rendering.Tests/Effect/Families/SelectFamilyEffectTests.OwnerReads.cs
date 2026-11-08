using Shouldly;
using Xunit;
using OwnerReadList = AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families.InputFamilyEffectTests.OwnerReadList;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Global Tokens that identity-less primitives of the select family read through <c>OwnerTokenResource</c>. The consumption
/// map attributes them to <c>shared-primitive</c>, so every member lists the primitives it carries (its own shared parts and
/// the input-family surface) and runs their reads as rows of its own; the drift guard keeps the select-family lists equal to
/// the theme AXAML (the input-family lists are guarded by <see cref="InputFamilyEffectTests"/>). Parts that only one member
/// uses read that member's TokenResource explicitly and appear in the map directly.
/// </summary>
public partial class SelectFamilyEffectTests
{
    private const string SelectThemes = "src/AtomUI.Desktop.Controls/Select/Themes/";

    /// <summary>The drop-down indicator / clear button cell shared by Select, Cascader and TreeSelect.</summary>
    internal static readonly OwnerReadList s_selectHandle = new(SelectThemes + "SelectHandleTheme.axaml",
        ["IconSize", "SpacingXS"]);

    internal static readonly OwnerReadList s_selectMaxCountIndicator =
        new(SelectThemes + "SelectMaxCountIndicatorTheme.axaml", ["ColorTextDescription"]);

    /// <summary>The multiple-mode tag box of Cascader and TreeSelect.</summary>
    internal static readonly OwnerReadList s_selectTagAwareTextBox =
        new(SelectThemes + "SelectTagAwareTextBoxTheme.axaml", ["ColorTextDisabled", "SpacingXXS"]);

    /// <summary>The single-mode filter input (no owner reads of its own; its TextBox base theme reads the owner).</summary>
    internal static readonly OwnerReadList s_selectFilterTextBox = new(SelectThemes + "SelectFilterTextBoxTheme.axaml", []);

    private static readonly OwnerReadList[] s_familyLists =
        [s_selectHandle, s_selectMaxCountIndicator, s_selectTagAwareTextBox, s_selectFilterTextBox];

    private static readonly OwnerReadList s_inputControlFrame = InputFamilyEffectTests.s_inputControlFrame;
    private static readonly OwnerReadList s_addOnDecoratedBox = InputFamilyEffectTests.s_addOnDecoratedBox;
    private static readonly OwnerReadList s_clearButton = InputFamilyEffectTests.s_clearButton;

    private const string SurfaceOnly = "drawn by the shared input surface; the input family renders it";

    /// <summary>The primitives each member carries, and the reads of them the member does not render (with the reason).</summary>
    private static readonly Dictionary<string, (OwnerReadList[] Lists, Dictionary<string, string> NotRendered)> s_hosts =
        new(StringComparer.Ordinal)
        {
            ["Select"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_clearButton, s_selectHandle, s_selectMaxCountIndicator,
                s_selectFilterTextBox], new(StringComparer.Ordinal)),
            ["ComboBox"] = ([s_inputControlFrame, s_addOnDecoratedBox], new(StringComparer.Ordinal)),
            ["ComboBoxItem"] = ([], new(StringComparer.Ordinal)),
            ["Cascader"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_clearButton, s_selectHandle, s_selectMaxCountIndicator,
                s_selectTagAwareTextBox, s_selectFilterTextBox], new(StringComparer.Ordinal)),
            ["CascaderView"] = ([], new(StringComparer.Ordinal)),
            ["CascaderViewItem"] = ([], new(StringComparer.Ordinal)),
            ["TreeSelect"] = ([s_inputControlFrame, s_addOnDecoratedBox, s_clearButton, s_selectHandle, s_selectMaxCountIndicator,
                s_selectTagAwareTextBox, s_selectFilterTextBox], new(StringComparer.Ordinal)),
        };

    /// <summary>Consumption-map rows of the member plus the rendered owner reads of the primitives it carries.</summary>
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
            // Members whose theme reads only through primitives have no rows of their own in the map.
            return [];
        }
    }

    [Fact]
    public void Owner_Read_Lists_Match_The_Primitive_Themes()
    {
        foreach (var list in s_familyLists)
        {
            ThemeOwnerReads.Read(list.Theme).Order(StringComparer.Ordinal)
                           .ShouldBe(list.Tokens.Order(StringComparer.Ordinal), list.Theme);
        }
    }

    [Fact]
    public void Every_Owner_Read_Is_Rendered_By_At_Least_One_Host()
    {
        foreach (var list in s_familyLists)
        {
            foreach (var token in list.Tokens)
            {
                s_hosts.Values.Any(host => host.Lists.Contains(list) && !host.NotRendered.ContainsKey(token))
                       .ShouldBeTrue($"{list.Theme} reads '{token}' but no member renders it.");
            }
        }
    }
}
