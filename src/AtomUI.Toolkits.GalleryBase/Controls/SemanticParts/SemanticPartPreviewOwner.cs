using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// 一个 Semantic Part Preview 的 owner 声明：owner 实例锚点加可选的 owner 类型。
/// 单个 Preview 需要覆盖多个公开 owner 时（例如 Message 的 notice 卡片与列表宿主），
/// 把它们依次声明在 <see cref="SemanticPartPreview.SemanticOwners" /> 上，
/// Preview 会按声明顺序合并各 descriptor 的 Part 列表，并在高亮时按 Part 所属 owner 解析。
/// </summary>
public class SemanticPartPreviewOwner : AvaloniaObject
{
    /// <summary>
    /// owner 实例锚点，用于确定 owner type、校验兼容性，并在 PreviewContent 中定位该类型的所有实例。
    /// </summary>
    public static readonly StyledProperty<Control?> OwnerProperty =
        AvaloniaProperty.Register<SemanticPartPreviewOwner, Control?>(nameof(Owner));

    /// <summary>
    /// owner 类型契约；未指定时取 <see cref="Owner" /> 的精确 CLR 类型，不做类型层次反射发现。
    /// </summary>
    public static readonly StyledProperty<Type?> OwnerTypeProperty =
        AvaloniaProperty.Register<SemanticPartPreviewOwner, Type?>(nameof(OwnerType));

    /// <summary>
    /// owner 实例锚点，用于确定 owner type、校验兼容性，并在 PreviewContent 中定位该类型的所有实例。
    /// </summary>
    public Control? Owner
    {
        get => GetValue(OwnerProperty);
        set => SetValue(OwnerProperty, value);
    }

    /// <summary>
    /// owner 类型契约；未指定时取 <see cref="Owner" /> 的精确 CLR 类型，不做类型层次反射发现。
    /// </summary>
    public Type? OwnerType
    {
        get => GetValue(OwnerTypeProperty);
        set => SetValue(OwnerTypeProperty, value);
    }
}
