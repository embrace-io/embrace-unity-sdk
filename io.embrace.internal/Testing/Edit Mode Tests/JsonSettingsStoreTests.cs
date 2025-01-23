using UnityEngine;
using NUnit.Framework;
using System.IO;
using System.Text.RegularExpressions;
using EmbraceSDK.EditorView;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class JsonSettingsStoreTests
    {
        private struct TestType
        {
            public int a;
            public int b;
            public bool c;
        }

        private string _testPath;
        private JsonSettingsStore _jsonSettingsStore;

        [SetUp]
        public void SetUp()
        {
            _testPath = Path.Combine(AssetDatabaseUtil.ProjectDirectory, "test.json");
            _jsonSettingsStore = new JsonSettingsStore(_testPath);
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_testPath);
        }

        [Test]
        public void ContainsKey_KeyExists_ReturnsTrue()
        {
            string key = "testKey";
            string value = "testValue";

            _jsonSettingsStore.SetValue(key, value);

            bool result = _jsonSettingsStore.ContainsKey(key);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsKey_KeyDoesNotExist_ReturnsFalse()
        {
            string key = "testKey";

            bool result = _jsonSettingsStore.ContainsKey(key);

            Assert.IsFalse(result);
        }

        [Test]
        public void DeleteKey_KeyExists_RemovesKey()
        {
            string key = "testKey";
            string value = "testValue";

            _jsonSettingsStore.SetValue(key, value);

            _jsonSettingsStore.DeleteKey(key);

            bool keyExists = _jsonSettingsStore.ContainsKey(key);
            Assert.IsFalse(keyExists);

            string retrievedValue = _jsonSettingsStore.GetValue<string>(key);
            Assert.IsNull(retrievedValue);
        }

        [Test]
        public void DeleteKey_KeyDoesNotExist_DoesNothing()
        {
            string key = "testKey";

            _jsonSettingsStore.DeleteKey(key);

            bool keyExists = _jsonSettingsStore.ContainsKey(key);
            Assert.IsFalse(keyExists);
        }

        [Test]
        public void SetValue_GetValue_SameValue()
        {
            int expectedValue = 42;

            _jsonSettingsStore.SetValue("testKey", expectedValue);
            int actualValue = _jsonSettingsStore.GetValue<int>("testKey");

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void SetValue_GetValue_SameValue_Struct()
        {
            TestType expectedValue = new TestType()
            {
                a = 42352,
                b = -521415,
                c = true,
            };

            _jsonSettingsStore.SetValue("testKey", expectedValue);
            TestType actualValue = _jsonSettingsStore.GetValue<TestType>("testKey");

            Assert.AreEqual(expectedValue, actualValue);
            Assert.AreEqual(expectedValue.a, actualValue.a);
            Assert.AreEqual(expectedValue.b, actualValue.b);
            Assert.AreEqual(expectedValue.c, actualValue.c);
        }

        [Test]
        public void SetValue_GetValue_SameValue_NestedJsonString()
        {
            Vector3 vec = new Vector3(334, 52324, 52524);
            string testValue = JsonUtility.ToJson(vec);

            _jsonSettingsStore.SetValue<string>("testKey", testValue);
            Vector3 value = JsonUtility.FromJson<Vector3>(_jsonSettingsStore.GetValue<string>("testKey"));

            Assert.AreEqual(vec, value);
        }

        [Test]
        public void GetValue_DefaultValue_ReturnsDefaultValue()
        {
            int defaultValue = -1;

            int actualValue = _jsonSettingsStore.GetValue<int>("nonExistentKey", defaultValue);

            Assert.AreEqual(defaultValue, actualValue);
        }

        [Test]
        public void GetValue_WrongType_LogsErrorAndReturnsDefaultValue()
        {
            int defaultValue = -1;

            _jsonSettingsStore.SetValue<string>("testKey", "wrongType");

            Regex logRegex = new Regex( "Embrace settings store encountered an error when getting value with key=testKey*");

            LogAssert.Expect(LogType.Error, logRegex);
            int actualValue = _jsonSettingsStore.GetValue<int>("testKey", defaultValue);
            Assert.AreEqual(defaultValue, actualValue);

            LogAssert.Expect(LogType.Error, logRegex);
            TestType testTypeValue = _jsonSettingsStore.GetValue<TestType>("testKey");
            Assert.AreEqual(new TestType(), testTypeValue);
        }

        [Test]
        public void Save_Load_SameValues()
        {
            int expectedValue = 123;
            _jsonSettingsStore.SetValue("testKey", expectedValue);

            _jsonSettingsStore.Save();
            _jsonSettingsStore = new JsonSettingsStore(_testPath);
            int actualValue = _jsonSettingsStore.GetValue<int>("testKey");

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void SetValue_Save_WritesFile()
        {
            _jsonSettingsStore.SetValue<string>("testKey", "valueA", false);
            Assert.IsFalse(File.Exists(_testPath));

            _jsonSettingsStore.SetValue<string>("testKey", "valueB", true);
            Assert.IsTrue(File.Exists(_testPath));
        }
    }
}