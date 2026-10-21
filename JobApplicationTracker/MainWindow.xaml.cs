using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JobApplicationTracker
{
    public partial class MainWindow : Window
    {
        private JobPreviewDisplay previewDisplay;
        private ApplicationRepository repository;
        public MainWindow()
        {
            InitializeComponent();

            repository = new ApplicationRepository();
            bool retVal = repository.VerifyFile();

            if (!retVal) 
            {
                MessageBox.Show("Warning: database file at "
                    + System.IO.Path.Combine(repository.pathToDir, repository.dbFileName)
                    + "could not be verified, database table structure may be invalid.\n"
                    + "To fix, please delete the current file or move elsewhere.", "Warning", MessageBoxButton.OK);
            }
            repository.CreateTable();
            previewDisplay = new JobPreviewDisplay(repository);

            ApplicationPreviewPanel.DataContext = previewDisplay.JobsOnDisplay;
            PageNumberDisplay.DataContext = previewDisplay.PageNumber;
            MaxPageNumber.DataContext = previewDisplay.MaxPages;
            ApplicationPanel.DataContext = null;
        }

        private void TextBox_PreviewLMB(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }
        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            previewDisplay.NextPage();
            PageNumberDisplay.DataContext = previewDisplay.PageNumber;
            ApplicationPreviewPanel.DataContext = previewDisplay.JobsOnDisplay;
        }

        private void SwitchPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SwitchPage_LostFocus(sender, e);
            }
        }

        private void SwitchPage_LostFocus(object sender, RoutedEventArgs e)
        {
            string inputString = PageNumberDisplay.Text;
            if (int.TryParse(inputString, out int result))
            {
                previewDisplay.ToPage(result);
            }
            PageNumberDisplay.DataContext = previewDisplay.PageNumber;
            ApplicationPreviewPanel.DataContext = previewDisplay.JobsOnDisplay;
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            previewDisplay.PrevPage();
            PageNumberDisplay.DataContext = previewDisplay.PageNumber;
            ApplicationPreviewPanel.DataContext = previewDisplay.JobsOnDisplay;
        }

        private void ApplicationPreviewPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is JobApplication jobApplication)
            {
                MainInfoOverlay.Opacity = 1;
                MainInfoOverlay.IsHitTestVisible = true;
                var application = new JobApplicationCompatibilityLayer(jobApplication);
                ApplicationPanel.DataContext = application;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilters_Click(sender, e);
            }
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            FilterPopup.IsOpen = !FilterPopup.IsOpen;
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            DateOnly? startDate = null;
            DateOnly? endDate = null;
            Status? jobStatus = StatusFilter.SelectedIndex == 0 ? null : (Status)StatusFilter.SelectedIndex - 1;
            if (StartDateFilter.SelectedDate != null)
            {
                startDate = DateOnly.FromDateTime((DateTime)StartDateFilter.SelectedDate);
            }
            if (EndDateFilter.SelectedDate != null)
            {
                endDate = DateOnly.FromDateTime((DateTime)EndDateFilter.SelectedDate);
            }
            bool isAscending = IsAscending.IsChecked == true;

            previewDisplay.IsAscending = isAscending;
            previewDisplay.NewJobIDs(repository.SearchApplications(SearchBox.Text, jobStatus, startDate, endDate, isAscending));
            PageNumberDisplay.DataContext = previewDisplay.PageNumber;
            MaxPageNumber.DataContext = previewDisplay.MaxPages;
            ApplicationPreviewPanel.DataContext = previewDisplay.JobsOnDisplay;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button delButton && delButton.DataContext is JobApplication job)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to delete your " + job.JobTitle + " application at "
                    + job.CompanyName + "?", "Confirm deletion", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    bool successful = repository.RemoveApplication(job.JobID);
                    ApplyFilters_Click(sender, e);

                    MessageBox.Show("Deletion " + (successful ? "successful" : "unsuccessful"));

                    if (successful)
                    {
                        // if the deleted job is being displayed in the right hand window, hide it
                        if (ApplicationPanel.DataContext is JobApplicationCompatibilityLayer displayedJob
                            && displayedJob.JobApp.JobID == job.JobID)
                        {
                            MainInfoOverlay.Opacity = 0;
                            IsHitTestVisible = true;
                        }
                    }
                }
            } 
        }

        private void AddApplication_Click(object sender, RoutedEventArgs e)
        {
            AddApplicationPopup.IsOpen = !AddApplicationPopup.IsOpen;
        }

        private async void SubmitApplication_Click(object sender, RoutedEventArgs e)
        {
            if (NewJobTitle.Text != "" && NewJobDate.SelectedDate != null && NewJobStatus.SelectedIndex != -1)
            {
                string newJobTitle = NewJobTitle.Text;
                string newCompanyName = NewCompanyName.Text;
                string newJobLink = NewJobLink.Text;
                string searchableText = "";
                Status newJobStatus = (Status)NewJobStatus.SelectedIndex;
                DateOnly newJobDate = DateOnly.FromDateTime((DateTime)NewJobDate.SelectedDate);

                if (newJobLink != "")
                {
                    string? retrievedText = await JobPostingService.getSearchableText(newJobLink);
                    if (retrievedText == null)
                    {
                        MessageBox.Show("Could not retrieve HTML data from job link");
                    } else
                    {
                        searchableText = retrievedText;
                    }
                }

                repository.AddApplication(newJobTitle, newCompanyName, newJobLink, searchableText, "", newJobStatus, newJobDate);

                NewJobTitle.Text = "";
                NewCompanyName.Text = "";
                NewJobLink.Text = "";
                NewJobStatus.SelectedIndex = -1;
                NewJobDate.SelectedDate = null;
            } else
            {
                MessageBox.Show("Must supply a value for job title, job status and job application date");
            }

            ApplyFilters_Click(sender, e);
        }

        private async void UpdateApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationPanel.DataContext is JobApplicationCompatibilityLayer appCL)
            {
                JobApplication app = appCL.JobApp;
                string oldJobLink = app.JobLink;
                string? newSearchableText = null;

                // fetch html text data only if job link has changed
                // if link is deleted, delete searchable text in storage
                if (EditJobLink.Text != oldJobLink && EditJobLink.Text != "")
                {
                    string? searchableText = await JobPostingService.getSearchableText(EditJobLink.Text);
                    if (searchableText == null)
                    {
                        MessageBox.Show("Could not retrieve HTML data from new job link: ", EditJobLink.Text);
                        BindingOperations.GetBindingExpression(EditJobLink, TextBox.TextProperty).UpdateTarget();
                    } else
                    {
                        newSearchableText = searchableText;
                    }
                } else if (EditJobLink.Text == "")
                {
                    newSearchableText = "";
                }

                if (EditDate.SelectedDate == null)
                {
                    EditDate.SelectedDate = appCL.ApplicationDateAsDateTime;
                }
                repository.UpdateApplication(app.JobID, EditJobTitle.Text, EditCompanyName.Text, EditJobLink.Text,
                    newSearchableText, EditExtraNotes.Text, (Status)EditJobStatus.SelectedIndex, DateOnly.FromDateTime((DateTime)EditDate.SelectedDate));
                
                // need to update in-memory application to match the wpf form
                app.JobLink = EditJobLink.Text;
                app.JobTitle = EditJobTitle.Text;
                app.CompanyName = EditCompanyName.Text;
                app.JobLink = EditJobLink.Text;
                app.ExtraNotes = EditExtraNotes.Text;
                appCL.JobStatusAsInt = EditJobStatus.SelectedIndex;
                appCL.ApplicationDateAsDateTime = (DateTime)EditDate.SelectedDate;
                
                // refresh the web browser with the new job link and refresh the search results
                BindingOperations.GetBindingExpression(WebBrowserWindow, Microsoft.Web.WebView2.Wpf.WebView2.SourceProperty).UpdateTarget();
                ApplyFilters_Click(sender, e);
            }
        }
    }
    public class JobApplicationCompatibilityLayer(JobApplication app)
    {
        public JobApplication JobApp { get; } = app;

        public string EmptyProtectedLink
        {
            get
            {
                if (JobApp.JobLink == "")
                {
                    return "about:blank";
                } else
                {
                    return JobApp.JobLink;
                }
            }
        }

        public int JobStatusAsInt
        {
            get
            {
                return (int)JobApp.JobStatus;
            }
            set
            {
                JobApp.JobStatus = (Status)value;
            }
        }
        public DateTime ApplicationDateAsDateTime
        {
            get
            {
                return JobApp.ApplicationDate.ToDateTime(TimeOnly.MinValue);
            }
            set
            {
                JobApp.ApplicationDate = DateOnly.FromDateTime(value);
            }
        }
    }
}