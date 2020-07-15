// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;

    /// <summary>
    /// Model View for Training Classes
    /// </summary>
    public class TrainingModelView
    {

        /// <summary>
        /// Gets or sets the course identifier.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Gets or sets the last completed date for the course.
        /// </summary>
        public DateTime? LastCompleted { get; set; }

        /// <summary>
        /// Gets the last completed date as a string.
        /// </summary>
        public string LastCompletedString
        {
            get
            {
                if (!this.LastCompleted.HasValue)
                {
                    return string.Empty;
                }
                else
                {
                    return this.LastCompleted.Value.ToShortDateString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string UserDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the user's NTID
        /// </summary>
        public string NTID { get; set; }
    }
}
