using System.Collections.Generic;

namespace EmbraceSDK
{
    /// <summary>
    /// Used to compare UnhandledExceptions for equality.
    /// </summary>
    class UnhandledExceptionEqualityComparer : EqualityComparer<UnhandledException>
    {
        public override bool Equals(UnhandledException ue1, UnhandledException ue2)
        {
            return ue1.Message == ue2.Message;
                   //&& ue1.StackTrace == ue2.StackTrace;
        }

        public override int GetHashCode(UnhandledException ue)
        {
            // from https://stackoverflow.com/a/263416
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + ue.Message.GetHashCode();
                //hash = hash * 23 + ue.StackTrace.GetHashCode();
                return hash;
            }
        }

    }
}
