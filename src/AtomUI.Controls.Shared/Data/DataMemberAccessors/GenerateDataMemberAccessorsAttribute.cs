namespace AtomUI.Controls.Data;

/// <summary>
/// Generates accessors for the instance properties of a data type and registers them in
/// <see cref="DataMemberAccessorRegistry" />, so that data-bound controls can resolve member paths of the type
/// without reflection.
/// </summary>
/// <remarks>
/// <para>
/// Apply it to a non-generic class, struct or interface. The generator creates one
/// <see cref="IDataMemberAccessor" /> for each public or internal instance property with an accessible getter,
/// including the properties that base types or inherited interfaces declare, and a factory for new items when the
/// type has a default constructor. Generic types get no accessor.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public sealed class GenerateDataMemberAccessorsAttribute : Attribute
{
}
