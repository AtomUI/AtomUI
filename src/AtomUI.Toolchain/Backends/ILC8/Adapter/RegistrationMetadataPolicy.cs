using Internal.TypeSystem;

namespace ILCompiler;

// Build records have no runtime contract. Blocking their exact resolved type also
// prevents CustomAttributeBasedDependencyAlgorithm from rooting their constructors/blobs.
internal sealed class RegistrationMetadataPolicy : MetadataBlockingPolicy
{
    private readonly MetadataBlockingPolicy _inner;
    private readonly MetadataType _recordAttribute;

    public RegistrationMetadataPolicy(MetadataBlockingPolicy inner, MetadataType recordAttribute)
    {
        _inner = inner;
        _recordAttribute = recordAttribute;
    }

    public override bool IsBlocked(MetadataType type) => type == _recordAttribute || _inner.IsBlocked(type);
    public override bool IsBlocked(MethodDesc method) => method.OwningType.GetTypeDefinition() == _recordAttribute || _inner.IsBlocked(method);
    public override bool IsBlocked(FieldDesc field) => field.OwningType.GetTypeDefinition() == _recordAttribute || _inner.IsBlocked(field);
}
