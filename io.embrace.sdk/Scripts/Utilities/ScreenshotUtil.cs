using UnityEngine;

#if UNITY_2020_2_OR_NEWER && UNITY_ANDROID
namespace EmbraceSDK.Utilities
{
    public static class ScreenshotUtil
    {
        internal static byte[] TakeScreenshot()
        {
            // Create a texture in RGB24 format the size of the screen
            var width = Screen.width;
            var height = Screen.height;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Read the screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            
            var bytes = tex.EncodeToJPG();

            if (bytes == null || bytes.Length == 0)
            {
                EmbraceLogger.LogError(EmbraceLogger.GetNullErrorMessage("bugshake screenshot byte array"));
            }

            return bytes;
        }
    }
}
#endif