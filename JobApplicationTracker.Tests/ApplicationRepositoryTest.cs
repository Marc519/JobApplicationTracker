using Microsoft.Data.Sqlite;

namespace JobApplicationTracker.Tests
{
    [TestClass]
    public sealed class ApplicationRepositoryOperationTest
    {
        private ApplicationRepository appRepo = null!;

        [TestInitialize]
        public void Startup()
        {
            appRepo = new ApplicationRepository();
            Assert.IsTrue(appRepo.VerifyFile());
            appRepo.CreateTable();
        }

        [TestCleanup]
        public void Cleanup()
        {
            SqliteConnection.ClearAllPools();
            string pathToDB = Path.Combine(appRepo.pathToDir, appRepo.dbFileName);
            System.IO.File.Delete(pathToDB);
            Console.WriteLine("Deleted " + pathToDB);
        }

        [TestMethod]
        public void TestAddingAndGettingApplication()
        {
            bool retVal = appRepo.AddApplication("product tester", "NuCaloric", "www.Google.com",
                "double drinkable cheeseburger", "add quadruple drinkable cheeseburger",
                Status.Applied, new DateOnly(2026, 6, 12));

            JobApplication expected = new JobApplication(1, "product tester", "NuCaloric", "www.Google.com",
                "double drinkable cheeseburger", "add quadruple drinkable cheeseburger",
                Status.Applied, new DateOnly(2026, 6, 12));

            Assert.IsTrue(retVal);

            List<JobApplication?> actual = appRepo.GetApplication([1]);

            Assert.HasCount(1, actual);
            Assert.IsTrue(JobApplication.AreEqual(actual[0], expected));
        }

        [TestMethod]
        public void TestRemovingApplication()
        {
            bool retVal = appRepo.AddApplication("product tester", "NuCaloric", "www.Google.com",
                "double drinkable cheeseburger", "add quadruple drinkable cheeseburger",
                Status.Applied, new DateOnly(2026, 6, 12));

            retVal = appRepo.RemoveApplication(1);

            List<JobApplication?> actual = appRepo.GetApplication([1]);
            Assert.HasCount(0, actual);
        }

        [TestMethod]
        public void TestRetrievingNonExistantApplication()
        {
            List<JobApplication?> actual = appRepo.GetApplication([1]);
            Assert.HasCount(0, actual);
        }
    }

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
            string pathToDB = Path.Combine(appRepo.pathToDir, appRepo.dbFileName);
            System.IO.File.Delete(pathToDB);
            Console.WriteLine("Deleted " + pathToDB);
        }

        private bool CheckTableStructure(SqliteConnection connection)
        {
            using var command2 = new SqliteCommand("PRAGMA table_info('applications')", connection);
            using SqliteDataReader reader = command2.ExecuteReader();

            var expectedNames = new List<string> { "job_id", "job_title", "company_name", "job_link",
                "searchable_text", "extra_notes", "job_status", "date" };
            var expectedTypes = new List<string> { "INTEGER", "TEXT", "TEXT", "TEXT", "TEXT", "TEXT", "INT8", "DATE"};
            var expectedNotNull = new List<int> { 0, 1, 0, 0, 0, 0, 1, 1 };

            int index = 0;
            while (reader.Read() && index < expectedNames.Count)
            {
                string columnName = reader.GetString(1);
                string columnType = reader.GetString(2);
                int columnNotNull = reader.GetInt32(3);
                int columnPrimaryKey = reader.GetInt32(5);

                if (!((index == 0 ^ columnPrimaryKey == 0)
                    && string.Equals(columnName, expectedNames[index])
                    && string.Equals(columnType, expectedTypes[index])
                    && columnNotNull == expectedNotNull[index]))
                {
                    Console.WriteLine("here1");
                    return false;
                }
                index++;
            }

            if (reader.Read() || index < expectedNames.Count)
            {
                return false;
            }
            return true;
 
        }

        [TestMethod]
        public void TestCreateTable()
        {
            appRepo.CreateTable();

            using var connection = new SqliteConnection(@"Data Source=" + Path.Combine(appRepo.pathToDir, appRepo.dbFileName));
            connection.Open();

            int expectedUserVersion = ApplicationRepository.dbSignature;

            using var command = new SqliteCommand("PRAGMA user_version", connection);
            long actualUserVersion = (long) command.ExecuteScalar()!;

            Assert.AreEqual(expectedUserVersion, actualUserVersion, "Expected and actual user version don't match");

            Assert.IsTrue(CheckTableStructure(connection), "Table structure is wrong");
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
