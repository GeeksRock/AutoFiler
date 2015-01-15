using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Data;
using System.Threading;

namespace AutoFiler
{
    public class AutoFilerService
    {
        public List<FileType> listUnmanagedCustomFileTypes { get; set; }
        public List<FileType> listUnmanagedPreloadFileTypes { get; set; }
        public List<FileType> listUnmanagedFileTypes { get; set; }
        public List<FileType> listAllManagedFileTypes { get; set; }
        public FileSystemWatcher fsw;
        public Thread PromptToManageThread;
        Thread AutoFileThread;
        FileInfo promptFI;
        FileType promptUFT;
        int filingNow;//used to force the Unmanaged File Type screen from displaying more than once per execution of the AutoFiler service.

        /// <summary>
        /// 
        /// </summary>
        public AutoFilerService()
        {
            this.listUnmanagedCustomFileTypes = new List<FileType>();
            this.listUnmanagedPreloadFileTypes = new List<FileType>();
            this.listUnmanagedFileTypes = new List<FileType>();
            TriggerAutoFiler();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConfigureUnmanagedFileTypes()
        {
            if (filingNow == 0)
            {
                filingNow++;
                PromptToManageThread = new Thread(new ThreadStart(PromptToConfigureUnmanagedFileTypes));
                PromptToManageThread.SetApartmentState(ApartmentState.STA);
                if (!PromptToManageThread.IsAlive)
                {
                    PromptToManageThread.Start();
                }
                else
                {
                    PromptToManageThread.Abort();
                }
            }
        }

        /// <summary>
        /// Prompts the user to manage unmanaged file types
        /// </summary>
        private void PromptToConfigureUnmanagedFileTypes()
        {
            if (PromptToManageThread.IsAlive)
            {
                if (this.listUnmanagedFileTypes.Count > 0)
                {
                    UnrecognizedFileTypes uft = new UnrecognizedFileTypes(true, PromptToManageThread, this);
                    uft.ShowDialog();
                    if (PromptToManageThread.IsAlive)
                    {
                        uft.ClosePrompt();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateDropFolder()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string folder = "AutoFiler Drop Folder";
                if (!Directory.Exists(path + "\\" + folder))
                {
                    Directory.CreateDirectory(path + "\\" + folder);
                    //File.SetAttributes(folder, File.GetAttributes
                    //      (folder | FileAttributes.System));
                    if ((File.GetAttributes(path + "\\" + folder) & FileAttributes.System) != FileAttributes.System)
                    {
                        File.SetAttributes(path + "\\" + folder, File.GetAttributes
                                          (path + "\\" + folder) | FileAttributes.System);
                    }
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetAllManagedFileTypes()
        {
            this.listAllManagedFileTypes = new List<FileType>();
            DataSet dsGetPreloaded = new DataSet().GetAllPreloadedFileTypes();
            DataSet dsGetCustom = new DataSet().GetAllCustomFileTypes();

            foreach (DataRow drPreloaded in dsGetPreloaded.Tables[0].Rows)
            {
                if (drPreloaded["FolderDestination"].ToString().Length > 0)
                {
                    FileType f = new FileType
                    {
                        Destination = drPreloaded["FolderDestination"].ToString(),
                        Extension = drPreloaded["FileTypeName"].ToString(),
                        ManageOption = String.Empty,
                        Type = "Preload"
                    };
                    this.listAllManagedFileTypes.Add(f);
                }
            }
            foreach (DataRow drCustom in dsGetCustom.Tables[0].Rows)
            {
                FileType f = new FileType
                {
                    Destination = drCustom["FolderDestination"].ToString(),
                    Extension = drCustom["FileTypeName"].ToString(),
                    ManageOption = String.Empty,
                    Type = "Custom"
                };
                this.listAllManagedFileTypes.Add(f);
            }
        }

        //http://www.codeproject.com/Articles/9331/Create-Icons-for-Folders-in-Windows-Explorer-Using
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iconFilePath"></param>
        /// <param name="infoTip"></param>
        private void CreateFolderIcon(string iconFilePath, string infoTip)
        {
            //if (CreateFolder())
            //{
            //    CreateDesktopIniFile(iconFilePath, infoTip);
            //    SetIniFileAttributes();
            //    SetFolderAttributes();
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Changed)
            {
                //Initialize list of all managed file types
                GetAllManagedFileTypes();

                //prompt to configure unmanaged filetypes
                if (Properties.Settings.Default.UnmanagedFileType == "AutoCheck")
                {
                    //sort the unmanaged file types: custom and preload
                    FindAndSortUniqueFileTypes();

                    if (this.listUnmanagedFileTypes.Count > 0 && filingNow == 0)
                    {
                        ConfigureUnmanagedFileTypes();
                    }
                    else
                    {
                        filingNow = 0;
                    }
                }
                //move managed files to their appropriate folder destinations
                AutoFile();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void FindAndSortUniqueFileTypes()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AutoFiler Drop Folder";
                DirectoryInfo dirDropFolder = new DirectoryInfo(path);
                FileInfo[] filDroppedFile = dirDropFolder.GetFiles();

                //find unique unmanaged file types
                foreach (FileInfo fi in filDroppedFile)
                {
                    //ignore managed file types
                    var extInList = from FileType f in this.listAllManagedFileTypes
                                 where f.Extension.Equals(fi.Extension.Remove(0, 1).ToString())
                                 select f;

                    if (extInList.Count() == 0)
                    {
                        var inList = from FileType u in this.listUnmanagedFileTypes
                                 where u.Extension.Equals(fi.Extension.Remove(0, 1).ToString()) 
                                 && !u.Extension.Equals("autofiler")
                                 select u.Extension;

                        //don't add duplicates to the list
                        if (inList.Count() == 0)
                        {
                            FileType uFT = new FileType
                            {
                                Extension = fi.Extension.Remove(0, 1).ToString(),
                                ManageOption = "",
                                Type = "",
                                Destination = ""
                            };

                            //ignore trigger.autofiler
                            if (!uFT.Extension.Equals("autofiler"))
                            {
                                this.listUnmanagedFileTypes.Add(uFT);
                            }
                        }
                    }
                }

                //get a list of all preload Extensions
                DataSet dsPreload = new DataSet().GetAllPreloadedFileTypes();
                ArrayList arPreload = new ArrayList();
                foreach (DataRow drPreload in dsPreload.Tables[0].Rows)
                {
                    arPreload.Add(drPreload["FileTypeName"].ToString());
                }

                //if the unmanaged file type does not exist in the list of all preloads, it must be custom.
                //sort unmanaged file types: preload vs custom
                foreach (FileType ft in listUnmanagedFileTypes)
                {
                    if (!arPreload.Contains(ft.Extension))
                    {
                        ft.Type = "Custom";
                        this.listUnmanagedCustomFileTypes.Add(ft);
                    }
                    else
                    {
                        ft.Type = "Preload";
                        this.listUnmanagedPreloadFileTypes.Add(ft);
                    }
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartDirectoryWatcher()
        {
            //http://www.codeproject.com/KB/cs/FileDirChangeNotifier.aspx
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folder = path + "\\" + "AutoFiler Drop Folder";
            fsw = new FileSystemWatcher(folder);
            fsw.Filter = "*.*";
            fsw.Created += new FileSystemEventHandler(fsw_Changed);
            fsw.EnableRaisingEvents = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="uft"></param>
        public void OverwriteDuplicateFilename(FileInfo fi, FileType uft)
        {
            try
            {
                fi.CopyTo(uft.Destination + "\\" + fi.Name, true);
                fi.Delete();
            }
            catch (IOException ioExc)
            {
                throw ioExc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="uft"></param>
        public void IgnoreDuplicateFilename(FileInfo fi, FileType uft)
        {
            int x = 2;
            string name = fi.Name;
            int position = name.LastIndexOf('.');
            name = name.Remove(position);

            while (File.Exists(uft.Destination + "\\" + name + " (" + x.ToString() + ")" + fi.Extension))
            {
                x++;
            }

            if (!File.Exists(uft.Destination + "\\" + name + " (" + x.ToString() + ")" + fi.Extension))
            {
                try
                {
                    fi.CopyTo(uft.Destination + "\\" + name + " (" + x.ToString() + ")" + fi.Extension);
                    fi.Delete();
                }
                catch (IOException ioExc)
                {
                    throw ioExc;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Moves files with custom file types to the specified location
        /// </summary>
        private void AutoFile()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\AutoFiler Drop Folder";
                DirectoryInfo dirDropFolder = new DirectoryInfo(path);
                FileInfo[] filDroppedFile = dirDropFolder.GetFiles();

                foreach (FileInfo fi in filDroppedFile)
                {
                    bool moved = false;

                    foreach (FileType mft in this.listAllManagedFileTypes)
                    {
                        if (fi.Extension.Remove(0, 1).ToString() == mft.Extension && moved == false)
                        {
                            //grab the destination name using its ID value
                            string idName = String.Empty;
                            DataSet dsDestination = new DataSet().GetDestinationNameById(Convert.ToInt16(mft.Destination));
                            foreach (DataRow dr in dsDestination.Tables[0].Rows)
                            {
                                mft.Destination = dr["FolderDestination"].ToString();
                            }
                            if (fi.Extension.Remove(0, 1).Equals("autofiler"))
                            {
                                try
                                {
                                    fi.Delete();
                                    moved = true;
                                }
                                catch (IOException ioExc)
                                {
                                    throw ioExc;
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                            else
                            {
                                if (!File.Exists(mft.Destination + "\\" + fi.Name))
                                {
                                    try
                                    {
                                        fi.CopyTo(mft.Destination + "\\" + fi.Name);
                                        fi.Delete();
                                        moved = true;
                                    }
                                    catch (IOException ioExc)
                                    {
                                        throw ioExc;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                                else
                                {
                                    switch (Properties.Settings.Default.DuplicateFilenames)
                                    {
                                        case "Ignore":
                                            IgnoreDuplicateFilename(fi, mft);
                                            break;
                                        case "Overwrite":
                                            OverwriteDuplicateFilename(fi, mft);
                                            break;
                                        case "Prompt":
                                            promptFI = fi;
                                            promptUFT = mft;
                                            AutoFileThread = new Thread(new ThreadStart(PromptToHandleDuplicateFilename));
                                            AutoFileThread.SetApartmentState(ApartmentState.STA);
                                            if (!AutoFileThread.IsAlive)
                                            {
                                                AutoFileThread.Start();
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    moved = false;
                }                        
            }
            catch (IOException ioExc)
            {
                if (!ioExc.ToString().Contains("it is being used by another process"))
                {
                    throw ioExc;
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }

        /// <summary>
        /// Allows the user to determine if a duplicate file should be overwritten or ignored
        /// </summary>
        private void PromptToHandleDuplicateFilename()
        {
            winDuplicateFilenamePrompt winPrompt = new winDuplicateFilenamePrompt(promptFI, promptUFT);
            winPrompt.ShowDialog();
            string option = winPrompt.option;

            if (option == "Ignore")
            {
                IgnoreDuplicateFilename(promptFI, promptUFT);
                if (AutoFileThread.IsAlive)
                {
                    winPrompt.ClosePrompt();
                }
            }
            else
            {
                OverwriteDuplicateFilename(promptFI, promptUFT);
                if (AutoFileThread.IsAlive)
                {
                    winPrompt.ClosePrompt();
                }
            }
        }

        /// <summary>
        /// Forces the AutoFiler service to execute on demand.
        /// </summary>
        public void TriggerAutoFiler()
        {
            //The service executes any time a file is added to the drop folder. 
            //To force the service to execute without waiting for the user to drop a file: 
            //  1. Create a special file in the drop folder, if it doesn't already exist.
            //  2. If it does exist, delete it, then recreate it. 
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string folder = "AutoFiler Drop Folder";
                string triggerFile = "trigger.autofiler";

                FileInfo fi = new FileInfo(path + "\\" + folder + "\\" + triggerFile);
                if (!File.Exists(fi.FullName))
                {
                    try
                    {
                        fi.Create();
                    }
                    catch (IOException ioExc)
                    {
                        throw ioExc;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else
                {
                    try
                    {
                        fi.Delete();
                        fi.Create();
                    }
                    catch (IOException ioExc)
                    {
                        throw ioExc;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (IOException ioEx)
            {
                throw ioEx;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
}
