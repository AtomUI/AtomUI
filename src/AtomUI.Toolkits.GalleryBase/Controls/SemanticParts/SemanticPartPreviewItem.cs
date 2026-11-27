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

    /// <summary>
    /// The type of the owner the part is declared on. A preview that covers several owners uses it to label
    /// the owner in the part list and to resolve the highlight of the part per owner.
    /// </summary>
    internal Type OwnerType { get; }

    /// <summary>
    /// Short name of the owner type, shown on the list row.
    /// </summary>
    public string OwnerTypeName => OwnerType.Name;

    /// <summary>
    /// Whether the list row shows which owner the part belongs to. It is <see langword="true" /> only when
    /// the preview covers several owners; a single-owner preview hides the label to avoid noise.
    /// </summary>
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
