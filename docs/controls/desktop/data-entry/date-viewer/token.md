# DateViewer Token 设计

DateViewerToken 为独立单值与范围面板提供默认资源。关联 [架构](overview.md)、[实现](implementation.md)、
[共享设计](shared-panel-design.md)、[Changelog](changelog.md)；全局规则见
[控件 Token 规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

[DateViewerToken.cs](../../../../../src/AtomUI.Desktop.Controls/DateViewer/DateViewerToken.cs)通过 ControlDesignToken
注册并生成 DateViewerTokenResource/TokenKind。Token 类是 internal，应用使用生成资源入口；
公共面板不依赖 DatePicker Token。DateViewer 与 RangeDateViewer 共享同一组资源。

## 2. Token 分类

| 分类 | Token | 默认计算 / 语义 |
| --- | --- | --- |
| 表面 | PanelBg | ColorBgContainer，面板背景。 |
| 交互 | CellHoverBg | ControlItemBgHover，Hover 背景。 |
| 连续范围 | CellRangeBg | ControlItemBgActive，范围/预览中段。 |
| Compact 尺寸 | PanelWidth | 7 × (ControlHeightSM × 1.5) + 2 × (UniformlyPadding + UniformlyPaddingXXS / 2)，默认 288。 |
| 值框尺寸 | CellHeight / CellWidth | ControlHeightSM，日期值框最小高度与宽度。 |
| 日期行距 | CellRowHeight | CellHeight + 3 × UniformlyPaddingXXS，默认 36；周标题和日期行采用同一节距。 |
| 周期值框 | PeriodCellWidth | ControlHeightLG × 1.5，月、季度、年使用固定宽度，避免背景随文本长度变化。 |
| 周期内边距 | PeriodCellPadding | 左右 UniformlyPaddingXS，上下 0；周期文本在固定值框内居中。 |
| Content 密度 | ContentCellMinHeight | ControlHeightLG × 2，内容单元格最小高度。 |
| Content 表面 | ContentCellSurfaceMargin / ContentCellTopBorderThickness | 表面左右内缩 UniformlyMarginXXS；顶边厚度使用 LineWidthBold。 |
| Content 前景 | ContentCellValueMargin / ContentCellContentMargin | 日期值左右留白为 UniformlyMarginXXS + UniformlyPaddingXS，顶部留白再加 LineWidthBold + UniformlyPaddingXS / 2；普通内容只取左右留白。 |
| 外层布局 | PanelPadding | 默认 0；面板正文留白由 DateBodyPadding 承担。 |
| 导航内边距 | HeaderHorizontalPadding | 左右 UniformlyPaddingXS、上下 0；导航箭头与面板边缘保持间距。 |
| 日期正文 | DateBodyPadding | 左右为 UniformlyPadding + UniformlyPaddingXXS / 2、上下为 UniformlyPaddingXS，默认 18、8。 |
| 范围布局 | PanelSpacing | UniformlyMargin，双面板及双 Header 间距。 |

主色、文字、禁用透明度、圆角与边框直接消费 SharedToken。日期值、PanelKind、活动端点、Hover 值与业务禁用
结果不进入 Token；本组没有独立 Motion Token。

## 3. 控件专项模型中的 Token 使用

DateViewerTheme/RangeDateViewerTheme 消费表面、外层 padding 和双面板间距；DatePanelTheme 消费 PanelWidth 与正文 padding。
Compact 使用自然宽度，Content 解除固定面板宽度并由外部测量宽度展开列。
DateViewerCellTheme 消费日期行距、值框尺寸、ContentCellMinHeight、Content 前景间距与状态色；范围背景和预览使用同一范围资源。

Header 默认使用 SharedToken 导航按钮，并通过 HeaderHorizontalPadding 保留左右间距；范围每个 Header 按单面板宽度与间距对齐。
内容模板只接管内容，不能以 Token 替代模型状态。

## 4. 控件家族影响

独立公共 owner 使用本组资源。Calendar 与 Picker 可通过内部主题配置表达产品完整视觉，
其业务资源和范围条 lane 不并入 DateViewerToken。独立 DateViewer 的 Today 页脚使用自身本地化资源；托管面板不显示该页脚，Picker 保留自己的 Footer。

## 5. 兼容性要求

十八个生成资源名、类型与含义构成定制契约。局部资源覆盖、资源移除、主题切换和 Token 动态变化
须保持可用；不能复制静态颜色规避资源生命周期。宿主产品输出仍需按完整视觉基线验证，
公共面板独立尺寸不能充当 Picker 尺寸等价证据。

## 6. 验证策略

验证单/双面板默认尺寸、Compact/Content、状态色、局部覆盖和主题变化，
同时保持原裁剪、布局与滚动条件。Token-only 与 Style-only 消费通过真实 TypeMap/裁剪专项验证。
宿主和 NativeAOT 的验收范围独立记录，见 [共享验收](shared-panel-design.md#10-重建边界与等价验收)。
