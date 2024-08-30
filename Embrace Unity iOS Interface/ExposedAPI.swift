import Foundation
import EmbraceOTelInternal

@_cdecl("embrace_sdk_start_native")
public func embrace_sdk_start_native(appId: UnsafePointer<CChar>?, 
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
                                             appGroupId: _appGroupId,
                                             endpoints: endpoints)
    }
    
    return false;
}

@_cdecl("embrace_sdk_is_started")
public func embrace_sdk_is_started() -> Bool {
    return EmbraceManager.isStarted();
}

@_cdecl("get_device_id")
public func get_device_id() -> UnsafeMutablePointer<Int8>? {
    return convert_str_to_cstr_pointer(str: EmbraceManager.getDeviceId())
}

@_cdecl("get_session_id")
public func get_session_id() -> UnsafeMutablePointer<Int8>? {
    return convert_str_to_cstr_pointer(str: EmbraceManager.getCurrentSessionId())
}

@_cdecl("get_last_run_end_state")
public func get_last_run_end_state() -> Int {
    if let endState = EmbraceManager.getLastRunEndState() {
        return endState.rawValue;
    }
    return 0;
}

@_cdecl("set_user_identifier")
public func set_user_identifier(userIdentifier: UnsafePointer<CChar>?) {
    guard let userIdentifier else {
        return
    }
    
    if let _userIdentifier = String(validatingUTF8: userIdentifier) {
        EmbraceManager.setUserIdentifier(userIdentifier: _userIdentifier)
    }
}

@_cdecl("clear_user_identifier")
public func clear_user_identifier() {
    EmbraceManager.clearUserIdentifier()
}

@_cdecl("add_breadcrumb")
public func add_breadcrumb(event: UnsafePointer<CChar>?) {
    guard let event else {
        return
    }
    
    if let _event = String(validatingUTF8: event) {
        EmbraceManager.addBreadCrumb(event: _event)
    }
}

@_cdecl("set_username")
public func set_username(username: UnsafePointer<CChar>?) {
    guard let username else {
        return
    }
    
    if let _username = String(validatingUTF8: username) {
        EmbraceManager.setUsername(userName: _username)
    }
}

@_cdecl("clear_username")
public func clear_username() {
    EmbraceManager.clearUsername()
}

@_cdecl("set_user_email")
public func set_user_email(email: UnsafePointer<CChar>?) {
    guard let email else {
        return
    }
    
    if let _email = String(validatingUTF8: email) {
        EmbraceManager.setUserEmail(userEmail: _email)
    }
}

@_cdecl("clear_user_email")
public func clear_user_email(email: UnsafePointer<CChar>?) {
    EmbraceManager.clearUserEmail()
}

@_cdecl("add_user_persona")
public func add_user_persona(persona: UnsafePointer<CChar>?) {
    guard let persona else {
        return
    }
    
    if let _persona = String(validatingUTF8: persona) {
        EmbraceManager.addUserPersona(persona: _persona)
    }
}

@_cdecl("clear_user_persona")
public func clear_user_persona(persona: UnsafePointer<CChar>?) {
    guard let persona else {
        return
    }
    
    if let _persona = String(validatingUTF8: persona) {
        EmbraceManager.clearUserPersona(persona: _persona)
    }
}

@_cdecl("clear_all_user_personas")
public func clear_all_user_personas() {
    EmbraceManager.clearAllUserPersonas()
}

@_cdecl("add_session_property")
public func add_session_property(key: UnsafePointer<CChar>?, value: UnsafePointer<CChar>?, permanent: Bool) {
    guard let key, let value else {
        return
    }
    
    if let _key = String(validatingUTF8: key), let _value = String(validatingUTF8: value) {
        EmbraceManager.addSessionProperty(key: _key, value: _value, permanent: permanent)
    }
}

@_cdecl("remove_session_property")
public func remove_session_property(key: UnsafePointer<CChar>?) {
    guard let key else {
        return
    }
    
    if let _key = String(validatingUTF8: key) {
        EmbraceManager.removeSessionProperty(key: _key)
    }
}

@_cdecl("log_message_with_severity_and_properties")
public func log_message_with_severity_and_properties(message: UnsafePointer<CChar>?, severity: UnsafePointer<CChar>?, propsJson: UnsafePointer<CChar>?) {
    
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

@_cdecl("set_user_as_payer")
public func set_user_as_payer() {
    EmbraceManager.setUserAsPayer()
}

@_cdecl("clear_user_as_payer")
public func clear_user_as_payer() {
    EmbraceManager.clearUserAsPayer()
}

@_cdecl("start_view")
public func start_view(viewName: UnsafePointer<CChar>?) -> UnsafeMutablePointer<CChar>? {
    guard let viewName else {
        return nil
    }
    
    if let _viewName = String(validatingUTF8: viewName) {
        let viewId = EmbraceManager.startView(viewName: _viewName)
        return convert_str_to_cstr_pointer(str: viewId)
    }
    
    return nil
}

@_cdecl("end_view")
public func end_view(viewId: UnsafePointer<CChar>?) {
    guard let viewId else {
        return
    }
    
    if let _viewId = String(validatingUTF8: viewId) {
        EmbraceManager.endView(spanId: _viewId)
    }
}

@_cdecl("log_network_request")
public func log_network_request(url: UnsafePointer<CChar>?,
                                httpMethod: UnsafePointer<CChar>?,
                                startInMillis: Double,
                                endInMillis: Double,
                                bytesSent: Double,
                                bytesReceived: Double,
                                statusCode: Double) {
    guard let url, let httpMethod else {
        return
    }
    
    if let _url = String(validatingUTF8: url), let _httpMethod = String(validatingUTF8: httpMethod) {
        EmbraceManager.logNetworkRequest(url: _url,
                                         httpMethod: _httpMethod,
                                         startInMillis: startInMillis,
                                         endInMillis: endInMillis,
                                         bytesSent: bytesSent,
                                         bytesReceived: bytesReceived,
                                         statusCode: statusCode)
    }
}

@_cdecl("log_network_client_error")
public func log_network_client_error(url: UnsafePointer<CChar>?,
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

@_cdecl("start_span")
public func start_span(name: UnsafePointer<CChar>?, parentSpanId: UnsafePointer<CChar>?, startTimeMs: Double) -> UnsafeMutablePointer<CChar>? {
    guard let name, let parentSpanId else {
        return nil
    }
    
    if let _name = String(validatingUTF8: name), let _parentSpanId = String(validatingUTF8: parentSpanId) {
        let spanId = EmbraceManager.startSpan(name: _name, parentSpanId: _parentSpanId, startTimeMs: startTimeMs)
        return convert_str_to_cstr_pointer(str: spanId)
    }
    
    return nil
}

@_cdecl("stop_span")
public func stop_span(spanId: UnsafePointer<CChar>?, errorCodeString: UnsafePointer<CChar>?, endTimeMs: Double) {
    guard let spanId, let errorCodeString else {
        return
    }
    
    if let _spanid = String(validatingUTF8: spanId), let _errorCodeString = String(validatingUTF8: errorCodeString) {
        EmbraceManager.stopSpan(spanId: _spanid, errorCodeString: _errorCodeString, endTimeMs: endTimeMs)
    }
}

@_cdecl("add_span_event_to_span")
public func add_span_event_to_span(spanId: UnsafePointer<CChar>?, name: UnsafePointer<CChar>?, time: Double, attributesJson: UnsafePointer<CChar>?) {
    guard let spanId, let name, let attributesJson else {
        return
    }
    
    if let _spanid = String(validatingUTF8: spanId),
        let _name = String(validatingUTF8: name),
       let _attributesJson = String(validatingUTF8: attributesJson) {
        EmbraceManager.addSpanEventToSpan(spanId: _spanid,
                                          name: _name,
                                          time: time,
                                          attributes: unpack_json_to_typed_dictionary(
                                            jsonStr: attributesJson,
                                            converter: { (str: String) -> AttributeValue in AttributeValue(str) } ))
    }
}

@_cdecl("add_span_attribute_to_span")
public func add_span_attribute_to_span(spanId: UnsafePointer<CChar>?, key: UnsafePointer<CChar>?, value: UnsafePointer<CChar>?) {
    guard let spanId, let key, let value else {
        return
    }
    
    if let _spanId = String(validatingUTF8: spanId), let _key = String(validatingUTF8: key), let _value = String(validatingUTF8: value) {
        EmbraceManager.addSpanAttributeToSpan(spanId: _spanId, key: _key, value: _value)
    }
}

@_cdecl("record_completed_span")
public func record_completed_span(
    name: UnsafePointer<CChar>?,
    startTimeMs: Double,
    endTimeMs: Double,
    errorCodeString: UnsafePointer<CChar>?,
    parentSpanId: UnsafePointer<CChar>?,
    attributesJson: UnsafePointer<CChar>?,
    eventsJson: UnsafePointer<CChar>?) {
        guard let name, let errorCodeString else {
            return
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
            EmbraceManager.recordCompletedSpan(name: _name,
                                               startTimeMs: startTimeMs,
                                               endTimeMs: endTimeMs,
                                               errorCodeString: _errorCodeString,
                                               parentSpanId: _parentSpanId,
                                               attributes: &attributes,
                                               events: events ?? [])
        }
}

@_cdecl("log_handled_exception")
public func log_handled_exception(name: UnsafePointer<CChar>?,
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

@_cdecl("log_unhandled_exception")
public func log_unhandled_exception(name: UnsafePointer<CChar>?,
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

// TODO: Remove the following function before shipping
@_cdecl("test_string")
public func test_string() -> UnsafeMutablePointer<Int8>? {
    print("Fair enough")
    let string = "This is a test string from Swift iOS Native. Hi Alyssa!"
    return strdup(string)
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
