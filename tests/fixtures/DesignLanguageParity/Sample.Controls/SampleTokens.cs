using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace Sample.Controls;

/// <summary>The tokens of <see cref="SampleBadge"/>: declarations only.</summary>
[ControlDesignToken]
public sealed partial class SampleBadgeToken : AbstractControlDesignToken
{
    /// <summary>Surface color of the badge.</summary>
    public partial Color BadgeBackground { get; set; }

    /// <summary>Edge length of the badge.</summary>
    public partial double BadgeSize { get; set; }
}

/// <summary>The tokens of <see cref="SampleTag"/>: declarations only.</summary>
[ControlDesignToken]
public sealed partial class SampleTagToken : AbstractControlDesignToken
{
    /// <summary>Surface color of the tag.</summary>
    public partial Color TagBackground { get; set; }
}
