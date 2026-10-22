# CT-06别名与DataGrid专项结果

## 实现

- Core descriptor声明直接Own别名，校验未知/类型/自身/链/环/重复并保留Bind元数据；无别名终端不分配额外元数据wrapper。
- 每个动态配置层先用规范codec解析/归一；同值合一，不同值`ATMTHM4008`。XML层同一合同使用`ATMTHM3004`及原位置；compiled定义在所选Control绑定时归一，不提前准备全部Control。XML v1/compiled ABI v1字段及真实资源名保持。
- WholeV1及V2按原分派求值后，复制规范值至历史字段，再冻结/投影；0和合法null存在状态保持。
- Generator验证同/跨包声明及静态接口，输出中立只读表；读取descriptor不运行用户Token静态初始化。V2别名不要求独立默认，不允许默认读写别名，保留全部按名称slot。
- DataGrid仅加partial、生成接口及17个显式属性声明；默认公式、AXAML、其他历史字段及whole evaluator不变。完整映射见正式[token设计](../../../controls/desktop/data-display/data-grid/token.md#51-规范字段与历史入口)。34个审计字段补上实现证据，M0分类和未批准范围保持。

## RED→GREEN

| 证据 | 实际结果与含义 |
| --- | --- |
| Core API缺失 | 新类型不存在的编译失败，仅证合同缺失 |
| Core有效反例 | 10FAIL/2PASS；旧新入口占独立slot、同层无冲突、资源投影缺失；实现后12/12PASS176ms |
| Core扩展 | 73/73PASS474ms，含15个临时用例与相关既有Normalizer/Merger/Compiler/XML/compiled对照；null/0、默认/重置、等价指纹及双版本 |
| Generator有效反例 | 9/9FAIL，别名元数据/入口缺失，V2错误要求旧字段默认；首批实现后9/9PASS3s |
| Generator扩展 | 82/82PASS7s，含跨包、顺序确定、缺生成器静态接口拒绝、错误声明及V2读写别名拒绝；包含拟保留的一项真实Emit→Compiler合同 |
| DataGrid真实消费者 | 4/4FAIL：三档规范header背景未到目标，过滤器仍White；声明后4/4PASS1s，检查header/cell/footer/details框体/网格线及两种过滤器Presenter；同层snapshot中17对值一致 |

临时设置错误单独排除：第一次测试使用Global同名Padding、diagnostic属性名和程序集加载设置错误；端到端探针初次把raw父配置当有效父配置，缺算法；更正后重跑。均不记为产品缺陷。初次AOT使用/var软链接目录导致MSBuild相对路径错误，改在/private/var规范目录执行。

## 脚本化视觉

AC-01来自用户“重构后不能有视觉差异”：实际AtomUI.Window、Skia Headless真实合成器，640×358、scale1，40行/4列/右侧冻结列、592×310grid、标题/footer、真实溢出滚动条；明暗×Large/Middle/Small×默认/既有Table配置共12场景。改动前12/12PASS26s，改动后12/12PASS33s。初版普通Avalonia.Window缺少产品暗色文字继承，已排除并清理，全部前证换产品Window后重采；没有拿失真的暗色图当兼容结论。

AC-02来自输入与状态合同：框架MouseMove/MouseDown/MouseUp到实际cell，选择实际变成非空；记录静态/hover/selected-hover。原动效保留，稳定采样至少500ms且覆盖声明Transition时长，连续3帧相同。改动后6个历史配置场景在同一grid上切到规范名再回旧名，选中状态保持，整帧差异0。

AC-03来自检错要求：每场景A/A差异0；前后37对完整RGBA图（含1DIP对照图）差异均0，无缩放、裁剪、遮罩或容差。1DIP单变量对照在改动前后均检出8649像素变化，恢复差异0。比较命令输出保存在本任务文本workspace；最初比较输出目录误配，修正到已分配artifacts路径后39项比较全部通过，未把目录错误计成视觉差异。

已查看产品明暗、历史自定义选择/hover及1DIP关键图：表头、冻结分界、行网格、滚动条和footer对应，原始颜色/文字/几何保持。历史自定义颜色是显式对照输入，不据此宣布所有品牌配色对比度合格。

上述视觉范围为Skia测试Host；原生最终显示面、无输入闪动、全部DataGrid交互/排序/展开/弹层/滚动回收及其他平台仍由CT-18/CT-25负责。详情框体及filter是实际主题属性验证，本批没有把它们写成已打开原生Popup的像素验收。WholeV1仍不会重算其他独立默认派生字段，完整Own依赖迁移属于CT-18。

## AOT与测试处置

当前macOS26/osx-arm64/.NET10真实NativeAOT生成并运行别名终端及实际Normalize→Merge→Compiler；输出`ALIAS-AOT-PASS 0|1|9|9|0|0|ATMTHM4008`。分别证明元数据读取前用户静态初始化0、合并一个规范slot、最终9/9、显式0/0及冲突诊断。临时探针使用直接生成factory调用，无运行时反射。Homebrew本机库有macOS26相对目标12的链接警告；不证明macOS12或其他平台发布可用。

长期只保留一项生成端到端别名合同，复用既有Core引用/Emit fixture。它可发现生成descriptor漏接表导致的静默错误，以及规范合并/投影错误；编译器、ApiCompat、现有默认计划测试及手写descriptor测试均不能覆盖该组合。错误会影响所有别名配置，需要每次全局验证；真实AOT不适合每次替代。冷探针约1s，最终成本由清理后日志确认。不保留每控件/每负例矩阵、临时helper、工程或媒体。最初误放普通目录的候选已移回Temporary，在价值审定后才晋升。

## 清理后验证与自审

- Core相关精确filter：58/58PASS356ms；Generator schema/resource/default/alias：64/64PASS6s；DataGrid相关theme/size/frozen/layout/filter：22/22PASS2s；既有Skia焦点用例1/1PASS605ms。均无skip，没有运行全量回归。
- DataGrid及依赖.NET8 Release普通restore/build：0warnings/0errors30.76s。首次no-restore因缺net8 assets失败不计产品缺陷；补齐恢复后通过。当前macOS26/.NET10别名动态编译NativeAOT通过，保留上述平台及路径边界。
- 保留的一项端到端合同在最终工作树单独执行1/1PASS，用例约1s（console取整），进程总时长2.4303s。没有新增重复Core别名常驻矩阵。
- 删除全部4组Temporary源文件/场景、外部AOT工程；两次媒体目录均removed=true/audit clean/findings0/errornull。产品媒体运行删除75项（74PNG及manifest），普通Host预备图已单独清理。无本次运行中的驱动/Native探针进程。
- 静态测试准入审计exit0：15projects/751files/750classes/297groups；这是静态审计，不是全量测试。文档文件链接及git diff --check通过。
- 作者按单独步骤自审，未使用subagent；发现并去掉compiled无别名路径的一次数组重复拷贝，随后重跑Core/Generator及DataGrid。该变化不改变先前动态AOT/Skia输入路径；本批没有把该证据扩成compiled literal别名NativeAOT验收。无其他可定位的阻止CT-06交接的问题。作者自审独立性弱于新上下文评审，按用户明确要求采用。

CT-06的机制、17项兼容接入、代表实际消费与视觉保持及清理门槛已通过，允许进入CT-07详细规划；不代表CT-18完整家族验收或全部重构完成。正式Own/Generator/DataGrid模块文档及34个字段记录已同步，未创建commit。全部后续实现、验证、文档及审查由主agent完成。
