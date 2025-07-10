namespace EmbraceSDK.EditorView
{
    public static class EmbraceTooltips
    {
        public const string AppId = "The 5-character App ID assigned to your app in the Embrace dashboard during creation.";
        public const string ApiToken = "This is the 32-digit hexadecimal token for your Embrace account that allows upload of symbolication files.";
        public const string MaxSessionSeconds = "Automate the end of the session every x amount of seconds (Min of 60 sec and Max of 604800)";
        public const string NdkEnabled = "Enable Native Development Kit (NDK) a set of tools that allows you to use C and C++ code with Android.";
        public const string ReportDiskUsage = "Selects whether the SDK collects disk usage for the app.";
        public const string CrashHandler = "Sets whether to enable the SDK from connecting to the uncaught exception handler.";
        public const string CaptureRequestContentLength = "Disable the gzip encoding for network calls.";
        public const string EnableNativeMonitoring = "Enable to capture network requests done through the Java URLConnection class.";
        public const string TrackIdHeader = "Sets the name of the header used for the trace ID. Defaults to \"x-emb-trace-id\".";
        public const string AsyncEnd = "Session messages will be uploaded asynchronously at the end of sessions. Enable this to reduce the chance that an upload will cause an ANR. Disable if you prefer to avoid some amount of sessions being uploaded in the following session.";
        public const string AutomaticallyEnd = "Sets whether the startup moment is automatically ended.";
        public const string CaptureQueryParams = "Sets whether to enable capturing of web view query parameters.";
        public const string WebViewEnableAndroid = "Sets whether to enable capturing of web views.";
        public const string CrashReportEnabled = "Selects whether you want to enable crash reporting.";
        public const string CrashReportProvider = "Selects which crsah reporter will be used.\n\nEmbrace - Use Embrace's internal crash reporter (default)\n\nCrashlytics - Use Crashlytics as the crash reporter\n\nNone - Disable crash reporting";
        public const string StartupMomentScreenshotEnabled = "Selects whether the Embrace startup moment will take a screenshot on completion.";
        public const string CaptureCoordinates = "Selects whether to capture the tap coordinates within the app. When disabled, Embrace still captures taps, but without exact coordinates.";
        public const string CaptureTappedElements = "Selects whether to capture of tap element names within the app. When disabled, Embrace still captures taps but not the name of the tapped element.";
        public const string BackgroundFetchCaptureEnable = "Selects whether Embrace will swizzle and capture requests made via the background task downloading API.";
        public const string CollectNetworkRequestMetrics = "Selects whether Embrace will capture detailed performance statistics about network requests. The default is on.";
        public const string EnableAutomaticViewCapture = "Selects whether Embrace will automatically capture all displayed view controllers. This can help give you useful timeline data for your sessions.";
        public const string EnableWkAutoReload = "Selects whether Embrace can perform some automatic webkit management for you. This is off by default as not all apps can safely use it";
        public const string DisabledUrlPatterns = "Use this field to specify an array of regex strings that prevent network requests with matching URLs from being captured";
        public const string UrlSessionCaptureFilters = "Selects whether Embrace will ignore the specified URLSessions entirely. Class names that match the regex strings in this array are not swizzled.";
        public const string StartupAutoEndSeconds = "Selects whether the SDK will attempt to automatically end the startup moment when the application settles.";
        public const string WebViewStripQueryParams = "Disables the capture of query parameters for webview URLs in the session.";
        public const string WebViewEnableIOS = "Selects whether the WKWebview capture feature is enabled. When disabled no WKWebview’s will be swizzled or recorded in your session data";
        public const string Network = "This dictionary can be added to the plist to allow more fine grained control of each URL used by the application.";
        public const string DefaultCaptureLimit = "Sets a default limit for how many instances of any given domain to capture in a single session.";
        public const string Domains = "This dictionary maps domains to capture limits. Any domain not in this list will use the DEFAULTCAPTURELIMIT.";
        public const string CapturePublicKey = "When present, the value in this field is used as a public RSA key to encrypt any captured network data to protect PII.";
        public const string NsurlConnectionProxyEnable = "Selects whether the capture of URLConnection requests within the SDK are enabled. When disabled no URLConnection objects are swizzled or recorded.";
        public const string TraceIdHeaderName = "Sets the name of the header added to all network requests. This may be required for certain server configurations.";
        public const string CustomPathHeaderInfo = "This dictionary is for auto-generating relative paths for network requests similar to how x-emb-path works.";
        public const string BaseUrls = "Mock API Tool base URLs.";
        public const string ScriptingDefineSymbols = "These settings determine which Embrace SDK configuration symbols to define in list found at:\n\"Project Settings > Player > Other Settings > Script Compliation\"";
        public const string DataDir = "Sets the directory where the Embrace SDK configuration data will be stored. The path is relative to the Assets folder.";
        public const string DataDirButton = "Setting a new data directory will relocate your existing configuration data.";
        public const string AutomatedNetworkCaptureWeaving = "When enabled, Embrace will weave network capture code into script assemblies to automatically capture network calls through UnityWebRequest and HttpClient.";
        public const string WeaverEditorOnly = "When enabled, automated network capture weaving will be skipped while in editor to reduce compile time.";
        public const string CaptureDataProcessingErrors = "When enabled, data processing errors from UnityWebRequest DownloadHandlers will be logged automatically even if the request itself is successful.";
        public const string StartupSpanCapture = "When enabled, Embrace will capture a span from the time the app starts until the various startup tasks are completed. This is useful for understanding the startup performance of your app.";
    }
}