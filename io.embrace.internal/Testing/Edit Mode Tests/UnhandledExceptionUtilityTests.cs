using NUnit.Framework;

namespace EmbraceSDK.Tests
{
    public class UnhandledExceptionUtilityTests
    {
        [Test]
        public void SplitConcatenatedExceptionNameAndMessage_ReturnsExpectedValues(
            [ValueSource(nameof(concatenatedValueTuples))] (string input, string name, string message) tuple)
        {
            (string name, string message) = UnhandledExceptionUtility.SplitConcatenatedExceptionNameAndMessage(tuple.input);
            
            Assert.IsNotNull(name);
            Assert.AreEqual(tuple.name, name);
            
            Assert.IsNotNull(message);
            Assert.AreEqual(tuple.message, message);
        }
        
        private static readonly (string input, string name, string message)[] concatenatedValueTuples = {
            ("ExceptionName: Exception message", "ExceptionName", " Exception message"),
            ("invalid format", "", "invalid format"),
            (": no name", "", " no name"),
            ("NoMessage:", "NoMessage", ""),
            ("ExceptionName: Message with additional : separator", "ExceptionName", " Message with additional : separator"),
            ("", "", ""),
            (null, "", ""),
        };
    }
}