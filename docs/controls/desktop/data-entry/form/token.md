# Form Token 设计

本文档定义 `AtomUI.Desktop.Controls.FormToken` 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Form 整体架构见 [Form 桌面版架构设计](overview.md)，内部实现原理见 [Form 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Form Changelog](changelog.md)。

## 1. 定位

FormToken 是 Form 的控件级 Token scope，描述标签视觉、必填标记、冒号间距和表单项布局间距。输入控件自身高度、边框、状态色、图标尺寸、字体基础值和通用 spacing 来自 SharedToken 或对应输入控件 Token。

八字段采用DependencyDefaultsV2、声明修订1；静态默认只依赖有效Global或原常量，没有Own依赖边。名称、类型、公共资源ID和slot保持，LabelColonMargin保留MarginXXS/2与MarginXS的原边分配，默认VerticalLabelMargin仍为0。

FormToken 不承载以下状态：

- `FieldName`、`InitialValues`、提交 values 或业务数据。
- `ValidateStatus`、`ValidateResult`、错误消息、警告消息或验证中状态。
- `FormLayout`、`RequiredMark`、`ValidateTrigger`、`ValidateStrategy` 等实例配置。
- 动态表单项数量、删除按钮显示状态或响应式 breakpoint 当前值。

公开表面Global按exact注册identity消费：Form、FormItem与FormItemDecorator各自独立；FormToken Own仍为既有组合输入。 不新增Own/key或状态字段。

## 2. Token 分类

### 2.1 标签文本 Token

- `LabelColor`
- `LabelFontSize`

这些 Token 控制 `FormItem` 标签文本的颜色和字号。默认值来自 `SharedToken.ColorTextHeading` 和 `SharedToken.FontSize`，使标签与桌面表单语义保持一致。

### 2.2 必填标记 Token

- `LabelRequiredMarkColor`

该 Token 控制默认必填星号颜色。默认值来自 `SharedToken.ColorError`。自定义必填或可选标记内容由 `CustomRequireMark`、`CustomRequireMarkTemplate`、`CustomOptionalMark` 和 `CustomOptionalMarkTemplate` 承载，不由 FormToken 生成具体内容。

### 2.3 标签冒号 Token

- `LabelColonMargin`

该 Token 控制标签冒号的 margin。它只描述冒号与标签内容之间的视觉间距，不决定冒号是否显示。冒号可见性由 `IsShowColon`、`LabelText` 和 `FormItem.Layout` 决定。

### 2.4 表单项间距 Token

- `FormItemSpacing`
- `InlineItemSpacing`

`FormItemSpacing` 控制表单项辅助信息区域和内容之间的垂直间距。`InlineItemSpacing` 控制 `FormLayout=Inline` 时 Form 根 StackPanel 的 item spacing。

### 2.5 垂直标签 Token

- `VerticalLabelPadding`
- `VerticalLabelMargin`

当前内置主题只消费VerticalLabelMargin。VerticalLabelPadding分类为Legacy/CompatibilityRetained：保留Thickness、默认四边0/0/0/有效Global.UniformlyPaddingXS、资源ID和显式配置/读取，内置模板没有消费者，不将其描述为有效的内置padding调节项，也不新增8 DIP默认间距。应用自定义主题仍可显式消费该兼容资源。

## 3. 控件专项模型中的 Token 使用

FormToken 使用路径：

```text
Effective Global / constants
  → FormToken
  → FormTheme / FormItemTheme
  → Form root spacing + FormItem label / marker / colon / help spacing
```

主题引用关系：

| Token | 主要使用点 |
| --- | --- |
| `LabelRequiredMarkColor` | `TextBlock#PART_DefaultRequireMark.Foreground`。 |
| `LabelColor` | `TextBlock#PART_Label.Foreground`。 |
| `LabelFontSize` | `TextBlock#PART_Label.FontSize`。 |
| `LabelColonMargin` | `TextBlock#PART_Colon.Margin`。 |
| `FormItemSpacing` | `PART_ContentLayout.Spacing` 和辅助信息区域最小高度。 |
| `InlineItemSpacing` | `FormLayout=Inline` 时 Form 根 items panel spacing。 |
| `VerticalLabelMargin` | `Layout=Vertical` 时标签区域 margin。 |

运行状态与 Token 分离：

```text
ValidateStatus / Help / ErrorMessageInlines
  → select visual state and message visibility
  → use SharedToken status colors or FormToken spacing
  → do not create per-state FormToken values
```

## 4. 控件家族影响

FormToken 直接影响 Form、FormItem 和 FormItemDecorator 参与的表单布局。它不会改变具体输入控件的高度、padding 或 border，这些由 LineEdit、Select、Cascader、TreeSelect、NumericUpDown 等控件自己的 Token 或 SharedToken 控制。

调整 FormToken 时必须评估：

- `FormTheme.axaml`
- `FormItemTheme.axaml`
- `FormItemDecoratorTheme.axaml`
- Form token.md 语义说明和 Gallery 示例
- `FormShowCasePageTests`

## 5. 兼容性要求

FormToken 属于 Form 主题契约。即使 `FormToken` 是 internal 类型，生成的 `FormTokenKind`、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不把验证结果、业务值、动态表单项数量或当前 breakpoint 写成 Token。
- 不在 FormToken 中复制输入控件的高度、padding、边框、状态色或 focus ring Token。
- 不把 `FormLayout`、`RequiredMark`、`ValidateTrigger` 或 `ValidateStrategy` 变成 Token；它们是实例行为属性。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改标签 Token | 验证水平、垂直、inline 布局下标签颜色、字号、tooltip 和 required mark 对齐。 |
| 修改冒号 Token | 验证 `IsShowColon=true/false`、长标签、右对齐和垂直布局下冒号可见性。 |
| 修改 spacing Token | 验证 Help、error、warning、无消息、inline 布局和动态表单项间距。 |
| 修改垂直标签 Token | 验证 `FormLayout=Vertical` 下标签与输入控件的垂直节奏。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |

## 公开字体与组合输入

表单根和两类 item 使用各自有效 Global 的可选 FontFamily/FontSize；未明确配置时仍继承实际父字体。标签继续 Form.Own.LabelFontSize/LabelColor 配方，附加正文与冒号使用根 Foreground。暗色正文/冒号按有效 ColorText 显示，实例属性与普通 Style 继续由 Avalonia 决定优先级。FormActionsItem 使用 FormItem 的 StyleKey；FormValidateFeedback 是无独立 Token identity 的内容适配表面，继承宿主排版。

这些入口消费有效 Global，不增加 Own 字段、资源 ID 或 slot。Semantic Part 定制仍使用生成的专用 Style；不能通过查找内部节点赋值替代。当前本机验证与未决框架恢复/CT25 边界见 [CT13 家族出口](../../../../superpowers/plans/2026-10-22-custom-theme/input-family-exit-results.md)。
