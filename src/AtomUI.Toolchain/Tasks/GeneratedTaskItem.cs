using System.Collections;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

internal sealed class GeneratedTaskItem : ITaskItem
{
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);

    internal GeneratedTaskItem(string itemSpec)
    {
        ItemSpec = itemSpec;
    }

    public string ItemSpec { get; set; }

    public int MetadataCount => _metadata.Count;

    public ICollection MetadataNames => _metadata.Keys.ToArray();

    public string GetMetadata(string metadataName)
    {
        return _metadata.TryGetValue(metadataName, out var value) ? value : string.Empty;
    }

    public void SetMetadata(string metadataName, string metadataValue)
    {
        _metadata[metadataName] = metadataValue;
    }

    public void RemoveMetadata(string metadataName)
    {
        _metadata.Remove(metadataName);
    }

    public void CopyMetadataTo(ITaskItem destinationItem)
    {
        foreach (var entry in _metadata)
        {
            destinationItem.SetMetadata(entry.Key, entry.Value);
        }
    }

    public IDictionary CloneCustomMetadata()
    {
        return new Dictionary<string, string>(_metadata, StringComparer.OrdinalIgnoreCase);
    }
}
