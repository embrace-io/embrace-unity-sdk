using System;
using System.Collections.Generic;

namespace EmbraceSDK
{
    /// Declares the functions that consist of Embrace's public API - specifically
    /// those that are only declared on Unity. You should not use
    /// EmbraceUnityApi directly or implement it in your own custom classes,
    /// as new functions may be added in future. Use the Embrace class instead.
    internal interface IEmbraceUnityApi: IEmbraceApi {

        /// <summary>
        /// Logs errors originating from your application. Although this method is invoked automatically from internal methods
        /// capturing errors and exceptions, it can also be invoked manually in case a custom error message and
        /// stack trace is desired.
        /// </summary>
        /// <param name="exceptionMessage">Custom message that will be attached to this error log.</param>
        /// <param name="stack">Stack trace of the error</param>
        [Obsolete("Please use LogUnhandledUnityException instead. This method will be removed in a future release.")]
        void logUnhandledUnityException(string exceptionMessage, string stack);

        /// <summary>
        /// Logs an unhandled exception originating from your application. Although this method is invoked automatically from internal methods
        /// capturing exceptions, it can also be invoked manually in case a custom error message and
        /// stack trace is desired.
        /// </summary>
        /// <param name="exceptionName">Type name of the exception</param>
        /// <param name="exceptionMessage">The string message associated with the exception (ie, the Exception.Message property).</param>
        /// <param name="stack">The managed stack trace for the exception.</param>
        void LogUnhandledUnityException(string exceptionName, string exceptionMessage, string stack);

        /// <summary>
        /// Logs an unhandled exception originating from your application. Although this method is invoked automatically from internal methods
        /// capturing exceptions, it can also be invoked manually in case a custom error message and
        /// stack trace is desired.
        /// </summary>
        /// <param name="exception">The exception instance to log.</param>
        /// <param name="stack">The managed stack trace for the exception. If null, the value of the exception.StackTrace property is used.</param>
        void LogUnhandledUnityException(Exception exception, string stack = null);

        /// <summary>
        /// Logs a handled exception originating from your application.
        /// </summary>
        /// <param name="exceptionName">Type name of the exception</param>
        /// <param name="exceptionMessage">The string message associated with the exception (ie, the Exception.Message property).</param>
        /// <param name="stack">The managed stack trace for the exception.</param>
        void LogHandledUnityException(string exceptionName, string exceptionMessage, string stack);

        /// <summary>
        /// Logs a handled exception originating from your application.
        /// </summary>
        /// <param name="exception">The exception instance to log.</param>
        /// <param name="stack">The managed stack trace for the exception. If null, the value of the exception.StackTrace property is used.</param>
        void LogHandledUnityException(Exception exception, string stack = null);
    }

    /// Declares the functions that consist of Embrace's public API. You should
    /// not use EmbraceApi directly or implement it in your own custom classes,
    /// as new functions may be added in future. Use the Embrace class instead.
    internal interface IEmbraceApi: ILogsApi, IMomentsApi, INetworkRequestApi, ISessionApi, IUserApi
    {
        /// <summary>
        /// Returns true if StartSDK has been called on the Embrace instance.
        /// </summary>
         bool IsStarted { get; }

        /// <summary>
        /// Starts instrumentation of the application using the Embrace SDK. This should be called during creation of the application, as early as possible.
        /// See Embrace Docs for integration instructions. For compatibility with other SDKs, the Embrace SDK must be initialized after any other SDK.
        /// </summary>
        /// <param name="enableIntegrationTesting">If true, debug sessions (those which are not part of a release APK) will go to the live integration testing tab of the dashboard. If false, they will appear in 'recent sessions'.</param>
        void StartSDK(bool enableIntegrationTesting = false);

        /// <summary>
        /// Logs a breadcrumb.
        /// Breadcrumbs track a user's journey through the application and will be shown on the timeline.
        /// </summary>
        /// <param name="message">the name of the breadcrumb to log</param>
        [System.Obsolete("Please use AddBreadcrumb() instead. This method will be removed in a future release.")]
        void LogBreadcrumb(string message);

        /// <summary>
        /// Adds a breadcrumb.
        /// Breadcrumbs track a user's journey through the application and will be shown on the timeline.
        /// </summary>
        /// <param name="message">the name of the breadcrumb to log</param>
        void AddBreadcrumb(string message);

        /// <summary>
        /// Get the user identifier assigned to the device by Embrace.
        /// </summary>
        /// <returns>the device identifier created by Embrace</returns>
        string GetDeviceId();

        /// <summary>
        /// Opens a view. There is a limit to 10 "started" views.
        /// </summary>
        /// <param name="name">name of the view state as it will show up on our dashboard</param>
        /// <returns>a boolean indicating whether the operation was successful or not</returns>
        bool StartView(string name);

        /// <summary>
        /// Closes the view state for the specified view or logs a warning if the view is not found.
        /// </summary>
        /// <param name="name">name of the view state</param>
        /// <returns>a boolean indicating whether the operation was successful or not</returns>
        bool EndView(string name);

        /// Gets whether the last run of the application ended in a crash or not. This function will return Invalid if
        /// called before the SDK has been started or if the Embrace crash reporter is not enabled.
        /// </summary>
        LastRunEndState GetLastRunEndState();

        /// <summary>
        /// Records push notification information into the session payload on iOS.
        /// </summary>
        /// <param name="iosArgs"></param>
        void RecordPushNotification(iOSPushNotificationArgs iosArgs);
        
        /// <summary>
        /// Records push notification information into the session payload on Android.
        /// </summary>
        /// <param name="androidArgs"></param>
        void RecordPushNotification(AndroidPushNotificationArgs androidArgs);
    }

    /// The public API that is used to send log messages.
    internal interface ILogsApi {

        /// <summary>
        /// Logs an event in your application for aggregation and debugging on the Embrace.io dashboard with an optional dictionary of up to 10 properties.
        /// </summary>
        /// <param name="message">the name of the message, which is how it will show up on the dashboard</param>
        /// <param name="severity">will flag the message as one of info, warning, or error for filtering on the dashboard</param>
        /// <param name="properties">an optional dictionary of up to 10 key/value pairs</param>
        void LogMessage(string message, EMBSeverity severity, Dictionary<string, string> properties = null, bool allowScreenshot = false);

        /// <summary>
        /// Logs an INFO event in your application for aggregation and debugging on the Embrace.io dashboard.
        /// </summary>
        /// <param name="message">the name of the message, which is how it will show up on the dashboard</param>
        void LogInfo(string message);

        /// <summary>
        /// Logs a WARNING event in your application for aggregation and debugging on the Embrace.io dashboard.
        /// </summary>
        /// <param name="message">the name of the message, which is how it will show up on the dashboard</param>
        void LogWarning(string message);
        
        /// <summary>
        /// Logs an ERROR event in your application for aggregation and debugging on the Embrace.io dashboard.
        /// </summary>
        /// <param name="message">the name of the message, which is how it will show up on the dashboard</param>
        void LogError(string message);
    }

    /// The public API that is used to start & end moments.    
    internal interface IMomentsApi {

        /// <summary>
        /// Signals that the app has completed startup. This can be helpful for keeping track of startup times
        /// through the application's embrace.io dashboard.
        /// </summary>
        /// <param name="properties">Properties to include as part of the startup moment</param>
        void EndAppStartup(Dictionary<string, string> properties = null);

        /// <summary>
        /// Starts recording data for an app moment with the provided name, optional identifier, and optional key/value metadata
        /// </summary>
        /// <param name="name">the name used to identify the moment</param>
        /// <param name="identifier">an identifier that is combined with the name to create a unique key for the moment (can be null)</param>
        /// <param name="properties">an optional dictionary containing metadata about the moment to be recorded (limited to 10 keys)</param>
        void StartMoment(string name, string identifier = null, bool allowScreenshot = false, Dictionary<string, string> properties = null);

        /// <summary>
        /// Stops recording data for an app moment with the provided name (and identifier), and adds properties to the moment.
        /// This marks the moment as “completed.” If no moment is found with the provided name (and an empty identifier), this call will be ignored. Additionally, if an app moment was started with a name and identifier, the same identifier must be used to end it.
        /// </summary>
        /// <param name="name">the name used to identify the moment</param>
        /// <param name="identifier">an identifier that is combined with the name to create a unique key for the moment (can be null)</param>
        /// <param name="properties">an optional dictionary containing metadata about the moment to be recorded (limited to 10 keys)</param>
        void EndMoment(string name, string identifier = null, Dictionary<string, string> properties = null);
    }
    
    /// The public API that is used for capturing network requests manually
    internal interface INetworkRequestApi {

        /// <summary>
        /// Logs a network request originating from your application for aggregation and debugging on the Embrace.io dashboard.
        /// </summary>
        /// <param name="url">The url where the request is being sent</param>
        /// <param name="method">The HTTP request method</param>
        /// <param name="startms">The time that the network call started (Unix Timestamp)</param>
        /// <param name="endms">The time that the network call was completed (Unix Timestamp)</param>
        /// <param name="bytesin">The number of bytes returned in response to this network call</param>
        /// <param name="bytesout">The number of bytes sent as part of this network call</param>
        /// <param name="code">The status code returned from the server</param>
        /// <param name="error">An error returned from the server, if any.</param>
        [System.Obsolete("Please use RecordNetworkRequest() instead. This method will be removed in a future release.")]
        void LogNetworkRequest(string url, HTTPMethod method, long startms, long endms, int bytesin, int bytesout, int code, string error);

        /// <summary>
        /// Records a network request originating from your application for aggregation and debugging on the Embrace.io dashboard.
        /// </summary>
        /// <param name="url">The url where the request is being sent</param>
        /// <param name="method">The HTTP request method</param>
        /// <param name="startms">The time that the network call started (Unix Timestamp)</param>
        /// <param name="endms">The time that the network call was completed (Unix Timestamp)</param>
        /// <param name="bytesin">The number of bytes returned in response to this network call</param>
        /// <param name="bytesout">The number of bytes sent as part of this network call</param>
        /// <param name="code">The status code returned from the server</param>
        /// <param name="error">An optional error message describing any non-HTTP errors, such as a connection error or exception.</param>
        void RecordNetworkRequest(string url, HTTPMethod method, long startms, long endms, int bytesin, int bytesout, int code, string error = "");
    }
    
    /// The public API that is used to interact with sessions.
    internal interface ISessionApi {

        /// <summary>
        /// Ends the current session and starts a new one.
        /// </summary>
        /// <param name="clearUserInfo">if true, clears all the user info on the device</param>
        void EndSession(bool clearUserInfo = false);

        /// <summary>
        /// Annotates the session with a new property. Use this to track permanent and ephemeral features of the session. A permanent property is added to all sessions submitted from this device, use this for properties such as work site, building, owner. A non-permanent property is added to only the currently active session.
        /// There is a maximum of 10 total properties in a session.
        /// </summary>
        /// <param name="key">the key for this property, must be unique within session properties</param>
        /// <param name="value">the value to store for this property</param>
        /// <param name="permanent">if true the property is applied to all sessions going forward, persist through app launches.</param>
        void AddSessionProperty(string key, string value, bool permanent);

        /// <summary>
        /// Removes a property from the session. If that property was permanent then it is removed from all future sessions as well.
        /// </summary>
        /// <param name="key">the key for the property you wish to remove</param>
        void RemoveSessionProperty(string key);

        /// <summary>
        /// Get a read-only representation of the currently set session properties.
        /// </summary>
        /// <returns>the properties as key-value pairs of strings</returns>
        Dictionary<string, string> GetSessionProperties();
                
        /// <summary>
        /// Get the ID for the current session.
        /// Returns null if a session has not been started yet or the SDK hasn't been initialized.
        /// </summary>
        /// <returns>the ID for the current Session, if available.</returns>
        string GetCurrentSessionId();
    }
    
    /// The public API that is used to interact with sessions.
    internal interface IUserApi {

        /// <summary>
        /// Sets the user ID. This would typically be some form of unique identifier such as a UUID or database key for the user.
        /// </summary>
        /// <param name="identifier">the unique identifier for the user</param>
        void SetUserIdentifier(string identifier);

        /// <summary>
        /// Removes a previously set user identifier
        /// </summary>
        void ClearUserIdentifier();

        /// <summary>
        /// Sets the username of the currently logged in user.
        /// </summary>
        /// <param name="username">the username to set</param>
        void SetUsername(string username);

        /// <summary>
        /// Clears the username of the currently logged in user, for example if the user has logged out.
        /// </summary>
        void ClearUsername();

        /// <summary>
        /// Sets the current user's email address.
        /// </summary>
        /// <param name="email">the email address of the current user</param>
        void SetUserEmail(string email);

        /// <summary>
        /// Clears the currently set user's email address.
        /// </summary>
        void ClearUserEmail();

        /// <summary>
        /// Sets this user as a paying user. This adds a persona to the user's identity.
        /// </summary>
        void SetUserAsPayer();

        /// <summary>
        /// Clears this user as a paying user. This would typically be called if a user is no longer paying for the service and has reverted back to a basic user.
        /// </summary>
        void ClearUserAsPayer();

        /// <summary>
        /// Sets a custom user persona. A persona is a trait associated with a given user.
        /// </summary>
        /// <param name="persona">the persona to set</param>
        [System.Obsolete("Please use AddUserPersona() instead. This method will be removed in a future release.")]
        void SetUserPersona(string persona);

        /// <summary>
        /// Sets a custom user persona. A persona is a trait associated with a given user.
        /// </summary>
        /// <param name="persona">the persona to set</param>
        void AddUserPersona(string persona);

        /// <summary>
        /// Clears the custom user persona, if it is set.
        /// </summary>
        /// <param name="persona">the persona to clear</param>
        void ClearUserPersona(string persona);

        /// <summary>
        /// Clears all custom user personas from the user.
        /// </summary>
        void ClearAllUserPersonas();
    }
}
