// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.DateShift
{
    /// <summary>
    /// Email options for Date Adjust
    /// </summary>
    public enum EmailOption
    {
        /// <summary>
        /// No emails sent
        /// </summary>
        None = 0,

        /// <summary>
        /// Email authors when their BOEs or Labor Tasks are date shifted.
        /// </summary>
        EmailBOEAuthors = 1,

        /// <summary>
        /// Email authors when their BOEs or Labor Tasks are date shifted 
        /// AND the operation results in errors that need to be manually corrected.
        /// </summary>
        EmailBOEAuthorsErrorsOnly = 2
    }
}
