using System;
using EmbraceSDK.EditorView;
using NUnit.Framework;
using UnityEngine;

namespace EmbraceSDK.Tests
{
    [TestFixture]
    public class GUIContentLibraryTests
    {
        [Test]
        public void GetContentTuple_ValidIdentifier_ReturnsTuple()
        {
            // Arrange
            var contentLibrary = new GUIContentLibrary();

            // Act
            var result = contentLibrary.GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId);

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
                contentLibrary.GetContentTuple((GUIContentLibrary.GUIContentIdentifier)100);
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
                var result = contentLibrary.GetContentTuple(id);
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
                GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId);

            // Assert
            var result = contentLibrary.GetContentTuple(GUIContentLibrary.GUIContentIdentifier.GettingStartedLabelAppId);
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
                contentLibrary.GetContentTuple((GUIContentLibrary.GUIContentIdentifier)100);
            });
        }
    }
}
