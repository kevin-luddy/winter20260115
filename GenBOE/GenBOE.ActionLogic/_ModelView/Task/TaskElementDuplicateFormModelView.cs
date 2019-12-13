// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace GenBOE.ActionLogic.ModelView
{
    public class TaskElementDuplicateFormModelView
    {
        /// <summary>
        /// ID related to each Task Element
        /// </summary>
        public int TaskID { get; set; }
        /// <summary>
        /// The number of duplicates to be created for the Task Element
        /// </summary>
        [DisplayName("# Dups")]
        [Range(0, 9, ErrorMessage = "# Dups must be between 0 and 9.")]
        public int DuplicateCount { get; set; }

        /// <summary>
        /// The TaskID string field from User Input
        /// </summary>
        public string BOETaskID { get; set; }

        public string TaskTitle { get; set; }


    }
}
