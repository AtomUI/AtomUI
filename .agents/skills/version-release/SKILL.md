---
name: version-release
description: Use when preparing or validating an AtomUI release, including version-scope confirmation, AtomUIVersion consistency, README version sync, CHANGELOG and Chinese release sections, the mandatory breaking-change audit and docs/releases migration docs, release validation, and the release commit. Use changelog-collect for ordinary changelog collection instead.
---

# Version Release

Use this skill when the user asks to release or prepare an AtomUI version, or review release readiness.

Durable rules, the breaking-change surface list, and the release-readiness checklist live in
`docs/engineering/workflows/release-preparation.md`. Read it before classifying any change.

## Workflow

1. Confirm the intended version and release scope from the user request.
   If the target version or range is ambiguous, state the assumption and proceed with
   preparation; get explicit confirmation before tag, push, or publish.
2. Inspect repository state before changing files:
   - `git status --short`
   - `git branch --show-current`
   - `git tag --sort=-v:refname | head`
   - `git log --oneline --decorate <previous-tag>..HEAD` when a previous tag exists
3. **Run the breaking-change audit below and write down its verdict before editing any
   release file.** The verdict decides whether migration docs are required.

## Breaking-change audit

Publishing a breaking change without migration docs is a release defect. The audit is
mandatory, and a "no breaking changes" verdict must be evidenced, never assumed.

1. Enumerate every commit carrying a breaking marker:

   ```bash
   git log <previous-tag>..HEAD --format='%h %s' | grep -E '!:'
   ```

   Classify each hit individually. A `!` commit is the repository telling you a breaking
   change exists; it may not be dismissed without evidence.

2. Decide against the consumer-visible surface, not against C# members alone. A change is
   breaking when it alters any of these, even when no C# `public` member is removed:

   - C# public API: types, members, signatures, enum values, default behavior.
   - MSBuild contract: `build/**` properties, targets, `UsingTask` registration, task
     parameters, item/property names.
   - Package contract: target frameworks, output layout (`tools/`, `buildTransitive/`),
     package ids, shipped files.
   - Build prerequisites: required SDK/runtime versions, `global.json`.
   - Runtime contracts: design tokens, theme keys, resource keys, AOT/trimming behavior.

3. Evidence rule. A "not breaking" verdict must positively state which surfaces the change
   touches and why consumers are unaffected. An empty negation-only check such as
   `grep '^-.*public '` is **not** evidence: it cannot see MSBuild, packaging, TFM or SDK
   changes, and must not be presented as a verdict.

4. Precedent check. Whenever a breaking candidate exists, or the change touches `build/`,
   packaging, or target frameworks, read the most recent `docs/releases/*-api-changes.md`
   and search all of them for the same area:

   ```bash
   grep -rln -iE 'build|packaging|msbuild|netstandard|net10|sdk|tools/' docs/releases/*-api-changes.md
   ```

   Build-task and packaging contract changes are documented as API changes in this
   repository even when no control API moves; see `docs/releases/6.1.4-api-changes.md`.

5. Record the verdict as a candidate → breaking? → evidence table and report it to the user
   before requesting the release commit, tag, or push.

## Automated breaking-change gates

The manual audit above is backed by two executable gates in the release flow. They observe
produced artifacts, not commit messages, so they still catch a break whose commit is missing
the `!` marker.

- Package API validation: `EnablePackageValidation` with `PackageValidationBaselineVersion`,
  wired in `build/PackageValidation.props`. Compares the `lib/` public API (ApiCompat, IL level)
  against the previously released packages. It cannot see `tools/`, `buildTransitive/` or the
  target-framework set.
- Package layout validation: `scripts/verification/verify-package-layout.ps1`. Compares the
  consumer-visible package layout (`lib/` TFM set, `tools/`, `build/`, `buildTransitive/`)
  against the previously released packages.

Both run from `scripts/BuildNuGetPackages.ps1` when given `-PackageValidationBaselineVersion`;
the release workflow passes it through the `PackageValidationBaselineVersion` input.

- Run both for the release and report the result. A failing gate is a release defect until the
  change is either documented as breaking or acknowledged.
- Acknowledge an intentional package API change in
  `build/PackageValidationSuppressions/<ProjectName>.xml` (generate with
  `-p:ApiCompatGenerateSuppressionFile=true`, then review).
- Acknowledge an intentional package layout change in
  `scripts/verification/package-layout-allowlist.json`, keyed by baseline version and package id
  with a `reason`. Unused entries fail the gate, so the allow list cannot rot.
- Acknowledging either kind does not replace the migration doc: add the section to
  `docs/releases/<version>-api-changes.md` too.

## Version consistency

- Keep `build/Versions.props` -> `AtomUIVersion` as the single source of truth.
- Packages normally inherit `$(AtomUIVersion)` through `build/PackageMetadata.props`.
  `AtomUI.Core` embeds it as assembly metadata and `PublishToLocal.ps1` reads it.
  Do not hardcode version strings in project files, package metadata, or scripts.
- Scan for stale previous-version references and update only release-required files.

## README version references

`README.md` and `README.zh-CN.md` are user-facing release pages and are part of every
release. Update both in the same release commit and keep their versions identical. The
version appears in four places in each file, and every one must change together:

1. The AtomUI badge near the top: `.../badge/AtomUI-<version>-1677ff?...`.
2. The `#### Latest Release Notes` (English) / `#### 最新版本说明` (Chinese) paragraph:
   rewrite the summary for this release instead of leaving the previous release's text,
   mention any breaking changes, and link the changelog.
3. The `dotnet add package ... --version <version>` block.
4. The `<PackageReference ... Version="<version>"/>` project example.

Bumping only the badge while the release-notes paragraph still describes the previous
version is a release defect. Verify no stale version remains before the release commit:

```bash
grep -n "<previous-version>" README.md README.zh-CN.md
```

## Changelog

- Update both `CHANGELOG.md` and `CHANGELOG.zh-CN.md` with the same version, release date,
  and information. Use `YYYY-MM-DD` dates.
- Group entries by control or module (`Dialog`, `ToolTip`, `Window`, `DataGrid`, `Theme`,
  `NativeAOT`, `Build`), not a mechanical Added/Changed/Fixed split.
- Write present-tense user-visible outcomes and include PR or issue references when available.
- Follow `docs/engineering/contributing/changelog-guidelines.md`.

## Breaking API changes

Create the migration docs whenever the audit finds **any** breaking change. "Breaking"
follows the surface list in the audit above; MSBuild, packaging, target-framework and
SDK-requirement changes count.

- Create `docs/releases/<version>-api-changes.md` and `docs/releases/<version>-api-changes.zh-CN.md`.
  Use `docs/releases/6.1.4-api-changes.md` (build-task contract) and
  `docs/releases/6.1.8-api-changes.md` (runtime API) as templates.
- Each file must include a quick-reference before/after table and migration instructions or
  code examples for every breaking change, plus cross-links between the English and Chinese
  versions.
- Add the new version links to `docs/releases/overview.md`.
- Put a `Breaking Changes` group first in the root changelog section and link to the detailed
  file.
- Every entry in that `Breaking Changes` group must map to a section in the migration docs.
  If one does not, either add the section or record the exemption the user agreed to.
- Do not create these files when the audit found no breaking changes.

## Validation

- Run `git diff --check`.
- Re-check the audit enumeration and confirm every `!` commit is accounted for in the
  changelog and, when breaking, covered by the migration docs.
- Run targeted tests for the controls or modules changed. Prefer
  `dotnet test <test-project> --framework net10.0 --no-restore`.
- **Run the full regression test suite. This step is mandatory when
  executing the `version-release` skill and cannot be replaced by targeted
  module tests. Use the repository's maintained full-test entry point, such
  as `dotnet test AtomUI.slnx --framework net10.0 --no-restore`, when
  applicable. Record the command, result, and any unrelated failures
  separately.**
- Run build or pack validation when packaging or build files changed.
- Run the two automated breaking-change gates against the previous release and report both
  results:

  ```bash
  pwsh -NoProfile -File scripts/BuildNuGetPackages.ps1 \
      -BuildType Release -PackageOutputDir <dir> -PackageValidationBaselineVersion <previous-version>
  ```

  The layout gate alone can also be run directly against an existing package directory:

  ```bash
  pwsh -NoProfile -File scripts/verification/verify-package-layout.ps1 \
      -PackageDirectory <dir> -BaselineVersion <previous-version>
  ```

  Its own tests run with
  `pwsh -NoProfile -File scripts/verification/verify-package-layout.Tests.ps1`.
- Run the Gallery NativeAOT publish flow when the release affects AOT, trimming,
  Window, theme, control templates, or source generators.
- Report unrelated failures separately; do not fix them as part of the release.

## Release commit

- Stage only release-required files. This includes `build/Versions.props`, both
  changelogs, any breaking-API docs, and both `README.md` and `README.zh-CN.md`.
- Follow repository history style, for example:
  `fix(CHANGELOG): update for AtomUI 6.1.5 release with breaking changes and new features`.

## Safety

Do not create tags, push commits, publish packages, or delete release artifacts unless the user explicitly asks for that action.

- Before tag, push, or publish, show the exact commands and stop for confirmation, together
  with the breaking-change audit verdict. Never request tag or push approval before the audit
  is complete.
- Preferred order: prepare and validate, commit, tag, then push and trigger publication.
- **Never move a tag that is already pushed.** If a defect is found after the tag is pushed,
  fix forward with a follow-up commit on the release branch by default and state the
  consequence. Only re-tag with `git tag -f` when the user explicitly instructs it, and never
  silently.

## Output

Summarize the changed files, the breaking-change audit verdict, validation results, the
release commit, and any manual steps that remain.
