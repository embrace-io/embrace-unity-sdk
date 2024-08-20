//
//  EmbraceExtension.h
//  Embrace
//
//  Created by Ariel Demarco on 03/07/2023.
//  Copyright © 2023 embrace.io. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface EmbraceExtension : NSObject

/**
 Performs the initial setup of the Embrace SDK for an App Extension with the provided API key.

 Note that this is the only `start` method that will work to enable observability for App Extensions.

 @param apiKey The unique Embrace API key that identifies your application.
 
 @param appGroupIdentifier The identifier of the App Group associated with an app or extension.
 */
+ (void)startAppExtensionWithApiKey:(nonnull NSString *)apiKey
                 appGroupIdentifier:(nonnull NSString *)appGroupIdentifier;

/**
 Performs the initial setup of the Embrace SDK for an App Extension with the provided API key.

 Note that this is the only `start` method that will work to enable observability for App Extensions.

 The difference between the other intialization API is that this one allows to toggle network access.

 @param apiKey The unique Embrace API key that identifies your application.

 @param appGroupIdentifier The identifier of the App Group associated with an app or extension.

 @param enableNetworkAccess A toggle to enable/disable network access.
 */
+ (void)startAppExtensionWithApiKey:(NSString *)apiKey
                 appGroupIdentifier:(NSString *)appGroupIdentifier
                enableNetworkAccess:(BOOL)enableNetworkAccess;

/**
 Manually starts a new session. This method should be used only after the usage of `stop` as it prevents the creation of a new session.
 
 If there's a current session, then this method does nothing.
 */

+ (void)startNewSession;

/**
 Manually ends the session without creating a new one.
 
 This is specifically for ending sessions in extensions that don't want an empty session in between extension runs.
 */

+ (void)stopCurrentSession;

- (instancetype)init NS_UNAVAILABLE;

@end

NS_ASSUME_NONNULL_END
