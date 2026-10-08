using System;
using System.Text.Encodings.Web;

namespace JobApplicationTracker;
public enum Status
{
	Rejected,
	Applied,
	Interview,
	Offer
}

public class JobApplication(long jobID, string? jobTitle, string? companyName, string? jobLink, string? searchableText, string? extraNotes, string? htmlData, string? cssData, Status jobStatus, DateTime date)
{
	public long JobID { get; set; } = jobID;
	public string? JobTitle { get; set; } = jobTitle;
	public string? CompanyName { get; set; } = companyName;
	public string? JobLink { get; set; } = jobLink;
	public string? SearchableText { get; set; } = searchableText;
	public string? ExtraNotes { get; set; } = extraNotes;
	public string? HTML { get; set; } = htmlData;
	public string? CSS { get; set; } = cssData;
	public Status JobStatus { get; set; } = jobStatus;
	public DateTime ApplicationDate { get; set; } = date;
}
