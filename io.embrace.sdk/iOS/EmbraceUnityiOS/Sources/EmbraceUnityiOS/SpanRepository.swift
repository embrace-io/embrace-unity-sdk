//
// Copyright © 2025 Embrace Mobile, Inc. All rights reserved.
//
// Created for internal use by the Embrace Unity SDK. Edit at your own risk.
//

// NOTE: this is essentially identical to SpanRepository.Swift in the Embrace
// React Native SDK. See: https://github.com/embrace-io/embrace-react-native-sdk/blob/v5.2.0/packages/core/ios/SpanRepository.swift

import Foundation
import OSLog
import OpenTelemetryApi
import EmbraceCommonInternal

// Should not get hit under normal circumstances, add as a guard against misinstrumentation
private let MAX_STORED_SPANS = 10000

class SpanRepository {

    private var log = OSLog(subsystem: "Embrace", category: "SpanRepository")
    
    private struct SpanData {
        var activeSpans = [String: Span]()
        var completedSpans = [String: Span]()
    }
    private let spanCache: EmbraceMutex<SpanData> = EmbraceMutex(SpanData())
    
    init() {
        NotificationCenter.default.addObserver(
            self,
            selector: #selector(onSessionEnded),
            name: Notification.Name.embraceSessionWillEnd,
            object: nil)
    }

    deinit {
        NotificationCenter.default.removeObserver(self)
    }

    func get(spanId: String) -> Span? {
        spanCache.withLock {
            guard let span = $0.activeSpans[spanId] else {
                return $0.completedSpans[spanId]
            }
            return span
        }
    }

    func spanStarted(span: Span) -> String {
        spanCache.withLock {
            guard $0.activeSpans.count <= MAX_STORED_SPANS else {
                os_log("Embrace SpanRepository.swift --> Too many active spans being tracked. Ignoring new active span. Please consider reducing the number of spans being tracked.",
                       log: self.log,
                       type: .error)
                return ""
            }

            let key = getKey(span: span)
            $0.activeSpans.updateValue(span, forKey: key)
            return key
        }
    }

    func spanEnded(span: Span) {
        let key = getKey(span: span)
        spanCache.withLock {
            $0.activeSpans.removeValue(forKey: key)
            guard $0.completedSpans.count <= MAX_STORED_SPANS else {
                os_log("Embrace SpanRepository.swift --> Too many completed spans being tracked. Ignoring new completed span. Please consider reducing the number of spans being tracked.",
                       log: self.log,
                       type: .error)
                return
            }
            $0.completedSpans.updateValue(span, forKey: key)
        }
    }

    @objc func onSessionEnded() {
        spanCache.withLock {
            $0.completedSpans.removeAll()
        }
    }

    private func getKey(span: Span) -> String {
        return "\(span.context.spanId.hexString)_\(span.context.traceId.hexString)"
    }
}
