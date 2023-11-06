/*
    Copyright 2016-2020 Lockheed Martin Corporation.

    This computer software has been provided in confidence, and contains trade secret and/or privileged or confidential 
    commercial or financial information. Public disclosure of any information marked as indicated above is prohibited 
    by the Trade Secrets Act (18 U.S.C. Sec. 1905) and the Economic Espionage Act of 1996 (18 U.S.C. Sec. 1831 et seq.) 
    and is not to be made available to third parties without the prior written permission of Lockheed Martin Corporation.
*/

namespace APTSPropricerApi.DTOs
{
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
		[JsonIgnore]
		public EntityId EntityId { get; set; }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		public string Id { get { return EntityId.ToString(); } }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BatchReportDto"/> class.
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