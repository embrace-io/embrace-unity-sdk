//
//  EmbraceTracer.h
//  Embrace
//
//  Created by Austin Emmons on 8/23/23.
//  Copyright © 2023 embrace.io. All rights reserved.
//

#import <Embrace/EmbraceOTelSpan.h>
#import <Embrace/EmbraceEventData.h>
#import <Embrace/EmbraceOTelSpanErrorCode.h>

#ifndef EmbraceTracer_h
#define EmbraceTracer_h

@protocol EmbraceTracer <NSObject>


///
/// Create a span with the given name and the specified parent if present
/// This span will not have a start time set, the caller should also call `start` on the span when timing should begin.
///
/// @Return Returns a Span that has not yet been started
///
- (nonnull id<EmbraceOTelSpan>) createSpanNamed:(nonnull NSString *)name parent:(nullable id<EmbraceOTelSpan>)parent;

///
/// Records a span for the runtime of the operation block passed.
///
/// @Discussion If your operation does not have a return value (void), see `recordSpanNamed:parent:block:` to allow for the void return type.
///
/// @Return Returns the value returned from the operation block
///
- (nullable id)recordSpanNamed:(nonnull NSString *)name parent:(nullable id<EmbraceOTelSpan>)parent operation:(id _Nullable (^_Nonnull)(void))operation;


/// Records a span for the runtime of the block parameter. Does not return value from the block.
///
- (void)recordSpanNamed:(nonnull NSString *)name parent:(nullable id<EmbraceOTelSpan>)parent block:(void (^_Nonnull)(void))block;


/// Records a completed span manually. Time parameters should be nanoseconds since the unix epoch.
///
/// @Return Returns true if the span is persisted correctly
- (BOOL)recordCompletedSpanNamed:(nonnull NSString *)name
                          parent:(nullable id<EmbraceOTelSpan>)parent
                  startTimeNanos: (long)startTimeNanos
                    endTimeNanos:(long)endTimeNanos
                      attributes:(nullable NSDictionary<NSString *, NSString *> *)attributes
                          events:(nullable NSArray<id<EmbraceEventData>> *)events
                       errorCode: (EmbraceOTelSpanErrorCode) errorCode;

@end

#endif /* EmbraceTracer_h */
