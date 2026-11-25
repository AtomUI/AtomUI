using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTransferToken : AbstractControlDesignToken
{
    /// <summary>Width of one source or target list panel.</summary>
    public partial double ListWidth { get; set; }

    /// <summary>Height of one source or target list panel.</summary>
    public partial double ListHeight { get; set; }

    /// <summary>Height reserved for a list panel header.</summary>
    public partial double HeaderHeight { get; set; }

    /// <summary>Inset of the header content and selection controls.</summary>
    public partial Thickness HeaderPadding { get; set; }
}
