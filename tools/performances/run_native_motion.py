#!/usr/bin/env python3
"""Run native Gallery motion samples in independent processes; no third-party modules."""
import argparse
import json
import math
from pathlib import Path
import statistics
import subprocess


def distribution(values):
    ordered = sorted(values)
    if not ordered:
        return {"count": 0, "median": None, "p95": None, "p99": None, "max": None}
    return {"count": len(ordered), "mean": statistics.mean(ordered),
            "median": statistics.median(ordered),
            "p95": ordered[math.ceil(len(ordered) * .95) - 1],
            "p99": ordered[math.ceil(len(ordered) * .99) - 1], "max": ordered[-1]}


def summarize(reports):
    first = reports[0]
    for report in reports:
        for key in ("Schema", "Scenario", "Warmup", "Iterations", "DeclaredRefreshRateHz",
                    "ControlsAssemblySha256", "ProductDiffSha256", "Configuration", "AvaloniaVersion",
                    "RenderScaling", "ClientSize", "ThemeVariant", "Platform", "Host", "Architecture"):
            if report[key] != first[key]:
                raise ValueError(f"Mismatched sample identity/configuration: {key}")
        if not report["Samples"]:
            raise ValueError("Native process produced no measured actions")
    samples = [sample for report in reports for sample in report["Samples"]]
    result = {key: first[key] for key in ("Schema", "Label", "Scenario", "Warmup", "Iterations",
              "DeclaredRefreshRateHz", "ControlsAssemblySha256", "ProductDiffSha256", "Platform",
              "Configuration", "AvaloniaVersion", "Presentation", "Instrumentation", "RenderScaling", "ClientSize", "ThemeVariant", "Host", "Architecture")}
    result["processes"] = len(reports)
    result["cases"] = {}
    for scenario, direction in sorted({(s["Scenario"], s["Direction"]) for s in samples}):
        group = [s for s in samples if (s["Scenario"], s["Direction"]) == (scenario, direction)]
        identities = []
        for report in reports:
            scene = next(s for s in report["Scenes"] if s["Scenario"] == scenario)
            report_samples = [s for s in report["Samples"] if (s["Scenario"], s["Direction"]) == (scenario, direction)]
            identities.append({"root": scene["Root"], "property": scene["PropertyName"],
                "target_types": sorted({s["TargetType"] for s in report_samples}),
                "animation_contracts": sorted({s["AnimationContract"] for s in report_samples})})
        if any(identity != identities[0] for identity in identities):
            raise ValueError(f"Scenario identity differs across processes: {scenario}/{direction}")
        if scenario in ("sidebar", "submenu", "progress", "statistic"):
            for sample in group:
                values = sample["Values"]
                numbers = [v["Value"] for v in values]
                if (len(values) < 3 or values[-1]["Time"] - values[0]["Time"] < 1 or len(sample["Rendered"]) < 2
                        or not any(min(numbers) < v < max(numbers) for v in numbers)):
                    raise ValueError(f"No intermediate animation values: {scenario}/{direction}")
        case = {"actions": len(group), "sample_ms": sorted({s["SampleMilliseconds"] for s in group}),
                "final_values": sorted({s["Final"] for s in group}), "identity": identities[0]}
        if any(s["MissedBatches"] for s in group):
            raise ValueError("Missing compositor observations")
        for channel in ("Values", "Bounds", "Layouts", "Commits", "Processed", "Rendered"):
            intervals = []
            simultaneous = 0
            for sample in group:
                times = sorted(v["Time"] if isinstance(v, dict) else v for v in sample[channel])
                for a, b in zip(times, times[1:]):
                    if b - a <= .1:
                        simultaneous += 1
                    else:
                        intervals.append(b - a)
            stats = distribution(intervals)
            stats["simultaneous_notifications"] = simultaneous
            if first["DeclaredRefreshRateHz"]:
                budget = 1000 / first["DeclaredRefreshRateHz"]
                stats["over_1_5_budget"] = sum(v > 1.5 * budget for v in intervals)
                stats["over_2_budget"] = sum(v > 2 * budget for v in intervals)
            case[channel + "_interval_ms"] = stats
        for metric in ("AllocatedBytes", "UiAllocatedBytes", "CpuMilliseconds", "ElapsedMilliseconds"):
            case[metric + "_per_action"] = distribution([s[metric] for s in group])
        case["layout_pass_ms"] = distribution([t for s in group for t in s["LayoutMilliseconds"]])
        case["layout_passes_per_action"] = distribution([len(s["LayoutMilliseconds"]) for s in group])
        case["gc_collections"] = [sum(s["Collections"][i] for s in group) for i in range(3)]
        result["cases"][scenario + "/" + direction] = case
    return result


def compare(before, current):
    for key in ("Schema", "Scenario", "Warmup", "Iterations", "DeclaredRefreshRateHz", "processes", "Configuration",
                "AvaloniaVersion", "RenderScaling", "ClientSize", "ThemeVariant", "Platform", "Host", "Architecture"):
        if before[key] != current[key]:
            raise ValueError(f"Comparison settings differ: {key}")
    if before["cases"].keys() != current["cases"].keys():
        raise ValueError("Comparison case sets differ")
    comparison = {}
    for key, case in current["cases"].items():
        old_case = before["cases"][key]
        if case["sample_ms"] != old_case["sample_ms"] or case["identity"] != old_case["identity"]:
            raise ValueError(f"Comparison workload differs: {key}")
        comparison[key] = {}
        for metric in ("Values_interval_ms", "Rendered_interval_ms", "CpuMilliseconds_per_action",
                       "UiAllocatedBytes_per_action", "layout_passes_per_action"):
            old = old_case[metric]["median"]
            new = case[metric]["median"]
            comparison[key][metric] = {"baseline": old, "current": new,
                "improvement_percent": (old - new) / old * 100 if old and new is not None else None}
    return comparison


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--binary", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--scenario", default="sidebar")
    parser.add_argument("--label", default="current")
    parser.add_argument("--processes", type=int, default=3)
    parser.add_argument("--iterations", type=int, default=30)
    parser.add_argument("--warmup", type=int, default=5)
    parser.add_argument("--sample-ms", type=int, default=1000)
    parser.add_argument("--refresh-rate", type=float)
    parser.add_argument("--compare", type=Path, help="Previous summary.json; settings and case sets must match")
    args = parser.parse_args()
    if args.processes < 1:
        parser.error("processes must be positive")
    args.output_dir.mkdir(parents=True, exist_ok=True)
    reports = []
    for iteration in range(args.processes):
        output = args.output_dir / f"process-{iteration + 1}.json"
        log = args.output_dir / f"process-{iteration + 1}.log"
        if output.exists() or log.exists():
            raise ValueError(f"Refusing to overwrite an existing run: {output}")
        command = [str(args.binary.resolve()), "--native-motion", "--scenario", args.scenario,
                   "--label", args.label, "--iterations", str(args.iterations), "--warmup", str(args.warmup),
                   "--sample-ms", str(args.sample_ms), "--output", str(output.resolve())]
        if args.refresh_rate is not None:
            command += ["--refresh-rate", str(args.refresh_rate)]
        print(f"Native motion {args.label}: process {iteration + 1}/{args.processes}", flush=True)
        with log.open("w") as stream:
            with subprocess.Popen(command, stdout=stream, stderr=subprocess.STDOUT) as process:
                try:
                    code = process.wait(timeout=1800)
                    if code:
                        raise subprocess.CalledProcessError(code, command)
                except BaseException:
                    process.terminate()
                    try:
                        process.wait(timeout=10)
                    except subprocess.TimeoutExpired:
                        process.kill()
                        process.wait()
                    raise
        reports.append(json.loads(output.read_text()))
    summary = summarize(reports)
    (args.output_dir / "summary.json").write_text(json.dumps(summary, indent=2) + "\n")
    if args.compare:
        before = json.loads(args.compare.read_text())
        comparison = compare(before, summary)
        (args.output_dir / "comparison.json").write_text(json.dumps(comparison, indent=2) + "\n")
    print(f"Native motion complete: {args.output_dir / 'summary.json'}", flush=True)


if __name__ == "__main__":
    main()
