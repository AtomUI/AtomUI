# AtomUI 6.1.3 Release Documentation Design

## Goal

Create complete, user-facing release documentation for AtomUI 6.1.3 from the changes after the
6.1.2 release-notes commit (`7590caeeddd7fe0c058f06cd21c00a38cbd6a639`) through the current
`release/6.0` HEAD, including the Avalonia 12.1.1 dependency upgrade.

## Changelog Scope

Add a `6.1.3` section dated `2026-08-08` to both `CHANGELOG.md` and `CHANGELOG.zh-CN.md`.
The two sections must have the same grouping, compatibility information, and user-visible detail.

Group the release by user-recognizable areas:

- Breaking Changes
- Calendar and DatePicker
- Localization, Generator and Build
- Theme
- Upload
- Navigation and Selection Controls
- Data Entry, Display and General Controls
- Window, DataGrid, Gallery and Dependencies

Do not reproduce commit messages mechanically. Exclude tests, design-document commits, formatting,
and private refactors unless they explain a public behavior, compatibility, NativeAOT, packaging, or
migration impact.

## README Scope

Update `README.md` and `README.zh-CN.md` together:

- Change the latest release summary from 6.1.2 to 6.1.3.
- Update all AtomUI NuGet command and `PackageReference` examples from 6.1.2 to 6.1.3.
- State the supported Avalonia runtime precisely as 12.1.1.
- Keep the release summary concise and link to the corresponding changelog.

The README summary should highlight the new Calendar, generated localization architecture with
Portuguese (Brazil) support, major control improvements, and the Avalonia 12.1.1 compatibility
upgrade without duplicating the full changelog.

## Compatibility Presentation

The changelog must explicitly identify breaking or migration-sensitive changes, including the
compiled theme architecture, localization catalog migration, Upload input-contract changes,
Timeline placement changes, Tag variants/checkable controls, and Separator spacing restoration
where public contracts changed.

The Avalonia entry should state that AtomUI now targets Avalonia 12.1.1 and that no AtomUI source
migration is required for this patch dependency upgrade. Internal test timing observations are not
release-note content because they do not represent a confirmed user-facing defect.

## Verification

- Confirm English and Chinese changelogs contain the same 6.1.3 groups and date.
- Confirm README package examples consistently use 6.1.3.
- Confirm README requirements consistently use Avalonia 12.1.1.
- Run the repository documentation verifier when available.
- Run `git diff --check` and review the final diff for unrelated changes.

