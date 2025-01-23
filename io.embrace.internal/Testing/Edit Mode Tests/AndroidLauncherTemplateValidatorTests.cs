using NUnit.Framework;
using System.IO;
using EmbraceSDK.Validators;

namespace EmbraceSDK.Tests
{
    [TestFixture]
    public class AndroidLauncherTemplateValidatorTests
    {
        private const string TestFilePath = "test.gradle";

        [OneTimeSetUp]
        public void SetUp()
        {
            // Create a temporary test file with content
            File.WriteAllText(TestFilePath, "// Some comments\napply plugin: 'embrace-swazzler'\n");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            // Clean up the temporary test file
            File.Delete(TestFilePath);
        }

        [Test, Order(0)] // Order required to ensure that the test executes before the contents of the file change.
        public void Validate_ValidFile_ReturnsTrue()
        {
            // Arrange & Act
            var result = AndroidLauncherTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Validate_MissingApplyPluginLine_ReturnsFalse()
        {
            // Arrange
            var content = "// Some comments\notherPlugin.apply()\n";
            File.WriteAllText(TestFilePath, content);

            // Act
            var result = AndroidLauncherTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_SkipLine_ReturnsTrue()
        {
            // Arrange
            var content = "// Some comments\n   \napply plugin: 'embrace-swazzler'\n";
            File.WriteAllText(TestFilePath, content);

            // Act
            var result = AndroidLauncherTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Validate_EmptyFile_ReturnsFalse()
        {
            // Arrange
            File.WriteAllText(TestFilePath, string.Empty);

            // Act
            var result = AndroidLauncherTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Validate_NonexistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonexistentFilePath = "nonexistent.gradle";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => AndroidLauncherTemplateValidator.Validate(nonexistentFilePath));
        }
    }
}

