using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using IES.Common;
using GenBOE.ActionLogic.ModelView;
using GenBOE.Dtos;
using System;

namespace GenBOE.Web.ModelView
{
    /// <summary>
    /// View model for Whos Online users.  Contains paging support.
    /// </summary>
    public class WhosOnlineGridModelView : PagedResultsModelView<string>
    {
        /// <summary>
        /// Paramater less constructor required by serialization of the object from the
        /// GenBOEMetricsWhosOnline page.  
        /// </summary>
        public WhosOnlineGridModelView()
        {
        }

        /// <summary>
        /// Constructor used when building the initial Whos Online object.
        /// </summary>
        /// <param name="usersOnlineDetails">Detail descriptor of online users.</param>
        public WhosOnlineGridModelView(GenBOEUsersOnlineDTO usersOnlineDetails)
        {
            this.CurrentPage = 1;
            this.ResultsPerPage = 15;
            this.UserResults = new Collection<UserOnlineDetails>();
            this.PagedIndexes = new Collection<string>();
            if (usersOnlineDetails != null && usersOnlineDetails.UserOnlineDetailsCollection != null)
            {
                // Sort by User display name.
                usersOnlineDetails.UserOnlineDetailsCollection = usersOnlineDetails.UserOnlineDetailsCollection.OrderByDescending(x => x.TimeLastAccessed).ToCollection();

                foreach (UserOnlineDetails user in usersOnlineDetails.UserOnlineDetailsCollection)
                {
                    TimeSpan timeSinceLast = DateTime.Now - user.TimeLastAccessed;

                    if (timeSinceLast.Days < 1) { user.TimeSinceLastAccess = timeSinceLast.ToString("hh\\:mm\\:ss"); }
                    else { user.TimeSinceLastAccess = "More than 24 hours"; }
                }
                this.UsersOnlineDetails = usersOnlineDetails;
                // Pages are indexed by the users unique NT Id.
                this.PagedIndexes = new Collection<string>(UsersOnlineDetails.UserOnlineDetailsCollection.Select(userAccount => userAccount.Ntid).ToArray());
                // 1st page results.
                this.UserResults = UsersOnlineDetails.UserOnlineDetailsCollection.Take(ResultsPerPage).ToArray();
            }            
        }

        /// <summary>
        /// List of users for the current page being viewed. 
        /// </summary>
        public ICollection<UserOnlineDetails> UserResults { get; set; }

        /// <summary>
        /// All online users.
        /// </summary>
        public GenBOEUsersOnlineDTO UsersOnlineDetails { get; set; }
    }
}