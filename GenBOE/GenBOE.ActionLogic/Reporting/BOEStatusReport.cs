// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export.BOE;
	using GenBOE.ActionLogic.Misc;
	using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Class to generate a ModelView used for BOE Status report display and export
    /// </summary>
    public class BOEStatusReport : IBOEStatusReport
    {
        private readonly string sEmpty = string.Empty;

        private ICommonDataMapper commonDataMapper;
        private IUserDTODataLoader userDTODataLoader;
        private IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation;
        private IPermissionsDTODataLoader permissionsLoader;
        private TravelTripCostCalculation travelTripCostCalculator;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BOEStatusReport"/> class.
        /// </summary>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="userDTODataLoader">The user dto data loader.</param>
        /// <param name="variableSelectBOEtoSumCalculation">The variable select bo eto sum calculation.</param>
        /// <param name="permissionsLoader">The permissions loader.</param>
        /// <param name="travelTripCostCalculator">The travel trip cost calculator.</param>
        public BOEStatusReport(
            ICommonDataMapper commonDataMapper,
            IUserDTODataLoader userDTODataLoader,
            IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation,
            IPermissionsDTODataLoader permissionsLoader,
            TravelTripCostCalculation travelTripCostCalculator)
        {
            this.commonDataMapper = commonDataMapper;
            this.userDTODataLoader = userDTODataLoader;
            this.variableSelectBOEtoSumCalculation = variableSelectBOEtoSumCalculation;
            this.permissionsLoader = permissionsLoader;
            this.travelTripCostCalculator = travelTripCostCalculator;
        }

        /// <summary>
        /// Generates a ModelViews used for BOE Status report display and export
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// Boe Status Report Model Views
        /// </returns>
        /// <exception cref="System.ArgumentNullException">exportInputs</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        public Collection<BOEStatusReportModelView> GenerateBOEStatusReport(BOEExportInputs exportInputs)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            List<BOEStatusReportModelView> toSortAndReturn = new List<BOEStatusReportModelView>();

			// Get all BOEs in the Workspace
			IReadOnlyCollection<BoeDTO> allBOEsInWorkspace = exportInputs.Boes;
            IDictionary<int,BOEStateModelView> boeStates = this.commonDataMapper.getBOEStatesDictionary();

            HashSet<PermissionsDTO> permissionsAssociatedWithBoes = new HashSet<PermissionsDTO>(this.permissionsLoader.GetBOEPermissions(allBOEsInWorkspace.Select(x => x.Id).Distinct().ToList()));
            HashSet<PermissionsDTO> authorsForBoes = new HashSet<PermissionsDTO>(permissionsAssociatedWithBoes.Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).ToCollection());
            HashSet<PermissionsDTO> approversForBoes = new HashSet<PermissionsDTO>(permissionsAssociatedWithBoes.Where(x => x.Role == Role.Approver).Select(x => x).ToCollection());
            HashSet<UserDTO> usersForBoe = new HashSet<UserDTO>(this.userDTODataLoader.GetByIds(permissionsAssociatedWithBoes.Select(x => x.ETIUserId).Distinct().ToList()));
            HashSet<TripDTO> allTravelTrips = new HashSet<TripDTO>(exportInputs.TravelTrips);
            HashSet<PerDiemDTO> allPerDiems = new HashSet<PerDiemDTO>(exportInputs.PerDiemsForTravelTrips);
            HashSet<EscalationRatesDTO> allEscalations = new HashSet<EscalationRatesDTO>(exportInputs.EscalationRates);
            HashSet<MiscTravelRateDTO> allMiscTravelRates = new HashSet<MiscTravelRateDTO>(exportInputs.MiscTravelRatesForTravelTrips);

            // Create a report model view for each BOE
            foreach (BoeDTO boe in allBOEsInWorkspace)
            {
                BOEStatusReportModelView modelView = new BOEStatusReportModelView();

                // Set WBS Data
                WbsDTO wbs = exportInputs.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
                modelView.ResourceDecimalPrecision = exportInputs.Workspace.DecimalPrecision;

                modelView.WBSNumber = (wbs == null ? "" : wbs.WbsNumber);
                modelView.WBSPaddedNumber = (wbs == null ? "" : wbs.WbsPaddedNumber);
                modelView.WBSTitle = (wbs == null ? "None" : wbs.WbsTitle);

                // Set CLIN Data
                ClinDTO clin = exportInputs.Clins.FirstOrDefault(x => x.Id == boe.CLINID);

                modelView.CLINNumber = (clin == null ? "" : clin.ClinNumber);
                modelView.CLINTitle = (clin == null ? "None" : clin.ClinTitle);
                modelView.CLINPaddedNumber = (clin == null ? "" : clin.ClinPaddedNumber);

                // Set BOE Data
                modelView.BOEID = boe.Id;
                modelView.StartDate = boe.StartDate;
                modelView.EndDate = boe.EndDate;
                modelView.BOETitle = boe.Title ?? this.sEmpty;

                ICollection<BoeTaskElementDTO> tasks = exportInputs.TaskElements.Where(x => x.BoeID == boe.Id).ToCollection();
                ICollection<TravelDTO> travelElements = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToCollection();
                ICollection<OtherDirectCostDTO> odcs = exportInputs.Odcs.Where(x => x.BoeID == boe.Id).ToCollection();

                modelView.TotalHours = (from taskElement in tasks
                                        from laborType in taskElement.taskElementLabors
                                        where laborType.SpreadType == SpreadType.Hours
                                              && laborType.ValueSpread.HasValue
                                        select laborType.ValueSpread.Value).Sum();
				
				// UCOT Data 
				FullWorkspace fullWorkspace = exportInputs.FullWorkspace;
				modelView.TotalUCOTHours = UCOTUtility.GetTaskElementsUCOTHours((IReadOnlyCollection<BoeTaskElementDTO>)tasks, fullWorkspace.MoqTypeSelections, fullWorkspace.ResourcesUsedInWsBoes, fullWorkspace.UCOTFactor, fullWorkspace.ResourceDecimalPrecision);
				modelView.TotalHoursWithUCOT = modelView.TotalHours + modelView.TotalUCOTHours;

				decimal taskCost = 0;

                // calculate labor cost
                foreach (ResourceTypeDto boeResource in tasks.SelectMany(x => x.taskElementLabors))
                {
                    taskCost += boeResource.SpreadType == SpreadType.Cost ? Convert.ToDecimal(boeResource.ValueSpread) : 0m;
                }

                decimal odcCost = ((decimal)(odcs.Sum(odc => odc.ODCTypes.Sum(odcTypes => odcTypes.ODCSpreads.Sum(odcSpreads => odcSpreads.CostSpreadValue))))) / 100;
                // Sum the trip data for all travel elements.
                decimal travelCost = 0;
                foreach (TravelDTO travel in travelElements)
                {
                    foreach (TravelTripType tripType in travel.TravelTrips)
                    {
                        TripDTO trip = allTravelTrips.First(i => i.TripID == tripType.SystemTripID);
                        MiscTravelRateDTO miscRateDTO = allMiscTravelRates.First(i => i.Id == trip.MiscTravelRateID);
                        PerDiemDTO perDiem = allPerDiems.First(i => i.Id == trip.PerDiemID);
                        travelCost += this.travelTripCostCalculator.CalculateTravelCost(tripType, exportInputs.FullWorkspace, trip, miscRateDTO.MiscTravelRate, perDiem, allEscalations).CostTotal;
                    }
                }
                
                modelView.TotalCost = taskCost + odcCost + travelCost;
                modelView.Status = boeStates[(int)boe.State].BOEState;
                modelView.isMaterial = boe.isMaterial;
                modelView.IsMultiClinWbs = boe.IsMultiClinWbs;
                modelView.WorkspaceID = exportInputs.Workspace.Id;

				// Set User Data
				Collection<PermissionsDTO> boeAuthors = authorsForBoes.Where(x => x.BOEId == boe.Id).ToCollection();
                if (boeAuthors.Any())
                {
					IEnumerable<string> authors = from author in boeAuthors
                                  where author.Role == Role.Author
                                  select usersForBoe.First(x => x.UserID == author.ETIUserId).DisplayName;

					IEnumerable<string> subcontractorAuthors = from author in boeAuthors
                                               where author.Role == Role.SubcontractorAuthor
                                               select usersForBoe.First(x => x.UserID == author.ETIUserId).DisplayName + " (Sub)";

					List<string> allAuthors = authors.Union(subcontractorAuthors).OrderBy(x => x).Distinct().ToList();
                    modelView.Authors = new Collection<string>(allAuthors.ToArray());
                }

				Collection<PermissionsDTO> boeApprovers = approversForBoes.Where(x => x.BOEId == boe.Id).ToCollection();
                if (boeApprovers.Any())
                {
					IEnumerable<string> approvers = from approver in boeApprovers
                                    select usersForBoe.First(x => x.UserID == approver.ETIUserId).DisplayName;

                    modelView.Approvers = new Collection<string>(approvers.ToArray());
                }

                // Add the model view to the list to sort and return
                toSortAndReturn.Add(modelView);
            }

            // Sort the modelViews, using custom comparer in the ModelView class for
            // desired multi-sort order
            toSortAndReturn.Sort();

            return new Collection<BOEStatusReportModelView>(toSortAndReturn.ToArray());
        }

        /// <summary>
        /// Send a BOEStatus report to file and return the file name
        /// </summary>
        /// <param name="templateFileLocation">The template file location.</param>
        /// <param name="statusReport">The generated status report</param>
        /// <param name="reportID">The report identifier.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// The path of the generated file
        /// </returns>
        [ExcludeFromCodeCoverage]
        public string SendBOEStatusReportToFile(string templateFileLocation, Collection<BOEStatusReportModelView> statusReport, int reportID, BOEExportInputs exportInputs)
        {
            if (templateFileLocation == null)
            {
                throw new ArgumentNullException(nameof(templateFileLocation));
            }

            if (statusReport == null)
            {
                throw new ArgumentNullException(nameof(statusReport));
            }

            string toReturn = "";

            if (statusReport.Any())
            {
				// Create all rows for the export file
				ExcelExportWorksheet worksheet = this.GetExcelExportWorksheet(statusReport, reportID, exportInputs);

                // Pass the rows to the generic Excel exporter
                toReturn = ExcelExporter.ExportToExcelFile(templateFileLocation, true, new List<ExcelExportWorksheet> { worksheet }, new int?[] { 1 });
                
            }

            // Return the file name of the Export File
            return toReturn;
        }

        /// <summary>
        /// Gets the excel export worksheet.
        /// </summary>
        /// <param name="statusReport">The status report.</param>
        /// <param name="reportID">The report identifier.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">exportInputs</exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmantainableCode"), SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists"), ExcludeFromCodeCoverage]
        public ExcelExportWorksheet GetExcelExportWorksheet(Collection<BOEStatusReportModelView> statusReport, int reportID, BOEExportInputs exportInputs)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

			// Pull Full Workspace to manipulate Report Data for UCOT
			FullWorkspace fullWorkspace = exportInputs.FullWorkspace;
			bool isUCOTEnabledForWorkspace = Utilities.ShowUCOTForWorkspace(fullWorkspace.CreationDate, fullWorkspace.Shortname);
			string hoursLabelUCOT = "Total UCOT " + FullObjectHelper.HoursLabel(exportInputs.Workspace);
			string grandTotalHoursLabel = "Grand Total " + FullObjectHelper.HoursLabel(exportInputs.Workspace);

			ExcelExportWorksheet toReturn = new ExcelExportWorksheet();
            string hoursFormatString = Utilities.PrecisionFormattingStringNoComma(exportInputs.Workspace.DecimalPrecision);
            string hoursLabel = "Total " + FullObjectHelper.HoursLabel(exportInputs.Workspace);

            switch (reportID)
            {
                case (int)Reports.BOEStatusByWBS:
					// Add Headers
					if (isUCOTEnabledForWorkspace)
					{
						toReturn.Add(ImportExportConstants.WBS_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER, ImportExportConstants.CLIN_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, hoursLabel, hoursLabelUCOT, grandTotalHoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER, ImportExportConstants.AUTHORS_COLUMN_HEADER, ImportExportConstants.APPROVERS_COLUMN_HEADER, ImportExportConstants.STATUS_COLUMN_HEADER, ImportExportConstants.MULTI_CLIN_COLUMN_HEADER, ImportExportConstants.MATERIAL_COLUMN_HEADER);
					}
					else
					{
						toReturn.Add(ImportExportConstants.WBS_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER, ImportExportConstants.CLIN_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, hoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER, ImportExportConstants.AUTHORS_COLUMN_HEADER, ImportExportConstants.APPROVERS_COLUMN_HEADER, ImportExportConstants.STATUS_COLUMN_HEADER, ImportExportConstants.MULTI_CLIN_COLUMN_HEADER, ImportExportConstants.MATERIAL_COLUMN_HEADER);
					}

                    if (statusReport.Any())
                    {
						IReadOnlyCollection<WbsDTO> allWBS = exportInputs.WbsElements;

                        var boesGroupedByWBS = from w in allWBS
                                               orderby w.WbsPaddedNumber
                                               select new
                                               {
                                                   WBS = w,
                                                   BOEs = (from b in statusReport
                                                           where b.WBSNumber == w.WbsNumber
                                                           select b).ToList()
                                               };

                        foreach (var wbs in boesGroupedByWBS)
                        {
                            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { WBSID = wbs.WBS.Id } } } },
                                new List<WorkspaceVariableDTO>() { new WorkspaceVariableDTO() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { WBSID = wbs.WBS.Id } } } },
                                exportInputs.WbsElements, exportInputs.Boes, exportInputs.TaskElements, exportInputs.ResourcesForWsResourceListId, exportInputs.Clins);

                            if (wbs.BOEs.Any())
                            {
								if (isUCOTEnabledForWorkspace)
								{
									toReturn.Add(
										wbs.WBS.WbsString,
										string.Empty,
										string.Empty,
										string.Empty,
										string.Empty,
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + this.variableSelectBOEtoSumCalculation.GetTotalBasedOnWBSID(wbs.WBS.Id,
											this.GetResourceTypesToBeSummed(), data).ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + wbs.BOEs.Sum(x => x.TotalUCOTHours).ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + wbs.BOEs.Sum(x => x.TotalHoursWithUCOT).ToString(hoursFormatString));
								}
								else
								{
									toReturn.Add(
										wbs.WBS.WbsString,
										string.Empty,
										string.Empty,
										string.Empty,
										string.Empty,
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + this.variableSelectBOEtoSumCalculation.GetTotalBasedOnWBSID(wbs.WBS.Id,
											this.GetResourceTypesToBeSummed(), data).ToString(hoursFormatString));
								}
                            }

                            foreach (BOEStatusReportModelView boe in wbs.BOEs)
                            {
								if (isUCOTEnabledForWorkspace)
								{
									toReturn.Add(
										Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " "),
										boe.BOETitle,
										Utilities.FormatNumberTitleString(boe.CLINNumber, boe.CLINTitle, " "),
										string.Format("{0: MM/yyyy}", boe.StartDate),
										string.Format("{0: MM/yyyy}", boe.EndDate),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHours.ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalUCOTHours.ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHoursWithUCOT.ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe.TotalCost),
										string.Join("; ", boe.Authors),
										string.Join("; ", boe.Approvers),
										boe.Status,
										boe.IsMultiClinWbs ? "Yes" : "No",
										boe.isMaterial ? "Yes" : "No");
								}
								else
								{
									toReturn.Add(
										Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " "),
										boe.BOETitle,
										Utilities.FormatNumberTitleString(boe.CLINNumber, boe.CLINTitle, " "),
										string.Format("{0: MM/yyyy}", boe.StartDate),
										string.Format("{0: MM/yyyy}", boe.EndDate),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHours.ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe.TotalCost),
										string.Join("; ", boe.Authors),
										string.Join("; ", boe.Approvers),
										boe.Status,
										boe.IsMultiClinWbs ? "Yes" : "No",
										boe.isMaterial ? "Yes" : "No");
								}
                            }
                        }
                    }
                    break;

                case (int)Reports.BOEStatusByCLIN:
					// Add Headers
					if (isUCOTEnabledForWorkspace)
					{
 						toReturn.Add(ImportExportConstants.CLIN_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER, ImportExportConstants.WBS_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, hoursLabel, hoursLabelUCOT, grandTotalHoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER, ImportExportConstants.AUTHORS_COLUMN_HEADER, ImportExportConstants.APPROVERS_COLUMN_HEADER, ImportExportConstants.STATUS_COLUMN_HEADER, ImportExportConstants.MULTI_CLIN_COLUMN_HEADER, ImportExportConstants.MATERIAL_COLUMN_HEADER);
					}
					else
					{
						toReturn.Add(ImportExportConstants.CLIN_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER, ImportExportConstants.WBS_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, hoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER, ImportExportConstants.AUTHORS_COLUMN_HEADER, ImportExportConstants.APPROVERS_COLUMN_HEADER, ImportExportConstants.STATUS_COLUMN_HEADER, ImportExportConstants.MULTI_CLIN_COLUMN_HEADER, ImportExportConstants.MATERIAL_COLUMN_HEADER);
					}

                    if (statusReport.Count > 0)
                    {
						IReadOnlyCollection<ClinDTO> allClin = exportInputs.Clins;

                        var boesGroupedByClin = from c in allClin
							orderby c.ClinPaddedNumber
							select new
							{
								CLINTitle = c.ClinString,
								BOEs = from b in statusReport
										where b.CLINNumber == c.ClinNumber
										select b
							};

                        foreach (var clin in boesGroupedByClin)
                        {
                            if (clin.BOEs.Any())
                            {
								if (isUCOTEnabledForWorkspace)
								{
									toReturn.Add(
										clin.CLINTitle,
										string.Empty,
										string.Empty,
										string.Empty,
										string.Empty,
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + clin.BOEs.Sum(b => b.TotalHours).ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + clin.BOEs.Sum(b => b.TotalUCOTHours).ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + clin.BOEs.Sum(b => b.TotalHoursWithUCOT).ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, clin.BOEs.Sum(b => b.TotalCost)));
								}
								else
								{
									toReturn.Add(
										clin.CLINTitle,
										string.Empty,
										string.Empty,
										string.Empty,
										string.Empty,
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + clin.BOEs.Sum(b => b.TotalHours).ToString(hoursFormatString),
										CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, clin.BOEs.Sum(b => b.TotalCost)));
								}

								foreach (BOEStatusReportModelView boe in clin.BOEs)
								{
									if (isUCOTEnabledForWorkspace)
									{
										toReturn.Add(
											clin.CLINTitle,
											boe.BOETitle,
											Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " "),
											string.Format("{0: MM/yyyy}", boe.StartDate),
											string.Format("{0: MM/yyyy}", boe.EndDate),
											CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHours.ToString(hoursFormatString),
											CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalUCOTHours.ToString(hoursFormatString),
											CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHoursWithUCOT.ToString(hoursFormatString),
											CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe.TotalCost),
											string.Join("; ", boe.Authors),
											string.Join("; ", boe.Approvers),
											boe.Status,
											boe.IsMultiClinWbs ? "Yes" : "No",
											boe.isMaterial ? "Yes" : "No");
									}
									else
									{
										toReturn.Add(
											clin.CLINTitle,
											boe.BOETitle,
											Utilities.FormatNumberTitleString(boe.WBSNumber, boe.WBSTitle, " "),
											string.Format("{0: MM/yyyy}", boe.StartDate),
											string.Format("{0: MM/yyyy}", boe.EndDate),
											CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHours.ToString(hoursFormatString),
											CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe.TotalCost),
											string.Join("; ", boe.Authors),
											string.Join("; ", boe.Approvers),
											boe.Status,
											boe.IsMultiClinWbs ? "Yes" : "No",
											boe.isMaterial ? "Yes" : "No");
									}
								}
                            }
                        }
                    }
                    break;

                default:
					if (isUCOTEnabledForWorkspace)
					{
						// Add Headers
						toReturn.Add(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER, ImportExportConstants.WBS_TITLE_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER, ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER, ImportExportConstants.CLIN_TITLE_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, hoursLabel, hoursLabelUCOT, grandTotalHoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER, ImportExportConstants.AUTHORS_COLUMN_HEADER, ImportExportConstants.APPROVERS_COLUMN_HEADER, ImportExportConstants.STATUS_COLUMN_HEADER, ImportExportConstants.MULTI_CLIN_COLUMN_HEADER, ImportExportConstants.MATERIAL_COLUMN_HEADER);

						toReturn.AddRange(
						from boe in statusReport
						select new Collection<string>
						{
							boe.WBSNumber,
							boe.WBSTitle,
							boe.BOETitle,
							boe.CLINNumber,
							boe.CLINTitle,
							string.Format("{0: MM/yyyy}", boe.StartDate),
							string.Format("{0: MM/yyyy}", boe.EndDate),
							CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHours.ToString(hoursFormatString),
							CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalUCOTHours.ToString(hoursFormatString),
							CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHoursWithUCOT.ToString(hoursFormatString),
							CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe.TotalCost),
							string.Join("; ", boe.Authors),
							string.Join("; ", boe.Approvers),
							boe.Status,
							boe.IsMultiClinWbs ? "Yes" : "No",
							boe.isMaterial ? "Yes" : "No"
						});
					}
					else
					{
						// Add Headers
						toReturn.Add(ImportExportConstants.WBS_NUMBER_COLUMN_HEADER, ImportExportConstants.WBS_TITLE_COLUMN_HEADER, ImportExportConstants.BOE_TITLE_COLUMN_HEADER, ImportExportConstants.CLIN_NUMBER_COLUMN_HEADER, ImportExportConstants.CLIN_TITLE_COLUMN_HEADER, ImportExportConstants.START_DATE_COLUMN_HEADER, ImportExportConstants.END_DATE_COLUMN_HEADER, hoursLabel, ImportExportConstants.TOTAL_COST_COLUMN_HEADER, ImportExportConstants.AUTHORS_COLUMN_HEADER, ImportExportConstants.APPROVERS_COLUMN_HEADER, ImportExportConstants.STATUS_COLUMN_HEADER, ImportExportConstants.MULTI_CLIN_COLUMN_HEADER, ImportExportConstants.MATERIAL_COLUMN_HEADER);

						toReturn.AddRange(
						from boe in statusReport
						select new Collection<string>
						{
							boe.WBSNumber,
							boe.WBSTitle,
							boe.BOETitle,
							boe.CLINNumber,
							boe.CLINTitle,
							string.Format("{0: MM/yyyy}", boe.StartDate),
							string.Format("{0: MM/yyyy}", boe.EndDate),
							CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + boe.TotalHours.ToString(hoursFormatString),
							CommonConstants.FORCE_AS_NUMBER_FOR_EXCEL + string.Format(Constants.MONEY_FORMATTING, boe.TotalCost),
							string.Join("; ", boe.Authors),
							string.Join("; ", boe.Approvers),
							boe.Status,
							boe.IsMultiClinWbs ? "Yes" : "No",
							boe.isMaterial ? "Yes" : "No"
						});
					}
                    
                    break;
            }


            return toReturn;
        }

        /// <summary>
        /// Return a collection of Resource Types to be summed.
        /// </summary>
        /// <returns>A collection of Resource Types to be summed.</returns>
        public virtual Collection<int> GetResourceTypesToBeSummed()
        {
            return new Collection<int>
            {
                (int)SumVariableResourceType.DSLabor,
                (int)SumVariableResourceType.ESLabor,
                (int)SumVariableResourceType.TSLabor,
                (int)SumVariableResourceType.LSLabor,
                (int)SumVariableResourceType.LOEIWTA,
                (int)SumVariableResourceType.LOESub
            };
        }
    }
}
