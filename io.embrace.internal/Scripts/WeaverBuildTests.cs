using System;
using UnityEngine.Scripting;

namespace EmbraceSDK.Internal
{
    /// <summary>
    /// Contains test cases for the Embrace weaver that have been shown to cause errors when compiling in release mode.
    /// </summary>
    public class WeaverBuildTests
    {
        [Preserve]
        public static void WeaverDoesNotThrow_WhenFirstInstruction_LoadsIDispoable(IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}