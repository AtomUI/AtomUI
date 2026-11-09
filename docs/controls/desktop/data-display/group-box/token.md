# GroupBox Token 设计

关联文档：[架构设计](overview.md)、[实现原理](implementation.md)、[Changelog](changelog.md)、[AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

GroupBox Token 将全局 SharedToken 转换为 GroupBox 可消费的组件级结构值，主要覆盖 Header 和内容区域的间距。颜色、边框厚度、圆角和字体基础值直接来自 SharedToken，不在 GroupBox Token 中重复定义。

GroupBox Token 不表达实例状态，也不负责 Header 缺口的运行时 bounds。Header 缺口由模板测量结果和控件渲染模型共同决定。

## 2. Token 分类

GroupBox Token 按语义分为两类。

### 内容区域 Token

| Token | 类型 | 用途 |
| --- | --- | --- |
| `ContentPadding` | `Thickness` | 内容区域内边距，应用到 `PART_ContentPresenter` 的 `Margin`。 |

### Header 结构 Token

| Token | 类型 | 用途 |
| --- | --- | --- |
| `HeaderContainerMargin` | `Thickness` | Header 容器外边距，决定 Header 行与 GroupBox 外边界的距离。 |
| `HeaderContentPadding` | `Thickness` | Header 内容内边距，扩大标题缺口和标题文字/图标周围留白。 |
| `HeaderIconMargin` | `Thickness` | Header 图标与标题之间的间距。 |

原 `TextPaddingInline`、`OrientationMarginPercent`、`VerticalMarginInline` 复制自 Separator，从未被消费；GroupBox 的标题缺口与标题到
边缘的距离由 `HeaderContentPadding` / `HeaderContainerMargin` 表达，GroupBox 也没有 antd 对应组件，三者已删除（见 [Changelog](changelog.md)）。

颜色、边框厚度、圆角、字号与图标尺寸是全局 Token（`ColorBgContainer`、`ColorBorder`、`BorderThickness`、`BorderRadius`、`ColorText`、
`FontSize`、`IconSizeLG`），经 `GroupBoxTokenResource` 读取，`Controls[GroupBox]` 覆盖生效。

## 3. 控件专项模型中的 Token 使用

Header 缺口模型使用 `HeaderContentPadding` 扩展 `PART_HeaderContent` 的实际 bounds。缺口宽度应随 Header 内容实际布局结果变化，而不是直接读取 Token 计算固定宽度。

Token 与渲染模型的关系：

```text
HeaderContainerMargin / HeaderContentPadding / HeaderIconMargin
        ↓
Template measure / arrange
        ↓
PART_HeaderContent bounds
        ↓
Header gap geometry
        ↓
GroupBox border geometry
```

因此，修改 Header 相关 Token 会影响布局和缺口尺寸；控件渲染不能缓存不含 Token 变化的过期 Header bounds。

## 4. 控件家族影响

GroupBox 没有派生控件家族，Token 只影响 `AtomUI.Desktop.Controls.GroupBox` 主题。GroupBox token.md 语义说明应与 `GroupBoxToken` 保持一致。

Browser theme 或桌面 theme 引用 GroupBox Theme 时，应继续通过 `GroupBoxTokenResource` 获取 GroupBox 专属间距，不在主题中复制固定数值。

## 5. 兼容性要求

GroupBox Token 是主题契约的一部分。维护时必须遵守：

- 不擅自重命名、删除或复用既有 Token。
- 不把 `Background`、`BorderBrush`、`HeaderTitleColor` 等已有公共属性迁移为 GroupBox Token。
- 不把 Header 实际 bounds、标题位置或图标可见性写成 Token。
- 不在 Token 中定义透明背景下的特殊遮挡颜色。
- 修改 Token 默认值时必须验证 Header 缺口、内容内边距、Header 左/中/右对齐和图标间距。

## 6. 验证策略

GroupBox Token 相关变更至少验证：

| 验证项 | 要求 |
| --- | --- |
| Token 生成 | `GroupBoxTokenKind` 包含新增或修改后的 Token。 |
| Theme 引用 | `GroupBoxTheme.axaml` 使用 `GroupBoxTokenResource`，不复制固定间距值。 |
| Token 文档 | GroupBox token.md 语义说明与 `GroupBoxToken` 属性一致。 |
| 视觉 | Header 左/中/右位置、带图标 Header、透明背景 Header 缺口均正常。 |
| 文档 | 本文件、`overview.md` 和 `changelog.md` 的 Token 描述一致。 |
