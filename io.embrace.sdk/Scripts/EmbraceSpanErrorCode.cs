namespace EmbraceSDK
{
    public enum EmbraceSpanErrorCode
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        NONE,
        /// <summary>
        /// An application failure caused the Span to terminate
        /// </summary>
        FAILURE,

        /// <summary>
        /// The operation tracked by the Span was terminated because the user abandoned and canceled it before it can complete successfully.
        /// </summary>
        USER_ABANDON,
        
        /// <summary>
        /// The reason for the unsuccessful termination is unknown
        /// </summary>
        UNKNOWN
    }
}
