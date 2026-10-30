using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using ILCompiler.DependencyAnalysis;
using ILCompiler.DependencyAnalysisFramework;
using Internal.IL;
using Internal.IL.Stubs;
using Internal.TypeSystem;

namespace ILCompiler;

internal sealed class ConditionalRegistrationBackend : ILProvider, ICompilationRootProvider
{
    private readonly ILProvider _inner;
    private readonly bool _useScanner;
    private readonly RegistrationDefinitions.Group[] _groups;
    private bool _frozen;
    private readonly MetadataType _recordAttribute;
    private readonly string _reportPath;
    private readonly string _inputHash;
    private readonly string _inputPath;
    private readonly MethodDesc _trimmedGetter;
    private readonly string _receiptSymbol;
    private readonly AtomUI.Build.Tasks.Registration.Native8Invocation _invocation;
    private string _objectFile;
    public IEnumerable<string> ExportSymbols(IEnumerable<string> symbols) => (symbols ?? Array.Empty<string>()).Append(_receiptSymbol);


    public ILProvider PreinitializationView { get; }

    public ConditionalRegistrationBackend(ILProvider inner, CompilerTypeSystemContext context, string inputs, bool useScanner)
    {
        _inner = inner;
        _inputPath = Path.GetFullPath(inputs);
        _useScanner = useScanner;
        ConditionalInputBinding binding = ConditionalInputBinder.Read(inputs, context, inner);
        _groups = binding.Groups;
        _recordAttribute = binding.RecordAttribute;
        _reportPath = binding.ReportPath;
        _inputHash = binding.InputHash;
        _trimmedGetter = binding.TrimmedGetter;
        _invocation = RegistrationInputBootstrap.Invocation ?? throw new InvalidDataException("Missing caller invocation");
        _invocation.RequireExact(binding.ReportPath, "analysis.json");
        _receiptSymbol = "__atomui_compile_" + _invocation.Id;
        foreach (RegistrationDefinitions.Group group in _groups)
        {
            group.Table = new RegistrationTableNode(group);
        }
        PreinitializationView = new PreinitBarrier(this);
        Console.WriteLine("[AtomUI ILC8] host=" + RuntimeInformation.FrameworkDescription + " scanner=" + _useScanner);
    }

    public MetadataBlockingPolicy WrapMetadataBlockingPolicy(MetadataBlockingPolicy inner) => new RegistrationMetadataPolicy(inner, _recordAttribute);

    public void AddCompilationRoots(IRootingServiceProvider rootProvider)
    {
        rootProvider.RootReadOnlyDataBlob(System.Text.Encoding.ASCII.GetBytes(_receiptSymbol), 1,
            "AtomUI compilation receipt", _receiptSymbol);
        if (_useScanner && _frozen)
        {
            return;
        }
        foreach (RegistrationDefinitions.Group group in _groups)
        {
            rootProvider.AddCompilationRoot(new RequestNode(group, _useScanner), "AtomUI candidate declarations");
        }
    }

    public void FreezeScanner(NodeFactory factory)
    {
        if (!_useScanner || _frozen)
        {
            throw new InvalidOperationException("Invalid registration scanner phase");
        }
        foreach (RegistrationDefinitions.Group group in _groups)
        {
            group.ScannerRequested = group.Table.Marked;
            group.FrozenEntries = group.ScannerRequested ? group.Table.Selected(factory) : Array.Empty<RegistrationDefinitions.Entry>();
            foreach (RegistrationDefinitions.Entry entry in group.FrozenEntries)
            {
                if (factory.MethodEntrypoint(entry.Method) is not ScannedMethodNode scanned || !scanned.StaticDependenciesAreComputed || scanned.Exception != null)
                {
                    throw new InvalidOperationException("Selected registration method did not complete scanner analysis: " + entry.Key);
                }
            }
        }
        _frozen = true;
    }

    public void AbortCompilation(string objectFile, string exportsFile)
    {
        _invocation.DeleteOwned(new[] { objectFile, exportsFile, _invocation.File("analysis.json"), _invocation.File("receipt.json") });
    }

    public void VerifyAndWriteReport(NodeFactory factory, string objectFile)
    {
        _objectFile = Path.GetFullPath(objectFile);
        var groups = _groups.Select(group =>
        {
            if (factory.MethodEntrypoint(group.FullCollector).Marked)
            {
                throw new InvalidOperationException("Full registration collector survived selected codegen: " + group.FullCollector);
            }
            bool requested = _useScanner ? group.ScannerRequested : group.Table.Marked;
            RegistrationDefinitions.Entry[] selected = _useScanner ? group.FrozenEntries : requested ? group.Table.Selected(factory) : Array.Empty<RegistrationDefinitions.Entry>();
            foreach (RegistrationDefinitions.Entry entry in selected)
            {
                // Static dispatch can inline an already-scanned thunk completely.
                // A native table, however, requires an emitted addressable entrypoint.
                if (!_useScanner && !factory.MethodEntrypoint(entry.Method).Marked)
                {
                    throw new InvalidOperationException("Selected entrypoint is missing from final codegen graph: " + entry.Key);
                }
            }
            return new
            {
                group.Id,
                group.PackageId,
                Requested = requested,
                Selected = selected.Select(e => e.Key).ToArray(),
                SelectedFragments = selected.Select(e => e.FragmentId).Distinct().ToArray(),
                UniqueMethods = selected.Select(e => e.Method).Distinct().Count(),
                SelectedMethodSymbols = selected.Select(e => e.Method).Distinct().Select(m => factory.NameMangler.GetMangledMethodName(m).ToString()).ToArray(),
                NativeTableEmitted = !_useScanner && group.Table.Marked,
                Materialization = _useScanner ? "static-dispatch" : "native-table",
                group.SymbolName,
                Conditions = group.Entries.Select(e => new
                {
                    e.Key,
                    Condition = new { e.Trigger.Assembly, Name = e.Trigger.MetadataName },
                    FinalTypeMarked = factory.NecessaryTypeSymbol(e.Condition).Marked,
                    FinalMethodMarked = factory.MethodEntrypoint(e.Method).Marked,
                    MethodSymbol = factory.NameMangler.GetMangledMethodName(e.Method).ToString()
                }).ToArray()
            };
        }).ToArray();
        File.WriteAllText(_reportPath, JsonSerializer.Serialize(new
            {
                Backend = "AtomUI.Registration.ILC8",
                InputKind = "conditional-record-v1",
                InputHash = _inputHash,
                InputPath = _inputPath,
                Stage = "codegen-verified",
                ReceiptSymbol = _receiptSymbol,
                InvocationId = _invocation.Id,
                ExecutingHost = typeof(ConditionalRegistrationBackend).Assembly.Location,
                TrimmedSwitchTemplate = ILCompiler.TrimmedSwitchTemplate.Id,
                HostRuntime = RuntimeInformation.FrameworkDescription,
                Scanner = _useScanner,
                TargetPointerSize = factory.Target.PointerSize,
                ObjectFile = _objectFile,
                ObjectHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(objectFile))).ToLowerInvariant(),
                Groups = groups
            }, new JsonSerializerOptions { WriteIndented = true }));
        foreach (var group in groups)
        {
            Console.WriteLine("[AtomUI ILC8] group=" + group.Id + " requested=" + group.Requested + " selectedEntries=" + group.Selected.Length + " selectedMethods=" + group.UniqueMethods);
        }
    }

    public void CompleteCompilation(string exportsFile)
    {
        if (string.IsNullOrEmpty(exportsFile) || !File.Exists(exportsFile) ||
            !File.ReadAllLines(exportsFile).Contains("_" + _receiptSymbol, StringComparer.Ordinal))
        {
            throw new InvalidDataException("Controlled native compilation requires its receipt in the exports list");
        }
        string reportPath = _invocation.File("analysis.json");
        var report = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(reportPath));
        report["Stage"] = "compiler-complete";
        report["ExportsFile"] = Path.GetFullPath(exportsFile);
        report["ExportsHash"] = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(exportsFile)));
        File.WriteAllText(_invocation.File("analysis.json"), report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        string pointer = _invocation.RequireExact(Environment.GetEnvironmentVariable("ATOMUI_ILC8_RECEIPT"), "receipt.json");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(pointer)));
        string temporary = _invocation.RequireOwned(pointer + "." + Guid.NewGuid().ToString("N") + ".tmp");
        File.WriteAllText(temporary, JsonSerializer.Serialize(new
        {
            format = 2, invocationId = _invocation.Id, reportPath = _reportPath,
            reportHash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(_reportPath)))
        }));
        File.Move(temporary, pointer, true);
    }

    public override MethodIL GetMethodIL(MethodDesc method)
    {
        if (method == _trimmedGetter)
        {
            var folded = new ILEmitter();
            ILCodeStream foldedBody = folded.NewCodeStream();
            foldedBody.EmitLdc(1);
            foldedBody.Emit(ILOpcode.ret);
            return folded.Link(method);
        }
        RegistrationDefinitions.Group normalized = _groups.FirstOrDefault(g => g.Entry == method);
        if (normalized != null)
        {
            var normalization = new ILEmitter();
            ILCodeStream stream = normalization.NewCodeStream();
            stream.EmitLdArg(0);
            stream.Emit(ILOpcode.call, normalization.NewToken(normalized.Collector));
            stream.Emit(ILOpcode.ret);
            return normalization.Link(method);
        }
        RegistrationDefinitions.Group group = _groups.FirstOrDefault(g => g.Collector == method);
        if (group == null)
        {
            return _inner.GetMethodIL(method);
        }
        if (!_useScanner)
        {
            return EmitNativeDispatch(group);
        }
        var emitter = new ILEmitter();
        ILCodeStream code = emitter.NewCodeStream();
        if (_frozen)
        {
            foreach (MethodDesc selected in group.FrozenEntries.Select(e => e.Method).Distinct())
            {
                code.EmitLdArg(0);
                code.Emit(ILOpcode.call, emitter.NewToken(selected));
            }
        }
        code.Emit(ILOpcode.ret);
        return emitter.Link(method);
    }

    private static MethodIL EmitNativeDispatch(RegistrationDefinitions.Group group)
    {
        TypeSystemContext context = group.Collector.Context;
        var emitter = new ILEmitter();
        ILCodeStream code = emitter.NewCodeStream();
        var table = emitter.NewLocal(context.GetWellKnownType(WellKnownType.IntPtr));
        var count = emitter.NewLocal(context.GetWellKnownType(WellKnownType.Int32));
        var index = emitter.NewLocal(context.GetWellKnownType(WellKnownType.Int32));
        ILCodeLabel check = emitter.NewCodeLabel();
        ILCodeLabel body = emitter.NewCodeLabel();
        var field = new ExternSymbolMappedField(context.GetWellKnownType(WellKnownType.Byte), group.SymbolName);
        code.Emit(ILOpcode.ldsflda, emitter.NewToken(field));
        code.Emit(ILOpcode.conv_i);
        code.EmitStLoc(table);
        code.EmitLdLoc(table);
        code.EmitLdc(8);
        code.Emit(ILOpcode.add);
        code.Emit(ILOpcode.ldind_i4);
        code.EmitStLoc(count);
        code.EmitLdc(0);
        code.EmitStLoc(index);
        code.Emit(ILOpcode.br, check);
        code.EmitLabel(body);
        code.EmitLdArg(0);
        code.EmitLdLoc(table);
        code.EmitLdc(RegistrationTableNode.HeaderSize);
        code.Emit(ILOpcode.add);
        code.EmitLdLoc(index);
        code.EmitLdc(context.Target.PointerSize);
        code.Emit(ILOpcode.mul);
        code.Emit(ILOpcode.conv_i);
        code.Emit(ILOpcode.add);
        code.Emit(ILOpcode.ldind_i);
        // Static/default is the managed calling convention, including a GC-tracked builder argument.
        var signature = new MethodSignature(MethodSignatureFlags.Static, 0, context.GetWellKnownType(WellKnownType.Void), new[] { group.Builder });
        code.Emit(ILOpcode.calli, emitter.NewToken(signature));
        code.EmitLdLoc(index);
        code.EmitLdc(1);
        code.Emit(ILOpcode.add);
        code.EmitStLoc(index);
        code.EmitLabel(check);
        code.EmitLdLoc(index);
        code.EmitLdLoc(count);
        code.Emit(ILOpcode.blt, body);
        code.Emit(ILOpcode.ret);
        return emitter.Link(group.Collector);
    }

    private sealed class PreinitBarrier : ILProvider
    {
        private readonly ConditionalRegistrationBackend _backend;
        public PreinitBarrier(ConditionalRegistrationBackend backend)
        {
            _backend = backend;
        }
        public override MethodIL GetMethodIL(MethodDesc method)
        {
            // Preserve every cctor path reaching a collector. Other preinitialization remains enabled.
            return _backend._groups.Any(g => g.Collector == method || g.Entry == method) ? null : _backend.GetMethodIL(method);
        }
    }

    private sealed class RequestNode : DependencyNodeCore<NodeFactory>
    {
        private readonly RegistrationDefinitions.Group _group;
        private readonly bool _scanner;
        public RequestNode(RegistrationDefinitions.Group group, bool scanner)
        {
            _group = group;
            _scanner = scanner;
        }
        public override bool InterestingForDynamicDependencyAnalysis => false;
        public override bool HasDynamicDependencies => false;
        public override bool HasConditionalStaticDependencies => true;
        public override bool StaticDependenciesAreComputed => true;
        public override IEnumerable<DependencyListEntry> GetStaticDependencies(NodeFactory context) => Array.Empty<DependencyListEntry>();
        public override IEnumerable<CombinedDependencyListEntry> SearchDynamicDependencies(List<DependencyNodeCore<NodeFactory>> nodes, int first, NodeFactory context) => Array.Empty<CombinedDependencyListEntry>();
        public override IEnumerable<CombinedDependencyListEntry> GetConditionalStaticDependencies(NodeFactory context)
        {
            // Codegen may inline the collector. The actual table-address dependency,
            // rather than a diagnostic method name, is the native request condition.
            DependencyNodeCore<NodeFactory> request = _scanner ? (DependencyNodeCore<NodeFactory>)context.MethodEntrypoint(_group.Collector) : (DependencyNodeCore<NodeFactory>)context.ExternSymbol(_group.SymbolName);
            yield return new CombinedDependencyListEntry(_group.Table, request, "AtomUI group requested");
        }
        protected override string GetName(NodeFactory context) => "AtomUI declaration " + _group.SymbolName;
    }
}
