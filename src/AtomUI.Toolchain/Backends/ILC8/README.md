# AtomUI .NET 8 NativeAOT registration backend

This module builds a private, pinned ILC8 driver on a .NET 10 managed host. It does not modify CoreLib or target runtime libraries. Product publish targets and NuGet tool delivery are integrated separately; this directory is not an automatically activated runtime dependency.

The source/tool baseline is data in `upstream.json`. The adapter uses original ILC dependency nodes. Loading candidate definitions creates no candidate roots.

## Execution paths

- With the scanner enabled, the collector request enables conditional registration edges. Selected thunk bodies participate in the actual scanner fixed point. After closure the selected collector becomes ordered, deduplicated static calls with the real builder argument. Codegen may inline those thunks; a missing standalone thunk symbol is therefore not evidence of missing behavior.
- Without the scanner, the collector has a fixed managed `calli` loop before analysis starts. Taking the generated table's address requests the group in the actual codegen graph. Conditional edges preserve addressable non-generic static thunk entrypoints. After graph closure the table emits only relocations to selected, already analyzed methods. The table header is version/pointer-width/count/reserved followed by pointer-sized entries. Absolute pointers use a loader-rebasable data section on Unix, rather than executable `__TEXT`.
- Both paths prevent preinitialization from interpreting a collector. Other static constructors remain eligible for normal preinitialization. Factory dependencies, generic consumers, exceptions, GC and platform decisions continue through ILC.

Generated thunk owners must be stateless non-generic types without a static constructor. Their exact signature is managed static void with the bound reference-type builder parameter. User controls/factories may have static constructors; this restriction applies only to generated thunk containers.

## Product contract binding

`ConditionalInputBinder` links the shared PE metadata reader, strict JSON/parser and snapshot implementation from `AtomUI.Toolchain/Common/Registration`. It validates complete identities and actual resolver bytes, normal type forwarding, package marker arguments, collector/full/stub IL, ordered thunk bodies, proxy inheritance/self-attribute/identity, and the exact builder method.

The provider normalizes `Collect -> CollectSelected` before marking. Consumed build-record attributes are excluded through a metadata policy for their exact resolved type. A marker without definitions is rejected, preventing legacy full-collection fallback.

`atomui.trimmed-switch.v1` recognizes the real Core getter semantically before specializing it for a selected publish. It admits only the fixed AppContext lookup, bool locals/constants, forward branches and the `found && value` result. Ordinary execution keeps its existing behavior. Unknown getter bodies fail validation.

The validated getter also enters the pinned official `FeatureSwitchManager` substitution path, so caller branches are folded before scanner import. Replacing the getter body alone leaves the untrimmed resolver visible to the conservative scanner. The build recipe generates a copy exposing only the substitution lookup as a protected virtual hook; the adapter returns true for that exact validated getter and delegates all other substitutions to the official implementation. Branch/EH rewriting and the preinitialization provider remain shared with the official pipeline; there is no separate reachability analyzer.

Compiler reports move from `codegen-verified` to `compiler-complete` only after the exports file is written. Each SDK invocation supplies a fresh GUID and an owned directory. Formal compilation exports a small data blob bound to that exact caller GUID; it introduces no application type or method root. `VerifyNative8RegistrationTask` checks the exact input/tool/object/export hashes and the nonce payload at its exported address. macOS uses the Mach-O dynamic export trie. Windows requires an AMD64 native PE32+ executable with an exact DATA export, bounded RVA mapping and complete payload; CLR headers, forwarding, duplicate names and malformed export tables fail. Actual compiler TargetOS/TargetArchitecture selects the format. The `linked` phase creates an immutable `linked.json` proof with `sdk-link-verified`; the linked phase cannot run again for the same invocation. The SDK captures that proof’s hash, and the `published` phase requires this caller-supplied hash plus an exact binary hash match before writing a separate `published-copy-verified` receipt. Runtime behavior and UI checks remain separate acceptance evidence.

Cleanup only touches a validated, explicit file set in the caller-owned invocation directory. Receipt JSON never grants deletion authority. Paths escaping the directory or traversing symlinks are rejected. Failed published-copy checks preserve the earlier linked proof and its source outputs. Failed final SDK transformations that occur outside these hooks do not produce a new verified receipt.

## Build interface

Use an isolated official runtime checkout and matching official native helper package. The source checkout is retained for ongoing compiler servicing work.

```sh
python3 src/AtomUI.Toolchain/Backends/ILC8/build/build_host.py \
  --source-root /absolute/runtime8 \
  --output /absolute/ilc8-output \
  --native-tools /absolute/runtime.osx-arm64.microsoft.dotnet.ilcompiler/8.0.33/tools \
  --host-rid osx-arm64
```

The command prints JSON containing `managedBundle`, `toolsPath`, `host` and `identity`. It builds only managed compiler components, reuses the official matching jitinterface/clrjit/objwriter libraries, and emits compact patches in the output's `build/driver.patch` and `build/feature-switch.patch`. Generated source copies stay in the build output; the generated source-checkout project is untracked. Original tracked Program.cs and FeatureSwitchManager.cs remain unchanged.

Supply `-p:IlcToolsPath=<toolsPath>` to the controlled publish. Formal inputs use `ATOMUI_ILC8_INPUTS=<absolute prepared inputs.json>`. `ATOMUI_ILC8_PREPARE_ROOT=<isolated directory>` freezes the actual parsed ILC input/reference set before loading modules and redirects resolution to those bytes. Formal transport is strict version 2, with hashes for all additional inputs. The SDK additionally supplies `ATOMUI_ILC8_INVOCATION_ID`, `ATOMUI_ILC8_OWNED_ROOT` and its receipt pointer; their mutable manifest/report/receipt files are private to that invocation. Parsed descriptor, substitution, satellite, direct-P/Invoke-list, MIBC and explicit JIT paths are redirected to their frozen files; adjacent symbols retain their layout. Effective parsed arguments are serialized into a create-only UTF-8 response file inside that invocation and read by the official parser in a one-time child process. A shared-read lease and exact length/hash checks protect this transport, without rereading the original mutable SDK response file or expanding the full reference set onto Windows' bounded process command line. The child executes the frozen managed/native tool bundle. This operation performs no build or restore. The two formal input modes are mutually exclusive.

The specialist verifier uses this same ConditionalRecord/input-v2/invocation transport. The former development manifest parser and environment transport have been removed; there is no alternate candidate format in the compiler.

## Maintainer bundle and SDK delivery

For packaging, build the portable managed bundle without native assets or a launcher:

```sh
python3 src/AtomUI.Toolchain/Backends/ILC8/build/build_host.py \
  --source-root /absolute/runtime8 \
  --output .artifacts/tools/registration/net8/ilc \
  --managed-only --ensure
```

`--ensure` validates the pinned source HEAD, clean tracked source and the recipe/adapter/shared-reader hashes before reuse. Source and output locks serialize generated project changes and bundle publication. A complete staging directory replaces the visible bundle under those locks. The managed payload uses AnyCPU with a RID-free deps file; host/target native libraries are selected at execution time by upstream ILC. The portable build path does not require native helper packages or execute a host launcher. Windows maintainer build evidence is recorded in the [Windows acceptance ledger](../../../../docs/superpowers/progress/2026-11-05-windows-theme-acceptance.md); it is separate from consumer delivery evidence.

`AtomUI.Registration.Native8.targets` uses the existing SDK `IlcToolsPath`, compiler environment and `CppLinker` extension points. `PrepareNative8RegistrationTask` combines the nine whitelisted managed files and capability manifest with matching helpers already restored by the SDK. It never fetches another package or builds runtime source in a consumer. Tool properties are restored after use. Controlled native objects, response files, exports and binaries use a fresh GUID directory for each invocation. The input cache remains immutable and separate; concurrent invocations cannot share mutable reports or consume each other’s self-consistent output sets. A different invocation cannot reuse a selected object from the stock SDK timestamp cache. Automatic .NET 8 routing admits SDK10.0.300/runtime8.0.33: an osx-arm64 host with an osx-arm64 or osx-x64 executable, or a Windows x64 host with a win-x64 executable. Unsupported combinations fail rather than silently select another backend.

Windows uses ASCII `.cmd` entries and private base64 configuration, preserving the SDK's arguments and exact inherited `DOTNET_HOST_PATH` without changing the shared console code page. Spaces, Unicode, `&` and `!` paths have execution coverage; percent-containing paths are explicitly rejected at the SDK expansion boundary. Filesystem path identity is Windows case-insensitive with safe extended DOS/UNC prefix normalization; protocol and assembly identities remain case-sensitive. The actual compiler's mapped input must match every frozen file byte. Only an exact OS-page-rounded, all-zero mapping tail is admitted.

Until content-aware SDK incremental inputs are wired for every option, each compatibility publish recompiles native code. Immutable input/tool caches are reused, but the SDK's narrower timestamp input set cannot skip the backend. This is an explicit initial delivery cost.

## Focused verification

```sh
python3 src/AtomUI.Toolchain/Backends/ILC8/verification/verify_backend.py \
  --compiler-output /absolute/ilc8-output
```

This opt-in, heavyweight mechanism verifier creates temporary fixture projects. It builds a minimal Core ABI fixture and two separate component DLLs before consumption. Each component carries real, hashed ConditionalRecord attributes and the exact collector/thunk/proxy templates; the second assembly preserves independent group gating under the one-package-per-assembly ABI. Each publish freezes actual compiler inputs and tools, executes the frozen child, and verifies its invocation, compiler-complete report, object and exports hashes before checking selected/unselected native methods, decoding final Mach-O or COFF/PE table relocations, and running the executable. It covers direct/token/style/array/typeof/generic uses, transitive dependencies, cycles, group gating, OR deduplication, same-name assembly separation, GC-tracked builder arguments, and normal/generic-cctor exception propagation. The generic scenario retains the original `new GenericControl<Payload>()` consumer and uses its non-generic `Payload` type as the ABI-admitted condition, reached by the real generic dependency closure without an added root. Scanner and optimization are separate axes. `--rids osx-x64` adds a cross-architecture run when the normal platform execution environment supports it.

Successful default runs delete runnable fixture/build outputs and retain compact logs. Failed runs retain their workspace for diagnosis. `--work-root` and `--keep-artifacts` support active integration work; retained artifacts must be cleaned when that work finishes.

The verifier is intentionally not part of the global unit-test run: real compiler marking, native relocation, GC/calli and preinitialization contracts cannot be independently checked by a mock graph or ordinary compilation. Run it for adapter/compiler/runtime updates and relevant ABI changes. It adds no default-suite tests. Fixture templates are private verification inputs, not package assets or runtime examples.

## Current boundaries

Validated mechanism/runtime combinations include a .NET10.0.8 managed host, official8.0.27 target assets, macOS arm64/x64 and Windows x64. Windows ran all46 original case/scanner/optimization rows; its22 no-scanner tables and36 ordered pointers were checked against COFF ADDR64, file-backed executable PE destinations and DIR64 loader relocations. Use `--rids win-x64` on Windows; the verifier uses normal SDK publish targets and a verifier-only linker map. Product output verification has no map/PDB dependency. Package/cold-consumer and final cleanup status belongs to the Windows ledger. Linux execution, other revisions/architectures and release-wide acceptance remain separate gates; portable managed IL alone proves no platform support.

A native optimized no-scanner graph may remove a boxed enum used only by `GC.KeepAlive`. Official .NET 10 TypeMap has the same observed result. Such a dead boxing is not a guaranteed condition root. Keep the negative case and use a genuinely observable enum use for a positive liveness test; do not compensate by rooting every IL-mentioned enum.

`TokenKey` and own `TokenKind` enums also have different resource-key domains. A raw TokenKey is not automatically the framework's own resource key; its extension performs the conversion. Verification must not invent a resource contract to force a selection test to pass.

Servicing update (2026-11-06): Windows x64 completed the unchanged46-row matrix on official8.0.33 target assets and the rebuilt exact-source managed bundle. The earlier8.0.27 macOS results above remain historical; they do not certify8.0.33 on macOS. ILLink8 stays independently pinned to8.0.27. See the Windows servicing audit for exact revisions and runtime/product outcomes.
