#!/bin/bash

# Set the release version to download, default to current supported if not provided
RELEASE_VERSION=${1:-6.5.1}

# Remove the old xcframeworks
rm -rf ./io.embrace.sdk/iOS/xcframeworks/*.xcframework
rm -rf ./Embrace\ Unity\ iOS\ Interface/xcframeworks/*.xcframework

# Pull down the target iOS release
cd Embrace\ Unity\ iOS\ Interface
./update_iOS_to_latest.sh "$RELEASE_VERSION"

# Build Unity SDK xcframework
cd ..
./upgrade_iOS_interface.sh True

# Remove downloaded files
rm -rf embrace_*.zip