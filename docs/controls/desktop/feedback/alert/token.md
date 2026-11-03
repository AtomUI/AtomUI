# Alert Token 设计

本文档定义 Alert 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Alert 整体架构见 [Alert 桌面版架构设计](overview.md)，内部实现原理见 [Alert 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Alert Changelog](changelog.md)。

## 1. 定位

Alert Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `AlertToken`，scope id 为 `Alert`，源码位于 `src/AtomUI.Desktop.Controls/Alert/AlertToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `DefaultIconSize`、`WithDescriptionIconSize`、`CloseIconSize` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `DefaultPadding`、`WithDescriptionPadding`、`MessageWithDescriptionMargin`、`IconDefaultMargin`、`IconWithDescriptionMargin`、`ExtraElementMargin` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | 按源码 Token 语义维护 |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Alert 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从有效 Alert Global 与常量派生，不读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

### 有效默认与实际消费者

Alert 的10个Own使用DependencyDefaultsV2 Revision1，无Own之间的依赖边；名称、类型、资源key/slot和未配置默认保持。明确0参与消费，清除回到当前主题输入。

| Own | 默认输入／公式 | 实际消费者 |
| --- | --- | --- |
| DefaultPadding | horizontal固定12，vertical为PaddingContentVerticalSM | 无描述时root.Padding |
| WithDescriptionPadding | PaddingContentHorizontalLG／UniformlyPaddingMD | 有描述时root.Padding |
| MessageWithDescriptionMargin | bottom=UniformlyMarginXS | **OptionalOverride**：仅明确Own时应用到有描述的Label/Marquee标题Margin；未定制时实际0 |
| IconDefaultMargin | right=UniformlyMarginXS | 无描述时四个状态Icon.Margin |
| IconWithDescriptionMargin | right=UniformlyMarginSM | 有描述时四个状态Icon.Margin |
| DefaultIconSize | FontSizeLG | 无描述状态Icon.Width/Height |
| WithDescriptionIconSize | FontSizeHeading3 | 有描述状态Icon.Width/Height |
| CloseIconSize | FontSizeIcon+2 | **OptionalOverride**：明确Own覆盖close.IconWidth/Height，未配置保留有效Alert Global IconSizeSM |
| ExtraElementMargin | left=UniformlyMarginXS | close.Margin；不扩大为ExtraAction内容布局的新含义 |
| DescriptionLabelMargin | top=UniformlyMarginXS | description Label.Margin |

OptionalOverride的“明确”指规范化ThemeConfig中Alert控制项提供该Own，不是资源字典中存在历史默认条目。标准配方的CloseIconSize资源为14，而历史实际关闭图标为IconSizeSM的12；MessageWithDescriptionMargin资源为4，而历史标题Margin为0。保留这些资源值，不能自动施加它们改变未配置布局。明确37/0以及清除按实际输入工作；有描述标题的Margin同时覆盖Label与Marquee，无描述时不由该Own施加。

### Global、字体与优先级

背景/边框的四个Type分支、状态图标、圆角、描边、内部间距、标题字号/行高和兼容关闭图标默认均读取实际Alert owner的有效Global。Success/Warning/Error图标读对应Color，Info图标继续读ColorPrimary；Info背景/边框仍读ColorInfoBg/ColorInfoBorder。根描边读BorderThickness、圆角读BorderRadiusLG，不在模板中重新派生全局资源。

FontFamily、root FontSize和Foreground只在相应Global明确配置时启用，未配置保留真实原生继承父的字体/前景。root本地值优先，清除恢复当前父级；FontWeight/FontStyle继续原生继承。标题保持原有角色：有描述用FontSizeLG，无描述用FontSize；root本地FontSize控制正文，不替换标题角色字号。专用AlertTitleStyle可明确覆盖标题字体、行高与Margin。

这些输入不是新增SizeType/字体预设。公开Width/Height、Padding、Background、Border等原生本地值及专用Part Style保持优先级。模板内四个Icon的颜色绑定为Template优先级，允许AlertIconStyle的FillBrush生效；不能用本地绑定压住专用Style。

关闭节点仍是独立IconButton，内部色彩、hover/pressed/disabled和输入按其原主题管理；AlertTheme拥有的glyph尺寸/Margin由上述Alert输入提供。关闭色彩可由专用AlertCloseStyle声明，不把独立IconButton的所有Global合同合并到Alert。Alert没有Popup/Flyout/Window host职责。

## 4. 控件家族影响

调整 Alert Token 时必须评估以下范围：

- `Alert`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
