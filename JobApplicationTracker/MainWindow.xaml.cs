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
    }
}