# Windows compiler servicing assessment

Status: servicing implemented. Windows x64 Native8 matrix46/46, serviced native product/input and native Gallery routes have executed. Current-source aggregate results belong to the registered Windows specialist.

The user delegated all gap-closure decisions and prohibited further questions. This servicing step narrows changes to the vulnerable native compiler/runtime packs. ILLink8 8.0.27, ILLink10 10.0.8, SDK 10.0.300 and Avalonia remain unchanged. The separate Reactive-family migration is assessed in reactive-evaluation.md. No shared installation or cache is replaced.

## Exact inputs

| Family | Current | Candidate | Official runtime source |
| --- | --- | --- | --- |
| ILC8 | 8.0.27 | 8.0.33 | a6bde67c455f2ac219988c7a66171631090b6f65 → da7a9beabf4cccc624399d583f52f080a4c858c2 |
| ILC10 / Windows runtime | 10.0.8 | 10.0.14 | b82454cad0aaaae3db2cf18fbf2cccc36e201ccc → 6aa72daa24f66c01c3d7b6a32519e9d21396deec |

Official repository: https://github.com/dotnet/runtime. The complete rename-aware changed-file inventories are adjacent: runtime8-changed-files.csv (202 files) and runtime10-changed-files.csv (463 files). They classify every input before narrowing the compiler investigation. The local reference repository is .referenceprojects/runtime; exact tags were fetched through the machine proxy.

The [Microsoft advisory](https://github.com/advisories/GHSA-p3jp-9v83-76w9) identifies these patched versions. The ordinary Gallery publish reproduces NU1903 for ILCompiler 10.0.8; this is the configuration RED. Disabling NuGet audit, NoWarn, suppression attributes or accepting vulnerable restored assets is excluded.

## Source-backed impact trace

- ILC8: zero changed files under src/coreclr/tools, src/libraries/Common/src and src/tools/illink/src/ILLink.Shared. These are the managed recipe's source boundaries. The scanner/conditional registration integration points therefore remain byte-identical upstream; the recipe still validates the new exact commit and must rebuild its capability/hash manifest.
- Native8 runtime: InteropHelpers now passes the bounded ByValTStr capacity into PInvokeMarshal.StringToAnsiString. The latter clamps output and preserves termination for zero-capacity buffers. Source-level native interop behavior changes, so Unicode/path transport and real native consumers must execute again.
- Brotli8: DecodeBlockTypeAndLength and its literal/command/distance callers now propagate explicit decoder error statuses instead of conflating malformed switches with missing input. ProcessCommandsInternal uses the status-returning safe macro. New native packages, rather than a managed warning configuration change, carry this correction.
- ILC10: the complete tools delta has four files. TypePreinit changes switch target comparison from > to >= (preinitialization correctness); MachObjectWriter adds bounded relocation anchors (Mach-O only, outside Windows PE output); TypeValidationChecker adjusts interface variance/constraint validation (ReadyToRun only); crossgen2 normalizes DLL/EXE output extensions (ReadyToRun only). Windows NativeAOT still requires actual PE/startup and TypeMap validation; source inspection alone does not certify it.
- Windows NativeAOT10 runtime: finally-funclet handling copies nonvolatile integer registers back to REGDISPLAY before the no-GC transition. PalMinWin handles failed allocations before conversion and tolerates unsuccessful APC scheduling. Exception cleanup, interop and collection scenarios must run on the serviced runtime.
- JIT sources enter SDK-restored compiler helpers; the scanner/optimization matrix is required even when the managed adapter is unchanged.
- BCL servicing affects XML, HTTP/socket/security, date/time and synchronization code. These are possible transitive consumers of image/resource/locale/publish paths, not proof of no impact. Native product/locale/theme/input and cold-package routes are the acceptance gate. Upstream tests clarify changes but do not replace AtomUI execution.
- Mono, Apple/Unix PAL and producer CI/build files do not enter the Windows x64 managed compiler recipe. No Windows execution result is relabeled as another platform's certification.

## Implementation and acceptance

1. Service restored known ILC packs in the repository-owned import without downgrading a newer SDK pack. Default repository Windows self-contained runtime packs to the admitted patched versions. NuGet deliberately excludes package imports from restore inputs: shipped registration targets must not change runtime/compiler pack selection only during build. External applications choose runtime and compiler packs in their own project or Directory.Build.targets; the supported Native8 and bridge gates remain strict.
2. Move the exact ILC8 recipe/capability, native host gate and SDK gate together to 8.0.33. Keep the independent ILLink8 engine unchanged.
3. Admit the Windows 10.0.14 bridge pack explicitly, preserving the existing signed identity/RuntimeList/frozen-input proof. Do not widen to arbitrary runtime packs.
4. Run focused task/backend contracts, all 46 actual ILC8 cases, real native8/direct-modern/old-package routes and actual package/layout isolation gates.
5. Publish and launch the Gallery through the official script and verify the restored/compiler/runtime identities. Any unchanged ReactiveUI trim/AOT diagnostics remain separate open work.

Windows source servicing is not a global SDK or ILLink-family upgrade. Existing non-Windows evidence retains its original versions and does not establish compatibility for newly serviced native packs.
