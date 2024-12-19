using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public static class EmbraceTesting
    {
        public const string REQUIRE_GRAPHICS_DEVICE = nameof(REQUIRE_GRAPHICS_DEVICE);

        public const string REQUIRE_GRAPHICS_DEVICE_IGNORE_DESCRIPTION =
            "This test was ignored because it requires a graphics device and Unity is running with the -nographics flag.";

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            bool hasNoGraphicsDevice = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
            ConditionalIgnoreAttribute.AddConditionalIgnoreMapping(REQUIRE_GRAPHICS_DEVICE, hasNoGraphicsDevice);
        }
    }
}