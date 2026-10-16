#!/usr/bin/env python3
"""AtomUI 启动采样：按变体交错运行独立进程，解析 PERF|key=value，输出原始样本、汇总与配对比较。

口径：每个变体至少 10 轮（少于 10 轮必须加 --smoke 并在报告中标注）；奇数轮按配置顺序、偶数轮倒序；
预热样本不计入；全部样本保留，不剔除离群值。Launch.* 指标由 Epoch.MainUnixMicroseconds 与启动时刻换算。
"""
import argparse
import hashlib
import json
import os
import platform
import statistics
import subprocess
import sys
import time

PERF_PREFIX = "PERF|"
LAUNCH_KEYS = (
    "Mark.WindowOpened", "Mark.FirstRender", "Mark.FirstRenderedBatch", "Mark.DispatcherIdle",
    "Mark.FirstUiPump", "Mark.MainWindowOpened", "Mark.MainWindowFirstBatch", "Mark.ShellFirstBatch", "Mark.Idle",
    "Mark.SplashFirstFrame", "Mark.MainWindowFirstFrameEvent",
)


def parse_perf(stdout):
    values = {}
    for line in stdout.splitlines():
        if not line.startswith(PERF_PREFIX):
            continue
        key, _, raw = line[len(PERF_PREFIX):].partition("=")
        try:
            values[key.strip()] = float(raw)
        except ValueError:
            continue
    return values


def read_load():
    return os.getloadavg()[0] if hasattr(os, "getloadavg") else None


def sha256(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest()


def _text(value):
    if value is None:
        return ""
    return value.decode("utf-8", "replace") if isinstance(value, bytes) else value


def run_once(variant, timeout):
    env = os.environ.copy()
    env.update(variant.get("env", {}))
    load_before = read_load()
    launch_us = time.time_ns() // 1000
    try:
        completed = subprocess.run(variant["command"], cwd=variant.get("cwd"), env=env,
                                   capture_output=True, text=True, timeout=timeout)
    except subprocess.TimeoutExpired as error:
        return {"status": "timeout", "launchUnixMicroseconds": launch_us, "loadBefore": load_before,
                "values": {}, "stdoutTail": _text(error.stdout)[-2000:]}
    values = parse_perf(completed.stdout)
    sample = {"status": "ok" if completed.returncode == 0 else f"exit:{completed.returncode}",
              "launchUnixMicroseconds": launch_us, "loadBefore": load_before, "values": values,
              "stderrTail": completed.stderr[-2000:]}
    if sample["status"] != "ok":
        return sample
    main_epoch = values.get("Epoch.MainUnixMicroseconds")
    if main_epoch is None:
        sample["status"] = "invalid:no-epoch"
        return sample
    launch_to_main = (main_epoch - launch_us) / 1000.0
    values["Launch.ToMain"] = launch_to_main
    if not 0.0 < launch_to_main < 2000.0:
        sample["status"] = "invalid:launch-offset"
        return sample
    for key in LAUNCH_KEYS:
        if key in values:
            values["Launch.To" + key.split(".", 1)[1]] = launch_to_main + values[key]
    for state in ("Committed", "Presenting", "Ready"):
        key = "Mark.State." + state
        if key in values:
            values["Launch.To" + state] = launch_to_main + values[key]
    missing = [key for key in variant.get("required_metrics", []) if key not in values]
    if missing:
        sample["status"] = "invalid:missing-metrics:" + ",".join(missing)
    return sample


def describe(values):
    return {"n": len(values), "median": statistics.median(values), "mean": statistics.fmean(values),
            "min": min(values), "max": max(values)}


def summarize(samples, names, compare, hashes):
    ok = [s for s in samples if s["status"] == "ok"]
    variants = {}
    for name in names:
        rows = [s["values"] for s in ok if s["variant"] == name]
        keys = sorted({k for row in rows for k in row if k != "Epoch.MainUnixMicroseconds"})
        variants[name] = {k: describe([row[k] for row in rows if k in row]) for k in keys}
    failures = {name: sum(1 for s in samples if s["variant"] == name and s["status"] != "ok") for name in names}
    cpu = os.cpu_count() or 1
    high_load = [{"variant": s["variant"], "round": s["round"], "load": s["loadBefore"]}
                 for s in samples if s["loadBefore"] is not None and s["loadBefore"] > cpu * 0.5]
    warnings = []
    by_file = {}
    for name, files in hashes.items():
        for path, digest in files.items():
            by_file.setdefault((os.path.basename(path), digest), []).append(name)
    for (basename, _), owners in by_file.items():
        if len(owners) > 1:
            warnings.append(f"{basename} 在变体 {', '.join(owners)} 中哈希相同，确认是否用错了二进制")
    pairs = {}
    if compare:
        base, cand = compare
        rounds = sorted({s["round"] for s in ok})
        for key in sorted({k for name in (base, cand) for k in variants.get(name, {})}):
            deltas = []
            for r in rounds:
                b = next((s["values"].get(key) for s in ok if s["variant"] == base and s["round"] == r), None)
                c = next((s["values"].get(key) for s in ok if s["variant"] == cand and s["round"] == r), None)
                if b is not None and c is not None:
                    deltas.append(c - b)
            if deltas:
                pairs[key] = {"pairs": len(deltas), "medianDelta": statistics.median(deltas),
                              "candidateFasterRounds": sum(1 for d in deltas if d < 0)}
    return {"variants": variants, "failures": failures, "highLoadSamples": high_load,
            "warnings": warnings, "compare": pairs}


def write_markdown(path, config, summary, args, hashes):
    lines = [f"# {config.get('label', 'startup samples')}", "",
             f"- 机器：{platform.platform()}；CPU 核数：{os.cpu_count()}",
             f"- 轮数：{args.rounds}（{'smoke' if args.smoke else '正式'}），预热：{args.warmup}，超时：{args.timeout}s",
             f"- 上下文：{json.dumps(config.get('context', {}), ensure_ascii=False)}",
             f"- 必需指标：{json.dumps(summary['requiredMetrics'], ensure_ascii=False)}", ""]
    for name, files in hashes.items():
        for file, digest in files.items():
            lines.append(f"- `{name}` {os.path.basename(file)} sha256 `{digest}`")
    lines += ["", "| 变体 | 指标 | n | 中位数 ms | 均值 | 最小 | 最大 |", "|---|---|---:|---:|---:|---:|---:|"]
    for name, metrics in summary["variants"].items():
        for key, s in metrics.items():
            lines.append(f"| {name} | {key} | {s['n']} | {s['median']:.1f} | {s['mean']:.1f} | {s['min']:.1f} | {s['max']:.1f} |")
    if summary["compare"]:
        lines += ["", "| 指标 | 配对数 | 中位差 ms（候选−基线） | 候选更快的轮数 |", "|---|---:|---:|---:|"]
        for key, s in summary["compare"].items():
            lines.append(f"| {key} | {s['pairs']} | {s['medianDelta']:.1f} | {s['candidateFasterRounds']} |")
    lines += ["", f"- 失败样本：{json.dumps(summary['failures'], ensure_ascii=False)}",
              f"- 高负载样本：{json.dumps(summary['highLoadSamples'], ensure_ascii=False)}"]
    lines += [f"- 告警：{w}" for w in summary["warnings"]]
    with open(path, "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines) + "\n")


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--config", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--rounds", type=int, default=10)
    parser.add_argument("--warmup", type=int, default=1)
    parser.add_argument("--timeout", type=float, default=60.0)
    parser.add_argument("--smoke", action="store_true", help="允许少于 10 轮，结果只能作为 smoke")
    args = parser.parse_args(argv)
    if args.rounds < 10 and not args.smoke:
        parser.error("正式采样至少 10 轮；少于 10 轮必须加 --smoke")
    with open(args.config, encoding="utf-8") as handle:
        config = json.load(handle)
    variants = config["variants"]
    names = [v["name"] for v in variants]
    if len(set(names)) != len(names):
        parser.error("变体名重复")
    for variant in variants:
        required = variant.get("required_metrics", [])
        if (not isinstance(required, list) or
                any(not isinstance(key, str) or not key.strip() for key in required) or
                len(set(required)) != len(required)):
            parser.error(f"变体 {variant['name']} 的 required_metrics 必须是不重复的非空指标名数组")
        if not args.smoke and not required:
            parser.error(f"正式采样变体 {variant['name']} 必须声明 required_metrics")
    hashes = {v["name"]: {p: sha256(p) for p in v.get("hash_paths", [])} for v in variants}
    os.makedirs(args.output, exist_ok=True)
    for v in variants:
        for _ in range(args.warmup):
            run_once(v, args.timeout)
    samples = []
    for round_index in range(1, args.rounds + 1):
        order = variants if round_index % 2 == 1 else list(reversed(variants))
        for v in order:
            sample = run_once(v, args.timeout)
            sample["variant"] = v["name"]
            sample["round"] = round_index
            samples.append(sample)
            print(f"[{round_index}/{args.rounds}] {v['name']}: {sample['status']}", file=sys.stderr)
    summary = summarize(samples, names, config.get("compare"), hashes)
    summary["hashes"] = hashes
    summary["requiredMetrics"] = {v["name"]: v.get("required_metrics", []) for v in variants}
    with open(os.path.join(args.output, "samples.json"), "w", encoding="utf-8") as handle:
        json.dump(samples, handle, ensure_ascii=False, indent=1)
    with open(os.path.join(args.output, "summary.json"), "w", encoding="utf-8") as handle:
        json.dump(summary, handle, ensure_ascii=False, indent=1)
    write_markdown(os.path.join(args.output, "summary.md"), config, summary, args, hashes)
    return 1 if any(s["status"] != "ok" for s in samples) else 0


if __name__ == "__main__":
    sys.exit(main())
