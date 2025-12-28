<p align="center">
  <a href="https://embrace.io/?utm_source=github&utm_medium=logo" target="_blank">
    <picture>
      <source srcset="https://embrace.io/docs/images/embrace_logo_white-text_transparent-bg_400x200.svg" media="(prefers-color-scheme: dark)" />
      <source srcset="https://embrace.io/docs/images/embrace_logo_black-text_transparent-bg_400x200.svg" media="(prefers-color-scheme: light), (prefers-color-scheme: no-preference)" />
      <img src="https://embrace.io/docs/images/embrace_logo_black-text_transparent-bg_400x200.svg" alt="Embrace">
    </picture>
  </a>
</p>

Embrace's Unity SDK lets you bring the deep, introspective and native debugging
power of Embrace into your Unity game or application.

[![codecov](https://codecov.io/gh/embrace-io/embrace-unity-sdk-internal/graph/badge.svg?token=1g3DrYjacn)](https://codecov.io/gh/embrace-io/embrace-unity-sdk-internal)

## Getting Started

The Unity documentation can be found [here](https://embrace.io/docs/unity/).

- [Go to our dashboard](https://dash.embrace.io/signup/) to create an account
  and get your API key
- Check our [guide](https://embrace.io/docs/unity/integration/) to integrate the
  SDK into your Unity project

## Building

It is important to note that the Unity SDK is not usable immediately after
cloning the repository. This is because it depends directly on the
[Apple SDK](https://github.com/embrace-io/embrace-apple-sdk), and statically
links to the xcframeworks as part of the build process.

To build the SDK, run the following command from the root of the project:

```bash
make
```

This will automatically pull down the dependencies and build the Unity
xcframeworks, and copy everything into the io.embrace.sdk folder. Unfortunately,
it is not currently possible to invoke the required iOS specific commands on
Windows. This pipeline requires xcpretty (`gem install xcpretty`) and the XCode
Build Tools to be installed.

If you run into any issues building the SDK locally, please reach out on our
[community Slack](https://embraceio-community.slack.com/archives/C078WQ3DJMC)!

## Publishing to npm

The iOS symbol upload tools (`run.sh` and `embrace_symbol_upload.darwin`) are
required for the npm package but are excluded from git via `.gitignore`. Before
publishing to npm, these dependencies will be automatically downloaded via the
`prepublishOnly` script, which runs `make install_ios_dependencies`.

To publish to npm:

```bash
cd io.embrace.sdk
npm publish
```

The `prepublishOnly` script will automatically download the required iOS tools
from https://downloads.embrace.io/embrace_support.zip before packaging.

**Note:** For local development and building, you must manually run `make` from
the repository root to download iOS dependencies and build the SDK. The
`prepublishOnly` script only runs automatically during `npm publish`.

## Usage

- Refer to our [Features page](https://embrace.io/docs/unity/features/) to learn
  about the features Embrace SDK provides

## Support & Contributions

We appreciate any feedback you have on the SDK and the APIs that it provides.

To contribute to this project please see our [Contribution Guidelines]. After
completing the Individual Contributor License Agreement (CLA), you'll be able to
submit a feature request, create a bug report, or submit a pull request.

For urgent matters (such as outages) or issues concerning the Embrace service or
UI, reach out in our [Community Slack] for direct, faster assistance.

[Contribution Guidelines]:
  https://github.com/embrace-io/embrace-unity-sdk/blob/main/CONTRIBUTING.md
[Community Slack]: https://community.embrace.io

## License

[![Apache-2.0](https://img.shields.io/badge/license-Apache--2.0-orange)](./LICENSE.txt)

Embrace Unity SDK is published under the Apache-2.0 license.

See the
[LICENSE](https://github.com/embrace-io/embrace-unity-sdk/blob/main/LICENSE.txt)
for full details.
