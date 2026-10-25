# CT19：既有键盘缺口只读审计

本项对应C7已记录、C8短设计明确另行处理的键盘缺口。当前仅审计源码及既有输入证据，不修改产品默认、模板、事件或已冻结的第十五批15文件，不包含在其Git审查范围。

- `ColorSpectrum` 有现成的四方向键、Ctrl与RTL分支，pointer pressed也调用`Focus()`；但实际Spectrum默认Focusable=false，既有原生探针已记录无法自然获得该键盘路径。ControlTheme已有`:focus-visible`的FocusEllipse样式，仅有样式不证明输入可达。
- `AbstractColorSlider`继承`RangeBase`并把Focusable设为true；实际`ColorSlider`和基类没有键盘数值处理。C7原生探针在真实焦点下收到方向键却没有颜色通知，不能记作支持键盘。内部thumb也可获得焦点，未来合同需明确从slider及thumb冒泡的键盘如何处理，不能靠在驱动中强设内部Focusable制造通过。
- 本地Avalonia参考来源仍为`/Users/chinboy/Projects/ReferenceProjects/Avalonia`、commit `8eeda4f6f546165b3f72e63c9f42247abb306905`；其ColorSlider直接继承Slider，能够得到通用键盘行为，AtomUI当前继承链没有该能力。该事实只作定位参考，不能直接照搬其公开API或默认值。
- C8让六模式可达后，还需核对每种实际二维轴的方向。静态可疑点：ValueSaturation的Y是正向Saturation，但现有Spectrum键盘代码把Down对应的非Hue分量一律降低；尚未用自然焦点和真实按键建立反例，不能报告该项已复现或修复。

后续在任何实施前需固定完整UX：Tab次序、指针获取焦点、四方向与RTL、边界／wrap、Ctrl及Home／End是否受现有合同支持、Alpha与未操作分量、禁用、输入框和渐变stop的邻接影响。Spectrum若改变默认焦点可达性，属于API审查L2的可观察默认语义变化，须明确短设计审查；C8的SV默认批准不扩大到该默认。

目前不新增公开签名、Token或PART，也不提出未经批准的键盘步长／导航策略。该审计不构成键盘验收或CT19完整家族通过。

2026-10-25审计跟进：色谱已按独立批准的C9短设计接通，VS方向取得有效反例并修正；[结果](color-spectrum-keyboard-results.md)含24项临时、清理后27项及原生边界，视觉31对限定审查待回复。此进展不覆盖滑轨；AbstractColorSlider／ColorSlider仍没有键盘数值处理，保留为明确缺口。

2026-10-25最新跟进：C9棋盘重挂及31对有限差异已获批准并清理媒体。用户已批准[C10短设计](color-slider-keyboard.md)；具体ColorSlider的单一键盘入口已实现，原8项RED／候选26项GREEN（空值与真实两stop数据分别验证）。真实Mac managed八窗口5688项通过，NativeAOT与像素对照继续；上述历史审计中的缺口是实施前状态，不能据此声称完整家族验收。

C10本机切片出口已完成，见[结果](color-slider-keyboard-results.md)：显式单分量及Alpha数值键盘路径、现有舍入、两自然焦点入口与RTL已验证。72对精确有限差异获批并清理媒体，27项／NET8及成员审计通过；此项不扩展到多stop编辑或其他平台最终像素，C9首轮AOT历史仍保留。
