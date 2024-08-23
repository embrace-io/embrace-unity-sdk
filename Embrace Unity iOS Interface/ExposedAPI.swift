//
//  ExposedAPI.swift
//  Embrace Unity iOS Interface
//
//  Created by Alyssa Syharath on 8/22/24.
//

import Foundation

@_cdecl("embrace_sdk_is_started")
public func embrace_sdk_is_started() -> Bool {
    return EmbraceManager.isStarted();
}

@_cdecl("embrace_sdk_start_native")
public func embrace_sdk_start_native(appId: UnsafePointer<CChar>?, 
                                     appGroupId: UnsafePointer<CChar>?) -> Bool {
    guard let appId else {
        return false;
    }
    
    guard let appGroupId else {
        return false;
    }
    
    if let _appId = String(validatingUTF8: appId), let _appGroupId = String(validatingUTF8: appGroupId) {
        return EmbraceManager.startNativeSDK(appId: _appId,
                                             appGroupId: _appGroupId);
    }
    
    return false;
}
