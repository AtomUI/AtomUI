using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace AtomUI.Build.Tasks.Tests;

public class TypeMapReceiptIntegrityTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    [InlineData(4 * 1024 * 1024)]
    public void File_hash_matches_SHA256_oracle(int size)
    {
        var path = Path.Combine(Path.GetTempPath(), "atomui-hash-" + Guid.NewGuid().ToString("N"));
        var bytes = new byte[size];
        new Random(42).NextBytes(bytes);
        File.WriteAllBytes(path, bytes);
        try
        {
            TypeMapBuildContract.Hash(path).ShouldBe(Convert.ToHexString(SHA256.HashData(bytes)));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Valid_zero_slot_receipt_is_bound_and_reusable()
    {
        using var fixture = new Fixture(false);
        fixture.Verify(true).ShouldBeTrue();
        fixture.Verify(false).ShouldBeTrue();
    }

    [Theory]
    [InlineData("input")]
    [InlineData("output")]
    [InlineData("symbols")]
    [InlineData("tool")]
    [InlineData("binding")]
    public void Changed_evidence_is_rejected_before_consumption(string change)
    {
        using var fixture = new Fixture(true);
        fixture.Verify(true).ShouldBeTrue();
        var path = change switch { "input" => fixture.Input, "output" => fixture.Output, "symbols" => fixture.Symbols,
            "tool" => fixture.Tool, _ => fixture.Receipt + ".binding" };
        File.AppendAllText(path, "changed");
        fixture.Verify(false).ShouldBeFalse();
        fixture.Engine.Errors.ShouldContain(e => e.Code == "ATOMUIREG007");
    }

    [Fact]
    public void Consumption_gate_rejects_an_empty_actual_input_set()
    {
        using var fixture = new Fixture(true);
        fixture.Verify(true).ShouldBeTrue();
        var task = fixture.Task(false);
        task.RequireConsumedAssemblies = true;
        task.Execute().ShouldBeFalse();
        fixture.Engine.Errors.ShouldContain(e => e.Code == "ATOMUIREG007");
    }

    [Fact]
    public void Replaced_actual_aot_input_is_rejected_even_when_linked_directory_is_valid()
    {
        using var fixture = new Fixture(true);
        fixture.Verify(true).ShouldBeTrue();
        var task = fixture.Task(false);
        var consumed = Path.Combine(fixture.Directory, "consumed", Path.GetFileName(fixture.Output));
        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(consumed)!);
        File.Copy(fixture.Output, consumed); File.AppendAllText(consumed, "changed");
        task.ConsumedAssemblies = [new TestTaskItem(consumed)];
        task.Execute().ShouldBeFalse();
        fixture.Engine.Errors.ShouldContain(e => e.Code == "ATOMUIREG007");
    }

    private sealed class Fixture : IDisposable
    {
        internal readonly string Directory = Path.Combine(Path.GetTempPath(), "atomui-receipt-" + Guid.NewGuid().ToString("N"));
        internal readonly RecordingBuildEngine Engine = new();
        internal string Input => Path.Combine(Directory, "input.dll");
        internal string Output => Path.Combine(Directory, "linked", "input.dll");
        internal string Symbols => Path.ChangeExtension(Output, ".pdb");
        internal string Tool => Path.Combine(Directory, "tool.dll");
        internal string Receipt => Path.Combine(Directory, "receipt.json");
        private string Signature => Path.Combine(Directory, "inputs.json");
        internal Fixture(bool withAssembly)
        {
            System.IO.Directory.CreateDirectory(Path.Combine(Directory, "linked"));
            var assembly = typeof(TypeMapReceiptIntegrityTests).Assembly.Location;
            File.Copy(assembly, Input); File.Copy(assembly, Output); File.Copy(assembly, Tool);
            File.WriteAllText(Symbols, "verified symbol bytes");
            var identity = AssemblyName.GetAssemblyName(Input).FullName!;
            var hash = TypeMapBuildContract.Hash(Input);
            var signature = new TypeMapLinkSignature(TypeMapBuildContract.Capability, Tool, identity, hash,
                Tool, "illink, Version=11.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", "11.0.0", Receipt, Path.GetDirectoryName(Output)!, "fixture",
                [new TypeMapFile(Tool, hash)], withAssembly ? [new TypeMapLinkInput(Input, identity, hash, "link:true")] : []);
            File.WriteAllText(Signature, JsonSerializer.Serialize(signature));
            File.WriteAllText(Receipt, JsonSerializer.Serialize(new
            {
                status = "output-verified", abi = 1, capability = TypeMapBuildContract.Capability, toolIdentity = identity, toolSha256 = hash,
                linkerIdentity = "illink, Version=11.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", linkerVersion = "11.0.0",
                assemblies = withAssembly ? new[] { new { identity, inputSha256 = hash, outputSha256 = hash, outputSymbolsSha256 = TypeMapBuildContract.Hash(Symbols) } } : [],
                accessors = withAssembly ? new[] { new { packageId = "fixture", assembly = identity, group = "fixture.Group", method = "GetMap", bodySha256 = new string('A', 64) } } : []
            }));
        }
        internal VerifyTypeMapReceiptTask Task(bool bind) => new() { BuildEngine = Engine, ReceiptPath = Receipt, SignaturePath = Signature, BindReceipt = bind };
        internal bool Verify(bool bind) => Task(bind).Execute();
        public void Dispose() => System.IO.Directory.Delete(Directory, true);
    }

}
