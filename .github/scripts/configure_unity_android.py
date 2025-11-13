#!/usr/bin/env python3
"""Configure Unity Android build settings for CI."""

import argparse
import os
import sys
from pathlib import Path


def configure_unity_prefs(editor_version: str, gradle_path: str, jdk_path: str):
    """Configure Unity preferences for Android builds."""
    if sys.platform == "darwin":
        prefs_path = Path.home() / "Library/Preferences/com.unity3d.UnityEditor5.x.plist"
        print(f"Configuring Unity preferences for macOS at {prefs_path}")
        # On macOS, Unity uses a binary plist format
        # Use plutil to add the preferences
        import subprocess

        # Set JDK path
        subprocess.run([
            "defaults", "write",
            "com.unity3d.UnityEditor5.x",
            "JdkUseEmbedded_h4019811976",
            "-bool", "false"
        ], check=True)
        subprocess.run([
            "defaults", "write",
            "com.unity3d.UnityEditor5.x",
            "JdkPath_h4019811976",
            "-string", jdk_path
        ], check=True)

        # Set Gradle path
        subprocess.run([
            "defaults", "write",
            "com.unity3d.UnityEditor5.x",
            "GradleUseEmbedded_h4019811976",
            "-bool", "false"
        ], check=True)
        subprocess.run([
            "defaults", "write",
            "com.unity3d.UnityEditor5.x",
            "GradlePath_h4019811976",
            "-string", gradle_path
        ], check=True)

        print(f"Configured Unity to use JDK at: {jdk_path}")
        print(f"Configured Unity to use Gradle at: {gradle_path}")

    elif sys.platform == "linux":
        prefs_path = Path.home() / ".config/unity3d/prefs"
        print(f"Configuring Unity preferences for Linux at {prefs_path}")

        # Unity on Linux uses a plain text prefs file
        # The hash h4019811976 is consistent across Unity installations
        prefs_content = f"""JdkUseEmbedded_h4019811976: 0
JdkPath_h4019811976: {jdk_path}
GradleUseEmbedded_h4019811976: 0
GradlePath_h4019811976: {gradle_path}
"""
        os.makedirs(prefs_path.parent, exist_ok=True)

        # Append to existing prefs file if it exists, or create new
        with open(prefs_path, 'a') as f:
            f.write(prefs_content)

        print(f"Configured Unity to use JDK at: {jdk_path}")
        print(f"Configured Unity to use Gradle at: {gradle_path}")

    else:
        raise NotImplementedError(f"Platform {sys.platform} not supported")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        description="Configure Unity Android build settings for CI"
    )
    parser.add_argument(
        "--editor-version",
        required=True,
        help="Unity editor version (e.g., 2021.3.45f2)"
    )
    parser.add_argument(
        "--gradle-path",
        required=True,
        help="Path to Gradle installation directory"
    )
    parser.add_argument(
        "--jdk-path",
        required=True,
        help="Path to JDK installation directory"
    )
    args = parser.parse_args()

    configure_unity_prefs(args.editor_version, args.gradle_path, args.jdk_path)
