# Card Token 设计

本文档定义 `AtomUI.Desktop.Controls.CardToken` 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Card 整体架构见 [Card 桌面版架构设计](overview.md)，内部实现原理见 [Card 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Card Changelog](changelog.md)。

## 1. 定位

CardToken 是 Card 的控件级 Token scope，描述卡片 Header、Body、Actions、Tabs、Extra、阴影、Grid item 和 action icon 的组件语义值。

CardToken 不承载以下状态：

- `Content`、`Header`、`Extra`、`Cover`、`Actions` 等实际内容。
- `ContentType`、`IsLoading`、`IsHoverable`、`IsInnerMode`、`StyleVariant` 等实例行为状态。
- `HeaderBorderThickness`、`EffectiveBorderThickness`、`EffectiveCornerRadius`、`IsActionsPanelVisible` 等内部派生状态。
- `CardGridItem.Row`、`Column`、`RowSpan`、`ColumnSpan` 等布局位置。
- `TabControl` 的 selected item、current content 或 tab collection。

## 2. 分类、有效默认与消费者

22字段使用DependencyDefaultsV2 Revision1；默认只读有效Card Global或原常量/阴影构造，没有Own依赖边。名称、type、resource key/slot和历史默认保持。18 Active中ActionsBg与ExtraColor为明确Own激活的OptionalOverride，其余4项为Reserved。

| 分类／字段 | 默认输入 | 实际消费 |
| --- | --- | --- |
| HeaderBg | 原透明色 | HeaderFrame.Background；Inner模式继续使用ColorFillAlter |
| HeaderFontSizeLG／HeaderFontSize／HeaderFontSizeSM | FontSizeLG／FontSize／FontSize | HeaderFrame的三档文字尺寸 |
| HeaderHeightLG／HeaderHeight／HeaderHeightSM | FontHeightLG+UniformlyPadding×2／FontHeight+UniformlyPaddingXS×2／FontHeightSM | HeaderFrame.MinHeight |
| HeaderPaddingLG／HeaderPadding／HeaderPaddingSM | 相应UniformlyPaddingLG/SM/XS水平Thickness | HeaderFrame.Padding |
| BodyPaddingLG／BodyPadding／BodyPaddingSM | PaddingLG／PaddingSM／PaddingXS | 普通/Meta body、GridItem与Tabs内容；Grid/Tabs外壳不额外套普通body padding |
| ActionsBg | ColorBgContainer | **OptionalOverride**：明确Own时CardActionPanel.Background→真实Frame；未配置保留原透明表面 |
| ExtraColor | ColorText | **OptionalOverride**：明确Own时HeaderExtra.Foreground；未配置保留当前继承/原生值 |
| CardActionsIconSize | FontSize | CardActionButton.IconWidth/IconHeight |
| CardShadows | 原三层常量BoxShadows | hoverable Card/GridItem的hover阴影 |
| CardGridItemShadows | LineWidth与ColorBorderSecondary的原五层BoxShadows | GridItem默认边线阴影 |

三档仍为Small/Middle/Large，不新增Custom枚举。原生Width/Height/Padding等及专用Style可在对应层级定制；Headerless只隐藏头部，body/Grid/Tabs职责不合并。明确0对合法几何值有效，字体遵守正字号约束，不把无效FontSize=0当成受支持状态。

### Reserved历史资源

| 字段 | 保留默认 | 当前替代职责／边界 |
| --- | --- | --- |
| CardHeadPadding | Padding | 三档HeaderPadding已有独立消费者，本字段无独立生产目标 |
| CardPaddingBase | PaddingLG | 三档BodyPadding已拥有实际内容内边距，本字段不成为别名 |
| TabsMarginBottom | bottom=-UniformlyPadding-LineWidth | 当前CardTabsContent含完整TabControl header+body，没有旧head-tabs负边距的独立目标 |
| ActionsSpacing | SpacingSM | 当前动作组均分布局没有既定margin/padding/gap激活合同，不据名称发明新功能 |

四项保留名称/type/key/slot、有效默认与配置/序列化往返，不宣称能改变界面。主题作者分别使用实际Header/Body输入、TabControl公开间距或专用CardActionsStyle；不以保留字段构建重复控制入口。

### 可选激活与原默认

ActionsBg虽然原有静态引用，但Frame的TemplateBinding读取空root Background，原实际表面透明；不能自动施加资源默认覆盖原生Card.Background。ExtraColor资源默认也不自动替换原HeaderExtra继承色。二者仅在规范化ThemeConfig明确Own时激活，清除回当前默认，专用CardExtraStyle/CardActionsStyle仍优先。资源默认存在不等于字段被明确配置。

## 3. Global、字体、Part与owner边界

根Card、内部action panel、Meta、Grid与Tabs主题按实际owner读取Global。Card、CardMetaContent、CardGridContent、CardGridItem、CardTabsContent分别拥有本identity的可选FontFamily/FontSize/ColorText；未配置保留真实原生继承，本地值优先，清除与重挂跟随当前父。标题/描述/动作等已有角色继续相应字号、weight、颜色，不把全部子Control属性复制为Card根值。

CardActionButton保留独立IconButton输入/颜色职责，CardActionsIconSize由Card Own提供；TabControl、Avatar和Skeleton也是独立组件，不因嵌入而复制其整个合同。CardMetaContent的Spacing/SpacingXS、标题ColorTextHeading/FontWeightStrong/FontSizeLG、描述ColorTextDescription属于该公开owner。

模板输入仍由ControlTheme拥有。root Padding投射到外Frame；GridItem三档Padding在root提供默认再由Frame TemplateBinding投射，使原生本地Padding真正优先。actions的Background/Padding投射到真实Frame，原无效top1 style默认去除，采用原生0保持实际默认布局；自绘分隔线跟随实际child坐标和变化输入失效。详细语义部件类型/route见[Semantic Part](semantic-part.md)。

## 4. 控件家族影响

CardToken 直接影响 Card 家族控件：

- `Card`
- `CardActionPanel`
- `CardActionButton`
- `CardGridContent`
- `CardGridItem`
- `CardTabsContent`
- `CardMetaContent`
- Card token.md 语义说明

CardToken 不应被其他 Data Display 控件直接复用。若多个控件需要共享同一语义值，应评估是否上升为 SharedToken，而不是跨控件引用 CardToken。

## 5. 兼容性要求

CardToken 属于 Card 主题契约。即使 `CardToken` 是 internal 类型，生成的 token kind、AXAML resource 使用点和 token.md 语义说明已经形成稳定依赖。

Token 变更要求：

- 不擅自重命名或删除现有 Token。
- 不改变 Header、Body、Actions、Tabs、Extra、Shadow 或 Grid item Token 的语义范围。
- 不把 `ContentType`、`IsLoading`、`IsHoverable`、`StyleVariant` 或 Grid item 位置状态迁移为 Token。
- 不在 CardToken 中复制 SharedToken 已表达的根边框、圆角、基础背景或通用文字颜色语义。
- 修改 size 相关 Token 时必须同时验证 Large / Middle / Small 下 Header、Body、Grid item 和 Tabs 内容。
- 修改 shadow Token 时必须验证 Card hover、borderless 和 grid item hover。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 Header Token | 验证 Large / Middle / Small Card Header 高度、padding、字体和 Headerless 状态。 |
| 修改 Body padding Token | 验证普通内容、Meta 内容、Tabs 内容和 Grid item padding。 |
| 修改 Actions Token | 验证底部操作区背景、action icon 尺寸、分隔线对齐和 hover 颜色。 |
| 修改 Tabs 相关输入 | 验证 CardTabsContent 与 Header/Body 边界；Reserved字段仅检查资源/配置往返，不误报为可见消费者。 |
| 修改 `ExtraColor` | 验证 Header Extra 和 HyperLinkButton 等内容视觉。 |
| 修改 shadow Token | 验证 hoverable Card、borderless Card 和 hoverable CardGridItem。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
