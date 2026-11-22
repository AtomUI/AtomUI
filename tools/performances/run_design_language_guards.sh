#!/usr/bin/env bash
#
# Design-language guard: the visual and leak guards of run_perf_guards.sh under every design-language variant, plus a
# key-by-key comparison of the compiled token values of every variant and seed case, baseline build against candidate.
#
# usage: tools/performances/run_design_language_guards.sh BASELINE_BIN CANDIDATE_BIN OUTPUT_DIR [SHOWCASES] [VARIANT_FILTER]
#
#   BASELINE_BIN    Release output folder of the baseline build (contains AtomUI.GalleryPerformance.dll and
#                   AtomUI.ControlBaseline.dll). It must support --freeze-loading-icons and --dump-tokens.
#   CANDIDATE_BIN   Release output folder of the candidate build.
#   OUTPUT_DIR      Results: <OUTPUT_DIR>/<variant key>/ per variant, <OUTPUT_DIR>/tokens/, summary.txt.
#   SHOWCASES       Comma-separated Gallery keys, default all.
#   VARIANT_FILTER  Extended regular expression matched against the lines of --list-variants
#                   ("<design language> <appearance> <density>"), default every variant.
#
# Environment:
#   ALLOW_ADDED_TOKENS=1  keys present only in the candidate's token dumps are listed but do not fail.
#
# Captures freeze loading-icon rotations (--freeze-loading-icons) and do not fail on changed pixels that are all next
# to other animations (--ignore-near-animation). Token renames and exceptions are read from
# design_language_token_renames.txt and design_language_token_exceptions.txt next to this script when they exist.
#
# Exit code is non-zero when any variant's guard or the token comparison fails.
set -uo pipefail

BASE="${1:?baseline bin folder}"
CAND="${2:?candidate bin folder}"
OUT="${3:?output folder}"
PAGES="${4:-all}"
FILTER="${5:-.}"
HERE="$(cd "$(dirname "$0")" && pwd)"

mkdir -p "$OUT"
SUMMARY="$OUT/summary.txt"
: > "$SUMMARY"
status=0

export SNAPSHOT_TOOL_ARGS="--freeze-loading-icons"
export SNAPSHOT_COMPARE_ARGS="--ignore-near-animation"

report() {
    echo "$1" | tee -a "$SUMMARY"
}

variants="$(dotnet "$CAND/AtomUI.GalleryPerformance.dll" --list-variants | grep -E -- "$FILTER")"
if [ -z "$variants" ]; then
    echo "no variant matches '$FILTER'" >&2
    exit 2
fi

while IFS= read -r line; do
    read -r language appearance density <<< "$line"
    key="$language.$appearance.$density"
    started=$(date +%s)
    "$HERE/run_perf_guards.sh" "$BASE" "$CAND" "$OUT/$key" "$PAGES" "$language" "$appearance" "$density" \
        > "$OUT/$key.log" 2>&1 < /dev/null
    code=$?
    elapsed=$(( $(date +%s) - started ))
    if [ $code -eq 0 ]; then
        report "[variant] $key: PASS (${elapsed}s)"
    else
        status=1
        report "[variant] $key: FAIL (${elapsed}s, details in $OUT/$key.log)"
    fi
done <<< "$variants"

echo "[tokens] dumping baseline and candidate"
rm -rf "$OUT/tokens"
mkdir -p "$OUT/tokens"
started=$(date +%s)
for side in baseline:$BASE candidate:$CAND; do
    name="${side%%:*}"; bin="${side#*:}"
    if ! dotnet "$bin/AtomUI.GalleryPerformance.dll" --dump-tokens "$OUT/tokens/$name" \
            > "$OUT/tokens/$name.log" 2>&1 < /dev/null; then
        status=1
        report "[tokens] $name dump failed (details in $OUT/tokens/$name.log)"
    fi
done
compare_args=()
[ -f "$HERE/design_language_token_renames.txt" ] && compare_args+=(--renames "$HERE/design_language_token_renames.txt")
[ -f "$HERE/design_language_token_exceptions.txt" ] && compare_args+=(--exceptions "$HERE/design_language_token_exceptions.txt")
[ "${ALLOW_ADDED_TOKENS:-}" = "1" ] && compare_args+=(--allow-added)
python3 "$HERE/compare_token_dumps.py" "$OUT/tokens/baseline" "$OUT/tokens/candidate" \
    ${compare_args[@]+"${compare_args[@]}"} > "$OUT/tokens.txt"
code=$?
elapsed=$(( $(date +%s) - started ))
if [ $code -eq 0 ]; then
    report "[tokens] PASS (${elapsed}s): $(tail -1 "$OUT/tokens.txt")"
else
    status=1
    report "[tokens] FAIL (${elapsed}s, details in $OUT/tokens.txt): $(tail -1 "$OUT/tokens.txt")"
fi

report "[result] $([ $status -eq 0 ] && echo PASS || echo FAIL) (details in $OUT)"
exit $status
