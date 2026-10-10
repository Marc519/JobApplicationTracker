using Microsoft.Data.Sqlite;

namespace JobApplicationTracker;
public class ApplicationRepository
{
	private string dataSource;

	public string pathToDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JobApplicationTracker");
	public string dbFileName = "applications.db";

	public static readonly int dbSignature = 1672453773;

	public ApplicationRepository()
	{
		System.IO.Directory.CreateDirectory(pathToDir);
		dataSource = "Data Source=" + Path.Combine(pathToDir, dbFileName);
		
		// if the file doesnt exist, create one and sign it
		if (!System.IO.File.Exists(Path.Combine(pathToDir, dbFileName)))
		{
			using var connection = new SqliteConnection(dataSource);
            connection.Open();

            using var command = new SqliteCommand("PRAGMA user_version = " + dbSignature, connection);
            command.ExecuteNonQuery();
		}
	}

	//verifies that the database file was created by this application
	public bool VerifyFile()
	{
		using var connection = new SqliteConnection(dataSource);
		connection.Open();

		using var command = new SqliteCommand("PRAGMA user_version", connection);
		long userVersion = (long) command.ExecuteScalar()!;

		return userVersion == dbSignature;
	}

	//creates the applications table in the database if it doesn't already exist
	public void CreateTable()
	{
		string sql = @"CREATE TABLE IF NOT EXISTS applications(
				job_id INTEGER PRIMARY KEY,
				job_title TEXT NOT NULL,
				company_name TEXT,
				job_link TEXT,
				searchable_text TEXT,
				extra_notes TEXT,
				job_status INT8 NOT NULL,
				date DATE NOT NULL)";

        using var connection = new SqliteConnection(dataSource);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
	}

	// returns true if application added successfuly
	public bool AddApplication(string jobTitle, string? companyName, string? jobLink, string? searchableText, string? extraNotes, Status jobStatus, DateOnly applicationDate)
	{
		string sql = @"INSERT INTO applications
					VALUES (@job_id, @job_title, @company_name, @job_link, @searchable_text, @extra_notes, @job_status, @date)";

        using var connection = new SqliteConnection(dataSource);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);
        
		command.Parameters.Add("@job_id", SqliteType.Integer).Value = DBNull.Value;
		command.Parameters.Add("@job_title", SqliteType.Text).Value = jobTitle;
		command.Parameters.Add("@company_name", SqliteType.Text).Value = companyName == null ? DBNull.Value : companyName;
		command.Parameters.Add("@job_link", SqliteType.Text).Value = jobLink == null ? DBNull.Value : jobLink;
		command.Parameters.Add("@searchable_text", SqliteType.Text).Value = searchableText == null ? DBNull.Value : searchableText;
		command.Parameters.Add("@extra_notes", SqliteType.Text).Value = extraNotes == null ? DBNull.Value : extraNotes;
		command.Parameters.Add("@job_status", SqliteType.Integer).Value = jobStatus;
		command.Parameters.Add("@date", SqliteType.Text).Value = applicationDate.ToString("yyyy-MM-dd");

		int output = command.ExecuteNonQuery();

		return output == 1;
	}

	// returns true if job id was deleted
	public bool RemoveApplication(long jobId)
	{
		string sql = "DELETE FROM applications WHERE job_id = @job_id";

		using var connection = new SqliteConnection(dataSource);
		connection.Open();

		using var command = new SqliteCommand(sql, connection);

		command.Parameters.Add("@job_id", SqliteType.Integer).Value = jobId;

		int output = command.ExecuteNonQuery();

		return output == 1;
	}
	
	// takes in applicationIDs and returns them in a list
	// returns applications in ascending order by id
	public List<JobApplication?> GetApplication(List<long> appIDs)
	{
		string sqlTemplate = "SELECT * FROM applications WHERE job_id IN (";

		var idAsStrings = new List<string> ();

		for (int i = 0; i < appIDs.Count; i++)
		{
			idAsStrings.Add("@id" + i.ToString());
		}

		string sql = sqlTemplate + string.Join(", ", idAsStrings) + ")" + "ORDER BY job_id";

		using var connection = new SqliteConnection(dataSource);
		connection.Open();

		using var command = new SqliteCommand(sql, connection);

		int index = 0;
		foreach (long appID in appIDs)
		{
			command.Parameters.Add("@id" + index.ToString(), SqliteType.Integer).Value = appID;
			index++;
		}

		using SqliteDataReader reader = command.ExecuteReader();
		var retList = new List<JobApplication?> ();

		while (reader.Read())
		{
			long jobId = reader.GetInt64(0);
			string jobTitle = reader.GetString(1);
			string? companyName = reader.IsDBNull(2) ? null : reader.GetString(2);
			string? jobLink = reader.IsDBNull(3) ? null : reader.GetString(3);
			string? searchableText = reader.IsDBNull(4) ? null : reader.GetString(4);
			string? extraNotes = reader.IsDBNull(5) ? null : reader.GetString(5);
			Status jobStatus = (Status) reader.GetInt32(6);
			DateOnly applicationDate = DateOnly.FromDateTime(reader.GetDateTime(7));

			var newApp = new JobApplication(jobId, jobTitle, companyName, jobLink, searchableText, extraNotes, jobStatus, applicationDate);
			retList.Add(newApp);
		}

		return retList;
	}
	
	public List<long> SearchApplications(string? keyword, Status? jobStatus, DateTime? startDate, DateTime? endDate)
	{
		//TODO: queries the database for bodies of text (company name, searchable text, notes, job title) using keyword
		//and searches for matches in job status and start/end date. use all nulls to just return the entire id list
		return [0];
	}

}
