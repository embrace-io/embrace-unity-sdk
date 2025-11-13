#!/usr/bin/env python3
"""Patch Gradle wrapper in Unity-generated Android projects."""

import argparse
import os
import sys
from pathlib import Path


def patch_gradle_wrapper(project_path: str, gradle_version: str = "8.14.1"):
    """Patch the gradle-wrapper.properties file to use a specific Gradle version."""
    wrapper_props_path = Path(project_path) / "Library/Bee/Android/Prj/IL2CPP/Gradle/gradle/wrapper/gradle-wrapper.properties"

    if not wrapper_props_path.exists():
        print(f"Gradle wrapper not found at {wrapper_props_path}")
        return False

    print(f"Patching Gradle wrapper at {wrapper_props_path}")

    # Read the existing properties
    with open(wrapper_props_path, "r") as f:
        lines = f.readlines()

    # Replace the distributionUrl line
    new_lines = []
    for line in lines:
        if line.startswith("distributionUrl="):
            new_lines.append(f"distributionUrl=https\\://services.gradle.org/distributions/gradle-{gradle_version}-all.zip\n")
            print(f"Updated distributionUrl to Gradle {gradle_version}")
        else:
            new_lines.append(line)

    # Write back
    with open(wrapper_props_path, "w") as f:
        f.writelines(new_lines)

    print(f"Gradle wrapper patched successfully")
    return True


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Patch Gradle wrapper in Unity Android projects"
    )
    parser.add_argument(
        "--project-path",
        required=True,
        help="Path to Unity project directory"
    )
    parser.add_argument(
        "--gradle-version",
        default="8.14.1",
        help="Gradle version to use (default: 8.14.1)"
    )
    args = parser.parse_args()

    success = patch_gradle_wrapper(args.project_path, args.gradle_version)
    sys.exit(0 if success else 1)
