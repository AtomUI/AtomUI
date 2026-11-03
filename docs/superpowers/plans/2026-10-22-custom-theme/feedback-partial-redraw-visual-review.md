# CT15 局部重绘差异的限定归属审查提案

状态：工程反例、纯Avalonia managed/AOT定位、临时清理与清理后52项精确Timeline验收已完成；表列3个纯框架反例及24个当前Timeline恢复路径已获限定归属批准，其他边界保留。本文提议另建CT25/FW2限定调查边界，不修改既有FW1，不将任何非零数字改为PASS，不设通用像素容差。

## 本提案仅覆盖的证据

| 场景 | 规模/原操作 | 当前严格恢复结果 | 对照 |
| --- | --- | --- | --- |
| 纯Avalonia rail，opaque原色 | 640×240，2×160 rail、三圆点11×11；原色→#aa2299→同一原brush | 14像素/max30，bbox100,30,103,191 | A/A0；背景根重绘0；无AtomUI运行程序集 |
| 纯Avalonia rail，alpha原色 | 相同几何，原#0f050505 | 14/max30，相同bbox | 同上，managed/AOT10帧cross全0 |
| 纯Avalonia Opacity | 640×240，179×32圆角矩形及两重叠细条，细条1→.73→1 | managed/AOT各19/max14，bbox104,70,278,102 | A/A0；实际Opacity1；背景根重绘0；15帧managed/AOT全0；各20检查17PASS/3原恢复FAIL |
| Timeline四算法、八专用Style逐个独立往返 | 1320×760，六根两方向/三模式、Height300、原标签/长文/Button/static PathIcon pending | 8个icon/title恢复0；其余24个具体数据如下 | 所有32个A/A0、Style/Tag/关键值命中、实例与完整几何恢复；明暗item/rail4组采集值相同、背景根重绘0 |

| 配方 | item/wrapper/section每项 | header | content | rail |
| --- | --- | --- | --- | --- |
| Default | 各40/max1 | 22/max1 | 112/max1 | 76/max30 |
| Dark | 各33/max1 | 11/max1 | 88/max1 | 76/max27 |
| Compact | 各22/max1 | 15/max1 | 112/max1 | 76/max30 |
| Dark+Compact | 各16/max1 | 8/max1 | 88/max1 | 76/max27 |

Opacity差异位于原Button的圆角描边边缘；rail差异位于圆点/rail邻接覆盖边缘。纯框架模型独立证明局部与完整重绘不一致；不据此把所有控件非零、native Message3497/max2或未复现GC都自动归为同一机制。当前只提议上述明确列出的局部重绘路径保留到新FW2调查；既有CT12/13/14结论不改。

## 反证与未采用方案

普通纯Border、DashedBorder父Opacity模型恢复0，说明不能按“任意Opacity都会错”概括。真正独立的rail/圆点和Opacity重叠模型均含实际可检出的custom阶段，非空白帧或零用例。

官方12.1.4公开UseSaveLayerRootClip开关未消除rail反例（22/max58），关闭区域dirty clip仍14/max30；两者均拒绝作为修复，没有改产品或现有宿主默认配置。没有替换官方依赖，也没有给产品补强制全屏重绘以掩盖触发。Skia issue327877721页面需登录，未取得内容，不能命名为同一已知上游bug。

## 请求边界

仅申请“上述独立反例及24个当前Timeline局部恢复路径归CT25/FW2继续调查”。批准只改变这组待调查问题的归属，不批准像素差异正确，不扩大容差，不关闭整个CT15。原P3-B102/102/82/82 max1继续原账保留；当前新最小场景不能倒写旧完整历史为通过。其他家族、native画布/初始阴影、真实时钟、原失败/缺帧、平台及GC均不在本批准范围，FW1原问题保持，不授权提交。


## 最终工程门槛

23临时case/3源28395字节、446取证及339会话848665821字节已清理，audit findings=[]；最终Timeline52PASS/0Skip、1s。无产品运行源变化，复用相同源码NET8零警告21.10s；没有全量、分支/worktree、委派或提交。作者自审完成，非独立审查。完整数值、拒绝方案及早期夹具失败见[timeline-part-restore-audit](timeline-part-restore-audit.md)。


## FW2限定归属批准登记

2026-11-01 用户明确回复“仅批准表列路径归 CT25/FW2，其余保留”（request_user_input_async / call_58d9ca164cbe4d43b4254cc21caa6b70 / item 0）。仅feedback-partial-redraw-visual-review.md列出的3个无AtomUI独立反例及24个当前Timeline局部恢复路径归CT25/FW2继续调查；非零仍FAIL，不设置或扩大任何像素容差，不修改既有FW1。原P3-B102/102/82/82、其他家族/native/平台/GC及完整CT15均保留，不授权提交。
