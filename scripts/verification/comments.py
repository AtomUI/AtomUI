#!/usr/bin/env python3
"""Source comment checks for the AtomUI comment standard (docs/engineering/development/code-comment-guidelines.md).

    comments.py list [--path P ...]        print every violation with its location
    comments.py check [--path P ...]       fail on any violation under the paths
    comments.py check                      fail when a scope exceeds its baseline
    comments.py check --staged             fail on violations added by staged files
    comments.py baseline                   rewrite scripts/verification/comments-baseline.json
    comments.py diff-check --base REF [--path P ...]
                                          fail when changed .cs or .axaml files differ beyond comments
    comments.py expand [--path P ...]      put every block-level XML tag on its own line and a space before />
    comments.py apply PATCH                insert or replace comment lines; the patch never touches code
    (@ insert above a declaration, ^ insert above a line,
                                           = replace comment lines, ~ replace the comment of a line)

Rules:
    R1  a block-level XML tag written on a single line
    R2  Chinese text in a comment
    R3  a comment that references repository docs/ or .md files, or cites a specification section
    R4  a [SemanticPart] attribute without a comment
    R5  a Token definition without an XML comment
    R6  an externally visible declaration without an XML comment
    R7  an XML comment placed after the attribute list
    R8  <param>, <typeparam>, <paramref> or <typeparamref> names that do not match the signature
    R9  a doc comment whose XML is not well formed
    R10 a self-closing XML tag written without a space before />
    R11 a TODO or FIXME comment that states no completion or removal condition
    R12 block XML tags in another order than the standard: summary, typeparam, param, returns or value,
        exception, remarks, example

R6 follows the scope of the compiler's CS1591: a declaration counts when its whole type chain is public or
protected. A private protected member that is virtual, abstract or an override counts as well: the assemblies the
project grants internal visibility to derive from these types. Parameterless constructors are exempt.
"""
import argparse
import collections
import json
import os
import re
import subprocess
import sys
import textwrap
import xml.etree.ElementTree as ElementTree
from pathlib import Path

RULES = ("R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11", "R12")
BLOCK_TAG = re.compile(r"\s*<(summary|typeparam|param|returns|value|exception|remarks|example)(?=[\s>])")
TAG_ORDER = {"summary": 0, "typeparam": 1, "param": 2, "returns": 3, "value": 3, "exception": 4, "remarks": 5, "example": 6}
SCAN_ROOTS = ("src", "tests", "controlgallery", "build", "scripts")
BASELINE = "scripts/verification/comments-baseline.json"
SKIP_DIRS = {"bin", "obj", ".git", ".artifacts", "artifacts", "node_modules", "TestResults", "__pycache__"}
# Generated icon classes carry no auto-generated header; their comments belong to the icon generator.
EXEMPT_DIRS = ("src/AtomUI.Icons.AntDesign/GeneratedIcons",)
SOURCE_SUFFIXES = (".cs", ".axaml")
# Scripts and MSBuild files take part in the comment language rule only.
HASH_SUFFIXES = (".py", ".sh", ".ps1")
MSBUILD_SUFFIXES = (".props", ".targets", ".csproj")
PROSE_SUFFIXES = HASH_SUFFIXES + MSBUILD_SUFFIXES
# The checker itself is code; the comment-only proof does not apply to it.
TOOL_PREFIX = "scripts/verification/"

HAN = re.compile("[\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff]")
IDENTIFIER = r"[A-Za-z_]\w*"
BLOCK_TAGS = "summary|param|returns|value|typeparam|remarks|exception|example|para|code"
SINGLE_LINE_BLOCK = re.compile(r"<(?:" + BLOCK_TAGS + r")\b[^>]*>\s*\S|\S\s*</(?:" + BLOCK_TAGS + r")>")
DOC_REFERENCE = re.compile(r"(?<![\w/.])docs/[\w\-./#]*|[\w\-./]+\.md\b")
# A citation of a design specification section or rule number, as in "(spec 7.7)", "Spec 8, check 4", "(rule 2)",
# "control-token-guidelines rule 1" or "§8.3";
# the section lives under docs/.
SPECIFICATION_REFERENCE = re.compile(
    r"\(\s*spec\s+(?:§\s*)?\d+(?:\.\d+)*(?:\s*[,;–\-]\s*\d+(?:\.\d+)*)*"   # (spec 7.7 / (spec 14, 15
    r"|\b[Ss]pec\s+\d+(?:\.\d+)*"                                             # Spec 8, check 4
    r"|\(\s*rules?\s+\d+(?:\s*(?:,|and|&)\s*\d+(?:\.\d+)*)*"              # (rule 2 / (rules 2 and 3
    r"|[\w-]*guidelines?\s+rules?\s+\d+(?:\s*(?:,|and|&)\s*\d+)*"        # control-token-guidelines rule 1
    r"|§\s*\d+(?:\.\d+)*", re.I)                                               # §8.3
CITATION_HINT = re.compile(r"spec|§|rule", re.I)
PARAMETER_TAG = re.compile(r'<(param|typeparam|paramref|typeparamref)\s+name="([^"]*)"')
TOKEN = re.compile(
    r"(?P<lcomment>//[^\n]*)"
    r"|(?P<bstart>/\*)"
    r'|(?P<raw>"{3,})'
    r'|(?P<vstart>@")'
    r'|(?P<str>"(?:[^"\\\n]|\\.)*"?)'
    r"|(?P<chr>'(?:[^'\\\n]|\\.)*'?)"
)
INTERPOLATED = ("$\"", "@$\"", "$@\"")
MODIFIER = re.compile(
    r"(?:new\(\)|new|override|virtual|abstract|sealed|static|partial|async|unsafe|extern|const|readonly"
    r"|volatile|file|ref|scoped|required|public|protected|internal|private)(?!\w)")
TYPE_KEYWORD = re.compile(r"\b(class|interface|enum|struct|record|delegate)\b")
MODIFIER_WORDS = frozenset(
    "new override virtual abstract sealed static partial async unsafe extern const readonly volatile file ref scoped"
    " required public protected internal private".split())
ENUM_MEMBER = re.compile(r"(" + IDENTIFIER + r")\s*(?:=[^=].*)?,?\s*$")
SEMPART = re.compile(r"\[\s*SemanticPart\b")
SEMPART_PATH = re.compile(r'\(\s*"([^"]*)"')
TOKEN_TYPE = re.compile(r"\b(?:ControlDesignToken|LanguageDesignToken)\b")
AXAML_COMMENT = re.compile(r"<!--(.*?)-->", re.S)
# The theme analyzer reads these markers; their text is not prose and must stay as written.
AXAML_MARKER = re.compile(r"^<!--\s*(?:layout|shared:|owner:)")
GENERATED_MARKER = re.compile(r"<auto-?generated", re.I)
NAMESPACE = re.compile(r"^\s*namespace\s+([\w.]+)")
GENERIC_ARGUMENTS = re.compile(r"([A-Za-z_]\w*)\s*<([^<>]*)>")
IDENTIFIER_END = re.compile(IDENTIFIER + r"$")
FIRST_WORD = re.compile(r"\s*(\w+)")
ATTRIBUTE_START = re.compile(r"\s*\[")
SEGMENT_END = re.compile(r"[;}{]")
ONE_LINE_BLOCK = re.compile(r"^(\s*)///\s*<(" + BLOCK_TAGS + r")((?:\s[^>]*)?)>(.*)</\2>\s*$")
OPEN_WITH_TEXT = re.compile(r"^(\s*)///\s*<(" + BLOCK_TAGS + r")((?:\s[^>]*)?)>(\S.*)$")
CLOSE_WITH_TEXT = re.compile(r"^(\s*)///\s*(\S.*)</(" + BLOCK_TAGS + r")>\s*$")
# A self-closing tag whose last character before /> is not a space, for example <see cref="X"/>.
TIGHT_SELF_CLOSING = re.compile(r"<[A-Za-z][\w.:-]*(?:\s[^<>]*?)?\S/>")
TAG = re.compile(r"<[^>]+>")
# A TODO or FIXME marker, and the words that tell a reader when the comment can go.
MARKER_WORD = re.compile(r"\b(?:TODO|FIXME|HACK|XXX)\b")
CONDITION_WORD = re.compile(
    r"\b(?:when|until|after|once|before|if|unless|remove\w*|removal|delet\w*|replac\w*|instead|resolv\w*|"
    r"done|complete\w*|fix\w*|upgrad\w*|no longer|lands?|merged?)\b", re.I)
WRAP_COLUMN = 120
PATCH_HEADER = re.compile(r"^([@^=~])\s+(\S+?):(\d+)(?:-(\d+))?\s*$")


def comment_kind(body):
    """Return doc for a documentation comment and line for any other comment start."""
    if body.startswith("///") and not body.startswith("////"):
        return "doc"
    return "line"


def block_kind(body):
    """Return doc for a /** */ documentation comment and block for any other block comment."""
    if body.startswith("/**") and body != "/**/":
        return "doc"
    return "block"


def take_verbatim(line, position):
    """Return the position after a verbatim string that closes on this line, or the line end when it stays open."""
    while True:
        end = line.find('"', position)
        if end < 0:
            return len(line), False
        if line[end + 1:end + 2] == '"':
            position = end + 2
            continue
        return end + 1, True


def take_raw(line, position, quotes):
    """Return the position after a raw string that closes on this line, or the line end when it stays open."""
    while True:
        end = line.find('"', position)
        if end < 0:
            return len(line), False
        stop = end
        while stop < len(line) and line[stop] == '"':
            stop += 1
        if stop - end >= quotes:
            return stop, True
        position = stop


def rich_string(line, position, spans, literals):
    """Consume the string literal whose opening quote is at ``position`` on a line that has interpolation.

    Returns the next code position, or ("verb" | "raw" | "block", position[, detail]) when a literal or comment
    that starts here stays open past the end of the line.
    """
    size, start = len(line), position
    while start > 0 and line[start - 1] in "$@":
        start -= 1
    prefix = line[start:position]
    interpolated, verbatim = "$" in prefix, "@" in prefix
    if not verbatim and line.startswith('"""', position):
        stop = position
        while stop < size and line[stop] == '"':
            stop += 1
        end, closed = take_raw(line, stop, stop - position)
        literals.append((start, end))
        return end if closed else ("raw", end, stop - position)
    cursor = position + 1
    while cursor < size:
        char = line[cursor]
        if char == "\\" and not verbatim:
            cursor += 2
        elif char == '"' and not (verbatim and line.startswith('""', cursor)):
            literals.append((start, cursor + 1))
            return cursor + 1
        elif char == "{":
            if not interpolated or line.startswith("{{", cursor):
                cursor += 2 if interpolated else 1
                continue
            literals.append((start, cursor))
            cursor = rich_code(line, cursor + 1, True, spans, literals)
            if isinstance(cursor, tuple):
                return cursor
            start = cursor
        else:
            cursor += 1
    literals.append((start, size))
    return ("verb", size) if verbatim else size


def rich_code(line, position, hole, spans, literals):
    """Scan code on a line that contains interpolation holes. Inside a hole, stop after its closing brace."""
    size, depth = len(line), 0
    while position < size:
        char = line[position]
        if char == "/" and line.startswith("//", position):
            spans.append((position, size, comment_kind(line[position:])))
            return size
        if char == "/" and line.startswith("/*", position):
            end = line.find("*/", position + 2)
            if end < 0:
                spans.append((position, size, block_kind(line[position:])))
                return ("block", size, block_kind(line[position:]) == "doc")
            spans.append((position, end + 2, block_kind(line[position:end + 2])))
            position = end + 2
        elif char == "{":
            depth, position = depth + 1, position + 1
        elif char == "}":
            if hole and depth == 0:
                return position + 1
            depth, position = depth - 1, position + 1
        elif char == "'":
            end = position + 1
            while end < size and line[end] != "'":
                end += 2 if line[end] == "\\" else 1
            end = min(end + 1, size)
            literals.append((position, end))
            position = end
        elif char == '"':
            position = rich_string(line, position, spans, literals)
            if isinstance(position, tuple):
                return position
        else:
            position += 1
    return size


def scan_csharp(text):
    """Split C# source into per-line ``code``, ``solid`` text and comment spans.

    ``code`` keeps literals and replaces comments with blanks. ``solid`` also blanks literal contents so that
    declarations and braces can be matched without parsing strings. Comments are
    (line, start, end, text, kind) tuples with kind doc, line or block; lines are 1-based.
    """
    code, solid, comments = [], [], []
    state, quotes, literal_start, block_doc = "code", 0, 0, False
    for number, line in enumerate(text.split("\n"), 1):
        spans, literals = [], []
        position, length, literal_start = 0, len(line), 0
        while True:
            if state != "code":
                if state == "block":
                    end = line.find("*/")
                    spans.append((0, length if end < 0 else end + 2, "doc" if block_doc else "block"))
                    position = length if end < 0 else end + 2
                    state = "code" if end >= 0 else state
                elif state == "verb":
                    position, closed = take_verbatim(line, position)
                    literals.append((literal_start, position))
                    state = "code" if closed else state
                else:
                    position, closed = take_raw(line, position, quotes)
                    literals.append((literal_start, position))
                    state = "code" if closed else state
                if state != "code" or position >= length:
                    break
            if any(marker in line for marker in INTERPOLATED):
                result = rich_code(line, position, False, spans, literals)
                if isinstance(result, tuple):
                    state, position = result[0], result[1]
                    if state == "raw":
                        quotes = result[2]
                    elif state == "block":
                        block_doc = result[2]
                break
            match = TOKEN.search(line, position)
            if match is None:
                break
            kind = match.lastgroup
            if kind == "lcomment":
                spans.append((match.start(), length, comment_kind(match.group())))
                break
            if kind == "bstart":
                end = line.find("*/", match.end())
                body = line[match.start():length if end < 0 else end + 2]
                spans.append((match.start(), match.start() + len(body), block_kind(body)))
                if end < 0:
                    block_doc, state = block_kind(body) == "doc", "block"
                    break
                position = end + 2
            elif kind in ("raw", "vstart"):
                state = "raw" if kind == "raw" else "verb"
                literal_start, position = match.start(), match.end()
                quotes = len(match.group()) if kind == "raw" else quotes
                continue
            else:
                literals.append((match.start(), match.end()))
                position = match.end()
        code_line, solid_line = list(line), list(line)
        for start, end, kind in spans:
            comments.append((number, start, end, line[start:end], kind))
            code_line[start:end] = solid_line[start:end] = [" "] * (end - start)
        for start, end in literals:
            first = start + 1 if line[start:start + 1] in ('"', "'") else start
            last = end - 1 if line[end - 1:end] in ('"', "'") else end
            if last > first:
                solid_line[first:last] = [" "] * (last - first)
        code.append("".join(code_line))
        solid.append("".join(solid_line))
    return code, solid, comments


def comment_free_csharp(text):
    """Return the source without comments, trailing whitespace and blank lines."""
    return [line.rstrip() for line in scan_csharp(text)[0] if line.strip()]


def axaml_comments(text):
    """Return (line, text) pairs for every AXAML comment that is not a machine-read marker."""
    result = []
    for match in AXAML_COMMENT.finditer(text):
        if AXAML_MARKER.match(match.group(0)):
            continue
        result.append((text.count("\n", 0, match.start()) + 1, match.group(1)))
    return result


def comment_free_axaml(text):
    """Return AXAML text without prose comments, trailing whitespace and blank lines; markers are kept."""
    stripped = AXAML_COMMENT.sub(lambda match: match.group(0) if AXAML_MARKER.match(match.group(0)) else "", text)
    return [line.rstrip() for line in stripped.split("\n") if line.strip()]


def axaml_code(text):
    """Return the AXAML lines with every comment replaced by blanks of the same length."""
    return AXAML_COMMENT.sub(lambda match: re.sub(r"[^\n]", " ", match.group(0)), text).split("\n")


def code_lines(path, text):
    """Return the lines of a .cs or .axaml file with comments blanked, or None for other files."""
    if path.endswith(".cs"):
        return scan_csharp(text)[0]
    if path.endswith(".axaml"):
        return axaml_code(text)
    return prose_code(path, text) if path.endswith(PROSE_SUFFIXES) else None


def comment_free(path, text):
    """Return ``text`` without comments for a .cs or .axaml file, or None for other files."""
    if path.endswith(".cs"):
        return comment_free_csharp(text)
    if path.endswith(".axaml"):
        return comment_free_axaml(text)
    lines = prose_code(path, text) if path.endswith(PROSE_SUFFIXES) else None
    return None if lines is None else [line.rstrip() for line in lines if line.strip()]


# ---------------------------------------------------------------- declaration parsing

def strip_attributes(text):
    """Remove the attribute lists at the start of ``text`` so that a declaration starts at index 0."""
    while True:
        match = ATTRIBUTE_START.match(text)
        if not match:
            return text
        depth, index = 0, match.end() - 1
        while index < len(text):
            if text[index] == "[":
                depth += 1
            elif text[index] == "]":
                depth -= 1
                if depth == 0:
                    break
            index += 1
        if depth != 0:
            return text.lstrip()  # the list continues on later lines, so the declaration does not start here
        text = text[index + 1:]


def leading_modifiers(text):
    """Return the modifier keywords at the start of ``text``."""
    words, position = [], 0
    while True:
        match = MODIFIER.match(text, position)
        if not match:
            return words
        words.append(match.group().replace("()", ""))
        position = match.end()
        while position < len(text) and text[position].isspace():
            position += 1


def accessibility(words):
    """Return the accessibility that a modifier list declares, or None when it declares none.

    ``private protected`` and ``protected internal`` return private and public: the first is visible only inside
    the assembly, the second also outside it.
    """
    if "public" in words:
        return "public"
    if "private" in words:
        return "private"
    if "protected" in words:
        return "public" if "internal" in words else "protected"
    return "internal" if "internal" in words else None


def matching(text, position):
    """Return the index after the parenthesis that closes the one at ``position``."""
    depth = 0
    while position < len(text):
        if text[position] == "(":
            depth += 1
        elif text[position] == ")":
            depth -= 1
            if depth == 0:
                return position + 1
        position += 1
    return len(text)


def is_assignment(text, position):
    """Return True for the "=" of an initializer; comparisons and the "=>" arrow are not assignments."""
    return text[position + 1:position + 2] not in ("=", ">") and text[position - 1:position] not in "=!<>"


def declaration_text(solid, start):
    """Join the lines of the declaration that starts at ``start`` up to its body, arrow, initializer or semicolon.

    Returns (text, end line).
    """
    depth, parts = 0, []
    for index in range(start, min(len(solid), start + 80)):
        line, position = solid[index], 0
        parts.append(line)
        while position < len(line):
            char = line[position]
            if char in "([":
                depth += 1
            elif char in ")]":
                depth -= 1
            elif depth == 0 and (char in "{;" or char == "=" and (is_assignment(line, position) or line.startswith("=>", position))):
                return " ".join(part.strip() for part in parts), index
            position += 1
    return " ".join(part.strip() for part in parts), min(len(solid), start + 79) - 1


def split_parameters(group):
    """Split a parenthesized list into parameters without attributes and default values."""
    parts, depth, part = [], 0, []
    for char in group[1:-1]:
        if char in "(<[{":
            depth += 1
        elif char in ")>]}" and depth:
            depth -= 1
        if char == "," and depth == 0:
            parts.append("".join(part))
            part = []
        else:
            part.append(char)
    parts.append("".join(part))
    result = []
    for item in parts:
        if "[" in item:
            item = re.sub(r"\[[^\]]*\]", "", item)
        if "=" in item:
            item = re.split(r"=(?!=)", item, 1)[0]
        item = item.strip()
        if item:
            result.append(re.sub(r"\s+", " ", item) if "  " in item or "\t" in item else item)
    return result


def parse_parameters(group):
    """Return the (type, name) pairs of a parenthesized list; the name is empty for an unnamed parameter.

    The escape character of a keyword used as a name is dropped: a parameter declared `@operator` is documented
    as `<param name="operator" />`, and the compiler rejects the escaped form (CS1572).
    """
    result = []
    for item in split_parameters(group):
        words = item.split(" ")
        name = words[-1].lstrip("@")
        if len(words) > 1 and re.fullmatch(IDENTIFIER, name):
            result.append((" ".join(words[:-1]), name))
        else:
            result.append((item, ""))
    return result


def classify(text):
    """Split a declaration into its lead text, top-level parenthesis groups and the character that ends the lead.

    Returns (lead, groups, parameters, end). ``lead`` precedes the first body brace, arrow, initializer or
    semicolon. ``groups`` are (start, end) spans of top-level parentheses and ``parameters`` the (type, name)
    pairs of each group. ``end`` is "{", ";", "=" or "=>".
    """
    groups, parameters, depth, generic, position = [], [], 0, 0, 0
    while position < len(text):
        char = text[position]
        if char == "(" and depth == 0 and generic == 0:
            if tuple_return_type(text, position):
                position = matching(text, position)  # the tuple type belongs to the lead of the declaration
                continue
            end = matching(text, position)
            groups.append((position, end))
            parameters.append(parse_parameters(text[position:end]))
            position = end
            continue
        if char in "([":
            depth += 1
        elif char in ")]":
            depth -= 1
        elif char == "<" and depth == 0 and generic == 0 and position and (text[position - 1].isalnum() or text[position - 1] == "_"):
            generic += 1  # a type argument list; parentheses inside it are not parameter lists
        elif char == ">" and generic > 0 and not text.startswith("=>", position):
            generic -= 1
        elif depth == 0 and (char in "{;" or char == "=" and (is_assignment(text, position) or text.startswith("=>", position))):
            break
        position += 1
    lead = text[:position]
    end = text[position:position + 2] if text.startswith("=>", position) else text[position:position + 1]
    return lead, groups, parameters, end


TUPLE_LEAD = re.compile(r"(?:\b(?:operator|delegate)|" + MODIFIER.pattern + r")\s*$")


def tuple_return_type(text, position):
    """Return True for the parenthesis at ``position`` of a declaration when it is a tuple return type.

    A parameter list follows the name of the method; a tuple type follows only modifiers, ``operator`` or
    ``delegate``.
    """
    return TUPLE_LEAD.search(text[:position]) is not None


def declaration_kind(lead, groups, end):
    """Return the kind of a declaration: type, operator, event, field, method or property."""
    before_list = re.split(r"\bwhere\b", lead[:groups[0][0]] if groups else lead)[0]
    if re.search(r"\boperator\b", before_list) and not TYPE_KEYWORD.search(before_list):
        return "operator"
    if TYPE_KEYWORD.search(before_list):
        return "type"
    if re.search(r"\bevent\b", before_list):
        return "event"
    if groups:
        return "method"
    if end == "=>":
        return "property"
    return "field" if end == "=" else "property"


def declaration_name(kind, text, lead, groups):
    """Return the simple name of a declaration; indexers are named this."""
    if kind == "type":
        match = re.search(r"\b(?:class|interface|enum|struct|record(?:\s+(?:class|struct))?)\s+(" + IDENTIFIER + ")", lead)
        if match:
            return match.group(1)
        lead = (lead[:groups[0][0]] if groups else lead).strip()
    elif kind == "method":
        for start, _ in groups:
            match = re.search(r"(" + IDENTIFIER + r")\s*(?:<[^<>]*>)?\s*$", text[:start])
            if match and match.group(1) not in MODIFIER_WORDS:  # a tuple return type leaves its modifiers behind
                return match.group(1)
        return "?"
    if re.search(r"\bthis\s*\[", lead):
        return "this"
    match = re.search(r"(" + IDENTIFIER + r")\s*$", lead.rstrip())
    return match.group(1) if match else "?"


def attribute_only(text):
    """Return True when ``text`` is an attribute list that stands alone on its line (or continues on the next
    lines). An attribute that shares its line with a complete declaration, as in ``[Required] public string A
    { get; set; }``, is that declaration, which belongs to the member above."""
    depth, position = 0, 0
    while position < len(text):
        if text[position] == "[":
            depth += 1
        elif text[position] == "]":
            depth -= 1
            if depth == 0:
                return not text[position + 1:].strip()
        position += 1
    return True


def leading_trivia(code, comment_lines, index):
    """Return ([doc comment texts], [attribute texts], doc-after-attribute) above the declaration at ``index``."""
    docs, attributes, order, position, after = [], [], [], index - 1, False
    while position >= 0:
        text = code[position].strip()
        if not text:
            found = [body for _, _, body, kind in comment_lines.get(position, []) if kind == "doc"]
            if found and "doc" not in order:
                order.append("doc")
            docs.extend(found)
            position -= 1  # blank lines do not detach a doc comment from its declaration
            continue
        if text.startswith("#"):
            position -= 1  # a preprocessor directive around the attribute list is part of the declaration
            continue
        if text.startswith("[") and not attribute_only(text):
            break  # an attribute and the complete declaration of the member above share the line
        if text.startswith("[") or (text.endswith("]") and text.count("]") > text.count("[")):
            if "attribute" not in order:
                order.append("attribute")
            attributes.append(text)
            after = after or bool(docs)  # collected bottom-up: an attribute above the comment
            balance = text.count("[") - text.count("]")
            while balance < 0 and position > 0:  # the attribute list continues on the lines above
                position -= 1
                earlier = code[position].strip()
                balance += earlier.count("[") - earlier.count("]")
                attributes.append(earlier)
            position -= 1
            continue
        break  # code: the leading trivia has ended
    # Collected bottom-up: the comment is valid only when no attribute list lies above it. A comment between two
    # attribute lists, as after a note that stands between them, is rejected by the compiler (CS1587).
    return docs[::-1], attributes[::-1], after


def semantic_part_findings(code, comment_lines, index, owner, path):
    """Return R4 findings for [SemanticPart] attributes that have no comment on the line directly above."""
    findings = []
    for order, match in enumerate(SEMPART.finditer(code[index])):
        above = comment_lines.get(index - 1, [])
        commented = order == 0 and bool(above) and not code[index - 1].strip() and all(kind != "doc" for _, _, _, kind in above)
        if commented:
            continue
        text, depth, line_index, column = "", 0, index, match.start()
        while line_index < min(len(code), index + 12):
            segment = code[line_index][column:]
            text += segment
            depth += segment.count("[") - segment.count("]")
            if depth <= 0:
                break
            line_index, column = line_index + 1, 0
        part = SEMPART_PATH.search(text)
        pieces = (part.group(1) if part else "").split(".")
        style = (owner["name"] if owner else "?") + "".join(piece[:1].upper() + piece[1:] for piece in pieces) + "Style"
        findings.append(("R4", path, index + 1, "[SemanticPart] has no comment directly above; generated style " + style))
    return findings


def new_record(kind, name, line, stack, namespace, docs, r7):
    """Return the analysis record of one declaration. ``line`` is the 1-based source line."""
    return {
        "kind": kind, "name": name, "line": line, "docs": docs, "r7": r7, "visible": False,
        "namespace": namespace, "owners": tuple(item["name"] for item in stack), "signature": "",
        "owners_key": tuple(item["signature"] + ("`%d" % item["arity"] if item["arity"] else "") for item in stack),
        "params": [], "typeparams": [], "owner_typeparams": (), "arity": 0, "depth": 0, "type_kind": "", "token": False,
        "token_owner": False, "exempt": False,
    }


def display_name(record):
    """Return the type chain and the name of a declaration."""
    return ".".join(record["owners"] + (record["name"],))


def explode(solid):
    """Split every line at every brace and semicolon so that a member written on one line with its type is still
    found. Returns [(text, source line index)].
    """
    result = []
    for index, line in enumerate(solid):
        if "{" not in line and "}" not in line and ";" not in line:
            result.append((line, index))
            continue
        position = 0
        for match in SEGMENT_END.finditer(line):
            if match.group() == "{" and match.start() > position:  # a brace opens the body of what precedes it
                result.append((line[position:match.start()], index))
                position = match.start()
            if match.end() > position:
                result.append((line[position:match.end()], index))
                position = match.end()
        if position < len(line):
            result.append((line[position:], index))
    return result


def starts_declaration(line, owner, depth):
    """Cheap test before the full parse: the line starts with a modifier or an attribute, or it is a member of an
    interface or an enum, whose members have no accessibility modifier."""
    if owner is not None and depth == owner["depth"] and owner["type_kind"] in ("interface", "enum"):
        return bool(line.strip()) and line.lstrip()[0] not in "{}#"
    if line.lstrip().startswith("["):
        return True
    match = FIRST_WORD.match(line)
    return match is not None and match.group(1) in MODIFIER_WORDS


def declaration_at(texts, code, comment_lines, index, real, first, stack, depth, namespace):
    """Return (record, end line, opens a type body) for the declaration that starts at line ``index``.

    ``index`` counts the exploded lines and ``real`` is the source line of the declaration. ``first`` is False for
    a declaration that follows another one on its source line; the comments above that line belong to the other
    declaration. Members of interfaces and enum values have no accessibility modifier; they are recognized by the
    enclosing type. Every other declaration needs one.
    """
    text = strip_attributes(texts[index]).strip()
    words = leading_modifiers(text)
    access = accessibility(words)
    owner = stack[-1] if stack else None
    if owner is not None and depth != owner["depth"]:
        return None, index, False  # an accessor, a local function or code inside a member body
    inside = owner is not None
    if access is None:
        if not (inside and text and text[0] not in "{}#"):
            return None, index, False
        if owner["type_kind"] == "enum":
            member = ENUM_MEMBER.match(text)
            if member is None:
                return None, index, False
            docs, _, r7 = leading_trivia(code, comment_lines, real) if first else ([], [], False)
            record = new_record("enumMember", member.group(1), real + 1, stack, namespace, docs, r7)
            record["visible"] = owner["visible"]
            record["signature"] = "enumMember " + record["name"]
            return record, index, False
        if owner["type_kind"] != "interface":
            return None, index, False
        access = "public"
    body, end_line = declaration_text(texts, index)
    lead, groups, parameters, end = classify(body)
    kind = declaration_kind(lead, groups, end)
    if kind == "operator":
        operator = re.search(r"((?:implicit\s+|explicit\s+)?operator\s+.+?)\s*$", re.split(r"\bwhere\b", lead[:groups[0][0]] if groups else lead)[0])
        name = operator.group(1) if operator else "?"
    else:
        name = declaration_name(kind, body, lead, groups)
        if kind == "method" and owner is not None and name == owner["name"]:
            kind = "ctor0" if not parameters[0] else "ctor"
    docs, attributes, r7 = leading_trivia(code, comment_lines, real) if first else ([], [], False)
    if r7:
        docs = []  # the compiler does not attach a comment that follows the attribute list
    record = new_record(kind, name, real + 1, stack, namespace, docs, r7)
    # A typeparamref may name a type parameter of an enclosing type; the compiler accepts that.
    record["owner_typeparams"] = tuple(entry for item in stack for entry in item["typeparams"])
    # A private protected member that a derived class can override is an extension point of the friend assemblies.
    overridable = "private" in words and "protected" in words and any(
        word in words for word in ("virtual", "abstract", "override"))
    record["visible"] = (access in ("public", "protected") or overridable) and all(
        item["visible"] for item in stack)
    record["token_owner"] = bool(owner and owner["token"])
    record["signature"] = kind + " " + name
    if kind == "type":
        keyword = TYPE_KEYWORD.search(lead)
        record["type_kind"] = keyword.group(1) if keyword else "class"
        record["token"] = TOKEN_TYPE.search(" ".join(attributes)) is not None
    if kind in ("method", "ctor", "ctor0", "operator"):
        record["signature"] += "(" + ",".join(item[0] for item in (parameters[0] if parameters else [])) + ")"
    if groups and kind in ("method", "ctor", "ctor0", "operator", "type"):
        record["params"] = [item[1] for item in parameters[0] if item[1]]
    if kind == "property" and name == "this":
        bracket = re.search(r"\[([^\]]*)\]", lead)
        record["params"] = [item[1] for item in (parse_parameters("(" + bracket.group(1) + ")") if bracket else []) if item[1]]
    for match in GENERIC_ARGUMENTS.finditer(lead):
        if match.group(1) == name:
            record["typeparams"] = [found.group(0) for found in
                                    (IDENTIFIER_END.match(item.strip()) for item in match.group(2).split(",")) if found]
            record["arity"] = len(record["typeparams"])
            break
    return record, end_line, kind == "type" and end == "{"


def parameter_findings(record, path):
    """Return R8 findings for parameter tag names that the signature does not have.

    `<param>` and `<typeparam>` declare a parameter; `<paramref>` and `<typeparamref>` only refer to one, so for
    them the name check alone applies. A keyword parameter is declared as ``@operator``; the compiler accepts the
    plain name ``operator`` in every tag and rejects the ``@`` spelling, so the names are compared as written.
    """
    tags = PARAMETER_TAG.findall(" ".join(record["docs"]))
    findings = []
    for tag, name in tags:
        names = record["typeparams"] + (list(record["owner_typeparams"]) if tag == "typeparamref" else []) \
            if tag in ("typeparam", "typeparamref") else record["params"]
        if name not in names:
            findings.append(("R8", path, record["line"], "<%s> name \"%s\" is not in the signature" % (tag, name)))
    documented = {name for tag, name in tags if tag == "param"}
    for name in record["params"]:
        if documented and name not in documented:
            findings.append(("R8", path, record["line"], "parameter \"%s\" has no <param> tag" % name))
    return findings


def order_findings(record, path):
    """Return an R12 finding when the block tags of a doc comment are not in the order of the standard."""
    tags = []
    for body in record["docs"]:
        match = BLOCK_TAG.match(body[3:] if body.startswith("///") else body)
        if match:
            tags.append(match.group(1))
    for earlier, later in zip(tags, tags[1:]):
        if TAG_ORDER[later] < TAG_ORDER[earlier]:
            return [("R12", path, record["line"], "block tags out of order: <%s> follows <%s>" % (later, earlier))]
    return []


def declaration_findings(record, path):
    """Return the comment findings of one declaration that do not need scope-wide information."""
    findings = []
    if record["r7"]:
        findings.append(("R7", path, record["line"], "XML comment is placed after the attribute list"))
    if record["docs"]:
        findings.extend(parameter_findings(record, path))
        findings.extend(order_findings(record, path))
        if malformed_doc(record["docs"]):
            findings.append(("R9", path, record["line"], "doc comment is not well-formed XML"))
    elif record["kind"] == "property" and record["token_owner"]:
        record["exempt"] = True  # reported as R5
        findings.append(("R5", path, record["line"], "token " + display_name(record) + " has no XML comment"))
    return findings


def comment_neighbourhood(comment_lines, number, span=4):
    """Return the texts of the comments on ``number`` and on the adjacent comment lines (at most ``span`` each way)."""
    texts = [body for _, _, body, _ in comment_lines[number - 1]]
    for step in (-1, 1):
        offset = 0
        while offset < span:
            offset += 1
            found = comment_lines.get(number - 1 + step * offset)
            if not found:
                break
            texts.extend(body for _, _, body, _ in found)
    return texts


def repository_reference(body):
    """Return the repository document or specification section that a comment references, or None."""
    if CITATION_HINT.search(body):
        match = SPECIFICATION_REFERENCE.search(body)
        if match:
            return match.group(0).strip()
    if "docs/" not in body and ".md" not in body:
        return None
    match = DOC_REFERENCE.search(body)
    return match.group(0).rstrip(".") if match else None


def analyze_csharp(text, path):
    """Return (findings, declarations) for one C# file.

    A finding is (rule, path, line, message). A declaration record describes one type or member; R6 is decided
    per scope afterwards so that the parts of partial types and members are merged.
    """
    code, solid, comments = scan_csharp(text)
    findings, declarations = [], []
    comment_lines = collections.defaultdict(list)
    for number, start, end, body, kind in comments:
        comment_lines[number - 1].append((start, end, body, kind))
        if kind == "doc" and "<" in body and SINGLE_LINE_BLOCK.search(body[3:]):  # drop the /// or /** prefix
            findings.append(("R1", path, number, "block XML tag on a single line"))
        if kind == "doc" and TIGHT_SELF_CLOSING.search(body[3:]):
            findings.append(("R10", path, number, "self-closing XML tag has no space before />"))
        if not body.isascii() and HAN.search(body):
            findings.append(("R2", path, number, "comment contains Chinese text"))
        reference = repository_reference(body)
        if reference:
            findings.append(("R3", path, number, "comment references " + reference))
    # R11 needs the comments that follow a marker, so it runs after every comment has been collected.
    for number, _, _, body, _ in comments:
        if MARKER_WORD.search(body) and not CONDITION_WORD.search(" ".join(comment_neighbourhood(comment_lines, number))):
            findings.append(("R11", path, number, "TODO or FIXME comment states no completion or removal condition"))
    namespace = ""
    for line in solid[:80]:
        match = NAMESPACE.match(line)
        if match:
            namespace = match.group(1)
            break
    lines = explode(solid)
    texts = [line for line, _ in lines]
    depth, stack, pending, skip_to, part_lines = 0, [], [], -1, set()
    for index, (_, real) in enumerate(lines):
        while stack and stack[-1]["depth"] > depth:
            stack.pop()
        owner = stack[-1] if stack else None
        if "SemanticPart" in texts[index] and real not in part_lines:
            part_lines.add(real)
            findings.extend(semantic_part_findings(code, comment_lines, real, owner, path))
        if index > skip_to and starts_declaration(texts[index], owner, depth):
            first = index == 0 or lines[index - 1][1] != real
            record, end_line, opens_body = declaration_at(
                texts, code, comment_lines, index, real, first, stack, depth, namespace)
            if record is not None:
                skip_to = max(skip_to, end_line)
                declarations.append(record)
                findings.extend(declaration_findings(record, path))
                if opens_body:
                    record["end_line"] = end_line
                    pending.append(record)
        before = depth
        depth += texts[index].count("{") - texts[index].count("}")
        for item in pending[:]:
            if item["end_line"] == index and depth > before:
                item["depth"] = depth
                stack.append(item)
                pending.remove(item)
    # [SemanticPart] attributes stand in the file-scoped namespace, above the partial class that owns them.
    lone = {record["name"] for record in declarations if record["kind"] == "type" and not record["owners"]}
    if len(lone) == 1:
        findings = [finding[:3] + (finding[3].replace("generated style ?", "generated style " + next(iter(lone))),)
                    if finding[0] == "R4" and "generated style ?" in finding[3] else finding for finding in findings]
    return findings, declarations


def analyze_axaml(text, path):
    """Return (findings, no declarations) for one AXAML file."""
    findings = []
    for line, body in axaml_comments(text):
        if HAN.search(body):
            findings.append(("R2", path, line, "comment contains Chinese text"))
        reference = repository_reference(body)
        if reference:
            findings.append(("R3", path, line, "comment references " + reference))
    return findings, []


def analyze(path, text):
    """Return (findings, declarations) for one source, AXAML, script or MSBuild file."""
    if path.endswith(".cs"):
        return analyze_csharp(text, path)
    if path.endswith(".axaml"):
        return analyze_axaml(text, path)
    return analyze_prose(text, path)


def missing_comment_findings(results):
    """Return the R6 findings of a scope after merging the parts of partial types and members."""
    groups = collections.OrderedDict()
    for path, (_, declarations) in results:
        for record in declarations:
            if record["visible"] and record["kind"] != "ctor0" and not record["exempt"]:
                # The number of type parameters keeps overloads and generic types apart from their plain namesakes.
                key = (record["namespace"], record["owners_key"], with_arity(record))
                groups.setdefault(key, []).append((path, record))
    findings = []
    for members in groups.values():
        if not any(record["docs"] for _, record in members):
            path, record = main_part(members)
            findings.append(("R6", path, record["line"], record["kind"] + " " + display_name(record) + " has no XML comment"))
    return findings


def with_arity(record):
    """Return the signature of a declaration with the number of its type parameters appended."""
    return record["signature"] + ("`%d" % record["arity"] if record["arity"] else "")


def main_part(members):
    """Return the part that reports a type: the file named after it, else the first part in path order."""
    for path, record in members:
        if record["kind"] == "type" and Path(path).stem == record["name"]:
            return path, record
    return members[0]


# ---------------------------------------------------------------- repository checks

def hash_comment_start(line):
    """Return the index where the "#" comment of a script line starts, or -1 when the line has none."""
    quote = None
    position = 0
    while position < len(line):
        char = line[position]
        if quote:
            if char == "\\" and quote != "'":
                position += 1
            elif char == quote:
                quote = None
        elif char in "'\"":
            quote = char
        elif char == "#" and (position == 0 or line[position - 1].isspace()):
            return position
        position += 1
    return -1


def prose_comments(path, text):
    """Return (line, text) pairs for the comments of a script or MSBuild file."""
    result = []
    if path.endswith(MSBUILD_SUFFIXES):
        for match in AXAML_COMMENT.finditer(text):
            result.append((text.count("\n", 0, match.start()) + 1, match.group(1)))
        return result
    block = False
    for index, line in enumerate(text.split("\n"), 1):
        if path.endswith(".ps1") and (block or line.lstrip().startswith("<#")):
            end = line.find("#>")
            result.append((index, line))
            block = end < 0
            continue
        start = hash_comment_start(line)
        if start >= 0:
            result.append((index, line[start + 1:].strip()))
    return result


def analyze_prose(text, path):
    """Return (findings, no declarations) for a script or MSBuild file: only the language of comments is checked."""
    findings = []
    for line, body in prose_comments(path, text):
        if HAN.search(body):
            findings.append(("R2", path, line, "comment contains Chinese text"))
    return findings, []


def prose_code(path, text):
    """Return the lines of a script or MSBuild file with their comments replaced by blanks."""
    if path.endswith(MSBUILD_SUFFIXES):
        return axaml_code(text)
    result, block = [], False
    for line in text.split("\n"):
        if path.endswith(".ps1") and (block or line.lstrip().startswith("<#")):
            start = 0 if block else line.index("<#")
            end = line.find("#>")
            blank = re.sub(r"[^\n]", " ", line[start:end + 2 if end >= 0 else len(line)])
            result.append(line[:start] + blank + (line[end + 2:] if end >= 0 else ""))
            block = end < 0
            continue
        start = hash_comment_start(line)
        result.append(line if start < 0 else line[:start] + " " * (len(line) - start))
    return result


def is_generated(text, path=""):
    """Return True for a file that declares itself as generated code in a comment near its top.

    In a C# file only a comment counts. A source generator writes the marker into the code it emits, and in the
    generator's own source that marker sits inside a string literal.
    """
    if not GENERATED_MARKER.search(text[:2000]):
        return False
    if not path.endswith(".cs"):
        return True
    lines, starts, offset = text.split("\n"), [], 0
    for line in lines:
        starts.append(offset)
        offset += len(line) + 1
    return any(starts[number - 1] + start < 2000 for number, start, _, comment, _ in scan_csharp(text)[2]
               if GENERATED_MARKER.search(comment))


def is_scanned(path):
    """Return True for a scanned source, script or MSBuild file that is neither exempt nor generated by name."""
    if not path.endswith(SOURCE_SUFFIXES + PROSE_SUFFIXES) or path.endswith((".g.cs", ".g.i.cs")):
        return False
    if path.split("/")[0] not in SCAN_ROOTS:
        return False
    return not any(path == exempt or path.startswith(exempt + "/") for exempt in EXEMPT_DIRS)


def index_files(root):
    """Return [(relative path, scope)] for every scanned source file.

    A scope is the project directory that holds the file, or the top-level directory when no project holds it.
    """
    projects, files = set(), []
    for top in SCAN_ROOTS:
        base = root / top
        if not base.is_dir():
            continue
        for directory, names, entries in os.walk(base):
            relative = Path(directory).relative_to(root).as_posix()
            names[:] = sorted(name for name in names if name not in SKIP_DIRS)
            if any(entry.endswith(".csproj") for entry in entries):
                projects.add(relative)
            files.extend(relative + "/" + entry for entry in sorted(entries) if is_scanned(relative + "/" + entry))
    result = []
    for path in files:
        scope, candidate = path.split("/")[0], path
        while "/" in candidate:
            candidate = candidate.rsplit("/", 1)[0]
            if candidate in projects:
                scope = candidate
                break
        result.append((path, scope))
    return result


def read(path):
    """Return the text of a file, or None when it cannot be read."""
    try:
        return path.read_text(encoding="utf-8-sig", errors="replace")
    except OSError:
        return None


def git(root, *arguments):
    """Return the standard output of a git command, or None when it fails."""
    result = subprocess.run(["git", "-C", str(root)] + list(arguments), capture_output=True, text=True, errors="replace")
    return result.stdout if result.returncode == 0 else None


def under(path, paths):
    """Return True when ``path`` is one of ``paths`` or lies below one of them."""
    return any(path == prefix or path.startswith(prefix + "/") for prefix in paths)


def selected(path, paths, direct=False):
    """Return True when ``path`` lies in one of ``paths``.

    With ``direct`` a path argument matches only the files directly in it when it is a directory, and only that
    file when it is a file path.
    """
    if not paths:
        return True
    if not direct:
        return under(path, paths)
    if path in paths:  # a file path selects exactly that file
        return True
    return path.rsplit("/", 1)[0] in paths  # a directory selects the files directly in it


def scopes_under(index, paths):
    """Return the scopes that hold files under any of ``paths``."""
    return {scope for path, scope in index if under(path, paths)}


def scope_findings(results):
    """Return all findings of one scope from its per-file results [(path, (findings, declarations))]."""
    findings = missing_comment_findings(results)
    for _, (file_findings, _) in results:
        findings.extend(file_findings)
    return findings


def load_scope(index, root, scope, overrides=None):
    """Return the per-file results of a scope. ``overrides`` maps paths to the text that replaces the file on disk."""
    results = []
    for path, file_scope in index:
        if file_scope != scope:
            continue
        text = overrides.get(path, read(root / path)) if overrides else read(root / path)
        if text is not None and not is_generated(text, path):
            results.append((path, analyze(path, text)))
    return results


def collect(root, scopes=None):
    """Analyze the repository and return {scope: [(rule, path, line, message)]}."""
    results = collections.defaultdict(list)
    for path, scope in index_files(root):
        if scopes is None or scope in scopes:
            text = read(root / path)
            if text is not None and not is_generated(text, path):
                results[scope].append((path, analyze(path, text)))
    return {scope: sorted(scope_findings(items), key=lambda finding: (finding[1], finding[2], finding[0]))
            for scope, items in results.items()}


def counts(findings):
    """Return {rule: violations}."""
    result = dict.fromkeys(RULES, 0)
    for rule, _, _, _ in findings:
        result[rule] += 1
    return result


def format_finding(finding):
    return "%s %s:%d: %s" % finding


def added_findings(before, after):
    """Return the findings that ``after`` adds to ``before``. A finding that only moved to another file is kept out."""
    remaining = collections.Counter((rule, message) for rule, _, _, message in before)
    added = []
    for finding in after:
        key = (finding[0], finding[3])
        if remaining[key]:
            remaining[key] -= 1
        else:
            added.append(finding)
    return added


def staged_errors(root):
    """Return the violations that the staged .cs and .axaml files add to their scopes.

    Each affected scope is analyzed twice with the same unstaged files: once with the HEAD text of the staged
    files and once with their staged text. Partial types that are documented in an unstaged file therefore stay
    documented. The staged texts are read once: a commit that stages thousands of files touches many scopes.
    """
    listing = git(root, "diff", "--cached", "--name-status", "--diff-filter=AM")
    if listing is None:
        return []
    staged = {entry.split("\t")[-1] for entry in listing.splitlines()}
    staged = {path for path in staged if is_scanned(path)}
    if not staged:
        return []
    index = index_files(root)
    texts = {path: (git(root, "show", "HEAD:" + path), git(root, "show", ":" + path)) for path in sorted(staged)}
    findings = []
    for scope in sorted(scopes_under(index, staged)):
        before = {path: head for path, (head, _) in texts.items()}
        after = {path: staged_text for path, (_, staged_text) in texts.items()}
        findings.extend(added_findings(scope_findings(load_scope(index, root, scope, before)),
                                       scope_findings(load_scope(index, root, scope, after))))
    return sorted(findings, key=lambda finding: (finding[1], finding[2], finding[0]))


def audit_comments(root, staged=False):
    """Return blocking messages for ``test.py audit``; the list is empty when the comments pass."""
    if staged:
        return [format_finding(finding) for finding in staged_errors(root)]
    path = root / BASELINE
    if not path.is_file():
        return [BASELINE + " is missing; run comments.py baseline"]
    baseline = json.loads(path.read_text()).get("scopes", {})
    result = collect(root)
    errors = []
    for scope in sorted(set(baseline) | set(result)):
        current, allowed = counts(result.get(scope, [])), baseline.get(scope, {})
        errors.extend("comments: %s in %s is %d, baseline %d" % (rule, scope, current[rule], allowed.get(rule, 0))
                      for rule in RULES if current[rule] > allowed.get(rule, 0))
    return errors


def diff_errors(root, base, paths, direct=False):
    """Return messages for changed source, AXAML, script or MSBuild files whose difference from ``base`` is not
    limited to comments. Files of the checker itself are not part of the proof."""
    errors = []
    for entry in (git(root, "diff", "--name-status", base, "--", *paths) or "").splitlines():
        fields = entry.split("\t")
        status, path = fields[0], fields[-1]
        if not is_scanned(path) or path.startswith(TOOL_PREFIX) or not selected(path, paths, direct):
            continue
        before = git(root, "show", base + ":" + path)
        after = None if status == "D" else load(root / path)
        if before is None or after is None:
            errors.append(path + ": a source file was added or removed")
        elif before.startswith("\ufeff") != after[1]:
            errors.append(path + ": byte order mark changed")
        elif comment_free(path, before.lstrip("\ufeff")) != comment_free(path, after[0]):
            errors.append(path + ": changed beyond comments")
    return errors


def malformed_doc(docs):
    """Return True when the XML of a doc comment is not well formed."""
    text = " ".join(re.sub(r"^\s*(///|/\*\*|\*)", "", body).strip() for body in docs)
    try:
        ElementTree.fromstring("<root>" + text + "</root>")
    except ElementTree.ParseError:
        return True
    return False


def wrap_doc(indent, text):
    """Wrap the text of one doc comment line so that no line exceeds the wrap column; tags stay on one line."""
    marker, pieces, position = "\x00", [], 0
    for match in TAG.finditer(text):
        pieces.append(text[position:match.start()])
        pieces.append(match.group(0).replace(" ", marker))  # a tag is never split over two lines
        position = match.end()
    pieces.append(text[position:])
    lines = textwrap.wrap("".join(pieces), width=max(WRAP_COLUMN - len(indent) - 4, 20), break_long_words=False,
                          break_on_hyphens=False)
    return [indent + "/// " + line.replace(marker, " ") for line in lines] or [indent + "///"]


def space_self_closing(line):
    """Return ``line`` with a space before the ``/>`` of every self-closing XML tag, or ``line`` when it has none."""
    return TIGHT_SELF_CLOSING.sub(lambda match: match.group(0)[:-2] + " />", line)


def expand_line(line):
    """Return the lines that replace a doc comment line whose tags are not spaced as the standard requires."""
    line = space_self_closing(line)
    indent = line[:len(line) - len(line.lstrip())]
    match = ONE_LINE_BLOCK.match(line)
    if match:
        inner = match.group(4).strip()
        if not inner:
            return []  # an empty tag carries nothing
        return ([indent + "/// <" + match.group(2) + match.group(3) + ">"] + wrap_doc(indent, inner)
                + [indent + "/// </" + match.group(2) + ">"])
    match = OPEN_WITH_TEXT.match(line)
    if match:
        return [indent + "/// <" + match.group(2) + match.group(3) + ">"] + wrap_doc(indent, match.group(4).strip())
    match = CLOSE_WITH_TEXT.match(line)
    if match:
        return wrap_doc(indent, match.group(2).strip()) + [indent + "/// </" + match.group(3) + ">"]
    return [line]


def expand_docs(text):
    """Return the C# source with block-level tags on their own lines and a space before every ``/>``."""
    code, _, comments = scan_csharp(text)
    doc_lines = {number - 1 for number, _, _, _, kind in comments if kind == "doc"}
    newline = "\r\n" if "\r\n" in text else "\n"
    result, changed = [], False
    for index, line in enumerate(text.split(newline)):
        if index in doc_lines and not code[index].strip() and line.lstrip().startswith("///"):
            replacement = expand_line(line)
            changed = changed or replacement != [line]
            result.extend(replacement)
        else:
            result.append(line)
    return newline.join(result) if changed else text


def expand_files(root, paths):
    """Expand the block-level XML tags of the scanned C# files under ``paths``; returns the changed files."""
    changed = []
    for path, _ in index_files(root):
        if not path.endswith(".cs") or (paths and not under(path, paths)):
            continue
        source, bom = load(root / path)
        if is_generated(source, path):
            continue
        expanded = expand_docs(source)
        if expanded != source:
            save(root / path, expanded, bom)
            changed.append(path)
    return changed


def load(path):
    """Return the text of a file and whether it starts with a byte order mark."""
    raw = path.read_bytes()
    return raw.decode("utf-8-sig"), raw.startswith(b"\xef\xbb\xbf")


def save(path, text, bom):
    path.write_bytes((b"\xef\xbb\xbf" if bom else b"") + text.encode("utf-8"))


def insertion_point(code, index):
    """Return the index above which the doc comment of the declaration at ``index`` goes.

    The comment precedes the attribute list of the declaration, so the index is the first line of that list when
    the declaration has one; preprocessor directives around the list belong to it. An index that is itself the first
    line of an attribute belongs to that attribute alone: stacked `[SemanticPart]` lists take one comment each.
    """
    if code[index].strip().startswith("["):
        return index  # an attribute line: alone in a stacked list or shared with the declaration
    position, start = index - 1, index
    while position >= 0:
        text = code[position].strip()
        if not text:
            # A blank line or a comment may stand between two attribute lists of one declaration: the comment
            # belongs above both of them. A region directive or code above ends the declaration's leading trivia.
            ahead = position
            while ahead >= 0 and not code[ahead].strip():
                ahead -= 1
            if ahead < 0:
                break
            upper = code[ahead].strip()
            if upper.startswith(("#region", "#endregion", "#pragma")):
                break  # a region directive or the end of a pragma region belongs to what stands above
            if upper.startswith("[") and not attribute_only(upper):
                break  # the declaration of the member above
            if upper.startswith(("#", "[")) or (upper.endswith("]") and upper.count("]") > upper.count("[")):
                position = ahead
                continue
            break
        if text.startswith("[") and not attribute_only(text):
            break  # an attribute and the declaration of the member above share the line
        if (text.startswith("#") and not text.startswith(("#region", "#endregion", "#pragma"))) \
                or text.startswith("[") \
                or (text.endswith("]") and text.count("]") > text.count("[")):
            # A region directive stays above the comment: the member belongs to the region, the comment to the member.
            start = position
            balance = text.count("[") - text.count("]")
            while balance < 0 and position > 0:  # the attribute list continues on the lines above
                position -= 1
                start = position
                balance += code[position].strip().count("[") - code[position].strip().count("]")
            position -= 1
            continue
        break  # a comment or code: the doc comment goes between it and the declaration
    return start


def comment_body(path, lines):
    """Return True when the body of a patch entry consists of comment lines for the type of ``path``."""
    stripped = [line.strip() for line in lines]
    if path.endswith(".cs"):
        return all(line.startswith("//") for line in stripped)
    if path.endswith(".axaml"):
        joined = " ".join(stripped)
        return joined.startswith("<!--") and joined.endswith("-->") and joined.count("<!--") == joined.count("-->")
    return all(line.startswith(("#", "<!--")) for line in stripped)


def inline_comment(path, text):
    """Return True when ``text`` is one comment that may replace the comment of a line."""
    text = text.strip()
    if path.endswith(".cs"):
        return text.startswith("//") or (text.startswith("/*") and text.endswith("*/") and text.count("/*") == 1)
    return comment_body(path, [text])


def parse_patch(text):
    """Return [(op, path, first, last, lines)] for a patch.

    "@ path:line" inserts comment lines above the declaration that starts at ``line``. "^ path:line" inserts them
    directly above that line. "= path:first[-last]" replaces a range of comment lines; an empty body deletes it.
    "~ path:line" replaces only the comment of that line and keeps the code before it; its body is one line.
    Lines are 1-based. The body of an entry runs to the next blank line or header. A line that starts with "#"
    outside an entry is a note.
    """
    entries, current = [], None
    for number, raw in enumerate(text.splitlines(), 1):
        line = raw.rstrip()
        header = PATCH_HEADER.match(line)
        if header:
            op, path = header.group(1), header.group(2)
            first, last = int(header.group(3)), int(header.group(4) or header.group(3))
            if last < first or (op != "=" and last != first):
                raise ValueError("patch line %d: bad line range" % number)
            if not is_scanned(path):
                raise ValueError("patch line %d: %s is not a scanned source file" % (number, path))
            current = [op, path, first, last, []]
            entries.append(current)
        elif not line:
            current = None  # a blank line ends the entry
        elif current is None:
            if not line.startswith("#"):
                raise ValueError("patch line %d: text outside an entry" % number)
        else:
            # Doc comment lines of a patch get the spacing the standard requires, whatever the patch author wrote.
            current[4].append(space_self_closing(line) if path.endswith(".cs") and line.lstrip().startswith("///")
                              else line)
    for op, path, first, _, lines in entries:
        if op != "=" and not lines:
            raise ValueError("%s %s:%d has no comment lines" % (op, path, first))
        if op == "~" and len(lines) != 1:
            raise ValueError("~ %s:%d takes exactly one comment line" % (path, first))
    return [tuple(entry) for entry in entries]


def apply_patch(root, text):
    """Apply a patch to the working tree and return (changed files, errors).

    Only comment lines are inserted or replaced. A file is changed only when every one of its entries is valid.
    """
    per_file = collections.defaultdict(list)
    for entry in parse_patch(text):
        per_file[entry[1]].append(entry)
    changed, errors = [], []
    for path, entries in sorted(per_file.items()):
        target = root / path
        if not target.is_file():
            errors.append(path + ": file not found")
            continue
        source, bom = load(target)
        newline = "\r\n" if "\r\n" in source else "\n"
        lines, code = source.split(newline), code_lines(path, source)
        if code is None:
            errors.append(path + ": only .cs and .axaml files take a patch")
            continue
        spans = collections.defaultdict(list)
        if path.endswith(".cs"):
            for comment in scan_csharp(source)[2]:
                spans[comment[0]].append(comment)
        operations, partial, problem = [], [], None
        for op, _, first, last, replacement in entries:
            if op == "~":
                found = spans.get(first, [])
                if not path.endswith(".cs") or not found or not inline_comment(path, replacement[0]):
                    problem = "%s:%d has no comment to replace, or the body is not a comment line" % (path, first)
                    break
                marked = [comment for comment in found if HAN.search(comment[3])]
                partial.append((first, marked[0] if len(marked) == 1 else found[-1], replacement[0].strip()))
                continue
            if last > len(lines):
                problem = "%s:%d is beyond the end of the file" % (path, last)
                break
            if op in "@^" and not path.endswith(".cs"):
                problem = "%s: %s needs a .cs file" % (path, op)
                break
            if not comment_body(path, replacement):
                problem = "%s:%d: the body is not made of comment lines" % (path, first)
                break
            point = insertion_point(code, first - 1) if op == "@" else first - 1
            if op == "=" and any(code[index - 1].strip() for index in range(first, last + 1)):
                problem = "%s:%d-%d is not entirely comment lines" % (path, first, last)
                break
            operations.append((point, last if op == "=" else point, replacement))
        if problem:
            errors.append(problem)
            continue
        operations.sort(key=lambda operation: (operation[0], operation[1]))
        if any(a[1] > b[0] or a[0] == b[0] == a[1] for a, b in zip(operations, operations[1:])):
            errors.append(path + ": two entries write the same lines")
            continue
        for number, comment, text in partial:  # they keep the line count, so the line operations stay valid
            at = number - 1
            lines[at] = lines[at][:comment[1]] + text + lines[at][comment[2]:]
        for start, end, replacement in reversed(operations):
            indent = lines[start][:len(lines[start]) - len(lines[start].lstrip())]
            lines[start:end] = [indent + line.lstrip() if line.strip() else "" for line in replacement]
        save(target, newline.join(lines), bom)
        changed.append(path)
    return changed, errors


def parser():
    result = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    result.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2], help="Repository root")
    sub = result.add_subparsers(dest="command", required=True)
    for name, help_text in (
            ("list", "Print every violation with its location"),
            ("check", "Fail on violations under --path, on staged additions, or on counts above the baseline"),
            ("baseline", "Rewrite the baseline from the current tree"),
            ("diff-check", "Fail when changed .cs or .axaml files differ beyond comments"),
            ("expand", "Put every block-level XML tag on its own line and a space before every />"),
            ("apply", "Insert or replace comment lines from a patch file")):
        command = sub.add_parser(name, help=help_text)
        if name in ("list", "check", "diff-check", "expand"):
            command.add_argument("--path", action="append", default=[], help="Limit to a path; repeatable")
            command.add_argument("--direct", action="store_true",
                                 help="Limit to the files directly in the paths, without their subdirectories; a file path selects that file")
        if name == "apply":
            command.add_argument("patch", type=Path, help="The patch file")
        if name == "check":
            command.add_argument("--staged", action="store_true", help="Compare the staged files with HEAD")
        if name == "diff-check":
            command.add_argument("--base", default="HEAD", help="Revision to compare the working tree with")
    return result


def report(findings):
    """Print findings, or only their counts when there are too many to read."""
    if len(findings) <= 40:
        for finding in findings:
            print(format_finding(finding))
    print("comments: %d violations %s" % (len(findings), counts(findings)))


def main(argv=None):
    args = parser().parse_args(argv)
    root, paths = args.root.resolve(), [path.strip("/") for path in getattr(args, "path", [])]
    if args.command == "baseline":
        result = collect(root)
        document = {"version": 1, "scopes": {scope: counts(result[scope]) for scope in sorted(result)}}
        (root / BASELINE).write_text(json.dumps(document, indent=2, sort_keys=True) + "\n")
        print("Wrote " + BASELINE + " with " + str(len(result)) + " scopes")
        return 0
    if args.command == "diff-check":
        errors = diff_errors(root, args.base, paths, getattr(args, "direct", False))
        for error in errors:
            print("BLOCKED: comments: " + error)
        print("comments: " + ("%d source file(s) changed beyond comments" % len(errors) if errors else "only comments changed"))
        return 1 if errors else 0
    if args.command == "expand":
        changed = expand_files(root, paths)
        print("comments: expanded block tags in %d file(s)" % len(changed))
        return 0
    if args.command == "apply":
        changed, errors = apply_patch(root, args.patch.read_text(encoding="utf-8"))
        for error in errors:
            print("BLOCKED: comments: " + error)
        print("comments: %d file(s) changed, %d file(s) skipped" % (len(changed), len(errors)))
        return 1 if errors else 0
    if getattr(args, "staged", False):
        findings = staged_errors(root)
    else:
        result = collect(root, scopes_under(index_files(root), paths) if paths else None)
        findings = [finding for scope in sorted(result) for finding in result[scope]
                    if selected(finding[1], paths, getattr(args, "direct", False))]
    if args.command == "list":
        for finding in findings:
            print(format_finding(finding))
        print("comments: %d violations %s" % (len(findings), counts(findings)), file=sys.stderr)
        return 0
    if getattr(args, "staged", False) or paths:
        report(findings)
        print("comments: " + ("FAIL" if findings else "OK"))
        return 1 if findings else 0
    path = root / BASELINE
    if not path.is_file():
        print("BLOCKED: comments: " + BASELINE + " is missing; run comments.py baseline")
        return 2
    baseline = json.loads(path.read_text()).get("scopes", {})
    errors, lower = [], 0
    for scope in sorted(set(baseline) | set(result)):
        current, allowed = counts(result.get(scope, [])), baseline.get(scope, {})
        errors.extend("comments: %s in %s is %d, baseline %d" % (rule, scope, current[rule], allowed.get(rule, 0))
                      for rule in RULES if current[rule] > allowed.get(rule, 0))
        lower += sum(1 for rule in RULES if current[rule] < allowed.get(rule, 0))
    for error in errors:
        print("BLOCKED: " + error)
    if not errors and lower:
        print("comments: %d rule counts are below the baseline; run comments.py baseline to lower it" % lower)
    print("comments: " + ("FAIL" if errors else "OK"))
    return 1 if errors else 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        print("Comments error: " + str(error), file=sys.stderr)
        sys.exit(2)
