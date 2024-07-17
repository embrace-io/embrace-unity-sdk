//
//  Embrace.h
//  Embrace
//
//  Created by Brian Wagner on 9/12/16.
//  Copyright © 2016 embrace.io. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <Embrace/EmbraceConfig.h>
#import <Embrace/EMBConstants.h>
#import <Embrace/EMBCustomFlow.h>
#import <Embrace/EMBNetworkRequest.h>
#import <Embrace/EMBPurchaseFlow.h>
#import <Embrace/EMBRegistrationFlow.h>
#import <Embrace/EMBSubscriptionPurchaseFlow.h>
#import <Embrace/RNEmbrace.h>
#import <Embrace/EMBFlutterEmbrace.h>
#import <Embrace/EmbraceExtension.h>
#import <Embrace/EmbraceTracer.h>

#if __has_include(<WebKit/WebKit.h>)
#import <WebKit/WebKit.h>
#endif
/**
 Project version number for the Embrace framework.
 */
FOUNDATION_EXPORT double EmbraceVersionNumber;

/**
 Project version string for the Embrace framework.
 */
FOUNDATION_EXPORT const unsigned char EmbraceVersionString[];

/**
 The EMBDelegate is used to notify the application of updates from the Embrace.io SDK. Currently the only update that will be
 delivered is whether the Embrace.io SDK is active for the given app launch.
 */
@protocol EmbraceDelegate <NSObject>

@optional

/**
 Optional callback that gets called to inform the user whether the Embrace.io SDK is enabled or disabled for the current
 app user. It is also called when the status changes from enabled to disabled or vice versa.
 
 @param enabled A bool indicating whether the Embrace.io SDK is on or off for the current application instance.
 */
- (void)embraceSDKIsEnabled:(BOOL)enabled;

@end

/**
 Entry point for Embrace SDK.
 */
@interface Embrace : NSObject <EmbraceTracer>

/**
 Optional delegate property that can be set to receive callbacks about the Embrace.io SDK's operations
 */
@property (nonatomic, weak, nullable) id<EmbraceDelegate> delegate;

/*
 Returns YES if the any "start" function variants have been called.
 */

@property (nonatomic) BOOL isStarted;

/*
 Returns EMBLastRunStateCrash if there was a crash since the last cold start.
 Otherwise it returns EMBLastRunStateCleanExit
 If called before start* functions, will always return EMBLastRunStateInvalid
 
 note, this will only work with the Embrace CRASH_REPORT_PROVIDER. If called when
 using Crashlaytic CRASH_REPORT_PROVIDER then it will always return EMBLastRunStateInvalid.
 */

@property (nonatomic, readonly) EMBLastRunEndState lastRunEndState;

/**
 Returns the shared `Embrace` singleton object.
 */
+ (nonnull instancetype)sharedInstance;

/**
 Performs the initial setup of the Embrace SDK with the default config file if present
 */
- (void)start;

/**
 Performs the initial setup of the Embrace SDK with the default config file if present
 
 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].
 */
- (void)startWithLaunchOptions:(nullable NSDictionary *)launchOptions;

/**
 Performs the initial setup of the Embrace SDK with the default config file if present
 
 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].
 
 @param framework The framework used by the app, e.g. EMBAppFrameworkReactNative.
 */
- (void)startWithLaunchOptions:(nullable NSDictionary *)launchOptions
                     framework:(EMBAppFramework)framework;

/**
 Performs the initial setup of the Embrace SDK with the default config file if present
 
 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].
 
 @param framework The framework used by the app, e.g. EMBAppFrameworkReactNative.
 
 @param enableIntegrationHelp If enabled (and only in development), the SDK will show an alert view when there's a critical error during the initialization process.
 */
- (void)startWithLaunchOptions:(nullable NSDictionary *)launchOptions
                     framework:(EMBAppFramework)framework
         enableIntegrationHelp:(BOOL)enableIntegrationHelp;

/**
 Performs the initial setup of the Embrace SDK with a custom EmbraceConfig.
 
 @param config The Embrace application object used to configure the service.
 */
- (void)startWithConfig:(nonnull EmbraceConfig *)config;

/**
 Performs the initial setup of the Embrace SDK with a custom EmbraceConfig.
 
 @param config The Embrace application object used to configure the service.
 
 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].
 */
- (void)startWithConfig:(nonnull EmbraceConfig *)config
          launchOptions:(nullable NSDictionary<UIApplicationLaunchOptionsKey, id> *)launchOptions;

/**
 Performs the initial setup of the Embrace SDK with a custom EmbraceConfig.
 
 @param config The Embrace application object used to configure the service.
 
 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].
 
 @param framework The framework used by the app, e.g. EMBAppFrameworkReactNative.
 */
- (void)startWithConfig:(nonnull EmbraceConfig *)config
          launchOptions:(nullable NSDictionary<UIApplicationLaunchOptionsKey, id> *)launchOptions
              framework:(EMBAppFramework)framework;

/**
 Performs the initial setup of the Embrace SDK with a custom EmbraceConfig.
 
 @param config The Embrace application object used to configure the service.
 
 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].
 
 @param framework The framework used by the app, e.g. EMBAppFrameworkReactNative.
 
 @param enableIntegrationHelp If enabled (and only in development), the SDK will show an alert view when there's a critical error during the initialization process.
 */
- (void)startWithConfig:(nonnull EmbraceConfig *)config
          launchOptions:(nullable NSDictionary<UIApplicationLaunchOptionsKey, id> *)launchOptions
              framework:(EMBAppFramework)framework
  enableIntegrationHelp:(BOOL)enableIntegrationHelp;

/**
 Performs the initial setup of the Embrace SDK with the provided API key.
 
 This puts in place device and network monitoring, kicks off reporting, and marks the beginning of the "App Startup" event.
 
 @param apiKey The unique Embrace API key that identifies your application.
 */
- (void)startWithKey:(nonnull NSString *)apiKey;

/**
 Performs the initial setup of the Embrace SDK with the provided API key.
 
 This puts in place device and network monitoring, kicks off reporting, and marks the beginning of the "App Startup" event.
 
 @param apiKey The unique Embrace API key that identifies your application.
 
 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].
 */
- (void)startWithKey:(nonnull NSString *)apiKey
       launchOptions:(nullable NSDictionary<UIApplicationLaunchOptionsKey, id> *)launchOptions;

/**
 Performs the initial setup of the Embrace SDK with the provided API key.

 This puts in place device and network monitoring, kicks off reporting, and marks the beginning of the "App Startup" event.

 @param apiKey The unique Embrace API key that identifies your application.

 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].

 @param framework The framework used by the app, e.g. EMBAppFrameworkReactNative.
 */
- (void)startWithKey:(nonnull NSString *)apiKey
       launchOptions:(nullable NSDictionary<UIApplicationLaunchOptionsKey, id> *)launchOptions
           framework:(EMBAppFramework)framework;

/**
 Performs the initial setup of the Embrace SDK with the provided API key.

 This puts in place device and network monitoring, kicks off reporting, and marks the beginning of the "App Startup" event.

 @param apiKey The unique Embrace API key that identifies your application.

 @param launchOptions The launchOptions as passed to [UIApplicationDelegate application:didFinishLaunchingWithOptions:].

 @param framework The framework used by the app, e.g. EMBAppFrameworkReactNative.
 
 @param enableIntegrationHelp If enabled (and only in development), the SDK will show an alert view when there's a critical error during the initialization process.
 */
- (void)startWithKey:(nonnull NSString *)apiKey
       launchOptions:(nullable NSDictionary<UIApplicationLaunchOptionsKey, id> *)launchOptions
           framework:(EMBAppFramework)framework
enableIntegrationHelp:(BOOL)enableIntegrationHelp;

/**
 Manually forces the end of the current session and starts a new session.
 
 Only call this method if you have an application that will stay in the foreground for an extended time, such as a
 point-of-sale app.
 */
- (void)endSession;

/**
 Manually forces the end of the current session and starts a new session.
 
 Only call this method if you have an application that will stay in the foreground for an extended time, such as a
 point-of-sale app.

 @param clearUserInfo Clear any username, user ID, and email values set when ending the session.
 */
- (void)endSession:(BOOL)clearUserInfo;

/**
 Marks the end of the "App Startup" event.
 */
- (void)endAppStartup;

/**
 Marks the end of the "App Startup" event with optional properties added
 @param properties An optional dictionary containing metadata about the moment to be recorded (limited to 10 keys).
 */
- (void)endAppStartupWithProperties:(nullable EMBProperties *)properties;

/**
 Starts recording data for an app moment with the provided name.
 
 If another app moment with the provided name is in progress, it will be overwritten.
 
 @param name The name used to identify the app moment
 */
- (void)beginEventWithName:(nonnull NSString *)name DEPRECATED_MSG_ATTRIBUTE("Please replace calls to start app moments with methods of the form startMomentWithName:");

/**
 Starts recording data for an app moment with the provided name.

 If another app moment with the provided name is in progress, it will be overwritten.

 @param name The name used to identify the app moment
*/
- (void)startMomentWithName:(nonnull NSString *)name;

/**
 Starts recording data for an app moment with the provided name and identifier.
 
 Identifiers can be used to separately log data for different instances of a given moment, e.g. an image download.
 All moments will be aggregated by name—the identifier is purely for avoiding naming collisions.
 A start event with a given name, identifier pair will overwrite any existing app moments with the same name and identifier.
 
 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to create a unique key for the moment.
 */
- (void)beginEventWithName:(nonnull NSString *)name
                identifier:(nullable NSString *)identifier DEPRECATED_MSG_ATTRIBUTE("Please replace calls to start app moments with methods of the form startMomentWithName:identifier:");

/**
 Starts recording data for an app moment with the provided name and identifier.

 Identifiers can be used to separately log data for different instances of a given moment, e.g. an image download.
 All moments will be aggregated by name—the identifier is purely for avoiding naming collisions.
 A start event with a given name, identifier pair will overwrite any existing app moments with the same name and identifier.

 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to create a unique key for the moment.
 */
- (void)startMomentWithName:(nonnull NSString *)name
                 identifier:(nullable NSString *)identifier;

/**
 Starts recording data for an app moment with the provided name, optional identifier, and option to take a screenshot.
 Screenshots will be taken if the SDK detects that the moment is late (taking longer than it should).
 Note: screenshots will be rate-limited, so if two moments trigger screenshots within 1 second of each other, only
 one screenshot will be taken.
 
 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to create a unique key for the moment (can be nil).
 @param allowScreenshot A flag for whether to enable screenshot functionality if the moment is late (defaults to YES).
 */
- (void)beginEventWithName:(nonnull NSString *)name
                identifier:(nullable NSString *)identifier
           allowScreenshot:(BOOL)allowScreenshot DEPRECATED_MSG_ATTRIBUTE("Please replace calls to start app moments with methods of the form startMomentWithName:identifier:allowScreenshot:");

/**
 Starts recording data for an app moment with the provided name, optional identifier, and option to take a screenshot.
 Screenshots will be taken if the SDK detects that the moment is late (taking longer than it should).
 Note: screenshots will be rate-limited, so if two moments trigger screenshots within 1 second of each other, only
 one screenshot will be taken.

 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to create a unique key for the moment (can be nil).
 @param allowScreenshot A flag for whether to enable screenshot functionality if the moment is late (defaults to YES).
 */
- (void)startMomentWithName:(nonnull NSString *)name
                 identifier:(nullable NSString *)identifier
            allowScreenshot:(BOOL)allowScreenshot  DEPRECATED_MSG_ATTRIBUTE("Please replace calls to start app moments with methods of the form startMomentWithName:identifier:");

/**
 Starts recording data for an app moment with the provided name, optional identifier, and optional key/value metadata
 
 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to create a unique key for the moment (can be nil).
 @param properties An optional dictionary containing metadata about the moment to be recorded (limited to 10 keys).
 */
- (void)startMomentWithName:(nonnull NSString *)name
                 identifier:(nullable NSString *)identifier
                 properties:(nullable EMBProperties *)properties;

/**
 Starts recording data for an app moment with the provided name, optional identifier, screenshot flag, and optional key/value metadata
 
 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to create a unique key for the moment (can be nil).
 @param allowScreenshot A flag for whether to take a screenshot if the moment is late (defaults to YES).
 @param properties An optional dictionary containing metadata about the moment to be recorded (limited to 10 keys).
 */
- (void)beginEventWithName:(nonnull NSString *)name
                identifier:(nullable NSString *)identifier
           allowScreenshot:(BOOL)allowScreenshot
                properties:(nullable EMBProperties *)properties DEPRECATED_MSG_ATTRIBUTE("Please replace calls to start app moments with methods of the form startMomentWithName:identifier:allowScreenshot:properties:");

/**
 Starts recording data for an app moment with the provided name, optional identifier, screenshot flag, and optional key/value metadata
 
 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to create a unique key for the moment (can be nil).
 @param allowScreenshot A flag for whether to take a screenshot if the moment is late (defaults to YES).
 @param properties An optional dictionary containing metadata about the moment to be recorded (limited to 10 keys).
 */
- (void)startMomentWithName:(nonnull NSString *)name
                 identifier:(nullable NSString *)identifier
            allowScreenshot:(BOOL)allowScreenshot
                 properties:(nullable EMBProperties *)properties  DEPRECATED_MSG_ATTRIBUTE("Please replace calls to start app moments with methods of the form startMomentWithName:identifier:properties:");

/**
 Stops recording data for an app moment with the provided name.
 
 This marks the moment as "completed."
 If no moment is found with the provided name (and an empty identifier), this call will be ignored.
 Additionally, if an app moment was started with a name and identifier, the same identifier must be used to end it.
 
 @param name The name used to identify the moment.
 */
- (void)endEventWithName:(nonnull NSString *)name DEPRECATED_MSG_ATTRIBUTE("Please replace calls to end app moments with methods of the form endMomentWithName:");

/**
 Stops recording data for an app moment with the provided name.
 
 This marks the moment as "completed."
 If no moment is found with the provided name (and an empty identifier), this call will be ignored.
 Additionally, if an app moment was started with a name and identifier, the same identifier must be used to end it.
 
 @param name The name used to identify the moment.
 */
- (void)endMomentWithName:(nonnull NSString *)name;

/**
 Stops recording data for an app moment with the provided name and adds properties to the moment.

 This marks the moment as "completed."
 If no moment is found with the provided name (and an empty identifier), this call will be ignored.
 Additionally, if an app moment was started with a name and identifier, the same identifier must be used to end it.

 @param name The name used to identify the moment.
 @param properties An optional dictionary containing metadata about the moment to be recorded (limited to 10 keys).
 */
- (void)endMomentWithName:(nonnull NSString *)name
               properties:(nullable EMBProperties *)properties;

/**
 Stops recording data for an app moment with the provided name and identifier.
 
 The moment that has the given name, identifier pair will be marked as "completed."
 If no moment is found with the given name AND identifier, this call will be ignored.
 
 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to uniquely identify the moment.
 */
- (void)endEventWithName:(nonnull NSString *)name
              identifier:(nullable NSString *)identifier DEPRECATED_MSG_ATTRIBUTE("Please replace calls to end app moments with methods of the form endMomentWithName:identifier:");

/**
 Stops recording data for an app moment with the provided name and identifier.
 
 The moment that has the given name, identifier pair will be marked as "completed."
 If no moment is found with the given name AND identifier, this call will be ignored.
 
 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to uniquely identify the moment.
 */
- (void)endMomentWithName:(nonnull NSString *)name
               identifier:(nullable NSString *)identifier;

/**
 Stops recording data for an app moment with the provided name and identifier, and adds properties to the moment.

 The moment that has the given name, identifier pair will be marked as "completed."
 If no moment is found with the given name AND identifier, this call will be ignored.

 @param name The name used to identify the moment.
 @param identifier An identifier that is combined with the name to uniquely identify the moment.
 @param properties An optional dictionary containing metadata about the moment to be recorded (limited to 10 keys).
 */
- (void)endMomentWithName:(nonnull NSString *)name
               identifier:(nullable NSString *)identifier
               properties:(nullable EMBProperties *)properties;

/**
 Annotates the session with a new property.  Use this to track permanent and ephemeral features of the session.
 A permanent property is added to all sessions submitted from this device, use this for properties such as work site, building, owner
 A non-permanent property is added to only the currently active session.
 
 There is a maximum of 10 total properties in a session.
 
 @param value The value to store for this property
 @param key The key for this property, must be unique within session properties
 @param permanent If true the property is applied to all sessions going forward, persist through app launches.
 
 @return A boolean indicating whether the property was added or not, see console log for details
 */
- (bool)addSessionProperty:(nonnull NSString *)value
                   withKey:(nonnull NSString *)key
                 permanent:(BOOL)permanent;

/**
 Removes a property from the session.  If that property was permanent then it is removed from all future sessions as well.
 
 @param key The key for the property you wish to remove
 */
- (void)removeSessionPropertyWithKey:(nonnull NSString *)key;

/**
 Get a read-only representation of the currently set session properties.  You can query and read from this representation however setting values in this object will not modify the actual properties in the session.
 To modify session properties see addSessionProperty.
 
 Properties are key-value pairs of NSString * objects.
 */
- (nonnull NSDictionary *)getSessionProperties;

/** Embrace ships with automatic view capturing enabled.  In this mode the SDK attempts to track and annotate all your view
 Presentations and logs them in the session.  In complex apps, or apps with unconvential UI (spritekit, swiftui, react) automatic capture
 might be unreliable or too noisy to be useful
 
 For such apps we also provide a manual view annotation API.  Using this API you decide when your app has entered and exited
 a view.  Maybe you want to consolidate many views under a single header (i.e. "ProfileView", instead of many individual labels).

 To use the API simply call "startViewWithName" whenever your app has entered a new view state and then call "endViewWithName"
 when the app exits that view state.  View states stack up to 20 levels deep and view states automatically close on session end.  This function will
 log a warning if you attempt to start a view that is already on the stack, or close a view that is not on the stack.
 
 @param name The name for this view state
 @return a boolean indicating whether the operation was successful or not (see console log for reasoning)
 */
- (bool)startViewWithName:(nonnull NSString *)name;

/**
 @see startViewWithName for full discussion of the manual view annotation system
 
 The endViewWithName function with close the view state for the specified view or log a warning if the view is not found
 
 @param name The name for this view state
 @return a boolean indicating whether the operation was successful or not (see console log for reasoning)
 */
- (bool)endViewWithName:(nonnull NSString *)name;

/**
 Logs an event in your application for aggregation and debugging on the Embrace.io dashboard.
 
 Events are grouped by name and severity.
 
 @param name The name of the message, which is how it will show up on the dashboard
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 */
- (void)logMessage:(nonnull NSString *)name
      withSeverity:(EMBSeverity)severity;

/**
 Logs an event in your application for aggregation and debugging on the Embrace.io dashboard
 with an optional dictionary of up to 10 properties.
 
 @param name The name of the message, which is how it will show up on the dashboard
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of up to 10 key/value pairs
 */
- (void)logMessage:(nonnull NSString *)name
      withSeverity:(EMBSeverity)severity
        properties:(nullable EMBProperties *)properties;

/**
 Logs an event in your application for aggregation and debugging on the Embrace.io dashboard
 with an optional dictionary of up to 10 properties and the ability to enable or disable a screenshot.
 
 @param name The name of the message, which is how it will show up on the dashboard
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of up to 10 key/value pairs
 @param takeScreenshot A flag for whether the SDK should take a screenshot of the application window to display on the dashboard
 */
- (void)logMessage:(nonnull NSString *)name
      withSeverity:(EMBSeverity)severity
        properties:(nullable EMBProperties *)properties
    takeScreenshot:(BOOL)takeScreenshot  DEPRECATED_MSG_ATTRIBUTE("Please replace calls to logMessage with methods of the form logMessage:severity:properties:");

/**
 Logs an INFO event in your application for aggregation and debugging on the Embrace.io dashboard.

 Events are grouped by name and severity.

 @param name The name of the message, which is how it will show up on the dashboard
 */
- (void)logInfo:(nonnull NSString *)name;

/**
 Logs a WARNING event in your application for aggregation and debugging on the Embrace.io dashboard.

 Events are grouped by name and severity.
 
 @param name The name of the message, which is how it will show up on the dashboard
 */
- (void)logWarning:(nonnull NSString *)name;

/**
 Logs an ERROR event in your application for aggregation and debugging on the Embrace.io dashboard.

 Events are grouped by name and severity.

 @param name The name of the message, which is how it will show up on the dashboard
 */
- (void)logError:(nonnull NSString *)name;

/**
 Logs an informative message to the Embrace.io API for aggregation and viewing on the dashboard.

 @param message The message used to find the log later, which is how it will be aggregated on the web dashboard
 @param properties An optional dictionary of custom key/value properties to be sent with the message
 */
- (void)logInfoMessage:(nonnull NSString *)message
            properties:(nullable EMBProperties *)properties DEPRECATED_MSG_ATTRIBUTE("Please replace calls to log info messages with methods of the form logMessage:withSeverity:");

/**
 Logs a warning message to the Embrace.io API for aggregation and viewing on the dashboard. Unlike info messages,
 warning messages will be accompanied by a screenshot and stack trace. The screenshot can be disabled with the `screenshot` argument.
 
 @param message The message used to find the warning later, which will be combined with the stack trace to aggregate the logs on the dashboard view.
 @param screenshot A flag for whether to take a screenshot or not.
 @param properties An optional dictionary of custom key/value properties to be sent with the warning log.
 */
- (void)logWarningMessage:(nonnull NSString *)message
               screenshot:(BOOL)screenshot
               properties:(nullable EMBProperties *)properties DEPRECATED_MSG_ATTRIBUTE("Please replace calls to log warning messages with methods of the form logMessage:withSeverity:");

/**
 Logs an error message to the Embrace.io API for aggregation and viewing on the dashboard. Unlike info messages,
 error messages will be accompanied by a screenshot and stack trace. The screenshot can be disabled with the `screenshot` argument.
 
 @param message The message used to find the error later, which will be combined with the stack trace to aggregate the logs on the dashboard view.
 @param screenshot A flag for whether to take a screenshot or not.
 @param properties An optional dictionary of custom key/value properties to be sent with the error log.
 */
- (void)logErrorMessage:(nonnull NSString *)message
             screenshot:(BOOL)screenshot
             properties:(nullable EMBProperties *)properties DEPRECATED_MSG_ATTRIBUTE("Please replace calls to log error messages with methods of the form logMessage:withSeverity:");

/**
 Logs an Error or NSError object to the Embrace.io API for aggregation on the dashboard. These errors will be treated similarly to
 error log messages, but the serialization will be done by Embrace. During the serialization calls, the error description,
 reason, domain, and code will be logged as properties. As with error logs, a stack trace will be taken, and
 optional screenshots and error properties can be specified.
 
 @param error The handled error object, which will be serialized and combined with the stack trace to aggregate the errors on the dashboard view.
 @param screenshot A flag for whether to take a screenshot or not.
 @param properties An optional dictionary of custom key/value properties to be sent with the error log.
 */
- (void)logHandledError:(nonnull NSError *)error
             screenshot:(BOOL)screenshot
             properties:(nullable EMBProperties *)properties DEPRECATED_MSG_ATTRIBUTE("Please replace calls to logHandledError with methods of the form logHandledError:properties:");

/**
 Logs an Error or NSError object to the Embrace.io API for aggregation on the dashboard. These errors will be treated similarly to
 error log messages, but the serialization will be done by Embrace. During the serialization calls, the error description,
 reason, domain, and code will be logged as properties. As with error logs, a stack trace will be taken, and
error properties can be specified.

 @param error The handled error object, which will be serialized and combined with the stack trace to aggregate the errors on the dashboard view.
 @param properties An optional dictionary of custom key/value properties to be sent with the error log.
 */
- (void)logHandledError:(nonnull NSError *)error
             properties:(nullable EMBProperties *)properties;

/**
 Logs a handled exception to the Embrace.io API for aggregation and viewing on the dashboard.
 The log will include the stacktrace found on the exception.
 
 @param exception The handled exception object, which will be serialized and combined with the stack trace to aggregate the errors on the dashboard view.
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 */
- (void)logHandledException:(nonnull NSException *)exception
               withSeverity:(EMBSeverity)severity;

/**
 Logs a handled exception to the Embrace.io API for aggregation and viewing on the dashboard.
 The log will include the stacktrace found on the exception.
 
 @param exception The handled exception object.
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param customStackTrace A custom Stack Trace to be logged and displayed on the dashboard view.
 */
- (void)logHandledException:(nonnull NSException *)exception
               withSeverity:(EMBSeverity)severity
           customStackTrace:(nonnull NSArray<NSString*> *)customStackTrace;

//NSArray<NSString *>
/**
 Logs a handled exception to the Embrace.io API for aggregation and viewing on the dashboard.
 The log will include the stacktrace found on the exception.
 
 @param exception The handled exception object, which will be serialized and combined with the stack trace to aggregate the errors on the dashboard view.
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of custom key/value properties to be sent with the error log.
 */
- (void)logHandledException:(nonnull NSException *)exception
               withSeverity:(EMBSeverity)severity
                 properties:(nullable EMBProperties *)properties;

/**
 Logs a handled exception to the Embrace.io API for aggregation and viewing on the dashboard.
 The log will include the stacktrace found on the exception.
 
 @param exception The handled exception object, which will be serialized and combined with the stack trace to aggregate the errors on the dashboard view.
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of custom key/value properties to be sent with the error log.
 @param customStackTrace A custom Stack Trace to be logged and displayed on the dashboard view. If this object is not nil, the Exception Stack Trace will be ignored.
 */
- (void)logHandledException:(nonnull NSException *)exception
               withSeverity:(EMBSeverity)severity
                 properties:(nullable EMBProperties *)properties
           customStackTrace:(nullable NSArray<NSString*> *)customStackTrace;

/**
 Logs a handled exception to the Embrace.io API for aggregation and viewing on the dashboard.
 The log will include the stacktrace found on the exception.

 @param exception The handled exception object, which will be serialized and combined with the stack trace to aggregate the errors on the dashboard view.
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of custom key/value properties to be sent with the error log.
 @param customStackTrace A custom Stack Trace to be logged and displayed on the dashboard view. If this object is not nil, the Exception Stack Trace will be ignored.
 @param takeScreenshot A flag for whether the SDK should take a screenshot of the application window to display on the dashboard
 */
- (void)logHandledException:(nonnull NSException *)exception
               withSeverity:(EMBSeverity)severity
                 properties:(nullable EMBProperties *)properties
           customStackTrace:(nullable NSArray<NSString*> *)customStackTrace
             takeScreenshot:(BOOL)takeScreenshot  DEPRECATED_MSG_ATTRIBUTE("Please replace calls to logHandledException with methods of the form logHandledException:severity:properties:customStacktrace:");

/**
 Logs a custom message within this session for the Embrace dashboard to surface on the Session Timeline and within
 the Activity Log.
 
 This will NOT trigger an http request to the Embrace.io backend and is useful for adding additional context to the
 actions a user performed within a session. Good uses for breadcrumbs could be your app's console error output
 or notes that significant events (add item to cart, start conversation, receive push notification) occurred.
 
 @param message The message that will be displayed within the session's Activity Log on the dashboard.
 */
- (void)logBreadcrumbWithMessage:(nonnull NSString *)message DEPRECATED_MSG_ATTRIBUTE("Use addBreadcrumbWithMessage instead. This method will be removed in future versions.");


/**
Logs a custom message within this session for the Embrace dashboard to surface on the Session Timeline and within
the Activity Log.

This will NOT trigger an http request to the Embrace.io backend and is useful for adding additional context to the
actions a user performed within a session. Good uses for breadcrumbs could be your app's console error output
or notes that significant events (add item to cart, start conversation, receive push notification) occurred.

@param message The message that will be displayed within the session's Activity Log on the dashboard.
*/
- (void)addBreadcrumbWithMessage:(nonnull NSString *)message;

/**
 Get the user identifier assigned to the device by Embrace

 @return A device identifier created by Embrace
 */
- (nullable NSString *)getDeviceId;

/**
 Get the ID for the current session. Returns nil if a session has not been started yet or the SDK hasn't been initialized.

 In order to prevent getting a nil value, try to avoid calling this method right after initializing the SDK or immediately after coming back from background.
 Please allow the SDK some time in order to confirm a new session has started before calling this method.
 
 @return The ID for the current Session, if available.
 */
- (nullable NSString *)getCurrentSessionId;

/**
 Associates the current app user with an internal identifier (e.g. your system's uid) to be made searchable in the dashboard.
 
 @param userId An internal identifier for the given user.
 */
- (void)setUserIdentifier:(nonnull NSString *)userId;

/**
 Resets the internal identifier for the current app user.
 */
- (void)clearUserIdentifier;

/**
 Associates the current app user with a username to be made searchable in the dashboard.
 
 @param username The current app user's associated username.
 */
- (void)setUsername:(nonnull NSString *)username;

/**
 Removes the username associated with the current app user.
 */
- (void)clearUsername;

/**
 Associates the current app user with an email address to be made searchable in the dashboard.
 
 @param email The current app user's associated email address.
 */
- (void)setUserEmail:(nonnull NSString *)email;

/**
 Removes the email address associated with the current app user.
 */
- (void)clearUserEmail;

/**
 Marks the current app user as a paying user.
 
 This is used for cohorting and segmentation in the dashboard.
 */
- (void)setUserAsPayer;

/**
 Marks the current app user as a non-paying user (the default).
 */
- (void)clearUserAsPayer;

/**
 Sets a custom persona for the current app user.
 
 Accepts string values to help you categorize and understand your users
 */
- (void)setUserPersona:(nonnull NSString *)persona  DEPRECATED_MSG_ATTRIBUTE("Use addUserPersona instead. This method will be removed in future versions.");

/**
 Adds a custom persona for the current app user.
 
 Accepts string values to help you categorize and understand your users
 */
- (void)addUserPersona:(nonnull NSString *)persona;

/**
 Removes the given custom persona for the current app user.
 */
- (void)clearUserPersona:(nonnull NSString *)persona;

/**
 Removes all custom personas for the current app user.
 */
- (void)clearAllUserPersonas;

/**
 Manually log a network request. In most cases the Embrace SDK automatically captures the details of your network requests.
 You can use this method to log any requests that the SDK is not capturing automatically.

 @param request An EMBNetworkRequest with at least the following set: url, method, start time, end time, and either status code or error.
 */
- (void)logNetworkRequest:(nonnull EMBNetworkRequest *)request DEPRECATED_MSG_ATTRIBUTE("Deprecated: use recordNetworkRequest instead. This method will be removed in future versions.");

/**
 Manually record a network request. In most cases the Embrace SDK automatically captures the details of your network requests.
 You can use this method to log any requests that the SDK is not capturing automatically.

 @param request An EMBNetworkRequest with at least the following set: url, method, start time, end time, and either status code or error.
 */
- (void)recordNetworkRequest:(nonnull EMBNetworkRequest *)request;

/**
 Logs enhanced metrics for a given URLSessionTask
 */
- (void)logURLSessionTaskMetrics:(nullable NSURLSessionTaskMetrics *)metrics
               forURLSessionTask:(nullable NSURLSessionTask *)task DEPRECATED_MSG_ATTRIBUTE("NSURLSessionTaskMetrics interactions are no longer recorded by the SDK. This method will be removed in future versions.") API_AVAILABLE(ios(10.0));

/**
 DEPRECATED

 Inform Embrace that a web view began a request that you'd like to have monitored. In order for data to be captured,
 ensure that you call `logWebViewCompletedRequest:` after the request is finished.
 
 @param request The NSURLRequest whose performance you'd like to instrument.
 */
- (void)logWebViewBeganRequest:(nullable NSURLRequest *)request DEPRECATED_MSG_ATTRIBUTE("WKWebView interactions are now recorded automatically by the SDK. This method will be removed in future versions.");

/**
 Cause a crash. Use this for test purposes only
 */
- (void)crash;

/**
 Cause a crash with an exception. Use this for test purposes only
 */
- (void)throwException;

/**
Enables or disables embrace's internal debug logging.
*/
- (void)setDebuggingEnabled:(BOOL)enabled;

/**
Enables or disables embrace's internal trace logging.
*/
- (void)setTraceEnabled:(BOOL)enabled;

/**
 Enables or disables embrace's clean logging format
 */
- (void)setCleanLogsEnabled:(BOOL)enabled DEPRECATED_MSG_ATTRIBUTE("This will no longer be supported.");

/**
 Suspend coordinate capture
 */
- (void)pauseTapCoordinateCapture;

/**
 Resume coordinate capture
 */
- (void)resumeTapCoordinateCapture;

/**
 Suspend tap element capture
 */
- (void)pauseTapElementCapture;

/**
 Resume tap element capture
 */
- (void)resumeTapElementCapture;

/**
 Notify Embrace that a push notification was received.
 Should be called from your `UIApplicationDelegate` implementation of `application:didReceiveRemoteNotification:`.
 This method should only be used if `PUSH_NOTIFICATIONS_CAPTURE_MODE` is set to `manual` in the EmbraceConfig.
 
 @param useInfo The dictionary containing the notification data.
 @return A boolean indicating whether the notification was captured or not.
 */
- (BOOL)applicationDidReceiveNotification:(nonnull NSDictionary *)useInfo;

/**
 Notify Embrace that a push notification response was received.
 Should be called from your `UNUserNotificationCenter` delegate implementation of `serNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:`.
 This method should only be used if `PUSH_NOTIFICATIONS_CAPTURE_MODE` is set to `manual` in the EmbraceConfig.
 
 @param response The UNNotificationResponse received.
 @return A boolean indicating whether the notification was captured or not.
 */
- (BOOL)applicationDidReceiveNotificationResponse:(nonnull UNNotificationResponse *)response API_AVAILABLE(ios(10.0)) API_UNAVAILABLE(tvos);

/**
 Notify Embrace that a silent push notification was received.
 Should be called from your `UIApplicationDelegate` implementation of `application:performFetchWithCompletionHandler:`.
 This method should only be used if `PUSH_NOTIFICATIONS_CAPTURE_MODE` is set to `manual` in the EmbraceConfig.
 
 @return A boolean indicating whether the notification was captured or not.
 */
- (BOOL)applicationDidReceiveSilentNotification;


/**
 DEPRECATED

 Enables or disables airplane mode
 
 Enabling airplane mode prevents all network traffic from the SDK. Use sparingly as this wil prevent any session data from making it to the embrace dash.
 */
- (void)setAirplaneModeEnabled:(BOOL)enabled DEPRECATED_MSG_ATTRIBUTE("No longer supported.");

#if __has_include(<WebKit/WebKit.h>)
/**
 Tracks the performance inside a `WKWebView`.
 
 @param webView The `WKWebView` to be instrumented.
 */
- (void)trackPerformanceInWebView:(nonnull WKWebView *)webView;
#endif
 
/**
 Manually tracks the performance of a Web View.
 
 @param tag An identifier to create a key for the instrumented WebView.
 @param message A string containing the performance metrics in a specific json format. If the format is not valid, it'll be ignored.
 */
- (void)trackWebViewPerformance:(nonnull NSString *)tag message:(nonnull NSString *)message;

///
/// Create a span with the given name
/// This span will not have a start time set, the caller should also call `start` on the span when timing should begin.
///
/// @Return Returns a Span that has not yet been started
///
- (nonnull id<EmbraceOTelSpan>) createSpanNamed:(nonnull NSString *)name;

///
/// Records a span for the runtime of the operation block passed.
///
/// @Discussion If your operation does not have a return value (void) and you are calling from ObjC,
///             see `recordSpanNamed:parent:block:` to allow for the void return type.
///
/// @Return Returns the value returned from the operation block
///
- (nullable id)recordSpanNamed:(nonnull NSString *)name operation:(id _Nullable (^_Nonnull)(void))operation;

///
/// Records a span for the runtime of the block parameter. Does not return value from the block.
///
- (void)recordSpanNamed:(nonnull NSString *)name block:(void (^_Nonnull)(void))block NS_SWIFT_UNAVAILABLE("Use recordSpanNamed:operation: instead.");

@end
