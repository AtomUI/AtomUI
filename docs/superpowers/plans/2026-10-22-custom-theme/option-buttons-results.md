# CT-12 OptionButton 默认依赖与实际消费结果

本包覆盖 OptionButton 的19字段及 OptionButton/Group 两个实际主题身份。前四终端91字段见[首包](buttons-defaults-results.md)，整个按钮家族尚未验收完成。

## 原始失败与修复

7项原产品反例均失败：显式字体20未进入对应比例；显式零比例未进入留白；Small错误读取Large高度平方；基础ButtonPadding未消费；普通和Outline选中背景保持透明。Small高度50的正确纵向结果为13（首版夹具14算术错误已更正，旧值0仍是有效失败）。

19字段迁移DependencyDefaultsV2/Revision1，显式字体→对应行高比例→留白，以及基础ButtonPadding→留白依赖。保留比例单位、字段名与颜色默认读值。Large留白资源15,0→15,7为历史公式纠正，默认固定高度实际渲染不变；显式值按新依赖进入渲染。

两背景用WhenExplicit存在位与普通资源绑定组合，缺省透明；Outline选中先取专属显式背景，再取普通显式背景。Solid选中及disabled保持专属语义色，ButtonColor覆盖两种未选中外观。无状态转换器不持有owner或订阅，不找节点改属性。Group和Item的Global各用真实identity；两个Transition集合复用动态owner绑定。

## 实际验证

临时Desktop15/15 PASS573ms，覆盖字体/比例/零值、Small高度、基础/显式下游留白、背景回退和原生本地优先级、状态色、Group Purple/Item Green隔离、800→200ms及同实例选择保留/清除。首次补充夹具存在namespace、Optional.Value、前景预期与禁用时机错误，修正未改产品；普通Headless未能证明hover/pressed命中，相关状态另走真实Skia输入，不能把未命中作为产品失败。Skia补充场景已实际验证悬停Magenta、按下Cyan、禁用Yellow，均断言真实伪类及最终画刷并通过；按下关键图已查看。

Skia/Avalonia12.1.4.0，1100×760、scale1，16组48项，两方向、两外观、四尺寸、选中/禁用及非白父背景。真实左中右hover命中，Click=1，Right键后选择有效。明暗A/A均0；1DIP检错17402/17394像素。默认idle/hover/pressed/selected/keyboard旧新均0；自定义变化69159/69184。Light自定义与Dark恢复关键图已查看，连体几何、透明父背景及禁用视觉保持。

主题往返有2个边缘像素RGB最大差1；严格旧源码重放也有相同2像素，新旧恢复阶段0，新旧初始/往返基准0。临时替换3份源码均按字节恢复。该原有栅格现象归CT-25，**不声称严格恢复为0或原生显示面通过**，不设置容差。

## 命令与出口

- 临时过滤：`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeOptionButtons`。
- 外部驱动：`dotnet run --project /private/tmp/atomui-option-visual/Visual.csproj -- <登记媒体目录> <before|after|legacy|colors> <文本目录> [dark]`。媒体和临时项目验收后删除，不作为可保留交付物。
- 当前macOS26/arm64真实NativeAOT程序运行PASS：font20→比例1.4→Padding7,3、选中Yellow、800ms和同实例选择保留/清除。保留本机OpenSSL/Brotli最低OS26而目标12的链接警告；仅验证当前OS和Headless注册/主题链，非最终显示面。清理后focused、NET8及清理结果在完成后追记；其他OS/Browser和原生闪动未验证。

出口完成：临时Desktop15用例、外部Skia/AOT项目及全部专用产物删除；58个媒体文件删除，audit clean。清理后OptionButtonGroup Theme/Orientation/Render30项PASS739ms；NET8 Release构建零警告零错误18.99秒（首次误用Debug因仓库仅net10而报7项TFM不匹配，改正确配置后通过）。成员审计2文件/0控件/0问题，154条本地文档链接有效，diff检查通过。作者自审确认可选背景转换器无owner状态，原有选择/组合几何与原生优先级保留；未宣称独立审查、原生显示面或其他平台通过。
