# DateViewer Token 设计

DateViewerToken 为独立单值与范围面板提供默认资源。关联 [架构](overview.md)、[实现](implementation.md)、
[共享设计](shared-panel-design.md)、[Changelog](changelog.md)；全局规则见
[控件 Token 规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

[DateViewerToken.cs](../../../../../src/AtomUI.Desktop.Controls/DateViewer/DateViewerToken.cs)通过 ControlDesignToken
注册并生成 DateViewerTokenResource/TokenKind。Token 类是公开类（供主题算法的控件阶段写入），应用仍使用生成资源入口；
公共面板不依赖 DatePicker Token。DateViewer 与 RangeDateViewer 共享同一组资源。

## 2. Token 分类

| 分类 | Token | 默认计算 / 语义 |
| --- | --- | --- |
| 表面 | PanelBg | ColorBgContainer，面板背景。 |
| 交互 | CellHoverBg | ControlItemBgHover，Hover 背景。 |
| 连续范围 | CellRangeBg | ControlItemBgActive，范围/预览中段；中段文字读全局 `ControlItemColorActive`（默认 `ColorText`）。 |
| Content 选中 | CellActiveColor | ColorPrimary，Content 呈现中今天与选中日期的文字（画在 ControlItemBgActive 上）；选中底色为实色的主题设为对比色。 |
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
| 分隔线 | HeaderBorderThickness / FooterBorderThickness | 底边 / 顶边 LineWidth：默认 Header 底部与独立 Footer 顶部分隔线（AtomUI 扩展，替换 `0,0,0,1` / `0,1,0,0` 字面量）。 |
| 周模式 | WeekNumberOpacity | 0.5：周模式下周序号的不透明度（AtomUI 扩展，替换字面量；antd 以半透明 colorTextLightSolid 表达）。 |

面板、Header 与日期格跟随实际 DateViewer/RangeDateViewer owner；两个终端通过 AbstractDateViewerToken 共享计算、独立设置具体样式输入。PanelSpacing 只属于 RangeDateViewer，FooterBorderThickness 只属于 DateViewer。嵌入 Calendar/Picker 的公开 viewer 仍保留其实际 viewer Own 身份（range 使用 RangeDateViewer），有效全局值按原宿主转发契约读取；来源 DateViewer 的 Own 不进入 RangeDateViewer。

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

## 独立 Token 身份与共享计算

迁移原 DateViewer.PanelSpacing 到 RangeDateViewer.PanelSpacing。CellHeight 显式值在各终端保留后参与 CellRowHeight 派生。

## 2026-11-20 新增 Token

默认值与新增前的外观一致；PrimeNG 主题在各自控件阶段中赋值。

| Token | 默认值 | 用途 |
| --- | --- | --- |
| `CellCornerRadius` | `BorderRadiusSM` | 日期单元格值框圆角 |
| `CellTodayBg` | 透明 | 今天单元格背景 |
| `CellTodayColor` | `ColorText` | 今天单元格文字色 |
| `CellTodayBorderColor` | `ColorPrimary` | 今天单元格描边 |
| `HeaderTitleFontWeight` | `Bold` | 面板头部标题字重 |
| `WeekHeaderHeight` | 0 | 星期行高度；0 沿用单元格行高，正值时星期名垂直居中 |
| `WeekHeaderFontWeight` | `Normal` | 星期名字重 |
