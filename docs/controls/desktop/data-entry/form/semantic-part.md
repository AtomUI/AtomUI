# Form Semantic Part 契约

本文档定义 Form 控件家族公开的 Semantic Part、Selector、类型约束、数量语义和定制边界。控件整体设计见
[Form 桌面版架构设计](overview.md)，真实模板、状态投影与布局边界见
[Form 桌面版实现原理](implementation.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

Form 家族的 Semantic Part 契约由 `FormItem` 声明：`root`、`label`、`content`、`extra`、`help`、`helpItem` 六个职责区域
（§1.1–1.6）。除 `helpItem` 外均为 `Single`，且来自 `FormItemTheme.axaml` 的唯一内置模板——
`Layout`、`FormLayout`、`RequiredMark`、验证状态和 `IsHideItemLabel` 只改变布局排列、可见性或有效视觉值，
不增删模板节点。`helpItem` 是运行时逐条创建的消息节点，数量随验证状态和 `Help` 变化（§1.6）。

`Form` 容器自身不注册 descriptor。上游稳定发布基线（6.6.0）的 Form semantic API（`root`、`label`、`content`、`help`、
`helpItem`、`extra`）中，form 级仅 `root` 落在表单根元素上；在 AtomUI 中对 Form owner 的整体定制通过
Avalonia 原生 owner 样式（`atom|Form` 类型 selector、ControlTheme、实例 Styles）直接完成，不需要
`.semantic-*` 契约。字段级语义（label、content、help、extra）全部由 `FormItem` 的模板节点承载。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `FormItem` |
| Part | `root` |
| Selector | FormItem 本身 |
| SelectorRoute | 不适用 |
| ContractType | `FormItem` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | FormItem owner |
| 职责 | 承载字段布局、验证状态、内容接入和 owner-scoped Semantic Style 入口。 |
| 相关 API | `Layout`、`LabelAlign`、`ValidateStatus`、`ValidateResult`、`IsRequired`、`Content` |
| 相关 Token | `FormToken`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是控件自身，不声明 `.semantic-root` marker。它适合定制 FormItem 整体 `Margin`、`Opacity`、对齐和
尺寸约束；标签列与内容列的 Grid 几何由 `PART_BodyLayout` 布局算法拥有（见实现原理 §7.1），不通过 Semantic
Style 改写。`FormActionsItem` 复用 FormItem 模板，`atom|FormItem` 类型 selector 同样命中它，不注册独立
descriptor。

### 1.2 `label`

| 字段 | 值 |
| --- | --- |
| Owner | `FormItem` |
| Part | `label` |
| Selector | `.semantic-label` |
| SelectorRoute | `/template/ .semantic-label` |
| Style Type | `FormItemLabelStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#PART_Label`（`PART_LabelContentLayout` 内的标签文本） |
| 职责 | 承载 `LabelText` 的文本呈现，包含颜色、字号、对齐和换行。 |
| 相关 API | `LabelText`、`LabelAlign`、`LabelWrapping`、`IsHideItemLabel` |
| 相关 Token | `LabelColor`、`LabelFontSize` |
| 稳定性 | stable since 6.2.0 |

`label` 对齐上游 `classNames.label` 的文本语义：标记（冒号、必填星号、可选文案、tooltip 图标和自定义
mark）拥有各自的 token 驱动样式，不属于 `label` Part。它适合定制 `Foreground`、`FontSize`、`TextAlignment`、
`Opacity` 和 `Margin`；默认主题的 `LabelColor`、`LabelFontSize` 和 `LabelAlign` selector 可以被同优先级的用户
Semantic Style 覆盖。

布局边界：水平布局下 `PART_Label` 的 `MaxWidth` 通过 `TemplateBinding` 投影自 `LabelMaxWidth`（由
`LayoutUpdated` 测量标签列宽后写入）。用户 Setter 覆盖 `Width` / `MaxWidth` 会中断该测量闭环，导致换行与
裁剪行为退化；这两个属性槽不推荐定制，标签列宽度应通过 `LabelColInfo` API 控制。

### 1.3 `content`

| 字段 | 值 |
| --- | --- |
| Owner | `FormItem` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `FormItemContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#ContentPresenter`（`ContentFrame` 内主内容 presenter） |
| 职责 | 承载 `Content` 输入控件的最终呈现位置。 |
| 相关 API | `Content`、`IsValidateContentType`、`ChildrenSpacing` |
| 相关 Token | `FormItemSpacing`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`content` 对齐上游 `classNames.content` 语义，是内容呈现区域，
不等于用户 `Content` 子控件本身。适合定制 `Margin`、`Opacity`、`VerticalAlignment` 和 presenter 级排版属性；
`Content` 创建的输入控件子树（LineEdit、Select 等自身的模板与 Semantic Part）不属于本 Part，继续由各自
owner 契约拥有。

布局边界：presenter 的 `MaxWidth` 投影自 `ContentPresenterMaxWidth`（`LayoutUpdated` 测量内容列宽后写入，
`FormLayout=Inline` 时为正无穷）。用户 Setter 覆盖 `Width` / `MaxWidth` 会中断测量闭环；内容列宽度应通过
`WrapperColInfo` API 控制。`ContentFrame` 的最小高度与标签列对齐，属于布局算法内部节点。

### 1.4 `extra`

| 字段 | 值 |
| --- | --- |
| Owner | `FormItem` |
| Part | `extra` |
| Selector | `.semantic-extra` |
| SelectorRoute | `/template/ .semantic-extra` |
| Style Type | `FormItemExtraStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#ExtraPresenter`（`PART_ContentLayout` 内 help 区域之后的 Extra presenter） |
| 职责 | 承载 `Extra` 与 `ExtraTemplate` 的最终呈现。 |
| 相关 API | `Extra`、`ExtraTemplate` |
| 相关 Token | SharedToken `ColorTextDescription`、`ControlHeightSM` |
| 稳定性 | stable since 6.2.0 |

`extra` 对齐上游 `classNames.extra` 语义，是 `Extra` API 的呈现区域。呈现位置与上游一致：位于输入控件
与 help 区域（`additional` 区）下方、与内容列对齐，不占用控件水平空间；默认主题使用说明文字色
（`ColorTextDescription`）与 `ControlHeightSM` 最小高度。`ExtraTemplate` 创建的用户子树不属于本 Part。
`Extra=null` 时 presenter 隐藏（`IsVisible` 绑定 `Extra` 非空），仍属于静态模板结构，marker 与 `Single`
数量不变。

### 1.5 `help`

| 字段 | 值 |
| --- | --- |
| Owner | `FormItem` |
| Part | `help` |
| Selector | `.semantic-help` |
| SelectorRoute | `/template/ .semantic-help` |
| Style Type | `FormItemHelpStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `StackPanel#ExtraInfoLayout`（`PART_ContentLayout` 内消息区域） |
| 职责 | 承载验证消息与 `Help` 文案的聚合展示区域。 |
| 相关 API | `Help`、`ValidateStatus`、`ErrorMessageForeground`、`WarningMessageForeground` |
| 相关 Token | `FormItemSpacing`、`ColorErrorText`、`ColorWarningText`、`ColorTextDescription` |
| 稳定性 | stable since 6.2.0 |

`help` 对齐上游 `classNames.help`（ErrorList 根节点）语义，覆盖同一段视觉职责：验证错误消息、警告
消息和 `Help` 帮助文案共同居住在该区域，逐条内容以 `helpItem` 节点呈现（§1.6）。`HasErrorOrWarningMsg=False`
时默认主题把该区域折叠为 `MaxHeight=0` 并由 `PART_ContentLayout` 的 `FormItemSpacing` 收紧间距；容器节点与
marker 始终存在，数量语义为 `Single`。

消息文本颜色由 `ValidateStatus` selector（`ColorErrorText` / `ColorWarningText` / `ColorTextDescription`）和
`ErrorMessageForeground` / `WarningMessageForeground` API 拥有，`help` 容器 Setter 不改变消息着色。适合定制
`Margin`、`Spacing`、`Opacity` 和对齐。

### 1.6 `helpItem`

| 字段 | 值 |
| --- | --- |
| Owner | `FormItem` |
| Part | `helpItem` |
| Selector | `.semantic-help-item` |
| SelectorRoute | `/template/ .semantic-help > .semantic-help-item` |
| Style Type | `FormItemHelpItemStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ExtraInfoLayout` 内逐条消息 `TextBlock`（运行时创建）与静态 `TextBlock#HelpText` |
| 职责 | 承载单条验证错误、警告消息或 `Help` 帮助文案的文本呈现。 |
| 相关 API | `Help`、`ValidateStatus`、`ErrorMessageForeground`、`WarningMessageForeground` |
| 相关 Token | `ColorErrorText`、`ColorWarningText`、`ColorTextDescription` |
| 稳定性 | stable since 6.2.0 |

`helpItem` 对齐上游 `classNames.helpItem`（ErrorList 逐条消息项）语义。FormItem 在验证结果变化时以代码逐条
创建消息 `TextBlock`，使用生成的 semantic class 常量添加 marker，并按"错误与警告消息在前、`Help` 文案在后"
的顺序排列；静态 `HelpText` 节点同样携带 marker，是 `Help` 文案的固定实例。消息节点带显式
`Foreground`（来自 `ErrorMessageForeground` / `WarningMessageForeground`），`HelpText` 着色由 `ValidateStatus`
selector 拥有；Semantic Style 服从 Avalonia 原生优先级，可以按需覆盖。

运行时边界：消息节点由验证结果应用的统一写入点创建、重建和清空，reset、detach 与新一轮验证不会遗留旧节点或
旧 marker；节点是 `ExtraInfoLayout` 的直接子节点，不进入 logical tree，不持有验证状态。适合定制 `FontSize`、
`Margin`、`Opacity` 和文本排版属性。

## 2. Owner 与家族边界

| 类型 | descriptor | 说明 |
| --- | --- | --- |
| `FormItem` | 有（本契约） | 字段级语义 owner；`root` 由生成器隐式加入。 |
| `FormActionsItem` | 复用 FormItem | 继承 FormItem 模板与契约，不声明独立 Part。 |
| `Form` | 无 | form 级 `root` 走 Avalonia 原生 owner 样式；无 `.semantic-*` 区域。 |
| `FormItemDecorator` | 无 | 组合适配器，无上游对应 semantic API。 |
| `FormValidateFeedback` | 无 | 运行时反馈内容承载，状态驱动内容切换。 |
| `SubmitButton` / `ResetButton` | 无 | 复用 Button ControlTheme；视觉契约归 Button owner。 |
| `ItemDeleteButton` | 无 | internal IconButton，不属于公共契约。 |

上游基线 `FormSemanticType` 的 `helpItem`（逐条消息项）采纳为运行时创建的 `Multiple` Part：验证错误与警告
消息以逐条 `TextBlock` 呈现，静态 `HelpText` 是 `Help` 文案的固定 `helpItem` 实例；消息构建与重建的统一写入点
见实现原理 §7.4。

## 3. Selector 用法

生成的 Style Type 已封装 FormItem owner 类型保护与 owner-relative route，应用不直接复制 `/template/`
路径：

```xml
<Style Selector="atom|FormItem.semantic-style-demo">
    <atom:FormItemLabelStyle x:SetterTargetType="TextBlock">
        <Setter Property="Foreground" Value="#1677FF" />
    </atom:FormItemLabelStyle>
    <atom:FormItemContentStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Opacity" Value="0.9" />
    </atom:FormItemContentStyle>
    <atom:FormItemExtraStyle x:SetterTargetType="ContentPresenter">
        <Setter Property="Opacity" Value="0.82" />
    </atom:FormItemExtraStyle>
    <atom:FormItemHelpStyle x:SetterTargetType="StackPanel">
        <Setter Property="Margin" Value="0,2,0,0" />
    </atom:FormItemHelpStyle>
    <atom:FormItemHelpItemStyle x:SetterTargetType="TextBlock">
        <Setter Property="Opacity" Value="0.88" />
    </atom:FormItemHelpItemStyle>
</Style>
```

作用域规则沿用系统约定：全局规则放 Application.Styles，局部规则放 Window / UserControl 样式宿主，单实例
规则放 owner 的 `Styles`，多实例共享时在 FormItem 上加业务 class 后用
`atom|FormItem.compact-form` 收窄。不得使用以下写法：

- `.semantic-root`、`PART_*`、Name selector、internal 类型或视觉祖先顺序作为应用主题契约。
- 把 `TextBlock.semantic-label`、`ContentPresenter.semantic-content` 等 `ContractType` 写入 Part 身份
  selector。
- 直接复制 `/template/ .semantic-*` route；route 只由生成的 Style Type 封装。
- 穿过 `Content`、`ExtraTemplate` 或自定义 mark 模板创建的用户内容继续匹配内部 Visual。
- 把 `Form`、`FormItemDecorator` 或 `SubmitButton` 当作这些 Part 的 owner；它们不注册 descriptor。

## 4. 状态与数量语义

五个静态 Part 均为 `Single`；`helpItem` 为 `Multiple`，数量随验证消息与 `Help` 变化。`Layout`、`FormLayout`、
`RequiredMark`、验证状态、`IsHideItemLabel` 和删除按钮只改变布局排列、可见性或折叠，不增删静态 marker，也不
改变对象身份：

| 场景 | root | label | content | extra | help | helpItem | 说明 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 默认水平 / 垂直布局 | 1 | 1 | 1 | 1 | 1（折叠） | 0 | 静态节点均已实例化。 |
| `FormLayout=Inline` | 1 | 1 | 1 | 1 | 1 | 0 | 只改变 Form 根排列方向。 |
| `IsHideItemLabel=true` | 1 | 1（标签列不布局） | 1 | 1 | 1 | 0 | `PART_LabelLayout` 节点保留，仅不参与 Grid 布局。 |
| `LabelText=null` | 1 | 1（隐藏） | 1 | 1 | 1 | 0 | 可见性由 `IsNotNull` converter 切换。 |
| `Extra=null` | 1 | 1 | 1 | 1（空） | 1 | 0 | presenter 保持存在。 |
| 验证 Error / Warning（N 条消息） | 1 | 1 | 1 | 1 | 1（展开、着色） | N | 消息节点随结果重建。 |
| 仅有 `Help` 文案 | 1 | 1 | 1 | 1 | 1（展开） | 1（静态 HelpText） | `Help` 是固定实例。 |
| `Help` + N 条消息 | 1 | 1 | 1 | 1 | 1 | N+1 | 消息在前、`Help` 在后。 |
| 验证通过 / reset | 1 | 1 | 1 | 1 | 1（折叠） | 0 或 1 | 运行时消息节点清空，`HelpText` 随 `Help` 保留。 |
| `FormActionsItem` | 1 | 1 | 1 | 1 | 1 | 0 | 复用同一模板，类型 selector 命中子类。 |
| 动态增删 FormItem | 1/实例 | 1/实例 | 1/实例 | 1/实例 | 1/实例 | 0..N/实例 | Part 契约随 FormItem 实例生命周期。 |

动态表单项（`IsShowItemDeleteButton` / `DeleteFormItem`）由 `Form` 的 ItemsControl 容器生命周期管理；
每个 FormItem 实例化或删除时 marker 随模板一起创建和释放，不涉及容器回收复用中的 marker 漂移（FormItem
容器不复用为其他 owner 类型）。运行时消息节点只经历创建、重建与清空，不经历跨 owner 复用。

## 5. 定制边界

以下区域明确不属于 Form 家族 Semantic Part：

- 标签附属标记：冒号（`PART_Colon`）、必填星号（`PART_DefaultRequireMark`）、可选文案（`OptionalMark`）、
  tooltip 图标（`TooltipIconPresenter`）和自定义 mark presenter；由 `RequiredMark` 策略与 token 拥有。
- `ContentFrame`、`PART_BodyLayout`、`PART_LabelLayout`、`PART_LabelContentLayout`、`PART_RootLayout`
  等布局容器；Grid 几何由布局算法拥有。
- `helpItem` 的消息文本着色；运行时消息节点由 `ErrorMessageForeground` / `WarningMessageForeground` 显式
  设置，静态 `HelpText` 由 `ValidateStatus` selector 拥有。
- `ContentFrame`、`PART_BodyLayout`、`PART_LabelLayout`、`PART_LabelContentLayout`、`PART_RootLayout`
  等布局容器；Grid 几何由布局算法拥有。
- 删除按钮区（`ItemDeleteButtonLayout` / `ItemDeleteButton`）、`FormValidateFeedback`、`FormItemDecorator`
  的 `Child` / `Extra` presenter。
- `Content`、`ExtraTemplate`、自定义 mark 模板创建的用户子树。
- `PART_*` 名称、internal 类型、模板层级与 `PART_ItemsPresenter`。

默认主题不消费 `.semantic-*` selector；静态 marker 只提供应用样式命中点，不改变默认属性优先级或增加状态
订阅。Semantic Style 服从 Avalonia 原生属性优先级；`label` 与 `content` 的 `Width` / `MaxWidth` 属性槽受
测量闭环约束（§1.2、§1.3），覆盖前必须验证换行、裁剪和布局结果。

## 6. 兼容性与验证

删除或重命名 Part、修改 selector class / route、收窄 `ContractType`、改变 cardinality，或者让内置模板缺少
marker，均属于公共主题契约变更。

验证至少覆盖：

- `FormItem` descriptor 只有 `root`、`label`、`content`、`extra`、`help`、`helpItem`，字段值与本文一致；`Form`、
  `FormItemDecorator`、`SubmitButton`、`ResetButton` 不注册 descriptor。
- 内置模板包含 `semantic-label`、`semantic-content`、`semantic-extra`、`semantic-help` marker，静态
  `HelpText` 携带 `semantic-help-item`；root 不声明 `.semantic-root`。
- 生成的 `FormItem*Style` 可以编译并命中最低 public `ContractType`，在 Application、局部 StyleHost 和 owner
  实例 Styles 三个作用域生效。
- `Layout`、`FormLayout`、`RequiredMark`、验证状态、`IsHideItemLabel`、`Extra` 置空和 `FormActionsItem`
  场景下静态 Part 保持对象身份与 `Single` 数量。
- 运行时消息节点在验证状态切换、reset、detach 和重复验证下正确创建、重建与清空：数量与消息一致、顺序保持
  消息在前 `Help` 在后、无旧节点或 marker 泄漏。
- 动态增删表单项后新实例 marker 完整、被删实例无 marker 泄漏。
- `label` / `content` 的布局属性定制在 Horizontal / Vertical / Inline 布局下的最终 Measure / Arrange 结果
  可预期，且不破坏 `LabelColInfo` / `WrapperColInfo` 响应式列宽。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- descriptor、生成 Style 与 NativeAOT 路径使用编译期生成数据，不依赖运行时反射或 VisualTree 扫描。
- Gallery Semantic Parts Tab 延迟创建 Preview，示例必须与上游 Semantic DOM 文档示例完全一致：`labelCol=8*` /
  `wrapperCol=16*` 的用户名 / 密码表单；用户名项为必填标签、输入内容和帮助文案 `Use 4 to 16 characters.`；密码项为
  必填标签、密码输入、两条错误消息 `Please input your password!` 与 `Use at least 8 characters.` 以及 Extra
  `Password must contain letters and numbers.`；使六个 Part 均有可见实例，并展示 `FormItem*Style` 的强类型
  Style 用法。
- Examples 列表末尾的 ShowCaseItem（`SourceKey="form-semantic-part"`，全宽、延迟加载）展示与上游
  style-class 示例对应的样式定制示例（示例文案不出现 “dom” 字样）：两张 `labelCol=4*` / `wrapperCol=20*` 的
  卡片表单垂直堆叠（一上一下），每张 `MaxWidth="800"` 水平铺满（等宽保证 4\*/20\* 栅格切分出一致的 label
  列宽），共享用户名（`Please enter username!`）、邮箱（`Please enter email!`）与 Submit / reset 按钮组内容；
  第一张以对象式定制 `label`（`#333333`、`FontWeight=Medium`、右对齐）与 `content`（左内边距 12）；第二张为
  `Filled` 变体，以函数式定制 `root`（`#1677FF` 描边）、`label`（`#1677FF`）与 `content`。root 的背景 /
  描边 / 阴影经 `/template/ Border#Frame` 落到 Form 模板根 Border；其余 Part 经后代 selector
  （`atom|Form.semantic-* <PartType>.semantic-*`）落到模板节点。
