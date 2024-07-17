//
//  EMBNetworkRequest.h
//  Embrace
//
//  Created by Fredric Newberg on 7/5/19.
//  Copyright © 2019 embrace.io. All rights reserved.
//

#import <Foundation/Foundation.h>

/**
 HTTP methods that can be sent to Embrace
 */
typedef NS_ENUM(NSInteger, EMBHttpMethod) {

    /**
     GET
     */
    EMBHttpMethodGET = 0,

    /**
     HEAD
     */
    EMBHttpMethodHEAD,

    /**
     POST
     */
    EMBHttpMethodPOST,

    /**
     PUT
     */
    EMBHttpMethodPUT,

    /**
     DELETE
     */
    EMBHttpMethodDELETE,

    /**
     CONNECT
     */
    EMBHttpMethodCONNECT,

    /**
     OPTIONS
     */
    EMBHttpMethodOPTIONS,

    /**
     TRACE
     */
    EMBHttpMethodTRACE,

    /**
     PATCH
     */
    EMBHttpMethodPATCH
};

/**
 This class is used to create manually-recorded network requests.
 */

@interface EMBNetworkRequest : NSObject

/**
 The request's URL. Must start with http:// or https://
 */
@property (nonatomic, strong, readonly, nonnull) NSURL *url DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 */
@property (nonatomic, strong, readonly, nonnull) NSString *method DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The time the request started.
 */
@property (nonatomic, strong, readonly, nonnull) NSDate *startTime DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The time the request ended. Must be greater than the startTime.
 */
@property (nonatomic, strong, nullable) NSDate *endTime DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The number of bytes received.
 */
@property (nonatomic, assign) NSInteger bytesIn DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The number of bytes received.
 */
@property (nonatomic, assign) NSInteger bytesOut DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The response status of the request. Must be in the range 100 to 599.
 */
@property (nonatomic, assign) NSInteger responseCode DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 NSError that describes a non-HTTP error, e.g. a connection error.
 */
@property (nonatomic, strong, nullable) NSError *error DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 Optional trace ID that can be used to trace a particular request. Max length is 64 characters.
 */
@property (nonatomic, strong, nullable) NSString *traceId DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The time the request started specified in seconds.
 */
@property (nonatomic, assign, readonly) NSTimeInterval startEpoch DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 The time the request ended specified in seconds.
 */
@property (nonatomic, assign, readonly) NSTimeInterval endEpoch DEPRECATED_MSG_ATTRIBUTE("Please use the factory functions instead rather than using properties. This will be removed in a future version.");

/**
 Creates an EMBNetwork Request with the URL, method, and start time set

 @param urlString The request's URL. Must start with http:// or https://
 @param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 @param startTime The time the request started.

 @return An EMBNetworkRequest object
 */
+ (nullable instancetype)initWithURLString:(nonnull NSString *)urlString
                                    method:(nonnull NSString *)method
                                 startTime:(nonnull NSDate *)startTime DEPRECATED_MSG_ATTRIBUTE("Please replace calls to initialize network requests with methods of the form networkRequestWithURLString:method:startTime:");

/**
Creates an EMBNetwork Request with the URL, method, and start time set

@param urlString The request's URL. Must start with http:// or https://
@param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
@param startTime The time the request started.

@return An EMBNetworkRequest object
*/
+ (nullable instancetype)networkRequestWithURLString:(nonnull NSString *)urlString
                                              method:(nonnull NSString *)method
                                           startTime:(nonnull NSDate *)startTime DEPRECATED_MSG_ATTRIBUTE("Please use fromIncompleteRequest instead. This will be removed in a future version.");

/**
 Creates an EMBNetwork Request with the URL, method, start time, end time, bytes sent, bytes received, response code, error, and traceId set.

 @param urlString The request's URL. Must start with http:// or https://
 @param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 @param startTime The time the request started.
 @param endTime The time the request ended. Must be greater than the startTime.
 @param bytesIn The number of bytes received.
 @param bytesOut The number of bytes received.
 @param responseCode The response status of the request. Must be in the range 100 to 599.
 @param error NSError that describes a non-HTTP error, e.g. a connection error.
 @param traceId Optional trace ID that can be used to trace a particular request. Max length is 64 characters.

 @return An EMBNetworkRequest object
 */
+ (nullable instancetype)initWithURLString:(nonnull NSString *)urlString
                                    method:(nonnull NSString *)method
                                 startTime:(nonnull NSDate *)startTime
                                   endTime:(nullable NSDate *)endTime
                                   bytesIn:(NSInteger)bytesIn
                                  bytesOut:(NSInteger)bytesOut
                              responseCode:(NSInteger)responseCode
                                     error:(nullable NSError *)error
                                   traceId:(nullable NSString *)traceId DEPRECATED_MSG_ATTRIBUTE("Please replace calls to initialize network requests with methods of the form networkRequestWithURLString:method:startTime:endTime:bytesIn:bytesOut:responseCode:error:traceId:");

/**
Creates an EMBNetwork Request with the URL, method, start time, end time, bytes sent, bytes received, response code, error, and traceId set.

@param urlString The request's URL. Must start with http:// or https://
@param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
@param startTime The time the request started.
@param endTime The time the request ended. Must be greater than the startTime.
@param bytesIn The number of bytes received.
@param bytesOut The number of bytes received.
@param responseCode The response status of the request. Must be in the range 100 to 599.
@param error NSError that describes a non-HTTP error, e.g. a connection error.
@param traceId Optional trace ID that can be used to trace a particular request. Max length is 64 characters.

@return An EMBNetworkRequest object
*/
+ (nullable instancetype)networkRequestWithURLString:(nonnull NSString *)urlString
                                              method:(nonnull NSString *)method
                                           startTime:(nonnull NSDate *)startTime
                                             endTime:(nullable NSDate *)endTime
                                             bytesIn:(NSInteger)bytesIn
                                            bytesOut:(NSInteger)bytesOut
                                        responseCode:(NSInteger)responseCode
                                               error:(nullable NSError *)error
                                             traceId:(nullable NSString *)traceId DEPRECATED_MSG_ATTRIBUTE("Please use fromCompletedRequest/fromIncompleteRequest instead. This will be removed in a future version.");

/**
 Creates an EMBNetwork Request with the URL, method, and start time set

 @param url The request's NSURL. Must start with http:// or https://
 @param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 @param startTime The time the request started.

 @return An EMBNetworkRequest object
 */
+ (nullable instancetype)initWithURL:(nonnull NSURL *)url
                              method:(nonnull NSString *)method
                           startTime:(nonnull NSDate *)startTime DEPRECATED_MSG_ATTRIBUTE("Please replace calls to initialize network requests with methods of the form networkRequestWithURL:method:startTime:");

/**
Creates an EMBNetwork Request with the URL, method, and start time set

@param url The request's NSURL. Must start with http:// or https://
@param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
@param startTime The time the request started.

@return An EMBNetworkRequest object
*/
+ (nullable instancetype)networkRequestWithURL:(nonnull NSURL *)url
                                        method:(nonnull NSString *)method
                                     startTime:(nonnull NSDate *)startTime DEPRECATED_MSG_ATTRIBUTE("Please use fromIncompleteRequest instead. This will be removed in a future version.");

/**
 Creates an EMBNetwork Request with the URL, method, start time, end time, bytes sent, bytes received, response code, error, and traceId set.

 @param url The request's NSURL. Must start with http:// or https://
 @param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 @param startTime The time the request started.
 @param endTime The time the request ended. Must be greater than the startTime.
 @param bytesIn The number of bytes received.
 @param bytesOut The number of bytes received.
 @param responseCode The response status of the request. Must be in the range 100 to 599.
 @param error NSError that describes a non-HTTP error, e.g. a connection error.
 @param traceId Optional trace ID that can be used to trace a particular request. Max length is 64 characters.

 @return An EMBNetworkRequest object
 */
+ (nullable instancetype)initWithURL:(nonnull NSURL *)url
                              method:(nonnull NSString *)method
                           startTime:(nonnull NSDate *)startTime
                             endTime:(nullable NSDate *)endTime
                             bytesIn:(NSInteger)bytesIn
                            bytesOut:(NSInteger)bytesOut
                        responseCode:(NSInteger)responseCode
                               error:(nullable NSError *)error
                             traceId:(nullable NSString *)traceId DEPRECATED_MSG_ATTRIBUTE("Please replace calls to initialize network requests with methods of the form networkRequestWithURL:method:startTime:endTime:bytesIn:bytesOut:responseCode:error:traceId:");

/**
Creates an EMBNetwork Request with the URL, method, start time, end time, bytes sent, bytes received, response code, error, and traceId set.

@param url The request's NSURL. Must start with http:// or https://
@param method The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
@param startTime The time the request started.
@param endTime The time the request ended. Must be greater than the startTime.
@param bytesIn The number of bytes received.
@param bytesOut The number of bytes received.
@param responseCode The response status of the request. Must be in the range 100 to 599.
@param error NSError that describes a non-HTTP error, e.g. a connection error.
@param traceId Optional trace ID that can be used to trace a particular request. Max length is 64 characters.

@return An EMBNetworkRequest object
*/
+ (nullable instancetype)networkRequestWithURL:(nonnull NSURL *)url
                                        method:(nonnull NSString *)method
                                     startTime:(nonnull NSDate *)startTime
                                       endTime:(nullable NSDate *)endTime
                                       bytesIn:(NSInteger)bytesIn
                                      bytesOut:(NSInteger)bytesOut
                                  responseCode:(NSInteger)responseCode
                                         error:(nullable NSError *)error
                                       traceId:(nullable NSString *)traceId DEPRECATED_MSG_ATTRIBUTE("Please use fromCompletedRequest/fromIncompleteRequest instead. This will be removed in a future version.");

/**
 * Construct a new EMBNetworkRequest instance where a HTTP response was returned.
 * If no response was returned, use fromIncompleteRequest instead.
 *
 * @param url The request's NSURL. Must start with http:// or https://
 * @param httpMethod The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 * @param startTime The time the request started.
 * @param endTime The time the request ended. Must be greater than the startTime.
 * @param bytesSent The number of bytes sent.
 * @param bytesReceived The number of bytes received.
 * @param statusCode The response status of the request. Must be in the range 100 to 599.
 */
+ (nullable instancetype)fromCompletedRequest:(nonnull NSURL *)url
                                   httpMethod:(EMBHttpMethod)httpMethod
                                    startTime:(nonnull NSDate *)startTime
                                      endTime:(nonnull NSDate *)endTime
                                    bytesSent:(NSInteger)bytesSent
                                bytesReceived:(NSInteger)bytesReceived
                                   statusCode:(NSInteger)statusCode;

/**
 * Construct a new EMBNetworkRequest instance where a HTTP response was returned.
 * If no response was returned, use fromIncompleteRequest instead.
 *
 * @param url The request's NSURL. Must start with http:// or https://
 * @param httpMethod The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 * @param startTime The time the request started.
 * @param endTime The time the request ended. Must be greater than the startTime.
 * @param bytesSent The number of bytes sent.
 * @param bytesReceived The number of bytes received.
 * @param statusCode The response status of the request. Must be in the range 100 to 599.
 */
+ (nullable instancetype)fromCompletedRequest:(nonnull NSURL *)url
                                   httpMethod:(EMBHttpMethod)httpMethod
                                    startTime:(nonnull NSDate *)startTime
                                      endTime:(nonnull NSDate *)endTime
                                    bytesSent:(NSInteger)bytesSent
                                bytesReceived:(NSInteger)bytesReceived
                                   statusCode:(NSInteger)statusCode
                                      traceId:(nullable NSString *)traceId;

/**
 * Construct a new EMBNetworkRequest instance where a HTTP response was not returned.
 * If a response was returned, use fromCompletedRequest instead.
 *
 * @param url The request's NSURL. Must start with http:// or https://
 * @param httpMethod The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 * @param startTime The time the request started.
 * @param endTime The time the request ended. Must be greater than the startTime.
 * @param error      the error that occurred.
 */
+ (nullable instancetype)fromIncompleteRequest:(nonnull NSURL *)url
                                    httpMethod:(EMBHttpMethod)httpMethod
                                     startTime:(nonnull NSDate *)startTime
                                       endTime:(nonnull NSDate *)endTime
                                         error:(nullable NSError *)error;

/**
 * Construct a new EMBNetworkRequest instance where a HTTP response was not returned.
 * If a response was returned, use fromCompletedRequest instead.
 *
 * @param url The request's NSURL. Must start with http:// or https://
 * @param httpMethod The request's method. Must be one of the following: GET, PUT, POST, DELETE, PATCH.
 * @param startTime The time the request started.
 * @param endTime The time the request ended. Must be greater than the startTime.
 * @param error      the error that occurred.
 * @param traceId    the trace ID of the request, used for distributed tracing.
 */
+ (nullable instancetype)fromIncompleteRequest:(nonnull NSURL *)url
                                    httpMethod:(EMBHttpMethod)httpMethod
                                     startTime:(nonnull NSDate *)startTime
                                       endTime:(nonnull NSDate *)endTime
                                         error:(nullable NSError *)error
                                       traceId:(nullable NSString *)traceId;

@end
