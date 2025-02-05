#!/usr/bin/env python3
"""Provides a way to install Unity editors and run tests."""

import argparse
import io
import logging
import os
import platform
import subprocess
import sys
import tempfile
import time
from contextlib import contextmanager
from os import path
from typing import Callable, Generator, Literal, Optional

if sys.platform == "win32":
    # Force UTF-8 encoding for sys.stdout and sys.stderr
    if sys.stdout.encoding != "utf-8":
        sys.stdout = io.TextIOWrapper(
            sys.stdout.buffer, encoding="utf-8", errors="replace"
        )
    if sys.stderr.encoding != "utf-8":
        sys.stderr = io.TextIOWrapper(
            sys.stderr.buffer, encoding="utf-8", errors="replace"
        )

logger = logging.getLogger("unity")


def run(
    args: list[str],
    logger: logging.Logger,
    logger_filter: Callable[[str], bool] = lambda _: True,
    log_path: str | None = None,
    raise_on_error: bool = False,
) -> int:
    """Run a command and log its output.

    This combines the stdout and stderr streams, and logs the output to a file.
    It also allows a filtered subset of the output to be logged to the passed
    logger.
    """
    process = subprocess.Popen(
        args,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        encoding="utf-8",
        errors="replace",
        bufsize=1,
    )
    if process.stdout is None:
        raise RuntimeError("process stdout is expectedly None")
    log_file: io.TextIOWrapper | None = None
    if log_path is not None:
        os.makedirs(path.dirname(log_path), exist_ok=True)
        log_file = open(log_path, "w", encoding="utf-8")
    try:
        while line := process.stdout.readline():
            if log_file is not None:
                log_file.write(line)
                log_file.flush()
            if logger_filter(line):
                logger.debug(line.rstrip())
        process.wait()
        if raise_on_error and process.returncode != 0:
            raise subprocess.CalledProcessError(process.returncode, args)
        return process.returncode
    finally:
        if log_file is not None:
            log_file.close()


def get_dummy_project_path() -> str:
    """Get the path to a dummy Unity project to use for one-off commands."""
    project_dir = path.join(tempfile.gettempdir(), "dummy-project")
    os.makedirs(path.join(project_dir, "Assets"), exist_ok=True)
    return project_dir


def activate_with_retries(
    args: list,
    logger: logging.Logger,
    retries: int | None = None,
    delay: int | None = None,
) -> None:
    """Activate a Unity license, retrying upon failure."""
    if retries is None:
        retries = 5
    if delay is None:
        delay = 15
    for attempt in range(1, retries + 1):
        try:
            logger.info("Activating Unity license (attempt %d)", attempt)
            run(
                args,
                logger,
                logger_filter=lambda line: "[Licensing::Client]" in line,
                raise_on_error=True,
            )
            logger.info("License activated")
            break
        except Exception:
            logger.exception("Failed to activate license (attempt %d)", attempt)
            if attempt < retries:
                logger.info("Retrying in %d seconds...", delay)
                time.sleep(delay)
            else:
                raise


class License:
    """The parameters needed to activate a Unity license."""

    serial: str
    username: str
    password: str

    def __init__(self, serial: str, username: str, password: str) -> None:
        self.serial = serial
        self.username = username
        self.password = password

    @classmethod
    def from_env(cls) -> "License":
        serial = os.getenv("UNITY_SERIAL")
        username = os.getenv("UNITY_EMAIL")
        password = os.getenv("UNITY_PASSWORD")
        if not serial or not username or not password:
            raise EnvironmentError(
                "UNITY_SERIAL, UNITY_EMAIL, and UNITY_PASSWORD must be set in the environment"
            )
        return cls(serial, username, password)


class Runner:
    """A class to manage the installation and running of Unity."""

    dummy_project_path: str
    """The path to a dummy Unity project to use for one-off commands."""

    editor_path: str
    """The path to the Unity editor installation folder."""

    editor_binary_path: str
    """The path to the Unity editor binary."""

    editor_binary_args: list[str]
    """The initial list of arguments to use to run the Unity editor.

    This is not necessarily just the path to the binary, as it may need to be
    prefixed with things like xvfb-run on Linux.
    """

    hub_path: str
    """The path to the Unity Hub executable."""

    logger: logging.Logger
    """The logger to use for logging messages."""

    version: str
    """The version of Unity to use."""

    def __init__(
        self,
        version: str,
        hub_path: str,
        editor_path: str,
        editor_binary_path: str,
        editor_binary_args: Optional[list[str]] = None,
    ) -> None:
        self.version = version
        self.hub_path = hub_path
        self.editor_path = editor_path
        self.editor_binary_path = editor_binary_path
        self.editor_binary_args = [editor_binary_path]
        self.dummy_project_path = get_dummy_project_path()
        self.logger = logging.getLogger(f"unity-{version}")

    def is_installed(self) -> bool:
        """Check if the Unity editor is installed."""
        return path.exists(self.editor_binary_path)

    def install(self, changeset: str) -> None:
        """Install the Unity editor."""
        if self.is_installed():
            self.logger.info("Editor is already installed")
            return
        self.logger.info("Installing editor")
        architecture = "x86_64"
        if sys.platform == "darwin" and platform.machine() == "arm64":
            architecture = "arm64"
        run(
            [
                self.hub_path,
                "--",
                "--headless",
                "install",
                "--version",
                self.version,
                "--changeset",
                changeset,
                "--module",
                "android",
                "--module",
                "ios",
                "--childModules",
                "--architecture",
                architecture,
            ],
            self.logger,
            raise_on_error=True,
        )
        self.logger.info("Installed editor")

    def uninstall(self) -> None:
        """Uninstall the Unity editor."""
        if not self.is_installed():
            self.logger.info("Editor is not installed")
            return
        self.logger.info("Uninstalling editor")
        run(["rm", "-rf", self.editor_path], self.logger, raise_on_error=True)
        self.logger.info("Uninstalled editor")

    @contextmanager
    def license(
        self, license: License, retries: int | None = None, delay: int | None = None
    ) -> Generator[None, None, None]:
        """Activate a Unity license for the duration of the context."""
        self.activate_license(license, retries, delay)
        try:
            yield
        finally:
            self.return_license(license)

    def activate_license(
        self, license: License, retries: int | None = None, delay: int | None = None
    ) -> None:
        """Activate a Unity license."""
        print(f"::add-mask::{license.serial[:-4]}XXXX")
        if sys.platform == "darwin":
            # Ensure the license directory is writable
            license_path = "/Library/Application Support/Unity"
            run(["sudo", "mkdir", "-p", license_path], self.logger, raise_on_error=True)
            run(
                ["sudo", "chmod", "-R", "u=rwx,g=rwx,o=rwx", license_path],
                self.logger,
                raise_on_error=True,
            )
        activate_with_retries(
            self.editor_binary_args
            + [
                "-batchmode",
                "-nographics",
                "-logFile",
                "-",
                "-quit",
                "-serial",
                license.serial,
                "-username",
                license.username,
                "-password",
                license.password,
                "-projectPath",
                self.dummy_project_path,
            ],
            self.logger,
            retries,
            delay,
        )

    def return_license(self, license: License) -> None:
        """Deactivate a Unity license."""
        run(
            self.editor_binary_args
            + [
                "-batchmode",
                "-nographics",
                "-logFile",
                "-",
                "-quit",
                "-returnlicense",
                "-username",
                license.username,
                "-password",
                license.password,
                "-projectPath",
                self.dummy_project_path,
            ],
            self.logger,
            logger_filter=lambda line: "[Licensing::Client]" in line,
            raise_on_error=True,
        )

    def run_test(
        self,
        run_id: str,
        project_path: str,
        build_target: Literal["android", "ios"],
        test_platform: Literal["editmode", "playmode"],
        coverage: bool,
    ) -> int:
        """Launch a Unity test run.

        This assumes that a license has been acquired.
        """
        self.logger.info("Running tests for %s", run_id)
        base_path = path.abspath(path.join(path.dirname(__file__), "..", ".."))
        build_path = path.join(base_path, "build")
        os.makedirs(build_path, exist_ok=True)
        args = self.editor_binary_args + [
            "-batchmode",
            "-nographics",
            "-logFile",
            "-",
            "-projectPath",
            project_path,
            "-buildTarget",
            build_target,
            "-runTests",
            "-testPlatform",
            test_platform,
            "-testResults",
            path.join(build_path, "test-results", f"{run_id}.xml"),
        ]
        if coverage:
            args += [
                "-debugCodeOptimization",
                "-enableCodeCoverage",
                "-coverageResultsPath",
                path.join(build_path, "coverage-results", run_id),
                "-coverageOptions",
                ";".join(
                    [
                        "generateAdditionalMetrics",
                        "assemblyFilters:+Embrace,+Embrace.*",
                        f"pathStrippingPatterns:{base_path}",
                    ]
                ),
            ]
        return_code = run(
            args,
            logger=logging.getLogger(f"unity-{run_id}"),
            logger_filter=lambda line: "[Test Profiler]" in line,
            log_path=path.join(build_path, "test-logs", f"{run_id}.log"),
        )
        if return_code != 0:
            self.logger.warning("Tests failed for %s", run_id)
        else:
            self.logger.info("Tests passed for %s", run_id)
        return return_code

    def run_tests(
        self,
        coverage: bool,
    ) -> None:
        """Run all the types of tests for this version of Unity.

        This will run tests for both Android and iOS in EditMode and PlayMode.
        It assumes that a license has been acquired.
        """
        year = self.version.split(".")[0]
        base_path = path.abspath(path.join(path.dirname(__file__), "..", ".."))
        project_path = path.join(base_path, "UnityProjects", year)
        if not path.exists(project_path):
            raise FileNotFoundError(f"Unity project not found: {project_path}")
        self.logger.info("Running tests for %s", year)
        for build_target in ("android", "ios"):
            for test_platform in ("editmode", "playmode"):
                run_id = f"{year}-{build_target}-{test_platform}"
                self.run_test(
                    run_id, project_path, build_target, test_platform, coverage
                )

    @classmethod
    def create_for_platform(cls, version: str) -> "Runner":
        """Create a Runner for the current platform."""
        editor_path = path.join("/Applications/Unity/Hub/Editor", version)
        return cls(
            version,
            hub_path="/Applications/Unity Hub.app/Contents/MacOS/Unity Hub",
            editor_path=path.join("/Applications/Unity/Hub/Editor", version),
            editor_binary_path=path.join(editor_path, "Unity.app/Contents/MacOS/Unity"),
        )


class ColorFormatter(logging.Formatter):
    formatters: dict[int, logging.Formatter]

    def __init__(
        self, fmt: str = "%(asctime)s %(levelname)-8s %(name)-30s %(message)s"
    ) -> None:
        super().__init__(fmt)
        self.formatters = {
            logging.DEBUG: logging.Formatter(f"\x1b[37m{fmt}\x1b[0m"),
            logging.INFO: logging.Formatter(f"\x1b[37;1m{fmt}\x1b[0m"),
            logging.WARNING: logging.Formatter(f"\x1b[33m{fmt}\x1b[0m"),
            logging.ERROR: logging.Formatter(f"\x1b[31m{fmt}\x1b[0m"),
            logging.CRITICAL: logging.Formatter(f"\x1b[31;1m{fmt}\x1b[0m"),
        }

    def format(self, record: logging.LogRecord) -> str:
        formatter = self.formatters.get(record.levelno)
        return formatter.format(record) if formatter else super().format(record)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--quiet", action="store_true")
    parser.add_argument("--version", type=str, required=True)
    subparsers = parser.add_subparsers(dest="command", required=True)

    install_parser = subparsers.add_parser("install")
    install_parser.add_argument("--changeset", type=str, required=True)

    _uninstall_parser = subparsers.add_parser("uninstall")

    _platform_parser = subparsers.add_parser("platform")

    test_parser = subparsers.add_parser("test")
    test_parser.add_argument(
        "--skip-coverage",
        dest="coverage",
        action="store_false",
        default=True,
        help="Skip generating code coverage reports",
    )
    test_parser.add_argument(
        "--skip-license",
        dest="license",
        action="store_false",
        default=True,
        help="Skip activating the Unity license",
    )

    args = parser.parse_args(sys.argv[1:])

    console_handler = logging.StreamHandler(sys.stderr)
    console_handler.setFormatter(ColorFormatter())
    console_handler.setLevel(logging.INFO if args.quiet else logging.DEBUG)
    root_logger = logging.getLogger()
    root_logger.handlers.clear()
    root_logger.addHandler(console_handler)
    root_logger.setLevel(console_handler.level)

    runner = Runner.create_for_platform(args.version)
    if args.command == "install":
        runner.install(args.changeset)
    elif args.command == "uninstall":
        runner.uninstall()
    elif args.command == "platform":
        if sys.platform == "darwin":
            print("darwin")
        elif sys.platform == "win32":
            print("win32")
        else:
            print("linux")
    elif args.command == "test":
        license = License.from_env() if args.license else None
        if license is not None:
            with runner.license(license):
                runner.run_tests(coverage=args.coverage)
        else:
            logger.info("Skipping Unity license activation")
            runner.run_tests(coverage=args.coverage)
    else:
        raise ValueError(f"Invalid command: {args.command}")


if __name__ == "__main__":
    main()
