# NumericUpDown 清除按钮几何重叠根因修复方案

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 从布局源头消除 IKLPBR 中浮动 Handle 出现时，suffix 与文本编辑区的几何重叠，保留现有后缀运动轨迹和控件外部尺寸。

**Architecture:** ButtonSpinnerDecoratedBox 继续拥有 hover、Handle 和侧内容偏移的动画状态。NumericUpDown 的输入区域用一个内部专用 DockPanel 接收共享动画偏移，在同一次 Arrange 中同时定位 suffix 和收窄文本编辑区；suffix 不再独立使用 RenderTransform。现有数值、文本、Form 和 Semantic Part 契约保持。

**Tech Stack:** Avalonia 12.1.3、.NET 10、AXAML、现有 Avalonia 属性绑定与布局基础设施。分析源码 HEAD：731dfdaaa。

## 方案状态与已验证证据

本文件取代上一版以 ZIndex 为最终实现的方案。用户要求从源头修复，因此最终目标由“让按钮优先命中”提升为“按钮与编辑区不存在错误的交互区域重叠”。ZIndex A/B 仍是有效诊断证据，但不是本文待实施的根因修复。

实现已落地：共享偏移动画驱动输入区布局，统一舍入动画中的布局边界，Small 避让按手柄宽度计算；按用户补充要求，Spinner 操作按钮按实际高度保持正方形。最终定向验证和清理结果见文末。

已完成的调查：

- 读取 Gitee AtomUI #IKLPBR 正文及三张附件。报告日期 2026-10-14，反馈版本 v6.2.4；分析日期 2026-10-18。
- 本地 v6.2.4 tag 与当前源码的 NumericUpDown.cs、NumericUpDownTheme.axaml 无差异。运行证据来自当前源码，不等价于运行发布的 NuGet 包。
- 临时 Headless 测试保留 Value=1、IsAllowClear=true、120×32 输入控件和浮动 Handle，通过 raw pointer MouseMove/MouseDown/MouseUp 复现，未直接 RaiseEvent(Click)。
- 静态几何诊断关闭 motion 以隔离最终状态；默认动画原生窗口尚未验收。

| 模式 | suffix ZIndex | suffix shift | 点击结果 |
| --- | --- | --- | --- |
| Input | 原始 0 | -16.5 | 命中 ScrollContentPresenter，Click=0，Value 仍为 1；目标清空断言失败 |
| Input | 仅改为 1 | -16.5 | 命中按钮后代，Click=1，Value=null、Text 为空、按钮隐藏 |
| Spinner | 原始 0 | 0 | 清除正常 |
| Spinner | 仅改为 1 | 0 | 清除正常 |

输入框横向客户区范围为 [12,94]；清除按钮由中心 (101,16) 平移到 (84.5,16)，其 14×14 的按钮横向范围变成 [77.5,91.5]，完全落入输入框区域。已检查祖先 bounds、ClipToBounds、Clip、IsHitTestVisible 和 ZIndex，未发现按钮被祖先裁掉。

上一轮清理临时诊断后，NumericUpDown 模块现有 9 个测试类、91 个用例通过。这证明原有测试缺少该真实命中验证，不能用来证明下文的新方案已通过。

## 审计结论与职责边界

| 项目 | 当前状态与结论 |
| --- | --- |
| 原始错误 | suffix 渲染位置左移，文本框布局槽和命中区域保持原大小 |
| 状态 owner | ButtonSpinnerDecoratedBox 已拥有 hover、ContentRightShift 和共享 motion token 驱动的 DoubleTransition |
| 重复状态计算 | NumericUpDown.SyncSuffixGroupShift 再次按 hover/padding/floatable/location 算出同一偏移，并给 suffix 运行独立 TransformOperationsTransition |
| 布局 owner 缺口 | 内部 DockPanel 只看到变换前的子项尺寸，无法为视觉平移后的 suffix 回收文本框空间 |
| 可复用机制 | DockPanel 的单次排列、现有 ContentRightShift 动画、现有 template CompositeDisposable、现有文本 ScrollViewer |
| 不宜直接复用的类型 | ButtonSpinnerContentPanel 位于外层 prefix/suffix/content 槽，当前 suffix 在 NumericUpDown 的 Content 子树中；仅改外层面板无法直接协调内层两个兄弟节点 |
| 不宜回退的结构 | 将 suffix 搬回外层 ContentRightAddOn 会改变 Spinner 模式中 suffix 与中间输入区、左右 action 段的关系，并改变 Semantic Part 树拓扑 |
| 必要新类型 | NumericUpDownInputPanel：一个内部布局 owner，唯一新增职责是为浮动操作区回收编辑空间，复用 DockPanel 排列；不是新的输入状态机或动画系统 |
| 直接相关共享风险 | IsShowHandle=false 时，现有 UpdateHandleVisualState 的侧内容偏移分支没有直接以 IsShowHandle 为条件；hover 中隐藏 Handle 时必须核对并归零位移 |

## Global Constraints

- 外部控件宽高、padding、前后缀顺序、浮动 Handle 的正常隐藏/hover 显示、suffix 的位移终点与运动时长保持。
- hover 时文本编辑区随 suffix 左移收窄，是此次要纠正的几何契约；长文本滚动、光标和选区必须实际验收，不能声称所有内部可观察状态原样不变。
- 保留 PART_TextBox、PART_SuffixGroup、PART_ClearButton 的名称、类型及所属模板；保留 Semantic Part marker 和 route，生成的专用 Style 必须继续命中。
- 不通过提高 ZIndex、禁用文本命中、强制透明穿透、永久预留 Handle 空间或隐藏按钮来通过验收。
- 不改 Value/Text/string mode/Form 的状态模型和公共 API。
- 只保留一个现有动画源 ContentRightShift；输入布局属性不再建立第二条动画。
- 可以由 ControlTheme 表达的绑定和过渡继续放在 AXAML；跨模板内部状态确需代码绑定时，注明原因，并由既有模板作用域管理释放。
- 新测试先临时验证，默认清理；优先增强现有布局测试，不默认扩大常驻测试数量。
- 不自行提交、不更新外部 issue、不主动运行全量回归。

## 核心几何契约

设内层可用宽度 W、suffix 正常占位 S、共享动画的当前右侧避让量 d=max(0,-ContentRightShift)。先将 d 限制在 [0,W]，再使用 W-d 执行原 DockPanel 排列；返回外层原 finalSize。

忽略示意中的既有 margin 时：

```text
suffix.Left = max(0, W - d - S)
editor.Right <= suffix.Left
editor.Width >= 0
outer.Bounds = 原尺寸
```

前一轮实测场景对应 W=96、S=14、d=16.5：

| 阶段 | 文本编辑区（内层坐标） | suffix（内层坐标） |
| --- | --- | --- |
| 正常 | [0,82] | [82,96] |
| 原始 hover | [0,82] | [65.5,79.5]，由 Transform 平移 |
| 目标 hover | [0,65.5] | [65.5,79.5]，由 Arrange 定位 |

suffix 的目标位置保持，编辑器真实边界让出重叠区域。动画每一帧都用同一个当前 d 计算两者，而非分别给两者启动动画。

## Task 1: 建立可区分层级补偿与几何修复的失败验证

**Files:**
- 临时：tests/AtomUI.Desktop.Controls.Tests/Temporary/IKLPBR/
- 优先增强：tests/AtomUI.Desktop.Controls.Tests/NumericUpDown/NumericUpDownSpinnerLayoutTests.cs
- 共享状态验证：tests/AtomUI.Desktop.Controls.Tests/ButtonSpinner/ButtonSpinnerSpacingTests.cs 或相关既有行为测试

- [ ] 用原始 Input、IsAllowClear=true、Value=1、Width=120 场景复现；保留真实模板、浮动 Handle、前后缀和约束。
- [ ] 同时断言真实 raw pointer 点击清空，以及将 textbox/suffix 转换到同一祖先坐标后的编辑器右边界不超过 suffix 左边界。只改 ZIndex 的版本应仍因几何断言失败。
- [ ] 在 suffix 的左/中/右可交互区域验证 InputHitTest，实际点击检查 Value、Text、清除按钮可见性。
- [ ] 默认动画开启时，在开始、中间、结束以及快速反向进入的代表帧中检查同一几何不变量；记录分离的鼠标输入、几何和视觉结果。
- [ ] 追加 hover 中 ShowButtonSpinner true→false→true 的临时验证，确认共享动画最终归零并可恢复。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~NumericUpDownSpinnerLayoutTests
```

原始版本应因真实点击或几何断言失败；模板查找、编译和环境失败不能算故障复现。

## Task 2: 让同一个布局 owner 排列编辑器与 suffix

**Create:** src/AtomUI.Desktop.Controls/NumericUpDown/NumericUpDownInputPanel.cs
**Modify:** src/AtomUI.Desktop.Controls/NumericUpDown/Themes/NumericUpDownTheme.axaml

内部面板的核心实现如下，最终成员顺序遵守仓库内部属性与运行方法布局规范：

```csharp
using Avalonia;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

internal class NumericUpDownInputPanel : DockPanel
{
    public static readonly StyledProperty<double> RightInsetProperty =
        AvaloniaProperty.Register<NumericUpDownInputPanel, double>(nameof(RightInset));

    public double RightInset
    {
        get => GetValue(RightInsetProperty);
        set => SetValue(RightInsetProperty, value);
    }

    static NumericUpDownInputPanel()
    {
        AffectsArrange<NumericUpDownInputPanel>(RightInsetProperty);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var inset = Math.Clamp(RightInset, 0d, finalSize.Width);
        base.ArrangeOverride(new Size(finalSize.Width - inset, finalSize.Height));
        return finalSize;
    }
}
```

- [ ] 两种 NumericUpDown 模板的内层 DockPanel 替换为该面板，并赋内部查找名 PART_InputPanel。原 suffix、textbox 子项顺序及其 margin、类标记、模板关系不变。
- [ ] 删除两个 suffix 上的 RenderTransform 声明，删除 NumericUpDownTheme 中 suffix TransformOperationsTransition Style。
- [ ] 不覆盖 suffix.Margin：这是用户可通过 Semantic Style 定制的布局属性，不应被新的运行时位移写入抢占。
- [ ] 不在 Measure 中按动画逐帧追加期望宽度，不让自动宽度控件在 hover 时整体变宽。RightInset 只直接使 Arrange 失效；验证实际子树测量次数和长文本 viewport 行为。
- [ ] 验证零/极窄可用宽度没有负尺寸，现有 MinWidth、margin、裁剪和文本滚动约束仍然正确；不能为了满足公式额外隐藏内容或删除既有约束。

## Task 3: 复用共享偏移动画并删除重复计算

**Modify:** src/AtomUI.Desktop.Controls/NumericUpDown/NumericUpDown.cs
**Modify when confirmed:** src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerDecoratedBox.cs

- [ ] 在 SetupTemplatePartBindings 中取得 NumericUpDownInputPanel 和当前 spinner.DecoratedBox，使用下列单向绑定替换原四个 hover/padding/floatable/location observer。

```csharp
// The animation source is internal state of the nested decorated-box template.
// The input panel belongs to NumericUpDown.Content, so a TemplateBinding cannot
// reach this source. This scoped binding consumes the current animated value;
// the panel has no second transition.
_templatePartBindings.Add(inputPanel.Bind(
    NumericUpDownInputPanel.RightInsetProperty,
    decoratedBox.GetObservable(ButtonSpinnerDecoratedBox.ContentRightShiftProperty)
                .Select(static shift => Math.Max(0d, -shift))));
```

绑定所需 System.Reactive.Linq using 在实际文件中按现有排序补齐。该代码消费已有动画的有效当前值，不使用 Dispatcher 延后同步，不再次动画目标属性，也不逐帧创建 Transform。

- [ ] 删除 SyncSuffixGroupShift、重复的 padding*1.5 计算及其四个独立刷新订阅。NumericUpDown 不再拥有浮动侧内容的位移算法。
- [ ] 保留 ButtonSpinnerDecoratedBoxTheme.axaml 已有 ContentRightShift DoubleTransition 及共享 motion token，原生窗口确认布局与该动画采样同步，没有一帧错位或快速重入闪烁。
- [ ] 验证并在 UpdateHandleVisualState 的源头统一条件：仅在有效启用、IsShowHandle、IsHandleFloatable、IsSpinnerContentHover 同时成立时产生浮动侧内容偏移；非该状态的目标偏移为 0。不要在 NumericUpDown 再补一套条件来掩盖共享源状态。
- [ ] 新增绑定由既有 _templatePartBindings 持有，模板重建先释放旧绑定；绑定双方均为当前模板内部节点，无全局资源/输入事件订阅。验证 detach/reattach、Mode 切换及旧面板不再收到新模板更新。

## Task 4: 验收、文档与清理后验证

**按需更新：**
- docs/controls/desktop/data-entry/numeric-up-down/implementation.md
- docs/controls/desktop/data-entry/numeric-up-down/changelog.md
- docs/controls/desktop/navigation/button-spinner/implementation.md（仅共享状态条件确有修改时）

| 验收点 | 关键判据 |
| --- | --- |
| 原始场景 | 默认层级、原窄宽度、hover 后按钮左中右命中并能清空，textbox 与 suffix 交互边界不重叠 |
| 动画与快速反向 | 默认 motion 的代表帧没有边界重叠；suffix 轨迹/终点及 Handle 视觉保持；不开第二条动画 |
| 长文本与输入 | 无新遮挡、原文本可编辑与选择、光标滚动 viewport 正确，原高度与宽度不变 |
| 自适应尺寸 | Small/Middle/Large、Custom、无显式宽度、窄宽度、后缀及 Semantic Style margin 定制不被覆盖 |
| 状态切换 | 空值、只读、禁用、关闭清除、hover 中切换 ShowButtonSpinner，最终几何与可见状态一致 |
| Spinner 模式 | 无浮动偏移；左右 action、中央输入、prefix/suffix 位置和原间距保持 |
| 主题与模板 | Mode 重建、主题/Token 改变、detach/reattach 后仍使用当前共享偏移；专用 Semantic Style 持续命中 |
| 性能与生命周期 | 采样布局次数/分配，确认没有重复动画、每帧 Transform 分配、无意义全树重测或旧模板更新 |

- [ ] 文档记载“共享偏移状态 + 单一输入布局 owner”的长期契约，不写成调高层级的使用建议。
- [ ] 优先增强已有用例，删除临时矩阵、几何 dump、辅助工程和取证媒体；如额外保留常驻测试，说明独立检错、长期收益、全局运行必要性和成本。
- [ ] 清理后按实际修改的两个模块执行定向验证，不扩大全量。

```bash
python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/NumericUpDown --path src/AtomUI.Desktop.Controls/ButtonSpinner
git diff --check
```

- [ ] 提交前交用户审查；未完成原生动画和原始交互验收，不宣称正式修复完成。

## 本轮方案交付边界

本轮只复查源码并更新方案。上一轮静态 A/B 证明原始重叠与层级的因果关系；本文新增的布局容器、共享动画绑定及状态修正均待实现和验证。生产源码及既有测试没有改动，没有创建提交，没有新增临时测试或取证媒体。

## 2026-10-18 实施与验收结果

- 修复前：临时原始场景 6 个用例，4 个 Input 几何场景和 hover 中隐藏 Handle 共 5 个按目标故障失败；Spinner 对照通过。正方形按钮临时用例 4 个全部失败，原 Small 按钮 36×22，Middle 36×30，Large 36×38，Custom 36×46。
- 修复后临时验证：11/11 通过，涵盖三尺寸点击、无重叠、方形按钮、自定义高度、81 个分数位移布局帧、模板更换解绑和 detach/reattach。临时测试源已删除；既有测试增强几何、真实指针、分数位移舍入和按钮形状断言，常驻测试数净新增为 0。
- 原生视觉验收：macOS、RenderScaling=2、窗口客户区620×460、源码构建的临时应用包；默认动画开启。Small/Middle/Large 的清除实际点击均清空，编辑区与后缀重叠为0；Headless另验证按钮左中右完整命中。
- Small 原生 Spinner 两个按钮均22×22，实际点击增减为1→2→1；Middle为30×30。Large/48自定义高度的正方形契约另由临时Headless验证。
- 原生拖拽文字选区0→14，文本可正常选择；快速移出/移入后采样59个Input动画帧，最大编辑区与后缀重叠为0。
- 已打开并检查最终两张原生截图：Small清除图标完整显示，与浮动箭头区保持间距；Small左右按钮正方形、图标居中。原始视觉前证来自用户现场截图和此前失败复现；修复前原生自动脚本未完成，不能将其记为已通过。
- 原生脚本结果：NATIVE ACCEPTANCE PASS。验收窗口已停止，不再占用鼠标。
- 范围：实际运行macOS原生验收；未运行Windows/Linux原生窗口或NativeAOT发布，不据此推断跨平台专项通过。

### 最终清理后验证

- `python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/NumericUpDown --path src/AtomUI.Desktop.Controls/ButtonSpinner`：iteration-passed，12个相关测试类、107个用例全部通过；未运行全量。
- 控件成员布局审查：明确检查 NumericUpDown.cs、NumericUpDownInputPanel.cs、ButtonSpinnerDecoratedBox.cs，3个文件、3个控件，0项问题。既有成员仅按仓库强制规则调整位置，访问级别与初始化语义保持。
- `git diff --check`通过；取证审计 findings=[]。原生窗口已关闭，临时测试、Demo、截图、脚本和编译缓存按任务范围清理，生产改动待用户审查，未提交。
