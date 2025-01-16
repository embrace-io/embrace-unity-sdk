namespace Embrace.Tools
{
    /// <summary>
    /// Credentails used with the Publisher Tool
    /// </summary>
    public class Credentials
    {
        public string awsAccessKey;
        public string awsSecretKey;

        public string npmAPIEndpoint;

        public bool HasCredentials()
        {
            if (string.IsNullOrEmpty(awsAccessKey) || string.IsNullOrEmpty(awsSecretKey))
            {
                return false;
            }

            return true;
        }

        public bool HasEndpoint()
        {
            if (string.IsNullOrEmpty(npmAPIEndpoint))
            {
                return false;
            }

            return true;
        }
    }

}