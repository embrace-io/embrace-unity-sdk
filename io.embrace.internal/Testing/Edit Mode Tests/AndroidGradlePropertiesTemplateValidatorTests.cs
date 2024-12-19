using NUnit.Framework;
using System.IO;
using EmbraceSDK.Validators;

namespace EmbraceSDK.Tests
{
    [TestFixture]
    public class AndroidGradlePropertiesTemplateValidatorTests
    {
        private const string TestFilePath = "test.gradle.properties";

        [OneTimeSetUp]
        public void SetUp()
        {
            // Create a temporary test file with content
            File.WriteAllText(TestFilePath, "android.useAndroidX=true\nandroid.enableJetifier=true");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            // Clean up the temporary test file
            File.Delete(TestFilePath);
        }

        [Test, Order(0)] // Order required to ensure that the test executes before the contents of the file change.
        public void Validate_ValidFile_ReturnsTrueForAndroidXAndJetifier()
        {
            // Arrange & Act
            var result = AndroidGradlePropertiesTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsTrue(result.foundAndroidX);
            Assert.IsTrue(result.foundJetifier);
        }

        [Test]
        public void Validate_MissingAndroidX_ReturnsFalseForAndroidX()
        {
            // Arrange
            var content = "android.enableJetifier=true";
            File.WriteAllText(TestFilePath, content);

            // Act
            var result = AndroidGradlePropertiesTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsFalse(result.foundAndroidX);
            Assert.IsTrue(result.foundJetifier);
        }

        [Test]
        public void Validate_MissingJetifier_ReturnsFalseForJetifier()
        {
            // Arrange
            var content = "android.useAndroidX=true";
            File.WriteAllText(TestFilePath, content);

            // Act
            var result = AndroidGradlePropertiesTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsTrue(result.foundAndroidX);
            Assert.IsFalse(result.foundJetifier);
        }

        [Test]
        public void Validate_EmptyFile_ReturnsFalseForAndroidXAndJetifier()
        {
            // Arrange
            File.WriteAllText(TestFilePath, string.Empty);

            // Act
            var result = AndroidGradlePropertiesTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsFalse(result.foundAndroidX);
            Assert.IsFalse(result.foundJetifier);
        }

        [Test]
        public void Validate_NonexistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonexistentFilePath = "nonexistent.gradle.properties";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => AndroidGradlePropertiesTemplateValidator.Validate(nonexistentFilePath));
        }
    }
}

