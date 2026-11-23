using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="DatePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnDatePickerDerivation : IControlDerivation<ShadcnToken, DatePickerToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<DatePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnInputFamily.DeriveGlobal(in context, global);
        ShadcnAbstractDatePickerDerivation.DeriveShared(in context, global, token);
        token.HeaderBorderThickness = new Thickness(0, 0, 0, global.LineWidth);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = ShadcnAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
    }
}
