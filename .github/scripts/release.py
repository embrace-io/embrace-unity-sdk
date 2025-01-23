#!/usr/bin/env python3

import argparse
import json
import logging
import os
import re
import sys
from os import path
from subprocess import check_output, run
from typing import get_args, Literal, TypeGuard


logger = logging.getLogger("release")

RefType = Literal["branch", "tag"]
ref_name_version_re: dict[RefType, re.Pattern[str]] = {
    "branch": re.compile(r"^release/v(\d+\.\d+\.\d+)$"),
    "tag": re.compile(r"^v(\d+\.\d+\.\d+(-rc\d+))?$"),
}
release_candidate_re = re.compile(r"^v\d+\.\d+\.\d+-rc(\d+)$")

root_dir = path.abspath(path.join(path.dirname(__file__), "..", ".."))
version_filenames = [
    path.join(root_dir, "io.embrace.sdk", "Resources", "Info", "EmbraceSdkInfo.json"),
    path.join(root_dir, "io.embrace.sdk", "package.json"),
]


class InvalidRefTypeError(ValueError):
    def __init__(self, ref_type: str | None) -> None:
        super().__init__(
            f"GITHUB_REF_TYPE is {ref_type!r}, but expected {get_args(RefType)!r}"
        )


class InvalidRefNameError(ValueError):
    def __init__(self, ref_name: str | None, pattern: str) -> None:
        super().__init__(
            f"GITHUB_REF_NAME is {ref_name!r}, but expected pattern {pattern}"
        )


def commit_and_push(ref_name: str, version: str) -> str:
    """Commit the changes and push. Return the commit SHA."""
    run(["git", "--no-pager", "diff", "--color", "--staged"], check=True)
    process = run(["git", "diff", "--staged", "--quiet", "--exit-code"])
    if process.returncode != 0:
        commit_user_name = os.getenv("COMMIT_USER_NAME")
        if not commit_user_name:
            raise ValueError("COMMIT_USER_NAME is not set")
        commit_user_email = os.getenv("COMMIT_USER_EMAIL")
        if not commit_user_email:
            raise ValueError("COMMIT_USER_EMAIL is not set")
        run(["git", "config", "--global", "user.name", commit_user_name], check=True)
        run(["git", "config", "--global", "user.email", commit_user_email], check=True)
        run(["git", "commit", "-m", f"CI/CD: Bump version to {version}"], check=True)
        run(["git", "push", "origin", ref_name], check=True)
    else:
        logger.info("No changes to commit")
    return check_output(["git", "rev-parse", "HEAD"], text=True).strip()


def is_ref_type(ref_type: str | None) -> TypeGuard[RefType]:
    return ref_type in get_args(RefType)


def get_ref_version() -> tuple[RefType, str, str]:
    """Get the version from the branch or tag name."""
    ref_type = os.getenv("GITHUB_REF_TYPE")
    if not is_ref_type(ref_type):
        raise InvalidRefTypeError(ref_type)
    version_re = ref_name_version_re[ref_type]
    ref_name = os.getenv("GITHUB_REF_NAME")
    if not ref_name:
        raise InvalidRefNameError(ref_name, version_re.pattern)
    match = version_re.match(ref_name)
    if not match:
        raise InvalidRefNameError(ref_name, version_re.pattern)
    return ref_type, ref_name, match.group(1)


def get_next_release_candidate(version: str) -> int:
    """Get the next release candidate number for the given version.

    If no release candidates exist for the given version, return 1. Raise an
    error if the version has already been released.
    """
    run(["git", "fetch", "--prune", "--unshallow", "--tags"], check=True)
    output = check_output(
        ["git", "tag", "--list", f"v{version}*"], text=True, encoding="utf-8"
    )
    rc = 0
    for line in output.splitlines():
        line = line.strip()
        matched = release_candidate_re.match(line)
        if not matched:
            if line == f"v{version}":
                raise ValueError(f"Version {version} has already been released")
            continue
        rc = max(rc, int(matched.group(1)))
    return rc + 1


def set_github_outputs(version: str, commit_sha: str) -> None:
    output_file = os.getenv("GITHUB_OUTPUT")
    if not output_file:
        logger.warning("GITHUB_OUTPUT is not set, not updating outputs")
        return
    outputs = {
        "release_version": version,
        "release_commit_sha": commit_sha,
    }
    logger.info("Writing outputs: %s", outputs)
    with open(output_file, "a") as f:
        for key, value in outputs.items():
            print(f"{key}={value}", file=f, flush=True)


def update_json_version(filename: str, version: str) -> None:
    """Update the version in the JSON file to the given version."""
    with open(filename) as f:
        package = json.loads(f.read())
    package["version"] = version
    with open(filename, "w") as f:
        f.write(json.dumps(package, indent=2))
    run(["git", "add", filename], check=True)


def verify_json_version(filename: str, expected_version: str) -> None:
    """Verify that the version in the JSON file matches the expected version."""
    with open(filename) as f:
        package = json.loads(f.read())
    version = package.get("version")
    if version != expected_version:
        raise ValueError(
            f"Version in {filename} is {version}, but expected {expected_version}"
        )


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--verbose", action="store_true")
    subparsers = parser.add_subparsers(dest="command")
    bump_parser = subparsers.add_parser("bump")
    bump_parser.add_argument(
        "--release",
        type=bool,
        default=False,
        help="If true, release the version, instead of creating a release candidate",
    )
    _verify_parser = subparsers.add_parser("verify")
    args = parser.parse_args(sys.argv[1:])

    logging.basicConfig(
        format="%(asctime)s %(levelname)8s %(name)s: %(message)s",
        level=logging.DEBUG if args.verbose else logging.INFO,
    )

    commit_sha = os.getenv("GITHUB_SHA")
    ref_type, ref_name, version = get_ref_version()
    if args.command == "bump":
        if ref_type != "branch":
            raise ValueError("Bump command can only be used with branches")
        if not args.release:
            release_candidate = get_next_release_candidate(version)
            version = f"{version}-rc{release_candidate}"
            logger.info("Creating release candidate %s", version)
        for filename in version_filenames:
            update_json_version(filename, version)
            logger.info("Updated to version %s in %s", version, filename)
        commit_sha = commit_and_push(ref_name, version)
    elif args.command == "verify":
        for filename in version_filenames:
            verify_json_version(filename, version)
            logger.info("Verified that version is %s in %s", version, filename)
    else:
        raise ValueError(f"Invalid command: {args.command}")
    if not commit_sha:
        raise ValueError("GITHUB_SHA is not set")
    set_github_outputs(version, commit_sha)


if __name__ == "__main__":
    try:
        main()
    except Exception as e:
        logger.exception("Error occurred: %s", e)
        sys.exit(1)
