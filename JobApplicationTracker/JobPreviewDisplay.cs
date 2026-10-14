using System;

namespace JobApplicationTracker;
public class JobPreviewDisplay
{
	public List<JobApplication> JobsOnDisplay = null;
	public int PageNumber = 1;
	public int MaxPages;
	private List<long> jobIDs = null;
	private int jobsToDisplay = 1;
	private ApplicationRepository appRepo = null;

	public JobPreviewDisplay(ApplicationRepository appRepo)
	{
		this.appRepo = appRepo;
		jobIDs = appRepo.SearchApplications(null, null, null, null);
		JobsOnDisplay = appRepo.GetApplication(jobIDs[0..Math.Min(jobsToDisplay, jobIDs.Count)]);

		int remainder;
		(MaxPages, remainder) = Math.DivRem(jobIDs.Count, jobsToDisplay);
		if (remainder > 0)
		{
			MaxPages++;
		}
	}

	public void PrevPage()
	{
		if (PageNumber > 1)
		{
			PageNumber--;
			int startIndex = (PageNumber - 1) * jobsToDisplay;
			int endIndex = Math.Min(startIndex + jobsToDisplay, jobIDs.Count);
			JobsOnDisplay = appRepo.GetApplication(jobIDs[startIndex..endIndex]);
		}
	}

	public void ToPage(int page)
	{
		if (page <= MaxPages && page > 0)
		{
			PageNumber = page;
			int startIndex = (PageNumber - 1) * jobsToDisplay;
			int endIndex = Math.Min(startIndex + jobsToDisplay, jobIDs.Count);
			JobsOnDisplay = appRepo.GetApplication(jobIDs[startIndex..endIndex]);
		}
	}

	public void NextPage()
	{
		if (PageNumber < MaxPages)
		{
			PageNumber++;
			int startIndex = (PageNumber - 1) * jobsToDisplay;
			int endIndex = Math.Min(startIndex + jobsToDisplay, jobIDs.Count);
			JobsOnDisplay = appRepo.GetApplication(jobIDs[startIndex..endIndex]);
		}
	}

}
