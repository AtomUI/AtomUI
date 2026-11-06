#!/usr/bin/env python3
"""Explicit, heavyweight ILC8 mechanism verification. Not part of the global test suite."""
import argparse
import hashlib
import json
import os
import pathlib
import re
import shutil
import struct
import subprocess
import sys
import tempfile
import uuid
from xml.sax.saxutils import escape

from fixture_records import prepare_fixture
from windows_binary import Coff, PE, map_symbols, verify_table

FIXTURES = pathlib.Path(__file__).resolve().parent / 'fixtures'
CASES = {
    'ALL': ['a','b','direct','generic','root1','root2','style','token'],
    'DIRECT':['direct'], 'TOKEN':['token'], 'STYLE':['style'], 'TRANSITIVE':['a','b'],
    'CYCLE':['root1','root2'], 'GENERIC':['generic'], 'STATIC':[], 'NONE':[], 'OFF':[],
    'CCTOR':['a','b'], 'SAME_NAME':[], 'OR':['direct','direct-alias'],
    'ARRAY':['direct'], 'TYPEOF':['direct'], 'THROW':['throw'], 'CCTOR_THROW':['throw'],
    'SECOND':[], 'BOTH':['direct']
}


def read_binary(path):
    path = pathlib.Path(path)
    # CPython on Windows without the long-path policy needs extended spelling
    # for I/O. Do not rewrite the compiler's reported identity or command paths.
    if os.name == 'nt' and len(str(path.absolute())) >= 248:
        value = str(path.absolute())
        prefix = chr(92) * 2 + '?' + chr(92)
        if not value.startswith(prefix):
            path = pathlib.Path(prefix + value)
    return path.read_bytes()


def same_path(left, right):
    def canonical(value):
        value = os.path.abspath(value)
        prefix = chr(92) * 2 + '?' + chr(92)
        if os.name == 'nt' and value.startswith(prefix):
            value = value[len(prefix):]
        return os.path.normcase(os.path.normpath(value))
    return canonical(left) == canonical(right)


def run_logged(command, log, **kwargs):
    with log.open('w', encoding='utf-8') as stream:
        result = subprocess.run(command, stdout=stream, stderr=subprocess.STDOUT, **kwargs)
    if result.returncode:
        raise RuntimeError(str(command) + '\n' + log.read_text(encoding='utf-8', errors='replace')[-6000:])


def macho_symbols(path):
    data = path.read_bytes()
    magic, _, _, _, ncmds, _, _, _ = struct.unpack_from('<8I', data)
    if magic != 0xfeedfacf:
        raise RuntimeError('This verifier currently requires a little-endian 64-bit Mach-O image')
    sections = []
    symtab = None
    offset = 32
    for _ in range(ncmds):
        command, size = struct.unpack_from('<II', data, offset)
        if command == 0x19:
            count = struct.unpack_from('<I', data, offset + 64)[0]
            for index in range(count):
                entry = offset + 72 + index * 80
                name = data[entry:entry+16].split(b'\0')[0].decode()
                segment = data[entry+16:entry+32].split(b'\0')[0].decode()
                address, length, file_offset = struct.unpack_from('<QQI', data, entry+32)
                sections.append((name, segment, address, length, file_offset))
        elif command == 2:
            symtab = struct.unpack_from('<4I', data, offset+8)
        offset += size
    symbol_offset, count, string_offset, _ = symtab
    symbols = {}
    for index in range(count):
        string_index, kind, section, _, value = struct.unpack_from('<IBBHQ', data, symbol_offset+16*index)
        if string_index and kind & 0xe == 0xe and not kind & 0xe0:
            end = data.index(0, string_offset+string_index)
            name = data[string_offset+string_index:end].decode()
            symbols[name] = (value, section)
    return data, sections, symbols


def verify_native_table(executable, report, group, symbols, data, sections):
    table_name = '_' + group['SymbolName']
    if report['Scanner'] or not group['Requested']:
        assert table_name not in symbols, 'Unexpected native dispatch table'
        return None
    address, section_id = symbols[table_name]
    section = sections[section_id-1]
    assert section[1] != '__TEXT', 'Absolute pointers must not require loader writes to __TEXT'
    position = address-section[2]+section[4]
    version, width, count, reserved = struct.unpack_from('<4I', data, position)
    assert (version,width,count,reserved) == (1,8,group['UniqueMethods'],0)
    # dyld_info decodes the actual linked chained fixups. Raw bytes on modern macOS
    # contain chain encoding, not ready-to-call process addresses.
    output = subprocess.check_output(['xcrun','dyld_info','-fixups',str(executable)],text=True,stderr=subprocess.STDOUT)
    rebases = {}
    for line in output.splitlines():
        match = re.search(r'(0x[0-9A-Fa-f]+)\s+rebase\s+(0x[0-9A-Fa-f]+)',line)
        if match:
            rebases[int(match[1],16)] = int(match[2],16)
    actual = [rebases[address+16+8*index] for index in range(count)]
    expected = [symbols['_'+name][0] for name in group['SelectedMethodSymbols']]
    assert actual == expected, ('Native table target/order mismatch',actual,expected)
    return {'section':section[1]+','+section[0],'entries':count,'version':version,'pointerWidth':width,'exactRelocations':True}


def main():
    parser=argparse.ArgumentParser()
    parser.add_argument('--compiler-output',type=pathlib.Path,required=True)
    parser.add_argument('--tasks-assembly',type=pathlib.Path,
                        default=pathlib.Path(__file__).resolve().parents[5]/'.artifacts/bin/Debug/toolchain/tasks/net10.0/AtomUI.Build.Tasks.dll')
    parser.add_argument('--work-root',type=pathlib.Path)
    parser.add_argument('--keep-artifacts',action='store_true')
    parser.add_argument('--cases',nargs='+',choices=list(CASES),default=list(CASES))
    parser.add_argument('--rids',nargs='+',default=['osx-arm64'])
    parser.add_argument('--no-optimization-crosscheck',action='store_true')
    args=parser.parse_args()
    if any(rid not in ('osx-arm64', 'osx-x64', 'win-x64') for rid in args.rids):
        parser.error('Supported mechanism targets: osx-arm64, osx-x64, win-x64')
    root=(args.work_root or pathlib.Path(tempfile.mkdtemp(prefix='atomui-test-ilc8-records-'))).resolve()
    root.mkdir(parents=True,exist_ok=True)
    logs=root/'logs'; logs.mkdir(exist_ok=True)
    compiler=args.compiler_output.resolve()
    fixture, primary_group, second_group, aliases = prepare_fixture(root, FIXTURES)
    app=fixture/'app'
    for project in ('core', 'components', 'second'):
        run_logged(['dotnet','build','-c','Release'],logs/(project+'-build.log'),cwd=fixture/project)
    results=[]
    configurations=[(case,scanner,True,rid) for rid in args.rids for case in args.cases for scanner in (False,True)]
    if not args.no_optimization_crosscheck:
        configurations += [(case,scanner,False,rid) for rid in args.rids for case in ('ALL','CCTOR','OFF','STATIC') if case in args.cases for scanner in (False,True)]
    try:
        for case,scanner,optimize,rid in configurations:
            label=f'{rid}-{case}-scan{int(scanner)}-opt{int(optimize)}'
            directory=root/'out'/label
            invocation_id=uuid.uuid4().hex
            owned=root/'invocations'/invocation_id
            owned.mkdir(parents=True)
            (owned/'invocation.json').write_text(json.dumps(dict(invocationId=invocation_id, ownedRoot=str(owned))), encoding='utf-8')
            report_path=owned/'analysis.json'
            env=dict(os.environ,ATOMUI_ILC8_PREPARE_ROOT=str(root/'cache'),
                     ATOMUI_ILC8_INVOCATION_ID=invocation_id,ATOMUI_ILC8_OWNED_ROOT=str(owned),
                     ATOMUI_ILC8_RECEIPT=str(owned/'receipt.json'))
            for variable in ('ATOMUI_ILC8_INPUTS','ATOMUI_ILC8_FROZEN_CHILD'):
                env.pop(variable,None)
            command=['dotnet','publish','-c','Release','-r',rid,f'-p:Case={case}',f'-p:NoScanner={str(not scanner).lower()}',f'-p:Optimize={str(optimize).lower()}',f'-p:IlcToolsPath={compiler}/tools/',f'-p:BaseIntermediateOutputPath={root}/obj/{label}/',f'-p:BaseOutputPath={root}/bin/{label}/',f'-p:NativeIntermediateOutputPath={owned}/native/','-o',str(directory)]
            windows = rid == 'win-x64'
            map_path = owned/'sdk-link.map'
            if windows:
                # Extend only the verifier project; retain the official SDK link route.
                map_targets = root/'windows-map.targets'
                native_targets=pathlib.Path(__file__).resolve().parents[5]/'build/AtomUI.Registration.Native8.targets'
                marker=owned/'sdk-owned-root.txt'
                map_targets.write_text(f'''<Project>
<PropertyGroup><AtomUIBuildTasksAssembly>{escape(str(args.tasks_assembly.resolve()))}</AtomUIBuildTasksAssembly></PropertyGroup>
<Import Project="{escape(str(native_targets))}" />
<ItemGroup><LinkerArg Include="/MAP:&quot;$(AtomUIMechanismMap)&quot;" /></ItemGroup>
<Target Name="AtomUIMechanismCaptureRoot" AfterTargets="AtomUIVerifyPublishedNative8Registration">
<WriteLinesToFile File="$(AtomUIMechanismRootMarker)" Lines="$(_AtomUINative8OwnedRoot)" Overwrite="true" Encoding="UTF-8" />
</Target></Project>''', encoding='utf-8')
                command=[arg for arg in command if not arg.startswith(('-p:IlcToolsPath=', '-p:NativeIntermediateOutputPath='))]
                command += [f'-p:CustomAfterMicrosoftCommonTargets={map_targets}', f'-p:AtomUIMechanismMap={map_path}',
                            f'-p:AtomUIMechanismRootMarker={marker}', f'-p:AtomUINative8ManagedCompilerBundle={compiler}',
                            f'-p:AtomUINative8IntermediateRoot={root}/cache']
            run_logged(command,logs/(label+'-publish.log'),cwd=app,env=env)
            if windows:
                owned=pathlib.Path(marker.read_text(encoding='utf-8-sig').strip())
                report_path=owned/'analysis.json'
                invocation_id=owned.name
            report=json.loads(report_path.read_text(encoding='utf-8'))
            shutil.copyfile(report_path,logs/(label+'-selection.json'))
            assert report['InputKind']=='conditional-record-v1'
            assert report['Stage']=='compiler-complete'
            assert report['InvocationId']==invocation_id
            receipt=json.loads((owned/'receipt.json').read_text(encoding='utf-8'))
            assert receipt['invocationId']==invocation_id
            assert receipt['reportHash']==hashlib.sha256(report_path.read_bytes()).hexdigest()
            snapshot=json.loads((owned/'inputs.json').read_text(encoding='utf-8'))
            assert snapshot['format']==2
            assert report['InputHash']==hashlib.sha256((owned/'inputs.json').read_bytes()).hexdigest()
            for entry in snapshot['assemblies']+snapshot['additionalInputs']:
                assert hashlib.sha256(read_binary(entry['path'])).hexdigest()==entry['sha256']
            assert any(same_path(entry['path'],report['ExecutingHost']) and entry['kind']=='tool' for entry in snapshot['additionalInputs'])
            assert report['TrimmedSwitchTemplate']=='atomui.trimmed-switch.v1'
            assert pathlib.Path(report['ObjectFile']).is_relative_to(owned)
            assert pathlib.Path(report['ExportsFile']).is_relative_to(owned)
            assert report['ObjectHash']==hashlib.sha256(read_binary(report['ObjectFile'])).hexdigest()
            assert report['ExportsHash']==hashlib.sha256(read_binary(report['ExportsFile'])).hexdigest()
            export_lines=read_binary(report['ExportsFile']).decode('utf-8').splitlines()
            receipt_export=report['ReceiptSymbol']+' DATA' if windows else '_'+report['ReceiptSymbol']
            assert sum(line.strip()==receipt_export for line in export_lines)==1
            assert report['HostRuntime'].startswith('.NET 10.'),report['HostRuntime']
            assert report['Scanner']==scanner
            executable=directory/('AtomUIRegistrationProbe.exe' if windows else 'AtomUIRegistrationProbe')
            if windows:
                assert (report['TargetOS'],report['TargetArchitecture'])==('Windows','X64')
                coff=Coff(read_binary(report['ObjectFile']))
                pe=PE(executable.read_bytes())
                data=pe.data
                symbols=map_symbols(map_path,pe)
                linked_path=owned/'linked.json'
                published_path=owned/'verified.json'
                linked=json.loads(linked_path.read_text(encoding='utf-8'))
                published=json.loads(published_path.read_text(encoding='utf-8'))
                assert linked['stage']=='sdk-link-verified' and published['stage']=='published-copy-verified'
                assert linked['invocationId']==published['invocationId']==invocation_id
                assert published['linkedReceiptHash']==hashlib.sha256(linked_path.read_bytes()).hexdigest()
                assert linked['binaryHash']==published['binaryHash']==hashlib.sha256(data).hexdigest()
                assert read_binary(linked['binaryPath'])==data
                assert linked['reportHash']==hashlib.sha256(report_path.read_bytes()).hexdigest()
                assert linked['objectHash']==report['ObjectHash'] and linked['exportsHash']==report['ExportsHash']
                assert pathlib.Path(linked['reportPath']).resolve()==report_path.resolve()
                assert pathlib.Path(linked['inputPath']).resolve()==(owned/'inputs.json').resolve()
                assert pathlib.Path(published['linkedReceiptPath']).resolve()==linked_path.resolve()
                assert pathlib.Path(published['binaryPath']).resolve()==executable.resolve()
            else:
                data,sections,symbols=macho_symbols(executable)
            tables=[]
            assert {g['Id'] for g in report['Groups']}=={primary_group,second_group}
            for group in report['Groups']:
                second=group['Id']==second_group
                expected=(['direct'] if case in ('SECOND','BOTH') else []) if second else CASES[case]
                requested=case in ('SECOND','BOTH') if second else case not in ('OFF','SECOND')
                selected_aliases=sorted(aliases[key] for key in group['Selected'])
                assert selected_aliases==expected,(label,group['Id'],selected_aliases,expected)
                assert group['Requested']==requested,(label,group['Id'])
                selected=set(group['SelectedMethodSymbols'])
                for condition in group['Conditions']:
                    name=condition['MethodSymbol'] if windows else '_'+condition['MethodSymbol']
                    # The scanner may inline selected thunks. Absence of unselected
                    # targets still must hold in both the object and final image.
                    if condition['MethodSymbol'] not in selected:
                        assert name not in symbols,(label,condition)
                        if windows:
                            assert name not in coff.symbols,(label,condition)
                    elif not scanner:
                        assert name in symbols,(label,condition)
                table=verify_table(report,group,coff,pe,symbols) if windows else verify_native_table(executable,report,group,symbols,data,sections)
                tables.append({'group':group['Id'],'table':table})
            if case in ('ALL','STATIC'):
                assert any('StaticOnly__Calculate' in name for name in symbols)
            sentinel='UNUSED_FACTORY_SENTINEL_9DBC2310'
            assert sentinel.encode() not in data and sentinel.encode('utf-16le') not in data
            runtime_expected={'OR':['direct'],'CCTOR_THROW':[],'SECOND':['second'],'BOTH':['direct','second']}.get(case,CASES[case])
            actual=subprocess.run([str(executable)],env=dict(os.environ,ATOMUI_EXPECT=','.join(runtime_expected)),capture_output=True,text=True,encoding='utf-8')
            execution={'exitCode':actual.returncode,'stdout':actual.stdout,'stderr':actual.stderr}
            (logs/(label+'-run.json')).write_text(json.dumps(execution,indent=2),encoding='utf-8')
            assert actual.returncode==0,(label,execution)
            assert 'runtime=.NET 8.0.33' in actual.stdout,execution
            result={'case':case,'rid':rid,'scanner':scanner,'optimize':optimize,'host':report['HostRuntime'],'selected':{g['Id']:sorted(aliases[key] for key in g['Selected']) for g in report['Groups']},'nativeTables':tables,'nativeSha256':hashlib.sha256(data).hexdigest(),'runtimeExit':actual.returncode,'runtime':'.NET 8.0.33'}
            if windows:
                result.update(targetOS=report['TargetOS'],targetArchitecture=report['TargetArchitecture'],
                              executingHostHash=hashlib.sha256(read_binary(report['ExecutingHost'])).hexdigest(),
                              sdkMapHash=hashlib.sha256(map_path.read_bytes()).hexdigest(),
                              linkedProofHash=published['linkedReceiptHash'],publishedProofHash=hashlib.sha256(published_path.read_bytes()).hexdigest())
            results.append(result)
            (logs/'matrix-results.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
            print(json.dumps(result),flush=True)
        print(f'PASS NativeAOT mechanism matrix {len(results)}; evidence={logs}',flush=True)
    except Exception:
        print('FAILED: evidence and runnable artifacts retained at '+str(root),file=sys.stderr)
        raise
    if not args.keep_artifacts:
        for name in ('fixture','obj','bin','out','cache','invocations'):
            shutil.rmtree(root/name,ignore_errors=True)
    return 0


if __name__=='__main__':
    sys.exit(main())
