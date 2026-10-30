# Calendar Changelog

本文档记录 Calendar 控件级设计、API、主题契约、Token 和实现结构的变化。它不替代仓库根目录 `CHANGELOG.md`，也不作为正式版本发布说明。

## 2026-10-04

- Theme
  - Mini DateViewer 高度改用模板绑定读取 Calendar 的有效展示资源，消除 IDE 对普通绑定路径的无法解析提示，保持 Gregorian 与 LunarCalendar 的动态尺寸。
  - RangeBarPanel 通过 Calendar 已拥有的 DateViewer 模板部件只读属性与 TemplateBinding 获取布局源，消除模板兄弟节点名称绑定的 IDE 解析提示。
  - 鼠标按下不再触发日期焦点描边，键盘可见焦点仍有提示；LunarCalendar Content 选中日期改为主色背景，公历和农历文字统一使用浅色。
  - Mini 日期 Today/焦点边框改为独立覆盖层，选中与焦点切换不再缩小值框内容区或移动日期文字。
  - Fullscreen 面板移除顶部横线，Mini 保留；Fullscreen 星期标题右对齐，并使用 Ant Design 6.0 的行高与右侧/底部留白。
- Token
  - CalendarToken 增加 WeekHeaderHeight 与 WeekHeaderPadding，按共享尺寸资源计算 Fullscreen 星期标题布局。

## 2026-10-03

- Architecture
  - Calendar 正文组合 DateViewer，删除独立 View、Cell、Builder、Automation 与旧主题；Value/Mode 仍由 Calendar 拥有，月份选择保留并截断日号，周号选择以 Date 来源提交行首日期。
  - CalendarCellContext 继承 DateViewerCellContext，构造参数与 CellType 统一使用 DateViewerCellType；Gallery 的业务模板同步消费共享类型。
  - 农历投影直接消费共享 CellModel，缓存包含真实可见日期首尾；LunarCalendarCellContent 只呈现，统一 Cell 承担交互与 Automation。
  - RangeBars 消费 DateViewer 的实际 arrange 几何，反向端点按升序绘制，不改写业务对象；移除重复布局输入和网格计算。
- Theme
  - header 定制覆盖稳定有效区域，item/itemContent 专用 Style 跨共享面板的真实模板路径，正式测试以编译 AXAML 声明专用样式。

## 2026-10-02

- Docs
  - 按已批准的共享 DateViewer 架构重写架构、实现、行为、农历、范围条、Token 与 Semantic Part 文档；不宣称源码已迁移。
  - 日期拓扑、基础 Cell、焦点与范围算法统一引用公共面板设计，删除独立 CalendarView 作为长期架构约束的描述。
- Architecture
  - Calendar 负责业务 Header、Value/Mode、事件、呈现与 RangeBars，LunarCalendar 在共享公历模型上扩展内容。
  - 农历缓存按显示区域与 Provider revision 失效，业务条消费共享拓扑/实际几何，均不创建第二个选择 owner。
  - 解除旧模板与 Semantic route 兼容约束，要求新专用 Style 按真实组合结构命中。

## 2026-08-17

- Semantic Part
  - 为 `Calendar` 与 `LunarCalendar` 建立 `root`、`header`、`body`、`content`、`item` 和 `itemContent` 公共契约。
  - `item` 与 `itemContent` 是运行时 Part，发布完整 owner-scoped `SelectorRoute`，经 scope marker 链跨越 Calendar 根模板与运行时 Cell 模板，不向用户暴露 internal 类型或 `PART_*` 名称。
  - 将 Fullscreen 背景由 CellHost 上移到控件级，面板默认自带 `FullPanelBg`（Mini）/ `FullBg`（Fullscreen）背景，并把根表面的 Background、BorderBrush、BorderThickness、CornerRadius、Padding 投影到模板根 Border，使 root 表面定制只落在面板外圈，Part 与表面 Setter 按 Avalonia 原生样式优先级生效。
- Theme
  - 在默认 Header、BodyPresenter、CalendarView 和两个 Cell 模板的 ItemContent 节点声明静态 marker；Cell marker 在 `CalendarViewCell` 构造时经生成常量一次性添加，容器池复用与模式切换不改变 Part 身份。
  - Fullscreen 单元格的 ItemContent 内容区域在没有自定义 `CellTemplate` 时也保持可见（对应 Ant Design full 单元格始终渲染的 `date-content` 区域，高度取自 `FullCellMinHeight` 中的 dateContentHeight 部分），使 `itemContent` 语义 Part 始终具备可标注的几何区域；Mini 单元格维持隐藏。
  - LunarCalendar 根主题经元素语法 `ControlTheme.BasedOn` 继承；语义资产解析补齐元素语法 typed BasedOn 支持，纯继承主题同样通过模板验证。
  - 默认 Header 的 Year/Month 选择器与 Month/Year 切换组补齐白色容器背景（`ColorBgContainer`），与 Ant Design Calendar Header 的 Select 与 Radio 按钮一致；边框与选中态沿用既有 outline 样式。年/月选择器直接使用标准 `ComboBox`，白色背景由 `AddOnDecoratedBox` outline 变体的默认背景提供；切换组白色背景经 `OptionButtonGroup` 自身 `Background` 绘制，不遮挡外框、分隔线与选中描边，也不改变 Header 区域自身背景。
- Gallery
  - 增加延迟创建的 Calendar Semantic Parts Preview（预览使用默认全屏 `Calendar`，与 Ant Design Semantic DOM 演示的裸 `<Calendar />` 一致，不设宽高、`Fullscreen` 或 `CellTemplate`），并增加 object/function 两种 root 表面定制对照示例（object：宽 600、圆角 8、Padding 10、`ColorPrimaryBg`；function：2px 绿色边框、圆角 10、Padding 10、半透明绿底），对齐 Ant Design `style-class` 示例经 `styles` 与 `classNames` 作用于根元素的全部取值。
- Tests
  - 覆盖 descriptor、静态与运行时 marker、42/48/12 网格拓扑、scope 链路由、根表面投影、容器复用，以及 Gallery 语义预览生命周期与样式示例生效。

## 2026-08-02

- Fixed
  - `Fullscreen=false` 改为无内部外框的卡片内容布局，`MiniContentHeight` 约束包含 WeekHeader 与六行日期的完整 CalendarView。
  - 默认 Header 的 Month/Year 切换改用 `OptionButtonGroup`，并对齐 Header、body 分隔线与纵向 Padding。
  - Header 控件组在卡片容器中保持右对齐；Year 模式月份 Cell 使用 `YearMonthCellWidth` 铺开选中背景，匹配 Ant Design 面板布局。
  - Mini 日期选中态改为主色实心与浅色文本，today、outside、disabled 和星期标题对齐各自的 SharedToken 状态语义；Fullscreen 选中态保持独立规则。
- Gallery
  - 卡片示例使用外部 300 宽边框容器，容器在 Showcase 内容区左对齐，并同步 Card 标题与三种语言说明文案。

## 2026-08-03

- Fixed
  - 默认 Header 年/月选择器的下拉列表视口按 Ant Design Select 的 256 高度（8 个 32 高选项）配置，弹层内边距保持由 ComboBox Token 控制。
  - LunarCalendar 卡片内容高度改由专用 Token 按双行 Cell 和共享间距派生；presentation adapter 把有效高度传给 CalendarView，修复六周网格中相邻行贴靠，同时保持普通 Calendar 的 256 高度不变。
  - ShowWeek 的周序号前缀表头改为视觉留空并保留本地化 Automation 名称；WeekHeader selector 同步匹配实际的 AtomUI `TextBlock`，标题前景改为继承 CalendarView 根的 `ColorText`，避免紧凑卡片中可见 `Week` 文本挤压首列间距并稳定恢复 Token 前景色。
- Internal
  - 农历面板预计算类型由 `LunarCalendarSnapshot` / `LunarCalendarSnapshotKey` 重命名为 `LunarCalendarPanelData` / `LunarCalendarPanelDataKey`，同步更新 adapter 字段与构建方法，不改变匹配、失效或 Provider 查询行为。
- Gallery
  - 在 Card 示例后增加可选择日历示例：Header 面板变化只同步 Calendar 面板值，实际日期选择同时更新面板值和三语 Alert 反馈。
  - 移除可选择日历后的 Year、ValidRange/DisabledDate、CellTemplate、FullCellTemplate、HeaderTemplate 和事件示例及对应页面专用状态。
  - 在 Calendar Showcase 末尾增加聚焦的 Customize Header 示例：通过 `HeaderTemplate` 的强类型 `CalendarHeaderContext` 读取 `Value` / `Mode`，并使用公开命令驱动年月选择和模式切换，不依赖默认 Header 或 Calendar 的内部模板结构。
  - Customize Header 的 8 像素内容边距使用 `Thickness` 值表达，避免把 `double` 类型的 SharedToken 动态资源绑定到 `Margin` 后在内容 materialize 或主题变化时触发类型转换异常。
  - 在可选择日历后增加 Show Week 示例，同一 ShowcaseItem 同时展示 `ShowWeek=True` 的 Fullscreen 与 Card 模式，卡片沿用外部 300 宽边框容器。
  - 应用级节假日 Provider 示例移到 Fullscreen LunarCalendar；紧凑卡片只展示内置农历、节气和传统节日，两个农历示例均显式关闭周末红色高亮，示例 Provider 的业务名称固定使用中文，不随 Gallery Culture 翻译。
- Docs
  - 新增 `lunar-calendar-design.md`，定义 `LunarCalendar` 在 Calendar 模块内的 Public API、Fullscreen/Card × Month/Year 模式、1900-2100 历法范围、二十四节气、传统节日、节假日 Provider、专用 Cell/adapter、Token、RangeBars 避让和验证契约。
  - 同步 overview、implementation、behavior、range-bar、token 与 Data Display 导航，并按当前源码将基础 Token 名称统一为 `CalendarToken` / `CalendarTokenResource`。

## 2026-07-31

- Fixed
  - 将 Year 面板修正为 3×4，并将纵向键盘步长修正为 3 个月。
  - 周序号 Cell 现在继承行首禁用状态，可选择行首日期并报告 `Date` 来源。
  - 修复 `DateTime.MinValue/MaxValue` 附近的网格、焦点与 Header 年份选项溢出。
  - 模板替换、模式收缩和 VisualTree detach 统一释放池化 Cell 的 owner、model、context 与模板引用。
  - WeekHeader 改为与 CellHost 同列的 Grid；CellTemplate 使用独立内容行；Fullscreen/Mini 使用不同根布局、Cell 视觉和可见焦点描边。
  - CalendarView Automation 补齐 `ISelectionProvider`，Cell 返回 SelectionContainer 并使用完整本地化名称。
- API
  - `Value` 与 `Mode` 默认绑定模式改为 `TwoWay`。
  - Calendar 文案和日期文化跟随 AtomUI 全局语言服务。
- Gallery
  - Fullscreen 示例改用真实宽布局，HeaderTemplate 绑定 `CalendarHeaderContext.Value`，事件示例移除 Loaded 匿名订阅。

- Docs
  - 将 Calendar 行为设计归档到 `docs/controls/desktop/data-display/calendar/`，并同步 overview、implementation、Token 与分类入口的导航。
  - 明确 CellTemplate/FullCellTemplate 优先级、月份首尾禁用、周序号选择、方向键跳过禁用 Cell、语言资源和 Template 生命周期契约。
  - 将 Automation 文档修正为 Avalonia 可移植的 `Table`/`ListItem` 与 SelectionItem 契约，不再宣称跨平台 Grid/GridItem provider。
  - 新增范围条专项设计，定义 `RangeBars` / `CalendarRangeBar` 契约、Calendar body overlay 绘制层、可见行坐标算法、非 Visual 资源宿主生命周期和验证边界。
  - Calendar 专属 Token 增加 `RangeBarHeight`，作为范围条 overlay 的默认条高；单条范围条仍通过实例属性覆盖颜色和高度。

## 2026-07-30

- Breaking / Rewrite
  - 按 Calendar 当前行为规则彻底重写控件，抛弃旧 WPF/Avalonia Calendar 移植体系（`BaseCalendarButton`、`CalendarDayButton`、`SelectedDatesCollection`、`BlackoutDates`、Decade 面板等全部删除）。
  - 全新 Public API：`Value`/`Mode`/`Fullscreen`/`ShowWeek`/`ValidRange`/`DisabledDate`/`CellTemplate`/`FullCellTemplate`/`HeaderTemplate` + `ValueChanged`/`Selected`/`PanelChanged` 事件。
  - `CalendarMode` 收敛为 `Month`/`Year`；`CalendarDateRange` 收敛为只服务 ValidRange（`end<start` 抛异常）。
  - 内部新增纯面板 `CalendarView`（命名空间 `AtomUI.Desktop.Controls.Internal.Calendar`），与 DatePicker 的 CalendarView 子系统完全隔离。
  - Token 收敛为独立的 `CalendarControlToken` 语义（`FullBg`/`FullPanelBg`/`ItemActiveBg`/`YearControlWidth`/`MonthControlWidth`/`MiniContentHeight`/`FullCellMinHeight`），旧 `CalendarToken` 保留给 DatePicker。
  - 事件顺序固定为 `PanelChanged -> ValueChanged -> Selected`；程序设值不触发用户事件。
  - 新增 roving focus、方向键导航和基于 Avalonia `Table`/`ListItem`/`ISelectionItemProvider` 的 Automation 契约。
  - Gallery 重写为九个示例，控件文档与 Token 文档同步更新。

## 2026-06-26

- Docs
  - Add LLMS metadata, semantic parts and export source mapping for `Calendar`。
  - Align generated output paths with `controls/calendar/index-cn.md` and `controls/calendar/semantic-cn.md`.

## 2026-06-24

- Docs
  - Complete Calendar desktop architecture and implementation docs with source-derived API groups, template parts, state flow and verification boundaries.
  - Establish Calendar desktop architecture documentation under `docs/controls/desktop/data-display/calendar/overview.md`.
  - Add Calendar implementation documentation covering source ownership, state flow, lifecycle, resources, AOT boundaries and maintenance invariants.
  - Add Calendar control-level changelog.
  - Add Calendar Token documentation covering `CalendarControlToken`。
