using NUnit.Framework;
using System.IO;
using EmbraceSDK.Validators;

namespace EmbraceSDK.Tests
{
    [TestFixture]
    public class AndroidBaseProjectTemplateValidatorTests
    {
        private const string TestFilePath = "test.gradle";

        [OneTimeSetUp]
        public void SetUp()
        {
            // Create a temporary test file with content
            File.WriteAllText(TestFilePath, @"
                dependencies {
                    classpath 'some.library:library:1.0.0'
                    classpath 'io.embrace:embrace-swazzler:1.2.3'
                    classpath 'io.embrace:embrace-bug-shake-gradle-plugin:1.2.3-SNAPSHOT'
                }
                repositories {
                    mavenCentral()
                    jcenter()
                }");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            // Clean up the temporary test file
            File.Delete(TestFilePath);
        }

        #if !UNITY_2022_2_OR_NEWER
        [Test, Order(0)]
        public void Validate_ValidFile_ReturnsTrueForFoundImportAndAllRepositoriesValid()
        {
            // Arrange & Act
            var result = AndroidBaseProjectTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsTrue(result.foundImport);
            Assert.IsTrue(result.allRepositoriesValid);
        }
        #else
        [Test, Order(0)]
        public void Validate_ValidFile_ReturnsTrueForFoundImport()
        {
            // Arrange & Act
            var result = AndroidBaseProjectTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsTrue(result);
        }
        #endif

        [Test]
        public void Validate_MissingClasspath_ReturnsFalseForFoundImport()
        {
            // Arrange
            var content = @"
                dependencies {
                    implementation 'some.library:library:1.0.0'
                }
                repositories {
                    mavenCentral()
                    jcenter()
                }";
            File.WriteAllText(TestFilePath, content);

            // Act
            var result = AndroidBaseProjectTemplateValidator.Validate(TestFilePath);

            // Assert
            #if !UNITY_2022_2_OR_NEWER
            Assert.IsFalse(result.foundImport);
            Assert.IsTrue(result.allRepositoriesValid);
            #else
            Assert.IsFalse(result);
            #endif
        }

        #if !UNITY_2022_2_OR_NEWER
        [Test]
        public void Validate_InvalidRepository_ReturnsFalseForAllRepositoriesValid()
        {
            // Arrange
            var content = @"
                dependencies {
                    classpath 'io.embrace:embrace-swazzler:1.2.3'
                }
                repositories {
                    mavenLocal()
                    jcenter()
                }";
            File.WriteAllText(TestFilePath, content);

            // Act
            var result = AndroidBaseProjectTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsTrue(result.foundImport);
            Assert.IsFalse(result.allRepositoriesValid);
        }
        

        [Test]
        public void Validate_EmptyFile_ReturnsFalseForFoundImportAndAllRepositoriesValid()
        {
            // Arrange
            File.WriteAllText(TestFilePath, string.Empty);

            // Act
            var result = AndroidBaseProjectTemplateValidator.Validate(TestFilePath);

            // Assert
            Assert.IsFalse(result.foundImport);
            Assert.IsFalse(result.allRepositoriesValid);
        }
        #endif

        [Test]
        public void Validate_NonexistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonexistentFilePath = "nonexistent.gradle";

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => AndroidBaseProjectTemplateValidator.Validate(nonexistentFilePath));
        }
    }
}