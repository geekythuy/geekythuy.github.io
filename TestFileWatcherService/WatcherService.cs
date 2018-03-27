using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TestFileWatcherService
{
    public partial class WatcherService : ServiceBase
    {
        string folderPathToRead = ConfigurationManager.AppSettings["folderPathToRead"];
        
        //Declare global FileSystemWatcher
        FileSystemWatcher watcher;
        //Constructor of WatchService class
        public WatcherService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            watcher = new FileSystemWatcher(folderPathToRead);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.Deleted += Watcher_Deleted;
            
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Library.WriteErrorLog(e.FullPath);
            Library.WriteErrorLog(" was successfully deleted");
            try
            {
                Library.UploadFileSFTP();
            }
           catch(Exception ex)
            {
                Library.WriteErrorLog(ex);
            }

        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Library.WriteErrorLog("File was successfully changed");
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Library.WriteErrorLog(e.FullPath);
            Library.WriteErrorLog("was successfully created");
            try
            {
                Library.UTF8Conversion();
                Library.cleanDirectory();
            }
            catch(Exception ex)
            {
                Library.WriteErrorLog(ex);
            }
        }

        protected override void OnStop()
        {
        }

       
    }
}
