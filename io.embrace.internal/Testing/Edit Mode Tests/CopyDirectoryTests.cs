using System.IO;
using EmbraceSDK.EditorView;
using NUnit.Framework;

namespace EmbraceSDK.Tests
{
    public class CopyDirectoryTests
    {
        [SetUp]
        public void SetUp()
        {
            string testSourceDirPath = GetTestSourceDir();
            Directory.CreateDirectory(testSourceDirPath);
            File.CreateText(Path.Combine(testSourceDirPath, "rootFile.txt")).Close();
            File.CreateText(Path.Combine(testSourceDirPath, "rootFile.txt.meta")).Close();

            string subDir = Path.Combine(testSourceDirPath, "subDirectory/");
            Directory.CreateDirectory(subDir);
            File.CreateText(Path.Combine(subDir, "subDirectoryFile.txt")).Close();
            File.CreateText(Path.Combine(subDir, "subDirectoryFile.txt.meta")).Close();
        }

        [TearDown]
        public void TearDown()
        {
            string testSourceDirPath = GetTestSourceDir();
            Directory.Delete(testSourceDirPath, true);

            string testDestinationDirPath = GetTestDestinationDir();
            if (Directory.Exists(testDestinationDirPath))
            {
                Directory.Delete(testDestinationDirPath, true);
            }
        }

        [Test]
        public void CopyDirectoryThrowsForInvalidDirectory()
        {
            Assert.Throws<DirectoryNotFoundException>(() =>
            {
                string fakePath = Path.Combine(AssetDatabaseUtil.ProjectDirectory, "/Temp/fakeDirectory/fakeSubDirectory");
                AssetDatabaseUtil.CopyDirectory("/Temp/fakeDirectory/fakeSubDirectory", "/FakeDestination/", true,
                    true);
            });
        }

        [Test]
        public void CopyDirectoryRecursivelyCopiesAllFilesInAllSubDirs()
        {
            string sourcePath = GetTestSourceDir();
            string destinationPath = GetTestDestinationDir();

            AssetDatabaseUtil.CopyDirectory(sourcePath, destinationPath, true, false);

            DirectoryInfo source = new DirectoryInfo(sourcePath);
            DirectoryInfo destination = new DirectoryInfo(destinationPath);
            Assert.IsTrue(destination.Exists);
            Assert.IsTrue(CheckAllSubDirectoriesMatch(source, sourcePath, destinationPath, false));
        }

        [Test]
        public void CopyDirectoryRemovesMetaFilesFromSourceDirectory()
        {
            string sourcePath = GetTestSourceDir();
            string destinationPath = GetTestDestinationDir();

            AssetDatabaseUtil.CopyDirectory(sourcePath, destinationPath, true, true);

            DirectoryInfo source = new DirectoryInfo(sourcePath);
            DirectoryInfo destination = new DirectoryInfo(destinationPath);
            Assert.IsTrue(destination.Exists);
            Assert.IsTrue(CheckAllSubDirectoriesMatch(source, sourcePath, destinationPath, true));

            Assert.IsTrue(DirectoryContainsMetaFiles(destination));
            Assert.IsFalse(DirectoryContainsMetaFiles(source));
        }

        private static bool CheckAllSubDirectoriesMatch(DirectoryInfo sourceDirectory, string sourcePath, string destinationPath, bool ignoreMetaFiles)
        {
            foreach (FileInfo file in sourceDirectory.EnumerateFiles())
            {
                if (ignoreMetaFiles && file.Extension.EndsWith("meta"))
                {
                    continue;
                }
                string matchedFilePath = file.FullName.Replace(sourcePath, destinationPath);
                if (!File.Exists(matchedFilePath))
                {
                    return false;
                }
            }
            foreach (DirectoryInfo subDirectory in sourceDirectory.EnumerateDirectories())
            {
                string matchedSubDirPath = subDirectory.FullName.Replace(sourcePath, destinationPath);
                if (!Directory.Exists(matchedSubDirPath) || !CheckAllSubDirectoriesMatch(subDirectory, sourcePath, destinationPath, ignoreMetaFiles))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DirectoryContainsMetaFiles(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                if (file.Extension.EndsWith("meta"))
                {
                    return true;
                }
            }

            foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
            {
                if (DirectoryContainsMetaFiles(subDirectory))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetTestSourceDir()
        {
            return Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Temp/AssetDatabaseUtilCopyDirectoryTest");
        }

        private static string GetTestDestinationDir()
        {
            return Path.Combine(AssetDatabaseUtil.ProjectDirectory, "Temp/AssetDatabaseUtilCopyDestionation");
        }
    }
}