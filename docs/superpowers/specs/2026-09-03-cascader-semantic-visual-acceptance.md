# Cascader Semantic Part 改造 · 真机视觉验收步骤

> 状态：**已关闭（2026-09-03，经用户授权裁剪范围）**。改造代码与测试已完成（见 `docs/superpowers/plans/2026-09-03-cascader-semantic-part.md`
> 执行期修订记录）。Semantic Part 相关判定——预览弹层钉住 + 步骤 1.6 静态判定（含 Filled 填充修复）——已由用户截图确认通过；
> 其余基线回归走查项（1.1–1.5、1.6 弹层展开子项、1.7、1.8、2.2–2.8、2.10）按用户 2026-09-03 指示裁剪：
> 本分支原则上只做 Semantic Part 相关视觉验收，除非 bug 修复需要，回归走查不再作为逐项门槛。
>
> ## 验收结论记录（文字证据）

| 日期 | 证据 | 结论 | 修复记录 |
| --- | --- | --- | --- |
| 2026-09-03 | 用户回传截图：Data Entry → Cascader → Semantic Parts 页签。预览为多选 + 预选 "West Lake" 标签；弹层钉住常开且页面无遮罩阻挡；`popup.list` 高亮同时罩住两列；右侧部件列表含 itemRemove / clear / popup.root / popup.list / popup.listItem | 预览弹层钉住 + light-dismiss 机制确认与 AutoComplete 一致，**步骤 2.1 / 2.9 的 popup 部分通过**（该场景即本轮 IsOpen 绑定移除 + AbstractSelect 机制下沉的修复点） | 移除模板 `IsOpen` 绑定、机制下沉 AbstractSelect（见计划修订记录第 8 条） |
| 2026-09-03 | 用户回传截图：样式示例两个带 prefix 的 Cascader（Outlined / Filled），已选中 "Zhejiang/Hangzhou"。像素实测：Outlined 控件 64 原始px = 32 逻辑px；Filled 灰底仅 60 原始px = 30 逻辑px，且与上方控件间距 42 原始px（= 间距 20 + 1px 透明边框环），证明布局高度本就是 32、可见填充被 `InnerBorderEdge` 裁进 1px 透明边框内侧 | **步骤 1.6 的 Filled 变体暴露共享层缺陷：填充可见高度比 Outlined 矮 2px**（Middle 应为 32）。headless 探针证实布局高度 Filled=32 无差异，根因为 Filled 保留 1px 透明占位边框 + ContentFrame `InnerBorderEdge` 裁切 | 共享 `AddOnDecoratedBoxTheme` 为 Filled 变体追加 `BackgroundSizing=OuterBorderEdge`（见计划修订记录 8.3）；新增 `AddOnDecoratedBoxVariantTests` 回归（先红后绿）。修复后经下一行截图复核关闭 |
| 2026-09-03 | 用户回传截图：Custom Semantic Part styling 两个带 prefix 的 Cascader（object styles=Outlined / function styles=Filled）。像素实测：Outlined 外框 y=1266–1329 共 64 原始px = 32 逻辑px；Filled 灰底 #F4F4F4 连续带 y=1370–1433 共 64 原始px = 32 逻辑px，首末行（y=1370/1371、1432/1433）全宽均为灰底无白边，两框间距 40 原始px = 20 逻辑px 即纯布局间距（证明填充已铺到外框边缘）；Filled 箭头主色 (24,144,255) = #1890FF，Outlined 箭头 (192,192,192) 默认浅灰；prefix 文字 #BFBFBF、占位符 #1890FF | **步骤 1.6 静态判定通过**：`BackgroundSizing=OuterBorderEdge` 修复真机确认生效——Filled 与 Outlined 等高 32px、灰底铺满圆角框上下无白边、Filled 箭头蓝色 #1890FF 且未样式化控件箭头保持默认浅灰。1.6 的弹层展开子项（object 弹层蓝框/候选项深色字、function 弹层灰框/候选项蓝字）仍待展开态截图 | 无需修复（纯验收确认） |
| 2026-09-03 | 用户指示：本分支原则上只做 Semantic Part 相关视觉验收（除非 bug 修复需要），1.1–1.5、1.6 弹层展开子项、1.7 全景、1.8 Select 抽查、2.2–2.8、2.10 等基线回归走查属多余项 | **Cascader 视觉验收关闭**：以已确认的弹层钉住 + light-dismiss 机制、1.6 静态判定（两框等高 32px / Filled 灰底铺满 / 箭头蓝色 #1890FF）为准；回归走查项不再等待回传 | 无需修复 |

## 背景

本次 Cascader 语义部件改造涉及四处可能影响渲染的变更，需要重点确认：

1. `CascaderTheme.axaml` 新增 9 处静态 marker，并新增 `ContentLeftAddOn` 前缀投影节点
   （`LeftAddOn` 内容的承载方式变化，带前缀的 Cascader 需重点回归）。
2. 共享资产微调：`SelectHandleTheme.axaml` 的清除按钮新增一个 inert class marker（Select /
   ComboBox 等共用该主题，理论上零视觉影响，需抽查一个 Select 页面确认）。
3. `CascaderViewTheme.axaml` 新增 scope 锚点与 `popup.list` 静态 marker（弹层视觉不变）。
4. `CascaderShowCase.axaml` 宿主从 `GalleryStickyTabsHost` 迁移到 `GalleryShowCaseHost` 并新增
   Semantic Parts 页签（示例内容本身不变，页面壳层结构有预期变化）。

Cascader 对外开放的语义部件：`root`、`prefix`、`content`、`placeholder`、`input`、`suffix`、
`clear`、`item`、`itemContent`、`itemRemove`、`popup.root`、`popup.list`、`popup.listItem`。
Semantic Parts 页签中的预览为多选模式并预选一项，标签与弹层同时可见。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
Data Entry（数据录入）→ Cascader 页面。

### 步骤 1：Examples 面板回归走查

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 基础用法示例：展开弹层、选择一项 | 与 6.1.7 基线一致：列宽、选项高亮、选中后回填路径文本、弹层关闭 | 1 张（弹层展开态） |
| 1.2 | 多选示例：勾选多个选项、移除一个 tag | tag 区域渲染与基线一致，maxCount 指示与后缀 handle 正常 | 1 张 |
| 1.3 | 带前缀/后缀的示例（Prefix and Suffix）：确认前缀内容渲染 | 前缀内容出现在内容区左侧且垂直居中（ContentLeftAddOn 投影重构重点证据）；后缀正常 | 1 张 |
| 1.4 | 搜索示例：输入关键字触发过滤 | 过滤列表出现、命中高亮、选择后回填完整路径 | 1 张 |
| 1.5 | 状态与尺寸示例各看一眼 | disabled / error / warning / 三档尺寸渲染与基线一致 | 1 张全景 |
| 1.6 | 自定义语义结构的样式示例：object styles 展开弹层看弹层边框与候选项字色；function styles（Filled 变体）同查 | 前缀文字灰色、占位符蓝色、弹层蓝色边框、候选项深色文字（object）；弹层灰边框、候选项蓝色文字（function/filled）。**Filled 与 Outlined 两个选择框等高（Middle 均为 32px），Filled 灰底铺满整个圆角框、上下无白边**（Filled 填充修复的验收点）；**Filled 的下拉指示箭头为蓝色 #1890FF**（suffix 语义样式改色 + antd 对齐的验收点），未设置样式的其他 Cascader 箭头保持默认浅灰 | 2 张 |
| 1.7 | Variants 示例四种变体并排目测高度（重点 Filled） | Outlined / Filled 灰底视觉等高；Borderless / Underlined 当前仍为 30/31（共享层等高补偿设计问题，待用户决策，见已知边界） | 1 张 |
| 1.8 | 抽查 Select 页面基础示例（共享受影响面） | 与基线一致（SelectHandleTheme marker 不应有任何视觉漂移；Filled 变体同样铺满） | 1 张 |

### 步骤 2：Semantic Parts 页签

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 进入 "Semantic Parts" 页签（首次进入才加载） | 出现 Cascader Preview，弹层钉住常开；Part 列表显示 10 个 Part 与本地化职责描述；底部展示用户侧 AXAML 用法 | 1 张 |
| 2.2 | Hover/Pin `root` | 高亮罩住整个选择框，四边完整不被父容器裁剪 | 1 张 |
| 2.3 | Hover/Pin `prefix`（示例无前缀内容时高亮框可能极窄或不可见，属预期；可临时给示例加前缀验证） | 高亮罩住内容前缀区域 | 1 张 |
| 2.4 | Hover/Pin `content` | 高亮罩住内容区整体（占位文本 + 后缀之间的区域） | 1 张 |
| 2.5 | Hover/Pin `placeholder` | 高亮紧贴占位文字（不得撑满整个内容区宽度） | 1 张 |
| 2.6 | Hover/Pin `suffix` | 高亮罩住右侧后缀区（maxCount 指示 + handle） | 1 张 |
| 2.7 | 鼠标悬停选择框使箭头切换为清除图标，Pin `clear` | 高亮精确罩住清除按钮（注意：clear 经 SelectHandle 跨嵌套解析，是跨嵌套机制重点证据） | 1 张 |
| 2.8 | Pin `item` / `itemContent` / `itemRemove` | 分别罩住选中标签整体、标签内文本、标签内移除按钮（标签由预选项呈现，是标签机制跨嵌套解析重点证据） | 3 张 |
| 2.9 | Pin `popup.root` / `popup.list` / `popup.listItem` | 分别罩住弹层圆角框、每一列、列内每个选项；弹层保持常开且页面其余区域无遮罩阻挡 | 3 张 |
| 2.10 | 切回 Examples 页签再回来、切换其他页面后返回 | 高亮释放、无残留 Adorner，预览与 Part 列表状态正常 | 1 张 |

### 判定标准

- 所有步骤的渲染与 6.1.7 基线一致，仅 Semantic Parts 高亮为新增视觉。
- 高亮框（Adorner）四边完整，不被 `ClipToBounds` 祖先裁剪。
- 弹层钉住期间页面其余区域可正常交互（无 light-dismiss 遮罩）。
- 回归步骤（1.1–1.6）无任何视觉漂移。

## 已知边界（验收时不作为缺陷）

- `input`（过滤输入框）仅在单选 + 过滤模式下可见，Preview 中不单独展示；其 marker 命中由
  `CascaderSemanticPartTests` 覆盖。
- Borderless / Underlined 变体总高度为 30 / 31（Outlined / Filled 为 32）：共享 `AddOnDecoratedBox`
  的边框占位未做等高补偿，与 antd 各变体恒定控制高度不一致，属改造前的既有共享层行为。本轮仅修复
  Filled 可见填充裁切，Borderless / Underlined 等高补偿待用户决策后另立任务。
