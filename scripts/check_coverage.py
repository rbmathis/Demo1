#!/usr/bin/env python3
"""Validate targeted code coverage from a Cobertura report."""

from __future__ import annotations

import argparse
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "report",
        type=Path,
        help="Path to Cobertura XML report",
    )
    parser.add_argument(
        "threshold",
        type=float,
        help="Minimum coverage percentage required",
    )
    parser.add_argument(
        "segments",
        nargs="*",
        default=[
            "/source/demo1/controllers/",
            "/source/demo1/middleware/",
            "/source/demo1/telemetry/",
        ],
        help="Case-insensitive path fragments that identify files to include",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    if not args.report.exists():
        print(f"::error::Coverage report '{args.report}' not found", file=sys.stderr)
        return 1

    tree = ET.parse(args.report)
    segments = tuple(fragment.lower() for fragment in args.segments)
    total = 0
    covered = 0

    for cls in tree.iterfind(".//class"):
        filename = cls.attrib.get("filename", "").replace("\\", "/").lower()
        if not any(fragment in filename for fragment in segments):
            continue

        for line in cls.iterfind(".//line"):
            total += 1
            if int(line.attrib.get("hits", "0")) > 0:
                covered += 1

    if total == 0:
        print("::error::No matching files found while calculating coverage", file=sys.stderr)
        return 1

    coverage = (covered / total) * 100.0
    print(f"Unit test coverage (targeted): {coverage:.2f}% ({covered}/{total})")

    if coverage < args.threshold:
        print(
            f"::error::Unit test coverage {coverage:.2f}% is below required {args.threshold:.2f}%",
            file=sys.stderr,
        )
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
