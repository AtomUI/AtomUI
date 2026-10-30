using AtomUI.Registration;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#pragma warning disable IL2026, IL3050, SYSLIB5004 // The exact generated TypeMap declarations/accessor ABI is the subject of this fixture.
[assembly: ControlPackageMarker("Empty", typeof(Empty.Group), 1)]
namespace Empty;
public sealed class Group
{
    private Group() { }
    internal static class BrowserMap
    {
        public static Dictionary<string, Type> Create() => new(StringComparer.Ordinal);
        public static void Add(Dictionary<string, Type> map, string key, RuntimeTypeHandle handle) => map.Add(key, Type.GetTypeFromHandle(handle)!);
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IReadOnlyDictionary<string, Type> Complete(Dictionary<string, Type> map) => map;
    }
}
public static class Entry
{
    [GeneratedTypeMapAccessor(typeof(Group))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Group.BrowserMap))]
    internal static IReadOnlyDictionary<string, Type> GetMap() => TypeMapping.GetOrCreateExternalTypeMapping<Group>();
    public static IReadOnlyDictionary<string, Type> Query() => GetMap();
}
#pragma warning restore IL2026, IL3050, SYSLIB5004
