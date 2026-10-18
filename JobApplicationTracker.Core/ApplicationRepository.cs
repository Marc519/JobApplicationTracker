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
				company_name TEXT NOT NULL,
				job_link TEXT NOT NULL,
				searchable_text TEXT NOT NULL,
				extra_notes TEXT NOT NULL,
				job_status INT8 NOT NULL,
				date DATE NOT NULL)";

        using var connection = new SqliteConnection(dataSource);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
	}

	public bool UpdateApplication(long jobId, string? jobTitle, string? companyName, string? jobLink,
		string? searchableText, string? extraNotes, Status? jobStatus, DateOnly? applicationDate)
	{
		List<string> buildQuery = [];

		if (jobTitle != null) buildQuery.Add("job_title = @job_title");
		if (companyName != null) buildQuery.Add("company_name = @company_name");
		if (jobLink != null) buildQuery.Add("job_link = @job_link");
		if (searchableText != null) buildQuery.Add("searchable_text = @searchable_text");
		if (extraNotes != null) buildQuery.Add("extra_notes = @extra_notes");
		if (jobStatus != null) buildQuery.Add("job_status = @job_status");
		if (applicationDate != null) buildQuery.Add("date = @date");

		string sql = "UPDATE applications SET " + string.Join(", ", buildQuery) + " WHERE job_id = @job_id";

        using var connection = new SqliteConnection(dataSource);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);

		command.Parameters.Add("@job_id", SqliteType.Integer).Value = jobId;
		if (jobTitle!= null) command.Parameters.Add("@job_title", SqliteType.Text).Value = jobTitle;
		if (companyName != null) command.Parameters.Add("@company_name", SqliteType.Text).Value = companyName;
		if (jobLink != null) command.Parameters.Add("@job_link", SqliteType.Text).Value = jobLink;
		if (searchableText != null) command.Parameters.Add("@searchable_text", SqliteType.Text).Value = searchableText;
		if (extraNotes != null) command.Parameters.Add("@extra_notes", SqliteType.Text).Value = extraNotes;
		if (jobStatus != null) command.Parameters.Add("@job_status", SqliteType.Integer).Value = jobStatus;
		if (applicationDate != null) command.Parameters.Add("@date", SqliteType.Text).Value = ((DateOnly)applicationDate).ToString("yyyy-MM-dd");

		int output = command.ExecuteNonQuery();

		return output == 1;
	}


	// returns true if application added successfuly
	public bool AddApplication(string jobTitle, string companyName, string jobLink, string searchableText,
		string extraNotes, Status jobStatus, DateOnly applicationDate)
	{
		string sql = @"INSERT INTO applications
					VALUES (@job_id, @job_title, @company_name, @job_link, @searchable_text, @extra_notes, @job_status, @date)";

        using var connection = new SqliteConnection(dataSource);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);
        
		command.Parameters.Add("@job_id", SqliteType.Integer).Value = DBNull.Value;
		command.Parameters.Add("@job_title", SqliteType.Text).Value = jobTitle;
		command.Parameters.Add("@company_name", SqliteType.Text).Value = companyName;
		command.Parameters.Add("@job_link", SqliteType.Text).Value = jobLink;
		command.Parameters.Add("@searchable_text", SqliteType.Text).Value = searchableText;
		command.Parameters.Add("@extra_notes", SqliteType.Text).Value = extraNotes;
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
	public List<JobApplication> GetApplication(List<long> appIDs, bool asc = true)
	{
		string sqlTemplate = "SELECT job_id, job_title, company_name, job_link, extra_notes, job_status, date FROM applications WHERE job_id IN (";

		var idAsStrings = new List<string> ();

		for (int i = 0; i < appIDs.Count; i++)
		{
			idAsStrings.Add("@id" + i.ToString());
		}

		string sql = sqlTemplate + string.Join(", ", idAsStrings) + ")" + " ORDER BY date" + (asc ? " ASC " : " DESC ");

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
		var retList = new List<JobApplication> ();

		while (reader.Read())
		{
			long jobId = reader.GetInt64(0);
			string jobTitle = reader.GetString(1);
			string companyName = reader.GetString(2);
			string jobLink = reader.GetString(3);
			string extraNotes = reader.GetString(4);
			Status jobStatus = (Status) reader.GetInt32(5);
			DateOnly applicationDate = DateOnly.FromDateTime(reader.GetDateTime(6));

			var newApp = new JobApplication(jobId, jobTitle, companyName, jobLink, "", extraNotes, jobStatus, applicationDate);
			retList.Add(newApp);
		}

		return retList;
	}
	
	public List<long> SearchApplications(string? keyword, Status? jobStatus, DateOnly? startDate, DateOnly? endDate, bool asc = true)
	{
		string sql = @"SELECT job_id
					   FROM applications";

		bool allNull = true;

		string extra = "";

		if (keyword != null)
		{
			extra = extra + @"(job_title LIKE @keyword
							OR company_name LIKE @keyword
							OR searchable_text LIKE @keyword
							OR extra_notes LIKE @keyword) AND ";
			allNull = false;
		}
		if (jobStatus.HasValue)
		{
			extra = extra + "job_status = @job_status AND ";
			allNull = false;
		}
		if (startDate.HasValue) 
		{
			extra = extra + "date >= @start_date AND ";
			allNull = false;
		}
		if (endDate.HasValue) 
		{
			extra = extra + "date <= @end_date AND ";
			allNull = false;
		}

		if (!allNull)
		{
			sql = sql + " WHERE " + extra;
            sql = sql.Substring(0, sql.Length - 4); 
		}
        sql = sql + " ORDER BY date" + (asc? " ASC " : " DESC ");

		using var connection = new SqliteConnection(dataSource);
		connection.Open();

		using var command = new SqliteCommand(sql, connection);

		command.Parameters.Add("@keyword", SqliteType.Text).Value = "%" + keyword + "%";
		if (jobStatus.HasValue) command.Parameters.Add("@job_status", SqliteType.Integer).Value = jobStatus.Value;
		if (startDate.HasValue) command.Parameters.Add("@start_date", SqliteType.Text).Value = startDate.Value.ToString("yyyy-MM-dd");
		if (endDate.HasValue) command.Parameters.Add("@end_date", SqliteType.Text).Value = endDate.Value.ToString("yyyy-MM-dd");

		using var reader = command.ExecuteReader();
		var returnIDs = new List<long>();

		while (reader.Read())
		{
			returnIDs.Add(reader.GetInt64(0));
		}
		return returnIDs;
	}

}
