# TreeSelect Token 设计

本文档定义 `AtomUI.Desktop.Controls.TreeSelectToken` 的专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。TreeSelect 整体架构见 [TreeSelect 桌面版架构设计](overview.md)，内部实现原理见 [TreeSelect 桌面版实现原理](implementation.md)，设计和契约变化记录见 [TreeSelect Changelog](changelog.md)。

## 1. 定位

TreeSelectToken 是 TreeSelect 的控件级 Token scope，目前只承载 TreeSelect 候选弹层的最小宽度下限。输入表面的通用边框、圆角、effective status、focus ring、disabled 背景和 AddOn 结构来自 `InputControlFrameTheme`、SharedToken 和 PopupHostToken；多选 tag、popup padding 和输入内容 padding 复用 SelectToken。

TreeSelectToken 不承载以下状态：

- `ItemsSource`、`Items`、`SelectedItem`、`SelectedItems` 等数据状态。
- `FilterValue`、`IsDropDownOpen`、`IsTreeCheckable`、`IsMultiple`、`Status` 等运行状态。
- 树节点 hover、pressed、selected、checked、expanded、disabled 的当前实例状态。
- Popup 当前宽度、高度、offset、placement 或异步加载结果。

## 2. Token 分类

### 2.1 弹层宽度 Token

- `MinPopupWidth`

该 Token 控制 TreeSelect 候选弹层的最小宽度下限，默认值为 `300`。当 `IsPopupMatchSelectWidth=true` 时，实际有效宽度还会受到输入控件当前宽度影响；当输入控件较窄时，`MinPopupWidth` 保证树形层级和节点文本有基本展示空间。

## 3. 控件专项模型中的 Token 使用

TreeSelectToken 使用路径：

```text
SharedToken / PopupHostToken / SelectToken
   ↓
TreeSelectToken
   ↓
TreeSelectTheme PopupFrame selector
   ↓
候选弹层最小宽度
```

运行时派生尺寸：

```text
EffectivePopupWidth = IsPopupMatchSelectWidth ? TreeSelect.Bounds.Width : 0
PopupFrame.MinWidth = max(EffectivePopupWidth, TreeSelectToken.MinPopupWidth)
```

`EffectivePopupWidth`、`MaxPopupHeight`、`SelectedCount`、`IsSelectionEmpty` 和 `EffectiveSelectedItems` 都是实例运行状态，不是 Token。

## 4. 控件家族影响

TreeSelectToken 直接影响 TreeSelect。TreeSelect 还复用以下 Token 系统：

- `SelectToken`：popup padding、多选 tag 高度和输入内容 padding。
- `InputControlFrameTheme` / `SharedToken`：输入壳体边框、圆角、状态和 CompactSpace。
- `PopupHostToken`：popup 圆角、阴影和 anchor margin。
- `SharedToken`：字体、颜色、间距、控件高度和 motion。

调整 TreeSelectToken 时必须评估：

- `TreeSelectTheme.axaml`
- TreeSelect token.md 语义说明
- `TreeSelectShowCasePageTests`
- TreeSelect popup 在单选、多选、勾选、过滤和异步加载场景下的宽度表现

## 5. 兼容性要求

TreeSelectToken 属于 TreeSelect 主题契约。即使 `TreeSelectToken` 是 internal 类型，生成的 `TreeSelectTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不把实例数据、过滤状态、loading 状态、选择集合、popup 打开状态或 Form 状态迁移为 Token。
- 不在 TreeSelectToken 中复制 SelectToken、InputControlFrameTheme 或 PopupHostToken 的通用职责。
- 不把 `DisplayPageSize`、`MaxCount`、`ShowCheckedStrategy` 或 `IsTreeCheckable` 变成 Token；它们是实例行为属性。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 `MinPopupWidth` | 验证单选、多选、勾选、过滤和长节点文本下 popup 宽度。 |
| 调整 TreeSelect 对 SelectToken 的使用 | 验证 tag 高度、tag 间距、popup padding 和 Large / Middle / Small / Custom 尺寸。 |
| 调整 PopupHostToken 协作 | 验证 popup 圆角、阴影、anchor margin 和 overlay popup。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |

## 根几何与 Effective Global 消费

公开 owner 的 `Padding`、`BorderThickness`、`CornerRadius` 默认值从该 Control 的 Effective Global 读取，再以原生主题优先级投射到实际框体。共享框体继续负责状态色、focus/hover shadow、variant 与 CompactSpace；不能把它描述为跳过 owner 的全局几何入口。字号和其他消费链按各控件当前模板分别定义。本次未增加 Own 字段或改变 Token identity。详见 [共享输入架构的根几何合同](../input-control-architecture-design.md#81-根表面几何与预设默认值)。
