# CT13 S2：Select候选项OptionPadding结果

状态：最小实现、本机专项及临时清理完成，[限定视觉审查](input-select-option-padding-visual-review.md)已独立批准。仅修复这一Own消费者，不关闭完整Select家族。

## 根因及最小实现

真实Popup中四尺寸Select配置Own.OptionPadding=9,4,17,6，候选项却读取干扰ListView.ItemPadding=41：原4FAIL、946ms。字段原先只声明/计算，没有实际消费者。候选不重写默认公式、不新增字段或WhenExplicit旁路，只在候选项派生主题使用SelectTokenResource.OptionPadding。

第一版普通类型选择器Setter被BasedOn的尺寸属性触发器覆盖，5FAIL/1视觉PASS、12s保留；这也说明视觉采样通过不等于消费者修复。最终在原派生选择器下匹配已有四种SizeType属性触发优先级，不增加状态旗标、代码绑定、模板结构或输入事件。

原编译专用SelectPopupListItemStyle命中测试1PASS、700ms；候选6PASS、12s，补真实Frame断言后直接绘制6PASS、13s；最后专用类及两Setter值的显式断言1PASS、783ms。专用AXAML Setter Padding=23,2,19,3及Opacity=.82优先，Own切换不抢占；退出class资格后恢复当前Own=5及Opacity1。不直接Styles.Remove，不用节点属性写回。

真实模板是Item.Padding→PixelAlignedBorder Frame.Padding→DockPanel/ContentPresenter布局，ContentPresenter自身Padding并不承担内缩。原生驱动初次误读ContentPresenter.Padding的失败已保留，修正只读目标后重跑；没有为满足断言改产品模板。最初临时代码缺namespace/原生类型歧义的编译准备失败也保留。

## 真实弹层/输入与像素

Skia540×400，保留标题、根宽260、8条原内容、DisplayPageSize=3，明暗/四根尺寸依次执行默认、Own非均匀9,4,17,6、0与清除，再真实中心点击第二项/关闭/重开/选中清除。实际viewport256×96、extent256×256、最大弹层高100、三行高32保持，scrollbar的IsVisible为真；静止帧不冒充自动隐藏滚动条始终可见的像素证明。

44根/viewport/extent/count记录及132项Bounds/文本/选中记录前后相同，未移除高度、内容或动效。A/A=0，1 DIP扰动可检出并恢复0；旧新完整历史一致。自然与独立直接绘制44对计数完全相同：24对默认/清除/AA/复位等为0，20对非均匀/0及配置选中阶段改变文字内缩，见[限定审查](input-select-option-padding-visual-review.md)。

默认值实际从继承的12,0变为Select有效Own12,5，但32行高及原垂直居中使像素相同；初步源码推测默认会改变已由实际证据纠正。已查看默认前后、配置、暗色0及选中完整帧，配置后原内容、框体和选中背景保持。

原生普通弹层曾有一轮在暗色清除阶段关闭；首轮无焦点轨迹，原因不能倒推。增加只读焦点轨迹后同流程1922项通过，增加实际窗口激活前置检查后1924项通过。后续AOT普通弹层日志明确为DEACTIVATED→OPEN false/active=false→popup-open断言失败，符合失去窗口激活时的既有light-dismiss，不将其当成Padding断链，也不删除原始失败。

最终原生资源/模板探针使用公开IsPopupPinnedOpen固定弹层，明确只验证属性、Frame布局与专用Style；真实普通弹层的像素/点击仍由前述Skia轨迹证明，不借固定状态声称原生最终像素/输入或完整弹层稳定性已通过。产品代码没有改变pin/dismiss逻辑，完整CT25仍开放。

最终实际NSWindow公开固定弹层驱动managed/arm64 NativeAOT各2044项、exit0，覆盖明暗、Single/Multiple/Tags、四根尺寸、配置/0/清除与专用Style资格撤回后的当前值。官方Avalonia12.1.4；无IL2026/IL3050或启动图抑制，五条本机OpenSSL/Brotli部署版本提示保留。NET8 Release0警告/0错误、19.92s。它们只证明当前Mac API/模板/资源与布局检查，不冒充原生像素或原生点击。

作者另步自审核对了继承触发优先级、原公式、实际Frame投射、专用Style优先级/资格撤回、scope及没有新非Visual资源生命周期；无独立评审。四临时C#/AXAML及目录、源码快照/媒体映射删除；外部工程426文件、1,208,070,993 bytes删除；媒体177文件清理，audit clean。常驻测试净增0，无本包进程遗留。

清理后SelectControl精确71PASS、0Skip、3s；git diff --check通过。无Git操作，所有原始未提交工作保留。用户独立批准[20对具体配置视觉](input-select-option-padding-visual-review.md)，默认/清除等24对均0，不建立通用容差。下一项见[内部排版owner核对](input-select-internal-owner-audit.md)。

```bash
CT_SELECT_PADDING_MEDIA="$PWD/.superpowers/sdd/completion-current/select-option-padding-media.json" CT_SELECT_PADDING_TEXT="$PWD/.superpowers/sdd/completion-current" CT_SELECT_PADDING_STAGE=after-direct CT_SELECT_PADDING_CAPTURE=direct dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeSelectPadding
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```
