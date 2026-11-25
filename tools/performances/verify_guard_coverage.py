#!/usr/bin/env python3
"""Verify that snapshot and leak reports cover the same controls and pages on both builds."""

import argparse
import json
from pathlib import Path
import re
import sys


def state_catalog(directory, expected_failures):
    inventories = []
    for capture in ('before-a', 'after-a', 'before-b', 'after-b'):
        log = (directory / f'states-{capture}.log').read_text()
        totals = re.findall(r'^state snapshots: types=(\d+) failed=(\d+)$', log, re.MULTILINE)
        failures = set(re.findall(r'^(\S+): FAILED ', log, re.MULTILINE))
        images = {path.stem for path in (directory / f'states-{capture}').glob('*.png')}
        if (len(totals) != 1 or not images or images & failures
                or tuple(map(int, totals[0])) != (len(images) + len(failures), len(failures))):
            raise ValueError(f'{capture}: incomplete control state snapshots')
        inventories.append((images, failures))
    images, failures = inventories[0]
    if any(inventory != inventories[0] for inventory in inventories[1:]):
        raise ValueError('Control state inventories differ between captures')
    if expected_failures is not None and failures != expected_failures:
        raise ValueError(f'Unexpected state capture failures: {", ".join(sorted(failures)) or "none"}')
    return images | failures


def page_leaks(path):
    log = path.read_text()
    totals = re.findall(r'^Pages with surviving instances: \d+/(\d+)$', log, re.MULTILINE)
    rows = re.findall(r'^(\S+): visits=(\d+) alive=(\d+)$', log, re.MULTILINE)
    pages = {name: (int(visits), int(alive)) for name, visits, alive in rows}
    if len(totals) != 1 or not pages or int(totals[0]) != len(pages) or len(rows) != len(pages):
        raise ValueError(f'{path.name}: incomplete page leak measurements')
    return pages


def control_leaks(path, catalog):
    rows = json.loads(path.read_text())
    controls = {row['type']: row for row in rows}
    if controls.keys() != catalog or len(controls) != len(rows):
        raise ValueError(f'{path.name}: incomplete control leak measurements')
    return controls


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    parser.add_argument('--expected-state-failures')
    args = parser.parse_args()
    directory = args.directory
    expected_failures = (set(filter(None, args.expected_state_failures.split(',')))
                         if args.expected_state_failures is not None else None)
    try:
        catalog = state_catalog(directory, expected_failures)
        before_pages, after_pages = (page_leaks(directory / f'leak-pages-{side}.txt')
                                     for side in ('before', 'after'))
        if {name: visits for name, (visits, _) in before_pages.items()} != {
                name: visits for name, (visits, _) in after_pages.items()}:
            raise ValueError('Page leak measurements cover different visits')
        problems = [f'pages:{name} {before_pages[name][1]} -> {alive}'
                    for name, (_, alive) in after_pages.items() if alive > before_pages[name][1]]

        before, after = (control_leaks(directory / f'leak-controls-{side}.json', catalog)
                         for side in ('before', 'after'))
        for name, result in after.items():
            original = before[name]
            if 'error' in result:
                if result.get('error') != original.get('error'):
                    problems.append(f'controls:{name} new measurement error ({result["error"]})')
            elif result['alive'] > original.get('alive', 0):
                problems.append(f'controls:{name} {original.get("alive", 0)} -> {result["alive"]}')
        print('[leak] new or larger leaks and measurement errors: ' + (', '.join(problems) or 'none'))
        return 1 if problems else 0
    except (OSError, ValueError, KeyError) as error:
        print(f'[coverage] {error}', file=sys.stderr)
        return 1


if __name__ == '__main__':
    sys.exit(main())
