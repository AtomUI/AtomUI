using System.Collections.Concurrent;

namespace AtomUI.Controls.Data;

/// <summary>
/// Holds the <see cref="IDataMemberAccessorDescriptor" /> of each registered data type, for the whole process.
/// </summary>
/// <remarks>
/// <para>
/// The accessors generated for <see cref="GenerateDataMemberAccessorsAttribute" /> register their descriptor when
/// the assembly that contains them loads.
/// </para>
/// </remarks>
public static class DataMemberAccessorRegistry
{
    private static readonly ConcurrentDictionary<Type, IDataMemberAccessorDescriptor> s_descriptors = new();
    private static readonly ConcurrentDictionary<Type, DescriptorLookupResult> s_compatibleDescriptorCache = new();

    /// <summary>
    /// Registers a descriptor for its <see cref="IDataMemberAccessorDescriptor.DataType" />, replacing the descriptor
    /// that was registered for that type.
    /// </summary>
    /// <param name="descriptor">
    /// The descriptor to register. Its data type is the key.
    /// </param>
    public static void Register(IDataMemberAccessorDescriptor descriptor)
    {
        s_descriptors[descriptor.DataType] = descriptor;
        s_compatibleDescriptorCache.Clear();
    }

    /// <summary>
    /// Looks up the descriptor that was registered for a data type.
    /// </summary>
    /// <param name="dataType">
    /// The data type to look for. A descriptor of a base type or an interface of it is not found;
    /// <see cref="TryGetCompatible" /> covers that case.
    /// </param>
    /// <param name="descriptor">
    /// The registered descriptor, or <see langword="null" /> when the method returns <see langword="false" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a descriptor is registered for <paramref name="dataType" />; otherwise
    /// <see langword="false" />.
    /// </returns>
    public static bool TryGet(Type dataType, out IDataMemberAccessorDescriptor descriptor)
    {
        return s_descriptors.TryGetValue(dataType, out descriptor!);
    }

    /// <summary>
    /// Looks up the descriptor that fits a data type, also when only its base type or an interface of it is registered.
    /// </summary>
    /// <param name="dataType">
    /// The data type that needs a descriptor.
    /// </param>
    /// <param name="descriptor">
    /// The descriptor registered for the type or for the most specific registered base type or interface of it, or
    /// <see langword="null" /> when the method returns <see langword="false" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when a fitting descriptor is found; otherwise <see langword="false" />.
    /// </returns>
    /// <remarks>
    /// <para>
    /// When several registered types fit, a type wins over its base type and a class wins over an interface; between
    /// interfaces that are not derived from one another the choice is not specified. The result is cached per data type
    /// until the next <see cref="Register" /> call.
    /// </para>
    /// </remarks>
    public static bool TryGetCompatible(Type dataType, out IDataMemberAccessorDescriptor descriptor)
    {
        if (TryGet(dataType, out descriptor!))
        {
            return true;
        }

        var result = s_compatibleDescriptorCache.GetOrAdd(dataType, FindCompatibleDescriptor);
        if (result.Descriptor != null)
        {
            descriptor = result.Descriptor;
            return true;
        }

        descriptor = null!;
        return false;
    }

    private static DescriptorLookupResult FindCompatibleDescriptor(Type dataType)
    {
        IDataMemberAccessorDescriptor? bestDescriptor = null;
        foreach (var descriptor in s_descriptors.Values)
        {
            var descriptorType = descriptor.DataType;
            if (!descriptorType.IsAssignableFrom(dataType))
            {
                continue;
            }

            if (bestDescriptor == null || IsMoreSpecific(descriptorType, bestDescriptor.DataType))
            {
                bestDescriptor = descriptor;
            }
        }

        return new DescriptorLookupResult(bestDescriptor);
    }

    private static bool IsMoreSpecific(Type candidateType, Type currentType)
    {
        if (candidateType == currentType)
        {
            return false;
        }

        if (currentType.IsAssignableFrom(candidateType))
        {
            return true;
        }

        return candidateType.IsClass && currentType.IsInterface;
    }

    private readonly struct DescriptorLookupResult
    {
        public IDataMemberAccessorDescriptor? Descriptor { get; }

        public DescriptorLookupResult(IDataMemberAccessorDescriptor? descriptor)
        {
            Descriptor = descriptor;
        }
    }
}
