// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System;

    public class TrainingCourseModelView
    {
        /// <summary>
        /// The ID for the course.
        /// </summary>
        public string CourseID { get; set; }

        /// <summary>
        /// If set, this course if valid until this date.
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        /// <summary>
        /// If set, this course must have been last completed by this date
        /// </summary>
        public DateTime? LastCompletedByDate { get; set; }

        /// <summary>
        /// This course is valid for a user when it has only been taken once
        /// </summary>
        public bool IsValidOnce { get { return !this.LastCompletedByDate.HasValue; } }
    }
}
