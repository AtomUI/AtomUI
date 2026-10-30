# Supported ILLink input audit

The TypeMap signature follows the actual ILLink 10.0.8 `_RunILLink` task boundary under SDK 10.0.300. The signature is build evidence, never application registration input. Unknown toolchain identities are still rejected.

| Actual task input | Signature representation |
|---|---|
| AssemblyPaths | Actual identity/content hashes, ordered identities and every consumed item metadata field |
| ReferenceAssemblyPaths | Complete-identity shadow normalization, remaining ordered paths/content, input PDBs |
| RootAssemblyNames | Ordered identities plus RootMode; existing path-valued roots also hash their files |
| RootDescriptorFiles | Ordered paths plus content hashes |
| TrimMode / DefaultAction | Named scalar values; per-assembly overrides below |
| RemoveSymbols / PreserveSymbolPaths | Named scalar values; original PE and available sidecar PDB inputs are hashed |
| BeforeFieldInit / OverrideRemoval / UnreachableBodies / UnusedInterfaces / IPConstProp / Sealer | Named scalar values |
| FeatureSettings / CustomData | Ordered identities and Value metadata |
| KeepMetadata | Ordered identities |
| Warn / NoWarn / TreatWarningsAsErrors / WarningsAsErrors / WarningsNotAsErrors / SingleWarn | Named scalar values; only NoWarn is canonicalized as the actual IL warning-code set |
| CustomSteps | Ordered DLL paths, Type, BeforeStep, AfterStep, assembly-reference dependency closure and available deps/runtimeconfig files; deps-declared runtime/native files are included when present beside the tool |
| OutputDirectory | Absolute signature output directory |
| DumpDependencies / DependenciesFileFormat | Named scalar values |
| ExtraArgs | Complete raw argument string plus referenced input-file content as below |
| ToolExe / ToolPath | Raw effective SDK values plus the resolved host path/content; same SDK host fallback is applied before capture |
| ContinueOnError | Fixed ErrorAndContinue policy recorded; ExitCode is an output, not an input |

Assembly metadata consumed by the supported LinkTask is exactly: TrimMode, IsTrimmable, BeforeFieldInit, OverrideRemoval, UnreachableBodies, UnusedInterfaces, IPConstProp, Sealer, TrimmerSingleWarn. Unrelated packaging metadata is not a linker input. Item ordering is retained; no root/custom-step/assembly option is omitted for no-op performance.

ExtraArgs file inputs covered by the supported driver are `-x`, `-reference`, existing file-valued `-a`, `--substitutions`, `--link-attributes` including its `@file-list`, and `--custom-step` with its tool dependencies. Search-directory `-d` membership and candidate DLL/EXE/WINMD content are hashed. Output-only options such as dependency output, assembly lists and P/Invoke reports remain captured as arguments but their generated files are not treated as inputs. Response-file quoting is parsed as ILLink parses it, without a shell. Custom steps whose opaque configuration or dynamic dependencies cannot be inferred from these standard inputs must declare those files through `AtomUITypeMapAdditionalLinkInput`. Ordinary AtomUI consumers need no extra items.

The actual linker DLL, ILLink.Tasks DLL, supported SDK target file, host, backend bundle, worker bundle and WASM task identity/content remain bound. No timestamps substitute for content hashes. The existing receipt/input/output/PDB/actual-consumption gates remain mandatory.

## Proven NoWarn equivalence

WASM outer compilation appends compiler warnings 1701/1702/8002 to NoWarn, while nested publish need not. The supported Driver's `ProcessWarningCodes` splits on comma, semicolon and space; trims; accepts only case-sensitive `IL` followed by a parsable ushort; and `NoWarn.UnionWith` consumes the result as a set. The signature therefore stores that set sorted numerically. Duplicate/order-equivalent IL codes and ignored compiler diagnostics are equivalent; changed valid IL codes still invalidate. This changes only the signature representation, never the actual SDK NoWarn property or emitted linker arguments.

This audit is checked by `verify_link_signature.py` against the real preparation target and by focused task tests. Real publish evidence must additionally show root/symbol/optimization behavior and unchanged no-op reuse; preparation-only tests do not claim runtime acceptance.
