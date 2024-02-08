//
//  EMBPurchaseFlow.h
//  Embrace
//
//  Created by Juan Pablo Crespi on 12/03/2018.
//  Copyright © 2018 embrace.io. All rights reserved.
//

#import <Embrace/EMBCustomFlow.h>
#import <Embrace/EMBConstants.h>

/**
 This class is responsible for tracking app performance during purchase flows.
 
 This class is thread-safe.
 */
__attribute__((deprecated("This class is deprecated. Please contact Embrace if you have a use case for this API and wish to see an alternative.")))
@interface EMBPurchaseFlow : EMBCustomFlow

/**
 Starts an add-to-cart app moment.

 This method should be called as soon as the user indicates an intent to add an item to their cart.

 @param itemId The ID that represents the item being added to the cart. This value is optional and, if present, will
               associate the value as a property of the moment.
 @param quantity The number of items being added to the cart. This value is optional and, if present, will associate
                 the value as a property of the moment.
 @param price The unit price of the item being added to the cart. This value is optional and, if present, will
              associate the value as a property of the moment.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return A moment identifier that can be used to close the add-to-cart moment. If an error was encountered, this
         method returns null.
 */
- (nonnull NSString *)addToCartStartWithItemId:(nullable NSString *)itemId
                                      quantity:(nullable NSNumber *)quantity
                                         price:(nullable NSNumber *)price
                                    properties:(nullable EMBProperties *)properties;

/**
 Ends a particular add-to-cart moment instance and generates an info log message that indicates that adding to the
 cart completed.

 This method should be called once the item is verified to be in the user's cart.

 @param momentId The moment identifier returned by the `PurchaseFlow.addToCartStart` method. This moment identifier
                 must be an identifier produced by this particular PurchaseFlow instance and must not have already
                 been marked as completed or failed.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)addToCartCompleteWithMomentId:(nonnull NSString *)momentId;

/**
 Ends a particular add-to-cart moment instance and generates an info log message that indicates that adding to the
 cart completed.

 This method should be called once the item is verified to be in the user's cart.

 @param momentId The moment identifier returned by the `PurchaseFlow.addToCartStart` method. This moment identifier
 must be an identifier produced by this particular PurchaseFlow instance and must not have already
 been marked as completed or failed.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)addToCartCompleteWithMomentId:(nonnull NSString *)momentId
                           properties:(nullable EMBProperties *)properties;

/**
 Ends a particular add-to-cart moment instance and generates an error log message that indicates that adding to the
 cart failed.

 This method should be called when it has been determined that the item could not be added to the cart.

 @param momentId The moment identifier returned by the `PurchaseFlow.addToCartStart` method. This moment identifier
                 must be an identifier produced by this particular PurchaseFlow instance and must not have already
                 been marked as completed or failed.
 @param message A message that explains the reason for why this operation failed. This value is optional and, if
                provided, will associate the value as a property of the error log message.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)addToCartFailWithMomentId:(nonnull NSString *)momentId
                          message:(nullable NSString *)message;

/**
 Ends a particular add-to-cart moment instance and generates an error log message that indicates that adding to the
 cart failed.

 This method should be called when it has been determined that the item could not be added to the cart.

 @param momentId The moment identifier returned by the `PurchaseFlow.addToCartStart` method. This moment identifier
 must be an identifier produced by this particular PurchaseFlow instance and must not have already
 been marked as completed or failed.
 @param message A message that explains the reason for why this operation failed. This value is optional and, if
 provided, will associate the value as a property of the error log message.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)addToCartFailWithMomentId:(nonnull NSString *)momentId
                          message:(nullable NSString *)message
                       properties:(nullable EMBProperties *)properties;

/**
 Starts a purchase moment.
 
 This method should be called as soon as the user indicates an intent to purchase the items in their cart. This
 means that all information pertaining to the order (e.g. billing, payment, shipping) should already be known prior
 to invoking this method.

 @param orderId The ID that represents the purchase order. This value is optional and, if present, will associate
                the value as a property of the moment.
 @param numItems The number of items in the purchase order. This value is optional and, if present, will associate
                 the value as a property of the moment.
 @param amount The total amount of the purchase order. This value is optional and, if present, will associate the
               value as a property of the moment.
 @param paymentType The payment system that will be fulfilling the purchase order (e.g. Google IAB, PayPal,
                    BrainTree). This value is optional and, if present, will associate the value as a property of
                    the moment.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)purchaseStartWithOrderId:(nullable NSString *)orderId
                        numItems:(nullable NSNumber *)numItems
                          amount:(nullable NSNumber *)amount
                     paymentType:(nullable NSString *)paymentType
                      properties:(nullable EMBProperties *)properties;

/**
 Ends the purchase moment and generates an info log message that indicates that the purchase completed.

 This method should be called once the purchase order has been confirmed.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)purchaseComplete;

/**
 Ends the purchase moment and generates an info log message that indicates that the purchase completed.

 This method should be called once the purchase order has been confirmed.

 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)purchaseCompleteWithProperties:(nullable EMBProperties *)properties;

/**
 Ends the purchase moment and generates an error log message that indicates that the purchase failed.
 
 This method should be called once the purchase order has been confirmed.
 
 @param message A message that explains the reason for why this operation failed. This value is optional and, if
                provided, will associate the value as a property of the error log message.
 
 @return True if the operation was successful; false otherwise.
 */
- (BOOL)purchaseFailWithMessage:(nullable NSString *)message;

/**
 Ends the purchase moment and generates an error log message that indicates that the purchase failed.

 This method should be called once the purchase order has been confirmed.

 @param message A message that explains the reason for why this operation failed. This value is optional and, if
 provided, will associate the value as a property of the error log message.
 @param properties A map of Strings to Objects that represent additional properties to associate with the moment.
                   This value is optional. A maximum of 10 properties (not including the ones set via arguments to
                   this method) may be set.

 @return True if the operation was successful; false otherwise.
 */
- (BOOL)purchaseFailWithMessage:(nullable NSString *)message
                     properties:(nullable EMBProperties *)properties;

@end
