# AtomUI 的 Shadcn 设计语言严格对齐方案

> 实施时按阶段执行。每批用原 Gallery 路径取证、修正控件所属的主题规则，并运行该批定向验证。

## 目标与范围

覆盖 Nova、Vega、Maia、Lyra、Mira、Luma、Sera、Rhea 的浅色和深色。首批交付截图里的 Button 和同一按钮家族的层次与状态对齐。随后完成表单、卡片与浮层，以及已知的暗色状态和色彩差异。以现有 AtomUI 控件、接口和配置为载体，保留应用对主题 Token 和语义部件的定制入口。

沿用[既有设计](../specs/2026-11-15-shadcn-latest-alignment-design.md)、[取值附录](../specs/2026-11-15-shadcn-latest-alignment-data.md)和[暗色验收记录](../progress/2026-11-20-shadcn-dark-gallery-audit.md)。对标版本固定为官方 `f50887ae68d0fdcbfbe86b57be08af94a4c88a68`。将 Base UI 的八种 Style 作为组件取值基准，用 Radix 交叉核对共享样式。颜色、尺寸、圆角、排版、边框、阴影及状态都按对应组件读取。AtomUI 的 Compact、Dashed、状态色扩展、复合控件和 Gallery 示例单独登记映射。字体与图标资产沿用既有项目范围。

- [官方 registry](https://github.com/shadcn-ui/ui/tree/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/registry)
- [Base UI 按钮定义](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/registry/bases/base/ui/button.tsx)
- [Radix 按钮定义](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/registry/bases/radix/ui/button.tsx)
- [Tailwind 阴影刻度](https://tailwindcss.com/docs/box-shadow)

## 第一批规则

| 对象与状态 | 对齐目标 |
| --- | --- |
| 实心主按钮常态 | 八套均为无常态投影。AtomUI Primary 对应 shadcn 的 `default`。 |
| 中性描边按钮常态 | 截图中 AtomUI Default 对应 `outline`。仅 Vega 为 `shadow-xs`，下移 1 DIP、模糊 2 DIP、5% 黑色。其余七套平贴，通过对应的边框和填充表达外形。 |
| 键盘焦点 | 单独应用上游 `focus-visible` 的边框和环。Nova／Vega／Maia 为 3 DIP／50%，Lyra 为 1 DIP／50%，Mira／Sera 为 2 DIP／30%，Luma／Rhea 为 3 DIP／30%。用真实 Tab 验证环出现和清除。 |
| 指针、弹层和禁用 | 上游普通按钮按下下移 1 DIP，具备弹层触发能力的按钮维持位置。已展开是独立状态。禁用整体透明度为 50%，点击没有动作。 |
| AtomUI 扩展 | Dashed 保留虚线，Success／Warning／预设颜色保留各自语义，Shadcn 下这些扩展默认平贴。Vega ButtonGroup 的文本段另按独立的 `shadow-xs` 规则登记。 |
| 表面层次 | Luma Card 用 `shadow-md`；Popover、DropdownMenu 和 Select 弹层用 `shadow-lg`；Dialog 用 `shadow-xl`。描边环、焦点环和投影分别按控件状态验证。 |

官方规则位置见 [Luma](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/registry/styles/style-luma.css)、[Vega](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/registry/styles/style-vega.css) 和 [Sera](https://github.com/shadcn-ui/ui/blob/f50887ae68d0fdcbfbe86b57be08af94a4c88a68/apps/v4/registry/styles/style-sera.css)。

## 工程边界

视觉规则在主题与控件 owner 内落地，优先复用已经可表达的 ControlTheme、模板绑定和 Token。首批以现有公开属性和资源键实现。显式应用资源、局部资源及 ThemeConfigProvider 的覆盖优先级都作为交付条件。若现有内部机制无法识别显式覆盖来源，先把阻塞和可验证的备选结构提交审查。各批使用现有工作区，分支、worktree 与提交按已约定的独立授权流程处理。

## 实施顺序与完成条件

### 1. 按钮家族与层次

- [x] 从 `controlgallery/AtomUIGallery/ShowCases/General/Button/Views/ButtonShowCase.axaml` 的 `button-shape` 重放原截图，测 Default／Primary 的常态与真实 Tab 焦点。从 `button-type` 取 Dashed，在同一真实窗口的临时验收区补 `Color=Default, Variant=Solid`。原 Luma 多余投影已复现，修复后通过八套浅深色矩阵。
- [x] 在 `src/AtomUI.Themes.Shadcn/Algorithms/Controls/ShadcnButtonTokens.cs` 整理 Button、DropdownButton、SubmitButton、ResetButton 的主题规则，由 `src/AtomUI.Desktop.Controls/Buttons/Button.Variants.cs` 解析变体外观。显式空阴影、同值、移除覆盖、切换主题，以及状态色和预设色的默认投影已跟随实际模板和资源路径验证。
- [x] 单独验证 Button 真实焦点装饰层的 `ColorFocusBorder`、`LineWidthFocus` 和偏移。真实 Tab、三层资源覆盖和透明焦点的局部像素复验均通过。
- [x] 核对 Button、DropdownButton、SplitButton 内部按钮、SubmitButton 和 ResetButton。普通 Button、带 Flyout 的 Button、DropdownButton 和 SplitButton 分开验证常态、按下和展开，沿有效变体验证 outline、secondary、ghost、link 与 destructive。AtomUI 对应的分组容器为 CompactSpace，布局回归 4 项通过。官方 Vega ButtonGroup 文本段的独立投影已登记为无对应专用部件。
- [x] Shadcn 主题与按钮族定向渲染通过。`ButtonFocusVisualRenderingTests`、实际 Gallery 的 Tab 与 App Token 覆盖均有运行时证据。八套浅深色的原样例验证结果见[实施记录](../progress/2026-11-20-shadcn-strict-alignment.md)。

### 2. 表单、卡片与浮层

- [x] Sera 的 LineEdit、TextArea、Select 只绘制下边框并保留测量占位。Luma／Rhea 的对应描边输入与选择触发器常态边框透明。Luma Select 图标使用 `muted-foreground`，同实例浅色切深色已复拍。
- [ ] 对照其余表单控件的厚度、圆角、内容间距、填充、描边和状态。
- [ ] 复核 `ShadcnInputTokens.cs`、`ShadcnCardTokens.cs`、`ShadcnOverlayTokens.cs`、`ShadcnMenuTokens.cs`、`ShadcnDialogTokens.cs` 与 `ShadcnDrawerTokens.cs` 的 owner 路由。Card、Menu、各类 Picker 弹层、Tooltip、Popover、Dialog、Drawer 分别消费其官方层次。保留有箭头浮层的一体描边和已有布局适配，验证两个弹出方向、菜单嵌套、阴影外沿及边缘焦点。
- [ ] 验证单行与换行输入、打开与关闭弹层、鼠标命中、键盘操控及遮罩状态。复用现有输入底色、TimePicker、NavMenu、Spinner 验收数据；当同一状态或 Token 有改动时重放对应样例。

### 3. 暗色状态、尺寸与扩展

- [ ] 将暗色审查中的 Steps 非当前项悬停、Card／Upload／DataGrid 链接、Tag 预设及农历辅助文字纳入差异清单；已改善的输入底色、ComboBox、TreeView、Calendar、CheckBox／RadioButton 一并对照上游。以实际底色、前景、标记和描边的组合判定状态。上游使用标记表达选中时，测试标记在真实点击后出现并保持可见。
- [ ] 按组件核对密度、字号字重、间距、圆角和按压反馈。Sera 大写及加宽字距写入组件对照表，现行文本能力与行为分别验收。AtomUI 独有控件采用其最接近的表面、文字、控件与反馈语义，在清单中标为扩展。
- [ ] 读清取色与对比的冲突。实际背景上，普通可读文字按 4.5:1、需要传递状态的图标和轮廓按 3:1 检查，按实际组合色取样。若官方规则与 AtomUI 场景的辨识要求发生冲突，记录来源、实测和拟议的本地适配边界，交由用户审查。禁用态单独检查辨识和交互反馈。

## 验收与收尾

- **矩阵**　默认配方做八种风格 × 浅深色的 16 组。首批在 Luma、Vega 的浅深色作原生补测；后来按风险补 Sera 单边边框及 Luma／Rhea 弹层。补一组非 Neutral 中性色、一组彩色品牌和既有 Compact 的代表回归。复用现有主题色轴测试与暗色审查结果。
- **证据**　在实际 Gallery `atom:Window` 使用真实指针和键盘脚本。为每个样例记录官方规则、实际 Token、有效状态、截图区域及结论。比较常态阴影外沿、边框、焦点环、填充和文字，使用同状态复拍及 1 DIP 扰动恢复校准。CSS 与 Avalonia 的抗锯齿差异按区域记录。
- **兼容**　以已有 Ant Design、PrimeNG 等主题的定向场景作前后对照。公开接口、语义部件、配置优先级和实际交互契约作为回归条件。需要调整公开契约的方案单独提请审查。
- **交付**　每批独立提交用户审查所需的代码差异、定向测试和可定位的证据结论。按实际覆盖逐项标为对齐、AtomUI 扩展或待核验，汇总在一份验收记录。临时驱动、截图及临时用例按项目规则清理。
