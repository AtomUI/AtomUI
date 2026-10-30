#!/usr/bin/env python3
"""Build a pinned, managed ILC8 host without rebuilding its native runtime.

All generated compiler projects/outputs stay in the explicitly supplied source/output
workspaces. This does not change the AtomUI build graph or the global NuGet cache.
"""
import argparse
import contextlib
import os
import time
import uuid
import difflib
import hashlib
import json
import pathlib
import shutil
import subprocess
import sys
from xml.sax.saxutils import escape

MODULE = pathlib.Path(__file__).resolve().parents[1]
REGISTRATION = MODULE.parents[1] / 'Common' / 'Registration'


def replace_once(text, before, after):
    if text.count(before) != 1:
        raise RuntimeError('Pinned compiler integration point changed: ' + before[:80])
    return text.replace(before, after)


@contextlib.contextmanager
def exclusive_lock(path):
    # All source-project rewrites and bundle publication use the same lock on every OS.
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('a+b') as stream:
        stream.seek(0, os.SEEK_END)
        if stream.tell() == 0:
            stream.write(b'0')
            stream.flush()
        deadline = time.monotonic() + 3600
        while True:
            try:
                stream.seek(0)
                if os.name == 'nt':
                    import msvcrt
                    msvcrt.locking(stream.fileno(), msvcrt.LK_NBLCK, 1)
                else:
                    import fcntl
                    fcntl.flock(stream.fileno(), fcntl.LOCK_EX | fcntl.LOCK_NB)
                break
            except OSError:
                if time.monotonic() >= deadline:
                    raise RuntimeError('Timed out waiting for compiler build lock: ' + str(path))
                time.sleep(0.1)
        try:
            yield
        finally:
            stream.seek(0)
            if os.name == 'nt':
                msvcrt.locking(stream.fileno(), msvcrt.LK_UNLCK, 1)
            else:
                fcntl.flock(stream.fileno(), fcntl.LOCK_UN)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source-root', type=pathlib.Path, required=True)
    parser.add_argument('--output', type=pathlib.Path, required=True)
    parser.add_argument('--native-tools', type=pathlib.Path)
    parser.add_argument('--host-rid')
    parser.add_argument('--managed-only', action='store_true', help='Build portable managed package assets without native helpers or a local launcher')
    parser.add_argument('--dotnet', default='dotnet')
    parser.add_argument('--ensure', action='store_true', help='Reuse only a validated bundle built from exactly these recipe/source inputs')
    args = parser.parse_args()
    if not args.managed_only and (args.native_tools is None or args.host_rid is None):
        parser.error('--native-tools and --host-rid are required unless --managed-only is specified')
    with exclusive_lock(args.output.resolve() / '.atomui-build.lock'), exclusive_lock(args.source_root.resolve() / '.atomui-build.lock'):
        return build(args)


def build(args):
    source = args.source_root.resolve()
    output = args.output.resolve()
    native = args.native_tools.resolve() if args.native_tools else output / 'no-native-assets'
    host_rid = '' if args.managed_only else args.host_rid
    upstream = json.loads((MODULE / 'upstream.json').read_text())
    commit = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=source, text=True).strip()
    if commit != upstream['commit']:
        raise RuntimeError('Compiler source commit does not match the pinned input')
    checked_paths = ['src/coreclr/tools', 'src/libraries/Common', 'src/tools/illink/src/ILLink.Shared']
    subprocess.run(['git', 'diff', '--exit-code', 'HEAD', '--', *checked_paths], cwd=source, check=True, stdout=subprocess.DEVNULL)
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '--', *checked_paths], cwd=source, text=True).splitlines()
    if any(path.endswith(('.cs', '.props', '.targets')) for path in untracked):
        raise RuntimeError('Untracked source could enter the managed compiler build')
    reader_names = ['ConditionalRegistrationManifest.cs', 'ConditionalRegistrationMetadata.cs', 'RegistrationContractJson.cs', 'ConditionalRegistrationSnapshot.cs']
    recipe_files = [pathlib.Path(__file__).resolve(), MODULE / 'upstream.json', *sorted((MODULE / 'Adapter').glob('*.cs')),
        *(REGISTRATION / name for name in reader_names), REGISTRATION / 'Native8Invocation.cs']
    recipe = dict(sourceCommit=commit, sourceRoot=str(source), hostRid=host_rid, managedOnly=args.managed_only,
        files={str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in recipe_files})
    stamp = output / 'build-inputs.json'
    bundle = output / 'managed-bundle'
    capability_path = bundle / 'atomui-ilc8-capability.json'
    if args.ensure and stamp.exists() and json.loads(stamp.read_text()) == recipe and capability_path.exists():
        capability = json.loads(capability_path.read_text())
        if (set(path.name for path in bundle.iterdir()) == set(capability['files']) | {'atomui-ilc8-capability.json'} and
            all(hashlib.sha256((bundle / name).read_bytes()).hexdigest() == expected for name, expected in capability['files'].items())):
            print(json.dumps({'managedBundle': str(bundle), 'reused': True, 'sourceCommit': commit}))
            return 0
    if not args.managed_only and native.parent.name != upstream['nativeToolPackageVersion']:
        raise RuntimeError('Native tool package version does not match the pinned compiler')
    host_os, host_arch = ('', '') if args.managed_only else args.host_rid.rsplit('-', 1)
    suffix = '.dll' if host_os == 'win' else '.dylib' if host_os == 'osx' else '.so'
    prefix = '' if host_os == 'win' else 'lib'
    for name in ([] if args.managed_only else [prefix + 'jitinterface_' + host_arch + suffix, prefix + 'objwriter' + suffix]):
        if not (native / name).is_file():
            raise RuntimeError('Missing matching native compiler dependency: ' + name)
    generated = output / 'build'
    host = output / 'host'
    tools = output / 'tools'
    for path in (generated, host, tools):
        path.mkdir(parents=True, exist_ok=True)
    driver = source / 'src/coreclr/tools/aot/ILCompiler/Program.cs'
    original = driver.read_text(encoding='utf-8-sig')
    modified = replace_once(original,
        '            Dictionary<string, string> inputFilePaths = new Dictionary<string, string>();',
        '''            var originalInputs = _command.Result.GetValue(_command.InputFilePaths);
            var originalReferences = Get(_command.ReferenceFiles);
            var registrationInputPlan = RegistrationInputBootstrap.Create(originalInputs, originalReferences, outputFilePath, Get(_command.ExportsFile),
                new Dictionary<string, string[]>
                {
                    ["root-descriptor"] = Get(_command.RdXmlFilePaths).Concat(Get(_command.LinkTrimFilePaths)).ToArray(),
                    ["reference"] = Get(_command.SatelliteFilePaths),
                    ["configuration"] = Get(_command.SubstitutionFilePaths).Concat(Get(_command.DirectPInvokeLists)).ToArray(),
                    ["opaque"] = Get(_command.MibcFilePaths),
                    ["tool"] = string.IsNullOrEmpty(Get(_command.JitPath)) ? Array.Empty<string>() : new[] { Get(_command.JitPath) }
                }, _command.Result.Tokens.Select(token => token.Value).ToArray());
            Dictionary<string, string> inputFilePaths = new Dictionary<string, string>();''')
    modified = replace_once(modified,
        '            foreach (var inputFile in _command.Result.GetValue(_command.InputFilePaths))',
        '            foreach (var inputFile in registrationInputPlan?.Inputs ?? originalInputs)')
    modified = replace_once(modified,
        '            typeSystemContext.ReferenceFilePaths = Get(_command.ReferenceFiles);',
        '            typeSystemContext.ReferenceFilePaths = registrationInputPlan?.References ?? originalReferences;')
    for option in ['RdXmlFilePaths', 'LinkTrimFilePaths', 'SatelliteFilePaths', 'SubstitutionFilePaths', 'DirectPInvokeLists', 'MibcFilePaths', 'JitPath']:
        # Only downstream consumers are redirected; preparation sees the original parsed options.
        boundary = modified.index('            typeSystemContext.ReferenceFilePaths =')
        source_prefix, tail = modified[:boundary], modified[boundary:]
        tail = tail.replace('Get(_command.' + option + ')', '(registrationInputPlan?.Redirect(Get(_command.' + option + ')) ?? Get(_command.' + option + '))')
        modified = source_prefix + tail
    modified = replace_once(modified,
        '            CompilerGeneratedState compilerGeneratedState = new CompilerGeneratedState(ilProvider, logger);',
        '''            ConditionalRegistrationBackend registrationBackend = null;
            if (registrationInputPlan != null)
            {
                if (multiFile || singleMethod != null)
                {
                    throw new CommandLineException("Conditional registration requires whole-program app compilation");
                }
                registrationBackend = new ConditionalRegistrationBackend(ilProvider, typeSystemContext, registrationInputPlan.SnapshotPath, useScanner);
                ilProvider = registrationBackend;
                compilationRoots.Add(registrationBackend);
            }

            CompilerGeneratedState compilerGeneratedState = new CompilerGeneratedState(ilProvider, logger);''')
    modified = replace_once(modified,
        '            DynamicInvokeThunkGenerationPolicy invokeThunkGenerationPolicy = new DefaultDynamicInvokeThunkGenerationPolicy();',
        '            mdBlockingPolicy = registrationBackend?.WrapMetadataBlockingPolicy(mdBlockingPolicy) ?? mdBlockingPolicy;\n\n            DynamicInvokeThunkGenerationPolicy invokeThunkGenerationPolicy = new DefaultDynamicInvokeThunkGenerationPolicy();')
    old_preinit = 'new PreinitializationManager(typeSystemContext, compilationGroup, ilProvider,'
    if modified.count(old_preinit) != 2:
        raise RuntimeError('Pinned compiler preinitialization managers changed')
    modified = modified.replace(old_preinit, 'new PreinitializationManager(typeSystemContext, compilationGroup, registrationBackend?.PreinitializationView ?? ilProvider,')
    modified = replace_once(modified, '                ILScanResults scanResults = scanner.Scan();',
        '                ILScanResults scanResults = scanner.Scan();\n                registrationBackend?.FreezeScanner(((Compilation)scanner).NodeFactory);')
    modified = replace_once(modified, '            CompilationResults compilationResults = compilation.Compile(outputFilePath, ObjectDumper.Compose(dumpers));',
        '''            CompilationResults compilationResults;
            try
            {
                compilationResults = compilation.Compile(outputFilePath, ObjectDumper.Compose(dumpers));
                registrationBackend?.VerifyAndWriteReport(((Compilation)compilation).NodeFactory, outputFilePath);
            }
            catch
            {
                registrationBackend?.AbortCompilation(outputFilePath, Get(_command.ExportsFile));
                throw;
            }''')
    modified = replace_once(modified,
        'new ExportsFileWriter(typeSystemContext, exportsFile, Get(_command.ExportDynamicSymbols))',
        'new ExportsFileWriter(typeSystemContext, exportsFile, registrationBackend?.ExportSymbols(Get(_command.ExportDynamicSymbols)) ?? Get(_command.ExportDynamicSymbols))')
    modified = replace_once(modified,
        '                defFileWriter.EmitExportedMethods();',
        '                defFileWriter.EmitExportedMethods();\n                registrationBackend?.CompleteCompilation(exportsFile);')
    modified = replace_once(modified, '        public int Run()\n        {',
        """        public int Run()
        {
            try
            {
                return RunCore();
            }
            catch
            {
                RegistrationInputBootstrap.CleanupFailedAttempt();
                throw;
            }
        }

        private int RunCore()
        {""")
    modified = replace_once(modified,
        '        private static int Main(string[] args) =>',
        '        private static int Main(string[] args) =>\n            RegistrationInputBootstrap.TryCleanup(args, out int cleanupExit) ? cleanupExit :')
    (generated / 'Program.cs').write_text(modified)
    (generated / 'driver.patch').write_text(''.join(difflib.unified_diff(original.splitlines(True), modified.splitlines(True), fromfile='a/src/coreclr/tools/aot/ILCompiler/Program.cs', tofile='b/src/coreclr/tools/aot/ILCompiler/Program.cs')))
    (generated / 'SharedStrings.cs').write_text('''namespace ILLink.Shared
{
    internal static class SharedStrings
    {
        internal static System.Resources.ResourceManager ResourceManager { get; } = new System.Resources.ResourceManager("ILLink.Shared.SharedStrings", typeof(SharedStrings).Assembly);
        internal static string InterfaceRequiresMismatchMessage => ResourceManager.GetString(nameof(InterfaceRequiresMismatchMessage));
        internal static string ImplementationRequiresMismatchMessage => ResourceManager.GetString(nameof(ImplementationRequiresMismatchMessage));
        internal static string BaseRequiresMismatchMessage => ResourceManager.GetString(nameof(BaseRequiresMismatchMessage));
        internal static string DerivedRequiresMismatchMessage => ResourceManager.GetString(nameof(DerivedRequiresMismatchMessage));
    }
}
''')
    def xml(value):
        return escape(str(value))
    props = f'''<Project><PropertyGroup>
<NetCoreAppToolCurrent>{upstream['hostTargetFramework']}</NetCoreAppToolCurrent><LangVersion>12.0</LangVersion>
<LibrariesProjectRoot>{xml(source)}/src/libraries</LibrariesProjectRoot>
<AllowUnsafeBlocks>true</AllowUnsafeBlocks><DotNetBuildFromSource>true</DotNetBuildFromSource>
<RuntimeIdentifier>{host_rid}</RuntimeIdentifier><PackageRID>{host_rid}</PackageRID><TargetArchitecture>{host_arch}</TargetArchitecture>
<HostOS>{host_os}</HostOS><TargetOS>{host_os}</TargetOS><LibPrefix>{prefix}</LibPrefix><LibSuffix>{suffix}</LibSuffix>
<CoreCLRArtifactsPath>{xml(native)}</CoreCLRArtifactsPath><OutputPath>{xml(host)}/</OutputPath>
<BaseIntermediateOutputPath>{xml(output)}/obj/$(MSBuildProjectName)/</BaseIntermediateOutputPath>
<EnableNETAnalyzers>false</EnableNETAnalyzers>
<SystemCommandLineVersion>{upstream['systemCommandLineVersion']}</SystemCommandLineVersion><NetStandardLibraryVersion>2.0.3</NetStandardLibraryVersion>
</PropertyGroup></Project>'''
    (generated / 'tool.props').write_text(props)
    (generated / 'tool.targets').write_text(f'''<Project>
<ItemGroup Condition="'$(MSBuildProjectName)' == 'ILCompiler.Compiler'">
<Compile Include="{xml(generated)}/SharedStrings.cs" />
<EmbeddedResource Include="{xml(source)}/src/tools/illink/src/ILLink.Shared/SharedStrings.resx" Condition="false" />
<EmbeddedResource Update="{xml(source)}/src/tools/illink/src/ILLink.Shared/SharedStrings.resx"><LogicalName>ILLink.Shared.SharedStrings.resources</LogicalName></EmbeddedResource>
</ItemGroup>
</Project>''')
    # Imported ILCompiler.props owns relative source/project includes, so the thin project
    # must live next to it. Only this untracked, generated file is placed in the checkout.
    project = driver.parent / 'ILCompiler.AtomUI.csproj'
    invocation_source = REGISTRATION / 'Native8Invocation.cs'
    reader_sources = ''.join(f'<Compile Include="{xml(REGISTRATION/name)}"/>' for name in reader_names) + f'<Compile Include="{xml(invocation_source)}"/>'
    project.write_text(f'''<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><RuntimeIdentifier>{host_rid or "linux-x64"}</RuntimeIdentifier><OutputPath>{xml(host)}/</OutputPath><ImplicitUsings>enable</ImplicitUsings><Nullable>annotations</Nullable></PropertyGroup>
<Import Project="ILCompiler.props" />
<PropertyGroup><RuntimeIdentifier>{host_rid}</RuntimeIdentifier></PropertyGroup>
<ItemGroup><Content Remove="@(Content)" Condition="'{str(args.managed_only).lower()}' == 'true'"/><Compile Remove="Program.cs"/><Compile Include="{xml(generated)}/Program.cs"/><Compile Include="{xml(MODULE)}/Adapter/*.cs"/>{reader_sources}</ItemGroup>
</Project>''')
    command = [args.dotnet, 'build', str(project), '-c', 'Release',
        '-p:DirectoryBuildPropsPath=' + str(generated / 'tool.props'),
        '-p:DirectoryBuildTargetsPath=' + str(generated / 'tool.targets'),
        # The default checkout lives below the AtomUI repository. Its parent central-package
        # configuration must not override the upstream compiler's explicit pinned versions.
        '-p:ImportDirectoryPackagesProps=false', '-p:ManagePackageVersionsCentrally=false',
        '-p:BuildingInsideVisualStudio=false', '-p:BuildProjectReferences=true',
        '-p:SelfContained=false', '-p:UseAppHost=false', '-p:NuGetAudit=false',
        # The pinned compiler also requires stable packages such as NETStandard.Library.
        # Escape the separator so MSBuild receives one property containing both sources.
        '-p:RestoreSources=https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-libraries/nuget/v3/index.json%3Bhttps://api.nuget.org/v3/index.json']
    with (output / 'build-host.log').open('w') as log:
        result = subprocess.run(command, cwd=output, stdout=log, stderr=subprocess.STDOUT)
    if result.returncode:
        print((output / 'build-host.log').read_text()[-16000:])
        return result.returncode
    for path in ([] if args.managed_only else native.glob('*' + suffix)):
        shutil.copy2(path, host / path.name)
    resolved_dotnet = shutil.which(args.dotnet) or args.dotnet
    if not args.managed_only:
        if host_os == 'win':
            launcher = '@echo off\r\n"' + str(resolved_dotnet) + '" "' + str(host / 'ilc.dll') + '" %*\r\n'
            (tools / 'ilc.cmd').write_text(launcher)
        else:
            import shlex
            launcher = '#!/bin/sh\nexec ' + shlex.quote(resolved_dotnet) + ' ' + shlex.quote(str(host / 'ilc.dll')) + ' "$@"\n'
            (tools / 'ilc').write_text(launcher)
            (tools / 'ilc').chmod(0o755)
    hashes = {}
    for path in sorted(host.iterdir()):
        if path.is_file():
            hashes[path.name] = hashlib.sha256(path.read_bytes()).hexdigest()
    identity = dict(upstream, hostRid=host_rid, hostFiles=hashes,
        adapterFiles={p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted((MODULE / 'Adapter').glob('*.cs'))})
    (output / 'host-identity.json').write_text(json.dumps(identity, indent=2))
    managed_names = ['ilc.dll', 'ilc.deps.json', 'ilc.runtimeconfig.json', 'System.CommandLine.dll'] + [
        'ILCompiler.' + name + '.dll' for name in ['Compiler', 'TypeSystem', 'MetadataTransform', 'DependencyAnalysisFramework', 'RyuJit']]
    bundle = output / 'managed-bundle'
    staged_bundle = output / ('.managed-bundle-' + uuid.uuid4().hex)
    staged_bundle.mkdir()
    try:
        for name in managed_names:
            shutil.copy2(host / name, staged_bundle / name)
        capability = dict(format=1, sourceCommit=upstream['commit'], nativeToolPackageVersion=upstream['nativeToolPackageVersion'],
            hostTargetFramework=upstream['hostTargetFramework'], inputFormat=2, collectorTemplate='atomui.collector.v1',
            trimmedSwitchTemplate='atomui.trimmed-switch.v1',
            files={name: hashlib.sha256((staged_bundle / name).read_bytes()).hexdigest() for name in managed_names})
        (staged_bundle / 'atomui-ilc8-capability.json').write_text(json.dumps(capability, indent=2))
        backup = output / ('.old-managed-bundle-' + uuid.uuid4().hex)
        if bundle.exists():
            bundle.rename(backup)
        try:
            staged_bundle.rename(bundle)
        except BaseException:
            if backup.exists():
                backup.rename(bundle)
            raise
        if backup.exists():
            shutil.rmtree(backup)
        staged_stamp = stamp.with_suffix('.' + uuid.uuid4().hex + '.tmp')
        staged_stamp.write_text(json.dumps(recipe, indent=2))
        os.replace(staged_stamp, stamp)
    finally:
        if staged_bundle.exists():
            shutil.rmtree(staged_bundle)
    print(json.dumps({'managedBundle': str(bundle), 'toolsPath': str(tools) + '/', 'host': str(host / 'ilc.dll'), 'identity': str(output / 'host-identity.json')}))
    return 0


if __name__ == '__main__':
    sys.exit(main())
