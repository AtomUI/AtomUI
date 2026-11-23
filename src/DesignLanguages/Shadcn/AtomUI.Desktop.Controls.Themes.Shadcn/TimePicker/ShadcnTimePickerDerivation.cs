using Avalonia;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="TimePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnTimePickerDerivation : IControlDerivation<ShadcnToken, TimePickerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<TimePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnInputFamily.DeriveGlobal(in context, global);
        ShadcnAbstractTimePickerDerivation.DeriveShared(in context, global, token);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        // shadcn centres the items and insets the item background by a line.
        token.ItemPadding = default;
        token.CenterItemContent   = true;
        token.ItemBackgroundInset = new Thickness(0, global.LineWidth);
    }
}
