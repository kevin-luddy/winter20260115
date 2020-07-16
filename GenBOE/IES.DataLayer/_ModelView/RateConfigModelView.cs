// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using IES.Common;

    /// <summary>
    /// The Model View used for Rate Details.
    /// </summary>
    public class RateConfigModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateConfigModelView"/> class.
        /// </summary>
        public RateConfigModelView()
        {
            this.Id = -1;
        }

        /// <summary>
        /// Gets or sets the unique id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the rate target, i.e. PPRD or Rate.
        /// </summary>
        public string RateTarget { get; set; }
        
        /// <summary>
        /// Gets or sets the rate category.
        /// </summary>
        public RateCategory? RateCategory { get; set; }

        /// <summary>
        /// Gets or sets the prefix, i.e. '$'.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the suffix, i.e. '%'.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the precision, i.e. number of digits after decimal.
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// Gets or sets the mulitiplier, i.e. 100 for percentage amounts.
        /// </summary>
        public int? Multiplier { get; set; }
    }
}
