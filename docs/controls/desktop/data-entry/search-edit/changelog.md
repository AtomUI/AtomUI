# SearchEdit Changelog

本文档记录 SearchEdit 控件级设计、API、主题契约、Token 和实现结构的变化。它用于维护控件设计历史，不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-21

- 新增 Own Token 类 `SearchEditToken`，含 `InputFontSizeSM`（小号字体，默认 `FontSize`，与 antd 一致），Small 字号改读它。Ant Design / Shadcn 渲染不变。
- PrimeNG：`InputFontSizeSM` 取 `FontSizeSM`（PrimeUIX `form.field.sm.font.size` 0.75rem），与 Small LineEdit、Button 同为 12px；并补上与 LineEdit 相同的 PrimeUIX form field 规则（悬停边框、禁用文字、图标色、各尺寸同一圆角等），此前 SearchEdit 没有 Own Token 类，主题无法为它单独计算这些值。
- 修复搜索按钮高度只增不减：按钮 `Height` 原绑定到装饰框自身的 `Bounds.Height`，而装饰框又按子元素测量，两者互相撑高。从高的主题（如 PrimeNG Lara 32px）切到矮的主题后，SearchEdit 一直停在 32px。改为 `VerticalAlignment="Stretch"`，由 `SearchEditPanel` 按行高排布按钮。
- Small 文本行高随输入框统一改为 `FontHeight`（见 LineEdit changelog），Small SearchEdit 与其他 Small 输入框同为 `ControlHeightSM`。

## 2026-11-11

- Theme
  - focus 外发光只作用于 `Outlined`，`Filled` / `Underlined` 不再设置 focus `BoxShadow`。
  - 焦点样式对齐 Ant Design：主题设置空 `FocusAdorner`，键盘聚焦不再叠加全局方框，焦点只由 frame 的主色边框与 `Outlined` 外发光表达。
- Fixed
  - 共享 `BorderRenderHelper` 在背景/边框几何路径中补绘外侧 `BoxShadow`；输入 frame 固定走该路径，`Outlined` 的 focus 外发光此前被丢弃，现在按主题正确绘制。

## 2026-11-08

- Changed
  - 主题改读 `SearchEditTokenResource`，输入表面经共享 `InputControlFrame` / `AddOnDecoratedBox` 以 `OwnerTokenResource` 读取
    SearchEdit 的有效值；`Controls[SearchEdit]` 不改变搜索按钮的颜色（按钮读取 Button 有效值），拼接圆角与 Text 型按钮的搜索语义
    色跟随 SearchEdit。
- Fixed
  - 按 `SizeType` 使用 SearchEdit 的 `FontSize` / `FontSizeLG`（小号与中号同取 `FontSize`，对齐 antd；`IsCustomFontSize=False` 时），此前没有字号预设。
  - 焦点阴影现在绘制；根 `BorderBrush` / `Background` 经 `HostBorderBrush` / `HostBackground` 投射到输入表面。

## 2026-11-05

- 实现结构
  - 输入模板直接使用 Avalonia `TextPresenter`，选区前景色刷新由框架负责；保留 `PART_TextPresenter`、`input` Semantic Part 及搜索行为。

## 2026-09-15

- Documentation
  - Replace the placeholder semantic contract in `overview.md` with the real five Selector Parts published by
    `SearchEdit.SemanticParts.cs`: `root`、`prefix`、`input`、`suffix`、`clear`、`button`. The previous table listed
    generic placeholder entries (`input` / `trigger` / `popup` / `validation`) that did not correspond to any declared
    descriptor, and the LLMS generator projected that table verbatim into `controls/search-edit/semantic-cn.md`.
  - Record the routing anchors (`semantic-scope-input-frame`、`semantic-scope-prefix`、`semantic-scope-suffix`) and
    clarify that `semantic-content` / `semantic-placeholder` are non-Part routing/visual helper markers, not public
    contract.
- API documentation
  - Correct two public-surface descriptions that did not match the source: `SearchButtonText` is typed `object?`
    (not `string`), and the inherited inner-content entry points are `InnerLeftContentTemplate` /
    `InnerRightContentTemplate` (there are no `InnerLeftContent` / `InnerRightContent` properties on the
    `LineEdit` base).

## 2026-09-01

- API
  - Rename the public enum `SearchEditButtonStyle` to `SearchButtonType`; values (`Default`, `Primary`) and the `SearchButtonStyle` property are unchanged. The old name collided with the generated `AtomUI.Theme.Styling.SearchEditButtonStyle` Semantic Part style class under the shared `atom:` XML namespace, which crashed the XAML compiler (AVLN2000) whenever both were referenced from XAML.

## 2026-08-27

- API
  - Follow the shared input family change: the standard root `Background` / `BorderBrush` now reach `InputControlFrame` through the `AbstractTextInput` relay with inline style semantics, and the SearchEdit theme no longer carries dead owner-level defaults. The full family contract is recorded in the LineEdit family changelog.

## 2026-08-23

- Architecture
  - Align SearchEdit with the shared `AbstractTextInput` logic layer and `InputControlFrame` surface layer.
  - Make `SearchEditDecoratedBox` a search-button layout extension rather than an independent input-surface status owner.
- Theme
  - Route SearchEdit border, background, variant, effective status, CompactSpace and motion through the shared frame theme.
  - Keep the public Button search part as a consumer of the frame effective status, not a validation owner.

记录原则：

- 只记录会影响 SearchEdit 设计理解、兼容边界、实现架构或维护方式的变化。
- 不记录临时讨论、纯格式化或没有长期价值的实现细节。
- 架构文档始终描述最新设计状态；历史变化记录在本文档。

## 2026-08-11

- API
  - Replace `HandleEnterAsSearch` with `IsSearchOnEnterEnabled` and enable Enter-triggered search by default.
  - Replace `SearchButtonClick` with `SearchRequested` and add `SearchRequestedEventArgs` plus `SearchTriggerSource`.
- Behavior
  - Route search-button clicks and unhandled Enter `KeyUp` events through `RaiseSearchRequested()`.
  - Include the query snapshot and `Button` / `EnterKey` trigger source in every search request.
  - Suppress repeated requests while `IsOperating=true` and leave Enter unconsumed when Enter search is disabled.
- Tests
  - Add SearchEdit behavior coverage for defaults, query snapshots, trigger sources and repeated-request suppression.
- Docs
  - Synchronize SearchEdit, LineEdit and AutoComplete contracts with the unified search-request model.

## 2026-08-01

- Theme
  - Give SearchEdit an independent Control identity while keeping it free of Own Tokens.
  - Replace the former LineEdit/Button Token ownership overlap with explicit `SearchEditTokenResource` and `ButtonTokenResource` references.
  - Use public `Button` as `PART_RightAddOn`; keep `SearchButtonTheme` as the typed Semantic Part Theme customization entry.
  - Register `SearchEditTheme.axaml`, `SearchEditDecoratedBoxTheme.axaml` and `SearchButtonTheme.axaml` as independent generated theme assets.
- Docs
  - Align the architecture and implementation documents with exact Control identity, Effective Global Token and generated asset registration.

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `SearchEdit`.
  - Align generated output paths with `controls/search-edit/index-cn.md` and `controls/search-edit/semantic-cn.md`.

## 2026-06-21

- Docs
  - Establish SearchEdit desktop architecture documentation under `docs/controls/desktop/data-entry/search-edit/overview.md`.
  - Add SearchEdit implementation documentation covering search button event flow, SearchEditDecoratedBox, SearchButton, SearchEditPanel layout and maintenance invariants.
  - Document the former shared Token ownership model, superseded by the independent SearchEdit identity design on 2026-08-01.
  - Add SearchEdit changelog and link the document set from the Data Entry category entry.
