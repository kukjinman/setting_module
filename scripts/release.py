#!/usr/bin/env python3
"""Keep repository/package versions aligned and create immutable release tags."""

from __future__ import annotations

import json
import re
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
VERSION_FILE = ROOT / "VERSION"
CHANGELOG_FILES = (
    ROOT / "CHANGELOG.md",
    ROOT / "Assets/SharedCoreModule/CHANGELOG.md",
    ROOT / "Assets/SharedAppFlowModule/CHANGELOG.md",
)
PACKAGE_FILES = (
    ROOT / "package.json",
    ROOT / "Assets/SharedCoreModule/package.json",
    ROOT / "Assets/SharedAppFlowModule/package.json",
)
APP_FLOW_PACKAGE = ROOT / "Assets/SharedAppFlowModule/package.json"
VERSIONED_DOCUMENTATION = (
    ROOT / "README.md",
    ROOT / "VERSIONING.md",
)
SEMVER_PATTERN = re.compile(
    r"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)"
    r"(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?"
    r"(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?$"
)


class ReleaseError(RuntimeError):
    pass


def run_git(*args: str) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=ROOT,
        check=False,
        capture_output=True,
        text=True,
    )
    if result.returncode != 0:
        raise ReleaseError(result.stderr.strip() or "git command failed")
    return result.stdout.strip()


def validate_semver(version: str) -> str:
    if not SEMVER_PATTERN.fullmatch(version):
        raise ReleaseError(f"Invalid semantic version: {version}")
    return version


def current_version() -> str:
    if not VERSION_FILE.exists():
        raise ReleaseError("VERSION file is missing")
    return validate_semver(VERSION_FILE.read_text(encoding="utf-8").strip())


def load_json(path: Path) -> dict:
    with path.open(encoding="utf-8") as stream:
        return json.load(stream)


def write_json(path: Path, data: dict) -> None:
    path.write_text(
        json.dumps(data, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


def ensure_clean_worktree() -> None:
    if run_git("status", "--porcelain"):
        raise ReleaseError("Working tree must be clean before this command")


def check_versions() -> str:
    version = current_version()
    problems: list[str] = []

    for path in PACKAGE_FILES:
        package_version = str(load_json(path).get("version", ""))
        if package_version != version:
            problems.append(
                f"{path.relative_to(ROOT)} has version {package_version!r}, expected {version!r}"
            )

    app_flow = load_json(APP_FLOW_PACKAGE)
    core_dependency = str(
        app_flow.get("dependencies", {}).get("com.sharedmodules.core-module", "")
    )
    if core_dependency != version:
        problems.append(
            "Assets/SharedAppFlowModule/package.json requires core version "
            f"{core_dependency!r}, expected {version!r}"
        )

    changelog_header = f"## [{version}] - "
    for path in CHANGELOG_FILES:
        changelog = path.read_text(encoding="utf-8") if path.exists() else ""
        if changelog_header not in changelog:
            problems.append(
                f"{path.relative_to(ROOT)} has no release entry for {version}"
            )

    expected_tag_reference = f"#v{version}"
    for path in VERSIONED_DOCUMENTATION:
        contents = path.read_text(encoding="utf-8") if path.exists() else ""
        if expected_tag_reference not in contents:
            problems.append(
                f"{path.relative_to(ROOT)} has no install example for v{version}"
            )

    if problems:
        raise ReleaseError("\n".join(problems))

    print(f"Version {version} is consistent across repository metadata.")
    return version


def set_version(version: str) -> None:
    ensure_clean_worktree()
    version = validate_semver(version)
    previous_version = current_version()
    VERSION_FILE.write_text(version + "\n", encoding="utf-8")

    for path in PACKAGE_FILES:
        data = load_json(path)
        data["version"] = version
        if path == APP_FLOW_PACKAGE:
            data.setdefault("dependencies", {})[
                "com.sharedmodules.core-module"
            ] = version
        write_json(path, data)

    for path in VERSIONED_DOCUMENTATION:
        contents = path.read_text(encoding="utf-8")
        contents = contents.replace(f"#v{previous_version}", f"#v{version}")
        path.write_text(contents, encoding="utf-8")

    print(f"Updated repository and package versions to {version}.")
    print(
        f"Next: add '## [{version}] - YYYY-MM-DD' to the root and package "
        "CHANGELOG files, then commit."
    )


def create_tag() -> None:
    ensure_clean_worktree()
    version = check_versions()
    tag = f"v{version}"

    existing_tags = run_git("tag", "--list", tag)
    if existing_tags:
        raise ReleaseError(f"Tag {tag} already exists; release tags are immutable")

    run_git("tag", "-a", tag, "-m", f"Release {tag}")
    print(f"Created annotated tag {tag} at {run_git('rev-parse', '--short', 'HEAD')}.")
    print(f"Push it with: git push origin {tag}")


def usage() -> None:
    print("Usage:")
    print("  python3 scripts/release.py check")
    print("  python3 scripts/release.py set <semver>")
    print("  python3 scripts/release.py tag")


def main() -> int:
    try:
        if len(sys.argv) == 2 and sys.argv[1] == "check":
            check_versions()
        elif len(sys.argv) == 3 and sys.argv[1] == "set":
            set_version(sys.argv[2])
        elif len(sys.argv) == 2 and sys.argv[1] == "tag":
            create_tag()
        else:
            usage()
            return 2
    except (OSError, json.JSONDecodeError, ReleaseError) as error:
        print(f"release error: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
