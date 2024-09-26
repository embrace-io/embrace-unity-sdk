import Foundation
import OSLog
import EmbraceIO
import EmbraceCrash
import EmbraceCommonInternal
import EmbraceOTelInternal

public class EmbraceManager: NSObject {
    private var os_log = OSLog(subsystem: "Embrace", category: "UnityiOSNativeEmbraceManager")
    private static var spanRepository = SpanRepository()
    static func startNativeSDK(appId: String, appGroupId: String?, endpoints: (baseUrl: String, devBaseUrl: String, configBaseUrl: String)?) -> Bool {
        do {
            var embraceOptions: Embrace.Options {
                var _crashReporter: CrashReporter? = EmbraceCrashReporter()
                let _servicesBuilder = CaptureServiceBuilder().addDefaults()
                _servicesBuilder.add(.pushNotification()) // Add the PushNotificationCaptureService by default for Unity
                var _endpoints: Embrace.Endpoints? = nil;
                
                if let endpoints {
                    _endpoints = Embrace.Endpoints(
                        baseURL: endpoints.baseUrl,
                        developmentBaseURL: endpoints.devBaseUrl,
                        configBaseURL: endpoints.configBaseUrl)
                }
                
                return .init(
                    appId: appId,
                    appGroupId: appGroupId,
                    platform: .unity,
                    endpoints: _endpoints,
                    captureServices: _servicesBuilder.build(),
                    crashReporter: _crashReporter
                )
            }
            
            try Embrace.setup(options: embraceOptions)
                .start()
            
            return true
        } catch let e {
            print("Error starting Native Embrace SDK \(e.localizedDescription)")
            return false
        }
        
    }
    
    static func isStarted() -> Bool {
        if let embraceStarted = Embrace.client?.started {
            return embraceStarted
        }
        return false
    }
    
    static func crash() {
        Embrace.client?.crash()
    }
    
    static func endCurrentSession() {
        Embrace.client?.endCurrentSession()
    }
    
    static func getDeviceId() -> String? {
        if let deviceId = Embrace.client?.currentDeviceId() {
            return deviceId
        }
        return nil
    }
    
    static func getCurrentSessionId() -> String? {
        if let sessionId = Embrace.client?.currentSessionId() {
            return sessionId
        }
        return nil
    }
    
    static func getLastRunEndState() -> LastRunEndState? {
        if let endState = Embrace.client?.lastRunEndState() {
            return endState
        }
        return nil
    }
    
    static func setUserIdentifier(userIdentifier: String) {
        Embrace.client?.metadata.userIdentifier = userIdentifier
    }
    
    static func clearUserIdentifier() {
        Embrace.client?.metadata.userIdentifier = nil
    }
    
    static func addBreadCrumb(event: String) {
        Embrace.client?.add(event: .breadcrumb(event))
    }
    
    static func setUsername(userName: String) {
        Embrace.client?.metadata.userName = userName
    }
    
    static func clearUsername() {
        Embrace.client?.metadata.userName = nil
    }
    
    static func setUserEmail(userEmail: String) {
        Embrace.client?.metadata.userEmail = userEmail;
    }
    
    static func clearUserEmail() {
        Embrace.client?.metadata.userEmail = nil
    }
    
    static func addUserPersona(persona: String) -> Bool {
        do {
            try Embrace.client?.metadata.add(persona: persona, lifespan: .session)
            return true
        } catch let error {
            return false
        }
    }
    
    static func clearUserPersona(persona: String) -> Bool {
        do {
            try Embrace.client?.metadata.remove(persona: PersonaTag(persona), lifespan: .session)
            return true
        } catch let error {
            return false
        }
    }
    
    static func clearAllUserPersonas() -> Bool {
        do {
            try Embrace.client?.metadata.removeAllPersonas()
            return true
        } catch let error {
            return false
        }
    }
    
    static func addResource(key: String, value: String, lifespan: MetadataLifespan) -> Bool {
        do {
            try Embrace.client?.metadata.addResource(
                key: key, value: value, lifespan: lifespan)
            return true
        } catch let error {
            
        }
        
        return false
    }
    
    static func addSessionProperty(key: String, value: String, permanent: Bool) -> Bool {
        do {
            let lifespan: MetadataLifespan = permanent ? .permanent : .session
            try Embrace.client?.metadata.addProperty(key: key, value: value, lifespan: lifespan)
            return true
        } catch let error {
            return false
        }
    }
    
    static func removeSessionProperty(key: String) -> Bool {
        do {
            try Embrace.client?.metadata.removeProperty(key: key, lifespan: .permanent)
            try Embrace.client?.metadata.removeProperty(key: key, lifespan: .session)
            return true
        } catch let error {
            return false
        }
    }
    
    static func logMessageWithSeverityAndProperties(
        message: String,
        severity: String,
        properties: [String: String]) {
            Embrace.client?.log(message,
                severity: convertStringToLogSeverity(from: severity),
                attributes: properties
            )
    }
    
    static func setUserAsPayer() -> Bool {
        do {
            try Embrace.client?.metadata.add(persona: "payer")
            return true
        } catch let error {
            return false
        }
    }
    
    static func clearUserAsPayer() -> Bool {
        do {
            try Embrace.client?.metadata.remove(persona: "payer", lifespan: .session)
            return true
        } catch let error {
            return false
        }
    }
    
    static func startView(viewName: String) -> String? {
        let span = Embrace.client?.buildSpan(name: "emb-screen-view")
            .setAttribute(key: "view.name", value: viewName)
            .setAttribute(key: "emb.type", value: "ux.view")
            .startSpan()
        
        guard let span else {
            return nil
        }
        
        return spanRepository.spanStarted(span: span)
    }
    
    static func endView(spanId: String) -> Bool {
        return stopSpan(spanId: spanId, errorCodeString: "", endTimeMs: 0.0)
    }
    
    static func logNetworkRequest(url: String,
                                  httpMethod: String,
                                  startInMillis: Double,
                                  endInMillis: Double,
                                  bytesSent: Double,
                                  bytesReceived: Double,
                                  statusCode: Double,
                                  error: String?) {
        var attributes = [
            "http.request.method": httpMethod.uppercased(),
            "url.full": url
        ]
        
        if statusCode >= 0 {
            attributes["http.response.status_code"] = String(Int(statusCode))
        }

        if bytesSent >= 0 {
            attributes["http.request.body.size"] = String(Int(bytesSent))
        }

        if bytesReceived >= 0 {
            attributes["http.response.body.size"] = String(Int(bytesReceived))
        }
        
        if let error {
            attributes["error.message"] = error
        }
        
        Embrace.client?.recordCompletedSpan(name: createNetworkSpanName(url: url, httpMethod: httpMethod),
                                            type: .networkRequest,
                                            parent: nil,
                                            startTime: convertDoubleToDate(ms: startInMillis),
                                            endTime: convertDoubleToDate(ms: endInMillis),
                                            attributes: attributes,
                                            events: [],
                                            errorCode: nil)
    }
    
    static func logNetworkClientError(url: String,
                                      httpMethod: String,
                                      startInMillis: Double,
                                      endInMillis: Double,
                                      errorType: String,
                                      errorMessage: String) {
        // In case it matters, if we stylistically prefer to guard rather than letting optional chaning handling things we could do as below:
        /*
         guard let Embrace.client else {
            // Possibly log the issue
            return
         }
         */
        Embrace.client?.recordCompletedSpan(name: createNetworkSpanName(url: url, httpMethod: httpMethod),
                                            type: .networkRequest,
                                            parent: nil,
                                            startTime: convertDoubleToDate(ms: startInMillis),
                                            endTime: convertDoubleToDate(ms: endInMillis),
                                            attributes: [
                                                "http.request.method": httpMethod.uppercased(),
                                                "url.full": url,
                                                "error.message": errorMessage,
                                                "error.type": errorType
                                            ],
                                            events: [],
                                            errorCode: .failure)
    }
    
    static func startSpan(name: String, parentSpanId: String, startTimeMs: Double) -> String? {
        let spanBuilder = Embrace.client?.buildSpan(name: name)
        
        guard let spanBuilder else {
            return nil
        }
        
        if !parentSpanId.isEmpty, let parent = spanRepository.get(spanId: parentSpanId) {
            spanBuilder.setParent(parent)
        } else {
            spanBuilder.markAsKeySpan()
        }
        
        if startTimeMs > 0.0 {
            spanBuilder.setStartTime(time: convertDoubleToDate(ms: startTimeMs))
        }
        
        let span = spanBuilder.startSpan()
        
        // Return the spanId
        return spanRepository.spanStarted(span: span)
    }
    
    static func stopSpan(spanId: String, errorCodeString: String, endTimeMs: Double) -> Bool {
        guard let span = spanRepository.get(spanId: spanId) else {
            return false
        }
        
        if endTimeMs <= 0.0 {
            span.end(errorCode: convertStringToErrorCode(str: errorCodeString))
        } else {
            span.end(errorCode: convertStringToErrorCode(str: errorCodeString),
                      time: convertDoubleToDate(ms: endTimeMs))
        }
        
        spanRepository.spanEnded(span: span)
        
        return true
    }
    
    static func addSpanEventToSpan(spanId: String, name: String, time: Double, attributes: [String: AttributeValue]) -> Bool {
        guard let span = spanRepository.get(spanId: spanId) else {
            return false
        }
        
        if attributes.isEmpty {
            span.addEvent(name: name, timestamp: convertDoubleToDate(ms: time))
        } else {
            span.addEvent(name: name,
                          attributes: attributes,
                          timestamp: convertDoubleToDate(ms: time))
        }
        
        return true
    }
    
    static func addSpanAttributeToSpan(spanId: String, key: String, value: String) -> Bool {
        guard let span = spanRepository.get(spanId: spanId) else {
            return false
        }
        
        span.setAttribute(key: key, value: value)
        Embrace.client?.flush(span)
        
        return true
    }
    
    static func recordCompletedSpan(
        name: String,
        startTimeMs: Double,
        endTimeMs: Double,
        errorCodeString: String,
        parentSpanId: String,
        attributes: inout [String: String],
        events: [RecordingSpanEvent]) -> Bool {
            
            var parent = parentSpanId.isEmpty ? spanRepository.get(spanId: parentSpanId) : nil
            attributes.updateValue("true", forKey: "emb.key")
            
            if Embrace.client == nil {
                return false
            }
            
            Embrace.client?.recordCompletedSpan(name: name,
                                                type: .performance,
                                                parent: parent,
                                                startTime: convertDoubleToDate(ms: startTimeMs),
                                                endTime: convertDoubleToDate(ms: endTimeMs),
                                                attributes: attributes,
                                                events: events,
                                                errorCode: convertStringToErrorCode(str: errorCodeString))
            return true
    }
    
    // TODO: Reduce code duplication between handled and unhandled exceptions
    static func logHandledException(name: String, message: String, stacktrace: String) {
        var attributes = [
            "exception.stacktrace": stacktrace,
            "emb.exception_handling": "handled",
            "emb.type": "sys.exception",
            "exception.message": message,
            "exception.type": name
        ]
        
        Embrace.client?.log("Unity exception",
                            severity: .error,
                            timestamp: Date(), // Should we let users input their own exception timestamp?
                            attributes: attributes,
                            stackTraceBehavior: .notIncluded)
    }
    
    static func logUnhandledException(name: String, message: String, stacktrace: String) {
        var attributes = [
            "exception.stacktrace": stacktrace,
            "emb.exception_handling": "unhandled",
            "emb.type": "sys.exception",
            "exception.message": message,
            "exception.type": name
        ]
        
        Embrace.client?.log("Unity exception",
                            severity: .error,
                            timestamp: Date(), // Should we let users input their own exception timestamp? That would create an API mismatch among the Hosted SDKs. Let's not.
                            attributes: attributes,
                            stackTraceBehavior: .notIncluded)
    }
    
    static func logPushNotification(title: String, body: String, subtitle: String, badge: Int, category: String) -> Bool {
        
        // Borrowed from Embrace Flutter SDK
        let pushData: [AnyHashable: Any?] = [
                        "aps": [
                            "alert" : [
                                "title" : title,
                                "subtitle" : subtitle,
                                "body" : body
                            ],
                            "badge" : badge as Any,
                            "category" : category as Any
                        ]
                    ]
        do {
            try Embrace.client?.add(event: .push(userInfo: pushData as [AnyHashable: Any]))
            return true
        } catch let error {
            // We should probably log this error
        }
        
        return false
    }
    
    private static func transferKVPs(dest: inout [String:String], src: NSDictionary) {
        for (key, value) in src {
            if let key = key as? String, let value = value as? String {
                dest.updateValue(value, forKey: key)
            }
        }
    }
    
    private static func convertNSDictToSwiftDict<T>(nsDict: NSDictionary, converter: (String) -> T) -> [String: T] {
        var swiftDict = [String: T]()
        
        for (key, value) in nsDict {
            if let key = key as? String, let value = value as? String {
                swiftDict.updateValue(converter(value), forKey: key)
            } else {
                // Should the whole operation fail or should we just log the issue?
                // We probably should not let the whole operation fail. We can skip the value
            }
        }
        
        return swiftDict
    }
    
    private static func createNetworkSpanName(url: String, httpMethod: String) -> String {
        var name = "emb-" + httpMethod.uppercased()
        
        if let fullUrl = URL(string: url) {
            let path = fullUrl.path
            if (!path.isEmpty && path != "/") {
                name += " " + path
            }
        }
        
        return name
    }
    
    // Assumes values in Unix Epoch time
    private static func convertDoubleToDate(ms: Double) -> Date {
        return Date(timeIntervalSince1970: TimeInterval(ms / 1000.0))
    }
    
    private static func convertStringToErrorCode(str: String) -> ErrorCode? {
        switch str {
        case "Failure": return .failure
        case "UserAbandon": return .userAbandon
        case "Unknown": return .unknown
        default: return nil
        }
    }
    
    private static func convertStringToLogSeverity(from inputString: String) -> LogSeverity {
        switch inputString {
        case "info": return .info
        case "warning": return .warn
        default: return .error
        }
    }
}
