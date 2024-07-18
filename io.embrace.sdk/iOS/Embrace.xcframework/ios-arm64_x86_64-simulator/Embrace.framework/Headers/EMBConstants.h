//
//  EMBConstants.h
//  Embrace
//
//  Created by Brian Wagner on 9/13/16.
//  Copyright © 2016 embrace.io. All rights reserved.
//

#import <Foundation/Foundation.h>

#ifndef DEBUG
#define EMBLogDefaultLevel EMBLogLevelInfo
#else
#define EMBLogDefaultLevel EMBLogLevelNone
#endif

/**
 The last state of the app
 */
typedef NS_ENUM(NSInteger, EMBLastRunEndState) {

    /**
     The SDK has not been started yet or the crash provider is not Embrace
     */
    EMBLastRunEndStateInvalid = 0,

    /**
     The last run resulted in a crash
     */
    EMBLastRunEndStateCrash = 1,

    /**
     The last run did not result in a crash
     */
    EMBLastRunEndStateCleanExit = 2,
};

/**
 The log levels are used to filter internal logging.
 */
typedef NS_ENUM(NSInteger, EMBLogLevel) {

    /**
     No logs.
     */
    EMBLogLevelNone = 0,

    /**
     Error, warning, info, debug and trace logs.
     */
    EMBLogLevelTrace,

    /**
     Error, warning, info and debug logs.
     */
    EMBLogLevelDebug,

    /**
     Error, warning and info logs.
     */
    EMBLogLevelInfo,

    /**
     Error and warning logs.
     */
    EMBLogLevelWarning,

    /**
     Error logs only.
     */
    EMBLogLevelError
};

/**
 Log message severity. Marks the log message as info, warning, or error for filtering on the dashboard.
 */
typedef NS_ENUM(NSInteger, EMBSeverity) {

    /**
     Error log message.
     */
    EMBSeverityError = 0,

    /**
     Warning log message.
     */
    EMBSeverityWarning,

    /**
     Info log message.
     */
    EMBSeverityInfo
};

/**
 The framework used by the app.
 */
typedef NS_ENUM(NSInteger, EMBAppFramework) {

    /**
     iOS native framework.
     */
    EMBAppFrameworkNative = 1,

    /**
     ReactNative framework.
     */
    EMBAppFrameworkReactNative,

    /**
     Unity framework.
     */
    EMBAppFrameworkUnity,

    /**
     Flutter framework.
     */
    EMBAppFrameworkFlutter,
};

/**
 Embrace properties type definition.
 */
typedef NSDictionary<NSString *, NSString *> EMBProperties;

/**
 Embrace bundle identifier.
 */
extern NSString *const EMBBundleIdentifier;

