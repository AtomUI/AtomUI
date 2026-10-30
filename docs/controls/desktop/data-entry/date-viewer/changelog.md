# DateViewer Changelog

本文记录 DateViewer 家族的设计、API、主题与文档变化，不替代发布说明。

## 2026-10-04

- API / Token
  - DateViewer 本地化目录增加 Today 与翻页箭头文案；新增 CellRowHeight、DateBodyPadding、HeaderHorizontalPadding 资源并更新默认面板尺寸。
- Performance
  - 无 Hover 时跳过 DatePanel 指针边界检查；有 Hover 时仅检查共享 session 的一至两个面板，消除 DateViewer 示例页多面板滚动时的整窗视觉树遍历。
- Behavior
  - 统一有效语言的周首、首周与跨年周号，恢复周模式的整行 Hover，并保持周号半透明文字。
  - 同端点范围预览只显示一个有效端点；空值周期浏览不产生主色选中态。
  - 范围 Preview 改为覆盖 committed Cell 视觉状态，反向编辑时只显示规范化后的两个预览端点，不再同时显示旧端点。
  - 使用单一视觉端点状态连接 selected 与 Preview，避免 Hover 时主色端点闪烁。
- Theme
  - Content 单元格改用状态表面、完整列宽范围/周背景、前景内容三个绘制层；留白由 DateViewerToken 从共享 Token 推导，移除范围背景的负 Margin，并改用 AtomUI 的圆角筛选 converter。
  - 默认 Header 使用 paddingXS 作为左右内边距，双月导航箭头不再贴住面板边缘，标题仍居中且底部分隔线保持全宽。
  - Content 中普通 Today 单元格即使受日期边界或 DisabledDate 禁用，仍保留主色顶边、浅主色整格背景和主色日期文字；禁用只控制可选择性。日期值框在 Content 中不绘制额外边框。
  - Content 日期的可见表面背景和日期文字使用 MotionDurationSlow 过渡；透明背景提供 Hover 与选中状态的连续起点，关闭动效时立即切换。
  - Content 单元格使用全幅状态表面、顶部边界和右上角日期；4px 视觉内缩保留外层完整 Pointer 命中，范围与整周背景仍跨列连续。CellTemplate 与 FullCellTemplate 继续占用原有内容区域，范围端点保留居中主色值框并与半格背景相接。
  - 动态创建的星期标题改由 DatePanel 内部主题参数接收对齐、行高和内边距，避免依赖无法命中的模板后代选择器。
  - 独立 DateViewer 默认采用四方向导航、居中加粗周期标题和 Today 页脚；Gallery 基础示例初始为空选择并移除面板下方的值文本。
  - 日期面板宽度、正文留白与行列节距按 Ant Design 面板 token 公式同步；Content 继续使用父布局宽度，托管面板隐藏独立页脚。
  - 与 Ant Design 6 对齐：日期、月、季度、年的值框背景和 Week 整行背景采用 MotionDurationMid；周行通过透明色与状态色过渡，禁用动效时移除过渡。
  - 月、季度、年使用固定 PeriodCellWidth 和 PeriodCellPadding，Compact 值框在行内垂直居中。
  - 范围起点仅保留左侧圆角，终点仅保留右侧圆角，并保持浅色中段连续衔接。
- Gallery
  - Semantic Parts 预览改用单个 DateViewer，并将标题与 content 部件说明同步为单面板语义；RangeDateViewer 仍在 Examples 中演示。
  - 将独立 DateViewer 的默认 Today 页脚发布为可定制的 footer Semantic Part，Gallery 预览增加部件说明与专用 Style 命中验证。

## 2026-10-03

- API
  - 同步已存在的 DateViewer/RangeDateViewer 属性、原子 DateViewerRange、强类型事件与 Cell/Header 上下文。
  - Header 上下文包含父面板、Page/Period 参数与周期导航状态；范围双面板提供两个真实 Header。
- Theme
  - 记录五个真实叶子主题、池化 Cell 内容结构和已生成的 owner-relative Semantic Style route。
  - 区分 DateViewer.header Optional 与 RangeDateViewer.header Multiple，明确 Cell 的 TemplatedControl 契约。
- Token
  - 同步 DateViewerToken 当前九个资源、默认计算和真实主题消费者。
- Gallery
  - 新增独立 DateViewer 导航页，覆盖单值/原子范围、五种单位、空值浏览、Compact/Content、强类型模板和专用 Style。
  - 页头与示例使用 XLIFF Catalog，源片段使用现有生成入口，保持 Examples/Semantic Parts 页面范式。
- Docs
  - 主文档从批准目标职责同步为现有公共面板源码事实，保留完整视觉等价的人工验收边界。
- Validation
  - 通过 osx-arm64 NativeAOT 发布、产物验证与可执行文件启动检查；通用 ReactiveUI trimming/AOT 与本机链接器提示仍按构建日志记录。

## 2026-10-02

- Docs
  - 建立 DateViewer/RangeDateViewer 的批准目标架构、实现职责、Token 与 Semantic Part 文档，明确源码尚未迁移。
  - 将完整日期家族共享设计集中到 shared-panel-design.md，作为 DatePicker 与 Calendar 的共同设计入口。
- Architecture
  - 确定一套日期内核、统一 TemplatedControl Cell、单值/范围公共面板与明确宿主 ownership。
  - 解除旧实现兼容限制，要求 DatePicker 家族视觉、交互、绑定与 Form 结果逐步等价验证。
