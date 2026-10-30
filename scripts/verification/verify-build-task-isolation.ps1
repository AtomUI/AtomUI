param(
    [string]$Configuration = 'Debug',
    [string]$PackageFile,
    [switch]$BuildPackage
)

$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$toolSource = Join-Path $repoRoot ".artifacts/bin/$Configuration/toolchain/tasks/net10.0"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("atomui-task-isolation-" + [Guid]::NewGuid().ToString('N'))
$toolRoot = Join-Path $testRoot 'tools with spaces'

function Escape-Xml([string]$value) {
    return [Security.SecurityElement]::Escape($value)
}

try {
    $featureRoot = Join-Path $repoRoot 'build'
    if ($BuildPackage) {
        $cleanBin = Join-Path $testRoot 'clean-bin'
        $cleanPackages = Join-Path $testRoot 'clean-packages'
        & dotnet pack (Join-Path $repoRoot 'src/AtomUI.Generator/AtomUI.Generator.csproj') `
            -c $Configuration --no-restore "-p:OutputPathWithoutFramework=$cleanBin" `
            -o $cleanPackages -v:minimal -nodeReuse:false
        if ($LASTEXITCODE -ne 0) { throw 'Packing into an empty toolset directory failed.' }
        $PackageFile = (Get-ChildItem -LiteralPath $cleanPackages -Filter 'AtomUI.Generator.*.nupkg').FullName
    }
    if ($PackageFile) {
        $packageRoot = Join-Path $testRoot 'package'
        [IO.Compression.ZipFile]::ExtractToDirectory([IO.Path]::GetFullPath($PackageFile), $packageRoot)
        $toolSource = Join-Path $packageRoot 'tools/net10.0'
        $featureRoot = Join-Path $packageRoot 'buildTransitive'
    }
    New-Item -ItemType Directory -Path $toolRoot -Force | Out-Null
    foreach ($toolFile in @('AtomUI.Build.Tasks.dll', 'AtomUI.Build.Tasks.deps.json', 'AtomUI.Build.Tasks.runtimeconfig.json')) {
        Copy-Item -LiteralPath (Join-Path $toolSource $toolFile) -Destination $toolRoot
    }
    Copy-Item -LiteralPath (Join-Path $toolSource 'Microsoft.Build.Framework.dll') -Destination $toolRoot
    Get-ChildItem -LiteralPath $toolSource -Filter 'System.*.dll' | Copy-Item -Destination $toolRoot
    $assembly = Escape-Xml (Join-Path $toolRoot 'AtomUI.Build.Tasks.dll')
    $sourceAssembly = Escape-Xml (Join-Path $toolSource 'AtomUI.Build.Tasks.dll')
    $themeTargets = Escape-Xml (Join-Path $featureRoot 'AtomUI.ThemeAssets.targets')
    $localizationTargets = Escape-Xml (Join-Path $featureRoot 'AtomUI.Localization.targets')
    $escapedToolRoot = Escape-Xml $toolRoot
    '<ResourceDictionary xmlns="https://github.com/avaloniaui" />' |
        Set-Content -LiteralPath (Join-Path $testRoot 'theme.axaml') -Encoding utf8
    $project = Join-Path $testRoot 'verify.proj'
    @"
<Project>
  <PropertyGroup>
    <AtomUIBuildTasksAssembly>$assembly</AtomUIBuildTasksAssembly>
  </PropertyGroup>
  <ItemGroup>
    <FixtureTheme Include="theme.axaml"><Link>Themes/Fixture.axaml</Link></FixtureTheme>
  </ItemGroup>
  <Import Project="$themeTargets" />
  <Import Project="$localizationTargets" />
  <Target Name="Verify">
    <AtomUI.Build.Tasks.GenerateThemeAssetWrappersTask TaskAssembly="$assembly" DotNetPath="`$(DOTNET_HOST_PATH)"
        ThemeAssets="@(FixtureTheme)" OutputDirectory="generated%253Bdir" AssemblyName="Fixture" GeneratedCodePath="generated%253Bdir/first.cs">
      <Output TaskParameter="GeneratedAssets" ItemName="GeneratedAssets" />
    </AtomUI.Build.Tasks.GenerateThemeAssetWrappersTask>
    <Error Condition="'@(GeneratedAssets)' == ''" Text="Theme input was lost across the process boundary." />
    <Error Condition="!Exists('%(GeneratedAssets.Identity)')" Text="Output item identity was corrupted." />
    <Error Condition="'%(GeneratedAssets.Link)' == ''" Text="Output item metadata was lost." />
    <AtomUI.Build.Tasks.ExportLanguageTemplatesTask TaskAssembly="$assembly" DotNetPath="`$(DOTNET_HOST_PATH)"
        SourceFiles="" TargetLanguage="ja-JP" OutputRootDirectory="templates">
      <Output TaskParameter="ExportedFiles" ItemName="ExportedFiles" />
    </AtomUI.Build.Tasks.ExportLanguageTemplatesTask>
    <Error Condition="'@(ExportedFiles)' != ''" Text="Empty item output was corrupted across the worker boundary." />
    <!-- Force an overwrite while the same MSBuild process is still alive. -->
    <Copy SourceFiles="$sourceAssembly" DestinationFiles="$assembly" SkipUnchangedFiles="false" Retries="0" />
    <CallTarget Targets="VerifyAgain" />
    <RemoveDir Directories="$escapedToolRoot" />
    <Error Condition="Exists('$escapedToolRoot')" Text="Task toolset remained locked after execution." />
  </Target>
  <Target Name="VerifyAgain">
    <AtomUI.Build.Tasks.GenerateThemeAssetWrappersTask TaskAssembly="$assembly" DotNetPath="`$(DOTNET_HOST_PATH)"
        ThemeAssets="" OutputDirectory="generated" AssemblyName="Fixture" GeneratedCodePath="generated/second.cs" />
    <Error Condition="!Exists('generated/second.cs')" Text="Second invocation did not execute." />
  </Target>
</Project>
"@ | Set-Content -LiteralPath $project -Encoding utf8
    & dotnet msbuild $project -target:Verify -nodeReuse:false -maxCpuCount:1 -nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build task isolation regression failed.' }
    Write-Host 'PASS: repeated task execution, forced overwrite and toolset deletion in a live MSBuild process.'

    # A controlled worker holds an exclusive file handle until the adapter cancels
    # it. This verifies cancellation against a running process, not just a flag.
    $blocker = Join-Path $testRoot 'blocking-worker.ps1'
    @'
$request = Get-Content -LiteralPath $args[0] -Raw | ConvertFrom-Json
$signal = $request.Properties.GeneratedCodePath
$handle = [IO.File]::Open($signal + '.lock', 'Create', 'Write', 'None')
try {
    [IO.File]::WriteAllText($signal, [string]$PID)
    [Threading.Thread]::Sleep(60000)
}
finally { $handle.Dispose() }
'@ | Set-Content -LiteralPath $blocker -Encoding utf8
    $harness = Join-Path $testRoot 'CancellationHarness.cs'
    $harnessSource = Get-Content -LiteralPath (Join-Path $featureRoot 'AtomUI.Build.Tasks.Process.cs') -Raw
    $harnessSource += @'

public sealed class ScalarProbe : AtomUI.Build.Tasks.IsolatedBuildTask
{
    public int Input { get; set; }
    [Output] public int Count { get; set; }
}

public sealed class VerifyCancellation : Microsoft.Build.Utilities.Task
{
    public string Worker { get; set; }
    public string Host { get; set; }
    public string Signal { get; set; }
    public override bool Execute()
    {
        var task = new AtomUI.Build.Tasks.GenerateThemeAssetWrappersTask
        {
            BuildEngine = new QuietEngine(), TaskAssembly = Worker, DotNetPath = Host,
            GeneratedCodePath = Signal, OutputDirectory = "unused", AssemblyName = "Fixture"
        };
        var execution = System.Threading.Tasks.Task.Run(() => task.Execute());
        try
        {
            if (!System.Threading.SpinWait.SpinUntil(() => System.IO.File.Exists(Signal), 10000))
                throw new Exception("Worker did not acquire its file handle.");
            task.Cancel();
            if (!execution.Wait(TimeSpan.FromSeconds(10)) || execution.Result)
                throw new Exception("Cancelled worker did not finish with failure.");
            using (System.IO.File.Open(Signal + ".lock", FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
            return true;
        }
        finally { task.Cancel(); execution.Wait(TimeSpan.FromSeconds(10)); }
    }
    private sealed class QuietEngine : IBuildEngine
    {
        public bool ContinueOnError { get { return false; } }
        public int LineNumberOfTaskNode { get { return 0; } }
        public int ColumnNumberOfTaskNode { get { return 0; } }
        public string ProjectFileOfTaskNode { get { return "cancellation.proj"; } }
        public void LogErrorEvent(BuildErrorEventArgs e) { }
        public void LogWarningEvent(BuildWarningEventArgs e) { }
        public void LogMessageEvent(BuildMessageEventArgs e) { }
        public void LogCustomEvent(CustomBuildEventArgs e) { }
        public bool BuildProjectFile(string name, string[] targets, IDictionary properties, IDictionary outputs) { return false; }
    }
}
'@
    [IO.File]::WriteAllText($harness, $harnessSource)
    $harnessXml = Escape-Xml $harness
    $blockerXml = Escape-Xml $blocker
    $signalXml = Escape-Xml (Join-Path $testRoot 'worker-started')
    $hostXml = Escape-Xml (Join-Path $PSHOME $(if ($IsWindows) { 'pwsh.exe' } else { 'pwsh' }))
    $scalarWorker = Join-Path $testRoot 'scalar-worker.ps1'
    @'
$request = Get-Content -LiteralPath $args[0] -Raw | ConvertFrom-Json
if ($request.Properties.Input -ne '2147483646') { throw 'Integer input conversion was not invariant or exact.' }
'{"Success":true,"Properties":{"Count":"2147483647"},"Items":{},"Diagnostics":[]}'
'@ | Set-Content -LiteralPath $scalarWorker -Encoding utf8
    $scalarWorkerXml = Escape-Xml $scalarWorker
    $cancelProject = Join-Path $testRoot 'cancel.proj'
    @"
<Project>
  <UsingTask TaskName="VerifyCancellation" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="`$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll">
    <Task><Code Type="Class" Language="cs" Source="$harnessXml" /></Task>
  </UsingTask>
  <UsingTask TaskName="ScalarProbe" TaskFactory="RoslynCodeTaskFactory" AssemblyFile="`$(MSBuildToolsPath)/Microsoft.Build.Tasks.Core.dll">
    <Task><Code Type="Class" Language="cs" Source="$harnessXml" /></Task>
  </UsingTask>
  <Target Name="Verify">
    <ScalarProbe TaskAssembly="$scalarWorkerXml" DotNetPath="$hostXml" Input="2147483646">
      <Output TaskParameter="Count" PropertyName="ScalarCount" />
    </ScalarProbe>
    <Error Condition="'`$(ScalarCount)' != '2147483647'" Text="Integer output conversion was not exact." />
    <VerifyCancellation Worker="$blockerXml" Host="$hostXml" Signal="$signalXml" />
  </Target>
</Project>
"@ | Set-Content -LiteralPath $cancelProject -Encoding utf8
    & dotnet msbuild $cancelProject -target:Verify -nodeReuse:false -maxCpuCount:1 -nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build task cancellation regression failed.' }
    Write-Host 'PASS: integer input/output conversion and cancellation waits for worker termination and releases its exclusive file handle.'
}
finally {
    $resolvedTestRoot = [IO.Path]::GetFullPath($testRoot)
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if (!$resolvedTestRoot.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -or
        !(Split-Path -Leaf $resolvedTestRoot).StartsWith('atomui-task-isolation-')) {
        throw "Unexpected cleanup path: $resolvedTestRoot"
    }
    if (Test-Path -LiteralPath $resolvedTestRoot) {
        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force
    }
}
