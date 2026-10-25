# CT19：透明棋盘派生画刷生命周期执行记录

> 使用 superpowers:executing-plans，由主 agent 沿既有 checkout 执行；遵守用户不使用 subagent、不创建分支／worktree及不自动提交的要求。

**Goal:** 既有公开主题配置在两类 View 离树改动、同模板重挂后到达实际棋盘画刷。

**Architecture:** 保留 ControlTheme 的资源绑定；共享 AbstractColorSlider 与 ColorBlock 依当前标量输入更新既有有界缓存画刷，不把派生状态刷新绑在挂树条件上。

**Tech Stack:** Avalonia12.1.4、.NET10/8、Headless/Skia 与真实 Mac managed／NativeAOT。

**Spec:** 既有 CT19 Own 消费与资源恢复合同，见 [任务目录](tasks.md)、[主题验收](../../../engineering/development/theming/verification.md)和 ColorPicker token.md；本项为该合同内的缺陷迭代，C9焦点短设计已获独立批准，其视觉与限定噪声另审。

## 合同与根因

ThemeConfigProvider 继续配置 ColorPicker Own identity 的 TransparentBgSize；两类 View 保留完整模板、普通／渐变／Alpha 滑轨、尺寸和正常动效。默认及在树行为不变，不新增公开签名、Token、Part、绑定或订阅。派生 DrawingBrush 由既有缓存函数生成，不能由 AXAML 标量绑定代替。

有效临时检查四项：普通／渐变 View 在树改 6→10 为两个正对照；整个 provider 子树离树后改为10再重挂，两项均失败，实际属性10、DrawingBrush tile6。首次探针把 Own 配给仅有 Global 的 View identity，以及随后把隐藏未物化节点当作可见消费者，是测试准备错误，分别保留记录；有效反例仅使用当前真实可见消费者，至少两条滑轨且同实例不替换。

根因是 AbstractColorSlider.OnPropertyChanged 只在挂树时更新 TransparentBgBrush；离树时有效输入已变，返回时未必再次发同值通知，旧画刷继续使用。不能通过强制重套模板或延迟刷新规避。

## 执行步骤

- [x] Temporary/CT19Checkerboard 建立四项真实公开配置检查，确认2失败／2通过，保存源SHA和原始日志。
- [x] 先独立整理该文件既有四项 internal 注册／wrapper 区域及注册顺序，成员指纹验证无语义变化；与功能改动保留独立快照。
- [x] 只解除派生画刷更新的挂树限制，保留原始非空 solid brush 条件和既有有界缓存；同四项转GREEN。
- [ ] 扩展颜色、0／恢复、多次同模板离树、identity／作用域隔离与GC；实际明暗同轨迹默认及配置重挂比较、独立A/A与1DIP，对新增可见差异逐项审查。
- [ ] 当前Mac managed／NativeAOT检查实际资源与重挂；清理临时源／驱动／媒体后，仅运行 ColorPicker 精确模块、NET8 Release、成员审计与diff check；作者另步自审，不代替独立审查。

Review focus：默认／在树不漂移；两类具体滑轨共享根因；off-tree恢复同实例；明确0及覆盖撤回；新计算不建立订阅或无界缓存。产品已按最小根因修复，视觉审查仍待完成；后续 Git 仍需具体快照批准。

同源扩展：ColorBlock使用相同挂树判断。先扩展四项真实公开配置检查，在已修复slider的候选上取得两项ColorPreview反例（属性10／tile6），两个在树对照通过。ColorBlock解除同一判断后，含0／撤回／重复重挂的全部棋盘场景与C9扩展共20项通过，真实Mac managed388项也包含ColorPreview／各具体slider；原生最终像素、视觉审查、NativeAOT与清理后模块仍待完成。ColorBlock现有成员审计1文件1Control零问题；AbstractColorSlider的纯布局中间快照成员指纹完全一致，注册初始化顺序未变。

最终补充：两类View独立Provider的Purple／Cyan／Orange颜色、size10、撤回8及邻接Provider14的原画刷引用均通过；含C9的24项通过后删除三份临时源，清理后ColorPicker27项和NET8 Release零警告通过。完整明暗取证、真实Mac及NativeAOT首轮输入缺口详见[结果](color-spectrum-keyboard-results.md)，31对有限自然差异单独送审，暂不打勾视觉及整个切片出口。
