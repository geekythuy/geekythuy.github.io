using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using Renci.SshNet;
using System.Diagnostics;

namespace TestFileWatcherService
{
    class Library
    {
        static string folderPathToRead = ConfigurationManager.AppSettings["folderPathToRead"];
        static string folderPathToWrite = ConfigurationManager.AppSettings["folderPathToWrite"];
        static string movedFiles = ConfigurationManager.AppSettings["movedFiles"];
        static string hostName = ConfigurationManager.AppSettings["hostName"];
        static string userName = ConfigurationManager.AppSettings["userName"];
        static string uploadSourceDir = ConfigurationManager.AppSettings["uploadSourceDir"];
        static string OpenSSHKey = ConfigurationManager.AppSettings["OpenSSHKey"];
        static string InventoryDir = ConfigurationManager.AppSettings["InventoryDir"];
        static string ManufactureDir = ConfigurationManager.AppSettings["ManufactureDir"];
        static string TransportDir = ConfigurationManager.AppSettings["TransportDir"];

        public static void WriteErrorLog(Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + " :" + ex.Source.ToString().Trim() + " ;" + ex.Message.ToString().Trim());
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {

            }
        }

        public static void WriteErrorLog(string message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFile.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + " :" + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {

            }

        }

        public static void ErrorLogging(Exception ex)
        {
            string exceptionFilePath = ConfigurationManager.AppSettings["exceptionFilePath"];

            if (!File.Exists(exceptionFilePath))
            {
                File.Create(exceptionFilePath).Dispose();
            }
            using (StreamWriter sw = File.AppendText(exceptionFilePath))
            {
                sw.WriteLine("=============Exception Logging ===========");
                sw.WriteLine("===========Start============= " + DateTime.Now);
                sw.WriteLine("Error Message: " + ex.Message);
                sw.WriteLine("Stack Trace: " + ex.StackTrace);
                sw.WriteLine("===========End============= " + DateTime.Now);

            }
        }



        public static void UTF8Conversion(string inputFilePath)
        {
            
            //read and write file from Elementum folder


           // foreach (string inputFileName in Directory.GetFiles(folderPathToRead, "*.txt"))
          //  {
                string outputFile = folderPathToWrite + @"\"+ Path.GetFileName(inputFilePath);
                
                using (StreamReader reader = new StreamReader(inputFilePath))
                {
                    using (StreamWriter writer = new StreamWriter(outputFile, false, Encoding.UTF8))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();

                            System.Diagnostics.Debug.WriteLine(String.Format("Line read: {0}", line));
                            foreach (var character in line)
                            {
                                if (character != '\0')
                                {
                                    writer.Write(character);
                                }
                            } //close foreach varchar
                            writer.WriteLine();
                        }//close while loop

                        

                        //log the success message 
                        using (StreamWriter writer2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\SuccessMessages.txt", true))
                        {
                            writer2.WriteLine("File" + inputFilePath + "  has been successfully converted to UTF-8");
                        }
                    } 


                }
            //close foreach file in Directory  } 
        }// close method UTF8Conversion


        //Move Original Files after writing files
        public static void cleanDirectory(string inputFilePath)
        {
            
          //  foreach(string inputFileName in Directory.GetFiles(folderPathToRead, "*.txt"))
          //  {
                string dupFile = Path.Combine(movedFiles, Path.GetFileName(inputFilePath));
                Console.WriteLine(dupFile);
                //check for duplicate file and delete the duplicate first
                if (File.Exists(dupFile))
                {
                    File.Delete(dupFile);
                }
                File.Move(inputFilePath, movedFiles + @"\" + Path.GetFileName(inputFilePath));
            //}
                
        }


        public static void UploadFileSFTP(string inputFilePath)
        {
            // Setup Credentials and Server Information
            ConnectionInfo ConnNfo = new ConnectionInfo(hostName, 22, userName,
                new AuthenticationMethod[]{

                // Pasword based Authentication
               // new PasswordAuthenticationMethod("username","password"),

                // Key Based Authentication (using keys in OpenSSH Format)
                new PrivateKeyAuthenticationMethod(userName,new PrivateKeyFile[]{
                    new PrivateKeyFile(OpenSSHKey)
                }),
                }
            );


            //start connecting to SFTP Client and upload files
            try
            {
                // Execute a (SHELL) Command - prepare upload directory
                using (var sftpClient = new SftpClient(ConnNfo))
                {
                    sftpClient.Connect();
                    if (sftpClient.IsConnected)
                    {
                        WriteErrorLog("Successfully connected to Elementum SFTP Server");
                        
                        WriteErrorLog("Upload File Path is: " + folderPathToWrite + @"\" + Path.GetFileName(inputFilePath));


                        // foreach (string uploadFileName in Directory.GetFiles(uploadfilePath, "*.txt", SearchOption.TopDirectoryOnly))
                        //  {
                        if (Path.GetFileName(inputFilePath).Contains("Inventory"))
                        {
                            sftpClient.ChangeDirectory(InventoryDir);
                        }
                        else if (Path.GetFileName(inputFilePath).Contains("ASN"))
                        {
                            sftpClient.ChangeDirectory(TransportDir);
                        }
                        else
                        {
                            sftpClient.ChangeDirectory(ManufactureDir);
                        }

                        string currentDir = sftpClient.WorkingDirectory.ToString();
                            string destinationPath = currentDir + @"/" + Path.GetFileName(inputFilePath);
                            WriteErrorLog("Destination Path is: " + destinationPath);
                            
                            //delete any duplicate file in remote server
                            if (sftpClient.Exists(destinationPath))
                            {
                                sftpClient.DeleteFile(destinationPath);
                                WriteErrorLog("Deleted duplicate file on SFTP server");
                            }
                            //start filestreaming for uploading
                            using (FileStream uplFileStream = new FileStream(folderPathToWrite + @"\" + Path.GetFileName(inputFilePath), FileMode.Open))
                            {
                                WriteErrorLog("Streaming file"+ " " + folderPathToWrite + @"\" + Path.GetFileName(inputFilePath));
                               
                                if (uplFileStream != null)
                                {
                                    sftpClient.UploadFile(uplFileStream, destinationPath, null);
                                    WriteErrorLog("successfully uploaded file to SFTP server");
                                }

                            }//end using FileStream

                            //end foreach    }

                            //Closing connection with SFTP
                            sftpClient.Disconnect();
                        WriteErrorLog("successfully disconnected from SFTP server");
                        //cleanup any lingering connection issues
                        sftpClient.Dispose();
                        WriteErrorLog("successfully disposed any lingering issue");
                    }//end if SFTP client is connected

                }//end using SFTPClient
            }
            catch (Exception exc)
            {
                WriteErrorLog(exc.Message);
            }
        }//end method

        //write eventLogEntry
        public static void writeEventLogError(string sSource, string sEvent)
        {
            // Check if the event source exists. If not create it.
            if (!EventLog.SourceExists(sSource))
            {
                EventLog.CreateEventSource(sSource, "Application");
                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 10);
            }
            else
            {
                EventLog.WriteEntry(sSource, sEvent, EventLogEntryType.Error, 10);
            }
        }// end method

    }
}

