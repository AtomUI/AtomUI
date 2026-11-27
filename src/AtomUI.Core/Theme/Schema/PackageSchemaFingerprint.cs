namespace AtomUI.Theme.Schema;

/// <summary>
/// The compile-time schema fingerprint of one registration package, emitted as a constant by the generator.
/// It only feeds the composition of the startup <c>Revision</c>. Its value need not be comparable with the
/// result of the full-traversal algorithm; it only has to be deterministic at compile time and to differ
/// whenever the content differs.
/// </summary>
internal readonly record struct PackageSchemaFingerprint(string PackageId, ulong Fingerprint);
