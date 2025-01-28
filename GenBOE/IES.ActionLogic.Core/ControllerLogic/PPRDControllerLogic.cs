// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.Core.ControllerLogic
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IES.ActionLogic.Core.Mediator;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Exceptions;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using Microsoft.Extensions.Configuration;

	/// <summary>
	/// PPRD Controller Logic
	/// </summary>
	public class PPRDControllerLogic : RdmControllerLogic, IPPRDControllerLogic
	{
		/// <summary>
		/// Configuration for appsettings.json.
		/// </summary>
		private readonly IConfiguration configuration;

		/// <summary>
		/// The file attachment loader
		/// </summary>
		private readonly IFileAttachmentLoader fileAttachmentLoader;

		/// <summary>
		/// Initializes a new instance of the <see cref="PPRDControllerLogic"/> class.
		/// </summary>
		/// <param name="fileAttachmentLoader">The file attachment loader.</param>
		/// <param name="revisionMediator">Revision Mediator</param>
		/// <param name="areaLockingLoader">Area Locking Loader</param>
		/// <param name="adUtils">AD Utilities</param>
		/// <param name="securityInfo">Security Information</param>
		public PPRDControllerLogic(IFileAttachmentLoader fileAttachmentLoader,
			IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryService adUtils, ISecurityInformation securityInfo, IConfiguration configuration)
			: base(areaLockingLoader, revisionMediator, adUtils, securityInfo, configuration)
		{
			this.fileAttachmentLoader = fileAttachmentLoader;
			this.configuration = configuration;
		}

		/// <summary>
		/// Validate that the section Ids passed in can be deleted from the WIP revision.
		/// </summary>
		/// <param name="sectionIds">Section Ids to validate against.</param>
		/// <param name="validationErrors">List of errors found.</param>
		/// <param name="sections">Sections to validate.</param>
		/// <param name="wipRevision">The work in progress Revision.</param>
		public void ValidateDeletionSections(ICollection<int> sectionIds, ICollection<ValidationMessage> validationErrors, ICollection<OptionModelView> sections,
			RevisionModelView wipRevision)
		{
			if (sectionIds == null)
			{
				throw new ArgumentNullException(nameof(sectionIds));
			}

			if (validationErrors == null)
			{
				throw new ArgumentNullException(nameof(validationErrors));
			}

			if (sections == null)
			{
				throw new ArgumentNullException(nameof(sections));
			}

			if (wipRevision == null)
			{
				throw new ArgumentNullException(nameof(wipRevision));
			}

			ICollection<FileAttachmentRowModelView> fileAttachments = fileAttachmentLoader.GetByRevision(wipRevision.Id);

			foreach (int sectionId in sectionIds)
			{
				// only validate existing sections
				if (sectionId >= 0)
				{
					OptionModelView section = sections.FirstOrDefault(s => s.Id == sectionId);
					if (section == null)
					{
						validationErrors.Add(new ValidationMessage($"No section was found with this Id: {sectionId}, please refresh the page to get up to date data."));
					}
					else if (fileAttachments.Any(f => f.SectionId == sectionId))
					{
						validationErrors.Add(new ValidationMessage($"Cannot delete because a File Attachment was found for Section {section.Label}"));
					}
				}
			}
		}

		/// <summary>
		/// Validate that the number of tables is correct for each section and the document. 
		/// Also check to see if any content nodes have child nodes. 
		/// Make sure all section content types are valid.
		/// </summary>
		/// <param name="sections">Sections to validate</param>
		/// <param name="validationErrors">List of errors found.</param>
		public void ValidateSections(ICollection<SectionModelView> sections, ICollection<ValidationMessage> validationErrors)
		{
			if (sections == null)
			{
				throw new ArgumentNullException(nameof(sections));
			}

			if (validationErrors == null)
			{
				throw new ArgumentNullException(nameof(validationErrors));
			}

			List<string> casbCoreSections = new();
			List<string> casbServiceSections = new();
			List<string> nonComplianceCoreSections = new();
			List<string> nonComplianceServiceSections = new();

			foreach (SectionModelView section in sections)
			{
				ValidateSection(section, validationErrors, casbCoreSections, casbServiceSections, nonComplianceCoreSections, nonComplianceServiceSections);
			}

			if (casbCoreSections.Count != 1)
			{
				validationErrors.Add(new ValidationMessage($"Exactly one section should be marked as 'Contains CASB Disclosure Statement'. The following sections were marked this way: {(casbCoreSections.Any() ? string.Join(", ", casbCoreSections) : "none")}"));
			}

			if (nonComplianceCoreSections.Count != 1)
			{
				validationErrors.Add(new ValidationMessage($"Exactly one section should be marked as 'Contains CAS Non-Compliance Issues'. The following sections are marked this way: {(nonComplianceCoreSections.Any() ? string.Join(", ", nonComplianceCoreSections) : "none")}"));
			}
		}

		/// <summary>
		/// Recursively validate the section and its children.
		/// </summary>
		/// <param name="section">The section node to check.</param>
		/// <param name="validationErrors">List of errors found.</param>
		/// <param name="casbCoreSections">A list of strings in which we'll keep track of sections that contain CASB Core setting; this is necessary to validate that it's only set once</param>
		/// <param name="casbServiceSections">A list of strings in which we'll keep track of sections that contain CASB Service setting; this is necessary to validate that it's only set once</param>
		/// <param name="nonComplianceCoreSections">A list of strings in which we'll keep track of sections that contain non-compliance Core setting; this is necessary to validate that it's only set once</param>
		/// <param name="nonComplianceServiceSections">A list of strings in which we'll keep track of sections that contain non-compliance Service setting; this is necessary to validate that it's only set once</param>
		private void ValidateSection(SectionModelView section, ICollection<ValidationMessage> validationErrors, ICollection<string> casbCoreSections, ICollection<string> casbServiceSections, ICollection<string> nonComplianceCoreSections, ICollection<string> nonComplianceServiceSections)
		{
			if (section.ContentType == SectionContentType.Section)
			{
				if (string.IsNullOrEmpty(section.Title))
				{
					validationErrors.Add(new ValidationMessage($"Section {section.ReferenceNumber} - Is missing required Title."));
				}

				if (section.SectionContainsCasbDisclosureCore)
				{
					casbCoreSections.Add(section.ReferenceNumber);
				}

				if (section.SectionContainsCasbDisclosureService)
				{
					casbServiceSections.Add(section.ReferenceNumber);
				}

				if (section.SectionContainsNonComplianceCore)
				{
					nonComplianceCoreSections.Add(section.ReferenceNumber);
				}

				if (section.SectionContainsNonComplianceService)
				{
					nonComplianceServiceSections.Add(section.ReferenceNumber);
				}

				int numTablesInSection = 0;
				int numAddressInSection = 0;
				foreach (SectionModelView child in section.ChildNodes)
				{
					if (child.ContentType == SectionContentType.RateTable)
					{
						numTablesInSection++;
					}

					if (child.ContentType == SectionContentType.Address)
					{
						numAddressInSection++;
					}

					ValidateSection(child, validationErrors, casbCoreSections, casbServiceSections, nonComplianceCoreSections, nonComplianceServiceSections);  // recursively validate children
				}

				if (numTablesInSection > 1)
				{
					validationErrors.Add(new ValidationMessage($"Section {section.ReferenceNumber} - Only 1 table is allowed per section."));
				}

				if (numAddressInSection > 1)
				{
					validationErrors.Add(new ValidationMessage($"Section {section.ReferenceNumber} - Only 1 Address is allowed per section."));
				}

			}
			else if (section.IsSectionContent())
			{
				// validate text and table elements
				if (section.ChildNodes.Count > 0)
				{
					validationErrors.Add(new ValidationMessage("Text or table nodes may not have children."));
				}
			}
			else
			{
				validationErrors.Add(new ValidationMessage("Invalid Content type detected."));
			}
		}
	}
}