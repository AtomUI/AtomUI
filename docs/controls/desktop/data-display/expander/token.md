# Expander Token 设计

本文档定义 `AtomUI.Desktop.Controls.ExpanderToken` 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Expander 整体架构见 [Expander 桌面版架构设计](overview.md)，内部实现原理见 [Expander 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Expander Changelog](changelog.md)。

## 1. 定位

ExpanderToken 是 Expander 的控件级 Token scope，描述 Header/Content 的默认 padding、背景、整体圆角和展开图标默认外边距。

identity为`AtomUI:Expander`，现有13字段以DependencyDefaultsV2／Revision1静态函数求值。默认只依赖有效Global或常量；Control专属Global进入该owner输入，明确Own优先，删除后恢复当前Global派生值。名称、类型、字段顺序、资源key／枚举数值／dense slot保持，不新增Own依赖或实例状态。

ExpanderToken 不承载以下状态：

- `Header`、`Content`、`AddOnContent` 等实例内容。
- `IsExpanded`、`ExpandDirection`、`TriggerType`、`ExpandIconPosition` 等实例行为状态。
- `HeaderPadding` / `ContentPadding` 的显式用户覆盖值。
- `EffectiveBorderThickness`、`ContentBorderThickness`、`EffectiveExpandButtonMargin` 等运行时派生状态。
- motion 运行状态、cancellation 或临时 transform。

## 2. Token 分类

### 2.1 Header padding Token

- `HeaderPadding`
- `HeaderPaddingSM`
- `HeaderPaddingLG`

这些 Token 控制默认 Header 内边距。`SizeType=Middle` 和 `SizeType=Custom` 使用 `HeaderPadding`，`SizeType=Small` 使用 `HeaderPaddingSM`，`SizeType=Large` 使用 `HeaderPaddingLG`。当实例显式设置 `HeaderPadding` 时，主题进入 `:custom-header-padding` 分支并跳过这些默认值。

三项默认分别为`Thickness(UniformlyPadding,UniformlyPaddingSM)`、`Thickness(UniformlyPaddingSM,UniformlyPaddingXS)`、`Thickness(UniformlyPaddingLG,UniformlyPadding)`，单位DIP。

### 2.2 Content padding Token

- `ContentPadding`
- `ContentPaddingSM`
- `ContentPaddingLG`

这些 Token 控制默认 Content 内边距。`SizeType=Middle` 和 `SizeType=Custom` 使用 `ContentPadding`，`SizeType=Small` 使用 `ContentPaddingSM`，`SizeType=Large` 使用 `ContentPaddingLG`。当实例显式设置 `ContentPadding` 时，主题进入 `:custom-content-padding` 分支并跳过这些默认值。

默认分别为`Thickness(16,UniformlyPadding)`、Global.PaddingSM和Global.PaddingLG；水平常量16保持原义，后两项保留完整Thickness。明确0不当作缺省，实例可空padding在非空时优先。

### 2.3 背景 Token

- `HeaderBg`
- `ContentBg`

`HeaderBg` 默认为Global.ColorFillAlter，用于Header及Borderless内容背景分支。`ContentBg` 默认为Global.ColorBgContainer，当前实际消费者为Ghost的Header背景；不把字段名当作默认body背景接线。

### 2.4 边框与圆角 Token

- `ExpanderBorderRadius`

`ExpanderBorderRadius` 默认为Global.BorderRadiusLG，控制 `PART_Frame` 圆角。边框颜色和边框厚度使用 SharedToken，不在 ExpanderToken 中重复定义。

### 2.5 展开图标外边距 Token

- `LeftExpandButtonHMargin`
- `RightExpandButtonHMargin`
- `LeftExpandButtonVMargin`
- `RightExpandButtonVMargin`

水平外边距Token控制默认Header布局下图标与文本距离，Start默认右边距UniformlyMarginSM、End默认左边距UniformlyMarginSM。四方向仍旋转原Header，并消费这两项HMargin。

两个VMargin仅兼容保留：默认分别为`Thickness(0,0,0,UniformlyMarginSM)`和`Thickness(0,UniformlyMarginSM,0,0)`，保留资源及配置往返，当前模板未消费，不标有效编辑项。不得删除、改名、复用为其他spacing或借迁移新增垂直图标布局。

实例显式设置 `HeaderPadding` 时，展开图标外边距由 `EffectiveExpandButtonMargin` 从 `HeaderPadding.Left` 或 `HeaderPadding.Right` 推导，而不是直接使用这些默认 Token。

## 3. 控件专项模型中的 Token 使用

Token 使用路径：

```text
有效Global／常量（明确Own优先）
   ↓
ExpanderToken
   ↓
ExpanderTheme.axaml
   ↓
PART_Frame / PART_HeaderDecorator / PART_ExpandButton / PART_ContentPresenter
```

重要映射：

| Token | 主要使用点 |
| --- | --- |
| `HeaderPaddingLG` / `HeaderPadding` / `HeaderPaddingSM` | `:not(:custom-header-padding)` 下按 SizeType 设置 `PART_HeaderDecorator.Padding`。 |
| `ContentPaddingLG` / `ContentPadding` / `ContentPaddingSM` | `:not(:custom-content-padding)` 下按 SizeType 设置 `PART_ContentPresenter.Padding`。 |
| `HeaderBg` | `PART_HeaderDecorator.Background`，Borderless 内容背景分支。 |
| `ContentBg` | Ghost Header 背景分支。 |
| `ExpanderBorderRadius` | `PART_Frame.CornerRadius`。 |
| `LeftExpandButtonHMargin` | 默认 `ExpandIconPosition=Start` 下的 `PART_ExpandButton.Margin`。 |
| `RightExpandButtonHMargin` | 默认 `ExpandIconPosition=End` 下的 `PART_ExpandButton.Margin`。 |

实例状态派生：

```text
SizeType
  → HeaderPadding* / ContentPadding*

HeaderPadding != null
  → EffectiveExpandButtonMargin
  → bypass default expand button margin token

IsBorderless / IsGhostStyle
  → EffectiveBorderThickness
```

`EffectiveExpandButtonMargin` 和 `EffectiveBorderThickness` 都是运行时派生值，不是 Token。

## 4. 控件家族影响

ExpanderToken 只直接影响 `AtomUI.Desktop.Controls.Expander` 主题和 Expander token.md 语义说明。

Collapse 拥有独立的 CollapseToken 和多面板布局模型。Expander 不复用 CollapseToken，Collapse 也不应直接依赖 ExpanderToken。若两个控件需要共享某个 spacing 语义，应评估是否上升到 SharedToken，而不是跨控件引用控件 Token。

## 5. 兼容性要求

ExpanderToken 属于 Expander 主题契约。即使 `ExpanderToken` 是 internal 类型，生成的 token kind、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不把 `IsExpanded`、`ExpandDirection`、`TriggerType`、`ExpandIconPosition` 等实例行为写成 Token。
- 不把显式 `HeaderPadding` / `ContentPadding` 的实例值写回 Token。
- 不把 motion 状态、动画临时高度或 transform 写成 Token。
- 修改 Header/Content padding 时必须验证 Large、Middle、Small、Custom 和显式 padding 分支。
- 修改展开图标 margin 时必须验证 `ExpandIconPosition=Start/End` 和自定义 HeaderPadding。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 Header padding Token | 验证 Large / Middle / Small / Custom 下 Header 高度、文字垂直居中和图标间距。 |
| 修改 Content padding Token | 验证 Content 文本内边距、展开/收起动画目标尺寸和 Ghost/Borderless 模式。 |
| 修改背景 Token | 验证普通、Borderless 和 Ghost 示例背景。 |
| 修改圆角 Token | 验证普通边框模式下 `PART_Frame` 圆角和裁剪。 |
| 修改展开图标 margin Token | 验证 Start/End 图标位置、AddOnContent、Header 文本间距和自定义 HeaderPadding 分支。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
