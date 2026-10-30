---
name: atomui-full-regression
description: Use when the user asks to run the full AtomUI regression test suite ("全量回归", "全量测试", "跑全部测试", "full regression"). Runs scripts/run-full-regression.sh which counts tests first, then executes with live test name, progress, ETA and stall detection, and produces a per-project summary. Never report full-suite success without the script's final [SUMMARY] line.
---

# AtomUI 全量回归测试 Skill

## 触发条件

仅在用户明确要求全量回归 / 跑全部测试 / full regression，或执行 version-release 时使用。Bug 修复和普通代码改动
完成后不自动调用本 skill，只验证所修改模块的测试；单个项目或少量测试的日常迭代直接使用精确 `dotnet test --filter`
或 `test.py run --scope iterate --path ...`。

## 执行方式

仓库根目录执行：

```bash
scripts/run-full-regression.sh
```

可选参数：

| 参数 | 说明 |
| --- | --- |
| `--projects p1,p2` | 只跑子集（调试 skill 或快速冒烟时用） |
| `--filter FX` | 透传 vstest 过滤器（统计阶段同步生效） |
| `--stall N` | 无进度告警阈值秒数（默认 120） |
| `--prune` | 预检发现本仓库残留 testhost 进程时自动清理 |

## Agent 执行规程

1. **后台启动**：用后台任务运行脚本，输出重定向到日志文件。
2. **周期播报**：每 60~90 秒读一次日志，向用户播报最新 `[PROG]` 行
   （全局进度、当前项目、ETA、当前测试名）与刚完成的 `[RUN]` 结果；
   无新进展时也要说明，不得静默。
3. **卡死处置**：出现 `[STALL]` 行时，立即把行内信息（哪个项目、最后完成的测试、
   热点进程）原样报告给用户，由用户决定 kill 或等待；不得擅自终止进程。
4. **失败处置**：`[RUN] ... ✘` 或 `[ERROR]` 出现时记录，继续跑完剩余项目，
   最后汇总时一并列出。
5. **完成判定**：只有看到最终 `[SUMMARY] TOTAL: ... 全部通过` 行才可宣称全量通过；
   `[SUMMARY]` 给出失败清单时必须逐条转述，不得省略。
6. **耗时对照**：使用最近一次同机、同项目集的 `[SUMMARY]` 或验证历史作为基线；不得沿用硬编码的旧耗时估计。
   显著放大（>2x）即使通过也要提示可能存在环境争抢或性能回归。
7. **Bug 修复硬顺序**：确认 focused 模块测试全绿且实现不再变化后才启动一次全量。禁止在实现过程中把全量当作探针。
8. **全量失败后**：必须让当前脚本跑完并收集全部失败。随后对失败逐项做 focused 修复，不得每修一项重跑全量；全部
   focused 结果转绿后，再执行一次最终全量确认。

## 输出行前缀参考

- `[COUNT]` 统计阶段（含构建）
- `[RUN]` 项目开始 / 项目完成（✔ / ✘）
- `[PROG]` 进度行（每 30 秒）
- `[STALL]` 卡死预警（附最后完成测试与热点进程）
- `[SUMMARY]` 最终汇总
- `[ERROR]` 致命错误

## 注意

- 脚本按项目串行执行，保证耗时与历史基线可比；不要并行运行两个实例。
- 预检报告的残留进程若非本次运行产物，清理前需用户确认（`--prune` 仅自动清理
  命令行中含本仓库 `.artifacts` 路径的 testhost，不会触碰 Rider 等无关进程）。
