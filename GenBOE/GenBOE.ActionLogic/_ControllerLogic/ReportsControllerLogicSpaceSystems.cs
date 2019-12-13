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
    using DataBridge.Reference;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.ActionLogic.WBS.BOE;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;
    using IES.Common.Exceptions;

    public class ReportsControllerLogicSpaceSystems : ReportsControllerLogic
    {
        private IBOEFormIBOEDTODataLoader iboeFormDataLoader;
        private IBOEFormPBOEDTODataLoader pboeFormDataLoader;
        private IResourceDTODataLoader resourceDTODataLoader;
        private IInUseDataLoader iInUseDataLoader;

        public ReportsControllerLogicSpaceSystems(
            IBOEExporter boeExporter,
            BOESummary boeSummary,
            IBOECustomExporter boeCustomExporter,
            IWorkspaceExportFormatDTODataLoader workspaceExportFormatDTOLoader,
            BOEDiscrepancyReport boeDiscrepancyReport,
            IResourceDTODataLoader resourceDTODataLoader,
            IBOEFormIBOEDTODataLoader iboeFormDataLoader,
            IBOEFormPBOEDTODataLoader pboeFormDataLoader,
            IInUseDataLoader iInUseDataLoader,
            IProposalLoader proposalLoader,
            IWorkspaceControllerLogic workspaceControllerLogic)
            : base(boeExporter, boeSummary, boeCustomExporter, workspaceExportFormatDTOLoader, boeDiscrepancyReport, proposalLoader, workspaceControllerLogic)
        {
            this.iboeFormDataLoader = iboeFormDataLoader;
            this.pboeFormDataLoader = pboeFormDataLoader;
            this.resourceDTODataLoader = resourceDTODataLoader;
            this.iInUseDataLoader = iInUseDataLoader;
        }

        /// <summary>
        /// Gets a boolen indicating if custom export is support per SSC company configuration
        /// </summary>
        public override bool SupportCustomExport { get { return true; } }

        /// <summary>
        /// Performs Validation for DisplayInlFormExportGrid
        /// </summary>
        /// <param name="ws">FullWorkspace</param>
        /// <returns>ICollection of ValidationMessage</returns>
        public override ICollection<ValidationMessage> DisplayInlFormExportGrid_Validate(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            Collection<ValidationMessage> validationMessage = base.DisplayInlFormExportGrid_Validate(ws).ToCollection();

            // get all resourceids that are in use in this workspace by some list type(is this a lookup value)
            HashSet<int> resourceIDsInUse = this.iInUseDataLoader.GetWorkspaceResourceIDsInUseByListID(ws.ResourceListID);

            // take the above ids and simply get all resources based on those ids above.
            // then we have all resources that are IN USE in THIS WORKSPACE
            ICollection<ResourceDTO> allResources = (this.resourceDTODataLoader
                .GetByListId(ws.ResourceListID)
                .Where(x => resourceIDsInUse.Contains(x.Id) && (x.RateType == RateType.Cost || x.RateType == RateType.Hours))).ToList();

            ICollection<int> resourcesInUseInWorkspace = this.pboeFormDataLoader.CurrentlyUsedResources(ws.Id, 0);
            ICollection<ResourceDTO> items = allResources.Where(x => x.ElementOfCost == ElementOfCostType.Sub && !resourcesInUseInWorkspace.Contains(x.Id)).ToList();
            if (items.Any())
            {
                string s = string.Join(", ", items.Select(x => x.Id + " - " + x.ResourceName));
                validationMessage.Add(new ValidationMessage("", "There are " + items.Count + " SUB Resources (" + s + ") that are not associated with any PBOE forms."));
            }

            resourcesInUseInWorkspace = this.iboeFormDataLoader.CurrentlyUsedResources(ws.Id, 0);
            items = allResources.Where(r => r.ElementOfCost == ElementOfCostType.IWTA && !resourcesInUseInWorkspace.Contains(r.Id)).ToList();
            if (items.Any())
            {
                string s = string.Join(", ", items.Select(x => x.Id + " - " + x.ResourceName));
                validationMessage.Add(new ValidationMessage("", "There are " + items.Count + " IWTA Resources (" + s + ") that are not associated with any IBOE forms."));
            }

            ICollection<FullClin> noContractClins = ws.ClinsNoMultiClin.Where(x => x.ContractType == Constants.CONTRACT_TYPE_NOT_SET).ToList();
            if (noContractClins.Any())
            {
                string s = string.Join(", ", noContractClins.Select(x => x.ClinString));
                validationMessage.Add(new ValidationMessage("ContractType", "There are " + noContractClins.Count + " CLINS (" + s + ") that do not have a Contract Type set."));
            }

            ICollection<FullBoe> boesWithNoClinSet = ws.Boes.Where(x => x.CLINID == null).ToList();
            if (boesWithNoClinSet.Any())
            {
                string s = string.Join(", ", boesWithNoClinSet.Select(x => x.Id + " - " + x.Title));
                validationMessage.Add(new ValidationMessage("", "Workspace contains " + boesWithNoClinSet.Count + " BOEs (" + s + ") not assigned to CLINS."));
            }

            ICollection<FullBoe> boesWithMultiClinWbsWithNoClinSet = ws.Boes.Where(x => x.IsMultiClinWbs && x.LaborTypes.Any(l => l.CLINID == null)).ToList();
            if (boesWithMultiClinWbsWithNoClinSet.Any())
            {
                string s = string.Join(", ", boesWithMultiClinWbsWithNoClinSet.Select(x => x.Id + " - " + x.Title));
                validationMessage.Add(new ValidationMessage("IsMultiClinWbs", "There are " + boesWithMultiClinWbsWithNoClinSet.Count + " MultiCLIN BOEs (" + s + ") that have Labor Type(s) with no CLIN set."));
            }

            return validationMessage;
        }
    }
}