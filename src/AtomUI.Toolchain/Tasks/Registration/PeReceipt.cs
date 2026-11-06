using System.Buffers.Binary;
using System.Text;

namespace AtomUI.Build.Tasks.Registration;

// Validate the exported address, never a byte search elsewhere in the image.
internal static class PeReceipt
{
    internal static void Verify(byte[] bytes, string symbol, byte[] payload)
    {
        void Bounds(long offset, long size)
        {
            if (offset < 0 || size < 0 || offset > bytes.Length - size)
            {
                throw new InvalidDataException("Truncated PE image or export table.");
            }
        }
        ushort U16(int offset) { Bounds(offset, 2); return BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(offset, 2)); }
        uint U32(int offset) { Bounds(offset, 4); return BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset, 4)); }
        if (U16(0) != 0x5a4d)
        {
            throw new InvalidDataException("Native8 requires a PE executable.");
        }
        int pe = checked((int)U32(0x3c));
        Bounds(pe, 24);
        int optional = checked(pe + 24);
        int optionalSize = U16(pe + 20);
        Bounds(optional, optionalSize);
        if (U32(pe) != 0x4550 || U16(pe + 4) != 0x8664 || (U16(pe + 22) & 0x2002) != 2 ||
            optionalSize < 240 || U16(optional) != 0x20b || U32(optional + 108) < 16 ||
            U32(optional + 112 + 14 * 8) != 0 || U32(optional + 116 + 14 * 8) != 0)
        {
            throw new InvalidDataException("Native8 Windows delivery requires an AMD64 PE32+ native executable without a CLR header.");
        }
        int count = U16(pe + 6);
        int sectionTable = checked(optional + optionalSize);
        Bounds(sectionTable, checked(count * 40));
        var sections = new List<(uint Rva, uint VirtualSize, uint Offset, uint RawSize)>();
        for (int i = 0; i < count; i++)
        {
            int section = sectionTable + i * 40;
            uint rva = U32(section + 12), virtualSize = U32(section + 8), rawSize = U32(section + 16), offset = U32(section + 20);
            Bounds(offset, rawSize);
            ulong size = Math.Max(virtualSize, rawSize);
            if ((ulong)rva + size > uint.MaxValue || sections.Any(s =>
                (rva < (ulong)s.Rva + Math.Max(s.VirtualSize, s.RawSize) && s.Rva < (ulong)rva + size) ||
                (rawSize > 0 && s.RawSize > 0 && offset < (ulong)s.Offset + s.RawSize && s.Offset < (ulong)offset + rawSize)))
            {
                throw new InvalidDataException("Ambiguous PE section mapping.");
            }
            sections.Add((rva, virtualSize, offset, rawSize));
        }
        int Map(uint rva, uint size)
        {
            var matches = sections.Where(s => rva >= s.Rva && (ulong)(rva - s.Rva) + size <= s.RawSize &&
                (ulong)(rva - s.Rva) + size <= Math.Max(s.VirtualSize, s.RawSize)).ToArray();
            if (matches.Length != 1 || rva == 0)
            {
                throw new InvalidDataException("PE export RVA has no unique file-backed mapping.");
            }
            int offset = checked((int)((ulong)matches[0].Offset + rva - matches[0].Rva));
            Bounds(offset, size);
            return offset;
        }
        string Name(uint rva)
        {
            var name = new List<byte>();
            for (uint i = 0; i <= 65536; i++)
            {
                byte c = bytes[Map(checked(rva + i), 1)];
                if (c == 0)
                {
                    if (name.Count == 0) { break; }
                    return Encoding.ASCII.GetString(name.ToArray());
                }
                if (c < 0x20 || c > 0x7e) { break; }
                name.Add(c);
            }
            throw new InvalidDataException("Invalid or unterminated PE export name.");
        }
        uint exportRva = U32(optional + 112), exportSize = U32(optional + 116);
        if (exportSize < 40 || (ulong)exportRva + exportSize > uint.MaxValue)
        {
            throw new InvalidDataException("Invalid PE export directory.");
        }
        int directory = Map(exportRva, exportSize);
        uint functions = U32(directory + 20), names = U32(directory + 24);
        if (functions == 0 || names == 0 || names > functions || functions > bytes.Length / 4 || names > bytes.Length / 4)
        {
            throw new InvalidDataException("Invalid PE export table counts.");
        }
        int addresses = Map(U32(directory + 28), checked(functions * 4));
        int nameTable = Map(U32(directory + 32), checked(names * 4));
        int ordinals = Map(U32(directory + 36), checked(names * 2));
        // Validate every entry, including entries preceding/following the receipt.
        for (int i = 0; i < functions; i++)
        {
            uint rva = U32(addresses + i * 4);
            if (rva != 0 && (rva < exportRva || rva >= (ulong)exportRva + exportSize))
            {
                Map(rva, 1);
            }
        }
        string? previousName = null;
        uint receiptRva = 0;
        for (int i = 0; i < names; i++)
        {
            string name = Name(U32(nameTable + i * 4));
            uint ordinal = U16(ordinals + i * 2);
            if ((previousName is not null && StringComparer.Ordinal.Compare(previousName, name) >= 0) || ordinal >= functions)
            {
                throw new InvalidDataException("PE export names must be strictly increasing, with valid ordinals.");
            }
            previousName = name;
            uint rva = U32(addresses + checked((int)ordinal * 4));
            if (rva == 0)
            {
                throw new InvalidDataException("Named PE export has no address.");
            }
            if (name == symbol)
            {
                if (rva >= exportRva && rva < (ulong)exportRva + exportSize)
                {
                    throw new InvalidDataException("Receipt cannot be a forwarded PE export.");
                }
                receiptRva = rva;
            }
        }
        if (receiptRva == 0 || !bytes.AsSpan(Map(receiptRva, checked((uint)payload.Length)), payload.Length).SequenceEqual(payload))
        {
            throw new InvalidDataException("Exact PE export does not contain this invocation's receipt payload.");
        }
    }
}
