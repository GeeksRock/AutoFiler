using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoFiler
{
    /// <summary>
    /// Interaction logic for winConfigure.xaml
    /// </summary>
    public partial class winConfigure : Window
    {
        public winConfigure()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserDuplicateFileNameOption();
            LoadUserUnmanagedFileTypeOption();
        }

        /// <summary>
        /// Selects the radio button corresponding to the user's saved User Option for Unmanaged File Types
        /// </summary>
        private void LoadUserUnmanagedFileTypeOption()
        {
            switch (Properties.Settings.Default.UnmanagedFileType)
            {
                case "AutoCheck":
                    rdoAutoCheck.IsChecked = true;
                    break;
                case "ManualCheck":
                    rdoManualCheck.IsChecked = true;
                    break;
            }
        }

        /// <summary>
        /// Selects the radio button corresponding to the user's saved User Option for Duplicate Filenames
        /// </summary>
        private void LoadUserDuplicateFileNameOption()
        {
            switch (Properties.Settings.Default.DuplicateFilenames)
            {
                case "Ignore":
                    rdoIgnore.IsChecked = true;
                    break;
                case "Overwrite":
                    rdoOverwrite.IsChecked = true;
                    break;
                case "Prompt":
                    rdoPrompt.IsChecked = true;
                    break;
            }
        }

        /// <summary>
        /// Returns the user's configuration selection for Duplicate File Names
        /// </summary>
        /// <returns>string</returns>
        private string GetUserDuplicateFileNameOption()
        {
            if (rdoIgnore.IsChecked == true) { return "Ignore"; }
            if (rdoOverwrite.IsChecked == true) { return "Overwrite"; }
            if (rdoPrompt.IsChecked == true) { return "Prompt"; }
            return null;
        }

        /// <summary>
        /// Returns the user's configuration selection for Unmanaged File Types
        /// </summary>
        /// <returns></returns>
        private string GetUserUnmanagedFileTypeOption()
        {
            if (rdoAutoCheck.IsChecked == true) { return "AutoCheck"; }
            if (rdoManualCheck.IsChecked == true) { return "ManualCheck"; }
            return null;
        }

        /// <summary>
        /// Saves the user's configuration selection to appConfig.
        /// </summary>
        private void SaveUserOption()
        {
            Properties.Settings.Default.DuplicateFilenames = GetUserDuplicateFileNameOption();
            Properties.Settings.Default.UnmanagedFileType = GetUserUnmanagedFileTypeOption();
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Unchecks all other radio buttons when another is checked.
        /// </summary>
        /// <param name="rdo">RadioButton</param>
        private void ToggleRadioButtons(RadioButton rdo)
        {
            switch (rdo.Name)
            {
                case "rdoIgnore":
                    rdoOverwrite.IsChecked = false;
                    rdoPrompt.IsChecked = false;
                    break;
                case "rdoOverwrite":
                    rdoIgnore.IsChecked = false;
                    rdoPrompt.IsChecked = false;
                    break;
                case "rdoPrompt":
                    rdoIgnore.IsChecked = false;
                    rdoOverwrite.IsChecked = false;
                    break;
                case "rdoManualCheck":
                    rdoAutoCheck.IsChecked = false;
                    break;
                case "rdoAutoCheck":
                    rdoManualCheck.IsChecked = false;
                    break;
            }
        }

        private void rdoIgnore_Checked(object sender, RoutedEventArgs e)
        {
            ToggleRadioButtons((RadioButton)sender);
        }

        private void rdoOverwrite_Checked(object sender, RoutedEventArgs e)
        {
            ToggleRadioButtons((RadioButton)sender);
        }

        private void rdoPrompt_Checked(object sender, RoutedEventArgs e)
        {
            ToggleRadioButtons((RadioButton)sender);
        }

        private void Configure_Click(object sender, RoutedEventArgs e)
        {
            SaveUserOption();
            this.Close();
        }

        private void rdoAutoCheck_Checked(object sender, RoutedEventArgs e)
        {
            ToggleRadioButtons((RadioButton)sender);
        }

        private void rdoManualCheck_Checked(object sender, RoutedEventArgs e)
        {
            ToggleRadioButtons((RadioButton)sender);
        }

    }
}
