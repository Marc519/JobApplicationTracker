using System;

namespace JobApplicationTracker;
public class JobPreviewDisplay
{
	public List<JobApplication> JobsOnDisplay = null!;
	public int PageNumber = 1;
	public int MaxPages;
	public List<long> JobIDs = null!;
	private int jobsToDisplay = 10;
	private ApplicationRepository appRepo = null!;

	public JobPreviewDisplay(ApplicationRepository appRepo)
	{
		this.appRepo = appRepo;
		JobIDs = appRepo.SearchApplications(null, null, null, null);
		JobsOnDisplay = appRepo.GetApplication(JobIDs[0..Math.Min(jobsToDisplay, JobIDs.Count)]);

		int remainder;
		(MaxPages, remainder) = Math.DivRem(JobIDs.Count, jobsToDisplay);
		if (remainder > 0)
		{
			MaxPages++;
		}
	}

	public void NewJobIDs(List<long> jobIDs)
	{
		JobIDs = jobIDs;

		int remainder;
		(MaxPages, remainder) = Math.DivRem(JobIDs.Count, jobsToDisplay);
		if (remainder > 0)
		{
			MaxPages++;
		}

		ToPage(1);
	}

	public void PrevPage()
	{
		if (PageNumber > 1)
		{
			PageNumber--;
			int startIndex = (PageNumber - 1) * jobsToDisplay;
			int endIndex = Math.Min(startIndex + jobsToDisplay, JobIDs.Count);
			JobsOnDisplay = appRepo.GetApplication(JobIDs[startIndex..endIndex]);
		}
	}

	public void ToPage(int page)
	{
		if (page <= MaxPages && page > 0)
		{
			PageNumber = page;
			int startIndex = (PageNumber - 1) * jobsToDisplay;
			int endIndex = Math.Min(startIndex + jobsToDisplay, JobIDs.Count);
			JobsOnDisplay = appRepo.GetApplication(JobIDs[startIndex..endIndex]);
		} else if (MaxPages == 0)
		{
			JobsOnDisplay = new List<JobApplication>();
		}
	}

	public void NextPage()
	{
		if (PageNumber < MaxPages)
		{
			PageNumber++;
			int startIndex = (PageNumber - 1) * jobsToDisplay;
			int endIndex = Math.Min(startIndex + jobsToDisplay, JobIDs.Count);
			JobsOnDisplay = appRepo.GetApplication(JobIDs[startIndex..endIndex]);
		}
	}

}
