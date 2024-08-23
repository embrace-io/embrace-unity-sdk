#!/bin/bash

cd "Embrace Unity iOS Interface"

./build_xcframework.sh

cd ..

cp -rf \
    ./"Embrace Unity iOS Interface"/build/EmbraceUnityiOS.xcframework \
    ./io.embrace.sdk/iOS6/xcframeworks/EmbraceUnityiOS.xcframework

rm -rf ./"Embrace Unity iOS Interface"/build