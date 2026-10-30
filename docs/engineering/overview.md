# 工程规范

本目录维护贡献、开发、平台、工作流、案例和工具链规则。架构文档定义系统不变量；Engineering 文档定义开发者
如何实现、评审和验证这些不变量。

## Contributing

- [AI 协作规范](contributing/agent-guidelines.md)
- [BUG Issue 规范](contributing/bug-issue-guidelines.md)
- [新特性请求 Issue 规范](contributing/feature-request-issue-guidelines.md)
- [Control 文档规范](contributing/control-documentation-guidelines.md)
- [Mobile 文档规范](contributing/mobile-documentation-guidelines.md)
- [文档结构与命名规范](contributing/documentation-structure-guidelines.md)
- [参考项目源码查找规范](contributing/reference-project-source-guidelines.md)
- [Changelog 规范](contributing/changelog-guidelines.md)

## Development

- [测试价值与生命周期规范](development/test-value-and-lifecycle.md)：Bug 修复强制验收、临时测试默认删除、常驻测试准入与清理。
- [AOT 编程规范](development/aot-programming-guidelines.md)
- [编译期诊断规范](development/compiler-diagnostics-guidelines.md)
- [代码风格规范](development/code-style-guidelines.md)
- [Control 研发规范](development/control-development-guidelines.md)
- [Control Token 规范](development/control-token-guidelines.md)
- [Scoped Resource Host](development/scoped-resource-host.md)
- [Popup Anchor 作用域](development/popup-anchor-scope.md)

## Platforms

- [平台工程总览](platforms/overview.md)
- [Apple iOS 工程环境](platforms/apple-ios/overview.md)
- [Android 工程环境](platforms/android/overview.md)：预实现架构；Host、命令和平台证据尚未验证。
- [Linux Gallery NativeAOT 发布](platforms/linux-native-aot-publish.md)
- [Windows Gallery NativeAOT 发布](platforms/windows-native-aot-publish.md)

## Workflows

- [按改动影响选择验证](workflows/affected-verification.md)
- [Gallery NativeAOT Release Workflow](workflows/gallery-aot-release-workflow.md)

## Case Studies

- [Avalonia DynamicResource 内存泄露案例](case-studies/avalonia-dynamic-resource-memory-leak-case-study.md)
- [Semantic Part Popup 首次打开生命周期竞态案例](case-studies/semantic-part-popup-first-open-lifecycle-case-study.md)
