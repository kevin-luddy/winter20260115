// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using GenBOE.ActionLogic.ModelView;
using IES.Common;
using IES.Common.OfficeUtilities;

namespace GenBOE.ActionLogic.IO.Import
{
    public class TrainingImporter
    {
        private Logger log = new Logger(typeof(TrainingImporter));

        // Individual column names
        private const string courseColumn = "Course Type / Program Number";
        private const string nameColumn = "Learner Name";
        private const string courseCompleteColumn = "Course P/T Comp Date";
        private const string learnerIdColumn = "Learner ID";
        private const string WORKSHEET_NAME = "Report";
        
        // Array of the columns that must be contained in the imported file
        private readonly string[] requiredColumns = new string[] { courseColumn, nameColumn, courseCompleteColumn, learnerIdColumn };

        /// <summary>
        /// Gets the required columns.
        /// </summary>
        public ICollection<string> RequiredColumns { get { return this.requiredColumns; } }

        /// <summary>
        /// Imports training data from excel file.
        /// </summary>
        /// <param name="excelFileStream">The excel file stream.</param>
        /// <param name="courseIds">The course ids to return.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">excelFileStream</exception>
        /// <exception cref="IES.Common.OfficeUtilities.NotExcelFileException">Imported file was in an incorrect format.</exception>
        public ICollection<TrainingModelView> ImportFromExcelFile(Stream excelFileStream, ICollection<string> courseIds)
        {
            if (excelFileStream == null)
            {
                throw new ArgumentNullException(nameof(excelFileStream));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                try
                {
                    ICollection<TrainingModelView> importResults = new List<TrainingModelView>();
                    // Open the document as read-only.
                    using (SpreadsheetDocument document = SpreadsheetDocument.Open(excelFileStream, false))
                    {
                        // Get a collection of all rows in the file, filtering out rows that only have data in
                        // non import-related columns. Each row is represented as a Key/Value pair Dictionary object
                        // in an enumerable collection

                        List<string> allColumns = new List<string>(this.requiredColumns);

                        // Get the specified worksheet part
                        WorksheetPart worksheetPart = ExcelUtilities.GetSpecifiedWorksheetPart(document, WORKSHEET_NAME);

                        if (worksheetPart == null)
                        {
                            throw new NotExcelFileException();
                        }

                        // Remove the first 2 rows so that ExcelUtilities will find the correct Header Row
                        ExcelUtilities.RemoveFirstRows(worksheetPart, 2);

                        ICollection<Dictionary<string, string>> allRows = ExcelUtilities.GetAllRowsFilteredBySpecifiedHeaders(document, WORKSHEET_NAME, this.requiredColumns, allColumns.ToArray(), null, null, this.requiredColumns);

                        // Turn each row into a DTO object and return the collection
                        importResults = this.CreateImportedTraining(allRows, courseIds);
                    }

                    return importResults;
                }
                catch (FileFormatException)
                {
                    throw new NotExcelFileException("Imported file was in an incorrect format.");
                }
            }
        }

        /// <summary>
        /// Creates the imported training.
        /// </summary>
        /// <param name="allRows">All rows.</param>
        /// <param name="courseIds">The course ids.</param>
        /// <returns>List of training data.</returns>
        private ICollection<TrainingModelView> CreateImportedTraining(ICollection<Dictionary<string, string>> allRows, ICollection<string> courseIds)
        {
            List<TrainingModelView> modelList = new List<TrainingModelView>();
            foreach (Dictionary<string, string> row in allRows)
            {
                string courseId = row[courseColumn];
                if (courseIds.Contains(courseId))
                {
                    double lastCompleted;
                    if (double.TryParse(row[courseCompleteColumn], out lastCompleted))
                    {
                        
                        TrainingModelView model = new TrainingModelView
                        {
                            CourseId = courseId,
                            LastCompleted = DateTime.FromOADate(lastCompleted),
                            UserDisplayName = row[nameColumn],
                            UserId = row[learnerIdColumn]
                        };

                        modelList.Add(model);
                    }
                    else
                    {
                        this.log.Error(string.Format("Date was not parsed correctly when importing training: {0}", row[courseCompleteColumn]));
                    }
                }
            }

            return modelList;
        }
    }
}
