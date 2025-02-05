//
// Copyright © 2025 Embrace Mobile, Inc. All rights reserved.
//
// Created for internal use by the Embrace Unity SDK. Edit at your own risk.
//

struct ConfigOptions: OptionSet {
    let rawValue: Int

    static let Default = ConfigOptions([]) // Equivalent to rawValue: 0
    static let DisableEmbraceCrashReporter = ConfigOptions(rawValue: 1 << 0)
    static let DisableEmbraceNativeViewCaptureService = ConfigOptions(rawValue: 1 << 1)
    static let DisableEmbraceNativePushNotificationCaptureSerivce = ConfigOptions(rawValue: 1 << 2)
}
