# Result Token 设计

本文档定义 Result 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Result 整体架构见 [Result 桌面版架构设计](overview.md)，内部实现原理见 [Result 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Result Changelog](changelog.md)。

## 1. 定位

Result Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ResultToken`，scope id 为 `Result`，源码位于 `src/AtomUI.Desktop.Controls/Result/ResultToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `HeaderFontSize`、`SubHeaderFontSize`、`StatusIconSize`、`ImageWidth`、`ImageHeight` |
| 间距与布局 | padding、margin、gap、offset、popup content padding。 | `ExtraMargin`、`FramePadding`、`ContentPadding`、`ContentMargin`、`HeaderMargin`、`StatusImageMargin` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `ResultInfoIconColor`、`ResultSuccessIconColor`、`ResultWarningIconColor`、`ResultErrorIconColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Result 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 Result Token 时必须评估以下范围：

- `Result`
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

## CT15 真实依赖与消费者

15Own迁DependencyDefaultsV2/revision1，四算法60默认/type/key/slot保持。HeaderFontSize依Global.FontSizeHeading3，SubHeaderFontSize依Global.FontSize；StatusIconSize独立依Global.FontSizeHeading3*3，不改成Own标题字号依赖。两插画尺寸保持250×295，四语义色只用于四命名状态图标，403/404/500 SVG固有内容色不统一染色。六项Thickness进入原Frame/Content/Header/Extra/StatusImage目标。

ControlTheme把原标题色、说明色、内容背景和相对行高改为实际Result owner Global；字体Family/Size为可选显式Global，缺省保留真实继承，Native优先。实际Header/SubHeader字号仍自身Own/Native角色。detach期间状态/自定义Icon变化在原实例重新attach回放，原内容合同保持。当前19项匹配验证通过，完整视觉/AOT、清理和最终模块验收仍待[CT15执行](../../../../superpowers/plans/2026-10-22-custom-theme/result-consumers.md)。

| Own | 默认输入 | 实际目标 |
| --- | --- | --- |
| HeaderFontSize | Global.FontSizeHeading3 | Header presenter字号与相对行高计算 |
| SubHeaderFontSize | Global.FontSize | SubHeader presenter字号与相对行高计算 |
| StatusIconSize | Global.FontSizeHeading3*3 | 普通状态Icon尺寸投射，不是插画尺寸 |
| ImageWidth / ImageHeight | 250 / 295 | 403/404/500 SVG容器 |
| ResultInfoIconColor | Global.ColorInfo | Info普通Icon画刷 |
| ResultSuccessIconColor | Global.ColorSuccess | Success普通Icon画刷 |
| ResultWarningIconColor | Global.ColorWarning | Warning普通Icon画刷 |
| ResultErrorIconColor | Global.ColorError | Error普通Icon画刷 |
| ExtraMargin | 0,Global.UniformlyMargin,0,0 | Extra presenter外间距 |
| FramePadding | PaddingLG*2,MarginXL | 根Frame内间距 |
| ContentPadding | Padding*2.5,PaddingLG | 正文presenter内间距 |
| ContentMargin | 0,PaddingLG,0,0 | 正文presenter外间距 |
| HeaderMargin | 0,MarginXS | 标题presenter外间距 |
| StatusImageMargin | 0,0,0,Margin | 普通Icon与异常SVG共同外间距 |

所有Global量以有效owner Global求值，不从实例状态推导。清除覆盖后原默认恢复；Style与Native覆盖行为按真实目标和原有优先级验证。Result没有SizeType，Custom体现为连续字号/尺寸/间距及内容自然布局，不造Small/Medium/Large状态。

CT15阶段结果：24匹配验证、managed/AOT各72检查，132跨运行精确0，候选/AOT各32恢复0；临时材料已清理，最终9精确用例及NET8零警告通过。60对有意变化含自定义glyph原生优先修正待限定审查，原失败账本/资源判据不足/平台/极值和完整家族边界保留，见[结果](../../../../superpowers/plans/2026-10-22-custom-theme/result-consumers-results.md)。
