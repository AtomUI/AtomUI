#!/usr/bin/env python3
"""Compare the token dumps of a baseline build and a candidate build.

Each side is written by `AtomUI.GalleryPerformance --dump-tokens <dir>`: one `*.tokens.txt` file per design-language
variant and seed case, one `<key> = <value>` line per global token and per control own token.

usage: compare_token_dumps.py BASE_DIR CANDIDATE_DIR [--renames FILE] [--exceptions FILE] [--allow-added]

  --renames FILE     one `old key -> new key` per line; a baseline key is compared under its new name
  --exceptions FILE  one key per line; differences on these keys are skipped and listed at the end
  --allow-added      keys only in the candidate are listed but do not fail (new tokens of the work package)

Lines starting with `#` and blank lines in both files are ignored.
exit code: 0 when nothing fails, 1 otherwise.
"""
import argparse
import os
import sys


def read_lines(path):
    if not path:
        return []
    with open(path, encoding='utf-8') as handle:
        return [line.strip() for line in handle if line.strip() and not line.lstrip().startswith('#')]


def read_renames(path):
    renames = {}
    for line in read_lines(path):
        old, separator, new = line.partition(' -> ')
        if not separator:
            raise SystemExit(f'not a rename line: {line!r}')
        renames[old] = new
    return renames


def parse_dump(path):
    entries = {}
    with open(path, encoding='utf-8') as handle:
        for line in handle:
            line = line.rstrip('\n')
            if not line:
                continue
            key, separator, value = line.partition(' = ')
            if not separator:
                raise SystemExit(f'{path}: not a dump line: {line!r}')
            if key in entries:
                raise SystemExit(f'{path}: duplicate key: {key!r}')
            entries[key] = value
    if not entries:
        raise SystemExit(f'{path}: no token values')
    return entries


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('base_dir')
    parser.add_argument('candidate_dir')
    parser.add_argument('--renames')
    parser.add_argument('--exceptions')
    parser.add_argument('--allow-added', action='store_true')
    args = parser.parse_args()

    renames = read_renames(args.renames)
    exceptions = set(read_lines(args.exceptions))

    def dumps(directory):
        return {name for name in os.listdir(directory) if name.endswith('.tokens.txt')}

    base_files, candidate_files = dumps(args.base_dir), dumps(args.candidate_dir)
    if not base_files or not candidate_files:
        print('Both builds must provide token dumps.', file=sys.stderr)
        return 1
    failures = 0
    counts = {'only-base-file': 0, 'only-candidate-file': 0, 'only-base': 0, 'only-candidate': 0, 'changed': 0}
    skipped = set()

    for name in sorted(base_files | candidate_files):
        if name not in candidate_files or name not in base_files:
            kind = 'only-base-file' if name in base_files else 'only-candidate-file'
            counts[kind] += 1
            failures += 1
            print(f'{name}: file only in {"baseline" if name in base_files else "candidate"}')
            continue

        before = {renames.get(key, key): value for key, value in parse_dump(os.path.join(args.base_dir, name)).items()}
        after = parse_dump(os.path.join(args.candidate_dir, name))
        for key in sorted(before.keys() | after.keys()):
            if key in before and key in after and before[key] == after[key]:
                continue
            if key in exceptions:
                skipped.add(key)
                continue
            if key not in after:
                counts['only-base'] += 1
                failures += 1
                print(f'{name}: only in baseline   {key} = {before[key]}')
            elif key not in before:
                counts['only-candidate'] += 1
                if not args.allow_added:
                    failures += 1
                print(f'{name}: only in candidate  {key} = {after[key]}' + ('  (allowed)' if args.allow_added else ''))
            else:
                counts['changed'] += 1
                failures += 1
                print(f'{name}: changed  {key}: {before[key]} -> {after[key]}')

    for key in sorted(skipped):
        print(f'exception skipped: {key}')
    compared = len(base_files & candidate_files)
    print(f'files compared: {compared}, files only in baseline: {counts["only-base-file"]}, '
          f'only in candidate: {counts["only-candidate-file"]}; keys only in baseline: {counts["only-base"]}, '
          f'only in candidate: {counts["only-candidate"]}, changed: {counts["changed"]}; failing: {failures}')
    return 1 if failures else 0


if __name__ == '__main__':
    sys.exit(main())
