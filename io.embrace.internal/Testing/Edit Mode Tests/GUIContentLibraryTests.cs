using System;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEngine;

namespace EmbraceSDK.Tests
{
    [TestFixture]
    public class GUIContentLibraryTests
    {
        private StyleConfigs styleConfigs;
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // We should setup the styleConfigs here.
            // We're not using mocks because there are a number of internal dependencies within Unity APIs that have certain requirements and expectations.
            // Basically: mock style configs interface with the Unity APIs somewhere in the various StyleConfigs and this throw unexpected exceptions.
            styleConfigs = Resources.Load<StyleConfigs>("StyleConfigs/MainStyleConfigs");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Resources.UnloadAsset(styleConfigs);
        }

        [Test]
        public void GetContentTuple_ValidIdentifier_ReturnsTuple()
        {
            // Arrange
            var contentLibrary = new GUIContentLibrary();

            // Act
            var result = contentLibrary.GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId, styleConfigs);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.content);
            Assert.IsNotNull(result.style);
        }

        [Test]
        public void GetContentTuple_InvalidIdentifier_ThrowsArgumentException()
        {
            // Arrange
            var contentLibrary = new GUIContentLibrary();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                contentLibrary.GetContentTuple((GUIContentLibrary.GUIContentIdentifier)100, styleConfigs);
            });
        }

        [Test]
        public void EachIdentifier_CreatesValidMapping()
        {
            // Arrange
            var contentLibrary = new GUIContentLibrary();

            // Act --- This takes advantage of the Lazy Loading paradigm internal to the library
            foreach (GUIContentLibrary.GUIContentIdentifier id in Enum.GetValues(typeof(GUIContentLibrary.GUIContentIdentifier)))
            {
                // Assert
                var result = contentLibrary.GetContentTuple(id, styleConfigs);
                Assert.IsNotNull(result);
            }
        }

        [Test]
        public void LazyCreate_ValidIdentifier_CreatesMapping()
        {
            // Arrange
            var contentLibrary = new GUIContentLibrary();

            // Act --- This takes advantage of the Lazy Loading paradigm internal to the library
            contentLibrary.GetContentTuple(
                GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId,
                styleConfigs);

            // Assert
            var result = contentLibrary.GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId, styleConfigs);
            Assert.IsNotNull(result);
        }

        [Test]
        public void LazyCreate_InvalidIdentifier_ThrowsArgumentException()
        {
            // Arrange
            var contentLibrary = new GUIContentLibrary();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
            {
                // This takes advantage of the Lazy Loading paradigm internal to the library
                contentLibrary.GetContentTuple((GUIContentLibrary.GUIContentIdentifier)100, styleConfigs);
            });
        }
    }
}
