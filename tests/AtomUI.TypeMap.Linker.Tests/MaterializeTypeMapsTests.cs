using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker;
using Xunit;

namespace AtomUI.TypeMap.Linker.Tests;

public class MaterializeTypeMapsTests
{
    [Fact]
    public void LowersOnlyMarkedEntriesDeterministicallyAcrossGroupsAndAliases()
    {
        using var fixture = new LinkerFixture();
        var first = fixture.AddPackage("First");
        var second = fixture.AddPackage("Second");
        var used = fixture.Proxy(first, "Used");
        fixture.Entry(first, "z", used);
        fixture.Entry(first, "a", used);
        fixture.Entry(first, "unused", fixture.Proxy(first, "Unused"), false);
        fixture.Entry(second, "dependency", fixture.Proxy(second, "Dependency"));
        fixture.Run();
        Assert.True(fixture.Context.ErrorsCount == 0, string.Join("\n", fixture.Messages));
        Assert.Equal(new[] { "a", "z" }, Keys(first));
        Assert.Equal(new[] { "dependency" }, Keys(second));
        Assert.Same(first.Complete, first.Accessor.Body.Instructions[^2].Operand);
        Assert.DoesNotContain(first.Accessor.Body.Instructions, i => i.Operand is MethodReference m && m.DeclaringType.Name == "TypeMapping");
        fixture.Verify();
        Assert.True(fixture.Context.ErrorsCount == 0, string.Join("\n", fixture.Messages));
    }

    [Fact]
    public void EmptyMapIsCompletedAndZeroReachableAccessorsIsValid()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        fixture.Run();
        Assert.Equal(new[] { OpCodes.Call, OpCodes.Call, OpCodes.Ret }, package.Accessor.Body.Instructions.Select(i => i.OpCode));
        fixture.Verify();
        using var empty = new LinkerFixture();
        empty.Run();
        empty.Verify();
        Assert.Equal(0, empty.Context.ErrorsCount);
    }

    [Fact]
    public void ClearsOldBodyAndAllDebugScopesBeforeWritingPortablePdb()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        var method = package.Accessor;
        method.Body.Variables.Add(new VariableDefinition(method.Module.TypeSystem.Int32));
        method.DebugInformation.SequencePoints.Add(new SequencePoint(method.Body.Instructions[0], new Document("old.cs")) { StartLine = 1, EndLine = 1 });
        method.DebugInformation.Scope = new ScopeDebugInformation(method.Body.Instructions[0], method.Body.Instructions[^1]);
        method.DebugInformation.CustomDebugInformations.Add(new StateMachineScopeDebugInformation());
        method.CustomDebugInformations.Add(new StateMachineScopeDebugInformation());
        fixture.Run();
        Assert.Empty(method.Body.Variables);
        Assert.Empty(method.Body.ExceptionHandlers);
        Assert.Empty(method.DebugInformation.SequencePoints);
        Assert.Null(method.DebugInformation.Scope);
        Assert.Empty(method.DebugInformation.CustomDebugInformations);
        Assert.Empty(method.CustomDebugInformations);
        using var image = new MemoryStream();
        using var pdb = new MemoryStream();
        package.Assembly.Write(image, new WriterParameters { WriteSymbols = true, SymbolWriterProvider = new PortablePdbWriterProvider(), SymbolStream = pdb });
        Assert.True(pdb.Length > 0);
    }

    [Theory]
    [InlineData("abi")]
    [InlineData("foreign-group")]
    [InlineData("wrong-group-call")]
    [InlineData("malformed-body")]
    [InlineData("helper-signature")]
    [InlineData("helper-unmarked")]
    [InlineData("target-unmarked")]
    [InlineData("foreign-proxy")]
    [InlineData("duplicate-key")]
    [InlineData("empty-key")]
    [InlineData("copy")]
    [InlineData("skip")]
    [InlineData("stale-capability")]
    public void InvalidContractsFailClosed(string mutation)
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        var proxy = fixture.Proxy(package, "Used");
        fixture.Entry(package, "used", proxy);
        switch (mutation)
        {
            case "abi": package.Assembly.CustomAttributes[0].ConstructorArguments[2] = new(package.Assembly.MainModule.TypeSystem.Int32, 2); break;
            case "foreign-group": package.Assembly.CustomAttributes[0].ConstructorArguments[1] = new(package.Assembly.MainModule.ImportReference(typeof(Type)), fixture.AddPackage("Other").Group); break;
            case "wrong-group-call": ((GenericInstanceMethod)package.Accessor.Body.Instructions[0].Operand).GenericArguments[0] = fixture.AddPackage("Other").Group; break;
            case "malformed-body": package.Accessor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldnull)); break;
            case "helper-signature": package.Add.Parameters.RemoveAt(2); break;
            case "helper-unmarked": package.Group.NestedTypes[0].Methods.Remove(package.Add); LinkerFixture.Method(package.Group.NestedTypes[0], "Add", typeof(void), typeof(Dictionary<string, Type>), typeof(string), typeof(RuntimeTypeHandle)); break;
            case "target-unmarked": var unmarked = fixture.Proxy(package, "Unmarked", marked: false); fixture.Entry(package, "bad", unmarked); break;
            case "foreign-proxy": fixture.Entry(package, "bad", fixture.Proxy(fixture.AddPackage("Other"), "Foreign")); break;
            case "duplicate-key": fixture.Entry(package, "used", proxy); break;
            case "empty-key": fixture.Entry(package, "", proxy); break;
            case "copy": fixture.Context.Annotations.SetAction(package.Assembly, AssemblyAction.Copy); break;
            case "skip": fixture.Context.Annotations.SetAction(package.Assembly, AssemblyAction.Skip); break;
            case "stale-capability": fixture.Context.SetCustomData("AtomUITypeMapBackend", "old"); break;
        }
        fixture.Run();
        Assert.True(fixture.Context.ErrorsCount > 0, string.Join("\n", fixture.Messages));
        var expectedCode = mutation switch { "duplicate-key" => "ATOMUIREG005", "malformed-body" or "wrong-group-call" => "ATOMUIREG007", _ => "ATOMUIREG006" };
        Assert.Contains(fixture.Messages, m => m.Contains(expectedCode));
        Assert.Contains(package.Accessor.Body.Instructions, i => i.Operand is GenericInstanceMethod);
    }

    [Fact]
    public void CompletionUsesRetainedMethodsEvenWhenAttributesHaveBeenSwept()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        fixture.Run();
        package.Assembly.CustomAttributes.Clear();
        package.Accessor.CustomAttributes.Clear();
        package.Accessor.Body.Instructions[^2].Operand = package.Create;
        fixture.Verify();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG007"));
    }

    [Fact]
    public void DuplicateInjectionIsRejectedBeforeAnyRewriting()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        fixture.Context.Pipeline.PrependStep(new MaterializeTypeMapsStep());
        fixture.Context.Pipeline.PrependStep(new MaterializeTypeMapsStep());
        fixture.Run();
        Assert.True(fixture.Context.ErrorsCount > 0);
        Assert.Contains(package.Accessor.Body.Instructions, i => i.Operand is GenericInstanceMethod);
    }

    [Fact]
    public void SameShortScopeWithWrongVersionIsRejected()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        var reference = new TypeReference(package.Group.Namespace, package.Group.Name, package.Assembly.MainModule,
            new AssemblyNameReference(package.Assembly.Name.Name, new Version(99, 0, 0, 0)));
        package.Accessor.CustomAttributes[0].ConstructorArguments[0] = new(package.Assembly.MainModule.ImportReference(typeof(Type)), reference);
        fixture.Run();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG005"));
    }

    [Fact]
    public void IncorrectPhaseAndMissingCapabilityFailWithBackendDiagnostic()
    {
        using var fixture = new LinkerFixture();
        fixture.AddPackage();
        fixture.Context.Pipeline.PrependStep(new Mono.Linker.Steps.MarkStep());
        fixture.Run();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG006"));
        using var missing = new LinkerFixture();
        missing.Context.SetCustomData("AtomUITypeMapBackend", "");
        missing.Run();
        Assert.Contains(missing.Messages, m => m.Contains("ATOMUIREG006"));
    }

    [Fact]
    public void SuccessReceiptProvesBackendExecutedEvenWithZeroSlots()
    {
        using var fixture = new LinkerFixture();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        fixture.Context.SetCustomData("AtomUITypeMapReceipt", path);
        fixture.Context.Pipeline.AppendStep(new Mono.Linker.Steps.OutputStep());
        try
        {
            fixture.Run();
            foreach (var step in fixture.Context.Pipeline.GetSteps().Where(s => s is not Mono.Linker.Steps.SweepStep && s is not Mono.Linker.Steps.OutputStep)) step.Process(fixture.Context);
            Assert.True(File.Exists(path), string.Join("\n", fixture.Messages));
            using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal("output-verified", json.RootElement.GetProperty("status").GetString());
            Assert.Equal(0, json.RootElement.GetProperty("accessors").GetArrayLength());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void FailedRunCannotLeavePreviousSuccessReceipt()
    {
        using var fixture = new LinkerFixture();
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(path, "stale success");
        fixture.Context.SetCustomData("AtomUITypeMapReceipt", path);
        fixture.Context.SetCustomData("AtomUITypeMapBackend", "old");
        try { fixture.Run(); Assert.False(File.Exists(path)); }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("illink, Version=11.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [InlineData("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=null")]
    public void LinkerIdentityMustMatchEvenWhenInformationalVersionMatches(string identity)
    {
        Assert.Throws<BackendDiagnostic>(() => ToolchainContract.ValidateIdentity(identity, ToolchainContract.ActualLinkerVersion));
    }

    [Fact]
    public void OfficialTypeMapLinkerVersionsAreAcceptedFromNet10()
    {
        ToolchainContract.ValidateIdentity("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.0");
        ToolchainContract.ValidateIdentity("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.9-servicing.26270.113+901ca941");
        ToolchainContract.ValidateIdentity("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "10.0.13");
        ToolchainContract.ValidateIdentity("illink, Version=11.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "11.0.0");
        ToolchainContract.ValidateIdentity("illink, Version=12.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "12.0.0");
        Assert.Throws<BackendDiagnostic>(() => ToolchainContract.ValidateIdentity("illink, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "11.0.0"));
    }

    [Fact]
    public void UnreachableAccessorIsNotLowered()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage(reachable: false);
        fixture.Run(); fixture.Verify();
        Assert.Equal(0, fixture.Context.ErrorsCount);
        Assert.Contains(package.Accessor.Body.Instructions, i => i.Operand is GenericInstanceMethod);
    }

    [Theory]
    [InlineData(AssemblyAction.CopyUsed)]
    [InlineData(AssemblyAction.Save)]
    [InlineData(AssemblyAction.Delete)]
    public void AllNonLinkActionsAreRejected(AssemblyAction action)
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        fixture.Context.Annotations.SetAction(package.Assembly, action);
        fixture.Run();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG006") && m.Contains(action.ToString()));
    }

    [Theory]
    [InlineData("accessor")]
    [InlineData("helper")]
    [InlineData("target")]
    public void SweepCannotRemoveAnyPartOfTheLoweredContract(string removed)
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        var target = fixture.Proxy(package, "Used");
        fixture.Entry(package, "used", target);
        fixture.Run();
        package.Accessor.CustomAttributes.Clear();
        package.Assembly.CustomAttributes.Clear();
        if (removed == "accessor") package.Accessor.DeclaringType.Methods.Remove(package.Accessor);
        else if (removed == "helper") package.Complete.DeclaringType.Methods.Remove(package.Complete);
        else package.Assembly.MainModule.Types.Remove(target);
        fixture.Verify();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG007"));
    }

    [Fact]
    public void AttributeRemovalDoesNotPreventSuccessfulVerification()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        fixture.Run();
        package.Accessor.CustomAttributes.Clear(); package.Assembly.CustomAttributes.Clear();
        fixture.Verify();
        Assert.Equal(0, fixture.Context.ErrorsCount);
    }

    [Fact]
    public void DuplicatePackageIdsAreNotSilentlyCombined()
    {
        using var fixture = new LinkerFixture();
        var first = fixture.AddPackage("First");
        var second = fixture.AddPackage("Second");
        second.Assembly.CustomAttributes[0].ConstructorArguments[0] = first.Assembly.CustomAttributes[0].ConstructorArguments[0];
        fixture.Run();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG005"));
    }

    [Theory]
    [InlineData("instance-call")]
    [InlineData("generic-container")]
    public void MalformedCallingContractsAreRejected(string malformed)
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        if (malformed == "instance-call") ((GenericInstanceMethod)package.Accessor.Body.Instructions[0].Operand).ElementMethod.HasThis = true;
        else package.Accessor.DeclaringType.GenericParameters.Add(new GenericParameter("T", package.Accessor.DeclaringType));
        fixture.Run();
        Assert.True(fixture.Context.ErrorsCount > 0);
    }

    [Fact]
    public void MissingBodyAfterSweepProducesDiagnostic()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        fixture.Run();
        package.Accessor.IsAbstract = true;
        package.Accessor.Body = null;
        fixture.Verify();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG007"));
    }

    [Fact]
    public void SkipActionCannotHideReachableButUnmarkedAccessors()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage(reachable: false);
        fixture.Context.Annotations.SetAction(package.Assembly, AssemblyAction.Skip);
        fixture.Run();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG006") && m.Contains("Skip"));
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("version")]
    [InlineData("culture")]
    [InlineData("key")]
    public void EveryForwardedDestinationMustHaveTheExactIdentity(string mutation)
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        var destination = AssemblyNameReference.Parse(package.Assembly.Name.FullName);
        if (mutation == "version") destination.Version = new Version(99, 0, 0, 0);
        if (mutation == "culture") destination.Culture = "fr-FR";
        if (mutation == "key") destination.PublicKeyToken = [1, 2, 3, 4, 5, 6, 7, 8];
        var inner = fixture.Forwarder("InnerFacade", package.Group, destination);
        var outer = fixture.Forwarder("OuterFacade", package.Group, AssemblyNameReference.Parse(inner.Name.FullName));
        var reference = new TypeReference(package.Group.Namespace, package.Group.Name, package.Assembly.MainModule,
            AssemblyNameReference.Parse(outer.Name.FullName));
        package.Accessor.CustomAttributes[0].ConstructorArguments[0] = new(package.Assembly.MainModule.ImportReference(typeof(Type)), reference);
        fixture.Run();
        if (mutation == "valid")
        {
            Assert.True(fixture.Context.ErrorsCount == 0, string.Join("\n", fixture.Messages));
            Assert.Same(package.Complete, package.Accessor.Body.Instructions[^2].Operand);
        }
        else
        {
            Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG005"));
            Assert.Contains(package.Accessor.Body.Instructions, i => i.Operand is GenericInstanceMethod);
        }
    }

    [Fact]
    public void ForwardingCyclesFailClosedWithoutRecursiveResolution()
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        var first = fixture.Forwarder("FirstFacade", package.Group, new AssemblyNameReference("SecondFacade", new Version(1, 0, 0, 0)));
        fixture.Forwarder("SecondFacade", package.Group, AssemblyNameReference.Parse(first.Name.FullName));
        var reference = new TypeReference(package.Group.Namespace, package.Group.Name, package.Assembly.MainModule,
            AssemblyNameReference.Parse(first.Name.FullName));
        package.Accessor.CustomAttributes[0].ConstructorArguments[0] = new(package.Assembly.MainModule.ImportReference(typeof(Type)), reference);
        fixture.Run();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG005") && m.Contains("cycle"));
    }

    public static IEnumerable<object[]> InvalidStaticContracts =>
        from member in new[] { "accessor", "Create", "Add", "Complete" }
        from invalid in new[] { "has-this", "explicit-this", "vararg", "unmanaged-c", "generic-convention", "unmanaged-implementation" }
        select new object[] { member, invalid };

    [Theory]
    [MemberData(nameof(InvalidStaticContracts))]
    public void StaticAccessorAndHelpersRequireTheManagedCallingContract(string member, string invalid)
    {
        using var fixture = new LinkerFixture();
        var package = fixture.AddPackage();
        var method = member switch { "accessor" => package.Accessor, "Create" => package.Create, "Add" => package.Add, _ => package.Complete };
        switch (invalid)
        {
            case "has-this": method.HasThis = true; break;
            case "explicit-this": method.ExplicitThis = true; break;
            case "vararg": method.CallingConvention = MethodCallingConvention.VarArg; break;
            case "unmanaged-c": method.CallingConvention = MethodCallingConvention.C; break;
            case "generic-convention": method.CallingConvention = MethodCallingConvention.Generic; break;
            case "unmanaged-implementation": method.IsUnmanaged = true; break;
        }
        fixture.Run();
        Assert.Contains(fixture.Messages, m => m.Contains("ATOMUIREG006"));
        Assert.Contains(package.Accessor.Body.Instructions, i => i.Operand is GenericInstanceMethod);
    }

    [Fact]
    public void ValidEmittedAssemblyAndSymbolsReceiveAReceipt()
    {
        using var fixture = new OutputFixture();
        fixture.LowerAndWrite();
        fixture.VerifyOutput();
        Assert.True(fixture.Linker.Context.ErrorsCount == 0, string.Join("\n", fixture.Linker.Messages));
        Assert.True(File.Exists(fixture.ReceiptPath));
    }

    [Theory]
    [InlineData("exception-region")]
    [InlineData("locals")]
    [InlineData("init-locals")]
    [InlineData("accessor-flags")]
    [InlineData("duplicate-accessor")]
    [InlineData("call-signature")]
    [InlineData("helper-flags")]
    [InlineData("helper-missing")]
    [InlineData("helper-owner")]
    [InlineData("sequence-point")]
    [InlineData("scope")]
    [InlineData("pdb-missing")]
    [InlineData("pdb-corrupt")]
    public void CorruptEmittedContractCannotReceiveASuccessReceipt(string corruption)
    {
        using var fixture = new OutputFixture();
        fixture.LowerAndWrite();
        fixture.MutateOutput((assembly, method) =>
        {
            var create = assembly.MainModule.GetType("Empty.Group").NestedTypes.Single().Methods.Single(m => m.Name == "Create");
            switch (corruption)
            {
                case "exception-region":
                    method.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
                    {
                        TryStart = method.Body.Instructions[0], TryEnd = method.Body.Instructions[1],
                        HandlerStart = method.Body.Instructions[1], HandlerEnd = method.Body.Instructions[^1],
                        CatchType = assembly.MainModule.ImportReference(typeof(Exception))
                    });
                    break;
                case "locals": method.Body.Variables.Add(new VariableDefinition(assembly.MainModule.TypeSystem.Int32)); break;
                case "init-locals": method.Body.InitLocals = true; break;
                case "accessor-flags": method.HasThis = true; break;
                case "duplicate-accessor":
                    var duplicate = new MethodDefinition(method.Name, method.Attributes, method.ReturnType);
                    duplicate.Body.GetILProcessor().Emit(OpCodes.Call, create);
                    duplicate.Body.GetILProcessor().Emit(OpCodes.Call, create.DeclaringType.Methods.Single(m => m.Name == "Complete"));
                    duplicate.Body.GetILProcessor().Emit(OpCodes.Ret);
                    method.DeclaringType.Methods.Add(duplicate);
                    break;
                case "call-signature":
                    method.Body.Instructions[0].Operand = new MethodReference(create.Name, create.ReturnType, create.DeclaringType) { HasThis = true, ExplicitThis = true };
                    break;
                case "helper-flags": create.HasThis = true; create.ExplicitThis = true; break;
                case "helper-missing":
                    method.Body.Instructions[0].Operand = new MethodReference(create.Name, create.ReturnType, create.DeclaringType);
                    create.DeclaringType.Methods.Remove(create);
                    break;
                case "helper-owner": create.DeclaringType.IsNestedPublic = true; break;
                case "sequence-point": method.DebugInformation.SequencePoints.Add(new SequencePoint(method.Body.Instructions[0], new Document("stale.cs")) { StartLine = 1, EndLine = 1 }); break;
                case "scope": method.DebugInformation.Scope = new ScopeDebugInformation(method.Body.Instructions[0], method.Body.Instructions[^1]); break;
            }
        });
        if (corruption == "pdb-missing") File.Delete(Path.ChangeExtension(fixture.OutputPath, ".pdb"));
        if (corruption == "pdb-corrupt") File.WriteAllText(Path.ChangeExtension(fixture.OutputPath, ".pdb"), "invalid symbols");
        if (corruption == "exception-region")
        {
            var loader = new System.Runtime.Loader.AssemblyLoadContext("invalid-eh-" + Guid.NewGuid(), isCollectible: true);
            try
            {
                var emitted = loader.LoadFromAssemblyPath(fixture.OutputPath);
                var exception = Assert.Throws<System.Reflection.TargetInvocationException>(() => emitted.GetType("Empty.Entry")!.GetMethod("Query")!.Invoke(null, null));
                Assert.IsType<InvalidProgramException>(exception.InnerException);
            }
            finally { loader.Unload(); }
        }
        fixture.VerifyOutput();
        Assert.Contains(fixture.Linker.Messages, m => m.Contains("ATOMUIREG007"));
        Assert.False(File.Exists(fixture.ReceiptPath));
    }

    [Theory]
    [InlineData("none")]
    [InlineData("target-missing")]
    [InlineData("base-identity")]
    [InlineData("self-attribute")]
    [InlineData("constructor")]
    [InlineData("add-override")]
    [InlineData("fragment-id")]
    public void EmittedProxyMetadataMustPreserveTheSelectedActivationContract(string corruption)
    {
        using var fixture = new OutputFixture("PackageA");
        fixture.LowerAndWrite();
        fixture.MutateOutput((assembly, _) =>
        {
            var target = assembly.MainModule.GetType("PackageA.UsedProxy");
            if (corruption == "target-missing")
            {
                var scope = AssemblyNameReference.Parse(assembly.Name.FullName);
                assembly.MainModule.AssemblyReferences.Add(scope);
                var missing = new TypeReference(target.Namespace, target.Name, assembly.MainModule, scope);
                foreach (var type in RegistrationAbi.AllTypes(assembly.MainModule.Types))
                foreach (var method in type.Methods.Where(m => m.HasBody))
                foreach (var instruction in method.Body.Instructions)
                    if (ReferenceEquals(instruction.Operand, target)) instruction.Operand = missing;
                assembly.MainModule.Types.Remove(target);
            }
            if (corruption == "base-identity") target.BaseType = assembly.MainModule.ImportReference(typeof(Attribute));
            if (corruption == "self-attribute") target.CustomAttributes.Clear();
            if (corruption == "add-override") target.Methods.Remove(target.Methods.Single(m => m.Name == "Add"));
            if (corruption == "fragment-id")
            {
                var property = target.Properties.Single(p => p.Name == "FragmentId");
                target.Methods.Remove(property.GetMethod);
                target.Properties.Remove(property);
            }
            if (corruption == "constructor")
            {
                target.CustomAttributes.Clear();
                target.CustomAttributes.Add(new CustomAttribute(new MethodReference(".ctor", assembly.MainModule.TypeSystem.Void, target) { HasThis = true }));
                target.Methods.Remove(target.Methods.Single(m => m.IsConstructor));
            }
        });
        fixture.VerifyOutput();
        if (corruption == "none")
        {
            Assert.True(fixture.Linker.Context.ErrorsCount == 0, string.Join("\n", fixture.Linker.Messages));
            Assert.True(File.Exists(fixture.ReceiptPath));
        }
        else
        {
            Assert.Contains(fixture.Linker.Messages, m => m.Contains("ATOMUIREG007"));
            Assert.False(File.Exists(fixture.ReceiptPath));
        }
    }

    private static string[] Keys(Package package) => package.Accessor.Body.Instructions.Where(i => i.OpCode == OpCodes.Ldstr).Select(i => (string)i.Operand).ToArray();
}
