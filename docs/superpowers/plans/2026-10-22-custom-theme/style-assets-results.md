# CT-08A 内部样式资产准备结果

子项范围见[计划](style-assets.md)。工作位置/基线沿用当前`feature/custom-theme`、`85407138f`加未提交修改；主agent独立完成，无subagent、创建分支/worktree、提交、Computer Use或全量回归。

## 实现

Core新增4个internal组件，尚无产品应用调用点：

- `ThemeStyleActivation`：不持有Control/Context的引用资格键。
- `ThemeStyleQualification.ActiveProperty`：继承的表现激活入口，不传递Token。
- `ThemeStyleAsset`：只读id/revision/ControlType与延迟Style工厂；创建必须UI线程；保留原root条件，追加注册类型和资格。
- `ThemeStyleGraph`：全图独占/父关系/资源/profile检查；原生nesting重验证；全部成功后才标记弱键，失败恢复原selector且不标记部分节点。当前profile只有Style节点，未知容器/include不加载。

没有修改Manager、Provider、Snapshot、Token缓存、任何产品Control/ControlTheme或现有AXAML；不是完整品牌应用API，也没有CTX/host自动接线。原生StyleKey匹配与Token真实identity保持分工，注册资产的类型声明一致性/Setter与模板能力仍需后续接入校验。

## 有效反例与通过证据

实际环境：macOS26、.NET10.0.8、Avalonia12.1.4，现有per-test Headless宿主。精确入口：

```sh
dotnet test tests/AtomUI.Core.Tests/AtomUI.Core.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.ThemeStyleAssets --logger 'console;verbosity=normal'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.ThemeStyleAssets --logger 'console;verbosity=normal'
```

1. 接口缺失RED：新Styling namespace不存在。最小资格接线后的行为RED为10FAIL/2PASS；补未限定子规则反例后11FAIL/2PASS。覆盖资源/未知节点、图复用/父关系/重复引用、空factory和访问边界。
2. 全图准备后12/13通过，剩余线程探针用Task.Run未证明实际不同线程，不能作为有效跨线程证据。改用独立Thread并断言不同线程ID，暂时移除访问检查后1/1有效失败，再恢复检查；GC验证弱键不保留Style通过。
3. selector-free root的AXAML在`^:disabled`处真实编译失败AVLN2000；显式SetterTargetType不能提供原生父selector。改为保留typed root，再追加exact类型/资格；root条件保留合同在旧严格拒绝实现上1/1失败后通过。错类型资产不会被重定向到注册owner。
4. 不依赖selector反射或自建解析器：对子Style原生SetParent重验证，拒绝独立selector、无selector子规则绕过资格；失败不使前面的新节点提前被标记。
5. 最终Core临时 **15/15PASS、0SKIP，总3.4309s**。包含元数据不执行factory、现有实例候选/激活/恢复、实例局部值、重置子不穿透、原根条件与类型交集、直接/嵌套/merged/theme-only资源、未知group/include、重复/挂载/父错位/复用，以及独立线程/GC。
6. 实际编译AXAML产品资产 **1/1PASS、0SKIP，505ms用例/总4.4282s**：既有生成CardHeaderStyle声明padding17和LightCyan；root前景Teal，原生AXAML disabled子规则Gray；同实例资格失活恢复基准，再激活仍为原header节点。全部Part Setter写在AXAML，代码只观测节点。

这里的线程/GC只证明准备组件，不能外推为Native popup或Gallery整图已释放。没有Skia像素、原生最终显示面、无闪烁、NativeAOT或其他OS新通过结论；旧控件视觉验收没有新增完成项。

## 生命周期与测试处置

弱标记CWT不持有owner或资源事件订阅，其value为无引用marker；asset factory持有的任意外部副作用不在此组件可恢复范围。Resources检查通过公开getter会创建空字典，这是每个准备Style的一次成本；没有在每Control或Render中调用，整体性能仍待CT-08 host场景验收。

临时Core测试1份及Desktop测试/编译AXAML3份均删除，专用namespace将在清理后DLL中检查。常驻净增0：内部组件尚无产品调用，全局每次运行的必要性未成立，不为已有探针制造永久理由；CT-08入口接入时复核最小共享契约并优先合入现有主题测试。没有截图/录屏/媒体或原生Gallery进程。

## 最终验证与后续

清理后精确验证：

- Core：`ThemeConfigProviderTests|ThemeScopeGraphTests|ThemeContextLeaseTests|ThemeContextLeaseLifecycleTests`，**29/29PASS、0SKIP，总2.2224s**。
- Desktop：`CardSemanticPartTests`，**11/11PASS、0SKIP，总3.9805s**。
- `dotnet build src/AtomUI.Core/AtomUI.Core.csproj --configuration Release --framework net8.0`：**0 warning/0 error，7.04s**。
- 5份相关文档66个本地文件链接通过；临时目录删除，两个重建后的测试DLL均无临时namespace；`git diff --check`通过。
- `atomui-resource-lifecycle`约束复核及作者分离自审完成，无本子项未处置阻断项；没有独立评审、全量运行或NativeAOT新通过结论。

**CT-08A准备组件完成，CT-08整体保持进行中**：下一子项接入ThemeContext/Provider与实际host lease，再细化root/scope/catalog协调、表现结果、stamp/重入和失败恢复；CT-07发现的模板frame退役义务不能遗漏。新的4个internal类型尚未被产品应用调用，不计为任何Control家族或品牌整体验收通过。
