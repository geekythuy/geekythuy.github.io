using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using Renci.SshNet;

namespace TestFileWatcherService
{
    class Library
    {
        static string folderPathToRead = ConfigurationManager.AppSettings["folderPathToRead"];
        static string folderPathToWrite = ConfigurationManager.AppSettings["folderPathToWrite"];
        static string movedFiles = ConfigurationManager.AppSettings["movedFiles"];
        static string hostName = ConfigurationManager.AppSettings["hostName"];
        static string userName = ConfigurationManager.AppSettings["userName"];
        static string uploadfilePath = ConfigurationManager.AppSettings["uploadfilePath"];
        static string OpenSSHKey = ConfigurationManager.AppSettings["OpenSSHKey"];
        static string changeDirectory = ConfigurationManager.AppSettings["changeDirectory"];

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



        public static void UTF8Conversion()
        {
            
            //read and write file from Elementum folder


            foreach (string inputFileName in Directory.GetFiles(folderPathToRead, "*.txt"))
            {
                string outputFile = folderPathToWrite + @"\"+ Path.GetFileName(inputFileName);
                
                //   string outputFile = Directory.GetFiles(outputFilePth).ToString();
                using (StreamReader reader = new StreamReader(inputFileName))
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
                            writer2.WriteLine("File" + inputFileName + "  has been successfully converted to UTF-8");
                        }
                    } 


                }
            } //close foreach file in Directory
        }// close method UTF8Conversion

        public static void cleanDirectory()
        {
            //Move Original Files after writing files
            foreach(string inputFileName in Directory.GetFiles(folderPathToRead, "*.txt"))
            {
                string dupFile = Path.Combine(movedFiles, inputFileName);
                Console.WriteLine(dupFile);
                //check for duplicate file and delete the duplicate first
                if (File.Exists(dupFile))
                {
                    File.Delete(dupFile);
                }
                File.Move(inputFileName, movedFiles + @"\" + Path.GetFileName(inputFileName));
            }
                
        }


        public static void UploadFileSFTP()
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


                        sftpClient.ChangeDirectory(changeDirectory);
                       string currentDir = sftpClient.WorkingDirectory.ToString();
                        WriteErrorLog("Upload Path is: ");
                        WriteErrorLog( uploadfilePath);

                        foreach (string uploadFileName in Directory.GetFiles(uploadfilePath, "*.txt", SearchOption.TopDirectoryOnly))
                        {
                            string destinationPath = currentDir + @"/" + Path.GetFileName(uploadFileName);
                           // Console.WriteLine("Destination Path is: {0}", destinationPath);
                            WriteErrorLog("Destination Path is: ");
                            WriteErrorLog(destinationPath);
                            //delete any duplicate file in remote server
                            if (sftpClient.Exists(destinationPath))
                            {
                                sftpClient.DeleteFile(destinationPath);
                                WriteErrorLog("Deleted duplicate file on SFTP server");
                            }

                            using (FileStream uplFileStream = new FileStream(uploadFileName, FileMode.Open))
                            {
                                WriteErrorLog("Streaming file");
                                WriteErrorLog(uploadFileName);
                                if (uplFileStream != null)
                                {
                                    sftpClient.UploadFile(uplFileStream, destinationPath, null);
                                    WriteErrorLog("successfully uploaded file to SFTP server");
                                }

                            }//end using FileStream

                        }//end foreach 

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
    }
}

