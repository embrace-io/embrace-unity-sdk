#!/bin/bash

cd "Embrace Unity iOS Interface"

./build_xcframework.sh

cd ..

echo "Copying xcframeworks to Unity project"

cp -rf \
    ./"Embrace Unity iOS Interface"/build/EmbraceUnityiOS.xcframework \
    ./io.embrace.sdk/iOS6/xcframeworks/EmbraceUnityiOS.xcframework

echo "Cleaning up"

rm -rf ./"Embrace Unity iOS Interface"/build