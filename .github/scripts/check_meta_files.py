#!/usr/bin/env python3
"""Verify that every file in the npm package has a matching Unity .meta file
and vice versa.

Unity refuses to import an asset without a .meta file when the package lives
in the immutable package cache (instead of generating one, as it would for a
mutable project asset), so a missing .meta breaks every consumer's project.
An orphan .meta with no matching asset is just dead weight.

Takes the path to a JSON file produced by `npm pack --dry-run --json` (or
`npm pack --json`).
"""

import json
import sys
from os import path

META_EXT = ".meta"


def is_hidden(file_path: str) -> bool:
    """Mirror Unity's AssetDatabase import rules: any path with a
    dot-prefixed segment (e.g. .build/, .swiftpm/, .git/) is invisible to
    Unity, so it's never imported and never needs a .meta file."""
    return any(part.startswith(".") for part in file_path.split("/"))


def main() -> int:
    if len(sys.argv) != 2:
        print(f"usage: {sys.argv[0]} <npm-pack-json-file>", file=sys.stderr)
        return 2

    with open(sys.argv[1], "r") as f:
        packs = json.load(f)

    files = [
        entry["path"]
        for pack in packs
        for entry in pack["files"]
        if not is_hidden(entry["path"])
    ]
    file_set = set(files)
    dir_prefixes = {path.dirname(f) for f in files if path.dirname(f)}

    errors = []
    for f in files:
        if f.endswith(META_EXT):
            continue
        meta = f + META_EXT
        if meta not in file_set:
            errors.append(f"missing .meta file: {meta} (asset {f} has none)")

    for f in files:
        if not f.endswith(META_EXT):
            continue
        asset = f[: -len(META_EXT)]
        if asset in file_set or asset in dir_prefixes:
            continue
        errors.append(f"orphan .meta file: {f} (no matching asset {asset})")

    if errors:
        print(f"Found {len(errors)} packaging problem(s):", file=sys.stderr)
        for e in sorted(errors):
            print(f"  - {e}", file=sys.stderr)
        return 1

    print(f"OK: all {len(files)} packaged files have matching .meta pairs.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
