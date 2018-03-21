namespace TestFileWatcherService
{
    partial class WatcherService
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.WatcherStep1Folder = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.WatcherStep1Folder)).BeginInit();
            // 
            // WatcherStep1Folder
            // 
            this.WatcherStep1Folder.EnableRaisingEvents = true;
            this.WatcherStep1Folder.Created += new System.IO.FileSystemEventHandler(this.Watcher_Created);
            this.WatcherStep1Folder.Deleted += new System.IO.FileSystemEventHandler(this.Watcher_Changed);
            // 
            // WatcherService
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.WatcherStep1Folder)).EndInit();

        }

        #endregion

        private System.IO.FileSystemWatcher WatcherStep1Folder;
    }
}
