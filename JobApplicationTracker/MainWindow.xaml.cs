using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JobApplicationTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            previewDisplay.NextPage();
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
            if (e.AddedItems.Count > 0)
            {
                var application = (JobApplication) e.AddedItems[0]!;
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
                }
            } 
        }

        private void AddApplication_Click(object sender, RoutedEventArgs e)
        {
            AddApplicationPopup.IsOpen = !AddApplicationPopup.IsOpen;
        }

        private async void SubmitApplication_Click(object sender, RoutedEventArgs e)
        {
            if (NewJobDate.SelectedDate != null && NewJobStatus.SelectedIndex != -1)
            {
                string newJobTitle = NewJobTitle.Text;
                string newCompanyName = NewCompanyName.Text;
                string newJobLink = NewJobLink.Text;
                Status newJobStatus = (Status)NewJobStatus.SelectedIndex;
                DateOnly newJobDate = DateOnly.FromDateTime((DateTime)NewJobDate.SelectedDate);

                string? searchableText = await JobPostingService.getSearchableText(newJobLink);
                if (searchableText == null && newJobLink != "")
                {
                    MessageBox.Show("Could not retrieve HTML data from job link");
                }

                repository.AddApplication(newJobTitle, newCompanyName, newJobLink, searchableText, "", newJobStatus, newJobDate);

                NewJobTitle.Text = "";
                NewCompanyName.Text = "";
                NewJobLink.Text = "";
                NewJobStatus.SelectedIndex = -1;
                NewJobDate.SelectedDate = null;

            } else
            {
                MessageBox.Show("Must supply a value for job status and job application date");
            }

            ApplyFilters_Click(sender, e);
        }
    }
}