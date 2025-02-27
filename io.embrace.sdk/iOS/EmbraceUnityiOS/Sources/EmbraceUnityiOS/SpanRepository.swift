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

// Should not get hit under normal circumstances, add as a guard against misinstrumentation
private let MAX_STORED_SPANS = 10000

class SpanRepository {
    private let activeSpansQueue = DispatchQueue(label: "io.embrace.Unity.activeSpans",
                                                 attributes: .concurrent)
    private let completedSpansQueue = DispatchQueue(label: "io.embrace.Unity.completedSpans",
                                                    attributes: .concurrent)
    private var activeSpans = [String: Span]()
    private var completedSpans = [String: Span]()
    private var log = OSLog(subsystem: "Embrace", category: "SpanRepository")

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
        var span: Span?

        activeSpansQueue.sync {
            span = activeSpans[spanId]
        }

        if span == nil {
            completedSpansQueue.sync {
                span = completedSpans[spanId]
            }
        }

        return span
    }

    func spanStarted(span: Span) -> String {
        var key = getKey(span: span)

        activeSpansQueue.async(flags: .barrier) {
            if self.activeSpans.count > MAX_STORED_SPANS {
                os_log("Embrace SpanRepository.swift --> Too many active spans being tracked. Ignoring new active span. Please consider reducing the number of spans being tracked.",
                       log: self.log,
                       type: .error)
                key = "";
                return
            }

            self.activeSpans.updateValue(span, forKey: key)
        }

        return key
    }

    func spanEnded(span: Span) {
        let key = getKey(span: span)

        activeSpansQueue.async(flags: .barrier) {
            self.activeSpans.removeValue(forKey: key)
        }

        completedSpansQueue.async(flags: .barrier) {
            if self.completedSpans.count > MAX_STORED_SPANS {
                os_log("Embrace SpanRepository.swift --> Too many completed spans being tracked. Ignoring new completed span. Please consider reducing the number of spans being tracked.",
                       log: self.log,
                       type: .error)
                return
            }
            self.completedSpans.updateValue(span, forKey: key)
        }
    }

    @objc func onSessionEnded() {
        completedSpansQueue.async(flags: .barrier) {
            self.completedSpans.removeAll()
        }
    }

    private func getKey(span: Span) -> String {
        return "\(span.context.spanId.hexString)_\(span.context.traceId.hexString)"
    }
}
