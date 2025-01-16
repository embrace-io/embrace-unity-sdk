#!/bin/zsh
# We require that the mock server already be running

# Arguments:
# $1 - Path to Unity executable
# $2 - Path to Unity project
# $3 - Port for mock server
# $4 - Path to mock server
# $5 - Path to test results

# Check if $2 is empty
if [ -z "$2" ]
then
    echo "No Unity project path provided"
    exit 1
fi

echo "Cleaning up old build data"

rm -rf $2/Library
rm -rf $2/Builds/SmokeTest
rm -rf $5

echo "Building XCode project from Unity"
$1 -batchmode -quit -logFile - \
    -projectPath $2 \
    -buildTarget iOS \
    -executeMethod Embrace.Internal.SmokeTests.SmokeTestBuild.Create

typeset buildPath=$2/Builds/SmokeTest
typeset _pwd=$PWD

cd $buildPath/Builds/SmokeTest
plutil -insert CONFIG_BASE_URL -string "http://localhost:$3/api" $buildPath/Embrace-Info.plist
plutil -insert DATA_BASE_URL -string "http://localhost:$3/api" $buildPath/Embrace-Info.plist
plutil -insert DATA_DEV_BASE_URL -string "http://localhost:$3/api" $buildPath/Embrace-Info.plist
plutil -insert IMAGES_BASE_URL -string "http://localhost:$3/api" $buildPath/Embrace-Info.plist
plutil -insert TEST_BASE_URL -string "http://localhost:$3/test" $buildPath/Embrace-Info.plist
plutil -p $buildPath/Embrace-Info.plist

cd $buildPath
xcodebuild

cd $_pwd/smoke_test_components/python_test_driver
python3 ios_smoke_test_autorun.py \
    --simulator "iPhone 14 Pro" \
    --appPath $buildPath/build/Release-iphonesimulator/embraceunitysdk.app \
    --resultsPath $5 \
    --bundleId com.DefaultCompany.embrace-unity-sdk \
    --serverPath $4 \
    --appId 12345