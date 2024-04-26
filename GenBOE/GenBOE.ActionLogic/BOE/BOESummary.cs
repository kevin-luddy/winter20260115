// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.IO;
	using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.ZoneTravel;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;

	public class BOESummary : IBOESummary
	{
		private TravelTripCostCalculation travelTripCostCalculator;
		private RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader;

		#region Public Functions

		/// <summary>
		/// Initializes a new instance of the <see cref="BOESummary"/> class.
		/// </summary>
		/// <param name="travelTripCostCalculator">The travel trip cost calculator.</param>
		/// <param name="rmsZoneTravelRatesFeesDataLoader">The RMS zone travel rates fees data loader.</param>
		public BOESummary(
			TravelTripCostCalculation travelTripCostCalculator,
			RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader)
		{
			this.travelTripCostCalculator = travelTripCostCalculator;
			this.rmsZoneTravelRatesFeesDataLoader = rmsZoneTravelRatesFeesDataLoader;
		}

		/// <summary>
		/// Get a collection of all Summary Grid Model Views for the given BOE.
		/// </summary>
		/// <param name="boe">The BOE</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <param name="isSubcontractorUser">Whether the user is a subcontractor</param>
		/// <returns>All summary grid model views for the BOE</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public virtual ICollection<BOESummaryGridModelView> GetBOESummaryGridModelViews(BoeDTO boe, BOEExportInputs exportInputs, bool isSubcontractorUser)
		{
			if (exportInputs == null)
			{
				throw new ArgumentNullException(nameof(exportInputs));
			}

			if (boe == null)
			{
				throw new ArgumentNullException(nameof(boe));
			}

			bool retrieveOnlyLaborData = isSubcontractorUser;
			// get data
			IList<BOESummaryGridModelView> travelData = retrieveOnlyLaborData ? new List<BOESummaryGridModelView>() : this.GetTravelAndCost(boe, exportInputs);
			IList<BOESummaryGridModelView> laborData = this.GetLaborTypesAndHours(exportInputs, boe);
			IList<BOESummaryGridModelView> odcData = retrieveOnlyLaborData ? new List<BOESummaryGridModelView>() : this.GetODCAndCost(boe, exportInputs);
			IList<BOESummaryGridModelView> rmsAirZoneTravelData = retrieveOnlyLaborData ? new List<BOESummaryGridModelView>() : this.GetRMSAirZoneTravel(boe, exportInputs);
			IList<BOESummaryGridModelView> rmsDomZoneTravelData = retrieveOnlyLaborData ? new List<BOESummaryGridModelView>() : this.GetRMSDomZoneTravel(boe, exportInputs);
			IList<BOESummaryGridModelView> rmsNonZoneTravelData = retrieveOnlyLaborData ? new List<BOESummaryGridModelView>() : this.GetRMSNonZoneTravelAndCost(boe, exportInputs);

			// collate (group) the data by category
			ICollection<BOESummaryGridModelView> allData =
				laborData.Union(odcData)
				.Union(travelData)
				.Union(rmsNonZoneTravelData).ToList();

			IOrderedEnumerable<IGrouping<ElementOfCostType, BOESummaryGridModelView>> groupedCategories =
																							from A in allData
																							group A by A.Category into G
																							orderby G.Key
																							select G;
			// sum up the totals for each category
			IList<BOESummaryGridModelView> results;

			IGrouping<ElementOfCostType, BOESummaryGridModelView> laborDataGroup;
			if ((laborDataGroup = groupedCategories.FirstOrDefault(g => g.Key == ElementOfCostType.LMLabor)) == null)
			{
				results = new List<BOESummaryGridModelView>();
			}
			else
			{
				results = new List<BOESummaryGridModelView>(laborDataGroup);
			}

			IGrouping<ElementOfCostType, BOESummaryGridModelView> iwtaDataGroup;
			if ((iwtaDataGroup = groupedCategories.FirstOrDefault(g => g.Key == ElementOfCostType.IWTA)) != null)
			{
				decimal iwtaTotalHours = iwtaDataGroup.Sum(d => d.TotalHours ?? 0);
				decimal iwtaTotalCost = iwtaDataGroup.Sum(d => d.TotalCost ?? 0);
				int iwtaRollupCount = iwtaDataGroup.Sum(d => d.RollupCount);
				results.Add(new BOESummaryGridModelView { BOEID = boe.Id, Category = ElementOfCostType.IWTA, LaborType = ElementOfCostType.IWTA.GetResourceTypeCategory(), TotalHours = iwtaTotalHours, TotalCost = iwtaTotalCost, RollupCount = iwtaRollupCount, ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision });
			}

			IGrouping<ElementOfCostType, BOESummaryGridModelView> subcontractorDataGroup;
			if ((subcontractorDataGroup = groupedCategories.FirstOrDefault(g => g.Key == ElementOfCostType.Sub)) != null)
			{
				decimal subcontractorTotalHours = subcontractorDataGroup.Sum(d => d.TotalHours ?? 0);
				decimal subcontractorTotalCost = subcontractorDataGroup.Sum(d => d.TotalCost ?? 0);
				int subcontractorRollupCount = subcontractorDataGroup.Sum(d => d.RollupCount);
				results.Add(new BOESummaryGridModelView { BOEID = boe.Id, Category = ElementOfCostType.Sub, LaborType = ElementOfCostType.Sub.GetResourceTypeCategory(), TotalHours = subcontractorTotalHours, TotalCost = subcontractorTotalCost, RollupCount = subcontractorRollupCount, ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision });
			}

			IGrouping<ElementOfCostType, BOESummaryGridModelView> odcDataGroup;
			if ((odcDataGroup = groupedCategories.FirstOrDefault(g => g.Key == ElementOfCostType.ODC)) != null)
			{
				decimal odcTotalHours = odcDataGroup.Sum(d => d.TotalHours ?? 0);
				decimal odcTotalCost = odcDataGroup.Sum(d => d.TotalCost ?? 0);
				int odcRollupCount = odcDataGroup.Sum(d => d.RollupCount);
				results.Add(new BOESummaryGridModelView { BOEID = boe.Id, Category = ElementOfCostType.ODC, LaborType = ElementOfCostType.ODC.GetResourceTypeCategory(), TotalHours = odcTotalHours, TotalCost = odcTotalCost, RollupCount = odcRollupCount, ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision });
			}

			IGrouping<ElementOfCostType, BOESummaryGridModelView> materialsDataGroup;
			if ((materialsDataGroup = groupedCategories.FirstOrDefault(g => g.Key == ElementOfCostType.Materials)) != null)
			{
				decimal materialsTotalHours = materialsDataGroup.Sum(d => d.TotalHours ?? 0L);
				decimal materialsTotalCost = materialsDataGroup.Sum(d => d.TotalCost ?? 0m);
				int materialsRollupCount = materialsDataGroup.Sum(d => d.RollupCount);
				results.Add(new BOESummaryGridModelView { BOEID = boe.Id, Category = ElementOfCostType.Materials, LaborType = ElementOfCostType.Materials.GetResourceTypeCategory(), TotalHours = materialsTotalHours, TotalCost = materialsTotalCost, RollupCount = materialsRollupCount, ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision });
			}
			if (rmsAirZoneTravelData.Count > 0)
			{
				foreach (BOESummaryGridModelView trip in rmsAirZoneTravelData)
				{
					results.Add(new BOESummaryGridModelView { ModeID = MSTTravelMode.ZoneAirfare, BOEID = boe.Id, Category = trip.Category, LaborType = trip.LaborType, TotalHours = 0, TotalCost = 0, NumPeople = trip.NumPeople, NumDays = trip.NumDays, RollupCount = 999, ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision });
				}
			}
			if (rmsDomZoneTravelData.Count > 0)
			{
				foreach (BOESummaryGridModelView trip in rmsDomZoneTravelData)
				{
					results.Add(new BOESummaryGridModelView { ModeID = MSTTravelMode.ZoneNoAirfare, BOEID = boe.Id, Category = trip.Category, LaborType = trip.LaborType, TotalHours = 0, TotalCost = 0, NumPeople = trip.NumPeople, NumDays = trip.NumDays, RollupCount = 999, ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision });
				}
			}
			IGrouping<ElementOfCostType, BOESummaryGridModelView> travelDataGroup;
			if ((travelDataGroup = groupedCategories.FirstOrDefault(g => g.Key == ElementOfCostType.Travel)) != null)
			{
				decimal travelTotalHours = travelDataGroup.Where(x => x.ModeID == MSTTravelMode.None).Sum(d => d.TotalHours ?? 0L);
				decimal travelTotalCost = travelDataGroup.Where(x => x.ModeID == MSTTravelMode.None).Sum(d => d.TotalCost ?? 0m);
				int travelRollupCount = travelDataGroup.Where(x => x.ModeID == MSTTravelMode.None).Sum(d => d.RollupCount);
				results.Add(new BOESummaryGridModelView { ModeID = MSTTravelMode.None, BOEID = boe.Id, Category = ElementOfCostType.Travel, LaborType = ElementOfCostType.Travel.GetResourceTypeCategory(), TotalHours = travelTotalHours, TotalCost = travelTotalCost, RollupCount = travelRollupCount, ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision });
			}
			return results;
		}

		#endregion

		#region Private Functions

		/// <summary>
		/// LINQ through BOE DTO and pull out labor type id and the hours associated with the ID for summary grid display
		/// </summary>
		/// <param name="boe">The BOE</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>collection of MV for summary grid</returns>
		private List<BOESummaryGridModelView> GetLaborTypesAndHours(BOEExportInputs exportInputs, BoeDTO boe)
		{
			List<BOESummaryGridModelView> summaryResults;

			ICollection<BoeTaskElementDTO> taskElementCollection = exportInputs.TaskElements.Where(t => t.BoeID == boe.Id).ToList();

			IEnumerable<BOESummaryGridModelView> result = from boeResourceHours in
								 (from task in taskElementCollection
								  from boeResource in BRCValidationUtility.ProcessLaborTypesForBrc(task.taskElementLabors).ToList()
								  where boeResource.ResourceID.HasValue
								  select new
								  {
									  Hours = boeResource.SpreadType == SpreadType.Hours ? boeResource.ValueSpread : 0,
									  // include discrete cost values, but nothing derived from labor rates
									  Cost = boeResource.SpreadType == SpreadType.Cost ? Convert.ToDecimal(boeResource.ValueSpread) : 0,
									  ResourceID = boeResource.ResourceID.Value,
									  LaborType = this.GetBOESummaryResourceTypeText(boeResource, exportInputs.ResourcesUsedInWsBoes),
									  ElementOfCost = exportInputs.ResourcesUsedInWsBoes.First(x => x.Id == boeResource.ResourceID.Value).ElementOfCost
								  })
														  orderby boeResourceHours.LaborType
														  group boeResourceHours by boeResourceHours.LaborType into groupedBoeLaborTypes
														  select new BOESummaryGridModelView
														  {
															  LaborType = groupedBoeLaborTypes.Key,
															  TotalHours = groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.Hours),
															  TotalCost = groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.Cost),
															  Category = groupedBoeLaborTypes.First().ElementOfCost,
															  BOEID = boe.Id,
															  RollupCount = groupedBoeLaborTypes.Count(),
															  ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision
														  };

			summaryResults = result.ToList();

			return summaryResults;
		}

		/// <summary>
		/// LINQ through BOE DTO and pull out labor type id and the hours associated with the ID for summary grid display
		/// </summary>
		/// <param name="boe">The BOE</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>collection of MV for summary grid</returns>
		private List<BOESummaryGridModelView> GetODCAndCost(BoeDTO boe, BOEExportInputs exportInputs)
		{
			ICollection<OtherDirectCostDTO> odcElements = exportInputs.Odcs.Where(x => x.BoeID == boe.Id).ToList();

			IEnumerable<BOESummaryGridModelView> result = from boeODCCost in
							 (from task in odcElements
							  from boeResource in task.ODCTypes
							  where boeResource.ResourceID.HasValue
							  select new
							  {
								  Cost = boeResource.SpreadCurve == SpreadCurves.Load ? boeResource.Cost * (1 + BoeTaskElementRecalculation.numberOfMonthsBetweenTwoMMYYYYDates(boeResource.StartDate.Value, boeResource.EndDate.Value)) : boeResource.Cost,
								  LaborType = this.GetBOESummaryODCResourceTypeText(boeResource, exportInputs.ResourcesUsedInWsBoes)
							  })
														  orderby boeODCCost.LaborType
														  group boeODCCost by boeODCCost.LaborType into groupedBoeLaborTypes
														  select new BOESummaryGridModelView
														  {
															  LaborType = groupedBoeLaborTypes.Key,
															  TotalCost = (decimal)(groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.Cost)),
															  Category = ElementOfCostType.ODC,
															  BOEID = boe.Id,
															  RollupCount = groupedBoeLaborTypes.Count(),
															  ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision
														  };

			return result.ToList();
		}

		/// <summary>
		/// LINQ through BOE DTO and pull out labor type id and the hours associated with the ID for summary grid display
		/// </summary>
		/// <param name="boe">The BOE</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>collection of MV for summary grid</returns>
		private List<BOESummaryGridModelView> GetTravelAndCost(BoeDTO boe, BOEExportInputs exportInputs)
		{
			ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();

			IEnumerable<BOESummaryGridModelView> result = from boeODCCost in
							 (from task in travelElements
							  from boeResource in task.TravelTrips
							  select new
							  {
								  Cost = this.travelTripCostCalculator.CalculateTravelCost(boeResource, exportInputs.FullWorkspace).CostTotal,
								  LaborType = "Travel"
							  })
														  orderby boeODCCost.LaborType
														  group boeODCCost by boeODCCost.LaborType into groupedBoeLaborTypes
														  select new BOESummaryGridModelView
														  {
															  LaborType = groupedBoeLaborTypes.Key,
															  TotalCost = groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.Cost),
															  Category = ElementOfCostType.Travel,
															  BOEID = boe.Id,
															  RollupCount = groupedBoeLaborTypes.Count(),
															  ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision,
															  ModeID = MSTTravelMode.None
														  };

			return result.ToList();
		}

		/// <summary>
		/// Gets summary for each resourceid used in non-zone rms travel trip grouped by resource
		/// </summary>
		/// <param name="boe">The BOE</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>collection of MV for summary grid</returns>
		private List<BOESummaryGridModelView> GetRMSNonZoneTravelAndCost(BoeDTO boe, BOEExportInputs exportInputs)
		{

			ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();

			if (!travelElements.Any())
			{
				return new List<BOESummaryGridModelView>();
			}

			Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = this.rmsZoneTravelRatesFeesDataLoader.getAllFeesAndCostsByWorkspace(exportInputs.Workspace.Id).ToDictionary(f => f.ModeID);

			ICollection<WorkspaceRMSEscalationRatesDTO> rates = this.rmsZoneTravelRatesFeesDataLoader.getAllEscalationRatesByWorkspace(exportInputs.Workspace.Id);
			Dictionary<int, decimal> airfareEscalationRates = rates.ToDictionary(x => x.Year, y => y.AirfareRate);
			Dictionary<int, decimal> perDiemEscalationRates = rates.ToDictionary(x => x.Year, y => y.PerDiemRate);
			Dictionary<int, decimal> miscEscalationRates = rates.ToDictionary(x => x.Year, y => y.MiscRate);

			foreach (TravelDTO travelTask in travelElements) // need indiv. cost for each trip for summing by resourceid 
			{
				foreach (MSTTravelTripType rmsTrip in travelTask.MSTTravelTrips)
				{
					if (rmsTrip.ModeID == MSTTravelMode.NonZoneDomestic || rmsTrip.ModeID == MSTTravelMode.NonZoneInternational)
					{
						NonZoneTravelCalculation calculationClass =
						new NonZoneTravelCalculation(
							rmsTrip.NumOfDays.GetValueOrDefault(0),
							rmsTrip.NumOfPeople.GetValueOrDefault(0),
							rmsTrip.NonZoneNumCars ?? 0,
							rmsTrip.NonZonePerDiemDaily ?? 0,
							rmsTrip.NonZoneCarRentalTrans ?? 0,
							rmsTrip.NonZoneAirfareEstimate ?? 0,
							airfareEscalationRates,
							perDiemEscalationRates,
							miscEscalationRates,
							rmsTrip.EstimateDate.Year,
							rmsTrip.TripDate.Year,
							rmsTrip.ModeID == MSTTravelMode.NonZoneDomestic,
							fees[(int)rmsTrip.ModeID].TravelAgencyFee,
							fees[(int)rmsTrip.ModeID].MiscOther,
							exportInputs.Workspace.DecimalPrecision
						);
						rmsTrip.Cost = calculationClass.TotalTripCost;
					}
				}
			}

			List<BOESummaryGridModelView> result = (from rmsTravelTrips in
							 (from task in travelElements
							  from boeResource in task.MSTTravelTrips
							  select new
							  {
								  Cost = boeResource.Cost,
								  LaborType = boeResource.ResourceIdForExport
							  })
													orderby rmsTravelTrips.LaborType
													group rmsTravelTrips by rmsTravelTrips.LaborType into groupedBoeLaborTypes
													select new BOESummaryGridModelView
													{
														LaborType = groupedBoeLaborTypes.Key,
														TotalCost = groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.Cost),
														Category = ElementOfCostType.Travel,
														BOEID = boe.Id,
														RollupCount = groupedBoeLaborTypes.Count(),
														ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision
													}).ToList();

			return result;
		}

		/// <summary>
		/// Gets summary for each resourceid used in zone rms travel trip grouped by resource
		/// </summary>
		/// <param name="boe">The BOE</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>collection of MV for zone travel summary grid</returns>
		private List<BOESummaryGridModelView> GetRMSAirZoneTravel(BoeDTO boe, BOEExportInputs exportInputs)
		{
			ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();
			List<BOESummaryGridModelView> result = (from rmsTravelTrips in
							 (from task in travelElements
							  from boeResource in task.MSTTravelTrips.Where(t => t.ModeID == MSTTravelMode.ZoneAirfare)
							  select new
							  {
								  NumPeople = (int)(boeResource.NumOfPeople),
								  NumDays = (int)(boeResource.NumOfDays),
								  LaborType = boeResource.ResourceIdForExport
							  })
													orderby rmsTravelTrips.LaborType
													group rmsTravelTrips by rmsTravelTrips.LaborType into groupedBoeLaborTypes
													select new BOESummaryGridModelView
													{
														LaborType = groupedBoeLaborTypes.Key,
														Category = ElementOfCostType.Travel,
														BOEID = boe.Id,
														ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision,
														NumPeople = groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.NumPeople),
														NumDays = 0, // not shown for zone air trips
														RollupCount = groupedBoeLaborTypes.Count(),
														ModeID = MSTTravelMode.ZoneAirfare
													}).ToList();
			return result;
		}

		/// <summary>
		/// Gets summary for each resourceid used in zone rms travel trip grouped by resource
		/// </summary>
		/// <param name="boe">The BOE</param>
		/// <param name="exportInputs">The export inputs.</param>
		/// <returns>collection of MV for zone travel summary grid</returns>
		private List<BOESummaryGridModelView> GetRMSDomZoneTravel(BoeDTO boe, BOEExportInputs exportInputs)
		{
			ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();
			List<BOESummaryGridModelView> result = (from rmsTravelTrips in
							 (from task in travelElements
							  from boeResource in task.MSTTravelTrips.Where(t => t.ModeID == MSTTravelMode.ZoneNoAirfare)
							  select new
							  {
								  NumPeople = (int)(boeResource.NumOfPeople),
								  NumDays = (int)(boeResource.NumOfDays),
								  LaborType = boeResource.ResourceIdForExport
							  })
													orderby rmsTravelTrips.LaborType
													group rmsTravelTrips by rmsTravelTrips.LaborType into groupedBoeLaborTypes
													select new BOESummaryGridModelView
													{
														LaborType = groupedBoeLaborTypes.Key,
														Category = ElementOfCostType.Travel,
														BOEID = boe.Id,
														NumPeople = groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.NumPeople),
														NumDays = groupedBoeLaborTypes.Sum(boeLaborType => boeLaborType.NumDays),
														RollupCount = groupedBoeLaborTypes.Count(),
														ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision,
														ModeID = MSTTravelMode.ZoneNoAirfare
													}).ToList();
			return result;
		}

		/// <summary>
		/// Returns the text to display in BOE Summary based on the element of cost type of the resource.
		/// </summary>
		/// <param name="laborType">Resource-type entry</param>
		/// <param name="resourcesFromDb">Table of resource info</param>
		/// <returns>The text to display in BOE Summary based on the element of cost type of the resource.</returns>
		private string GetBOESummaryResourceTypeText(ResourceTypeDto laborType, IReadOnlyCollection<ResourceDTO> resourcesFromDb)
		{
			if (laborType.ResourceID.HasValue)
			{
				ResourceDTO resource = resourcesFromDb.FirstOrDefault(x => x.Id == laborType.ResourceID.Value);

				if (resource != null)
				{
					if (resource.ElementOfCost == ElementOfCostType.LMLabor)
					{
						return resource.LaborType;
					}
					else if (resource.ElementOfCost == ElementOfCostType.Materials || resource.ElementOfCost == ElementOfCostType.Travel || resource.ElementOfCost == ElementOfCostType.IWTA || resource.ElementOfCost == ElementOfCostType.Sub || resource.ElementOfCost == ElementOfCostType.ODC)
					{
						return resource.ElementOfCost.GetDescription();
					}
				}
			}

			return string.Empty;
		}

		/// <summary>
		/// returns the text to display in BOE summary based on the element of cost type of the resource.
		/// </summary>
		/// <param name="odcType">ODC Type</param>
		/// <param name="resourcesFromDb">Table of resource info</param>
		/// <returns>The text to display in BOE summary based on the element of cost type of the resource.</returns>
		private string GetBOESummaryODCResourceTypeText(OtherDirectCostType odcType, IReadOnlyCollection<ResourceDTO> resourcesFromDb)
		{
			if (odcType.ResourceID.HasValue)
			{
				ResourceDTO resource = resourcesFromDb.FirstOrDefault(x => x.Id == odcType.ResourceID.Value);

				if (resource != null)
				{
					if (resource.ElementOfCost == ElementOfCostType.ODC)
					{
						return resource.LaborType;
					}
				}
			}

			return string.Empty;
		}

		#endregion
	}
}