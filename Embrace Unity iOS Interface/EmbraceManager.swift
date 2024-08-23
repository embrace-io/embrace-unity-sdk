//
//  EmbraceManager.swift
//  Embrace Unity iOS Interface - Heavily based on RNEmbrace Manager
//
//  Created by Alyssa Syharath on 8/21/24.
//

import Foundation
import EmbraceIO
import EmbraceCrash
import EmbraceCommonInternal
import EmbraceOTelInternal

public class EmbraceManager: NSObject {
    static func startNativeSDK(appId: String, appGroupId: String) -> Bool {
        do {
            var embraceOptions: Embrace.Options {
                var crashReporter: CrashReporter? = EmbraceCrashReporter()
                let servicesBuilder = CaptureServiceBuilder().addDefaults()
                var endpoints: Embrace.Endpoints? = nil;
                
                return .init(
                    appId: appId,
                    appGroupId: appGroupId,
                    platform: .unity,
                    endpoints: endpoints,
                    captureServices: servicesBuilder.build(),
                    crashReporter: crashReporter
                )
            }
            
            try Embrace.setup(options: embraceOptions)
            
            return true
        } catch let e {
            print("Error starting Native Embrace SDK \(e.localizedDescription)")
            return false
        }
        
    }
    
    static func isStarted() -> Bool {
        if let embraceStarted = Embrace.client?.started {
            return embraceStarted;
        }
        return false;
    }
}
