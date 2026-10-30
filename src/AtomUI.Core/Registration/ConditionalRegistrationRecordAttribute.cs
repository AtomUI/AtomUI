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
    public int Format { get; }
    public string Kind { get; }
    public string Identity { get; }
    public string Payload { get; }

    public ConditionalRegistrationRecordAttribute(int format, string kind, string identity, string payload)
    {
        Format = format;
        Kind = kind;
        Identity = identity;
        Payload = payload;
    }
}
