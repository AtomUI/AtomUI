# AtomUI 主题定义 XML v1 规范（已废止）

> v1 格式（namespace `https://atomui.net/schemas/theme/v1`）已被 [主题定义 XML v2 规范](theme-definition-xml-v2.md) 取代。
> `ThemeDocumentReader` 不再读取 v1 文档：根元素在 v1 namespace 时报 `ATMTHM1004`，消息中给出换写命令。

v1 用 `<Algorithms>` 声明算法链，一个文件只对应一种明暗与密度；v2 里主题只描述配置——所属语言、固定的轴、覆盖的值——
计算全部属于设计语言，明暗、密度、风格与形态的差异写在 `<When>` 条件段里。

现有 v1 文件用工具链的转换命令一次性换写为 v2：

```bash
dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<directory> [-p:AtomUIThemeDefinitionOutput=<directory>]
```

省略输出目录时原地替换。逐项的换写规则（算法链、`DesignLanguage`、`BrandColor` / `NeutralColor`、`IsDefault`、Control 的
`Algorithm` 与 `<Algorithms>`）见 v2 规范的「从 v1 换写」一节。

v1 的 XSD 仍保留在 [schemas/atomui-theme-v1.xsd](schemas/atomui-theme-v1.xsd)，只供换写前核对旧文件，运行时与构建期都不使用它。
