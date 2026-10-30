# 固定图标类型迁移 Rollout 审计

日期：2026-09-29

## 结论

停止将内置固定 `AntDesignIconProvider` 引用改成具体图标类型，并回退四个试点主题及其测试。该迁移没有改变已选
NativeAOT 闭包，不具备继续推广的收益条件。

## 试点

首批选择 ButtonSpinner、ComboBox、Pagination、QRCode 四个控件族。四个主题均可改成 AXAML property element，相关
模板/主题测试 57/57 通过，说明语义替代本身可行。

随后用独立 NativeAOT 探针分别只保留 ButtonSpinnerHandle 和 ComboBoxHandle：

| 控件 | `CreateIconChunk` 节点 | `AntDesignIconProvider` 相关节点 | 结果 |
| --- | ---: | ---: | --- |
| ButtonSpinnerHandle | 30 / 32,638 bytes | 37 / 33,656 bytes | 完整动态工厂仍保留 |
| ComboBoxHandle | 30 / 32,638 bytes | 37 / 33,656 bytes | 完整动态工厂仍保留 |

两个试点均无保留收益，因此剩余两个即使成功，最多也只有 2/4 成功，无法达到 rollout 所需的至少 3/4。停止继续发布
Pagination/QRCode 变体，避免无意义构建成本。

## 处理

- 回退四个试点主题到原有 Provider 写法。
- 删除只验证具体图标写法的试点测试。
- 保留本审计结论，避免后续重复进行同一迁移。
- 动态 Provider 的真正保留来源需要单独从编译型 AXAML/图标程序集闭包分析；在找到可缩小且保持动态 API 的根因前，
  不批量改写主题。
