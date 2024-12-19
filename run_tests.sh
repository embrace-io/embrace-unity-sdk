#!/bin/bash

# Run test passes in multiple Unity projects from the command line. Supports MacOS and Windows via cygwin
# 
# Usage: 
#   ./run_tests.sh
#  
# The default command with no arguments will run edit and play mode tests for iOS and Android on all Unity projects found in the UnityProjects directory.
# Use the options below to customize behavior.
#
#
# OPTIONAL ARGUMENTS
# ------------------
# 
# To test a subset of projects in the UnityProjects directory, simply add the name of the desired projects as arguments separated by a space. 
# For example:
#
#   ./run_tests.sh 2021 2020
#
# This will run tests in the 2021 and 2020 projects, but skip all other projects in the UnityProjects directory
#
#
# OPTIONAL ENVIRONMENT VARIABLES
# ------------------------------
#
# UNITY_TEST_MODE (default='playmode,editmode')
#
# Set this environment variable with a comma separated list of test modes to run. Accepted values are 'editmode' and 'playmode'. 
#
# UNITY_BUILD_TARGET (default='iOS,Android')
#
# Set this environment variable with a comma separated list of build targets to run. Accepted values are 'iOS' and 'Android' (case-sensitive).
#
# UNITY_ADDITIONAL_ARGS (default='')
#
# Use this environment variable to pass any additional args to Unity when invoking the test runs.
#
# CI (default=<not set>)
#
# Set this environment variable to enable verbose logging. The Unity log output will be redirected to stdout, and the test results xml files will 
# be output to stdout after each run.

function runTests () {

    projectPath="UnityProjects/$unityMajorVersion/"
    coverageOptions="generationAdditionalMetrics;generateHtmlReport;generateBadgeReport;assemblyFilters:+Embrace,+Embrace.*,-Embrace.SDK.Editor.Tests,-Embrace.EditTests"

    resultsPath=${buildTarget}-${testMode}results.xml

    unityFullVersion=$(awk '/m_EditorVersion:/ {print $2}' ${projectPath}ProjectSettings/ProjectVersion.txt)

    #TODO add support for linux
    if [[ "$OSTYPE" == "darwin"* ]]; then
        unityEditorPath="/Applications/Unity/Hub/Editor/$unityFullVersion/Unity.app/Contents/MacOS/Unity"
    else
        unityEditorPath="C:/Program Files/Unity/Hub/Editor/$unityFullVersion/Editor/Unity.exe"
    fi

    printf "Running $buildTarget $testMode Tests for Unity $unityFullVersion..."

    additionalArgs=$UNITY_ADDITIONAL_ARGS

    # if [ $CI ]
    # then 
    #     additionalArgs="$additionalArgs -logFile -"
    # fi

    additionalArgs="$additionalArgs -logFile -"

    "$unityEditorPath" -batchmode -projectPath $projectPath -runTests -testPlatform $testMode -buildTarget $buildTarget -testResults $resultsPath -enableCodeCoverage -debugCodeOptimization -coverageOptions "$coverageOptions" $additionalArgs

    code=$?

    if [ $code -eq 0 ]
    then
        printf "\033[32;4mPASSED!\033[0m\n"
    else
        printf "\033[31;4mFAILED!\033[0m\n"
        printf "See ${projectPath}${resultsPath} for details.\n"
        exit $code
    fi

    if [ $CI ]
    then
        printf "###########################\n#    $testMode Results    #\n###########################\n"
        cat $projectPath$resultsPath
    fi
}


# Parse Build Platforms
if [ -z $UNITY_BUILD_TARGET ] 
then
    UNITY_BUILD_TARGET='iOS,Android'
fi
IFS=', ' read -r -a buildTargets <<< $UNITY_BUILD_TARGET


# Parse Unity Versions
testProjects=()
if [ $# -eq 0 ] || [ $1 = 'all' ]  ;
then
    for dir in ./UnityProjects/*/ ; do
        dir2=${dir%/}       # Remove the trailing /
        dir3=${dir2##*/}    # Remove everything up to, and including, the last /
        testProjects+=( $dir3 )
    done
else
    testProjects+=( "$@" )
fi

# Parse Test Mode
if [ -z $UNITY_TEST_MODE ]
then
    UNITY_TEST_MODE='playmode,editmode'
fi
IFS=', ' read -r -a testModes <<< $UNITY_TEST_MODE

# Run Tests
for dir in "${testProjects[@]}"; do
    unityMajorVersion=$dir
    for target in "${buildTargets[@]}"; do
        buildTarget=$target
        for testMode in "${testModes[@]}"; do
            testMode=$testMode
            runTests
        done
    done

done
