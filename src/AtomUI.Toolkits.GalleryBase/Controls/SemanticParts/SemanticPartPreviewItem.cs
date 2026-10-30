using System.ComponentModel;
using System.Runtime.CompilerServices;
using AtomUI.Theme.Schema;

namespace AtomUI.Toolkits.GalleryBase.Controls;

internal sealed class SemanticPartPreviewItem : INotifyPropertyChanged
{
    private bool _isPinned;

    public SemanticPartPreviewItem(
        SemanticPartDescriptor descriptor,
        Type ownerType,
        string description,
        string? codeSnippet)
    {
        Descriptor      = descriptor;
        OwnerType       = ownerType;
        Description     = description;
        CodeSnippet     = codeSnippet;
        Name            = descriptor.Name;
        Path            = descriptor.Path;
        Selector        = descriptor.SelectorClass is null ? "root" : $".{descriptor.SelectorClass}";
        SelectorRoute   = descriptor.SelectorRoute ?? "root";
        ContractType    = descriptor.ContractType.Name;
        StyleType       = descriptor.StyleType?.FullName ?? "-";
        Cardinality     = descriptor.Cardinality.ToString();
        Customization   = descriptor.Customization.ToString();
        Since           = descriptor.Since;
        IsCrossRoot     = descriptor.CrossVisualRoot;
        IsRuntimeCreated = descriptor.RuntimeCreated;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    internal SemanticPartDescriptor Descriptor { get; }

    /// <summary>Part 所属 owner 类型；多 owner Preview 用于在 UI 上标注归属并按 owner 解析高亮。</summary>
    internal Type OwnerType { get; }

    /// <summary>owner 类型短名，供列表行显示。</summary>
    public string OwnerTypeName => OwnerType.Name;

    /// <summary>多 owner Preview 时在列表行标注 Part 归属；单 owner 时隐藏，避免噪音。</summary>
    public bool IsOwnerLabelVisible { get; internal set; }

    internal string? CodeSnippet { get; }

    public string Name { get; }

    public string Path { get; }

    public string Selector { get; }

    public string SelectorRoute { get; }

    public string ContractType { get; }

    public string StyleType { get; }

    public string Cardinality { get; }

    public string Customization { get; }

    public string? Since { get; }

    public string Description { get; }

    public bool IsCrossRoot { get; }

    public bool IsRuntimeCreated { get; }

    public bool IsPinned
    {
        get => _isPinned;
        internal set => SetField(ref _isPinned, value);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
