---
name: atomui-control-documentation
description: Use when creating, completing, splitting, reviewing, or synchronizing AtomUI control documentation under docs/controls, including overview.md, implementation.md, token.md, changelog.md, topic design documents, Gallery API/Token/ShowCase alignment, or documentation impact from control API, theme, behavior, and architecture changes.
---

# AtomUI Control Documentation

## Core Rule

Treat [`docs/engineering/contributing/control-documentation-guidelines.md`](../../../docs/engineering/contributing/control-documentation-guidelines.md)
as the canonical documentation contract. Read it before editing. Do not copy its full rules into control documents or this skill.

Also read:

- `docs/engineering/development/control-development-guidelines.md` for API, theme, template, file-layout, and compatibility boundaries.
- `docs/engineering/contributing/mobile-documentation-guidelines.md` when the target is under `docs/controls/mobile` or the task concerns `AtomUI.Mobile.Controls`.
- The target control's existing `overview.md`, `implementation.md`, `token.md`, `changelog.md`, source, Themes, tests, and Gallery surface.
- `docs/engineering/development/aot-programming-guidelines.md` when the design involves reflection, dynamic discovery, binding paths, generators, or NativeAOT.

## Classify the Document

Choose the document by responsibility before writing:

| Document | Owns | Must not become |
| --- | --- | --- |
| `overview.md` | Current design positioning, public contract, state/behavior model, visual/theme model, compatibility, navigation | Implementation walkthrough or API dump |
| `implementation.md` | Current source ownership, composition, data/state flow, lifecycle, algorithms, resource/performance/AOT boundaries, maintenance invariants | User guide, design history, or private-method catalog |
| `<topic>-design.md` | One stable cross-cutting design spanning model/API, strategy, architecture, Template, algorithm, compatibility, and verification | Issue analysis, ADR, option comparison, or implementation plan |
| `token.md` | Control-specific Token semantics, categories, family impact, compatibility, validation | Generated Token table or runtime state model |
| `changelog.md` | Dated design/API/theme/Token/implementation-structure changes | Current design explanation or release changelog |

Create a topic design document only when the canonical guideline's creation conditions are met. Keep a short topic in
`overview.md` or `implementation.md`.

## Required Audit Gate

Before editing, establish this fact map from the repository:

```text
Control:
Task: create / update / split / review / synchronize
Package and namespace:
Source and Themes directories:
Existing control docs:
Public API and defaults:
Template Parts / pseudo-classes / ControlTheme keys:
State, data, lifecycle, and owner model:
Token scope and consumers:
Gallery API / Token / ShowCase coverage:
Tests and platform/AOT validation:
Platform evidence status (iOS / Android / Release / publication):
Selected document type(s):
Contradictions or missing evidence:
```

Do not edit until the selected document type and evidence are clear. For a narrow change, keep the audit short; for a new or
complex control, inspect the full control family and comparable controls.

## Evidence Order

Use these sources in order and resolve conflicts explicitly:

1. Public/protected API registrations, CLR wrappers, events, interfaces, defaults, and control source.
2. `Themes/` ControlTheme/ControlTemplate structure, Template Parts, selectors, resource keys, and runtime composition.
3. Tests proving behavior, lifecycle, platform, and theme contracts.
4. Gallery structured API/Token tables and stable ShowCase examples.
5. Existing control documents and changelog.

Issues, screenshots, commits, PRs, and investigation notes may explain why work started, but they are not design facts. Never
use them as the narrative frame of a current control document.

When source, Gallery, tests, and docs disagree, report the contradiction and determine the intended contract. Do not silently
pick the most convenient source.

## Workflow

### 1. Inspect

- Read the canonical documentation and development guidelines.
- List the control docs, source files, Themes, tests, Gallery rows, and stable examples.
- Read `git status` and relevant diffs; preserve unrelated user changes.
- Identify comparable controls only when they clarify an established local pattern.

### 2. Select the Edit Shape

- Update existing documents before creating new ones.
- Create the standard control directory only when the control lacks it.
- Split a topic document only when it has a stable independent model and would overload both main documents.
- Keep pre-implementation reasoning, option comparison, and task planning outside control documents. A topic design document
  contains only the final design, without status or Issue narration.
- Do not create a second guideline when the rule belongs in `control-documentation-guidelines.md`.

### 3. Write the Current Design

- Start from the control capability, not from the bug, Issue, commit, or migration story.
- State API semantics and defaults precisely; do not mechanically copy every member.
- Describe actual Template and composition nodes from Themes, not imagined generic structure.
- Define state/data owners, direction of flow, lifecycle acquire/release pairs, and boundary behavior.
- For algorithms, define inputs, outputs, units, coordinate systems, main flow, invalidation, and degradation rules.
- For platform or mode variants, define shared invariants first, then use a complete matrix for differences and fallback.
- Record source file structure only when it expresses stable ownership, not a target-file checklist.
- Keep Token semantics in `token.md`; keep history in `changelog.md`.

Topic design documents follow this shape when each section applies:

```text
Position -> Principles -> Model/API -> Variant strategy -> Architecture/ownership
-> Template/integration -> Algorithm/data flow/lifecycle -> Performance/AOT
-> Compatibility/customization -> Verification
```

### 4. Synchronize the Documentation Set

- Keep `overview.md` and `implementation.md` as the primary entry points.
- Add bidirectional links for each topic design document.
- Update `changelog.md` for actual design, API, theme, Token, or implementation-structure changes.
- Update the category index when adding a new control directory.
- Check Gallery API/Token tables and ShowCase examples when public usage changes.

### 5. Review

Read the result as a maintainer who has not seen the task:

- Can they identify the contract without reading history?
- Can they find the owner for every state, metric, resource, and lifecycle?
- Do API, Template, Gallery, tests, and docs use the same names and defaults?
- Are platform and mode differences complete rather than scattered exceptions?
- Does every specialized section belong in this control rather than a global guideline?
- Are all links valid and all required main-document sections present?

Fix contradictions and vague language before reporting completion.

## Red Flags

Stop and reshape the document when any of these appear:

- The opening starts with an Issue, screenshot, reproduction, or pixel analysis.
- `overview.md` or `implementation.md` contains “target files,” “planned API,” implementation status, or a task checklist.
- A topic design document contains adopted/rejected option comparison or historical root-cause analysis.
- Public API is listed only in a topic document and absent from the overview summary.
- Template/composition claims are not grounded in actual Themes or an approved design contract.
- Internal types are presented as user APIs.
- A control-level document repeats global AOT, Token, Gallery, or documentation rules.
- A new helper document has no independent audience, lifecycle, or ownership boundary.
- A Mobile Control directory is created before Public API source, Theme, Contract/Headless tests, and Gallery API/Token/ShowCase evidence exist.

## Verification

Always run:

```bash
git diff --check
```

Then verify according to impact:

- Documentation only: inspect relative links, required sections, stale names, and `git status`.
- Public API or behavior: run the targeted control tests and verify Gallery API/example synchronization.
- Theme/Template: run theme contract tests and inspect Light/Dark plus affected platform hosts.
- Gallery source changes: run `tests/AtomUIGallery.Tests`.
- AOT-sensitive changes: run the required NativeAOT publish validation.

## Completion Report

Report:

```text
Documents created/updated:
Contract or design represented:
Source/Theme/Gallery evidence:
Validation performed:
Known mismatch or residual platform validation:
Commit created: No unless explicitly requested
```
