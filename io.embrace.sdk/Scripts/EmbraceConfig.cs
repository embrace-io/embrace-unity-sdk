using System;

namespace EmbraceSDK
{
    /// <summary>
    /// Configuration options for the Embrace Native SDKs (iOS and Android).
    /// This is currently only used for iOS.
    /// Primarily used to disable specific Embrace services, either because
    /// you don't want to use them or because you are using a different service.
    /// </summary>
    [Flags]
    public enum EmbraceNativeConfig : int
    {
        Default =  0,
        DisableEmbraceCrashReporter = 1 << 0,
        DisableEmbraceNativeViewCaptureService = 1 << 1,
        DisableEmbraceNativePushNotificationCaptureSerivce = 1 << 2,
    }    
}
