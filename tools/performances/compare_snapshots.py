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
    diff = ImageChops.difference(a, b).convert('L')
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
    parser.add_argument('--near', type=int, default=6,
                        help='pixels within this distance of an unstable pixel are reported as near-animation')
    parser.add_argument('--ignore-near-animation', action='store_true',
                        help='changed pixels that are all near-animation are reported but do not fail; '
                             'changed pixels farther away still fail')
    args = parser.parse_args()

    names = sorted(set(os.listdir(args.before_a)) | set(os.listdir(args.after_a)))
    names = [name for name in names if name.endswith('.png')]
    failures = 0
    print(f"{'page':18}{'size':>14}{'unstable px':>13}{'changed px':>12}{'away px':>9}{'max diff':>10}  result")
    for name in names:
        page = name[:-4]
        images = [load(directory, name) for directory in (args.before_a, args.before_b, args.after_a, args.after_b)]
        if any(image is None for image in images):
            failures += 1
            print(f"{page:18}{'':>14}{'':>13}{'':>12}{'':>10}  MISSING in {[d for d, i in zip('ABCD', images) if i is None]}")
            continue

        before_a, before_b, after_a, after_b = images
        if len({image.size for image in images}) != 1:
            sizes = {image.size for image in images}
            stable_sizes = before_a.size == before_b.size and after_a.size == after_b.size
            failures += 1 if stable_sizes else 0
            verdict = 'LAYOUT CHANGED' if stable_sizes else 'unstable size within a build'
            print(f"{page:18}{str(sizes):>14}{'':>13}{'':>12}{'':>10}  {verdict}")
            continue

        unstable = ImageChops.lighter(nonzero_mask(before_a, before_b), nonzero_mask(after_a, after_b))
        changed = ImageChops.subtract(nonzero_mask(before_a, after_a), unstable)
        changed_count = count(changed)
        # Changed pixels far from anything that flickers within one build cannot be explained by animation timing.
        near = unstable.filter(ImageFilter.MaxFilter(2 * args.near + 1)) if args.near > 0 else unstable
        away_count = count(ImageChops.subtract(changed, near))
        max_diff = 0
        ignored = args.ignore_near_animation and away_count == 0
        if changed_count:
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
