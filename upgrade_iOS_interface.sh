#!/bin/bash

./"Embrace Unity iOS Interface"/build.xcframework.sh

cp -f \
    ./"Embrace Unity iOS Interface"/build/EmbraceUnityiOS.xcframework \
    ./io.embrace.sdk/iOS6/xcframeworks/EmbraceUnityiOS.xcframework