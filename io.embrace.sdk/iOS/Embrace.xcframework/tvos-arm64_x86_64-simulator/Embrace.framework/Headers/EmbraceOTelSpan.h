//
//  EmbraceOTelSpan.h
//  Embrace
//
//  Created by Austin Emmons on 8/23/23.
//  Copyright © 2023 embrace.io. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Embrace/EmbraceOTelSpanErrorCode.h>
#import <Embrace/EmbraceEventData.h>

#ifndef EmbraceOTelSpan_h
#define EmbraceOTelSpan_h

NS_ASSUME_NONNULL_BEGIN

@protocol EmbraceOTelSpan

/// A custom name to describe the span
@property(readonly) NSString *spanName;

/// The spans unique identifier within this trace
@property(readonly) NSString *traceId;

/// The spans unique identifier
@property(readonly) NSString *spanId;

/**
    Start the span at the current time
 */
-(void) start;

/**
    Stop the span at the current time
 */
-(void) stop;

/**
    Stop the span at the current time and mark with an Embrace error code
    Will mark span status in "ERROR" if errorCode is not `None`
 */
-(void) stopWithErrorCode:(EmbraceOTelSpanErrorCode) errorCode;

/**
    Adds an event to the span at the given time

        @param name The name of the event
        @param time The unix time, in nanoseconds, at which this event occurred
        @param attributes A dictionary to add additional context
 */
-(void) addEventNamed:(NSString *)name time: (NSUInteger) time attributes: (nullable NSDictionary<NSString*, NSString*> *) attributes;

/**
    Adds an attribute to this span

        @param key The attribute key
        @param value The attribute value
 */
-(void) addAttributeWithKey:(NSString *)key value: (NSString *)value;

@end

NS_ASSUME_NONNULL_END

#endif /* EmbraceOTelSpan_h */
