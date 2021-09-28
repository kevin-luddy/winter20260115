// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.GeneralHelper
{
    using System.Text;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    ///  Creates HTML to return to controller logic classes.
    /// </summary>
    public class HtmlHelper : IHtmlHelper
    {
        /// <summary>
        /// Active Directory Utils
        /// </summary>
        private IActiveDirectoryUtilities adUtils = null;

        /// <summary>
        /// User Mapper
        /// </summary>
        private IUserMapper userMapper = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="userMapper">User Mapper</param>
        public HtmlHelper(
            IActiveDirectoryUtilities adUtils,
            IUserMapper userMapper)
        {
            this.adUtils = adUtils;
            this.userMapper = userMapper;
        }

        /// <summary>
        /// Returns a list of users that belong to a group
        /// </summary>
        /// <param name="groupId">group ID to breakdown</param>
        /// <returns>Alphabetically-ordered List (&lt;ol&gt;) of users</returns>
        public string BreakdownGroup(int groupId)
        {
            StringBuilder toReturn = new StringBuilder();

            var group = this.userMapper.GetById(groupId);

            var users = this.adUtils.GetAdGroupUsers(group.Ntid);

            toReturn.Append("<ul>");

            foreach (var user in users.OrderBy(x => x.DisplayName, SortOrder.Ascending))
            {
                toReturn.Append("<li>");
                toReturn.Append(user.DisplayName);
                toReturn.Append("</li>");
            }

            toReturn.Append("</ul>");

            return toReturn.ToString();
        }
    }
}
