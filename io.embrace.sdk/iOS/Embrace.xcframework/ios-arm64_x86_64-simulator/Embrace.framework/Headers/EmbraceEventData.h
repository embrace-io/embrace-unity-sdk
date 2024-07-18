//
//  EmbraceEventData.h
//  Embrace
//
//  Created by Austin Emmons on 8/25/23.
//  Copyright © 2023 embrace.io. All rights reserved.
//

#ifndef EmbraceEventData_h
#define EmbraceEventData_h

@protocol EmbraceEventData


/// The name of the event
@property(nonnull, readonly) NSString *name;

/// The unix time, in nanoseconds, at which this event occurred
@property(readonly) NSUInteger time;

/// A dictionary to add additional context
@property(nonnull, readonly) NSDictionary <NSString *, NSString *>* attributes;

@end

#endif /* EmbraceEventData_h */
