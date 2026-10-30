# ILLink 8 conditional registration mechanism

This build-only module implements the managed-linker part of the
[approved design](../../docs/superpowers/specs/2026-10-05-aot-dependency-analysis-design/overview.md).
It runs on a .NET 10 tool host and binds exactly to ILLink 8.0.27, runtime source commit
`a6bde67c455f2ac219988c7a66171631090b6f65`. It does not change the target runtime.
The module is a packaged build-only tool selected automatically for admitted .NET 8 managed trimming. Target-framework routing and capability admission remain separate.
Its mechanism evidence does not imply that the complete product/platform rollout matrix has passed.

## Boundaries

`ConditionalRegistrationBackend.Install` accepts validated Cecil definitions from the same
`LinkContext`. The input adapter owns metadata/schema, complete identity and forwarding resolution,
collector-template validation, input snapshots and declaration isolation. The mechanism does not
discover candidate assemblies or read application source/usage records.

A group has full, selected and forwarding collectors. Its fragments carry a registration method,
the common emitter's collection-order key, and one or more resolved type conditions. Thunks and
collectors use the same ordinary builder parameter and belong to the same assembly. Distinct
collector roles and thunk/collector separation prevent accidental recursive dispatch.

Installation validates all definitions before mutation, rewrites the forwarding collector away
from its full branch, and replaces the selected stub with an analysis-only empty body. The original
full collector becomes unreachable through this registration path. Registration methods enter the
actual marker through `MarkMethod`; their virtual calls, constructors, resources and further type
references are handled by the original linker work queues.

The formal net8 input step also validates `ControlRegistrationRuntime.get_IsTrimmed` against
`atomui.trimmed-switch.v1`: exact Core/CoreLib authority and signature, the fixed switch key, one
`AppContext.TryGetSwitch(string, bool&)` call, a bounded boolean-only forward-flow template, and all
four `found/enabled` results equal to `found && enabled`. Only the publish view becomes `true`.
Ordinary Core8 files and execution retain their original getter; the synthetic mechanism API and
net10 feature-switch path do not use this normalizer. Unknown getter bodies fail before Mark.

## Condition notifications

The pinned source has four sites writing relevant-to-variant-casting state and one instantiation
site. The adapter observes each actual operation, rather than polling candidate types:

| Official operation | Observation |
| --- | --- |
| Instantiation requirements | `MarkRequirementsForInstantiatedTypes` |
| Reflection visibility/dataflow | `MarkTypeVisibleToReflection` |
| Generic type arguments | After `MarkType` completes `GetOriginalType` / private `MarkGenericArguments` |
| Generic method arguments | After `MarkMethod` completes `GetOriginalMethod` / private `MarkGenericArguments` |
| Array constructor, including multidimensional arrays | After the array-constructor branch of `MarkMethod` |
| `newarr` and eligible `isinst` | Actual `MarkInstruction` callback |

Generic notifications consult only the arguments of the operation just completed and their official
semantic state. This catches a type becoming relevant after its one-shot type-mark callback ran.
There is no `DoAdditionalProcessing` candidate scan.

As in the .NET 10 external TypeMap initializer, explicit types already rooted before pending marks
are processed are captured once after all mark-handler initialization. This initialization snapshot
uses `IsMarked`; subsequent metadata-only reachability never activates a condition. The initial
snapshot is distinct from treating every marked type as relevant.

A group becomes requested when its selected collector is processed. Conditions arriving before or
after that request have the same result. Multiple conditions select a fragment once; different groups
remain independent. Unrooted cycles receive no starting edge.

## Materialization and evidence

After the base MarkStep finishes, every selected thunk must be marked, processed and have Parse
action. Selected collectors receive only direct calls to those already analyzed methods, ordered by
the common collection-order key. No helper or dependency is added after marking. Sweep and normal
linker output then continue.

The optional callback returns group/selection/notification evidence for this stage only. It is not
a final-output or consumed-publish receipt. The build adapter must independently reread outputs,
check identities and leftovers, and verify the SDK consumes the verified artifacts.

## Verified scope and remaining work

Temporary real-publish fixtures exercised direct, token/style surrogate conditions, reflection,
type checks, generic type/method/field/signature use, late generic attributes and fragment-induced
generic use, one- and two-dimensional arrays, an initial XML root, multiple assemblies/groups,
both event orders, empty groups, shared-fragment OR, rooted/unrooted cycles and builder-parameter
dispatch. Static-only, signature-only and ordinary attribute metadata do not select fragments.
Independent Cecil checks covered selected/unselected methods/types and removal of full collectors.
Removing either generic observation hook from temporary copies caused the fixtures to fail.

These are engine-mechanism results, not complete product acceptance. Real generated ABI ingestion,
AtomUI lifecycle/resource/AXAML behavior, all supported SDK/RID combinations, tool delivery,
incremental/parallel publish integrity and final receipts have separate owners and validation gates.
This module does not establish NativeAOT, Browser or complete .NET 10 parity.
