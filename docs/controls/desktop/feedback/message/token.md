# Message Token 设计

本文档定义 Message 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Message 整体架构见 [Message 桌面版架构设计](overview.md)，内部实现原理见 [Message 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Message Changelog](changelog.md)。

## 1. 定位

Message Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- internal `MessageCardToken`，scope id 为 `MessageCard`，源码位于 `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`。

## 2. 实际字段与默认

六字段使用DependencyDefaultsV2 Revision1，默认仅依赖有效MessageCard Global或原常量，无Own依赖边；保留名称/type/key/slot。五Active，一Reserved。

| 字段 | 默认输入 | 消费合同 |
| --- | --- | --- |
| ContentBg | ColorBgElevated | 卡片root Background→Frame，以及manager两层静态背板背景 |
| ContentPadding | horizontal=(ControlHeightLG-FontSize×RelativeLineHeight)/2，vertical=UniformlyPaddingXS | root Padding→真实Frame，保留自适应高度 |
| MessageIconSize | FontSizeSM×RelativeLineHeightSM | IconPresenter Width/Height |
| MessageIconMargin | right=UniformlyMarginXS | IconPresenter Margin |
| MessageTopMargin | 四边UniformlyMarginXS | manager Padding→StackHost.Margin |
| CardHeight | 原double 0 | **Reserved**：当前无生产消费者，不作为固定高度或MinHeight入口 |

CardHeight保留历史资源、默认、type/key/slot及配置往返，不能据名称声称调节界面。实际高度由正文、图标和Padding测量；原生Width/Height/MinHeight及Part增量定制保持。Message没有SizeType，不创造三档或Custom枚举。合法几何0与明确透明值有效，清除恢复当前默认。

### Global、排版与优先级

Card和manager分别读取真实owner Global。IconPresenter是独立公开身份，Message状态色必须明确SourceControlType=MessageCard；不能误取IconPresenter Global。manager背板的圆角/阴影属于manager，背景仍复用Card.ContentBg。

正文原有FontSize/ColorText角色默认由Card root提供，再由SelectableTextBlock继承；实际默认画面保持，root原未被正文消费的字号/前景有效值随此角色归位。原生root FontSize/Foreground与专用MessageCardTitleStyle可达真实正文，LineHeight/选择色仍分别消费对应Card Global，不复制为同一变量。FontFamily使用有限真实原生继承源，明确Card Global可覆盖，清除与重挂由当前owner接管。manager字体保留独立owner与原生继承；卡片字号/前景角色不自动变为manager字号。

主题默认、状态、原生本地值和专用Style遵循Avalonia优先级。五种默认图标由现有类型Style内的Template逐卡实例化；切换类型同步更新，用户Icon优先，null按原合同恢复当前类型默认。Loading仍使用正常Spin，不用关闭动效规避动态验证。

## 3. 控件专项模型中的 Token 使用

Message 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

`MessageTopMargin` 为四边 `UniformlyMarginXS`，作为 `WindowMessageManager.Padding` 的默认值；模板将 Padding
投影到 StackHost 的 Margin。应用可覆盖 owner Padding，卡片外框自身保持零外边距。

Stack当前展开gap16、折叠offset8、背板高度16与缩进由共享反馈堆叠实现拥有；这些常量尚不能仅凭Global schema可配置就声称已消费。Notification与Message共用几何语义，不在MessageCardToken复制一组Stack字段。背板背景复用
`ContentBg`，阴影复用 SharedToken tertiary shadow。Stack 状态和活动项数量属于运行时状态，不能写入 Token。

## 4. 控件家族影响

调整 Message Token 时必须评估以下范围：

- `Message`
- `MessageCard`
- `WindowMessageManager`
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
