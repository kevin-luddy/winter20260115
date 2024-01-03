// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    /// <summary>
    /// RDM base Model View. 
    /// Currently trying to transition ModelView common behaviors to this base class.
    /// This can be used as the base class for anything coming back from the client
    /// that may be dirty.
    /// </summary>
    public class IESModelView
    {
        /// <summary>
        /// Gets/Sets the Admin User flag.
        /// </summary>
        public bool AdminUser { get; set; }

        /// <summary>
        /// Gets/Sets the COBRA Admin User flag.
        /// </summary>
        public bool CobraAdminUser { get; set; }

        /// <summary>
        /// Gets or sets the Dirty flag.
        /// </summary>
        public bool Dirty { get; set; }
    }
}
