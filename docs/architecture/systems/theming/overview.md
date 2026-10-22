# 主题系统

AtomUI 主题系统跨越 Core 运行时、Control 包、主题资产、Source Generator、配置文件和 Gallery 工具。

- [主题系统架构](runtime.md)：配置、编译、Snapshot、资源、作用域、事务、缓存、生命周期和 AOT。
- [Own Token有效求值](own-token-evaluation.md)：依赖默认值、显式覆盖、别名规范化、求值版本和继承边界的目标设计。
- [品牌配方与样式作用域](brand-recipes.md)：完整品牌表达、托管规则资格、跨根、提交前预激活与失败结果的目标设计。
- [Control主题输入与渲染](control-rendering.md)：owner、真实节点、原生优先级、状态/尺寸、自绘、更新与生命周期合同。
- [精确主题实现与验证边界](verification.md)：现行源码与上述目标合同的差异，机制、控件、平台及发布证据边界。
- [应用资源宿主设计](application-resource-host.md)：主题与语言共同挂载、外部主题优先级及单一所有权契约。
- [应用资源宿主验证](application-resource-host-verification.md)：源码实现证据、性能对照和验收边界。
- [惰性物化与资源查询](lazy-materialization.md)：独立惰性槽、资源 key 路由、稳定 provider、registry 复用边界与内存计费。
- [编译主题包](compiled-theme-packages/overview.md)：包内 XML 的类型化 C# 生成、统一运行期计算、生命周期和验证边界。
- [Control Design Token 继承架构](control-design-token-inheritance.md)：抽象定义层、终端 `sealed` Token、生成期
  schema 扁平化、跨程序集复用和运行时隔离边界。
- [Semantic Part 系统](semantic-parts.md)：稳定视觉区域、Selector、descriptor、Popup 和兼容性。
- [全局主题研发规范](../../../engineering/development/theming/overview.md)：主题实现、消费、兼容性及验收要求；目标设计与源码符合状态分别维护。
- [字体子系统](../typography/overview.md)：字体 Token 的定义、派生算法和字体资源注册边界。
- [主题定制指南](../../../guides/theming/customization.md)：应用主题、Token、ControlTheme 和 Semantic Part 的使用路径。
- [与其他框架的主题集成](../../../guides/theming/framework-theme-integration.md)：面向应用开发者的主题共存、资源覆盖和 Fluent 接入说明。
- [第三方 AtomUI Control Package 指南](../../../guides/theming/third-party-control-packages.md)：包作者的项目配置、主题约定、注册入口和 AOT 验证。
- [主题定义 XML v1](../../../reference/theming/theme-definition-xml-v1.md)：版本化主题文件协议。
- [AtomUI.Core 模块](../../../modules/core/overview.md)：主题运行时在 Core 项目中的源码所有权。
- [AtomUI.Generator 模块](../../../modules/generator/overview.md)：主题和 Semantic Part 的构建期生成实现。
