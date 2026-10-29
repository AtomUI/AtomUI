# CT13 S3：三选择owner内部标签布局与计数文字

状态：最小实现、本机专项与临时清理完成，[限定视觉审查](input-select-internal-owner-visual-review.md)已独立批准；不关闭完整家族。

## 根因、实现与有效反例

Select/TreeSelect/Cascader真实多选标签的wrap与响应式面板仍读取Shared.SpacingXXS=4，各公开owner的有效18/11/7未到达这6个入口；计数文字仍为Shared.ColorTextDescription，未到达指定洋红/绿/蓝。实际内部目标没有独立公开identity，不能把三者改成统一Select配置。

唯一产品改动为三共享AXAML的7个资源入口：两标签布局主题各3处SpacingXXS、计数主题1处ColorTextDescription，改用既有OwnerTokenResource。没有新Own字段、identity、代码绑定、布局算法、事件或缓存，也未改TagAware禁用Foreground与公开图标/Tag颜色分支。

首两轮9FAIL、1s中仅6项为有效间距RED；3项错误限定具体SolidColorBrush，而合法结果也可为ImmutableSolidColorBrush，未计作颜色RED。改为ISolidColorBrush后，单变量只将计数资源切回原Shared，3项真正颜色反例FAIL、997ms：实际#72000000而期望#ffd02088/#ff008833/#ff3355cc。精确候选及hash已保护，随后恢复，mustRestore已清空；原始类型/nullable编译准备失败均保留。

最终9消费者加完整场景10PASS、8s；进一步加入真实viewport内横/纵间距断言后直接绘制10PASS、9s。Responsive旧实现将未显示标签停放在int.MaxValue处，IsVisible本身不足以证明可见；观测器用与真实panel viewport相交的Bounds判断并检查实际间距，没有修改既有停放算法或移除内容。

## 原完整场景与视觉

Skia1020×450、明暗各三公开根、根默认尺寸、固定宽300、每根6个原标签及“6 / 8”计数，原换行和响应式MaxTagCount=2、0/禁用/启用/清除、真实第一根标签删除后“5 / 8”均保持。原自然/直接各1PASS、8s；A/A=0、1DIP扰动可检出并复位0，旧新完整历史一致。

84条根X/Y/宽度、数据计数和状态记录前后相同。配置间距下原三根高度84分别变为112/98/90，正是三行标签两段行间距的18/11/7替代4；这属于限定配置变化。响应式实际保留首标签和剩余数量标记，标记起点由82分别移到96/89/85；选择数据未被删除或重建。父框体原已消费的其它Global角色保留，不能把从默认切配置的全部差异归为本包新增效果。

自然和独立直接绘制各28对中，14非修复阶段均0，14配置阶段按实际间距/计数颜色改变。部分配置阶段两后端计数不同，分别保留，不能宣称全表计数一致或建立容差；详见[限定审查](input-select-internal-owner-visual-review.md)。已查看原/候选明色配置及原默认、暗色响应式/删除、明色0的完整帧，内容及数据保留，间距进入实际位置和高度。

实际NSWindow managed最初236项通过，增加viewport实际间距后managed/arm64 NativeAOT各290项、exit0；仅有限已知内部类型名与公开布局/文字属性观察，没有CLR成员反射扫描。不冒充原生最终像素或原生点击。官方Avalonia12.1.4，无IL2026/IL3050或启动图抑制；五条本机OpenSSL/Brotli部署版本提示保留。NET8 Release0警告/0错误、18.12s。

作者另步自审核对了三实际owner、7个目标的Scope/继承、0/清除和真实viewport几何、公开子Tag干扰隔离、无新非Visual资源宿主/代码订阅/缓存；无独立评审。三个临时C#及目录、原/候选快照和媒体映射删除；外部工程422文件、1,233,604,262 bytes删除；媒体113文件清理且audit clean。常驻测试净增0，无本包进程遗留。

清理后精确Select/TreeSelect/Cascader共148PASS、0Skip、7s；git diff --check通过。无Git操作，全部原始未提交工作保留。完整家族、其余Global/Part/尺寸及CT25仍开放，具体14对视觉已独立批准。

```bash
CT_SELECT_INTERNAL_MEDIA="$PWD/.superpowers/sdd/completion-current/select-internal-owner-media.json" CT_SELECT_INTERNAL_TEXT="$PWD/.superpowers/sdd/completion-current" CT_SELECT_INTERNAL_STAGE=after-direct CT_SELECT_INTERNAL_CAPTURE=direct dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeSelectInternalOwner
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.TreeSelectControl|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Cascader'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```
