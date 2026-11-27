using Avalonia.Media.Fonts;

namespace AtomUI.Fonts.AlibabaSans;

/// <summary>
/// The Alibaba Sans fonts that are embedded in this assembly, served under the font key
/// <c>fonts:AlibabaSans</c> from the assembly's <c>Assets</c> location.
/// </summary>
public class AlibabaSansFontCollection : EmbeddedFontCollection
{
    public AlibabaSansFontCollection() : base(
        new Uri("fonts:AlibabaSans", UriKind.Absolute), 
        new Uri("avares://AtomUI.Fonts.AlibabaSans/Assets", UriKind.Absolute))
    {
    }
}