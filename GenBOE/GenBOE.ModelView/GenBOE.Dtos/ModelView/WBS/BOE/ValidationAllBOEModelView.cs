// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using IES.Common;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;

	/// <summary>
	/// This model view is used to return any validation messages that need to be shown
	/// to a user after the "Validate All BOE's" button has been selected
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ValidationAllBOEModelView
	{
		public ValidationAllBOEModelView()
		{
			AllBOEs = new Collection<ValidationBOEModelView>();
		}

		//Collection of each BOE validation method.
		public ICollection<ValidationBOEModelView> AllBOEs { get; set; }

		/// <summary>
		/// Flatten the BOEs
		/// </summary>
		/// <returns>Flattened Validation Messages</returns>
		public ICollection<FlattenedValidateAllBOEModelView> Flatten()
		{
			List<FlattenedValidateAllBOEModelView> flattenedValidateAllBOEs = new List<FlattenedValidateAllBOEModelView>();
			foreach (ValidationBOEModelView boe in AllBOEs)
			{
				if (boe.BOEHeaderMsgs != null && boe.BOEHeaderMsgs.Any())
				{
					AddMessages(flattenedValidateAllBOEs, boe, boe.BOEHeaderMsgs, IESWebConstants.GET_BOE_HEADER_MSG_HEADER);
				}

				if (boe.BOECustomFieldValidationMessages != null && boe.BOECustomFieldValidationMessages.Any())
				{
					AddMessages(flattenedValidateAllBOEs, boe, boe.BOECustomFieldValidationMessages, IESWebConstants.GET_BOE_CUSTOM_VALIDATION_HEADER);
				}

				if (boe.BOECommentandApprovals != null && boe.BOECommentandApprovals.Any())
				{
					AddMessages(flattenedValidateAllBOEs, boe, boe.BOECommentandApprovals, IESWebConstants.GET_BOE_COMMENTS_HEADER);
				}

				if (boe.Tasks != null && boe.Tasks.Any())
				{
					AddSectionMessages(flattenedValidateAllBOEs, boe, boe.Tasks, IESWebConstants.GET_BOE_LABOR_HEADER, FlattenedValidateTaskType.Labor);
				}

				if (boe.Materials != null && boe.Materials.Any())
				{
					AddSectionMessages(flattenedValidateAllBOEs, boe, boe.Materials, IESWebConstants.GET_BOE_MATERIAL_HEADER, FlattenedValidateTaskType.Material);
				}

				if (boe.Costs != null && boe.Costs.Any())
				{
					AddSectionMessages(flattenedValidateAllBOEs, boe, boe.Costs, IESWebConstants.GET_BOE_COST_HEADER, FlattenedValidateTaskType.ODC);
				}

				if (boe.Travels != null && boe.Travels.Any())
				{
					AddSectionMessages(flattenedValidateAllBOEs, boe, boe.Travels, IESWebConstants.GET_BOE_TRAVEL_HEADER, FlattenedValidateTaskType.Travel);
				}
			}

			return flattenedValidateAllBOEs;
		}

		/// <summary>
		/// Add section messages
		/// </summary>
		/// <param name="flattenedValidateAllBOEs">Running list of flattened rows</param>
		/// <param name="boe">Original BOE</param>
		/// <param name="sections">Sections to flatten</param>
		/// <param name="sectionHeader">Section Header</param>
		/// <param name="taskType">Task Type</param>
		private void AddSectionMessages(List<FlattenedValidateAllBOEModelView> flattenedValidateAllBOEs, ValidationBOEModelView boe, ICollection<ValidationBOETasks> sections, string sectionHeader, FlattenedValidateTaskType taskType)
		{
			FlattenedValidateAllBOEModelView flatHeader = new FlattenedValidateAllBOEModelView
			{
				BOEId = boe.BOEID,
				BOE = boe.BOEName,
				IsHeader = true,
				Level = sectionHeader
			};
			flattenedValidateAllBOEs.Add(flatHeader);

			foreach (ValidationBOETasks section in sections)
			{
				FlattenedValidateAllBOEModelView flatTask = new FlattenedValidateAllBOEModelView
				{
					BOEId = boe.BOEID,
					BOE = boe.BOEName,
					TaskId = section.TaskId,
					TaskType = taskType,
					Level = section.TaskMessage
				};

				flattenedValidateAllBOEs.Add(flatTask);

				if (!string.IsNullOrWhiteSpace(section.TaskElementDetails?.TaskElementDetailsHeader))
				{
					FlattenedValidateAllBOEModelView taskDetailsHeader = new FlattenedValidateAllBOEModelView
					{
						BOEId = boe.BOEID,
						BOE = boe.BOEName,
						Section = section.TaskElementDetails.TaskElementDetailsHeader,
					};

					flattenedValidateAllBOEs.Add(taskDetailsHeader);
				}

				foreach (string message in section.TaskElementDetails.TaskElementDetailValidationMessages)
				{
					FlattenedValidateAllBOEModelView taskMessage = new FlattenedValidateAllBOEModelView
					{
						BOEId = boe.BOEID,
						BOE = boe.BOEName,
						Message = message
					};

					flattenedValidateAllBOEs.Add(taskMessage);
				}

				if (section.LaborTypes.Any())
				{
					foreach (ValidationBOELaborType laborType in section.LaborTypes)
					{
						FlattenedValidateAllBOEModelView laborHeader = new FlattenedValidateAllBOEModelView
						{
							BOEId = boe.BOEID,
							BOE = boe.BOEName,
							TaskId = section.TaskId,
							Section = laborType.LaborTypeHeader,
						};

						flattenedValidateAllBOEs.Add(laborHeader);

						foreach (string message in laborType.LaborTypeValidationMsgs)
						{
							FlattenedValidateAllBOEModelView laborMessage = new FlattenedValidateAllBOEModelView
							{
								BOEId = boe.BOEID,
								BOE = boe.BOEName,
								Message = message
							};

							flattenedValidateAllBOEs.Add(laborMessage);
						}
					}
				}
			}
		}

		/// <summary>
		/// Add message rows
		/// </summary>
		/// <param name="flattenedValidateAllBOEs">Flattened Validation Messages holder</param>
		/// <param name="boe">Validate BOE (none flattened model)</param>
		private static void AddMessages(List<FlattenedValidateAllBOEModelView> flattenedValidateAllBOEs, ValidationBOEModelView boe, ICollection<string> messages, string headerName)
		{
			FlattenedValidateAllBOEModelView flatHeader = new FlattenedValidateAllBOEModelView
			{
				BOEId = boe.BOEID,
				BOE = boe.BOEName,
				IsHeader = true,
				Level = headerName
			};
			flattenedValidateAllBOEs.Add(flatHeader);

			foreach (string message in messages)
			{
				FlattenedValidateAllBOEModelView flatMessage = new FlattenedValidateAllBOEModelView
				{
					BOEId = boe.BOEID,
					BOE = boe.BOEName,
					Message = message
				};

				flattenedValidateAllBOEs.Add(flatMessage);
			}
		}
	}
}
