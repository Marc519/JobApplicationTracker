using System;
using Microsoft.Data.Sqlite;

namespace JobApplicationTracker;
public class ApplicationRepository
{
	private string databaseFile;

	public ApplicationRepository(string pathToDatabase)
	{
		databaseFile = "Data Source=" + pathToDatabase;
	}

	public bool CreateTable()
	{
		//TODO: creates the application table if database is newly created
		return true;
	}

	public bool IsValid()
	{
		//TODO: checks if table structure is valid
		return true;
	}

	public bool AddApplication(JobApplication app)
	{
		//TODO: adds application to table
		return true;
	}

	public bool RemoveApplication(long appID)
	{
		//TODO: removes application with id appID
		return true;
	}

	public List<JobApplication> GetApplication(List<long> appID)
	{
		//TODO: returns job applications with the IDs in the list
		var test = new JobApplication(1, null, null, null, null, null, null, null, Status.Applied, new DateTime(2026, 7, 11));
		return new List<JobApplication> { test };
	}

	
	public List<long> SearchApplications(string? keyword, Status? jobStatus, DateTime? startDate, DateTime? endDate)
	{
		//TODO: queries the database for bodies of text (company name, searchable text, notes, job title) using keyword
		//and searches for matches in job status and start/end date. use all nulls to just return the entire id list
		return new List<long> { 0 };
	}

}
