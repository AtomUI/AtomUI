using AtomUI.Registration;
using System.Reflection;
using System.Runtime.InteropServices;
#pragma warning disable SYSLIB5004 // Bootstrap metadata under test.
[assembly: TypeMapAssemblyTarget<PackageA.Group>("PackageA")]
[assembly: TypeMapAssemblyTarget<PackageB.Group>("PackageB")]
[assembly: TypeMapAssemblyTarget<Empty.Group>("Empty")]
#pragma warning restore SYSLIB5004

Console.WriteLine(typeof(PackageA.UsedTrigger).Name);
Console.WriteLine(typeof(PackageA.AliasTrigger).Name);
var first = PackageA.Entry.Query();
var second = PackageB.Entry.Query();
var empty = Empty.Entry.Query();
if (!first.TryGetValue("used", out var used) || !first.TryGetValue("alias", out var alias) || used != alias || first.TryGetValue("unused", out _))
    throw new InvalidOperationException("Selected/alias map mismatch");
var builder = new ControlPackageRegistrationBuilder();
var fragment = used.GetCustomAttribute<ControlRegistrationFragmentAttribute>() ?? throw new InvalidOperationException("Proxy activation failed");
fragment.Add(builder);
if (!second.TryGetValue("dependency", out var dependency) || second.TryGetValue("unused", out _) || empty.TryGetValue("anything", out _))
    throw new InvalidOperationException("Dependency/empty map mismatch");
(dependency.GetCustomAttribute<ControlRegistrationFragmentAttribute>() ?? throw new InvalidOperationException("Dependency activation failed")).Add(builder);
Console.WriteLine("ATOMUI_TYPEMAP_PROBE_PASS used,alias,dependency; empty=ok; fragments=" + string.Join(",", builder.Collected));
