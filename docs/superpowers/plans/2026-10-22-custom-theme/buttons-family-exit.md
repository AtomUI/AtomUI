# CT-12 按钮家族总出口执行计划

> 使用superpowers:executing-plans，由主agent在既有feature/custom-theme及当前目录执行。用户已明确要求开始CT12，不新建分支/worktree，不提交或推送，不使用子agent/CUA。

**Goal:** 关闭CT12九个公开owner及五个Own终端的按钮主题合同，保留明确的CT20/CT25边界。
**Architecture:** 复用已完成CT11、V2默认求值、ControlTheme/native优先级和现有渲染链。先逐字段及真实消费路径审计，已有切片不重做；缺口先取得有效RED，再做最小修复并验证。
**Tech Stack:** 官方Avalonia12.1.4、NET10精确filter/真实Skia、NET8及当前Mac NativeAOT。
**Spec:** [原家族计划](buttons.md)、[逐owner出口审计](buttons-exit-audit.md)、[总任务](tasks.md)、正式主题消费/验收与测试生命周期规范。

## 全局约束

- CT11的100个未提交路径作为本次开工输入保护，不重置或重复实施；先记录全部工作树文件hash。Git HEAD仍4560091de。
- 范围九owner：Button、DropdownButton、HyperLinkButton、SplitButton、IconButton、ToggleIconButton、OptionButton、OptionButtonGroup及Common.SimpleButton；14条metadata包括5个抽象合同。不新增identity，不把其他家族派生按钮无条件吞入本包。
- 五Own终端133字段：Button50、Dropdown46、Hyper17、Option19、Split1；五历史保留字段按已批准分类，Active/Derived须有实际消费或依赖证据。
- SimpleButton保持已核定原生兼容包装。SplitButton固定Custom宽140的溢出保留CT20；官方框架长寿命Flyout关系、已记录合成差异及其他平台仍按具体CT25合同处理，不泛化豁免。
- 现有状态/尺寸/焦点/滚动/弹层保持；正常动效视觉单独验证，不以关闭motion或重建页面回避差异。新可见效果仍交具体审查。
- 新测试默认临时，放所属项目Temporary/CustomThemeButtonsExit；媒体仅登记的系统临时目录。验证结束删除，再精确受影响模块验证。禁止full/change/affected-wide。

## 开工审计

请求类型：已有主题迁移的完整出口，属广域消费/正确性审计。主要状态owner是实际公开控件；Option Group拥有选择模型，容器投影checked；Split组合拥有主操作/下拉意图，两个子Button保持其原生属性优先级。

已有证据：V2及默认依赖、字重继承、六owner字体族、Dropdown独立23字段、Hyper loading/icon-only/danger/root paint、SimpleButton边界、按钮持续motion等切片已完成。CT11已关闭，不再作为阻塞。

生命周期：Button的scope/继承字体/实际Flyout target资源绑定均有detach释放；Dropdown/Split跨根Flyout事件与relay绑定按替换/关闭/卸载释放；Option Group需补生成容器/回收和内容替换的最终证据。运行时创建的Flyout及跨兄弟目标不能仅因代码绑定而机械迁AXAML；真正模板可达的绑定仍遵循AXAML优先。

源审计结果：无新增产品修改。Button/Dropdown许多颜色/阴影Own由ButtonThemeTokenReader及Variant*属性读取，不是AXAML漏接；Option ButtonPadding和LineHeight进入DefaultFor依赖，不能当作无消费者。当前不能以source-only引用表关闭逐字段/状态出口。

初始化/结构：Button1378行、Split1040、Dropdown373，现有文件无需按长度拆分；若触及代码，行为修复与成员重排分开，完整成员审计。潜在API/渲染变化仅限有效RED证明的主题消费修复，缺证不改。

## Review Focus

1. 控件/组合/独立公开子Button identity与Own表不能串用。
2. 同值显式输入、透明色/0、缺省及清除不能以值猜存在位。
3. 同实例切换size/shape/variant、checked/loading/disabled与本地属性不能残留。
4. 真实输入、生成容器替换及跨根Flyout保持业务状态与可释放关系。
5. 历史已批准差异不能覆盖新的像素范围；最终原生显示面证据必须对应本次产物。

## Task 1：字段与消费出口

- [x] 读原计划/九owner出口/133字段分类及现有源边界。
- [x] 记录当前完整工作树输入hash；更新session和ledger，CT11状态保持完成。
- [x] 对128个有效字段（含Derived/Optional）及5兼容字段核对实际目标、适用状态/尺寸、依赖和旧证据；C#变体读取与AXAML分别追踪。
- [x] 在真实Window中验证逐字段显式值、0/透明/清除、owner隔离，生成审查可读的精简结果。
- [x] 对真正缺口建立有效RED，记录根因与具体最小候选；同断言GREEN、默认原新视觉及必要AOT，不修改误判对应产品。

## Task 2：九owner业务与定制矩阵

- [x] 真实鼠标/键盘/焦点，checked/loading/disabled，shape/variant/size恢复、Icon/Content替换及父字体变更。
- [x] Button/Dropdown/Split实际展开和关闭，Option生成容器/回收；专用Semantic Style在AXAML声明式命中，不用code-behind设置目标样式。
- [x] 同实例scope A→B→A、兄弟owner、Local/Style优先级、重模板/分离/重挂与GC。

## Task 3：视觉、原生和交付

- [x] 基于本次开工输入采明暗适用分支、AA/1DIP与正常动效；必要修复取得相同历史前后精确对照与具体审查。
- [x] 当前Mac最终产物managed/AOT及实际原生显示面稳定性；不把属性值检查冒充像素或跨平台。
- [x] 删除所有本包临时源/驱动/媒体，最终精确模块/NET8/成员audit/链接/diff检查；作者独立步骤自审。
- [x] 更新正式控件文档、必要Gallery合同及CSV/task状态；全部通过后关闭CT12并汇报，不自行提交。

字段/业务156PASS；未发现新的产品断链，没有本轮产品RED或实现修改。8临时源已删除，清理后Desktop162/Gallery16/Rendering1及Release NET8通过。原生最终无输入采样与原始异常处置尚待完成；不得提前关闭CT12。

当前：实际NSWindow managed/AOT各482检查、52帧，全部阶段内/恢复0，跨运行12对0，指针/按下0、焦点保持；8临时测试、632外部工程文件及563取证目录文件均已清理。复核原始问题表发现V11未被128字段矩阵覆盖，撤回“仅剩两对”的结论。

## V11补充门禁

- [x] 4项实际探针确认预设色Global高度/颜色有效，Blur3/Spread0固定且PrimaryShadow不控制该分支。
- [x] 用户核定公开BoxShadow属性入口，不改变既有Token语义；已写[合同](../../specs/2026-10-22-custom-theme/button-box-shadow.md)与[执行计划](buttons-box-shadow.md)。
- [x] 按选定公开合同完成设计/计划、有效RED与最小候选，实际Style或Own优先级/清除及默认像素验证。
- [x] 根据实际改动重新验证清理后模块、必要AOT/Native并完成两对细线范围核定，再关闭CT12。

V11候选已落地，2项有效RED→GREEN、18补充通过，明暗18对默认前后0。显式定制5390像素、清除24像素/通道差1及单变量damage定位见[候选结果](buttons-box-shadow-results.md)；等待当前产物原生终验与具体范围处置，不能沿用修改前原生证据。

2026-10-30接手工程收尾完成：当前产物1236/1236、52原生帧及12跨运行0；临时清理与最后189项/NET8/成员通过，见[最终工程结果](buttons-box-shadow-finish-results.md)。V11不再待实现；唯一视觉批准见[汇总表](buttons-box-shadow-final-visual-review.md)，无其他待开发项。不提交。

最终完成：2026-10-30，用户已明确回复“批准本表限定范围，保留 CT25”（request_user_input_async / call_a2fe796a8df0481aadd7e4393060b87e / item 0）。公开BoxShadow工程、默认原生验证、清理后189项及限定视觉批准已全部完成，CT12在既定批准范围完成并关闭。原始24/max1、两对3968/max2及其他既定历史仍按CT25保留，不设通用容差，不扩大其他平台/背景/恢复模式，无Git授权。 依据[最后结果](buttons-box-shadow-finish-results.md)与[批准表](buttons-box-shadow-final-visual-review.md)。
