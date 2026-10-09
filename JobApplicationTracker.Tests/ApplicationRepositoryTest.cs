using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace JobApplicationTracker.Tests
{
    [TestClass]
    public sealed class ApplicationRepositoryCreationTest
    {
        private ApplicationRepository appRepo = null!;

        [TestInitialize]
        public void Setup()
        {
            appRepo = new ApplicationRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            SqliteConnection.ClearAllPools();
            System.IO.File.Delete(Path.Combine(appRepo.pathToDir, appRepo.dbFileName));
        }

        [TestMethod]
        public void TestCreateTable()
        {
            appRepo.CreateTable();

            using var connection = new SqliteConnection(@"Data Source=" + Path.Combine(appRepo.pathToDir, appRepo.dbFileName));
            connection.Open();

            int expectedUserVersion = 1672453773;

            using var command = new SqliteCommand("PRAGMA user_version", connection);
            long actualUserVersion = (long) command.ExecuteScalar()!;

            Assert.AreEqual(expectedUserVersion, actualUserVersion, "Expected and actual user version don't match");
        }

        [TestMethod]
        public void TestCreateTableWhenOneExists()
        {
            appRepo.CreateTable();
            appRepo.CreateTable();
        }

        [TestMethod]
        public void TestVerifyFile()
        {
            Assert.IsTrue(appRepo.VerifyFile(), "Database file could not be verified");
        }
    }
}
