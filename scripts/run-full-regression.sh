#!/usr/bin/env bash
#
# AtomUI 全量回归测试运行器
#
# 流程：预检残留进程 → 逐项目统计测试数 → 带进度/ETA/卡死预警的运行 → 汇总
#
# 用法:
#   scripts/run-full-regression.sh [--projects p1,p2] [--filter FX] [--stall 秒] [--prune]
#
# 输出行前缀（供 agent 解析）:
#   [COUNT]  统计阶段   [RUN] 项目开始   [PROG] 进度（每 30s）
#   [STALL]  卡死预警   [SUMMARY] 汇总   [ERROR] 错误
set -o pipefail

REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
FRAMEWORK="net10.0"
POLL=5          # 秒，轮询间隔
CADENCE=30      # 秒，进度行打印间隔
STALL=120       # 秒，无进度告警阈值
FILTER=""
PRUNE=0

# 默认项目集：按预期耗时降序，大套件先跑尽早暴露问题
PROJECTS=(
    AtomUI.Desktop.Controls.Tests
    AtomUI.Desktop.Controls.Rendering.Tests
    AtomUI.Localization.IntegrationTests
    AtomUIGallery.Tests
    AtomUI.Generator.Tests
    AtomUI.TypeMap.Linker.Tests
    AtomUI.Core.Tests
    AtomUI.Toolkits.GalleryBase.Tests
    AtomUI.Controls.Shared.Tests
    AtomUI.Desktop.Controls.DataGrid.Tests
    AtomUI.Build.Tasks.Tests
    AtomUI.Localization.Tests
    AtomUI.Controls.Tests
    AtomUI.Toolkits.GalleryBase.Generator.Tests
    AtomUI.Icons.Shared.Tests
)

usage() {
    sed -n '3,10p' "$0" | sed 's/^# \{0,1\}//'
    exit "${1:-0}"
}

while [ $# -gt 0 ]; do
    case "$1" in
        --projects) IFS=',' read -r -a PROJECTS <<< "$2"; shift 2 ;;
        --filter)   FILTER="$2"; shift 2 ;;
        --stall)    STALL="$2"; shift 2 ;;
        --prune)    PRUNE=1; shift ;;
        -h|--help)  usage 0 ;;
        *)          echo "[ERROR] 未知参数: $1"; usage 2 ;;
    esac
done

WORKDIR="$(mktemp -d /tmp/atomui-regression.XXXXXX)"
FAILURES="$WORKDIR/failures.txt"
: > "$FAILURES"
filter_args=""
[ -n "$FILTER" ] && filter_args="--filter $FILTER"

echo "[MODE] FINAL FULL REGRESSION — only after focused bug-module tests are green"

fmt_clock() { # 秒 -> mm:ss
    awk -v s="$1" 'BEGIN { printf "%02d:%02d", int(s/60), s%60 }'
}

# ---------- ① 预检残留进程 ----------
stray="$(ps ax -o pid=,command= | grep -E 'testhost|vstest' | grep -F "$REPO_ROOT/.artifacts" | grep -v grep || true)"
if [ -n "$stray" ]; then
    echo "[PREFLIGHT] ⚠ 发现本仓库残留测试进程:"
    echo "$stray" | sed 's/^/    /'
    if [ "$PRUNE" -eq 1 ]; then
        echo "$stray" | awk '{print $1}' | while read -r pid; do
            kill -9 "$pid" 2>/dev/null && echo "[PREFLIGHT] 已清理 PID $pid"
        done
        sleep 1
    else
        echo "[PREFLIGHT] 这些进程会抢占 CPU 导致耗时虚高；重跑并加 --prune 可自动清理"
    fi
fi

# ---------- ② 统计测试数（含构建） ----------
echo "[COUNT] 统计测试数（含构建, framework=$FRAMEWORK）..."
counts=()
total=0
for p in "${PROJECTS[@]}"; do
    cs="$REPO_ROOT/tests/$p/$p.csproj"
    if [ ! -f "$cs" ]; then
        echo "[ERROR] 测试项目不存在: $cs"
        exit 1
    fi
    out="$(dotnet test "$cs" --framework "$FRAMEWORK" --list-tests $filter_args 2>&1)"
    if [ $? -ne 0 ]; then
        echo "[ERROR] $p 构建/枚举失败:"
        printf '%s\n' "$out" | tail -15 | sed 's/^/    /'
        exit 1
    fi
    n="$(printf '%s\n' "$out" | grep -cE '^    [A-Za-z]' || true)"
    counts+=("$n")
    total=$((total + n))
    echo "[COUNT] $p: $n tests"
done
echo "[COUNT] 共 ${#PROJECTS[@]} 个项目, $total 个测试"

# ---------- ③ 运行 ----------
run_one() { # $1=项目名 $2=预期数 $3=全局已完成基数
    local p="$1" expected="$2" gbase="$3"
    local cs="$REPO_ROOT/tests/$p/$p.csproj"
    local log="$WORKDIR/$p.log"
    : > "$log"

    echo "[RUN] $p ($expected tests)"
    dotnet test "$cs" --framework "$FRAMEWORK" --no-build $filter_args \
        --logger 'console;verbosity=detailed' > "$log" 2>&1 &
    local pid=$!
    local t0 lastprog lastdone stalled
    t0=$(date +%s); lastprog=$t0; lastdone=0; stalled=0
    local now nextcadence
    nextcadence=$((t0 + CADENCE))

    while kill -0 "$pid" 2>/dev/null; do
        sleep "$POLL"
        now=$(date +%s)
        local donep lastname
        donep=$(grep -cE '^  (Passed|Failed|Skipped) ' "$log" || true)
        if [ "$donep" -gt "$lastdone" ]; then
            lastdone=$donep; lastprog=$now; stalled=0
        fi
        if [ "$now" -ge "$nextcadence" ]; then
            nextcadence=$((now + CADENCE))
            lastname=$(grep -E '^  (Passed|Failed) ' "$log" | tail -1 | sed 's/^  [A-Za-z]* //' | cut -c1-90)
            local elapsed gdone eta etafmt
            elapsed=$((now - t0)); gdone=$((gbase + donep))
            eta=$(awk -v e="$elapsed" -v d="$gdone" -v t="$total" \
                'BEGIN { if (d > 0) { printf "%d", e * t / d - e } else { print "?" } }')
            if [ "$eta" = "?" ]; then etafmt="?"; else etafmt=$(fmt_clock "$eta"); fi
            printf '[PROG] %5d/%d (%d%%) | %s | +%s | ETA %s | last: %s\n' \
                "$gdone" "$total" $(( total > 0 ? gdone * 100 / total : 0 )) \
                "$p" "$(fmt_clock $elapsed)" "$etafmt" "${lastname:--}"
        fi
        if [ $((now - lastprog)) -ge "$STALL" ] && [ "$stalled" -eq 0 ]; then
            stalled=1
            lastname=$(grep -E '^  (Passed|Failed) ' "$log" | tail -1 | sed 's/^  [A-Za-z]* //' | cut -c1-90)
            echo "[STALL] ⚠ $p 已 ${STALL}s 无进度; 最后完成的测试: ${lastname:-<无>}; 热点进程:"
            ps ax -o pid=,pcpu=,command= | sort -k2 -rn | head -3 | sed 's/^/    /'
        fi
    done

    wait "$pid"
    local rc=$?
    local passed failed skipped
    passed=$(grep -cE '^  Passed ' "$log" || true)
    failed=$(grep -cE '^  Failed ' "$log" || true)
    skipped=$(grep -cE '^  Skipped ' "$log" || true)
    grep -E '^  Failed ' "$log" | sed 's/^  Failed //' | awk -v p="$p" '{ print p ": " $0 }' >> "$FAILURES"
    local dur=$(( $(date +%s) - t0 ))
    local mark="✔"
    [ "$rc" -ne 0 ] && mark="✘"
    echo "[RUN] $p DONE $mark passed=$passed failed=$failed skipped=$skipped rc=$rc ($(fmt_clock $dur))"
    if [ "$rc" -ne 0 ]; then
        echo "[DIAGNOSTICS] $p 完整失败日志: $log"
    else
        rm -f "$log"
    fi
    return $rc
}

echo ""
overall_start=$(date +%s)
gbase=0
any_fail=0
i=0
for p in "${PROJECTS[@]}"; do
    if ! run_one "$p" "${counts[$i]}" "$gbase"; then
        any_fail=1
    fi
    gbase=$((gbase + counts[$i]))
    i=$((i + 1))
done

# ---------- ④ 汇总 ----------
total_dur=$(( $(date +%s) - overall_start ))
echo ""
echo "[SUMMARY] ===== 全量回归汇总 ====="
i=0
for p in "${PROJECTS[@]}"; do
    echo "[SUMMARY] $p: ${counts[$i]} tests"
    i=$((i + 1))
done
if [ -s "$FAILURES" ]; then
    echo "[SUMMARY] 失败测试:"
    sed 's/^/    /' "$FAILURES"
fi
if [ "$any_fail" -eq 0 ]; then
    echo "[SUMMARY] TOTAL: $total 个测试全部通过, 总耗时 $(fmt_clock $total_dur)"
else
    echo "[SUMMARY] TOTAL: 存在失败, 详见上方清单; 总耗时 $(fmt_clock $total_dur)"
    echo "[DIAGNOSTICS] 失败清单与完整异常栈已保留: $WORKDIR"
fi
if [ "$any_fail" -eq 0 ]; then
    rm -rf "$WORKDIR"
fi
exit "$any_fail"
