using System.Reflection;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

internal static class RegistrationReferenceInputs
{
    internal static IEnumerable<string> Select(IEnumerable<ITaskItem> inputs, IEnumerable<string> implementationIdentities)
    {
        var implementations = implementationIdentities.ToHashSet(StringComparer.Ordinal);
        return inputs.Where(i => i.GetMetadata("Kind") != "Reference" ||
            !implementations.Contains(AssemblyName.GetAssemblyName(i.ItemSpec).FullName!)).Select(i => i.ItemSpec);
    }
}
