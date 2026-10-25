# CT-18 G1 DataGrid 默认计划结果

## 范围与实现

当前分包只覆盖 DataGridToken 默认求值和兼容组合。78个既有字段、类型、资源编号及slot保留；17个显式别名继续按CT06逐层归一与最终投影，其余61项改为DependencyDefaultsV2／revision 1的静态默认函数。字段与别名声明留在DataGridToken.cs，默认计划集中在DataGridToken.Defaults.cs。DataGridTokenAlignmentTests原有四类语义断言改用当前生成求值入口，无新增常驻case。

原代码直接赋值的8个非别名副本声明Own依赖：HeaderSplitColor、HeaderSortActiveBg、HeaderSortHoverBg、BodySortBg、FixedHeaderSortActiveBg、HeaderFilterHoverBg、SelectionColumnWidth、ExpandIconBg分别更新对应历史Table字段。派生字段仍可独立明确配置，清除后恢复当前依赖，显式0保留；这些字段不新增Alias。局部expand size／scale等公式继续展开原Global输入，HeaderIconHoverColor仍保留原RGB公式。默认计划不接通尚无实际消费者的历史槽位，不能据资源变化宣布所有视觉消费者已通过。

## RED／GREEN 与默认兼容

临时验证在tests/AtomUI.Desktop.Controls.DataGrid.Tests/Temporary/CT18Defaults，实际ThemeConfigProvider使用Default／Dark算法并断言ActualThemeVariant及ThemeContext.Appearance。最初把ISolidColorBrush误当Color读取、BoxShadows错误使用LINQ导致的驱动／编译错误不计产品RED。

- 首次合格6项：4失败／2独立覆盖正对照通过，11.574s。规范颜色未重算历史副本，SelectionColumnWidth=0后历史依赖仍32。
- 扩展至8对直接副本和独立覆盖／清除，旧版14项为12失败／2正对照通过，2.632s（已构建）；新版14项、原对齐1项及snapshot捕获1项共16通过，22.142s。
- 两套实际算法的默认及各14项自定义Global输入，4场景×78字段共312条，值、值类型、真实resource ID、storage slot全相同。自定义包含分数字号17.3、FontHeight1.37、ControlInteractiveSize19.7、LineWidth1.25、非对称PaddingXS/SM及非默认alpha/RGB颜色。
- A/B只切换Token原文件、默认函数文件及旧对齐入口，所有其他源码相同。使用当时当前字节备份并finally恢复，结果restored=true；未通过HEAD重置当前修改。

## Mac 原生程序与编译主题

macOS26／arm64／.NET10，真实AtomUI.Desktop.Controls.Window 640×440、scale2：managed37项3.551s，NativeAOT37项3.667s。验证compiled literal中旧TableHeaderBg归一、规范Own更新依赖、SelectionColumnWidth显式0、scope覆盖compiled根、派生明确值优先及逐层清除回根。同一个grid/provider往返，无运行时反射或测试后门。

NativeAOT发布179.920s，实际binary SHA256为540e94859d4119e7531b1cc04e5b2b3fcc2411a5d9c61b7b68fa97d41ec2eaad。Homebrew OpenSSL／Brotli有5项最低OS链接警告，不证明macOS12可发布。首次外部临时工程以/var软链接路径构建造成7项MSB3202，使用规范/private/var路径清理本工程obj后重建通过，非产品RED。

此范围是实际原生进程的资源／状态合同，未称为原生最终像素、其他OS或完整家族验收。

## Skia 对照

契约为实际产品Window 640×358、grid592×310、40行／4列／右冻结列1，标题/footer和双向溢出滚动条保留；明暗×Large／Middle／Small。原motion保持，采样至少500ms且覆盖声明Transition时长，连续3帧一致，8s上限。框架输入走hover、点击选择、双向wheel，选择及offset在配置／清除间保持。main／AA／1DIP使用独立实例和相同完整历史，不缩放、裁剪、对齐、遮罩或加容差。

驱动初次框架／SizeType及nullable编译问题单独记录。首批只切换局部scope，暗色文字仍继承明色Window；已查看失真明暗图后排除并清理127文件。改为公开ThemeManager设置实际Application／Window主题后，6项脚本通过135.310s，108帧中36组AA有2组不同：明色Middle滚动态23,932像素、暗色Small滚动态19,512像素，仅首条可见行hover背景；几何、selection、双向offset相同，其余34组AA为0，36组1DIP全部检出。已查看两组实际差异，保留失败计数，未作为合格基线。

完整轨迹补上滚动后从窗口空区移入实际可见行的框架MouseMove，断言真实hit属于该行及IsPointerOver。保留原wheel、约束、选择和滚动；该分包只对显式移动后的稳定hover取终态，原stationary-pointer回收后的hover更新不宣称已修复，留在G2输入／虚拟化出口。全部版本及main／AA／DIP均使用同一新增轨迹重新采样。

最终旧版6项151.080s／新版6项155.485s，216帧、252次比较全部通过：旧新版各36组AA为0；两版共72组1DIP变化8,317–13,047像素；old/new的main、AA、DIP各36对，共108对均0差异。显式hover、点击选择、双向wheel后实际offset为vertical200／horizontal100；全程grid592×310、origin(24,44)，DIP为(25,44)，双滚动条可见、右冻结列1、source40行；三档实际realized rows随viewport变化保持虚拟化。配置往返保持选择key1及双向offset，未重建grid。

已查看明色Middle滚动、暗色Large历史Own、明色Small历史Own、旧版暗色Small选择以及1DIP关键帧：表头、行文本、冻结分界、网格线、标题/footer和滚动条对应，暗色文字可读，选择背景清楚；所设绿色Own hover背景与文字低对比来自本次显式测试输入，不宣称该任意配色具备对比度合格结论。最终完整轨迹的脚本／视觉范围通过，不扩大到未完成的静止指针回收或整个DataGrid。

## 清理与最终验证

Native临时工程已核实purpose／checkout及无运行中的本次进程后删除，432文件／1,262,468,952字节，二进制SHA与37项结果单独保留。三份G1临时C#、6项专用helper／备份已删除，12份TRX压缩为counter／失败消息／hash后删除；最终媒体253文件清理，audit clean、findings0、audit_error=null，之前两组失格媒体各127文件也已清理。

清理后当前DataGrid主题／语义、尺寸、圆角、冻结及两项Windows CI失败方法的精确filter共34项通过，25.809s，无skip。该Mac复现为PASS，不能据此宣称Windows两项失败已修复。DataGrid及依赖NET8 Release构建32.072s、0warnings／0errors；3项C#成员审计0问题，正式／结果文档本地链接及git diff --check通过，未运行本地change／full回归。

作者按独立步骤检查了字段／别名／默认函数形态、8项依赖与独立覆盖边界、现有对齐断言、同历史比较和处置记录，无额外公共API或模板变更。按用户要求未使用subagent，作者自审独立性弱于新上下文评审。

G1默认计划与本分包限定的兼容／Mac NativeAOT／Skia／清理出口通过；CT18完整家族、静止指针回收、G2／G3及CT25其他平台保持开放。本分包未获Git提交批准。
