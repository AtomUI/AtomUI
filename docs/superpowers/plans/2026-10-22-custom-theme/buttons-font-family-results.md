# CT-10 / CT-12 字体族精确消费结果

覆盖Button/Dropdown、Hyper、Split、Option/Group六个公开owner；本包不代表全部控件字体验收。

## 原始失败与改动

实际6项失败：祖先Courier New正确继承，但控件Global FontFamily=Times New Roman后仍Courier。Option最初独立使用触发父Group断言，改为真实Group后重新取得同样6项失败676ms。

WhenExplicit从Own扩展到已注册Control的类型化Global，检查正规化共享及该Control的直接Global字段；Own/alias路径保持，普通Shared/wrong-owner仍拒绝，显式0有效，缺省found+Unset阻止外层回退。内部包装key改名，不改公开普通查找、默认求值或配置来源。公开合同是直接输入存在性，不从算法输出或数值猜来源。

Core内部ControlTypographyBindings使用编译Self observable；各控制仅通过内部接口读受保护实际InheritanceParent，AXAML MultiBinding拥有最终FontFamily。父订阅在分离停止，ParentProperty/挂载刷新，原生绑定释放时移除事件；无反射、强全局表或代码改目标值。既有Button FontWeight机制保持。

临时13/13 PASS732ms：六owner覆盖配置/清除/祖先实时变化/同值显式/本地优先级；共享Global存在、0、owner拒绝；原始资源和应用Style覆盖、字重Bold/400/恢复；移动/重挂；长寿命父节点移除Button/Hyper/Split/Group后4项GC通过。资源夹具先误用owner-neutral字典key，改生成的ButtonTokens.Identity资格后通过；其namespace编译错误不计产品失败。

## 视觉与声明结构

Skia/Avalonia12.1.4.0，1240×720、scale1，24实例/四尺寸/六owner，原本有效的Courier New祖先。明暗A/A均0，1DIP检错23712/24199；idle/hover/pressed/keyboard旧新全部0。Times New Roman实际变化36105/36391，清除恢复0；真实Click=1/Tab保持。已查看原始Light和配置Dark完整图。

首次批量接口插入误匹配同文件ButtonType枚举，被结构审计捕获后纠正；缺LogicalTree import属于编译接线错误。Split既有26项声明排列问题与新增接口region一并整理：5文件/2控件审计0，重排前后所有成员、头部和import token哈希完全相同。保留既有RelayBind到运行时非Visual Flyout，补明ControlTemplate不可达及unregister释放原因。未把结构整理冒充行为修复。

## 出口

临时filter为FullyQualifiedName~Temporary.CustomThemeButtonFontFamily；外部Skia驱动before/after及dark，NativeAOT Probe按六owner真实字体/清除/祖先/共享输入检查。AOT、清理后focused/NET8及媒体清理待完成后追记；其他owner和平台仍开放。

当前macOS26/arm64 NativeAOT实际程序PASS：六owner继承Courier→控件Times→清除Courier→祖先Arial→共享Verdana→清除Arial，Button Bold继承保持。保留本机OpenSSL/Brotli最低OS26对目标12链接警告；不外推其他OS或原生显示面。6文件/2控件成员审计0，356本地链接有效。Split静态属性注册相对顺序保持，ClickEvent移至事件段；初始化项无相互引用、消费者在静态构造完成后运行，成员哈希并不代替这项初始化审查。最终清理后Core/Desktop/NET8仍在执行。

出口完成：13项临时测试、两个外部项目与全部专用产物删除；24截图及管理元数据共25个取证文件清理，audit clean。清理后Core30项PASS430ms、Desktop223项PASS5秒，NET8 Release零警告零错误22.61秒。无新增常驻测试，成员/链接/diff检查通过，作者自审完成。没有提交；其余owner与最终平台继续按任务表推进。
