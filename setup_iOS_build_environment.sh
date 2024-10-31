#!/bin/bash

# Set the release version to download, default to current supported if not provided
RELEASE_VERSION=${1:-6.5.1}

echo $RELEASE_VERSION

# Remove the old xcframeworks
rm -rf ./io.embrace.sdk/iOS/xcframeworks/*.xcframework
rm -rf ./Embrace\ Unity\ iOS\ Interface/xcframeworks/*.xcframework

# Pull down the target iOS release
gh release download "$RELEASE_VERSION" --repo embrace-io/embrace-apple-sdk --pattern 'embrace_*.zip' --dir ./

# Copy into Swift Interface
unzip -o embrace_*.zip -d ./Embrace\ Unity\ iOS\ Interface
rm -rf ./Embrace\ Unity\ iOS\ Interface/run.sh ./Embrace\ Unity\ iOS\ Interface/*.darwin

# Build Unity SDK xcframework
./upgrade_iOS_interface.sh True

# Remove downloaded files
rm -rf embrace_*.zip
