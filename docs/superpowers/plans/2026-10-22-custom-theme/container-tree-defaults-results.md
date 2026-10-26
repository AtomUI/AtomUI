# CT14 N3-T：TreeView 默认求值结果

本片执行[TreeView 默认迁移计划](container-tree-defaults.md)，属于[容器消费者计划 N3](container-consumers.md)。只完成 TreeViewToken 的 Global-only 默认函数与兼容分类；不表示 CT14、完整 N2/N3/N4、TreeView 家族或 CT25 已验收。本片尚未提交，第十六批授权不覆盖它。

## 源码与合同

`TreeViewToken` 的 12 项默认改为 `DependencyDefaultsV2`／声明 `Revision = 1`，由静态函数读取 `ControlTokenDefaultContext.Global`。没有 Own→Own 依赖，也没有新字段、Control API、Part、绑定、订阅、缓存或布局变化。保留原空构造、名称、类型、资源枚举数值与 dense slot。正式语义见 [Token 文档](../../../controls/desktop/data-display/tree-view/token.md)。

迁移前的 Global 和 Control 专属 Global 两项实际 scope 探针已经通过：ControlHeightSM=39／ControlItemBgHover=#123456 到达两个真实 header 和资源。此片不能描述为修复原版 Global 消费失败。

两目录选中色全源码／Gallery 核查仅有声明与默认赋值，没有目录树专用入口、状态或默认主题消费。`DirectoryNodeSelectedColor`／`DirectoryNodeSelectedBg` 归 Reserved／CompatibilityRetained，保留资源和配置往返，不接到普通 NodeSelectedBg、不新增目录树功能。容器已确认保留字段由 Menu/NavMenu 的 14 项加这 2 项成为 16 项；不据此推断其他家族分类。

## 前后验证与纠正记录

| 验证 | 原版 | 候选 | 含义 |
| --- | --- | --- | --- |
| V2 运行期 descriptor、默认与资源兼容五项 | 版本合同 1 FAIL，其余 4 PASS | 5 PASS | 原版 WholeV1 的机制缺口；不是消费 bug 反例 |
| 明暗实际默认／类型／key 名／枚举数值／dense slot | 24 行 | 24 行相等 | key 枚举数值与 slot 分别采集，不能相互替代 |
| 扩展 Global／Control Global／Own 透明与 0／撤回／同实例重挂／reset | 8 PASS | 9 PASS（含 V2 合同） | 保留原消费与明确下游优先级 |

实际运行期 `EvaluatorRevision` 为生成摘要 `13445413757306629316`，不是声明 Revision 的字面值 1；生成摘要还包括默认函数及依赖身份。最初把二者等同的临时断言失败已保留，纠正为版本 V2／摘要非零后重跑通过，没有为错误断言修改产品。原版 Dark 全局 seed 的 literal 断言也曾失败：Dark 算法会转换主色／错误色，改为独立公共 Shared 投影作预期后，原版与候选均通过。这些准备错误不计作产品 RED。

明暗基线及候选均来自真实公开 TreeView 的 scope 资源，未从公式文本计算预期。数值表、命令与日志保留于 `.superpowers/sdd/container-tree-audit/`；通用求值器与现有 TreeView 契约测试提供长期保障，不新增永久迁移测试。

## 视觉证据

固定 900×640 Window、520×430 TreeView、22 个子节点与长标题，保留原垂直溢出和裁剪；默认→Global→Control→Own→选中→折叠→reset，两个版本使用同样历史、数据和几何。正常动效后等待最小 650ms／实际最长 transition，再要求连续三帧相等，单阶段上限 4 秒。没有遮罩、裁剪、对齐、归一化或容差。

首轮 84 帧核对 provider 的 Light/Dark 算法。实际看图发现外层 Window 仍为 Light，因此只保留为作用域算法兼容证据，不算完整 Dark Host。补充 84 帧通过公开 Application ThemeRequest 切换真正 Window 主题，并逐帧断言空白背景明暗范围；已查看实际暗底／亮文字的默认与选中画面。

| 两批合计 | 数量 | 结果 |
| --- | ---: | --- |
| 原图 | 168 | 两版、两主题、main／A/A／1 DIP |
| A/A | 56 对 | 全 0 |
| 1 DIP 正对照 | 56 对 | 全部检出 |
| 配置恢复 | 24 对 | 全 0 |
| 匹配历史新旧 | 28 对 | 全 0 |
| 公开资源／header／viewport／状态 | 108 对 | 相等 |

四次采样执行分别为候选／原版 scope 各 234 检查、候选／原版完整 Host 各 276 检查。没有新视觉差异或有限容差需要批准；这仍是 Headless/Skia 证据，不是原生最终像素验收。

## Mac 原生与 AOT

真实 NSWindow／scale2，managed 与 NativeAOT 各 304/304 项通过，核对实际 V2 descriptor、非零摘要、12 项资源及 key 数值／类型、Global／Control Global／Own 0／撤回／同一 TreeView 重挂与恢复、实际 header MinHeight。两类 provider 外观各一窗口；外层原生 Window 保持 Light，不能写成完整原生 Dark 像素或真实键盘／指针验收。

原生工程第一轮受 `/var/folders` 与 `/private/var/folders` 路径别名影响，NuGet 生成的相对 transitive 项路径失效。将同一物理工程目录规范为实际路径、清理其专用 obj 后构建通过；没有为环境准备错误修改产品。

运行验证的实现 SHA 为 `dca44e88e193a5dc0c1e3ac06c6ced33f32b01b0a3c6a948d9529b409390486c`。最终源 SHA 为 `aed512ff9f74d140dedd3760c72add4530734505657b6fb1c7c7751c3b0b2197`：差别严格只有两个目录保留字段的 XML 说明行，删除这两行即恢复完整已测源码，没有声明或函数体差异；最终清理后的模块及 NET8 使用最终 SHA。

## 清理与最终验证

- 两份 Temporary C#、专用测试目录已删除，永久测试净新增 0。
- 外部原生工程 384 文件、1,049,628,152 字节及相关驱动已删除；未在仓库保存副本。
- 168 张 PNG 与 manifest 共 169 文件已删除，媒体审计 clean；只保留数值、身份、日志及清理收据，不提供已删截图链接。
- 清理后精确 `FullyQualifiedName~TreeView`：57/57，通过，测试耗时 2 秒（含构建命令总计约 46.7 秒）。
- Desktop Controls `net8.0` Release 构建：0 warning／0 error，约 20 秒。
- 完整 Token 源文件成员检查无问题；该文件不继承 Avalonia Control，不能报告成已审一个 Control。

正式文档更新当前求值与 Reserved 边界；CSV 只改 12 条 TreeView Token 和 1 条 TreeView 控件记录，其余行保留。十个实际消费者字段仅标为 V2DefaultsVerified，两保留字段 CompatibilityRetained；没有把本片资源与兼容证据升级成每个状态／Part／输入的完整验收。

作者另步自审已核对最终函数／接口差异、34个相关文档链接、13条目录行及清理身份；随后本批清理后 NavMenu／TreeView 合并精确范围 365/365、0 Skip，通过。此为作者审查，不声称独立评审。未改 Gallery 用法、ControlTheme 或交互。其他 Host、原生最终显示面、N2 的完整字体／交互／尺寸及 N3 其他容器、N4 保持开放。
