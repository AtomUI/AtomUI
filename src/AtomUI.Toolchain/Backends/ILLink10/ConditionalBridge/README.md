# Conditional-record to official TypeMap bridge

This build-time compatibility layer lets a .NET 10 application consume an existing net8 package's
format 1 conditional registration records. It converts the ABI before dependency analysis. The
stock .NET 10 ILLink or NativeAOT compiler still decides every condition and dependency. The bridge
never computes a selected set, executes application assemblies, scans source usage, or retains the
full collector as a fallback.

## Modules and ownership

- `BridgeProgram` is the controlled .NET 10 CLI entry point. Its own DLL, runtimeconfig, deps file and
  Mono.Cecil DLL must belong to the verified frozen tool inputs. Application assemblies are read
  only through PE metadata and Cecil.
- `ConditionalInputTransformer.Transform` validates an immutable input snapshot, binds definitions
  through the source-only Shared binder, and writes new isolated package/application copies. It emits
  the official TypeMap attributes and accessor, existing proxy/candidate/helper ABI, and application
  map target declarations. Original build outputs and NuGet assets remain untouched.
- `FrameworkBindingPolicy` permits an explicit full net8 framework identity to bind to a matching
  signed net10 implementation in the actual SDK runtime pack. Each source identity, target identity,
  target hash, pack identity and RuntimeList hash appears in the transformation manifest. SDK format 2
  conversion reads the frozen manifest and implementation bytes. Third-party version fallback is
  never permitted; the net8 binder keeps its default empty mapping.
- `ConditionalProtocolGate` validates mixed-input packages with an official accessor or conditional
  records; marked packages with neither protocol fail. An official empty map remains valid.
- `InitializeStep` is a pre-Mark ILLink convenience adapter. The offline transformer has no dependency
  on running ILLink and is the route used before NativeAOT compilation.

## SDK boundary

`build/AtomUI.Registration.Bridge.targets` is an explicit
automatic official-framework/input-ABI routing. Ordinary builds do not transform assemblies.
`PrepareConditionalRegistrationBridgeTask` first probes every candidate using metadata-only reads.
A pure net10 input set returns the original SDK items without requiring the compatibility tool.
Mixed inputs resolve all declared tool candidates before freezing the complete tool/input set and
launching the isolated bridge process. Raw file-bearing options are rejected unless the exact SDK
option is bound to a declared frozen input. Standard browser substitutions have this explicit binding.

The CLI requires transport format 2; the ConditionalRecord ABI stays at format 1. The transformer
writes `transformation.json` with the original snapshot hash and an original-to-transformed assembly
and PDB hash chain. The SDK consumes returned paths directly, preserving its normal compiler options.
NativeAOT uses the actual AOT implementation pack from `SetupProperties`, rather than the CoreCLR
pack entry returned earlier in framework-reference resolution. Its official ILC tool bundle and
per-invocation object/native output directories are isolated too.

`VerifyConditionalRegistrationBridgeTask` keeps three boundaries separate:

1. `analysis-input`: actual SDK assembly/additional-input paths equal the prepared set, and hashes
   still match the immutable snapshot and transformation.
2. `linked-output`: record the new formal compiler-output hashes. These are intentionally different
   from ABI-conversion input hashes. A NativeAOT receipt labels this `compiled-output`; it is not an
   independent proof of type selection.
3. `published-output`: verify the SDK's copied/deployed bytes. Browser AOT may strip IL after linking;
   the verifier records linked hashes, actual final SDK assembly hashes and deployed hashes separately,
   together with the actual final native WASM asset. It does not compare changed AOT DLL bytes to the
   pre-AOT hash.

Browser ordering is injection, ABI conversion/frozen-step remapping, then the existing official
TypeMap signature capture. The normal net10 route retains the same official backend and ordering.
The existing Browser receipt verifies pre-AOT input consumption in the nested publish; the outer
build verifies final assets returned by that nested build.

## Verification scope

The current acceptance work uses SDK 10.0.300, ILLink/NativeAOT 10.0.8, and osx-arm64 on this machine.
The Browser workload is Mono/WASM 10.0.10 with Webcil disabled for independent managed-PE verification.
Targets reject unaccepted SDK/platform/post-link combinations instead of silently falling back to
full registration. Runtime checks include precompiled net8 A/B/Off packages, transitive selection,
unused-candidate removal, and unrequested groups. These are focused compatibility fixtures, not a
claim that the complete product/platform rollout matrix has passed.
