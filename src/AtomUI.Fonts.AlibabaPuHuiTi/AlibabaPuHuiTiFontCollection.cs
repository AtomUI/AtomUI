using Avalonia.Media.Fonts;

namespace AtomUI.Fonts.AlibabaPuHuiTi;

/// <summary>
/// The Alibaba PuHuiTi fonts that are embedded in this assembly, served under the font key
/// <c>fonts:AlibabaPuHuiTi</c> from the assembly's <c>Assets</c> location.
/// </summary>
public class AlibabaPuHuiTiFontCollection : EmbeddedFontCollection
{
    public AlibabaPuHuiTiFontCollection()
        : base(new Uri("fonts:AlibabaPuHuiTi", UriKind.Absolute),
               new Uri("avares://AtomUI.Fonts.AlibabaPuHuiTi/Assets", UriKind.Absolute))
    {
    }
}
