// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenTRAC.ActionLogic.GeneralHelper
{
    /// <summary>
    /// Creates HTML to return to controller logic classes.
    /// </summary>
    public interface IHtmlHelper
    {
        /// <summary>
        /// Returns a list of users that belong to a group
        /// </summary>
        /// <param name="groupId">group ID to breakdown</param>
        /// <returns>Alphabetically-ordered List (&lt;ol&gt;) of users</returns>
        string BreakdownGroup(int groupId);
    }
}
