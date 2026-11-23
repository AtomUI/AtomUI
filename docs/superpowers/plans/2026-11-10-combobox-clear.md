# ComboBox IsAllowClear 修复计划与验收记录

> 在当前分支内执行，不创建分支或 worktree，不提交。任务结束时进行一次独立代码审查。

**Goal:** 修复 #533 已有 `IsAllowClear` 属性的实际功能，保留原下拉、编辑、过滤、绑定和主题行为。

**Architecture:** 复用 InputClearIconButton；ComboBox 仅从现有 SelectedIndex、Text、IsEditable 投影 CanClearValue，不新建选择模型。ControlTheme 声明按钮、hover/focus/disabled 显隐；点击通过 SetCurrentValue 清空既有状态。新增 clear 语义部件及生成专用 Style，按既有语义契约同步 Gallery 和文档。

**Tech Stack:** Avalonia 12.1.5 / AtomUI 6.2.13 / net10.0。

**Spec:** [Issue #533](https://github.com/AtomUI/AtomUI/issues/533)、下述有限行为契约和现有 ComboBox 文档。

## 范围与契约

- IsAllowClear 默认 false，不改变现有默认布局；true 且有选择，或编辑态有非空文本时，hover/焦点内显示清除按钮；禁用、无值或关闭清除时隐藏。
- 清除后 SelectedIndex=-1、SelectedItem=null、Text 为空，双向绑定持续有效；过滤跟随现有 Text -> FilterValue 流程清理，不另写过滤状态机。
- 原箭头继续可用；清除不误开/切换下拉，已打开/钉住的弹层状态保持原状态；焦点回到现有输入目标。
- 保留 340 宽度、原内容/过滤条件、下拉滚动和 Popup 宿主；不处理 #534，不改依赖、共享基础设施或用户其他修改。
- 处理空字符串选项、null 项和未匹配编辑文本；不通过把清空文本重新匹配成选项而留下选择。
- 模板替换时解除旧按钮处理，重新挂接新按钮；不引入全局订阅、计时器或反射生产路径。
- clear 使用专用生成 Style 声明式定制；已有 Semantic Part 的 route/类型/身份不变。

## 工作与验证

- [x] 临时精确测试：既有失败 demo 证据已确认；新增临时用例覆盖实际点击、状态矩阵、空文本选项、绑定、禁用和模板替换，先执行红证。
- [x] 最小控件修复：ComboBox.cs、ComboBoxTheme.axaml、ComboBox.SemanticParts.cs；不拆文件、不改 ComboBoxHandle。
- [x] 同步现有 Semantic 测试、Gallery 声明式预览及本地化说明、控件 overview/implementation/semantic-part/changelog；不改发布 CHANGELOG。
- [x] 临时测试转绿、原生 demo 清除与下拉脚本、Headless+Skia 清除/几何/焦点脚本与截图审查；Light/Dark 和现有尺寸取直接相关代表场景。
- [x] 独立审查；处理有效问题，删除仓库内临时测试、一次性探针与取证媒体；用户要求的临时 demo 脚本保留在项目外，运行最终 `iterate --path src/AtomUI.Desktop.Controls/ComboBox --path controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox`、成员布局审查和 diff 检查；保留用户要求的可操作 demo。

## Review Focus

1. 清空 Text 时 Avalonia 的文本匹配不能选回空字符串/null 项。
2. SetCurrentValue 必须保留 SelectedItem/SelectedIndex/Text 的应用绑定。
3. IconButton 的点击不得向 ComboBox 冒泡成开关下拉，清除不能关闭钉住弹层。
4. clear 语义 Style 必须真正命中模板节点，Gallery 不得采用 code-behind 属性回退。
5. 新按钮模板重应用不能残留旧 Click 处理，禁用和焦点行为须保持一致。

## 已有证据与当前基线

- 当前 HEAD：8128dd8c90fb602446e21130de3c0ab99e435c16；当前分支 release/6.0，有其他任务改动；ComboBox/Input/Select 范围原无改动。
- 系统临时 demo：`/private/tmp/atomui-issue-533-demo-GjRFoA`。既有 --verify：9/9 无 clear 部件，5 个有值案例失败，TextBox/Select 正对照点击通过。
- demo 宿主已改为 AtomUI Window；原生 --verify-popup：9/9 打开、选项点击、关闭通过。它修正的是 demo 宿主，不是 #533。
- 本地 Avalonia 参考：`/Users/chinboy/Projects/ReferenceProjects/Avalonia`；ComboBox.TextChanged 会按文本匹配项，清除必须避开空字符串重选。
- 生产绑定库存：Popup pin relay 因 internal 成员不可由编译 AXAML 访问；根画刷 relay 用 LocalValue 压过帧状态机；容器运行时创建。现有这些绑定无等价主题迁移，本次不新增代码绑定。

## 执行结果

- 修复前临时类 13/13 因缺少 PART_ClearButton 失败；首次完整清除接线后 13/13 通过，补充 SelectedIndex 双向绑定后 14/14 通过。
- `dotnet test ... --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ComboBox|FullyQualifiedName~Issue533ClearTests'`：83/83 通过（69 项既有 + 14 项临时），4 秒。
- 后缀按钮的 TemplatedParent 为 null，逻辑祖先为 StackPanel -> AddOnContentPresenter -> AdaptiveSpacingDockPanel -> AddOnDecoratedBoxContentFrame -> DockPanel -> AddOnDecoratedBox -> Panel -> ComboBox；CanClearValue=True、IsPointerOver=True 时普通 /template/ 选择器仍不命中。改由根主题设置样式状态，再以编译绑定（Template 优先级）传递，不增加代码绑定。
- Gallery `dotnet test tests/AtomUIGallery.Tests/AtomUIGallery.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~ComboBox`：8/8 通过，5 秒；专用 clear Style 的类型、Setter、实际可见命中及无 code-behind 回退均有断言。
- 独立审查发现唯一 P2：Template=null 时旧按钮处理未解绑。已在恢复模板前触发旧按钮取得失败证据，再由 OnTemplateChanged 即时退役旧按钮；最终临时测试 14/14 通过，1 秒。未收到其他有效审查问题。
- 临时测试类及专用 Model 已删除；未新增永久测试方法/类，扩展既有 Semantic 与 Gallery 契约断言。
- 第一次清理后 iterate：102 项 Desktop + 8 项 Gallery = 110 项通过，75.794 秒；包含现有 AOT 成员保留、Popup 清单和溢出提示检查。
- 额外 Small 视觉检出清除后 24 -> 22 高度差。IsAllowClear=false（通过原有属性清空）与 true（点击按钮）均复现；根因为 Small 选中内容有上下各 1 的 margin，而占位符缺失。对占位符使用相同 margin 后，两项临时用例均通过（623ms），不设置固定高度、不改占位色。
- 新增 Small 临时测试已删除，后续验证仍使用原参数化 demo；不保留永久复现类。
- 默认 Auto Width 风险检查：有值但未 hover/聚焦时宽 110，hover 后新按钮使宽度变为 132。临时用例确认失败后，启用清除时以静态 AXAML 槽预留 SharedToken.IconSize；关闭清除时槽退出布局。相同用例已通过，断言 hover 前后 Bounds 与箭头位置均不变；临时类已删除。
- 默认原生后端：9 组下拉打开/选择/关闭、9 次清除，异常 0。默认 Headless+Skia：9 个真实 clear 部件，5 次有值清除、2 次真实文本输入后清除通过；框架、焦点与弹层状态保持；TextBox/Select 两个正对照通过。
- 最终参数化视觉脚本：默认 Light/Middle/Outlined、实际 Dark/Small/Filled、Light/Large/Borderless 均通过。每个脚本完成 9 组部件/hover/焦点检查、5 次有值清除、2 次框架真实文本输入后清除及 TextBox/Select 正对照；missingClearParts=0、selectedContractFailures=0。Small 前后 340×24 保持，Large/Borderless 前后 340×38 保持。
- 最终原生脚本：9 次打开、选项点击、关闭与清除，exceptions=0，输入焦点保持；实际后台为 Avalonia.Native.WindowImpl。原生截图采用 RTB 即时渲染，不称为原生合成器截屏；Skia 截图使用 CaptureRenderedFrame。已查看清除按钮/下拉、清除后状态及 Dark/Small、Large/Borderless 关键图，未用图像缩放或遮罩掩盖问题。
- 初次 `--dark` 只改 Avalonia RequestedThemeVariant，实际仍为 Light，未计为深色证据。最终通过 ThemeManager.ApplyThemeAsync(Default+Dark) 切换并断言 ActualThemeVariant=Dark。
- Windows 原生与 NativeAOT 发布未执行；未改变依赖、平台或注册后端。最终清理后 iterate 结果见交付说明及 `.artifacts/verification/latest.json`。详细临时日志不复制到其他永久文档。
- 本次仅作功能修复，无性能优化声明；按 control-performance 核对静态 AXAML 节点、绑定优先级与事件退役，无代码动态视觉、全局订阅或忽略状态标记。

## 精确执行入口

历史临时用例（验证后已删除，合计 17 个参数化 case）：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Issue533ClearTests
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~SmallClearGeometryTests
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AutoWidthClearGeometryTests
```

最终清理后模块与结构检查：

```bash
python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/ComboBox --path controlgallery/AtomUIGallery/ShowCases/Navigation/ComboBox
python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/ComboBox
git diff --check
```

可重放视觉入口（用户要求保留的项目外临时 demo，默认不输出媒体）：

```bash
dotnet run --project /private/tmp/atomui-issue-533-demo-GjRFoA/Issue533Demo.csproj --no-build -- --verify-popup
dotnet run --project /private/tmp/atomui-issue-533-demo-GjRFoA/Issue533Demo.csproj --no-build -- --verify
dotnet run --project /private/tmp/atomui-issue-533-demo-GjRFoA/Issue533Demo.csproj --no-build -- --verify --dark --size=Small --variant=Filled
dotnet run --project /private/tmp/atomui-issue-533-demo-GjRFoA/Issue533Demo.csproj --no-build -- --verify --size=Large --variant=Borderless
```

## 最终交付状态

- 最终清理后的 iterate 为 iteration-passed：Desktop 102/102（6.395s）、Gallery 8/8（7.855s），总耗时 73.395s；范围仅两个显式模块路径，不称为全量回归。
- 成员布局：8 个文件、3 个控件，问题 0；git diff --check 通过。
- 最终原生脚本及 Dark/Small/Filled、Light/Large/Borderless 脚本全部退出 0；关键清除前后图片已查看。原生为输入与 RTB 渲染证据，Skia 为合成器帧，二者不混为同一像素基线。
- 全部 17 个仓库内临时 case 和专用 Model 已删除；永久测试方法/类净新增为 0，仅更新既有语义契约断言。用户要求的系统临时 demo 与可重放脚本保留在仓库外。
- 所有取证媒体已查看并清理，审计无仓库二进制发现；自动验证进程均已退出。交互 demo 按此前请求保持运行。
- 未创建分支、worktree 或 commit；其他任务的修改保留。Windows 原生与 AOT 发布未运行。

## 用户追加的间距与系统排查

用户反馈红框内清除/箭头间距浪费，并要求系统性排查。已按原生真实按钮 bounds 测得 8 DIP；TextBox 的相邻操作为 4 DIP，Select 采用互斥重叠槽。ComboBox 保留独立下拉入口，将两按钮放入独立操作组用 SpacingXXS=4，外层用户后缀/Form 反馈仍为 SpacingXS=8；不修改共享控件/全局 Token。clear 的新语义路由同步新增私有 actions scope，已有 indicator/suffix 路由不变。

有限审查：两主题 × 三个标准尺寸 × 四个现有 StyleVariant（含 Underlined）=24 组；同实例检查值/空值、hover/焦点、禁用、定宽/自动宽度、4 DIP 内间距、图标中心/边缘命中。另检查两主题中的用户后缀与 Form 反馈分组间距。功能继续复用原生 9 组与既有模块验证；不另建验证框架或新项目。

### 系统排查结果

- 原生布局红证：清除/箭头控件 bounds 间距为 8 DIP，4 DIP 断言失败；操作组拆分后原生 9 个案例逐项实测均为 4 DIP，清除/下拉全部通过。外层后缀与反馈仍按原层级使用 8 DIP。
- 同一 ComboBox 实例完成 Light/Dark × Small/Middle/Large × Outlined/Filled/Borderless/Underlined 共 24 组；每组均检查清除前后定宽尺寸、Auto Width 与箭头位置、两个按钮各左/中/右命中（6/6）、禁用点击、焦点与下拉保持。24/24 通过。
- 外层两主题场景通过：公开 FormValidateFeedback 配置标准尺寸成功图标后，反馈实际宽 14；用户后缀/反馈与反馈/操作组分别测得 8/8 DIP。驱动最初未提供 SuccessFeedback 内容，未物化的场景未计为通过；修正驱动后完整重放 24 组及两组外间距。
- 已打开并审查两主题各 Small/Filled、Middle/Outlined、Large/Borderless、Large/Underlined 共 8 张代表图及原生 4 DIP 图。不同 StyleVariant 沿用现有边框盒尺寸，逐项确认 hover/清除不导致尺寸改变，不擅自改动共享样式尺寸规则。
- `--audit-layout` 参数化入口保留在项目外 demo，可一条命令重放，不新增测试项目或永久验证框架。
- 最终 iterate 再次通过 102+8=110 项，总耗时 78.848s；成员布局 8 文件/3 控件问题 0，diff 检查通过。截图已查看后清理；不提交。
