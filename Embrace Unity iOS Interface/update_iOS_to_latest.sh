#!/bin/bash

rm -rf ./xcframeworks/*

# Check if a release tag is provided as an argument
if [ -z "$1" ]; then
  # No release tag provided, download the latest release
  gh release download --repo embrace-io/embrace-apple-sdk --pattern 'embrace_*.zip' --dir ./
else
  # Release tag provided, download the specified release
  gh release download "$1" --repo embrace-io/embrace-apple-sdk --pattern 'embrace_*.zip' --dir ./
fi

unzip -o embrace_*.zip

mv run.sh "../io.embrace.sdk/iOS/"
mv *.darwin "../io.embrace.sdk/iOS/"

rm -rf embrace_*.zip

if [ "$2" != "skip" ]; then
  echo "Updating the EmbraceUnityiOS.xcodeproj file"
  xcodegen
fi
