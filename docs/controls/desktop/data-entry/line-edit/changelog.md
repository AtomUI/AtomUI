# LineEdit Changelog

## 2026-11-12

- Fixed
  - TextArea：未设置 `Height` 且由父容器拉伸（Grid `*` 行、`FlexPanel` + `Flex.Grow`）时，文本视口不再塌缩为 `Lines` 高的一条并垂直居中，现在填满输入框，正文从首行开始，滚动条和 PageDown 作用于整个视口。`Lines` 只决定自然高度，未拉伸时（如 StackPanel 中）仍显示 `Lines` 行。此前期望高度会随内容增长，在 FlexPanel 中可能撑出窗口，现已修复。（Gitee IKV2LQ）

## 2026-11-11

- Theme
  - focus 外发光只作用于 `Outlined`：`InputControlFrameTheme`、`LineEditTheme`、`TextBoxTheme`、`TextAreaTheme` 移除 `Filled` / `Underlined` 的 focus/pressed `BoxShadow`，`TextBoxTheme` 的 focus shadow 增加 `Outlined` 条件，嵌入式（Borderless）文本框不再获得阴影。
  - LineEdit、TextBox、TextArea 焦点样式对齐 Ant Design：主题设置空 `FocusAdorner`，键盘聚焦不再叠加全局方框，焦点只由 frame 的主色边框与 `Outlined` 外发光表达。
- Fixed
  - 共享 `BorderRenderHelper` 在背景/边框几何路径中补绘外侧 `BoxShadow`；输入 frame 固定走该路径，`Outlined` 的 focus 外发光此前被丢弃，现在按主题正确绘制。
  - TextArea：`TextAreaDecoratedBox` 的裁剪下移到 `Border#TextAreaContentFrame`，`TextArea` 宿主设置 `ClipToBounds=False`；内容仍被裁剪，frame 外侧的 focus 外发光可见。
- Accessibility
  - 共用清除按钮 `InputClearIconButton` 的自动化名称取自通用语言包 `CommonLangResourceKind.Clear`（Ant Design `locale.global.clear`），随语言切换更新。输入家族的清除按钮因此都有本地化名称。
  - 该名称由 `InputClearIconButtonTheme` 的 setter 绑定语言资源（`{atom:CommonLangResource Clear}`），处于 ControlTheme 层：应用 Style、本地值或绑定都能覆盖。此前代码以 Template 优先级设值，高于 Style，应用样式无法覆盖。
- Fixed
  - 清除按钮按下 Enter / Space 或被点击清除后，焦点回到输入（键盘触发时保持键盘焦点可见）。此前按钮随文本清空隐藏，焦点丢失；TextArea 与 Mentions 的清除按钮是 Tab 停靠点，可直接复现。
- Theme
  - `InputControlFrameTheme` 统一设置空 `FocusAdorner`：模板为接收内边距点击而把外框设为可聚焦时，键盘到达外框也不会叠加焦点描边，焦点仍由 `:focus-within` 外框表达。
  - TextArea：内部 `TextAreaDecoratedBox` 设 `IsTabStop=False`，只保留指针聚焦，不再是额外的 Tab 停靠点。

## 2026-11-08

- Changed
  - 共享输入表面 `InputControlFrame`、`AddOnDecoratedBox` 及 `SearchEditDecoratedBox` / `TextAreaDecoratedBox` 的主题改以
    `OwnerTokenResource` 读取所属公开控件的有效 Global Token，`Controls[TextBox]`、`Controls[LineEdit]`、`Controls[SearchEdit]`、
    `Controls[TextArea]` 的覆盖作用于各自的边框、背景、圆角、状态色、焦点阴影与内边距，且不影响同窗口的其他输入控件；
    LineEdit / SearchEdit / TextArea / TextBox 主题中为 hover / focus 重复设置表面颜色的样式随之删除。默认视觉不变。
  - LineEdit、SearchEdit、TextArea 的主题改读各自的 `XxxTokenResource`；`TextBoxTheme` 的全局值以 `OwnerTokenResource`
    读取，因为它也是 EmbeddedTextBox 等无 identity 内嵌编辑器的基础主题，这些编辑器跟随宿主（NumericUpDown、选择器、筛选框）。
  - 宿主根上的 `BorderBrush` / `Background` 改经模板绑定写入输入表面的内部属性 `HostBorderBrush` / `HostBackground`，由
    `InputControlFrameTheme` 在全部状态色之后投射；`AbstractTextInput` 不再以本地值写入内部表面。
- Fixed
  - LineEdit 按 `SizeType` 使用 `InputFontSize` / `InputFontSizeLG` / `InputFontSizeSM`（`IsCustomFontSize=False` 时；`Custom`
    取中档），SearchEdit 按 `SizeType` 使用自己的 `FontSize` / `FontSizeLG`（小号取 `FontSize`）。此前二者没有字号预设，
    大号与小号沿用继承字号（默认主题下 14px）；现在大号为 16px，小号与中号同为 14px，对齐 antd Input
    （`InputFontSizeSM` 默认等于 `InputFontSize`，原默认 `FontSizeSM` 未曾生效）。
  - 输入框获得焦点和 Error / Warning 焦点时的 `InputActiveShadow` / `InputErrorActiveShadow` / `InputWarningActiveShadow`
    阴影此前不绘制（共享边框绘制在非均匀背景路径中丢弃了阴影），现在可见；TextArea 不再裁剪到自身边界，阴影绘制在控件外。
  - TextArea 的 resize 指示线此前绘制在输入表面之下、被表面背景覆盖；现由叠放在表面之上的 `ResizeHandle` 绘制，指示与命中
    区尺寸使用 `TextArea.ResizeHandleSize`，线宽使用 `LineWidth`。
  - `AddOnDecoratedBox` 的状态色改为可撤销的 Template 优先级值：附加区图标与呈现器上的用户本地 FillBrush / StrokeBrush /
    Foreground 与部件样式不再被 `SetCurrentValue` 覆盖或在退出状态时被 `ClearValue` 擦除，退出状态只释放自身施加的值。
  - TextArea 的内容呈现器位于注册控件 ScrollViewer 之内，行高、最小高度与小号内容边距改为显式读取 TextArea 的有效值。

## 2026-11-05

- 实现结构
  - TextBox、LineEdit、SearchEdit、TextArea 的输入模板统一直接使用 Avalonia `TextPresenter`，移除内部选区缓存兼容子类；PART 名称、绑定、Semantic Part 契约和选择/密码行为保持一致。

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
