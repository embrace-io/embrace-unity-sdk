#!/bin/bash

#
# Wrapper script to help with updating native Embrace SDK versions.
# It is primarily used by .github/workflows/update-native-sdks.yaml and not humans.
#

if [[ $# -eq 0 ]]; then
  echo "Usage: $0 <get|set> <android|apple> [version]"
  exit 1
fi

action=$1   # get or set
platform=$2 # example: android or apple
version=$3  # only for "set"

if [[ $action == "get" ]]; then
  if [[ $platform == "android" ]]; then
    grep ANDROID_SDK_VERSION io.embrace.sdk/Editor/Data/VersionsRepository.cs | cut -f 2 -d\"
    exit 0
  fi

  if [[ $platform == "apple" ]]; then
    grep https://github.com/embrace-io/embrace-apple-sdk.git io.embrace.sdk/iOS/EmbraceUnityiOS/Package.swift | cut -f 4 -d\"
    exit 0
  fi
fi

if [[ $action == "set" ]]; then
  if [[ $(uname) == "Darwin" ]]; then
    SED="sed -i''"
  else
    SED="sed -i"
  fi

  # Example: https://github.com/embrace-io/embrace-unity-sdk/pull/89
  if [[ $platform == "android" ]]; then
    $SED -E "s/(ANDROID_SDK_VERSION = \")[^\"]*(\")/\1$version\2/" io.embrace.sdk/Editor/Data/VersionsRepository.cs
    $SED -E "s/(spec=\"io\.embrace:embrace-[^:]+:)[0-9.]+/\1$version/" io.embrace.sdk/Editor/EmbraceSDKDependencies.xml
    $SED -E "s/(classpath \"io\.embrace:embrace-[^:]+:)[0-9.]+/\1$version/" UnityProjects/*/Assets/Plugins/Android/baseProjectTemplate.gradle
  fi

  # Example: https://github.com/embrace-io/embrace-unity-sdk/pull/130
  if [[ $platform == "apple" ]]; then
    $SED -E "/url: \"https:\/\/github.com\/embrace-io\/embrace-apple-sdk.git\"/ s/(exact: \")[^\"]*(\")/\1$version\2/" io.embrace.sdk/iOS/EmbraceUnityiOS/Package.swift
    cd io.embrace.sdk/iOS/EmbraceUnityiOS && swift package resolve
  fi
fi
