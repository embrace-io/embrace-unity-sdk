//
//  EMBCustomFlow.h
//  Embrace
//
//  Created by Juan Pablo Crespi on 09/03/2018.
//  Copyright © 2018 embrace.io. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Embrace/EMBConstants.h>

/**
 Base class for creating custom domain-specific flows that are essentially convenience wrappers around existing SDK
 functionality.
 */
__attribute__((deprecated("This class is deprecated. Please contact Embrace if you have a use case for this API and wish to see an alternative.")))
@interface EMBCustomFlow : NSObject

/**
Creates an EMBCustomFlow instance.
 */

+ (nonnull instancetype)flow;

/**
 Starts a custom moment.
 
 @param momentName The name of the moment.
 @param allowScreenshot If true, a screenshot will be taken if the moment exceeds the late threshold. If this
                        value is false, a screenshot will be not be taken regardless of the moment duration.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties may be set.
 
 @return A moment identifier that uniquely identifies the newly started moment instance.
 */
- (nullable NSString *)momentStartWithName:(nonnull NSString *)momentName
                           allowScreenshot:(BOOL)allowScreenshot
                                properties:(nullable EMBProperties *)properties;

/**
 Completes all started instances of the specified custom moment.

 Note that only moment instances managed by this Flow object will be completed. In other words, if another Flow
 instance starts a moment with the same name, completing the moment on this instance will not affect it.

 @param momentName The name of the moment.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentCompleteWithName:(nonnull NSString *)momentName;

/**
 Completes all started instances of the specified custom moment.

 Note that only moment instances managed by this Flow object will be completed. In other words, if another Flow
 instance starts a moment with the same name, completing the moment on this instance will not affect it.

 @param momentName The name of the moment.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentCompleteWithName:(nonnull NSString *)momentName
                    properties:(nullable EMBProperties *)properties;

/**
 Completes a started instance of the custom moment specified by the moment identifier.

 Note that only moment instances managed by this Flow object will be completed. In other words, if another Flow
 instance starts a moment with the same name, completing the moment on this instance will not affect it.

 @param momentName The name of the moment.
 @param momentId The optional moment identifier returned by the `momentStart` method. This moment identifier must be
                 an identifier produced by this particular Flow instance that has not already been completed or
                 failed. This value can also be null, in which case all instances of the given moment name
                 registered with this Flow instance will be completed.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentCompleteWithName:(nonnull NSString *)momentName
                      momentId:(nullable NSString *)momentId;

/**
 Completes a started instance of the custom moment specified by the moment identifier.

 Note that only moment instances managed by this Flow object will be completed. In other words, if another Flow
 instance starts a moment with the same name, completing the moment on this instance will not affect it.

 @param momentName The name of the moment.
 @param momentId The optional moment identifier returned by the `momentStart` method. This moment identifier must be
 an identifier produced by this particular Flow instance that has not already been completed or
 failed. This value can also be null, in which case all instances of the given moment name
 registered with this Flow instance will be completed.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentCompleteWithName:(nonnull NSString *)momentName
                      momentId:(nullable NSString *)momentId
                    properties:(nullable EMBProperties *)properties;

/**
 Fails all started instances of the specified custom moment and generates an error log message for each failed
 moment instance.
 
 Note that only moment instances managed by this Flow object will be failed. In other words, if another Flow
 instance fails a moment with the same name, failing the moment on this instance will not affect it.
 
 @param momentName The name of the moment.
 @param message A message that explains the reason for why this operation failed. This value is optional and, if
                provided, will associate the value as a property of the error log message.
 
 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentFailWithName:(nonnull NSString *)momentName
                   message:(nullable NSString *)message;

/**
 Fails all started instances of the specified custom moment and generates an error log message for each failed
 moment instance.

 Note that only moment instances managed by this Flow object will be failed. In other words, if another Flow
 instance fails a moment with the same name, failing the moment on this instance will not affect it.

 @param momentName The name of the moment.
 @param message A message that explains the reason for why this operation failed. This value is optional and, if
 provided, will associate the value as a property of the error log message.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentFailWithName:(nonnull NSString *)momentName
                   message:(nullable NSString *)message
                properties:(nullable EMBProperties *)properties;

/**
 Fails a started instance of the custom moment specified by the moment identifier and sends an error log message for
 the failed moment instance.
 
 Note that only moment instances managed by this Flow object will be failed. In other words, if another Flow
 instance fails a moment with the same name, failing the moment on this instance will not affect it.
 
 @param momentName The name of the moment.
 @param momentId The optional moment identifier returned by the `momentStart` method. This moment identifier must be
                 an identifier produced by this particular Flow instance that has not already been completed or
                 failed. This value can also be null, in which case all instances of the given moment name
                 registered with this Flow instance will be completed.
 @param message A message that explains the reason for why this operation failed. This value is optional and, if
                provided, will associate the value as a property of the error log message.
 
 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentFailWithName:(nonnull NSString *)momentName
                  momentId:(nullable NSString *)momentId
                   message:(nullable NSString *)message;

/**
 Fails a started instance of the custom moment specified by the moment identifier and sends an error log message for
 the failed moment instance.

 Note that only moment instances managed by this Flow object will be failed. In other words, if another Flow
 instance fails a moment with the same name, failing the moment on this instance will not affect it.

 @param momentName The name of the moment.
 @param momentId The optional moment identifier returned by the `momentStart` method. This moment identifier must be
 an identifier produced by this particular Flow instance that has not already been completed or
 failed. This value can also be null, in which case all instances of the given moment name
 registered with this Flow instance will be completed.
 @param message A message that explains the reason for why this operation failed. This value is optional and, if
 provided, will associate the value as a property of the error log message.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)momentFailWithName:(nonnull NSString *)momentName
                  momentId:(nullable NSString *)momentId
                   message:(nullable NSString *)message
                properties:(nullable EMBProperties *)properties;

@end
