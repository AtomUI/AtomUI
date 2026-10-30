using AtomUI.Registration;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#pragma warning disable IL2026, IL3050, SYSLIB5004 // The exact generated TypeMap declarations/accessor ABI is the subject of this fixture.
[assembly: ControlPackageMarker("PackageB", typeof(PackageB.Group), 1)]
[assembly: TypeMap<PackageB.Group>("dependency", typeof(PackageB.DependencyProxy), typeof(PackageB.DependencyTrigger))]
[assembly: TypeMap<PackageB.Group>("unused", typeof(PackageB.UnusedProxy), typeof(PackageB.UnusedTrigger))]
namespace PackageB;
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
public sealed class DependencyTrigger { }
public sealed class UnusedTrigger { }
[DependencyProxy]
internal sealed class DependencyProxy : ControlRegistrationFragmentAttribute
{
    public override string FragmentId => "PackageB:DependencyProxy";
    public override void Add(ControlPackageRegistrationBuilder builder)
    {
        builder.Collected.Add("DependencyProxy");
    }
}
[UnusedProxy]
internal sealed class UnusedProxy : ControlRegistrationFragmentAttribute
{
    public override string FragmentId => "PackageB:UnusedProxy";
    public override void Add(ControlPackageRegistrationBuilder builder)
    {
        builder.Collected.Add("UnusedProxy");
    }
}
#pragma warning restore IL2026, IL3050, SYSLIB5004
