namespace EmbraceSDK
{
    /// <summary>
    /// Represents HTTP methods used when logging network requests.
    /// enum values are converted to int when used as arguments in iOS and Android method calls.
    /// </summary>
    public enum HTTPMethod
    {
        GET = 0,
        POST,
        PUT,
        DELETE,
        PATCH,
        OTHER
    }
}
