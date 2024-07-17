//
//  EmbraceConfig.h
//  Embrace
//
//  Created by Juan Pablo on 23/10/2018.
//  Copyright © 2018 embrace.io. All rights reserved.
//

#import <Foundation/Foundation.h>

/**
 The Embrace sdk configurations. This is used to setup configurations.
 */
@interface EmbraceConfig : NSObject

/**
 Returns the default config. The first time this is called it synchronously reads
 Embrace-Info.plist from disk.
 Returns nil if a valid plist file is not find at default path.
 */
- (nullable instancetype)initWithDefaultConfig;

/**
 Initializes a customized instance of EmbraceConfig from the file at the given plist file path. This
 will read the file synchronously from disk.
 
 @param path Embrace info property list file path.
 
 @return nil if a valid plist file is not find at path.
 */
- (nullable instancetype)initWithContentsOfFile:(nonnull NSString *)path;

/**
 Initializes a customized instance of EmbraceConfig with required fields.
 
 @param apiKey The unique Embrace API key that identifies your application.

 @return nil if an invalid app ID is specified.
 */
- (nullable instancetype)initWithAPIKey:(nonnull NSString *)apiKey;

/**
 The Embrace app ID. This is used to identify the app within the database.
 
 @note Plist detail
    - Key: API_KEY
    - Type: String
    - Default: N/A
 */
@property(atomic, strong, readonly, nonnull) NSString *APIKey;

// MARK: Represents the base URLs element specified in the Embrace config file.

/**
 Data base URL.
 
 @note Plist detail
    - Key: DATA_BASE_URL
    - Type: String
    - Default: data.emb-api.com
 */
@property(atomic, strong, readonly, nullable) NSString *baseURL;

/**
 Data dev base URL.
 
 @note Plist detail
    - Key: DATA_DEV_BASE_URL
    - Type: String
    - Default: data-dev.emb-api.com
 */
@property(atomic, strong, readonly, nullable) NSString *devBaseURL;

/**
 Config base URL
 
 @note Plist detail
    - Key: CONFIG_BASE_URL
    - Type: String
    - Default: config.emb-api.com
 */
@property(atomic, strong, readonly, nullable) NSString *configBaseURL;

/**
 Images base URL.
 
 @note Plist detail
    - Key: IMAGES_BASE_URL
    - Type: String
    - Default: images.emb-api.com
 */
@property(atomic, strong, readonly, nullable) NSString *imagesBaseURL;

/**
 URLSessionDelegate proxying filters, if a string from this filter is found, Embrace will not proxy that URLSession's delegate
 Tasks associated with the session are still proxied.

 @note Plist detail
 - Key: URLSESSION_CAPTURE_FILTERS
 - Type: Array<String>
 - Default: nil
 */
@property (atomic, strong, readonly, nullable) NSArray<NSString *> *urlSessionCaptureFilters;

// MARK: Represents the crash handler element specified in the Embrace config file.

/**
 DEPRECATED. Control whether the Embrace SDK automatically attaches to the uncaught exception handler.
 
 @note Plist detail
    - Key: CRASH_REPORT_ENABLED
    - Type: Boolean
    - Default: false
 */
@property (atomic, strong, readonly, nullable) NSNumber *crashReportEnabled DEPRECATED_MSG_ATTRIBUTE("Please replace this property by crashReportProvider");

/**
 Control whether the crash report provider to handle the exceptions. Allowed values are `embrace`,  `crashlytics`, or `none`
 
 @note Plist detail
    - Key: CRASH_REPORT_PROVIDER
    - Type: String
    - Default: Will default to using the Embrace crash reporter.
 */
@property (atomic, strong, readonly, nullable) NSString *crashReportProvider;

// MARK: Represents the startup moment configuration element specified in the Embrace config file.

/**
 Control whether the startup moment is automatically ended.
 
 @note Plist detail
    - Key: STARTUP_AUTOEND_SECONDS
    - Type: Number
    - Default: nil
 */
@property (atomic, strong, readonly, nullable) NSNumber *startupAutoendSeconds;

/**
 Control whether startup moment screenshots are taken.
 
 @note Plist detail
    - Key: STARTUP_MOMENT_SCREENSHOT_ENABLED
    - Type: Boolean
    - Default: true
 */
@property(atomic, assign, readonly) BOOL startupScreenshotEnabled;

// MARK: Represents the networking configuration element specified in the Embrace config file.

/**
 The Trace ID Header that can be used to trace a particular request.
 
 @note Plist detail
    - Key: TRACE_ID_HEADER_NAME
    - Type: String
    - Default: x-emb-trace-id
 */
@property(atomic, strong, readonly, nullable) NSString *traceIdHeader;

/**
 The default capture limit for the specified domains.
 
 @note Plist detail
    - Key: DEFAULT_CAPTURE_LIMIT
    - Type: Number
    - Default: nil
 */
@property (atomic, strong, readonly, nullable) NSNumber *networkCaptureLimit;

/**
 List of domains to be limited for tracking.
 
 @note Plist detail
    - Key: DOMAINS
    - Type: Dictionary
    - Default: nil
 */
@property (atomic, strong, readonly, nullable) NSDictionary<NSString *, NSNumber *> *networkCaptureDomains;

/**
 URLs that should not be captured.
 
 @note Plist detail
    - Key: DISABLED_URL_PATTERNS
    - Type: Array
    - Default: nil
 */
@property(atomic, strong, readonly, nullable) NSArray *disabledUrlPatterns;

/**
 This is a dictionary that contains two keys and is used like x-emb-path except that it's value is auto generated from a value of an http header.
 The auto generated value is <Domain>/<custom_path>/<header_value>
    - HEADER : The name of the http header thats value is used to replace <header_value> above
    - RELATIVE_URL_PATH : A string that is used to fill in <custom_path> above
 
 @note this will only work if x-emb-path is not present in the header. So x-emb-path is prioritized
 @note Plist detail
    - Key: CUSTOM_PATH_HEADER_INFO
    - Type: Dictionary
    - Default: nil
 */

@property(atomic, strong, readonly, nullable) NSDictionary* customRelativeHeaderInfo;

/**
 URLs that should not be captured.
 
 @note Plist detail
    - Key: IGNORE_CANCELLED_REQUESTS
    - Type: Array
    - Default: nil
 */
@property(atomic, strong, readonly, nullable) NSArray *ignoreCancelledRequests;

/**
 Control whether network request metrics is captured.
 
 @note This configuration has been deprecated as the SDK no longer captures these highly detailed network metrics
    - Key: COLLECT_NETWORK_REQUEST_METRICS
    - Type: Boolean
    - Default: true
 */
@property(atomic, assign, readonly) BOOL collectNetworkRequestMetrics;

/**
 Control whether NSURLConnection proxy is enabled.
 
 @note Plist detail
    - Key: NSURLCONNECTION_PROXY_ENABLE
    - Type: Boolean
    - Default: true
 */
@property(atomic, assign, readonly) BOOL nsurlconnectionProxyEnable;

/**
 Public RSA key to encrypt and store the network capture payload as a base64 ng.

 Inlcude your public RSA key here, network body capture will be fully encrypted and only you can decrypt it using your private key.
 
 @note Plist detail
    - Key: CAPTURE_PUBLIC_KEY
    - Type: String
    - Default: nil
 */
@property(atomic, strong, readonly, nullable) NSString *networkCapturePublicKey;

/**
 Specify if the SDK will capture network requests automatically.
 
 @note Plist detail
    - Key: NETWORK_CAPTURE_ENABLED
    - Type: Number
    - Default: YES
 */
@property(atomic, strong, readonly, nonnull) NSNumber* isNetworkCaptureEnabled;

// MARK: Represents the session configuration element specified in the Embrace config file.

/**
 Specify a maximum time before a session is allowed to exist before it is ended.
 
 @note Plist detail
    - Key: MAX_SESSION_SECONDS
    - Type: Number
    - Default: nil
 */
@property(atomic, strong, readonly, nullable) NSNumber *maxSessionSeconds;

// MARK: Represents the webView configuration element specified in the Embrace config file.

/**
 Specify a webview URL maximum length .
 
 @note Plist detail
    - Key: WEBVIEW_URL_LENGTH
    - Type: Number
    - Default: 1024
 */
@property(atomic, assign, readonly) NSUInteger webviewURLLength;

/**
 Control whether webview query parameters are captured.
 
 @note Plist detail
    - Key: WEBVIEW_STRIP_QUERYPARAMS
    - Type: Boolean
    - Default: false
 */
@property(atomic, assign, readonly) BOOL webviewStripQueryparams;

/**
 Control whether webview information is captured.
 
 @note Plist detail
    - Key: WEBVIEW_ENABLE
    - Type: Boolean
    - Default: true
 */
@property(atomic, assign, readonly) BOOL webviewEnable;

/**
 Embrace will always report on WKWebView content thread terminations when they happen.

 Optionally, you can enable this setting to have Embrace call Reload on the WKWebView for you.
 Note: If your application uses third party WKWebView content, such as advertising SDKS, it is recommended to leave this setting off
 
 @note Plist detail
    - Key: ENABLE_WK_AUTO_RELOAD
    - Type: Boolean
    - Default: false
 */
@property(atomic, assign, readonly) BOOL wttAutoReloadEnabled;

// MARK: Represents the system configuration element specified in the Embrace config file.

/**
 Control whether tap coordinates are captured.
 
 @note Plist detail
    - Key: CAPTURE_COORDINATES
    - Type: Boolean
    - Default: true
 */
@property(atomic, assign, readonly) BOOL captureCoordinatesEnabled;

/**
 Control whether tapped element names are captured.

 @note Plist detail
    - Key: CAPTURE_TAPPED_ELEMENTS
    - Type: Boolean
    - Defult: true
 */
@property(atomic, assign, readonly) BOOL captureTappedElementsEnabled;

/**
 Control whether automatic view capture is enabled, disable this if you are using custom view API.
 
 @note Plist detail
    - Key: ENABLE_AUTOMATIC_VIEW_CAPTURE
    - Type: Boolean
    - Default: true
 */
@property(atomic, assign, readonly) BOOL automaticViewCaptureEnabled;

/**
 Control whether automatic push notifications capture is enabled. Allowed values are `automatic`,  `manual`  or `disabled`.
 
 @note Plist detail
    - Key: PUSH_NOTIFICATIONS_CAPTURE_MODE
    - Type: String
    - Default: Disabled
 */
@property(atomic, strong, readonly, nullable) NSString *pushNotificationsCaptureMode;

/**
 Disable this to prevent the Embrace SDK from capturing any data from the Push Notifications payloads.
 Push Notifications will still be captured but only their timestamp and type will be recorded.
 
 @note Plist detail
    - Key: ENABLE_PUSH_NOTIFICATIONS_DATA_CAPTURE
    - Type: Boolean
    - Default: true
 */
@property(atomic, assign, readonly) BOOL pushNotificationsDataCaptureEnabled;

/**
 The App Group Identifier. This is used to allow app extensions to share information with the host app.
 
 @note Plist detail
    - Key: APP_GROUP_IDENTIFIER
    - Type: String
    - Default: N/A
 */
@property (atomic, strong, readonly, nullable) NSString *appGroupIdentifier;

@end
