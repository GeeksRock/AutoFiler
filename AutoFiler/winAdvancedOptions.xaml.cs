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
using System.Data;

namespace AutoFiler
{
    /// <summary>
    /// Interaction logic for winAdvancedOptions.xaml
    /// </summary>
    public partial class winAdvancedOptions : Window
    {
        AutoFilerService AFS;
        public bool updated = false;
        string option = "B";
        List<FolderDestination> lstFD = new List<FolderDestination>();

        public winAdvancedOptions(AutoFilerService _AFS)
        {
            InitializeComponent();
            AFS = _AFS;
        }

        private void rdoBeginsWith_Checked(object sender, RoutedEventArgs e)
        {
            if (this.rdoBeginsWith.IsChecked == true)
            {
                this.option = "B"; 
            }
            else if (this.rdoContains.IsChecked == true)
            {
                this.option = "C";
            }
            else
            {
                this.option = "E";
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtFileName.Text.Length == 0)
            {
            }
            else
            {
                bool _override = false;
                if (this.chkOverride.IsChecked == true)
                {
                    _override = true;
                }

                string id = string.Empty;
                DataSet ds = new DataSet().GetDestinationIdByName(this.cboFolders.SelectedItem.ToString());
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    id = dr["idDestinations"].ToString();
                }

                FileName fn = new FileName
                {
                    fileName = this.txtFileName.Text,
                    fileOption = this.option,
                    doOverride = _override,
                    destination = id
                };

                this.lstFileNameText.Items.Add(fn.fileName);
                bool isCreated = new bool().AddNewFileName(fn.fileName, fn.fileOption, fn.doOverride, Convert.ToInt16(fn.destination));

                this.txtFileName.Clear();
                this.txtFileName.Focus();
                this.updated = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDestinationFolders();
        }

        private void LoadDestinationFolders()
        {
            try
            {
                DataSet dsFolders = new DataSet().GetAllFolderDestinations();
                this.cboFolders.Items.Clear();

                foreach (DataRow dr in dsFolders.Tables[0].Rows)
                {
                    this.cboFolders.Items.Add(dr["FolderDestination"].ToString());
                    FolderDestination fd = new FolderDestination
                    {
                        FolderName = dr["FolderDestination"].ToString(),
                        ID = dr["idDestinations"].ToString()
                    };
                    this.lstFD.Add(fd);
                }

                if (this.cboFolders.Items.Count > 0) { this.cboFolders.SelectedIndex = 0; }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at LoadDestinationFolders()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Unmanaged File Type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cboFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAssociatedFileNameText();
        }

        private void ShowAssociatedFileNameText()
        {
            this.lstFileNameText.Items.Clear();
            DataSet ds = new DataSet().GetFileNames(this.cboFolders.SelectedItem.ToString());
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                this.lstFileNameText.Items.Add(dr["FileName"]);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isDeleted = new bool().RemoveFileName(this.lstFileNameText.SelectedItem.ToString());
                object objRemove = (object)this.lstFileNameText.SelectedItem.ToString();
                this.lstFileNameText.Items.Remove(objRemove);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
