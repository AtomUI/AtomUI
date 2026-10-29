# 图标按钮变更

## 2026-10-27

- IconButton的loading动画投射跟随owner IsMotionEnabled开关，恢复时使用当前动画与时长，保留内容Icon原生本地值优先级。

## 2026-10-23

- Theme: resolve Global inputs through each public control identity and update Transition duration dynamically.
- Theme: consume ToggleIconButton's existing Normal/Active/Selected/DisabledIconBrush properties in ControlTheme, preserving default colors and native property priority.
- Docs: document shared implementation ownership, independent identities and state-brush semantics; no new Semantic Part contract.
