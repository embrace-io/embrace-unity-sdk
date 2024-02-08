//
//  Embrace+ConfigModule.h
//  Embrace
//
//  Created by Joni Bandoni on 11/11/2022.
//  Copyright © 2022 embrace.io. All rights reserved.
//


//This needs to be a private headers to avoid issues with the climb in the compilation time. Some duplicated declarations appears

#import "Embrace.h"
#import "EMBConfigModule.h"
#import "EMBConfigManager.h"
#import "EMBAppGroupCrashSender.h"
#import "EMBAppGroupSessionSender.h"

@interface Embrace (ConfigModule) <EMBConfigModuleDelegate, EMBAppGroupCrashSenderDelegate, EMBAppGroupSessionSenderDelegate>

@property NSUInteger configFetchInterval;
@property dispatch_queue_t configQueue;
@property EMBConfigModule* configModule;
@property EMBConfigManager* configHelper;       //TODO: This will need to be refactored individually

- (EMBConfigManager*) configHelper;
- (void) setConfigHelper:(EMBConfigManager*)configHelper; 

@end
