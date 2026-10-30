using System.Text;

namespace AtomUI.SourceGeneration;

internal static class GeneratedCodeNamespace
{
    private const string RootNamespace = "AtomUI.Generated";

    internal static string ForAssembly(string? assemblyName)
    {
        return string.IsNullOrWhiteSpace(assemblyName)
            ? RootNamespace
            : RootNamespace + "." + CreateOwnerIdentifier(assemblyName!);
    }

    private static string CreateOwnerIdentifier(string assemblyName)
    {
        var identifier = new StringBuilder(assemblyName.Length + 8);
        var capitalizeNext = true;
        foreach (var character in assemblyName)
        {
            if (!char.IsLetterOrDigit(character))
            {
                capitalizeNext = true;
                continue;
            }

            if (identifier.Length == 0 && char.IsDigit(character))
            {
                identifier.Append("Assembly");
            }

            identifier.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return identifier.Length == 0 ? "Assembly" : identifier.ToString();
    }
}
