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
using System.IO;

namespace AutoFiler
{
    /// <summary>
    /// Interaction logic for winDuplicateFilenamePrompt.xaml
    /// </summary>
    public partial class winDuplicateFilenamePrompt : Window
    {
        public string option = string.Empty;
        FileInfo fileInfo;
        FileType fileType;

        public winDuplicateFilenamePrompt(FileInfo fi, FileType ft)
        {
            InitializeComponent();
            fileInfo = fi;
            fileType = ft;
        }

        private void Ignore_Click(object sender, RoutedEventArgs e)
        {
            option = "Ignore";
            this.Close();
        }

        private void Overwrite_Click(object sender, RoutedEventArgs e)
        {
            option = "Overwrite";
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
        }

        delegate void ShutdownDelegate();

        internal void ClosePrompt()
        {
            ShutdownDelegate d = new ShutdownDelegate(ClosePrompt);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string name = fileInfo.Name;
            int position = name.LastIndexOf('.');
            name = name.Remove(position);

            StringBuilder sb = new StringBuilder();
            sb.Append("A file named ");
            sb.Append("<" + name + ">");
            sb.Append(" already exists in the Destination Folder you've configured to accept files of type ");
            sb.Append("<" + fileType.Extension + ">. ");
            sb.Append("How do you want AutoFiler to handle this duplicate file?");
            txtMessage.Text = sb.ToString();
            lblFileName.Content = name;
            lblFileType.Content = fileType.Extension;
            lblFolder.Content = fileType.Destination;
        }
    }
}
