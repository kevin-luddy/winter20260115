// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using Newtonsoft.Json;

    /// <summary>
    /// Model View for the Resource Code Replication page in RDM.
    /// </summary>
    public class RateCodeModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Gets or sets from.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is deleted.
        /// </summary>
        [JsonProperty(PropertyName = "Del")]
        public bool IsDeleted { get; set; }
    }
}
