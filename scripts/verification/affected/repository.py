"""Git inputs and conservative source/test inventory; never executes MSBuild."""
import hashlib
import re
import subprocess
import xml.etree.ElementTree as ET
from functools import lru_cache
from pathlib import Path


@lru_cache(maxsize=1024)
def glob_regex(pattern):
    # Unlike fnmatch, **/ also matches zero directories and * never crosses /.
    parts = re.split(r"(\*\*/|\*\*|\*|\?)", pattern)
    translations = {"**/": "(?:.*/)?", "**": ".*", "*": "[^/]*", "?": "[^/]"}
    return re.compile("".join(translations.get(p, re.escape(p)) for p in parts))


def matches(path, pattern):
    return glob_regex(pattern).fullmatch(path) is not None


def scrub_csharp(source):
    pattern = r'//[^\n]*|/\*.*?\*/|\$*""".*?"""|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\''
    return re.sub(pattern, lambda m: " " * len(m.group()), source, flags=re.S)


def test_classes(source):
    """Use actual containing class scopes, including partial/nested declarations.

    Discovery is deliberately independent of filenames and project namespaces.
    The audit rejects test projects with no supported test classes.
    """
    text = scrub_csharp(source)
    namespaces = list(re.finditer(r"\bnamespace\s+([\w.]+)\s*([;{])", text))
    braces = {}
    stack = []
    for match in re.finditer(r"[{}]", text):
        if match.group() == "{":
            stack.append(match.start())
        elif stack:
            braces[stack.pop()] = match.start()
    scopes = []
    for match in re.finditer(r"\bclass\s+(@?\w+)", text):
        opening = text.find("{", match.end())
        if opening in braces:
            scopes.append((opening, braces[opening], match.group(1).lstrip("@")))
    result = set()
    for attribute in re.finditer(r"\[[^\[\]]*\]", text):
        if not re.search(r"(?:^|,)\s*(?:[\w]+\.)*(?:Fact|Theory|AvaloniaFact|AvaloniaTheory)(?:Attribute)?\b", attribute.group()[1:-1]):
            continue
        pos = attribute.start()
        containers = sorted(s for s in scopes if s[0] < pos < s[1])
        if not containers:
            continue
        ns = [m.group(1) for m in namespaces if m.start() < pos and (m.group(2) == ";" or pos < braces.get(m.end() - 1, -1))]
        result.add((".".join(ns) + "." if ns else "") + "+".join(s[2] for s in containers))
    return sorted(result)


def symbols(source):
    result = set(re.findall(r"\b(?:class|interface|struct|enum)\s+(@?\w+)", scrub_csharp(source)))
    result.update(re.findall(r"\brecord\s+(?:(?:class|struct)\s+)?(@?\w+)\s*(?=[(:;{<])", scrub_csharp(source)))
    # Resource keys and XAML classes are dependencies even without C# references.
    result.update(s.rsplit(".", 1)[-1] for s in re.findall(r'\bx:(?:Key|Class)="([\w.]+)"', source))
    result.update(re.findall(r'\bTargetType="(?:\{x:Type\s+)?(?:\w+:)?(\w+)', source))
    # Extension methods are called through the receiver, not their declaring class.
    result.update(re.findall(r"\b(\w+)\s*(?:<[^{};]+>)?\s*\(\s*this\s+", scrub_csharp(source)))
    return {s.lstrip("@") for s in result if len(s) > 2 and s not in {"return", "new", "this", "base", "with"}}


class Repository:
    def __init__(self, root):
        self.root = Path(root).resolve()
        self._texts = {}
        self.discovery_snapshot = self.snapshot()
        self.files = sorted(set(self.git("ls-files", "--cached", "--others", "--exclude-standard", "-z").split("\0")) - {""})
        self.projects = {}
        for path in self.files:
            if path.endswith(".csproj") and (self.root / path).is_file():
                tree = ET.fromstring(self.text(path))
                test = any("test.sdk" in x.get("Include", "").lower() or "xunit" in x.get("Include", "").lower() for x in tree.iter("PackageReference"))
                test |= any((x.text or "").strip().lower() == "true" for x in tree.iter("IsTestProject"))
                references = []
                for x in tree.iter("ProjectReference"):
                    value = x.get("Include", "").replace("\\", "/")
                    if "$" not in value and value:
                        candidate = (self.root / path).parent / value
                        try:
                            references.append(candidate.resolve().relative_to(self.root).as_posix())
                        except ValueError:
                            pass
                self.projects[path] = {"path": path, "test": bool(test), "references": references}
        self.tests = {}
        for path in self.files:
            project = self.owner(path)
            if path.endswith(".cs") and project and self.projects[project]["test"] and (self.root / path).is_file():
                classes = test_classes(self.text(path))
                if classes:
                    self.tests[path] = {"project": project, "classes": classes}
        if self.snapshot() != self.discovery_snapshot:
            raise ValueError("Repository changed during project/test discovery; retry with consistent inputs")

    def git(self, *args):
        run = subprocess.run(["git", "-C", str(self.root), *args], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if run.returncode:
            raise ValueError(run.stderr.decode(errors="replace").strip())
        return run.stdout.decode("utf-8", errors="surrogateescape")

    def normalize(self, path):
        target = (self.root / path).resolve()
        try:
            return target.relative_to(self.root).as_posix()
        except ValueError:
            raise ValueError("Change path is outside the repository: " + str(path))

    @lru_cache(maxsize=None)
    def owner(self, path):
        candidates = [p for p in self.projects if path.startswith(str(Path(p).parent).replace("\\", "/") + "/")]
        return max(candidates, key=len) if candidates else None

    def text(self, path):
        if path not in self._texts:
            candidate = self.root / path
            self._texts[path] = candidate.read_text(errors="replace") if candidate.is_file() else ""
        return self._texts[path]

    def is_file(self, path):
        return (self.root / path).is_file()

    def old_text(self, path, base="HEAD"):
        try:
            return self.git("show", base + ":" + path)
        except ValueError:
            return ""

    def changes(self, base=None):
        reference = self.git("rev-parse", "--verify", (base or "HEAD") + "^{commit}").strip()
        if base:
            reference = self.git("merge-base", "HEAD", reference).strip()
        # --no-renames exposes both sides, including a rename subsequently edited.
        changed = self.git("diff", "--name-only", "--no-renames", "-z", reference, "--").split("\0")
        changed += self.git("diff", "--cached", "--name-only", "--no-renames", "-z", reference, "--").split("\0")
        changed += self.git("ls-files", "--others", "--exclude-standard", "-z").split("\0")
        return sorted(set(changed) - {""})

    def focus_paths(self, paths):
        """Expand explicit module directories without pulling in sibling modules."""
        focused = set()
        for path in paths:
            normalized = self.normalize(path)
            candidate = self.root / normalized
            if candidate.is_dir():
                prefix = normalized.rstrip("/") + "/"
                matches = [item for item in self.files if item.startswith(prefix) and (self.root / item).is_file()]
                if not matches:
                    raise ValueError("Focused module directory contains no Git-eligible files: " + normalized)
                focused.update(matches)
            else:
                focused.add(normalized)
        return sorted(focused)

    def snapshot(self):
        digest = hashlib.sha256()
        # Refresh inventory: edits and new files during a run invalidate evidence.
        files = sorted(set(self.git("ls-files", "--cached", "--others", "--exclude-standard", "-z").split("\0")) - {""})
        digest.update(self.git("rev-parse", "HEAD").encode())
        digest.update(self.git("diff", "--cached", "--binary", "--no-ext-diff").encode("utf-8", errors="surrogateescape"))
        for path in files:
            digest.update(path.encode("utf-8", errors="surrogateescape") + b"\0")
            candidate = self.root / path
            if candidate.is_file():
                with candidate.open("rb") as stream:
                    for chunk in iter(lambda: stream.read(1024 * 1024), b""):
                        digest.update(chunk)
            else:
                digest.update(b"<deleted>")
        return digest.hexdigest()
