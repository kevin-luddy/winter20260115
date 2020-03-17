// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.Common.Email;
    using GenBOE.ActionLogic.CopyBOE;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.ActionLogic.WBS;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.ActionLogic.ZoneTravel;
    using GenBOE.DataBridge.Common.Interfaces;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;

    public class BOEControllerLogicMST : BOEControllerLogic
    {
        private IMSTMetricLoader mstMetricsLoader;
        private IOffloadRatesDTOLoader offloadRatesDTOLoader;

        #region Protected Properties and Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEControllerLogicMST(
            IMSTMetricLoader mstMetricsLoader,
            IBOESummary inBOESummary,
            IUserDTODataLoader inUserLoader,
            IActiveDirectoryUtilities inActiveDirectoryUtil,
            IPermissionsDTODataLoader inPermissionsLoader,
            IFullObjectFactory inFactory,
            IBOEExporter inBOEExporter,
            IBOECustomExporter inBoeCustomExporter,
            IGenBOEControllerLogic inGenBOEControllerLogic,
            IBoeMediator inBoeMediator,
            IValidationHelper inValidationHelper,
            IBOECommentDTODataLoader inBoeCommentLoader,
            IBoeEmailer inEmailer,
            IBoeTaskElementMediator inBoeTaskElementMediator,
            IWorkspaceVariableDTODataLoader inWorkspaceVariableLoader,
            IBOEStateMachine inBOEStateMachine,
            IVariableSelectBOEtoSumCalculation inVariableSelectBOEtoSumCalculation,
            IBOELaborControllerLogic inBOELaborControllerLogic,
            IValidateBOE inValidateBOE,
            ISecurityInformation inSecurityInformation,
            IBOESearchDTODataLoader inBoeSearchLoader,
            ISecurityAccess inSecurityAccess,
            IBoeTaskElementRecalculation inBoeTaskElementRecalculation,
            IBOEImporter inBOEImporter,
            IVariableCircularReferenceChecker inVariableCircularReferenceChecker,
            IConflictBOE inConflictBOE,
            INestedWBSUtilities inNestedWBSUtilities,
            IOffloadRatesDTOLoader offloadRatesDTOLoader,
            IProjectMapDataLoader projectMapLoader,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader,
            IRteTemplateDataLoader rteTemplateDataLoader
            )
            : base(inBOESummary, inUserLoader, inActiveDirectoryUtil, 
            inPermissionsLoader, inFactory, inBOEExporter, inBoeCustomExporter, inGenBOEControllerLogic,
            inBoeMediator, inValidationHelper, inBoeCommentLoader, inEmailer, inBoeTaskElementMediator, inWorkspaceVariableLoader, inBOEStateMachine,
            inVariableSelectBOEtoSumCalculation, inBOELaborControllerLogic, inValidateBOE, inSecurityInformation, inBoeSearchLoader, inSecurityAccess,
            inBoeTaskElementRecalculation, inBOEImporter, inVariableCircularReferenceChecker, inConflictBOE, inNestedWBSUtilities, projectMapLoader, zoneTravelRatesFeesLoader,
            rteTemplateDataLoader)
        {
            this.mstMetricsLoader = mstMetricsLoader;
            this.offloadRatesDTOLoader = offloadRatesDTOLoader;
        }

        #endregion

        /// <summary>
        /// Get BOE Header Model View
        /// </summary>
        /// <param name="boe">The <see cref="BoeDTO" /> used to populate the <see cref="BOEHeaderISGSModelView" /></param>
        /// <returns>
        /// the populated <see cref="BOEHeaderISGSModelView" />
        /// </returns>
        public override IBOEHeaderModelView GetCreateBOEHeaderMV(BoeDTO boe, ICollection<RTECustomTemplateQuestionAnswerModelView> answers)
        {
            // The header requirements are identical to SSC implementation so we can re-use it
            return new BOEHeaderSpaceModelView(boe, answers);
        }

        /// <summary>
        /// Returns a bool indicating if historic metrics should be shown
        /// </summary>
        /// <param name="ids">The id's of the boe's to check</param>
        /// <returns>true if the metrics should be shown, false otherwise</returns>
        public override bool ShowHistoricMetricCheck(ICollection<int> ids)
        {
            // Same as Space Systems but is repeated here since it is only two lines of code.
            Collection<MSTMetricDetailsDTO> metrics = this.mstMetricsLoader.GetByBoeIds(ids).ToCollection();
            return metrics.Any();
        }

        /// <summary>
        /// Gets company specific header information for the manage Boe page.
        /// </summary>
        /// <returns>Header Info for Manage Boe</returns>
        public override string GetCompanySpecificManageBoeHeaderInfo
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the Boe offload data.
        /// </summary>
        /// <param name="ws">The full workspace.</param>
        /// <param name="boeId">The boe identifier.</param>
        /// <returns>A Model View housing data for a Boe Offload</returns>
        public override BoeOffloadModelView RetrieveBoeOffloadData(FullWorkspace ws, int boeId)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            ICollection<FullBoe> boes = ws.Boes.Where(b => b.Id == boeId).ToList();
            if (boes.None())
            {
                throw new ArgumentException($"The boe Id {boeId} passed is invalid.");
            }

            OffloadLaborRates offloadRates = new OffloadLaborRates();
            BoeOffloadModelView model = new BoeOffloadModelView
            {
                BoeId = boeId
            };
            
            FullBoe offloadedBoe = boes.First();
            List<int> offloadedResourceTypeIds = new List<int>();

            try
            {
                OffloadLaborRatesResults results = offloadRates.OffloadWorkspace(boes, ws);

                offloadedBoe = results.Boes.First();
                
                model = new BoeOffloadModelView
                {
                    IsMultiClinWbs = offloadedBoe.IsMultiClinWbs,
                    BoeId = boeId,
                    Clin = offloadedBoe.Clin?.ClinString ?? string.Empty,
                    Wbs = offloadedBoe.Wbs?.WbsString ?? string.Empty,
                    CostPrecision = ws.CostDecimalPrecision,
                    ResourceHoursPrecision = ws.ResourceDecimalPrecision,
                    NoResultsMessage = "No Labor Types were offloaded in this BOE."
                };

                List<ResourceTypeDto> resourceTypes = offloadedBoe.TaskElements.SelectMany(te => te.taskElementLabors).ToList();
                DateTime spreadStartDate = resourceTypes.Where(s => s.StartDate.HasValue).Select(s => s.StartDate.Value).Min().Normalize(DateTimePrecision.Month);
                DateTime spreadEndDate = resourceTypes.Where(s => s.EndDate.HasValue).Select(s => s.EndDate.Value).Max().Normalize(DateTimePrecision.Month);

                model.SpreadDates = spreadStartDate.CreateSpreadDateRange(spreadEndDate);

                // Add the Offloaded Resources
                foreach (ResourceTypeDto item in resourceTypes)
                {
                    SubResourceTypeDto offloadedResourceType = item as SubResourceTypeDto;
                    if (offloadedResourceType != null)
                    {
                        ResourceTypeDto originalResourceType = resourceTypes.First(rt => rt.Id == offloadedResourceType.InHouseResourceTypeId);
                        offloadedResourceTypeIds.Add(originalResourceType.Id);
                        offloadedResourceTypeIds.Add(offloadedResourceType.Id);

                        BoeOffloadResourceModelView offloadedResource = new BoeOffloadResourceModelView
                        {
                            OffloadedResource = offloadedResourceType.SubResourceName,
                            Clin = offloadedBoe.IsMultiClinWbs ? string.Empty : ws.Clins.FirstOrDefault(c => c.Id == offloadedResourceType.CLINID)?.ClinString ?? string.Empty,
                            Wbs = offloadedBoe.IsMultiClinWbs ? string.Empty : ws.WbsElements.FirstOrDefault(w => w.Id == offloadedResourceType.WBSID)?.WbsString ?? string.Empty,
                            TotalOffloadedCost = Utilities.FormatStringWithPrecision(offloadedResourceType.ValueSpread.Value, ws.CostDecimalPrecision),
                            ExistingModifiedHours = Utilities.FormatStringWithPrecision(originalResourceType.ValueSpread.Value, ws.ResourceDecimalPrecision),
                            StartDate = offloadedResourceType.StartDateValue.ToMonthString(),
                            EndDate = offloadedResourceType.EndDateValue.ToMonthString(),
                            ExistingSpreadCurve = originalResourceType.SpreadCurveID.Value.GetDescription(),
                            TotalHoursOffloaded = Utilities.FormatStringWithPrecision(offloadedResourceType.OffLoadedHourSpreads.Sum(ls => ls.LaborSpreadValue), ws.ResourceDecimalPrecision),
                            ExistingResource = ws.ResourcesForWsResourceListId.FirstOrDefault(r => r.Id == originalResourceType.ResourceID)?.ResourceName ?? string.Empty,
                            PerformingOrg = ws.PerformingOrgsForWsList.FirstOrDefault(p => p.Id == originalResourceType.PerformingOrgID)?.PerformingOrgName ?? string.Empty,
                            ExistingSpreads = originalResourceType.LaborSpreads,
                            OffloadSpreads = offloadedResourceType.LaborSpreads,
                            OffloadHours = offloadedResourceType.OffLoadedHourSpreads
                        };

                        model.OffloadResources.Add(offloadedResource);
                    }
                }

            }
            catch (GenValidationException)
            {
                // this is thrown when there are missing years.  Show this warning/exception to the user below
                model.NoResultsMessage = "Validation Errors stopped Offload from completing.  See below.";
            }

            // Add the Offload Warnings/Errors
            ICollection<OffloadRatesDTO> offloadRatesDtos = this.offloadRatesDTOLoader.GetByWorkspaceId(ws.Id);

            // loop over offloadedBoe since it is set as original BOE in one path, and as the actual offloaded (updated) BOE in other path
            foreach (ResourceTypeDto item in offloadedBoe.TaskElements.SelectMany(te => te.taskElementLabors))
            {
                if (item.CanOffload && !offloadedResourceTypeIds.Contains(item.Id))
                {
                    OffloadLaborRatesValidationResults validationResults = OffloadLaborRates.ValidateLaborResourceCanOffload(item, offloadRatesDtos, ws);
                    if (!validationResults.IsValid)
                    {
                        model.OffloadWarnings.Add(validationResults);
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Calculates the total cost of travel for all MSTTravelTrips for the BOE.
        /// </summary>
        /// <param name="travels">The travel elements.</param>
        /// <param name="decimalPrecision">The decimal precision for the workspace.</param>
        /// <param name="escalationRates">The escalation rates.</param>
        /// <param name="fees">The fees.</param>
        /// <returns>Total cost of travel for all MSTTravelTrips for the BOE.</returns>
        internal override decimal CalculateTotalCostTravel(ICollection<TravelDTO> travels, int decimalPrecision, ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates, Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees)
        {
            decimal totalCostTravel = 0;

            if (travels != null)
            {
                foreach (TravelDTO travelDto in travels)
                {
                    if (travelDto.MSTTravelTrips != null)
                    {
                        foreach (MSTTravelTripType trip in travelDto.MSTTravelTrips)
                        {
                            if (trip.ModeID == MSTTravelMode.NonZoneDomestic ||
                                trip.ModeID == MSTTravelMode.NonZoneInternational)
                            {
                                NonZoneTravelCalculation nztc = new NonZoneTravelCalculation(
                                    trip.NumOfDays ?? 0,
                                    trip.NumOfPeople ?? 0,
                                    trip.NonZoneNumCars ?? 0,
                                    trip.NonZonePerDiemDaily ?? 0,
                                    trip.NonZoneCarRentalTrans ?? 0,
                                    trip.NonZoneAirfareEstimate ?? 0,
                                    escalationRates.ToDictionary(rate => rate.Year, rate => rate.AirfareRate),
                                    escalationRates.ToDictionary(rate => rate.Year, rate => rate.PerDiemRate),
                                    escalationRates.ToDictionary(rate => rate.Year, rate => rate.MiscRate),
                                    trip.EstimateDate.Year,
                                    trip.TripDate.Year,
                                    trip.ModeID == MSTTravelMode.NonZoneDomestic,
                                    fees[(int)trip.ModeID].TravelAgencyFee,
                                    fees[(int)trip.ModeID].MiscOther,
                                    decimalPrecision);

                                totalCostTravel += nztc.TotalTripCost;
                            }
                        }
                    }
                }
            }

            return totalCostTravel;
        }
    }
}
