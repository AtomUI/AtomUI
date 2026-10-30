# AtomUI 代码风格规范

本文定义 AtomUI 仓库中跨模块通用的代码风格约束。命名规则仍以仓库根目录的
[`.editorconfig`](../../../.editorconfig) 为机器可执行事实来源；本文补充必须由开发者和 AI agent 在编写、
修改与审查代码时主动遵守的结构性规则。

## C# 控制流块体

所有 C# 控制流语句只要语法允许使用块体，就必须显式写出 `{}`，即使主体只有一行也不得省略。

适用范围包括但不限于：

- `if`、`else if`、`else`
- `for`、`foreach`、`while`、`do`
- `using` 语句、`lock`、`fixed`
- `checked`、`unchecked`、`unsafe`

正确写法：

```csharp
if (isEnabled)
{
    UpdateState();
}
```

禁止写法：

```csharp
if (isEnabled)
    UpdateState();
```

## Agent 完成门禁

AI agent 在任何任务中新增或修改 C# 代码时，都必须满足本规范。即使本次只新增或修改一行代码，也不得省略控制流
块体的大括号。

如果 agent 触及的代码片段、相邻上下文或同一编辑区域中存在可见的省略大括号控制流语句，必须在本次改动中一并
修正。未完成修正、未说明更大范围存量问题，或在新增代码中继续省略 `{}`，均不算任务完成。

表达式主体成员、switch expression、lambda expression、`using` declaration 等语法上不是控制流语句块体的形式，不受
本条约束；不得以这些例外为理由省略普通控制流语句的大括号。


## 控件声明结构检查

控件成员必须遵守[控件成员排列规范](control-development-guidelines.md#控件成员排列建议)。新增属性时，先确认注册字段、CLR wrapper、DirectProperty backing field 的实际访问级别与所属分区，再编辑；不能只在语义相近的属性旁插入。

控件修改完成后，针对所有触及的控件文件运行结构审查，而不只查看新增行：

```bash
python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/ImagePreviewer
```

`--path` 必须替换为本次实际范围，可以重复指定。检查器使用 .NET 10 SDK 的 Roslyn，临时构建位于系统临时目录，结束后删除；不会修改源码或运行测试。具体覆盖与边界见[可执行成员布局门禁](../../../.agents/skills/atomui-control-optimization/references/api-layout-and-splitting.md#executable-member-layout-gate)。

功能测试、编译成功和视觉验收不能替代声明结构检查。完成记录必须分别给出结构检查与模块验证结果。
