#!/usr/bin/env python3
"""Helper to get variables for use by make or GitHub workflows."""

import argparse
import json
import re
import sys
from os import path
from typing import Literal, TypedDict

version_re = re.compile(r"m_EditorVersionWithRevision: ([^ ]+) \(([^\)]+)\)")


def get_platform() -> Literal["darwin", "linux", "win32"]:
    """Get a string representing the current platform."""
    if sys.platform == "darwin":
        return "darwin"
    elif sys.platform == "win32":
        return "win32"
    else:
        return "linux"


class ProjectEditorVersion(TypedDict):
    """The version and changeset used for a Unity project."""

    project: str
    version: str
    changeset: str


def get_editor_version_with_changeset(project_path: str) -> tuple[str, str]:
    """Get the version and changeset of the Unity editor used in a project."""
    version_filename = path.join(project_path, "ProjectSettings", "ProjectVersion.txt")
    with open(version_filename, "r") as f:
        for line in f:
            line = line.strip()
            match = version_re.match(line)
            if match is not None:
                return match.group(1), match.group(2)
    raise ValueError(f"Unable to find version in {version_filename!r}")


def get_editor_versions(projects: list[str]) -> list[ProjectEditorVersion]:
    """Get the version and changeset of the Unity editor used in a list of projects."""
    results: list[ProjectEditorVersion] = []
    base_path = path.abspath(path.join(path.dirname(__file__), "..", ".."))
    for project in projects:
        project_path = path.join(base_path, "UnityProjects", project)
        version, changeset = get_editor_version_with_changeset(project_path)
        results.append({"project": project, "version": version, "changeset": changeset})
    return results


def main() -> None:
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(dest="command", required=True)
    _platform_parser = subparsers.add_parser("platform")
    _versions_parser = subparsers.add_parser("versions")
    args = parser.parse_args(sys.argv[1:])
    if args.command == "platform":
        print(get_platform())
    elif args.command == "versions":
        print(json.dumps(get_editor_versions(["2021", "2022"])))
    else:
        raise ValueError(f"Invalid command: {args.command}")


if __name__ == "__main__":
    main()
