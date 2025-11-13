#!/usr/bin/env python3
"""Patch gradleTemplate.properties files to set Java home."""

import argparse
import os
import sys
from pathlib import Path


def patch_gradle_properties(project_path: str, jdk_path: str):
    """Add org.gradle.java.home to gradleTemplate.properties."""
    gradle_props_path = Path(project_path) / "Assets/Plugins/Android/gradleTemplate.properties"

    if not gradle_props_path.exists():
        print(f"Warning: gradleTemplate.properties not found at {gradle_props_path}")
        return False

    print(f"Patching {gradle_props_path}")

    # Read existing content
    with open(gradle_props_path, "r") as f:
        content = f.read()

    # Check if already configured
    if "org.gradle.java.home" in content:
        print(f"  Already contains org.gradle.java.home, skipping")
        return True

    # Add the property at the end
    with open(gradle_props_path, "a") as f:
        f.write(f"\n# Added by CI to use Java 17\n")
        f.write(f"org.gradle.java.home={jdk_path}\n")

    print(f"  Added org.gradle.java.home={jdk_path}")
    return True


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Patch gradleTemplate.properties files with JDK path"
    )
    parser.add_argument(
        "--jdk-path",
        required=True,
        help="Path to JDK installation"
    )
    args = parser.parse_args()

    # Find all Unity projects
    base_path = Path(__file__).parent.parent.parent
    unity_projects_path = base_path / "UnityProjects"

    if not unity_projects_path.exists():
        print(f"Error: UnityProjects directory not found at {unity_projects_path}")
        sys.exit(1)

    success_count = 0
    for project_dir in unity_projects_path.iterdir():
        if project_dir.is_dir():
            print(f"\nProcessing {project_dir.name}...")
            if patch_gradle_properties(str(project_dir), args.jdk_path):
                success_count += 1

    print(f"\nPatched {success_count} project(s)")
    sys.exit(0)
