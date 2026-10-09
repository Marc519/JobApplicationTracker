using System;
using System.Text.Encodings.Web;

namespace JobApplicationTracker;
public enum Status
{
	Rejected = 0,
	Applied = 1,
	Interview = 2,
	Offer = 3
}

public class JobApplication(long jobID, string? jobTitle, string? companyName, string? jobLink, string? searchableText, string? extraNotes, Status jobStatus, DateOnly date)
{
	public long JobID { get; set; } = jobID;
	public string? JobTitle { get; set; } = jobTitle;
	public string? CompanyName { get; set; } = companyName;
	public string? JobLink { get; set; } = jobLink;
	public string? SearchableText { get; set; } = searchableText;
	public string? ExtraNotes { get; set; } = extraNotes;
	public Status JobStatus { get; set; } = jobStatus;
	public DateOnly ApplicationDate { get; set; } = date;
}
