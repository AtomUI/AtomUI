# Gallery 主题工作台设计

本文定义主题工具的状态、操作、所有权、失败与验收。模块边界见[入口](overview.md)，完整品牌表达由[配方设计](../../architecture/systems/theming/brand-recipes.md)拥有。下文状态名是领域概念，不表示已有同名公开类型。

## 1. 职责与数据来源

AtomUIGallery拥有具体品牌、示例、编辑页面和生成服务接入；GalleryBase只承载已形成稳定复用的中立Shell、展示及检查能力。Core提供schema、规范化、编译与唯一提交协调者，Generator提供typed元数据/资产工厂。

工作台复用已提交Catalog、ThemeState、Global/Own及Part descriptor，不靠反射扫描控件、实例化所有Control或手工复制一份Token列表。品牌选择、Light/Dark/System、Compact、Motion/Wave按现有请求合同组合。

## 2. 草稿、预览、应用与保存

| 状态 | 所有权与语义 |
| --- | --- |
| 已应用 | 来自ThemeManager真实提交与配方表现结果，菜单点击不能直接当成成功 |
| 草稿 | 当前可编辑不可变输入，有原始、继承/有效值、显式存在状态和诊断；不修改活动资源 |
| 预览 | 草稿通过schema/值校验后，在明确隔离scope/owner中由同一运行时计算；失败不应用半成品 |
| 已保存 | 持久化的版本化配置和资产引用；保存本身不意味着已经应用，反之亦然 |

编辑产生新的草稿版本，旧预览请求取消/被替代时不覆盖新结果。恢复单字段清除显式覆盖，恢复品牌回到选中配方基线；不能用0/null冒充“继承”。清空全部草稿与切换应用主题分开表达。

普通预览和应用更新已有实例，保持输入、选择、焦点和滚动。必要的模板结构替换明确状态保存责任，不靠重建页面掩盖缓存和本地值残留。

## 3. 定制与诊断

- Global允许完整schema；当前Control未使用某个合法值时可以无视觉效果，不建立消费白名单。
- Active Own显示类型、单位、范围、默认依赖、适用状态和有效值；Legacy、Reserved、Deprecated区别呈现，不暗示保留字段已经有效。
- Part使用生成专用Style工厂及真实ContractType/route；预览高亮不能替代样式命中，禁止找节点直接赋值兜底。
- 诊断解释owner/scope、显式与继承值、别名冲突、当前状态/尺寸、目标Part和本地值压制等原因；生产控件不永久追踪全部访问。
- 并排比较品牌、状态和尺寸时各自作用域明确，兄弟预览不串值；已打开弹层参与owner预览和关闭清理。

## 4. 品牌与完整往返

提供Ant默认、shadcn、Bootstrap及零圆角、粗描边、大字体、紧凑、关闭动效等边界配方，覆盖按钮/输入/选择、反馈、日期时间、DataGrid和ColorPicker等实际启用能力。

Token-only导出标明范围；完整配方导出保留实际Token、组件覆盖、样式资产及修订/能力要求。导入缺少资产、Control、版本、值类型或产生冲突时明确诊断，不能忽略部分内容后声称等价。

工作台不修改XML v1或编译ABI v1的既有含义；新的组合格式遵守品牌协议版本。导入数据不执行C#/脚本或运行期AXAML。

## 5. 受限生成入口

生成服务只返回当前schema支持的Token、已注册品牌资产选择或已定义的类型化Part Setter数据。服务输出先作为草稿，经校验、编译预览后再应用/保存；流式未完成文本不逐段应用。

生成请求、取消、超时和旧响应有generation所有权，不使旧结果覆盖新编辑。不引入旁路ResourceDictionary、Token求值或Style优先级规则。服务模型、权限及平台网络能力按实际集成验证，不能依据前端设计宣称AI功能已交付。

## 6. 生命周期与失败

页面/预览scope拥有临时实例、资源binding、请求、跨根lease和高亮；离页、关闭弹层、换owner和功能关闭时释放。检查工具不修改控件的Clip、主题或行为来配合演示。

纯校验/准备失败且未改变活动表现时保留已应用状态；接线或预激活后失败，只有恢复成功才显示旧品牌保持。Token尚未提交但恢复失败时，明确显示旧Token仍有效、表现未完整恢复及需要修复的host。

Token已提交后的发布诊断及品牌表现结果分别按正式运行合同显示。普通观察者异常只形成独立诊断；实际影响样式、绑定、资源传播或表现核对失败时才报告表现未完整应用。不能把“Token Committed但表现失败”显示为完整品牌成功或已无损回滚。Shell状态从真实已提交结果及表现状态重新投影。

## 7. 验证

验证编辑→隔离预览→应用→保存→导入的完整路径，以及字段恢复、无效值、别名冲突、缺失资产、版本不兼容、生成取消/过期响应和失败草稿。导出再导入的有效值、样式和实际渲染等价。

已有实例状态保持、嵌套/兄弟、Native/Overlay及离页释放共同验证。手动工作台与AI入口分别有实现证据；Browser的保存/网络/宿主能力及各OS差异单独标明，不以一个Host通过外推全部。


## 8. 当前手动实现

Desktop Gallery 的 Theme → Theme workbench 打开现有 workspace 下的工具窗。它复用当前 runtime 和主题菜单，提供 Ant、shadcn、Bootstrap、Square、HeavyBorder、LargeType、Compact、ReducedMotion 八套 AtomUI 适配配方；第三方名称表示适配方向，不承诺逐像素复刻第三方组件。

先选择配方/明暗，再从注册 schema 选择 Global 或 Control owner 与 Global/Own 字段。界面显示类型、显式/继承状态和当前预览的有效值；Set override 创建草稿输入，Clear override 删除显式项，Restore preset 回到配方基线。兼容别名显示 canonical 名，Legacy/Reserved 无内置消费者字段为只读说明，完整 JSON 仍保留它们。字段不因列在 schema 中就被宣称具有当前视觉效果。

Preview draft 更新左侧现有实例，右侧基线是独立 scope；按钮、输入/选择、反馈、日期/颜色与Grid不会因换草稿重建。Root 与局部配方各保留自己的已注册基础 definition，根菜单切主题不会迫使局部配方改用另一份 definition；版本要求仍严格校验。预览 current 来自 scope 完成 receipt（含 NoOp），赋值/排队本身不算成功。

预览根在 AXAML 中声明当前 scope 的 ColorText、FontSize 和 FontFamily。应用草稿改变外层窗口字体时，预览中的继承字体仍来自各自 scope；恢复配方也不依赖外层应用先恢复。输入控件继续遵循原有字体优先级，工作台不另行改写子节点属性。

Apply to Gallery 使用同一个 ThemeManager 的真实事务。界面分别显示 Token 与 Presentation 结果、实际主题/品牌选择和最后应用的草稿修订。取消/新编辑/离页撤销旧请求所有权，旧返回不覆盖新草稿。代码不会恢复一个旧结果来替换新输入。

完整配方 JSON 可编辑后显式 Read JSON as draft；未读取的编辑阻止预览/应用/保存，异步文件导入同时检查 session 与编辑器修订。缺 owner、资产、版本、类型或别名冲突均显示既有协议诊断。完整保存沿宿主 StorageProvider 写入精确 UTF-8 并在完成后标记实际保存内容；保存不等于应用，旧保存完成不能标记新草稿已保存。仅 Token 导出标明 tokenOnly，不能把该输入升级伪装成 complete。宿主没有文件能力时可使用 JSON 区，明确显示能力诊断；本轮不驱动 OS 文件选择器 UI，实际 Mac 文件 API 的读写/截断/往返另有验证。

页面在离树时解除 runtime/请求/预览事件；Grid 的类型化本地 source 随视图释放。编译绑定与显式生成注册支持 AOT，没有运行期 AXAML、反射扫描全控件、第二个 ThemeManager 或旁路 Token 字典。CT24 AI 服务仍暂缓，本文第5节保持目标合同，不表示已经接入。

实现/当前Host证据与保留边界见 [CT22–26 执行记录](../../superpowers/plans/2026-10-22-custom-theme/brand-workbench-exit.md)。
