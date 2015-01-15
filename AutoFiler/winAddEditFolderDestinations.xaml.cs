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
using System.Collections;

namespace AutoFiler
{
    /// <summary>
    /// Interaction logic for winAddEditFolderDestinations.xaml
    /// </summary>
    public partial class winAddEditFolderDestinations : Window
    {
        public string function;
        public string load;
        public string result;
        public bool doClose = false;
        private AutoFilerService _AFS;
        public List<FileType> updateCustom;
        public List<FileType> removedCustom = new List<FileType>();
        public List<FileType> updatePreload;
        public List<FileType> lstFileTypes = new List<FileType>();

        public winAddEditFolderDestinations(string f, string l, AutoFilerService _afs)
        {
            InitializeComponent();
            function = f; //Add or Edit
            load = l;     //Destination to Load
            _AFS = _afs;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BuildDefaultFileTypesList();

            //determine if user is creating a new destination or editing an existing destination
            if (function == "Add")
            {
                EnableOrDisableUsedPreloadedFileTypes();
                this.Browse.Focus();
            }
            else
            {
                this.txtDestination.Text = load;
                this.txtDestination.ToolTip = load;
                SelectPreloadedFileTypes();
                EnableOrDisableUsedPreloadedFileTypes();
                LoadCustomFileTypes();
                PrepareToUpdate();
            }
        }

        private void PrepareToUpdate()
        {
            DataSet ds = new DataSet().GetCustomFileTypes(load);
            updateCustom = new List<FileType>();
            foreach (DataRow d in ds.Tables[0].Rows)
            {
                FileType f = new FileType()
                {
                    Destination = d["FolderDestination"].ToString(),
                    Extension = d["FileTypeName"].ToString()
                };
                updateCustom.Add(f);
            }

            ds = new DataSet().GetPreloadedFileTypes(load);
            updatePreload = new List<FileType>();
            foreach (DataRow d in ds.Tables[0].Rows)
            {
                FileType f = new FileType()
                {
                    Destination = d["FolderDestination"].ToString(),
                    Extension = d["FileTypeName"].ToString()
                };
                updatePreload.Add(f);
            }
        }

        private void BuildDefaultFileTypesList()
        {
            DataSet ds = new DataSet().GetAllPreloadedFileTypes();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                FileType ft = new FileType();
                ft.Destination = dr["FolderDestination"].ToString();
                ft.Extension = dr["FileTypeName"].ToString();
                ft.Type = "";
                ft.ManageOption = "";
                lstFileTypes.Add(ft);
            }
        }

        #region Show Associated FileTypes

        /// <summary>
        /// Displays custom file types.
        /// </summary>
        private void LoadCustomFileTypes()
        {
            try
            {
                DataSet dsFolders = new DataSet().GetCustomFileTypes(load);

                this.customfiletypes.Items.Clear();

                if (dsFolders.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsFolders.Tables[0].Rows)
                    {
                        this.customfiletypes.Items.Add(dr["FileTypeName"].ToString());
                    }
                }
                if (this.customfiletypes.Items.Count < 1) { this.Remove.IsEnabled = false; }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at LoadCustomFileTypes()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Folder Destination Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Enables or Disables preloaded filetypes based on their current use.
        /// </summary>
        private void EnableOrDisableUsedPreloadedFileTypes()
        {
            int x = 0;

            while (x < 24)
            {
                this.doc.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.docx.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.xls.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.xlsx.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.ppt.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.pptx.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.log.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.txt.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.rtf.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.msg.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.csv.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.dat.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.xml.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.avi.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.flv.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.mov.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.mp3.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.mp4.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.png.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.gif.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.ico.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.jpg.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.bmp.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
                this.tif.IsEnabled = IsFileTypeEnabled(lstFileTypes[x].Extension.ToString()); x++;
            }
        }

        /// <summary>
        /// Determines if a preloaded file type option should be enabled.
        /// </summary>
        /// <param name="s">string</param>
        /// <returns>bool</returns>
        private bool IsFileTypeEnabled(string s)
        {
            try
            {
                DataSet ds = new DataSet().GetAllPreloadedFileTypes();
                DataSet ds2 = new DataSet().GetDestinationIdByName(load);
                int id = 0;

                foreach (DataRow dr in ds2.Tables[0].Rows)
                {
                    id = Convert.ToInt16(dr["idDestinations"].ToString());
                }

                //RULE1: FileType associated with current FolderDestination = Enabled
                foreach (DataRow dr1 in ds.Tables[0].Rows)
                {
                    if (dr1["FileTypeName"].ToString().Equals(s))
                    {
                        if (dr1["FolderDestination"].ToString().Equals(id.ToString()))
                        {
                            return true;
                        }
                    }
                }
                //RULE2: FileType not associated with any FolderDestinations = Enabled
                foreach (DataRow dr2 in ds.Tables[0].Rows)
                {
                    if (dr2["FileTypeName"].ToString().Equals(s))
                    {
                        if (dr2["FolderDestination"].ToString().Equals(String.Empty))
                        {
                            return true;
                        }
                    }
                }
                //RULE3: FileType associated with other FolderDestinations = Disabled
                return false;
            }
            catch (MySql.Data.MySqlClient.MySqlException exc)
            {
                throw exc;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred at IsFileTypeDisabled()" +
                    Environment.NewLine + Environment.NewLine + ex.ToString(),
                    "AutoFiler - Folder Destination Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }

        /// <summary>
        /// Checks any preloaded file type associated with the current folder destination.
        /// </summary>
        private void SelectPreloadedFileTypes()
        {
            int x = 0;

            while (x < 24)
            {
                this.doc.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.docx.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.xls.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.xlsx.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.ppt.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.pptx.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.log.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.txt.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.rtf.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.msg.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.csv.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.dat.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.xml.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.avi.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.flv.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.mov.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.mp3.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.mp4.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.png.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.gif.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.ico.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.jpg.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.bmp.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
                this.tif.IsChecked = IsSelected(lstFileTypes[x].Extension.ToString()); x++;
            }
        }

        /// <summary>
        /// Determines if a preloaded file type is associated with the current folder destination
        /// </summary>
        /// <param name="s">string</param>
        /// <returns>bool</returns>
        private bool IsSelected(string s)
        {
            try
            {
                DataSet ds = new DataSet().GetPreloadedFileTypes(load);
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (dr["FileTypeName"].ToString().Equals(s)) return true;
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at IsSelected()" +
                     Environment.NewLine + Environment.NewLine + exc.ToString(),
                     "AutoFiler - Folder Destination Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
        #endregion
        
        #region Manage Destination and File Types

        /// <summary>
        /// Prevents a custom file type from being created, or associated with a folder destination. 
        /// </summary>
        private void RemoveCustomFileType()
        {
            if (this.customfiletypes.Items.Count > 0)
            {
                FileType f = new FileType
                {
                    Destination = load,
                    Extension = this.customfiletypes.SelectedItem.ToString()
                };
                removedCustom.Add(f);
                this.customfiletypes.Items.Remove(this.customfiletypes.SelectedItem);
                if (this.customfiletypes.Items.Count < 1) { this.Remove.IsEnabled = false; }
                if (this.customfiletypes.Items.Count > 0) { this.customfiletypes.SelectedIndex = 0; }
            }
        }

        /// <summary>
        /// Adds a unique custom file type to the list "on-screen" only. 
        /// </summary>
        private void AddCustomFileType()
        {
            try
            {
                DataSet dsPreloaded = new DataSet().GetAllPreloadedFileTypes();
                DataSet dsCustom = new DataSet().GetAllCustomFileTypes();

                bool errorFound = false;

                if (this.txtCustom.Text.Trim() != "")
                {
                    foreach (DataRow drPreloaded in dsPreloaded.Tables[0].Rows)
                    {
                        if (drPreloaded["FileTypeName"].ToString() == this.txtCustom.Text)
                        {
                            //prevent creation of custom file types that are already preloaded
                            MessageBox.Show("The custom file type you entered is a preloaded file type." +
                                Environment.NewLine + "If not in use by another folder destination, you can select it above.", "AutoFiler");
                            errorFound = true;
                        }
                    }

                    if (errorFound == false)
                    {
                        if (dsCustom.Tables.Count > 0)
                        {
                            foreach (DataRow drCustom in dsCustom.Tables[0].Rows)
                            {
                                if (drCustom["FileTypeName"].ToString().ToLower() == this.txtCustom.Text.ToLower())
                                {
                                    //prevent creation of custom file types that have already been created
                                    MessageBox.Show("The custom file type you entered is in use by another folder destination." +
                                        Environment.NewLine + "You must first remove the custom file type from its current folder destination.", "AutoFiler");
                                    errorFound = true;
                                }
                            }
                        }
                    }

                    if (errorFound == false)
                    {
                        if (this.txtCustom.Text.Trim() == "autofiler")
                        {
                            //prevent creation of the custom extension "autofiler"
                            MessageBox.Show("Unable to create the <autofiler> custom file type." +
                                Environment.NewLine + "The <autofiler> file type is used by the AutoFiler applicatiton.", "AutoFiler");
                            errorFound = true;
                        }
                    }

                    if (errorFound == false)
                    {
                        //don't add duplicate entries to the list
                        if (!this.customfiletypes.Items.Contains(this.txtCustom.Text))
                        {
                            this.customfiletypes.Items.Add(this.txtCustom.Text);
                            this.txtCustom.Clear();
                            this.txtCustom.Focus();
                        }
                        else
                        {
                            this.txtCustom.Clear();
                            this.txtCustom.Focus();
                        }
                    }
                }
                else
                {
                    //prompt user to enter an extension
                    MessageBox.Show("Unable to create a custom file type." +
                    Environment.NewLine + "Enter a file extension to continue.", "AutoFiler");
                }

                if (this.customfiletypes.Items.Count > 0) { this.Remove.IsEnabled = true; }
            }
            catch (MySql.Data.MySqlClient.MySqlException exc)
            {
                throw exc;
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at AddCustomFileType()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Folder Destination Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Save user created file types to CustomFileTypes.xml
        /// </summary>
        /// <param name="customfiletype">string</param>
        private bool CreateCustomFileType(string customfiletype, int idDestination)
        {
            bool isCreated = false;
            //Required: Prevent the use of autofiler as a custom extension
            if (customfiletype == "autofiler")
            {
                //prevent creation of the custom extension
                MessageBox.Show("Unable to create this custom file type." +
                    Environment.NewLine + "The <autofiler> file type is used by the AutoFiler applicatiton.", "AutoFiler");
            }
            else
            {
                try
                {
                    isCreated = new bool().AddNewCustomFileType(customfiletype, idDestination);
                }
                catch (Exception exc)
                {
                    MessageBox.Show("An unexpected error occurred at CreateCustomFileType()" +
                        Environment.NewLine + Environment.NewLine + exc.ToString(),
                        "AutoFiler - Folder Destination Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return isCreated;
        }

        /// <summary>
        /// Retrieves a list of user created file types
        /// </summary>
        /// <returns>ArrayList</returns>
        private ArrayList GetCustomFileTypes()
        {
            ArrayList custom = new ArrayList();
            foreach (string s in this.customfiletypes.Items)
            {
                custom.Add(s);
            }

            return custom;
        }

        /// <summary>
        /// Retrieves a list of the preloaded file types selected by the user
        /// </summary>
        /// <returns>ArrayList</returns>
        private ArrayList GetPreloadedFileTypeSelections()
        {
            ArrayList selections = new ArrayList();
            if (this.doc.IsChecked == true) { selections.Add("doc"); }
            if (this.docx.IsChecked == true) { selections.Add("docx"); }
            if (this.xls.IsChecked == true) { selections.Add("xls"); }
            if (this.xlsx.IsChecked == true) { selections.Add("xlsx"); }
            if (this.ppt.IsChecked == true) { selections.Add("ppt"); }
            if (this.pptx.IsChecked == true) { selections.Add("pptx"); }
            if (this.log.IsChecked == true) { selections.Add("log"); }
            if (this.txt.IsChecked == true) { selections.Add("txt"); }
            if (this.rtf.IsChecked == true) { selections.Add("rtf"); }
            if (this.msg.IsChecked == true) { selections.Add("msg"); }
            if (this.csv.IsChecked == true) { selections.Add("csv"); }
            if (this.dat.IsChecked == true) { selections.Add("dat"); }
            if (this.xml.IsChecked == true) { selections.Add("xml"); }
            if (this.avi.IsChecked == true) { selections.Add("avi"); }
            if (this.flv.IsChecked == true) { selections.Add("flv"); }
            if (this.mov.IsChecked == true) { selections.Add("mov"); }
            if (this.mp3.IsChecked == true) { selections.Add("mp3"); }
            if (this.mp4.IsChecked == true) { selections.Add("mp4"); }
            if (this.png.IsChecked == true) { selections.Add("png"); }
            if (this.gif.IsChecked == true) { selections.Add("gif"); }
            if (this.ico.IsChecked == true) { selections.Add("ico"); }
            if (this.jpg.IsChecked == true) { selections.Add("jpg"); }
            if (this.bmp.IsChecked == true) { selections.Add("bmp"); }
            if (this.tif.IsChecked == true) { selections.Add("tif"); }

            return selections;
        }

        /// <summary>
        /// Updates the current folder destination
        /// </summary>
        private bool UpdateDestination()
        {
            bool isUpdated = true;
            try
            {
                //If a new destination is selected, ensure that the new destination does not already exist
                bool isDuplicate = false;
                bool needtoUpdate = true;
                //Find the unique ID for the destination, and update
                int id = 0;
                DataSet ds1 = new DataSet().GetDestinationIdByName(load);
                foreach (DataRow d1 in ds1.Tables[0].Rows)
                {
                    id = Convert.ToInt16(d1["idDestinations"].ToString());
                }
                DataSet ds = new DataSet().GetAllFolderDestinations();
                List<FolderDestination> lstFD = new List<FolderDestination>();
                foreach (DataRow d in ds.Tables[0].Rows)
                {
                    FolderDestination fd = new FolderDestination();
                    fd.FolderName = d["FolderDestination"].ToString();
                    fd.ID = d["idDestinations"].ToString();
                    lstFD.Add(fd);
                }
                //Rule1: If the user does not change the destination, no update is needed
                if (this.txtDestination.Text == load) 
                { 
                    needtoUpdate = false;                 
                }
                //Rule2: If the user changes the destination to one that already exists, no update is needed, inform user
                if (needtoUpdate == true)
                {
                    foreach (FolderDestination fd in lstFD)

                    if (fd.FolderName.Equals(this.txtDestination.Text))
                    {
                        isDuplicate = true;
                        needtoUpdate = false;
                    }
                }
                //Rule3: If the user changes the destination to one that doesn't exist, an update is needed
                if (needtoUpdate == true)
                {
                    ArrayList arl = new ArrayList();
                    foreach (FolderDestination fd in lstFD)
                    {
                        arl.Add(fd.FolderName);
                    }
                    if (!arl.Contains(this.txtDestination.Text)) { needtoUpdate = true; }
                }

                if (isDuplicate == true)
                {
                    MessageBox.Show("The file destination you selected already exists." + Environment.NewLine + 
                        "To make changes, edit the folder destination.", "AutoFiler");
                    isUpdated = false;
                }
                else
                {
                    //Update the folder destination, if it has changed
                    if (needtoUpdate == true)
                    {
                        isUpdated = new bool().UpdateFolderDestination(this.txtDestination.Text.ToString(), id);
                    }

                    //Update custom file types
                    List<FileType> lstCustom = new List<FileType>();
                    foreach (string s in this.customfiletypes.Items)
                    {
                        FileType f = new FileType
                        {
                            Destination = this.txtDestination.Text,
                            Extension = s
                        };
                        lstCustom.Add(f);
                    }
                    // -- removed custom file types
                    foreach (FileType f in removedCustom)
                    {
                        isUpdated = new bool().RemoveCustomFileType(f.Extension.ToString());
                    }
                    // -- new custom file types
                    needtoUpdate = true;
                    foreach (FileType f in lstCustom)
                    {
                        foreach (FileType ft in updateCustom)
                        {
                            if (f.Extension.Equals(ft.Extension))
                            {
                                needtoUpdate = false;
                            }
                        }
                        if (needtoUpdate == true)
                        {
                            isUpdated = new bool().AddNewCustomFileType(f.Extension, id);
                        }
                        needtoUpdate = true;
                    }
                    //Update preloaded file types
                    ArrayList arlSelections = GetPreloadedFileTypeSelections();
                    // -- removed preload file types
                    foreach (FileType f in updatePreload)
                    {
                        if (!arlSelections.Contains(f.Extension.ToString()))
                        {
                            isUpdated = new bool().RemovePreloadedFileAssociation(f.Extension.ToString());
                        }
                    }
                    // -- new preload file types
                    foreach (string s in arlSelections)
                    {
                        foreach (FileType f in updatePreload)
                        {
                            if (f.Extension.Equals(s.ToString()))
                            {
                                needtoUpdate = false;
                            }
                        }
                        if (needtoUpdate == true)
                        {
                            isUpdated = new bool().AssociatePreloadedFileType(s.ToString(), id);
                        }
                        needtoUpdate = true;
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at UpdateDestination()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Folder Destination Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                return isUpdated;
            }
            return isUpdated;
        }

        /// <summary>
        /// Saves a destination and its custom and preloaded file types
        /// </summary>
        private bool AddDestination()
        {
            bool canContinue = true;

            try
            {
                //Required: Specify Folder Destination
                if (this.txtDestination.Text.Trim().ToString() == String.Empty)
                {
                    MessageBox.Show("Select a folder destination to continue.", "AutoFiler");
                    canContinue = false;
                }

                //Required: Unique Folder Destinations
                if (canContinue == true)
                {
                    foreach (FileType ft in lstFileTypes)
                    {
                        if (ft.Destination == this.txtDestination.Text)
                        {
                            //prevent creation of duplicate folder destinations
                            MessageBox.Show("Select a unique folder destination to continue.", "AutoFiler");
                            canContinue = false;
                        }
                    }
                }

                if (canContinue == true)
                {
                    //save destination path and file types
                    try
                    {
                        //1. add new Destination table to the datasource
                        int newDestinationID = new int().AddNewFolderDestination(this.txtDestination.Text);

                        //2. add new CustomFileTypes row to the Destination table
                        ArrayList arlCustom = GetCustomFileTypes();
                        
                        foreach (string custom in arlCustom)
                        {
                            //save each custom file type and associate them with the destination
                            canContinue = CreateCustomFileType(custom, newDestinationID);
                        }

                        //3. associated each selected file type with the destination
                        ArrayList arlSelections = GetPreloadedFileTypeSelections();
                        foreach (string selection in arlSelections)
                        {
                            canContinue = new bool().AssociatePreloadedFileType(selection, newDestinationID);
                        }
                    }
                    catch (Exception exc)
                    {
                        canContinue = false;
                        throw exc;
                    }
                }
            }
            catch (Exception exc)
            {
                canContinue = false;
                MessageBox.Show("An unexpected error occurred at AddDestination()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - Folder Destination Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return canContinue;
        }

        #endregion

        #region Event Handlers

        private void AddCustom_Click(object sender, RoutedEventArgs e)
        {
            AddCustomFileType();
        }

        private void txtCustom_KeyDown(object sender, KeyEventArgs e)
        {
            // Allow alphanumeric and space.             
            if( e.Key >= Key.A && e.Key <= Key.Z )             
            {                 
                e.Handled = false;             
            }             
            else             
            {                 
                e.Handled = true;             
            }              
            // If tab is presses, then the focus must go to the next control.             
            if( e.Key == Key.Tab )             
            {                 
                e.Handled = false;             
            }
        }

        private void txtDestination_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            result = "Close";
            this.Close();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            RemoveCustomFileType();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (function == "Add")
            {
                doClose = AddDestination();
            }
            else
            {
                doClose = UpdateDestination();
            }
            if (doClose == true)
            {
                result = "Save";
                this.Close();
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Choose a destination folder:";
            fbd.ShowDialog();
            this.txtDestination.Text = fbd.SelectedPath;
        }

        #endregion
    }
}
