namespace APTSPropricerApi.DTOs
{
    using System.Collections.Generic;
    using EBS.ProPricer.Data;
    using EBS.ProPricer.Model;
    using Newtonsoft.Json;

    /// <summary>
    /// Class used to send back data about Batch Reports
    /// </summary>
    public class BatchReportDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonIgnore]
        public EntityId EntityId { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get { return EntityId.ToString(); } }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProposalFolderInfo"/> class.
        /// </summary>
        public BatchReportDto() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchReportDto"/> class.
        /// </summary>
        /// <param name="batchReport">The batch Report.</param>
        public BatchReportDto(BatchReport batchReport)
        {
            this.Name = batchReport.Name;
            this.Category = batchReport.Category.ComponentDescription;
            this.EntityId = batchReport.Id;
            this.Description = batchReport.Description;
        }

        #endregion
    }
}