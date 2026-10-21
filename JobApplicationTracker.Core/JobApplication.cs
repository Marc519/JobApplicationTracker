
namespace JobApplicationTracker;
public enum Status
{
	Rejected = 0,
	Applied = 1,
	Interview = 2,
	Offer = 3
}

public class JobApplication(long jobID, string jobTitle, string companyName, string jobLink, string searchableText, string extraNotes, Status jobStatus, DateOnly applicationDate)
{
	public long JobID { get; set; } = jobID;
	public string JobTitle { get; set; } = jobTitle;
	public string CompanyName { get; set; } = companyName;
	public string JobLink { get; set; } = jobLink;
	public string SearchableText { get; set; } = searchableText;
	public string ExtraNotes { get; set; } = extraNotes;
	public Status JobStatus { get; set; } = jobStatus;
    public DateOnly ApplicationDate { get; set; } = applicationDate;

	override public string ToString()
	{
		return "(" + JobID.ToString() + ", " + JobTitle + ", " + CompanyName + ", " + JobLink + ", " + SearchableText
			+ ", " + ExtraNotes + ", " + JobStatus.ToString() + ", " + ApplicationDate.ToString() + ")";

	}

	public static bool AreEqual(JobApplication? jobA, JobApplication? jobB)
	{
		if (jobA == null && jobB == null)
		{
			return true;
		}

		if (jobB == null ^ jobA == null)
		{
			return false;
		}

		if (jobB.JobID == jobA.JobID
			&& string.Equals(jobB.JobTitle, jobA.JobTitle)
			&& string.Equals(jobB.CompanyName, jobA.CompanyName)
			&& string.Equals(jobB.JobLink, jobA.JobLink)
			&& string.Equals(jobB.SearchableText, jobA.SearchableText)
			&& string.Equals(jobB.ExtraNotes, jobA.ExtraNotes)
			&& jobB.JobStatus == jobA.JobStatus
			&& jobB.ApplicationDate == jobA.ApplicationDate)
		{
			return true;
		} else
		{
			return false;
		}
	}
}
