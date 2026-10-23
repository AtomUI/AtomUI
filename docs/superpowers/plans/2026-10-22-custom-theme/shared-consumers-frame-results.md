# CT-10 S3：共享输入框体几何验收

日期：2026-10-23。分支 `feature/custom-theme`，基础提交 `79f2fc00e`；本批未提交，作者自审，不是独立审查。仅证明本页列明的几何消费，不代替各家族 Own V2、排版、交互或最终原生显示面验收。

## 问题与修复

原十三个公开 owner 的真实框体均无法完整消费根 `Padding=11,13`、`BorderThickness=3`、`CornerRadius=0`：十二项内边距未到达，TextBox 的尺寸样式把圆角恢复为 6。控制级 Global 配置另发现 TextBox 仍读共享边框/圆角。修复把具体 owner 的 Effective Global 作为根属性默认值，由 TemplateBinding 投射到实际框体，尺寸选择仅写内部预设默认值，不压制用户原生属性和 Style。

覆盖 LineEdit、TextBox、TextArea、SearchEdit、Select、ComboBox、Cascader、TreeSelect、DatePicker、RangeDatePicker、TimePicker、RangeTimePicker、NumericUpDown，以及独立 ButtonSpinner。TextArea 的内边距属于内层文本框，保留原有右侧四分之一间距和滚动/后缀预留；NumericUpDown 分段模式的根内边距属于文本 Margin，外层保持零内边距；ButtonSpinner 保留 handle 预留。

Select/Cascader/TreeSelect 的多选空/非空与 Custom 高度分支分别验证。Custom 紧凑计算只产生主题默认值，显式根输入和普通应用 Style 可以覆盖。可见紧凑内边距与标签固有测量预算分离，避免 Borderless/Underlined 标签高度改变。动态资源是原生 BindingBase，未引入订阅标记或第二套优先级。三者仍有既有 Select Own 字段依赖，留给 CT-14，不算 Own V2 迁移完成。

SearchEdit 右侧按钮的已安排高度反向进入 Panel 测量，导致 Padding 恢复后高度无法缩回。现在自然高度使用当前文本框、左 addon 和按钮最小高度；保留原按钮高度绑定、形态、宽度及排列。四种尺寸和固定 Custom 高度的五个失败前证修复后通过，输入文本、焦点、选择和目标实例保持。

可选根画刷保留条件式代码转发，并在代码说明原因：实际 Avalonia 12.1.4 的初始 LocalValue 绑定即使返回 Unset，也会播种 null 元数据值，使四个既有默认/清除用例失败；降低绑定优先级又改变显式根画刷覆盖状态样式的既有合同。失败的纯转换器方案已删除，没有把不等价绑定迁入 ControlTheme。

## 验证

- 临时几何矩阵 22 项、控制级 Global 15 项、SearchEdit 恢复 5 项、Custom 默认/显式样式 3 项、资源/owner 释放 1 项，共 46 项；相关既有用例一起运行 **102/102，通过，3 秒**。原始矩阵、控制级覆盖和 Custom 覆盖均有真实失败前证。
- 真实 Skia、1020×650、缩放 1；24 组场景各 24 个实例，覆盖四种 variant、三档/Custom、禁用、错误、多选、addon 和数值分段。明暗各 24 组，共 **48 组旧默认→新默认全部 0 像素差异**；全部 48 组刺激均产生真实像素变化。LineEdit 额外 1 DIP 检错为 530 像素；各组 A/A 为 0。
- 同实例恢复：除下述旧有诊断文字漂移外全部 0。三组 Custom 紧凑多选在明暗下默认和恢复均为 0；修改分别产生明色 39816/40314/39816、暗色 39791/40289/39791 像素差异。高度 24 下显式大内边距可使文本空间不足，这是该显式输入与既有固定高度的约束结果，未移除高度来制造成功。
- SearchEdit 修复后默认比较 0，修改 76942，恢复 0。实际编译 AXAML 用普通 owner Style 设置根几何、生成的 `LineEditInputStyle` 设置 `TextElement.FontWeight=Bold`，真实框体与文本均命中；旧实现根几何断言失败。未通过 code-behind 修改语义目标。
- DatePicker/TimePicker 原始诊断标题文字在高度变化后有明色 4/16、暗色 2/23 个 RGB1 漂移像素。完整旧源码 A/B 在同 checkout 临时反向、最后逐字节恢复，证明旧版也产生完全相同结果：**旧高度恢复→新几何恢复在两种主题均 0**。未裁剪、遮罩、放宽容差；此旧有原生文字重绘问题归 CT-25，不宣称已修复。
- 已查看编译 Style、TextArea 内层 inset、Numeric 分段、SearchEdit 恢复、紧凑多选默认/修改及相关差异图。几何、状态和极端固定高度下的裁剪与对应断言一致。

临时命令：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --filter 'FullyQualifiedName~Temporary.CustomThemeFrames|FullyQualifiedName~TextInputRootBrushRelayTests|FullyQualifiedName~ButtonSpinnerRootSurfaceTests|FullyQualifiedName~ComboBoxRootSurfaceRelayTests|FullyQualifiedName~SelectRootSurfaceRelayTests|FullyQualifiedName~SearchEditLayoutTests|FullyQualifiedName~SearchEditBehaviorTests|FullyQualifiedName~CustomizableSizeTypeContractTests'`。

脚本驱动：外部 `Visual.csproj` 的 `before/final/after-extra/after-more/compact-fixed/search-fixed/part/legacy-label` 场景；暗色以 `ATOMUI_FRAME_DARK=1` 启动并断言实际 appearance。驱动、场景、媒体和专用产物按临时生命周期清理，文字结果保留；清理后 focused 验证结果见下方追加记录。

## 剩余边界

本批没有新增 Own 字段/identity，没有改变 SVG 数学几何或默认品牌外观。Cascader 多选空 Small 旧版使用 Middle padding 的差异保持，交 CT-14/CT-20 决策。根字体/前景、图标、绘制缓存与其他共享消费者继续 S4；各控件完整 token/状态审计仍属 CT-12–CT-20。未运行全量回归，未声称原生按钮无闪烁、所有 OS 或 AOT 验收完成。

## 清理后验证与自审

临时 46 项、外部驱动及 bin/obj 已删除；受管目录 372 个文件删除，媒体 audit clean，无新增取证二进制。清理后上述既有精确组及 LineEditBorderRender、ButtonSpinnerSpacing、NumericUpDownSpinnerLayout/PrefixLayout：**66/66，通过，1 秒**。Desktop NET8 Release：**0 warning / 0 error，19.94 秒**。六个触及的 C# 文件成员布局审计 0 问题。作者自审未发现本批未处理的 Critical/Important；S3 排版/前景勾选继续等待 S4。
