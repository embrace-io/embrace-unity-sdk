//
//  FlutterEmbrace.h
//  Embrace
//
//  Created by Chris Gaudino on 9/21/22.
//  Copyright © 2022 embrace.io. All rights reserved.
//


#import <Foundation/Foundation.h>

/**
 Entry point for the Embrace Flutter SDK.
 */
@interface EMBFlutterEmbrace: NSObject

/**
 Returns the shared `EMBFlutterEmbrace` singleton object.
 */
+ (nonnull instancetype)sharedInstance;

/**
 Sets the Dart runtime version number.
 
 @param version Dart runtime version number.
 */
- (void)setDartVersion:(nullable NSString *)version;

/**
 Sets the Embrace Flutter SDK version number.
 
 @param version Embrace Flutter SDK version number.
 */
- (void)setEmbraceFlutterSDKVersion:(nullable NSString *)version;

/**
 Log a Flutter exception through the native embrace sdk.
 
 @param name The name of the message, which is how it will show up on the dashboard
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of up to 10 key/value pairs
 @param takeScreenshot A flag for whether the SDK should take a screenshot of the application window to display on the dashboard
 @param flutterStackTrace dart stack trace coming from the the flutter side
 @param flutterContext context associated with the error
 @param flutterLibrary library associated with the error
*/
- (void)logMessage:(nullable NSString *)name
      withSeverity:(EMBSeverity)severity
        properties:(nullable NSDictionary *)properties
    takeScreenshot:(BOOL)takeScreenshot
 flutterStackTrace:(nullable NSString *)flutterStackTrace
    flutterContext:(nullable NSString *)flutterContext
    flutterLibrary:(nullable NSString *)flutterLibrary DEPRECATED_MSG_ATTRIBUTE("Use the new logHandledExceptionWithName and logUnhandledExceptionWithName methods");

/**
 Log a flutter exception through the native embrace sdk.
 
 @param name The name of the message, which is how it will show up on the dashboard
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of up to 10 key/value pairs
 @param takeScreenshot A flag for whether the SDK should take a screenshot of the application window to display on the dashboard
 @param flutterStackTrace dart stack trace coming from the the flutter side
 @param flutterContext context associated with the error
 @param flutterLibrary library associated with the error
 @param wasHandled mark it as handled or not
*/
- (void)logMessage:(nullable NSString *)name
      withSeverity:(EMBSeverity)severity
        properties:(nullable NSDictionary *)properties
    takeScreenshot:(BOOL)takeScreenshot
 flutterStackTrace:(nullable NSString *)flutterStackTrace
    flutterContext:(nullable NSString *)flutterContext
    flutterLibrary:(nullable NSString *)flutterLibrary
        wasHandled:(BOOL)wasHandled DEPRECATED_MSG_ATTRIBUTE("Use the new logHandledExceptionWithName and logUnhandledExceptionWithName methods");

/**
 Log a flutter exception through the native embrace sdk.
 
 @param name The name of the message, which is how it will show up on the dashboard
 @param severity Will flag the message as one of info, warning, or error for filtering on the dashboard
 @param properties An optional dictionary of up to 10 key/value pairs
 @param takeScreenshot A flag for whether the SDK should take a screenshot of the application window to display on the dashboard
 @param flutterStackTrace dart stack trace coming from the the flutter side
 @param flutterContext context associated with the error
 @param flutterLibrary library associated with the error
 @param flutterErrorType runtime type of the error
 @param wasHandled mark it as handled or not
*/
- (void)logMessage:(nullable NSString *)name
      withSeverity:(EMBSeverity)severity
        properties:(nullable NSDictionary *)properties
    takeScreenshot:(BOOL)takeScreenshot
 flutterStackTrace:(nullable NSString *)flutterStackTrace
    flutterContext:(nullable NSString *)flutterContext
    flutterLibrary:(nullable NSString *)flutterLibrary
  flutterErrorType:(nullable NSString *)flutterErrorType
        wasHandled:(BOOL)wasHandled DEPRECATED_MSG_ATTRIBUTE("Use the new logHandledExceptionWithName and logUnhandledExceptionWithName methods");

/**
 Log a flutter handled exception through the native embrace sdk.
 
 @param name The name of the exception
 @param message The message of the exception
 @param stackTrace dart stack trace coming from the the flutter side
 @param context context associated with the error
 @param library library associated with the error
*/
- (void)logHandledExceptionWithName:(nonnull NSString *)name
                            message:(nonnull NSString *)message
                         stackTrace:(nullable NSString *)stackTrace
                            context:(nullable NSString *)context
                            library:(nullable NSString *)library;

/**
 Log a flutter unhandled exception through the native embrace sdk.
 
 @param name The name of the exception
 @param message The message of the exception
 @param stackTrace dart stack trace coming from the the flutter side
 @param context context associated with the error
 @param library library associated with the error
*/
- (void)logUnhandledExceptionWithName:(nonnull NSString *)name
                              message:(nonnull NSString *)message
                           stackTrace:(nullable NSString *)stackTrace
                              context:(nullable NSString *)context
                              library:(nullable NSString *)library;

@end
