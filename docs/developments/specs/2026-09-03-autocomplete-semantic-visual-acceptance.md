# AutoComplete Semantic Part 改造 · 真机视觉验收步骤

> 状态：通过（2026-09-03 用户授权直接关闭；未执行逐步真机走查、未回传截图，本记录如实标注豁免）。
>
> ## 验收结论记录（文字证据）
>
> | 日期 | 证据 | 结论 | 修复记录 |
> | --- | --- | --- | --- |
> | 2026-09-03 | 用户会话授权「把这几个的视觉验收标记成已经通过」 | 验收关闭。AutoComplete 未回传任何截图/录屏，属用户授权的豁免关闭而非证据确认；后续如需可按下方步骤补做走查 | 无 |

## 背景

本次 AutoComplete 语义部件改造（`f8e41d095`）涉及四处可能影响渲染的变更，需要重点确认：

1. 三个 owner 变体各自注册语义部件：`AutoComplete`（LineEditBox 承载）、`AutoCompleteSearchEdit`、
   `AutoCompleteTextArea`，对应 `AutoCompleteTheme.axaml`、`AutoCompleteSearchEditTheme.axaml`、
   `AutoCompleteTextAreaTheme.axaml` 各新增静态 marker。
2. 共享输入框主题同步微调：`LineEditTheme.axaml`、`SearchEditTheme.axaml`、`TextAreaTheme.axaml`
   （8~20 行）——普通 LineEdit / SearchEdit / TextArea 的渲染也可能受影响。
3. 跨嵌套与弹层机制扩展：AutoComplete 嵌套在 Form/其他容器内时，候选弹层的归属与定位路径变化。
4. `AutoCompleteShowCase.axaml` 大幅重做（约 370 行）对齐 antd 示例，页面示例内容本身有预期变化。

AutoComplete 对外开放的语义部件：`root`、`input`、`trigger`（清除等操作区）、`popup`（候选弹层）、
`validation`（校验反馈）。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
Data Entry（数据录入）→ AutoComplete 页面。

### 步骤 1：Examples 面板逐项检查（回归 + antd 对齐）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 查看 基本使用 示例，输入 `a` 触发候选 | 输入框渲染与 6.1.7 基线一致；候选弹层出现在输入框下方对齐，边框/圆角/阴影与 antd 一致 | 2 张（静止态、弹层展开态各 1） |
| 1.2 | 候选展开后：键盘上下键移动、点击选中、再点击外部关闭弹层，再重新聚焦触发 | 弹层打开-关闭-重新打开全流程正常：高亮项随键盘移动、选中后回填输入框、关闭无残影、重开定位不漂移 | 2 张（键盘高亮态、重开态各 1） |
| 1.3 | 查看 查询模式 - 确定类目 与 查询模式 - 不确定类目 示例并输入 | 确定类目按分组渲染候选；不确定类目异步加载时 loading 态与空结果态显示正确（无布局塌陷） | 各 1 张 |
| 1.4 | 查看 自定义选项 示例 | OptionTemplate 生效（自定义内容渲染完整，不被裁剪） | 1 张 |
| 1.5 | 查看 自定义输入组件 示例（嵌套 SearchEdit/TextArea 变体） | AutoCompleteSearchEdit 变体带搜索按钮、AutoCompleteTextArea 变体为多行输入；两者的候选弹层均正确定位且候选高亮正常——跨嵌套机制重点证据 | 2 张（两个变体各 1） |
| 1.6 | 查看 自定义清除按钮 示例：输入文本后观察清除按钮，点击清除 | 清除按钮出现在后缀区且垂直居中，点击后清空并隐藏 | 1 张 |
| 1.7 | 查看 自定义状态 示例 | error/warning 状态的边框着色正确，校验反馈区域与 antd 一致 | 1 张 |
| 1.8 | 查看 多种形态 示例（filled/outlined 等变体与尺寸） | 各形态边框/背景/圆角正确，尺寸切换下输入区与触发区比例正常 | 1 张 |
| 1.9 | 回归共享受影响面：进入 LineEdit 与 SearchEdit、TextArea 页面各看一眼基础示例 | 普通 LineEdit / SearchEdit / TextArea 渲染与基线一致（共享主题微调未引入漂移） | 1 张全景即可 |

### 步骤 2：Semantic Parts 页签（新功能）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 进入页面顶部 "Semantic Parts" 页签（首次进入才加载） | 出现 AutoComplete Preview（覆盖 root/input/trigger/popup/validation 契约区域；多变体时分组呈现） | 1 张 |
| 2.2 | Hover / Pin `input` 条目 | 高亮精确覆盖输入区域（含 placeholder 区域），四边完整 | 1 张 |
| 2.3 | Hover / Pin `trigger` 条目（Preview 呈现清除按钮时） | 高亮覆盖清除/操作按钮区域，不外溢到输入区 | 1 张 |
| 2.4 | Hover / Pin `popup` 条目并触发候选展开 | 弹层展开时高亮覆盖整个候选面板（含边框）；弹层关闭后无残留高亮 | 2 张（展开态、关闭后各 1） |
| 2.5 | 查看 "自定义语义结构的样式" 示例 | 自定义样式仅作用于目标 Part（如仅候选面板着色或仅输入区变色），不扩散 | 1 张 |

### 步骤 3：交互回归（可录屏替代 3.1–3.3 截图）

| # | 操作 | 预期现象 |
| --- | --- | --- |
| 3.1 | 将 AutoComplete 放入 Form 场景（如登录/注册示例中有组合使用时）触发校验错误 | 嵌套宿主下弹层仍正确定位（不被 Form 布局裁剪或错位）；错误状态与候选弹层同时存在时不互相遮挡 |
| 3.2 | 快速连续输入触发异步候选（不确定类目示例）多次 | 无候选错序、无弹层闪烁抖动；滚动候选列表到底部后弹层尺寸稳定 |
| 3.3 | 切换 Window 深浅色主题 | 两种主题下 AutoComplete 各示例（含弹层）与 Semantic Parts 页签无异常 |

## 判定标准

- Examples 渲染以 antd 官方示例为目标（本次改造明确对齐 antd 示例），与 6.1.7 基线的差异仅限对齐 antd 的预期变化
  与新增的 Semantic Parts 页签；LineEdit / SearchEdit / TextArea 页面必须与基线完全一致。
- 弹层定位漂移、跨嵌套被裁剪、清除按钮偏位、共享输入框主题漂移，均判为缺陷。
- 未回传截图/录屏前，本次改造标注「待视觉验收」，不宣称通过。
