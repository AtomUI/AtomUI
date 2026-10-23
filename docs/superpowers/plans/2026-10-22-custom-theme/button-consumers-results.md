# CT-12 按钮实际Global、独立配色和状态画刷结果

本包接续[五终端历史110字段](option-buttons-results.md)，仍不代表完整家族/原生显示面验收。处理Button、Dropdown、HyperLink、Split、Icon、Toggle六类公开owner及7个主题文件。

## 原始失败与改动

实际Global/时长反例：Button/Hyper高度、Split边框、Icon/Toggle图标尺寸未消费自身配置，三个剩余Transition集合配置800ms仍200ms。Dropdown初始夹具错误检查MinHeight，修正为原Height合同后重新用旧源码证明32而非48；产品不改固定高度。9项消费检查最终全绿。

107处Shared Global读取改成对应主题owner的类型化TokenResource；Hyper/Icon/Toggle三集合接入TokenTransitions，保留原style Setter与默认200ms。动态scope清除和独立identity检查通过。

另外两个反例：Dropdown主色配置Green却读取Button Red；Button.DefaultBg=Red意外渗透Dropdown。为已有渲染算法补23个Dropdown颜色/阴影Own字段（23→46），默认来自Dropdown有效Global，reader有限23项静态key映射并分别捕获两身份；不合并配置、不解析枚举字符串或扫描反射。历史1195字段保留统计口径，新增23另计，当前审计表含Rate新增1共1219行。显式DefaultBg/PrimaryColor/TextTextColor和清除通过；未注册应用派生类沿用已知产品基类。

Hyper加载margin的旧LocalValue代码绑定压住AXAML覆盖（期望5,2，实际13,7）。直接编译ElementName style Setter被实际编译器AVLN2000拒绝，模板名字不在该编译namescope；项目AOT规范禁止新增字符串绑定。因此保留有具体原因注释的同模板强类型代码兜底，改Template优先级并删除重复loading资源Setter。实际源margin9,3→13,7、AXAML覆盖5,2、移除覆盖、重模板、分离/重挂全部通过；两个端点属于同一模板，重模板释放旧绑定，不新增外部host订阅。没有宣称完全声明式或新增私有Part公共合同。

Toggle四个既有状态画刷API仅声明未消费，normal/selected/disabled三反例失败后通过。ControlTheme编译Self绑定提供Normal=ColorIcon、Active=ColorText、Disabled=ColorTextDisabled、Selected跟随Normal，最终按disabled/pressed/hover/checked/normal选择。默认checked颜色保持；真实鼠标验证Normal Magenta、pressed Cyan、点击选中Yellow、禁用Lime，均断言实际画刷与交互状态。未直接设伪类或改私有Part。

临时Desktop18/18 PASS841ms，零skip。没有把错误夹具、未执行状态或编译错误算产品RED。

## 像素与输入

真实Skia/Avalonia12.1.4.0，1820×680、scale1，48实例，四尺寸、普通/主色/危险、图标、disabled、Text及组合场景；真实hover/pressed/released、Click=1和Tab。明暗A/A均0，1DIP检错26256/27209像素。Global接线、Dropdown独立Own、Hyper优先级和Toggle画刷接入后，最终idle/hover/pressed/released/keyboard旧新全0。Global自定义变化86999/87625，配置清除0。已查看Dark自定义和Toggle选中关键图。

首版1400宽夹具中Custom Split的既有子项溢出遮住邻列；保留根Width140和原条件，只扩大列距/viewport后重采全部基线，旧14媒体清理，不混作对照。既有溢出并未修复或隐藏，CT-20另核；循环loading不冻结冒充稳定，仍在家族动态过程专项。

## 出口

命令：临时过滤FullyQualifiedName~Temporary.CustomThemeButtonConsumers；外部Visual.csproj阶段before/after/variant/final/toggle及dark，媒体由scripted-visual-acceptance登记在仓库外。当前NativeAOT、临时清理后focused/NET8、链接和自审出口待实测追记；原生用户闪动、完整字体/字段/弹层和其他平台仍开放。

当前macOS26/arm64 NativeAOT真实程序PASS：六Global owner、三时长800ms链、Button Red/Dropdown Yellow独立Own、Hyper实际同模板margin13,7、Toggle选中画刷和同实例清除。保留本机OpenSSL/Brotli最低OS26对目标12的链接警告，不外推其他平台或最终显示面。成员审计3文件/1控件/0问题；清理后的最终聚焦/NET8出口仍在执行。

清理出口：18项临时测试及AXAML夹具、两个外部项目/专用编译产物全部删除；当前58媒体及首版14媒体删除，audit clean。清理后的Buttons模块193项PASS6秒，NET8 Release零警告零错误23.50秒；282本地文档链接有效，diff clean。作者自审确认有限映射无反射、所有新增字段各自V2默认、原生优先级和默认像素保留。剩余字体/全字段、popup/loading动态过程、SimpleButton边界与原生显示面继续按家族计划推进，未计整个家族完成。
