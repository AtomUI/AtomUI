# CT13 S1：Select 默认协议本机结果

产品只将 SelectToken 的 25 个历史字段迁移至 DependencyDefaultsV2、声明修订 1；无新增字段、别名、Own 边、消费者绑定、身份或运行时 helper。原字段声明顺序、类型、Global 算术、普通高度 -2、小号 +4、大号原值、Round/Ceiling 与前缀零下限保持。SingleModePadding/PrefixIndent 复用同一 Global 默认算术，不读取显式 MultiModePadding。

## 协议与实际值

原代码协议反例 1FAIL（WholeEvaluatorV1），另三项明暗资源/覆盖和独立 Global 算术正对照 PASS，878ms。明暗各25条默认值、类型、resource key、公开ID和内部slot前后完全一致。所有字段显式覆盖、与默认相等的显式值在 Global 改变后仍保持、数值/Thickness 显式0及最终清除均验证。

候选首轮夹具误将 descriptor 的 evaluator 指纹当作声明 revision=1；改为核对 V2、非零指纹及实际 attribute.Revision=1。独立算术夹具同步生成求值入口。原 SelectTokenTests 直接调用 V1 whole evaluator，候选下实际前缀期望7/实际0；仅改为生成 EvaluateDefaults 后保留全部三档补偿断言。最终四项临时合同加既有前缀用例共5PASS、0Skip、1秒。没有为内部 Token 额外保留旧 evaluator shim。

## 完整轨迹与视觉边界

真实 Skia 合成器、Avalonia Window、1160×650、缩放1。20个实例包括 Select Single/Multiple/Tags、TreeSelect/Cascader，多档 Large/Middle/Small/Custom，分配四种原有 variant；20项按既有模式保存选中对象。同实例依次经过空值、AA、1DIP扰动/复位、已选、禁用/启用、已有Own配置、配置弹层、实际 Down/Enter、清除配置、默认弹层及关闭。

弹层保持6条内容、DisplayPageSize=3、原最大高度与可见竖滚动条；脚本断言实际 extent>viewport、popup打开、两次KeyDown到达和选中结果，没有移除滚动/高度条件。每个版本24帧×20实例的480条目标几何/字号/标签/frame记录相同。

首轮中立焦点未固定，原 Light 启用恢复341像素、最大通道差224；264个大差异位于 Tags 输入光标。将两侧都明确保持焦点在外部按钮后，原 Light 剩余77像素、单通道差1，直接绘制恢复0。该修正改变的是比较的焦点合同，不关闭光标或产品动效。

原最初宿主外层白色背景使 Dark 禁用文字缺少有意义的对比；该组结果保留为宿主诊断，另以实际 scope.ColorBgContainer 背景补采原版和候选。补采前暂存本包候选，恢复原 Token/原前缀测试，采完后按记录原样恢复候选；没有改变HEAD或其他未提交工作。

两种宿主各48对旧新画面（合成器及直接绘制）全部精确0，补采480条状态记录也相同。AA与1DIP复位均0；最初中立宿主1DIP检错Light41324/Dark27149像素，直接绘制41444/27179，主题背景补充亦实际检出。

**严格往返断言仍失败，未宣称完整视觉通过：** 在主题背景宿主下，原版和候选 Light 启用恢复均77像素/最大通道1，Dark均155像素/最大通道3；直接绘制两主题恢复均0，其他记录的配置恢复为0。严格断言未删除或增加容差，采样先收集两主题再报告FAIL；原始计数及各次日志保留CT25。新旧同阶段0只证明本次默认迁移没有引入差异，不关闭这个既有合成器问题或完整家族。

已实际查看默认、配置弹层、Light启用恢复及Dark禁用关键图。没有新增需批准的旧新视觉变化；未将此前任何视觉许可扩大到本包。

## 当前Mac与构建

产品Window的真实NSWindow、Light/Dark，managed与实际Mach-O arm64 NativeAOT各256项、exit0。通过公开ControlTokenAccessor验证全部25默认/显式值、0/清除、Control Global、原选择对象保持和打开弹层热更新/恢复；三家族模板存在。该检查是实际资源/模板和API路径，不冒充原生像素或原生键盘验收。

ILC输入明确为官方Avalonia12.1.4/lib/net10.0/Avalonia.Base.dll，未改正式依赖。最终无IL2026/IL3050或启动图警告；本机五条OpenSSL/Brotli的macOS部署版本链接警告仍保留，不据此外推最低OS。

NET8 Release构建0警告/0错误、18.57秒；单Token完整成员检查1文件/0自带注册control/0问题。私有默认函数之外无额外运行时状态或缓存。

## 清理、模块与范围

两份临时C#、源码候选/原版快照及媒体映射已删除；常驻测试净增0，只同步既有前缀测试入口。外部原生工程422文件、1,232,955,671 bytes已删除；206PNG及manifest共207文件已删除；清理后Select/TreeSelect/Cascader精确148PASS、0FAIL、0Skip、6秒。媒体审计clean，无项目内取证二进制。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~Temporary.CustomThemeSelectDefaults.SelectDefaultsTests|FullyQualifiedName~SelectControl.SelectTokenTests'
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.SelectControl|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.TreeSelectControl|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Cascader'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

仅主agent作者另步自审；没有独立评审、Git提交/推送、新Actions或NuGet发布。OptionPadding/SelectAffixPadding/FixedItemMargin的实际消费、其他Global/Part/状态/Custom及完整CT13/CT14/CT25继续开放。本包完成的是本机默认协议迁移，不是任何完整控件家族。
