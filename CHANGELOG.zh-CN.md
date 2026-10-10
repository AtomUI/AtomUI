# 更新日志

AtomUI 的重要变更记录在此文件中。

`AtomUI` 遵循 Semantic Versioning 2.0.0。

英文版本见 [CHANGELOG.md](CHANGELOG.md)。

## 6.2.5

`2026-10-10`

- 破坏性变更
  - 注册：六个控件包公开生成的 `ControlPackageGroup_*` 类型名随版本变化。请同步升级 AtomUI 包、重新编译已编译的消费方，并重新发布裁剪/NativeAOT 应用；详见 [6.2.5 API 迁移指南](docs/releases/6.2.5-api-changes.zh-CN.md)。
- Window
  - 修复应用主题切换后窗口内容背景未更新的问题，同时保留透明窗口行为和显式设置的后备背景。#496

## 6.2.4

`2026-10-07`

- Breaking Changes
  - TimePicker 和 RangeTimePicker：`IsNeedConfirm` 默认启用、24 小时制，普通滚动默认只浏览。需要旧模式时显式设置，并复核提交/Now 时机；详见 [6.2.4 API 迁移指南](docs/releases/6.2.4-api-changes.zh-CN.md)。
  - TimePicker 主题：调整列宽、单元格内边距与循环滚动模板；自定义主题及滚动假设需按迁移指南复核。
  - 包与注册：Release 在 `net10.0` 之外恢复 `net8.0`，增加 .NET 8 裁剪/AOT 构建资产。请同步升级 AtomUI 包、重编译预编译消费方并重新发布裁剪/AOT 应用；构建前提及平台限制见迁移指南。
- TimePicker
  - 分离提交值、候选值和悬停预览；显式确认统一提交有效候选，关闭确认时在关闭或失焦提交。
  - 新增 `IsChangeOnScroll`，时/分/秒循环选择保持居中，列内焦点滚动不再推动外围页面。
  - 修复无效文本、外部值更新、范围端点切换和钉住弹层重复确认；Gallery 示例同步展示实际提交值。
- ColorPicker
  - 为 ColorPicker 和 GradientColorPicker 增加可选 `IsNeedConfirm`，提供本地化确认和取消操作。
  - 草稿编辑与清除可取消，渐变草稿独立，有效输入仅提交一次；保留默认同步模式。
- NativeAOT、构建与打包
  - 仅 .NET 8 目标使用兼容依赖后端；现代目标使用匹配的官方 TypeMap 引擎，并自动转换受支持的预编译 .NET 8 包输入。
  - 将构建工具统一到 AtomUI.Toolchain，校验包的框架与内容契约，工具运行文件不进入应用输出。
  - 修复无 Generator 引用测试项目的冷构建/并行构建错误，避免重复导出导入的本地化 Catalog。
- Gallery
  - 修复分数缩放下 ShowCase 布局循环导致 Form 标签消失；DateViewer 延迟示例初次只物化一项。

## 6.2.3

`2026-10-05`

- Breaking Changes
  - Calendar：将 `CalendarCellType` 替换为 `DateViewerCellType`，`CalendarCellContext` 改为继承 `DateViewerCellContext`；自定义单元格上下文和模板需要重新编译。详见 [6.2.3 API 迁移指南](docs/releases/6.2.3-api-changes.zh-CN.md)。
  - DatePicker：移除旧 `CalendarView` 公共类型，迁移并密封 `DateSelectedEventArgs`，密封 `ChoosingStatusEventArgs`；请按迁移指南使用共享面板契约。
  - Theme 与 Token：替换旧日期面板模板和 Semantic Part 目标，移除过时的 DatePicker Token 键，将 `RangeCalendarSpacing` 更名为 `RangePanelSpacing`，并调整生成的 Calendar/DatePicker Token 枚举值。请重新编译自定义主题并使用具名 Token 成员，详见迁移指南。
  - Registration：六个控件包中版本相关的生成 `ControlPackageGroup_*` 标识变化；请重新编译生成注册消费方并重新发布 trimming/NativeAOT 应用，详见迁移指南。
- DateViewer、Calendar 与 DatePicker
  - 新增独立 DateViewer 和 RangeDateViewer，支持日期、周、月、季度、年选择，Compact/Content 展示、强类型模板、原子范围和 Semantic Part 专用样式。
  - Calendar、LunarCalendar 与 DatePicker 共享日期面板，保留 Calendar 业务 Header、农历内容和范围条。
  - 改进日期面板几何、Today/焦点视觉、周格式与语言切换，统一范围高亮和端点圆角。
  - 修复 RangeDatePicker 端点轮换、弹层锚点、反向 Hover 预览和关闭/重开后的部分确认；保持复用的确认按钮和时间控件可交互。
- 输入与窗口生命周期
  - 防止跨窗口指针输入改变 Rate、Slider、TimePicker、ButtonSpinner、ToolTip 和 TreeView 拖拽状态；禁用、卸载和窗口关闭时安全释放临时状态。
  - 移除 CandidateList、Select 和 Cascader 下拉项继承的分隔线。
- Watermark 与布局
  - 修复 Watermark 本地化 Glyph 资源、owner 挂载及主题/语言更新；跳过无效和完全裁剪的平铺项，同时保持旋转、交错和镜像相位。
  - 修正 Rate、ToggleSwitch、Segmented、Skeleton、ColorBlock 和 Popup 阴影/箭头布局的最终 Bounds 几何，覆盖 Margin 和缩放。
- Gallery 与 Generator
  - 新增本地化 DateViewer 示例和 Semantic Part 预览。
  - 修复 ShowCase 延迟物化和占位视觉，改进放大内容边界、覆盖层与命中，同时保留滚动行为。
  - 缓存生成的注册描述符，减少启动工作并保持注册契约。

## 6.2.2

`2026-09-30`

- 破坏性变更
  - Packaging：产品包现在只提供 `lib/net10.0`；面向 `net8.0` 的应用与类库必须改为 `net10.0` 并使用 .NET 10 SDK。详见 [6.2.2 API 迁移指南](docs/releases/6.2.2-api-changes.zh-CN.md)。
  - 注册与主题：以生成的 TypeMap 替换 Unit/Plan/Sidecar AOT 注册 ABI，移除旧注册、单主题 provider 和资产 Schema 契约。第三方控件包必须使用 v6.2.2 生成器重建；迁移步骤见指南。
  - 构建与平台元数据：退役 `AtomUI.LinkedRegistration*` 资产及自定义主题平台元数据，改用 `AtomUI.Registration.targets` 与标准平台 Attribute。自定义构建导入和主题作者需按指南迁移。
  - 生成 Token 与桌面注册：移除未声明 Own Token 的控件所对应的生成 Token key 类型及 `UseAllDesktopControls()`；DataGrid 和 WindowTitleBar Token 枚举数字值发生变化。集成代码需重建，并改用 `UseDesktopControls()` 和 Token 成员名称；详见迁移指南。
- DataGrid
  - 增加顶部/底部分页区域的额外内容与模板，以及控制页大小选择器显示的 `IsShowPageSizeSelector`。
- Button、WindowTitleBar 与 Steps
  - 恢复 Button 键盘焦点环，并对齐 WindowTitleBar AddOn 与 caption 按钮的几何尺寸。
  - 修复 Steps 自然尺寸测量，同时保留连接线。
- Flyout、FloatButton 与 RibbonBadge
  - 支持通过键盘触发 Flyout 和 FloatButton 组的展开与收起。
  - 修复 RibbonBadge 折角绘制坐标。
- Dialog、Motion 与 Gallery
  - 将 Dialog 遮罩动效移至合成器并统一 Zoom 状态。
  - 增加 Gallery 示例放大与 DataGrid 分页示例，更新 Badge 示例和 Gallery 品牌图形。
- NativeAOT、构建与依赖
  - 以 TypeMap 管线替换 linked registration 与裁剪流程，改进反射裁剪并缩小生成的 Token 描述符。
  - 桌面裁剪与 NativeAOT 发布兼容后续 .NET 10 补丁工具链，Browser 继续使用固定工具链。
  - 将 Avalonia 升级到 12.1.3。

## 6.2.1

`2026-09-22`

- 破坏性变更
  - Pagination：固定导航容器与 Semantic Part marker 池由 11 个缩减为 9 个，同时将可见页码窗口与快速跳页行为对齐 Ant Design。依赖旧数量的快照、自动化或主题工具需要同步更新；迁移说明见 [6.2.1 API 变更示例](docs/releases/6.2.1-api-changes.zh-CN.md)。
- Pagination
  - 新增可通过指针与 Enter 键触发的向前/向后快速跳页项、本地化自动化名称、默认 5 页跳转，以及 `IsShowLessItems=true` 时的 3 页跳转。
  - 新增 `IsShowLessItems` 与 `IsShowPrevNextJumpers`；页码窗口变化时复用稳定导航容器，保持 hover/focus 过渡连续。
  - 新增 `SizeChangerTemplate` 与 `PaginationSizeChangerContext`，允许应用替换默认 page-size ComboBox，同时继续由 Pagination 管理状态并保留 TwoWay binding。
- Menu
  - 防止键盘导航到叶子项时打开空子菜单，并在集合动态变化后正确协调钉住或延迟填充的子菜单状态。
- Steps
  - 修复垂直副标题叠印，并让 Inline hover 背景与 Panel 裁剪正确跟随 Steps 类型。
- Message 与 Notification
  - 管理器激活时重新激活窗口反馈层，使跨管理器反馈层叠顺序保持确定。
- Gallery
  - 修复登录 Form 的选项行布局。
  - 保持 ColorPicker Semantic Part 触发器完整显示在钉住弹层上方，并新增 Pagination 自定义 size changer 示例。

## 6.2.0

`2026-09-18`

- 破坏性变更
  - SearchEdit：将公共 `SearchEditButtonStyle` 枚举重命名为 `SearchButtonType`。`SearchButtonStyle` 名称和枚举值保持不变，请更新 C# 与 XAML 中的枚举类型引用。详见[6.2.0 API 变更示例](docs/releases/6.2.0-api-changes.zh-CN.md)。
  - OtpLineEdit：移除 `SeparatorInterval`。设置 `Separator` 后会在每个相邻 cell 之间显示分隔符；自定义内容使用 `SeparatorTemplate`，整体间距使用 `OtpLineEditToken.CellGap`。详见迁移指南。
  - Button：移除 `CustomBackground`，改用标准 `Background` 和 `BorderBrush` 属性。详见迁移指南。
  - ProgressBar：移除受保护的 `AbstractGeneralProgressBar.DrawIndicatorBar` 辅助方法；自定义派生控件需要自行实现绘制或使用模板/Semantic Part 样式。
  - Separator：移除 `AbstractSeparator.DashStyle` 和 `DotStyle`；需要标准虚线定义时使用 `SeparatorRail.DashStyle` 和 `SeparatorRail.DotStyle`。
  - Theme manager：增加 `IThemeManager.SemanticParts` 语义部件 registry 属性。自定义实现必须增加该属性，内置 `ThemeManager` 无需改动。详见迁移指南。
  - Notification：移除旧的八参数构造函数以及 `CardExpiredPollingInterval` / `CleanupPollingInterval` 属性；生命周期调度器现在是唯一的过期协调器。
  - Notification 管理器：新增 `IMessageManager.DestroyAll()` 和 `INotificationManager.DestroyAll()`；自定义实现需要增加这些方法。
  - Transfer：将受保护的 `BuildSourcePanelSource` 和 `BuildTargetPanelSource` 返回类型改为 `IReadOnlyList<IItemKey>?`；自定义派生控件需要更新重写方法和调用方。
  - Notification Token：移除 `NotificationMarginBottom`、`NotificationContentMargin` 和 `HeaderMargin`；生成的 Token 枚举值按当前源码声明生成。
- Semantic Part 与 Gallery
  - 为输入、导航、数据展示、反馈和布局控件族增加生成式 Semantic Part 契约和专用样式入口。
  - 为 Popup、Dialog、Drawer、Tour 和 ImagePreviewer 增加跨视觉根高亮与 Gallery 预览，修复 owner 作用域样式和本地化目录一致性。
  - 增加 ComboBox、Splash、GroupBox、Expander、DataGrid、Menu、TabControl、TabStrip、NumericUpDown 等控件的语义示例。
  - 修复 sticky tab 提升更新与模板销毁竞态导致访问已释放视觉部件的问题。
- Popup、Dialog 与 Theme
  - 修复 Dialog、Drawer、PopupConfirm、Menu 和 Picker 控件的钉住遮罩所有权、跨窗口弹层路由、语义作用域跟踪和根表面定制。
  - 修复主题源码命名空间漂移，将 Semantic Part 预览契约统一放在公共 `AtomUI.Theme` 命名空间。
- 异步、动效与缓存
  - 防止异步展开/搜索加载的并发重复生产，正确区分显式取消，并修复有界主题缓存的完成发布竞态。
  - 改进反馈堆叠、Masonry 动效、Segmented 选中滑块过渡以及不可见控件的任务关停行为。
- 本地化、构建与 AOT
  - 让生成的设计 Token 枚举与当前源码声明保持一致；新增或重命名 Token 属性属于 v6.2.0 API 表面变更。
  - 恢复 canonical XLIFF 单元排序并补齐 Gallery 葡萄牙语（巴西）目录覆盖。
  - 增加以上一 NuGet 版本为基线的 API 与包布局发布门禁，并使用 content-addressed 仓库构建任务工具集避免过期 MSBuild 程序集。
  - 保持 linked registration、trimming 和 NativeAOT Gallery 验证路径与扩展后的语义控件表面一致。

## 6.1.9

`2026-09-11`

- 破坏性变更
  - Build：AtomUI MSBuild 任务改为通过 SDK 编译器适配器在单次运行的 .NET 进程中执行，不再从影子副本进程内加载 `AtomUI.Build.Tasks`。构建工具以 `net10.0` 程序集交付，因此消费方项目需要使用 .NET 10 SDK 构建；原有的进程内加载入口和影子目录结构不再保留。产品库目标框架不变。迁移示例见 [6.1.9 API 变更示例](docs/releases/6.1.9-api-changes.zh-CN.md)。
- DataGrid
  - 修复单选行在行之间切换时需要点击两次的问题。#461
  - 修复排序后回收单元格泄漏排序状态的问题。#462
  - 修复动态设置右冻结列数量不生效的问题。
- Motion、Collapse、Expander 与 NavMenu
  - 以共享执行器统一内容展开与收起动效：同步驱动完整高度与透明度插值、稳定内容裁剪、反转连续性，并由单一执行所有者和单一时钟驱动。修复手风琴在展开收起与快速切换分支时的闪烁和抖动。
- Window 与 ImagePreviewer
  - 修复全屏标题栏 Logo 重复挂载导致的崩溃。
  - 修复 ImagePreviewer 预览窗口标题栏按钮无法点击的问题；Windows 标题栏 AddOn 按钮与开关按钮几何对齐托管 caption button，同时保留 Linux/macOS 视觉。
- Avatar 与图片加载
  - 通过新增的 `SvgConformanceMode` 选项支持含重复 id 的安全本地 SVG 资源：`Compatible`（默认）容忍重复 id，`Strict` 将其判定为 `InvalidImageData`。
  - 将文字测量、缩放与居中移入模板，修复 Avatar 在短文本、临界宽度和超长文本下的文字居中问题。
- BorderBeam
  - 新增 `Count` 属性，在单一动画时钟和 Presenter 内绘制多条等距光束。
  - 修复未设置 `Outset` 时的默认边框几何，使流光贴合内容边界。
  - 补充多光束、悬停触发光束与自定义容器 Gallery 示例。

## 6.1.8

`2026-09-08`

- 破坏性变更
  - DataGrid：以基于 Range 的 `IDataGridSource`、不可变 `DataGridQuery`、`DataGridSelectionState` 和 `CurrentRowKey` 契约替代集合型 `ItemsSource` 与可变 CollectionView 状态。本地集合必须包装为 `DataGridLocalSource<T>`，远端数据源需实现 `IDataGridSource`；列查询改为通过 `FieldId` 和 Source Schema 绑定，不再使用成员路径排序/过滤 API。
  - 图片加载：以封闭的 `ImageSource` 类型层次及其构造器替代 `ImageLoadSource` 工厂；将 `ImageCacheMode` 拆分为 `ImageCacheReadPolicy` 与 `ImageCacheStoragePolicy`；加载结果改用 `ImageLoadOrigin`、`ImageSourceValidation` 和内容身份描述缓存与校验状态。
  - 主题：具体 `[ControlDesignToken]` 类型现在必须为 `sealed`；需要共享继承时必须引入显式标记的抽象 Token 层。`ColorPickerToken` 现已封闭，不能继续派生。迁移示例见 [6.1.8 API 变更示例](docs/releases/6.1.8-api-changes.zh-CN.md)。
- DataGrid
  - 新增基于本地或远端 Range Source 的不可变查询、分页、分组展开与声明式选择状态，支持有界 Viewport 加载、取消、Snapshot 校验和远端能力接口。#455
  - 修复排序后选中行样式丢失，并在数据变化时正确重置 Viewport Range 与自动行高估值。#454 #457
- 图片加载与 ImagePreviewer
  - 新增封闭的强类型图片来源模型、来源校验、内容寻址缓存、独立的缓存读取/存储策略以及更安全的 borrowed image 所有权。
  - 新增 `ImageSwitchMode.Immediate` 和 `WaitForLoaded`，并修复快速切换预览时过期加载覆盖当前图片的问题。#450
- Window 与导航
  - 新增 `Window.IsTitleVisible`，改进标题栏 Logo 的实时回退行为，并扩充 Gallery 标题栏示例。#451
  - 统一 NavMenu 指针和键盘激活顺序，保护可重入选择变化，并恢复 Pressed 视觉与动效 Easing。#452
- Select 家族
  - 修复 Select、TreeSelect 和 Cascader 多选模式的 Prefix 间距，并统一 Select、AutoComplete 与 Mentions 的空结果提示。
- NativeAOT、Browser Gallery 与构建
  - 保留 trimmed/NativeAOT 应用跨项目和 binary-only 依赖的 linked-registration 闭包。#453
  - 普通 Browser Gallery 构建继续启用原生 WebAssembly 链接，确保运行时包含 SkiaSharp 原生资产。
  - 修复 NuGet 发布产物上传路径，将 canonical XLIFF 统一为单个末尾 LF，并新增 VSTest 临时目录自动清理且不收集崩溃 Dump。
- Gallery、依赖与平台
  - 降低 ShowCase Masonry 在窗口缩放过程中的闪烁。
  - 将 Avalonia 升级到 `12.1.2`，并简化平台相关项目接线。

## 6.1.7

`2026-09-02`

- DataGrid
  - 修复启用网格线和冻结列时左上角表头在行表头旁额外绘制列分隔线的问题。
- TextBox
  - 修复 `SizeType=Small` 与自定义高度 TextBox 的布局，使输入框架正确跟随共享尺寸 Token 或本地高度。
- Button 和 Browser Gallery
  - Browser 环境下的桌面控件改用共享的 Button、DropdownButton 和 IconButton 主题资产，不再维护 Browser-only override 副本，使 Button 家族在不同目标上的视觉表现保持一致。
- 文档
  - 刷新 README 赞助者致谢信息。

## 6.1.6

`2026-08-29`

- 破坏性变更
  - Avatar：将 `Avatar` 和 `AbstractAvatar` 移至 `AtomUI.Controls`；以强类型 `Source`、`FallbackSource` 和 `RequestOptions` 替代 `Src`、`BitmapSrc`；将 `TextRenderTransform` 调整为内部 API；并将生成的 Avatar Token API 移至 `AtomUI.Controls.DesignTokens`。`CardMetaContent.Avatar` 现在使用基础控件包中的 Avatar 类型。
  - ImagePreviewer：以包含不可变 `ImagePreviewItem` 的 `ItemsSource` 替代 `Source`、`Sources`、`FallbackSource`、`MaxConcurrentLoads` 及预览器专用的 source/loader 契约，图片来源统一使用 `ImageLoadSource`。并发改为通过应用级 `UseImageLoading` 配置，标题解析器需接受 `in ImagePreviewTitleResolveContext` 并读取 `context.Item`。
  - 输入控件：`LineEdit` 不再继承 AtomUI `TextBox`；`LineEdit`、`TextBox` 和 `TextArea` 现在共享公开基类 `AbstractTextInput`。`OtpLineEditCell` 及其生成的单元格 Token API 调整为内部实现；OTP 外观应通过 `OtpLineEdit` 和共享输入框架契约配置。迁移示例见 [6.1.6 API 变更示例](docs/releases/6.1.6-api-changes.zh-CN.md)。
- 图片加载、Avatar 与 ImagePreviewer
  - 新增应用级统一图片加载管线，提供 `AsyncImage`、强类型 URI/文件/资源/存储/字节/流/图片来源、请求选项、加载状态、进度与错误事件、有界并发、优先级以及编码/解码缓存。
  - 新增安全的网络 SVG 加载，并提供重定向、响应大小、图片尺寸和凭据转发限制；同时强化取消、缓存所有权以及附加/分离生命周期行为。
  - 将 Avatar 和 ImagePreviewer 迁移到统一管线，支持缩略图/回退来源、重新加载、封面与当前图片加载状态、不可变预览项和稳定的标题元数据。
- 输入控件与 Form
  - 以 `AbstractTextInput` 和 `InputControlFrame` 统一 TextBox、TextArea、LineEdit 与 OTP 输入的校验、状态、尺寸、外观变体、AddOn 和反馈架构。
  - 编辑期间保持 TextBox 输入框架宽度稳定，并在模板和生命周期变化后保留校验反馈订阅。
- Steps、Masonry 与导航
  - 新增 Steps `Panel` 类型及 Filled、Outlined 变体，提供 Token 化表面和响应式布局行为。
  - 新增 Masonry `StableColumns` 和 `Reflow` 布局策略，默认使用稳定列，同时避免图片解码与布局反馈循环，并恢复最终宽度测量和加载骨架布局。
  - TreeView 重新附加到可视树后恢复节点绑定；NavMenu 临时弹层关闭时保留选择；语言切换后重新测量 ToggleSwitch 内容。
- TabControl 和 TabStrip
  - 修复 overflow 页签关闭行为：遵循最终生效的 `IsClosable`，统一转发到控件 owner 的关闭流程；关闭被拒绝或取消时保留源页签和菜单项，并确保 `Items` 与 `ItemsSource` 路径下的 `Closing`/`Closed`、选择状态和集合语义一致。
- Window 与平台
  - 新增公开的 `WindowTitleBarButton` 和 `WindowTitleBarToggleButton`，用于实现标题栏 AddOn 操作。
  - 修复 Windows 实时调整窗口大小时的合成回退对齐，以及 macOS 录屏或显示几何变化后的 caption button 位置。
- Motion、Popup 与生命周期
  - 控件实际不可见时暂停周期性动画、计时器和视觉工作，恢复可见后从当前状态继续。
- 构建、打包与本地化
  - 从按内容键控的影子副本加载 MSBuild Task，避免重复构建时使用过期 Task 程序集或发生依赖冲突。
  - 将托管构建输出迁移到 `.artifacts`，并在消费项目树中隐藏语言包实现文件。

## 6.1.5

`2026-08-21`

- Dialog、Drawer 和 MessageBox
  - 新增 `Dialog.IsMaskClosable`（默认 `true`）和 `MessageBoxOptions.IsMaskClosable`，控制点击模态遮罩是否关闭弹窗；`IsClosable` 仍只控制头部关闭按钮，设为 `false` 时遮罩点击会被忽略。
  - 让 Dialog 或 Window Drawer 内容中的 Popup、Flyout、MenuFlyout、ToolTip、ContextMenu 和带 Popup 控件始终留在所属 Window 的 `TopLevel`，位于模态内容之上并保留正常 light-dismiss 层级，包括 DataGrid 过滤弹层。
  - 直接实例化的 Dialog 默认居中，与 `DialogOptions` 和静态 Dialog API 保持一致，同时保留显式 `Custom` 锚点和偏移语义。
  - Dialog 关闭动画期间保持完整 Surface 内容附着，使文本、输入控件、Footer 操作和阴影同步完成动画与释放。
- Popup
  - 新增可选的 `Popup.SurfaceBackground`，默认值为 `null`。设置非空 Brush 时启用 host-owned surface；内置 Flyout、ToolTip、ContextMenu、选择器、Picker、菜单、Tour 和 ColorPicker 家族继续使用既有 content-owned surface，不重复绘制 frame 表面。
- ToolTip
  - 新增 `TextWrapping`（默认 `Wrap`）和 `TextTrimming` 附加属性，长提示文本在 `ToolTipMaxWidth` 内换行，而不是被裁剪。
  - 允许以 `ToolTip` 实例作为 `Tip` 时覆盖宿主的呈现属性；实例上显式设置的呈现属性优先，未设置时回落到宿主。
  - 将 `IsOpen` 视为声明式的期望打开状态，与 `Tip` 就绪状态和宿主挂载状态协调，而不是设置即打开的边沿触发行为。
- Window 和 WindowTitleBar
  - 统一逻辑树中各标题栏的宿主行为与 CSD 几何；内容区标题栏获得标题按钮、拖动和双击行为，但不拥有尺寸提示。
  - 隐藏 AtomUI 标题栏时保留 `WindowDecorations.Full`，仅隐藏绘制层，避免留下标题栏高度的空白带。
- 候选交互
  - 统一 Select、AutoComplete、ComboBox、Cascader 和 Mentions 的指针与键盘候选导航，使高亮项与 `Enter` 提交目标保持一致；指针移动仍不会提交选择。
- NativeAOT 和 Build
  - 按程序集 identity 和契约 hash 规范化 linked-registration Sidecar 输入，优先使用正式 Project 或 Package Sidecar，再回退到 metadata extraction；发现冲突 manifest 时直接报告，而不是向 linked build 输入重复注册计划。
  - 在隔离的 .NET task host 中运行 linked-registration、Localization 和主题资产 MSBuild Task，避免不同构建之间发生 Task 依赖与加载上下文冲突。

## 6.1.4

`2026-08-18`

- 破坏性变更
  - SearchEdit：以 `SearchRequested` 替代 `SearchButtonClick`。事件处理器需迁移到 `SearchRequestedEventArgs`，从中读取查询文本快照以及 `Button` / `EnterKey` 触发来源；Enter 现在默认发起搜索，可通过 `IsSearchOnEnterEnabled=false` 禁用。
  - Localization：移除 `LanguageCatalogAttribute`、`LanguageCatalogDescriptor` 和 `TranslationBundleDescriptor` 的 `ContractVersion`，包括 Descriptor 构造参数；移除 `AtomUILanguageContractVersion` 和 `AtomUIRequireVerifiedLanguageContract`；并以 `AtomUIBuildTasksAssembly` 替代自定义构建任务路径覆盖属性 `AtomUILocalizationBuildTasksAssembly`。自定义 Catalog 和语言包需改用稳定 Catalog Key 与源文本 fingerprint，并基于 6.1.4 重新构建。迁移写法见 [6.1.4 API 变更示例](docs/releases/6.1.4-api-changes.zh-CN.md)。
- AOT、Generator 和 Build
  - 为 trimming、NativeAOT 和 WebAssembly AOT 应用新增编译期 linked registration。AtomUI 现在根据 C# 和 AXAML 使用情况生成静态注册计划，保留所需控件包资源，并通过有界的包级 fallback 处理动态场景，不进行运行时程序集扫描。
  - 新增构建期 linked-registration sidecar 和包 manifest；普通构建不加载 publish analyzer，Gallery 普通构建不再隐式执行 publish 工作，并减少重复生成的 Descriptor Factory。
  - 为第三方控件包新增 `ControlPackageRegistrationEntryAttribute` 和默认包粒度注册；真正可独立裁剪的控件族可以显式启用目录粒度。
  - 集中维护 NuGet 发布清单，将 `AtomUI.Desktop.Controls.Extras` 纳入扩展包 build、pack 和产物校验，并在项目没有 AXAML 输入时跳过主题资产生成。
- Window 和导航
  - 为 `Window` 新增 `IsMinimizeCaptionButtonVisible` 和 `IsMaximizeCaptionButtonVisible`，补齐最小化、最大化、关闭、全屏和置顶 caption button 的独立可见性控制，同时继续由平台能力和窗口状态决定操作是否有效。
  - 修复 Wayland 启动时原生标题栏闪现；避免 trimmed Window 应用保留完整 Ant Design 图标目录；为标题栏 Logo 与 LeftAddOn 新增 Token 化间距。
  - NavMenu 新增可配置的折叠 Tooltip，支持节点级内容、Header fallback、Placement 和延迟设置。
  - 修复 Tab Header 进入 overflow menu 后丢失模板的问题，保持 `TabControl.HeaderTemplate` 和 `CardTabStrip.ItemTemplate` 渲染一致。#430
- 数据录入和 DataGrid
  - Select 将 pointer hover 和键盘导航统一到同一个 active candidate，使 Single、Multiple 和 Tags 模式下的高亮项与 Enter 提交目标保持一致。
  - 修复非编辑 ComboBox 重新打开后键盘焦点丢失的问题，使连续使用 Down 和 Enter 仍能更新选中项。#428
  - 修复 `ItemsSource` 为空时 DataGrid Star 列宽未按有限 Header viewport 正确分配的问题。
- Gallery
  - 为桌面和浏览器 Gallery Shell 新增响应式内容断点、Metadata 布局和侧边栏折叠行为。
  - Icon Gallery 新增实时筛选和一键复制图标名称，并提供本地化成功或失败反馈。

## 6.1.3

`2026-08-08`

- 破坏性变更
  - Theme：自定义主题和控件包需要从手工维护 Token/Theme 注册迁移到生成式控件包 descriptor 和 `Themes/**/*.axaml` 资源清单，并在主题 schema 冻结前通过 `Application.UseAtomUI(...)` 完成注册。主题 builder、attribute、descriptor 和状态中的字符串算法 ID 需要改为 `ThemeAlgorithm` 枚举值。
  - Localization：已移除 `LanguageCode`、`LanguageVariant`、语言 Provider/Pool 和 ThemeManager 语言注册 API，请迁移到 `LanguageTag`、Catalog enum、XLIFF 2.1 资源、`UseLanguages()` 和生成式模块注册。Catalog enum 成员名和 XLIFF unit ID 现在是稳定契约 identity；应用与静态语言包必须一起重新构建，不能混用原数字 ID Catalog 与新 Key Catalog。
  - Upload：移除 `Upload.IsOpenFileDialogOnClick`，改用 `UploadDropZone.IsOpenFileDialogOnClick` 和 `SourceKind`；以 `AllowedFileTypes` 替代 `Upload.Accepts`；移除旧 drop 事件和仅 URI 文件契约；自定义集成需迁移到强类型 source、input batch、admission 和 completion 契约，并以 `Files` 作为唯一状态所有者。
  - Timeline：新增水平 `Orientation`，以逻辑方向 `Start` / `End` 替代 `TimelineMode.Left` / `Right`，并将 `IndicatorLeftModeMargin` / `IndicatorRightModeMargin` 重命名为 `IndicatorStartModeMargin` / `IndicatorEndModeMargin`。
  - Tag：以 `TagVariant`（`Filled`、`Solid` 或 `Outlined`）替代 `IsBordered`；可选择标签场景请使用新的 `CheckableTag` / `CheckableTagGroup` 选择 API。
  - Separator：`SizeType` 从 `SizeType` 改为 `CustomizableSizeType`，并实现 `ICustomizableSizeTypeAware`；`Custom` 间距由控件实例或其所在样式负责。迁移写法见 [6.1.3 API 变更示例](docs/releases/6.1.3-api-changes.zh-CN.md)。
- Calendar 和 DatePicker
  - 新增 Calendar 控件，支持日期、月份和年份视图、Mini/Fullscreen 密度、自定义 Header 和 Cell、周数显示、日程通知、范围条、禁用日期组合、键盘导航及自动化访问。
  - DatePicker 新增可切换 picker 模式以及 `MinDate` / `MaxDate` 范围约束。
- Localization、Generator 和 Build
  - 以生成式 XLIFF 2.1 Catalog、强类型资源扩展、应用 bootstrap 和不可变语言快照替代运行时语言 Provider，同时保留现有 `{atom:XxxLangResource Key}` XAML 用法。
  - 新增静态语言包构建契约、active/dormant 模块生成式注册、verified/deferred 包 manifest、局部覆盖和 Catalog 校验；注册流程不依赖运行时程序集扫描或 XLIFF 解析，并保持 NativeAOT 兼容。
  - 为 AtomUI 各模块新增官方葡萄牙语（巴西）`pt-BR` 语言包，并在 Gallery 中支持语言切换。
- Theme
  - 将主题定义和生成式控件主题 manifest 编译为完整、不可变的根级与局部快照，原子发布主题变化，并按规范化内容 digest 复用等价主题缓存。
  - 对齐 Ant Design Token 语义、完整控件 Token 快照和组合输入控件状态，包括生成式 exact control identity、资源 key 以及 Effective Global/Own Token 访问。
- Upload
  - 将文件选择、目录选择、拖放和程序化输入统一到跨平台准入管线，使用强类型文件 source 所有权、串行批次和确定性终态。
  - 新增目录策略、遍历限制、数量溢出策略和快照错误报告，并统一 picker 与 drop 输入的 `IsMultipleEnabled` 行为。
  - 加固替换、取消、清理和上传进度收尾；已接收 source 会保留到执行退出，任务进入终态后不再更新进度。
- 导航和选择控件
  - Menu 新增 `IsScrollEnabled`，支持可滚动的弹出菜单。
  - NavMenu 新增层级条目组合、可折叠侧边栏 Header，改进内联折叠显示，并在 Items 替换时清理过期选择。
  - Segmented 新增垂直和圆角变体、键盘导航及动态选项加载。
  - OptionButtonGroup 新增垂直布局；Timeline 新增水平布局和逻辑 Start/End/Alternate 放置模式。
- 数据录入、数据展示和通用控件
  - Tag 新增 Filled/Solid/Outlined 变体、预设/状态/自定义颜色处理、CheckableTag 以及支持单选和多选的 CheckableTagGroup。
  - Slider 新增多 Handle 值和单个 Handle 禁用状态。
  - Button 新增 `IconWidth` 和 `IconHeight`；Steps 新增 `ItemHeaderForeground`、`ItemSubHeaderForeground` 和 `ItemRailBackground` 语义样式属性。
  - TextBox 和 TextArea 使用文本 viewport metrics 改进 OverflowTip 判断，恢复 Separator 预设间距，并新增 Gallery Drawer 表单示例。
  - 新增 LunarCalendar，支持农历日期换算、节气、传统节日、节假日/调休标记和可配置的周末高亮。
- Window、DataGrid、Gallery 和依赖
  - 新增 Window 标题栏 add-on facade，并修复 ImagePreviewer 从客户端装饰 overlay 路由工具栏请求的行为。
  - 修复 DataGrid 行重排重置崩溃、集合移动和重排生命周期问题，并减少重排路径中的内存分配。
  - 补充打包版本 Gallery Developer Tools 所需的运行时依赖。
  - Avalonia 从 `12.1.0` 升级到 `12.1.1`；该补丁依赖升级不要求 AtomUI 源码迁移。

## 6.1.2

`2026-07-27`

- Theme 和 Gallery
  - 新增通用 `ThemePreference` 枚举，支持 `Light`、`Dark` 和 `System`。
  - Gallery 外观菜单新增浅色、深色和跟随系统模式；跟随系统时会随操作系统浅深色变化自动切换。
- Dialog、Drawer 和 Window
  - 修复 overlay Dialog 和 Drawer 的 mask 没有完整覆盖绘制窗口 chrome，或因此改变窗口内容几何的问题。
- Splitter
  - 修复交叉轴无限约束时测量结果不为有限尺寸的问题。

## 6.1.1

`2026-07-25`

- 破坏性变更
  - ListView：将选择状态从可替换的 Avalonia `ISelectionModel` 调整为只读 `IListViewSelection` 门面。直接设置 `Selection`、`SelectedItem`、`SelectedItems` 或 `SelectedValue` 的代码，需要迁移到 `SelectedIndex` 或 `Selection.Select` / `Selection.Deselect` / `Selection.Clear` / `Selection.SelectAll`；`SelectionChanged` 事件参数改为 `ListViewSelectionChangedEventArgs`，并移除旧的 `ListCollectionViewChangedEventArgs` 类型。`ListItemData.IsSelected` 已移除，`ListItemData`、`GroupListItemData` 和 `SelectOption` 从 record 调整为 class。迁移示例见 [6.1.1 API 变更示例](docs/releases/6.1.1-api-changes.zh-CN.md)。
- Window 和 Dialog
  - 修复 macOS modal dialog window 的标题栏和 resize 区域行为，避免窗口内外出现重复 resizer，并稳定点击 owner 外区域时的 modal 闪动。
  - 修复 Wayland dialog/window resize 过程中尺寸约束丢失、窗口突然变大、fractional scale 边框和 mask 错位的问题。
  - 修复 Windows CSD dialog/window resize、背景预热、frame dark mode 和最小高度约束，避免原生边界与绘制装饰不同步。
  - 统一 Window 平台 chrome 管理边界，保持 Windows、macOS、Wayland、X11 和其他 Linux 装饰逻辑分离。
- WindowTitleBar
  - 新增 `WindowTitleBar.TitleAlignment` 和 `Window.TitleAlignment`，支持 `Auto`、`Left`、`Center`、`WindowCenter`、`Right` 标题对齐模式。
  - 修复 Windows/Linux 下 logo 与 left add-on 的排列顺序、Windows caption button 光标和标题栏 padding。
- ListView、Select 和 Transfer
  - 修复过滤、分组、分页和重复数据项场景下选择状态与数据视图索引错位的问题。
  - 新增 `ItemKeySelector` 和 `SelectedIndexes`，让 ListView 可以按稳定业务 key 和 source index 维护选择状态。
- DataGrid
  - 修复 DataGrid template 重新应用后分页器没有回放当前页、页大小和总数的问题。
- ImagePreviewer
  - 修复预览 dialog 打开时的 resize motion、标题居中和背景稳定性问题。
- Drawer、ScrollViewer 和基础布局
  - 修复 Drawer mask 没有覆盖可见窗口 frame 的问题。
  - 修复 fractional scale 下子像素溢出导致 auto scrollbar 错误显示的问题。
  - 修复 SplitButton arrange 过程中子元素 bounds 不稳定的问题。
- Gallery、文档和构建
  - 移除 Gallery 中内置 API 和 Design Token metadata table sidecar，LLMS 文档不再引用这些 Gallery 表格。
  - 将可复现的 generated files 从仓库跟踪中移除，并修复生成 theme asset manifest 时的 CRLF 输出问题。

## 6.1.0

`2026-07-20`

- 破坏性变更
  - Dialog 和 MessageBox：围绕异步 Session 重建静态展示和宿主生命周期。直接引用 `IDialogHost`、`IDialogHostProvider`、`IDialogActionResult` 或 `IMessageBoxActionResult`，或依赖 callback 式宿主关闭 API 的自定义代码，应迁移到 `ShowDialogAsync`、`ShowDialogModalAsync`、`ShowMessageBoxAsync`、`ShowMessageBoxModalAsync`、`Dialog.OpenAsync`、`Dialog.BeforeCloseAsync` 和返回的 `Task<object?>`。迁移示例见 [6.1.0 API 变更示例](docs/releases/6.1.0-api-changes.zh-CN.md)。
  - Steps：将基于 Selection 的契约替换为受控步骤状态。请将 `CurrentStep` 迁移到 `Current`，`InitialStep` 迁移到 `Initial`，`CurrentStepStatus` / `StepsItemStatus` 迁移到 `Status` / `StepsStatus`，`Style` / `ItemIndicatorType` 迁移到 `Type`，`LabelPlacement` 迁移到 `TitlePlacement`，`ProgressValue` / `IsShowItemProgress` 迁移到可空 `Percent`；步骤项描述从 `Description` / `DescriptionTemplate` 迁移到 `Content` / `ContentTemplate`，可点击步骤变更改为处理 `CurrentChangeRequested`，不再依赖 Selection 直接变更。迁移示例见 [6.1.0 API 变更示例](docs/releases/6.1.0-api-changes.zh-CN.md)。
- Theme
  - 新增基于 resolver 的主题定义加载能力，支持内置资源、应用资源和显式启用的用户主题目录，并统一使用 XML 主题定义格式。
  - 新增 `IThemeDefinitionResolver`、`IThemeManager.AvailableThemes`、`IThemeManager.CurrentTheme`、`ThemeCatalogDiagnostics`、`ThemeCatalogChanged` 和 `ReloadThemesAsync`，应用可以发现、切换并原子刷新主题 Catalog。
  - 新增 `WithApplicationId`、`UseUserThemeDirectory()` 和 `UseUserThemeDirectory(string directory)`；显式启用时，默认用户主题目录为应用数据目录下的 `{ApplicationId}/Themes`。
  - 重建主题解析、绑定、Catalog 编译、Snapshot 缓存和 Token 资源发布流程，让根主题和局部主题都发布完整 Snapshot，并在解析或编译失败时保留上一版状态。
  - 生成 AXAML 共享控件 Token 资源元数据，并隔离控件 Token 作用域，使 primary、link 和 text button 状态随当前主题稳定变化。
- Gallery 主题
  - 新增 Daybreak Blue、Polar Green、Sunset Orange、Golden Purple 和 Magenta 等内置主题定义，并通过带色块的主题设置子菜单暴露。
  - Gallery 通过 Theme manager 完成主题切换，并为桌面 Gallery 启用用户主题目录加载。
- Window 和 Native
  - 将 Avalonia 依赖升级到 `12.1.0`，并新增原生 Wayland 支持。
  - 稳定 Windows 和 Linux 绘制装饰，包括 Windows 10 CSD frame 渲染、深色主题标题栏、caption 按钮、实时 resize、Wayland 圆角 CSD 裁剪和 Linux client-frame 视觉层。
  - 为 Dialog 和 Drawer overlay 分离完整窗口 mask、可见 frame bounds 和阴影范围，让 mask 覆盖绘制标题栏，同时 placement 保持在可见 frame 内。
- Dialog、Drawer 和 MessageBox
  - 将 Dialog 和 MessageBox 统一到同一套 Session、Presenter 和 Surface 生命周期，覆盖 UI Dispatcher 构造、异步打开/关闭、关闭拦截、owner close 和确定性 teardown。
  - 优化 overlay 与 window presenter 的尺寸、拖动、resize、最大化、mask 路由、焦点恢复、取消和重入行为。
  - 降低 overlay dialog transition 开销。
- 导航和选择控件
  - Steps：新增 `OutlineDot`、受控导航请求事件、确定性的步骤编号/status/connector 语义、pointer click wave 行为，以及用于水平、垂直和导航布局的新 layout panel。
  - Cascader：支持键盘候选导航，并稳定展开、过滤、选中 option 同步和 popup 状态。
  - Menu 和 NavMenu：新增子菜单 hover intent，支持 detached title bar 中的 popup，新增生命周期安全的节点命令，并修复 pointer 或布局变化导致的子菜单状态跳变。
  - Collapse 和 Expander：优化 separator、accordion、内容可见性和 motion cleanup 行为。
- 数据录入、数据显示和通用控件
  - 为 CascaderView、ListView 和 ListBox 新增自定义 EmptyIndicator 支持。
  - 为 TransferItemDecorator 新增 list 和 tree transfer 视图的 item count 处理。
  - 修复 AddOnDecoratedBox 仅模板提供 add-on 内容、模板拥有的 EmbeddedTextBox 输入框、embedded 输入 padding、DatePicker 和 TimePicker 输入尺寸，以及 NumericUpDown 默认输入 padding 覆盖。
  - 移除 ColorPicker spectrum 颜色名称 tooltip，并稳定 ImagePreviewer 搜索和主题资源。
  - 修复 Row/Splitter 布局恢复和精确 grid line 换行问题。
- 反馈和浮层控件
  - Notification：普通通知默认不显示 type icon，对齐关闭按钮 hover/pressed 状态，使用 primary 色生成进度条渐变，将默认自动关闭时长设为 4.5 秒，并缩小内部卡片间距。
  - Tour：修正目标 placement，并为高亮区域添加动画。
- Icons、Gallery、Generator 和文档
  - 新增面向 AI、应用和社交/服务场景的 SVG 图标。
  - 为 Gallery 新增 Windows 渲染 fallback，并刷新 Gallery baseline、生成文档和主题设计文档。
  - 新增 XML 主题绑定、Token 发布和可选控件包使用的生成式主题 schema 与控件 Token 资源元数据。
  - 将包元数据更新到 `6.1.0`。

## 6.0.8

`2026-07-10`

- 破坏性变更
  - ImagePreviewer：将 `SourceUri`、`SourceUris`、`FallbackSourceUri` 统一替换为基于 `IImagePreviewSource` 的 `Source`、`Sources`、`FallbackSource`；URI、本地文件和 Avalonia 资源请使用 `UriImagePreviewSource`，按需数据流请使用 `StreamImagePreviewSource`。迁移示例见 [6.0.8 API 变更示例](docs/releases/6.0.8-api-changes.zh-CN.md)。
  - Upload：移除旧的 `IsUploadDirectoryEnabled`、`IsShowUploadTrigger` 和 `DefaultTaskList` 组合方式，改为由 `UploadTrigger`、`UploadDropZone` 和 `Files` 组成可组合上传入口。迁移示例见 [6.0.8 API 变更示例](docs/releases/6.0.8-api-changes.zh-CN.md)。
- 数据录入和选择控件
  - 新增 OtpLineEdit 一次性验证码输入控件，支持 `Text` 双向绑定、`Length`、`InputMode`、`Formatter`、遮罩、分隔符、`Completed` 事件、四种 `StyleVariant`、Form 和 `DataValidationErrors` 集成。
  - 将 Form 验证统一接入 Avalonia `DataValidationErrors`，并新增 `ValidateTrigger`；默认改为值变更时验证，也可配置为失焦触发。
  - 为 Select、Cascader、TreeSelect、ListView、CheckBoxGroup、RadioButtonGroup、Rate、DatePicker、TimePicker、Transfer、ColorPicker、Slider、Dialog、Tour、ImagePreviewer 等控件补齐或调整默认双向绑定与数据验证语义。
  - 为 Select、ComboBox 和 Cascader 新增溢出内容提示能力，支持 `IsShowOverflowTip`、`OverflowTipDelay` 和 `OverflowTipPlacement`。
  - 为 DatePicker 和 TimePicker 新增弹层展示锚点属性，并补充范围选择绑定示例。
- Upload
  - 重构 Upload 为文件状态协调器，新增 `UploadFileItem`、`UploadTrigger`、`UploadDropZone`、`UploadFileValueMode`、`AutoUpload`、`ListMaxHeight`、`ListScrollBarVisibility`、`SuccessAutoRemoveDelay`、`PendingText` 和 `TriggerContent` 等能力。
  - 优化文件选择、目录选择、拖拽上传、上传列表滚动、图片列表预览、删除状态和成功自动移除行为。
- ImagePreviewer
  - 新增基于 `IImagePreviewSource` 的 URI 与懒加载数据流图片源模型，支持 `CoverIndex`、`MaxConcurrentLoads` 和 `PreloadCount`。
  - 修复多图加载中单张失败会过早切换 fallback 的问题；现在只有整组图片全部失败时才使用 `FallbackSource`。
  - 优化多图预览标题、封面加载、预览窗口导航图标和 20 张远程图片示例。
- DataGrid
  - 新增列过滤数据模型，支持 `Filters`、`SelectedFilterValues`、`FilterTextMemberPath`、`FilterValueMemberPath`、`FilterChildrenMemberPath`、`FilterPresenterMode`、`FilterSelectionMode` 和 `FilterApplyMode`。
  - 优化过滤选择、树形过滤、筛选值绑定和 Gallery 示例。
- TabControl 和 TabStrip
  - 新增 Tab 拖动排序能力，支持 `IsTabReorderEnabled`、`TabActivationTrigger`、`TabReordering` 和 `TabReordered`。
  - 优化 Chrome 风格拖动预览、滚动时锚点保持、选中指示条同步、左右 placement 下无图标 Tab 的布局和默认 Line Tab 垂直紧凑间距。
- TreeView、Cascader 和 NavMenu
  - 修复 TreeView `ItemsSource` 拖拽移动崩溃，并改进拖拽、选中项双向绑定、表单值和 descendants bring-into-view 行为。
  - 修复 Cascader 选择同步和展开状态崩溃问题，并补齐 `SelectedOption` / `SelectedOptions` 双向绑定。
  - 新增 NavMenu popup frame，改进弹出层宿主、定位和关闭行为。
- Dialog、FloatButton 和基础视觉
  - 为 Dialog 新增 `BeforeCloseAsync`，支持异步关闭校验，并统一关闭请求处理。
  - 为 FloatButton 新增命令支持，并优化滚动容器场景下的浮动层定位和 controlled `IsOpen` 行为。
  - 优化 Avatar 图片裁剪、控件边框按布局缩放渲染、Button 边框渲染和 Space 预设间距。
- Gallery、文档和构建
  - 扁平化标准 Showcase 示例结构，补充 Tab 拖动、Upload、ImagePreviewer、OtpLineEdit、选择绑定和范围选择示例。
  - 更新控件设计文档、Window 标题栏定制说明、Feature Request issue 规范、README 包版本和 Gallery banner。
  - 清理多处未使用 localization using，移除 Labs 包并更新工程文档。

## 6.0.7

`2026-07-03`

- DatePicker
  - 新增日期、周、月、季度和年份等选择模式，并优化范围面板行为和输入框首选宽度计算。
  - 优化范围选择状态、hover 渲染和 CalendarView 生命周期处理。
- DataGrid
  - 优化行详情高度估算和展开行虚拟化稳定性。
  - 修复空或 null `ItemsSource` 场景下点击表头/标题按钮导致崩溃的问题。
  - 优化选择列初始状态处理，以及带边框表格的外框圆角行为。
  - 修复 DataGrid 模板单元格中的 AutoComplete 焦点保持问题。
- Select
  - 修复 Tags 模式动态选项创建会修改用户 `OptionsSource` 的问题。
  - 优化 Tags 键盘交互：Enter 提交当前动态候选项，Escape 关闭弹层，Up/Down 可从搜索输入框导航候选项。
  - 修复多选和 Tags 在 IME 预编辑文本渲染期间 placeholder 显示不正确的问题。
- ImagePreviewer
  - 新增通过 `ImageSourceUri` 加载本地和远程图片源的能力。
  - 新增预览窗口标题解析、`PreviewTitleIcon`、加载 skeleton 和本地化错误状态。
- Gallery 和文档
  - 新增可复用的 Gallery 源码展示组件、代码查看器懒加载、可复制代码选区和生成式代码片段目录支持。
  - 新增 Gallery API 表格和 Design Token 表格文本可选中能力。
  - 新增 LLMS 文档生成，并刷新控件文档覆盖范围。
  - 新增组织介绍文档页面，并将 README 包示例更新到 `6.0.7`。
- Splash 和 Window
  - 新增 Splash 控件和服务，支持 owner window、Gallery 版本标签和更稳定的显示逻辑。
  - 新增 Windows 窗口 chrome 管理，支持 DWM 阴影处理并优化 resize 伪影。
- 控件
  - 新增 Extras 和 Labs 补充控件模块。
  - 新增 Splitter 分隔线样式 API 和 Gallery 示例覆盖。
  - 优化 NumberUpDown handle 定制和模式行为。
  - 优化 Button 阴影渲染、SplitButton resize 布局稳定性、Drawer 右键行为、Message 和 Notification 反馈层级、弹出层阴影以及 Separator 紧凑间距。
  - 优化 Form validator 兼容性，并新增 `ExtraExtraExtraLarge` 断点布局处理。
- Localization 和 Build
  - 更新语言 provider 生成方式，并为 GalleryBase 新增中文语言支持。
  - 新增 AppImage launcher 验证支持，并更新 Gallery 和 NuGet 包发布工作流。

## 6.0.6

`2026-06-24`

- 兼容性说明
  - 多个尺寸感知控件现在使用可定制尺寸模型，除内置尺寸类型外也支持 `Custom` 尺寸行为。直接依赖旧尺寸类型契约的项目应验证源码兼容性。
- 新增
  - 为 Button 系列、DropdownButton、NumericUpDown、Mentions、Select、TreeSelect、ToggleSwitch、SpinIndicator 以及相关尺寸感知控件新增可定制尺寸支持。
  - 新增 Button 图标位置配置能力。
  - 新增 Slider 桌面控件、文档和 Gallery 示例。
  - 新增 `Col.Flex` 支持，并优化 Row/Grid 响应式布局行为。
  - 新增 ComboBox 可编辑过滤、候选项键盘导航、Enter/Escape 行为和空结果反馈。
  - 新增 NavMenu 键盘导航、激活项反馈和内联折叠模式。
  - 新增初始主题算法配置，让应用首帧即可按深色主题渲染。
  - 新增 TreeView/Cascader 可绑定节点或选项支持，覆盖面向绑定的数据场景。
  - 新增可复用的 GalleryBase toolkit 包，并集成 Gallery shell 和路由基础能力。
- 变更
  - 优化 Button、SplitButton、LineEdit、SearchEdit、DatePicker、TimePicker、ColorPicker、Select、TreeSelect 等控件的自定义尺寸布局和 Gallery 示例。
  - 优化 Form 校验流程，避免提交型校验过早显示错误。
  - 优化 Collapse 手风琴状态处理、padding 计算和内容可见性行为。
  - 优化 Expander 指示器间距、内容可见性和布局行为。
  - 优化 Descriptions、Segmented、Steps、ProgressBar、Skeleton、Card、AvatarGroup、Transfer、Upload、Pagination 和 Breadcrumb 的实现结构和正确性。
  - 优化 SpinIndicator 动画处理、默认对齐和可定制尺寸渲染。
  - 优化 RibbonBadge 定位和 adorner 可见性行为。
  - 优化 CompactSpace 内部协作和布局处理。
  - 将 Avalonia 依赖从 `12.0.4` 升级到 `12.0.5`。
- 修复
  - 修复 DatePicker 和 TimePicker 输入框首选宽度计算问题。
  - 修复 ListBox/ListView 过滤项状态处理问题。
  - 修复 NavMenu 在内联折叠模式下的选择保持问题。
  - 修复 Gallery AppImage installer 资源路径问题。
  - 修复 Gallery macOS DMG 工作流，避免使用未使用的 Homebrew taps。
  - 修复 Breadcrumb 生成项状态清理和 `IBreadcrumbItemData.Content` 映射问题。
- 文档
  - 新增或补全文档覆盖：LineEdit、SearchEdit、ListView、NumericUpDown、Select、Slider、ToggleSwitch、TreeSelect、Descriptions、Expander、Segmented、ProgressBar、Card、Cascader、Form 等桌面控件。
  - 新增控件优化 skill 文档，包含 API 布局、文件拆分、生命周期和根因优化规则。
  - 更新 Gallery 示例，覆盖自定义尺寸、键盘导航和新文档控件场景。

## 6.0.5

`2026-06-19`

- 破坏性变更
  - 响应式布局 API 统一到 `AtomUI.Controls.Shared.MediaQuery`。
  - 新增共享的 `ResponsiveInt`、`ResponsiveDouble`、`ResponsiveGutter` 和 `ResponsiveValueMap` 模型。
  - 新增 `xxxl` 断点和对应的 `ScreenXXXL` token 支持。
  - Grid、Descriptions 和 Masonry 现在共享同一套 mobile-first 响应式解析和 fallback 规则。
  - 依赖旧控件级响应式解析的自定义代码，尤其是依赖 `DescriptionsMediaBreakInfo` 的代码，应迁移到共享响应式值类型。
- Gallery
  - 重构 Gallery shell 和 showcase 页面结构。
  - 新增 sticky showcase tabs、延迟加载 showcase、场景诊断和快照归一化。
  - 为多数已迁移 showcase 页面新增本地化 API 表格和 Design Token 表格。
  - 对齐 Browser Gallery 路由和导航到共享桌面 Gallery 组件。
  - 新增 Gallery 版本展示和 Community Telegram 区域。
- 新控件和布局
  - 新增 BorderBeam，包含几何、颜色 stop、token、Gallery、文档和测试。
  - 新增 Masonry，支持响应式列、gutter、加载 skeleton、图片 demo、Gallery 示例和布局测试。
  - 为 Grid、Descriptions 和 Masonry 新增共享响应式布局支持。
- 桌面控件
  - Button：新增 `Color`、`Variant` 和 `CustomBackground` 支持，包括渐变背景以及更新后的 Gallery/API 文档。
  - NumericUpDown：新增 inline spinner 模式并优化 handle 样式。
  - ButtonSpinner：优化禁用状态处理，并新增样式 variant 支持。
  - NavMenu：新增 `ClearSelection`、`IsItemBackgroundEnabled`，并优化选择协作、内联间距和背景行为。
  - TextArea：优化 resize 行为。
  - GroupBox：修复背景透明时边框不可见的问题。
  - Pagination：优化 page size 校验和自定义 page size 选项。
  - Dialog 和 Menu：防止 dialog close/open 流程重入，并优化 menu 关闭行为。
  - HyperLinkButton：让图标颜色跟随前景色。
- NativeAOT、诊断和 DataGrid
  - 强化 NativeAOT 场景下的生成式数据成员访问器。
  - 新增 AOT 数据成员路径 analyzer 和编译器诊断规范。
  - 修复 DataGrid 排序路径解析，并新增排序回归测试。
  - 修复 Gallery Design Token 表格空状态。
  - 为非 Visual `AvaloniaObject` 使用场景实现 scoped resource 和 theme host，避免动态资源滞留泄漏。
- Native Window 和平台稳定性
  - 优化 Linux 窗口初始化、缩放、frame extents、window chrome 管理、弹出层支持和标题栏菜单关闭行为。
  - 优化 macOS 标题栏 logo 可见性。
  - 优化 Windows Win32 composition 选项以提升性能和稳定性。
  - 修正 popup、dialog、title bar 和 Gallery 文本问题。
- Build 和 Release
  - 恢复桌面诊断，并稳定生成源码的换行。
  - 优化 NativeAOT Gallery 发布工作流和验证。
  - 更新 `PublishToLocal.ps1` 版本提取逻辑，改为使用 `AtomUIVersion`。
  - 更新 AtomUI 6.0 发布定位相关包元数据。
