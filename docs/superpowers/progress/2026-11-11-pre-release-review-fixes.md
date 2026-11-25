# 发版前评审问题修复记录

2026-11-11，分支 `bugfix/6.2.14-pre-release-review`（基于 `release/6.0` 及当时未提交的工作区）。逐项处理
[发版前改动评审记录](2026-11-11-pre-release-review.md) 中的缺陷：先评估，再用临时用例复现，修复后转绿。临时用例已全部删除，
永久测试净新增 0。

## 已修复

| 问题 | 结论 | 修复 | 证据 |
| --- | --- | --- | --- |
| ProgressBar 换模板重复挂载内容 | 复现 | `OnApplyTemplate` 先释放旧呈现器的内容与模板 | 修复前 `InvalidOperationException`，修复后新呈现器为唯一视觉父级 |
| ProgressBar 清除旧模板可见性值 | 确认 | 以可释放的 Animation 优先级隐藏标签与图标，清除内容或换模板时释放；共享层保留代码隐藏（Mobile 模板不依赖 Desktop 主题） | 自定义模板的本地 `IsVisible=false` 与绑定在套模板、设清内容后保留；内置 labelinner 竖排隐藏正确。首版改为主题伪类的方案使共享层 `ProgressInfoTests` 4 项失败，已放弃 |
| ProgressBar 自定义信息区尺寸过期 | 复现（线形、Steps、Circle） | internal `ProgressInfoPresenter` 在期望尺寸变化时使宿主失效；线形/Steps 测量时刷新信息区 | 修复前信息区停在旧宽度 20，修复后 50 且轨道不重叠，Circle 重新居中 |
| CountBadge 初始隐藏漏挂目标 | 复现；DotBadge 同构 | 目标挂载/替换独立于指示器可见性 | 两种初始隐藏路径与 DotBadge A/B：去掉修复即失败 |
| TreeView 扩展选区发出中间空值 | 复现 | 只同步差量，`AvaloniaList` 批量增删 | 事件序列由清空+逐项变为 `(1, 0)` / `(0, 1)` |
| MarqueeLabel 悬停时无法恢复 | 复现（3 个场景） | 启动统一检查溢出、挂树、门控与悬停暂停 | 恢复、重排保持暂停、延时保持暂停均转绿 |
| AsyncImage 自适应布局停在 Idle | 复现 | 自动尺寸零轴按位置判定；尺寸变化重判；检查有效可见性；`IsLazy`+`Idle` 显示占位 | 只设宽度的懒加载图片加载；占位撑开 24 高；显式零尺寸仍不加载 |
| Cover 解码欠采样 | 真实 Skia 复现 32×15 | 按单轴解码前检查 Skia 截断后的另一轴，不足则换轴 | Skia 探针 `DecodeToHeight(16)` 得 32×16；轴选择表驱动用例 |
| Avatar 累积属性帧 | 复现 | 每个派生值持有一个可替换句柄，条件退出时释放；避免首次 Size 重复配置 | 50 次改尺寸后帧数不再增长 |
| TreeSelect 截获用户输入框按键 | 复现 | 隧道转发只接收自身 `SelectFilterTextBox` | 附加区输入框 Enter 不再提交；过滤输入仍可导航提交 |
| Skeleton 微光覆盖自定义边框 | 复现 | 微光块按 `BorderThickness` 收缩到内缘 | 块从 (0,0,200,16) 变为 (2,2,196,12) |
| Empty 新入口缺少 Desktop 接线 | 确认 `Image` 设后空白 | Desktop 模板渲染 `Image`（对齐 antd `Empty.image`），同高容器，隐藏 SVG | 设置 `Image` 后可见，清除后 SVG 恢复 |
| Watermark 共享字形重入 | 复现订阅泄漏 | owner/前景版本校验，外层不覆盖内层句柄 | A/B：去掉修复后离树仍跟随主题 |
| Watermark 离树前景约定 | 文档与实现不一致 | 保留实现（离树保留最后主题色），修正文档 | — |
| Spin 自定义动画重入 | 复现 start,start | 启动期间的重启/停止延后重放 | 启停保持配对 |
| Card 清空模板保留 LoadingHost | 复现 | 以 `TemplatedParent` 判断 host 是否仍在模板内 | 清空模板后不再向旧 host 加指示器 |
| Space 首项超宽产生空行 | 复现（水平、垂直） | 只断开已有项的行 | A/B：去掉修复后首项被推下 |
| Dialog 右键激活与焦点分离 | 复现 | 抬升后焦点仍在该 Dialog 外时交给其 Surface；保留“Escape 由最顶层处理”的既有契约 | A/B：去掉修复后焦点停在 B，Escape 作用于 A |

## 评估后未修改

- **TreeView 选中背景遮住活动项提示**：属于新视觉设计；NavMenu 采用同样的“选中时不显示活动态”模式，需要跨控件统一决策并做真实窗口视觉验收。
- **ComboBox 清除动作缺少键盘入口**：Ant Design 的清除图标同样不可聚焦，属于功能增强而非回归。
- **ComboBox 显式 `VirtualizingStackPanel` 绕过离场保护**：按源码确认存在。Avalonia 的自动定位为私有逻辑，控件级只能临时改写公开的 `AutoScrollToSelectedItem`，会干扰调用方绑定；记为已知边界。
- **Spin/MarqueeLabel 全局运动 Token**：Ant Design 的旋转动画不依赖 motion Token，现状一致，无需修改，已写入 Spin 变更记录。
- **Empty 相同值预设写入推迟冲突校验**：已记录的边界，维持。
- **Empty 对象描述的样式入口**：需要新增 Semantic Part 契约，不在缺陷修复范围。
- **兼容性变化**（Skeleton 基类迁移、`SpaceItemsAlignment` 类型身份、`Empty.Description`/`Result.Icon` 签名）：属于发版兼容策略决策，未改动。
- **性能、测试与文档改进项**：不属于缺陷，未处理。

## 复核“评估后未修改”各项

逐条对照源码与 Ant Design 6.7.0 重新判断。结论分三类：是真问题，已修复；原判断成立，维持；需要你决策，未改动。

| 项 | 复核结论 | 处理 |
| --- | --- | --- |
| TreeView 选中背景遮住活动项提示 | 已由 `e42303547` 的活动项焦点描边解决：描边画在内容框外侧，与选中背景并存 | 补测试：选中行在 Tab 移入、Ctrl+方向键、Tab 移出时的描边；NavMenu 选中且活动的项同样有描边 |
| ComboBox 清除缺少键盘入口 | 原判断不成立：rc-select 的清除元素是可 Tab 到达的 `<button type="button">`，带本地化 `aria-label`（`locale.global.clear`），Enter / Space 清除后焦点回到输入；antd 6.7.0 样式里也有清除按钮的 `focus-visible` 描边 | 已修复，见下 |
| ComboBox 显式 `VirtualizingStackPanel` 绕过离场保护 | 限制属实，但没有干净的修法：`ItemsControl.ScrollIntoView` 非虚、`ItemsPresenter.ScrollIntoView` 为 internal；临时覆盖 `AutoScrollToSelectedItem` 时，Avalonia 双向绑定会把每次有效值变化写回数据源；在面板之上拦截 `RequestBringIntoView` 也不够，面板在此之前已经围绕目标项重新布局 | 维持“已知边界”，已写入 ComboBox 实现文档 |
| Spin / MarqueeLabel 全局运动 Token | 原判断成立：antd 的 Spin 旋转时长固定（1.2s / 1s），`motion: false` 只把各种过渡时长清零；桌面端独立与内嵌的 SpinIndicator 都不绑定 `EnableMotion`，表现一致 | 维持 |
| Empty 相同值预设推迟冲突校验 | 已记录的边界：只有在把派生默认值改写成相同的本地值时才会推迟，Avalonia 对“值不变、优先级变化”不发通知 | 维持 |
| Empty 对象描述的样式入口 | 缺口属实：antd 的 `styles.description` 作用于外层容器，字符串和节点都生效 | 用户决定对齐 Ant Design：`description` 部件改为同时承载文字与自定义内容的 `ContentPresenter`，作为本版的主题契约变化写入迁移指南 |
| 兼容性变化（Skeleton 基类、`SpaceItemsAlignment`、`Empty.Description` / `Result.Icon`） | 属于发版兼容策略 | 用户选择保留并作为破坏性变更发布：新增 [6.2.14 迁移指南](../../releases/6.2.14-api-changes.zh-CN.md) 与英文版，登记到 `docs/releases/overview.md`，各控件 changelog 链接迁移指南 |
| ComboBox 关闭时重复过滤 | 属实：`HandlePopupClosed` 每次关闭都对全部条目调用一次过滤器 | 已修复：只在离场期间有被推迟的刷新、或过滤条件与已呈现的不同时才刷新 |
| Space 默认排布多一次遍历 | 只多一次线性计数，影响可以忽略 | 维持 |
| Watermark 每帧重复计算字形尺寸 | `TextGlyph.GetDesiredSize()` 只读取已缓存的 `FormattedText` 宽高，开销可以忽略 | 维持 |
| ComboBox 清除语义部件 `Since="6.2.13"` | 属实：`v6.2.13` 标签中还没有该部件。但仓库 `AtomUIVersion` 仍为 6.2.13，下一版版本号未定 | 定版时同步 |
| 下游预留入口、新增测试价值、验收宿主规则说明 | 需要确认下游使用方，或属于文档与测试整理 | 未处理 |

### ComboBox 清除与键盘停靠点修复

- **清除按钮**：模板去掉 `Focusable="False"`，成为停靠点；Enter / Space 清除后，按触发方式把焦点还给宿主或文本框。
- **基类焦点转交**：`OnGotFocus` 在焦点落到清除按钮时不调用基类，因为 Avalonia 可编辑 ComboBox 会把控件内的任何焦点转给文本框。
- **Tab 顺序**：清除按钮在外框模板中排在文本框之前，所以外框设为 `TabNavigation=Local`、本身不作停靠点的分组，文本框与清除按钮分别为 `TabIndex` 0、1；可编辑模式下宿主不作停靠点。
- **自动化名称**：共用的 `InputClearIconButton` 取 `CommonLangResourceKind.Clear`（新增键，追加在枚举末尾以保持既有成员的数值；en-US / zh-CN / zh-TW / pt-BR 语言包同步补充），随语言切换更新；持有设值句柄，避免累积属性帧。
- **排查中发现的问题**：用真实 Tab 键逐站统计各输入控件的停靠点，又发现两处，均已修复：
  - 不可编辑 ComboBox 多出两个停靠点：看不见的外框和下拉箭头；
  - NumericUpDown 完全无法用 Tab 进入：其旋钮主题继承了 ButtonSpinner 的 `TabNavigation=None`。
- **未处理**：DatePicker / TimePicker 及两种范围选择器的唯一停靠点仍是外框，输入框到不了。这属于审计中的“键盘 P1：DatePicker”，需要整体的选择器键盘方案，本次未处理。

### 验证

- **受影响项目**（最终代码，串行）：AtomUI.Controls 222、Desktop.Controls 4261、DataGrid 325、Rendering 77、Gallery 412，全部通过；pt-BR 通用语言包项目构建通过。没有运行全量回归。
- **新增测试**：
  - `ComboBoxKeyboardNavigationTests` 6 项：清除按钮的停靠点、名称与 Enter / Space 清除，两种模式的 Tab 顺序，以及关闭时不重复过滤；
  - `FocusOutlineTargetTests` 中：11 类输入宿主“外框不作停靠点、箭头与步进按钮不作停靠点、宿主可达”，以及 TreeView / NavMenu 选中且活动项的描边。
- **脚本视觉验收**：21/21 PASS。
  - 宿主为 atom:Window，原生截图，写法取自 Gallery，Tab / Enter 经框架输入路由驱动，亮暗两种主题；
  - 门禁 A/A=0、扰动 3750、还原 0，DLL 一致，窗口全程激活；
  - 覆盖：两种 ComboBox 的清除按钮被 Tab 到达，描边挂在按钮上，3 DIP 描边色、1 DIP 间隙，带名称 “Clear”；Enter 清除后焦点回到 ComboBox；NumericUpDown 能被 Tab 进入输入框，外框显示焦点态，无叠加描边。

## 第二轮复审修复

对照 `release/6.0` 重新审查本分支全部 6 个提交，按焦点描边、Skeleton/动效/进度条、共享基类、ComboBox/对话框、日期时间选择器分 5 路进行。上一轮修复逐项复核均正确；以下为本分支新引入或复审中确认的问题，均先用临时用例复现为红、修复后转绿，临时用例已删除。

### P2

| 问题 | 修复 |
| --- | --- |
| `ManagedFocusOutline.Hide` 直接移除装饰器，AdornerLayer 对目标与全部祖先的订阅不释放，键盘活动项每移动一次泄漏一条（Window 订阅 15→35/20 次） | 移除前先清空 `AdornedElement`；`FocusOutlineAdorner` 在被移出层时兜底清空，覆盖 Avalonia `Control.OnLostFocus` 的同类泄漏（上游缺陷） |
| `FocusOutlineAdorner` 关闭裁剪后，滚出视口的元素描边仍绘制在视口外 | `IsClipEnabled` 保持关闭，`Render` 按祖先裁剪逐边裁剪：贴身包裹层不裁描边，视口切到的一侧严格裁剪 |
| CircleProgress 每次改宽高以 Template 优先级写 FontSize 并丢弃句柄，属性帧无上限增长（50 次 17→117） | 新增 internal `HeldValueExtensions.SetHeldValue`，持有句柄替换；默认完成图标同样改为持有句柄 |
| DatePicker / RangeDatePicker 的 `Clear()` 被当作键入并打开弹层 | 先清值与草稿、同步 `Text`，再清输入框并关闭弹层 |
| RangeDatePicker Tab 离开控件时丢弃有效键入值 | 只丢弃无效文本，有效值按焦点离开规则提交 |
| RangeDatePicker 失焦提交单端可产生 Start > End | 所有提交路径统一经 `CommitRange` 按日期排序，未变化端点不写 |
| NumericUpDown 清除按钮成为首个停靠点，Enter 清除后焦点丢失 | 输入框 `TabIndex=0`、清除按钮 1，替换 Avalonia 的 TabIndex 转发；清除后焦点回到输入框（TextArea、Mentions 基线同类问题一并修复） |
| NavMenu Vertical/Horizontal 模式 Shift+Tab 回到菜单时出现两道描边 | 项设 `FocusAdorner=null`；Tab/方向键焦点落到项上时收回菜单根 |

### P3

- 描边圆角在形状变化而尺寸不变时不刷新：装饰器订阅目标及其形状提供者的属性变化，圆角变化时重绘。
- Expander / CollapseItem 点击内容后按 Space/Enter 误切换：隧道阶段记录按下位置，内容区按下屏蔽切换键，键盘聚焦或按 Header 解除。
- TreeView Shift 范围选择：增量计算改用 HashSet；同步期间的中间 `SelectionChanged` 标为已处理，最后统一发一次。
- ComboBox：附加区可聚焦内容按视觉顺序参与 Tab 且不再被困住（共享 `AddOnDecoratedBoxTheme` 规则，也修正选择器左右附加顺序）；焦点在清除按钮或附加内容时打开下拉，焦点移到输入；钉住时选择不再重复过滤（5 遍→1 遍）。
- 清除按钮自动化名称改为 ControlTheme setter 绑定语言资源，应用样式可覆盖，删除代码侧语言监听。
- Dialog 触控/笔点击改在抬起后交接焦点，不再先聚焦 Surface。
- AsyncImage：隐藏期间进入视口、再显示时重新判定；懒加载占位改由 internal 属性 `IsViewportPending`（有 Source 且等待视口）控制，不新增公开伪类。
- MotionGate 在宿主 `OnAttachedToVisualTree` 中创建时不再重复挂载祖先订阅。
- Skeleton（Button/Input/Avatar/Element）与 Tag 的 Template 优先级写值改为持有句柄（基线已有）。
- 日期面板导航目标格禁用时不翻页；脱离逻辑树/视觉树或换模板时丢弃键入草稿，不回写；钉住时 Escape 恢复候选与输入文本（TimePicker、RangeTimePicker、DatePicker、RangeDatePicker）。

### 遗留

- 祖先 `ClipToBounds`/`Clip` 单独变化不会触发描边重算；圆角裁剪按外接矩形处理。
- TreeView 的 `SelectedItems` 集合观察者仍先看到删除再看到添加（Avalonia 无批量 API）。
- 范围部分确认后、关闭前，提交值可能短暂逆序。TimePicker 生命周期关闭且 `IsNeedConfirm=false` 时仍提交候选（基线行为）。
- NumericUpDown 附加内容与选择器内附加尚未按视觉顺序排 Tab；SkeletonAvatar 非 Custom 尺寸下 `Size` 被样式覆盖（基线问题）。
- 新增常驻测试 1 项：`ComboBoxKeyboardNavigationTests.AddOn_Content_Keeps_Visual_Tab_Order_Without_Trapping_Focus`（键盘陷阱属严重可达性故障且无其他覆盖）。焦点描边三项（视口裁剪与贴身包裹、NavMenu 单描边、Expander 形状刷新）已做脚本视觉验收：atom:Window 宿主、原生截图、亮暗两种主题，25/25 PASS，门禁 A/A=0、扰动 160、还原 0。

### 验证

- `python3 scripts/verification/test.py run --scope change --base HEAD`：passed，8 个测试项目共 5314 项、0 失败（Controls.Shared 146、Controls 167、Core 366、DataGrid 325、Rendering 76、Desktop.Controls 3698、GalleryBase 197、Gallery 339）。
- 没有运行全量回归与 Gallery NativeAOT；视觉验收只覆盖焦点描边三项。
