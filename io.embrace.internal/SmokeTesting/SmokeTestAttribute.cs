using System;
using System.Reflection;
using UnityEngine.TestTools;

/*
 * Notably, this has not been used or expanded upon in awhile because of the state of Copeland's test harness.
 * We will likely be deprecating this in the future.
 */
namespace Embrace.Internal.SmokeTests
{
    /// <summary>
    /// Add this attribute to a method to declare it as a smoke test. If no test ID parameter is provided,
    /// the name of the method will be used as the ID.
    /// </summary>
    [ExcludeFromCoverage]
    [AttributeUsage(AttributeTargets.Method)]
    public class SmokeTestAttribute : System.Attribute
    {
        private string testId;

        /// <summary>
        /// Declare a method as a smoke test. The method name will be used as the ID to invoke the test.
        /// </summary>
        public SmokeTestAttribute()
        {
            testId = null;
        }

        /// <summary>
        /// Declare a method as a smoke test.
        /// </summary>
        /// <param name="testId">A custom ID used to invoke the test.</param>
        public SmokeTestAttribute(string testId)
        {
            this.testId = testId;
        }

        /// <summary>
        /// Gets the testId for a given smoke test method
        /// </summary>
        public string GetTestId(MethodInfo member)
        {
            return testId ?? member.Name;
        }
    }
}