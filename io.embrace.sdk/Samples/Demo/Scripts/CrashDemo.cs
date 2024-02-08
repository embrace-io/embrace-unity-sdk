using UnityEngine.Diagnostics;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demo demonstrates various crashes which are captured by embrace. For more info please see our documentation.
    /// https://embrace.io/docs/unity/integration/crash-report/
    /// </summary>
    public class CrashDemo : DemoBase
    {
        public void Start()
        {
            Embrace.Instance.StartSDK();
        }

        public void NativeCrash()
        {
            #pragma warning disable CS0618
            Embrace.Instance.Crash();
            #pragma warning restore CS0618
        }

        public void AccessViolation()
        {
            Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
        }

        public void FatalError()
        {
            Utils.ForceCrash(ForcedCrashCategory.FatalError);
        }

        public void Abort()
        {
            Utils.ForceCrash(ForcedCrashCategory.Abort);
        }

        public void PureVirtualFunction()
        {
            Utils.ForceCrash(ForcedCrashCategory.PureVirtualFunction);

        }

        public void ThrowException()
        {
            throw new System.Exception("This is a test exception thrown from the Embrace demo app.");
        }
    }
}