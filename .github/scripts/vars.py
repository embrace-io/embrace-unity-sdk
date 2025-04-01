#!/usr/bin/env python3
"""Helper to get variables for use by make or GitHub workflows."""

import argparse
import json
import re
import sys
from os import path
from typing import Literal, TypedDict

base_path = path.abspath(path.join(path.dirname(__file__), "..", ".."))
version_re = re.compile(r"m_EditorVersionWithRevision: ([^ ]+) \(([^\)]+)\)")


class ProjectEditorVersion(TypedDict):
    """The version and changeset used for a Unity project."""

    project: str
    version: str
    changeset: str


def get_apple_sdk_version() -> str:
    filename = path.join(
        base_path, "io.embrace.sdk", "iOS", "EmbraceUnityiOS", "Package.resolved"
    )
    with open(filename, "r") as f:
        data = json.load(f)
        for pin in data["pins"]:
            if pin["identity"] == "embrace-apple-sdk":
                return pin["state"]["version"]
    raise ValueError("Unable to find embrace-apple-sdk version")


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
    for project in projects:
        project_path = path.join(base_path, "UnityProjects", project)
        version, changeset = get_editor_version_with_changeset(project_path)
        results.append({"project": project, "version": version, "changeset": changeset})
    return results


def get_platform() -> Literal["darwin", "linux", "win32"]:
    """Get a string representing the current platform."""
    if sys.platform == "darwin":
        return "darwin"
    elif sys.platform == "win32":
        return "win32"
    else:
        return "linux"


def get_sdk_version() -> str:
    """Get the SDK version to use for the .unitypackage."""
    with open(path.join(base_path, "io.embrace.sdk", "package.json"), "r") as f:
        return json.load(f)["version"]


def main() -> None:
    parser = argparse.ArgumentParser()
    subparsers = parser.add_subparsers(dest="command", required=True)

    _apple_sdk_version_parser = subparsers.add_parser("apple-sdk-version")

    editor_versions_parser = subparsers.add_parser("editor-versions")
    editor_versions_parser.add_argument(
        "--project",
        type=str,
        action="append",
        help="If specified, override the project to get the version for.",
    )
    editor_versions_parser.add_argument(
        "--field",
        type=str,
        choices=["project", "version", "changeset"],
        default=None,
        help="If specified, only output this field (as text).",
    )

    _sdk_version_parser = subparsers.add_parser("sdk-version")

    _platform_parser = subparsers.add_parser("platform")

    args = parser.parse_args(sys.argv[1:])
    if args.command == "apple-sdk-version":
        print(get_apple_sdk_version())
    elif args.command == "editor-versions":
        versions = get_editor_versions(args.project or ["2021", "2022"])
        if args.field:
            field: Literal["project", "version", "changeset"] = args.field
            print("\n".join(v[field] for v in versions))
        else:
            print(json.dumps(versions))
    elif args.command == "platform":
        print(get_platform())
    elif args.command == "sdk-version":
        print(get_sdk_version())
    else:
        raise ValueError(f"Invalid command: {args.command}")


if __name__ == "__main__":
    main()
