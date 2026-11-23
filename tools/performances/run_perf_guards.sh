#!/usr/bin/env bash
#
# Regression guards for performance work: no visual change and no new leak against an unoptimized build.
#
# usage: tools/performances/run_perf_guards.sh BASELINE_BIN CANDIDATE_BIN OUTPUT_DIR [SHOWCASES] [DESIGN_LANGUAGE APPEARANCE DENSITY]
#
#   BASELINE_BIN   Release output folder of the unoptimized product (contains AtomUI.GalleryPerformance.dll
#                  and AtomUI.ControlBaseline.dll), for example a worktree of the base commit.
#   CANDIDATE_BIN  Release output folder of the optimized working tree.
#   SHOWCASES      Comma-separated Gallery keys, default all.
#   DESIGN_LANGUAGE APPEARANCE DENSITY
#                  Optional design-language variant, as listed by AtomUI.GalleryPerformance --list-variants
#                  (for example "Shadcn.Nova Dark Compact"). Every page, state and leak run uses it on both builds.
#
# Environment:
#   SNAPSHOT_TOOL_ARGS     extra arguments for the page and state captures, e.g. --freeze-loading-icons
#   SNAPSHOT_COMPARE_ARGS  extra arguments for compare_snapshots.py, e.g. --ignore-near-animation
#   NORMALIZE_PALETTE_NAMESPACE=1  compare the candidate Palette namespace label with the approved baseline text
#   REPEAT_UNSTABLE_PAGES=1  collect two more frames per build for any failed page with observed motion
#
# Visual guard: each build captures every Gallery page twice at full height with all ShowCase items materialized,
# and every control type twice across its states (visual enum values, booleans, disabled, forced pseudo-classes);
# pixels that differ between the two captures of one build (animations, clocks) are masked, every other pixel
# must match. Leak guard: control types and Gallery pages must be collectable after close or navigation, and
# the candidate may not leave more instances alive than the baseline.
#
# Exit code is non-zero on incomplete capture or leak evidence, visual change, or a new or larger leak.
set -uo pipefail

BASE="${1:?baseline bin folder}"
CAND="${2:?candidate bin folder}"
OUT="${3:?output folder}"
PAGES="${4:-all}"
VARIANT_ARGS=()
if [ $# -ge 5 ]; then
    if [ $# -ne 7 ]; then
        echo "a variant needs DESIGN_LANGUAGE, APPEARANCE and DENSITY" >&2
        exit 2
    fi
    VARIANT_ARGS=(--design-language "$5" --appearance "$6" --density "$7")
fi
HERE="$(cd "$(dirname "$0")" && pwd)"
candidate_snapshot_args=()
if [ "${NORMALIZE_PALETTE_NAMESPACE:-}" = "1" ]; then
    candidate_snapshot_args=(--normalize-palette-namespace)
fi

mkdir -p "$OUT"
status=0

snapshot() {
    local bin="$1" dir="$2" pages="$3" result=0
    shift 3
    rm -rf "$dir"
    dotnet "$bin/AtomUI.GalleryPerformance.dll" --audit --audit-full --audit-loose-ready \
        --audit-showcases "$pages" --timeout-ms 40000 --snapshot-dir "$dir" \
        ${VARIANT_ARGS[@]+"${VARIANT_ARGS[@]}"} ${SNAPSHOT_TOOL_ARGS:-} "$@" > "$dir.log" 2>&1 || result=$?
    grep -c FAILED "$dir.log" | sed "s|^|[snapshot] $(basename "$dir") failed pages: |"
    if [ "$result" -ne 0 ]; then
        echo "[snapshot] $(basename "$dir") exited $result (see $dir.log)"
    fi
    return "$result"
}

echo "[visual] capturing baseline and candidate twice each"
snapshot "$BASE" "$OUT/before-a" "$PAGES" || status=1
snapshot "$CAND" "$OUT/after-a" "$PAGES" ${candidate_snapshot_args[@]+"${candidate_snapshot_args[@]}"} || status=1
snapshot "$BASE" "$OUT/before-b" "$PAGES" || status=1
snapshot "$CAND" "$OUT/after-b" "$PAGES" ${candidate_snapshot_args[@]+"${candidate_snapshot_args[@]}"} || status=1
python3 "$HERE/compare_snapshots.py" "$OUT/before-a" "$OUT/before-b" "$OUT/after-a" "$OUT/after-b" \
    --diff-dir "$OUT/diff" ${SNAPSHOT_COMPARE_ARGS:-} > "$OUT/visual.txt" 2>&1
visual=$?
if [ "$visual" -ne 0 ] && [ "${REPEAT_UNSTABLE_PAGES:-}" = "1" ]; then
    unstable_pages=$(awk '$3 > 0 && /VISUAL CHANGE/ {print $1}' "$OUT/visual.txt" | paste -sd , -)
    if [ -n "$unstable_pages" ]; then
        echo "[visual] repeating pages with observed motion: $unstable_pages"
        mv "$OUT/visual.txt" "$OUT/visual-initial.txt"
        for repeat in c d; do
            snapshot "$BASE" "$OUT/before-$repeat" "$unstable_pages" || status=1
            snapshot "$CAND" "$OUT/after-$repeat" "$unstable_pages" ${candidate_snapshot_args[@]+"${candidate_snapshot_args[@]}"} || status=1
        done
        python3 "$HERE/compare_snapshots.py" "$OUT/before-a" "$OUT/before-b" "$OUT/after-a" "$OUT/after-b" \
            --before-repeat "$OUT/before-c" --before-repeat "$OUT/before-d" \
            --after-repeat "$OUT/after-c" --after-repeat "$OUT/after-d" --repeat-pages "$unstable_pages" \
            --diff-dir "$OUT/diff" ${SNAPSHOT_COMPARE_ARGS:-} > "$OUT/visual.txt" 2>&1
        visual=$?
    fi
fi
grep -E "VISUAL CHANGE|LAYOUT CHANGED|MISSING|^pages:" "$OUT/visual.txt"
[ $visual -ne 0 ] && status=1

echo "[visual] control states (every type: enums, booleans, disabled, pseudo-classes)"
for side in before-a:$BASE after-a:$CAND before-b:$BASE after-b:$CAND; do
    name="${side%%:*}"; bin="${side#*:}"
    rm -rf "$OUT/states-$name"
    dotnet "$bin/AtomUI.ControlBaseline.dll" --mode atomui --state-snapshots "$OUT/states-$name" \
        ${VARIANT_ARGS[@]+"${VARIANT_ARGS[@]}"} ${SNAPSHOT_TOOL_ARGS:-} > "$OUT/states-$name.log" 2>&1 || status=1
    tail -1 "$OUT/states-$name.log" | sed "s|^|[states] $name: |"
done
python3 "$HERE/compare_snapshots.py" "$OUT/states-before-a" "$OUT/states-before-b" "$OUT/states-after-a" "$OUT/states-after-b" \
    --diff-dir "$OUT/states-diff" ${SNAPSHOT_COMPARE_ARGS:-} > "$OUT/states.txt" 2>&1
states=$?
grep -E "VISUAL CHANGE|LAYOUT CHANGED|MISSING|^pages:" "$OUT/states.txt" | sed 's/^pages:/control types:/'
[ $states -ne 0 ] && status=1

echo "[leak] Gallery pages"
for side in before:$BASE after:$CAND; do
    name="${side%%:*}"; bin="${side#*:}"
    dotnet "$bin/AtomUI.GalleryPerformance.dll" --audit --audit-loose-ready --audit-showcases "$PAGES" \
        --timeout-ms 40000 --leak-check 3 ${VARIANT_ARGS[@]+"${VARIANT_ARGS[@]}"} > "$OUT/leak-pages-$name.txt" 2>&1 || status=1
    tail -1 "$OUT/leak-pages-$name.txt" | sed "s|^|[leak] $name: |"
done

echo "[leak] control types"
for side in before:$BASE after:$CAND; do
    name="${side%%:*}"; bin="${side#*:}"
    dotnet "$bin/AtomUI.ControlBaseline.dll" --mode atomui --leak --count 20 --json "$OUT/leak-controls-$name.json" \
        ${VARIANT_ARGS[@]+"${VARIANT_ARGS[@]}"} > "$OUT/leak-controls-$name.txt" 2>&1 || status=1
    tail -1 "$OUT/leak-controls-$name.txt" | sed "s|^|[leak] $name: |"
done

coverage_args=()
if [ "${EXPECTED_STATE_CAPTURE_FAILURES+x}" ]; then
    coverage_args=(--expected-state-failures "$EXPECTED_STATE_CAPTURE_FAILURES")
fi
python3 "$HERE/verify_guard_coverage.py" "$OUT" ${coverage_args[@]+"${coverage_args[@]}"} || status=1

echo "[result] $([ $status -eq 0 ] && echo PASS || echo FAIL) (details in $OUT)"
exit $status
