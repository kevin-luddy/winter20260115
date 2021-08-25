// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export.BOE
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.ZoneTravel;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// The BOE Export Converter for RMS.
    /// </summary>
    /// <seealso cref="GenBOE.ActionLogic.IO.Export.BOE.BOEExportConverter" />
    public class BOEExportConverterRMS : BOEExportConverter
    {
        /// <summary>
        /// The MST zone travel resource dto data loader.
        /// </summary>
        private MSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader;

        /// <summary>
        /// The RMS zone travel rates fees data loader.
        /// </summary>
        private RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="BOEExportConverterRMS" /> class.
        /// </summary>
        /// <param name="userDTODataLoader">The user dto data loader.</param>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="variableSelectBOEtoSumCalculation">The variable select bo eto sum calculation.</param>
        /// <param name="travelTripCostCalculation">The travel trip cost calculation.</param>
        /// <param name="mstZoneTravelResourceDTODataLoader">The MST zone travel resource dto data loader.</param>
        /// <param name="rmsZoneTravelRatesFeesDataLoader">The RMS zone travel rates fees data loader.</param>
        public BOEExportConverterRMS(IUserDTODataLoader userDTODataLoader, ICommonDataMapper commonDataMapper, IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation,
            TravelTripCostCalculation travelTripCostCalculation, MSTZoneTravelResourceDTODataLoader mstZoneTravelResourceDTODataLoader,
            RMSZoneTravelRatesFeesDataLoader rmsZoneTravelRatesFeesDataLoader)
                    : base(userDTODataLoader, commonDataMapper, variableSelectBOEtoSumCalculation, travelTripCostCalculation)
        {
            this.mstZoneTravelResourceDTODataLoader = mstZoneTravelResourceDTODataLoader;
            this.rmsZoneTravelRatesFeesDataLoader = rmsZoneTravelRatesFeesDataLoader;
        }

        /// <summary>
        /// Gets custom field values for mst
        /// </summary>
        /// <param name="workspaceCustomFields">custom fields in the workspace</param>
        /// <param name="workspaceCustomFieldValues">custom field values in the workspace</param>
        /// <param name="boe">boe with custom fields</param>
        /// <param name="boeExportModelView">export model view for the boe</param>
        protected override void GetCompanySpecificBOECustomFields(IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields, IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues, BoeDTO boe, BOEExportModelView boeExportModelView)
        {
            if (boeExportModelView != null)
            {
                // get IPT/Sub IPT Custom fields for MST Templates
                CustomFieldDTO iptCustomField = (from c in workspaceCustomFields
                                                 where c.CustomFieldName.Equals("IPT/Sub IPT", StringComparison.CurrentCultureIgnoreCase) &&
                                                 c.CustomFieldDisplayID == CustomFieldType.BoeDisplay
                                                 select c).FirstOrDefault();

                if (iptCustomField != null)
                {
                    CustomFieldValueDTO ipt = (from v in workspaceCustomFieldValues
                                               from c in boe.CustomFieldValueContainers
                                               where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == iptCustomField.Id
                                               select v).FirstOrDefault();

                    if (ipt != null)
                    {
                        boeExportModelView.ExportFields[BOEExporter.FieldName_IPT] = string.Concat(
                            ipt.CustomFieldValueName ?? string.Empty,
                            " ",
                            ipt.CustomFieldValueDescription ?? string.Empty);
                    }
                }

                // get Asset custom fields for MST Templates
                CustomFieldDTO assetCustomField = (from c in workspaceCustomFields
                                                   where c.CustomFieldName.Equals("Asset", StringComparison.CurrentCultureIgnoreCase) &&
                                                   c.CustomFieldDisplayID == CustomFieldType.BoeDisplay
                                                   select c).FirstOrDefault();
                if (assetCustomField != null)
                {
                    CustomFieldValueDTO asset = (from v in workspaceCustomFieldValues
                                                 from c in boe.CustomFieldValueContainers
                                                 where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == assetCustomField.Id
                                                 select v).FirstOrDefault();

                    if (asset != null)
                    {
                        boeExportModelView.ExportFields[BOEExporter.FieldName_Asset] = string.Concat(
                            asset.CustomFieldValueName ?? string.Empty,
                            " ",
                            asset.CustomFieldValueDescription ?? string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Gets task level custom field values for mst
        /// </summary>
        /// <param name="workspaceCustomFields">custom fields in the workspace</param>
        /// <param name="workspaceCustomFieldValues">custom field values in the workspace</param>
        /// <param name="boeTaskElement">task element with custom fields</param>
        /// <param name="boeExportTaskElement">export task element for the task</param>
        protected override void GetCompanySpecificTaskCustomFields(IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields, IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues, BoeTaskElementDTO boeTaskElement, BOEExportTaskElement boeExportTaskElement)
        {
            if (boeExportTaskElement != null)
            {
                // get Cost Center Custom fields for MST Templates
                this.setTaskCustomField(workspaceCustomFields, workspaceCustomFieldValues, boeTaskElement, boeExportTaskElement, "Cost Center", BOEExporter.FieldName_TaskCostCenter, true);

                // get Estimate Method Custom Fields
                this.setTaskCustomField(workspaceCustomFields, workspaceCustomFieldValues, boeTaskElement, boeExportTaskElement, BOEExporterConstants.CustomFieldName_EstimateMethod, BOEExporter.FieldName_EstimateMethod, false);
                
                // get Company Custom Fields
                this.setTaskCustomField(workspaceCustomFields, workspaceCustomFieldValues, boeTaskElement, boeExportTaskElement, BOEExporterConstants.CustomFieldName_Company, BOEExporter.FieldName_Company, false);
            }
        }

        /// <summary>
        /// Set the Export Field for the given Task Custom Field
        /// </summary>
        /// <param name="workspaceCustomFields">Workspace Custom Fields</param>
        /// <param name="workspaceCustomFieldValues">Workspace Custom Field Values</param>
        /// <param name="boeTaskElement">BOE Task ELement</param>
        /// <param name="boeExportTaskElement">BOE Export Task Element</param>
        /// <param name="customFieldName">Name of the Custom Field</param>
        /// <param name="fieldName">Field name for the Export Field</param>
        /// <param name="includeId">Bool noting if custom field ID should be included in text</param>
        private void setTaskCustomField(IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields, IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues, BoeTaskElementDTO boeTaskElement, BOEExportTaskElement boeExportTaskElement, string customFieldName, string fieldName, bool includeId)
        {
            CustomFieldDTO customField = (from c in workspaceCustomFields
                where c.CustomFieldName.Equals(customFieldName, StringComparison.CurrentCultureIgnoreCase) &&
                      c.CustomFieldDisplayID == CustomFieldType.TaskDisplay
                select c).FirstOrDefault();

            if (customField != null)
            {
                CustomFieldValueDTO customFieldValue = (from v in workspaceCustomFieldValues
                    from c in boeTaskElement.CustomFieldValueContainers
                    where v.CustomFieldValueID == c.CustomFieldValueID &&
                          v.CustomFieldID == customField.Id
                    select v).FirstOrDefault();

                if (customFieldValue != null)
                {
                    boeExportTaskElement.ExportFields[fieldName] = includeId
                        ? string.Concat(
                            customFieldValue.CustomFieldValueName ?? string.Empty,
                            " ",
                            customFieldValue.CustomFieldValueDescription ?? string.Empty)
                        : customFieldValue.CustomFieldValueDescription ?? string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Populates the Travel Tasks in the export
        /// </summary>
        /// <param name="boe">BOE DTO containing the Travel Tasks</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="writelogstatements">Bool noting to write logs</param>
        /// <param name="boeExportTaskElements">BOE Export Task Elements</param>
        /// <exception cref="System.ArgumentNullException">
        /// exportInputs
        /// or
        /// BOEExportTaskElements
        /// </exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        protected override void PopulateTravelTasks(BoeDTO boe, BOEExportInputs exportInputs, bool writelogstatements, Collection<BOEExportTaskElement> boeExportTaskElements)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            if (boeExportTaskElements == null)
            {
                throw new ArgumentNullException(nameof(boeExportTaskElements));
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - travel tasks begin");
            }

            ICollection<TravelDTO> travelTasks = exportInputs.Travels.Where(x => x.BoeID == boe.Id).ToList();

            ICollection<MSTZoneTravelResourceDTO> zoneTravelResources = new Collection<MSTZoneTravelResourceDTO>();
            ICollection<ResourceDTO> nonzoneTravelResources = new Collection<ResourceDTO>();

            ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates = new Collection<WorkspaceRMSEscalationRatesDTO>();

            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees = new Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO>();

            // only grab the travel resources if there are travel tasks
            if (travelTasks.Any())
            {
                zoneTravelResources = this.mstZoneTravelResourceDTODataLoader.GetAllResources();
                nonzoneTravelResources = exportInputs.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel).ToCollection();
                escalationRates = this.rmsZoneTravelRatesFeesDataLoader.getAllEscalationRatesByWorkspace(exportInputs.Workspace.Id);
                fees = this.rmsZoneTravelRatesFeesDataLoader.getAllFeesAndCostsByWorkspace(exportInputs.Workspace.Id).ToDictionary(f => f.ModeID);
            }

            IReadOnlyCollection<PerformingOrgDTO> perfOrgsFromDb = exportInputs.PerformingOrgsUsedInBoes;

            foreach (TravelDTO travelElement in travelTasks)
            {
                BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
                boeExportTaskElement.BoeID = boe.Id;
                boeExportTaskElement.BOETaskDesc = travelElement.Description;
                boeExportTaskElement.BOETaskElementID = travelElement.Id;
                boeExportTaskElement.TaskTitle = travelElement.TaskTitle;
                boeExportTaskElement.BOETaskID = travelElement.TaskID;
                boeExportTaskElement.StartDate = travelElement.StartDate ?? boe.StartDate;
                boeExportTaskElement.EndDate = travelElement.EndDate ?? boe.EndDate;
                boeExportTaskElement.ElementType = BOEExportTaskElementType.Travel;
                boeExportTaskElement.BOETaskElementOrder = travelElement.BOETaskElementOrder;

                // get Task Element Labors
                Collection<BOEExportTaskElementLabor> boeExportLabors = new Collection<BOEExportTaskElementLabor>();

                foreach (MSTTravelTripType travelTrip in travelElement.MSTTravelTrips)
                {
                    BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);
                    boeExportLabor.Cost = 0m;
                    string destinationLocation;
                    string departureLocation;
                    if (travelTrip.ModeID == MSTTravelMode.NonZoneDomestic || travelTrip.ModeID == MSTTravelMode.NonZoneInternational)
                    {
                        boeExportLabor.Cost = this.CalculateNonzoneTravelTripCost(exportInputs, travelTrip, escalationRates, fees);
                        departureLocation = travelTrip.NonZoneFrom;
                        destinationLocation = travelTrip.NonZoneTo;

                        ResourceDTO resource = nonzoneTravelResources.FirstOrDefault(r => r.Id == travelTrip.NonZoneResourceID);
                        if (resource != null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceID] = resource.Id.ToString();
                            boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceName] = resource.ResourceName;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDescription] = resource.ResourceName + " - " + resource.ResourceDesc;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
                            boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceRateType] = resource.RateType.ToString();
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypes] = resource.LaborType;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_SegmentRegion] = resource.Segment.ToString();
                        }
                    }
                    else
                    {
                        departureLocation = travelTrip.ZoneOriginName;
                        destinationLocation = travelTrip.ZoneDestCity + ", " + travelTrip.ZoneDestinationName;

                        if (travelTrip.ModeID == MSTTravelMode.ZoneAirfare)
                        {
                            MSTZoneTravelResourceDTO primaryResource = zoneTravelResources.FirstOrDefault(r => r.Resource == travelTrip.ResourceIdForExport && r.OriginID == travelTrip.ZoneOriginID);
                            if (primaryResource != null)
                            {
                                boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeZone] = primaryResource.Zone.ToString();
                                boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceName] = primaryResource.Resource;
                                boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDescription] = primaryResource.Resource + " - " + primaryResource.Description;
                                boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceElementOfCost] = ElementOfCostType.Travel.ToString();
                            }

                            MSTZoneTravelResourceDTO secondaryResource = zoneTravelResources.FirstOrDefault(r => r.Resource == travelTrip.SecondaryResourceIdForExport && r.OriginID == travelTrip.ZoneOriginID);
                            if (secondaryResource != null)
                            {
                                boeExportLabor.ExportFields[BOEExporter.FieldName_SecondaryResourceName] = secondaryResource.Resource;
                                boeExportLabor.ExportFields[BOEExporter.FieldName_SecondaryResourceDescription] = secondaryResource.Resource + " - " + secondaryResource.Description;
                            }
                        }
                        else
                        {
                            MSTZoneTravelResourceDTO resource = zoneTravelResources.FirstOrDefault(r => r.ResourceID == travelTrip.ZoneResourceID);
                            if (resource != null)
                            {
                                boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeZone] = resource.Zone.ToString();
                                boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceName] = resource.Resource;
                                boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDescription] = resource.Resource + " - " + resource.Description;
                                boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceElementOfCost] = ElementOfCostType.Travel.ToString();
                            }
                        }
                    }

                    decimal people = travelTrip.NumOfPeople ?? 0m;
                    decimal days = (travelTrip.NumOfDays ?? 0m) * people;

                    boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCost] = boeExportLabor.Cost.Value.ToString("C0", this.CurrencyFormatter);
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TravelTripID] = travelTrip.Id.ToString();
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeTravelMode] = travelTrip.ModeID.ToDescription();
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeGroupID] = travelTrip.GroupID.ToString();
                    PerformingOrgDTO perfOrg = perfOrgsFromDb.First(x => x.Id == travelTrip.PerfOrgID);
                    boeExportLabor.ExportFields[BOEExporter.FieldName_PerfOrg] = perfOrg.PerformingOrgName;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_GenBOEResourceID] = travelTrip.Id.ToString();
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDeparture] = departureLocation;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDestination] = destinationLocation;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypePurpose] = travelTrip.Purpose;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypePeople] = Utilities.FormatStringWithPrecision(people, exportInputs.Workspace.DecimalPrecision);
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDays] = Utilities.FormatStringWithPrecision(days, exportInputs.Workspace.DecimalPrecision);
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeCars] = travelTrip.NonZoneNumCars.HasValue ? Utilities.FormatStringWithPrecision(travelTrip.NonZoneNumCars.Value, exportInputs.Workspace.DecimalPrecision) : "0";
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDate] = travelTrip.TripDate.ToString("MM/yyyy");

                    if (boe.IsMultiClinWbs)
                    {
                        string wbsString = travelTrip.WbsId == null ? "None" : Utilities.FormatNumberTitleString(travelTrip.WbsNumber, travelTrip.WbsTitle, " - ");
                        string clinString = travelTrip.ClinId == null ? "None" : Utilities.FormatNumberTitleString(travelTrip.ClinNumber, travelTrip.ClinTitle, " - ");

                        boeExportLabor.ExportFields[BOEExporter.FieldName_MultiLabel] = "WBS: " + wbsString + ", CLIN: " + clinString;
                    }

                    boeExportLabors.Add(boeExportLabor);
                }

                boeExportTaskElement.taskElementLabors = boeExportLabors;

                boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString("C0", this.CurrencyFormatter);

                boeExportTaskElements.Add(boeExportTaskElement);
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - travel tasks end");
            }
        }

        /// <summary>
        /// Calculates the total cost for a Nonzone Travel Trip
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="trip">Trip to calculate costs for</param>
        /// <param name="escalationRates">Collection of airfare escalation rates for the workspace</param>
        /// <param name="fees">Dictionary of all Travel Agency Fees and Misc/Other Costs for the Workspace</param>
        /// <returns>
        /// Total cost for the Trip
        /// </returns>
        private decimal CalculateNonzoneTravelTripCost(BOEExportInputs exportInputs, MSTTravelTripType trip, ICollection<WorkspaceRMSEscalationRatesDTO> escalationRates,
            Dictionary<int, WorkspaceRMSTravelNonzoneFeesAndCostsDTO> fees)
        {
            Dictionary<int, decimal> airfareEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.AirfareRate);
            Dictionary<int, decimal> perDiemEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.PerDiemRate);
            Dictionary<int, decimal> miscEscalationRates = escalationRates.ToDictionary(x => x.Year, y => y.MiscRate);

            NonZoneTravelCalculation calculationClass = new NonZoneTravelCalculation(trip.NumOfDays ?? 0, trip.NumOfPeople ?? 0, trip.NonZoneNumCars ?? 0, trip.NonZonePerDiemDaily ?? 0,
                trip.NonZoneCarRentalTrans ?? 0, trip.NonZoneAirfareEstimate ?? 0, airfareEscalationRates, perDiemEscalationRates, miscEscalationRates,
                trip.EstimateDate.Year, trip.TripDate.Year, trip.ModeID == MSTTravelMode.NonZoneDomestic, fees[(int)trip.ModeID].TravelAgencyFee, fees[(int)trip.ModeID].MiscOther, exportInputs.Workspace.CostDecimalPrecision);

            decimal toReturn = calculationClass.TotalTripCost;

            return toReturn;
        }
    }
}
