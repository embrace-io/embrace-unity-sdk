function archive {
    echo "Archiving: \n- scheme: $1 \n- destination: $2;\n- archivePath: $3.xcarchive"
    IS_ARCHIVE=1 xcodebuild archive \
        -project "EmbraceUnityiOS.xcodeproj" \
        -scheme "$1" \
        -destination "$2" \
        -archivePath "$3" \
        SKIP_INSTALL=NO \
        BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
        ONLY_ACTIVE_ARCH=NO \
    | xcpretty
}

function create_xcframework {
    echo "Creating XCFramework: \n- archive_one: $1 \n- archive_two: $2 \n- output: $3"
    xcodebuild -create-xcframework \
        -archive "$1" \
        -framework "Embrace_Unity_iOS_Interface.framework" \
        -archive "$2" \
        -framework "Embrace_Unity_iOS_Interface.framework" \
        -output "$3" \
    | xcpretty
}

archive "EmbraceUnityiOS" "generic/platform=iOS" "./build/EmbraceUnityiOS"
archive "EmbraceUnityiOS" "generic/platform=iOS Simulator" "./build/EmbraceUnityiOS-simulator"

create_xcframework "./build/EmbraceUnityiOS.xcarchive" "./build/EmbraceUnityiOS-simulator.xcarchive" "./build/EmbraceUnityiOS.xcframework"

rm -rf ./build/*.xcarchive

echo "XCFramework created at: ${PWD}/build/EmbraceUnityiOS.xcframework"
