using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AtomUI.Build.Tasks;

// .NET 8 uses bundle format 6: the apphost marker points at the manifest; each entry carries
// offset, uncompressed size, compressed size, type and a BinaryWriter UTF-8 relative path.
internal static class ConditionalBundleReader
{
    private static readonly byte[] Signature =
    [0x8b, 0x12, 0x02, 0xb9, 0x6a, 0x61, 0x20, 0x38, 0x72, 0x7b, 0x93, 0x02, 0x14, 0xd7, 0xa0, 0x32,
     0x13, 0xf5, 0xb9, 0xe6, 0xef, 0xae, 0x33, 0x18, 0xee, 0x3b, 0x2d, 0xce, 0x24, 0xb3, 0x6a, 0xae];

    internal static Dictionary<string, string> ReadHashes(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var headers = new HashSet<long>();
        var start = 0;
        while (start < bytes.Length)
        {
            var found = bytes.AsSpan(start).IndexOf(Signature);
            if (found < 0)
            {
                break;
            }
            var position = start + found;
            if (position >= sizeof(long))
            {
                var offset = BitConverter.ToInt64(bytes, position - sizeof(long));
                if (offset > 0 && offset < bytes.Length - 12 && BitConverter.ToUInt32(bytes, (int)offset) == 6)
                {
                    headers.Add(offset);
                }
            }
            start = position + Signature.Length;
        }
        if (headers.Count != 1)
        {
            throw new InvalidDataException("Expected one .NET 8 format-6 single-file bundle manifest.");
        }
        using var stream = new MemoryStream(bytes, writable: false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        stream.Position = headers.Single();
        if (reader.ReadUInt32() != 6 || reader.ReadUInt32() != 0)
        {
            throw new InvalidDataException("Unsupported single-file bundle version.");
        }
        var count = reader.ReadInt32();
        if (count is <= 0 or > 100000)
        {
            throw new InvalidDataException("Invalid bundle entry count.");
        }
        ReadString(reader);
        // Deps/runtimeconfig ranges and flags precede the individual entries in format 2+.
        reader.ReadInt64(); reader.ReadInt64(); reader.ReadInt64(); reader.ReadInt64(); reader.ReadUInt64();
        var entries = new List<(long Offset, long Size, long CompressedSize, string Name)>();
        for (var index = 0; index < count; index++)
        {
            var offset = reader.ReadInt64();
            var size = reader.ReadInt64();
            var compressedSize = reader.ReadInt64();
            var kind = reader.ReadByte();
            var name = ReadString(reader).Replace('\\', '/');
            var storedSize = compressedSize == 0 ? size : compressedSize;
            if (offset < 0 || size < 0 || compressedSize < 0 || offset > bytes.Length || storedSize > bytes.Length - offset ||
                size > int.MaxValue || kind > 5 || string.IsNullOrWhiteSpace(name) || Path.IsPathFullyQualified(name) ||
                name.Split('/').Any(part => part is ".." or ""))
            {
                throw new InvalidDataException("Invalid bundle payload range or relative path.");
            }
            entries.Add((offset, size, compressedSize, name));
        }
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            using var input = new MemoryStream(bytes, (int)entry.Offset, (int)(entry.CompressedSize == 0 ? entry.Size : entry.CompressedSize), writable: false);
            using var inflated = entry.CompressedSize == 0 ? null : new DeflateStream(input, CompressionMode.Decompress);
            Stream payload = inflated is null ? input : inflated;
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buffer = new byte[65536];
            long consumed = 0;
            int read;
            while ((read = payload.Read(buffer)) != 0)
            {
                consumed += read;
                if (consumed > entry.Size)
                {
                    throw new InvalidDataException("Bundle decompression exceeded its declared payload size.");
                }
                hash.AppendData(buffer, 0, read);
            }
            if (consumed != entry.Size || !result.TryAdd(entry.Name, Convert.ToHexStringLower(hash.GetHashAndReset())))
            {
                throw new InvalidDataException("Duplicate or incomplete bundle payload.");
            }
        }
        return result;
    }

    private static string ReadString(BinaryReader reader)
    {
        var count = reader.Read7BitEncodedInt();
        if (count < 0 || count > 65536 || count > reader.BaseStream.Length - reader.BaseStream.Position)
        {
            throw new InvalidDataException("Invalid bundle string length.");
        }
        return new UTF8Encoding(false, true).GetString(reader.ReadBytes(count));
    }
}
