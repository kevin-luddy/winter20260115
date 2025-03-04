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
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using GenBOE.ActionLogic;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.OfficeUtilities;

    /// <summary>
    /// Converts FullWorkspace into ModelViews for export
    /// </summary>
    /// <seealso cref="BOEExportUtilities" />
    public class BOEExportConverter : BOEExportUtilities
    {
        /// <summary>
        /// The common data mapper.
        /// </summary>
        private ICommonDataMapper commonDataMapper;

        /// <summary>
        /// The variable select boe to sum calculator.
        /// </summary>
        private IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation;
        
        /// <summary>
        /// The travel trip cost calculation.
        /// </summary>
        private TravelTripCostCalculation travelTripCostCalculation;

        /// <summary>
        /// Gets the workspace decimal precision.
        /// </summary>
        protected int WorkspaceDecimalPrecision { get; private set; }
        
        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected Logger Logger { get; private set; }
        
        /// <summary>
        /// Gets the currency formatter.
        /// </summary>
        protected NumberFormatInfo CurrencyFormatter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BOEExportConverter" /> class.
        /// </summary>
        /// <param name="userDTODataLoader">The user dto data loader.</param>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="variableSelectBOEtoSumCalculation">The variable select boe to sum calculation.</param>
        /// <param name="travelTripCostCalculation">The travel trip cost calculation.</param>
        public BOEExportConverter(IUserDTODataLoader userDTODataLoader, ICommonDataMapper commonDataMapper, IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation,
            TravelTripCostCalculation travelTripCostCalculation)
                    : base(userDTODataLoader)
        {
            this.Logger = new Logger(typeof(BOEExportConverter));
            this.CurrencyFormatter = new NumberFormatInfo();
            this.CurrencyFormatter.CurrencyNegativePattern = BOEExporterConstants.CURRENCY_NEGATIVE_PATTERN_MINUS_DOLLAR;
            this.CurrencyFormatter.CurrencySymbol = "$";
            this.commonDataMapper = commonDataMapper;
            this.variableSelectBOEtoSumCalculation = variableSelectBOEtoSumCalculation;
            this.travelTripCostCalculation = travelTripCostCalculation;
        }

        /// <summary>
        /// This function will populate the BOEExportModelViews based on the BOE DTOs.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// the BOE Export ModelViews.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">exportInputs</exception>
        public ICollection<BOEExportModelView> ConvertBoeDTOsToExportMVs(BOEExportInputs exportInputs)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

            ICollection<BOEExportModelView> modelViews = new List<BOEExportModelView>();

            bool writelogstatements = ConfigurationUtilities.GetAppSetting("ExportLoggingEnableInternalMV", false);

            // The workspace's export format doesn't have the correct template type if this is a user template so always use the exportFormatDTO
            WorkspaceExportFormatDTO exportFormatDTO = exportInputs.WorkspaceExportFormats.FirstOrDefault(x => x.Id == exportInputs.Workspace.TemplateID);

            CustomFieldDTO boeSegregationCustomField = (from c in exportInputs.CustomFields
                                                        where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_BOESegregation, StringComparison.CurrentCultureIgnoreCase) &&
                                                              c.CustomFieldDisplayID == CustomFieldType.BoeDisplay
                                                        select c).FirstOrDefault();

            CustomFieldDTO boePwsCustomField = (from c in exportInputs.CustomFields
                                                where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_PWS, StringComparison.CurrentCultureIgnoreCase) &&
                                                      c.CustomFieldDisplayID == CustomFieldType.BoeDisplay
                                                select c).FirstOrDefault();

            // Get Rev Code Value for current Task Element
            CustomFieldDTO revCodeCustomField = (from c in exportInputs.CustomFields
                                                 where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_RevCode, StringComparison.CurrentCultureIgnoreCase)
                                                       && c.CustomFieldDisplayID == CustomFieldType.TaskDisplay
                                                 select c).FirstOrDefault();

            // Get Task Segregation for current task
            CustomFieldDTO taskSegregationCustomField = (from c in exportInputs.CustomFields
                                                         where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_TaskSegregation, StringComparison.CurrentCultureIgnoreCase) &&
                                                               c.CustomFieldDisplayID == CustomFieldType.TaskDisplay
                                                         select c).FirstOrDefault();

            // Get Skill Level Value for current Resource Type
            CustomFieldDTO skillLevelCustomField = (from c in exportInputs.CustomFields
                                                    where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_SkillLevel, StringComparison.CurrentCultureIgnoreCase) &&
                                                          c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                                                    select c).FirstOrDefault();

            // Get Site Value for current Resource Type
            CustomFieldDTO siteCustomField = (from c in exportInputs.CustomFields
                                              where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_Site, StringComparison.CurrentCultureIgnoreCase) &&
                                                    c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                                              select c).FirstOrDefault();

            // Get Skill Mix Value for current Resource Type
            CustomFieldDTO skillMixCustomField = (from c in exportInputs.CustomFields
                                                  where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_SkillMix, StringComparison.CurrentCultureIgnoreCase) &&
                                                        c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                                                  select c).FirstOrDefault();

            // Get ST/OT Value for current Resource Type
            CustomFieldDTO stotCustomField = (from c in exportInputs.CustomFields
                                              where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_STOT, StringComparison.CurrentCultureIgnoreCase) &&
                                                    c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                                              select c).FirstOrDefault();

            // Get "Premium" custom field value for current Resource Type
            CustomFieldDTO premiumCustomField = (from c in exportInputs.CustomFields
                                                 where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_Premium, StringComparison.CurrentCultureIgnoreCase) &&
                                                       c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                                                 select c).FirstOrDefault();

            // Get Rate BOE Value for current Resource Type
            CustomFieldDTO laborTypeRateBOECustomField = (from c in exportInputs.CustomFields
                                                          where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_RateBOE, StringComparison.CurrentCultureIgnoreCase) &&
                                                                c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                                                          select c).FirstOrDefault();

            // Get Company Value for current Resource Type
            CustomFieldDTO laborTypeCompanyCustomField = (from c in exportInputs.CustomFields
                                                          where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_Company, StringComparison.CurrentCultureIgnoreCase) &&
                                                                c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                                                          select c).FirstOrDefault();

            // Get Govt Labor Type Value for current Resource Type
            CustomFieldDTO govtLaborTypeCustomField = (from c in exportInputs.CustomFields
                where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_GovtLaborCategory, StringComparison.CurrentCultureIgnoreCase) &&
                      c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                select c).FirstOrDefault();

            // Get Key Personnel Value for current Resource Type
            CustomFieldDTO keyPersonnelCustomField = (from c in exportInputs.CustomFields
                where c.CustomFieldName.Equals(BOEExporterConstants.CustomFieldName_KeyPersonnel, StringComparison.CurrentCultureIgnoreCase) &&
                      c.CustomFieldDisplayID == CustomFieldType.LaborTypeDisplay
                select c).FirstOrDefault();

            foreach (BoeDTO boe in exportInputs.Boes)
            {
                BOEExportModelView modelView = this.ConvertBoeDTOToExportMV(boe, exportInputs, writelogstatements,
                    exportFormatDTO, boeSegregationCustomField, revCodeCustomField, taskSegregationCustomField,
                    boePwsCustomField, skillLevelCustomField, siteCustomField, skillMixCustomField, stotCustomField,
                    premiumCustomField, laborTypeRateBOECustomField, laborTypeCompanyCustomField,
                    govtLaborTypeCustomField, keyPersonnelCustomField);
                modelViews.Add(modelView);
            }

            return modelViews;
        }

        /// <summary>
        /// This function will populate the BOEExportModelView based on the BOE DTO
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="writelogstatements">if set to <c>true</c> [writelogstatements].</param>
        /// <param name="exportFormatDTO">The export format dto.</param>
        /// <param name="boeSegregationCustomField">The boe segregation custom field.</param>
        /// <param name="revCodeCustomField">The rev code custom field.</param>
        /// <param name="taskSegregationCustomField">The task segregation custom field.</param>
        /// <param name="boePwsCustomField">The boepws custom field.</param>
        /// <param name="skillLevelCustomField">The skill level custom field.</param>
        /// <param name="siteCustomField">The site custom field.</param>
        /// <param name="skillMixCustomField">The skill mix custom field.</param>
        /// <param name="stotCustomField">The stot custom field.</param>
        /// <param name="premiumCustomField">The premium custom field.</param>
        /// <param name="laborTypeRateBOECustomField">The labor type rate boe custom field.</param>
        /// <param name="laborTypeCompanyCustomField">The labor type company custom field.</param>
        /// <param name="govtLaborCategoryCustomField">The govt labor category custom field</param>
        /// <param name="keyPersonnelCustomField">The key personnel custom field</param>
        /// <returns>
        /// the BOE Export ModelView
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private BOEExportModelView ConvertBoeDTOToExportMV(BoeDTO boe, BOEExportInputs exportInputs,
            bool writelogstatements, WorkspaceExportFormatDTO exportFormatDTO, CustomFieldDTO boeSegregationCustomField,
            CustomFieldDTO revCodeCustomField, CustomFieldDTO taskSegregationCustomField,
            CustomFieldDTO boePwsCustomField, CustomFieldDTO skillLevelCustomField, CustomFieldDTO siteCustomField,
            CustomFieldDTO skillMixCustomField, CustomFieldDTO stotCustomField, CustomFieldDTO premiumCustomField,
            CustomFieldDTO laborTypeRateBOECustomField, CustomFieldDTO laborTypeCompanyCustomField,
            CustomFieldDTO govtLaborCategoryCustomField, CustomFieldDTO keyPersonnelCustomField)
        {
            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - begin");
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - get DTO's begin");
            }

            // get the wbs 
            WbsDTO wbsDTO = exportInputs.WbsElements.FirstOrDefault(x => x.Id == boe.WBSID);
            string wbsNum = wbsDTO != null ? wbsDTO.WbsNumber : "NO WBS";
            string wbsTitle = wbsDTO != null ? wbsDTO.WbsTitle : "NO WBS";

            // get the clin
            ClinDTO clinDTO = exportInputs.Clins.FirstOrDefault(x => x.Id == boe.CLINID);
            string clinNum = clinDTO != null ? clinDTO.ClinNumber : "NO CLIN";

            // get BOE# for HDR-Hawaii
            string boeNumber = wbsNum + "-" + clinNum;

            // get the workspace custom fields and custom field exportInputs.
            IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields = exportInputs.CustomFields;
            IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues = new Collection<CustomFieldValueDTO>();
            if (workspaceCustomFields.Any())
            {
                workspaceCustomFieldValues = exportInputs.CustomFieldValues;
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - get DTO's end");
            }

            // set up BOEExportModelView
            BOEExportModelView boeExportModelView = new BOEExportModelView();
            boeExportModelView.BoeID = boe.Id;
            boeExportModelView.ProgramName = exportInputs.Workspace.WorkspaceName;
            boeExportModelView.WorkspaceDescription = exportInputs.Workspace.Description;
            boeExportModelView.SolicitationNumber = string.IsNullOrEmpty(exportInputs.Workspace.RFPNumber) ? string.Empty : exportInputs.Workspace.RFPNumber;
            boeExportModelView.TrackingNumber = exportInputs.Workspace.TrackingNumber;
            boeExportModelView.ProposalSubmittalDate = exportInputs.Workspace.ProposalSubmittalDate;
            boeExportModelView.WBSTitle = wbsTitle;
            boeExportModelView.WBSNumber = wbsNum;
            boeExportModelView.PaddedWbsName = (wbsDTO != null) ? wbsDTO.WbsPaddedNumber : string.Empty;
            boeExportModelView.CLINNumber = clinNum;
            boeExportModelView.CLINTitle = clinDTO != null ? clinDTO.ClinTitle ?? string.Empty : "NO CLIN";
            boeExportModelView.CLINStartDate = clinDTO != null ? clinDTO.StartDate : null;
            boeExportModelView.CLINEndDate = clinDTO != null ? clinDTO.EndDate : null;
            boeExportModelView.ContractStartDate = exportInputs.Workspace.ContractStartDate;
            boeExportModelView.ContractEndDate = exportInputs.Workspace.ContractEndDate;
            boeExportModelView.PaddedClinName = (clinDTO != null) ? clinDTO.ClinPaddedNumber : string.Empty;
            boeExportModelView.ProposalSubmittalDate = exportInputs.Workspace.ProposalSubmittalDate;
            boeExportModelView.ContainsOCI = exportInputs.Workspace.ContainsOCI;
            boeExportModelView.DataSource = BOEExportConverter.GetRteOverride(boe.Id, null, boe.DataSource, RteTemplateSource.BoeSources, exportInputs.RTETemplatesOverrides);
            boeExportModelView.IsMaterial = boe.isMaterial;
            boeExportModelView.IsMultiClinWbs = boe.IsMultiClinWbs;
            boeExportModelView.StartDate = boe.StartDate;
            boeExportModelView.EndDate = boe.EndDate;
            boeExportModelView.Category = boe.Category;
            boeExportModelView.SOWNumber = boe.SOW;
            boeExportModelView.SOWTitle = boe.SOWTitle;
            boeExportModelView.IsProjectMap = exportInputs.Workspace.IsProjectMapWorkspace;
            boeExportModelView.ClassOfCost = boe.ClassOfCost.GetDescription();

            string addDelete;
            try { addDelete = exportInputs.TaskElements?.Where(x => x.BoeID == boe.Id).SelectMany(x => x.taskElementLabors).Select(x => x.AddOrDelete).FirstOrDefault(x => !string.IsNullOrEmpty(x)); }
            catch { addDelete = string.Empty; }
            boeExportModelView.AddDelete = addDelete;

            //ToDo: DUSAN -> Project Map -> do we need this?
            /*
            string rationale;
            try { rationale = exportInputs.TaskElements?.Where(x => x.BoeID == boe.Id).Select(x => x.MOQText).FirstOrDefault(x => !string.IsNullOrEmpty(x)); }
            catch { rationale = string.Empty; }
            boeExportModelView.Rationale = rationale;
            */

            boeExportModelView.ActivityId = boe.Title;

            // Get BOE Segregation for current BOE
            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - get segregation info begin");
            }

            if (boeSegregationCustomField != null)
            {
				CustomFieldValueDTO boeSegregation = (from v in workspaceCustomFieldValues
                                      from c in boe.CustomFieldValueContainers
                                      where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == boeSegregationCustomField.Id
                                      select v).FirstOrDefault();

                if (boeSegregation != null)
                {
                    boeExportModelView.ExportFields[BOEExporter.FieldName_BOESegregation] = string.Concat(
                        boeSegregation.CustomFieldValueName == null ? string.Empty : boeSegregation.CustomFieldValueName,
                        " ",
                        boeSegregation.CustomFieldValueDescription == null ? string.Empty : boeSegregation.CustomFieldValueDescription);
                }
            }

            if (boePwsCustomField != null)
            {
				CustomFieldValueDTO boePws = (from v in workspaceCustomFieldValues
                              from c in boe.CustomFieldValueContainers
                              where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == boePwsCustomField.Id
                              select v).FirstOrDefault();

                if (boePws != null)
                {
                    boeExportModelView.ExportFields[BOEExporter.FieldName_BOEPWS] = string.Concat(
                        boePws.CustomFieldValueName ?? string.Empty,
                        " ",
                        boePws.CustomFieldValueDescription ?? string.Empty);
                }
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - get segregation info end");
            }

            // if the BOE is in awaiting approval or has been approved, precede the author's name with "//S//" otherwise precede the author's name with "//Not Signed//"

            this.GetCompanySpecificBOECustomFields(workspaceCustomFields, workspaceCustomFieldValues, boe, boeExportModelView);

            boeExportModelView.Authors = this.DetermineBoeExportAuthors(boe, exportInputs.BoeIdsAndLastUserToSubmitThemForApprovalMapping, exportInputs.GetUserDataForBoesForWs);
            boeExportModelView.PreparedBy = string.Join(WordUtilities.NEWLINE_CHAR.ToString(), boeExportModelView.Authors);

            boeExportModelView.SubmittedDate = boe.SubmitForApprovalDate.Year == DateTime.MinValue.Year ? string.Empty : boe.SubmitForApprovalDate.ToShortDateString();

            boeExportModelView.Approvers = this.DetermineBoeExportApprovers(boe.Id, exportInputs.BoeMappingWithApproverResponses, exportInputs.GetUserDataForBoesForWs);

            boeExportModelView.BOETitle = boe.Title;
            boeExportModelView.BOEDescription = BOEExportConverter.GetRteOverride(boe.Id, null, boe.Description, RteTemplateSource.BoeDescription, exportInputs.RTETemplatesOverrides);
            boeExportModelView.ExportFormat = exportFormatDTO.ExportFormat;

            Collection<BOEExportTaskElement> boeExportTaskElements = this.PopulateLaborAndMissionTasks(boe,
                exportInputs, writelogstatements, exportFormatDTO, workspaceCustomFields, workspaceCustomFieldValues,
                revCodeCustomField, taskSegregationCustomField,
                skillLevelCustomField, siteCustomField, skillMixCustomField, stotCustomField, premiumCustomField,
                laborTypeRateBOECustomField, laborTypeCompanyCustomField, govtLaborCategoryCustomField,
                keyPersonnelCustomField, boeNumber);
            this.PopulateOdcTasks(boe, exportInputs, writelogstatements, boeExportTaskElements);
            this.PopulateTravelTasks(boe, exportInputs, writelogstatements, boeExportTaskElements);
            this.PopulateMaterialTasks(boe, exportInputs, writelogstatements, boeExportTaskElements);

            boeExportModelView.TaskElements = boeExportTaskElements;

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - end");
            }

            return boeExportModelView;
        }

        #region Populate Data

        /// <summary>
        /// Populates the labor and mission tasks.
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="writelogstatements">if set to <c>true</c> [writelogstatements].</param>
        /// <param name="exportFormatDTO">The export format dto.</param>
        /// <param name="workspaceCustomFields">The workspace custom fields.</param>
        /// <param name="workspaceCustomFieldValues">The workspace custom field values.</param>
        /// <param name="revCodeCustomField">The rev code custom field.</param>
        /// <param name="taskSegregationCustomField">The task segregation custom field.</param>
        /// <param name="skillLevelCustomField">The skill level custom field.</param>
        /// <param name="siteCustomField">The site custom field.</param>
        /// <param name="skillMixCustomField">The skill mix custom field.</param>
        /// <param name="stotCustomField">The stot custom field.</param>
        /// <param name="premiumCustomField">The premium custom field.</param>
        /// <param name="laborTypeRateBOECustomField">The labor type rate boe custom field.</param>
        /// <param name="laborTypeCompanyCustomField">The labor type company custom field.</param>
        /// <param name="govtLaborCategoryCustomField">The govt labor category custom field</param>
        /// <param name="keyPersonnelCustomField">The key personnel custom field</param>
        /// <param name="boeNumber">BOE Number (wbs-clin)</param>
        /// <returns>
        /// A collection of converted Tasks for export.
        /// </returns>
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        private Collection<BOEExportTaskElement> PopulateLaborAndMissionTasks(BoeDTO boe, BOEExportInputs exportInputs,
            bool writelogstatements, WorkspaceExportFormatDTO exportFormatDTO,
            IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields,
            IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues, CustomFieldDTO revCodeCustomField,
            CustomFieldDTO taskSegregationCustomField, CustomFieldDTO skillLevelCustomField,
            CustomFieldDTO siteCustomField, CustomFieldDTO skillMixCustomField, CustomFieldDTO stotCustomField,
            CustomFieldDTO premiumCustomField, CustomFieldDTO laborTypeRateBOECustomField,
            CustomFieldDTO laborTypeCompanyCustomField, CustomFieldDTO govtLaborCategoryCustomField,
            CustomFieldDTO keyPersonnelCustomField, string boeNumber)
        {
            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - labor tasks begin");
            }

			Collection<BOEExportTaskElement> boeExportTaskElements = new Collection<BOEExportTaskElement>();
            IDictionary<int, ElementOfCostTypeModelView> allElementOfCostTypes = this.commonDataMapper.GetElementOfCostTypesDictionary();
            IDictionary<int, SpreadCurveModelView> allSpreadCurves = exportInputs.Workspace.IsProjectMapWorkspace ? this.commonDataMapper.getProjectMapSpreadCurveDictionary() : this.commonDataMapper.getSpreadCurveDictionary();
            IDictionary<int, SikorskyLegacyResourceDTO> allLegacyResources = this.commonDataMapper.GetSikorskyLegacyResourcesDictionary(exportInputs.Workspace.IsProjectMapWorkspace);

            ICollection<BoeTaskElementDTO> taskElements = exportInputs.TaskElements.Where(x => x.BoeID == boe.Id).ToList();
            
            IReadOnlyCollection<ResourceDTO> resourcesForLabors = exportInputs.ResourcesUsedInWsBoes;
            IReadOnlyCollection<PerformingOrgDTO> performingOrgsFromDb = exportInputs.PerformingOrgsUsedInBoes;

			bool hasTMRatesForWorkspace = BOETaskUtility.IsUsingTMRates(exportInputs.FullWorkspace.TMResourceRatesForWorkspace.ToList(), resourcesForLabors?.ToList());

			foreach (BoeTaskElementDTO boeTaskElement in taskElements)
            {
				BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
                boeExportTaskElement.BoeID = boeTaskElement.BoeID;
                boeExportTaskElement.BOETaskDesc = BOEExportConverter.GetRteOverride(boeTaskElement.BoeID, boeTaskElement.Id, boeTaskElement.Description, RteTemplateSource.TaskDescription, exportInputs.RTETemplatesOverrides);
                boeExportTaskElement.BOETaskElementID = boeTaskElement.Id;
                boeExportTaskElement.BOETaskID = boeTaskElement.BOETaskID;
                boeExportTaskElement.EndDate = boeTaskElement.EndDate;
                boeExportTaskElement.MOQEquation = boeTaskElement.MOQHoursEquation;
                boeExportTaskElement.MOQText = BOEExportConverter.GetRteOverride(boeTaskElement.BoeID, boeTaskElement.Id, boeTaskElement.MOQText, RteTemplateSource.TaskMOQ, exportInputs.RTETemplatesOverrides);
                boeExportTaskElement.MOQType = boeTaskElement.MOQType.GetDescription();
                boeExportTaskElement.OrdinaryVariables = boeTaskElement.OrdinaryVariables;
                boeExportTaskElement.StartDate = boeTaskElement.StartDate;
                boeExportTaskElement.TaskTitle = boeTaskElement.TaskTitle;
                boeExportTaskElement.IMS_ID = boeTaskElement.IMS_ID;
                boeExportTaskElement.BOETaskElementOrder = boeTaskElement.BOETaskElementOrder;
                boeExportTaskElement.MOQTypes = exportInputs.MOQTypes.Where(x => x.TaskId == boeTaskElement.Id).ToCollection();
				boeExportTaskElement.HasTMRates = hasTMRatesForWorkspace;

				if (Utilities.ShowSkillMixForTask(exportInputs.Workspace.CreationDate, boeExportTaskElement.HasTMRates))
				{
					boeExportTaskElement.SkillMixTable = boeTaskElement.SkillMixTable;
					boeExportTaskElement.CommonDisclosureTable = boeTaskElement.CommonDisclosureTable;
				}

				boeExportTaskElement.SetTaskElementType(boeTaskElement.TaskElementType);

                if (revCodeCustomField != null)
                {
					CustomFieldValueDTO revcode = (from v in workspaceCustomFieldValues
                                   from c in boeTaskElement.CustomFieldValueContainers
                                   where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == revCodeCustomField.Id
                                   select v).FirstOrDefault();

                    if (revcode != null)
                    {
                        boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskElementRevCode] = revcode.CustomFieldValueName;
                    }
                }

                // get Workspace Variables associated with BOE DTO Task Element
                boeExportTaskElement.WorkspaceVariables = exportInputs.WorkspaceVariables.Where(x => boeTaskElement.WorkspaceVariableIDs.Contains(x.Id)).ToCollection();

                if (taskSegregationCustomField != null)
                {
					CustomFieldValueDTO taskSegregation = (from v in workspaceCustomFieldValues
                                           from c in boeTaskElement.CustomFieldValueContainers
                                           where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == taskSegregationCustomField.Id
                                           select v).FirstOrDefault();

                    if (taskSegregation != null)
                    {
                        boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskSegregation] = string.Concat(
                            taskSegregation.CustomFieldValueName ?? string.Empty,
                            " ",
                            taskSegregation.CustomFieldValueDescription ?? string.Empty);
                    }
                }

                // Populate MOQ variable model views
                boeExportTaskElement.MOQVariableModelViews = this.GetSumOfBoeMOQVariableModelView(boeExportTaskElement, exportInputs);

                // get Task Element Labors
                Collection<BOEExportTaskElementLabor> boeExportLabors = new Collection<BOEExportTaskElementLabor>();

                foreach (ResourceTypeDto laborType in boeTaskElement.taskElementLabors)
                {
                    BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);

                    boeExportLabor.LaborTypeOrder = laborType.LaborTypeOrder;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeID] = laborType.Id.ToString();

                    if (laborType.SpreadType == SpreadType.Hours)
                    {
                        boeExportLabor.Hours = laborType.ValueSpread;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeHours] = boeExportLabor.Hours.HasValue ? Utilities.FormatStringWithPrecision(boeExportLabor.Hours.Value, this.WorkspaceDecimalPrecision) : string.Empty;
                    }
                    else if (laborType.SpreadType == SpreadType.Cost)
                    {
                        boeExportLabor.Cost = (decimal)laborType.ValueSpread;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCost] = boeExportLabor.Cost.HasValue ? boeExportLabor.Cost.Value.ToString("C0", this.CurrencyFormatter) : string.Empty;
                    }

                    decimal taskElementCostTotal = 0m;

                    if (laborType.SpreadType == SpreadType.Cost)
                    {
                        taskElementCostTotal = laborType.LaborSpreads.Sum(s => s.LaborSpreadValue);
                    }

                    boeExportLabor.Cost = taskElementCostTotal;

                    if (laborType.ResourceID.HasValue)
                    {
                        ResourceDTO resource = resourcesForLabors.First(x => x.Id == laborType.ResourceID.Value);

						if (exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.DS_ES_STANDARD_PORTRAIT_WITH_COST ||
                            exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.DS_ES_STANDARD_PORTRAIT_WITHOUT_COST ||
                            exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.DS_STANDARD_PORTRAIT_WITHOUT_COST_2 ||
                            exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.LMSI_GSM_O_LANDSACPE_WITH_TIME_PHASED_SUMMARIES)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypes] = resource.LaborType;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_SegmentRegion] = resource.SegRegion;
                        }
                        else if (exportFormatDTO.ExportFormat.TemplateType == ExcelReportTemplateType.LMSI_STANDARD_LANDSCAPE)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypes] = resource.ResourceName;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_SegmentRegion] = resource.SegRegion;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCompany] = resource.LaborType;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypes] = allElementOfCostTypes[(int)resource.ElementOfCost].ElementOfCostName;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_SegmentRegion] = resource.Segment.ToString();
                        }

                        boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDescription] = resource.ResourceDesc;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceID] = resource.Id.ToString();
                        boeExportLabor.ExportFields[BOEExporter.FieldName_GenBOEResourceID] = laborType.Id.ToString();

						if (exportInputs.Workspace.IsProjectMapWorkspace)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceName] = Utilities.FormatResourceNames(resource.ResourceName, this.commonDataMapper.GetSikorskyLegacyResourceID(laborType.LegacyID, allLegacyResources), laborType is SubResourceTypeDto);
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceName] = resource.ResourceName;
                        }
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeMatSubIwtaCost] = "$0";
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeOtherCost] = "$0";

                        if (resource.ElementOfCost == ElementOfCostType.Materials || resource.ElementOfCost == ElementOfCostType.Sub || resource.ElementOfCost == ElementOfCostType.IWTA)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeMatSubIwtaCost] = boeExportLabor.Cost.HasValue ? boeExportLabor.Cost.Value.ToString("C0", this.CurrencyFormatter) : "$0";
                        }
                        else if (resource.ElementOfCost == ElementOfCostType.Travel || resource.ElementOfCost == ElementOfCostType.ODC)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeOtherCost] = boeExportLabor.Cost.HasValue ? boeExportLabor.Cost.Value.ToString("C0", this.CurrencyFormatter) : "$0";
                        }

                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceRateType] = resource.RateType.ToString();
                    }

					if (Utilities.IsBRCEnabledForWorkspace(exportInputs.Workspace.Shortname))
					{
						boeExportLabor.ExportFields[BOEExporter.FieldName_BrcID] = laborType.BusinessResourceCodeID.HasValue 
							? laborType.BusinessResourceCodeID.ToString() : string.Empty;
					}

					if (laborType.PerformingOrgID.HasValue)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_PerfOrgId] = laborType.PerformingOrgID.ToString();
                        PerformingOrgDTO perfOrg = performingOrgsFromDb.First(x => x.Id == laborType.PerformingOrgID.Value);
                        boeExportLabor.ExportFields[BOEExporter.FieldName_PerfOrg] = perfOrg.PerformingOrgName;
                    }

                    if (laborType.StartDate.HasValue)
                    {
                        boeExportLabor.StartDate = laborType.StartDate.Value;
                    }

                    if (laborType.EndDate.HasValue)
                    {
                        boeExportLabor.EndDate = laborType.EndDate.Value;
                    }

                    boeExportLabor.TieredPercent = laborType.TieredPercentage;

                    boeExportLabor.ExportFields[BOEExporter.FieldName_SpreadCurve] =
                        laborType.SpreadCurveID.HasValue ? allSpreadCurves[(int)laborType.SpreadCurveID.Value].SpreadCurveName.Replace("Hours", FullObjectHelper.HoursLabel(exportInputs.Workspace)) : string.Empty;

                    if (skillLevelCustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSkillLevel] = string.Empty;
                    }
                    else
                    {
						CustomFieldValueDTO skillLevel = (from v in workspaceCustomFieldValues
                                          from c in laborType.CustomFieldValueContainers
                                          where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == skillLevelCustomField.Id
                                          select v).FirstOrDefault();

                        if (skillLevel == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSkillLevel] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSkillLevel] = skillLevel.CustomFieldValueName;
                        }
                    }

                    if (siteCustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSite] = string.Empty;
                    }
                    else
                    {
						CustomFieldValueDTO site = (from v in workspaceCustomFieldValues
                                    from c in laborType.CustomFieldValueContainers
                                    where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == siteCustomField.Id
                                    select v).FirstOrDefault();

                        if (site == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSite] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSite] = site.CustomFieldValueDescription;
                        }
                    }

                    if (skillMixCustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSkillMix] = string.Empty;
                    }
                    else
                    {
						CustomFieldValueDTO skillMix = (from v in workspaceCustomFieldValues
                                        from c in laborType.CustomFieldValueContainers
                                        where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == skillMixCustomField.Id
                                        select v).FirstOrDefault();

                        if (skillMix == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSkillMix] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeSkillMix] = skillMix.CustomFieldValueDescription;
                        }
                    }

                    if (stotCustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeSTOT] = string.Empty;
                    }
                    else
                    {
						CustomFieldValueDTO stot = (from v in workspaceCustomFieldValues
                                    from c in laborType.CustomFieldValueContainers
                                    where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == stotCustomField.Id
                                    select v).FirstOrDefault();

                        if (stot == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeSTOTId] = string.Empty;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeSTOT] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeSTOTId] = stot.CustomFieldValueName;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeSTOT] = stot.CustomFieldValueName + "-" + stot.CustomFieldValueDescription;
                        }
                    }

                    if (premiumCustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_PremiumCustomField] = string.Empty;
                    }
                    else
                    {
                        CustomFieldValueDTO premiumCustomFieldValue = (from v in workspaceCustomFieldValues
                                                                       from c in laborType.CustomFieldValueContainers
                                                                       where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == premiumCustomField.Id
                                                                       select v).FirstOrDefault();
                        if (premiumCustomFieldValue == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_PremiumCustomField] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_PremiumCustomField] = premiumCustomFieldValue.CustomFieldValueName + "-" + premiumCustomFieldValue.CustomFieldValueDescription;
                        }
                    }

                    if (laborTypeRateBOECustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeRateBOE] = string.Empty;
                    }
                    else
                    {
						CustomFieldValueDTO rateBOE = (from v in workspaceCustomFieldValues
                                       from c in laborType.CustomFieldValueContainers
                                       where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == laborTypeRateBOECustomField.Id
                                       select v).FirstOrDefault();

                        if (rateBOE == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeRateBOEId] = string.Empty;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeRateBOE] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeRateBOEId] = rateBOE.CustomFieldValueName;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeRateBOE] = rateBOE.CustomFieldValueName + "-" + rateBOE.CustomFieldValueDescription;
                        }
                    }

                    if (govtLaborCategoryCustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_GovtLaborCategory] = string.Empty;
                    }
                    else
                    {
                        CustomFieldValueDTO govtLaborCategory = (from v in workspaceCustomFieldValues
                            from c in laborType.CustomFieldValueContainers
                            where v.CustomFieldValueID == c.CustomFieldValueID &&
                                  v.CustomFieldID == govtLaborCategoryCustomField.Id
                            select v).FirstOrDefault();
                        if (govtLaborCategory == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_GovtLaborCategory] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_GovtLaborCategory] = govtLaborCategory.CustomFieldValueDescription;
                        }
                    }

                    if (keyPersonnelCustomField == null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_KeyPersonnel] = string.Empty;
                    }
                    else
                    {
                        CustomFieldValueDTO keyPersonnel = (from v in workspaceCustomFieldValues
                            from c in laborType.CustomFieldValueContainers
                            where v.CustomFieldValueID == c.CustomFieldValueID &&
                                  v.CustomFieldID == keyPersonnelCustomField.Id
                            select v).FirstOrDefault();
                        if (keyPersonnel == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_KeyPersonnel] = string.Empty;
                        }
                        else
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_KeyPersonnel] = keyPersonnel.CustomFieldValueDescription;
                        }
                    }
                    
                    // populated above (from ResourceDTO)
                    if (exportFormatDTO.ExportFormat.TemplateType != ExcelReportTemplateType.LMSI_STANDARD_LANDSCAPE)  
                    {
                        if (laborTypeCompanyCustomField == null)
                        {
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCompanyId] = string.Empty;
                            boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCompany] = string.Empty;
                        }
                        else
                        {
							CustomFieldValueDTO company = (from v in workspaceCustomFieldValues
                                           from c in laborType.CustomFieldValueContainers
                                           where v.CustomFieldValueID == c.CustomFieldValueID && v.CustomFieldID == laborTypeCompanyCustomField.Id
                                           select v).FirstOrDefault();

                            if (company == null)
                            {
                                boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCompanyId] = string.Empty;
                                boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCompany] = string.Empty;
                            }
                            else
                            {
                                boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCompanyId] = company.CustomFieldValueName;
                                boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCompany] = company.CustomFieldValueName + "-" + company.CustomFieldValueDescription;
                            }
                        }
                    }

                    boeExportLabors.Add(boeExportLabor);
                }

                boeExportTaskElement.taskElementLabors = boeExportLabors;

                boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskHoursTotal] = Utilities.FormatStringWithPrecision(boeExportTaskElement.taskElementLabors.Where(l => l.Hours.HasValue).Sum(l => l.Hours.Value), this.WorkspaceDecimalPrecision);

                boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors
                    .Where(x => x.ExportFields.ContainsKey(BOEExporter.FieldName_SpreadCurve))
                    .Where(l => l.Cost.HasValue && l.ExportFields[BOEExporter.FieldName_SpreadCurve].Equals(SpreadCurves.DiscreteCost.ToDescription()))
                    .Sum(l => l.Cost.Value).ToString("C0", this.CurrencyFormatter);
                
                boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskMatSubIWTACostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue
                                                                                                                                         && l.ExportFields.ContainsKey(BOEExporter.FieldName_ResourceElementOfCost)
                                                                                                                                         && (l.ExportFields[BOEExporter.FieldName_ResourceElementOfCost].Equals("Materials")
                                                                                                                                             || l.ExportFields[BOEExporter.FieldName_ResourceElementOfCost].Equals("Sub")
                                                                                                                                             || l.ExportFields[BOEExporter.FieldName_ResourceElementOfCost].Equals("IWTA")))
                    .Sum(l => l.Cost.Value).ToString("C0", this.CurrencyFormatter);
                boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskOtherCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue
                                                                                                                                    && l.ExportFields.ContainsKey(BOEExporter.FieldName_ResourceElementOfCost)
                                                                                                                                    && (l.ExportFields[BOEExporter.FieldName_ResourceElementOfCost].Equals("ODC")
                                                                                                                                        || l.ExportFields[BOEExporter.FieldName_ResourceElementOfCost].Equals("Travel")))
                    .Sum(l => l.Cost.Value).ToString("C0", this.CurrencyFormatter);

                boeExportTaskElement.ExportFields[BOEExporter.FieldName_BoeNumber] = boeNumber;

                this.GetCompanySpecificTaskCustomFields(workspaceCustomFields, workspaceCustomFieldValues, boeTaskElement, boeExportTaskElement);

                boeExportTaskElements.Add(boeExportTaskElement);
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - labor tasks end");
            }

            return boeExportTaskElements;
        }

        /// <summary>
        /// Populates the odc tasks.
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="writelogstatements">if set to <c>true</c> [writelogstatements].</param>
        /// <param name="boeExportTaskElements">The boe export task elements.</param>
        private void PopulateOdcTasks(BoeDTO boe, BOEExportInputs exportInputs, bool writelogstatements, Collection<BOEExportTaskElement> boeExportTaskElements)
        {
            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - odc tasks begin");
            }

            ICollection<OtherDirectCostDTO> odcElements = exportInputs.Odcs.Where(x => x.BoeID == boe.Id).ToList();

            foreach (OtherDirectCostDTO odcElement in odcElements)
            {
                BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
                boeExportTaskElement.BoeID = boe.Id;
                boeExportTaskElement.BOETaskDesc = odcElement.TaskDescription;
                boeExportTaskElement.BOETaskElementID = odcElement.Id;
                boeExportTaskElement.MOQText = odcElement.MoqText;
                boeExportTaskElement.TaskTitle = odcElement.TaskTitle;
                boeExportTaskElement.BOETaskID = odcElement.TaskID;
                boeExportTaskElement.StartDate = odcElement.StartDate.HasValue ? odcElement.StartDate : boe.StartDate;
                boeExportTaskElement.EndDate = odcElement.EndDate.HasValue ? odcElement.EndDate : boe.EndDate;
                boeExportTaskElement.ElementType = BOEExportTaskElementType.ODC;
                boeExportTaskElement.BOETaskElementOrder = odcElement.BOETaskElementOrder;

                // get Task Element Labors
                Collection<BOEExportTaskElementLabor> boeExportLabors = new Collection<BOEExportTaskElementLabor>();

                // Aggregate ODC Types by matching Resource and Performing Org
                var aggregatedOdcTypes = (from odcType in odcElement.ODCTypes
                                          group odcType by new
                                          {
                                              odcType.ResourceID,
                                              odcType.PerformingOrgID
                                          }
                    into grouping
                                          select new
                                          {
                                              ResourceID = grouping.Key.ResourceID,
                                              PerformingOrgID = grouping.Key.PerformingOrgID,
                                              Cost = (from odcType in grouping
                                                      where odcType.Cost.HasValue
                                                      select (odcType.SpreadCurve == SpreadCurves.Load) ?
                                                          odcType.Cost.Value * odcType.ODCSpreads.Count :
                                                          odcType.Cost.Value).Sum()
                                          }).ToList();

                IReadOnlyCollection<ResourceDTO> resourcesForOdcs = exportInputs.ResourcesUsedInWsBoes;
                IReadOnlyCollection<PerformingOrgDTO> performingOrgsFromDb = exportInputs.PerformingOrgsUsedInBoes;

                foreach (var odcType in aggregatedOdcTypes)
                {
                    BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);

                    boeExportLabor.Cost = odcType.Cost;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCost] = odcType.Cost.ToString("C0", this.CurrencyFormatter);

                    if (odcType.ResourceID.HasValue)
                    {
                        ResourceDTO resource = resourcesForOdcs.First(x => x.Id == odcType.ResourceID.Value);
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypes] = resource.LaborType;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDescription] = resource.ResourceDesc;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceName] = resource.ResourceName;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceID] = resource.Id.ToString();
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceRateType] = resource.RateType.ToString();
                    }

                    if (odcType.PerformingOrgID.HasValue)
                    {
                        PerformingOrgDTO perfOrg = performingOrgsFromDb.First(x => x.Id == odcType.PerformingOrgID.Value);
                        boeExportLabor.ExportFields[BOEExporter.FieldName_PerfOrg] = perfOrg.PerformingOrgName;
                    }

                    boeExportLabors.Add(boeExportLabor);
                }

                boeExportTaskElement.taskElementLabors = boeExportLabors;

                boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString("C0", this.CurrencyFormatter);

                boeExportTaskElements.Add(boeExportTaskElement);
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - odc tasks end");
            }
        }

        /// <summary>
        /// Populates the travel tasks.
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="writelogstatements">if set to <c>true</c> [writelogstatements].</param>
        /// <param name="boeExportTaskElements">The boe export task elements.</param>
        /// <exception cref="System.ArgumentNullException">workspace  or boeExportTaskElements</exception>
        protected virtual void PopulateTravelTasks(BoeDTO boe, BOEExportInputs exportInputs, bool writelogstatements, Collection<BOEExportTaskElement> boeExportTaskElements)
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

            ICollection<ResourceDTO> travelResources = new Collection<ResourceDTO>();

            // only grab the travel resources if there are travel tasks
            if (travelTasks.Any())
            {
                travelResources = exportInputs.ResourcesForWsResourceListId.Where(x => x.ElementOfCost == ElementOfCostType.Travel).ToList();
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
                boeExportTaskElement.StartDate = travelElement.StartDate.HasValue ? travelElement.StartDate : boe.StartDate;
                boeExportTaskElement.EndDate = travelElement.EndDate.HasValue ? travelElement.EndDate : boe.EndDate;
                boeExportTaskElement.ElementType = BOEExportTaskElementType.Travel;
                boeExportTaskElement.BOETaskElementOrder = travelElement.BOETaskElementOrder;

                // get Task Element Labors
                Collection<BOEExportTaskElementLabor> boeExportLabors = new Collection<BOEExportTaskElementLabor>();

                foreach (TravelTripType travelTrip in travelElement.TravelTrips)
                {
                    BOEExportTaskElementLabor boeExportLabor = new BOEExportTaskElementLabor(boeExportTaskElement);
                    boeExportLabor.Cost = this.travelTripCostCalculation.CalculateTravelCost(travelTrip, exportInputs.FullWorkspace).CostTotal;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypeCost] = boeExportLabor.Cost.Value.ToString("C0", this.CurrencyFormatter);
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TravelTripID] = travelTrip.TravelTripID.ToString();

					TripDTO systemTrip = exportInputs.FullWorkspace.GetTripById(travelTrip.SystemTripID);
					LocationDTO departureLocation = exportInputs.FullWorkspace.GetLocationById(systemTrip.DepartureLocationID);
					LocationDTO destinationLocation = exportInputs.FullWorkspace.GetLocationById(systemTrip.DestinationLocationID);

                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDescription] =
                        departureLocation.LocationName +
                        " to " +
                        destinationLocation.LocationName +
                        " - " +
                        travelTrip.Purpose +
                        " - " +
                        travelTrip.NumOfTrips +
                        " Trip(s), " +
                        travelTrip.NumOfPeople +
                        " Traveler(s), " +
                        travelTrip.NumOfDays +
                        " Day(s) in " +
                        travelTrip.TripDate.ToString("MM/yyyy");

					ResourceDTO resource = travelResources.FirstOrDefault(r => r.Segment == travelTrip.Segment);

                    if (resource != null)
                    {
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceID] = resource.Id.ToString();
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceName] = resource.ResourceName;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceElementOfCost] = resource.ElementOfCost.ToString();
                        boeExportLabor.ExportFields[BOEExporter.FieldName_ResourceRateType] = resource.RateType.ToString();
                        boeExportLabor.ExportFields[BOEExporter.FieldName_LaborTypes] = resource.LaborType;
                        boeExportLabor.ExportFields[BOEExporter.FieldName_SegmentRegion] = resource.Segment.ToString();
                    }

                    PerformingOrgDTO perfOrg = perfOrgsFromDb.First(x => x.Id == travelTrip.PerfOrgID);
                    boeExportLabor.ExportFields[BOEExporter.FieldName_PerfOrg] = perfOrg.PerformingOrgName;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_GenBOEResourceID] = travelTrip.TravelTripID.ToString();
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDeparture] = departureLocation.LocationName;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDestination] = destinationLocation.LocationName;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypePurpose] = travelTrip.Purpose;
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeTrips] = travelTrip.NumOfTrips.ToString();
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypePeople] = travelTrip.NumOfPeople.ToString();
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDays] = travelTrip.NumOfDays.ToString();
                    boeExportLabor.ExportFields[BOEExporter.FieldName_TaskTypeDate] = travelTrip.TripDate.ToString("MM/yyyy");

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
        /// Populates the material tasks.
        /// </summary>
        /// <param name="boe">The boe.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="writelogstatements">if set to <c>true</c> [writelogstatements].</param>
        /// <param name="boeExportTaskElements">The boe export task elements.</param>
        private void PopulateMaterialTasks(BoeDTO boe, BOEExportInputs exportInputs, bool writelogstatements, Collection<BOEExportTaskElement> boeExportTaskElements)
        {
            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - materials tasks begin");
            }

            ICollection<MaterialDTO> materials = exportInputs.Materials.Where(x => x.BoeID == boe.Id).ToList();

            foreach (MaterialDTO materialsElement in materials)
            {
                BOEExportTaskElement boeExportTaskElement = new BOEExportTaskElement();
                boeExportTaskElement.BoeID = boe.Id;
                boeExportTaskElement.BOETaskDesc = materialsElement.TaskDescription;
                boeExportTaskElement.BOETaskElementID = materialsElement.Id;
                boeExportTaskElement.TaskTitle = materialsElement.TaskTitle;
                boeExportTaskElement.MOQText = materialsElement.MoqText;
                boeExportTaskElement.StartDate = materialsElement.StartDate;
                boeExportTaskElement.EndDate = materialsElement.EndDate;
                boeExportTaskElement.BOETaskID = materialsElement.TaskID;
                boeExportTaskElement.ElementType = BOEExportTaskElementType.Material;

                boeExportTaskElement.ExportFields[BOEExporter.FieldName_TaskCostTotal] = boeExportTaskElement.taskElementLabors.Where(l => l.Cost.HasValue).Sum(l => l.Cost.Value).ToString("C0", this.CurrencyFormatter);
                boeExportTaskElements.Add(boeExportTaskElement);
            }

            if (writelogstatements)
            {
                this.Logger.Info("Exporting - BOEExporter - ConvertBoeDTOToExportMV - material tasks end");
            }
        }

        #endregion

        /// <summary>
        /// Gets BOE level custom field values for isgs/space
        /// </summary>
        /// <param name="workspaceCustomFields">custom fields in the workspace</param>
        /// <param name="workspaceCustomFieldValues">custom field values in the workspace</param>
        /// <param name="boe">boe with custom fields</param>
        /// <param name="boeExportModelView">export model view for the boe</param>
        protected virtual void GetCompanySpecificBOECustomFields(IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields, IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues, BoeDTO boe, BOEExportModelView boeExportModelView)
        {
            // none needed for space
        }

        /// <summary>
        /// Gets task level custom field values for isgs/space
        /// </summary>
        /// <param name="workspaceCustomFields">custom fields in the workspace</param>
        /// <param name="workspaceCustomFieldValues">custom field values in the workspace</param>
        /// <param name="boeTaskElement">task element with custom fields</param>
        /// <param name="boeExportTaskElement">export task element for the task</param>
        protected virtual void GetCompanySpecificTaskCustomFields(IReadOnlyCollection<CustomFieldDTO> workspaceCustomFields, IReadOnlyCollection<CustomFieldValueDTO> workspaceCustomFieldValues, BoeTaskElementDTO boeTaskElement, BOEExportTaskElement boeExportTaskElement)
        {
            // none needed for space
        }

        /// <summary>
        /// Collects any ordinary and workspace variables that are sum of boe types and creates model views containing the selected boe's wbs, clin and total information.  Ignores discrete variables
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns>
        /// the model view collection of variables and selected boe information
        /// </returns>
        private Collection<BOEExportTaskElementMOQVariableModelView> GetSumOfBoeMOQVariableModelView(BOEExportTaskElement element, BOEExportInputs exportInputs)
        {
            Collection<OrdinaryVariableDto> variables = element.OrdinaryVariables;
            Collection<WorkspaceVariableDTO> wsVariables = element.WorkspaceVariables;
            Collection<BOEExportTaskElementMOQVariableModelView> toReturn = new Collection<BOEExportTaskElementMOQVariableModelView>();
            foreach (OrdinaryVariableDto item in variables)
            {
                if (item.ValueType == VarValueType.SumOfBOEs)
                {
                    BOEExportTaskElementMOQVariableModelView ovmv = new BOEExportTaskElementMOQVariableModelView();
                    ovmv.OrdinaryVariableName = item.OrdinaryVariableName;
                    foreach (SelectBOEsToSum item2 in item.SelectedBOEsToSum)
                    {
                        if (item2.BoeID.HasValue && item2.BoeID > 0)
                        {
                            BOEExportTaskElementMOQVariableBOEModelView ovboemv = new BOEExportTaskElementMOQVariableBOEModelView();

                            // get CLIN and WBS IDs if missing
                            // only BOE ID is populated when saving variables
                            // this is a temporary fix to get the export working properly sooner
                            // root cause to be addressed later when more time is available
                            BoeDTO boe = exportInputs.AllWorkspaceBoes.FirstOrDefault(x => x.Id == item2.BoeID);
                            if (boe != null)
                            {
                                if (item2.WBSID == null)
                                {
                                    item2.WBSID = boe.WBSID;
                                }

                                if (item2.CLINID == null)
                                {
                                    item2.CLINID = boe.CLINID;
                                }
                            }

                            WbsDTO wbs = exportInputs.WbsElements.FirstOrDefault(x => x.Id == item2.WBSID);
                            ClinDTO clin = exportInputs.Clins.FirstOrDefault(x => x.Id == item2.CLINID);

                            if (wbs != null)
                            {
                                ovboemv.WBSNumber = wbs.WbsNumber;
                                ovboemv.WBSTitle = wbs.WbsTitle;
                            }

                            if (clin != null)
                            {
                                ovboemv.ClinNumber = clin.ClinNumber;
                            }

                            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = item2.BoeID } } } },
                                new List<WorkspaceVariableDTO>() { new WorkspaceVariableDTO() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = item2.BoeID } } } }, 
                                exportInputs.WbsElements, exportInputs.AllWorkspaceBoes, exportInputs.TaskElements, exportInputs.ResourcesForWsResourceListId, exportInputs.Clins);
                            ovboemv.Total = this.variableSelectBOEtoSumCalculation.GetTotalBasedOnBoeID(item2.BoeID.Value, item.SumVariableResourceTypeIDs.ToCollection(), data);
                            ovmv.ReferencedBOEs.Add(ovboemv);
                        }
                    }

                    ovmv.Total = ovmv.ReferencedBOEs.Sum(x => x.Total);
                    toReturn.Add(ovmv);
                }
            }

            foreach (WorkspaceVariableDTO item in wsVariables)
            {
                if (item.ValueType == VarValueType.SumOfBOEs)
                {
                    BOEExportTaskElementMOQVariableModelView ovmv = new BOEExportTaskElementMOQVariableModelView();
                    ovmv.OrdinaryVariableName = item.WorkspaceVariableName;
                    foreach (SelectBOEsToSum item2 in item.SelectedBOEsToSum)
                    {
                        if (item2.BoeID.HasValue && item2.BoeID > 0)
                        {
                            BOEExportTaskElementMOQVariableBOEModelView ovboemv = new BOEExportTaskElementMOQVariableBOEModelView();

                            // get CLIN and WBS IDs if missing
                            // only BOE ID is populated when saving variables
                            // this is a temporary fix to get the export working properly sooner
                            // root cause to be addressed later when more time is available
                            BoeDTO boe = exportInputs.AllWorkspaceBoes.FirstOrDefault(x => x.Id == item2.BoeID);
                            if (boe != null)
                            {
                                if (item2.WBSID == null)
                                {
                                    item2.WBSID = boe.WBSID;
                                }

                                if (item2.CLINID == null)
                                {
                                    item2.CLINID = boe.CLINID;
                                }
                            }

                            WbsDTO wbs = exportInputs.WbsElements.FirstOrDefault(x => x.Id == item2.WBSID);
                            ClinDTO clin = exportInputs.Clins.FirstOrDefault(x => x.Id == item2.CLINID);

                            if (wbs != null)
                            {
                                ovboemv.WBSNumber = wbs.WbsNumber;
                                ovboemv.WBSTitle = wbs.WbsTitle;
                            }

                            if (clin != null)
                            {
                                ovboemv.ClinNumber = clin.ClinNumber;
                            }

                            DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
                            data.FillData(new List<OrdinaryVariableDto>() { new OrdinaryVariableDto() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = item2.BoeID } } } },
                                new List<WorkspaceVariableDTO>() { new WorkspaceVariableDTO() { SelectedBOEsToSum = new Collection<SelectBOEsToSum>() { new SelectBOEsToSum() { BoeID = item2.BoeID } } } },
                                exportInputs.WbsElements, exportInputs.AllWorkspaceBoes, exportInputs.TaskElements, exportInputs.ResourcesForWsResourceListId, exportInputs.Clins);
                            ovboemv.Total = this.variableSelectBOEtoSumCalculation.GetTotalBasedOnBoeID(item2.BoeID.Value, item.SumVariableResourceTypeIDs, data);
                            ovmv.ReferencedBOEs.Add(ovboemv);
                        }
                    }

                    ovmv.Total = ovmv.ReferencedBOEs.Sum(x => x.Total);
                    toReturn.Add(ovmv);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Sets the WorkspaceDecimalPrecision for the export
        /// </summary>
        /// <param name="ws">Workspace containing BOE(s) in export</param>
        public void SetWorkspacePrecisionVariables(WorkspaceDTO ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            this.WorkspaceDecimalPrecision = ws.DecimalPrecision;
        }

        /// <summary>
        /// Gets an RTE field or the overridden prompts and answers.
        /// </summary>
        /// <param name="boeId">The boe for the override.</param>
        /// <param name="taskId">The task for the override.</param>
        /// <param name="original">The original text if not overridden.</param>
        /// <param name="source">The source for the override.</param>
        /// <param name="answers">The overrides for this workspace.</param>
        /// <returns></returns>
        public static string GetRteOverride(int boeId, int? taskId, string original, RteTemplateSource source, IReadOnlyCollection<RTECustomTemplateQuestionAnswerModelView> answers)
        {
            ICollection<RTECustomTemplateQuestionAnswerModelView> overrides = answers.Where(a => a.SourceId == (int)source && a.BoeId == boeId && a.TaskId == taskId).OrderBy(t => t.SortOrder).ToList();

            if (overrides.Any())
            {
                StringBuilder sb = new StringBuilder();
                foreach (RTECustomTemplateQuestionAnswerModelView answer in overrides)
                {
                    sb.AppendLine();
                    sb.Append("<b>");
                    sb.Append(answer.QuestionText);
                    sb.AppendLine("</b>");
                    sb.AppendLine(answer.AnswerText ?? "<br />");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            else
            {
                return original ?? string.Empty;
            }
        }
    }
}
