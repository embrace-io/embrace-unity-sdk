namespace EmbraceSDK
{
    /// <summary>
    /// Represents the end state of the last run of the application.
    /// </summary>
    public enum LastRunEndState
    {
        /// <summary>
        /// The SDK has not been started yet or the crash provider is not Embrace
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The last run resulted in a crash
        /// </summary>
        Crash = 1,

        /// <summary>
        /// The last run did not result in a crash
        /// </summary>
        CleanExit = 2,
    }
}