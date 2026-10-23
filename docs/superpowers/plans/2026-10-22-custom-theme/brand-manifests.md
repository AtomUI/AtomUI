# CT-09 注册资产与版本化品牌配方

依据已批准[品牌规范](../../../architecture/systems/theming/brand-recipes.md)，在CT-08统一事务之上完成数据边界；不是另建runtime。主agent独立执行，无新分支/worktree、不自动提交。

## 合同与具体接口

- `AtomUI.Theme.Branding.ThemeStyleAssetRegistration`：只读ID/revision、ControlType/ControlTokenIdentity、能力要求及静态编译`Func<Style>`。构造/登记不调用工厂；同ID/revision只能对应同一规范registration，不允许换工厂，同ID各修订保持owner。只对已启用的Token或Semantic Control元数据绑定，不触发可选包自动启用。
- `ThemeStyleCapabilities`表达协议能力（资格样式、编译模板、已注册静态资产）；scoped公共资源投影仍不支持，要求它必须诊断。不是OS平台认证标签。工厂图仍通过现有准备/owner/nesting/资源检查，拒绝将Visual实例作为共享Setter值。
- `IThemeManagerBuilder.AddThemeStyleAsset`及IAtomUIBuilder扩展负责显式登记，旧外部builder默认拒绝新能力；Manager构建后创建只读规范注册表。package append合并候选注册表，冲突/未知owner在任何工厂前失败，随既有package fence交换引用/回滚；仅登记资产不激活。
- `ThemeBrandRecipes`是同一Manager上的静态公开数据入口：Read、Write、ApplyAsync、ExportCurrent；不提供第二套Token编译/Style优先级引擎。旧/其他runtime明确不支持，不能返回成功。
- 版本1JSON manifest包含kind(complete/token-only)、id/revision、themeId、精确基础定义/registry要求、原始或有效Token配置（Inherit、nullable算法、Global/Control tokens/算法模式）、有序资产ID/revision、能力要求。完整导出使用已提交有效配置+选中资产及精确基础修订；token-only明确不能带样式，不能冒充完整等价。
- 读取手工JsonDocument/Utf8JsonWriter，无反射序列化/任意.NET类型/AXAML/代码。限制输入大小/深度/项数/字符串，拒绝重复key、未知成员/版本/算法/能力/资产/Control/修订；调用现有Normalizer验证值/别名，错误不运行工厂。
- 配方选择identity独立于Token snapshot。选择变化但资产内容相同只更新选中输入引用，不重建图；真正资产变化用CT-08事务。请求键/NoOp包含选择，失败保留旧选择。原XML v1/compiled ABI v1不改变。

## 执行清单

### D1 规范注册与能力

- [x] 搜索/复用既有Theme资产和包注册；临时metadata/重复/未启用owner/能力/无工厂副作用验证。
- [x] 新增`Theme/Branding/ThemeStyleAssetRegistration.cs`、`ThemeStyleCapabilities.cs`、`ThemeStyleAssetRegistry.cs`；对接ThemeManagerBuilder/接口/扩展及BuildCore。
- [x] 对接PackageState候选/提交/回滚及asset-only append门槛，已有资产descriptor保持规范引用。
- [x] 现有Style图验证增加所需Setter/模板边界，代表编译模板与专用Part实际命中/释放验证。

### D2 配方模型、格式与选择

- [x] 新增不可变recipe/reference/requirements/result模型及手工JSON reader/writer；先写限制/负例/nullable算法/控制算法/别名与往返用例。
- [x] Read/Apply复核同runtime、registry/base定义修订、显式已注册资产和能力，不从数据加载代码/资源/可选包。
- [x] Config/请求键/有效样式输入加入配方选择；区分选择变化、图内容变化和Token变化；同资产不同配方不重建图。
- [x] ExportCurrent完整与token-only标记、已提交有效配置与基础要求；导入同版本实际像素和状态等价。

### D3 验收

- [x] 负例有效RED/GREEN、旧格式合同聚焦、已有/新增scope与Popup、失败选择恢复、跨registry拒绝、package rollback、工厂/graph释放。
- [x] 脚本化默认前后零差异、完整manifest往返零差异及1DIP检错；实际编译模板/Part样式，不用找节点赋值定制。
- [x] 临时源/媒体/驱动清理、聚焦Core/Desktop及NET8，更新正式模块文档和进度；作者自审。

实际实现、失败前证、清理及验证见[结果](brand-manifests-results.md)。
