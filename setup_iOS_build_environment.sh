#!/bin/bash

# Remove the old xcframeworks
rm -rf ./io.embrace.sdk/iOS/xcframeworks/*.xcframework
rm -rf ./Embrace\ Unity\ iOS\ Interface/xcframeworks/*.xcframework

# Pull down the target iOS release
gh release download 6.4.2 --repo embrace-io/embrace-apple-sdk --pattern 'embrace_*.zip' --dir ./

# Copy into Swift Interface
unzip -o embrace_*.zip -d ./Embrace\ Unity\ iOS\ Interface
rm -rf ./Embrace\ Unity\ iOS\ Interface/run.sh ./Embrace\ Unity\ iOS\ Interface/*.darwin

# Build Unity SDK xcframework
./upgrade_iOS_interface.sh True

# Remove downloaded files
rm -rf embrace_*.zip
