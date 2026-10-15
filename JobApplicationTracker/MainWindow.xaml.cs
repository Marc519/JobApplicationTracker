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
            if (e.Key == Key.Enter && sender is TextBox tb)
            {
                previewDisplay.NewJobIDs(repository.SearchApplications(tb.Text, null, null, null));
                PageNumberDisplay.DataContext = previewDisplay.PageNumber;
                ApplicationPreviewPanel.DataContext = previewDisplay.JobsOnDisplay;
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

            previewDisplay.NewJobIDs(repository.SearchApplications(SearchBox.Text, jobStatus, startDate, endDate));
            PageNumberDisplay.DataContext = previewDisplay.PageNumber;
            ApplicationPreviewPanel.DataContext = previewDisplay.JobsOnDisplay;
        }
    }
}