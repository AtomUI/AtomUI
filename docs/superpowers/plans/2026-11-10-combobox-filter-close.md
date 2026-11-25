# ComboBox 过滤提交与离场稳定性修复

> 在现有 release/6.0 checkout 执行本计划；不建分支/worktree、不提交。

**Goal:** 选中提交正常更新业务值和绑定，同时保持候选过滤投影、自然高度、空态与滚动直到物理 Popup.Closed。
**Architecture:** 复用原有选中提交边界，将单层事件标记收敛为可嵌套的提交深度；过滤投影在提交/离场期间延后，物理关闭或同步提交结束时收敛。重开和 pinned 状态按实时查询投影，不修改通用 Popup 动效。
**Spec:** 用户 2026-11-10 确认的四点方案；原始 demo IsEditable=True、IsFilterEnabled=True、默认动效、340 宽，点击 Banana。

## 有限完成条件

- 鼠标/Enter/外部选中触发关闭时，绑定、Text/FilterValue 和现有 SelectionChanged 时序正确；3 行/104 高度保持到离场完成。
- Popup.Closed 后应用最新过滤结果；非过滤对照不退化。
- 正常输入、外部 Text/FilterValue 继续实时过滤；pinned 不冻结；无动效/零时长同步收尾；快速重开取消旧离场并应用最新查询。
- 不改 public API、模板尺寸/限高/滚动条/动画定义，不增加计时器、Dispatcher 延时、替代视觉树或固定高度。
- 所有临时测试完成前后验证后删除；最终仅运行 ComboBox focused 模块，必要的共享 Popup 测试用精确过滤；不跑全量。

## 原始证据

- 同一临时 demo Case5 切换 filter true/false，保持原始数据/宽度与动效。Skia 真实帧时钟采样：true 点 Banana 后 Text/FilterValue 先更新，visible 3→1，104→40，然后 Popup.Closed；false 保持 3/104 到 Closed。
- 原生采样在 RenderTimer 初始化阶段失败（-6661），未进入控件；不把该失败算作控件复现或原生通过。
- 源码根因：ComboBox.OnPropertyChanged 的选中回填文本触发 SyncFilterValueFromText/RefreshFilteredItemVisibility，发生在关闭请求之前；Popup 离场前 UpdateLayout 消费了已缩小的列表。

## 执行

- [x] 临时精确用例建立红证：提交回调和离场中的候选数/高度、关闭后最新投影、正常查询、pinned、同步关闭、重开。
- [x] ComboBox 提交协调、已呈现过滤条件与默认面板自动定位边界；同步该控件文档。
- [x] 临时用例转绿；原始 Skia 时序脚本与离场帧验收；一次独立审查。
- [x] 清理临时测试与媒体，执行最终 ComboBox iterate、成员布局和 diff 检查。

## Review Focus

- 选中值变化引起的嵌套 Text/FilterValue 不能提前刷新视图；直接外部查询不能被误冻结。
- 物理 Closed 回调在 businessOpen=False 时仍需收尾；零时长 Closed 可能发生于提交调用栈内。
- 快速重开必须恢复最新查询，旧 Closed 不回写新状态。
- pinned 与清除编辑文本时仍可正常刷新；模板更换/卸载不能遗留提交状态。

## 结果

- 原始 12 个临时 case：修复前 6 fail/6 pass；首轮修改后 12/12 pass。新增限高滚动与模板替换后 14/14 pass。
- 独立审查指出虚拟化边界：在过滤为 A*、限高 4 行的 60 项列表中，外部选择未实现的 B29。运行确认先闪现 B 行，补已呈现查询条件后仍因基类自动 ScrollIntoView 回收当前视图导致 136→8。采用标准 VirtualizingStackPanel 的内部子类，在提交/离场期间仅阻止自动定位，同时保留完整虚拟化/样式身份；最终 15/15 pass。
- 过滤条件快照由当前可见投影拥有，包含查询、启用态、filter/selector/text binding；在取消过滤或 detach 时释放。它为新实现容器提供真实旧查询，不增加延时任务、固定尺寸或第二套选择模型。
- 最新查询、快速重开、pinned、同步关闭、SelectionChanged 次序、限高 viewport/extent/offset/可见滚动条、模板替换均在上述临时用例中覆盖。
- 原始脚本与最终清理后模块结果待填。

## 原始动效复核与清理

- 原始 340 宽、IsEditable/IsFilterEnabled=True、三项水果、默认 Motion 保持。修复前 Text/FilterValue 回填后 3→1、104→40，再 Closed；修复后 after-click 时值已更新为 Banana，但视图仍为 3 项/104，Closed 时才成为 1 项。false 对照始终 3/104。脚本每帧断言，退出 0。
- 已打开 Skia before/exit/after 三帧：点选行反馈正常，离场维持完整候选列表和尺寸，结束后弹层消失。未取消动效、未固定高度、未复制视觉树。
- 初次全窗像素稳定等待因聚焦 caret 的动态像素超时，未算通过；改为观测真实 Popup motion 状态、覆盖其时长并驱动真实帧时钟后采样，不关闭 caret 或动效、不裁剪/遮罩。
- 15/15 临时 case 通过；原有 ComboBox 与临时用例合并精确运行 84/84 通过（6s）。临时类及其全部辅助方法已删除，永久测试净新增 0。
- 原生后端此前 RenderTimer -6661，未进入控件，不申报本次原生通过。生产 Popup.cs 未修改。

历史命令：

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~FilterCloseProjectionTests
dotnet run --project /private/tmp/atomui-issue-533-demo-GjRFoA/Issue533Demo.csproj --no-build -- --trace-filter-close --headless
python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/ComboBox
```

最终清理后模块结果由 `.artifacts/verification/latest.json` 和交付说明记录。

## 最终结果

- 清理后 `run --scope iterate --path src/AtomUI.Desktop.Controls/ComboBox` 为 iteration-passed：102/102，9.056s（含构建总耗时 39.412s）。只声明本模块范围通过，不申报全量。
- 成员布局 9 文件/3 个控件、问题 0；git diff --check 通过。生产 Popup 源码和动效定义无改动，无新 public API。
- 已有提交边界用嵌套深度协调，不以新增 ignore 标记/延时刷新掩盖事件重入；呈现条件值快照与默认面板定位策略分别负责新容器一致性与 viewport 稳定。默认元数据允许应用 ItemsPanel 覆盖，派生面板保留 VirtualizingStackPanel 的 StyleKey 和正常虚拟化。
- 独立审查唯一 P2 已通过实际红证确认并修复；未发起重复审查。15 个临时用例及 helper 已删除，永久测试净新增 0。
- Skia 真实默认动画与逐帧几何/候选断言通过，三张关键图已查看；媒体清理。原生当前 RenderTimer 初始化错误 -6661 属环境未验证项，不以软件采样冒充原生通过。
- 未创建分支、worktree 或提交，前序清除/间距修复和用户其他改动保留。

### 原生补验

原生环境随后恢复启动；使用同一个 demo、默认动效、同一 Case5 对照 filter=true/false，脚本退出 0。true：after-click 43ms 时 Text/FilterValue 已为 Banana，仍为 3 项/104 高度，Popup.Closed 257.1ms 时才变为 1 项；false：保持 3/104 到 212.6ms Closed。逐帧稳定断言通过。此前 -6661 仅记作失败的环境启动，不覆盖或删除该记录；当前原生验收已完成。

```bash
dotnet run --project /private/tmp/atomui-issue-533-demo-GjRFoA/Issue533Demo.csproj --no-build -- --trace-filter-close
```

交互 demo 进程保持运行供用户审查；脚本进程已退出。
