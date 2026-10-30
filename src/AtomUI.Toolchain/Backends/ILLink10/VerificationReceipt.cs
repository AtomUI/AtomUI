using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker;

// A build receipt is optional for the extension ABI; automated build integration must require and validate it.
// It describes verified inputs/outputs only. Nothing here is runtime registration input.
internal sealed class VerificationReceipt : IStep
{
    private readonly string _path;
    private readonly IReadOnlyList<MapSlot> _slots;
    private readonly Dictionary<AssemblyDefinition, string> _inputHashes;
    private readonly Dictionary<MethodDefinition, string> _bodyHashes;
    private readonly HashSet<AssemblyDefinition> _symbolOutputs;
    private readonly RegistrationMetadataIndex _metadata;
    internal bool SweepVerified { get; set; }

    private VerificationReceipt(LinkContext context, string path, IReadOnlyList<MapSlot> slots,
        RegistrationMetadataIndex metadata)
    {
        _path = path;
        _slots = slots;
        _metadata = metadata;
        _inputHashes = slots.Select(s => s.Accessor.Module.Assembly).Distinct().ToDictionary(a => a,
            a => HashFile(context.GetAssemblyLocation(a)));
        _symbolOutputs = slots.Select(s => s.Accessor.Module.Assembly).Where(a => context.LinkSymbols && a.MainModule.HasSymbols).ToHashSet();
        _bodyHashes = slots.ToDictionary(s => s.Accessor, s => HashBody(s.Accessor));
    }

    internal static string? Prepare(LinkContext context)
    {
        if (!context.TryGetCustomData("AtomUITypeMapReceipt", out var path))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
        {
            throw BackendDiagnostic.Unsupported("AtomUITypeMapReceipt must be an absolute build-output file path.");
        }

        File.Delete(path);
        return path;
    }

    internal static VerificationReceipt? Schedule(LinkContext context, string? path, IReadOnlyList<MapSlot> slots,
        RegistrationMetadataIndex? metadata = null)
    {
        if (path is null)
        {
            return null;
        }

        if (!context.Pipeline.ContainsStep(typeof(OutputStep)))
        {
            throw BackendDiagnostic.Unsupported("The TypeMap receipt requires the standard ILLink OutputStep after Sweep.");
        }

        var receipt = new VerificationReceipt(context, path, slots, metadata ?? new RegistrationMetadataIndex());
        context.Pipeline.AddStepAfter(typeof(OutputStep), receipt);
        return receipt;
    }

    public void Process(LinkContext context)
    {
        if (context.ErrorsCount != 0)
        {
            return;
        }

        try
        {
            if (!SweepVerified)
            {
                throw BackendDiagnostic.Unlowered("TypeMap output receipt cannot be emitted without successful post-Sweep verification.");
            }

            var artifacts = new List<object>();
            foreach (var (assembly, inputHash) in _inputHashes)
            {
                var output = Path.Combine(context.OutputDirectory, assembly.MainModule.Name);
                if (!File.Exists(output))
                {
                    throw BackendDiagnostic.Unlowered($"Verified assembly '{assembly.Name.FullName}' was not emitted by ILLink.");
                }

                var symbols = Path.ChangeExtension(output, ".pdb");
                var readSymbols = _symbolOutputs.Contains(assembly) || File.Exists(symbols);
                using var emitted = AssemblyDefinition.ReadAssembly(output, new ReaderParameters
                {
                    ReadSymbols = true, SymbolReaderProvider = new DefaultSymbolReaderProvider(throwIfNoSymbol: false),
                    AssemblyResolver = context.Resolver
                });
                if (readSymbols && !emitted.MainModule.HasSymbols)
                {
                    throw BackendDiagnostic.Unlowered($"Expected emitted symbols for '{assembly.Name.FullName}' are missing or unreadable.");
                }

                if (emitted.Name.FullName != assembly.Name.FullName)
                {
                    throw BackendDiagnostic.Unlowered($"Output identity for '{assembly.Name.FullName}' changed after Sweep.");
                }

                var emittedMethods = _metadata.Methods(emitted);
                foreach (var slot in _slots.Where(s => s.Accessor.Module.Assembly == assembly))
                {
                    var methods = emittedMethods
                        .Where(m => m.FullName == slot.AccessorIdentity).Take(2).ToArray();
                    if (methods.Length != 1 || !methods[0].HasBody)
                    {
                        throw BackendDiagnostic.Unlowered($"Emitted accessor '{slot.AccessorIdentity}' is missing, ambiguous or has no body.");
                    }

                    var method = methods[0];
                    slot.Contract.Validate(emitted, method, _metadata);
                    if (HashBody(method) != _bodyHashes[slot.Accessor])
                    {
                        throw BackendDiagnostic.Unlowered($"Emitted accessor '{slot.Accessor.FullName}' does not match its verified lowered body.");
                    }
                }
                artifacts.Add(new { identity = assembly.Name.FullName, inputSha256 = inputHash, outputSha256 = HashFile(output), outputSymbolsSha256 = File.Exists(symbols) ? HashFile(symbols) : null });
            }
            var content = JsonSerializer.Serialize(new
            {
                status = "output-verified", abi = 1, capability = ToolchainContract.Capability,
                toolIdentity = typeof(MaterializeTypeMapsStep).Assembly.FullName,
                toolSha256 = HashFile(typeof(MaterializeTypeMapsStep).Assembly.Location),
                linkerIdentity = typeof(LinkContext).Assembly.FullName, linkerVersion = ToolchainContract.ActualLinkerVersion,
                accessors = _slots.Select(s => new { packageId = s.PackageId, group = RegistrationAbi.Identity(s.Group),
                    assembly = s.Accessor.Module.Assembly.Name.FullName, method = s.Accessor.FullName, bodySha256 = _bodyHashes[s.Accessor] }),
                assemblies = artifacts
            }, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, content);
        }
        catch (BackendDiagnostic diagnostic) { diagnostic.Report(context); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or BadImageFormatException or SymbolsNotFoundException or SymbolsNotMatchingException)
        {
            BackendDiagnostic.Unlowered($"Could not validate/write TypeMap output receipt '{_path}': {error.Message}").Report(context);
        }
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
    private static string HashBody(MethodDefinition method)
    {
        var instructions = method.Body.Instructions.Select(i => new
        {
            opcode = i.OpCode.Code.ToString(), operand = i.Operand switch
            {
                MethodReference m => m.FullName + ", " + Scope(m.DeclaringType),
                TypeReference t => t.FullName + ", " + Scope(t),
                null => null,
                string text => text,
                _ => throw BackendDiagnostic.Unlowered($"Unsupported operand in lowered accessor '{method.FullName}'.")
            }
        });
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(instructions))));
    }
    private static string Scope(TypeReference type) => type.Scope switch
    {
        ModuleDefinition module => module.Assembly.Name.FullName,
        AssemblyNameReference assembly => assembly.FullName,
        _ => throw BackendDiagnostic.Unlowered($"Unknown lowered operand scope for '{type.FullName}'.")
    };
}
