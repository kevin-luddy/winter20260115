// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System.Collections.Generic;

	/// <summary>
	/// Section Details for RDSB
	/// </summary>
	public class SectionDetailModelView
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SectionDetailModelView"/> class.
		/// </summary>
		public SectionDetailModelView()
		{
			this.IsRdsbRequired = false;
			this.ChildNodes = new List<SectionDetailModelView>();
			this.SectionContainsCasbDisclosureCore = false;
			this.SectionContainsNonComplianceCore = false;
		}

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the reference number.
		/// </summary>
		public string ReferenceNumber { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this instance has table.
		/// </summary>
		public bool HasTable { get; set; }

		/// <summary>
		/// Gets or sets whether the Section is required for RDSB
		/// </summary>
		public bool IsRdsbRequired { get; set; }

		/// <summary>
		/// Does section contain CASB Disclosure Statements Core
		/// </summary>
		public bool SectionContainsCasbDisclosureCore { get; set; }

		/// <summary>
		/// Does section contain CASB Disclosure Statements Service
		/// </summary>
		public bool SectionContainsCasbDisclosureService { get; set; }

		/// <summary>
		/// Is Disclosure Statement determined to be adequate Core
		/// </summary>
		public bool IsDisclosureStatementAdequateCore { get; set; }

		/// <summary>
		/// Is Disclosure Statement determined to be adequate Service
		/// </summary>
		public bool IsDisclosureStatementAdequateService { get; set; }

		/// <summary>
		/// Does Section contain Non Compliance Issues? Core
		/// </summary>
		public bool SectionContainsNonComplianceCore { get; set; }

		/// <summary>
		/// Does Section contain Non Compliance Issues? Service
		/// </summary>
		public bool SectionContainsNonComplianceService { get; set; }

		/// <summary>
		/// Asks if the user was notified of possible non-compliance with the Disclosure Statement or Cost Account Standards Core
		/// </summary>
		public bool NonComplianceNotificationCore { get; set; }

		/// <summary>
		/// Asks if the user was notified of possible non-compliance with the Disclosure Statement or Cost Account Standards Service
		/// </summary>
		public bool NonComplianceNotificationService { get; set; }

		/// <summary>
		/// Gets or sets the Address Office
		/// </summary>
		public string Office { get; set; }

		/// <summary>
		/// Gets or sets the Address Agency
		/// </summary>
		public string Agency { get; set; }

		/// <summary>
		/// Gets or sets the Address LMBA
		/// </summary>
		public string LMBA { get; set; }

		/// <summary>
		/// Gets or sets the Address Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Address Street
		/// </summary>
		public string Street { get; set; }

		/// <summary>
		/// Gets or sets the Address Office
		/// </summary>
		public string CityST { get; set; }

		/// <summary>
		/// Gets or sets the Address Phone
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// Gets or sets the Address Email
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the Address Other
		/// </summary>
		public string Other { get; set; }

		/// <summary>
		/// Gets or sets the child nodes.
		/// </summary>
		public ICollection<SectionDetailModelView> ChildNodes { get; set; }
	}
}
