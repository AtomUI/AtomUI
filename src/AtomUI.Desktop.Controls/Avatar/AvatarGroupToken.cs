using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class AvatarGroupToken : AbstractControlDesignToken
{
    /// <summary>
    /// 折叠头像弹层中头像之间的间距（antd Avatar groupSpace）
    /// Spacing between avatars in the folded avatars popover (antd Avatar groupSpace)
    /// </summary>
    public double GroupSpace { get; set; }

    /// <summary>
    /// 头像组中相邻头像的重叠宽度（antd Avatar groupOverlapping）
    /// Overlapping width of adjacent avatars in the group (antd Avatar groupOverlapping)
    /// </summary>
    public double GroupOverlapping { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GroupSpace       = EffectiveGlobalToken.UniformlyMarginXXS;
        GroupOverlapping = EffectiveGlobalToken.UniformlyMarginXS;
    }
}
