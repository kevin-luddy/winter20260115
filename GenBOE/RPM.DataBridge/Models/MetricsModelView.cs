// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace RPM.DataBridge.Models
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The Proposal metrics information, the property names are specifically short because of JSON serialization sizes.
    /// </summary>
    [Serializable]
    public class MetricsModelView
    {
        /// <summary>
        /// Gets or sets the submitted proposal value in millions.
        /// </summary>
        public decimal? SVal { get; set; }

        /// <summary>
        /// Gets or sets the profit value in millions.
        /// </summary>
        public decimal? Profit { get; set; }

        /// <summary>
        /// Gets or sets the Line of Business
        /// </summary>
        public string LOB { get; set; }

        /// <summary>
        /// Gets or sets the program area.
        /// </summary>
        public string PA { get; set; }

        /// <summary>
        /// Gets or sets the type of the contract.
        /// Legal values are CP (Cost Plus), FP (Fixed Price), Hybrid, IDIQ (Indefinite Delivery/Indefinite Quantity), IWTA, T&amp;M (Time &amp; Materials)
        /// </summary>
        public string ContType { get; set; }

        /// <summary>
        /// Gets or sets the Proposal type.
        /// </summary>
        public string PropType { get; set; }

        /// <summary>
        /// Gets or sets the contract leader/PoC.
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// Gets or sets the lead pricer.
        /// </summary>
        public string Pricer { get; set; }

        /// <summary>
        /// Gets or sets the ros.  This will have to be calculated on client :(
        /// </summary>
        public decimal ROS { get; set; }

        /// <summary>
        /// Gets or sets the proposal start date
        /// </summary>
        [IgnoreDataMemberAttribute]
        public DateTime ProposalStartDate { get; set; }

        /// <summary>
        /// Gets or sets the proposal start date
        /// </summary>
        public int Month { get; set; }
    }
}
