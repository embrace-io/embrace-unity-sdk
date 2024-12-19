# Testing changes in the SDK

The [example apps](UnityProjects/) can be used to test changes in the SDK. Just plugin an iOS or Android device
and you should be able to test locally.

These instructions use the Unity 2021 example app but the approach should be very similar between Unity versions.

## Testing Android changes

You can test Android changes in our Unity SDK by altering the dependency in the Unity package's `build.gradle`. You can either publish a local artefact with `./gradlew publishToMavenLocal` in the Android repo, or if you need CI to pass - publish a beta as documented in the [Android repo](https://github.com/embrace-io/embrace-android-sdk3#qa-releases).

### Local artefact
1. Publish locally with `./gradlew publishToMavenLocal -Pversion=<your-version-here>`
2. Find `allprojects.repositories` in `UnityProjects/2021/Assets/Plugins/Android/baseProjectTemplate.gradle` and add `mavenLocal()`
3. In the `dependencies` block of `UnityProjects/2021/Assets/Plugins/Android/launcherTemplate.gradle` add `implementation "io.embrace:embrace-android-sdk:<your-version-here>"`
4. Run the app in the normal way

### Beta artefact

1. Follow the [Android repo](https://github.com/embrace-io/embrace-android-sdk3#qa-releases) instructions for creating a beta
2. Find `allprojects.repositories` in `UnityProjects/2021/Assets/Plugins/Android/baseProjectTemplate.gradle` and add `maven {url "https://repo.embrace.io/repository/beta"}`
3. In the `dependencies` block of `UnityProjects/2021/Assets/Plugins/Android/launcherTemplate.gradle` add `implementation "io.embrace:embrace-android-sdk:<your-version-here>"`
4. Run the app in the normal way

## Testing iOS changes locally

The iOS SDK is vendored into the Unity repo. To test iOS changes, you should:

1. Generate a release build in the iOS repo
2. Copy the build to `io.embrace.sdk/iOS`
3. Run the app in the normal way

# Releasing the SDK

See the [notion docs](https://www.notion.so/embraceio/Publishing-the-Unity-SDK-36cadcff54d543128ea40d4b916c947a).

# Releasing a beta SDK version

To release a beta version of the SDK you should add a suffix to the version string. For example, `1.3.0-beta01` would be an appropriate name.

To generate a beta, build a `.unitypackage` file and then share this directly with the customer.
