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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Collections;
using System.Threading;

namespace AutoFiler
{
    /// <summary>
    /// Interaction logic for winFolderDestinationManager.xaml
    /// </summary>
    public partial class winFolderDestinationManager : Window
    {
        public AutoFilerService AFS = new AutoFilerService();
        private WindowState m_storedWindowState = WindowState.Normal;
        private System.Windows.Forms.ContextMenuStrip ctxTrayMenu;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        public bool foldersReloading = false;

        public winFolderDestinationManager()
        {
            InitializeComponent();

            m_notifyIcon = new System.Windows.Forms.NotifyIcon();

            //Create a object for the context menu
            ctxTrayMenu = new System.Windows.Forms.ContextMenuStrip();

            //Add the Menu Item to the context menu
            System.Windows.Forms.ToolStripMenuItem mnuManage = new System.Windows.Forms.ToolStripMenuItem();
            mnuManage.Text = "Manage AutoFiler";
            mnuManage.Click += new EventHandler(mnuManage_Click);
            ctxTrayMenu.Items.Add(mnuManage);

            System.Windows.Forms.ToolStripMenuItem mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            mnuAbout.Text = "About AutoFiler";
            mnuAbout.Click += new EventHandler(mnuAbout_Click);
            ctxTrayMenu.Items.Add(mnuAbout);

            System.Windows.Forms.ToolStripMenuItem mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            mnuExit.Text = "Exit AutoFiler";
            mnuExit.Click += new EventHandler(mnuExit_Click);
            ctxTrayMenu.Items.Add(mnuExit);

            //Add the Context menu to the Notify Icon Object
            m_notifyIcon.ContextMenuStrip = ctxTrayMenu;
        }

#region Notificaiton Functionality
        void OnClose(object sender, CancelEventArgs args)
        {
            m_notifyIcon.Dispose();
            m_notifyIcon = null;
        }

        void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                if (m_notifyIcon != null)
                    m_notifyIcon.ShowBalloonTip(2000);
            }
            else
            {
                m_storedWindowState = WindowState;
            }
        }

        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            //Show();
            //WindowState = m_storedWindowState;
        }

        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }

        void mnuAbout_Click(object sender, EventArgs e)
        {
            winAbout about = new winAbout();
            about.ShowDialog();
        }

        void mnuManage_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }

        void mnuExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
#endregion

#region StartUp / ShutDown
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AFS.CreateDropFolder();
            m_notifyIcon.BalloonTipText = "The AutoFiler service has been minimized, but is still running.";
            m_notifyIcon.BalloonTipTitle = "AutoFiler";
            m_notifyIcon.Text = "AutoFiler";
            m_notifyIcon.Icon = new System.Drawing.Icon("shell32_4.ico");
            m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
            LoadDestinationFolders();
            AFS.StartDirectoryWatcher();

            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Loads saved destination folders
        /// </summary>
        private void LoadDestinationFolders()
        {
            try
            {
                this.folders.Items.Clear();

                DataSet dsFolders = new DataSet().GetAllFolderDestinations();

                foreach (DataRow dr in dsFolders.Tables[0].Rows)
                {
                    this.folders.Items.Add(dr["FolderDestination"].ToString());
                }

                if (this.folders.Items.Count > 0)
                {
                    this.mnuEditDestination.IsEnabled = true;
                    this.mnuDeleteDestination.IsEnabled = true;
                }
                else
                {
                    this.mnuEditDestination.IsEnabled = false;
                    this.mnuDeleteDestination.IsEnabled = false;
                }

                foldersReloading = false;
                if (this.folders.Items.Count > 0) { this.folders.SelectedIndex = 0; }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at LoadDestinationFolders()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - File Type Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Displays file types associated with the selected folder destination
        /// </summary>
        private void ShowAssociatedFileTypes()
        {
            try
            {
                if (foldersReloading == false)
                {
                    string load = this.folders.SelectedItem.ToString();
                    DataSet dsFileTypes = new DataSet().GetPreloadedFileTypes(load);
                    this.files.Items.Clear();

                    // display associated preloaded file types
                    try
                    {
                        if (dsFileTypes.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in dsFileTypes.Tables[0].Rows)
                            {
                                this.files.Items.Add(dr["FileTypeName"].ToString());
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        throw exc;
                    }

                    // display associated custom file types
                    try
                    {
                        dsFileTypes = new DataSet().GetCustomFileTypes(load);
                        foreach (DataRow dr in dsFileTypes.Tables[0].Rows)
                        {
                            this.files.Items.Add(dr["FileTypeName"].ToString() + " (custom)");
                        }
                    }
                    catch (Exception exc)
                    {
                        throw exc;
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at ShowAssociatedFileTypes()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - File Type Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ShowAssociatedFileNameText()
        {
            try
            {
                if (foldersReloading == false)
                {
                    string load = this.folders.SelectedItem.ToString();
                    DataSet dsFileNames = new DataSet().GetFileNames(load);

                    // display associated file names
                    try
                    {
                        if (dsFileNames.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in dsFileNames.Tables[0].Rows)
                            {
                                string filename = dr["FileName"].ToString();
                                string option;
                                string over_ride = dr["Override"].ToString();
                                string destination = dr["FolderDestination"].ToString();

                                if(dr["FileOption"].ToString().Equals("B"))
                                {
                                    option = "begins with";
                                }
                                else if (dr["FileOption"].ToString().Equals("C"))
                                {
                                    option = "contains";
                                }
                                else
                                {
                                    option = "ends with";
                                }

                                if (dr["Override"].ToString() == "True")
                                {
                                    over_ride = "true";
                                }
                                else
                                {
                                    over_ride = "false";
                                }

                                string data = filename + " (Option: " + option + " | Overrides: " + over_ride + ")";
                                this.files.Items.Add(data);
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        throw exc;
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at ShowAssociatedFileNameText()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - File Type Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        #endregion

#region Event Handlers
        private void folders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowAssociatedFileTypes();
            ShowAssociatedFileNameText();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.WindowState = WindowState.Minimized;
        }

        private void mnuRunAutoFiler_Click(object sender, RoutedEventArgs e)
        {
            //force the service to run
            AFS.TriggerAutoFiler();
        }

        private void mnuUnmanaged_Click(object sender, RoutedEventArgs e)
        {
            UnrecognizedFileTypes winFileTypes = new UnrecognizedFileTypes(false, AFS.PromptToManageThread, AFS);
            winFileTypes.ShowDialog();
        }

        private void mnuConfigure_Click(object sender, RoutedEventArgs e)
        {
            winConfigure config = new winConfigure();
            config.ShowDialog();
        }

        private void mnuNewDestination_Click(object sender, RoutedEventArgs e)
        {
            CreateNewFolderDestination();
        }

        private void mnuEditDestination_Click(object sender, RoutedEventArgs e)
        {
            EditFolderDestination();
        }

        private void mnuDeleteDestination_Click(object sender, RoutedEventArgs e)
        {
            //determines if a folder destination can be deleted and deletes if possible.
            DeleteFolderDestination();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            winAbout about = new winAbout();
            about.ShowDialog();
        }
#endregion

#region Add/Edit/Delete Buttons

        /// <summary>
        /// Creates a new folder destination
        /// </summary>
        private void CreateNewFolderDestination()
        {
            winAddEditFolderDestinations winDef = new winAddEditFolderDestinations("Add", "", this.AFS);
            winDef.ShowDialog();
            if (winDef.result == "Save")
            {
                foldersReloading = true;
                LoadDestinationFolders();
                foldersReloading = false;
                if (this.folders.Items.Count > 0) { this.folders.SelectedIndex = 0; }
            }
        }

        /// <summary>
        /// Updates the current folder destination.
        /// </summary>
        private void EditFolderDestination()
        {
            winAddEditFolderDestinations winDef = new winAddEditFolderDestinations("Edit", this.folders.SelectedItem.ToString(), this.AFS);
            winDef.ShowDialog();
            if (winDef.result == "Save")
            {
                foldersReloading = true;
                LoadDestinationFolders();
                foldersReloading = false;
                if (this.folders.Items.Count > 0) { this.folders.SelectedIndex = 0; }
            }
        }

        /// <summary>
        /// Removes/Deletes file type associates prior to deleting the folder destination
        /// </summary>
        private void DeleteFolderDestination()
        {
            try
            {
                MessageBoxResult results = MessageBox.Show("Are you sure you want to delete this folder destination and remove all file type associations?", "AutoFiler", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (results == MessageBoxResult.Yes)
                {
                    DataSet dsFileTypes = new DataSet();
                    bool isRemovedOrDeleted = false;

                    // Delete custom file types
                    dsFileTypes = dsFileTypes.GetCustomFileTypes(this.folders.SelectedItem.ToString());
                    foreach (DataRow dr in dsFileTypes.Tables[0].Rows)
                    {
                        isRemovedOrDeleted = new bool().RemoveCustomFileType(dr["FileTypeName"].ToString());
                    }

                    // Remove associations to preload file types
                    dsFileTypes = dsFileTypes.GetPreloadedFileTypes(this.folders.SelectedItem.ToString());
                    foreach (DataRow dr in dsFileTypes.Tables[0].Rows)
                    {
                        isRemovedOrDeleted = new bool().RemovePreloadedFileAssociation(dr["FileTypeName"].ToString());
                    }

                    // Delete destination folder
                    isRemovedOrDeleted = new bool().DeleteFolderDestination(this.folders.SelectedItem.ToString());
   
                    // Update available folder destinations list
                    foldersReloading = true;
                    this.folders.Items.Remove(this.folders.SelectedItem);

                    foldersReloading = false;
                    if (this.folders.Items.Count > 0)
                    {
                        this.folders.SelectedIndex = 0;
                        this.mnuEditDestination.IsEnabled = true;
                        this.mnuDeleteDestination.IsEnabled = true;
                    }
                    else
                    {
                        this.files.Items.Clear();
                        this.mnuEditDestination.IsEnabled = false;
                        this.mnuDeleteDestination.IsEnabled = false;
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("An unexpected error occurred at PrepareToDelete()" +
                    Environment.NewLine + Environment.NewLine + exc.ToString(),
                    "AutoFiler - File Type Manager", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void mnuAdvancedOptions_Click(object sender, RoutedEventArgs e)
        {
            winAdvancedOptions winAO = new winAdvancedOptions(this.AFS);
            winAO.ShowDialog();
            if (winAO.updated == true)
            {
                try
                {
                    LoadDestinationFolders();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }       
}
