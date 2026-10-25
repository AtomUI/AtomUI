# CT-17 绘制控件执行计划

沿既有 `feature/custom-theme` 和当前工作目录执行，不创建分支/worktree，不自动提交，不使用子 agent。范围按既有目录补齐 Skeleton、四种 Progress、Rate、Slider、Switch、Separator、Watermark、BorderBeam 及实际内部绘制目标；一个线形修复不代表整族验收。

## P0 普通线形 Progress 的布局与文本缓存

先处理第三轮 CI 已复现的 indicator 右缘601而 owner宽600。`AbstractGeneralProgressBar.GetExtraInfoRect` 在模板路径采用实际 layout rounding 尺寸计算锚点；保留非模板自绘路径。默认明暗695／696像素的三处1 DIP文字位移已获用户批准，见[审查](progress-rounding-results.md)。

补充同一实例字体、数值范围与格式更新：度量必须使用实际 `Percentage`，与标签格式化一致，完整包含 FontFamily／FontStyle／FontWeight；相关属性变更后失效文本尺寸、stroke及body布局。以 DesiredSize 提供字体所需交叉尺寸，不因字体变化覆盖用户 Min/Max。按真实渲染缩放1／1.25／1.5／2往返，同时验证 rounding 开关。保持原窗口、横纵尺寸、完整恢复历史与正常 motion；用有界条件确认动画实际到达终值，不能假定650ms覆盖800ms的Value transition。

原始七控件默认600→360→600、纵向240→160→240逐帧对照；已有字体及格式定制另外建立旧新、独立A/A和1 DIP对照。历史Headless合成路径7像素变化已获用户限定批准，后续单变量region宿主复核42对A/A全部0，不消费该容差；见[后续结果](progress-token-results.md)。补充源与媒体已清理，精确Progress类、NET8及成员审计已通过；完整形态及其他绘制入口继续下包。

## P1 Progress 四形态 Own／Global

从 `ProgressBarToken` 13个既有字段逐项核对，迁移V2时保持身份、资源编号、默认类型和值；`CircleTextColor`、`LineBorderRadius` 及未消费的步骤／动效字段按既有T13明确实际接线或兼容分类，不能直接从编辑入口删去后称完成。四个公开owner分别验证字体、颜色、描边、圆角、间距和最小尺寸，保持本地值、专用生成Style及继承优先级。线形、步骤、圆形、仪表盘分别覆盖范围归一化、异常／完成／禁用、方向、分段／缺口和动态缓存。

[P1-A](progress-token-results.md)完成13字段默认计划及两项接线；[P1-B](progress-circle-results.md)继续圆形动态尺寸、本地字号、实际信息内容互斥和位置更新，[P1-B3](progress-circle-fit-results.md)已完成信息适配、完整字体失效及异常退出刷新；再处理普通／步骤间距等其余字段。分包完成不计完整家族验收。

## P2 其余绘制目标

每个实际Control逐字段核定有效Own依赖、可选Global、public属性与自绘缓存；保留固定几何与内容语义，不把所有常量都变为主题。Skeleton核对端点helper、梯度和连续周期；Rate／Slider／Switch保留输入、焦点、命中与范围；Separator保留线宽／文字；Watermark保留内容、旋转与重建成本；BorderBeam保留时长、范围和独立owner。涉及模板样式的绑定优先AXAML，运行时计算保留可解释的代码边界。

## 共同出口

各包均先有效失败，再最小修改、目标定制生效及原画面对照，实际输入、生命周期与性能按风险验证；当前Mac NativeAOT和其他OS／Browser分别留证。不增加常驻测试，临时验证及专用产物清理后运行精确模块检查。正式overview／implementation／token／semantic-part／changelog与Gallery示例按实际变化同步，目录字段和全家族状态只在证据满足时更新；完整CT-17仍开放。
