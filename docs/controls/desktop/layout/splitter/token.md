# Splitter Token 设计

本文档定义 Splitter 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Splitter 整体架构见 [Splitter 桌面版架构设计](overview.md)，内部实现原理见 [Splitter 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Splitter Changelog](changelog.md)。

## 1. 定位

Splitter Token 只表达组件级视觉变量，包括分割线尺寸、拖拽命中区、折叠按钮定位、handle 颜色和图标尺寸。Token 不承载运行时拖拽状态、折叠状态、面板尺寸、业务布局数据或用户内容背景。

当前 Token scope：

- `SplitterToken`，scope id 为 `Splitter`，源码位于 `src/AtomUI.Desktop.Controls/Splitter/SplitterToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 命中区域 | 分割边界可拖拽区域和整体 handle 占位。 | `SplitTriggerSize`、`SplitBarHandleSize` |
| 可见分割线 | 分割线默认厚度和 grip 厚度。 | `SplitBarSize`、`HandleLineThickness` |
| 拖拽提示 | grip 沿主轴的可见长度。 | `SplitBarDraggableSize` |
| 折叠按钮定位 | 折叠按钮相对 handle 中心的偏移。 | `SplitBarCollapseOffset`、`SplitBarCollapseOffsetNegative`、`SplitBarCollapseCrossOffset` |
| 颜色与状态视觉 | 普通、hover、dragging 和图标状态颜色。 | `HandleLineColor`、`HandleLineHoverColor`、`HandleLineDragColor`、`HandleIconColor`、`HandleIconHoverColor`、`HandleIconPressedColor` |
| 图标尺寸 | 折叠按钮图标尺寸。 | `HandleIconSize` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. Token 语义边界

### 3.1 命中区域与可见线条

Splitter 必须区分两种尺寸：

- hit area：用户可拖拽区域，由 `HandleSize` public API 和 `SplitBarHandleSize` token 影响。
- visible line：用户看到的分割线或 grip，由 `LineThickness` 语义和 `HandleLineThickness` / `SplitBarSize` token 影响。

这两种尺寸不能互相替代。增大可见分割线不应自动改变面板布局占位；增大拖拽命中区也不应让分割线变粗。

### 3.2 SplitBarSize 与 HandleLineThickness

`SplitBarSize` 是历史上表达可见 split bar 尺寸的 token。`HandleLineThickness` 是更明确的 handle line 语义 token。主题消费可见线条厚度时应以 `HandleLineThickness` 作为语义入口，`SplitBarSize` 保留为默认值计算和兼容来源。

维护要求：

- `PART_HandleLine` 的宽度或高度应来自 handle 的 `LineThickness`。
- `PART_Grip` 的短轴厚度应来自 drag bar 的 `LineThickness`。
- handle 和 drag bar 的 `LineThickness` 默认值应能从 Splitter Token 派生。
- 不在主题里绕过 `LineThickness` 直接写死 `SplitBarSize`，否则实例级样式 API 无法覆盖。
- `SplitBarSize` 同时是面板之间的间距（handle spacing）：`SplitterPanel` 挂载期以 `TokenResourceBinder` 绑定到内部属性
  `SplitBarSize` 并标记 `AffectsMeasure`，主题或 `Controls[Splitter]` 变化后立即重新测量（此前每次测量用资源查找读取一次，变化后不重测）。
- `SplitTriggerSize` 是拖拽条（命中区）宽度，`SplitBarHandleSize` 是 handle 宽度，二者都不绘制可见像素；生效测试断言布局后的尺寸。
- handle、drag bar 是无 identity 的单宿主部件，主题以 `SplitterTokenResource` 读取全局 Token（`BorderRadiusXS`、`ColorFill`、
  `ControlItemBg*`、`FontSizeSM`、`ZIndexPopupBase`），`Controls[Splitter]` 覆盖生效。

### 3.3 圆角

Splitter 当前没有专属圆角 token。根框架圆角应优先复用 `CornerRadius` 基础属性或 SharedToken；可见分割线和 grip 圆角应通过实例级 `LineCornerRadius` 语义或 SharedToken 映射。不要为子面板圆角新增 Splitter Token，子面板圆角属于用户内容容器。

## 4. 控件专项模型中的 Token 使用

Splitter 的 Theme 消费路径：

```text
SplitterToken / SharedToken
  -> Splitter root default values
  -> SplitterPanel internal propagation
  -> SplitterHandle line state
  -> SplitterDragBar grip state
  -> PART_HandleLine / PART_Grip / collapse IconButton
```

维护规则：

- C# 控件负责状态归一和 internal handle 状态传递。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、尺寸、圆角和状态视觉。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 5. 控件家族影响

调整 Splitter Token 时必须评估以下范围：

- `Splitter` 根控件默认值。
- `SplitterPanel` 生成 handle 时的状态传递。
- `SplitterHandleTheme.axaml` 中 `PART_HandleLine` 的厚度、颜色和状态 selector。
- `SplitterDragBarTheme.axaml` 中 `PART_Grip` 的短轴厚度、主轴长度和圆角。
- 折叠按钮尺寸、定位和 hover/pressed 颜色。
- Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Desktop/Browser 宿主和不同 orientation。

## 6. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不把 `HandleSize` 的默认值和可见线条厚度绑定成同一个语义。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。
- 若实例级样式 API 能表达单控件差异，不应为每个局部视觉差异新增 Token。

## 7. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 normal、hover、dragging、disabled、collapsed 等状态视觉。 |
| 线条样式 | 检查 `PART_HandleLine` 和 `PART_Grip` 是否同时响应 line thickness 和 line corner radius。 |
