// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using IES.Common;

    public class TrainingCourseGroupModelView
    {
        /// <summary>
        /// The course name for the group of courses.
        /// </summary>
        public string CourseGroupName { get; set; }

        /// <summary>
        /// The group of valid courses for this Course Grouping.  Users only need to pass one of these courses for the group to be passed.
        /// </summary>
        public ICollection<TrainingCourseModelView> Courses { get; set; }

        /// <summary>
        /// The permission required for this course to be applicable for a user
        /// </summary>
        public Role? PermissionRequired { get; set; }
    }
}
