//
//  EMBRegistrationFlow.h
//  Embrace
//
//  Created by Juan Pablo Crespi on 12/03/2018.
//  Copyright © 2018 embrace.io. All rights reserved.
//

#import <Embrace/EMBCustomFlow.h>
#import <Embrace/EMBConstants.h>

/**
 This class is responsible for tracking app performance during registration flows.
 
 This class is thread-safe.
 */
__attribute__((deprecated("This class is deprecated. Please contact Embrace if you have a use case for this API and wish to see an alternative.")))
@interface EMBRegistrationFlow : EMBCustomFlow

/**
 Starts a registration moment.

 This method should be called as soon as a user wishes to submit their user registration info. This method is
 designed specifically for cases where the registration occurs through an internally managed service as opposed to
 an external service (e.g. Google, Facebook, GitHub Login).

 @param userId An identifier that uniquely represents the user (e.g. frequent flyer number). This value is optional
               and, if present, will associate the value as a property of the moment.
 @param username The username of the user. This value is optional and, if present, will associate the value as a
                 property of the moment.
 @param email The email address of the user. This value is optional and, if present, will associate the value as a
              property of the moment.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationStartWithId:(nullable NSString *)userId
                       username:(nullable NSString *)username
                          email:(nullable NSString *)email
                     properties:(nullable EMBProperties *)properties;

/**
 Starts a registration moment.

 This method should be called as soon as a user wishes to submit their user registration info. This method is
 designed specifically for cases where the registration occurs through an external authentication service (e.g.
 Google, Facebook, GitHub Login).

 @param source The registration system that will be authenticating the user. This value is optional and,
               if present, will associate the value as a property of the moment.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationStartWithSource:(nullable NSString *)source
                         properties:(nullable EMBProperties *)properties;

/**
 Ends the registration moment and generates an info log message that indicates that the registration completed.

 This method should be called once the registration information has been submitted. If any of the following values
 were defined when the registration moment was initially started, the SDK will register the device ID to all valid
 user information.
   - Email
   - Username
   - User Identifier

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationComplete;

/**
 Ends the registration moment and generates an info log message that indicates that the registration completed.

 This method should be called once the registration information has been submitted. If any of the following values
 were defined when the registration moment was initially started, the SDK will register the device ID to all valid
 user information.
 - Email
 - Username
 - User Identifier

 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationCompleteWithProperties:(nullable EMBProperties *)properties;

/**
 Ends the registration moment and generates an info log message that indicates that the registration completed.

 This method should be called once the registration information has been submitted. If any of the following values
 were defined when the registration moment was initially started, the SDK will register the device ID to all valid
 user information.
   - Email
   - Username
   - User Identifier

 @param isPayer An optional value that indicates whether the user is a payer or not. If this value is null, then it
                will not modify the payer persona status of the user.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationCompleteAsPayer:(BOOL)isPayer;

/**
 Ends the registration moment and generates an info log message that indicates that the registration completed.

 This method should be called once the registration information has been submitted. If any of the following values
 were defined when the registration moment was initially started, the SDK will register the device ID to all valid
 user information.
 - Email
 - Username
 - User Identifier

 @param isPayer An optional value that indicates whether the user is a payer or not. If this value is null, then it
 will not modify the payer persona status of the user.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationCompleteAsPayer:(BOOL)isPayer
                         properties:(nullable EMBProperties *)properties;

/**
 Ends the registration moment and generates a log error message that indicates that the registration failed.

 This method should be called once the registration information has been submitted.

 @param message A message that explains the reason for why this operation failed. This value is optional and, if
                provided, will associate the value as a property of the error log message.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationFailWithMessage:(nullable NSString *)message;

/**
 Ends the registration moment and generates a log error message that indicates that the registration failed.

 This method should be called once the registration information has been submitted.

 @param message A message that explains the reason for why this operation failed. This value is optional and, if
 provided, will associate the value as a property of the error log message.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)registrationFailWithMessage:(nullable NSString *)message
                         properties:(nullable EMBProperties *)properties;

@end
