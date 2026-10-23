# OtpLineEdit Changelog

本文档记录 OtpLineEdit 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-23

- 追加独立CaretBlinkDuration/CaretBlinkEasing Own及V2修订2，保留旧四ID，默认1s线性往返不变；非正周期为静态可见输入反馈，装饰motion开关不改变既有光标节奏。三个关键帧保持AXAML声明，每光标持有原生动画Style，隐藏暂停、detach退役及可变缓动隔离遵循框架生命周期。
- 将caret显隐移入ControlTheme，保持活动且启用的合同；修复主题字号热更新后，已填字符的caret右移量停留在旧字号的问题，保留既有默认几何与blink曲线。
- 光标宽度、下划线底边和分隔符左间距接入有效Global，默认1/1/4不变；分隔符默认Margin移至共享ControlTheme，允许专用Style覆盖。
- 修正cell/separator部件路由以越过生成容器，补全部件正式契约及编译AXAML命中验证。
- 根与cell的45处资源改为OtpLineEdit有效Global，接通三档高度/字号、状态颜色、选区、占位符、操作间距及motion，保留四个Own和原生属性优先级。

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
