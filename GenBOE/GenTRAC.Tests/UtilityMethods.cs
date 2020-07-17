// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Tests
{
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Utility methods for general assistance with test development and execution
    /// </summary>
    public static class UtilityMethods
    {
        /// <summary>
        /// Locate the root path of the Web project directory
        /// </summary>
        /// <param name="webFolderName">Name of the web project folder</param>
        /// <returns>Path of the Web project directory</returns>
        public static string FindWebDirectory(string webFolderName)
        {
            string assemblyDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            DirectoryInfo directory = new DirectoryInfo(assemblyDirectory);

            DirectoryInfo testResultsDirectory = null;

            while ((directory = directory.Parent) != null)
            {
                if (directory.Name == "TestResults" || directory.Name == "GenTRAC")
                {
                    testResultsDirectory = directory;
                    break;
                }
            }

            string webDirectoryPath = null;

            directory = testResultsDirectory;

            while (directory.Parent != null)
            {
                directory = directory.Parent;

                SearchOption searchOption = (directory.Parent == null) ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
                DirectoryInfo[] subdirs = directory.GetDirectories(webFolderName, searchOption);
                DirectoryInfo webDirectory = subdirs.FirstOrDefault(d => d.Name == webFolderName);

                if (webDirectory != null)
                {
                    webDirectoryPath = webDirectory.FullName;
                    break;
                }
            }

            return webDirectoryPath;
        }
    }
}