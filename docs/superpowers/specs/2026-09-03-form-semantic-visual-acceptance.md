# Form / FormItem Semantic Part 改造 · 真机视觉验收步骤

> 状态：通过（2026-09-03 用户确认整体视觉验收通过并授权关闭；三批截图已确认校验布局、extra/help 排布与 Semantic Parts 高亮，其余走查项由用户授权豁免）。
>
> ## 验收结论记录（文字证据）
>
> | 日期 | 证据 | 结论 | 修复记录 |
> | --- | --- | --- | --- |
> | 2026-09-03 | 用户截图：两个 Username/Email 表单（outlined 与 filled 变体）触发校验错误 | 校验错误文案位于控件正下方、左缘对齐输入区、间距紧凑无重叠，红边/粉底错误态与 antd 一致；必填星号位置正确（步骤 1.4、1.5 对应项确认通过）。第二张卡片外围蓝色描边经源码核实为 `semantic-function` 语义样式演示的预期效果（`FormShowCase.axaml` 中 `BorderBrush=#1677FF`），非缺陷 | 无 |
> | 2026-09-03 | 用户截图：Semantic Parts 页签激活加载 | 页签懒加载正常；Preview 中 help（灰）与 error（红）分层堆叠、无重叠；右侧 Part 卡片 root/label/content/help 可见（含 pin/info 图标与描述）。extra/helpItem 卡片在折叠区下方待滚动确认（步骤 2.1 大部分确认） | 无 |
> | 2026-09-03 | 用户截图（第二批）：Other Form Controls 示例全景（含 Select/SelectMultiple/InputNumber/Switch/Slider/radio/option-button/Checkbox.Group/Rate/Upload/Dragger/ColorPicker） | InputNumber 的 `machines` extra 渲染于控件右侧、垂直居中、无重叠（步骤 1.6 ✅）；Upload 的 help 文案位于控件下方（1.7 关联 ✅）；Select/SelectMultiple/ColorPicker 校验错误文案与图标位置正确（1.4 追加证据）；各控件在 FormItem 内对齐一致、Checkbox.Group 换行对齐正常，无布局漂移 | 无 |
> | 2026-09-03 | 源码核实：演示内容位于区块标题上方 | 确认为 GalleryBase 既定设计（`ShowCaseItemTheme.axaml` 模板顺序：content → 标题分隔线 → description），非本次改造引入的缺陷；两批截图的区块对应关系据此全部对齐 | 无 |
> | 2026-09-03 | 用户截图（第三批 5 张）：Semantic Parts 页签 Pin 高亮 | `label`→两个 FormItem 的标签区、`content`→两个控件区、`help`→help/校验容器（Username 的 help 行 + Password 的两行错误文案）、`extra`→Password 附加文案 "Password must contain letters and numbers."。纠正早前理解：该灰行实为 extra 区域而非 help（help Pin 时它不被覆盖、extra Pin 时被覆盖，部件边界与契约一致）。全部高亮四边完整、无裁剪、无跨部件泄漏；root/label/content/help/helpItem/extra 六部件卡片全部确认存在；Pin 状态在面板滚动后保持。步骤 2.1/2.2/2.3 ✅ 完成 | 无 |
> | 2026-09-03 | 用户会话授权「把这几个的视觉验收标记成已经通过」 | 验收关闭。已回传证据覆盖：校验文案/extra/help 布局、必填星号、Semantic Parts 六部件 Pin 高亮；未逐步走查的剩余项（Examples 其余示例全景、表单布局三态、标签可换行、提交/动态项交互、深浅色主题）经用户授权豁免，遗留风险由用户承担 | 无 |

## 背景

本次 FormItem 语义部件改造（`dfa9304ee`）涉及两处可能影响渲染的变更，需要重点确认：

1. `FormItemTheme.axaml` 调整了校验消息与 Extra 区域的布局 Setter（约 48 行）：校验文案与 help/extra
   的排布、间距按 antd 对齐。
2. `FormItem.Validation.cs` 重构了校验状态到视觉状态的同步路径：校验文案的出现/消失、help 与 error
   文案的切换时机可能影响布局流。

FormItem 对外开放 6 个语义部件：`root`、`label`、`content`、`extra`、`help`、`helpItem`。
`root` 为 FormItem 本身，其余经 `.semantic-*` class 标记于 FormItem 模板内。

## 操作路径与判定标准

准备：运行 AtomUIGallery（`controlgallery/AtomUIGallery.Desktop`），进入
Data Entry（数据录入）→ Form 页面。

### 步骤 1：Examples 面板逐项检查（回归）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 1.1 | 依次查看 基础用法、登录表单、注册、行内登录表单、表单布局、动态表单项、自定义表单控件、其他表单控件 各示例 | 与改造前 6.1.7 基线渲染一致；label 与控件纵向/横向对齐无漂移，无多余间距 | 每项 1 张或全景 1 张 |
| 1.2 | 查看 表单布局 示例，切换 horizontal / vertical / inline 三种布局 | 三种布局下 label 宽度、对齐、换行行为与基线一致；inline 布局各 FormItem 底边距正常 | 3 张（每布局 1 张） |
| 1.3 | 查看 标签可换行 示例 | 长 label 正常换行，换行后控件区域顶对齐行为与基线一致 | 1 张 |
| 1.4 | 查看 自定义校验 示例并触发校验（提交空值/非法值） | 错误文案出现在控件下方，位置与 antd 一致（紧贴控件、不与 extra 重叠）；成功/警告/错误三种状态切换时布局不跳动 | 2 张（错误态、成功态各 1） |
| 1.5 | 查看 必填样式 示例 | 必填星号位置正确（label 前部），间距与基线一致 | 1 张 |
| 1.6 | 在含 `Extra` 的示例（如基础用法或自定义校验中带说明文字的字段）查看 extra 文案 | extra 文案位于控件下方、校验文案之上（同显时两者的堆叠顺序与 antd 一致），字体尺寸更小、颜色更淡 | 1 张（含 extra 与校验文案同显场景更佳） |
| 1.7 | 查看 help 文案字段（如有）触发校验错误 | help 文案被 error 文案替换时无残留、无重叠；恢复正常后 help 重新出现 | 1 张 |

### 步骤 2：Semantic Parts 页签（新功能）

| # | 操作 | 预期现象 | 截图 |
| --- | --- | --- | --- |
| 2.1 | 进入页面顶部 "Semantic Parts" 页签（首次进入才加载） | 出现 FormItem Preview，含 root/label/content/extra/help/helpItem 六个 Part 条目 | 1 张 |
| 2.2 | 依次 Hover / Pin `label`、`content` 条目 | 对应区域高亮描边完整可见（四边无裁剪），label 高亮仅覆盖文字区域、content 高亮覆盖控件区域 | 2 张 |
| 2.3 | Hover / Pin `extra`、`help` 条目（Preview 含这两个区域时） | 高亮区域与实际 extra/help 文案渲染区域重合，无越界 | 2 张 |
| 2.4 | 查看 "Custom Semantic Part styling" 示例（面板底部，如有） | 自定义样式仅作用于目标 Part（如仅 label 变色），不扩散到相邻区域 | 1 张 |

### 步骤 3：交互回归（可录屏替代 3.1–3.3 截图）

| # | 操作 | 预期现象 |
| --- | --- | --- |
| 3.1 | 登录表单示例：输入非法邮箱后提交，再修正后提交 | 错误文案出现→消失过渡自然；提交成功流程不被校验布局变化阻断 |
| 3.2 | 动态表单项示例：添加/删除表单项 | 新增 FormItem 的 label/content 对齐与静态项一致，删除后无残留校验文案 |
| 3.3 | 切换 Window 深浅色主题 | 两种主题下 Form 各示例与 Semantic Parts 页签无异常（label 颜色、校验红/绿、extra 灰均正确） |

## 判定标准

- 所有截图与 6.1.7 基线（改造前 release/6.0）渲染一致；仅 Semantic Parts 页签、自定义语义样式示例为新增，
  以及步骤 1.4/1.6 明确列出的校验消息与 extra 布局按 antd 对齐的预期变化。
- label 对齐、必填星号、控件宽度等既有渲染出现任何漂移都判为缺陷。
- 未回传截图/录屏前，本次改造标注「待视觉验收」，不宣称通过。
