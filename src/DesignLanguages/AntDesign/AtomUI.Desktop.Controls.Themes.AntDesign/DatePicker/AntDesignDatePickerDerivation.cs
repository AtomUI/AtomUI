using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="DatePickerToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignDatePickerDerivation : IControlDerivation<AntDesignToken, DatePickerToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<DatePickerToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        AntDesignAbstractDatePickerDerivation.DeriveShared(in context, global, token);
        token.HeaderBorderThickness = new Thickness(0, 0, 0, global.LineWidth);
        token.ItemWidth = global.ControlHeightLG * 1.4;
        token.ItemHeight = global.ControlHeight - 4;
        token.PeriodHostWidth = token.ItemWidth;
        token.ItemPadding = AntDesignAbstractTimeViewDerivation.CalculateTimeItemPadding(token.ItemWidth, token.ItemHeight);
    }
}
