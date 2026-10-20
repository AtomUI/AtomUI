# LineEdit Changelog

## 2026-10-19

- Compatibility
  - Restore `TextBox` rendering for the inherited `InnerLeftContent` and `InnerRightContent` properties, including text, controls and implicit DataTemplates, without adding public API.
- Theme
  - Keep side content outside the text viewport, separate nonempty auxiliary regions from the editor using `SpacingXXS`, collapse null content without reserving space, and preserve clear/reveal/Form feedback/count layout. Side content follows `VerticalContentAlignment`; embedded text boxes inherit the same support.

## 2026-08-29

- Contract
  - Register the `OtpLineEdit` Semantic Part descriptor (`root`, `cellList`, `cell`, `separator`), matching the model documented in `otp-line-edit/overview.md`. `cell` and `separator` are `Multiple` + RuntimeCreated parts realized by the cells host's item template through the internal `.semantic-scope-cell` route scope.
- API
  - Center the `OtpLineEdit` separator glyph automatically: the new internal `OtpSeparatorPresenter` measures the glyph ink box from the current font's glyph metrics at display time and translates the ink center onto the line-box center, so any character, font, and font size renders centered without per-character tuning. The interim `SeparatorGlyphCompensationRatioX/Y` properties and the compensation tokens were removed before release; measurement falls back to no translation when glyph metrics are unavailable, and custom `SeparatorTemplate` content owns its own centering.
- Gallery
  - Add the `OtpLineEdit` Semantic Part preview to the LineEdit Semantic Parts tab, listing the four registered parts next to the LineEdit, Password, TextArea, and SearchEdit previews. The preview uses an empty value without the clear button and zero compensation so the `-` separator stays centered.

## 2026-08-28

- Contract
  - Register dedicated Semantic Part descriptors for `SearchEdit` (`root`, `prefix`, `input`, `suffix`, `clear`, `button`) and `TextArea` (`root`, `textarea`, `clear`, `count`). Each descriptor stays bound to its public owner; inheritance or template reuse does not grant another owner's Semantic Styles.
  - Mark `SearchEdit`'s search `button` as a RuntimeCreated part: the marker class is applied by `SearchEditDecoratedBox` after template application, and the generated `SearchEditButtonStyle` route documents the decorated-box scope.
- Docs
  - Rewrite the family Semantic Part contract to cover the three registered owners, their style types and customization boundaries; drop the earlier "not registered this round" boundary statements.
  - Document the compensation tokens for OTP separator glyph alignment and the self-drawn focused-cell caret for `OtpLineEdit`.


本文档记录 LineEdit 输入控件家族级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

记录原则：

- 只记录会影响 LineEdit 家族设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-08-27

- API
  - Make the standard root `Background` / `BorderBrush` a real customization channel for `TextBox`, `LineEdit`, `SearchEdit` and `TextArea`. `AbstractTextInput` relays effective owner values onto `InputControlFrame` as local values, mirroring inline style semantics where local values win over state classes: customized slots freeze the interactive color changes of that property, the focus `BoxShadow` glow survives on its own slot, and clearing the value restores the frame state machine. This fixes a broken API surface instead of adding parallel customization properties.
- Theme
  - Remove the dead owner-level `Background` / `BorderBrush` defaults from the LineEdit, TextBox, SearchEdit and TextArea themes, and remove the two `TemplateBinding`s from the TextBox template. The frame theme is the single rest-state source and the owner value doubles as the customization signal.
  - Align the TextBox rest background with `ColorBgContainer`, matching LineEdit / SearchEdit / TextArea.
- Tests
  - Add `TextInputRootBrushRelayTests` covering the relay, hover/focus border yielding, focus glow survival, clear-to-restore, pre-template assignment and the uncustomized rest baseline across all four controls.

## 2026-08-24

- Semantic Parts
  - Publish the `root`, `prefix`, `input`, `suffix`, `clear` and `count` Semantic Part contract for `LineEdit`.
  - Add explicit owner-relative selector routes through `AddOnDecoratedBox` while keeping route scope classes internal.
  - Keep all selector Part markers static and `Single`; content, visibility, status and size changes do not add or remove markers.
  - Keep `SearchEdit`, `TextArea` and `TextBox` outside this descriptor scope.
- Theme
  - Add a stable `AddOnContentPresenter` prefix wrapper that preserves content, template-only and `InnerLeftContentTemplate` behavior while exposing `ContentPresenter` as the public contract type.
  - Preserve the existing Large / Middle / Small and Outlined / Filled / Borderless / Underlined size baselines.
  - Make the TextBox input frame fill the width allocated by its owner so placeholder and text measurement cannot resize the visible input surface.
- Gallery
  - Add the deferred Semantic Parts Preview, localized descriptions and a strongly typed LineEdit Semantic Style example.
- Tests
  - Add descriptor, marker, generated Style, state stability, size matrix, Gallery snapshot and prefix template regressions.
  - Cover stable TextBox frame width across placeholder, short-text and long-text states.
- Docs
  - Add the complete LineEdit Semantic Part contract and regenerate the LineEdit LLMS output from source documentation.

## 2026-08-03

- Architecture
  - Define an internal text viewport metric owned by TextBox/TextArea so consumers can react to effective text width without traversing input templates.
  - Define viewport, padding, presenter margin and template reapply lifecycle as the single source of truth for input text width.

## 2026-08-23

- Architecture
  - Establish `AbstractTextInput` as the shared text-input logic owner for `TextBox`, `LineEdit` and `TextArea`.
  - Establish `InputControlFrame` as the shared input-surface owner for variant, effective status, border, background, corner, shadow, CompactSpace and motion.
  - Define `AddOnDecoratedBox` and its specialized descendants as layout extensions of `InputControlFrame`; they no longer own duplicated input-surface status selectors.
  - Define template-owned events and subscriptions as template-lifetime resources replaced on template reapply, while external Form feedback subscriptions follow logical attach/detach.
- API
  - Align `StyleVariant`, `Status`, clear, count, Form, native validation and CompactSpace semantics across the three base text-input controls.
  - Define `NativeValidationStatus`, `FormStatus`, `ExplicitStatus` and the `EffectiveStatus` priority used by all input surfaces.
- Theme
  - Move shared input-surface values to `SharedToken` and the frame theme; retain control tokens only for stable text/layout or TextArea resize differences.
  - Move stable clear, reveal, feedback, inner-right and count state projection from C# relay bindings to AXAML template or typed ancestor bindings.
- Tests
  - Cover logical detach/reattach for the complete `AbstractTextInput` descendant set and AutoComplete composition hosts.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `LineEdit`.
  - Align generated output paths with `controls/line-edit/index-cn.md` and `controls/line-edit/semantic-cn.md`.

## 2026-06-21

- Docs
  - Establish LineEdit desktop architecture documentation under `docs/controls/desktop/data-entry/line-edit/overview.md`.
  - Add LineEdit implementation documentation covering TextBox, LineEdit, SearchEdit, TextArea, template hookup, Form, CompactSpace and resize flows.
  - Add LineEdit Token documentation for `LineEditToken` and `TextAreaToken`.
  - Add LineEdit changelog and link the document set from the Data Entry category entry.
  - Document `CustomizableSizeType.Custom` as a Middle-baseline custom size path for LineEdit family controls.
  - Link SearchEdit's dedicated control documentation and clarify that SearchEdit's right external add-on slot is occupied by the search button.
