using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace TestFileWatcherService
{
    class Library
    {
        static string folderPathToRead = ConfigurationManager.AppSettings["folderPathToRead"];

        static string folderPathToWrite = ConfigurationManager.AppSettings["folderPathToWrite"];
        static string movedFiles = ConfigurationManager.AppSettings["movedFiles"];

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
                File.Move(inputFileName, movedFiles + @"\" + Path.GetFileName(inputFileName));
            }
                
        }


    }
}

