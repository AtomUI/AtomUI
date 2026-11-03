# CT15 Tour 消费者准备（2026-10-31）

仅只读准备；Drawer正在独占共享产物三版采样，不修改Tour产品/并行构建。输入7metadata/9Own仍在原137/274中，ButtonSpinner复用CT13，CT12/13/14既定批准与CT25/FW1不变，当前Own NeedsDecision3仅GroupBox。

## 真实输入

Tour、TourLayer、TourStep、TourStepsView、TourIndicator、DefaultTourIndicator、TextTourIndicator。Own9：CloseBtnSize、PrimaryPrevBtnBg、PrimaryNextBtnHoverBg、TourViewMinWidth/Height、HeaderColor、IndicatorSize、PopupMarginToAnchor、TourBorderRadius，现均有源码消费者，旧whole evaluator待V2R1；四算法36默认/type/key/slot须先冻结，不默认改值。

CloseBtnSize原FontSize×RelativeLineHeight；Prev背景ColorTextLightSolid alpha0.15；Next hover原ColorBgTextHover叠ColorWhite；Indicator6、边界圆角LG、最小200×120、Header原ColorTextHeading、PopupMargin原SpacingXXS，Own默认依赖不擅由其它Own替代。

## 复用与边界

公共Tour为TemplatedControl；Popup中ArrowDecoratedBox、Step/StepsView与Indicator各有独立模板资产，TourLayer是TopLevel VisualLayerManager共享遮罩，并已有真正LogicalChildren当前owner切换/退出与CrossRootsChanged。已批准的CT11动效、遮罩成员与popup翻转typed OneWayToSource桥复用，不重写结论；内部成员不可反射绑定的原因已有代码注释。

Tour语义包括popup.root/mask/section/cover/close/header/title/description/footer/actions/indicators/indicator等，route实际跨模板/逻辑content/独立root，不能只看声明就算专用Style命中。样例/临时验证须用生成的AXAML Style、关键Setter及无节点赋值回退断言。不存在消费者的例外不得凭名称臆造；先核对真实actual styles再修route。

## 下一执行门槛

1. 原源/36默认、原遮罩孔坐标/目标/步骤/索引及内容、Compact/RTL和Default/Primary状态反证。
2. 逐Own实际close/Primary按钮/indicator/minsize/偏移/圆角及热更新；局部Global、字体/Native、公开与内部owner角色，保持原popup位置/翻转/target滚动/裁剪合同。
3. 步进/前后/finish/close/点击禁用/键盘焦点与自定义indicator、打开期间换目标/集合/模板、关闭/重开/卸载的订阅与资源释放。现PrepareTourLayer的运行时绑定与自定义indicator投射须按现有生命周期取得反证，不能一律认为已正确或擅自重做。
4. 正常Skia旧新/AOT同完整历史、静态/持续态分开、独立A/A/1DIP/恢复零和实际输入；原失败/非零留账，不扩大容差或并入CT25/FW1。临时清理后精确Tour模块/NET8、五正式文档/CSV/总表/作者自审与具体限定视觉审查，不能用默认迁移或分包通过关闭整个CT15。


2026-11-01 当前工程接入见[tour-consumers-results.md](tour-consumers-results.md)：36默认同，7真实Global RED已接线；重挂/退休两有效RED修复后35聚焦通过，Own九分支、Primary/自定义指示器、专用AXAML Style、正常Skia/AOT、清理与最终门槛继续，不把共享机制或本分包当完整CT15。


## 最终普通Skia/AOT轨迹（2026-11-01）

源冻结前51聚焦通过、36默认精确同、最终15源/7控件0问题。真实普通motion、Skia/region dirty rect、原800×600/72×32目标/120×48封面及Original first/description内容保留。默认/12专用Style/Own九与0/Global/Native/Primary实际hover/鼠标和Enter导航、retarget/move与定时mask、指示器/集合/动作、两层模板/重挂/RTL/pinned外点和Escape/失锚/空集/恢复等，每算法53帧由list锁定：41主静态+8定时+4独立cal，四算法212帧。静态≥750ms+12同帧、同坐标RGBA零通用容差，定时0/64/128/256记录实际时刻。

每个算法在独立进程重放，原/候选/AOT完全同动作历史；原版若在已证缺陷中异常，按算法隔离记录退出/缺帧，不继续假称完整或拿别版填图。候选必须212齐全，AOT发布路径与执行路径使用同一变量并写SHA256，避免此前Drawer错路径重演。官方12.1.4不替换，Headless overlay宿主只复用现有测试的静态已知成员配置，实际AOT须发布运行后才算兼容证据。

初smoke正常失锚异常批、exit-fixed50帧/24全通过与增强keyboard53帧/27全通过均保留至取证判断后清理，不挑单轮消差。当前三版采样进行中；不修改产品或并行构建，不关完整Tour/CT15，不提交。


清理后最终验收（2026-11-01）：run.lock空闲独占后，Tour21+PopupPlacement8精确29PASS/0FAIL/0Skip（用例1s，命令32.08s）；NET8 Desktop Release 0警告/0错误（构建17.12s、命令17.29s），git diff --check通过。独立最终4文件与两个marker/计数文件已清理，原31临时/1448取证/614原会话均无残留。仅44明确静态待限定批准；原版72缺帧/18可比非零、14跨运行非零、平台/GC/极值和完整Tour/CT15保留，不扩大CT25/FW1、不改CT12/13/14既定批准，不提交/全量/分支/worktree/子agent。下一Flyouts4/11继续。


限定批准登记：2026-11-01，用户明确回复“批准表内44对，其余边界保留”（request_user_input_async / call_e83d092a248045288230fca221d69589 / item 0）。仅Tour表内44对明确静态变化获批准；其余原失败、缺帧、恢复/持续态、平台/GC/极值和完整家族/CT15继续保留，不授权提交。 Primary大min仍沿原生slide/重叠，不批准“极值不重叠”。
