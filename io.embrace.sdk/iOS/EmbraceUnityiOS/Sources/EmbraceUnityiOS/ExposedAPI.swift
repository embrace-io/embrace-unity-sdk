//
// Copyright © 2025 Embrace Mobile, Inc. All rights reserved.
//
// Created for internal use by the Embrace Unity SDK. Edit at your own risk.
//

import Foundation
import EmbraceOTelInternal
import OpenTelemetryApi

@_cdecl("embrace_sdk_start_native")
public func embrace_sdk_start_native(appId: UnsafePointer<CChar>?,
                                     config: Int,
                                     appGroupId: UnsafePointer<CChar>?,
                                     baseUrl: UnsafePointer<CChar>?,
                                     devBaseUrl: UnsafePointer<CChar>?,
                                     configBaseUrl: UnsafePointer<CChar>?) -> Bool {
    guard let appId else {
        return false;
    }

    var _appGroupId: String?
    if let appGroupId {
        _appGroupId = String(validatingUTF8: appGroupId)
    } else {
        _appGroupId = nil;
    }

    var endpoints: (baseUrl: String,
                     devBaseUrl: String,
                     configBaseUrl: String)? = nil;
    if let baseUrl, let devBaseUrl, let configBaseUrl {
        if let _baseUrl = String(validatingUTF8: baseUrl),
           let _devBaseUrl = String(validatingUTF8: devBaseUrl),
           let _configBaseUrl = String(validatingUTF8: configBaseUrl) {
            endpoints = (_baseUrl, _devBaseUrl, _configBaseUrl)
        }
    }

    if let _appId = String(validatingUTF8: appId) {
        return EmbraceManager.startNativeSDK(appId: _appId,
                                             config: ConfigOptions(rawValue: config),
                                             appGroupId: _appGroupId,
                                             endpoints: endpoints)
    }

    return false;
}

@_cdecl("embrace_sdk_is_started")
public func embrace_sdk_is_started() -> Bool {
    return EmbraceManager.isStarted();
}

@_cdecl("embrace_ios_sdk_version")
public func embrace_ios_sdk_version() -> UnsafeMutablePointer<Int8>? {
    return convert_str_to_cstr_pointer(str: EmbraceManager.sdkVersion());
}

@_cdecl("embrace_crash")
public func embrace_crash() {
    EmbraceManager.crash()
}

@_cdecl("embrace_set_unity_metadata")
public func embrace_set_unity_metadata(unityVersion: UnsafePointer<CChar>?, buildGuid: UnsafePointer<CChar>?, sdkVersion: UnsafePointer<CChar>?) {
    guard let unityVersion, let buildGuid, let sdkVersion else {
        return
    }

    if let _unityVersion = String(validatingUTF8: unityVersion), let _buildGuid = String(validatingUTF8: buildGuid), let _sdkVersion = String(validatingUTF8: sdkVersion) {
        _ = EmbraceManager.addResource(key: "hosted_platform_version", value: _unityVersion, lifespan: .process)
        _ = EmbraceManager.addResource(key: "unity_build_id", value: _buildGuid, lifespan: .process)
        _ = EmbraceManager.addResource(key: "hosted_sdk_version", value: _sdkVersion, lifespan: .process)
    }
}

@_cdecl("embrace_end_session")
public func embrace_end_session() {
    EmbraceManager.endCurrentSession()
}

@_cdecl("embrace_get_device_id")
public func embrace_get_device_id() -> UnsafeMutablePointer<Int8>? {
    return convert_str_to_cstr_pointer(str: EmbraceManager.getDeviceId())
}

@_cdecl("embrace_get_session_id")
public func embrace_get_session_id() -> UnsafeMutablePointer<Int8>? {
    return convert_str_to_cstr_pointer(str: EmbraceManager.getCurrentSessionId())
}

@_cdecl("embrace_get_last_run_end_state")
public func embrace_get_last_run_end_state() -> Int {
    if let endState = EmbraceManager.getLastRunEndState() {
        return endState.rawValue;
    }
    return 0;
}

@_cdecl("embrace_set_user_identifier")
public func embrace_set_user_identifier(userIdentifier: UnsafePointer<CChar>?) {
    guard let userIdentifier else {
        return
    }

    if let _userIdentifier = String(validatingUTF8: userIdentifier) {
        EmbraceManager.setUserIdentifier(userIdentifier: _userIdentifier)
    }
}

@_cdecl("embrace_clear_user_identifier")
public func embrace_clear_user_identifier() {
    EmbraceManager.clearUserIdentifier()
}

@_cdecl("embrace_add_breadcrumb")
public func embrace_add_breadcrumb(event: UnsafePointer<CChar>?) {
    guard let event else {
        return
    }

    if let _event = String(validatingUTF8: event) {
        EmbraceManager.addBreadCrumb(event: _event)
    }
}

@_cdecl("embrace_set_username")
public func embrace_set_username(username: UnsafePointer<CChar>?) {
    guard let username else {
        return
    }

    if let _username = String(validatingUTF8: username) {
        EmbraceManager.setUsername(userName: _username)
    }
}

@_cdecl("embrace_clear_username")
public func embrace_clear_username() {
    EmbraceManager.clearUsername()
}

@_cdecl("embrace_set_user_email")
public func embrace_set_user_email(email: UnsafePointer<CChar>?) {
    guard let email else {
        return
    }

    if let _email = String(validatingUTF8: email) {
        EmbraceManager.setUserEmail(userEmail: _email)
    }
}

@_cdecl("embrace_clear_user_email")
public func embrace_clear_user_email(email: UnsafePointer<CChar>?) {
    EmbraceManager.clearUserEmail()
}

@_cdecl("embrace_add_user_persona")
public func embrace_add_user_persona(persona: UnsafePointer<CChar>?) {
    guard let persona else {
        return
    }

    if let _persona = String(validatingUTF8: persona) {
        _ = EmbraceManager.addUserPersona(persona: _persona)
    }
}

@_cdecl("embrace_clear_user_persona")
public func embrace_clear_user_persona(persona: UnsafePointer<CChar>?) {
    guard let persona else {
        return
    }

    if let _persona = String(validatingUTF8: persona) {
        _ = EmbraceManager.clearUserPersona(persona: _persona)
    }
}

@_cdecl("embrace_clear_all_user_personas")
public func embrace_clear_all_user_personas() {
    _ = EmbraceManager.clearAllUserPersonas()
}

@_cdecl("embrace_add_session_property")
public func embrace_add_session_property(key: UnsafePointer<CChar>?, value: UnsafePointer<CChar>?, permanent: Bool) -> Bool {
    guard let key, let value else {
        return false
    }

    if let _key = String(validatingUTF8: key), let _value = String(validatingUTF8: value) {
        return EmbraceManager.addSessionProperty(key: _key, value: _value, permanent: permanent)
    }

    return false
}

@_cdecl("embrace_remove_session_property")
public func embrace_remove_session_property(key: UnsafePointer<CChar>?) {
    guard let key else {
        return
    }

    if let _key = String(validatingUTF8: key) {
        _ = EmbraceManager.removeSessionProperty(key: _key)
    }
}

@_cdecl("embrace_log_message_with_severity_and_properties")
public func embrace_log_message_with_severity_and_properties(message: UnsafePointer<CChar>?, severity: UnsafePointer<CChar>?, propsJson: UnsafePointer<CChar>?) {

    guard let message, let severity else {
        return
    }

    if let _message = String(validatingUTF8: message),
        let _severity = String(validatingUTF8: severity) {
        EmbraceManager.logMessageWithSeverityAndProperties(message: _message,
                                                           severity: _severity,
                                                           properties:
                                                            unpack_json_to_typed_dictionary(
                                                                jsonStr: propsJson,
                                                                converter: { (str: String) -> String in str }))
    }
}

@_cdecl("embrace_log_message_with_attachment")
public func embrace_log_message_with_attachment(message: UnsafePointer<CChar>?, severity: UnsafePointer<CChar>?,
    propsJson: UnsafePointer<CChar>?, attachment: UnsafePointer<UInt8>?, length: Int) {
    guard let message, let severity else {
        return
    }

    guard let data = attachment, length > 0 else {
        return
    }

    let attachmentData = Data(bytes: data, count: length)

    if let _message = String(validatingUTF8: message),
        let _severity = String(validatingUTF8: severity) {
        EmbraceManager.logMessageWithAttachment(message: _message,
                                                severity: _severity,
                                                attributes: unpack_json_to_typed_dictionary(
                                                                jsonStr: propsJson,
                                                                converter: { (str: String) -> String in str }),
                                                attachment: attachmentData)
    }
}

@_cdecl("embrace_log_message_with_attachment_url")
public func embrace_log_message_with_attachment_url(message: UnsafePointer<CChar>?, severity: UnsafePointer<CChar>?,
    propsJson: UnsafePointer<CChar>?, attachmentId: UnsafePointer<CChar>?, attachmentUrl: UnsafePointer<CChar>?) {
    guard let message, let severity else {
        return
    }

    guard let attachmentId, let attachmentUrl else {
        embrace_log_message_with_severity_and_properties(message: message, severity: severity, propsJson: propsJson)
        return
    }

    if let _message = String(validatingUTF8: message),
        let _severity = String(validatingUTF8: severity),
        let _attachmentId = String(validatingUTF8: attachmentId),
        let _attachmentUrl = String(validatingUTF8: attachmentUrl) {
        EmbraceManager.logMessageWithAttachmentUrl(message: _message,
                                                   severity: _severity,
                                                   attributes: unpack_json_to_typed_dictionary(
                                                                   jsonStr: propsJson,
                                                                   converter: { (str: String) -> String in str }),
                                                   attachmentId: _attachmentId,
                                                   attachmentUrl: _attachmentUrl)
    }
}

@_cdecl("embrace_set_user_as_payer")
public func embrace_set_user_as_payer() {
    _ = EmbraceManager.setUserAsPayer()
}

@_cdecl("embrace_clear_user_as_payer")
public func embrace_clear_user_as_payer() {
    _ = EmbraceManager.clearUserAsPayer()
}

@_cdecl("embrace_start_view")
public func embrace_start_view(viewName: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
    guard let viewName else {
        return nil
    }

    if let _viewName = String(validatingUTF8: viewName) {
        let viewId = EmbraceManager.startView(viewName: _viewName)
        return convert_str_to_cstr_pointer(str: viewId)
    }

    return nil
}

@_cdecl("embrace_end_view")
public func embrace_end_view(viewId: UnsafePointer<CChar>?) -> Bool {
    guard let viewId else {
        return false
    }

    if let _viewId = String(validatingUTF8: viewId) {
        return EmbraceManager.endView(spanId: _viewId)
    }

    return false
}

@_cdecl("embrace_log_network_request")
public func embrace_log_network_request(url: UnsafePointer<CChar>?,
                                httpMethod: UnsafePointer<CChar>?,
                                startInMillis: Double,
                                endInMillis: Double,
                                bytesSent: Double,
                                bytesReceived: Double,
                                statusCode: Double,
                                error: UnsafePointer<CChar>?) {
    guard let url, let httpMethod else {
        return
    }

    if let _url = String(validatingUTF8: url), let _httpMethod = String(validatingUTF8: httpMethod) {
        if let error, let _error = String(validatingUTF8: error) {
            EmbraceManager.logNetworkRequest(url: _url,
                                             httpMethod: _httpMethod,
                                             startInMillis: startInMillis,
                                             endInMillis: endInMillis,
                                             bytesSent: bytesSent,
                                             bytesReceived: bytesReceived,
                                             statusCode: statusCode,
                                             error: _error)
        } else {
            EmbraceManager.logNetworkRequest(url: _url,
                                             httpMethod: _httpMethod,
                                             startInMillis: startInMillis,
                                             endInMillis: endInMillis,
                                             bytesSent: bytesSent,
                                             bytesReceived: bytesReceived,
                                             statusCode: statusCode,
                                             error: nil)
        }
    }
}

@_cdecl("embrace_log_network_client_error")
public func embrace_log_network_client_error(url: UnsafePointer<CChar>?,
                                     httpMethod: UnsafePointer<CChar>?,
                                     startInMillis: Double,
                                     endInMillis: Double,
                                     errorType: UnsafePointer<CChar>?,
                                     errorMessage: UnsafePointer<CChar>?) {
    guard let url, let httpMethod, let errorType, let errorMessage else {
        return
    }

    if let _url = String(validatingUTF8: url),
       let _httpMethod = String(validatingUTF8: httpMethod),
       let _errorType = String(validatingUTF8: errorType),
       let _errorMessage = String(validatingUTF8: errorMessage) {
        EmbraceManager.logNetworkClientError(url: _url,
                                             httpMethod: _httpMethod,
                                             startInMillis: startInMillis,
                                             endInMillis: endInMillis,
                                             errorType: _errorType,
                                             errorMessage: _errorMessage)
    }
}

@_cdecl("embrace_start_span")
public func embrace_start_span(name: UnsafePointer<CChar>?, parentSpanId: UnsafePointer<CChar>?, startTimeMs: Double) -> UnsafeMutablePointer<CChar>? {
    guard let name else {
        return nil
    }

    if let _name = String(validatingUTF8: name) {
        if let parentSpanId, let _parentSpanId = String(validatingUTF8: parentSpanId)  {
            let spanId = EmbraceManager.startSpan(name: _name, parentSpanId: _parentSpanId, startTimeMs: startTimeMs)
            return convert_str_to_cstr_pointer(str: spanId)
        }
        else {
            let spanId = EmbraceManager.startSpan(name: _name, parentSpanId: nil, startTimeMs: startTimeMs)
            return convert_str_to_cstr_pointer(str: spanId)
        }
    }

    return nil
}

@_cdecl("embrace_stop_span")
public func embrace_stop_span(spanId: UnsafePointer<CChar>?, errorCodeString: UnsafePointer<CChar>?, endTimeMs: Double) -> Bool {
    guard let spanId, let errorCodeString else {
        return false
    }

    if let _spanid = String(validatingUTF8: spanId), let _errorCodeString = String(validatingUTF8: errorCodeString) {
        return EmbraceManager.stopSpan(spanId: _spanid, errorCodeString: _errorCodeString, endTimeMs: endTimeMs)
    }

    return false
}

@_cdecl("embrace_add_span_event_to_span")
public func embrace_add_span_event_to_span(spanId: UnsafePointer<CChar>?, name: UnsafePointer<CChar>?, time: Double, attributesJson: UnsafePointer<CChar>?) -> Bool {
    guard let spanId, let name, let attributesJson else {
        return false
    }

    if let _spanid = String(validatingUTF8: spanId),
        let _name = String(validatingUTF8: name) {
        return EmbraceManager.addSpanEventToSpan(spanId: _spanid,
                                          name: _name,
                                          time: time,
                                          attributes: unpack_json_to_typed_dictionary(
                                            jsonStr: attributesJson,
                                            converter: { (str: String) -> AttributeValue in AttributeValue(str) } ))
    }

    return false
}

@_cdecl("embrace_add_span_attribute_to_span")
public func embrace_add_span_attribute_to_span(spanId: UnsafePointer<CChar>?, key: UnsafePointer<CChar>?, value: UnsafePointer<CChar>?) -> Bool {
    guard let spanId, let key, let value else {
        return false
    }

    if let _spanId = String(validatingUTF8: spanId), let _key = String(validatingUTF8: key), let _value = String(validatingUTF8: value) {
        return EmbraceManager.addSpanAttributeToSpan(spanId: _spanId, key: _key, value: _value)
    }

    return false
}

@_cdecl("embrace_record_completed_span")
public func embrace_record_completed_span(
    name: UnsafePointer<CChar>?,
    startTimeMs: Double,
    endTimeMs: Double,
    errorCodeString: UnsafePointer<CChar>?,
    parentSpanId: UnsafePointer<CChar>?,
    attributesJson: UnsafePointer<CChar>?,
    eventsJson: UnsafePointer<CChar>?) -> Bool {
        guard let name, let errorCodeString else {
            return false
        }

        var _parentSpanId: String = "";
        if let parentSpanId {
            _parentSpanId = String(validatingUTF8: parentSpanId) ?? ""
        }

        var events: [RecordingSpanEvent]? = nil;
        if let eventsJson,
           let jsonData = String(validatingUTF8: eventsJson)?.data(using: .utf8) {
            do {
                let jsonBlob = try JSONSerialization.jsonObject(with: jsonData, options: [])
                if let event_array = jsonBlob as? [[String: Any]] {
                    events = unpack_event_array_to_event_object_array(events: event_array)
                } else {
                    print("Type declaration for array incorrect")
                }
            } catch let e {
                print("Error decoding JSON array: \(e.localizedDescription)")
            }
        }

        if let _name = String(validatingUTF8: name),
           let _errorCodeString = String(validatingUTF8: errorCodeString) {
            var attributes = unpack_json_to_typed_dictionary(
                jsonStr: attributesJson,
                converter: { (str: String) -> String in str })
            return EmbraceManager.recordCompletedSpan(name: _name,
                                               startTimeMs: startTimeMs,
                                               endTimeMs: endTimeMs,
                                               errorCodeString: _errorCodeString,
                                               parentSpanId: _parentSpanId,
                                               attributes: &attributes,
                                               events: events ?? [])
        }

        return false
}

@_cdecl("embrace_log_handled_exception")
public func embrace_log_handled_exception(name: UnsafePointer<CChar>?,
                                  message: UnsafePointer<CChar>?,
                                  stacktrace: UnsafePointer<CChar>?) {
    guard let name, let message, let stacktrace else {
        return
    }

    if let _name = String(validatingUTF8: name),
       let _message = String(validatingUTF8: message),
       let _stacktrace = String(validatingUTF8: stacktrace) {
        EmbraceManager.logHandledException(name: _name, message: _message, stacktrace: _stacktrace)
    }
}

@_cdecl("embrace_log_unhandled_exception")
public func embrace_log_unhandled_exception(name: UnsafePointer<CChar>?,
                                    message: UnsafePointer<CChar>?,
                                    stacktrace: UnsafePointer<CChar>?) {
    guard let name, let message, let stacktrace else {
        return
    }

    if let _name = String(validatingUTF8: name),
       let _message = String(validatingUTF8: message),
       let _stacktrace = String(validatingUTF8: stacktrace) {
        EmbraceManager.logUnhandledException(name: _name, message: _message, stacktrace: _stacktrace)
    }
}

@_cdecl("embrace_log_push_notification")
public func embrace_log_push_notification(title: UnsafePointer<CChar>?, body: UnsafePointer<CChar>?, subtitle: UnsafePointer<CChar>?, badge: Int, category: UnsafePointer<CChar>?) {
    guard let title, let body, let subtitle, let category else {
        return
    }

    if let _title = String(validatingUTF8: title),
       let _body = String(validatingUTF8: body),
       let _subtitle = String(validatingUTF8: subtitle),
       let _category = String(validatingUTF8: category) {
        _ = EmbraceManager.logPushNotification(title: _title, body: _body, subtitle: _subtitle, badge: badge, category: _category)
    }
}

private func unpack_event_array_to_event_object_array(events: [[String: Any]]) -> [RecordingSpanEvent] {
    var spanEvents = [RecordingSpanEvent]()
    for event in events {
        let (name, date, attributes) = unpack_event_to_tuple(event: event)
        let newEvent = RecordingSpanEvent(name: name,
                                          timestamp: date,
                                          attributes: attributes)
        spanEvents.append(newEvent)
    }
    return spanEvents
}

private func unpack_event_to_tuple(event: [String:Any]) -> (name: String, date: Date, attributes: [String: AttributeValue]) {

    // This conversion is pretty fragile. Is there anything we can do to limit impact?
    let name = event["name"] as? String ?? ""
    let timestamp = Double(event["timestampMs"] as? Int64 ?? 0)
    let date = Date(timeIntervalSince1970: TimeInterval(timestamp / 1000.0))
    let attributes = event["attributes"] as? [String: String] ?? [:]

    var conv_attr = [String: AttributeValue]()
    for (key, value) in attributes {
        conv_attr[key] = AttributeValue(value)
    }

    return (name, date, conv_attr)
}

private func unpack_json_to_typed_dictionary<T>(jsonStr: UnsafePointer<CChar>?,
                                                 converter: ((String) -> T)) -> [String: T] {
    if let jsonStr,
       let jsonData = String(validatingUTF8: jsonStr)?.data(using: .utf8) {
        do {
            if let string_dict = try JSONSerialization.jsonObject(with: jsonData) as? [String: String] {
                var typed_dict: [String: T] = [:]

                for (key, value) in string_dict {
                    typed_dict[key] = converter(value)
                }

                return typed_dict
            }
        } catch let e {
            print("Failed to deserialize JSON \(e.localizedDescription)")
        }
    }

    return [String: T]()
}

private func convert_str_to_cstr_pointer(str: String?) -> UnsafeMutablePointer<Int8>? {
    guard let str else {
        return nil
    }
    return strdup(str)
}

struct EmbraceSpanEvent: Codable {
    var name: String
    var timestampMs: Double
    var timestampNanos: Double
    var attributes: [String: AttributeValue]
}
