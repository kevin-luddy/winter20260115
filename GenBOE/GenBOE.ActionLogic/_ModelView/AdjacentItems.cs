// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    /// <summary>
    /// The adjacent ids of items adjacent to a searched item.
    /// </summary>
    public class AdjacentItems
    {
        /// <summary>
        /// The Id of the previous item, or null if the searched item was the first item.
        /// </summary>
        public int? PreviousId { get; set; }

        /// <summary>
        /// The id of the next item, or null if the searched item was the last item.
        /// </summary>
        public int? NextId { get; set; }
    }
}
