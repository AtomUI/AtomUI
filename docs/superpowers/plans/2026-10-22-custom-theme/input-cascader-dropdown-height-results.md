# Cascader DropdownHeight 高度消费者结果

2026-10-28，现有feature/custom-theme、HEAD346732602。合同已由用户选择Own上限与较小页数预算；限定视觉仍待审。N3-SC13默认协议与本消费者修复分开验收。

## 原始反例与根因

Frame原来TemplateBinding根MaxPopupHeight；该DirectProperty同时被Own资源Setter和AbstractSelect行数预算写入。已实际观测初始化320/Own180，Global46后460，Global52后520；Own180/208/96/0均被覆盖。最初5几何case中4有效FAIL、1页数正对照及1记录case通过，33s；日志输出Popup.MaxHeight=Infinity最初导致JSON准备异常，改为字符串后重跑，该准备失败不计RED。

新增Own在小页数之后变更的顺序反例与编译专用Style撤回反例，原版direct完整9项为6有效FAIL/3positivePASS、21s。专用Style245/.89原已正确命中，撤回应回当前180却回460；不能绕过生成Style。原版四尺寸真实输入正对照4PASS2s，fixture曾误把Header（实际为option对象）当文字，按AttachedOption.Header纠正；未据此改生产代码。

## 最终实现

CascaderTheme保留原Frame与Popup树，在Frame现有Style Setter内以MultiBinding合并名义页数预算与Own.DropdownHeight。根DirectProperty只由原预算流程拥有，移除Own对该槽的写入。没有改Cascader事件/选择/滚动代码。

新增内部CascaderThemeBindings仅包含现有AvaloniaProperty的只读别名与无状态Math.Min转换器；内部注册字段不能被XAML直接解析，桥接复用既有TimelineThemeBindings模式，不注册新属性/Token或持有owner。绑定全在ControlTheme，初始化输入不足时DoNothing，两个double就绪后取精确最小值。显式专用popup.root Style继续按原生优先级覆盖合并后的主题默认。

两个被否决候选保留：模板内Token绑定抬高优先级，使专用Style245实际180；移入Style后Headless通过，但外层Popup承载预算时Native156实际144，宿主外围额度使内容少12DIP。最终将两约束合并到同一Frame，原宿主测量与阴影流程保留。中间不合法的Qualified TemplateBinding/CompiledBinding元素编译准备日志也保留，不算产品RED或通过。

## 实际验证

最终自然14PASS21s；加入单变量宽度取证后直接绘制完整15PASS、0FAIL/Skip、24s。包含五个上限/页数case、更新顺序、短3项自然收缩、清除/重开、生成专用Style覆盖/撤回、四尺寸输入与原始完整轨迹。

实际Frame：Own180/208/96/0在Global52后各180/208/96/0；Own240+3页预算156实际156，Own在页数之后变更仍156。默认320→180，短内容仍小于180自然布局。四尺寸各3次实际row pointer事件、1次wheel，Offset100；保留24条原数据、根宽300，点击原对象2后关闭。

两后端各78帧/比较对，Root位置/宽度/Count/Size/Page/Own记录78/78相等；默认高度与自然列宽差异见[限定视觉审查](input-cascader-dropdown-height-visual-review.md)，不能报告为无视觉差异。只有8对Own0阶段为旧新0，其余70对含校验帧；自然/直接的少量计数差分开保留。原始整Panel往返3像素失败继续CT25。

单变量宽度实验：只改cap180→320→180，实现标签0–7→0–13→保留0–13；Frame宽186→193→193，字体14和同24条数据不变。自然列宽变化必须随本表明确批准，不用增加Root缓存或先实例化全数据消差。

标题1DIP检错与A/A：

| 阶段 | 主题 | A/A | 1DIP像素 | 复位 |
| --- | --- | ---: | ---: | ---: |
| before | Light | 0 | 1422 | 0 |
| before | Dark | 0 | 1414 | 0 |
| before-direct | Light | 0 | 1422 | 0 |
| before-direct | Dark | 0 | 1414 | 0 |
| after | Light | 0 | 1422 | 0 |
| after | Dark | 0 | 1414 | 0 |
| after-direct | Light | 0 | 1422 | 0 |
| after-direct | Dark | 0 | 1414 | 0 |

当前Mac独立原生工程引用当前源码和同AXAML专用Style。managed与实际Mach-O NativeAOT各1044检查、exit0：请求Overlay/Native两种模式分别实际观测TopLevel为Window/PopupRoot，Native popup handle非零；明暗、四尺寸、Own/Global/页数/0/清除和Style均真实检查。已验证Native内容156不再变144；不冒充原生像素、鼠标或键盘验收。

NET8 Release0警告/0错误、27.02s。AOT构建无IL2026/IL3050，五条本机OpenSSL/Brotli部署目标警告保留，不声明其publish零警告。新内部绑定辅助类成员审计1文件/0Control/0问题；没有新增公开API、Control属性或schema。

资源生命周期按atomui-resource-lifecycle审查：资源绑定实际目标仍为既有Visual Border，MultiBinding由样式拥有；转换器无捕获，属性别名不持有控件，没有新非Visual资源目标、应用级订阅或owner缓存。未据此宣称全Gallery内存专项通过。

## 命令与处置

```bash
CT_HEIGHT_TRACE="$PWD/.superpowers/sdd/completion-current/cascader-height-original-full.jsonl" CT_HEIGHT_STAGE=before-direct CT_HEIGHT_TEXT="$PWD/.superpowers/sdd/completion-current" CT_HEIGHT_CAPTURE=direct dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeCascaderHeight
CT_HEIGHT_TRACE="$PWD/.superpowers/sdd/completion-current/cascader-height-same-frame-direct-contract.jsonl" CT_HEIGHT_STAGE=after-direct CT_HEIGHT_TEXT="$PWD/.superpowers/sdd/completion-current" CT_HEIGHT_CAPTURE=direct dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeCascaderHeight
dotnet publish <临时目录>/HeightNative.csproj -c Release -f net10.0 -r osx-arm64 -p:PublishAot=true -p:SelfContained=true
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/Cascader/CascaderThemeBindings.cs
```

所有新增测试临时存放，不晋升常驻；框架/既有资源机制与原Cascader契约继续提供长期保障，常驻净增0。6个临时源（25,137 bytes）、原主题专用快照已删除；外部工程426文件（1,220,202,448 bytes）及312PNG/manifest共313文件已删除，媒体audit clean，临时测试目录为空、无本包进程遗留。清理后 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Cascader.` 共51PASS、0FAIL/Skip、3s。git diff --check通过。

正式overview/implementation/token/changelog同步；Gallery API和使用形态不变。作者另步自审，遵守单agent约束，无独立评审、Git提交、分支/worktree、推送或发布。完整家族、其他模式/列/variant、原生像素/输入及CT25仍开放。未获本表视觉批准前，本包不能标完整完成。
