// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;
    using BOEComment = ModelView.BOE.BOEComment;

    [ExcludeFromCodeCoverage]
    public class CommentsAndResponsesExporter 
    {
        /// <summary>
        /// Exports all Comments and Responses within a workspace to an Excel file.
        /// </summary>
        /// <param name="templateFileLocation">The location of the CommentsAndResponses Excel file template</param>
        /// <param name="boeComments">The dictionary of comments keyed by BOE IDs to export</param>
        /// <returns>Path to the exported Comments and Responses file</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public string ExportToExcelFile(string templateFileLocation, IDictionary<int, ICollection<BOEComment>> boeComments)
        {
            // Check inputs
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }
            if (boeComments == null)
            {
                throw new ArgumentNullException(nameof(boeComments));
            }

            var toReturn = string.Empty;

            var worksheet = this.GetExcelExportWorksheet(boeComments);

            // Pass the rows to the generic Excel exporter
            toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, worksheet);            

            // Return the file path
            return toReturn;
        }

        /// <summary>
        /// Gets the excel export worksheet.
        /// </summary>
        /// <param name="boeComments">The dictionary of comments keyed by BOE IDs to export</param>
        /// <returns>An ExcelExportWorksheet</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "ExcelExportWorksheet's base class is List<T> but Collection<T> does not have AddRange()")]
        public ExcelExportWorksheet GetExcelExportWorksheet(IDictionary<int, ICollection<BOEComment>> boeComments)
        {
            if (boeComments == null)
            {
                throw new ArgumentNullException(nameof(boeComments));
            }

            var toReturn = new ExcelExportWorksheet();

            if (boeComments.Count > 0)
            {
                foreach (KeyValuePair<int, ICollection<BOEComment>> entry in boeComments)
                {
                    foreach (BOEComment comment in entry.Value)
                    {
                        toReturn.Add(
                            string.Format($"{entry.Key} {comment.BOETitle}"),
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.ClinString,
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.WbsString,
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.BOEAuthors,
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.ReviewerName,
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.CommenterRole,
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.ReviewerComment,
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.ReviewerCommentUpdateDT + " MST",
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.AuthorName,
                            CommonConstants.FORCE_AS_STRING_VALUE + comment.AuthorResponse,
                            comment.AuthorResponseUpdateDT == null ? null : CommonConstants.FORCE_AS_STRING_VALUE + comment.AuthorResponseUpdateDT + " MST"
                            ); 
                    }
                }
            }

            return toReturn;
        }
    }
}
