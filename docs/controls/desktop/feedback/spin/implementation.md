# Spin 实现

公共入口与状态见 [overview](overview.md)，默认值见 [token](token.md)，专用样式见 [semantic-part](semantic-part.md)。

`AtomUI.Controls/Spin/AbstractSpin.cs` 定义组合 API、遮罩透明度与模板状态；`AbstractSpinIndicator.cs` 定义独立指示器、当前动画目标、有效显隐追踪与自定义内容生命周期；`SpinIndicatorDotPanel.cs` 测量四个点并按右、下、左、上布局。Desktop Spin/SpinIndicator 只接入主题及实际 inheritance-parent 桥，两个 AXAML 主题拥有字体、颜色、尺寸和模板绑定。

组合模板中 ContentPresenter 与 MaskLayout 同层；后者包含 Mask 与垂直 section，section 内为 SpinIndicator 和 Tip。IsSpinning 只控制 MaskLayout/Indicator 的可见性；IsTipVisible 单独控制 Tip。原内容对象、模板及内容滚动约束保持。非模糊 spinning 时原内容透明度为0.5；模糊选项保留5 DIP Blur。IsMaskBackgroundEnabled 控制 Global ColorBgMask，关闭后退回透明。Opacity Transitions 继续位于 ControlTheme style setter。

SpinIndicator 的内置面板与自定义 presenter 都有 content marker，同一时刻一个可见。Content 或 ContentTemplate 任一存在即选择自定义路径。模板重套用先停止旧目标动画并解绑旧 presenter，再绑定新目标；有效隐藏/卸载停止合成动画，重挂重启。IsMotionEnabled 是组合遮罩过渡入口，不宣称它会关闭独立指示器持续旋转。

原旋转使用 Compositor RotationAngle、完整2π/Forever，四点各自 Alternate opacity 与错峰延时；默认周期1200ms，实际小于1ms保持原防护。持续态使用真实 clock 验证，静态终态不能替代完整周期或平台输入结论。

Caller-owned PathIcon 没有模板 TemplatedParent，无法由本控件 ControlTheme 等价穿透到内容节点。仅对这个运行时成员使用 typed、Style 优先级的可释放尺寸 relay：无值时跟随 IndicatorSize，原生 Local/普通 Style 保持优先。替换内容、换模板及 detach 释放，重挂重新建立；不写死 caller 的 Width/Height，不接管其 Foreground，不处置 caller 对象。此投射不是 Semantic Part 定制回退。

重模板前记录当前原生 Content 子树焦点的弱引用与代际，待 presenter 建立后恢复仍属当前 owner/TopLevel 的输入。后续焦点请求优先，detach 清空待恢复状态。正文对象、文字、caret 和滚动约束分别验证，不能以重建内容规避焦点问题。

字体/全局与 Own 资源绑定都留在 AXAML，主题宿主负责订阅释放；代码只处理真实动态 membership、输入焦点与 Composition 生命周期。所有新增验证为临时，清理后执行精确模块门禁。Headless/Skia 的通过不表示 OS/Browser、GC 全部通过或完整 CT15 关闭。


## 精确循环边界

旋转以明确0为隐式起点、仅声明2π终点，保留原easing/Forever/停止和有效显隐。四点透明度声明起点仍0.3、终点1、Alternate周期P与index×P/3延时；整体稳态周期为2P，不能将startup与P时刻直接等图比较。

Opacity首key使用最小正规正float进度，避免显式0与隐式0的零长度区间；该位置低于TimeSpan任何合法非零tick进度，正常可表示相位及原延时前初值保持。使用正规值避免原生目标将次正规位置冲零。起动时明确Composition opacity为0.3，不改变公开DotBgBrush/大小/CustomIndicator或Semantic Part入口。

默认1200ms、原生500ms/0与Own2000ms分别验证；raw0继续保留既有Compositor 1ms下限。热duration与CubicEaseInOut、显隐、重模板及detach/reattach验证成员退休，测试时钟只在临时取证中使用，不进入产品。

根控件的 MaskOpacity 自动过渡使用 `TransitionInitializationSession`：构造时抑制、Loaded 完成基线、detach 复位，
恢复最新 Transitions 来源。指示器的持续反馈独立于该初始化作用域，不通过私有开关或 Dispatcher 猜测就绪。
