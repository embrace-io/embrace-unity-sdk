using UnityEngine;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using System.IO;
using EmbraceSDK.Internal;

namespace EmbraceSDK.Tests
{
    public class PackageInfoTests
    {
        [Test]
        public void PackageVersion_IsEqualTo_SdkInfoVersion()
        {
            string packageJson = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/package.json");
            Package package = JsonUtility.FromJson<Package>(packageJson);

            TextAsset targetFile = Resources.Load<TextAsset>("Info/EmbraceSdkInfo");
            EmbraceSdkInfo sdkInfo = JsonUtility.FromJson<EmbraceSdkInfo>(targetFile.text);

            Assert.AreEqual(package.version, sdkInfo.version);
        }
    }
}