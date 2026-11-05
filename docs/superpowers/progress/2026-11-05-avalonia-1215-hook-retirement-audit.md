# Avalonia 12.1.5 hook 退役复核

日期：2026-11-05。范围：评估当前 Avalonia 私有成员桥、平台 hook 与兼容覆写是否可删；本轮不改产品实现、依赖或生成文件，不创建 commit。

## 结论

1. **本次 12.1.4→12.1.5 新增等价覆盖**：Dispatcher 共享 Stopwatch 的补丁，当前 HEAD 已删除，无需重复处理。
2. **额外发现可退役的历史兼容补丁**：`src/AtomUI.Desktop.Controls/Input/InputTextPresenter.cs`。上游选择前景缓存修复已包含在 12.1.4 与 12.1.5；本轮用 12.1.5 实际程序集验证原生 presenter 与当前补丁输出等价。此前升级表对此类的“保留”建议需要更正。它通过继承补行为，不是反射 hook，但属于本次应审视的框架兼容代码。
3. 其余已核对的平台 hook 与私有成员桥，未找到被本次上游变更等价替代的证据。不能把“同一个控件修了 bug”“私有字段还在”或“现有测试绿”单独当作删除依据或保留的全部依据；以下按原有职责说明。

## 来源与方法

- 按仓库本地优先顺序实际查找：项目 `.referenceprojects`、项目/父目录 `ReferenceProjects` 未命中，采用本地 `/atomware/workspace/projects/ReferenceProjects/Avalonia`。
- 精确比较 `2d79ba55fae1b1e71c69f93a668080c9029e6a31`（12.1.4）与 `5825bf8c66fba0c11e67c980d8ece6444576bffa`（12.1.5）。使用 tag blob/diff，不依赖参考仓库当前 checkout。完整 339 条处置沿用[升级评估](../specs/2026-11-05-avalonia-12.1.5-upgrade-design.md)。
- 扩大搜索至 `Reflection`、私有字段/方法访问、WndProc callbacks、native input regions、渲染接口包装、兼容注释及自建 Menu/TextPresenter 行为；检查新增 public/protected 声明是否匹配现有私有访问。DataGrid 的用户数据成员反射、source generator 和构建工具自己的反射，不等同于运行时 Avalonia hook。
- 对读到的每类边界追踪使用方和原职责；对于 InputTextPresenter 的反例，进一步读取引入历史、上游修复生产 diff，并以临时外部程序集进行对照。

## InputTextPresenter：建议退役的依据

上游修复提交 [1476e73a0d00539d6aed9ef4fb4fcaaa903fb9fa](https://github.com/AvaloniaUI/Avalonia/commit/1476e73a0d00539d6aed9ef4fb4fcaaa903fb9fa) 改动实际 `TextPresenter.OnPropertyChanged`：

- SelectionForegroundBrush、ShowSelectionHighlight 变化时，调用完整 InvalidateTextLayout，清 run cache。
- SelectionStart/SelectionEnd 变化且 SelectionForegroundBrush 非空时，同样清 run cache；为空时保留 shaping cache。
- CreateTextLayout 仅在 ShowSelectionHighlight、非空选区、非空 SelectionForegroundBrush 同时成立时创建选区 foreground override。因此 foreground 为空时，并无需要随选区移动而替换的选区前景样式。

`git merge-base --is-ancestor` 确认该提交是两个 tag 的祖先；对应 [12.1.4 OnPropertyChanged](https://github.com/AvaloniaUI/Avalonia/blob/2d79ba55fae1b1e71c69f93a668080c9029e6a31/src/Avalonia.Controls/Presenters/TextPresenter.cs) 与 [12.1.5 OnPropertyChanged](https://github.com/AvaloniaUI/Avalonia/blob/5825bf8c66fba0c11e67c980d8ece6444576bffa/src/Avalonia.Controls/Presenters/TextPresenter.cs#L1092) 均含上述逻辑。两 tag 的此文件 diff 只公开 CursorRectangle、移除旧 internal getter；这并不支持“原有补丁仍必要”，只能说明它不是此次 patch 才被覆盖。AtomUI 补丁引入于 `5fcb6d19f`，目前先调用 base，再无条件对四个属性重复完整失效。

### 临时对照结果

- 系统临时目录中的外部 net10.0 Release 探针，使用当前 Debug 构建的真实 `InputTextPresenter` 与 Avalonia 12.1.5 原生 `TextPresenter`，没有复制一个自制 shim 冒充产品实现。利用现有 `AtomUI.Performance` friend assembly 访问 internal 类；不修改产品或生成内容。
- 实际加载的 Desktop DLL SHA256：`a59b90baa69cb3dab188b4db332150a874841eac8fe1117776dc3139370f4af3`；复制后比对当前产物与运行时记录一致。Skia + Headless、DejaVu Sans 18、400×120 DIP。
- **281 个 A/B 状态检查无差异**：比较 TextLayout 宽高、TextRun 文本及 foreground、PNG 像素。覆盖英文、阿拉伯/中文混排、换行、emoji/组合字符，白/红/null 前景，移动/反向/清空/全选，高亮切换、密码/reveal、普通前景变化及 preedit。另独立断言 ASCII 选区 1..5 的文本 `elec` 确实为红色，避免只比较两个同样错误的结果。
- 非空选区前景下，四种单独属性变化的完整失效调用分别为：原生 1、当前补丁 2。说明存在重复调用；失效是惰性操作，**不据此宣称 shaping 两倍或页面性能提高某个百分比**。前景为空时原生保留 cache，也是可收回的无条件失效成本；本轮未给其时间/分配收益定量。
- 临时工程 build 成功；工具使用旧 Bitmap.Save overload 产生 1 个 CS0618 warning，0 error。进程退出 0，Failures 为空、stderr 为空。未运行已陈旧的整套性能工具，也不以其“presenter 必须是 InputTextPresenter”的类型断言否定等价实现。

### 后续删除的完整范围（本轮未执行）

- 删除该 internal 类，六个模板改用 Avalonia TextPresenter，并保留 Name、Classes、Bindings 和 Semantic Part：`Input/Themes/{TextBoxTheme,LineEditTheme,SearchEditTheme,TextAreaTheme}.axaml`、`ComboBox/Themes/ComboBoxTextBoxTheme.axaml`、`OtpLineEdit/Themes/OtpTextBoxTheme.axaml`。
- `OtpLineEdit/OtpTextBox.cs` 的 internal TemplatePart 元数据、字段和 NameScope 查询改用 TextPresenter；保留其自绘 caret、占位和焦点逻辑。不能只删 Input 下四个引用而漏掉 ComboBox/OTP。
- Semantic Part 的公开 ContractType 已是 TextPresenter（LineEdit/SearchEdit/TextArea 及其 AutoComplete、Mentions 消费链），保持专用 Style 类名、Setter 与命中行为。更新三个 SemanticPart 测试中固定内部实际类型的预期，由生成器重建真实 selector，不手改生成文件或写 code-behind 样式回退。
- 性能工具的 CountingInputTextPresenter 改为基于原生 presenter 的契约验证；去掉“必须使用补丁类型”的断言，保留选区前景、高亮、资源绑定和正确像素断言。同步 input/search-edit/mentions 等文档中的内部节点描述与旧 workaround 说明。
- 执行前/后保持选区颜色、密码、preedit、caret 和重挂载场景；定向验证 Input、ComboBox、OtpLineEdit、AutoComplete、Mentions 与相应 Semantic Part；新验证默认临时并清理，最终按实际改动路径 iterate，不运行全量。当前 281 项只是 presenter 原语对照，**不冒充替换六个产品模板后的完整集成验收**。

## 其余边界的去留

| AtomUI 边界 | 建议 | 精确版本源码及未被覆盖的职责 |
| --- | --- | --- |
| DispatcherReflectionExtensions / TryShareManagedClock | 已删除 | Dispatcher.cs / Dispatcher.Timers.cs 新增连续 Now 与 dueTime 转换，上一轮正对照已验证；本轮确认 HEAD 无此文件。保留平台就绪后启动契约。 |
| Linux Rendering/WaylandFramePacing*、prepared-frame、presentation bridge | 保留 | WaylandWorker、WaylandEglWsiSurface、WaylandGlobals 两 tag 相同；WSurface diff 只改 Popup configure payload。上游仍按自己提交路径申请 frame callback，未绑定 AtomUI 的 wp_presentation 预测，也未替代包装器的 prepared-frame 提交/回调所有权。Dispatcher 修复不是呈现预测功能。 |
| Linux/WaylandInitialScaling | 保留 | WindowImplBase 仍以 RenderScaling=1 初始化；OutputsTracker/WorkerClient/Globals 相同。新增 Position/坐标转换不提前提供首帧真实 scale。当前按契约绑定已替代旧版本白名单，不能再把“可删白名单”算作新工作。 |
| Linux/WaylandPopupShadowInputRegion、WaylandWindowReflectionExtensions、X11PopupShadowInputRegion | 保留 | WSurface.ApplyInputRegion 仍只有整窗无限/空区域，新增 Popup.Position 与坐标通知不提供按内容形状的 native input region；X11 对应输入形状能力未被此次渲染/后台调度修改代替。 |
| GridMeasureCacheCompatibility | 保留 | Grid.cs blob 相同；measure scratch 的跨 Grid 保留关系没有变为 measure-finally 清理。WindowBase 新增重测不能保证所有 Grid 都进入 Arrange，更不清理 measure-only 缓存。当前 shim 自带行为探测，未来满足清理条件会不接入。 |
| BorderReflectionExtensions.ReleaseDetachedCompositionCache / StyledElementReflectionExtensions.ReleaseTemplatedParentTheme / Flyout 清旧目标 | 保留 | Border、Visual、StyledElement、PopupFlyoutBase 两 tag 相同；释放 discarded Popup 祖先链、旧 theme/last target 的职责仍存在。Skia Path Bounds/stroke cache 修复不释放这类宿主引用。 |
| PopupLightDismissRegistration | 保留 | LightDismissOverlayLayer blob 相同；Popup.Open 仍仅在 open 时 IsLightDismissEnabled 为真才取得 lease，相关属性变化没有重建/释放该 lease 的新路径。指针锚点缓存/scaleY 修复不提供运行中 pin/unpin/pass-through 切换。 |
| DefaultMenuInteractionHandler 的本地延迟意图管理 | 保留 | 新上游 _closeCount/DelayRunWhileOpen 仅阻止跨 menu close 的旧回调。本地 _openIntentGeneration/_closeIntentGeneration 还处理同一打开会话中的目标切换、owner detach、键盘/access key、捕获与 sibling 意图，不可整类改回基类或只留关闭计数。 |
| WindowsBorderlessStyleHook | 保留 | WindowImpl.SetFullScreen 在 12.1.5 仍 `SetStyle(current & ~WS_OVERLAPPEDWINDOW, false)`；未补 WS_POPUP。新触摸 drag/Alt+Space 处理与 redirection surface 创建前的 style 补全不同。 |
| WindowsLegacyFrameMarginHook | 保留 | WindowImpl.UpdateExtendMargins 仍 `defaultMargin = UseRedirectionBitmap ? 1 : 0`。Win10 边框修正并未被新增托管 measure 覆盖。 |
| WindowsCsdSizingHook | 保留 | Window 的首次原生约束同步和 WindowBase.ArrangeCore 重测，不替代 WM_GETMINMAXINFO 的 Dialog track bounds 及用户 resize 生命周期。WndProc.cs/CustomCaptionProc.cs blob 相同，AppWndProc 新变更没有补上述职责。 |
| WindowsBackgroundHook | 保留 | 首显客户区底色、live resize 避免 GDI 擦除闪烁的策略仍未被改动；触摸 move/resize 的起动等待不等于填充/擦除时序修复。 |
| WindowsPopupShadowHitTestHook / MacOSPopupShadowPassThrough | 保留 | WM_NCHITTEST+跨进程 pass-through 和 NSWindow ignoresMouseEvents 管理不同于单纯 IsHitTestVisible。Windows 输入修复和 native automation.mm 变化不提供内容形状的阴影穿透。 |
| PixelAlignedBorder / WindowVisualLayerClip | 保留 | Skia 几何缓存变更不改变 layout/clip 分数像素量化；Wayland render target 和缓存提交路径不变，未证明末端 buffer 分配与 clip 完全同尺度。PixelAlignedBorder 还是 public 类型，删除另有 API 影响。 |
| GlobalClockReflection / MotionFrameDriver | 保留 | IGlobalClock、MediaContext 与动画帧时来源仍原样且接口 internal。Dispatcher.Now 连续不提供同一 GlobalClock 的公开等价入口，不能随旧共享 Stopwatch hook 一并删除。 |
| Popup、ComboBox、TextBox、ContextMenu、Flyout 固定成员桥 | 保留现有用途 | Popup.Closing/SetPopupParent 仍 internal、HandlePositionChange 仍 private。ComboBox diff 仅捕获/导航，_popup/_inputTextBox 未公开；其他 owner blob 相同。注入 AtomUI host、默认目标清理与内部输入/撤销功能未被新 API 替代。 |
| TopLevel/VisualLayerManager、WindowDrawnDecorations/ResizeGrip、ScrollBar/ScrollContentPresenter、Items/RawPointer/Focus/Visual/AvaloniaProperty 等桥 | 不按本次升级直接删除 | 对应 owner 源码相同，私有 setter/宿主与内部路由仍无新公开等价入口。个别通用 bridge 的存量死代码可另做调用/外部 friend 使用审计，但本轮不凭本仓单次 rg 无命中就删除。 |
| AnimatableReflectionExtensions.EnableTransitions/DisableTransitions | 保留 | Animatable blob 相同；这是 public AtomUI legacy API。当前本仓少用不等于可删除。 |
| TextPresenter.CursorRectangle 新公开属性 | 无对应 hook 可删 | 全仓没有旧 GetCursorRectangle/_caretBounds 访问。该公开 API 与选区缓存补丁是两项不同问题；后者可退役的依据来自更早的生产修复。 |

## 验证边界与处置

本轮是源码去留评估和独立 presenter 原语对照，没有做产品删除、六模板替换、NativeAOT 重发布或 Windows/macOS 运行验收，也没有重跑全量。现有功能、25 项 Linux 基线失败和性能限制沿用既有验收记录，不因本评估改写。

- 独立只读复核确认退役建议有源码和当前产物对照支持，未发现重要问题；六模板、OTP、Semantic Part、测试/工具/文档引用范围完整。复核明确不认证尚未实施的模板集成、attach/detach、可见 caret、原生 IME 或 NativeAOT 行为。
- 临时工程、复制 DLL/native libraries、PNG 的内存输出、原始 JSON 和日志已清理，探针进程已结束，零新增常驻测试。源代码 diff 摘要与本轮开始一致，现有三处菜单修复及用户未跟踪文档保留。`git diff --check` 通过。


## 批准后的实施

用户批准后，InputTextPresenter 兼容类已删除，六处模板直接使用原生 TextPresenter，OTP 类型引用、现有契约测试、输入验证工具和当前控件文档同步调整。历史评估证据与本轮实施验证分别记录；实施结果及平台边界见[退役执行记录](2026-11-05-retire-input-text-presenter.md)。其他 hook 的保留结论不变。
