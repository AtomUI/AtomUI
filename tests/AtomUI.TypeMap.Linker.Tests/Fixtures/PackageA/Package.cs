using AtomUI.Registration;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#pragma warning disable IL2026, IL3050, SYSLIB5004 // The exact generated TypeMap declarations/accessor ABI is the subject of this fixture.
#if BAD_ABI
[assembly: ControlPackageMarker("PackageA", typeof(PackageA.Group), 2)]
#else
[assembly: ControlPackageMarker("PackageA", typeof(PackageA.Group), 1)]
#endif
[assembly: TypeMap<PackageA.Group>("used", typeof(PackageA.UsedProxy), typeof(PackageA.UsedTrigger))]
[assembly: TypeMap<PackageA.Group>("alias", typeof(PackageA.UsedProxy), typeof(PackageA.AliasTrigger))]
[assembly: TypeMap<PackageA.Group>("unused", typeof(PackageA.UnusedProxy), typeof(PackageA.UnusedTrigger))]
namespace PackageA;
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
#if !OMIT_HELPER_ROOT
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Group.BrowserMap))]
#endif
    internal static IReadOnlyDictionary<string, Type> GetMap() => TypeMapping.GetOrCreateExternalTypeMapping<Group>();
    public static IReadOnlyDictionary<string, Type> Query() => GetMap();
}
public sealed class UsedTrigger { }
public sealed class AliasTrigger { }
public sealed class UnusedTrigger { }
[UsedProxy]
internal sealed class UsedProxy : ControlRegistrationFragmentAttribute
{
    public override string FragmentId => "PackageA:UsedProxy";
    public override void Add(ControlPackageRegistrationBuilder builder)
    {
        builder.Collected.Add(typeof(PackageB.DependencyTrigger).Name);
    }
}
[UnusedProxy]
internal sealed class UnusedProxy : ControlRegistrationFragmentAttribute
{
    public override string FragmentId => "PackageA:UnusedProxy";
    public override void Add(ControlPackageRegistrationBuilder builder)
    {
        builder.Collected.Add("UnusedProxy");
    }
}
#pragma warning restore IL2026, IL3050, SYSLIB5004
