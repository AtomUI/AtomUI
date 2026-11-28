using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A separator that supplies the layout with four separator design tokens when the theme asks for them: the
/// padding around the title, the default position of the title, the horizontal margin of a vertical line and the
/// vertical block inset of a vertical line. Values set on the control itself take precedence over the tokens.
/// </summary>
public partial class Separator : AbstractSeparator
{
    private static readonly object[] s_layoutKeys =
    [
        SeparatorTokenKind.TextPaddingInline,
        SeparatorTokenKind.OrientationMarginPercent,
        SeparatorTokenKind.VerticalMarginInline,
        SeparatorTokenKind.VerticalMarginBlock
    ];

    /// <summary>
    /// Returns a token set on this control with one slot for each layout token of the theme: the padding around the
    /// title (<c>SeparatorTokenKind.TextPaddingInline</c>), the ratio that places the title
    /// (<c>SeparatorTokenKind.OrientationMarginPercent</c>), the margin that widens a vertical separator
    /// (<c>SeparatorTokenKind.VerticalMarginInline</c>) and the vertical block inset of a vertical separator
    /// (<c>SeparatorTokenKind.VerticalMarginBlock</c>). The set raises <paramref name="changed" /> when a token
    /// value changes.
    /// </summary>
    /// <param name="changed">
    /// The callback the set raises when a token value changes.
    /// </param>
    /// <returns>
    /// A token set with four slots, in the order of the tokens above.
    /// </returns>
    /// <remarks>
    /// The base class calls it when the control joins a visual tree and when the theme switches the tokens on or
    /// off while the control is attached. It owns the returned set and disposes it when it is replaced and when
    /// the control is detached.
    /// </remarks>
    private protected override OwnerTokenSet CreateSeparatorLayoutTokenSet(Action changed)
    {
        var tokens = new OwnerTokenSet(this, s_layoutKeys.Length, changed);
        for (var index = 0; index < s_layoutKeys.Length; ++index)
        {
            tokens.SetResourceKey(index, s_layoutKeys[index]);
        }
        return tokens;
    }
}

/// <summary>
/// A <see cref="Separator" /> whose line is vertical by default.
/// </summary>
public class VerticalSeparator : Separator
{
    static VerticalSeparator()
    {
        OrientationProperty.OverrideDefaultValue<VerticalSeparator>(Orientation.Vertical);
    }

    /// <summary>
    /// Returns <see cref="Separator" /> so that the vertical separator is styled by the separator theme.
    /// </summary>
    protected override Type StyleKeyOverride => typeof(Separator);
}
