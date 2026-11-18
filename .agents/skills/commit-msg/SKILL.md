---
name: atomui-commit-msg
description: Generate a commit message for AtomUI by reading the project's git staged area and recent commit style. Use when the user asks for a commit message, says "msg", "commit msg", "写提交信息", "创建 commit", "创建commit", or wants text that covers all staged changes. Output is a short English title (≤72 chars, what changed) plus an optional concise bullet body (why / impact / key details); Chinese only on explicit request.
---

# AtomUI Commit Message Generation Skill

## Goals

**1. Accurately summarize the commit** — Generate a commit message based on the current **git staging area**, covering all staged changes: a short title stating what changed, plus an optional body for why / impact / key details.

**2. Stay consistent with repository style** — Match the repository's existing commit conventions rather than mechanically applying a template.

## Core Principle

> A commit message is not a per-file listing of the diff — it is a compressed expression of the commit's intent. Read the staging area first, then summarize: the title answers *what*, the optional body answers *why / impact*.

## IMPORTANT: Trigger Conditions

This skill MUST be invoked when the user's message matches ANY of the following patterns. Do NOT skip this skill and handle the request manually.

| Trigger phrase | Example |
|---|---|
| `创建 commit` / `创建commit` | "创建 commit" |
| `commit msg` / `msg` | "msg", "commit msg" |
| `提交信息` / `写提交信息` | "帮我写提交信息" |
| Asks for a commit message | "generate a commit message" |
| Asks to summarize staged changes into a commit title | "summarize my staged changes" |

When triggered, this skill handles the **entire commit workflow**: reading staged changes, generating the message, creating the commit, and verifying the result. The caller should NOT add extra trailers (e.g., `Co-Authored-By`) or wrap the message in additional formatting.

## Fundamental Rules

### 1. Default to staged changes only

Generate the message based on **staged changes** only, because that is what will actually be committed.

Only include unstaged changes if the user explicitly requests it (e.g., "include unstaged" or "cover all changes").

### 2. Always check recent commit style first

Before generating a message, review recent commits to avoid producing a format inconsistent with the repository's conventions.

### 3. Default to a short title, optional body

The deliverable is the **commit message itself**: a short header line stating what changed, followed by an optional concise body (blank line + up to 5 wrapped bullets) when there is genuinely extra context worth recording. Never output analysis, commentary, code blocks, or quotes around the message. Trivial commits stay title-only.

## Execution Steps

### Step 1: Read git status, staging area, and recent commits

Gather the following information before generating. Do not guess or rely on filenames alone.

**Recommended commands:**

```bash
git status --short
git diff --cached --stat
git diff --cached
git log --oneline -10 --no-merges
```

- `git status --short` — Confirm which files are staged and whether unstaged changes exist.
- `git diff --cached --stat` — Quickly assess the scope of changes.
- `git diff --cached` — Read the actual content being committed. This is the primary basis for the message.
- `git log --oneline -10 --no-merges` — Check the repository's recent commit style, language, and type/scope conventions. When reviewing commit style, **ignore any `Co-Authored-By:` trailers** — they are not part of the project's commit convention and must not be reproduced.

### Step 2: Check if generation is possible

If the staging area is empty:

- Do NOT fabricate a message.
- Clearly inform the user that there are no staged changes and an accurate commit message cannot be generated.

### Step 3: Identify the primary semantic intent

Based on the `git diff --cached` output:

1. Determine the **primary purpose** — new feature, bug fix, documentation, refactoring, performance optimization, test, Gallery ShowCase, script/dependency update, etc.
2. Identify the **primary affected scope** — specific control name, infrastructure module (TokenSystem / Theme / Motion, etc.), Gallery, scripts, docs, etc.
3. If the diff contains multiple files or mixed small changes, use a higher-level summary that covers everything. Do not concatenate per-file descriptions into one long sentence.

### Step 4: Align with AtomUI repository style

Prefer to mimic the repository's recent conventions. Common patterns in AtomUI:

- `feat(Component): ...`
- `fix(Component): ...`
- `refactor(Component): ...`
- `docs: ...` / `docs(Core): ...`
- `chore(deps): ...`
- `gallery: ...` / `gallery(Component): ...`
- `build(ci): ...`
- `perf(TokenSystem): ...`

Notes:

- `type` and `scope` **MUST be in English**.
- `subject` **defaults to English**; Chinese is acceptable if the user is conversing in Chinese or explicitly requests it.
- If the repository already uses a more natural phrasing, prefer matching the existing style.
- `gallery` is used as a type prefix in AtomUI (analogous to `site` in ant-design) for ShowCase / Gallery app changes.

### Step 5: Generate title + optional body

The output must be:

- **A header line** stating what changed (≤72 chars hard, ≤50 soft target)
- **An optional body** (blank line + wrapped bullets, ≤5) for why / impact / key details
- **Covering all staged changes** honestly across title + body
- **Consistent with repository style**
- **Ready to use as-is for the commit**

## Writing Rules

### Header format

Use one of the following:

```text
<type>(<scope>): <subject>
```

or (when scope cannot be determined):

```text
<type>: <subject>
```

Examples:

- `feat(Button): add danger style variant for ghost button`
- `fix(Select): prevent dropdown from closing on multi-select click`
- `refactor(Avatar): extract AbstractAvatar to AtomUI.Controls`
- `docs(Core): add three-tier architecture documentation`
- `gallery: add QRCode showcase with comprehensive examples`
- `chore(deps): bump Avalonia from 11.3.1 to 11.3.2`
- Chinese subject only on explicit request: `fix(Select): 修复多选模式下点击后下拉框意外关闭`

### Header rules

- Use **imperative mood** — `add` / `fix` / `update`, not `added` / `fixed` / `updated`. For Chinese subjects, start with a verb: "修复" / "添加" / "调整".
- Do **NOT** capitalize the first letter of an English subject.
- Do **NOT** end with a period (`.` or `。`).
- Keep the header line to **72 characters or fewer**; soft target **≤50** so it stays scannable in `git log --oneline`.
- The header answers **what changed** — never why / how / impact; push those into the body.
- Do **NOT** chain extra facts into the header with `——`, `（）`/`()` asides, semicolons, or comma-spliced clauses; one change, one phrase.
- Avoid vague terms like `WIP`, `misc`, `update files`.

### Type selection

All types are **lowercase**:

- `feat` — New feature, control, property, or API
- `fix` — Bug fix — incorrect behavior, crash, memory leak, rendering issue
- `refactor` — Code restructuring with no behavior change — moving code between layers, renaming, splitting classes
- `docs` — Documentation only — README, XML doc comments, architecture docs, copilot-instructions
- `style` — Code formatting only (no logic change) — whitespace, indentation, code style
- `perf` — Performance improvement — rendering optimization, reduced allocations, caching
- `test` — Adding or updating tests
- `build` — Build system or dependency changes — MSBuild props, `Directory.Packages.props`, scripts
- `chore` — Maintenance that doesn't modify src/test — tooling config, `.editorconfig`, IDE settings
- `ci` — CI/CD configuration changes
- `gallery` — Gallery / ShowCase changes — demo pages, gallery navigation, gallery assets
- `release` — Version bumps, changelog updates, release preparation
- `revert` — Reverting a previous commit (must reference the reverted commit hash in the body)

When uncertain:

- User-visible behavior correction → prefer `fix`
- Only text, examples, or descriptions updated → prefer `docs`
- Only tooling, dependencies, or scripts adjusted → prefer `chore`
- ShowCase / Gallery demo changes → prefer `gallery`
- Code structure changed but behavior unchanged → prefer `refactor`

### Scope selection

- For changes to a single control, use the control name in PascalCase: `Button`, `Select`, `DatePicker`, `TreeView`, `Cascader`, `FloatButton`
- For infrastructure modules, use the module name: `TokenSystem`, `Theme`, `Motion`, `WaveSpirit`, `Core`, `Icons`, `Fonts`, `Generator`, `Native`, `i18n`, `MediaQuery`
- For cross-cutting maintenance, use a summary scope: `deps`, `scripts`, `Packaging`, `Bindings`, `MemoryLeak`
- For multiple closely related controls, use comma-separated names: `Select,AutoComplete`
- If no clear scope exists, it may be omitted

### Body rules

Add a body only when the title alone would lose information worth recording (root cause, motivation, impact scope, key trade-offs). Otherwise omit it — trivial commits stay title-only.

- Format: one blank line after the header, then bullet lines starting with `- ` (a short paragraph is acceptable when prose reads better).
- **At most 5 bullets**; wrap each line at **72 characters**.
- Content: why (root cause / motivation), impact scope, notable decisions or follow-ups — not a restatement of the title.
- Language: English by default, same as the header.

Example:

```text
fix(NavMenu): center text in dark horizontal top-level items

- dark branch missed the Padding=0 reset and inherited vertical
  ItemContentPadding, shrinking the text viewport below the 46px
  HorizontalLineHeight so glyphs sank to the line-box bottom
- move the Padding=0 setter into the shared IsTopLevel=True scope
```

## Language Rules

- `type` and `scope` **MUST be English** (they are fixed keywords / identifiers)
- `subject` and `body` **default to English regardless of the conversation language**
- Use Chinese only when the user **explicitly requests** it for that commit (e.g. "用中文写提交信息")

## Layer Awareness

AtomUI uses a three-tier architecture: Foundation → Base Control (`AtomUI.Controls`) → Platform Control (`AtomUI.Desktop.Controls`).

When changes span layers:

- If each layer's changes can be understood independently, suggest splitting into multiple commits
- If tightly coupled and inseparable, use a single commit with the control name as the scope

## Edge Cases

### Mixed change types

If staged changes include a mix of docs, styles, types, and small fixes:

- Identify the **primary purpose** first
- If there is no single primary purpose, use a higher-level summary
- The goal is to "honestly cover all changes", not to cram every detail into the title

### Scattered / unrelated changes

If the staging area contains clearly unrelated groups of changes:

- Still produce an honest title-only message
- Do not pretend these changes share a single specific purpose
- Use a broad summary, e.g., `chore(controls): clean up styles, types and docs`

### Breaking Changes

If the changes include a breaking API change:

- Append `!` after type/scope, e.g., `refactor(TokenSystem)!: rename ControlDesignToken to AbstractControlDesignToken`
- If the user requests a body, include a `BREAKING CHANGE:` footer

## Prohibited

- Generating a message without reading `git diff --cached`
- Guessing content based only on filenames
- Describing only some files/changes while ignoring other staged content
- Outputting analysis or commentary around the message — the structured title + optional body itself is the deliverable
- Breaking from established repository style just to fit a template
- Writing headers longer than 72 characters unless truly unavoidable
- Wrapping the final output in backticks, quotes, or code blocks
- Do not push to the remote end unless explicitly requested
- Do not add "Co-Authored-By: xxx" or information regarding other agents. Enforced by the `scripts/hooks/commit-msg` git hook (`core.hooksPath scripts/hooks`), which rejects AI/model attribution lines.

## Reference

For detailed type/scope definitions, style examples, and input→output samples, see `references/commit-message-convention.md`.