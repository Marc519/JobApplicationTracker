using Microsoft.Data.Sqlite;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JobApplicationTracker;
public class ApplicationRepository
{
	private string dataSource;

	public string pathToDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JobApplicationTracker");
	public string dbFileName = "applications.db";

	private static readonly int dbSignature = 1672453773;

	public ApplicationRepository()
	{
		System.IO.Directory.CreateDirectory(pathToDir);
		dataSource = "Data Source=" + Path.Combine(pathToDir, dbFileName);

		using var connection = new SqliteConnection(dataSource);
		connection.Open();

		using var command = new SqliteCommand("PRAGMA user_version = " + dbSignature, connection);
		command.ExecuteNonQuery();
	}

	//verifies that the existing database file was created by this application
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
				job_title TEXT,
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

	public bool AddApplication(string? jobTitle, string? companyName, string? jobLink, string? searchableText, string? extraNotes, Status jobStatus, DateOnly applicationDate)
	{
		string sql = @"INSERT INTO applications
					VALUES (@job_id, @job_title, @company_name, @job_link, @searchable_text, @extra_notes, @job_status, @date)";

        using var connection = new SqliteConnection(dataSource);
        connection.Open();

        using var command = new SqliteCommand(sql, connection);
        
		command.Parameters.Add("@job_id", SqliteType.Integer).Value = null;
		command.Parameters.Add("@job_title", SqliteType.Text).Value = jobTitle;
		command.Parameters.Add("@company_name", SqliteType.Text).Value = companyName;
		command.Parameters.Add("@job_link", SqliteType.Text).Value = jobLink;
		command.Parameters.Add("@searchable_text", SqliteType.Text).Value = searchableText;
		command.Parameters.Add("@extra_notes", SqliteType.Text).Value = extraNotes;
		command.Parameters.Add("@job_status", SqliteType.Integer).Value = jobStatus;
		command.Parameters.Add("@date", SqliteType.Text).Value = applicationDate.ToString("YYYY-MM-DD");

		int output = command.ExecuteNonQuery();

		return output == 1;
	}

	public bool RemoveApplication(long appID)
	{
		//TODO: removes application with id appID
		return true;
	}

	public List<JobApplication> GetApplication(List<long> appIDs)
	{
		var test = new List<JobApplication>();
		return test;
	}
	
	public List<int> SearchApplications(string? keyword, Status? jobStatus, DateTime? startDate, DateTime? endDate)
	{
		//TODO: queries the database for bodies of text (company name, searchable text, notes, job title) using keyword
		//and searches for matches in job status and start/end date. use all nulls to just return the entire id list
		return [0];
	}

}
