# CT13 CR4：四个公开owner可选字体结果

## 合同与最小实现

承接[计划](input-check-radio-typography.md)，CheckBox、RadioButton、CheckBoxGroup、RadioButtonGroup分别消费自身有效Global FontFamily/FontSize。未显式配置时保留真实InheritanceParent，显式配置（包括恰好等于当前父值）生效；清除、换父及重挂读取当前父值。普通owner Style及LocalValue保留优先级，公开子item维持独立身份与继承链。

只改四个C#薄封装与四个ControlTheme：复用已有内部IThemeInheritanceSource只读桥及ControlTypographyBindings三输入转换器，最终八个Setter均在AXAML；不改Core、不新增Own/尺寸档位/IsCustomFontSize、缓存、旗标、代码绑定或字体写回。protected父不能由AXAML直接读取的原因在桥接注释中说明。既有颜色、输入/选择、Own默认与Transitions保持。

## 有效前后与当前字体

四owner字体族与字号共8项原始失败，642ms：原父Courier New/18与实际字形正确，配置Times New Roman/27后仍停留在父值。候选8项通过，扩展13项涵盖实际GlyphTypeface/FontRenderingEmSize、兄弟隔离、显式值等于父值的存在位、Style/LocalValue覆盖及清除、当前祖先更新、7个关闭后对象的WeakReference GC回收；最后13项0FAIL/0Skip，790ms。

完整Frame另通过。先保留仅父字体场景，随后补足实际未配置根字体的完整基线；两侧单独进程仅执行Frame用例，独立自然与直接实例共60状态记录/侧。实际默认getter为$Default/12，父Courier/18、Arial/21、配置及清除完整同轨迹；两次1DIP均检出且复位0。非配置224条owner字体、Bounds及实际字形记录相同。没有通过禁用正常动效、重建一侧或更改尺寸条件消差。

全部配置像素、原始同版本失败及七对精确边缘差异集中见[视觉审查](input-check-radio-typography-visual-review.md)。用户已单独批准本次范围；直接非配置阶段均0，实际默认/祖先改变/最终退出原→候选均0；N01–N07仍按原始25/32点记录，未改成0或设置容差，CT25继续。

```bash
# 临时阶段，现已删除源码
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeCheckRadioTypography
# 两侧完整Frame采用相同精确filter独立进程
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeCheckRadioTypography.TypographyFrames
# 清理后的最终模块
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~AtomUI.Desktop.Controls.Tests.CheckBox|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.RadioButton'
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
```

## 当前Mac managed / NativeAOT

实际NSWindow、scale2、520×260，各8窗口覆盖明暗四owner，managed/NativeAOT各433项、exit0。公开API检查真实字体族及字号、GlyphTypeface/FontRenderingEmSize、逻辑焦点保持、父变化时显式值稳定、Style/LocalValue及撤回、清除、换父、相等值存在位和同一实例重挂。未用internal ThemeScope或反射推断原生消费，未把逻辑焦点API检查当作原生键盘事件。

ILC实际引用官方Avalonia12.1.4/net10.0/Avalonia.Base.dll。五条本机OpenSSL/Brotli macOS26 dylib对deployment12链接警告保留，不能外推最低OS。当前原生资源/字形检查不代表最终显示面或其他Host验收。

## 清理和作者另步自审

两份临时源码与目录、临时原/候选源快照和媒体映射删除，常驻测试净增0；外部工程与全部产物419文件、1,376,157,799 bytes删除。314PNG（含两个完整差异图）与manifest共315文件清理，媒体audit clean。NET8 Release 0 warning/0 error，34.62秒；清理后原CheckBox/RadioButton模块51PASS、0FAIL、0Skip，1秒。

成员审计四个薄封装文件、0个自带注册声明的control、0问题；另按实际四控制手工核对构造顺序、内部显式接口与protected生命周期入口，未把审计工具的0控制当成未检查的借口。无新public/protected成员或属性注册，无C#绑定；字体入口与所属token身份、实际父读取、Style/Local原生优先级、父监听释放、AOT和源文件边界均已作者另步自审。用户要求主agent独立执行，因此没有独立评审。

本字体分包不关闭完整CT13、其他字体维度/业务状态、平台及CT25；未提交、未调度Actions、未发布NuGet。
