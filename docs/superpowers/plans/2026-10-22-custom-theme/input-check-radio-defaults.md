# CT-13 CR1：CheckBox / RadioButton 有效 Own 默认依赖

主agent、当前feature/custom-theme及目录，承接已批准Own V2机制。无新API/字段/identity、无提交、无分支/worktree/clone/NuGet发布。Common内部indicator仍归公开CheckBox/RadioButton owner。

## 合同与边界

- CheckBox四字段迁移V2 rev1，保留默认：CheckIndicatorSize=有效Global ControlInteractiveSize；CheckedMarkSize依赖有效Own CheckIndicatorSize×0.6；三态标记继续Global FontSizeLG/2，TextMargin继续UniformlyMarginXS左右。
- Radio七字段迁移V2 rev1，保留默认：RadioSize=有效Global FontSizeLG；DotPadding=4；DotSize依赖有效Own RadioSize、DotPadding及有效Global LineWidth，原式`size-(padding+lineWidth)*2`，不新加clamp；余颜色/边距默认保持。
- 显式下游、0及清除保留原生存在位合同；不将未配置与值相等混淆。RadioBgColor消费裁定、Check/Radio全Global/语义部件/全家族与平台出口另列，不能用默认迁移代表完成。
- Radio实际自绘使用RadioDotEffectSize；必须证明新资源、StyledProperty和真实绘制输入一致更新，原checked/disabled/unchecked公式及正常动效、波纹、焦点/组选择不变。计算含状态及尺寸算术，沿现有唯一计算函数补齐失效；不新增缓存/旗标/中继。可等价AXAML绑定按强约束迁移，无法等价的运行时计算写明原因。

## 执行与证据

- [x] CR1-R：临时真实Window失败：Check上游尺寸→勾选标记；Radio上游尺寸、padding→DotSize；直接DotSize动态0的实际自绘尺寸；明确下游与清除、兄弟身份、选择/输入和禁用分支。
- [x] CR1-B：保留11字段明暗默认值、资源key/公开ID/slot；同历史默认/鼠标/键盘/选中/三态/禁用前后完整帧，AA0、1DIP检错、恢复0。正常动效不关闭来消差；变化参数独立比较。
- [x] CR1-I：每字段一个静态默认函数，标注三条有效Own依赖，sealed partial+IControlTokenDefaults；删除旧整体evaluator与空ctor。由实际RED确定Radio失效修复，不改原公式或状态路径。
- [x] CR1-V：同实例A→B→A、显式下游/0/清除、真实输入、专用Style优先级及模板/重挂/释放，当前Mac AOT/NET8与原参数兼容。存在原始视觉/资源失败则保留计数并定位，不能削弱断言。
- [x] CR1-C：删除临时源码/驱动/媒体及专用产物后，精确CheckBox/Radio模块验证，成员/文档/CSV/总表及作者自审；必要视觉变化单独明确审查，新提交仍另审具体快照。

Review focus：checked、unchecked（DotSize×0.6）、disabled（RadioSize-DotPadding×2）互相独立；3个输入的live更新必须到达Render；非正尺寸不添加新语义；三态不误读CheckIndicatorSize；Global/Own及public节点职责不混淆。

CR1：11默认V2与三条有效Own边；原5FAIL→最终6PASS，明暗22条默认/key/ID/slot一致；managed/AOT各51，清理后原51项/NET8零警告；直接AA/恢复0，精确配置变化与自然90/76、3/6已单独批准，CT25合成专项及完整家族保持开放。
