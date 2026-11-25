using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.Resources;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public partial class Separator : AbstractSeparator
{
    private static readonly object[] s_layoutKeys =
    [
        SeparatorTokenKind.TextPaddingInline,
        SeparatorTokenKind.OrientationMarginPercent,
        SeparatorTokenKind.VerticalMarginInline
    ];

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

public class VerticalSeparator : Separator
{
    static VerticalSeparator()
    {
        OrientationProperty.OverrideDefaultValue<VerticalSeparator>(Orientation.Vertical);
    }

    protected override Type StyleKeyOverride => typeof(Separator);
}
