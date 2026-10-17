#!/usr/bin/env python3
"""Read-only Roslyn audit of explicitly selected control sources; build only in system temp."""

import argparse
from pathlib import Path
import subprocess
import sys
import tempfile
from xml.sax.saxutils import escape


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--path', action='append', required=True, help='C# file or control directory; repeatable')
    parser.add_argument('--inventory', help='Optional JSON member inventory for review')
    parser.add_argument('--snapshot', help='Optional token fingerprints for a layout-only comparison')
    args = parser.parse_args()
    paths = [Path(value).resolve(strict=True) for value in args.path]
    source = Path(__file__).with_name('MemberLayout.cs')
    with tempfile.TemporaryDirectory(prefix='atomui-control-member-layout-') as directory:
        temporary = Path(directory)
        project = temporary / 'MemberLayout.csproj'
        project.write_text(f'''<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <RestoreSources></RestoreSources>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="{escape(str(source), {'"': '&quot;'})}" />
    <Reference Include="Microsoft.CodeAnalysis" HintPath="$(MSBuildToolsPath)/Roslyn/bincore/Microsoft.CodeAnalysis.dll" />
    <Reference Include="Microsoft.CodeAnalysis.CSharp" HintPath="$(MSBuildToolsPath)/Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll" />
  </ItemGroup>
</Project>
''')
        build = subprocess.run(
            ['dotnet', 'build', str(project), '--nologo', '--verbosity', 'quiet', '--output', str(temporary / 'out')],
            text=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, check=False,
        )
        if build.returncode:
            print(build.stdout, file=sys.stderr)
            return 2
        command = ['dotnet', str(temporary / 'out/MemberLayout.dll')]
        for path in paths:
            command.extend(['--path', str(path)])
        for name in ('inventory', 'snapshot'):
            if value := getattr(args, name):
                command.extend(['--' + name, str(Path(value).resolve())])
        return subprocess.run(command, check=False).returncode


if __name__ == '__main__':
    sys.exit(main())
