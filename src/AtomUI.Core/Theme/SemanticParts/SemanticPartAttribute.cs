namespace AtomUI.Theme;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class SemanticPartAttribute : Attribute
{
    public SemanticPartAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }
    public string? Path { get; set; }
    public string? SelectorClass { get; set; }
    public string? SelectorRoute { get; set; }
    public Type? ContractType { get; set; }
    public SemanticPartCardinality Cardinality { get; set; } = SemanticPartCardinality.Single;
    public SemanticPartCustomization Customization { get; set; } = SemanticPartCustomization.Selector;
    public string? ThemePropertyName { get; set; }
    public bool CrossVisualRoot { get; set; }
    public string? Since { get; set; }
    public bool RuntimeCreated { get; set; }
    public bool CrossNestedOwners { get; set; }

    /// <summary>
    /// 部件在控件静止状态下即为透明/隐藏（如 ImagePreviewer cover 悬停遮罩）是设计语义；
    /// 语义预览定位时跳过 Opacity&gt;0 资格过滤，仍对其描边。
    /// </summary>
    public bool RestHidden { get; set; }
}
