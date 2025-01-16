using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Embrace.Internal.SmokeTests
{
    /// <summary>
    /// Responsible for invoking smoke tests based on the process start arguments
    /// </summary>
    [ExcludeFromCoverage]
    public class SmokeTestDriver : MonoBehaviour
    {
        private delegate IEnumerator CoroutineSmokeTestDelegate();

        private const string UNITY_SMOKE_TEST_ARG = "--unitySmokeTest=";

        private void Start()
        {
            Dictionary<string, MethodInfo> tests = GetAvailableTests();
            string commandedTest = GetCommandedTest();

            if (string.IsNullOrWhiteSpace(commandedTest))
            {
                Debug.LogWarning("No Unity smoke test found in process start arguments.");
                return;
            }

            if (!tests.TryGetValue(commandedTest, out MethodInfo testMethod))
            {
                Debug.LogError($"No test found with ID {commandedTest}");
                return;
            }

            // Avoid potential unintended test loop caused by instantiating a new driver for tests
            // declared within this type
            if (typeof(SmokeTestDriver).IsAssignableFrom(testMethod.DeclaringType))
            {
                Debug.LogError("Smoke tests defined inside the SmokeTestDriver, or any subclass of SmokeTestDriver, are not supported.");
                return;
            }

            Debug.Log($"Running smoke test: {commandedTest}");

            RunSmokeTest(testMethod);
        }

        private void RunSmokeTest(MethodInfo testMethod)
        {
            if (testMethod.ReturnType == typeof(IEnumerator))
            {
                CoroutineSmokeTestDelegate testDelegate = null;
                if (testMethod.IsStatic)
                {
                    testDelegate = (CoroutineSmokeTestDelegate)Delegate.CreateDelegate(typeof(CoroutineSmokeTestDelegate), testMethod);
                }
                else
                {
                    object instance = typeof(MonoBehaviour).IsAssignableFrom(testMethod.DeclaringType)
                        ? gameObject.AddComponent(testMethod.DeclaringType)
                        : Activator.CreateInstance(testMethod.DeclaringType);

                    testDelegate =
                        (CoroutineSmokeTestDelegate)Delegate.CreateDelegate(typeof(CoroutineSmokeTestDelegate),
                            instance, testMethod);
                }

                StartCoroutine(testDelegate());
            }
            else
            {
                object instance = null;
                if (!testMethod.IsStatic)
                {
                    instance = typeof(MonoBehaviour).IsAssignableFrom(testMethod.DeclaringType)
                        ? gameObject.AddComponent(testMethod.DeclaringType)
                        : Activator.CreateInstance(testMethod.DeclaringType);
                }

                testMethod.Invoke(instance, null);
            }
        }

        private static Dictionary<string, MethodInfo> GetAvailableTests()
        {
            Dictionary<string, MethodInfo> tests = new Dictionary<string, MethodInfo>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach(MethodInfo method in type.GetMethods())
                {
                    SmokeTestAttribute attribute = method.GetCustomAttribute<SmokeTestAttribute>();
                    if (attribute == null) continue;

                    string id = attribute.GetTestId(method);

                    if (tests.ContainsKey(id))
                    {
                        Debug.LogError($"Multiple tests found with ID {id}.");
                    }

                    tests[id] = method;
                }
            }

            return tests;
        }

        private static string GetCommandedTest()
        {
            string[] args = GetStartArguments();

            if (args != null)
            {
                foreach (var arg in args)
                {
                    if (!arg.StartsWith(UNITY_SMOKE_TEST_ARG)) continue;
                    return arg.Substring(UNITY_SMOKE_TEST_ARG.Length);
                }
            }

            return null;
        }

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string ios_getStartArguments();
        #endif

        private static string[] GetStartArguments()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            return ios_getStartArguments().Split(' ');
            #else
            return Array.Empty<string>();
            #endif
        }
    }
}