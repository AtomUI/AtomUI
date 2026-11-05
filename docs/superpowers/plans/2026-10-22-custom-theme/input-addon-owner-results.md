# CT13 S7：三选择控件 add-on 的有效 Global 消费

2026-10-29；现有 `feature/custom-theme` 工作目录，未新建分支/worktree、未使用子 agent、未提交。CT12 原生采样由用户确认已结束后才恢复构建；其源码与进度不在本包修改范围。

状态：实现、专项、临时清理和最终模块验收完成；[限定视觉审查](input-addon-owner-visual-review.md)已于本会话获用户明确批准，保留CT25调查，不关闭完整CT13。

## 根因与修复

三个公开 owner 配置 `ColorBorder`、`ColorError`、`ColorWarning` 和 `ColorFillTertiary` 后，真实 add-on 表面仍分别显示 Shared 默认值；12项有效 RED（1s），根 `BorderBrush`/`Background` 均为null，排除实例覆盖。初期枚举/identity命名空间及nullable编译准备错误不算RED。

Select、TreeSelect、Cascader 的内部 add-on 主题各23处、共69处通用颜色读取改用各自 typed TokenResource。原Global键、模板树、选择器、优先级及根画刷中继保持；不新增 Own、identity、资源ID、公开API、C#字段/helper、订阅或缓存。初期静态计数把Select另一个主题引用并入，替换前置断言阻止了错误计数下的修改，最终精确69处。

## 功能与生命周期

最初12项全部转绿；扩展19项验证包含：11个颜色键及各自真实hover分支、热更新、显式透明、兄弟隔离、根画刷优先级、清除、重挂及重模板；六个owner/frame对象的回收；三个控件真实MouseDown/Up打开原24项与三行预算，打开期间更新错误色且popup保持打开。

即时属性/优先级测试单独覆盖显式motion=false；正常motion的默认与配置兼容由下述完整Skia轨迹及原生驱动覆盖，没有关闭动画解决像素失败。

补充输入夹具最初使用没有Overlay层的Avalonia Window，三例抛出IPopupImpl/overlay准备异常；更正为真实产品Window后通过，产品弹层逻辑未改。GC探针曾仅同步RunJobs/GC而未推进实际帧，末个Cascader仍被保留；改为有1.2s上限的真实Skia帧/时钟推进后，完整19项通过、六对象回收，未加产品清理或重绘补丁。即时失败和独立进程失败日志均保留，不把准备错误计为产品RED。

## 默认兼容与实际绘制

明暗、四尺寸、四variant、三owner共48实例，1120×800、缩放1。正常motion保持开启；同一完整历史捕获自然/直接两后端，共88张、44对比较。24对初始/AA/检错/复位/真实hover/默认错误态前后严格0；直接清除另2对0。各侧AA和标题扰动复位均0，1DIP明1307/暗1282像素；192条最终owner/frame几何记录一致，8轮均记录到真实指针事件。

配置变化按两个后端分别记录。原始自然清除仍非零：新旧清除帧明1/暗6点，通道差1；原版自身恢复4/8、候选3/2点，直接均0。详细坐标、计数及所见效果见[限定视觉表](input-addon-owner-visual-review.md)，保留CT25后续调查，不建立通用容差、不用直接绘制替代自然合成器验收。

## 当前 Mac 与构建

独立临时工程引用当前源码，实际 `NSWindow` managed/NativeAOT 各325检查：明暗、四尺寸、Outlined/Filled及三状态、透明、同实例清除、重模板、编译AXAML根Style与三个生成专用Suffix Style命中/撤回均通过。这里是原生属性/资源/几何检查，不是原生最终像素或raw输入验收。

NativeAOT编译没有IL2026/IL3050；五条本机OpenSSL/Brotli部署目标链接警告保留，不称publish零警告。工程初期 `/var` 与 `/private/var` 路径导致相对ProjectReference准备失败，规范为真实路径；补齐Desktop扩展命名空间及partial App后托管构建0警告/0错误。NET8 Desktop Release含依赖0警告/0错误，21.20s。

## 命令与最终处置

```bash
# 临时源存在期间
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~CustomThemeSelectAddOn.OwnerConsumerTests
# CT13_ADDON_PATHS/TRACE/STAGE分别指向登记的项目外媒体映射、文字记录与before/after；直接绘制另设CT13_ADDON_CAPTURE=direct。
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~CustomThemeSelectAddOn.VisualTests
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

正式 Select/TreeSelect/Cascader implementation、token、changelog 同步；API、Own和Gallery调用形态未改。常驻测试默认净增0，复用现有三模块行为、选择、主题与Semantic Part测试作为长期保障。产品改动仅三个AXAML，故C#成员审计0文件/0Control；作者另步核对69处输入、原选择器及状态优先级、原生Style和资源生命周期，无新增生产工件，不宣称速度提升。

19项最终临时检查2s通过，已删除两个临时源（22,379 bytes）、三份原版主题文字备份；项目外原生工程430文件（1,236,405,417 bytes）和88张PNG/manifest共89文件已删除，媒体audit clean、无本包进程。CT12的6个临时文件未触及。清理后精确SelectControl/TreeSelectControl/Cascader共148PASS、0FAIL/Skip、7s，命令：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.TreeSelectControl.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Cascader.'`。相对链接和git diff --check通过。完整家族、剩余字体/公开子Control/Part/状态、其他Host和CT25仍开放。
