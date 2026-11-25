using System.Collections.Concurrent;
using System.Reflection;

namespace AtomUI.Theme.Schema;

/// <summary>
/// Generated into a control package: the SHA-256 digest of its control token schema (every bound control token class
/// with its token names and value types). A value package records the digest it was built against (spec 3.3).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class TokenSchemaDigestAttribute(string digest) : Attribute
{
    public string Digest { get; } = digest;
}

/// <summary>
/// Generated into a value package, once per control package whose token classes it derives: the control package's
/// assembly name and the <see cref="TokenSchemaDigestAttribute"/> digest it was built against.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class CoversTokenSchemaAttribute(string assemblyName, string digest) : Attribute
{
    public string AssemblyName { get; } = assemblyName;
    public string Digest { get; } = digest;
}

/// <summary>
/// Spec 8, check 4: a value package was built against the token schema of the control package that is actually loaded.
/// A control package that gained a token after the value package was built would otherwise run with a missing value.
/// </summary>
internal static class TokenSchemaDigests
{
    private static readonly ConcurrentDictionary<(Assembly ValuePackage, Assembly ControlPackage), string?> s_failures = new();

    /// <summary>Throws when the value package records a digest for the control package that differs from the loaded one.</summary>
    internal static void Validate(Assembly valuePackage, Assembly controlPackage)
    {
        if (ReferenceEquals(valuePackage, controlPackage))
        {
            return;
        }

        var failure = s_failures.GetOrAdd((valuePackage, controlPackage), static key =>
        {
            var name = key.ControlPackage.GetName().Name ?? string.Empty;
            string? recorded = null;
            foreach (var covers in key.ValuePackage.GetCustomAttributes<CoversTokenSchemaAttribute>())
            {
                if (string.Equals(covers.AssemblyName, name, StringComparison.Ordinal))
                {
                    recorded = covers.Digest;
                    break;
                }
            }

            var actual = key.ControlPackage.GetCustomAttribute<TokenSchemaDigestAttribute>()?.Digest;
            return Describe(key.ValuePackage.GetName().Name ?? string.Empty, name, recorded, actual);
        });
        if (failure is not null)
        {
            throw new ThemeSchemaException(failure);
        }
    }

    /// <summary>The failure message, or <see langword="null"/> when the digests agree or one side records none.</summary>
    internal static string? Describe(string valuePackage, string controlPackage, string? recorded, string? actual) =>
        recorded is null || actual is null || string.Equals(recorded, actual, StringComparison.Ordinal)
            ? null
            : $"Value package '{valuePackage}' was built against the token schema of '{controlPackage}' with digest " +
              $"'{recorded}', but the loaded '{controlPackage}' has digest '{actual}'. Install a version of " +
              $"'{valuePackage}' built against this '{controlPackage}'.";
}
