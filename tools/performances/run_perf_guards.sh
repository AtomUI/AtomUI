#!/usr/bin/env bash
#
# Regression guards for performance work: no visual change and no new leak against an unoptimized build.
#
# usage: tools/performances/run_perf_guards.sh BASELINE_BIN CANDIDATE_BIN OUTPUT_DIR [SHOWCASES]
#
#   BASELINE_BIN   Release output folder of the unoptimized product (contains AtomUI.GalleryPerformance.dll
#                  and AtomUI.ControlBaseline.dll), for example a worktree of the base commit.
#   CANDIDATE_BIN  Release output folder of the optimized working tree.
#   SHOWCASES      Comma-separated Gallery keys, default all.
#
# Visual guard: each build captures every Gallery page twice at full height with all ShowCase items materialized,
# and every control type twice across its states (visual enum values, booleans, disabled, forced pseudo-classes);
# pixels that differ between the two captures of one build (animations, clocks) are masked, every other pixel
# must match. Leak guard: control types and Gallery pages must be collectable after close or navigation, and
# the candidate may not leave more instances alive than the baseline.
#
# Exit code is non-zero when the visual guard reports a change or the candidate leaks more than the baseline.
set -uo pipefail

BASE="${1:?baseline bin folder}"
CAND="${2:?candidate bin folder}"
OUT="${3:?output folder}"
PAGES="${4:-all}"
HERE="$(cd "$(dirname "$0")" && pwd)"

mkdir -p "$OUT"
status=0

snapshot() {
    local bin="$1" dir="$2"
    rm -rf "$dir"
    dotnet "$bin/AtomUI.GalleryPerformance.dll" --audit --audit-full --audit-loose-ready \
        --audit-showcases "$PAGES" --timeout-ms 40000 --snapshot-dir "$dir" > "$dir.log" 2>&1
    grep -c FAILED "$dir.log" | sed "s|^|[snapshot] $(basename "$dir") failed pages: |"
}

echo "[visual] capturing baseline and candidate twice each"
snapshot "$BASE" "$OUT/before-a"
snapshot "$CAND" "$OUT/after-a"
snapshot "$BASE" "$OUT/before-b"
snapshot "$CAND" "$OUT/after-b"
python3 "$HERE/compare_snapshots.py" "$OUT/before-a" "$OUT/before-b" "$OUT/after-a" "$OUT/after-b" \
    --diff-dir "$OUT/diff" > "$OUT/visual.txt"
visual=$?
grep -E "VISUAL CHANGE|LAYOUT CHANGED|MISSING|^pages:" "$OUT/visual.txt"
[ $visual -ne 0 ] && status=1

echo "[visual] control states (every type: enums, booleans, disabled, pseudo-classes)"
for side in before-a:$BASE after-a:$CAND before-b:$BASE after-b:$CAND; do
    name="${side%%:*}"; bin="${side#*:}"
    rm -rf "$OUT/states-$name"
    dotnet "$bin/AtomUI.ControlBaseline.dll" --mode atomui --state-snapshots "$OUT/states-$name" > "$OUT/states-$name.log" 2>&1
    tail -1 "$OUT/states-$name.log" | sed "s|^|[states] $name: |"
done
python3 "$HERE/compare_snapshots.py" "$OUT/states-before-a" "$OUT/states-before-b" "$OUT/states-after-a" "$OUT/states-after-b" \
    --diff-dir "$OUT/states-diff" > "$OUT/states.txt"
states=$?
grep -E "VISUAL CHANGE|LAYOUT CHANGED|MISSING|^pages:" "$OUT/states.txt" | sed 's/^pages:/control types:/'
[ $states -ne 0 ] && status=1

echo "[leak] Gallery pages"
for side in before:$BASE after:$CAND; do
    name="${side%%:*}"; bin="${side#*:}"
    dotnet "$bin/AtomUI.GalleryPerformance.dll" --audit --audit-loose-ready --audit-showcases "$PAGES" \
        --timeout-ms 40000 --leak-check 3 > "$OUT/leak-pages-$name.txt" 2>&1
    tail -1 "$OUT/leak-pages-$name.txt" | sed "s|^|[leak] $name: |"
done

echo "[leak] control types"
for side in before:$BASE after:$CAND; do
    name="${side%%:*}"; bin="${side#*:}"
    dotnet "$bin/AtomUI.ControlBaseline.dll" --mode atomui --leak --count 20 > "$OUT/leak-controls-$name.txt" 2>&1
    tail -1 "$OUT/leak-controls-$name.txt" | sed "s|^|[leak] $name: |"
done

python3 - "$OUT" <<'EOF' || status=1
import re, sys
out = sys.argv[1]
def alive(path, pattern):
    result = {}
    for line in open(path):
        m = re.match(pattern, line)
        if m:
            result[m.group(1)] = int(m.group(2))
    return result
regressions = []
for kind, pattern in (('pages', r'(\S+): visits=\d+ alive=(\d+)'), ('controls', r'(\S+): (\d+)/\d+ alive after close')):
    before = alive(f'{out}/leak-{kind}-before.txt', pattern)
    after = alive(f'{out}/leak-{kind}-after.txt', pattern)
    for key, count in after.items():
        if count > before.get(key, 0):
            regressions.append(f'{kind}:{key} {before.get(key, 0)} -> {count}')
print('[leak] new or larger leaks: ' + (', '.join(regressions) if regressions else 'none'))
sys.exit(1 if regressions else 0)
EOF

echo "[result] $([ $status -eq 0 ] && echo PASS || echo FAIL) (details in $OUT)"
exit $status
