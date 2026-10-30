namespace AtomUI.Toolkits.GalleryBase.Controls;

public sealed class SemanticPartDescription
{
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Part 所属的 owner 类型。单个 Preview 覆盖多个 owner 时用于消歧（如 Message 的 <c>root</c>
    /// 在 MessageCard 与 WindowMessageManager 上都存在）；单 owner Preview 可省略。
    /// </summary>
    public Type? OwnerType { get; set; }

    public string? Description { get; set; }

    public string? CodeSnippet { get; set; }
}
