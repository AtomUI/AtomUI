# 设计语言与品牌色、PrimeNG 官方对齐：进度

更新于 2026-11-15。所有提交都在本地 `release/6.0`，**均未推送**。

## 权威来源

- PrimeNG 以**最新版**为准：primeng 22.1.5（primeng.dev 当前运行版本）、`@primeuix/themes` 3.1.1、`@primeuix/styled` 1.2.0，2026-11-15 在 npm 核对过。
- 优先级：先看官方文档 https://primeng.dev/theming/styled，再看 3.1.1 发布包源码，最后用内置浏览器量 primeng.dev 的计算样式核对。
- primeng.dev 首次访问时，Aura、Lara、Nora 的默认主色都是 **noir**（`primary:'noir'`）。预设源码里写的 emerald 只是回退值。

## 一、设计语言与品牌色分离（已完成并合入）

- 规格：`docs/superpowers/specs/2026-11-15-design-language-brand-color-design.md`。计划：`docs/superpowers/plans/2026-11-15-design-language-brand-color.md`。
- 主题声明 `DesignLanguage`、`Density`、`BrandColor`（`Fixed` / `Seed`），由 `ThemeSelectionResolver` 解析。只有声明为 `Seed` 的设计语言才能换品牌色。
- 执行过程：9 个任务，每个任务都经过审查；之后做整分支终审，修复 I1–I3 和 M1、M2、M3、M7、M8，复审通过。
- M4、M5、M6、M9 暂缓（裁定：在已发布主题上没有可见故障，代价是以后补一个小 PR）。
- 先变基到 `release/6.0`，再快进合入，止于 `363d28ca4`。合入前全量回归 12681 项通过。
- 功能分支和 worktree 都已删除。

## 二、PrimeNG 默认主色 noir 与选中高亮（已完成）

| 提交 | 内容 |
| --- | --- |
| `b7714e23f` | 默认主色改为 noir：12 个 PrimeNG 主题的种子是 `#020617`，中性色族 950 级的种子都视为 noir，主色板取当前外观的表面色阶；Gallery 品牌色块为 Noir 加官网 16 色 |
| `1de2c247e` | Nora 亮色选中底按 3.1.1 改为实色主色 600 |
| `445279a0a` | TreeView、TreeSelect、TransferTreeView 新增 `NodeSelectedColor`；ToggleSwitch 新增 `HandleCheckedBg`；Ant Design 渲染不变 |
| `1f7dd1745` | 选中项严格按 PrimeUIX highlight 设置；ToggleSwitch 选中把手按 PrimeUIX |
| `d2c19fb6c` | Shadcn 暗色的选中把手取背景色（原来是白把手压在近白轨道上） |
| `43a988a2b` | Core 新增全局 Alias Token `ControlItemColorActive`（选中项文字色，默认 `ColorText`）；列表、树、DataGrid（`RowSelectedColor`）、时间与日期格都接入 |
| `dacafbb35` | PrimeNG 只写这一个全局值；日期选择器按 PrimeUIX 改为实色高亮 |
| `7ee364342` | DataGrid 选中行、ComboBox、Cascader、TreeView 的选中文字色随背景同步过渡 |

验证：

- 聚焦测试全部通过。
- 全量回归 12716 项通过（截至 `7ee364342`）。
- 渲染图用普通 Avalonia 窗口生成：noir 亮/暗，Nora 的翡翠绿、蓝、靛蓝亮/暗。

已知行为变化：

- 彩色种子下，Aura 和 Lara 的选中文字变成主色 700，DataGrid 和日期格也一样。这是跟随官方。
- Nora 亮色翡翠绿上的白字对比度是 3.77:1，Nora 暗色靛蓝是 4.45:1，和 PrimeUIX 自身取值一致。

暂未处理：

- LunarCalendar 的选中格。
- 紧凑日历选中格的文字是瞬时切换（Ant Design 原有行为）。
- NavMenu 的选中项仍是浅色底。

## 三、弹层边框与阴影（已提交，等待用户看图）

用户反馈：PrimeNG 弹层和官方相差太多，关键是**边框**。

官方实测（Aura 亮色）：边框 1px `#E2E8F0`，圆角 6px，阴影 `0 4px 6px -1px` 加 `0 2px 4px -2px`（10% 黑），内边距 10px，gutter 10px，箭头连同边框一体描边。

| 提交 | 内容 |
| --- | --- |
| `f619ba31d` | 带边框的 ArrowDecoratedBox 不再隐藏箭头，改为框体与箭头一体描边，箭头底边无横线。新增 `ContentBorderColor`、`ContentBorderThickness`、`ContentMinHeight`。框体自带边框时，弹层不再绘制描边环。Tooltip 固定无边框。Ant Design 渲染不变 |
| `8bafa9557` | PrimeNG 弹层按 3.1.1 overlay：箭头 flyout 带 popover 边框、内边距、10px gutter 和阴影；Select、菜单等面板用 overlay.select 描边环和圆角 |
| `3e7791e19` | 控件文档与未发布 API 变更说明（中英文） |

## 四、Link 按钮变蓝（已提交，等待用户看图）

- `0e41a4e32`：Link 按钮按 PrimeUIX `button.link.color` 取主色三态，只作用于 Button。noir 下是黑色，彩色种子下与主色一致；全局链接色保持深色级。
- 根因分析写在子代理报告里，整理时报告还没生成，待补。

## 五、进度条颜色（进行中）

- 原因：AtomUI 进度条沿用 Ant 的写法，进度色是 `ColorInfo`，轨道是 `ColorFillSecondary`。
- 官方实测（Aura noir 亮色）：进度色是主色 `#020617`；轨道亮色 `#E2E8F0`、暗色 surface 700；条内文字白色（对比色）10px/600；高 18px；圆角 6px。
- 做法：只在 PrimeNG 下覆盖直线、圆形、仪表盘、步骤四种进度条，Ant Design 不变。成功和异常状态用 PrimeNG 的状态色。
- 现状：子代理正在改，工作区里还有未提交文件（ProgressBar Token 和主题、PrimeNG 三个预设表、文档）。

## 下一步

1. 进度条修完后审代码，把弹层、Link、进度条的渲染图（PrimeNG Aura 亮/暗、Nora，加 Ant Design 对照）发给用户看。**这一批不跑全量回归**（用户要求）。
2. 待用户决定：Shadcn 的带箭头弹层是否用新的一体描边补上 `ColorBorder` 边框。
3. 所有渲染图都还没在真实 Gallery（`atom:Window`）里亲眼确认。
4. 推送 `release/6.0` 需要用户授权。
