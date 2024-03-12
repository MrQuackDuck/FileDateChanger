using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;
using System.Linq;

namespace FileDataChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Initialization

        private DateTime dateValue; // Currently selected date
        private OpenFileDialog file; // Currently selected file
        private VistaFolderBrowserDialog folder; // Currently selected folder

        public MainWindow()
        {
            // Setting up start values
            dateValue = DateTime.Now;
            InitializeComponent();
            creationDateCheckBox.IsChecked = true;
            ScaleWindow(1.15);

            // Centering the window relative to the screen
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            Top = (screenHeight - Height) / 2;
            Left = (screenWidth - Width) / 2;
        }

        /// <summary>
        /// Set resolution of window
        /// </summary>
        /// <param name="scaleSize"></param>
        private void ScaleWindow(double scaleSize)
        {
            MainContainer.LayoutTransform = new ScaleTransform(scaleSize, scaleSize, 0, 0);
            Width *= scaleSize;
            Height *= scaleSize;
        }

        #endregion

        #region Event Listeners

        private void onOpenFileClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (!(fileDialog.ShowDialog() ?? false)) return;
            
            file = fileDialog;
            folder = null;
            txtFileName.Text = fileDialog.SafeFileName;
            txtDateCreated.Text = File.GetCreationTime(fileDialog.FileName).ToString();
            txtDateModified.Text = File.GetLastWriteTime(fileDialog.FileName).ToString();

            UnlockControls();
        }

        private void onOpenFolderClicked(object sender, RoutedEventArgs e)
        {
            var fileDialog = new VistaFolderBrowserDialog();
            bool result = fileDialog.ShowDialog() ?? false;
            if (!result && string.IsNullOrWhiteSpace(fileDialog.SelectedPath)) return;

            folder = fileDialog;
            file = null;
            string folderName = fileDialog.SelectedPath;

            // Remove path info (e.g: turn "C:/Program Files/Folder" into "/Folder")
            folderName = folderName.Substring(folderName.LastIndexOf("\\")).Replace('\\', '/');

            txtFileName.Text = folderName;
            txtDateCreated.Text = Directory.GetCreationTime(fileDialog.SelectedPath).ToString();
            txtDateModified.Text = Directory.GetLastWriteTime(fileDialog.SelectedPath).ToString();

            UnlockControls();
        }

        private void onApplyClicked(object sender, RoutedEventArgs e)
        {
            if (folder != null) SetDate(EntityType.Folder);
            else SetDate(EntityType.File);
        }

        private void onCreationDateButtonChecked(object sender, RoutedEventArgs e)
        {
            txtDateCreated.Opacity = 1;
            txtDateModified.Opacity = 0.4;
            modifyDateCheckBox.IsChecked = false;
        }

        private void onModifiedDateButtonChecked(object sender, RoutedEventArgs e)
        {
            txtDateCreated.Opacity = 0.4;
            txtDateModified.Opacity = 1;
            creationDateCheckBox.IsChecked = false;
        }

        private void dateAndTimeValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var newDate = dateAndTime.Value.Value;
            dateValue = newDate;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates "last modified" and "creation date" labels
        /// </summary>
        private void UpdateData()
        {
            if (folder is null)
            {
                txtDateCreated.Text = File.GetCreationTime(file.FileName).ToString();
                txtDateModified.Text = File.GetLastWriteTime(file.FileName).ToString();
                return;
            }

            txtDateCreated.Text = Directory.GetCreationTime(folder.SelectedPath).ToString();
            txtDateModified.Text = Directory.GetLastWriteTime(folder.SelectedPath).ToString();
        }

        /// <summary>
        /// Unlocks buttons and fields
        /// </summary>
        private void UnlockControls()
        {
            dateAndTime.IsReadOnly = false;
            dateAndTime.Opacity = 1;
            creationDateCheckBox.IsEnabled = true;
            modifyDateCheckBox.IsEnabled = true;
            btnApply.IsEnabled = true;
        }

        /// <summary>
        /// Sets last modified date or the creation date (depending on user's choice)
        /// </summary>
        /// <param name="type"></param>
        private void SetDate(EntityType type)
        {
            if (type is EntityType.File)
            {
                string filePath = file.FileName;
                if (modifyDateCheckBox.IsChecked is true)
                {
                    File.SetLastWriteTime(filePath, dateValue);
                }
                else
                {
                    File.SetCreationTime(filePath, dateValue);
                }
            }
            else if (type is EntityType.Folder)
            {
                string folderPath = folder.SelectedPath;
                if (modifyDateCheckBox.IsChecked is true)
                {
                    Directory.SetLastWriteTime(folderPath, dateValue);
                }
                else
                {
                    Directory.SetCreationTime(folderPath, dateValue);
                }
            }

            UpdateData();
        }

        #endregion
    }

    enum EntityType
    {
        File,
        Folder
    }

    public static class DateTimeExtensions
    {
        /// <summary>
        /// DateTime extension method. Allows to edit DateTime object and returns new DateTime object
        /// </summary>
        public static DateTime ChangeTime(this DateTime dateTime, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }
    }
}
