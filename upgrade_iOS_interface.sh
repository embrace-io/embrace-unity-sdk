#!/bin/bash

cd "Embrace Unity iOS Interface"

./build_xcframework.sh

cd ..

echo "Removing old xcframework"

if [ "$1" = "True" ]; then
    rm -rf ./io.embrace.sdk/iOS/xcframeworks/*.xcframework
else
    rm -rf ./io.embrace.sdk/iOS/xcframeworks/EmbraceUnityiOS.xcframework
fi

echo "Copying xcframeworks to Unity project"

if [ "$1" = "True" ]; then
    cp -rf \
        ./"Embrace Unity iOS Interface"/xcframeworks/* \
        ./io.embrace.sdk/iOS/xcframeworks
else
    cp -rf \
        ./"Embrace Unity iOS Interface"/build/EmbraceUnityiOS.xcframework \
        ./io.embrace.sdk/iOS/xcframeworks/EmbraceUnityiOS.xcframework
fi

cp -rf \
    ./"Embrace Unity iOS Interface"/build/EmbraceUnityiOS.xcframework \
    ./io.embrace.sdk/iOS/xcframeworks/EmbraceUnityiOS.xcframework

echo "Cleaning up"

# rm -rf ./"Embrace Unity iOS Interface"/build