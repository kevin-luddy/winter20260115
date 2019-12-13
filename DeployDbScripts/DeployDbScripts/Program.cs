// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2015 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace DeployDbScripts
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Transactions;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    /// <summary>
    /// This is a simple console application that we will use to deploy DB scripts as a part of the Jenkins build
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry Point into the application
        /// </summary>
        /// <param name="args">We are expecting 2 arguments:
        /// The first argument should be the DB connection string
        /// The second argument is the path where the SQL scripts are located; only .sql files will be executed; the files will be ordered by their name;
        /// 
        /// An example of an execution command is:
        /// 
        /// DeployDbScripts "data source=VFASSQL23\ARCHSVCSdev2012;initial catalog=GenBoe;integrated security=True;" "C:\Users\paliderd\Projects"
        /// </param>
        
        static int Main(string[] args)
        {
            // how do we return a code  back to Jenkins to indicate problem/success?
            // can we print to Jenkins?
            try
            {
                if (args.Count() != 2) { Console.WriteLine("ERROR: No DB changes were made => incorrect number of inputs. The application expects 2 - Connection String followed by Folder containing the SQL scripts."); return 1; }

                string connectionString = args.ElementAt(0);
                string path = args.ElementAt(1);

                return ReadSqlFilesFromFolderAndExecuteAgainstDb(connectionString, path);
            }
            catch(Exception ex)
            {
                Console.WriteLine("FATAL: No DB changes were made => Unhandled exception caused termination.");

                string errorMessage = ex.Message;
                if (ex.InnerException != null && ex.InnerException.Message != null)
                {
                    errorMessage += "\n      More Details: " + ex.InnerException.Message;
                }

                Console.WriteLine("   Exception details: " + errorMessage);

                return 1;
            }
        }

        /// <summary>
        /// Reads and executes all files from the folder, against the specified DB
        /// </summary>
        /// <param name="connectionString">Connection String</param>
        /// <param name="path">Path to the folder that contains .sql files to be executed</param>
        private static int ReadSqlFilesFromFolderAndExecuteAgainstDb(string connectionString, string path)
        {
            DirectoryInfo folderContainingSqlScripts = new DirectoryInfo(path);

            if (folderContainingSqlScripts.Exists)
            {
                List<FileInfo> files = folderContainingSqlScripts.GetFiles().Where(x => x.Extension.ToLower() == ".sql").OrderBy(x => x.Name).ToList();

                if (!files.Any()) { Console.WriteLine("WARNING: No matching files found. Aborting execution."); return 0; }

                List<string> fileContents = new List<string>();

                foreach (FileInfo file in files)
                {
                    using (StreamReader sr = file.OpenText())
                    {
                        fileContents.Add(sr.ReadToEnd());
                    }
                }

                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            // Server is being used to allow "GO" commands in the files
                            Server server = new Server(new ServerConnection(conn));

                            foreach (string singleFileContents in fileContents)
                            {
                                server.ConnectionContext.ExecuteNonQuery(singleFileContents);
                            }
                        }

                        scope.Complete();
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine("ERROR: No DB changes were made => DB execution failed. Transaction was rolled back and no DB changes were performed.");

                        string errorMessage = ex.Message;
                        if (ex.InnerException != null && ex.InnerException.Message != null)
                        {
                            errorMessage += "\n      More Details: " + ex.InnerException.Message;
                        }

                        Console.WriteLine("   Exception details: " + errorMessage);

                        return 1; 
                    }
                }
            }
            else { Console.WriteLine("WARNING: No DB changes were made => The specified folder does not exist."); return 0; }

            Console.WriteLine("Success => DB changes were deployed.");
            return 0;
        }
    }
}