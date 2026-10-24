# CT-16 D4-C RangeDateViewer 页头几何执行计划

> 主agent使用superpowers:executing-plans逐项执行；用户要求不使用subagent。沿现有分支和目录，无新建分支／worktree授权，不自动提交。

**Goal:** 让范围viewer的每个页头对应实际日期面板，保持Compact布局、Content内容、周列、双面板间距及日期交互。

**Architecture:** RangeDateViewerTheme中三个页头当前固定读取PanelWidth；DatePanel的Content模式会清除固定Width，按实际内容测量，因此页头与各自面板宽度、第二面板起点分离。先量化几何，再验证AXAML内绑定实际面板尺寸是否能以最小改动表达合同；不修改Token公式、业务状态或DatePanel测量算法。

**Tech Stack:** Avalonia12.1.4／NET10及NET8、现有Headless／真实Skia宿主、当前Mac NativeAOT。

**Spec:** [日期家族计划](date-time-consumers.md)、[D4-A结果](date-viewer-global-results.md)，正式DateViewer合同及RangeDateViewer语义部件声明。

## 约束与验收

- D4-A原始窗口1560×1220、四个Compact／Content viewer；单viewer540×360／540×760，range960×360／960×760，ShowWeek及2026-10日期保持，不缩小内容、删周列或改变宽高规避。
- 每个可见header和对应DatePanel的左右边界一致；两面板间距保持有效PanelSpacing。Compact旧新默认画面须0 RGBA差异。Content对齐造成的默认变化须量化并按项目视觉规范交用户批准，未经批准不填视觉完成。
- HeaderTemplate、ShowHeader、SelectionUnit、正常motion、真实导航／范围输入及既有专用Style合同保持。单面板模板通过真实RangeDatePicker宿主核对，不能改内部PanelCount制造成功。
- PanelWidth仍是独立Own输入，保留名称、公式、类型、ID和slot；按实际消费者判断，不改成CellWidth派生。
- 语义定制只使用生成专用Style编译AXAML，查找部件仅用于读取；绑定优先放在ControlTheme，不新增代码节点赋值。
- 新测试在Temporary/CT16RangeHeader；有效RED／GREEN后删除。媒体登记在仓库外，AA／1 DIP、相同完整轨迹、无容差／遮罩；记录文字观察后清理。净新增常驻用例0。
- 第二轮Actions基于已提交1fe3358ec独立运行，不混入本分包尚未审查的变更。

## 执行步骤

- [x] 在原产品源码下复现Content宽度及第二面板偏移，Compact作正对照；记录完整祖先／裁剪／命中几何。
- [x] 验证三个header在AXAML中读取各自DatePanel实际宽度的最小原型，核对布局收敛、显隐、尺寸更新、Template及Style优先级。
- [x] 四viewer明暗视觉完整历史、功能配置／清除／HeaderTemplate、真实导航／范围输入及独立AA／1 DIP已按各自适用宿主执行；Mac NativeAOT72项另记，最终原生显示面像素仍属CT25，不混作通过。
- [x] 量化Content默认视觉差异并依项目规范取得具体修复批准；其他未解释差异继续CT25。
- [x] 删除临时源／工程／媒体／源备份，清理后日期337项及NET8通过，正式文档／结果／目录与作者自审已完成；提交另行审查。见[结果](date-viewer-header-results.md)。
