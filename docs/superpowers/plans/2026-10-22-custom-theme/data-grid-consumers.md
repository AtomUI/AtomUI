# CT-18 DataGrid 主题消费实施计划

> **For agentic workers:** 使用 superpowers:executing-plans 由主agent逐任务实施；用户要求不使用subagent，不自动提交。

**Goal:** 完成42项metadata及DataGrid78个既有Own字段的实际消费、默认依赖、状态／布局与兼容验证。

**Architecture:** 复用现有DependencyDefaultsV2和17项显式别名机制；先做默认计划，再逐实际模板和内部owner验证。独立身份／Own scope保持，不造别名近似匹配或资源编号重排。

**Tech Stack:** 当前Avalonia12／.NET10及NET8，生成默认计划、真实Skia／现有DataGrid测试宿主及平台专项。

**Spec:** [总任务](tasks.md#精确范围与前置出口)、[Own默认](../../../architecture/systems/theming/own-token-evaluation.md)、[别名结果](aliases-results.md)、[DataGrid合同](../../../controls/desktop/data-display/data-grid/overview.md)。

## 共同约束

沿当前feature/custom-theme与checkout；29文件审查快照保持字节，不创建分支／worktree，不自动提交／推送。临时验证先RED再GREEN，清理后精确选测；全局常驻case净增0默认。保留虚拟化、冻结、滚动／裁剪／输入、已有状态和调用方优先级。语义部件定制仅使用生成的专用AXAML Style；实际默认画面无未经批准差异。

## G1：78字段默认计划及兼容

- [x] 在tests/AtomUI.Desktop.Controls.DataGrid.Tests/Temporary/CT18Defaults复用宿主和真实ThemeConfigProvider，验证规范Own改变后非别名依赖默认更新，明确的派生Own继续优先，清除逐层恢复。实际14项旧版12FAIL／2独立覆盖PASS，新版14PASS；单独记录驱动错误。
- [x] DataGridToken改为DependencyDefaultsV2 revision1并实现IControlTokenDefaults，78声明不变；17个显式别名不定义默认函数，其余61个字段各有唯一默认函数。静态函数只读context或声明的Own参数。
- [x] 直接赋值的8个非别名镜像声明canonical Own输入；保留各自slot和显式独立配置，不新增Alias。局部算式和未接线历史槽位保留原公式／默认，不启用消费者或改Hover RGB公式。
- [x] 旧／新78字段在明暗默认及14项自定义Global输入下，共312条值、类型、slot／resource ID全部相同。0和派生独立覆盖／清除通过；现有对齐测试改为生成默认入口，4类语义保留。真实compiled literal旧别名及V2组合由Mac managed／NativeAOT各37项覆盖，冲突机制沿CT06。
- [x] 以CT06同40行／4列／冻结列、实际滚动和标题／footer宿主，默认与已有Table配置做同轨迹旧新、独立AA／1DIP，配置和清除保持状态。216帧旧新108对0差异，72组AA0、72组1DIP全部检出；完整轨迹限定为滚动后实际移动指针，静止指针回收留G2，见[结果](data-grid-default-results.md)。
- [x] 当前Mac真实NativeAOT验证V2与别名组合，managed／AOT各37通过；逐平台按CT25补证，不把单平台冒充全部。三份临时C#／6helper／外部工程／媒体全部清理后，DataGrid主题／尺寸／布局及两项CI反例方法精确34项通过，NET8零警告、3文件成员审计0；Windows故障仍开放。

## G2：真实消费者与状态

- [ ] 逐78字段确认实际消费者与Reserved／Legacy边界；不是只查名字或资源出现。重点header/cell/footer、排序／过滤、checkbox／selection、rowdetail、拖拽与冻结阴影；未使用槽位不虚报生效。
- [ ] 逐42 metadata验证实际owner和Scope、Global排版／状态画刷、局部Own、compiled专用Style、清除与优先级；生命周期尤其非Visual列对象随挂载／detach释放。
- [ ] 定向复现后修复动态字体、间距、线宽、缓存、视觉裁剪与几何；原冻结／回收／虚拟化及滚动条／输入合同保留，业务状态不中断。

## G3：完整出口

- [ ] 在实际状态／尺寸／方向／Custom、已开过滤弹层、冻结与滚动回收路径完成视觉、生命周期／性能及CT25平台专项。
- [ ] 同步正式文档和目录证据；全家族出口通过前不把G1代表切片填写为CT18完成。提交仅在具体快照批准后执行。

## 当前调查

源声明78字段，17已有别名，其余61默认。原V1先整体计算再写Own，非别名内部镜像不能随明确规范Own重算；本轮已用实际资源RED及新版GREEN验证。G1默认计划已实现，视觉及清理后验证仍在执行，G2/G3开放。


G1前证：修正资源颜色按ISolidColorBrush.Color读取后，实际Default／Dark算法及Appearance断言的6项为4失败／2独立覆盖正对照通过，11.574秒；颜色依赖未重算、0宽派生仍32。78字段两主题默认值／类型／resource ID／slot已实际捕获。首轮画刷误当Color及捕获helper对BoxShadows错误使用LINQ的编译错误不计产品RED。

Ruling：只把原代码直接赋值的8个非别名镜像声明为Own依赖；局部变量共享的expand size/scale等仍展开为原Global公式，不因名字推断新依赖。17个别名保持原显式Alias合同，不新增Alias或改变可独立覆盖的字段。61默认函数按默认计划职责分在DataGridToken.Defaults.cs，原文件保留78字段／17Alias及版本声明；现有对齐用例使用当前生成入口，仍为低成本纯逻辑，真实Compiler由临时资源验证覆盖。

当前Mac真实640×440窗口、scale2，managed37项3.551s／NativeAOT37项3.667s通过：compiled literal的TableHeaderBg归一、规范Own→非别名依赖、显式SelectionColumnWidth=0、scope覆盖compiled根、独立派生覆盖及逐级清除。AOT发布179.920s，5项本机Homebrew dylib最低OS链接警告，不证明macOS12支持；首次/var软链接目录7项MSB3202在规范/private/var路径重建后消除，属于临时工程路径错误。此处不是原生最终像素验收。

视觉驱动初轮编译引用／SizeType冲突已修正；6项执行虽通过，但查看暗色图发现仅局部ThemeConfigProvider为Dark，窗口继承文字仍明色，该批108帧排除，记录后清理。现改为公开ThemeManager切换实际Application／Window主题，保留相同轨迹重新采样，不把驱动问题记录为产品失败。
