# CT13 CB5：ComboBox 手柄与公开候选项 Global 结果

2026-10-29，现有feature/custom-theme工作目录，单agent、无新分支/worktree或提交。候选实现和专项完成，临时清理和最终模块验收完成，限定视觉审查已获用户明确批准；完整CT13继续开放。

## 有效反例与最小接线

原版四项中3有效FAIL/1正对照PASS、921ms：ComboBox.Global.IconSizeSM=23时实际箭头仍12；真实MouseDown的pressed颜色仍Shared `#0958d9`；独立ComboBoxItem.Global.BorderRadiusSM=0时实际容器及绘制圆角仍4。父组件OptionFontSize=21本地投射对照通过，未把候选项FontSize=39无效果误判为断链。

仅将ComboBoxHandleTheme中两个IconSizeSM及一个ColorPrimaryActive改为ComboBox typed Global；ComboBoxItemTheme的BorderRadiusSM改为公开候选项自身的typed Global。四个资源来源以外的源字节一致，原选择器、Own字段/ID、API、布局树、候选字号投射、hover Own及CT11动态时长机制保持。没有新生产C#、订阅、字段、helper或缓存。

## 实际消费者与原生优先级

8项最终专项、1s通过：三反例转绿、OptionFontSize正对照、编译AXAML专用Style形态/命中/撤回、父与公开子Control的Global隔离、0/清除/重开、单变量高度与显式Height、真实按住时的热更新/透明/清除。专用Style测试锁定实际生成的ComboBoxIndicatorStyle/ComboBoxPopupListItemStyle类型以及29/31和CornerRadius9的Setter值，scope更新到25/6时仍保留样式、撤回后恢复当前25/6；code-behind只有预编译AvaloniaXamlLoader.Load，没有Name/Loaded/Unloaded节点属性回退。

临时测试曾误用生成Style命名空间、几何记录nullable，原生日志曾误把默认IconSizeSM当作16；均为夹具准备/预期错误，不计作产品RED或源修复。实际初始记录、源Global公式和最终恢复均为12；全部原始日志保留。

## 同历史视觉、几何与输入

600×520产品Window、亮色外部Host内的明/暗局部Provider，四尺寸、八选项/三行popup；标题和数据保持。两侧完整操作均为AA/标题1DIP及严格复位、每尺寸真实hover/pressed/open、配置、点击第二候选项、配置pressed和清除。正常motion开启，不重建一侧或调整像素容差。

自然与直接共288张、144对新旧：96对默认/hover/pressed/open/清除和检错控制画面均严格0；48对对应24个显式配置场景。标题1DIP各侧均1857像素、AA及复位0；16条每尺寸最终几何/数据/真实点击记录旧新一致。配置变化按后端分别保留，见[限定视觉表](input-combo-box-global-visual-review.md)，不把自然与直接的不同边缘计数合并成0。

Middle单变量12→23→12使根高度32→33→32，popup锚点Y76→77→76；popup300×104、viewport292×96、extent292×256不变。显式Height=32仍32、根宽300、原数据和打开状态保持。原生四尺寸补充确认Large40、Small24保持，Middle/Custom32→33；这一配置布局变化须随视觉表批准，不加Height补丁消差。作者已查看明/暗Middle前后，箭头变大、选中项直角与自然布局如实列出；该媒体不证明应用全局Dark算法/整窗背景验收。

## 当前Mac managed / NativeAOT

独立临时工程引用当前源码与同一预编译SemanticView。managed和实际Mach-O NativeAOT各355项通过；两种请求分别观测实际Window与PopupRoot、Native句柄非零。覆盖明暗局部Provider、四尺寸、真实容器绘制圆角、同实例0/清除、原八项/三行、父字号与专用Style热更新和撤回。

原生探针使用公开钉住弹层保证资源/几何检查不受桌面焦点变化影响，验证后恢复正常关闭；普通弹层真实点击及按下由Headless验证。不声称原生raw pressed输入或原生最终像素通过。

AOT无IL2026/IL3050，五条本机OpenSSL/Brotli部署目标链接警告保留。最终托管构建0警告/错误、8.70s；NET8 Desktop Release含依赖0警告/错误、19.32s。

## 处置与命令

```bash
# 临时验证阶段；CB5_PATHS/TRACE/STAGE指定登记媒体映射、文字轨迹和before/after；直接后端另设CB5_CAPTURE=direct。
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~CustomThemeComboGlobal.ConsumerTests
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-build --no-restore --filter FullyQualifiedName~CustomThemeComboGlobal.VisualTests
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

正式ComboBox implementation/token/changelog同步，澄清Token身份与Semantic descriptor，并纠正已过时的条目逻辑route说明；实际既有route未改。Gallery使用形态不变。C#成员审计0生产文件/0Control，作者另步确认四处精确替换、无新生产工件及原生样式/资源生命周期；不声称性能提升。

新增验证全部临时，常驻净增0；4个临时源（21,746 bytes）、两份原版主题文字备份已删除；项目外原生工程427文件（1,211,182,651 bytes）、288张PNG与manifest共289文件已清理，媒体audit clean。清理后精确`FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ComboBox.`共69PASS、0FAIL/Skip、2s，git diff --check通过。本包进程均已结束，CT12临时源和进度未触及。整个CT13、其余字体/Own/Global/状态、完整家族/其他平台继续，不以本包关闭。
