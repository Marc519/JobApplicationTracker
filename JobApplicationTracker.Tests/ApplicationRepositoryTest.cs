using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using System.Net.Http.Headers;

namespace JobApplicationTracker.Tests
{
    [TestClass]
    public sealed class ApplicationRepositoryModifyTest
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

            List<JobApplication> actual = appRepo.GetApplication([1]);

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

            List<JobApplication> actual = appRepo.GetApplication([1]);
            Assert.HasCount(0, actual);
        }
    }

    [TestClass]
    public sealed class ApplicationRepositoryQueryTest()
    {

        private static ApplicationRepository appRepo = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            appRepo = new ApplicationRepository();
            Assert.IsTrue(appRepo.VerifyFile());
            appRepo.CreateTable();

            bool retVal = appRepo.AddApplication("product tester", "NuCaloric", "www.Google.com",
                "Double drinkable cheeseburger", "Add quadruple drinkable cheeseburger",
                Status.Applied, new DateOnly(2882, 9, 2));
            retVal = appRepo.AddApplication("mercenary", "Traxus", "www.Google.com",
                "Vulcan is cool", "They operate beyond scale",
                Status.Offer, new DateOnly(2891, 10, 29));
            retVal = appRepo.AddApplication("software engineer", "CyberAcme", "www.Google.com",
                "Requires rampant AI experience", "ONI is irish",
                Status.Offer, new DateOnly(2893, 6, 12));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            SqliteConnection.ClearAllPools();
            string pathToDB = Path.Combine(appRepo.pathToDir, appRepo.dbFileName);
            System.IO.File.Delete(pathToDB);
            Console.WriteLine("Deleted " + pathToDB);
        }

        [TestMethod]
        public void TestAllNulls()
        {
            List<long> actual = appRepo.SearchApplications(null, null, null, null);
            Assert.AreSequenceEqual([1, 2, 3], actual, "Returned search results did not match expected");
        }
        
        [TestMethod]
        public void TestRetrievingNonExistantApplication()
        {
            List<JobApplication> actual = appRepo.GetApplication([4]);
            Assert.HasCount(0, actual);
        }

        [TestMethod]
        [DataRow(new long[] {1}, "Nucaloric")]
        [DataRow(new long[] {1}, "nu")]
        [DataRow(new long[] {}, "Google")]
        [DataRow(new long[] {2}, "mercenary")]
        [DataRow(new long[] {2, 3}, "is")]
        public void TestSearchKeywords(long[] expected, string keyword)
        {
            List<long> actual = appRepo.SearchApplications(keyword, null, null, null);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {1}, Status.Applied)]
        [DataRow(new long[] {}, Status.Rejected)]
        [DataRow(new long[] {2, 3}, Status.Offer)]
        public void TestSearchStatus(long[] expected, Status jobStatus)
        {
            List<long> actual = appRepo.SearchApplications(null, jobStatus, null, null);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {1, 2, 3}, 2026, 10, 8)]
        [DataRow(new long[] {}, 3000, 6, 7)]
        [DataRow(new long[] {2, 3}, 2891, 10, 29)]
        public void TestSearchStartDate(long[] expected, int year, int month, int day)
        {
            var startDate = new DateOnly(year, month, day);
            List<long> actual = appRepo.SearchApplications(null, null, startDate, null);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {}, 2026, 10, 8)]
        [DataRow(new long[] {1, 2, 3}, 3000, 6, 7)]
        [DataRow(new long[] {1, 2}, 2891, 10, 29)]
        public void TestSearchEndDate(long[] expected, int year, int month, int day)
        {
            var endDate = new DateOnly(year, month, day);
            List<long> actual = appRepo.SearchApplications(null, null, null, endDate);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {1, 2, 3}, 2882, 9, 2, 2893, 6, 12)]
        [DataRow(new long[] {}, 2893, 6, 12, 2882, 9, 2)]
        [DataRow(new long[] {1, 2}, 2882, 9, 2, 2893, 6, 11)]
        [DataRow(new long[] {2, 3}, 2891, 9, 2, 2893, 6, 12)]
        [DataRow(new long[] {2}, 2882, 9, 3, 2893, 6, 11)]
        public void TestSearchStartAndEndDate(long[] expected, int startYear, int startMonth, int startDay,
            int endYear, int endMonth, int endDay)
        {
            var startDate = new DateOnly(startYear, startMonth, startDay);
            var endDate = new DateOnly(endYear, endMonth, endDay);
            List<long> actual = appRepo.SearchApplications(null, null, startDate, endDate);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {1}, "Nucaloric", Status.Applied)]
        [DataRow(new long[] {}, "Nucaloric", Status.Offer)]
        [DataRow(new long[] {2, 3}, "is", Status.Offer)]
        public void TestSearchKeywordAndStatus(long[] expected, string keyword, Status jobStatus)
        {
            List<long> actual = appRepo.SearchApplications(keyword, jobStatus, null, null);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {2, 3}, "o", 2891, 10, 29)]
        [DataRow(new long[] {}, "scrimblo", 2026, 3, 12)]
        [DataRow(new long[] {2}, "le", 2891, 10, 29)]
        public void TestSearchKeywordAndStartDate(long[] expected, string keyword, int year, int month, int day)
        {
            DateOnly startDate = new DateOnly(year, month, day);
            List<long> actual = appRepo.SearchApplications(keyword, null, startDate, null);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {1, 2}, "le", 2891, 10, 29)]
        [DataRow(new long[] {}, "scrimblo", 3000, 3, 12)]
        [DataRow(new long[] {2}, "is", 2891, 10, 29)]
        public void TestSearchKeywordAndEndDate(long[] expected, string keyword, int year, int month, int day)
        {
            DateOnly endDate = new DateOnly(year, month, day);
            List<long> actual = appRepo.SearchApplications(keyword, null, null, endDate);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {}, Status.Applied, 2891, 10, 29)]
        [DataRow(new long[] {3}, Status.Offer, 2893, 6, 12)]
        [DataRow(new long[] {1}, Status.Applied, 2026, 10, 29)]
        public void TestSearchStatusAndStartDate(long[] expected, Status status, int year, int month, int day)
        {
            DateOnly startDate = new DateOnly(year, month, day);
            List<long> actual = appRepo.SearchApplications(null, status, startDate, null);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
        }

        [TestMethod]
        [DataRow(new long[] {}, Status.Offer, 2890, 10, 29)]
        [DataRow(new long[] {2}, Status.Offer, 2891, 10, 29)]
        [DataRow(new long[] {2, 3}, Status.Offer, 3000, 10, 29)]
        public void TestSearchStatusAndEndDate(long[] expected, Status status, int year, int month, int day)
        {
            DateOnly endDate = new DateOnly(year, month, day);
            List<long> actual = appRepo.SearchApplications(null, status, null, endDate);
            Assert.AreSequenceEqual(expected, actual, "Returned search results did not match expected");
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
