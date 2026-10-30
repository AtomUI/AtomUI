using Mono.Cecil;
using Xunit;

namespace AtomUI.TypeMap.Linker.Tests;

public class RegistrationMetadataIndexTests
{
    [Fact]
    public void Types_And_Methods_Are_Flattened_Once_Per_Assembly_Instance()
    {
        using var assembly = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition("IndexFixture", new Version(1, 0)),
            "IndexFixture",
            ModuleKind.Dll);
        var outer = new TypeDefinition("Demo", "Outer", Mono.Cecil.TypeAttributes.Public);
        var nested = new TypeDefinition("", "Nested", Mono.Cecil.TypeAttributes.NestedPublic);
        var method = new MethodDefinition(
            "Execute",
            Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.Static,
            assembly.MainModule.TypeSystem.Void);
        nested.Methods.Add(method);
        outer.NestedTypes.Add(nested);
        assembly.MainModule.Types.Add(outer);
        var index = new RegistrationMetadataIndex();

        var firstTypes = index.Types(assembly);
        var secondTypes = index.Types(assembly);
        var firstMethods = index.Methods(assembly);
        var secondMethods = index.Methods(assembly);

        Assert.Same(firstTypes, secondTypes);
        Assert.Same(firstMethods, secondMethods);
        Assert.Contains(outer, firstTypes);
        Assert.Contains(nested, firstTypes);
        Assert.Contains(method, firstMethods);
    }
}
