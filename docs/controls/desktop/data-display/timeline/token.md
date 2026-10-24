# Timeline Token 设计

本文档定义 Timeline 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Timeline 整体架构见 [Timeline 桌面版架构设计](overview.md)，方向布局策略见 [Timeline 方向与布局设计](orientation-layout-design.md)，内部实现原理见 [Timeline 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Timeline Changelog](changelog.md)。

## 1. 定位

Timeline Token 只表达节点、连接线和 Item 的组件级尺寸、间距与颜色。Token 不承载 Orientation、Mode、视觉索引、首尾、Reverse、Pending 邻接或其他实例运行时状态。

当前 Token scope：

- `TimelineToken`，scope id 为 `Timeline`，源码位于 `src/AtomUI.Desktop.Controls/Timeline/TimelineToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 连接线宽度、节点尺寸、圆点尺寸、边框宽度和内容最小尺寸。 | `IndicatorTailWidth`、`LastItemContentMinHeight`、`IndicatorSize`、`IndicatorDotSize`、`IndicatorDotBorderWidth` |
| 垂直 Item 间距 | 垂直 Timeline 中普通 Item 和 Pending 邻接 Item 的 block-end 间距。 | `ItemPaddingBottom`、`ItemPaddingBottomLG` |
| 逻辑位置间距 | Vertical Start、End 和双侧模式中 Indicator 与相邻内容的间距。 | `IndicatorStartModeMargin`、`IndicatorEndModeMargin`、`IndicatorMiddleModeMargin` |
| 颜色与装饰 | 节点连接线的颜色和节点的结构视觉。 | `IndicatorTailColor`、`IndicatorTailWidth`、`IndicatorDotSize`、`IndicatorDotBorderWidth` |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Timeline 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责 Orientation、Mode、视觉顺序和布局状态归一。
- AXAML/ControlTheme 负责把 Token 映射到节点尺寸、连接线、Item padding 和逻辑位置 margin。
- Token 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

方向布局中的资源边界：

- `ItemPaddingBottom` 和 `ItemPaddingBottomLG` 只应用于 Vertical（作为 item 的下内边距，对齐上游 `li` 的 `paddingBottom`），不能增加 Horizontal Item 的 block-end 空间。
- `IndicatorStartModeMargin`、`IndicatorEndModeMargin` 只表达 Vertical 逻辑位置；RTL 下由布局和 FlowDirection 映射物理方向。
- `IndicatorMiddleModeMargin` 用于 Vertical 双侧布局，不承载 Alternate 奇偶状态。
- Horizontal 内容与轴线之间的 gap 直接使用 `SharedToken.UniformlyPaddingXS`，不为同一全局间距复制 Timeline 专属 Token。
- `IndicatorSize`、`IndicatorDotSize`、`IndicatorDotBorderWidth`、`IndicatorTailWidth` 和 `IndicatorTailColor` 同时服务 Vertical 与 Horizontal renderer。

### 有效默认与实际消费

`TimelineToken` 使用 `DependencyDefaultsV2`（Revision 1）。11 个字段的名称、类型、资源 ID、slot 与未配置明暗默认保持；仅 `ItemPaddingBottomLG` 有 Own 依赖，读取有效 `ItemPaddingBottom`，显式 LG 再覆盖派生值。

| 字段 | 默认输入／公式 | 消费区域与边界 |
| --- | --- | --- |
| `IndicatorTailColor` | 有效 Global `ColorSplit` | 两方向 rail 背景 |
| `IndicatorTailWidth` | 有效 Global `LineWidthBold` | 两方向 rail 排列宽度 |
| `ItemPaddingBottom` | 有效 Global `UniformlyPadding * 1.25` 的 bottom Thickness | Vertical 普通项；末项不保留 |
| `ItemPaddingBottomLG` | 有效 Own `ItemPaddingBottom * 2` | Vertical 非倒序的 pending 前项、倒序的 pending 自身 |
| `LastItemContentMinHeight` | 资源默认为有效 Global `ControlHeightLG * 1.2` | **OptionalOverride**：仅明确配置 Own 时，应用到两方向最后可见项的 ContentPresenter；未配置保留原生 MinHeight=0 |
| `IndicatorSize` | 有效 Global `SizeMS` | 轴线槽及图标尺寸 |
| `IndicatorDotSize` | 常量 8 | 内置 dot 几何 |
| `IndicatorStartModeMargin` | 有效 Global `UniformlyMargin` 的 right Thickness | Vertical Start |
| `IndicatorEndModeMargin` | 同上，left Thickness | Vertical End |
| `IndicatorMiddleModeMargin` | 同上，horizontal Thickness | Vertical Alternate／双侧 label |
| `IndicatorDotBorderWidth` | 有效 Global `LineWidth * 3` | dot 默认描边；排列盒边长仍为 dotSize + borderWidth |

`LastItemContentMinHeight` 的历史资源默认在标准主题下是 48，但此前没有实际消费者；自动应用会改变已有布局，因此保留资源值，通过 `WhenExplicit=True` 启用。明确值 73 生效，明确值 0 仍为 0，清除后回到 0；应用的 `TimelineItemContentStyle` 可覆盖该默认 Setter，不依赖找节点赋值。

内部 Indicator 的 `ColorPrimary`、`ColorBgContainer` 和 `RelativeLineHeight` 读取实际公开 `TimelineItem` 的有效 Global；Timeline Own 几何仍属于 `Timeline`。`IndicatorColor=null` 实时回退到有效 Primary，公开明确颜色优先；清除颜色后继续回退，不在模板应用时复制本地值。独立内部 primitive 没有注册 owner 时使用 Shared 回退。两种公开 owner 的配置职责不合并。

### 公开 owner 的排版与框体

Timeline 和 TimelineItem 的 FontFamily、FontSize、ColorText 各自读取本 identity 的有效 Global，仅在明确配置时启用；未配置时保留实际继承父的字体和前景。没有 SizeType／IsCustomFontSize 新 API；字体大小不是按名称推断的三档值。清除恢复当前祖先，公开本地值及专用 Title／Content Style 的原生优先级保持。FontWeight／FontStyle 继续沿原生继承传播。

根已有 Background／BorderBrush 读取 Timeline 的 ColorBgContainer／ColorBorder；不自动增加原来为0的边框或圆角。section 的 IndicatorSpacing 读取公开 TimelineItem 的 UniformlyPaddingXS。节点掩膜的背景仍属于 Item：仅设置 root 背景不会复制到 Item；若配方要求同色，可设置共同全局 ColorBgContainer，或分别配置两个 owner。公共 root Background 本地值的含义也不等于设置条目掩膜。

明确配置为0的 Own 几何／间距和 alpha=0 的轨迹颜色仍按实际输入消费，不以 false／0／透明替换为默认值。Own 的 dot 盒、rail 宽度、icon 尺寸、末项高度、pending 间距及三种 margin 的运行覆盖见 [P2 结果](../../../../superpowers/plans/2026-10-22-custom-theme/timeline-typography-results.md)。

## 4. 控件家族影响

调整 Timeline Token 时必须评估以下范围：

- `Timeline`
- `TimelineItem`
- `TimelineIndicator` 的垂直与水平 renderer。
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题以及 Browser/Desktop 宿主。

## 5. 兼容性要求

- 稳定逻辑位置 Token 名称为 `IndicatorStartModeMargin`、`IndicatorEndModeMargin` 和 `IndicatorMiddleModeMargin`；不得重新引入 Left/Right 物理命名或旧资源别名。
- 不删除或重命名其他已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不为 Orientation x Mode x 奇偶组合展开 Token 矩阵；组合关系由 Panel 和 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Vertical/Horizontal、Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 覆盖 Orientation x Start/End/Alternate、RTL、Pending 和 Label 布局。 |
