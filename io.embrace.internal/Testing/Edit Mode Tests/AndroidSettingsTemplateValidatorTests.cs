#if UNITY_2022_2_OR_NEWER
using NUnit.Framework;
using System.IO;
using EmbraceSDK.Validators;

namespace EmbraceSDK.Tests
{
    [TestFixture]
    public class AndroidSettingsTemplateValidatorTests
    {
        private string validSettingsTemplate = @"repositories {
                                                mavenCentral()
                                            }";

        private string invalidSettingsTemplate = @"repositories {
                                                  jcenter()
                                              }";

        [Test]
        public void Validate_ValidTemplate_ReturnsTrue()
        {
            // Arrange
            var filePath = CreateTempFile(validSettingsTemplate);

            try
            {
                // Act
                var result = AndroidSettingsTemplateValidator.Validate(filePath);

                // Assert
                Assert.IsTrue(result);
            }
            finally
            {
                // Cleanup
                File.Delete(filePath);
            }
        }

        [Test]
        public void Validate_InvalidTemplate_ReturnsFalse()
        {
            // Arrange
            var filePath = CreateTempFile(invalidSettingsTemplate);

            try
            {
                // Act
                var result = AndroidSettingsTemplateValidator.Validate(filePath);

                // Assert
                Assert.IsFalse(result);
            }
            finally
            {
                // Cleanup
                File.Delete(filePath);
            }
        }

        [Test]
        public void Validate_TemplateWithNoRepositories_ReturnsFalse()
        {
            // Arrange
            var filePath = CreateTempFile("");

            try
            {
                // Act
                var result = AndroidSettingsTemplateValidator.Validate(filePath);

                // Assert
                Assert.IsFalse(result);
            }
            finally
            {
                // Cleanup
                File.Delete(filePath);
            }
        }

        private string CreateTempFile(string content)
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, content);
            return filePath;
        }
    }
}
#endif