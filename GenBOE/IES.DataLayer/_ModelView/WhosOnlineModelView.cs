// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;

    /// <summary>
    /// ModelView for Who's Online
    /// </summary>
    public class WhosOnlineModelView
    {
        /// <summary>
        /// ntid of user online
        /// </summary>
        public string Ntid { get; set; }

        /// <summary>
        /// display name of user online
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The date/time the user last accessed the server
        /// </summary>
        public DateTime TimeLastAccessed { get; set; }

        /// <summary>
        /// Formatted string of the time last accessed
        /// </summary>
        public string TimeLastAccessedString
        {
            get { return this.TimeLastAccessed.ToString("M/d/yyyy h:mm:ss tt"); }
        }

        /// <summary>
        /// The time since their last access.
        /// </summary>
        public string TimeSinceLastAccess
        {
            get
            {
                TimeSpan timeSince = DateTime.Now - this.TimeLastAccessed;
                if (timeSince.Days > 0)
                {
                    return "More than 24 hours";
                }
                else
                {
                    return timeSince.ToString(@"h\:mm\:ss");
                }
            }
        }
    }
}
