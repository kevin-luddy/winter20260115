// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using IES.Common.Core.Enums;

	/// <summary>
	/// The Model for a section in a document.
	/// </summary>
	public class SectionModelView : IESUpdateableModelView
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SectionModelView"/> class.
		/// </summary>
		public SectionModelView()
		{
			this.IsInternalSection = false;
			this.DisplayRateCode = false;
			this.TextContent = string.Empty;
			this.ChildNodes = new List<SectionModelView>();
			this.OldId = null;
			this.OldUpdateDate = null;
			this.RevisionUniqueSectionId = -1;
			this.IsRdsbRequired = false;
			this.SectionContainsCasbDisclosure = false;
			this.IsDisclosureStatementAdequate = false;
			this.SectionContainsNonCompliance = false;
			this.NonComplianceNotification = false;
			this.Office = string.Empty;
			this.Agency = string.Empty;
			this.LMBA = string.Empty;
			this.Name = string.Empty;
			this.Street = string.Empty;
			this.CityST = string.Empty;
			this.Phone = string.Empty;
			this.Email = string.Empty;
			this.Other = string.Empty;
			this.IncludeInCoversheet = false;
		}

		/// <summary>
		/// Constructor with parameters.
		/// </summary>
		/// <param name="revisionId">Revision Id</param>
		/// <param name="displayOrder">Display Order</param>
		/// <param name="isInternalSection">Is Internal Section</param>
		/// <param name="title">Title</param>
		/// <param name="textContent">Text Content</param>
		/// <param name="displayRateCode">Display Rate Code</param>
		/// <param name="contentType">Content Type</param>
		/// <param name="referenceNumber">Reference Number</param>
		/// <param name="revisionUniqueSectionId">Section Identifier unique to the version</param>
		/// <param name="sectionContainsCasbDisclosure">Section contains CASB Disclosure Statements</param>
		/// <param name="sectionContainsNonCompliance">Section contains Non Compliance issues</param>
		/// <param name="office">Office address</param>
		/// <param name="agency">agency address</param>
		/// <param name="LMBA">LMBA address content type</param>
		/// <param name="name">Name address content type</param>
		/// <param name="street">Street address content type</param>
		/// <param name="cityST">city state address content type</param>
		/// <param name="phone">Phone address content type</param>
		/// <param name="email">email address content type</param>
		/// <param name="other">other address content type</param>
		/// <param name="includeInCoversheet">includeInCoversheet address content type</param>
		public SectionModelView(int revisionId, int displayOrder, bool? isInternalSection, string title,
			string textContent, bool? displayRateCode, SectionContentType contentType, string referenceNumber, int revisionUniqueSectionId, bool sectionContainsCasbDisclosure, Nullable<bool> isDisclosureStatementAdequate, bool sectionContainsNonCompliance,
			Nullable<bool> nonComplianceNotification, string office, string agency, string LMBA, string name, string street, string cityST, string phone, string email, string other, bool? includeInCoversheet)
		{
			this.RevisionId = revisionId;
			this.DisplayOrder = displayOrder;
			this.IsInternalSection = isInternalSection;
			this.Title = title;
			this.TextContent = textContent;
			this.DisplayRateCode = displayRateCode;
			this.ContentType = contentType;
			this.ReferenceNumber = referenceNumber;
			this.RevisionUniqueSectionId = revisionUniqueSectionId;
			this.ChildNodes = new List<SectionModelView>();
			this.OldId = null;
			this.OldUpdateDate = null;
			this.SectionContainsCasbDisclosure = sectionContainsCasbDisclosure;
			this.IsDisclosureStatementAdequate = isDisclosureStatementAdequate.HasValue ? isDisclosureStatementAdequate.Value : false;
			this.SectionContainsNonCompliance = sectionContainsNonCompliance;
			this.NonComplianceNotification = nonComplianceNotification.HasValue ? nonComplianceNotification.Value : false;
			this.Office = office;
			this.Agency = agency;
			this.LMBA = LMBA;
			this.Name = name;
			this.Street = street;
			this.CityST = cityST;
			this.Phone = phone;
			this.Email = email;
			this.Other = other;
			this.IncludeInCoversheet = includeInCoversheet;
		}

		#endregion

		/// <summary>
		/// Revision Id
		/// </summary>
		[Required]
		public int RevisionId { get; set; }

		/// <summary>
		/// Optional Parent Id
		/// </summary>
		public int? ParentId { get; set; }

		/// <summary>
		/// Display Order
		/// </summary>
		[Required]
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Is Internal Section
		/// </summary>
		public bool? IsInternalSection { get; set; }

		/// <summary>
		/// Title (shared by content blocks as well as the section)
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Text block content.. RTE
		/// </summary>
		[DisplayFormat(ConvertEmptyStringToNull = false)]
		public string TextContent { get; set; }

		/// <summary>
		/// If rate table, should we display the Rate Code
		/// </summary>
		public bool? DisplayRateCode { get; set; }

		/// <summary>
		/// Item Type
		/// </summary>
		public SectionContentType ContentType { get; set; }

		/// <summary>
		/// Used to determine if section contains content
		/// </summary>
		/// <returns>Will return true if this section contains content</returns>
		public bool IsSectionContent()
		{
			return this.ContentType == SectionContentType.Text ||
				   this.ContentType == SectionContentType.RateTable || this.ContentType == SectionContentType.Address;
		}

		/// <summary>
		/// Section number, e.g. 1.2.2. Transient value populated in the SectionLoader when loading from DB and updated by PPRDController.js on client side
		/// </summary>
		public string ReferenceNumber { get; set; }

		/// <summary>
		/// Child nodes
		/// </summary>
		public ICollection<SectionModelView> ChildNodes { get; set; }

		/// <summary>
		/// Gets or sets old section Id (used by SectionLoader UpdateSectionsAndContent method to re-map rate codes)
		/// </summary>
		public int? OldId { get; set; }

		/// <summary>
		/// Last Update Date
		/// </summary>
		public DateTime? OldUpdateDate { get; set; }

		/// <summary>
		/// A Section Id that is not a PK, but is unique to a specific Revision
		/// 
		/// The idea is that this will allow us to track the sections accross revisions, allowing us to compare changes
		/// </summary>
		public int RevisionUniqueSectionId { get; set; }

		/// <summary>
		/// gets/sets whether section is required in RDSB
		/// </summary>
		public bool IsRdsbRequired { get; set; }

		/// <summary>
		/// Does section contain CASB Disclosure Statements
		/// </summary>
		public bool SectionContainsCasbDisclosure { get; set; }

		/// <summary>
		/// Is Disclosure Statement determined to be adequate
		/// </summary>
		public Nullable<bool> IsDisclosureStatementAdequate { get; set; }

		/// <summary>
		/// Does Section contain Non Compliance Issues?
		/// </summary>
		public bool SectionContainsNonCompliance { get; set; }

		/// <summary>
		/// Asks if the user was notified of possible non-compliance with the Disclosure Statement or Cost Account Standards 
		/// </summary>
		public Nullable<bool> NonComplianceNotification { get; set; }

		/// <summary>
		/// Address Office
		/// </summary>
		public string Office { get; set; }

		/// <summary>
		/// Address Agency
		/// </summary>
		public string Agency { get; set; }
		/// <summary>
		/// Address LMBA
		/// </summary>
		public string LMBA { get; set; }
		/// <summary>
		/// Address Name
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Address Street
		/// </summary>
		public string Street { get; set; }
		/// <summary>
		/// Address CityST
		/// </summary>
		public string CityST { get; set; }
		/// <summary>
		/// Address Phone
		/// </summary>
		public string Phone { get; set; }
		/// <summary>
		/// Address Email
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// Address Other
		/// </summary>
		public string Other { get; set; }

		/// <summary>
		/// address includeinCoversheet section
		/// </summary>
		public bool? IncludeInCoversheet { get; set; }
	}
}