# CT-12：首批Own默认、字重与shape结果

本包覆盖Button50、DropdownButton23、HyperLinkButton17、SplitButton1，共4个Own终端91字段。OptionButton19及完整9个具体控件的全部消费、原生闪动和平台出口仍单独执行，不能计作整个按钮家族完成。

## 失败与实现

初始6项验证5失败/1正对照：三类按钮显式字体20已进入FontSize，布局行高仍22而非28；显式Padding10,6未进入额外内容留白（7.5而非5）；字重700未消费（Normal）。祖先Bold继承原本正确，须保留。

四个终端改为DependencyDefaultsV2/Revision1，91个静态默认函数声明真实Own依赖。保留字段、类型、历史IconOny名称及原缺省公式；缺省用存在位，字体→DIP布局行高→Padding及Padding→extra/circle margin读取有效上游。所有110个既有Own默认值（包括未迁移Option）明暗逐值相同。

字重直接使用可选资源Unset的试验实际失败：原生绑定仍填充Normal默认，double700也不能直接转enum。最终ControlTheme MultiBinding提供明确继承回退、有限enum转换及原始资源查找；WhenExplicit只查询有效配置Own存在性。Button只观察受保护的实际InheritanceParent并投射只读值，分离释放；编译属性accessor无反射。显式400/700、清除、祖先实时变化、局部属性/普通Style/资源优先级及长寿命父对象移除后GC全部通过。

shape失败：Round将调用者CornerRadius3,4,5,6写成32。修复只保存内部测量半径，不在Measure修改公共属性，Default恢复原圆角。保留测量目标高度几何，Stretch排列高度不代替旧输入；140高Stretch的原测量半径合同已验证。

最终临时Desktop21/21通过（3.5106秒），覆盖三类字体依赖、六项LG/SM、显式0行高、两类Padding依赖、字重优先级、GC及shape；Core4/4（1.5074秒）覆盖缺省Unset/普通默认不变、显式0、Global拒绝、类型化owner拒绝和alias/canonical存在性。新增验证默认删除。

## 视觉与AOT

实际Skia/Headless Drawing=false、Avalonia12.1.4.0、scale1，1000×720，48实例，真实鼠标Click=1及键盘Tab。明暗A/A均0；1DIP检错35117/36546像素。Own迁移、字重接入、shape修复分别与原baseline比较idle/hover/pressed/release/focus，全部0像素。显式字体变化30360/30895，清除0；字重变化22161/22725，清除、局部覆盖恢复、祖先恢复均0；圆角Default→Round→Circle→Default回原场景0，恢复完整交互状态0。关键变化及暗色圆形图像已查看。

夹具先有ToggleIcon API、键盘参数和未分组Option错误；按真实API/分组合同修正。原670高夹具裁切末行2px，改720高后重新采完整基线；旧夹具媒体已清理，不能混入比较。默认函数初始命名与既有属性冲突，由编译器拒绝后统一DefaultFor前缀；临时大小枚举namespace错误修正。上述不计作原产品失败。

当前macOS26/arm64真实NativeAOT运行：三类20→28、可见Padding1，Split V2，字重900→700→400→900、原生本地清除和shape保留原圆角，全部PASS。链接本机openssl@3/brotli，保留最低OS26与目标12警告；不宣称旧OS、原生显示面或其他平台通过。

临时Desktop/Core目录为Temporary/CustomThemeButtons；外部Visual.csproj与Probe.csproj在系统临时目录。精确测试filter为FullyQualifiedName~Temporary.CustomThemeButtons；渲染驱动阶段before/after/font/shape及dark。声明位置调整前后全部成员语法hash相同。清理、最终focused/NET8与作者自审结果在出口追记。

出口完成：临时Desktop21/Core4及全部helper、驱动、AOT产物删除；71个取证文件清理、audit clean。清理后Core54/Desktop92聚焦用例通过，NET8 Desktop含Core/Common/Shared零警告零错误18.59秒。成员审计8文件/1控件0问题；本地文档链接与diff检查通过。作者自审保留完整家族边界，未将Option、其他全字段消费、原生闪动或其他平台标为通过。
