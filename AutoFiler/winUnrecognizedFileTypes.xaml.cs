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
using System.Collections;
using System.Data;
using System.IO;
using System.Threading;

namespace AutoFiler
{
    /// <summary>
    /// Interaction logic for UnrecognizedFileTypes.xaml
    /// </summary>
    public partial class UnrecognizedFileTypes : Window
    {
        bool autoDisplay = false;
        delegate void ShutdownDelegate();
        Thread callingThread;
        internal void ClosePrompt()
        {
            ShutdownDelegate d = new ShutdownDelegate(ClosePrompt);
        }

        public List<FileType> listUFT = new List<FileType>();
        public List<FolderDestination> lstFD = new List<FolderDestination>();
        public AutoFilerService _afs;
        public UnrecognizedFileTypes(bool isAutoDisplay, Thread thread, AutoFilerService afs)
        {
            autoDisplay = isAutoDisplay;
            InitializeComponent();
            callingThread = thread;
            _afs = afs;
            callingThread = _afs.PromptToManageThread;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFolderDestinations();
            LoadUnmanagedFileTypes();
            this.rdoExisting.IsChecked = true;
            if (autoDisplay == true)
            {
                MessageBox.Show("Some file types in your AutoFiler drop folder have not been associated with folder destination.", "AutoFiler", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region Determine which file types in the drop folder are unmanaged

        /// <summary>
        /// Loads unmanaged custom and preloaded file types that have been added to the drop folder
        /// </summary>
        private void LoadUnmanagedFileTypes()
        {
            try
            {
                _afs.GetAllManagedFileTypes();
                _afs.FindAndSortUniqueFileTypes();

                foreach (FileType ft in _afs.listUnmanagedFileTypes)
                {
                    if (ft.Extension != "autofiler") { this.cboFileTypes.Items.Add(ft); }
                }

                this.cboFileTypes.DisplayMemberPath = "Extension";
                if (this.cboFileTypes.Items.Count > 0) { this.cboFileTypes.SelectedIndex = 0; }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at LoadUnmanagedFileTypes()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Unmanaged File Type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Associate unmanaged file types with new or existing folder destinations

        /// <summary>
        /// Loads predefined folder destinations.
        /// </summary>
        private void LoadFolderDestinations()
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
                MessageBox.Show("An unexpected error occurred at LoadFolderDestinations()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Unmanaged File Type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
       /// <summary>
        /// Associates file types with folder destinations
        /// </summary>
        private void AssociateFileTypeWithFolderDestination()
        {
            try
            {
                FileType uft = (FileType)this.cboFileTypes.SelectedItem;

                foreach (FileType f in this._afs.listUnmanagedFileTypes)
                {
                    if (uft.Extension.Equals(f.Extension))
                    {
                        if (rdoExisting.IsChecked == true)
                        {
                            f.ManageOption = "Existing";

                            if (this.cboFolders.Items.Count == 0)
                            {
                                MessageBox.Show("No predefined folder destinations exist." +
                                Environment.NewLine + "You must browse for a folder destination to continue.", "AutoFiler", MessageBoxButton.OK, MessageBoxImage.Error);
                                rdoNew.IsChecked = true;
                            }
                            else
                            {
                                f.Destination = this.cboFolders.SelectedItem.ToString();
                                if (ManageFileTypes(f) == false)
                                {
                                    this.cboFileTypes.Items.Remove(uft);
                                    if (this.cboFileTypes.Items.Count > 0)
                                    {
                                        this.cboFileTypes.SelectedIndex = 0;
                                    }
                                    else
                                    {
                                        this.cboFileTypes.IsEnabled = false;
                                        this.Update.IsEnabled = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            f.ManageOption = "New";

                            if (txtFolder.Text.Trim() == string.Empty)
                            {
                                MessageBox.Show("You must browse for a folder destination to continue.", "AutoFiler", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                f.Destination = this.txtFolder.Text;
                                if (ManageFileTypes(f) == false)
                                {
                                    this.cboFileTypes.Items.Remove(uft);
                                    if (this.cboFileTypes.Items.Count > 0)
                                    {
                                        this.cboFileTypes.SelectedIndex = 0;
                                        LoadFolderDestinations();
                                        this.txtFolder.Clear();
                                    }
                                    else
                                    {
                                        this.cboFileTypes.IsEnabled = false;
                                        this.Update.IsEnabled = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at AssociateFileTypeWithFolderDestination()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Unmanaged File Type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Associates an unmanaged file type with a folder destination.
        /// </summary>
        /// <param name="uft">UnmanagedFileType</param>
        /// <returns>bool</returns>
        private bool ManageFileTypes(FileType uft)
        {
            bool errorFound = false;

            try
            {
                //Required: Specify Folder Destination
                if (uft.Destination.Trim() == String.Empty)
                {
                    MessageBox.Show("Select a folder destination to continue.", "AutoFiler");
                    errorFound = true;
                }

                //Required: Unique Folder Destinations
                if (errorFound == false)
                {
                    if (uft.ManageOption == "New")
                    {
                        foreach (FolderDestination fd in lstFD)
                        {
                            if (fd.FolderName == uft.Destination.Trim())
                            {
                                //prevent creation of duplicate folder destinations
                                MessageBox.Show("Select a unique folder destination to continue.", "AutoFiler");
                                errorFound = true;
                            }
                        }
                    }
                }

                //Required: Prevent the use of autofiler as a custom extension
                if (errorFound == false)
                {
                    if (uft.Extension == "autofiler")
                    {
                        //prevent creation of the custom extension
                        MessageBox.Show("Unable to create this custom file type." +
                            Environment.NewLine + "The <autofiler> file type is used by the AutoFiler applicatiton.", "AutoFiler");
                        errorFound = true;
                    }
                }

                if (errorFound == false)
                {
                    errorFound = true;//anticipate errors

                    //save destination path and file types
                    if (uft.ManageOption == "New")
                    {
                        try
                        {
                            AddToNewFolderDestination(uft);
                            errorFound = false;
                        }
                        catch (Exception exc)
                        {
                            errorFound = true;
                            throw exc;
                        }
                    }
                    else
                    {
                        try
                        {
                            AddToExistingFolderDestination(uft);
                            errorFound = false;
                        }
                        catch (Exception exc)
                        {
                            errorFound = true;
                            throw exc;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at ManageFileTypes()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Unmanaged File Type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return errorFound;
        }

        /// <summary>
        /// Associates unmanaged file type with an existing folder destination
        /// </summary>
        /// <param name="uft">UnmanagedFileType</param>
        private void AddToExistingFolderDestination(FileType uft)
        {
            try
            {
                //retrieve the destination ID
                DataSet ds = new DataSet().GetDestinationIdByName(uft.Destination);
                int id = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    id = Convert.ToInt16(dr["idDestinations"]);
                }

                if (uft.Type == "Custom")
                {
                    bool isAssociated = new bool().AddNewCustomFileType(uft.Extension, id);
                }
                else
                {
                    bool isAssociated = new bool().AssociatePreloadedFileType(uft.Extension, id);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at AddToExistingFolderDestination()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Unmanaged File Type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

         /// <summary>
        /// Associates unmanaged file type with a new folder destination
        /// </summary>
        /// <param name="uft">UnmanagedFileType</param>
        private void AddToNewFolderDestination(FileType uft)
        {
            try
            {
                //create a new destination and retrieve its ID
                int id = new int().AddNewFolderDestination(uft.Destination);

                if (uft.Type == "Custom")
                {
                    bool isCreated = new bool().AddNewCustomFileType(uft.Extension, id);
                }
                else
                {
                    bool isAssociated = new bool().AssociatePreloadedFileType(uft.Extension, id);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at AddToNewFolderDestination()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Unmanaged File Type", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Event Handlers

        private void rdoNew_Checked(object sender, RoutedEventArgs e)
        {
            this.Browse.IsEnabled = true;
        }

        private void rdoNew_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Browse.IsEnabled = false;
        }

        private void rdoExisting_Checked(object sender, RoutedEventArgs e)
        {
            this.cboFolders.IsEnabled = true;
        }

        private void rdoExisting_Unchecked(object sender, RoutedEventArgs e)
        {
            this.cboFolders.IsEnabled = false;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            //force the service to execute before closing
            //_afs.TriggerAutoFiler();
            this.Close();
        }

        private void cboFileTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            AssociateFileTypeWithFolderDestination();
        } 

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Choose a destination folder:";
            fbd.ShowDialog();
            this.txtFolder.Text = fbd.SelectedPath;
        }

        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (callingThread != null)
            {
                if (callingThread.IsAlive)
                {
                    callingThread.Abort();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
