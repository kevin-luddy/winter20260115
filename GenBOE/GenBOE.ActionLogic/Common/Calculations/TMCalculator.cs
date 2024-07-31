// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Calculations
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.BOE;
	using GenBOE.ActionLogic.ModelView.BOE;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.Exceptions;

	public class TMCalculator
	{
		#region Public Methods

		/// <summary>
		/// Calculates total cost for a labor task using the T&amp;M rates for the associated resource.
		/// </summary>
		/// <param name="workspace">The fullWorkspace.</param>
		/// <param name="laborTask">The labor task containing the labor spread values</param>
		/// <returns>The total cost after multiplying each labor spread value against the corresponding T&amp;M rate.</returns>
		public decimal TotalCostForTaskSpread(FullWorkspace workspace, ResourceTypeDto laborTask)
		{
			if (ReferenceEquals(workspace, null))
			{
				throw new ArgumentNullException(nameof(workspace));
			}
			if (ReferenceEquals(laborTask, null))
			{
				throw new ArgumentNullException(nameof(laborTask));
			}

			decimal totalCost = 0;
			try
			{
				// Get T&M rates from fullWorkspace for this resourceId.
				IReadOnlyCollection<TMResourceRateDTO> wsRates = workspace.TMResourceRatesForWorkspace;

				//Left Join (DefaultIfEmpty()) spreads to T&M spreadRates by resourceId & workspaceId, where laborTask date in T&M resource date range.
				// Missing T&M Rates will throw NullReferenceException.
				totalCost = (from laborSpread in laborTask.LaborSpreads
							 join tmResoureRate in wsRates
							 on new { res = laborTask.ResourceID.Value, ws = workspace.Id } equals
							 new { res = tmResoureRate.ResourceID, ws = tmResoureRate.WorkspaceID } into hrs
							 from hr in hrs.Where(tmResoureRate => tmResoureRate.StartDate.Value <= laborSpread.LaborSpreadDate)
								 .Where(tmResoureRate => tmResoureRate.EndDate.Value >= laborSpread.LaborSpreadDate).DefaultIfEmpty()
							 select (laborSpread.LaborSpreadValue * hr.ResourceRate.Value)).Sum();
			}
			catch (NullReferenceException)
			{
				throw new GenValidationException($"T&M Rates are missing for Resource: {laborTask.ResourceID.ToString()}.");
			}
			catch (Exception ex)
			{
				throw new GenValidationException($"Application encountered an error calculating T&M rates for {laborTask.ToString()}:" + ex.Message);
			}

			return totalCost;
		}

		/// <summary>
		/// Converts the summarized version of the DTO to a ModelView.
		/// </summary>
		/// <param name="dto">The DTO to convert.</param>
		/// <param name="workspace">The Workspace to use for retrieving TotalCost.</param>
		/// <param name="resourceLoader">Resource Loader</param>
		/// <param name="resourceIdsWithValidTMRates">list of Resource IDs that have valid T&amp;M rates.</param>
		/// <returns>Converted modelview from dto.</returns>
		public BOEFormModelView ConvertSummaryDtoToModelView(BOEFormDTO dto, FullWorkspace workspace, IResourceDTODataLoader resourceLoader, ICollection<int> resourceIdsWithValidTMRates)
		{
			decimal totalCost = 0m;
			decimal tmTotalCost = 0m;
			bool hasValidTMRates = true;

			foreach (BoeTaskElementDTO taskElement in workspace.TaskElements)
			{
				CalculateTotals(dto, workspace, resourceLoader, resourceIdsWithValidTMRates, ref totalCost, ref tmTotalCost, ref hasValidTMRates, taskElement);
			}
			return new BOEFormModelView
			{
				BOEFormId = dto.Id,
				BOEFormName = dto.FormName,
				BOEFormType = dto.BOEFormType,
				TotalCost = totalCost,
				TMCost = hasValidTMRates ? tmTotalCost : 0, // note: value will not be displayed if hasValidTMRates is false
				HasValidTMRates = hasValidTMRates
			};
		}

		/// <summary>
		/// Get BOE Totals 
		/// </summary>
		/// <param name="pboeDtos"></param>
		/// <param name="fullWorkspace"></param>
		/// <param name="resourceLoader"></param>
		/// <param name="resourceIdsWithValidTMRates"></param>
		/// <returns></returns>
		public ICollection<PBOERow> GetBOETotals(ICollection<BOEFormPBOEDTO> pboeDtos, FullWorkspace fullWorkspace, IResourceDTODataLoader resourceLoader, ICollection<int> resourceIdsWithValidTMRates)
		{
			return GetPBOETotals(pboeDtos, fullWorkspace, resourceLoader, resourceIdsWithValidTMRates);
		}

		/// <summary>
		/// Validates the specified IBOE and PBOE Form T&amp;M resources.
		/// </summary>
		/// <param name="validationErrors">output parameter - collection of error messages</param>
		/// <param name="resourceIdsWithValidTMRates">output parameter - list of Resource IDs that have valid T&amp;M rates.</param>
		/// <param name="workspace">A fullWorkspace to validate boe forms against.</param>
		/// <param name="iboeForms">IBOE forms to validate</param>
		/// <param name="pboeForms">PBOE forms to validate</param>
		public void ValidateBOEFormsTMResources(ICollection<ValidationMessage> validationErrors,
			ICollection<int> resourceIdsWithValidTMRates, FullWorkspace workspace, ICollection<BOEFormIBOEDTO> iboeForms,
			ICollection<BOEFormPBOEDTO> pboeForms, ITMResourceRateDTODataLoader tmResourceRateLoader, IResourceDTODataLoader resourceLoader)
		{
			if (ReferenceEquals(validationErrors, null))
			{
				throw new ArgumentNullException(nameof(validationErrors));
			}

			if (ReferenceEquals(resourceIdsWithValidTMRates, null))
			{
				throw new ArgumentNullException(nameof(resourceIdsWithValidTMRates));
			}

			if (ReferenceEquals(workspace, null))
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			// Validate each of the selected IBOE and PBOE forms
			if (iboeForms != null)
			{
				foreach (BOEFormIBOEDTO iboe in iboeForms)
				{
					this.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace, iboe.ResourceIds, tmResourceRateLoader, resourceLoader);
				}
			}

			if (pboeForms != null)
			{
				foreach (BOEFormPBOEDTO pboe in pboeForms)
				{
					this.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace, pboe.ResourceIds, tmResourceRateLoader, resourceLoader);
				}
			}
		}

		#endregion Public Methods

		#region Private / Internal Methods
		/// <summary>
		/// Validates the IBOE or PBOE Form T&amp;M Resources
		/// </summary>
		/// <param name="validationErrors">output parameter - collection of error messages</param>
		/// <param name="resourceIdsWithValidTMRates">output parameter - list of Resource IDs that have valid T&amp;M rates.</param>
		/// <param name="workspace">A fullWorkspace to validate boe forms against.</param>
		/// <param name="resourceIds">IBOE or PBOE Resource Ids</param>
		internal void ValidateBOEFormTMResources(ICollection<ValidationMessage> validationErrors, ICollection<int> resourceIdsWithValidTMRates, FullWorkspace workspace, ICollection<int> resourceIds, ITMResourceRateDTODataLoader tmResourceRateLoader, IResourceDTODataLoader resourceLoader)
		{
			if (ReferenceEquals(validationErrors, null))
			{
				throw new ArgumentNullException(nameof(validationErrors));
			}

			if (ReferenceEquals(resourceIdsWithValidTMRates, null))
			{
				throw new ArgumentNullException(nameof(resourceIdsWithValidTMRates));
			}

			if (ReferenceEquals(workspace, null))
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			if (ReferenceEquals(resourceIds, null))
			{
				throw new ArgumentNullException(nameof(resourceIds));
			}

			if (workspace.IsUsingTM)
			{
				// Get list of T&M resources for this fullWorkspace 
				ICollection<TMResourceRateDTO> workspaceTMResourceRates = tmResourceRateLoader.GetByWorkspaceId(workspace.Id);

				foreach (int resourceId in resourceIds)
				{
					int startingErrorCount = validationErrors.Count;

					// Only validate Sub and IWTA resources with RateType = Hours
					ResourceDTO resource = resourceLoader.GetById(resourceId);
					if ((resource.ElementOfCost == ElementOfCostType.Sub || resource.ElementOfCost == ElementOfCostType.IWTA) && resource.RateType == RateType.Hours)
					{
						// Perform the following validations:
						// - Make sure we have rates for selected resources   
						// - Make sure the rates all have start dates, end dates, and rate values        
						// - Make sure the rate dates cover the period when they are used (BOEJ-2862)
						// - Make sure the rate dates are sequential, i.e. don't overlap, or contain gaps.
						IList<TMResourceRateDTO> tmResourceRates = workspaceTMResourceRates.Where(x => x.ResourceID == resourceId).OrderBy(x => x.StartDate).ToList();
						if (!tmResourceRates.Any())
						{
							validationErrors.Add(new ValidationMessage(string.Format("There are no T&M rates for resource {0}", resource.ResourceName)));
						}
						else if (tmResourceRates.Any(x => !x.StartDate.HasValue || !x.EndDate.HasValue))
						{
							validationErrors.Add(new ValidationMessage(string.Format("One or more of the T&M rates for resource {0} are missing a start or end date.", resource.ResourceName)));
						}
						else
						{
							if (tmResourceRates.Any(x => !x.ResourceRate.HasValue))
							{
								validationErrors.Add(new ValidationMessage(string.Format("One or more of the T&M rates for resource {0} are not populated.", resource.ResourceName)));
							}

							if (this.StartAndEndDatesAreValid(workspace, tmResourceRates, resourceId))
							{
								bool isSequential = true;
								DateTime? previousEndDate = null;
								foreach (TMResourceRateDTO tmResourceRate in tmResourceRates)
								{
									if (previousEndDate.HasValue && isSequential && tmResourceRate.StartDate.HasValue)
									{
										isSequential = this.CheckConsecutiveMonths(previousEndDate.Value, tmResourceRate.StartDate.Value);
									}
									previousEndDate = tmResourceRate.EndDate;
								}

								if (!isSequential)
								{
									validationErrors.Add(new ValidationMessage(string.Format("The T&M rates for resource {0} are not sequential, i.e. there are gaps or overlaps in the date ranges.",
										resource.ResourceName)));
								}
							}
							else
							{
								validationErrors.Add(new ValidationMessage(string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.",
									resource.ResourceName, workspace.Boes.SelectMany(x => x.LaborTypes.Where(z => (z.ResourceID.HasValue && z.ResourceID == resourceId) || (z.BusinessResourceCodeID.HasValue && z.BusinessResourceCodeID == resourceId))).Min(x => x.StartDate),
									workspace.Boes.SelectMany(x => x.LaborTypes.Where(z => (z.ResourceID.HasValue && z.ResourceID == resourceId) || (z.BusinessResourceCodeID.HasValue && z.BusinessResourceCodeID == resourceId))).Max(x => x.EndDate))));
							}
						}
					}

					int endingErrorCount = validationErrors.Count;
					if (startingErrorCount == endingErrorCount)
					{
						// no errors logged for this resource, so add it to the validated list
						resourceIdsWithValidTMRates.Add(resourceId);
					}
				}
			}
		}

		/// <summary>
		/// Validates whether start & end dates are valid
		/// </summary>
		/// <param name="workspace">Full WS</param>
		/// <param name="tmResourceRates">T&amp;M Resource Rates</param>
		/// <param name="resourceId">Resource Id (which we are validating)</param>
		/// <returns>Whether the rates are valid or not</returns>
		private bool StartAndEndDatesAreValid(FullWorkspace workspace, IList<TMResourceRateDTO> tmResourceRates, int resourceId)
		{
			DateTime? rateStartDate = tmResourceRates.First().StartDate;
			DateTime? rateEndDate = tmResourceRates.Last().EndDate;

			bool startAndEndDatesValid = rateStartDate.HasValue && rateEndDate.HasValue;

			if (startAndEndDatesValid)
			{
				// get start/end dates based on when the resource is actually used
				Collection<ResourceTypeDto> resourceUsages = workspace.Boes.SelectMany(x => x.LaborTypes.Where(z => (z.ResourceID.HasValue && z.ResourceID == resourceId) || (z.BusinessResourceCodeID.HasValue && z.BusinessResourceCodeID == resourceId))).ToCollection();
				if (resourceUsages.Any())
				{
					DateTime resourceUsageStartDate = resourceUsages.Min(x => x.StartDateValue);
					DateTime resourceUsageEndDate = resourceUsages.Max(x => x.EndDateValue);

					startAndEndDatesValid = (rateStartDate.Value.Date <= resourceUsageStartDate.Date) && (rateEndDate.Value.Date >= resourceUsageEndDate.Date);
				}
			}

			return startAndEndDatesValid;
		}

		/// <summary>
		/// Compare two dates and return true if they represent consecutive months. 
		/// Note: This code assume the start and end dates are normalized to the 15th of the month at 12pm.
		/// </summary>
		/// <param name="date1"></param>
		/// <param name="date2"></param>
		/// <returns>True if date2 is 1 month later than date1; False otherwise.</returns>
		private bool CheckConsecutiveMonths(DateTime date1, DateTime date2)
		{
			return DateTime.Compare(date1.AddMonths(1), date2) == 0;
		}

		/// <summary>
		/// Calculate Totals
		/// </summary>
		/// <param name="dto">BOE Form DTO</param>
		/// <param name="fullWorkspace">Full Workspace</param>
		/// <param name="resourceLoader">Resouce Loader</param>
		/// <param name="resourceIdsWithValidTMRates">List of Resource Id's with Valid TM Rates</param>
		/// <param name="totalCost">Reference to Total Cost</param>
		/// <param name="tmTotalCost">Reference to T&M Total Cost</param>
		/// <param name="hasValidTMRates">Bool to show Valid TM Rates</param>
		/// <param name="taskElement">Workspace Task Element</param>
		private void CalculateTotals(BOEFormDTO dto, FullWorkspace fullWorkspace, IResourceDTODataLoader resourceLoader, ICollection<int> resourceIdsWithValidTMRates, ref decimal totalCost, ref decimal tmTotalCost, ref bool hasValidTMRates, BoeTaskElementDTO taskElement)
		{
			//get brc labors based on 1lmx start date
			List<ResourceTypeDto> taskElementLabors = BRCValidationUtility.ProcessLaborTypesForBrc(taskElement.taskElementLabors).ToList();

			foreach (ResourceTypeDto laborTask in taskElementLabors)
			{
				if (laborTask.ValueSpread.HasValue && laborTask.ResourceID.HasValue && dto.ResourceIds.Contains(laborTask.ResourceID.Value))
				{
					if (laborTask.SpreadType == SpreadType.Cost)
					{
						totalCost += laborTask.ValueSpread.Value;
					}
					if (fullWorkspace.IsUsingTM && laborTask.SpreadType == SpreadType.Hours)
					{
						ResourceDTO resource = resourceLoader.GetById(laborTask.ResourceID.Value);
						if (!resourceIdsWithValidTMRates.Contains(laborTask.ResourceID.Value))
						{
							hasValidTMRates = false;
						}

						if (hasValidTMRates && (resource.ElementOfCost == ElementOfCostType.IWTA ||
							 resource.ElementOfCost == ElementOfCostType.Sub) &&
							resource.RateType == RateType.Hours)
						{
							tmTotalCost += TotalCostForTaskSpread(fullWorkspace, laborTask);
						}
					}
				}
			}
		}

		/// <summary>
		/// Calculate Totals
		/// </summary>
		/// <param name="dto">PBOE Form DTO</param>
		/// <param name="fullWorkspace">Full Workspace</param>
		/// <param name="resourceLoader">Resouce Loader</param>
		/// <param name="resourceIdsWithValidTMRates">List of Resource Id's with Valid TM Rates</param>
		/// <param name="totalCost">Reference to Total Cost</param>
		/// <param name="tmTotalCost">Reference to T&M Total Cost</param>
		/// <param name="hasValidTMRates">Bool to show Valid TM Rates</param>
		/// <param name="taskElement">Workspace Task Element</param>
		private void CalculateTotals(BOEFormPBOEDTO dto, FullWorkspace fullWorkspace, IResourceDTODataLoader resourceLoader, ICollection<int> resourceIdsWithValidTMRates, ref decimal totalCost, ref decimal tmTotalCost, ref bool hasValidTMRates, BoeTaskElementDTO taskElement)
		{
			//get brc labors based on 1lmx start date
			List<ResourceTypeDto> taskElementLabors = BRCValidationUtility.ProcessLaborTypesForBrc(taskElement.taskElementLabors).ToList();

			foreach (ResourceTypeDto laborTask in taskElementLabors)
			{
				if (laborTask.ValueSpread.HasValue && laborTask.ResourceID.HasValue && dto.ResourceIds.Contains(laborTask.ResourceID.Value))
				{
					if (laborTask.SpreadType == SpreadType.Cost)
					{
						totalCost += laborTask.ValueSpread.Value;
					}
					if (fullWorkspace.IsUsingTM && laborTask.SpreadType == SpreadType.Hours)
					{
						ResourceDTO resource = resourceLoader.GetById(laborTask.ResourceID.Value);
						if (!resourceIdsWithValidTMRates.Contains(laborTask.ResourceID.Value))
						{
							hasValidTMRates = false;
						}

						if (hasValidTMRates && (resource.ElementOfCost == ElementOfCostType.IWTA ||
							 resource.ElementOfCost == ElementOfCostType.Sub) &&
							resource.RateType == RateType.Hours)
						{
							tmTotalCost += TotalCostForTaskSpread(fullWorkspace, laborTask);
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets PBOE Totals
		/// </summary>
		/// <param name="pboeDtos">Collection of PBOE DTOs</param>
		/// <param name="fullWorkspace">Full Workspace</param>
		/// <param name="resourceLoader">Resource Loader</param>
		/// <param name="resourceIdsWithValidTMRates">Collection of Id's with Valid TM Rates</param>
		/// <returns>Collection of PBOE Row Data</returns>
		private ICollection<PBOERow> GetPBOETotals(ICollection<BOEFormPBOEDTO> pboeDtos, FullWorkspace fullWorkspace, IResourceDTODataLoader resourceLoader, ICollection<int> resourceIdsWithValidTMRates)
		{
			ICollection<PBOERow> rowList = new List<PBOERow>();

			decimal totalCost = 0m;
			decimal tmTotalCost = 0m;
			bool hasValidTMRates = true;

			foreach (BOEFormPBOEDTO pboe in pboeDtos)
			{
				foreach (BoeTaskElementDTO taskElement in fullWorkspace.TaskElements)
				{
					CalculateTotals(pboe, fullWorkspace, resourceLoader, resourceIdsWithValidTMRates, ref totalCost, ref tmTotalCost, ref hasValidTMRates, taskElement);
				}

				rowList.Add(new PBOERow
				{
					PBOEId = pboe.Id,
					PBOEName = string.Empty,
					Cost = totalCost,
					TMCost = tmTotalCost,
					TotalCost = totalCost + tmTotalCost,
				});

				// Reset the Totals for the next row
				totalCost = 0m;
				tmTotalCost = 0m;
			}

			return rowList;
		}

		#endregion Private / Internal Methods
	}
}
