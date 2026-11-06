"""Bounded AMD64 COFF/PE reader for the opt-in registration mechanism gate."""
import re
import struct


class Binary:
    def __init__(self, data):
        self.data = data

    def span(self, offset, size):
        if offset < 0 or size < 0 or offset + size > len(self.data):
            raise ValueError('Binary range outside file')
        return self.data[offset:offset + size]

    def unpack(self, fmt, offset):
        return struct.unpack(fmt, self.span(offset, struct.calcsize(fmt)))

    def string(self, start, end):
        self.span(start, end - start)
        stop = self.data.find(b'\0', start, end)
        if stop < 0:
            raise ValueError('Unterminated binary string')
        return self.data[start:stop].decode('utf-8', errors='strict')


class Coff(Binary):
    def __init__(self, data):
        super().__init__(data)
        big = self.span(0, 4) == b'\0\0\xff\xff'
        if big:
            if self.unpack('<HH', 4) != (2, 0x8664) or self.span(12, 16) != bytes.fromhex('c7a1bad1eebaa94baf20faf66aa4dcb8'):
                raise ValueError('Unsupported bigobj header')
            count, symoff, symcount = self.unpack('<III', 44)
            head, symsize = 56, 20
        else:
            machine, count, _, symoff, symcount, optional, _ = self.unpack('<HHIIIHH', 0)
            if machine != 0x8664 or optional:
                raise ValueError('Expected AMD64 COFF object')
            head, symsize = 20, 18
        self.span(head, count * 40)
        header_end = head + count * 40
        if symoff < header_end:
            raise ValueError('COFF symbol table overlaps headers')
        self.span(symoff, symcount * symsize)
        stroff = symoff + symcount * symsize
        strlen, = self.unpack('<I', stroff)
        if strlen < 4:
            raise ValueError('Invalid COFF string table')
        self.span(stroff, strlen)

        def long_name(offset):
            if not 4 <= offset < strlen:
                raise ValueError('Invalid COFF long-name offset')
            return self.string(stroff + offset, stroff + strlen)

        self.sections = []
        for i in range(count):
            pos = head + 40 * i
            rawname = self.span(pos, 8).split(b'\0')[0]
            name = long_name(int(rawname[1:])) if rawname.startswith(b'/') else rawname.decode('ascii')
            _, _, size, raw, reloc, _, nreloc, _, flags = self.unpack('<IIIIIIHHI', pos + 8)
            if raw:
                if size and raw < header_end:
                    raise ValueError('COFF section data overlaps headers')
                self.span(raw, size)
            elif size and not flags & 0x80:
                raise ValueError('Missing COFF section data')
            if flags & 0x1000000:
                if nreloc != 0xffff:
                    raise ValueError('Invalid relocation overflow marker')
                nreloc, zero, kind = self.unpack('<IIH', reloc)
                if nreloc < 1 or zero or kind:
                    raise ValueError('Invalid relocation overflow count')
                self.span(reloc, nreloc * 10)
                reloc += 10
                nreloc -= 1
            self.span(reloc, nreloc * 10)
            if nreloc and reloc < header_end:
                raise ValueError('COFF relocations overlap headers')
            self.sections.append(dict(name=name, size=size, raw=raw, flags=flags, reloc=reloc, nreloc=nreloc))
        self.symbols, by_index = {}, {}
        index = 0
        while index < symcount:
            pos = symoff + index * symsize
            rawname = self.span(pos, 8)
            name = long_name(self.unpack('<I', pos + 4)[0]) if rawname[:4] == bytes(4) else rawname.split(b'\0')[0].decode('utf-8')
            value, = self.unpack('<I', pos + 8)
            section, _, storage, aux = self.unpack('<iHBB' if big else '<hHBB', pos + 12)
            if section < -2 or section > count or index + aux >= symcount:
                raise ValueError('Invalid COFF symbol section/aux count')
            if section > 0 and value > self.sections[section - 1]['size']:
                raise ValueError('COFF symbol outside section')
            symbol = dict(name=name, value=value, section=section, storage=storage)
            by_index[index] = symbol
            if section > 0 and storage == 2:
                if name in self.symbols:
                    raise ValueError('Duplicate defined external symbol')
                self.symbols[name] = symbol
            index += 1 + aux
        for section in self.sections:
            section['relocations'] = []
            for i in range(section['nreloc']):
                address, target, kind = self.unpack('<IIH', section['reloc'] + i * 10)
                if target not in by_index or address >= section['size'] or (kind == 1 and address + 8 > section['size']):
                    raise ValueError('Invalid COFF relocation target/range')
                section['relocations'].append((address, kind, by_index[target]))


class PE(Binary):
    def __init__(self, data):
        super().__init__(data)
        if self.span(0, 2) != b'MZ':
            raise ValueError('Missing DOS signature')
        nt, = self.unpack('<I', 60)
        if self.span(nt, 4) != b'PE\0\0':
            raise ValueError('Missing PE signature')
        machine, count, _, _, _, size, flags = self.unpack('<HHIIIHH', nt + 4)
        opt = nt + 24
        self.span(opt, size)
        if machine != 0x8664 or not 1 <= count <= 96 or not flags & 2 or flags & 0x2000 or size < 240 or self.unpack('<H', opt)[0] != 0x20b:
            raise ValueError('Expected native AMD64 PE32+ executable')
        self.base, = self.unpack('<Q', opt + 24)
        ndir, = self.unpack('<I', opt + 108)
        if ndir < 16 or 112 + 8 * ndir > size or self.unpack('<II', opt + 112 + 14 * 8) != (0, 0):
            raise ValueError('Invalid native PE data directories')
        self.directories = [self.unpack('<II', opt + 112 + i * 8) for i in range(16)]
        self.sections = []
        self.span(opt + size, count * 40)
        for i in range(count):
            pos = opt + size + i * 40
            name = self.span(pos, 8).split(b'\0')[0].decode('ascii')
            virtual, rva, rawsize, raw = self.unpack('<IIII', pos + 8)
            self.span(raw, rawsize)
            extent = max(virtual, rawsize)
            if rva + extent > 0x100000000 or any(rva < s['rva'] + s['extent'] and s['rva'] < rva + extent for s in self.sections):
                raise ValueError('Overlapping/overflowed PE sections')
            flags, = self.unpack('<I', pos + 36)
            self.sections.append(dict(name=name, rva=rva, extent=extent, raw=raw, size=rawsize, flags=flags))
        self.dir64 = set()
        rva, size = self.directories[5]
        if size:
            offset = self.offset(rva, size)
            end = offset + size
            while offset < end:
                page, length = self.unpack('<II', offset)
                if page & 4095 or length < 8 or length % 4 or offset + length > end:
                    raise ValueError('Invalid PE relocation block')
                for pos in range(offset + 8, offset + length, 2):
                    entry, = self.unpack('<H', pos)
                    kind, target = entry >> 12, page + (entry & 4095)
                    if kind == 10:
                        self.offset(target, 8)
                        if target in self.dir64:
                            raise ValueError('Duplicate DIR64 relocation')
                        self.dir64.add(target)
                    elif kind != 0:
                        raise ValueError('Unsupported AMD64 base relocation')
                offset += length

    def offset(self, rva, size):
        matches = [s for s in self.sections if s['rva'] <= rva and rva + size <= s['rva'] + s['size']]
        if len(matches) != 1:
            raise ValueError('Unmapped or ambiguous PE RVA')
        return matches[0]['raw'] + rva - matches[0]['rva']


def map_symbols(path, pe):
    symbols = {}
    for line in path.read_text(encoding='utf-8').splitlines():
        if line.strip() == 'Static symbols':
            break  # Translation-unit-local names are not globally unique.
        match = re.match(r'\s*([0-9a-fA-F]{4}):([0-9a-fA-F]{8,16})\s+(\S+)\s+([0-9a-fA-F]{16})(?:\s|$)', line)
        if match:
            name, address = match[3], int(match[4], 16)
            if int(match[1], 16) == 0:  # Absolute linker constants, e.g. __ImageBase.
                continue
            # Maps include zero-filled hydrated/BSS data and section-end labels.
            # Only table contents and pointer destinations require file-backed data.
            rva = address - pe.base
            if not any(s['rva'] <= rva <= s['rva'] + s['extent'] for s in pe.sections):
                raise ValueError('Linker map address outside PE virtual sections')
            if name in symbols and symbols[name][0] != address:
                raise ValueError('Conflicting linker map symbol')
            symbols[name] = (address, int(match[1], 16))
    if not symbols:
        raise ValueError('Empty SDK linker map')
    return symbols


def verify_table(report, group, coff, pe, symbols):
    name = group['SymbolName']
    if report['Scanner'] or not group['Requested']:
        assert name not in symbols and name not in coff.symbols, 'Unexpected native dispatch table'
        return None
    table = coff.symbols[name]
    section = coff.sections[table['section'] - 1]
    offset = section['raw'] + table['value']
    expected_header = (1, 8, group['UniqueMethods'], 0)
    assert table['value'] + 16 + 8 * group['UniqueMethods'] <= section['size']
    assert coff.unpack('<4I', offset) == expected_header
    count = group['UniqueMethods']
    relocations = [(address, kind, target['name']) for address, kind, target in section['relocations']
                   if table['value'] <= address < table['value'] + 16 + 8 * count]
    expected = [(table['value'] + 16 + 8 * i, 1, target) for i, target in enumerate(group['SelectedMethodSymbols'])]
    assert relocations == expected, ('COFF ADDR64 order mismatch', relocations, expected)
    assert coff.span(offset + 16, 8 * count) == bytes(8 * count), 'Nonzero pointer addend'
    address = symbols[name][0]
    rva = address - pe.base
    pos = pe.offset(rva, 16 + 8 * count)
    assert pe.unpack('<4I', pos) == expected_header
    section = next(s for s in pe.sections if s['rva'] <= rva < s['rva'] + s['size'])
    assert not section['flags'] & 0x20000000, 'Pointer table in executable section'
    actual = [pe.unpack('<Q', pos + 16 + 8 * i)[0] for i in range(count)]
    expected = [symbols[target][0] for target in group['SelectedMethodSymbols']]
    assert actual == expected, ('PE target/order mismatch', actual, expected)
    for destination in actual:
        target_rva = destination - pe.base
        pe.offset(target_rva, 1)
        assert any(s['rva'] <= target_rva < s['rva'] + s['size'] and s['flags'] & 0x20000000
                   for s in pe.sections), 'Selected pointer destination is not executable'
    assert all(rva + 16 + 8 * i in pe.dir64 for i in range(count)), 'Missing DIR64 base relocation'
    return dict(section=section['name'], entries=count, version=1, pointerWidth=8, exactRelocations=True,
                coffRelocation='AMD64_ADDR64', peRelocation='DIR64')
