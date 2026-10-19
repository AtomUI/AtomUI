"""Explainable ownership + source consumer expansion + mandatory policy rules."""
import re
from collections import defaultdict, deque
from pathlib import Path
from functools import lru_cache

from .repository import matches, symbols, scrub_csharp
from .test_value_audit import audit_low_value_permanent_tests


class Planner:
    def __init__(self, repository, policy):
        self.repo = repository
        self.policy = policy
        self.source_fingerprint = repository.discovery_snapshot
        if repository.snapshot() != self.source_fingerprint:
            raise ValueError("Repository discovery is stale; rebuild the project/test inventory")
        if policy.get("version") != 1:
            raise ValueError("Unsupported test policy version")
        self.groups = {}
        self.file_groups = {}
        self.file_symbols = {}
        self.file_tokens = {}
        self.derivatives = defaultdict(dict)
        self.tokens = defaultdict(set)
        self.test_tokens = defaultdict(set)
        self.file_readers = defaultdict(set)
        self.dependencies = {}
        for project in repository.projects:
            seen, queue = set(), [project]
            while queue:
                current = queue.pop()
                if current not in seen:
                    seen.add(current)
                    queue.extend(repository.projects.get(current, {}).get("references", []))
            self.dependencies[project] = seen
        for path in repository.files:
            area, root = self.area(path)
            if area:
                relative = path[len(root) + 1:]
                parts = relative.split("/")[:-1][:area.get("depth", 0)]
                key = area["id"] + ("/" + "/".join(parts) if parts else "/_root")
                group = self.groups.setdefault(key, {"area": area, "paths": [], "symbols": set(), "derived": defaultdict(set), "projects": set()})
                group["paths"].append(path)
                project = repository.owner(path)
                if project:
                    group["projects"].add(project)
                if Path(path).suffix in (".cs", ".axaml"):
                    text = repository.text(path)
                    self.file_symbols[path] = symbols(text)
                    self.file_tokens[path] = set(re.findall(r"\b\w+\b", scrub_csharp(text) if path.endswith(".cs") else text))
                    group["symbols"].update(self.file_symbols[path])
                    if path.endswith(".cs"):
                        for declared, bases in re.findall(r"\b(?:class|struct|interface|record(?:\s+(?:class|struct))?)\s+(\w+)(?:<[^{}]+?>)?(?:\([^{}]*?\))?\s*:\s*([^{};]+)", scrub_csharp(text)):
                            for base in re.findall(r"\b\w+\b", bases):
                                group["derived"][base].add(declared)
                self.file_groups[path] = key
        for path, tokens in self.file_tokens.items():
            for token in tokens:
                self.tokens[token].add(path)
        for key, group in self.groups.items():
            for base, derived in group["derived"].items():
                self.derivatives[base][key] = derived
        for path in repository.tests:
            source = repository.text(path)
            for token in set(re.findall(r"\b\w+\b", source)):
                self.test_tokens[token].add(path)
            # File-reading tests are independent of runtime ProjectReference edges.
            for literal in re.findall(r'"([^"\r\n]+)"', source):
                name = literal.replace("\\", "/").rsplit("/", 1)[-1]
                if re.fullmatch(r"[\w. -]+\.(?:cs|axaml|csproj|slnx|props|targets|md|json|yml|yaml|ps1|sh|xlf|nuspec|snapshot|baseline|svg)", name):
                    self.file_readers[name].add(path)
        if repository.snapshot() != self.source_fingerprint:
            raise ValueError("Repository changed during impact discovery; run again for a consistent plan")

    def area(self, path):
        choices = [(area, root) for area in self.policy.get("areas", []) for root in area["roots"] if path.startswith(root + "/")]
        return max(choices, key=lambda x: len(x[1])) if choices else (None, None)

    @lru_cache(maxsize=None)
    def existed_at_base(self, path, base="HEAD"):
        try:
            self.repo.git("cat-file", "-e", base + ":" + path)
            return True
        except ValueError:
            return False

    @lru_cache(maxsize=None)
    def suite_files(self, pattern):
        return [p for p in self.repo.tests if matches(p, pattern)]

    def audit(self):
        errors = audit_low_value_permanent_tests(self.repo)
        for owner in self.policy.get("areas", []) + self.policy.get("rules", []):
            for pattern in owner.get("tests", []):
                if not self.suite_files(pattern):
                    errors.append(owner["id"] + ": test pattern selects no test classes: " + pattern)
        for path in self.repo.files:
            if path.startswith(("src/", "controlgallery/", "tools/")):
                mapped = self.area(path)[0] or any(matches(path, g) for r in self.policy.get("rules", []) for g in r["changes"])
                ignored = any(matches(path, g) for g in self.policy.get("ignore", []))
                if not mapped and not ignored:
                    errors.append("Unowned production input: " + path)
        test_projects = [p for p, value in self.repo.projects.items() if value["test"]]
        for project in test_projects:
            if not any(v["project"] == project for v in self.repo.tests.values()):
                errors.append("No supported test classes discovered in " + project)
            if not any(self.suite_files(g) and any(self.repo.tests[p]["project"] == project for p in self.suite_files(g)) for owner in self.policy.get("areas", []) + self.policy.get("rules", []) for g in owner.get("tests", [])):
                errors.append("Test project has no production selection rule: " + project)
        return {"errors": sorted(set(errors)), "projects": len(test_projects), "test_files": len(self.repo.tests), "classes": len({c for t in self.repo.tests.values() for c in t["classes"]}), "source_groups": len(self.groups)}

    def plan(self, changes, scope="change", base="HEAD", extra_tests=None):
        if scope not in ("change", "iterate", "full"):
            raise ValueError("Unknown verification scope: " + scope)
        changes = sorted({self.repo.normalize(p) for p in changes})
        selected = defaultdict(set)
        gaps, warnings, obligations, ignored = [], [], {}, []
        touched = defaultdict(set)
        affected_symbols = defaultdict(set)
        propagating_symbols = defaultdict(set)
        tooling = scope == "full"
        builds = set()

        def choose(patterns, reason, required=True):
            found = set()
            for pattern in patterns:
                paths = self.suite_files(pattern)
                if required and not paths:
                    gaps.append(reason + ": test pattern selects no classes: " + pattern)
                for path in paths:
                    selected[path].add(reason)
                found.update(paths)
            return found

        if scope == "full":
            for path in self.repo.tests:
                selected[path].add("Explicit full baseline")
        choose(extra_tests or [], "Explicit additional tests")
        for path in changes:
            handled = False
            for reader in self.file_readers.get(Path(path).name, set()):
                selected[reader].add("Reads repository input: " + path)
                handled = True
            # Iteration stays inside the changed modules. Shared/cross-module policy
            # rules apply to change scope (CI) and to explicit or release full runs.
            for rule in self.policy.get("rules", []) if scope != "iterate" else []:
                if any(matches(path, g) for g in rule["changes"]):
                    choose(rule.get("tests", []), rule["id"] + ": " + path)
                    handled = True
            for obligation in self.policy.get("obligations", []):
                if any(matches(path, g) for g in obligation["changes"]):
                    obligations[obligation["id"]] = {k: v for k, v in obligation.items() if k != "changes"}
            if any(matches(path, g) for g in self.policy.get("tooling", [])):
                tooling = handled = True
            owner = self.repo.owner(path)
            if owner and self.repo.projects[owner]["test"]:
                handled = True
                if path in self.repo.tests:
                    selected[path].add("Changed test file: " + path)
                else:
                    choose([str(Path(owner).parent) + "/**/*.cs"], "Test project/helper/input change: " + path)
            area, root = self.area(path)
            if area:
                handled = True
                if owner:
                    builds.add(owner)
                key = self.file_groups.get(path)
                if key is None:
                    parts = path[len(root) + 1:].split("/")[:-1][:area.get("depth", 0)]
                    key = area["id"] + ("/" + "/".join(parts) if parts else "/_root")
                    self.groups.setdefault(key, {"area": area, "paths": [], "symbols": set(), "derived": defaultdict(set), "projects": {owner} if owner else set()})
                # Union old and current declarations; renamed/deleted contracts still have consumers.
                changed_symbols = self.file_symbols.get(path, set()) | symbols(self.repo.old_text(path, base))
                affected_symbols[key].update(changed_symbols or self.groups[key]["symbols"])
                propagating_symbols[key].update(affected_symbols[key])
                touched[key].add("Changed source: " + path)
                if path.endswith((".csproj", ".props", ".targets")) or key.endswith("/_root"):
                    choose(area["tests"], "Project/shared entry change: " + path)
            if not handled:
                if any(matches(path, g) for g in self.policy.get("ignore", [])):
                    ignored.append(path)
                elif not (self.repo.root / path).exists() and self.existed_at_base(path, base):
                    # A changed path missing from the worktree but present in the base commit
                    # is a deletion. Files with an area, owner, or rule were already claimed
                    # above (deleted contracts keep propagating to consumers); only a deletion
                    # nothing claims remains, and no test can be selected through a path whose
                    # project no longer exists.
                    ignored.append(path)
                else:
                    gaps.append("No selection policy for changed input: " + path)

        queue = deque(touched)
        queued = set(touched)
        direct = set(touched)
        visited = set()
        expanded = defaultdict(set)
        inheritance_expanded = defaultdict(set)
        while queue:
            key = queue.popleft()
            queued.remove(key)
            visited.add(key)
            group = self.groups[key]
            # Inheritance preserves the changed contract, including within one family.
            inheritance = list(propagating_symbols[key])
            while inheritance:
                for derived in group["derived"].get(inheritance.pop(), set()) - propagating_symbols[key]:
                    propagating_symbols[key].add(derived)
                    affected_symbols[key].add(derived)
                    inheritance.append(derived)
            for symbol in propagating_symbols[key]:
                for source in self.tokens.get(symbol, set()):
                    if self.file_groups[source] == key:
                        affected_symbols[key].update(self.file_symbols[source])
            # A composition consumer can itself have derived controls. Their
            # inherited behavior is affected even after the composition hop is used.
            inheritance = list(affected_symbols[key])
            while inheritance:
                for derived in group["derived"].get(inheritance.pop(), set()) - affected_symbols[key]:
                    affected_symbols[key].add(derived)
                    inheritance.append(derived)
            found = set()
            for symbol in sorted(affected_symbols[key]):
                for path in self.test_tokens.get(symbol, set()):
                    if group["projects"] & self.dependencies.get(self.repo.tests[path]["project"], set()):
                        found.add(path)
            # Direct family ownership remains complete. For a consumer, only its
            # referencing files contribute symbols, not unrelated directory siblings.
            segment = key.split("/")[-1]
            if key in direct:
                for pattern in group["area"]["tests"]:
                    for path in self.suite_files(pattern):
                        if segment != "_root" and any(part.lower() == segment.lower() for part in Path(path).parts):
                            found.add(path)
            if scope == "iterate":
                owned = {p for pattern in group["area"]["tests"] for p in self.suite_files(pattern)
                         if segment != "_root" and (any(part.lower() == segment.lower() for part in Path(p).parts)
                         or segment.rstrip("s").lower() in Path(p).stem.lower())}
                if owned:
                    found = owned
            reason = "; ".join(sorted(touched[key]))
            for path in found:
                selected[path].add(reason)
            if key in direct and (not found or group["area"].get("conservative", False)):
                choose(group["area"]["tests"], "Conservative owner suite: " + key)
                if not found:
                    warnings.append("No precise test ownership/reference for " + key + "; selected complete owner suites")
            if not found and key not in direct:
                builds.update(group["projects"])
                warnings.append("Consumer compiled without a dedicated test suite: " + key)
            if scope != "iterate":
                inheritance_forwarding = affected_symbols[key] - inheritance_expanded[key]
                inheritance_expanded[key].update(inheritance_forwarding)
                for symbol in sorted(inheritance_forwarding):
                    for consumer, derived in sorted(self.derivatives.get(symbol, {}).items()):
                        if consumer == key:
                            continue
                        allowed = any(group["projects"] & self.dependencies.get(p, set()) for p in self.groups[consumer]["projects"])
                        if allowed:
                            touched[consumer].add("Inherits affected " + symbol + " from " + key)
                            changed = bool(derived - affected_symbols[consumer])
                            affected_symbols[consumer].update(derived)
                            if (consumer not in visited or changed) and consumer not in queued:
                                queue.append(consumer)
                                queued.add(consumer)
                forwarding = propagating_symbols[key] - expanded[key]
                expanded[key].update(forwarding)
                for symbol in sorted(forwarding):
                    for source in sorted(self.tokens.get(symbol, set())):
                        consumer = self.file_groups[source]
                        # Declarations alone are not a dependency. In-family
                        # inheritance was handled above; composition stays local.
                        if consumer == key:
                            continue
                        consumer_group = self.groups[consumer]
                        allowed = group["projects"] & self.dependencies.get(self.repo.owner(source), set())
                        if allowed:
                            touched[consumer].add("Consumes " + symbol + " from " + key)
                            related = self.file_symbols.get(source, set())
                            inherited = consumer_group["derived"].get(symbol, set())
                            changed = bool(related - affected_symbols[consumer] or inherited - propagating_symbols[consumer])
                            affected_symbols[consumer].update(related)
                            propagating_symbols[consumer].update(inherited)
                            if (consumer not in visited or changed) and consumer not in queued:
                                queue.append(consumer)
                                queued.add(consumer)

        jobs = {}
        for path, reasons in sorted(selected.items()):
            test = self.repo.tests[path]
            job = jobs.setdefault(test["project"], {"project": test["project"], "classes": set(), "files": [], "reasons": set()})
            job["classes"].update(test["classes"])
            job["files"].append(path)
            job["reasons"].update(reasons)
        for job in jobs.values():
            job["classes"] = sorted(job["classes"])
            job["reasons"] = sorted(job["reasons"])
        staged = set(self.repo.git("diff", "--cached", "--name-only", "-z").split("\0")) - {""}
        unstaged = set(self.repo.git("diff", "--name-only", "-z").split("\0")) - {""}
        return {"version": 1, "scope": scope, "source_fingerprint": self.source_fingerprint, "validates": "working-tree", "staged_differs_from_worktree": sorted(staged & unstaged), "requested_tests": extra_tests or [], "changes": changes, "tests": sorted(jobs.values(), key=lambda j: j["project"]), "builds": sorted(builds | set(jobs)), "tooling": tooling, "gaps": sorted(set(gaps)), "warnings": sorted(set(warnings)), "obligations": sorted(obligations.values(), key=lambda o: o["id"]), "ignored": ignored, "affected_groups": sorted(visited), "inventory": {"projects": sum(p["test"] for p in self.repo.projects.values()), "test_files": len(self.repo.tests)}}
