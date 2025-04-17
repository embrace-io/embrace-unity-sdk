namespace EmbraceSDK
{
    internal static class UnhandledExceptionUtility
    {
        /// <summary>
        /// Splits the Unity-formatted exception type name and message into separate strings.
        /// </summary>
        /// <param name="exception">The exception string provided by Unity in the format "ExceptionType: Exception message."</param>
        public static (string name, string message) SplitConcatenatedExceptionNameAndMessage(string exception)
        {
            if (string.IsNullOrEmpty(exception))
            {
                return ("", "");
            }

            var separatorIndex = exception.IndexOf(':');
            if(separatorIndex < 0)
            {
                return ("", exception);
            }

            var name = exception.Substring(0, separatorIndex);
            var message = exception.Substring(separatorIndex + 1);

            return (name, message);
        }
    }
}