using System;

namespace EmbraceSDK { 
    /// <summary>
    /// An unhandled exception.
    /// </summary>
    public class UnhandledException
    {
        private string[] newLineSplitter = new string[] { "\n" };

        public UnhandledException(string message, string stackTrace)
        {
            // Ignore everything beyond the first line. In Unity 2019, one of the subsequent lines can include
            // tap coordinates, which can vary between otherwise equivalent exceptions.
            Message = message.Split(newLineSplitter, StringSplitOptions.None)[0];
            StackTrace = stackTrace;
        }

        public string Message { get; }

        public string StackTrace { get; }
    }
}
