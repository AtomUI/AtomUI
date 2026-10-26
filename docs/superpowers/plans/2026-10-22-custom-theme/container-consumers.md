# CT-14 容器与导航消费执行计划

沿现有feature/custom-theme工作树，单agent实施，不创建分支/worktree、不提交。目录85项metadata、258个Own字段（不是85个独立公开owner）是范围输入，不能仅以示例名单缩小。实际公开identity、内部owner与抽象/布局节点分别按既有注册合同处理。

## N1 历史未公开能力

先处理T37：完整搜索确认NavMenu.KeyGestureColor没有快捷键文字节点；Menu的DangerItemColor/DangerItemHoverColor没有危险项入口，MenuTearOffHeight没有tear-off入口。仅这四字段归Reserved，保留名称、类型、ID、默认函数与配置往返，XML/正式文档/CSV标明未消费，CT23字段目录不可编辑。Menu.KeyGestureColor有真实InputGestureText，继续Active；NavMenu其他危险项字段须独立核定，不能从同名推定功能已公开。此包不新增危险项、快捷键或tear-off功能，不改运行语句，无新增测试必要。

## N2 实际字体、Global 与内部 owner

按ListBox/ListView、Tree/Transfer、Menu/NavMenu、Tab/TabStrip、Cascader/TreeSelect、Breadcrumb/Pagination/Expander/Segmented分组。先实际失败、原状态默认基线，再整改各ControlTheme与代码创建子对象；保留回收、方向、selected/open、组标题、滚动/裁剪及输入。SelectFilterTextBox虽然保留LineEdit Own回退，但各真实组合已用TemplateBinding或代码Content关系传递root字体；先证明哪条实际链仍受其影响，不以字符串存在推定失败。

CT10的共享表面/字体/内部路由和CT11的83集合仅提供机制。各实际公开child保留自身身份，内部对象解析实际owner；若代码绑定不能由AXAML等价表达，写明真实树拓扑及释放原因，不通过找Part直接改值。

## N3 Own 默认、尺寸与语义部件

按258字段的实际默认依赖和消费者逐项决定Active/Derived/Reserved/Legacy，迁移V2时保留默认值、类型、ID及显式下游。每个Custom入口分别核定其拥有的尺寸，三档与方向/Min/Max保持。所有Part定制使用生成专用Style并证明命中及关键Setter，不用Loaded/Name代码回退。

## N4 家族出口

实际Window/适用Popup同实例明暗、尺寸/密度、选中/展开/禁用/输入及配置往返；新旧默认0未解释差异，连续动画与真实输入单独观测，回填CT11具体路径。验证滚动条/viewport/extent/回收/换父/重模板/关闭及GC。各包临时测试与媒体清理后，只跑显式模块filter、NET8及当前Mac NativeAOT；其他平台归CT25，最终正式文档/CSV/总表/自审同步。

N1的四字段已同步XML、正式Token/changelog及CSV的CompatibilityRetained分类，没有运行语句变化或新增测试；CT23可编辑目录仍须实现保留项规则。Menu快捷键节点与资源引用已核实。N2–N4尚未验收，不能把兼容分类记作整个CT14完成。

### N2-A ListBox/ListView 禁用色与默认函数

先对实际disabled ContentPresenter证明各自ItemDisabledColor未消费，以及root有效Global的ColorTextDisabled没有进入其Own默认消费。只改原style Setter的资源输入，保持disabled选择器、节点、尺寸、状态和优先级；ListView现有其他Own已使用自身identity，不能假定继承了ListBox主题。两终端14/16字段均只依赖Global，迁移为V2 Revision1的30个静态默认函数，保留旧默认、字段、类型和ID，无需虚构Own间依赖。

实现前保存明暗60个实际Compiler默认值/ID和八根四尺寸基线；实现后逐值相同。临时验证实际Own、Global、显式0/透明及清除、兄弟和重新启用；Skia默认/选择/悬停/输入及配置往返、viewport/extent/可见滚动条/回收，当前Mac NativeAOT实际运行；临时源/驱动/媒体清理后ListBox/ListView/相关Menu分类聚焦及NET8、文档/CSV/自审。该包不代替其余Global、完整列表家族及CT14全部验收。

N1补查纠正：NavMenu十个DangerItem/DarkDangerItem字段也仅有声明和默认赋值，实际条目/数据/状态及模板没有危险项入口；先前文档中的“已公开危险项合同”描述不成立，已改正。这十字段独立归Reserved，与原T37四项合计14项保留，不改变代码行为。XML摘要已在源回放结束后同步；工作台不可编辑规则仍属CT23。

N2-A专项及清理出口已通过，见[列表结果](container-list-results.md)：30默认V2、实际禁用色/五继承主题、28 NativeAOT/JIT帧0、原生差异对照、最终262项/NET8；完整N2其他Global/字体/业务Host、N3及N4仍开放。

N3-T 的 [TreeView Global-only 默认结果](container-tree-defaults-results.md)已完成本机切片：12项V2、明暗24资源/key数值/type/slot不变、Global与Control Global／明确Own0／撤回／同实例重挂兼容；Mac managed／AOT各304项资源检查通过。补充真正明暗Window后的总168帧：56AA／24恢复／28新旧均0、56组1DIP全检出；临时清理后57项及NET8零警告。两个目录选中色仅兼容保留，容器当前14+2=16项Reserved，不新增目录树交互。完整N2/N3/N4、原生最终像素及CT14仍开放；本片后经27文件整合授权提交为ff0d3d572。

N3-TR 的[Transfer默认求值结果](container-transfer-defaults-results.md)完成本机机制切片出口：三个scope共11项V2、明暗22资源/类型/key枚举数值/slot不变，原Global/Control Global消费本已正常。Mac managed/AOT各684项、清理后原44项及NET8零警告通过；用户已接受[34对精确限定自然差异](container-transfer-defaults-noise-review.md)，原始失败保留，203张PNG及manifest共204文件已清理且媒体审计clean，CSV仅回填11条Token与3条Control。第十七批待审14文件原始blob已固定为可校验补丁，此段及目录更新属后续未提交差异，不扩大其授权；完整Transfer家族、原生最终像素、其他Host、N2/N3/N4和CT14仍开放。

Transfer后续核对并修正文档中的无效专用Style、SetterTargetType及Semantic owner混写：旧2项FAIL，修正后4项通过，原文编译AXAML的8Style/9Setter命中两分区/三条目，临时源清理后原44项通过。另确认TreeTransfer owner分页wrapper为internal，原34对及684原生脚本该阶段仅为内部模板诊断；[纯公开API补充](container-transfer-public-results.md)60自然帧、20组1DIP全检出、30对资源/几何一致，80直接绘制的40AA/16恢复/40新旧全0；新14对自然历史差异已于2026-10-26独立获批，原始计数保留，140张PNG及manifest共141文件已清理、Git媒体审计clean；TreeTransfer.ListWidthLG随后按共享公开属性标识核定Active条件消费，无新增树分页API。产品三份SHA保持，完整CT14及其他平台/原生最终像素未关闭。

TreeView、Transfer及NavMenu测试采样后经27文件整合授权提交并推送ff0d3d572；[第十七轮](platform-seventeenth-results.md)三OS各6405项、三NativeAOT及两Browser均成功，原冻结14文件已被该提交包含，不能重复提交。

N3-B 的[Breadcrumb八项默认结果](container-breadcrumb-defaults-results.md)完成本机机制切片：明暗16默认与288条资源/key/slot记录新旧相同，原消费已正常；正式84帧的28AA／12恢复／42新旧均0，28组1DIP全检出。Mac managed136／NativeAOT137、80资源记录一致；临时清理后原18项、NET8零警告及单Token成员审计通过，169媒体文件及419文件原生工程已清理。八条Token／一条Control回填，不关闭其余Global／字体、图标／键盘／完整状态、其他Host、原生最终像素或整个CT14；尚未提交。

Breadcrumb后经第十八批13文件批准提交并推送93ef4ba8d；[第十八轮实际结果](platform-eighteenth-results.md)8个job均成功，三OS各6405项、三NativeAOT各26／两Browser各23检查，未取消／重试／发布。

N3-P [Pagination本机实现](pagination-input-wiring-results.md)完成14项V2默认、经批准的Mini宽度与Simple输入背景接线；原版明暗六处不更新反例修复，四尺寸／只读／禁用／透明／0／撤回／焦点／光标／真实Enter保护检查通过。明暗28默认及168资源记录保持，Macro36组新旧0；Mac managed206／AOT207与140记录一致，临时清理后原57项及NET8零警告通过。14Token／2Control回填，其中两项兼容保留，容器Reserved当前16+2=18。用户2026-10-26已批准所示视觉修复及N01–N10十对精确限定自然差异；158张PNG及manifest共159文件已清理，媒体审计clean，原始失败计数保留。本机默认机制及两项输入接线切片出口完成；完整家族、其他Host及原生最终像素仍开放，本批Git另审。
