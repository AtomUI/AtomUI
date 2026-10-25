# CT19 C4：空值标记与选择环的实际主题消费

> **For agentic workers:** 使用superpowers:executing-plans在当前会话执行；用户明确不使用subagent，不自动提交。

**Goal:** 保留默认绘制，令有效Global颜色与线宽到达空值标记／选择环，并验证身份、作用域及恢复。

**Architecture:** 复用OwnerTokenResource和原生ControlTheme。内部绘制输入由AXAML绑定，ColorBlock保留原Render路径及自身惰性画笔；不新增资源订阅或公开Token。

**Tech Stack:** 现有C#／AXAML、Avalonia12.1.4／Skia、xUnit／Shouldly、当前Mac managed／NativeAOT。

**Spec:** [总路线图](overview.md)、[CT19任务](tasks.md)、[主题验收规范](../../../engineering/development/theming/verification.md)。

## Review Focus

- 默认ColorError与历史红色不同：只认normalized显式状态，默认／恢复严格不变。
- ColorPicker／Gradient／View及scope身份不同：实际消费者必须归正确owner且彼此隔离。
- 明确0线宽／反复配置／重置：0不回退2，不留下旧画笔；保持同一实例。
- 模板／重挂载及变体：沿原资源宿主生命周期，不新增外部事件订阅。
- 输入／禁用及Semantic Part：保留选择、清空、几何、专用Style和透明度。

## 文件责任

- src/AtomUI.Core/Theme/Resources/OwnerTokenResourceExtension.cs：暴露已有可选Global绑定，保留原controlOnly语义。
- src/AtomUI.Desktop.Controls.ColorPicker/ColorBlock.cs：内部绘制输入、失效及自身画笔缓存。
- src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorBlockTheme.axaml：实际owner颜色／线宽绑定。
- src/AtomUI.Desktop.Controls.ColorPicker/Themes/ColorView/ColorSpectrumTheme.axaml：选择环线宽绑定。
- tests/AtomUI.Desktop.Controls.Tests/Temporary/CT19Drawing：临时反例及真实主题／绘制验证，完成后删除。

## 审计与合同

本包为主题消费缺陷修复，沿现有feature/custom-theme／checkout，不创建分支、worktree或subagent，不自动提交。第七轮已获批提交f0549a3a9／5c7c4a2cc并运行40155448673，当前研发不改变该远端提交或扩展其Git授权。

ColorBlock的空值斜线直接使用静态红色2 DIP Pen，ColorSpectrum选择环在模板写死StrokeThickness2；现有完整Global ColorError／LineWidthBold不能到达实际绘制。Global LineWidthBold的默认公式LineWidth+1为2，选择环默认可保持。空值红色与默认ColorError并不相同，必须保留历史红色，只有明确配置ColorError才接通该角色；明确为默认值也须生效，不能通过比较值猜测显式状态。

状态所有者：公共ColorPicker／GradientColorPicker及实际View，内部ColorBlock／Spectrum不增加伪造公开owner。保持真实窗口、触发器／弹层、色谱内容、命中、尺寸、选择数据、clear事件和disabled透明度。语义部件定制继续专用生成Style，不以代码找节点改样式。

方案：OwnerTokenResourceExtension只增加内部AXAML WhenExplicit入口，将已有CreateBinding的whenExplicit=true、controlOnly=false公开给内部主题；原WhenControlExplicit逻辑原样保持。ColorBlock增加内部Stroke／StrokeThickness绘制输入，主题用OwnerTokenResource的ColorError显式投射与LineWidthBold正常投射，历史无明确ColorError时红色保持。画笔按实际输入惰性缓存，输入变化只失效自身，0线宽不绘制；全部绑定声明在ControlTheme。选择环只把模板常量换为同一有效LineWidthBold。没有新公开API、Own字段、Token ID、反射或自建订阅。

## 执行与验收

### Task 1: 保留默认的标记和选择环主题接线

- [x] Temporary/CT19Drawing复用产品Window、配置作用域和公开两owner，DrawingGroup读取实际绘制Pen与SelectionEllipse。建立默认红2正对照、明确颜色／线宽、身份隔离、同实例恢复及0线宽的有效RED；核对实际Root／Popup。
- [x] 最小修复四产品文件。成员布局另存等价证明；原WhenControlExplicit仍不接受一般Global输入。沿原继承／reparent和GC资源链，不新增监听器。
- [x] 旧新完整Light／Dark、默认／配置／恢复，同历史、独立A/A及1 DIP。实际专用Style与输入保留；配置视觉逐项审查，不复用其他平台容差。
- [x] 实际Mac managed／NativeAOT按风险验证。清理临时源／驱动／媒体后执行ColorPicker及相关Core／TimeView精确选测、NET8、成员审计和正式文档同步。

本包完成也不等于整个CT19或CT25关闭；24个既有Own的其余真实消费者、各状态及完整平台／显示面继续按逐Control表验收。

## 最小验收合同

| AC | 来源／前提 | 操作和独立预期 | 观测 |
| --- | --- | --- | --- |
| C4-01 | 历史外观合同，两公开picker、真实Window，空Value | 默认→明确Global颜色／线宽→匹配Control配置→0→reset；历史默认红2与reset逐像素相同，明确输入到实际斜线，0无斜线 | DrawingGroup实际Pen、同实例marker、完整窗口RGBA |
| C4-02 | 相同Scope及公开弹层View身份，原色谱几何 | 打开同一pinned弹层→6→0→reset；环2／6／0／2，popup和ring实例保持，宿主专属Global不能越过独立View身份 | 实际SelectionEllipse、公共Value、可见弹层、完整窗口RGBA |
| C4-03 | 资源重挂载／释放风险，配置作用域不同 | 同一owner从紫6移到青4作用域，旧scope变化不影响新scope，reset恢复；卸载后整个控件图可GC | 实际绘制、同实例、WeakReference，不用RSS |
| C4-04 | 共用OwnerTokenResource显式入口的兼容风险 | TimeViewCell保留ListView明确Own优先于一般Global，仅其自身Control明确Global接管 | 真实主题cell的Foreground，不修改生产TimeView |

视觉固定Mac arm64 Headless／Skia、scale1、1200×820，Default和Default+Dark算法连同Window实际变体同时核对。同轨迹的新旧主实例、独立A/A及独立1DIP实例，每条走完整10阶段，不裁剪、对齐或遮罩。最终原生managed／NativeAOT检查另外记录，不由Headless像素替代。


本切片清理及作者自审完成，结果见[color-picker-drawing-results](color-picker-drawing-results.md)。15项最终功能／生命周期、原生404各次、已批准的初始240帧及补充范围均保留独立数字与SHA。752个专用媒体文件经Git无新增二进制审计后清理，驱动／原生临时工程／源备份已删除。最终清理后Desktop23项5.0879s、Core17项1.8225s、NET8 Release零警告／错误、两文件成员审计零问题；C5挂载补充及C6输入问题各自记录，不能把C4完成扩大为CT19家族通过。未创建Git提交。
