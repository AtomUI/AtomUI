# AtomUI Git Commit Message Convention

## Style Reference

Common commit patterns in the AtomUI repository:

- `feat(QRCode): add error correction level selection`
- `fix(MemoryLeak): fix event handler leak in input controls`
- `refactor(Segmented): extract AbstractSegmented to AtomUI.Controls`
- `docs(Core): add three-tier architecture documentation`
- `chore(deps): bump Avalonia to 11.3.2`
- `gallery: add QRCode showcase with comprehensive examples`
- `build(ci): add macOS ARM64 to build matrix`
- Chinese subject only on explicit request: `fix(Select): 修复多选模式下点击后下拉框意外关闭`

Key observations:

- `type` and `scope` **MUST be English** (they are fixed keywords / identifiers)
- `subject` and `body` **default to English regardless of the conversation language**; Chinese only on explicit request
- Two common forms: `type(scope): subject` and `type: subject` (when scope is unnecessary)
- `gallery` is used as a direct type prefix for showcase / gallery app changes (analogous to `site` in ant-design)

---

## Format

```
<type>(<scope>): <subject>

[optional body]

[optional footer(s)]
```

---

## 1. Types

All types MUST be **lowercase**.

| Type | When to Use |
|---|---|
| `feat` | New control, property, API, or user-facing behavior |
| `fix` | Bug fix — incorrect behavior, crash, memory leak, rendering issue |
| `refactor` | Code restructuring with no behavior change — moving code between layers, renaming, splitting classes |
| `docs` | Documentation only — README, XML doc comments, architecture docs, copilot-instructions |
| `style` | Code formatting only (no logic change) — whitespace, indentation, code style |
| `perf` | Performance improvement — optimizing rendering, reducing allocations, caching |
| `test` | Adding or updating tests — test cases, test infrastructure, test fixtures |
| `build` | Build system or dependency changes — MSBuild props, `Directory.Packages.props`, scripts |
| `chore` | Maintenance that doesn't modify src/test — tooling config, `.editorconfig`, IDE settings |
| `ci` | CI/CD configuration — GitHub Actions workflows, build pipeline |
| `gallery` | Control gallery / showcase app changes — ShowCase demos, gallery navigation, gallery assets |
| `release` | Version bumps, changelog updates, release preparation |
| `revert` | Reverting a previous commit (must reference the reverted commit hash in body) |

> **Note**: `feat` and `fix` will appear in auto-generated changelogs.

---

## 2. Scope (Recommended)

The scope identifies *what area* of the codebase is affected. Placed in parentheses after the type. Use **PascalCase**.

### Scope Categories

| Category | Examples | When to Use |
|---|---|---|
| **Control name** | `Button`, `Select`, `Tag`, `DatePicker`, `TreeView`, `Cascader`, `FloatButton`, `ColorPicker`, `DataGrid` | Change is specific to one control |
| **Infrastructure** | `TokenSystem`, `Theme`, `Motion`, `WaveSpirit`, `Core`, `Icons`, `Fonts`, `Generator`, `Native`, `i18n`, `MediaQuery` | Change to core framework infrastructure |
| **Cross-cutting** | `deps`, `scripts`, `Packaging`, `Bindings`, `MemoryLeak` | Change spans multiple areas or is a maintenance concern |

### Scope Selection Rules

#### When to use `fix(Component)`

The change corrects a component's behavior:

- Interaction doesn't match the expected design spec
- Boundary / edge-case rendering error
- Type, logic, or layout error that affects usage

```
fix(Table): correct pagination when data source is empty
fix(Form): preserve validation status after reset
fix(Select): prevent dropdown from closing on multi-select click
```

#### When to use `feat(Component)`

The change adds new user-facing capability to a component:

```
feat(Tag): add closable animation with motion system
feat(QRCode): add error correction level selection
feat(Button): add danger style variant for ghost button
```

#### When to use `refactor(Component)` or `refactor(Infrastructure)`

The change restructures code without altering external behavior:

```
refactor(Avatar): extract AbstractAvatar to AtomUI.Controls
refactor(TokenSystem): migrate to three-layer token derivation
refactor(Bindings): refactor reactive bindings across controls
```

#### When to use `docs`

Only documentation text changed — README, XML doc comments, architecture docs, demo descriptions, copilot-instructions:

```
docs: update FAQ link in issue template
docs(Button): clarify loading demo description
docs(Core): add three-tier architecture documentation
```

#### When to use `gallery`

Changes to the control gallery / showcase app — demo pages, gallery navigation, gallery assets:

```
gallery: add QRCode showcase with comprehensive examples
gallery(Button): update danger button demo layout
```

#### When to use `chore`

Dependency upgrades, lint rules, script maintenance, non-business engineering tasks:

```
chore: update .editorconfig formatting rules
chore(deps): bump Avalonia from 11.3.1 to 11.3.2
chore(scripts): update release helper
```

### Multiple Scopes

When a change spans multiple closely related controls, use a comma-separated list or a higher-level scope:

```
fix(Select,AutoComplete): fix shared dropdown positioning logic
fix(MemoryLeak): fix event handler leak in input controls
```

### Omitting Scope

Scope MAY be omitted for broadly cross-cutting changes:

```
chore: update .editorconfig formatting rules
build: upgrade to .NET 10 RC1
```

---

## 3. Subject (Required)

The subject is a succinct description of the change.

### Rules

- **Imperative, present tense**: "add", "fix", "change" — not "added", "fixed", "changed". 中文时同理，使用动词开头："修复"、"添加"、"调整"。
- **Do NOT capitalize** the first letter (English subject).
- **Do NOT end** with a period (or `。`).
- **Max 72 characters** for the entire header line (`type(scope): subject`); soft target **≤50** so it stays scannable in `git log --oneline`.
- The subject answers **what changed** — never why / how / impact; push those into the body.
- Do **NOT** chain extra facts into the subject with `——`, `（）`/`()` asides, semicolons, or comma-spliced clauses; one change, one phrase.
- Be **specific** — describe *what* changed, not just *where*.
- **Language**: English by default; Chinese only when explicitly requested.

### Good vs. Bad

| ❌ Bad | ✅ Good |
|---|---|
| `fix(Button): fix bug` | `fix(Button): prevent double-click event during animation` |
| `feat(Tag): Tag updates` | `feat(Tag): add closable animation with motion system` |
| `refactor: refactor code` | `refactor(TokenSystem): migrate to three-layer token derivation` |
| `Fixed the issue.` | `fix(Select): correct dropdown z-index in overlay layer` |
| `fix(Select): 修了个问题` | `fix(Select): 修复多选模式下点击后下拉框意外关闭` |

---

## 4. Body (Optional)

Provides additional context when the title alone would lose information worth recording (root cause, motivation, impact scope, key trade-offs). Trivial commits stay title-only.

- Separate from subject with **one blank line**.
- **At most 5 bullets**, each wrapped at **72 characters**.
- Explain **why** and the impact, not *how* (the diff shows *how*); do not restate the title.
- Language: English by default, same as the header.

```
refactor(Avatar): extract platform-agnostic logic to AbstractAvatar

- move shared properties (Shape, Size, Src, Alt) to AbstractAvatar
  in AtomUI.Controls
- keep desktop-specific token registration in Avatar
- prepares for future Mobile control layer reuse
```

---

## 5. Footer (Optional)

### Breaking Changes

Append `!` after type/scope **and** add a `BREAKING CHANGE:` footer:

```
refactor(TokenSystem)!: rename ControlDesignToken to AbstractControlDesignToken

BREAKING CHANGE: All custom token classes must now inherit from
AbstractControlDesignToken instead of ControlDesignToken.
```

### Issue References

```
fix(Cascader): fix search result not highlighting matched text

Fixes #234
```

Keywords: `Fixes #N`, `Closes #N`, `Resolves #N`, `Refs #N`, `Related to #N`.

---

## 6. Multi-Change Commits

When the staging area contains multiple small related changes, use a higher-level summary to cover them all:

| Situation | Commit Message |
|---|---|
| Multiple small control fixes | `chore(controls): clean up styles and types` |
| Docs + code examples together | `docs: update Button demo and README` |
| Bug fix that also adjusts styling | `fix(Table): correct pagination and styling` |
| Multiple gallery demo adjustments | `gallery: refine theme preview interactions` |
| Type fix + test + minor style in one control | `fix(Table): adjust pagination types, styles and test` |

**Principle**: pick the most semantically significant type, and let the subject describe the combined scope.

---

## 7. Layer-Aware Commits

AtomUI has a three-tier architecture. When a change spans layers, prefer **separate commits per layer** when independently meaningful:

```
# Commit 1: Base layer (AtomUI.Controls)
refactor(Avatar): extract AbstractAvatar to AtomUI.Controls

# Commit 2: Platform layer (AtomUI.Desktop.Controls)
refactor(Avatar): update desktop Avatar to inherit AbstractAvatar
```

If the changes are tightly coupled and make no sense independently, a single commit with a descriptive body is acceptable.

---

## 8. Special Commits

### Revert

```
revert: revert "feat(Tag): add closable animation"

This reverts commit abc1234.
Reason: animation causes layout flicker on macOS.
```

### Release

```
release: v5.2.0
```

### Merge

Merge commits generated by Git/GitHub are acceptable as-is.

---

## 9. Summary Table

| Element | Rule |
|---|---|
| Type | Required. Lowercase English. One of: `feat`, `fix`, `refactor`, `docs`, `style`, `perf`, `test`, `build`, `chore`, `ci`, `gallery`, `release`, `revert` |
| Scope | Recommended. PascalCase English. Parenthesized. |
| Subject | Required. Imperative mood. Lowercase start (English) or verb start (Chinese). No period. ≤72 chars total header (soft ≤50). States *what*, never why/how. English default; Chinese only on explicit request. |
| Body | Optional. Blank line after subject. ≤5 bullets, wrap at 72 chars. English default. |
| Footer | Optional. `BREAKING CHANGE:` for breaking changes. `Fixes #N` for issues. |
| Breaking | Append `!` after scope, and/or add `BREAKING CHANGE:` footer. |

---

## 10. AI Agent Output Requirements

When generating commit messages automatically (e.g., from `git diff --cached`):

- Output a **short header line** plus an **optional body** (blank line + up to 5 wrapped bullets) — title-only for trivial commits
- Do **NOT** wrap in backticks or quotes
- Do **NOT** explain reasoning unless asked
- Keep the header line **≤ 72 characters** (soft target ≤50); no `——` / `（）` asides in the header
- `type` and `scope` MUST be **English**
- `subject` and `body` default to **English regardless of the conversation language**; use Chinese only if the user explicitly requests it

---

## 11. Input → Output Examples

**Input**: `git diff --cached` shows changes only in `src/AtomUI.Desktop.Controls/Button/ButtonToken.cs` — a new token property `DangerColor` was added.

**Output**: `feat(Button): add DangerColor token property`

---

**Input**: Staging area contains a type fix in `Tag.cs`, a style adjustment in `TagTheme.axaml`, and a test update in `TagTests.cs`.

**Output**: `fix(Tag): adjust tag type definition, theme style and test`

---

**Input**: Only `controlgallery/AtomUIGallery/ShowCases/QRCodeShowCase.axaml` and its code-behind changed.

**Output**: `gallery(QRCode): add error correction level demo`

---

**Input**: `scripts/PublishToLocalSources.ps1` was modified.

**Output**: `chore(scripts): update local publish script`

---

**Input**: Multiple files across `AtomUI.Controls/Avatar/` and `AtomUI.Desktop.Controls/Avatar/` were restructured, moving shared properties to the abstract base class.

**Output**: `refactor(Avatar): extract shared properties to AbstractAvatar`

---

**Input**: `Directory.Packages.props` bumped the Avalonia version from 11.3.1 to 11.3.2.

**Output**: `chore(deps): bump Avalonia from 11.3.1 to 11.3.2`

---

**Input**: `src/AtomUI.Core/Theme/DesignToken.Alias.cs` had a performance optimization reducing dictionary lookups.

**Output**: `perf(TokenSystem): reduce dictionary lookups in alias token calculation`

---

**Input**: `README.md` and `README.zh-CN.md` were updated with new badges and installation instructions.

**Output**: `docs: update README badges and installation guide`

---

**Input**: Large refactoring that renames `ControlDesignToken` to `AbstractControlDesignToken` across 40+ files, breaking the public API.

**Output**:

```
refactor(TokenSystem)!: rename ControlDesignToken to AbstractControlDesignToken

BREAKING CHANGE: All custom token classes must now inherit from
AbstractControlDesignToken instead of ControlDesignToken.
```

---

**Input**: A single-theme file fix where the dark branch of a horizontal menu item missed a padding reset, making text render below center.

**Output**:

```
fix(NavMenu): center text in dark horizontal top-level items

- dark branch missed the Padding=0 reset and inherited vertical
  ItemContentPadding, shrinking the text viewport below the 46px
  HorizontalLineHeight so glyphs sank to the line-box bottom
- move the Padding=0 setter into the shared IsTopLevel=True scope
```



