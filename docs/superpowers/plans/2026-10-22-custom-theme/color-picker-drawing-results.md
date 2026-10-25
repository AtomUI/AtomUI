# CT19 C4：空值斜线与选择环接线审查

范围为ColorBlock实际空值斜线和Spectrum实际选择环；[执行计划及AC](color-picker-drawing.md)。沿当前feature/custom-theme，不添加公开API、Own字段、资源编号或外部监听。本文为作者自审，无独立子agent审查。

## 原版与候选

原版ColorBlock静态红色2 DIP，SelectionEllipse模板固定2 DIP。资源正对照确认ColorError／LineWidthBold正确进入scope后，原版10项2通过／8失败，4.0809s；候选10项全通过，3.9197s。明确Global紫6、匹配Control青4、另一owner隔离、0不绘制及同实例reset均读取实际DrawingGroup Pen；弹层环从2到6到0到2。

ColorBlock只添加内部绘制输入与自身惰性Pen；颜色通过OwnerTokenResource已有显式状态入口投射，未明确提供时保留历史红色，不将默认ColorError冒充旧颜色。AXAML绑定有效LineWidthBold，0不回退。选择环线宽由ControlTheme style setter接线。ColorBlock先做纯成员布局，Roslyn逐成员token完全相同，静态初始化相对顺序相同；最终成员审计一文件一控件零问题。

追加作用域、释放和controlOnly检查首次15项13通过／2个GC失败：未推进Headless真实render时图仍存活。仅追加一次ForceRenderTimerTick，不改产品或延长等待，两个GC均通过，owner／scope／marker／window四个weak各全部false，3.8325s。原版对照及最终整组检查仍继续，不能以本段单独关闭完整资源生命周期验收。

## 像素合同

真实产品Window、公开ColorPicker／GradientColorPicker及实际pinned Popup，Mac arm64 Headless／Skia，1200×820、scale1。Default和Default+Dark算法以及Window实际Light／Dark同时记录。每实例完整轨迹：empty-default→general→control→zero→reset→popup-default→general→zero→reset→disabled。新旧各独立main／AA／1DIP三实例，完全同历史，不用像素容差、裁剪、重定位或遮罩。

240帧，80组A/A全0、80组1DIP全检出、16组同实例恢复全0。40组新旧比较全部符合预期；默认、两类reset、disabled共20对整图0。20对显式配置变化如下，每种控件的Light／Dark计数相同：

| 阶段 | 纯色Light／Dark | 渐变Light／Dark | 变化范围 |
| --- | --- | --- | --- |
| 明确Global ColorError紫／LineWidthBold6 | 184／184 | 184／184 | 空值斜线颜色和线宽 |
| 匹配Control ColorError青／LineWidthBold4 | 144／144 | 144／144 | 空值斜线颜色和线宽 |
| 明确LineWidthBold0 | 102／102 | 102／102 | 斜线消失，框和几何保留 |
| popup明确LineWidthBold6 | 30／30 | 30／30 | 色谱右上角选择环笔画 |
| popup明确LineWidthBold0 | 32／32 | 32／32 | 选择环笔画消失，色谱和选择保留 |

斜线全图差异闭区间：纯色x118–141／y84–107、渐变x44–67／y84–107；0值斜线范围缩为纯色x119–140／y85–106、渐变x45–66／y85–106。选择环6为x308–313、纯色y134–139／渐变154–159，0为x306–313、纯色y134–141／渐变154–161。保留原色谱边缘裁剪、控件尺寸、弹层内容与值，未扩张模板或隐藏交互来消差。

已打开检查旧版／候选紫色斜线、暗色青色、暗色默认红色、0斜线、0选择环、暗色选择环及全图差异：预期颜色和宽度命中，邻接边框、文字、色谱、滑块及背景无额外变化。AA／DIP只证明比较器资格，不替代这些视觉审查。

## 准备失败与验收边界

首次trace相对路径不适用测试工作目录，暗色首次只请求Window变体却被主题同步覆盖，另一轮有新增临时探针缺namespace编译失败；均为准备失败，不计有效暗色或产品RED。修正绝对trace路径、完整manager算法和scope后重放全部240帧。有效旧新源码SHA、全部帧SHA、比较记录在.superpowers/sdd/color-picker-drawing文本证据中。

原生探针最初把宿主专属Global错误扩展到独立View，已纠正为分别验证两者身份；原生最终managed／NativeAOT仍未完成，清理后的模块与NET8仍待执行。本文此时只提交所示20对显式配置视觉变化审查，不表示Git提交授权、原生最终像素或完整CT19家族通过。媒体在审查后按规范清理，最终记录不依赖媒体存活。

## 渲染队列的单变量对照

原版同一完整控件图、配置和关闭流程，推进一次真实render时两项均通过，owner／scope／marker／window四weak全部false；3.8065s。不推进时纯色一次已释放，渐变四weak全true，两个case中一项失败。候选初次未推进时两项失败；推进一次后两项全部false。正式生命周期验收明确在真实compositor已消费排队工作后检查，没有更改产品释放代码或增加GC次数／等待时长以消差；不把尚未消费的渲染队列当资源订阅泄漏，也不据此免除完整开放／关闭图的后续验收。

原生诊断核对了颜色输入与有效输出：默认算法输入ColorError=#800080输出Purple，Default+Dark输出#ff700370。旧探针错误地要求暗色输出也等于原始Purple，候选实际颜色正是有效资源值；已经改用独立scope资源查询作预期，仍要求默认／reset为历史Red2。该校正不改变前述像素、产品实现或视觉审查范围。

## 2026-10-25 视觉批准及完整消费补充

用户已明确批准上述斜线与选择环主题接线的20对视觉变化；该批准不含Git提交、完整家族或新发现的其他阶段差异。

追加真实ClearColor、IsNeedConfirm空草稿ColorPreview及框架原始指针点击。每实例仍先重放前述十阶段，再执行clear-default／general／zero／reset、draft-default／general／zero／reset和cleared。新旧每次清除均只有一次ClearRequest与一次ColorValueCleared，公开值最终为null，弹层保留；历史公开值变化次数纯色2、渐变3，新旧一致，未把原有事件合同改为一次。

首次补充216帧不是合格验收：旧版Dark Gradient四个draft阶段A/A各1722像素、候选Light Color四个draft阶段各5像素。前者实际白／红Alpha位图不一致，已用受控worker证明ColorSlider旧请求晚完成会覆盖新颜色／尺寸，详见[C5顺序修复](color-slider-bitmap-order.md)。没有将1722或5像素套用此前C1／C2容差。

移除全部C5控制点后重放公开历史，候选36组A/A全部0、36组1DIP全检出、8组恢复0；初次候选的5像素未出现在本次正式输入。为隔离C4效果，再用C4原源码加同一C5顺序守卫作单变量基线。36组新旧比较全部符合预期：默认、reset及cleared20对0；每个控件明／暗的clear-general214、clear-zero134、draft-general618、draft-zero358像素。原基线35／36组A/A为0，Light Gradient cleared三像素仍待渲染诊断：闭区间x45–51、y360–367，单通道最大16；其余配置阶段全部0，36组1DIP均检出、8组恢复0。保留真实计数，尚不将补充采样或新视觉范围标记验收通过。

原生最终managed／NativeAOT各404项检查通过：真实NSWindow、scale2、Window实际明暗、两类公开owner与独立View、真实清除色块／草稿预览、0及恢复，外加32次公开颜色／空值与宽度快速往返，直接读取实际Alpha Bitmap像素与track物理尺寸。每次最终白／蓝与公开输入一致，位图444×16、末端alpha254；两条执行路径一致。先前错误使用/var与/private/var别名造成MSBuild引用路径准备失败，后续误把AtomUI内部Track当Avalonia Track导致探针查询失败，均已校正且不计产品RED。NativeAOT存在Homebrew动态库构建目标26与链接目标12的警告，当前真实宿主运行通过，不据此宣称旧macOS支持。清理后的模块、NET8及补充渲染诊断仍继续。

## 补充视觉审查表（尚待明确批准）

| 单变量C4阶段 | 纯色Light／Dark | 渐变Light／Dark | 闭区间范围 |
| --- | --- | --- | --- |
| clear-general：弹层清除色块紫6 | 214／214 | 214／214 | x290–313；纯色y134–171、渐变y134–191，含选择环 |
| clear-zero：清除色块与选择环0 | 134／134 | 134／134 | x291–313；纯色y135–173、渐变y135–193 |
| draft-general：空草稿全部斜线紫6 | 618／618 | 618／618 | 纯色x52–313／y84–367、渐变x44–313／y84–387 |
| draft-zero：空草稿斜线和选择环0 | 358／358 | 358／358 | 纯色x52–312／y85–366、渐变x45–312／y85–386 |

以上16对为同一C5守卫下的C4旧／新源码比较，配置外和20对默认／恢复／最终清除全图0。已查看明色清除色块、暗色草稿及相应全图差异，变化只在预期斜线和环；不是以前20对批准的自动扩张。

C5的原始Dark Gradient AA旧图仍显示红色Alpha背景，但当前文本为#FFFFFF。修复后同历史AA显示正确白色；draft-default和draft-reset各1722像素，仅x53–268／y380–387。general／zero另含C4预期618／358，合计2340／2080；不把合计解释为额外噪声。受控完成顺序的两项RED／GREEN证明旧请求覆盖根因，公开完整历史和native404各次真实像素证明当前值消费，不能把旧／新main碰巧都为白色的0差异当作修复前无故障。

原基线Light Gradient cleared A/A的三个像素实际为：(51,360)灰137→121、(45,361)灰247→248、(51,367)灰136→121，alpha均255。仅本Host Mac arm64 Headless/Skia、scale1、1200×820、文中完整十加九阶段历史、C4旧+C5守卫源码、main／AA比较适用，坐标与通道上限逐项固定；不适用于候选新图、其他状态、控件或平台。其来源位于原有滑轨thumb边界，未改产品半径或重绘策略。另一次完整同历史诊断的main／AA自然compositor、独立直接Skia、全节点诊断重绘均0；274个完整公开树节点的类型、几何、矩阵、裁剪、字体、画刷／Bitmap SHA、slider输入和shape笔画逐项相同。诊断未稳定重现先前三像素，故不声称已精确证明那一次缓存内部原因；该历史比较保留失败和真实计数，只申请接受上述三个实际像素的限定差异，不使用遮罩或通用容差。诊断重绘只在临时探针，不进入产品或正式比较过程。


用户随后明确允许补充斜线和Alpha位图修复，并接受审查表所列历史3像素限定。批准仅覆盖表内16对C4变化、C5错误红色Alpha的1722像素及指定旧基线3像素；真实计数／失败保留，不扩大到其他阶段、候选或平台，不含Git提交。15项最终功能／生命周期临时验证4.6325s通过，临时C4测试、旧源备份和首个原生工程已删除；清理后ColorPicker两原模块及TimePickerDisplayTime共23项5.5631s通过。随后C5挂载复核发现独立的离树改色后旧位图问题，新增临时反例／最小修复继续，前述23项不认证该新增源码。


最终C4专用媒体752个已按evidence.py清理，Git index/worktree/untracked新增二进制审计findings为空；全部帧SHA与真实差异计数保留为文本。C4临时源、源备份、驱动和原生工程已删除。C5补拍临时源也删除后，Desktop原三模块23/23（5.0879s）；Core原资源模块17/17（1.8225s），NET8 Release零警告／错误，两触及控件成员审计files2/controls2/issues0，产品源SHA与Core／NET8 receipt逐项相同。C4作者自审通过，未新增公开API／Own／资源编号，未增加外部订阅或定时器。此结论限本切片，不关闭C5新视觉审查、C6或整个CT19。
