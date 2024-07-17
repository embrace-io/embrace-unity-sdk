//
//  EmbraceOTelSpanErrorCode.h
//  Embrace
//
//  Created by Austin Emmons on 8/23/23.
//  Copyright © 2023 embrace.io. All rights reserved.
//

#ifndef EmbraceOTelSpanErrorCode_h
#define EmbraceOTelSpanErrorCode_h

/**
    More specific outcomes for span that ends with an error status
 */
typedef NS_ENUM(NSInteger, EmbraceOTelSpanErrorCode) {
    // No error status
    None = 0,

    // Span ended in an expected, but less than optimal error state
    Failure,

    // Span ended becuase user reverted intent
    UserAbandon,

    // Span ended in some other way
    Unknown
};

#endif /* EmbraceOTelSpanErrorCode_h */

