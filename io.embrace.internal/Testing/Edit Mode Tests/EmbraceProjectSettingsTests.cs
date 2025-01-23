using NUnit.Framework;
using EmbraceSDK.EditorView;
using NSubstitute;

namespace EmbraceSDK.Tests
{
    public class EmbraceProjectSettingsTests
    {
        [TearDown]
        public void TearDown()
        {
            EmbraceProjectSettings.MockProjectSettings = null;
            EmbraceProjectSettings.MockUserSettings = null;
        }

        [Test]
        public void Project_ReturnsMockProjectSettings_WhenMockSet()
        {
            // Arrange
            var expectedSettings = Substitute.For<ISettingsStore>();
            EmbraceProjectSettings.MockProjectSettings = expectedSettings;

            // Act
            var result = EmbraceProjectSettings.Project;

            // Assert
            Assert.AreEqual(expectedSettings, result);
        }

        [Test]
        public void User_ReturnsMockUserSettings_WhenMockSet()
        {
            // Arrange
            var expectedSettings = Substitute.For<ISettingsStore>();
            EmbraceProjectSettings.MockUserSettings = expectedSettings;

            // Act
            var result = EmbraceProjectSettings.User;

            // Assert
            Assert.AreEqual(expectedSettings, result);
        }

        [Test]
        public void Project_ReturnsNonNullSettings_WhenMockNotSet()
        {
            EmbraceProjectSettings.MockProjectSettings = null;

            Assert.IsNotNull(EmbraceProjectSettings.Project);
        }

        [Test]
        public void User_ReturnsNonNullSettings_WhenMockNotSet()
        {
            EmbraceProjectSettings.MockUserSettings = null;

            Assert.IsNotNull(EmbraceProjectSettings.User);
        }
    }
}