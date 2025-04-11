// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Loaders
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using IES.Common;
	using GenBOE.Models;
	using GenBOE.DataBridge.Core.DTO.Travel;
	using IES.Common.Core;
	using GenBOE.DataBridge.Core.DTO;

	/// <summary>
	/// Data loader for the RMS Zone Travel Escalation Rates and Fees
	/// </summary>
	public class RMSZoneTravelRatesFeesDataLoader
	{
		private IEscalationRatesDTOLoader systemEscalationRatesLoader;
		private IMSTTravelNonzoneFeesAndCostsDTODataLoader systemFeesLoader;

		/// <summary>
		/// Nominal constructor
		/// </summary>
		public RMSZoneTravelRatesFeesDataLoader(
			IEscalationRatesDTOLoader escalationLoader,
			IMSTTravelNonzoneFeesAndCostsDTODataLoader feesAndCostsLoader)
		{
			systemEscalationRatesLoader = escalationLoader;
			systemFeesLoader = feesAndCostsLoader;
		}

		/// <summary>
		/// Used only for testing
		/// </summary>
		[Obsolete("Only used for testing purposes")]
		public RMSZoneTravelRatesFeesDataLoader()
		{ }

		/// <summary>
		/// Gets all Fees And Costs DTOs
		/// </summary>
		/// <param name="workspaceId">The id of the workspace for the workspace specific fees and costs.</param>
		/// <returns>Collection of all Fees and Costs DTOs</returns>
		public virtual ICollection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO> getAllFeesAndCostsByWorkspace(int workspaceId)
		{
			ICollection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO> toReturn = new Collection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO>();

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				toReturn = (from f in gbe.WorkspaceRMSTravelNonzoneFeesAndCosts
							where f.WorkspaceID == workspaceId
							select new WorkspaceRMSTravelNonzoneFeesAndCostsDTO
							{
								Id = f.FeesAndCostsID,
								UpdateDate = f.UpdateDT,
								WorkspaceId = f.WorkspaceID,
								ModeID = f.ModeID,
								TravelAgencyFee = f.TravelAgencyFee,
								MiscOther = f.MiscOther
							}).ToCollection();
			}

			return toReturn;
		}

		/// <summary>
		/// Save updates to a Travel Mode's Fee and Cost
		/// </summary>
		/// <param name="feeAndCost">Fee And Cost DTO to be saved</param>
		public int saveWorkspaceFeesAndCosts(WorkspaceRMSTravelNonzoneFeesAndCostsDTO feeAndCost)
		{
			if (feeAndCost == null)
			{
				throw new ArgumentNullException(nameof(feeAndCost));
			}

			int result = feeAndCost.Id;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				result = gbe.upsertWorkspaceRMSTravelNonzoneFeesAndCosts(feeAndCost.Id, feeAndCost.WorkspaceId, feeAndCost.UpdateDate, feeAndCost.ModeID, feeAndCost.TravelAgencyFee, feeAndCost.MiscOther).First().Value;
			}

			return result;
		}

		/// <summary>
		/// Copy the System Default Fees over top of the workspace Escalation fees.
		/// </summary>
		/// <param name="workspaceId">The workspace Id to copy on top of.</param>
		public void CopySystemDefaultFees(int workspaceId)
		{
			if (workspaceId < 1)
			{
				throw new ArgumentException("workspaceId must be a positive integer.");
			}

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.resetWorkspaceDefaultFees(workspaceId);
			}
		}

		/// <summary>
		/// Determines if the workspace fees are out of date compared to system fees.
		/// </summary>
		/// <param name="workspaceId">The workspace Id to check.</param>
		/// <returns></returns>
		public bool AreCurrentFeesOutOfDate(int workspaceId)
		{
			bool outOfDate = false;
			ICollection<WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = getAllFeesAndCostsByWorkspace(workspaceId);
			ICollection<MSTTravelNonzoneFeesAndCostsDTO> systemFees = systemFeesLoader.getAllFeesAndCosts();

			if (fees.Count != systemFees.Count)
			{
				outOfDate = true;
			}

			foreach (WorkspaceRMSTravelNonzoneFeesAndCostsDTO fee in fees)
			{
				MSTTravelNonzoneFeesAndCostsDTO systemRate = systemFees.FirstOrDefault(s => s.ModeID == fee.ModeID);
				if (systemRate == null || systemRate.UpdateDate != fee.UpdateDate)
				{
					outOfDate = true;
					break;
				}
			}

			return outOfDate;
		}

		/// <summary>
		/// Gets all Escalation Rates DTOs by workspace.
		/// </summary>
		/// <param name="workspaceId">The id of the workspace for the workspace specific fees and costs.</param>
		/// <returns>Collection of all Fees and Costs DTOs</returns>
		public virtual ICollection<WorkspaceRMSEscalationRatesDTO> getAllEscalationRatesByWorkspace(int workspaceId)
		{
			ICollection<WorkspaceRMSEscalationRatesDTO> toReturn = new Collection<WorkspaceRMSEscalationRatesDTO>();

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				toReturn = (from f in gbe.WorkspaceRMSTravelEscalationRates
							where f.WorkspaceID == workspaceId
							select new WorkspaceRMSEscalationRatesDTO
							{
								Id = f.TravelEscalationRateID,
								UpdateDate = f.UpdateDT,
								WorkspaceId = f.WorkspaceID,
								Year = f.Year,
								AirfareRate = f.Escalation,
								PerDiemRate = f.PerDiemRate ?? 0,
								MiscRate = f.MiscRate ?? 0
							}).ToCollection();
			}

			return toReturn;
		}

		/// <summary>
		/// Save updates to a Workspace's Escalation Rate.
		/// </summary>
		/// <param name="feeAndCost">Fee And Cost DTO to be saved</param>
		public int saveWorkspaceEscalationRate(WorkspaceRMSEscalationRatesDTO rate)
		{
			if (rate == null)
			{
				throw new ArgumentNullException(nameof(rate));
			}

			int result = rate.Id;
			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				result = gbe.upsertWorkspaceRMSTravelEscalationRate(rate.Id, rate.WorkspaceId, rate.UpdateDate, rate.Year, rate.AirfareRate, rate.PerDiemRate, rate.MiscRate).First().Value;
			}

			return result;
		}

		/// <summary>
		/// Copy the System Default Escalation rates over top of the workspace Escalation rates.
		/// </summary>
		/// <param name="workspaceId">The workspace Id to copy on top of.</param>
		public void CopySystemDefaultRates(int workspaceId)
		{
			if (workspaceId < 1)
			{
				throw new ArgumentException("workspaceId must be a positive integer.");
			}

			using (GenBoeEntities gbe = new GenBoeEntities())
			{
				gbe.resetWorkspaceDefaultRates(workspaceId);
			}
		}

		/// <summary>
		/// Determines if the workspace rates are out of date compared to system rates.
		/// </summary>
		/// <param name="workspaceId">The workspace Id to check.</param>
		/// <returns></returns>
		public bool AreCurrentEscalationRatesOutOfDate(int workspaceId)
		{
			bool outOfDate = false;
			ICollection<WorkspaceRMSEscalationRatesDTO> rates = getAllEscalationRatesByWorkspace(workspaceId);
			ICollection<EscalationRatesDTO> systemRates = systemEscalationRatesLoader.GetAll();

			if (rates.Count != systemRates.Count)
			{
				outOfDate = true;
			}

			foreach (WorkspaceRMSEscalationRatesDTO rate in rates)
			{
				EscalationRatesDTO systemRate = systemRates.FirstOrDefault(s => s.Year == rate.Year);
				if (systemRate == null || systemRate.UpdateDate != rate.UpdateDate || systemRate.MiscRate != rate.MiscRate || systemRate.DevEscalation != rate.AirfareRate || systemRate.LMSIEscalation != rate.PerDiemRate)
				{
					outOfDate = true;
					break;
				}
			}

			return outOfDate;
		}
	}
}
