# Dialog 嵌套弹层焦点修复计划与证据

> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 从 Dialog 宿主消除内容弹层点击时的额外失焦和激活时的内容卸载，保持选择器与普通窗口一致。

**Architecture:** Dialog 隧道 pointer handler 只维护 presenter 激活；实际点击焦点交给 Avalonia FocusManager 在目标阶段按视觉祖先分配。DialogOverlayLayer 激活仅移动已有子节点顺序，不 remove/add。Picker 不新增命中特判、pointer-over 逃生口或延时。

**Tech Stack:** AtomUI 6.2.13、Avalonia 12.1.5、net10.0，当前 release/6.0 工作目录。

**Spec:** [Issue #532](https://github.com/AtomUI/AtomUI/issues/532) 与本会话“这个是一类问题，请仔细分析从源头修复”。原始复现与下文为唯一证据记录；临时 demo 位于 /private/tmp/atomui-issue532-demo，修复验收后删除。

## Global Constraints

- 当前分支和工作目录；不创建 branch/worktree，不提交，不触碰已有无关改动。
- 不改变 Public API、Token、主题、默认值、尺寸、动效、light-dismiss、确认语义。
- 不扩大到全量或 affected-wide；清理临时测试后执行 Dialog 模块 focused 验证。
- 新专项测试放 Temporary，验证后删除；优先加强现有 Dialog 共享契约用例，常驻用例净新增为 0。
- 二进制视觉证据只放仓库外，审查并记录数字和观察后清理。

## Review Focus

1. 时间与日期单值/范围 picker 的真实子项点击不能经由 Dialog 产生瞬时 Surface 焦点。
2. 具有确认按钮的 picker 在确认前不提交，确认后仅提交实际候选值。
3. 外点、Escape 与 Dialog 关闭后的原焦点恢复必须保持。
4. 点击空白 Surface 仍能获得焦点，多个 modeless Dialog 点击激活与键盘操作保持。
5. Dialog 重排不得 detach 内容、丢失输入焦点、关闭现有 popup 或重建资源宿主。

## Task 1：建立共享故障证据并修复宿主

**Files:**
- Modify: src/AtomUI.Desktop.Controls/Dialog/OverlayHost/OverlayDialogPresenter.cs
- Modify: src/AtomUI.Desktop.Controls/Dialog/OverlayHost/DialogOverlayLayer.cs（仅在 detach 失败证据成立时）
- Modify: tests/AtomUI.Desktop.Controls.Tests/Dialog/DialogPopupControlFamilyTests.cs
- Modify: tests/AtomUI.Desktop.Controls.Tests/Dialog/OverlayDialogPresenterTests.cs
- Temporary: tests/AtomUI.Desktop.Controls.Tests/Temporary/Issue532/

**Interfaces:** 消费现有 DialogPopupTestHost、Dialog/Picker API、Avalonia 框架输入；不新增生产接口。

- [x] 扩展已有四种 picker 家族用例：点真实子项，验证不发生 Surface 焦点、面板保持打开，再 light-dismiss。
- [x] 扩展已有层激活用例：断言顺序调整不 detach，并保留内容与焦点。
- [x] 精确 dotnet test filter 执行并记录预期失败；原始 demo 红证据复用。
- [x] 删除宿主额外强制 Focus；如重排实测 detach，则用 Children.Move 保持挂载。
- [x] 原 filter 转绿；临时补充普通控件焦点、空白 Surface、confirm 与键盘路径验证。

## Task 2：原始 UX、文档与清理后验收

**Files:**
- Modify: docs/controls/desktop/feedback/modal/popup-layering-design.md
- Modify: docs/controls/desktop/feedback/modal/implementation.md
- Modify: docs/controls/desktop/feedback/modal/changelog.md

**Interfaces:** 消费 Task 1 最终源码与现有临时 demo；无新增 API。

- [x] 原始同一 demo 同一操作历史在 Headless+Skia 与 macOS 原生转绿；截图核对普通窗口和 Dialog 的点选与确认。
- [x] 同类范围由四 picker 家族用例与已有 Dialog popup 家族、会话/激活/键盘用例覆盖；不将未执行 Windows 标为通过。
- [x] 文档补充宿主激活、焦点和重排生命周期不变量；不改 Gallery 或 Picker 的公开设计。
- [x] 评审本次 diff；清理临时验证、demo、媒体与进程。
- 最终验证入口：`python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls/Dialog/OverlayHost/OverlayDialogPresenter.cs --path src/AtomUI.Desktop.Controls/Dialog/OverlayHost/DialogOverlayLayer.cs`；结果由验证报告与交付说明登记。
- [x] `git diff --check`；交用户审查，不提交。

## 执行与证据

- 已授权按上述用户目标在当前环境内实施；不另建环境或重复申请已授权修复。自身执行计划，最后一次独立代码评审。
- 初始源码 HEAD ae5ac0a32，相关 Dialog/TimePicker 源码无本次前置改动；工作区已有其他控件改动保持。
- 原始失败：普通窗口 10:09:20 点小时 11，open=true、SelectedTime=10:09:20、通知 0；确认后 11:09:20、通知 1、open=false。Overlay Dialog 同一点选 open=false、值仍 10:09:20、通知 0。
- 初始 Headless+Skia 与 macOS 原生各已复现两轮，真实命中单元格并收到一次 PointerPressed。原生第二轮用 NSWindow 截图，三列与 Now/OK 可见，点击后面板消失。取证图已查看并清理。
- 根因源码证据：OverlayDialogPresenter 隧道 handler 在目标前调用 Surface.Focus；Avalonia FocusManager 仅在 sender==Source 时沿视觉祖先选择可聚焦元素。DateTimePickerPanel 本身 Focusable=true。
- 边界：Drawer 未发现同类强制 Focus。DatePicker 使用新 DateViewer 路径，不预断它与时间 picker 完全相同，按实际结果记录。
- AOT 判断：预期仅删除额外焦点操作并使用已有集合 Move；无反射、动态发现、注册或绑定新增，不需要重新发布 Gallery AOT。

- RED：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Picker_Family_In_Dialog_Uses_The_Shared_InfoPicker_Popup_Path|FullyQualifiedName~Dialog_Layer_Activates_The_Whole_Presenter_Atomically' --logger 'console;verbosity=normal'`：5/5 失败，执行 4.95s。四家族 Surface 聚焦次数实际 1、期望 0；层激活 detach 次数实际 1、期望 0。
- 实现：仅移除宿主强制 Focus；激活 remove/add 改为 Children.Move。Drawer 无同类额外 Focus，无需更改。新调用为既有静态类型 API，不增加资源订阅、反射或 AOT 注册。

- GREEN：原精确 filter 5/5 通过，执行 5.29s；保留的是已有四家族和已有激活用例的行为断言，常驻测试净新增 0。
- 临时专项：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Temporary.Issue532' --logger 'console;verbosity=normal'`：1/1 通过，执行 3.66s。重复点击真实 LineEdit 不产生 LostFocus，草稿不变；点击空白区域恰有一次正常失焦且焦点到 Surface。前两次临时 fixture 存在缺少基础 TextBox 主题与错误查找内嵌 TextBox 的验证代码问题，复用真实 LineEdit 自身后通过；不计为产品失败。
- 原场景同轨迹复验：`dotnet /private/tmp/atomui-issue532-demo/bin/Debug/net10.0/Issue532.dll --script --evidence <本轮仓库外artifacts>` 与增加 `--headless` 的同一命令，两进程退出码均 0。未改变 demo 输入顺序、默认动效、窗口900×720、Dialog480×300或初始10:09:20。
- 修复后 AC-01（普通窗口）与 AC-02（Overlay Dialog）：点小时11后均 open=true、SelectedTime=10:09:20、通知0、焦点DateTimePickerPanel、presenter仍挂载；点OK后11:09:20、通知1、open=false。脚本 PASS；已查看两后端点击后图及Dialog打开/点选/确认关键图，面板保持打开、Now/OK与三列仍可用，确认后面板关闭；视觉 PASS。
- 视觉边界：数字居中重排后指针悬停12的灰色行，输入框显示已有悬停预览12:09:20；蓝色选择保持11，移到OK并确认后提交11:09:20。普通窗口与Dialog一致，不把该预览作为已提交值。Windows/Linux原生与触控未在本轮运行。
- 成员结构审计：`python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/Dialog/OverlayHost/OverlayDialogPresenter.cs --path src/AtomUI.Desktop.Controls/Dialog/OverlayHost/DialogOverlayLayer.cs`：2文件、1控件、0问题。
- 独立只读评审：无可行动的Critical/Important/Minor；核对了FocusManager目标阶段聚焦、Surface空白点击、Panel.Move不改parent和键盘路由。平台实测与最终module结果由本记录承担。未另建环境，未修改额外文件。
- 保留理由：在现有共享契约用例内增加实际弹层子项交互与无detach激活断言，保护所有Dialog内容，不为Issue新增重复用例；原有只开关弹层的断言无法发现瞬时失焦，编译/AOT亦无法发现。四家族单例热执行约0.25–0.46s，激活用例约0.03s，复用既有初始化。

- 清理：本次Temporary/Issue532、/private/tmp/atomui-issue532-demo（含驱动与日志/二进制）已删除；三个视觉取证run均已审查后cleanup，最近一次removed=true、audit clean，无仓库二进制发现；demo进程均结束。仅本Markdown保存必要数字，不保留失效媒体链接。
- 最终验证范围预检：iterate精确指向两个改动的生产文件，映射Desktop测试19类与Rendering测试1类，gaps/专项义务为空；未扩展到全量、变更全域或其他用户改动。

- 清理后首次module运行：Rendering 3/3通过（5.0s），Desktop Dialog 264/264通过（16.589s），共267项；构建通过。执行中本记录更新导致全仓输入fingerprint改变，runner退出1，故该轮不能认证最终快照。没有产品测试失败。
- 收敛措施：在此冻结仓库文件，复跑完全相同的两个生产文件范围，不增加用例/平台/目标。最终执行结果、测试数、耗时及认证状态以 `.artifacts/verification/latest.json` 为准，并在交付说明报告；不再为登记结果在运行中或结束后改写本记录。
