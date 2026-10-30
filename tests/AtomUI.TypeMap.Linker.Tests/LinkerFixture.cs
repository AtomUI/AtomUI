using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;
using Mono.Linker.Steps;

namespace AtomUI.TypeMap.Linker.Tests;

internal sealed class LinkerFixture : IDisposable, ILogger
{
    public const string Capability = "atomui-typemap-v1-illink-10.0.8";
    public LinkContext Context { get; }
    public List<string> Messages { get; } = [];
    public AssemblyDefinition Core { get; }
    public AssemblyDefinition Bcl { get; }
    public TypeDefinition FragmentBase { get; }
    public TypeDefinition Marker { get; }
    public TypeDefinition AccessorMarker { get; }

    public LinkerFixture()
    {
        Context = new LinkContext(new Pipeline(), this, Path.GetTempPath());
        Context.Pipeline.AppendStep(new SweepStep());
        Context.SetCustomData("AtomUITypeMapBackend", Capability);
        Bcl = AssemblyDefinition.ReadAssembly(typeof(object).Assembly.Location, new ReaderParameters { AssemblyResolver = Context.Resolver });
        Context.Resolver.CacheAssembly(Bcl);
        Context.RegisterAssembly(Bcl);
        Core = Assembly("AtomUI.Core");
        Marker = Type(Core, "AtomUI.Registration", "ControlPackageMarkerAttribute", typeof(Attribute));
        Constructor(Marker, typeof(string), typeof(Type), typeof(int));
        AccessorMarker = Type(Core, "AtomUI.Registration", "GeneratedTypeMapAccessorAttribute", typeof(Attribute));
        Constructor(AccessorMarker, typeof(Type));
        FragmentBase = Type(Core, "AtomUI.Registration", "ControlRegistrationFragmentAttribute", typeof(Attribute));
        Type(Core, "AtomUI.Registration", "ControlPackageRegistrationBuilder", typeof(object));
    }

    public AssemblyDefinition Assembly(string name, int version = 1)
    {
        var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, new Version(version, 0, 0, 0)), name, new ModuleParameters { Kind = ModuleKind.Dll, AssemblyResolver = Context.Resolver });
        Context.Resolver.CacheAssembly(assembly);
        Context.RegisterAssembly(assembly);
        Context.Annotations.SetAction(assembly, AssemblyAction.Link);
        return assembly;
    }

    public Package AddPackage(string name = "Package", int version = 1, bool reachable = true)
    {
        var assembly = Assembly(name, version);
        var group = Type(assembly, "Generated", "Group", typeof(object));
        group.IsSealed = true;
        var package = new CustomAttribute(assembly.MainModule.ImportReference(Marker.Methods[0]));
        package.ConstructorArguments.Add(new(assembly.MainModule.TypeSystem.String, name));
        package.ConstructorArguments.Add(new(assembly.MainModule.ImportReference(typeof(Type)), group));
        package.ConstructorArguments.Add(new(assembly.MainModule.TypeSystem.Int32, 1));
        assembly.CustomAttributes.Add(package);
        var helpers = new TypeDefinition("", "BrowserMap", TypeAttributes.NestedAssembly | TypeAttributes.Abstract | TypeAttributes.Sealed, assembly.MainModule.ImportReference(typeof(object)));
        group.NestedTypes.Add(helpers);
        var create = Method(helpers, "Create", typeof(Dictionary<string, Type>));
        var add = Method(helpers, "Add", typeof(void), typeof(Dictionary<string, Type>), typeof(string), typeof(RuntimeTypeHandle));
        var complete = Method(helpers, "Complete", typeof(IReadOnlyDictionary<string, Type>), typeof(Dictionary<string, Type>));
        complete.NoInlining = true;
        var holder = Type(assembly, "Generated", "Registration", typeof(object));
        var accessor = Method(holder, "GetTypeMap", typeof(IReadOnlyDictionary<string, Type>));
        accessor.IsPublic = false;
        accessor.IsAssembly = true;
        var attr = new CustomAttribute(assembly.MainModule.ImportReference(AccessorMarker.Methods[0]));
        attr.ConstructorArguments.Add(new(assembly.MainModule.ImportReference(typeof(Type)), group));
        accessor.CustomAttributes.Add(attr);
        var typeMapping = Bcl.MainModule.GetType("System.Runtime.InteropServices.TypeMapping");
        var externalMap = typeMapping.Methods.Single(m => m.Name == "GetOrCreateExternalTypeMapping");
        var call = new GenericInstanceMethod(assembly.MainModule.ImportReference(externalMap));
        call.GenericArguments.Add(group);
        accessor.Body.Instructions.Clear();
        accessor.Body.GetILProcessor().Emit(OpCodes.Call, call);
        accessor.Body.GetILProcessor().Emit(OpCodes.Ret);
        foreach (var item in new IMetadataTokenProvider[] { assembly, group, helpers, create, add, complete, holder }) Mark(item);
        if (reachable) Mark(accessor);
        return new(assembly, group, accessor, create, add, complete);
    }

    public TypeDefinition Proxy(Package package, string name, bool marked = true)
    {
        var type = Type(package.Assembly, "Generated", name, typeof(Attribute));
        type.BaseType = package.Assembly.MainModule.ImportReference(FragmentBase);
        type.IsSealed = true;
        var ctor = Constructor(type);
        type.CustomAttributes.Add(new CustomAttribute(ctor));
        if (marked) Mark(type);
        return type;
    }

    public CustomAttribute Entry(Package package, string key, TypeDefinition proxy, bool selected = true, AssemblyDefinition? source = null)
    {
        source ??= package.Assembly;
        var attribute = Bcl.MainModule.GetType("System.Runtime.InteropServices.TypeMapAttribute`1");
        var generic = new GenericInstanceType(source.MainModule.ImportReference(attribute));
        generic.GenericArguments.Add(source.MainModule.ImportReference(package.Group));
        var constructor = new MethodReference(".ctor", source.MainModule.TypeSystem.Void, generic) { HasThis = true };
        foreach (var t in new[] { typeof(string), typeof(Type), typeof(Type) }) constructor.Parameters.Add(new(source.MainModule.ImportReference(t)));
        var declaration = new CustomAttribute(constructor);
        declaration.ConstructorArguments.Add(new(source.MainModule.TypeSystem.String, key));
        declaration.ConstructorArguments.Add(new(source.MainModule.ImportReference(typeof(Type)), source.MainModule.ImportReference(proxy)));
        declaration.ConstructorArguments.Add(new(source.MainModule.ImportReference(typeof(Type)), source.MainModule.ImportReference(proxy)));
        source.CustomAttributes.Add(declaration);
        if (selected) Context.Annotations.Mark(declaration, new DependencyInfo(DependencyKind.Unspecified, source));
        return declaration;
    }

    public AssemblyDefinition Forwarder(string name, TypeDefinition type, AssemblyNameReference destination)
    {
        var assembly = Assembly(name);
        assembly.MainModule.AssemblyReferences.Add(destination);
        assembly.MainModule.ExportedTypes.Add(new ExportedType(type.Namespace, type.Name, assembly.MainModule, destination)
            { IsForwarder = true });
        return assembly;
    }

    public void Mark(IMetadataTokenProvider item) => Context.Annotations.Mark(item, new DependencyInfo(DependencyKind.Unspecified, item), default);
    public void Run() => new MaterializeTypeMapsStep().Process(Context);
    public void Verify() => Context.Pipeline.GetSteps().Single(s => s is not SweepStep).Process(Context);
    public void LogMessage(MessageContainer message) => Messages.Add(message.ToString());
    public void Dispose() => Context.Dispose();

    public static TypeDefinition Type(AssemblyDefinition assembly, string ns, string name, Type baseType)
    {
        var type = new TypeDefinition(ns, name, TypeAttributes.Public | TypeAttributes.Class, assembly.MainModule.ImportReference(baseType));
        assembly.MainModule.Types.Add(type);
        return type;
    }
    public static MethodDefinition Method(TypeDefinition type, string name, Type returns, params Type[] parameters)
    {
        var method = new MethodDefinition(name, MethodAttributes.Static | MethodAttributes.Public, type.Module.ImportReference(returns));
        foreach (var parameter in parameters) method.Parameters.Add(new(type.Module.ImportReference(parameter)));
        type.Methods.Add(method);
        method.Body.GetILProcessor().Emit(OpCodes.Ret);
        return method;
    }
    private static MethodDefinition Constructor(TypeDefinition type, params Type[] parameters)
    {
        var method = Method(type, ".ctor", typeof(void), parameters);
        method.IsStatic = false; method.HasThis = true; method.IsSpecialName = true; method.IsRuntimeSpecialName = true;
        return method;
    }
}

internal sealed record Package(AssemblyDefinition Assembly, TypeDefinition Group, MethodDefinition Accessor,
    MethodDefinition Create, MethodDefinition Add, MethodDefinition Complete);

// Loads a real compiled fixture through the public resolver so receipt input locations are authentic.
// In-memory model-only tests above intentionally do not manufacture a receipt input path.
internal sealed class OutputFixture : IDisposable
{
    internal LinkerFixture Linker { get; } = new();
    internal string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "atomui-typemap-output-" + Guid.NewGuid());
    private readonly string _packageName;
    internal string OutputPath => Path.Combine(DirectoryPath, _packageName + ".dll");
    internal string ReceiptPath => Path.Combine(DirectoryPath, "receipt.json");
    internal AssemblyDefinition Assembly { get; }

    internal OutputFixture(string packageName = "Empty")
    {
        _packageName = packageName;
        Directory.CreateDirectory(DirectoryPath);
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !Directory.Exists(Path.Combine(root.FullName, "src", "AtomUI.Toolchain"))) root = root.Parent;
        if (root is null) throw new InvalidOperationException("Cannot locate the source fixture root.");
        var configuration = typeof(OutputFixture).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyConfigurationAttribute), false)
            .Cast<System.Reflection.AssemblyConfigurationAttribute>().Single().Configuration;
        var source = Path.Combine(root.FullName, "tests/AtomUI.TypeMap.Linker.Tests/Fixtures", packageName, "bin", configuration, "net10.0", packageName + ".dll");
        Linker.Context.Resolver.AddSearchDirectory(Path.GetDirectoryName(typeof(object).Assembly.Location)!);
        Assembly = Linker.Context.Resolver.GetAssembly(source);
        Linker.Context.Resolver.CacheAssembly(Assembly);
        Linker.Context.Annotations.SetAction(Assembly, AssemblyAction.Link);
        Linker.Context.OutputDirectory = DirectoryPath;
        Linker.Context.LinkSymbols = true;
        foreach (var type in RegistrationAbi.AllTypes(Assembly.MainModule.Types))
        {
            Linker.Mark(type);
            foreach (var method in type.Methods) Linker.Mark(method);
        }
        foreach (var attribute in Assembly.CustomAttributes.Where(a => a.AttributeType is GenericInstanceType generic && generic.Name == "TypeMapAttribute`1"))
            Linker.Context.Annotations.Mark(attribute, new DependencyInfo(DependencyKind.Unspecified, Assembly));
        Linker.Context.SetCustomData("AtomUITypeMapReceipt", ReceiptPath);
        Linker.Context.Pipeline.AppendStep(new OutputStep());
    }

    internal void LowerAndWrite()
    {
        Linker.Run();
        if (Linker.Context.ErrorsCount != 0) throw new InvalidOperationException(string.Join("\n", Linker.Messages));
        Linker.Context.Pipeline.GetSteps().OfType<VerifyTypeMapsStep>().Single().Process(Linker.Context);
        if (Linker.Context.ErrorsCount != 0) throw new InvalidOperationException(string.Join("\n", Linker.Messages));
        Assembly.Write(OutputPath, new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new PortablePdbWriterProvider() });
    }

    internal void MutateOutput(Action<AssemblyDefinition, MethodDefinition> mutate)
    {
        using var pe = new MemoryStream(File.ReadAllBytes(OutputPath));
        using var pdb = new MemoryStream(File.ReadAllBytes(Path.ChangeExtension(OutputPath, ".pdb")));
        using var emitted = AssemblyDefinition.ReadAssembly(pe, new ReaderParameters
        {
            ReadSymbols = true, SymbolStream = pdb, SymbolReaderProvider = new PortablePdbReaderProvider(),
            AssemblyResolver = Linker.Context.Resolver
        });
        var accessor = emitted.MainModule.GetType(_packageName + ".Entry").Methods.Single(m => m.Name == "GetMap");
        emitted.CustomAttributes.Clear(); // Sweep may already have removed discovery attributes.
        mutate(emitted, accessor);
        emitted.Write(OutputPath, new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new PortablePdbWriterProvider() });
    }

    internal void VerifyOutput() => Linker.Context.Pipeline.GetSteps().OfType<VerificationReceipt>().Single().Process(Linker.Context);

    public void Dispose()
    {
        Linker.Dispose();
        Directory.Delete(DirectoryPath, recursive: true);
    }
}
