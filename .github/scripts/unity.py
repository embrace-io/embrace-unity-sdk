#!/usr/bin/env python3
"""Provides a way to install Unity editors and run tests."""

import argparse
import io
import json
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

base_path = path.abspath(path.join(path.dirname(__file__), "..", ".."))
build_path = path.join(base_path, "build")
logger = logging.getLogger("unity")


def get_platform() -> Literal["darwin", "linux", "win32"]:
    """Get a string representing the current platform."""
    if sys.platform == "darwin":
        return "darwin"
    elif sys.platform == "win32":
        return "win32"
    else:
        return "linux"


def run(
    args: list[str],
    logger: logging.Logger,
    logger_filter: Callable[[str], bool] = lambda _: True,
    log_path: Optional[str] = None,
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
    log_file: Optional[io.TextIOWrapper] = None
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
    retries: Optional[int] = None,
    delay: Optional[int] = None,
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

    logger: logging.Logger
    """The logger to use for logging messages."""

    version: str
    """The version of Unity to use."""

    def __init__(self, version: str) -> None:
        self.dummy_project_path = get_dummy_project_path()
        self.logger = logging.getLogger(f"unity-{version}")
        self.version = version
        if sys.platform == "darwin":
            self.editor_path = path.join("/Applications/Unity/Hub/Editor", version)
            self.editor_binary_path = path.join(
                self.editor_path, "Unity.app/Contents/MacOS/Unity"
            )
            self.editor_binary_args = [self.editor_binary_path]
        elif sys.platform == "win32":
            self.editor_path = path.join(
                "C:", "Program Files", "Unity", "Hub", "Editor", version
            )
            self.editor_binary_path = path.join(self.editor_path, "Editor", "Unity.exe")
            self.editor_binary_args = [self.editor_binary_path]
        else:
            self.editor_path = path.join("/opt/unity/editors", version)
            self.editor_binary_path = path.join(self.editor_path, "Editor/Unity")
            self.editor_binary_args = [
                "xvfb-run",
                "--auto-servernum",
                self.editor_binary_path,
            ]

    def build(self) -> None:
        """Build the Unity package for the Embrace SDK."""
        with open(path.join(base_path, "io.embrace.sdk", "package.json"), "r") as f:
            version = json.load(f)["version"]
            filename = f"EmbraceSDK_{version}.unitypackage"
        self.logger.info("Building %s", filename)
        project_path = path.join(base_path, "UnityProjects", "2021")
        os.makedirs(build_path, exist_ok=True)
        run(
            self.editor_binary_args
            + [
                "-batchmode",
                "-buildTarget",
                "android",
                "-executeMethod",
                "EmbraceSDK.CIPublishTool.ExportUnityPackage",
                "-logFile",
                "-",
                "-nographics",
                "-projectPath",
                project_path,
                "-quit",
            ],
            logger=self.logger,
            log_path=path.join(build_path, "build.log"),
            raise_on_error=True,
        )
        run(
            [
                "mv",
                "-v",
                path.join(project_path, filename),
                path.join(build_path, filename),
            ],
            logger=self.logger,
            raise_on_error=True,
        )

    def is_installed(self) -> bool:
        """Check if the Unity editor is installed."""
        return path.exists(self.editor_binary_path)

    def install(self, changeset: str, modules: list[str]) -> None:
        """Install the Unity editor."""
        if self.is_installed():
            self.logger.info("Editor is already installed")
            return
        self.logger.info("Installing editor")
        architecture = "x86_64"
        if sys.platform == "darwin" and platform.machine() == "arm64":
            architecture = "arm64"
        hub_args = [
            "--headless",
            "install",
            "--version",
            self.version,
            "--changeset",
            changeset,
            "--architecture",
            architecture,
            "--childModules",
        ]
        for module in modules:
            hub_args += ["--module", module]
        if sys.platform == "darwin":
            args = [
                "/Applications/Unity Hub.app/Contents/MacOS/Unity Hub",
                "--",
            ] + hub_args
        elif sys.platform == "win32":
            args = [
                "powershell.exe",
                "-ExecutionPolicy",
                "Bypass",
                "-Command",
                "Start-Process",
                "-FilePath",
                "'C:\\Program Files\\Unity Hub\\Unity Hub.exe'",
                "-ArgumentList",
                ",".join([f"'{v}'" for v in ["--"] + hub_args]),
                "-Wait",
                "-PassThru",
            ]
        else:
            args = ["/usr/bin/unity-hub"] + hub_args
        run(args, self.logger, raise_on_error=True)
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
        self,
        license: License,
        retries: Optional[int] = None,
        delay: Optional[int] = None,
    ) -> Generator[None, None, None]:
        """Activate a Unity license for the duration of the context."""
        self.activate_license(license, retries, delay)
        try:
            yield
        finally:
            self.return_license(license)

    def activate_license(
        self,
        license: License,
        retries: Optional[int] = None,
        delay: Optional[int] = None,
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
        self.logger.info("Returning Unity license")
        return_code = run(
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
            raise_on_error=False,
        )
        if return_code != 0:
            self.logger.warning("Error returning license (%d), ignoring", return_code)
        else:
            self.logger.info("Returned Unity license")

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
        self, coverage: bool, build_targets: list[Literal["android", "ios"]]
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
        for build_target in build_targets:
            for test_platform in ("editmode", "playmode"):
                run_id = f"{year}-{get_platform()}-{build_target}-{test_platform}"
                self.run_test(
                    run_id, project_path, build_target, test_platform, coverage
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
    install_parser.add_argument(
        "--module",
        type=str,
        action="append",
        choices=["android", "ios"],
        help="Which modules to install",
    )

    _uninstall_parser = subparsers.add_parser("uninstall")

    build_parser = subparsers.add_parser("build")
    build_parser.add_argument(
        "--skip-license",
        dest="license",
        action="store_false",
        default=True,
        help="Skip activating the Unity license",
    )

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
    test_parser.add_argument(
        "--build-target",
        type=str,
        action="append",
        choices=["android", "ios"],
        help="The build targets to run tests for",
    )

    args = parser.parse_args(sys.argv[1:])

    console_handler = logging.StreamHandler(sys.stderr)
    console_handler.setFormatter(ColorFormatter())
    console_handler.setLevel(logging.INFO if args.quiet else logging.DEBUG)
    root_logger = logging.getLogger()
    root_logger.handlers.clear()
    root_logger.addHandler(console_handler)
    root_logger.setLevel(console_handler.level)

    runner = Runner(args.version)
    if args.command == "install":
        runner.install(args.changeset, args.module or ["android", "ios"])
    elif args.command == "uninstall":
        runner.uninstall()
    elif args.command == "build":
        license = License.from_env() if args.license else None
        if license is not None:
            with runner.license(license):
                runner.build()
        else:
            logger.info("Skipping Unity license activation")
            runner.build()
    elif args.command == "test":
        license = License.from_env() if args.license else None
        if license is not None:
            with runner.license(license):
                runner.run_tests(
                    coverage=args.coverage,
                    build_targets=(args.build_target or ["android", "ios"]),
                )
        else:
            logger.info("Skipping Unity license activation")
            runner.run_tests(
                coverage=args.coverage,
                build_targets=(args.build_target or ["android", "ios"]),
            )
    else:
        raise ValueError(f"Invalid command: {args.command}")


if __name__ == "__main__":
    main()
