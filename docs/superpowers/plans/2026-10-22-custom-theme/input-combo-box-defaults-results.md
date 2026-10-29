# CT13 CB1：ComboBox 十一项默认结果

[计划](input-combo-box-defaults.md)的十一字段已迁移DependencyDefaultsV2 rev1，每字段静态函数，无Own依赖；删除空ctor和whole evaluator。只有ComboBoxToken运行源码变化，字段顺序/类型、名称、key/ID/slot及运算保留，特别是ItemBgColor=ColorBgElevated、PopupContentPadding纵向BorderRadiusLG.TopLeft/2、ItemMargin上下0.5。未修改控件、Popup、模板、选择、字体或语义路由。

## 前后证据

原注册协议反例1FAIL，已有Global/显式Own/0/清除正对照1PASS，651ms；候选加入完整Frame共3PASS，17秒。独立覆盖两个Global背景的不同值，防止把Elevated误接到Container；显式ItemColor不影响ItemHoverColor，未虚构Own边。明暗22条默认、类型、key、ID和slot完全一致。

Skia、640×360、scale1，实际Default/Default+Dark scope，Overlay popup已打开，三行高度约束与四个条目保留。ScrollViewer有效viewport、extent溢出和垂直条节点IsVisible检查通过；没有删高度限制、滚动或内容。两侧同历史：closed→open→AA→1DIP→复位→真实Down/Enter→selected→reopen→既有Own配置→恢复→close。正常动效启用，键盘确实提交SelectedIndex=1，pin/open/close状态保持。

22对新旧完整帧全部0；原/候选明暗AA、扰动复位及配置恢复全部0；1DIP明暗7643/3933全部检出。已查看明暗实际完整图，未缩放、遮罩或设置容差，没有新视觉例外申请。头less宿主使用既有ComboBox测试的VisualLayerManager/ScopeAwareOverlayLayerPanel，仅测试初始化反射启用框架内部overlay开关；修正准备期移除的GetVisualRoot及缺少overlay宿主，不计产品RED。

## 当前Mac实际消费者

产品AtomUI Window、NSWindow、scale2、400×240，managed与NativeAOT各102项、exit0。此平台实际Popup顶层为AtomUI Window，即Overlay路径，未声称独立Native Popup。十一Own默认三轮读取、明暗实际variant、独立Global/Own/0/清除，以及已打开popup Padding、普通项Padding/Foreground、选中背景、Elevated背景、选择与正常关闭通过。

原生驱动仅使用公开API与ControlTokenAccessor，不使用测试宿主的内部反射。ILC实际引用官方Avalonia12.1.4/net10.0/Avalonia.Base.dll；本地OpenSSL/Brotli五条macOS26对deployment12链接警告保留，不外推最低OS。资源与模板消费者不等于原生最终像素或原生键盘输入，其他Host仍开放。

```bash
# 临时阶段，现已删除
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeComboBoxDefaults
# 清理后精确模块
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.ComboBox
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

## 清理与范围

两份临时源码、原源码快照及媒体映射删除；外部原生工程/产物419文件、1,375,748,450 bytes删除。44PNG及manifest共45媒体文件清理，audit clean，常驻测试净增0。NET8 Release零警告/错误、24.82秒；单Token完整成员/语法审计1文件、0注册control、0问题，作者另步自审默认来源与字段顺序。清理后原ComboBox模块69PASS、0FAIL、0Skip，3秒。

本包只完成默认机制。ItemDisabledColor的内置消费缺口及HandleHoverColor真实hover命中核对、全Global/实际业务状态、完整ComboBox家族、其他Host与原生最终像素继续；不会以十一资源存在计作实际字段全部生效。无独立评审、无Git提交、无新Actions或NuGet发布。
