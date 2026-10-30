#!/usr/bin/env python3
"""AtomUI's default change verification entry point. Run --help for scope rules."""
import argparse
import json
import subprocess
import sys
import time
from pathlib import Path

sys.dont_write_bytecode = True

from affected.planner import Planner
from affected.repository import Repository
from affected.runner import run_verification, run_specialist_check


def print_plan(plan):
    count = sum(len(j["classes"]) for j in plan["tests"])
    print("Scope: " + plan["scope"] + (" (preview with supplied paths)" if plan.get("preview") else ""))
    if plan.get("focused"):
        print("FOCUSED BUG-FIX ITERATION: validates only the supplied module paths; it does not certify the working tree.")
    print(str(len(plan["changes"])) + " changed inputs; " + str(len(plan["tests"])) + "/" + str(plan["inventory"]["projects"]) + " test projects; " + str(count) + " test classes")
    for job in plan["tests"]:
        print("  " + job["project"] + ": " + str(len(job["classes"])) + " classes")
    if plan["tooling"]:
        print("  Verification tooling self-tests")
    if plan.get("staged_differs_from_worktree"):
        print("NOTE: Staged and working versions differ for " + str(len(plan["staged_differs_from_worktree"])) + " files; this verifies the working tree, not the index snapshot.")
    for warning in plan["warnings"]:
        print("NOTE: " + warning)
    for gap in plan["gaps"]:
        print("BLOCKED: " + gap)
    for check in plan["obligations"]:
        print("REQUIRED " + check["id"] + ": " + check["description"])
    if not plan["changes"] and plan["scope"] != "full":
        print("No local changes relative to HEAD. Use --base REF to include committed branch changes.")
    print("Full regression is not part of the change gate: run scripts/run-full-regression.sh only on explicit request or during a release.")


def parser():
    result = argparse.ArgumentParser(description=__doc__)
    result.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2], help="Repository root (default: this checkout)")
    sub = result.add_subparsers(dest="command", required=True)
    for name in ("plan", "run"):
        command = sub.add_parser(name, help="Explain selection" if name == "plan" else "Build once, run selected tests, and report evidence")
        command.add_argument("--base", help="Compare the merge-base with REF plus all working-tree/untracked changes; default HEAD")
        command.add_argument("--scope", choices=("iterate", "change", "full"), default="change")
        command.add_argument("--json", action="store_true", help="Print structured output")
        command.add_argument("--include-test", action="append", default=[], help="Add a test source glob; never removes automatically selected tests")
        command.add_argument("--path", action="append", help=(
            "Preview specified paths without running tests (repeatable)" if name == "plan" else
            "Bug-fix module path to verify; repeatable and valid only with --scope iterate"))
        if name == "run":
            command.add_argument("--fresh", action="store_true", help="Do not reuse a passing receipt")
            command.add_argument("--tests-only", action="store_true", help="Report only the test phase; preserve outstanding specialist obligations (CI test job)")
            command.add_argument("--timeout", type=int, default=1800, help="Seconds per build/test command")
            command.add_argument("--configuration", choices=("Debug", "Release"), default="Debug")
    audit = sub.add_parser("audit", help="Check source ownership, suite mappings and all discovered test projects")
    audit.add_argument("--json", action="store_true")
    sub.add_parser("self-test", help="Run only the verification tool's regression tests")
    sub.add_parser("metrics", help="Show the last 100 compact verification timing records")
    check = sub.add_parser("check", help="Execute a specialist verifier and attach its evidence to current inputs")
    check.add_argument("--id", required=True, help="Obligation ID from the plan")
    check.add_argument("--timeout", type=int, default=3600)
    check.add_argument("argv", nargs=argparse.REMAINDER, help="-- executable argument ... (a script must verify all steps of the obligation)")
    return result


def validate_run_arguments(args):
    if args.command != "run":
        return
    paths = args.path or []
    if paths and args.scope != "iterate":
        raise ValueError("--path is only valid with --scope iterate; focused paths cannot certify change or full coverage")
    if args.scope == "iterate":
        if not paths:
            raise ValueError("Bug-fix iteration requires at least one --path for the affected module")
        if args.base:
            raise ValueError("--base cannot be combined with focused --scope iterate --path verification")
    elif args.scope == "change" and not args.base and not args.tests_only:
        raise ValueError("Local bug fixing must use --scope iterate --path; branch/CI change verification requires --base")
    elif args.scope == "full" and not args.tests_only:
        raise ValueError("Final full regression must use scripts/run-full-regression.sh")


def verification_changes(args, repository):
    return repository.focus_paths(args.path) if args.path else repository.changes(args.base)


def main(argv=None):
    args = parser().parse_args(argv)
    validate_run_arguments(args)
    started = time.monotonic()
    root = args.root.resolve()
    output = root / ".artifacts/verification"
    if args.command == "self-test":
        return subprocess.call([sys.executable, "-B", "-m", "unittest", "discover", "-s", str(root / "scripts/verification/tests"), "-v"], cwd=root)
    if args.command == "metrics":
        history = output / "history.json"
        print(history.read_text() if history.exists() else "No verification history yet.")
        return 0
    repository = Repository(root)
    policy = json.loads((root / "scripts/verification/test-policy.json").read_text())
    if args.command == "check":
        candidates = [o for o in policy["obligations"] if o["id"] == args.id]
        if not candidates:
            raise ValueError("Unknown specialist check ID: " + args.id)
        check = {k: v for k, v in candidates[0].items() if k != "changes"}
        print(check["description"], file=sys.stderr)
        argv = args.argv[1:] if args.argv and args.argv[0] == "--" else args.argv
        result = run_specialist_check(repository, check, argv, output, timeout=args.timeout,
                                      announce=lambda message: print(message, file=sys.stderr, flush=True))
        print(json.dumps(result, indent=2))
        return 0 if result["status"] == "passed" else 1
    planner = Planner(repository, policy)
    audit = planner.audit()
    if args.command == "audit":
        print(json.dumps(audit, indent=2) if args.json else "Projects: {projects}; test files: {test_files}; classes: {classes}; source groups: {source_groups}".format(**audit))
        if not args.json:
            for error in audit["errors"]:
                print("BLOCKED: " + error)
        return 2 if audit["errors"] else 0
    preview = args.command == "plan" and bool(args.path)
    focused = args.command == "run" and bool(args.path)
    base = repository.git("merge-base", "HEAD", args.base).strip() if args.base else "HEAD"
    plan = planner.plan(verification_changes(args, repository), scope=args.scope, base=base, extra_tests=args.include_test)
    plan["preview"] = preview
    plan["focused"] = focused
    if focused:
        plan["validates"] = "selected-paths"
    plan["base"] = repository.git("rev-parse", base).strip()
    plan["gaps"] = sorted(set(plan["gaps"] + audit["errors"]))
    if args.command == "plan":
        print(json.dumps(plan, indent=2) if args.json else "", end="" if args.json else "")
        if not args.json:
            print_plan(plan)
        return 2 if plan["gaps"] else 0
    if not args.json:
        print_plan(plan)
    report = run_verification(repository, plan, output, reuse=not args.fresh, configuration=args.configuration,
                              timeout=args.timeout, tests_only=args.tests_only, preparation_seconds=time.monotonic() - started,
                              announce=lambda message: print(message, file=sys.stderr, flush=True))
    if args.json:
        print(json.dumps(report, indent=2))
    else:
        print("Verification: " + report["status"] + " (" + str(report["timings"]["total"]) + "s)")
        for error in report["errors"]:
            print(error, file=sys.stderr)
        print("Report: " + str(output / "latest.json"))
    return 0 if report["status"] in ("passed", "reused", "iteration-passed", "tests-passed", "no-changes") else 2 if report["status"] in ("blocked", "pending") else 1


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        print("Verification error: " + str(error), file=sys.stderr)
        sys.exit(2)
