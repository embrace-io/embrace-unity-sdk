#!/usr/bin/env python3
"""Generate a Markdown report from Unity test results."""

import argparse
import glob
import sys
from dataclasses import dataclass
from os import path
from xml.etree import ElementTree


@dataclass
class UnityBuild:
    """A tuple of the properties passed to Unity for a single test run."""

    year: str
    """The year of the Unity version used to run the tests."""
    build_target: str
    """Build target ("android" or "ios") used for the test run."""
    test_platform: str
    """The platform ("editmode" or "playmode") used for the test run."""

    @classmethod
    def create_from_filename(cls, filename: str) -> "UnityBuild":
        """Parse a UnityBuild out of the test results filename."""
        parts = path.splitext(path.basename(filename))[0].split("-")
        if len(parts) != 3:
            raise ValueError(f"Invalid test result filename: {filename}")
        return cls(*parts)


@dataclass
class Failure:
    """A single failed test case."""

    unity_build: UnityBuild
    """The parameters passed to Unity for the test run."""
    name: str
    """The class path of the test case."""
    message: str
    """A short message describing the reason for the test failure."""

    @classmethod
    def create_from_test_case(
        cls, unity_build: UnityBuild, test_case: ElementTree.Element
    ) -> "Failure":
        """Create a Failure object from a test case XML element."""
        if test_case.attrib["result"] != "Failed":
            raise ValueError("Test case is not a failure")
        message = test_case.find("./failure/message")
        if message is None or message.text is None:
            raise ValueError("Test case does not have a failure message")
        return cls(unity_build, test_case.attrib["fullname"], message.text)

    @classmethod
    def create_all_from_test_results(
        cls, unity_build: UnityBuild, tree: ElementTree.ElementTree
    ) -> list["Failure"]:
        """Create a list of Failure objects from the parsed test result XML."""
        return [
            cls.create_from_test_case(unity_build, test_case)
            for test_case in tree.findall('.//test-case[@result="Failed"]')
        ]


def render_markdown(
    num_passed: int, num_failed: int, num_skipped: int, failures: list[Failure]
) -> str:
    """Render a Markdown report from the test results."""
    url = f"https://svg.test-summary.com/dashboard.svg?p={num_passed}&f={num_failed}&s={num_skipped}"
    alt = f"{num_failed} tests failed" if num_failed > 0 else "Tests passed."
    md = f'<img src="{url}" alt="{alt}"/>\n\n'
    if num_failed == 0:
        return md
    md += "| Year | Build Target | Test Platform | Test Name | Message |\n"
    md += "| ---- | ------------ | ------------- | --------- | ------- |\n"
    for failure in failures:
        message = failure.message.replace("|", "\\|").replace("\n", "<br>")
        md += f"| {failure.unity_build.year} | {failure.unity_build.build_target} | {failure.unity_build.test_platform} | {failure.name} | {message} |\n"
    return md


def main() -> None:
    base_dir = path.abspath(path.join(path.dirname(__file__), "..", ".."))
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--test-results",
        default=path.join(base_dir, "build", "test-results"),
        type=str,
        help="Path to the folder containing test result XML files",
    )
    args = parser.parse_args(sys.argv[1:])

    num_passed = 0
    num_failed = 0
    num_skipped = 0
    failures: list[Failure] = []
    for filename in sorted(glob.glob(path.join(args.test_results, "*.xml"))):
        unity_build = UnityBuild.create_from_filename(filename)
        tree = ElementTree.parse(filename)
        test_suite = tree.find("./test-suite")
        if test_suite is None:
            raise ValueError("Test suite not found")
        num_passed += int(test_suite.attrib["passed"])
        num_failed += int(test_suite.attrib["failed"])
        num_skipped += int(test_suite.attrib["skipped"])
        failures.extend(Failure.create_all_from_test_results(unity_build, tree))
    print(render_markdown(num_passed, num_failed, num_skipped, failures))
    if num_failed > 0:
        sys.exit(1)


if __name__ == "__main__":
    main()
