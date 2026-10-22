# 自定义主题交付与验证方案

整体目标、范围与工作包见[入口](overview.md)，逐问题安排见[矩阵](problem-matrix.csv)。本文是工程分解与验收方案，产品代码实施前仍需完成对应机制规格及文件级任务审查。

## 1. 工作包的具体交付

| 工作包 | 主要源码/资料边界 | 要做什么 | 通过出口 |
| --- | --- | --- | --- |
| W0 | `docs/engineering/development/theming/`、现有runtime/Token/Part/协议与Control目录 | 审定12条规范；按公开产品Control与生成注册交叉建立目录；分类Active/Derived/Legacy/Reserved；记录状态/尺寸/平台、消费目标、当前缺口与基线 | 每个Control有owner和验证入口；现行与新增合同无混淆；性能/像素基线与容差按Host冻结 |
| W1a | `src/AtomUI.Core/Theme/Compilation/ThemeCompiler.cs`、`Schema/ControlTokenDescriptor.cs`、`DesignTokens/`；`src/AtomUI.Generator/DesignToken/`与`TokenResourceKeyGenerator.cs` | 形成新版Own声明、输入/覆盖、依赖、继承、版本、cache revision及兼容规格；验证代表原型 | EVT-01/02/03及生成/跨程序集/AOT合同证明；未迁移旧路径语义明示 |
| W1b | `ThemeManager.cs`、`ThemeTransaction.cs`、`ThemeConfigProvider.cs`、`ThemeScope.cs`、资源bridge/lease；`SemanticParts/SemanticPartManifestWriter.cs`与资产工厂 | 按正式品牌设计落实集合/scope资格、提交前接线/预激活、发布/退役及版本化表现结果；验证native优先级及失败边界 | EVT-04/05/10/11；现行不可回滚Publish没有被混用，新合同故障可检测且支持边界明确 |
| W2 | `src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/`、`Popup/`、公共图标/文字/波纹/箭头及资源绑定 | 公共路径使用正确owner；真实frame、排版、动态动效和缓存接通；不新造重复primitive | Button、LineEdit/Select、Timeline和跨根场景验证；public子Control自身配置仍有效 |
| W3 | `src/AtomUI.Controls/`、Desktop及DataGrid/ColorPicker实际Control家族；对应tests、docs/controls及Gallery示例 | 逐控件完成入口→owner→节点→优先级→状态/尺寸→更新→证据的完整循环 | 所有Control均有结论；每个Active Own/派生边/Part route关闭，原始UX和默认主题保持 |
| W4 | 现有主题定义/编译主题包、生成AXAML资产与Gallery主题配置 | 三套完整品牌及边界配方，包含实际生效Token与样式资产 | 多维外观、嵌套和弹层一致；完整往返，未支持平台效果有明示 |
| W5 | `controlgallery/AtomUIGallery/ShowCases/General/CustomizeTheme/`及现有workspace/Token/Part工具；GalleryBase只承载产品中立能力 | 编辑/预览/诊断/恢复/完整保存加载；显示原始/继承/有效/保留状态；随后接受限AI | 手动配置先通过EVT-11；AI非法/未知输入失败且不应用半成品，实际服务接线单独验收 |
| W6 | 实际改动模块、注册/生成资产、平台Host、发布与迁移文档 | 清理临时验证与废弃接线，检查scope/cache/lease、兼容资料及所有未关闭项 | 最终受影响模块、专项证据和范围清单完整；无未核实通过项 |

W0目录由纯元数据和公开产品合同建立，不能靠预先实例化所有控件获得。文件数、Own类型数、SemanticPart文件数均不是Control数。一个公开Control关联多个模板/变体或平台实现时，目录应保留这些差异；内部Part不因此获得identity。

## 2. 逐控件迁移顺序

| 批次 | 覆盖 | 重点 |
| --- | --- | --- |
| 代表批次 | Button家族、LineEdit/Select、DataGrid、Skeleton、Timeline、ColorPicker | 覆盖owner、Own依赖、别名、root/frame、本地值、自绘与跨根，先证明机制可用 |
| 基础呈现/输入 | 文本/图标、按钮、单多行输入、数字、OTP、选择与装饰框体 | 排版、尺寸、状态、Public属性与真实节点投射 |
| 列表/选择/导航 | List、Tree、Menu、Nav、Tab、Cascader、TreeSelect、Transfer等实际目录项 | 容器回收、项目owner、条件样式、Custom与密度 |
| 反馈/日期时间 | Tooltip、Confirm、Dialog、消息/通知、日期/时间/Calendar等 | Native/Overlay、owner字体、焦点与数据状态、内部public子Control |
| 自绘/复杂及可选包 | Progress、Rate、Slider、Switch、分隔/拖拽图形、DataGrid、ColorPicker等 | 几何/pen/文本依赖、细线、形态、虚拟化、非Visual资源host |
| 工具与补充范围 | GalleryBase/Gallery、插画/命名色及实际Extras能力 | 工具token与内容数据边界、目录/文档、适用平台 |

批次是执行顺序，不代替W0逐Control清单；重复出现在代表批次的Control无需重复建立保障。每批完成相同的规范、消费、运行与文档循环，不能先全库替换再第一次观察效果。

## 3. 统一验证场景

以下EVT编号是方案证据分类，不是已经存在或已经通过的测试名。每个Control按适用合同展开，复用参数化临时验证和现有机制保障。

| 编号 | 场景与必须成立的反例 |
| --- | --- |
| EVT-01 Own有效求值 | 未覆盖下游时，上游覆盖驱动其默认；显式下游保持；0/null存在位正确；每条依赖边及全部Active字段有证据；暗色、Control算法、继承版本及跨程序集保持合同 |
| EVT-02 owner与scope | Global基线、Control覆盖、嵌套和兄弟同时存在；只影响目标identity；内部对象用正确owner，public子Control保留自身身份；未注册exact type不静默fallback |
| EVT-03 别名及输入 | DataGrid公开HeaderBg/CellPadding等到达实际表头/单元格；旧名兼容；旧新不同值冲突可诊断，同值可规范化；Reserved与Deprecated不伪装成Active |
| EVT-04 优先级与形态 | 默认、状态、实例值和专用Style分别到达正确目标；Button Round→Default无残留；Timeline边框/半径可定制；CircleProgress显式字体不被布局重写；品牌资格不破坏BindingPriority |
| EVT-05 跨根与重置 | Native/Overlay、新建/已打开、owner迁移/关闭；Token、字体与品牌样式均符合scope；父品牌有padding、重置子只有foreground时不残留父padding；普通应用Styles行为维持 |
| EVT-06 尺寸与布局 | S/M/L、Custom有/无显式值、方向/variant、父级Min/Max、字体与缩放；OptionButton计算档位/量纲正确；粗边框、大字号、零半径、紧凑配方保持命中与滚动 |
| EVT-07 Part真实命中 | 每条独立route、替代节点及cardinality有证据；关键Setter影响真实布局/外观；示例AXAML使用生成专用Style，不以找节点赋值或Name/Loaded代码回退通过 |
| EVT-08 实际绘制 | ArrowSize驱动geometry与自然尺寸；Rate完整字体进入VisualBrush；Switch/Slider/线条等按已批准形态和描边输入绘制；命中区、占用空间与绘制几何一致 |
| EVT-09 已有实例更新 | A→B→A、resize、状态切换、re-template、容器回收、detach/attach；Skeleton渐变、Gradient文字、默认色/动效无旧值；普通配方切换不重建页面，输入/选择/焦点/滚动保持 |
| EVT-10 生命周期与故障 | owner变更/离树/关闭后释放资源host、binding、lease、动画及style；WeakReference/对象计数无历史图增长；准备、接线、激活和退役异常分别注入，现行Token发布异常仍是Committed+诊断 |
| EVT-11 完整配方与工具 | Ant/shadcn/Bootstrap及边界配方多维生效；导出→导入保留Token和样式资产/引用；类型、未知资产/Control、版本、冲突、非法值均可解释；失败草稿不应用半成品 |
| EVT-12 平台/AOT/性能 | 真实启用包、静态/动态配置等价、trim/NativeAOT消费；无新增反射/运行期AXAML；同场景首次使用/切换/嵌套/弹层的时间、分配和对象基线，独立惰性与有界缓存保持 |

Global只需对实际消费语义选择有检错能力的代表，完整schema仍可配置；Active Own不能抽样代替全部。相关状态按最小充分组合覆盖，避免没有检错意义的笛卡尔积。仅新增字段、资源值变化、编译通过或截图相似都不足以证明消费成立。

## 4. 失败到通过的验证生命周期

1. 找现有测试和原始场景；记录当前触发、期望、实际、owner、值及树/绘制路径。
2. 必要时在系统临时目录或所属测试项目`Temporary/<任务标识>/`建立有效失败证据。已有测试只验证默认值时，不能作为用户覆盖有效的证明。
3. 根因修复后以同一输入和断言验证；UI保留原始高度、内容、滚动、裁剪、命中和输入条件。
4. 按独立价值选择删除临时测试、合并已有保障或例外保留最小长期合同；不为90组条目永久留下90套重复测试。
5. 清理临时测试、helper、资源和Demo/进程；最终对实际改动模块执行focused验证，专项义务独立完成。
6. 记录源码/差异、配置及作用域、适用分支、失败/通过结果、渲染/交互证据、测试处置及未覆盖平台。记录写入交付说明，不建立新的永久逐测试登记系统。

规范依据：[测试生命周期](../../../engineering/development/test-value-and-lifecycle.md)、[按影响验证](../../../engineering/workflows/affected-verification.md)、[主题验收](../../../engineering/development/theming/verification.md)。

## 5. 已有验证入口与缺口

| 现有入口 | 可以复用 | 仍须补的证据 |
| --- | --- | --- |
| `tests/AtomUI.Core.Tests/Theme/`的identity、Compiler、scope、transaction、bridge/cache测试 | 现行运行时合同 | 新Own版本、品牌样式协调与结果演进，不能假定旧用例已覆盖 |
| `tests/AtomUI.Generator.Tests/TokenResourceKeyGeneratorTests.cs`、SemanticParts与Registration测试 | typed key、生成、route、资产合同 | 新依赖元数据、跨程序集组合及有资格样式工厂 |
| Desktop的`ControlEffectiveGlobalBindingTests`、`ButtonFamilyTokenIsolationTests`、`TokenResourceBindingLifetimeTests` | 控件覆盖、隔离和绑定生命周期 | 各真实模板、状态、内部对象和动态实例的接线 |
| `TimelineSemanticStylePriorityTests`及各控件SemanticPart测试 | 专用Style与优先级合同 | 未覆盖Part/替代节点、实际渲染和恢复过程 |
| `DataGridTokenAlignmentTests` | 默认计算值对齐 | 不能证明公开覆盖经过Compiler到实际消费者；需S03反例和旧新名验证 |
| Gallery主题/ShowCase及可选包测试 | 页面、配置和已声明的控件行为 | 完整配方、编辑/往返、失败草稿及动态渲染 |

## 6. 验证命令与执行范围

以下是实施时的入口，**本轮没有执行这些产品测试或publish**。所有命令在`feature/custom-theme`的worktree根目录运行，只选择当次实际改动模块。

```bash
# 预览主题机制改动的验证范围，不等于已经执行
python3 scripts/verification/test.py plan --path src/AtomUI.Core/Theme

# 仅当Core主题机制与对应生成代码均实际修改时
python3 scripts/verification/test.py run --scope iterate \
  --path src/AtomUI.Core/Theme \
  --path src/AtomUI.Generator/DesignToken \
  --path src/AtomUI.Generator/TokenResourceKeyGenerator.cs

# Button家族的模块验证；涉及其他模块时显式增加实际路径
python3 scripts/verification/test.py run --scope iterate \
  --path src/AtomUI.Desktop.Controls/Buttons \
  --path tests/AtomUI.Desktop.Controls.Tests/Buttons

# 可选包消费链的模块验证
python3 scripts/verification/test.py run --scope iterate \
  --path src/AtomUI.Desktop.Controls.DataGrid

# 现行事务的精确合同入口；新版本验证另外按实际测试过滤
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj \
  --framework net10.0 \
  --filter 'FullyQualifiedName~AtomUI.Core.Tests.Theme.ThemeManagerTransactionTests'

git diff --check
```

触及注册/资产/AOT时，完成相应专项和真实Host验证；macOS Gallery发布入口示例：

```bash
pwsh -NoLogo -NoProfile \
  -File controlgallery/AtomUIGallery.Desktop/scripts/PublishToLocal.ps1 \
  -publishRootPath /tmp/atomui-custom-theme-aot-run \
  -runtime osx-arm64 -buildType Release -publishAot true
```

发布成功只证明对应产物路径，不代替启动、交互、内存或其他平台。新worktree按实际需要restore，不强行以`--no-restore`隐藏环境缺失。

本地不自行执行change/full或全量回归。全量只在用户明确要求或发布时运行；如果执行且失败，收集完整失败清单，逐项focused修复后只做一次最终全量确认。不能把零测试、skip、pending或未完成专项写为通过。

## 7. 交付状态和验收判定

Control/问题处置使用：未审计→静态核对→失败证据→已修改→运行验收通过；兼容保留/范围外另有明确理由。机制使用：提案→规格审定→原型失败/通过→实施→专项验收。文档到位不提升运行状态。

W6交付需逐项满足[入口的六项门槛](overview.md)，并同时报告当前规范采纳状态、各包/Host验证范围、全部剩余项和测试去留。当前90组矩阵均未标记运行通过。
