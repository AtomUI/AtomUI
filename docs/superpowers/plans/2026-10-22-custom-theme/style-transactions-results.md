# CT-08C 统一表现事务验收

工作位置：既有 `feature/custom-theme` worktree；基线提交 `79f2fc00e` 加未提交的CT-08B。主 agent 独立实现和自审，不使用 subagent 或 Computer Use，没有创建分支/worktree或提交。

## 实现范围

- 内部不可变资产输入独立于 Token Snapshot；请求等价包含规范 descriptor，scope 继承/重置及本层有序替换。相同输入保留资格代，Token相同但资产变化仍提交。
- `ThemeStyleBatch` 在全部 Context 间协调准备、接线、预激活、校验、无通知引用提交和退役；工厂执行前逐目标复核原捕获。host stamp 比较实际 styling 祖先链、样式组引用及顺序，不仅登记计数。
- root/FollowSystem、scope 注册/修改使用同一批次；catalog 在后台准备前捕获表现，package 在configure前捕获，均在提交前校验。它们继续保留现有表现代际，不隐式启用新品牌来源。
- 版本1 `ThemePresentationResult` 与Token结果分开。Manager/Provider发布结果；失败事件在恢复尝试之后发出，scope追加普通warning不丢失表现结果。
- 外层Window/Provider/root释放失败保留准确责任；下一次变更/显式重试/Dispose重试。失败时禁止新增host/主题变更，同一对象重试不增加责任条目。提交后退役失败保持Token Committed并报告Degraded。
- 诊断日志器异常不能中断已提交的发布或使请求永不完成。旧外部IThemeManager实现可继续编译，新表现成员默认明确拒绝，不能假报成功。

公开资产注册、能力校验、品牌manifest属于CT-09；本次内部入口不等于可发布的完整品牌产品。Token XML v1/compiled ABI v1未修改，没有迁移产品Own V2或控件家族。

## 失败与通过证据

临时用例位于所属Core/Desktop测试项目的`Temporary/StyleTransactions`，全部真实编译/发现/执行，最终删除。

| 合同 | 实际证据 |
| --- | --- |
| 外部移除候选、改变styling祖先 | 原实现2/2 FAIL，补全stamp后2/2 PASS |
| 仅资产变化、嵌套继承/重置 | 原路径2/2 FAIL（NoOp/资产缺失），统一输入/协调后通过 |
| 前一个工厂扩大后一个Context的host集合 | 1/1 FAIL；逐目标复核原始捕获后通过 |
| 失败事件先于恢复、warning复制丢失表现结果 | 2/2 FAIL；恢复后事件及复制保留后通过 |
| 诊断日志器抛错 | 原路径2秒超时1/1 FAIL；best-effort日志后通过，结果Committed且有观察者warning |
| 恢复/退役错误 | 所有参与Context恢复；持续恢复失败为Degraded/阻止新装配，解除故障后显式重试成功；退役错误不回滚已提交Token |
| 外层Window清理 | bridge/订阅照常释放，样式失败由Manager保留；新host被阻止，移除故障后样式及资格清空 |
| 重入/异步入口 | 工厂请求令旧整体Superseded；Token await期间host修改中止；局部仅样式更新；package configure前捕获且相同表现不重建 |
| 生命周期/兼容 | 保留已完成batch句柄时旧host/graph弱引用均回收；旧IThemeManager实现编译并可执行原Token入口，新表现调用明确拒绝 |
| Core专项 | 最终临时 **17/17 PASS、0SKIP、410ms**；此前含16临时的聚焦组合 **74/74 PASS、0SKIP、620ms** |
| 产品编译Part | 专用CardHeaderStyle在AXAML声明Padding17/LightCyan；同Card/Header/Content切换品牌↔空集合10次，reset scope保持自己的padding |
| 实际弹层 | **Desktop3/3 PASS、0SKIP**，实际类型断言PopupRoot/Overlay；已打开和重开均消费当前owner资产，恢复后回到基准 |

最初Popup两个断言在下一布局之前读取旧12,0,12,0；单变量补真实host布局后达到17，原期望/宿主/内容保持，未为此更改产品。Headless fixture临时启用真实Skia且`OverlayPopups=false`，随后恢复；这些是Headless两类物理树证据，不是macOS最终显示面。辅助测试曾有类型/namespace编译错误，修正后真实执行；不计作产品失败前证。

精确命令形态：

```sh
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.StyleTransactions
ATOMUI_BRAND_VISUAL_OUTPUT=<登记的仓库外artifacts> dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.StyleTransactions
```

## 脚本化视觉

外部临时`Visual.csproj`引用当前产品源码，UseSkia + HeadlessDrawing=false，自驱动真实AtomUI Window/Provider/Card/主与默认Button/选中CheckBox。固定默认主题、客户区480×360、scale1，覆盖实际Transition时长且连续3个真实帧相同后捕获。基线使用本worktree暂时逆向本任务patch至已提交79f2fc00e；finally逐文件SHA256确认所有修改原样恢复，没有另建隔离副本。

- 同版本A/A **0差异像素**；Card左边距1DIP检错 **3947像素**。
- 已提交基线 vs 本轮默认代表场景 **0 RGBA差异像素**，控件/内容引用与选中状态保持。
- 编译Part品牌场景480×480，品牌实际变化 **32121像素**；同实例10次往返后恢复 **0差异像素**。已查看before/active/restored及默认前后/检错差异图，品牌仅改变声明的header形态，reset scope自己的样式保持；其位置随父Card高度合法变化。
- 全尺寸、同坐标、零通用容差，不缩放/对齐/裁剪/遮罩。没有通过桌面输入或识图推进。

这不证明其他控件/配置、原生OS闪动、多窗口vsync、NativeAOT或其他平台；对应CT-12/CT-25出口保持未通过。

## 清理与最终检查

所有Core17项/Desktop3项临时测试及AXAML、测试fixture临时配置、外部驱动/编译产物、逆向基线patch与fixture备份均已删除。媒体audit无发现，登记目录cleanup成功删除12文件（含清单），无本次UI进程。重建后的两个测试DLL均不存在临时namespace/CardBrandStyle等UTF8/UTF16标记。

清理后精确验证：

```sh
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~ThemeConfigProviderTests|FullyQualifiedName~ThemeScopeGraphTests|FullyQualifiedName~ThemeContextLeaseTests|FullyQualifiedName~ThemeContextLeaseLifecycleTests|FullyQualifiedName~ThemeManagerTests|FullyQualifiedName~ThemeManagerTransactionTests|FullyQualifiedName~ThemeCatalogReloadTests'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~CardSemanticPartTests|FullyQualifiedName~WindowThemeContextLeaseTests|FullyQualifiedName~ThemePopupInheritanceTests'
dotnet build src/AtomUI.Core/AtomUI.Core.csproj --configuration Release --framework net8.0 --no-restore
```

Core **58/58 PASS、0SKIP、667ms；最终仅成员区域整理后58/58 PASS、642ms**。Desktop **17/17 PASS、0SKIP**。NET8 Release **0warning/0error**。Provider成员组织审计1文件/1控件/0问题；diff检查通过。未运行全量回归，未新增常驻测试；既有共享lease安装/释放断言保持，新增机制验证默认临时，结果记录代替将一次性故障矩阵加入全局集合。

作者另起步骤自审，逐项核对工厂前捕获、阶段门槛、提交无回调、失败事件顺序、所有清理边界、有界重试、旧实现默认拒绝、无Context/Visual进Snapshot和已完成句柄释放。发现的跨Context捕获、结果复制、事件顺序与日志器故障均以有效失败/通过处置；本子项无未处置Critical/Important。没有独立评审；性能、多OS/AOT、公开品牌注册和全部控件视觉仍按对应任务验收。

CT-08机制出口完成；整体精确主题项目继续CT-09及CT-10–CT-26。没有提交。
