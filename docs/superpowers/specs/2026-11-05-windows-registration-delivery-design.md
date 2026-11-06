# Windows x64 registration delivery design

## Intent and authority

Complete Windows tasks for feature/custom-theme at initial HEAD d3fef28a2. The user explicitly included the currently unsupported Windows .NET 8 NativeAOT and modern consumption of net8-only packages on 2026-11-05. The Windows acceptance ledger is `docs/superpowers/progress/2026-11-05-windows-theme-acceptance.md`; original CT25 approvals and failures retain their independent scope.

This design addresses the new registration delivery support. The user approved it on 2026-11-05: “认可设计，按方案实施（推荐）”. Existing Windows module/native-window/consumer acceptance proceeds independently under the current CT25 plan. Implementation and verification proceed within that approval; the Windows ledger records subsequent evidence-based integration decisions.

## Constraints and support matrix

- Host: Windows x64; SDK 10.0.300; managed tool host .NET 10.
- Native8: official target runtime/native helpers 8.0.27 and pinned compiler source a6bde67c455f2ac219988c7a66171631090b6f65; target win-x64 executable.
- Modern old-package bridge: target net10.0/win-x64/runtime10.0.8; managed full trim or NativeAOT single-module executable.
- Keep existing macOS and Browser routes. Other Windows architectures, SDK/runtime revisions, NET8 Browser, bridge R2R/single-file/native libraries and arbitrary platform combinations remain separate work.
- Keep ConditionalRecord format1, frozen input format2, the existing registration lifecycle, real engine dependency graphs, compiler/helper identity checks and fixed tool whitelist.
- No application runtime/toolchain dependency, global SDK/NuGet/native-cache replacement, new public runtime API, broad rooting, swallowed validation failures, relaxed pixel tolerances, commits/push/releases or full regression.
- New validation is temporary by default; clean it before exact final module verification. The existing specialist verifier remains an on-demand mechanism gate.

## Approaches

The proposed approach extends the existing SDK extension points and platform adapters. Windows SDK already invokes `ilc` and `CppLinker` via Exec and supplies `.obj/.def/.exe` suffixes. A generated `.cmd` entry can therefore delegate to the existing managed driver, while official SDK targets continue to prepare response files, link and copy the application. This has the smallest integration surface and preserves existing invocation-bound evidence.

A separate native launcher/application project adds another platform binary and package identity without providing a missing SDK capability. Routing a net8 application through ILC10 changes its target/runtime contract. Neither alternative meets the same goal with lower cost. The choice of thin Windows entries is the agent's proposal for review, not an already-approved implementation detail.

## Native8 host and invocation ownership

`PrepareNative8RegistrationTask` adds exactly Windows x64 host/target and checks the corresponding SDK-restored `runtime.win-x64.microsoft.dotnet.ilcompiler/8.0.27` package. The controlled host includes `jitinterface_x64.dll`, `objwriter.dll` and `clrjit_win_x64_x64.dll`, plus the existing portable managed whitelist/capability manifest. All bytes remain fingerprinted and immutable.

Generate `tools/ilc.cmd` and `tools/native-linker.cmd` only for Windows. Use `setlocal DisableDelayedExpansion`, individually quoted literal paths, escaped literal percent signs and unmodified SDK `%*` arguments. Capture exit status outside parenthesized expansion. Keep existing Unix launchers. Windows launcher/path behavior is tested through the actual SDK Exec path, including spaces, Unicode and shell metacharacters supported by that SDK boundary; reject or explicitly report unsupported paths rather than silently misexecute them.

Add a private managed driver mode for native linking. It receives explicit invocation root/id/object/exports/binary and the real linker path captured after SDK `SetupOSSpecificProps`. `ProcessStartInfo.ArgumentList` passes original SDK arguments, including its response-file argument, without reparsing/reconstructing linker options. Preserve standard output/error and the original exit code. On start failure, process failure or nonzero exit, use the existing explicit caller-owned cleanup set; receipt JSON never grants deletion authority. Preserve immutable linked proofs and their source outputs during later published-copy failures.

Use OS-aware path equality for filesystem ownership, deduplication and alias prevention (Windows OrdinalIgnoreCase, Unix Ordinal). Protocol values, assembly identities, symbol names and hashes remain case-sensitive. Reject junction/symlink traversal and paths outside the caller's owned root. Each compilation retains a fresh GUID and cannot share mutable reports or timestamp-reused native objects with another invocation.

## Windows compiler and final-image proof

The existing read-only nonce data blob remains caller-bound and introduces no application type/method root. At Windows codegen the adapter asks the official exports writer for its nonce as a DATA export and verifies the generated `.def` entry before `compiler-complete`. Record the real target OS/architecture so output-format selection is tied to compilation, not the verifier's host.

Add an internal native PE verifier beside the existing Mach-O verifier. Require AMD64 PE32+ native executable without a CLR header. Read the export directory, all name/ordinal/function-RVA bounds and the unique exact nonce name. Reject malformed/truncated tables, duplicate nonce names, invalid ordinals, forwarded exports, out-of-file or ambiguous mappings and incorrect payload. Resolve the export RVA through the image's sections and compare the complete nonce payload at that address; a string search is insufficient.

Keep input/tool/object/exports hashes, actual frozen-host identity, immutable `sdk-link-verified` receipt and separate `published-copy-verified` receipt. Published validation must match both the previously captured linked receipt hash and original executable bytes. Never authenticate a modified final executable by rerunning the linked phase.

## Windows specialist mechanism verification

Extend the existing on-demand `verify_backend.py` with a Windows x64 branch and `.exe` execution. Preserve scanner/no-scanner, existing optimization axes, original candidate groups, real generic dependency closure, GC-tracked builder arguments, preinitialization and exception checks.

For Windows, use SDK linker map output only in this verifier. Parse actual COFF symbols/sections/relocations (including bigobj/aux/long names). In no-scanner output validate the header and ordered AMD64 ADDR64 relocations to selected thunks; require unused targets absent. Resolve final table/method addresses via the map, verify actual PE pointer values and corresponding DIR64 base relocations. Scanner code may inline selected thunks; standalone thunk symbols are not universally required. Product output proof does not acquire a map/PDB dependency.

## Modern bridge inputs and runtime binding

Extend `AtomUI.Registration.Bridge.targets` only for the matrix above. Pure net10 inputs still bypass the bridge; only validated old records activate it. Keep existing unsupported transformation and raw compiler-argument restrictions.

`FrameworkBindingPolicy` recognizes exact supported SDK runtime-pack IDs using NuGet's case-insensitive package-ID comparison, including Windows managed and NativeAOT packs. Keep exact version/RuntimeList/implementation-assembly identities and signed 8→10 framework mapping; do not accept arbitrary runtime prefixes.

Windows ILC references include native PE DLLs. Reuse strict PE classification at the preparation and analysis-consumption boundaries: managed PE with an assembly manifest joins the existing assembly identity snapshot; genuine native PE is admitted only in the NativeAOT SDK reference input set and frozen as an additional reference input. Preserve original SDK metadata and remap the original compile/reference/satellite item sets to their frozen paths. Actual analysis verifies exact managed identities plus exact native frozen reference paths/hashes. Missing, mutated, cache-original or foreign native inputs fail. Corrupt PE, CLR-header-without-metadata and managed modules without assembly manifests remain errors.

Do not classify by directory or RuntimeList `Type`: the Windows NativeAOT CoreLib is marked Native in that list yet contains managed metadata. Old Core remains in the conversion manifest and undergoes the existing strict getter/template and actual CoreLib10 normalization. Original package/cache DLLs remain immutable.

Retain bridge analysis-input → native compile/link → CopyNativeBinary/deployment verification. Windows final proof is taken after the SDK replaces the temporary apphost with the native executable. This proof does not replace actual execution or unused-target removal checks.

## Evidence and test plan requirements

First demonstrate current rejection/failures with actual Windows SDK inputs. Temporary internal tests go under the owning test project's `Temporary/WindowsRegistrationDelivery/`; standalone consumer/driver projects live in the owned system-temp session.

Native8 cases: real host preparation, compiler `.def` completion, valid linked/published PE; malformed exports/forwarders/ordinals/RVAs/machine/payload; object/export/input or proof mutation; foreign invocation; duplicate mutable invocation; compiler parse failure and linker exit23 cleanup with external sentinel; immutable linked proof preserved after copy failure; path casing/alias and junction escape. Run the real scanner and no-scanner compiler, not a simulated graph.

Bridge cases: actual lowercase Windows runtime pack positive and wrong-package/version negatives; old managed assemblies plus real native SDK PE reference; precise frozen/consumed native paths and mutation/missing/original-path refusal; corrupt managed PE rejection; old Core8 with real CoreLib10 positive and unknown-getter/wrong-BCL negatives; pure modern path bypass.

Prepare the portable compiler through the existing maintainer entry, then normally build/pack the 14 RuntimeProduct/Analyzer packages. Check real package layout, DLL TFMs, imports, tools and worker lifetime. Create net8-only specialist packages through normal pack without editing ZIP/cache; retain canonical dual-TFM packages separately.

Use isolated initially empty NuGet package/HTTP caches and force AtomUI packages to the local feed. Restore once with the actual publish matrix, then publish `--no-restore`. Execute Windows net8 ILLink8, ILC8 scanner/no-scanner, direct modern trim/AOT and modern old-package trim/AOT with real template/theme/scope/JSON/input-retention/unload assertions. Check actual IsTrimmed, unused target removal and each relevant input/linked/deployment proof. No single headless consumer or native fixture result substitutes for all these routes.

Clean temporary sources, helpers, media, native drivers and their processes; execute final exact changed-module gates and diff validation. Synchronize formal backend/routing/contract documentation and the one Windows ledger. Completion requires explicit runtime and proof evidence, not compilation or a removed platform guard.

## Review focus

1. Windows shell argument/path handling must preserve SDK semantics and cannot launch an unintended command.
2. PE bounds/export mapping must reject a nonce found only in unrelated bytes or a forwarded export.
3. Cleanup authority and path equality must neither erase another invocation nor allow a case/junction alias escape.
4. Native references must be frozen and actually consumed; genuine CLR assemblies cannot be skipped as native.
5. Tests and package preparation must identify the current adapter/tool bytes, and prior Mac/Linux/old Windows evidence cannot certify them.
