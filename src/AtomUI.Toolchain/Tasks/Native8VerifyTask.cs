using static AtomUI.Build.Tasks.RegistrationFiles;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using AtomUI.Build.Tasks.Registration;
using Microsoft.Build.Framework;

namespace AtomUI.Build.Tasks;

/// <summary>Separates native-link validation from exact consumption of its published copy.</summary>
public sealed class VerifyNative8RegistrationTask : RegistrationBuildTask
{
    [Required] public string InvocationId { get; set; } = string.Empty;
    [Required] public string OwnedRoot { get; set; } = string.Empty;
    [Required] public string Phase { get; set; } = string.Empty;
    public string ExpectedLinkedReceiptHash { get; set; } = string.Empty;
    [Required] public string ReceiptPointer { get; set; } = string.Empty;
    [Required] public string NativeObject { get; set; } = string.Empty;
    [Required] public string ExportsFile { get; set; } = string.Empty;
    [Required] public string NativeBinary { get; set; } = string.Empty;
    [Output] public string VerifiedReceipt { get; set; } = string.Empty;
    [Output] public string LinkedReceiptHash { get; set; } = string.Empty;

    public override bool Execute()
    {
        Native8Invocation? invocation = null;
        var ownedCleanup = new List<string>();
        try
        {
            if (Phase is not ("linked" or "published"))
            {
                throw new InvalidDataException("Native verification phase must be linked or published.");
            }
            invocation = new Native8Invocation(InvocationId, OwnedRoot);
            string pointerPath = invocation.RequireExact(ReceiptPointer, "receipt.json");
            string reportPath = invocation.File("analysis.json");
            string inputsPath = invocation.File("inputs.json");
            string linkedPath = invocation.File("linked.json");
            NativeObject = invocation.RequireOwned(NativeObject);
            ExportsFile = invocation.RequireOwned(ExportsFile);
            NativeBinary = Phase == "linked" ? invocation.RequireOwned(NativeBinary) : Native8Invocation.RequireUnlinkedAbsolute(NativeBinary);
            // The linked proof is immutable. A failed copy validation never deletes it or its source outputs.
            if (Phase == "published")
            {
                VerifiedReceipt = invocation.File("verified.json");
                ownedCleanup.Add(VerifiedReceipt);
                invocation.DeleteOwned(ownedCleanup);
            }
            else if (!System.IO.File.Exists(linkedPath))
            {
                ownedCleanup.AddRange(new[] { NativeObject, ExportsFile, NativeBinary, pointerPath, reportPath });
            }
            using var pointer = JsonDocument.Parse(System.IO.File.ReadAllBytes(pointerPath));
            RegistrationContractJson.ValidateShape(pointer.RootElement);
            RegistrationContractJson.Fields(pointer.RootElement, "format", "invocationId", "reportPath", "reportHash");
            invocation.RequireExact(pointer.RootElement.GetProperty("reportPath").GetString()!, "analysis.json");
            if (pointer.RootElement.GetProperty("format").GetInt32() != 2 ||
                pointer.RootElement.GetProperty("invocationId").GetString() != InvocationId ||
                Hash(reportPath) != pointer.RootElement.GetProperty("reportHash").GetString())
            {
                throw new InvalidDataException("Stale or foreign native compilation receipt.");
            }
            using var report = JsonDocument.Parse(System.IO.File.ReadAllBytes(reportPath));
            var result = report.RootElement;
            if (result.GetProperty("InvocationId").GetString() != InvocationId ||
                result.GetProperty("Backend").GetString() != "AtomUI.Registration.ILC8" ||
                result.GetProperty("Stage").GetString() != "compiler-complete" ||
                result.GetProperty("InputKind").GetString() != "conditional-record-v1" ||
                result.GetProperty("TrimmedSwitchTemplate").GetString() != "atomui.trimmed-switch.v1")
            {
                throw new InvalidDataException("Native compilation did not complete this invocation's registration contract.");
            }
            invocation.RequireExact(result.GetProperty("InputPath").GetString()!, "inputs.json");
            if (Hash(inputsPath) != result.GetProperty("InputHash").GetString())
            {
                throw new InvalidDataException("Native compilation input snapshot changed.");
            }
            using var inputs = JsonDocument.Parse(System.IO.File.ReadAllBytes(inputsPath));
            var input = inputs.RootElement;
            RegistrationContractJson.ValidateShape(input);
            RegistrationContractJson.Fields(input, "format", "coreAssemblyIdentity", "assemblies", "additionalInputs", "analysisReportPath");
            invocation.RequireExact(input.GetProperty("analysisReportPath").GetString()!, "analysis.json");
            if (input.GetProperty("format").GetInt32() != 2)
            {
                throw new InvalidDataException("Expected native input transport v2.");
            }
            var extra = input.GetProperty("additionalInputs").EnumerateArray().ToArray();
            foreach (var file in input.GetProperty("assemblies").EnumerateArray().Concat(extra))
            {
                if (Hash(file.GetProperty("path").GetString()!) != file.GetProperty("sha256").GetString())
                {
                    throw new InvalidDataException("Frozen native compilation input was changed.");
                }
            }
            string executingHost = result.GetProperty("ExecutingHost").GetString()!;
            if (!extra.Any(file => file.GetProperty("kind").GetString() == "tool" && file.GetProperty("path").GetString() == executingHost))
            {
                throw new InvalidDataException("Native compilation ran outside the frozen tool bundle.");
            }
            if (invocation.RequireOwned(result.GetProperty("ObjectFile").GetString()!) != NativeObject ||
                invocation.RequireOwned(result.GetProperty("ExportsFile").GetString()!) != ExportsFile ||
                Hash(NativeObject) != result.GetProperty("ObjectHash").GetString() || Hash(ExportsFile) != result.GetProperty("ExportsHash").GetString())
            {
                throw new InvalidDataException("Native object or export list does not match this invocation.");
            }
            string symbol = result.GetProperty("ReceiptSymbol").GetString()!;
            if (symbol != "__atomui_compile_" + InvocationId || System.IO.File.ReadAllLines(ExportsFile).Count(line => line == "_" + symbol) != 1)
            {
                throw new InvalidDataException("Missing caller-bound compilation export.");
            }
            string binaryHash = Hash(NativeBinary);
            if (System.IO.File.Exists(linkedPath))
            {
                if (Phase == "linked")
                {
                    throw new InvalidDataException("This invocation already has a linked proof; it cannot be authenticated again.");
                }
                LinkedReceiptHash = Hash(linkedPath);
                if (Phase == "published" && LinkedReceiptHash != ExpectedLinkedReceiptHash)
                {
                    throw new InvalidDataException("Linked receipt differs from the caller's previously captured proof.");
                }
                using var linked = JsonDocument.Parse(System.IO.File.ReadAllBytes(linkedPath));
                var proof = linked.RootElement;
                if (proof.GetProperty("invocationId").GetString() != InvocationId || proof.GetProperty("stage").GetString() != "sdk-link-verified" ||
                    proof.GetProperty("binaryHash").GetString() != binaryHash || proof.GetProperty("reportHash").GetString() != Hash(reportPath) ||
                    proof.GetProperty("objectHash").GetString() != Hash(NativeObject) || proof.GetProperty("exportsHash").GetString() != Hash(ExportsFile))
                {
                    throw new InvalidDataException("Native binary differs from the previously verified linked image.");
                }
                string originalBinary = invocation.RequireOwned(proof.GetProperty("binaryPath").GetString()!);
                if (Hash(originalBinary) != binaryHash)
                {
                    throw new InvalidDataException("The previously linked native image changed.");
                }
                if (Phase == "published")
                {
                    RegistrationFiles.CreateJson(VerifiedReceipt, new { format = 2, invocationId = InvocationId, stage = "published-copy-verified",
                        linkedReceiptPath = linkedPath, linkedReceiptHash = LinkedReceiptHash,
                        binaryPath = NativeBinary, binaryHash });
                }
                else
                {
                    VerifiedReceipt = linkedPath;
                }
                return true;
            }
            if (Phase != "linked")
            {
                throw new InvalidDataException("Published copy has no independently verified linked image.");
            }
            byte[] native = System.IO.File.ReadAllBytes(NativeBinary);
            MachOReceipt.Verify(native, "_" + symbol, Encoding.ASCII.GetBytes(symbol));
            RegistrationFiles.CreateJson(linkedPath, new { format = 2, invocationId = InvocationId, backend = "AtomUI.Registration.ILC8", stage = "sdk-link-verified",
                reportPath, reportHash = Hash(reportPath), inputPath = inputsPath, objectPath = NativeObject, objectHash = Hash(NativeObject),
                exportsPath = ExportsFile, exportsHash = Hash(ExportsFile), binaryPath = NativeBinary, binaryHash, receiptSymbol = symbol });
            VerifiedReceipt = linkedPath;
            LinkedReceiptHash = Hash(linkedPath);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or InvalidDataException or JsonException or InvalidOperationException or KeyNotFoundException or OverflowException)
        {
            invocation?.DeleteOwned(ownedCleanup);
            VerifiedReceipt = LinkedReceiptHash = string.Empty;
            return Fail("ATOMUIREG008", error);
        }
    }


}

internal static class MachOReceipt
{
    internal static void Verify(byte[] bytes, string symbol, byte[] payload)
    {
        uint U32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4));
        ulong U64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset, 8));
        if (bytes.Length < 32 || U32(0) != 0xfeedfacf || U32(12) != 2)
        {
            throw new InvalidDataException("Native8 delivery requires a thin 64-bit Mach-O executable.");
        }
        uint commands = U32(16);
        int end = checked(32 + (int)U32(20));
        if (commands > 1024 || end > bytes.Length)
        {
            throw new InvalidDataException("Invalid Mach-O load commands.");
        }
        int trieOffset = 0;
        int trieSize = 0;
        ulong imageBase = 0;
        var segments = new List<(ulong Address, ulong Size, ulong FileOffset)>();
        int position = 32;
        for (int i = 0; i < commands; i++)
        {
            uint command = U32(position);
            int size = checked((int)U32(position + 4));
            if (size < 8 || position + size > end)
            {
                throw new InvalidDataException("Invalid Mach-O command size.");
            }
            if (command == 0x19 && size >= 72)
            {
                ulong address = U64(position + 24);
                ulong fileOffset = U64(position + 40);
                ulong fileSize = U64(position + 48);
                if (fileSize != 0)
                {
                    segments.Add((address, fileSize, fileOffset));
                    if (fileOffset == 0)
                    {
                        imageBase = address;
                    }
                }
            }
            if (command == 0x80000033 && size >= 16)
            {
                trieOffset = checked((int)U32(position + 8));
                trieSize = checked((int)U32(position + 12));
            }
            else if ((command is 0x22 or 0x80000022) && size >= 48 && trieSize == 0)
            {
                trieOffset = checked((int)U32(position + 40));
                trieSize = checked((int)U32(position + 44));
            }
            position += size;
        }
        if (trieOffset <= 0 || trieSize <= 0 || (long)trieOffset + trieSize > bytes.Length)
        {
            throw new InvalidDataException("Final native image has no valid dynamic export trie.");
        }
        int trieEnd = trieOffset + trieSize;
        ulong ReadUleb(ref int cursor)
        {
            ulong value = 0;
            for (int shift = 0; shift < 64 && cursor < trieEnd; shift += 7)
            {
                byte part = bytes[cursor++];
                value |= (ulong)(part & 0x7f) << shift;
                if ((part & 0x80) == 0)
                {
                    return value;
                }
            }
            throw new InvalidDataException("Malformed export trie integer.");
        }
        int node = 0;
        int matched = 0;
        var visited = new HashSet<int>();
        ulong exportedAddress;
        while (true)
        {
            if (node < 0 || node >= trieSize || !visited.Add(node))
            {
                throw new InvalidDataException("Malformed export trie path.");
            }
            int cursor = trieOffset + node;
            int terminalSize = checked((int)ReadUleb(ref cursor));
            int children = checked(cursor + terminalSize);
            if (children >= trieEnd)
            {
                throw new InvalidDataException("Malformed export trie terminal.");
            }
            if (matched == symbol.Length && terminalSize > 0)
            {
                ulong flags = ReadUleb(ref cursor);
                if (flags != 0)
                {
                    throw new InvalidDataException("Receipt must be an ordinary defined export, not a re-export or resolver.");
                }
                exportedAddress = checked(imageBase + ReadUleb(ref cursor));
                break;
            }
            cursor = children;
            int childCount = bytes[cursor++];
            bool found = false;
            for (int child = 0; child < childCount; child++)
            {
                int start = cursor;
                while (cursor < trieEnd && bytes[cursor] != 0)
                {
                    cursor++;
                }
                if (cursor == trieEnd)
                {
                    throw new InvalidDataException("Unterminated export trie edge.");
                }
                string edge = Encoding.UTF8.GetString(bytes, start, cursor++ - start);
                int next = checked((int)ReadUleb(ref cursor));
                if (symbol.AsSpan(matched).StartsWith(edge, StringComparison.Ordinal))
                {
                    matched += edge.Length;
                    node = next;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                throw new InvalidDataException("Final native image did not export this invocation's receipt; object/export consumption is unproven.");
            }
        }
        foreach (var segment in segments)
        {
            if (exportedAddress >= segment.Address && exportedAddress - segment.Address + (ulong)payload.Length <= segment.Size)
            {
                int offset = checked((int)(segment.FileOffset + exportedAddress - segment.Address));
                if (bytes.AsSpan(offset, payload.Length).SequenceEqual(payload))
                {
                    return;
                }
            }
        }
        throw new InvalidDataException("Exported receipt address does not contain this invocation's object payload.");
    }
}
