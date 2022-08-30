using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileDataChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime dateValue; // Date value selected in UI
        private Microsoft.Win32.OpenFileDialog file; // File selected in UI
        private VistaFolderBrowserDialog folder; // Folder selected in UI

        public MainWindow()
        {
            InitializeComponent();
            creationDateCheckBox.IsChecked = true;
            ScaleWindow(1.10);
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

            // Center screen
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            Top = (screenHeight - Height) / 2;
            Left = (screenWidth - Width) / 2;
        }

        private void btnOpenFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                file = fileDialog;
                folder = null;
                txtFileName.Text = fileDialog.SafeFileName;
                txtDateCreated.Text = File.GetCreationTime(fileDialog.FileName).ToString();
                txtDateModified.Text = File.GetLastWriteTime(fileDialog.FileName).ToString();

                UnlockControls();
            }
        }

        private void btnOpenFolder(object sender, RoutedEventArgs e)
        {
            var fileDialog = new VistaFolderBrowserDialog();
            bool? result = fileDialog.ShowDialog();
            if (result == true && !string.IsNullOrWhiteSpace(fileDialog.SelectedPath))
            {
                folder = fileDialog;
                file = null;
                string folderName = fileDialog.SelectedPath;
                for (int i = 0; i <= 20; i++)
                    folderName = folderName.Substring(folderName.IndexOf("\\") + 1);


                txtFileName.Text = folderName;
                txtDateCreated.Text = Directory.GetCreationTime(fileDialog.SelectedPath).ToString();
                txtDateModified.Text = Directory.GetLastWriteTime(fileDialog.SelectedPath).ToString();

                UnlockControls();
            }
        }

        private void selectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            btnApply.IsEnabled = true;
            dateValue = selectDate.SelectedDate.Value;
        }

        private void selectedTimeTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int hours = Int32.Parse(selectHours?.Text ?? "0");
                int minutes = Int32.Parse(selectMinutes?.Text ?? "0");
                int seconds = Int32.Parse(selectSeconds?.Text ?? "0");
                TimeSpan currentTime = new TimeSpan(hours, minutes, seconds);
                dateValue = dateValue.ChangeTime(currentTime.Hours, currentTime.Minutes, currentTime.Seconds);
            }
            catch
            {
                selectHours.Text = "00";
                selectMinutes.Text = "00";
                selectSeconds.Text = "00";
            }
        }

        private void btnApplyClick(object sender, RoutedEventArgs e)
        {
            if (folder != null)
            {
                SetDate(EntityType.Folder);
            }
            else
            {
                SetDate(EntityType.File);
            }
        }

        private void creationDateCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            modifyDateCheckBox.IsChecked = false;
        }

        private void modifyDateCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            creationDateCheckBox.IsChecked = false;
        }

        /// <summary>
        /// Shows last modify and creation dates in UI
        /// </summary>
        private void UpdateData()
        {
            if (folder == null)
            {
                txtDateCreated.Text = File.GetCreationTime(file.FileName).ToString();
                txtDateModified.Text = File.GetLastWriteTime(file.FileName).ToString();
            }
            else
            {
                txtDateCreated.Text = Directory.GetCreationTime(folder.SelectedPath).ToString();
                txtDateModified.Text = Directory.GetLastWriteTime(folder.SelectedPath).ToString();
            }
        }

        /// <summary>
        /// Unlock buttons and fields
        /// </summary>
        private void UnlockControls()
        {
            selectDate.IsEnabled = true;
            selectHours.IsEnabled = true;
            selectMinutes.IsEnabled = true;
            selectSeconds.IsEnabled = true;
            creationDateCheckBox.IsEnabled = true;
            modifyDateCheckBox.IsEnabled = true;
            btnApply.IsEnabled = true;
        }

        /// <summary>
        /// Sets last modify date or creation date
        /// </summary>
        /// <param name="type"></param>
        private void SetDate(EntityType type)
        {
            if (type == EntityType.File)
            {
                // Set file last modify time
                if (modifyDateCheckBox.IsChecked == true)
                    File.SetLastWriteTime(file.FileName, dateValue);
                // Set file creation time
                else
                    File.SetCreationTime(file.FileName, dateValue);
                UpdateData();
            }
            else if (type == EntityType.Folder)
            {
                // Set folder last write time
                if (modifyDateCheckBox.IsChecked == true)
                    Directory.SetLastWriteTime(folder.SelectedPath, dateValue);
                // Set folder creation time
                else
                    Directory.SetCreationTime(folder.SelectedPath, dateValue);
                UpdateData();
            }
        }

        enum EntityType
        {
            File,
            Folder
        }
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
