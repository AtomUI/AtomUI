#!/usr/bin/env python3
"""Compare Gallery page snapshots of an unoptimized and an optimized build.

Each side is captured twice by `AtomUI.GalleryPerformance --audit --audit-full --snapshot-dir <dir>`.
Pixels that differ between the two captures of the same build (running animations, clocks) form the
unstable mask; every other pixel must be identical between the builds. A page whose size differs is a
layout change and is reported as such.

usage: compare_snapshots.py BEFORE_A BEFORE_B AFTER_A AFTER_B [--diff-dir DIR]
exit code: 0 when no stable pixel differs, 1 otherwise.
"""
import argparse
import os
import sys

from PIL import Image, ImageChops, ImageFilter


def load(directory, name):
    path = os.path.join(directory, name)
    return Image.open(path).convert('RGB') if os.path.exists(path) else None


def nonzero_mask(a, b):
    # 255 where any channel differs, 0 elsewhere.
    red, green, blue = ImageChops.difference(a, b).split()
    diff = ImageChops.lighter(ImageChops.lighter(red, green), blue)
    return diff.point(lambda value: 255 if value else 0)


def count(mask):
    return mask.histogram()[255]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('before_a')
    parser.add_argument('before_b')
    parser.add_argument('after_a')
    parser.add_argument('after_b')
    parser.add_argument('--diff-dir')
    parser.add_argument('--before-repeat', action='append', default=[], help='extra baseline frames for selected pages')
    parser.add_argument('--after-repeat', action='append', default=[], help='extra candidate frames for selected pages')
    parser.add_argument('--repeat-pages', help='comma-separated pages required in all extra capture directories')
    parser.add_argument('--near', type=int, default=6,
                        help='pixels within this distance of an unstable pixel are reported as near-animation')
    parser.add_argument('--ignore-near-animation', action='store_true',
                        help='changed pixels that are all near-animation are reported but do not fail; '
                             'changed pixels farther away still fail')
    args = parser.parse_args()

    directories = (args.before_a, args.before_b, args.after_a, args.after_b)
    names = sorted({name for directory in directories for name in os.listdir(directory) if name.endswith('.png')})
    if args.before_repeat or args.after_repeat or args.repeat_pages:
        if not args.repeat_pages or len(args.before_repeat) != 2 or len(args.after_repeat) != 2:
            parser.error('repeated pages require two extra capture directories per build')
        requested = {key + '.png' for key in args.repeat_pages.split(',')}
        if not requested <= set(names):
            parser.error('repeated pages must be present in the initial captures')
        for directory in (*args.before_repeat, *args.after_repeat):
            actual = {name for name in os.listdir(directory) if name.endswith('.png')}
            if actual != requested:
                print(f'{directory}: expected repeat snapshots {sorted(requested)}, found {sorted(actual)}',
                      file=sys.stderr)
                return 1
    if not names:
        print('No PNG snapshots to compare.', file=sys.stderr)
        return 1

    failures = 0
    print(f"{'page':18}{'size':>14}{'unstable px':>13}{'changed px':>12}{'away px':>9}{'max diff':>10}  result")
    for name in names:
        page = name[:-4]
        images = [load(directory, name) for directory in directories]
        if any(image is None for image in images):
            failures += 1
            print(f"{page:18}{'':>14}{'':>13}{'':>12}{'':>10}  MISSING in {[d for d, i in zip('ABCD', images) if i is None]}")
            continue

        before_a, before_b, after_a, after_b = images
        before_repeats = [frame for directory in args.before_repeat if (frame := load(directory, name)) is not None]
        after_repeats = [frame for directory in args.after_repeat if (frame := load(directory, name)) is not None]
        before_sizes = {frame.size for frame in (before_a, before_b, *before_repeats)}
        after_sizes = {frame.size for frame in (after_a, after_b, *after_repeats)}
        sizes = before_sizes | after_sizes
        if len(sizes) != 1:
            stable_sizes = len(before_sizes) == 1 and len(after_sizes) == 1
            failures += 1
            verdict = 'LAYOUT CHANGED' if stable_sizes else 'unstable size within a build'
            print(f"{page:18}{str(sizes):>14}{'':>13}{'':>12}{'':>10}  {verdict}")
            continue

        unstable = ImageChops.lighter(nonzero_mask(before_a, before_b), nonzero_mask(after_a, after_b))
        for first, repeats in ((before_a, before_repeats), (after_a, after_repeats)):
            for repeat in repeats:
                unstable = ImageChops.lighter(unstable, nonzero_mask(first, repeat))
        changed = ImageChops.subtract(nonzero_mask(before_a, after_a), unstable)
        changed_count = count(changed)
        away_count = max_diff = 0
        ignored = False
        if changed_count:
            # Changed pixels far from anything that flickers within one build cannot be explained by animation timing.
            near = unstable.filter(ImageFilter.MaxFilter(2 * args.near + 1)) if args.near > 0 else unstable
            away_count = count(ImageChops.subtract(changed, near))
            ignored = args.ignore_near_animation and away_count == 0
            delta = ImageChops.difference(before_a, after_a)
            masked = Image.composite(delta, Image.new('RGB', delta.size), changed)
            max_diff = max(high for _, high in masked.getextrema())
            if not ignored:
                failures += 1
            if args.diff_dir:
                os.makedirs(args.diff_dir, exist_ok=True)
                highlight = before_a.copy()
                highlight.paste((255, 0, 255), mask=changed)
                highlight.save(os.path.join(args.diff_dir, page + '.diff.png'))
        size = f"{before_a.size[0]}x{before_a.size[1]}"
        if changed_count == 0:
            verdict = 'identical'
        elif away_count == 0:
            verdict = ('next to animated pixels only (ignored)' if ignored
                       else 'VISUAL CHANGE (all next to animated pixels)')
        else:
            verdict = 'VISUAL CHANGE'
        print(f"{page:18}{size:>14}{count(unstable):>13}{changed_count:>12}{away_count:>9}{max_diff:>10}  {verdict}")

    print(f"pages: {len(names)}, failing: {failures}")
    return 1 if failures else 0


if __name__ == '__main__':
    sys.exit(main())
