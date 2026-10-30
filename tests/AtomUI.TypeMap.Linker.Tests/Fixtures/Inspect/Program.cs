using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Security.Cryptography;
using System.Text.Json;

var aotDeployment = args.Length == 3 && args[2] == "--aot-deployment";
if (args.Length != 2 && !aotDeployment) throw new ArgumentException("Usage: Inspect <linked directory> <receipt>");
using var receipt = JsonDocument.Parse(File.ReadAllText(args[1]));
var root = receipt.RootElement;
if (root.GetProperty("status").GetString() != "output-verified" || root.GetProperty("accessors").GetArrayLength() != 3)
    throw new InvalidOperationException("Receipt did not verify all three fixture slots");
foreach (var artifact in root.GetProperty("assemblies").EnumerateArray())
{
    var name = artifact.GetProperty("identity").GetString()!.Split(',')[0];
    var path = Path.Combine(args[0], name + ".dll");
    if (!File.Exists(path)) path = Directory.GetFiles(args[0], name + ".*.dll").Single();
    if (!aotDeployment && Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))) != artifact.GetProperty("outputSha256").GetString())
        throw new InvalidOperationException("Receipt hash mismatch: " + path);
    using var assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { ReadSymbols = File.Exists(Path.ChangeExtension(path, ".pdb")) });
    var types = assembly.MainModule.Types.ToArray();
    if (types.Any(t => t.Name is "UnusedTrigger" or "UnusedProxy")) throw new InvalidOperationException("Unused metadata retained: " + name);
    var accessor = types.SelectMany(t => t.Methods).Single(m => m.Name == "GetMap");
    var calls = accessor.Body.Instructions.Where(i => i.Operand is MethodReference).Select(i => (MethodReference)i.Operand).ToArray();
    if (calls.Any(m => m.DeclaringType.Name == "TypeMapping") || (!aotDeployment && (calls.First().Name != "Create" || calls.Last().Name != "Complete")))
        throw new InvalidOperationException("Unlowered accessor: " + name);
    var keys = accessor.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldstr).Select(i => (string)i.Operand).ToArray();
    var expected = name switch { "PackageA" => new[] { "alias", "used" }, "PackageB" => new[] { "dependency" }, "Empty" => Array.Empty<string>(), _ => throw new InvalidOperationException(name) };
    if (!aotDeployment && !keys.SequenceEqual(expected)) throw new InvalidOperationException("Unexpected selection: " + name);
    if (accessor.DebugInformation.HasSequencePoints || accessor.DebugInformation.Scope is not null)
        throw new InvalidOperationException("Stale debug scope: " + name);
    if (aotDeployment && !types.Any(t => t.Name == "Group")) throw new InvalidOperationException("AOT metadata missing group");
    Console.WriteLine($"ARTIFACT_PASS {name}: keys=[{string.Join(',', keys)}]; unused types/proxies absent; receiptHashChecked={!aotDeployment}; no TypeMapping call; PDB clean");
}
if (Directory.EnumerateFiles(args[0]).Any(f => Path.GetFileName(f) is "AtomUI.TypeMap.Linker.dll" or "illink.dll" or "Mono.Cecil.dll"))
    throw new InvalidOperationException("Build tool leaked to deployment");
