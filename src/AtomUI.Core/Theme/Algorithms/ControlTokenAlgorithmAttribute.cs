namespace AtomUI.Theme.Algorithms;

/// <summary>
/// Declares an <see cref="IControlTokenAlgorithm{TToken}"/> class as the control token stage of the algorithm
/// <c>Catalog:Id</c>. A class may serve several algorithms, for example the light and dark variant of one family.
/// </summary>
/// <remarks>
/// The AtomUI generator turns every attribute into one descriptor registered by the assembly's
/// <c>GeneratedThemeAlgorithmRegistration.Register</c>. The class needs a public parameterless constructor.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ControlTokenAlgorithmAttribute(string catalog, string id, int revision) : Attribute
{
    /// <summary>
    /// The catalog of the algorithm this class serves.
    /// </summary>
    public string Catalog { get; } = catalog;

    /// <summary>
    /// The id of the algorithm this class serves.
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// The revision of this control stage; raise it whenever its output changes so persisted caches are invalidated.
    /// </summary>
    public int Revision { get; } = revision;
}
