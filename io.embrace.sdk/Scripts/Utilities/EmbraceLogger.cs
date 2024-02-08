#if (UNITY_EDITOR && EMBRACE_SILENCE_EDITOR_TYPE_LOG) || (DEVELOPMENT_BUILD && EMBRACE_SILENCE_DEV_TYPE_LOG) || (!UNITY_EDITOR && !DEVELOPMENT_BUILD && EMBRACE_SILENCE_RELEASE_TYPE_LOG)
    #define EMBRACE_SILENCE_TYPE_LOG
#endif

#if (UNITY_EDITOR && EMBRACE_SILENCE_EDITOR_TYPE_WARNING) || (DEVELOPMENT_BUILD && EMBRACE_SILENCE_DEV_TYPE_WARNING) || (!UNITY_EDITOR && !DEVELOPMENT_BUILD && EMBRACE_SILENCE_RELEASE_TYPE_WARNING)
    #define EMBRACE_SILENCE_TYPE_WARNING
#endif

#if (UNITY_EDITOR && EMBRACE_SILENCE_EDITOR_TYPE_ERROR) || (DEVELOPMENT_BUILD && EMBRACE_SILENCE_DEV_TYPE_ERROR) || (!UNITY_EDITOR && !DEVELOPMENT_BUILD && EMBRACE_SILENCE_RELEASE_TYPE_ERROR)
    #define EMBRACE_SILENCE_TYPE_ERROR
#endif

#if !EMBRACE_SILENCE_TYPE_LOG || !EMBRACE_SILENCE_TYPE_WARNING || !EMBRACE_SILENCE_TYPE_ERROR
    #define EMBRACE_LOG_ENABLED
#endif

using System;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if !EMBRACE_LOG_ENABLED || EMBRACE_SILENCE_TYPE_LOG || EMBRACE_SILENCE_TYPE_WARNING || EMBRACE_SILENCE_TYPE_ERROR
using System.Diagnostics;
#endif

namespace EmbraceSDK
{
    /// <summary>
    /// Wraps UnityEngine.Debug.Log and its variations with conditional attributes so we can optionally omit
    /// logging separately in editor, development builds, and release builds.
    /// </summary>
    public static class EmbraceLogger
    {
        public const string EMBRACE_SILENCE_EDITOR_TYPE_LOG = nameof(EMBRACE_SILENCE_EDITOR_TYPE_LOG);
        public const string EMBRACE_SILENCE_EDITOR_TYPE_WARNING = nameof(EMBRACE_SILENCE_EDITOR_TYPE_WARNING);
        public const string EMBRACE_SILENCE_EDITOR_TYPE_ERROR = nameof(EMBRACE_SILENCE_EDITOR_TYPE_ERROR);
 
        public const string EMBRACE_SILENCE_DEV_TYPE_LOG = nameof(EMBRACE_SILENCE_DEV_TYPE_LOG);
        public const string EMBRACE_SILENCE_DEV_TYPE_WARNING = nameof(EMBRACE_SILENCE_DEV_TYPE_WARNING);
        public const string EMBRACE_SILENCE_DEV_TYPE_ERROR = nameof(EMBRACE_SILENCE_DEV_TYPE_ERROR);
        
        public const string EMBRACE_SILENCE_RELEASE_TYPE_LOG = nameof(EMBRACE_SILENCE_RELEASE_TYPE_LOG);
        public const string EMBRACE_SILENCE_RELEASE_TYPE_WARNING = nameof(EMBRACE_SILENCE_RELEASE_TYPE_WARNING);
        public const string EMBRACE_SILENCE_RELEASE_TYPE_ERROR = nameof(EMBRACE_SILENCE_RELEASE_TYPE_ERROR);

        public const string EMBRACE_USE_THREADING = nameof(EMBRACE_USE_THREADING);

        public const string LOG_TAG = "[Embrace Unity SDK] ";

        // The Conditional attribute used on all functions below depends on the symbols defined at the caller
        // rather than in this script. Therefore, rather than requiring the define statements from the top of this
        // file to be copied into every script which calls into the logger, we instead use a Conditional that is always
        // false, (meaning the invocations will always be stripped), and then use the symbols defined locally
        // to strip out the conditional when the calls should not be stripped.
        private const string EMBRACE_FALSE = nameof(EMBRACE_FALSE);

        private static ILogger _wrappedLogger;

        public static ILogger WrappedLogger
        {
            get => _wrappedLogger;
            set
            {
                if (value == null)
                {
                    _wrappedLogger = Debug.unityLogger;
                    return;
                }

                _wrappedLogger = value;
            }
        }

        public static bool IsSilenced
        {
            #if EMBRACE_LOG_ENABLED
            get => false;
            #else
            get => true;
            #endif
        }

        public static bool WarningsSilenced
        {
            #if !EMBRACE_SILENCE_TYPE_WARNING
            get => IsSilenced;
            #else
            get => true;
            #endif
        }

        public static bool LogsSilenced
        {
            #if !EMBRACE_SILENCE_TYPE_LOG
            get => IsSilenced;
            #else
            get => true;
            #endif
        }
        
        public static bool ErrorsSilenced
        {
            #if !EMBRACE_SILENCE_TYPE_ERROR
            get => IsSilenced;
            #else
            get => true;
            #endif
        }

        static EmbraceLogger()
        {
            _wrappedLogger = Debug.unityLogger;
        }

        public static string GetNullErrorMessage(string objectName)
        {
            return $"null {objectName} is not allowed through the Embrace SDK.";
        }

        #region Log
        #if EMBRACE_SILENCE_TYPE_LOG
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void Log(object message)
        {
            _wrappedLogger.Log(LOG_TAG, message);
        }

        #if EMBRACE_SILENCE_TYPE_LOG
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void Log(string tag, object message)
        {
            _wrappedLogger.Log(tag, message);
        }

        #if !EMBRACE_LOG_ENABLED
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void Log(LogType logType, object message)
        {
            if (IsFilteredLog(logType))
                return;
            
            _wrappedLogger.Log(logType, LOG_TAG, message);
        }

        #if !EMBRACE_LOG_ENABLED
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void Log(LogType logType, object message, Object context)
        {
            if (IsFilteredLog(logType))
                return;
            
            _wrappedLogger.Log(logType, LOG_TAG, message, context);
        }

        #if !EMBRACE_LOG_ENABLED
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void Log(LogType logType, string tag, object message)
        {
            if (IsFilteredLog(logType))
                return;
            
            _wrappedLogger.Log(logType, tag, message);
        }

        #if !EMBRACE_LOG_ENABLED
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void Log(LogType logType, string tag, object message, Object context)
        {
            if (IsFilteredLog(logType))
                return;
            
            _wrappedLogger.Log(logType, tag, message, context);
        }

        #if EMBRACE_SILENCE_TYPE_LOG
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void Log(string tag, object message, Object context)
        {
            _wrappedLogger.Log(tag, message, context);
        }

        #if EMBRACE_SILENCE_TYPE_WARNING
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogWarning(object message)
        {
            _wrappedLogger.LogWarning(LOG_TAG, message);
        }

        #if EMBRACE_SILENCE_TYPE_WARNING
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogWarning(string tag, object message)
        {
            _wrappedLogger.LogWarning(tag, message);
        }

        #if EMBRACE_SILENCE_TYPE_WARNING
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogWarning(string tag, object message, Object context)
        {
            _wrappedLogger.LogWarning(tag, message, context);
        }

        #if EMBRACE_SILENCE_TYPE_ERROR
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogError(object message)
        {
            _wrappedLogger.LogError(LOG_TAG, message);
        }

        #if EMBRACE_SILENCE_TYPE_ERROR
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogError(string tag, object message)
        {
            _wrappedLogger.LogError(tag, message);
        }

        #if EMBRACE_SILENCE_TYPE_ERROR
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogError(string tag, object message, Object context)
        {
            _wrappedLogger.LogError(tag, message, context);
        }

        #if !EMBRACE_LOG_ENABLED
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogFormat(LogType logType, string format, params object[] args)
        {
            if (IsFilteredLog(logType))
                return;
            
            _wrappedLogger.LogFormat(logType, format,  args);
        }

        #if EMBRACE_SILENCE_TYPE_ERROR
        [Conditional(EMBRACE_FALSE)]
        #endif
        public static void LogException(Exception exception)
        {
            _wrappedLogger.LogException(exception);
        }
        #endregion

        public static bool IsFilteredLog(LogType logType)
        {
            if (logType == LogType.Log)
            {
                #if EMBRACE_SILENCE_TYPE_LOG
                return true;
                #else
                return false;
                #endif
            }
            
            if (logType == LogType.Warning)
            {
                #if EMBRACE_SILENCE_TYPE_WARNING
                return true;
                #else
                return false;
                #endif
            }
            
            if (logType == LogType.Error)
            {
                #if EMBRACE_SILENCE_TYPE_ERROR
                return true;
                #else
                return false;
                #endif
            }

            return false;
        }
    }
}