<p align="center">
  <a href="https://embrace.io/?utm_source=github&utm_medium=logo" target="_blank">
    <picture>
      <source srcset="https://embrace.io/docs/images/embrace_logo_white-text_transparent-bg_400x200.svg" media="(prefers-color-scheme: dark)" />
      <source srcset="https://embrace.io/docs/images/embrace_logo_black-text_transparent-bg_400x200.svg" media="(prefers-color-scheme: light), (prefers-color-scheme: no-preference)" />
      <img src="https://embrace.io/docs/images/embrace_logo_black-text_transparent-bg_400x200.svg" alt="Embrace">
    </picture>
  </a>
</p>

Embrace's Unity SDK lets you bring the deep, introspective and native debugging power of Embrace into your Unity game or application.

[![codecov](https://codecov.io/gh/embrace-io/embrace-unity-sdk-internal/graph/badge.svg?token=1g3DrYjacn)](https://codecov.io/gh/embrace-io/embrace-unity-sdk-internal)

## Getting Started
The Unity documentation can be found [here](https://embrace.io/docs/unity/).

- [Go to our dashboard](https://dash.embrace.io/signup/) to create an account and get your API key
- Check our [guide](https://embrace.io/docs/unity/integration/) to integrate the SDK into your Unity project

## Building
It is important to note that the Unity SDK WILL NOT COMPILE after pulling down. This is because it depends directly on the [Apple SDK](https://github.com/embrace-io/embrace-apple-sdk), and statically links to the xcframeworks as part of the build process.

To build, download the latest SUPPORTED release of the Apple SDK and place the xcframeworks in the `Embrace Unity iOS Interface/xcframeworks/`. You can find this information notated in the [Unity SDK changelog](https://embrace.io/docs/unity/changelog/). Then run the script `build_xcframework.sh` to construct the missing Unity specific xcframework. 

Then, copy the contents of the iOS release into `io.embrace.sdk/iOS`, including `run.sh` and `embrace_symbol_upload.darwin`. Leave everything in its relative position. 

Next copy the final constructed xcframework into the `io.embrace.sdk/iOS/xcframeworks` folder; it should be named `EmbraceUnityiOS.xcframework`.

Finally, make sure to mark all xcframeworks as iOS only plugins (to avoid build blocking issues on Android).

If you run into any issues building the SDK locally, please reach out on our [community Slack](https://embraceio-community.slack.com/archives/C078WQ3DJMC)!

Unfortunately, it is not currently possible to invoke the required iOS specific commands on Windows. However, for ease of use we have provided a bash script to do all of this for you on Apple devices: setup_iOS_build_environment.sh 

This pipeline requires: xcpretty, xcodebuild, and create_xcframework
To install XCode Build Tools however works best for you and your team
Then invoke the following commands in your shell/terminal:

```
brew install segment-integrations/formulae/swift-create-xcframework
gem install xcpretty
```

## Usage

- Refer to our [Features page](https://embrace.io/docs/unity/features/) to learn about the features Embrace SDK provides

## Support & Contributions
Embrace does not currently accept pull requests from external contributors. If you have a feature suggestion or have spotted something that doesn't look right please reach out in our [Community Slack](https://join.slack.com/t/embraceio-community/shared_invite/zt-ywr4jhzp-DLROX0ndN9a0soHMf6Ksow) for direct, faster assistance.

## License

[![Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-orange)](./LICENSE.txt)

Embrace Unity SDK is published under the Apache-2.0 license.

See the [LICENSE](https://github.com/embrace-io/embrace-unity-sdk/blob/main/LICENSE.txt) 
for full details.
