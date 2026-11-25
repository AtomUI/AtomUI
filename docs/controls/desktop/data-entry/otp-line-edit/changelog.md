# OtpLineEdit Changelog

本文档记录 OtpLineEdit 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-11-20

- Theme
  - Shadcn Luma 默认连排无分隔符的验证码格，内侧接缝为单线，只有首尾外侧保留圆角。显式分隔符和自定义格宽、画刷继续生效。
- Fixed
  - Luma 连排验证码格在悬停、按下或聚焦时，已有的左侧接缝同步变色。

## 2026-11-11

- Theme
  - `OtpLineEditTheme` 设置空 `FocusAdorner`：焦点由当前格的外发光表达（Ant Design Input.OTP），整组外面不再叠加焦点描边。
- Fixed
  - 当前格外发光不再被裁剪：`OtpLineEdit` 与 `ItemsControl#PART_CellsHost` 都派生自 `TemplatedControl`，默认裁剪到自身边界，贴着控件边缘的上、下、左（以及末格右侧）外发光被切掉，只有落在格间距里的部分可见；两者现在都设置 `ClipToBounds=False`。
- Tests
  - `OtpLineEditBehaviorTests` 新增外发光祖先不裁剪测试。
  - 输入格 pressed / active 外发光（含 Error / Warning）只作用于 `Outlined`；Borderless、Filled、Underlined 输入格不绘制阴影。
- Fixed
  - 共享 `BorderRenderHelper` 在背景/边框几何路径中补绘外侧 `BoxShadow`；输入 frame 固定走该路径，`Outlined` 的 focus 外发光此前被丢弃，现在按主题正确绘制。

## 2026-11-10

- Theme
  - OTP cell 状态和清除按钮读取 OtpLineEdit 的有效全局 Token；根边框画刷通过内部投射保持状态优先级；separator 间距跟随 CellGap；CellWidth 改为条件主题绑定，清除后恢复尺寸 Token。

## 2026-11-05

- 实现结构
  - 内部 OtpTextBox 模板与 PART 类型统一使用 Avalonia `TextPresenter`；保留 cell 自绘 caret、只读文本承载和模板重建/卸载清理。

## 2026-09-16

- Behavior
  - Relay the owner root `BorderBrush` onto every cell `InputControlFrame`, so an owner-scoped root setter reaches the cell outlines instead of being silently ignored. `CellBorderBrush` remains the cell-specific override and now takes precedence over the root `BorderBrush`, with the root value as its fallback when cleared. `Background` is deliberately not relayed: the owner's `Background` is a control-level transparent background and relaying it would overwrite the cell theme's fill.

## 2026-08-23

- Architecture
  - Align OtpLineEdit with the shared input-state contract and `InputControlFrame` input-surface layer.
  - Define OTP cells as projections of the root `EffectiveStatus`; cells do not own an independent validation or Form status source.
  - Keep clear-button interaction for the lifetime of the applied template and pair external Form feedback subscriptions with logical attach/detach.
- Theme / Token
  - Route input-surface border, background, focus shadow, disabled, error and warning semantics through `InputControlFrameTheme` and SharedToken.
  - Keep `OtpLineEditToken` limited to cell geometry; separator placement remains a template layout concern.
- Tests
  - Cover clear-button and Form feedback behavior across logical detach/reattach.

## 2026-07-07

- Docs
  - 新增 OtpLineEdit 控件级架构设计、实现原理、Token 设计和 changelog 文档。
- API
  - 定义 `Text`、`Length`、`InputMode`、`Formatter`、`IsMasked`、`Separator`、`Completed` 和 Form 集成契约。
- Theme
  - 定义根模板、cell host、cell、clear button 和 Form feedback 的稳定职责边界。
  - 补齐 `StyleVariant` 的 `Outlined`、`Filled`、`Borderless`、`Underlined` 四种 cell 输入表面契约。
- Token
  - 定义 `OtpLineEditToken` 的 cell 宽度、cell 间距和 separator 间距语义。
