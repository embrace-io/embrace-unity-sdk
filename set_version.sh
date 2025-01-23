#!/bin/bash
# Run this script in the root of the embrace-unity-sdk repo to automatically set the version in the package and sdk info files.
# Depends on jq.

if ! command -v jq &> /dev/null
then
    echo "Script dependency jq could not be found. Please install and add it to your PATH."
    exit 1
fi

if [ $# -ne 1 ]
then
    echo 'Please pass a single parameter representing the new package version'
    exit 1
fi

new_version=$1

declare -r PACKAGE_JSON_FILE='io.embrace.sdk/package.json'
declare -r SDK_INFO_FILE='io.embrace.sdk/Resources/Info/EmbraceSdkInfo.json'

set_package_version () {
    local json_path=$1
    contents=$(jq ".version = \""$new_version"\"" $json_path) && \
    echo -E "${contents}" > $json_path
}

set_package_version $PACKAGE_JSON_FILE
set_package_version $SDK_INFO_FILE

