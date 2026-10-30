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
