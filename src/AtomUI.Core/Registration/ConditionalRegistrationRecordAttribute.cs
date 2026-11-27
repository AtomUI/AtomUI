using System.ComponentModel;

namespace AtomUI.Registration;

/// <summary>
/// Carries build-time conditional registration definitions without introducing type roots.
/// Consumers must validate the format and complete record set before interpreting a record.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ConditionalRegistrationRecordAttribute : Attribute
{
    /// <summary>
    /// The format version of the record. Format 1 stores canonical JSON in <see cref="Payload" /> and is the
    /// only version that the generator writes.
    /// </summary>
    public int Format { get; }
    /// <summary>
    /// The kind of the record: <c>package</c> for the one record that describes the assembly,
    /// <c>fragment</c> for the registration of one control, or <c>condition</c> for one trigger that
    /// includes a fragment.
    /// </summary>
    public string Kind { get; }
    /// <summary>
    /// The key that identifies the record among the records with the same <see cref="Kind" />.
    /// </summary>
    public string Identity { get; }
    /// <summary>
    /// The data of the record as a canonical JSON object. Its fields depend on <see cref="Kind" />.
    /// </summary>
    public string Payload { get; }

    /// <summary>
    /// Creates a record attribute. The four values are stored as given and are not validated.
    /// </summary>
    public ConditionalRegistrationRecordAttribute(int format, string kind, string identity, string payload)
    {
        Format = format;
        Kind = kind;
        Identity = identity;
        Payload = payload;
    }
}
