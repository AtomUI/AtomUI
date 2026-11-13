namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// 更小的组件高度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double ControlHeightXS { get; set; }

    /// <summary>
    /// 较小的组件高度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double ControlHeightSM { get; set; }

    /// <summary>
    /// 较高的组件高度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial double ControlHeightLG { get; set; }
}