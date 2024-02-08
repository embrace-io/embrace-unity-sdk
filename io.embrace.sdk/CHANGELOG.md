---
title: Unity SDK Changelog
description: Changelog for the Unity Embrace SDK
sidebar_position: 4
---

# Unity SDK Changelog

## 1.21.0
*January 25th, 2024

* Updated iOS SDK version to 5.24.5
* Updated Embrace Unity API signatures for Embrace Android 6.x API

## 1.20.0
*January 12th, 2024

* Updated Android SDK to 6.2.0
* Added screenshot capture for Unity Android Bugshake
* Patched issue for users of EDM; we now generate the EmbraceSDKDependencies.xml file dynamically.

## 1.19.1
*December 21st, 2023*

* Patched issue affecting Unity Android EDM customers where a change in dependencies blocked builds.

## 1.19.0
*December 20th, 2023*
* This version of the SDK had a build blocking issue for customers using EDM. Please update to the latest version.
* Added support on Unity Android for the Embrace Android Bug Shake feature! Available for users who have Embrace Bug Shake accounts.
* Updated iOS SDK to 5.24.2
* Updated Android SDK to 6.1.0 (Bug Shake uses the Bug Shake Plugin version 0.9.0)
* Patched iOS SDK specific issue involving Current Session ID API
* Patched code signature issue on iOS for XCode 15

## 1.18.2
*December 7th 2023*
:::warning Important
This version of the Unity SDK has two issues:
- Build blocking issue on iOS around the current session ID API
- Build blocking issue on iOS regarding code signature issues with XCode 15
Please update to the latest version.
:::

* Hotfix patch for bug where the Unity SDK throws an exception when capturing network requests that are null or contain null parameters.

## 1.18.1
*November 6th 2023*

* This version of the Unity SDK causes an exception when the Unity SDK tries to capture network requests that are null or that contain null parameters. Please update to the latest version.
* Hotfix patch for bug for SDK where Android SDK and Unity SDK misalignment resulted in multiple dropped exceptions.

## 1.18.0
*October 30, 2023*

* This version of the Unity SDK introduced a bug between the Unity and internal Android SDK resulting in dropped exceptions when communicating between the two SDKs.
* Embrace.Instance.SetUserPersona, .LogBreadcrumb, .LogNetworkRequest have been deprecated and replaced by the following functions respectively: .AddUserPersona, .AddBreadcrumb, .RecordNetworkRequest
* iOS users can now specify their Crash Report Provider in via the iOS configuration of the Embrace SDK
* Embrace.Instance now has a .IsStarted boolean property that reports if the Embrace SDK has finished starting/initializing.
* Support for uploading il2cpp metadata for symbolication Unity C# code
* Added more obvious in-window reports indicating incomplete integration of Embrace Unity SDK
* Fixed: XCode Symbol Upload Build Phase now uses unix newline character instead of current environment line ending
* Fixed: Automatic Network Capture reporting connection error when result was protocol error.
* Updated internal iOS Embrace SDK to 5.23.2
* Updated internal Android Embrace SDK to 5.25.0

## 1.17.0
*August 10, 2023*

* Added methods for recording iOS and Android push notification data.
* Updated exceptions API to allow logging of handled exceptions as well as unhandled exceptions.
* Fixed an issue that caused Embrace to clean the Unity build cache unnecessarily when the weaver assembly was recompiled in projects not using automatic network capture.
* Fixed a potential null reference exception when calling `GetSessionProperties` on Android before initializing the SDK.
* Updated Android SDK to version 5.23.0

## 1.16.0
*July 13, 2023*

* Fixed an issue that could cause the Unity package manager manifest to be refreshed when a recompile is triggered in the editor
* Fixed an issue that could lead to unhandled exceptions being lost or misidentified if no valid stack trace was available
* Updated Android SDK to version 5.22.0
* Updated iOS SDK to version 5.21.1

## 1.15.0
*June 27, 2023*

* Added `GetLastRunEndState` method to retrieve enum indicating a crash or clean exit on the previous app launch
* Updated Android SDK to version 5.21.0
* Updated iOS SDK to version 5.21.0

## 1.14.1
*June 16, 2023* 

* Updated iOS SDK to version 5.20.1

## 1.14.0
*June 14, 2023*

* Fixed a potential compatibility issue with other packages using Mono.Cecil
* Updated Android SDK to version 5.19.0
* Updated iOS SDK to version 5.20.0

## 1.13.0
*May 4, 2023*

* The Embrace Unity SDK now detects the presence of the External Dependency Manager and automatically configures the Android Swazzler as required
* Added additional SDK log silencing options to filter logs by type
* Exposed new options for ANR capture behavior
* Added new sample scene to easily trigger ANRs
* Fixed a bug that added Embrace.xcframework to exported Xcode projects using an absolute rather than relative path
* Fixed a build-time exception thrown when an Embrace configuration inspector was open when the build was initiated
* Updated Android SDK to version 5.18.0
* Updated iOS SDK to version 5.19.2

## 1.12.1
*April 12, 2023*

* Fixed configuration selection becoming unresponsive in some states
* Updated iOS SDK to 5.18.0

## 1.12.0
*April 11, 2023*

* Improved Embrace configuration editor UI 
* Removed editor UI assets from runtime resources
* Updated Android SDK to version 5.16.0

## 1.11.0
*March 24, 2023*

* Added support for building for the iOS simulator
* Added support for tvOS
* Added new option to automatically log UnityWebRequest data processing errors
* Updated iOS SDK to version 5.16.3
* Updated Android SDK to version 5.15.3

## 1.10.1
*February 21, 2023*

* Fixed automatic network capture beta failing to resolve assemblies in some scenarios when the project contains pre-compiled DLLs.
* The Embrace Unity SDK now explicitly declares dependencies on the following built-in Unity packages:
    - com.unity.modules.androidjni
    - com.unity.modules.jsonserialize
    - com.unity.ugui
    - com.unity.modules.unitywebrequest 

## 1.10.0
*February 13, 2023*

* Added option to automatically log Unity Scene changes as Views (beta)
* Updated Android SDK to version 5.13.0
* Updated iOS SDK to version 5.16.1
* Fixed successful network requests being logged as errors in the dashboard on iOS
* Fixed network request durations being rounded to the nearest second on iOS
* Fixed network request error logs showing incorrect error message on iOS

## 1.9.3
*January 23, 2023*

* Updated Android SDK to version 5.12.0
* Updated iOS SDK to version 5.15.0

## 1.9.2
*January 11, 2023*

* Updated Android SDK to version 5.11.0
* Fixed a bug that could cause the value of `Exception-Free Sessions` shown in the dashboard to be innacurate for Unity Android apps
* Updated iOS SDK to version 5.14.1
* Further improved reliability of exception logs on iOS when app is immediately terminated after the exception

## 1.9.1
*December 13, 2022*

* Updated Android SDK to version 5.10.0

## 1.9.0
*December 1, 2022*

* The Embrace Unity SDK now automatically updates the version of the `embrace-swazzler` dependency defined in the project's `baseProjectTemplate.gradle`
* Fixed a bug that could cause multiple small editor windows to open while the Embrace SDK is imported for the first time
* Fixed a bug that could throw an exception when adding the Embrace scoped registry to the project's package manifest
* Embrace SDK editor windows no longer open when the Unity editor is running in batch mode
* Fixed a bug that could cause the Embrace SDK to fail to load configurations when the Embrace data path was set to a custom path and the project was opened on a new machine
* Fixed a potential build-time `NullReferenceException` when automatic network capture is enabled and a UnityWebRequest was disposed via an `IDisposable` reference as the first instruction in a method
* Updated iOS SDK to version 5.12.4
* Updated Android SDK to version 5.9.3

## 1.8.1
*November 9, 2022*

* Excluded all Embrace assemblies when building for platforms other than iOS and Android.
* Disabled Embrace IL weaver when building for platforms other than iOS and Android.

## 1.8.0
*November 3, 2022*

* Added support for selecting environment configurations at build time by setting the `EMBRACE_ENVIRONMENTS_NAME` or `EMBRACE_ENVIRONMENTS_INDEX` environment variable
* Updated Android SDK to version 5.9.0
* Updated iOS SDK to version 5.12.1
    - Improved reliability of exception logs on iOS when app is immediately terminated after the exception
    - Fixed app freezing when encountering a native crash
    - Fixed some dSYMs failing to upload

## 1.7.6
*October 4, 2022*
:::warning Important
This version of the Unity SDK introduced a bug in the iOS crash handler that can cause the app to freeze when encountering a native crash. Please update to the latest version.
:::

* Updated Android SDK to version 5.7.0
* Updated iOS SDK to version 5.10.0
* Fixed issue with null Embrace data path which occurs when the same version of the SDK is re-installed.

## 1.7.5
*September 26, 2022*
:::warning Important
The version of the Embrace Android SDK used in this version contains a bug that can cause Embrace to fail to initialize properly. Please update to Unity SDK version 1.8.0 or later and Android SDK version 5.7.0 or later.
:::

* Fixed a compatibility issue with Embrace Android SDK version 5.6.0 and higher.

## 1.7.4
*September 26, 2022*
:::warning Important
This version includes a compatibility issue with Embrace Android SDK versions 5.6.0 and greater. Please update to Unity SDK version 1.8.0 or later.
:::

* Fixed an issue that could cause the Embrace SDK to fail to load the active configuration in the editor.
* Fixed the list of available configurations in the settings window getting out of sync with the configurtations available in the project.

## 1.7.3
*September 22, 2022*
:::warning Important
This version includes a compatibility issue with Embrace Android SDK versions 5.6.0 and greater. Please update to Unity SDK version 1.8.0 or later.
:::

* Updated Android SDK to version 5.6.1
* Updated iOS SDK to version 5.9.3
* Fixed a UI bug with the Getting Started and Embrace Settings windows that occured when the Environments object was deleted from embrace data directory manually.

## 1.7.2
*September 15, 2022*

* Resolved a dependency conflict between the Embrace SDK and Unity's Burst package.
* Fixed a UI bug in the Embrace Settings window that occurred when the SDK was reset.

## 1.7.1
*September 14, 2022*

* Removed unnecessary dependency on Unity's default version control package introduced in 1.7.0

## 1.7.0
*September 12, 2022*

* Added support for automatically logging web requests made via UnityWebRequest and HttpClient (BETA)
* Updated Android SDK to 5.5.4

## 1.6.0
*August 29, 2022*

* Updated iOS SDK to 5.9.1
* Updated Android SDK Version 5.5.3
* Bugfix for swapped bytesin/bytesout values in Android LogNetworkRequest
* Embrace SDK configuration scriptable objects moved to project Assets/Embrace folder.
* Fixed issue with dSYM upload scripts sometimes not being executable
* Settings window now features full list of configuration options.
* Bugfix for UnhandledExceptionRateLimiter

## 1.5.10
*July 07, 2022*

* Fixed breaking change in Embrace class
* Improved comments

## 1.5.9
*July 01, 2022*

* Upgrades native iOS SDK to 5.9.0

## 1.5.8
*June 17, 2022*

* Upgrades native iOS SDK to 5.8.1

## 1.5.7
*June 16, 2022*

* Added support for symbols upload in Unity 2020.
* Removed UGUI package from being included in our SDK's unitypackage.

## 1.5.6
*June 15, 2022*

* Fixed compatibility issue with Unity versions prior to 2020.2 introduced in v1.5.5.

## 1.5.5
*June 10, 2022*
:::warning Important
This version introduced a compatibility issue with Unity versions before 2020.2. Please upgrade to version 1.5.6.
:::

* Added Multi-Threaded Logging toggle to Main Settings editor window.
* Added option to disable Embrace Unity SDK logs.
* Upgraded native iOS SDK to 5.8.0
* Fixes a potential crash in the Android Provider where successful attachment to the Java VM was not being checked.

## 1.5.4
*June 06, 2022*

* Fixed issue with JSON conversion for log properties.
* Upgrades native iOS SDK to 5.7.8

## 1.5.3
*May 27, 2022*

* Upgrades native Android SDK to 5.1.0 for builds using the external dependency manager.
* Upgrades Embrace Swazzler to 5.1.0 to allow NDK stacktrace collection.

## 1.5.2
*May 16, 2022*

* Upgrades native Android SDK to 5.1.0-beta02 for builds using the external dependency manager.

## 1.5.1
*May 12, 2022*

* Upgrades native Android SDK to 5.1.0 for builds using the external dependency manager.
* Upgrades Embrace Swazzler to 5.1.0 to allow NDK stacktrace collection.

## 1.5.0
*May 05, 2022*

* Added Scoped Registries to allow users to manage, download and install packages using the Package Manager.

## 1.4.2
*May 04, 2022*

* Upgrades native Android SDK to 5.0.4 for builds using the external dependency manager.

## 1.4.1
*April 22, 2022*

* Upgrades native Android SDK to 5.0.2 for builds using the external dependency manager.

## 1.4.0
*March 15, 2022*

* Updated SDK to use the official package management system for Unity. These changes require you to delete your previous version of our SDK before importing the new update.
* Upgrades native iOS SDK to 5.7.6

## 1.3.9
*February 28, 2022*

* Added demos to help users get started using the Embrace SDK.
* Fixed serialization bug that affected config environments ability to save.

## 1.3.8
*February 11, 2022*

* Fixes TimeUtil main thread bug.
* Updated android config settings.
* Upgrades native Android SDK to 4.15.0 for builds using the external dependency manager.

## 1.3.7
*January 24, 2022*

* Introduces a Settings Window which exposes new features and settings using an editor window.
* Allows users to create and manage environments which enables you to handle configurations based on your desired environment.
* Added A text field that allows users to add Android settings to the embrace-config.json file that are not currently provided by the Unity SDK editor windows.

## 1.3.6
*January 03, 2022*

* Updated native SDK's to change default behavior for screenshot capturing.

## 1.3.5
*December 21, 2021*

* Upgrades native iOS SDK to 5.7.1
* Upgrades native Android SDK to 4.14.0 for builds using the external dependency manager

## 1.3.4
*December 01, 2021*

* Provides validation for the config ID and Token Key, For both the Embrace Post Build Processor and the customer-facing editor
* Upgrades native Android SDK to 4.13.0 for builds using the external dependency manager.

## 1.3.3
*November 24, 2021*

* Upgrades native iOS SDK to 5.7.0
* Updated Embrace Android configurations to allow enable_native_monitoring as an option in the Embrace Editor window. Includes a CustomAndroidConfigurations JSON file that allows overriding of the editor configurations.
* Improved namespaces and stability updates.

## 1.3.2
*October 18, 2021*

* Added editor windows to improve the SDK experience. 
* Enabled users to configure both Android and iOS at the same time using the new Embrace editor window. 
* Welcome window informs users of important changes, new features or warns them of potential issues.

## 1.3.1
*September 17, 2021*

* Fixed issue with iOS symbol upload

## 1.3.0
*September 17, 2021*

* Improved how configuration files are handled for both IOS and Android.

## 1.2.13
*August 9, 2021*

* Fixed issue where all dSYM files would not be uploaded for 2019+ Unity projects
* Enforce execution permission on scripts used in upload of dSYM files when they are copied to the Xcode project

## 1.2.12
*July 29, 2021*

* Made initialization more robust, removing initialization steps from Awake method.
* Disable SDK gracefully on unsupported platforms.
* Upgraded to the latest Android SDK version.

## 1.2.11
*July 20, 2021*

* Expanded search path in dSYM upload script to automatically include location of dSYMs for all supported Unity versions and configurations.

## 1.2.10
*July 15, 2021*

* Updated to unity-resolver config to use latest version of Android SDK, which addresses a compatibility issue with androidx.lifecycle v.2.3.0+
* Automatically set dSYM configuration for UnityFramework target.

## 1.2.9
*July 13, 2021*

* Removed references to Unity source in iOS project.

## 1.2.8
*June 15, 2021*

* Include the latest iOS 5.5.2 SDK

## 1.2.7
*June 14, 2021*

* Ensure that the NDK is always referenced correctly

## 1.2.6
*May 27, 2021*

* Update android dependency to 4.8.10
* Ensure JNI is always attached to current thread before usage

## 1.2.5
*May 17, 2021*

* Update android dependency to 4.8.7

## 1.2.4
*May 17, 2021*

* Fix quotes in iOS dSYM build phase to handle path spaces
* Update run.sh to support iOS dSYM search depth

## 1.2.3
*May 6, 2021*

* Fix iOS timestamps for manually logged network requests
* Support for latest Android sdk

## 1.2.2
*May 4, 2021*

* Use correct group id in external depedency xml

## 1.2.1
*May 4, 2021*

* Update Android artifact version in external depedency xml

## 1.2
*April 30, 2021*

* Support for unhandled exception reporting

## 1.1
*March 18, 2021*

* Add method to enable debug logging for Android platform
* Add method to manually log a network request
* Fix exception when logging warning messages on Android platform

## 1.0.14
*March 9, 2021*

* Adopt 5.3.7 iOS native build
* Fix typo in iOS info log severity value

## 1.0.13
*Feb 22, 2021*

* Support for the External Dependency Manager

## 1.0.12
*Feb 8, 2021*

* Adopt 5.3.6 iOS native build

## 1.0.11
*Feb 5, 2021*

* Adopt 5.3.5 iOS native build

## 1.0.10
*Jan 29, 2021*

* Adopt 5.3.4 iOS native build

## 1.0.9
*Jan 26, 2021*

* Adopt 5.3.3 iOS native build

## 1.0.8
*Jan 18, 2021*

* Adopt 5.3.2 iOS native build

## 1.0.7
*Jan 14, 2021*

* Updated post-build processing to support pre 2019.3 project configuration on Android

## 1.0.6
*Jan 11, 2021*

* Updated post-build processing to support pre 2019.3 project configuration on iOS

## 1.0.5
*Dec 20, 2020*

* First public release of the Embrace Unity SDK
