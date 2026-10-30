# Calendar Cell Selector Simplification Plan

> **For agentic workers:** Use superpowers:executing-plans task by task. Do not create a commit without explicit user authorization.

**Goal:** Simplify the Calendar DateViewerCell theme without changing rendered states or interaction, and assess whether selector cost changes in realistic calendar scenarios.

**Architecture:** Keep the static ControlTheme and DateViewerCell state owner. Reuse the cell's Foreground as the single source for date text colors; remove only state exclusions made redundant by DatePanel's week-cell template invariant. Keep state predicates that distinguish hover, disabled, selected, Today, week number, mode, and focus.

**Tech Stack:** Avalonia 12.1.3 selectors and inherited TextElement.Foreground, AtomUI Calendar/LunarCalendar themes, xUnit Headless, temporary Skia headless rendering probe.

**Spec:** User screenshot of `CalendarDateCellTheme.axaml` and `docs/controls/desktop/data-display/calendar/implementation.md`.

## Audit

- Request type: broad selector cleanup with a performance question; scope is CalendarDateCellTheme and its LunarCalendar consumer.
- State owner: DateViewerCell projects model states to pseudo classes; Calendar/LunarCalendar own business input, not cell styling flags.
- Template/lifecycle: static CalendarDateCellTheme inflates 42 cells per month or 48 with week numbers; no new dynamic visuals, subscriptions, or disposables.
- Comparable themes: DateViewerCellTheme and LunarCalendarTheme use the same template boundary; keep their owner-scoped style routes intact.
- Current selector shape: 33 Style rules, 34 selector branches, 11 `:not` terms, 28 `/template/` steps.
- Runtime evidence: Avalonia's Selector.Match creates activators for changing pseudo classes; selector length alone does not establish a page-load bottleneck.
- Baseline measurements: four Mini calendars attach/layout median 81.7472 ms; date change median 0.10404 ms in a Debug Headless probe. This is whole-control cost, not isolated selector cost.
- API/render contract: no public API changes and no intentional pixel/state/input changes.
- Residual risk: selected+disabled/outside/week precedence, LunarCalendar selected colors, transitions, template content and focus.

## Tasks

### 1. Establish visual and state baseline

- [x] Capture Calendar fullscreen, Mini, year, and LunarCalendar fullscreen raster baselines with temporary Skia headless probe.
- [x] Record matched Headless attach/layout and value-change metrics.
- [x] Run focused state tests for selected, disabled, outside, week, Today, focus and templates (40 Calendar/Lunar tests passed before the final interleaved comparison).

### 2. Simplify duplicated date color selectors

- [x] Let `PART_Value.Foreground` inherit DateViewerCell.Foreground, avoiding a per-cell TemplateBinding.
- [x] Remove redundant template-targeted selected/outside/disabled/week Foreground rules and add one cell-level week rule at the correct precedence.
- [x] Remove `:not(:week)` only from `:cell-template` and `:full-template`, because DatePanel never assigns those templates to week cells.
- [x] Compare Calendar fullscreen, Mini, year and LunarCalendar fullscreen pixels against baseline; all four scenarios were identical in four interleaved A/B pairs.

### 3. Measure and decide

- [x] Repeat the Release Headless benchmark in four interleaved A/B pairs: attach medians were noisy with no consistent regression; per-four-calendar allocation fell by 735–767 KiB and date-toggle time fell in every pair. Selector rules fell from 33 to 29, `:not` terms from 11 to 7, and `/template/` steps from 28 to 23.
- [x] Update Calendar implementation notes for inherited foreground ownership.
- [x] Clean temporary probes and their dedicated outputs.
- [ ] Run focused iterate verification on changed module paths and `git diff --check` (user explicitly requested no more testing after the completed A/B and focused tests; do not run further verification).
