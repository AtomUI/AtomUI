# CT19 C5：ColorSlider异步位图的最新请求与宿主生命周期

> 使用superpowers:executing-plans，在当前会话独立执行，不创建分支／worktree／subagent，不自动提交。

**Goal:** 令实际Alpha／颜色滑轨只呈现当前输入和布局对应的位图，较早计算晚结束不得覆盖当前结果；保持原尺寸、色值、thumb、指针及空草稿行为。

**Architecture:** 先用临时worker门闩和实际控件Task观测证明完成顺序；生产方法仅引入请求版本并保留原attached判定和位图替换释放路径。ControlTheme不承担运行时像素计算。临时挂钩全部删除后复验。

**Tech:** 现有Task.Run／PooledList、Avalonia Bitmap和真实ColorSlider／Window，Headless-Skia及Mac managed／NativeAOT。

## 证据与Review Focus

C4补充216帧中，旧版Dark Gradient空草稿的main／AA相差1722像素，Alpha滑轨闭区间x53–268、y380–387：同一点(266,384)分别RGBA(247,247,247,255)和(247,1,1,255)，是白／红两种渐变，而非小数圆角噪声。ColorSlider.UpdateBackgroundAsync先拷贝输入在后台生成，完成时只检查attached，没有请求顺序守卫。首次C4补充还出现5像素输入框差异；该问题另行核对，不复用C1／C2容差。

- 仅控制worker完成顺序，不删除真实模板、scope、窗口约束、像素生成或异步路径。
- 比较Bitmap实际像素及尺寸，而不是只断言颜色属性；最后输入和布局拥有可见结果。
- 旧任务不能dispose新位图；PooledList在成功／过期／detach路径均释放。
- 释放当前epoch、重新挂载仍生成当前内容，不能以忽略全部任务隐藏故障。
- C4已通过的240帧和Native276项不代替本修复；补充采样在本问题解决后重新资格核对。

### Task 1: 原顺序反例、最小修复及完整消费复验

- [ ] 在Temporary/CT19SliderBitmap暂停较早红色请求，放行最新白色／尺寸请求，再结束旧请求；真实Bitmap保持最新白／尺寸。另查detach／reattach；旧代码必须有效RED。
- [x] 按成员审计先保存纯布局等价证据，再增加最小请求版本并保留原释放边界；不改变Token、模板、事件或输入几何。
- [ ] 删除门闩和Task观测挂钩，用真实公开两picker重放C4补充完整历史和独立AA／1DIP／恢复，区分新的预期像素与合成问题。
- [ ] Mac managed／NativeAOT、清理后的精确ColorPicker模块、NET8和成员审计；文档同步，默认视觉变化单独交审，不自动提交。

## 原顺序反例及最小候选

两项受控反例均有效RED，3.1661s。先挂起红色旧请求，完成白色新请求，再完成旧请求；当前Hsv为A1／H0／S0／V1，原版实际RGBA(255,0,0,253)，缩窄case预期180却被220×8旧图覆盖。候选仅增加请求版本，完成后只有最新版本且仍attached才替换自身位图；两项GREEN，3.1802s。缩窄实际180×8、RGBA(255,255,255,252)，不缩窄220×8、RGBA(255,255,255,253)。没有新增事件、Token、绑定或宿主支持。

ColorSlider只补公共事件region，Roslyn全部成员／imports token完全相同、审计零问题；纯布局patch单独保存。两份临时控制点及Task观测已移除，Helpers逐字恢复原SHA，临时测试删除；后续正式模块与完整UI复验继续。没有为无前证的跨宿主/DPI行为增加新override，原attached检查与PooledList释放保留，不将本次版本守卫称完整宿主生命周期验收。

删除挂钩后的公开两Picker、明暗、main／AA／1DIP补充108帧：候选36组A/A全0、36组1DIP全检出、8组恢复0；原候选5像素未出现。原始Dark Gradient AA的draft-default／reset错误红色Alpha变正确白色，各1722像素，仅x53–268／y380–387；与C4颜色／线宽改动分开记录，详见[C4补充审查表](color-picker-drawing-results.md)。未通过隐藏渐变、修改值、删除确认区或等待旧请求重试来消差。

真实Mac NSWindow scale2、明暗和两类公开owner的managed／NativeAOT各404项通过。新增32次公开颜色／空值及宽度快速往返，实际Alpha Bitmap的末端RGB逐次为当前白／蓝，444×16与真实track物理尺寸一致。没有反射读取私有字段或使用测试门闩。原生最终像素及完整生命周期仍不以这些检查替代；清理后focused／NET8尚待执行。


### 挂载补充（在前述404项之后发现）

同一真实ColorSlider先显示白色，脱离视觉树后改Blue并等待该次真实后台Task结束，再以相同Bounds挂回。请求版本守卫仍显示White，有效RED1项3.3488s。挂载时重新计算运行时位图后同例GREEN1项3.0173s；模板、Bounds、Color、指针和资源绑定均保持，不靠属性重复赋值触发。ControlTheme无法自行生成运行时像素，故由现有visual attach边界发起更新，继续使用同一版本守卫／attached判定和PooledList释放路径。

临时Task观测字段、五处观测赋值、临时测试与旧源备份均已删除。先前404项针对添加挂载更新之前的源码，不认证本补充。新Mac原生探针最初错误地把独立GradientColorPickerView.Value赋值等同于选择活动节点，实际上其未激活stop时Hsv仍为White，故该预期不能当产品失败；本补充原生限定验证公共ColorPickerView的同一实例脱离改色／返回，原来两类picker正常会话的404项范围不扩大，Gradient完整选中状态生命周期继续家族验收。


挂载补充的独立60帧：同一公共ColorPickerView、800×620、Mac arm64 Headless/Skia scale1，明／暗各按White默认→离树Blue并回树→离树Green并回树→离树Orange并回树→White恢复；新旧各main／AA／1DIP独立完整历史。20组A/A全0、20组1DIP全检出，4组同实例恢复0，默认／最终恢复四对新旧全图0。Blue／Green／Orange六对各1722像素，仅闭区间x282–497、y278–285的Alpha背景；30组公开值、实际Bitmap尺寸、slider Bounds／矩阵、主题／客户端／scale逐项一致。原图该条保持White，新图依公开值更新Blue／Green／Orange，末端像素逐项记录。已查看明暗旧新及全图差异，没有遮罩／阈值或重绘策略改动。

同一最终源码的Mac NSWindow scale2，公共ColorPickerView明／暗managed／NativeAOT各82项通过，包括保留实例脱离改三种颜色再返回、实际Alpha Bitmap RGB和物理尺寸、选择环2→6→0→2及原生实际变体。原生宿主和临时Task观测工程均已删除，媒体留到视觉审查后清理。NET8 Release零警告／错误、ColorBlock和ColorSlider两文件成员审计零问题；Core资源原两模块17项、Desktop原三模块23项通过。补拍临时源删除后的Desktop最终检查仍需执行，产品源SHA与上述Core／NET8输入相同，不以此前临时assembly认证清理后的Desktop输入。

补拍发现另一独立旧问题：公共ColorPickerView离树改色后，HEX文字仍#FFFFFF，旧版与候选完全相同，而Spectrum／公开Value已改。该差异不由本次Alpha重挂更新造成，不把它写为输入区已修复；另列CT19 C6继续检查ColorPickerInput的挂载／有效输入同步和实际光标、格式、确认态。C5只申请上述六对Alpha变化，不代替公共View、Gradient活动节点或完整家族生命周期验收。
