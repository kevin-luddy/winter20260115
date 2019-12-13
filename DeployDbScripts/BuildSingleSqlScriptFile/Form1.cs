namespace BuildSingleSqlScriptFile
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Windows.Forms;

    /// <summary>
    /// This quick app is used to combine all of the script files into a single one, for easier execution.
    /// 
    /// It assumes the following structure:
    /// 
    /// SQL ROOT FOLDER
    ///   -> Object Scripts -> (folders w/ scripts)
    ///   -> Release X.X - blah -> (scripts).. Where X.X is a release number (we care about the "Release " and " - " as termination strings    /// 
    /// </summary>
    public partial class Form1 : Form
    {
        #region Constants

        private const string OBJECT_SCRIPT_FOLDER = "Object Scripts";
        private const string RELEASE_PARENT_FOLDER = "Release Scripts";
        
        private const string RELEASE_FOLDER_PRE = "release ";
        private const string RELEASE_FOLDER_POST = " - ";

        private const string FILE_HEADER = "/*{0}    This file was auto-generated for Release: {1}, on {2}.{0}    It contains all of the "
                        + "Release specific scripts, modifying data/tables as well as all of the Stored Procedures and User Defined Table Types.{0}*/"; // {0} is for new lines..
        private const string FILE_SEPARATOR_TEXT = "{0}{0}/*{0}    File: \\{1}\\{2}{0}*/{0}PRINT '### Starting file: \\{1}\\{2}';{0}{3}"; // {0} is for new lines..

        private const string RESULT_FILE_NAME = "\\Sql Script For Release {0} Generated On {1}.sql";

        #endregion

        public Form1()
        {
            InitializeComponent();

            // Dusan - doing this for myself only, w/ my laptop's location, so I don't have to keep typing it in.. because I'm lazy :|
            if (WindowsIdentity.GetCurrent().Name.ToLower().Equals(@"acct04\paliderd"))
            {
                this.txtFromFolder.Text = @"C:\Projects\dev\GenBOE.Database\Scripts";
                this.txtToFolder.Text = @"C:\Users\paliderd\Desktop";

                this.txtFromFolder_TextChanged(null, null);
                this.txtToFolder_TextChanged(null, null);
            }
        }

        #region Events

        private void btnFromFolder_Click(object sender, EventArgs e)
        {
            this.txtFromFolder.Text = this.SelectFolder();
        }

        private void btnToFolder_Click(object sender, EventArgs e)
        {
            this.txtToFolder.Text = this.SelectFolder();
        }

        private void txtFromFolder_TextChanged(object sender, EventArgs e)
        {
            if (this.ValidateSqlFolder())
            {
                this.PopulateAvailableReleases();
            }
        }

        private void txtToFolder_TextChanged(object sender, EventArgs e)
        {
            this.ValidateDestinationFolder();
        }


        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (this.lbVersions.SelectedItem == null)
            {
                this.lblValidation.Text = "Please select appropriate version.";
                return;
            }

            if (this.ValidateSqlFolder() &&
                this.ValidateDestinationFolder())
            {
                try
                {
                    // prep work
                    DirectoryInfo fromFolder = new DirectoryInfo(this.txtFromFolder.Text);
                    DirectoryInfo folderToRead;
                    string selectedRelease = lbVersions.SelectedItem.ToString();

                    // File header
                    string resultingSqlString = "PRINT '###### SCRIPT IS STARTING ######';" + Environment.NewLine
                        + string.Format(FILE_HEADER, Environment.NewLine, selectedRelease, DateTime.Now.ToShortDateString());

                    // Insert Release files
                    folderToRead = fromFolder.GetDirectories().First(x => x.FullName.Contains(RELEASE_PARENT_FOLDER));
                    folderToRead = folderToRead.GetDirectories().First(x => x.FullName.Contains(selectedRelease));

                    resultingSqlString += this.ReadAllFilesInFolder(folderToRead);

                    // Insert SQL Objects
                    folderToRead = fromFolder.GetDirectories().First(x => x.FullName.Contains(OBJECT_SCRIPT_FOLDER));
                    resultingSqlString += this.ReadAllSqlFilesFromFolderIncludingOneLayerDown(folderToRead);


                    // File footer
                    resultingSqlString += Environment.NewLine + Environment.NewLine + "PRINT '###### SCRIPT FINISHED ######';";

                    // Write the actual file
                    string newFilepath = this.txtToFolder.Text + string.Format(RESULT_FILE_NAME, selectedRelease, DateTime.Now.ToShortDateString().Replace("/", "_"));
                    using (StreamWriter sw = new StreamWriter(newFilepath, false))
                    {
                        sw.Write(resultingSqlString);
                        sw.Flush();
                        sw.Close();
                    }

                    // Final cleanup
                    int numberOfLines = resultingSqlString.Count(x => x == '\n');
                    lblValidation.Text = string.Format("### SUCCESS ###{0}{0}The generated file contains {1} lines.", Environment.NewLine, numberOfLines.ToString("N0"));
                }
                catch (Exception ex)
                {
                    lblValidation.Text = "Error Occured: " + ex.Message;
                }
            }
        }

        #endregion

        #region File Reading

        /// <summary>
        /// Reads the files in child folders.. Used to get all of the Object scripts..
        /// </summary>
        private string ReadAllSqlFilesFromFolderIncludingOneLayerDown(DirectoryInfo folderToRead)
        {
            string result = string.Empty;

            foreach (DirectoryInfo dir in folderToRead.GetDirectories())
            {
                result += this.ReadAllFilesInFolder(dir);
            }

            return result;
        }

        /// <summary>
        /// Reads the files in the specified folders.. Used by the method above, as well as for release scripts..
        /// </summary>
        private string ReadAllFilesInFolder(DirectoryInfo dir)
        {
            string result = string.Empty;

            foreach (FileInfo file in dir.GetFiles().Where(x => x.Extension.ToLower() == ".sql"))
            {
                using (StreamReader sr = file.OpenText())
                {
                    string temp = sr.ReadToEnd();
                    sr.Close();

                    result += string.Format(FILE_SEPARATOR_TEXT, Environment.NewLine, dir.Name, file.Name, temp);
                }
            }

            return result;
        }

        #endregion
        
        #region Validation Helpers

        private bool ValidateSqlFolder()
        {
            if (this.txtFromFolder.Text.Length > 0)
            {
                DirectoryInfo fromFolder = new DirectoryInfo(this.txtFromFolder.Text);
                if (!fromFolder.Exists)
                { this.lblValidation.Text = "SQL Root Folder does not exist."; return false; }

                if (!fromFolder.GetDirectories().Any(x => x.FullName.Contains(OBJECT_SCRIPT_FOLDER))
                    || !fromFolder.GetDirectories().Any(x => x.FullName.Contains(RELEASE_PARENT_FOLDER))
                    )
                { this.lblValidation.Text = "SQL Root Folder does not contain expected folders."; return false; }
            }
            else { this.lblValidation.Text = "Please select the SQL Root Folder.."; return false; }

            return true;
        }

        private bool ValidateDestinationFolder()
        {
            if (this.txtToFolder.Text.Length > 0)
            {
                DirectoryInfo toFolder = new DirectoryInfo(this.txtToFolder.Text);
                if (!toFolder.Exists)
                { this.lblValidation.Text = "Destination Folder does not exist."; return false; }
            } 
            else { this.lblValidation.Text = "Please select the Destination Folder.."; return false; }

            return true;
        }

        #endregion

        #region Other Helpers

        private string SelectFolder()
        {
            this.folderBrowser.ShowDialog();
            string path = this.folderBrowser.SelectedPath;

            return path;
        }

        private void PopulateAvailableReleases()
        {
            DirectoryInfo fromFolder = new DirectoryInfo(this.txtFromFolder.Text);
            DirectoryInfo releaseParentFolder = fromFolder.GetDirectories().First(x => x.FullName.Contains(RELEASE_PARENT_FOLDER));

            lbVersions.Items.Clear();

            foreach (DirectoryInfo dir in releaseParentFolder.GetDirectories().Reverse())
            {
                string folderName = dir.Name.ToLower();

                int startIndex = folderName.LastIndexOf(RELEASE_FOLDER_PRE);
                if (startIndex >= 0)
                {
                    int endIndex = RELEASE_FOLDER_PRE.Length;

                    string versionInfo = folderName.Substring(endIndex);
                    startIndex = endIndex;
                    lbVersions.Items.Add(versionInfo);
                }

                lbVersions.SelectedIndex = 0;
            }
        }

        #endregion
    }
}