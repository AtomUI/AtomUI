# 编译主题包生成 ABI v1（已废止）

> 编译主题 ABI v1 已被 [编译主题 ABI v4](compiled-theme-abi-v4.md) 取代。`CompiledThemeProgram` 只接受 ABI 4，按旧 ABI
> 生成的程序在构造时抛 `NotSupportedException`（`Compiled theme ABI '…' is not supported; rebuild the package against the current AtomUI.`）。

ABI v1 的程序登记算法链（`SetAlgorithms`）、Control 算法模式（`ControlAlgorithmMode`）与 `IsDefault`，构建契约携带
`ThemeBuildAlgorithmAttribute`。这些概念随主题算法一起移除：ABI 4 的程序登记设计语言、固定的轴、锁定的种子和条件段，构建
契约改为携带 `ThemeBuildLanguageAttribute`。

旧包不需要手工迁移：用当前的 AtomUI 重新构建即可。包里的主题 XML 若仍是 v1 格式，先用
`dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<directory>` 换写为
[v2](theme-definition-xml-v2.md)。
