# 精确自定义主题优化与重构任务路线图

> **For agentic workers:** 使用 superpowers:executing-plans 按已审查的子计划执行；只有用户明确选择委派后才使用 superpowers:subagent-driven-development。路线图中的待细化任务不能直接作为批量改码指令。

**Goal:** 把已确认的主题设计与90组审计条目转化为可追溯、可分批审查、可验证的基础设施及控件整改任务。

**Architecture:** 在现有ThemeManager、Compiler/Snapshot、Scope、原生ControlTheme及专用Semantic Part Style上演进。先核定Control/Token合同与失败基线，分别验证Own求值和品牌表现协调，再按共享消费与控件家族迁移。完整品牌、手动工作台和AI分别交付。

**Tech Stack:** C#、Avalonia、Roslyn incremental generator、AXAML、现有xUnit/Shouldly及仓库focused验证CLI。

**Spec:** [整体方案](../../specs/2026-10-22-custom-theme/overview.md)、[正式主题设计](../../../architecture/systems/theming/overview.md)、[强制研发规范](../../../engineering/development/theming/overview.md)。

## Global Constraints

- 固定工作位置为当前feature/custom-theme及其worktree；本计划不授权创建其他分支/worktree。
- 设计基线为5e3327575；实施时先检查后续差异，路径/符号变化后重新核对，不按旧行号盲改。
- Snapshot是有效Token值的唯一真源；ThemeManager统一提交，exact identity与现行scope语义保持。
- Global保持完整合法schema；Active Own、派生边及Part route逐项验收，不用Global消费白名单或抽样代替。
- 现行整体evaluator与新依赖默认值合同显式分版；XML v1、编译ABI v1不静默扩义。
- ControlTheme优先，Transitions位于style setter；Part使用生成专用Style，禁止代码找节点赋值兜底。
- Token CommitCore保持静默，Publish不可回滚；品牌表现结果另行报告，普通观察者异常不自动代表表现失效。
- 临时测试优先且真实执行，验证后删除；长期保障优先复用现有测试。只跑实际改动模块的focused验证，不自行执行change/full回归。
- 保持默认外观、输入/选择/焦点/滚动、原始尺寸/裁剪/命中合同；新增形态/API及兼容性变更须在对应子计划中明确。
- 视觉一致性是硬门槛：同环境、同配置、同状态下重构前后无视觉差异；执行[正式验收规范](../../../engineering/development/theming/verification.md#视觉一致性硬门槛)。新定制生效与原外观无回归分别验收，不能相互替代。
- 每批源码、控件文档、Gallery及验证一并交付审查；创建commit须另有用户明确指令，本轮提交授权不跨批次沿用。

## Review Focus

| 易漏输入/失败 | 应有行为 | 负责任务 |
| --- | --- | --- |
| 父品牌设置padding，重置子仅设置foreground | 子owner不残留父品牌padding，普通应用Styles仍按原生合同 | CT-07/CT-08；CT-02先记录现行scope对照，新增品牌反例在CT-07落实 |
| 显式0/null、旧名/新名跨层同时出现 | 存在状态不丢失；同层冲突、跨层正常覆盖 | CT-03–CT-06；CT-01明确逐字段合法性 |
| 热切换时已打开Native/Overlay及容器回收 | Token、字体、样式一致更新并释放，输入状态保持 | CT-08/CT-10/CT-14–CT-19；CT-02建立Host与既有实例基线 |
| 样式激活/恢复失败与普通发布监听器抛错 | 分开报告Token与表现；恢复失败不伪称旧品牌完好 | CT-07/CT-08/CT-23/CT-25，包含独立故障用例 |
| 修改字号/线宽后本地值、绘制缓存或Custom预设仍占用；默认及既有配置的视觉漂移 | 有效值到达实际几何与像素；同输入前后原外观无差异，实例/专用Style优先级有效 | CT-02建立视觉对照，CT-10–CT-21逐Control比较，CT-25/CT-26核对证据 |

## 1. 文档归属与当前状态

| 资料 | 用途 |
| --- | --- |
| [任务目录](tasks.md) | 27个任务包的依赖、源码边界、交付物和完成门槛 |
| [首批执行计划](readiness.md) | CT-00–CT-02的文件级步骤；先建立目录、合同分类与代表基线 |
| [问题到任务映射](problem-task-map.csv) | 90个审计ID各有一个主任务及必要协作任务 |
| [Control目录](control-inventory.csv) | 编译元数据交叉核定的720个对象及owner待决项 |
| [字段审查记录](token-contract-review.csv) | 95个Own终端的1,195字段分类提案，未决合同不删除 |
| [视觉安排](visual-coverage.csv) | 逐目录对象的owner、比较任务、维度候选和取证缺口 |
| [实际基线](baseline.md) | 代表失败、脚本化Skia A/A与检错正对照、性能采样及未完成边界 |
| [原问题矩阵](../../specs/2026-10-22-custom-theme/problem-matrix.csv) | 问题内容、原始位置、TH/EVT及证据状态的唯一过程来源 |

本目录只管理实施安排，不复制正式设计合同。27个任务包不是27个控件或27个已复现Bug；一个家族任务必须继续展开到CT-00目录中的每个适用Control。原矩阵仍全部待处置/待验证。

CT-00–CT-02的元数据目录、源码级字段分类/依赖、代表失败、Skia基线、原生Window/Popup Host及单场景生命周期样本已记录，已通过M0代表基线最终审查，允许下一阶段机制规划。脚本化视觉验收skill已完善；原生最终显示面无输入时的闪动保留CT-12/CT-25专项门槛，未声明通过。CT-03具体合同、CT-04生成与CT-05 Compiler机制已经实现并完成清理后的聚焦验证，正在做本批最终审查；其余任务包形成接口及测试明确的子计划后实施，不能直接批量改码。唯一总进度见[tasks.md](tasks.md#0-执行进度)。

## 2. 依赖与推进方式

~~~mermaid
flowchart TD
    A[CT-00 Control目录] --> B[CT-01 Token合同分类]
    B --> C[CT-02 代表失败及Host基线]
    C --> D[CT-03-CT-05 Own声明与求值]
    C --> E[CT-06 别名兼容]
    C --> F[CT-07-CT-09 品牌资格/协调/格式]
    C --> G[CT-10-CT-11 共享消费与动效]
    D --> H[CT-12-CT-19 控件家族]
    E --> H
    G --> H
    B --> I[CT-20 Custom逐属性核对]
    F --> J[CT-22 完整品牌]
    H --> J
    I --> J
    G --> K[CT-21 Gallery工具壳]
    F --> L[CT-23 手动工作台]
    K --> L
    J --> L
    L --> M[CT-24 受限AI独立交付]
    H --> N[CT-25 平台/AOT/性能分批验证]
    F --> N
    L --> N
    N --> O[CT-26 清理兼容及最终交付]
~~~

图中箭头表示完整交付依赖，不要求所有工作串行等待：

- CT-06、CT-07及CT-10/CT-11在CT-02后可以分别推进；同一文件/合同的修改仍串行协调。本路线图不自动启动多agent。
- CT-12–CT-19只等待本家族实际需要的机制；例如DataGrid优先依赖CT-06，ColorPicker与Skeleton依赖CT-05，不能为了赶批次复制临时算法。
- CT-20随家族迁移逐属性关闭，不在最后做全库selector替换。CT-25从每个机制和首批Control开始收集证据，最后汇总，不把AOT或真实Host验证全部留到收尾。
- CT-22的小样在机制阶段用于检错，最终完整品牌依赖全范围控件验收。CT-23可以先构建手动编辑纵向切片，完整验收等待CT-22。
- CT-24以手动闭环通过为前提，独立验收；核心主题/手动工作台交付不冒充AI已完成，也不被尚未接入的生成服务阻塞。

## 3. 阶段出口

| 阶段 | 任务 | 可审查成果与门槛 |
| --- | --- | --- |
| M0：边界和基线 | CT-00–CT-02 | Control主目录、逐字段分类、真实Host、代表失败及视觉A/A基线；每个Control有视觉比较安排，改动前必须完成其前证 |
| M1a：Own与兼容 | CT-03–CT-06 | 版本化声明/默认依赖/存在位/别名合同，生成、Compiler、跨程序集及惰性缓存证据 |
| M1b：品牌表达 | CT-07–CT-09 | 原生资格与优先级原型、跨根激活/恢复结果、版本化配方格式；失败则修订设计，禁止批量接入 |
| M2：共享与代表控件 | CT-10–CT-13、CT-17–CT-19中的代表切片 | Button、LineEdit/Select、Skeleton、Timeline、DataGrid、ColorPicker沿真实链路通过 |
| M3：全部控件 | CT-12–CT-20 | Control主目录每项有逐Control结论，Active Own/依赖/Part及尺寸状态完整闭合；既有外观前后无差异 |
| M4：产品闭环 | CT-21–CT-23 | 三套品牌及边界配方、手动预览/应用/保存/往返等价 |
| M5：交付证据 | CT-25/CT-26 | 每批focused、真实平台/AOT/性能与兼容证据，临时产物清理、残余项明确 |
| M6：生成入口 | CT-24 | 受限输出、取消/过期、非法值和服务接线单独通过；可与M5分开交付 |

## 4. 任务开工与完成

开工前：阅读对应正式设计和问题矩阵；核定具体Control/文件、状态owner、API影响、可失败验证、平台范围及临时验证位置。跨接口任务先给出明确的新签名和版本迁移，再进入代码实施。

完成时：同输入失败→修复→通过，说明测试去留并清理，执行最终实际改动模块验证；报告默认行为/主题兼容、实际Host、剩余问题和文档同步。任务完成不能自动把整组问题或整个家族标为通过，必须覆盖其所有子项。

视觉验收贯穿全部任务：CT-02先证明基线采样可重复，CT-10–CT-21每批改码前为实际受影响Control采集前证、改码后做同条件比较；共享机制须覆盖所有受影响消费者。CT-25补真实Host/动效证据，CT-26确认无未解释或未经批准的差异。各批不能通过更新基线消除失败；原来未生效的配置若会产生视觉变化，单独列出具体输入及兼容影响供审查。

首批由主agent在当前会话顺序执行；阶段进度和真实执行结果见readiness及baseline。临时取证不表示产品机制已经重构，不修改尚未闭合问题的状态。
