// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.ActionLogic.Reporting;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Objects;
	using IES.Common;
	using IES.Common.OfficeUtilities;

	[ExcludeFromCodeCoverage]
    public class WorkspaceExporterSpaceSystems:WorkspaceExporter
    {
        public override string WORKSPACE_DATA_EXCEL_MAP_PATH
        {
            get {
				if (Utilities.IsBRCEnabledForSystem)
				{
					return "~/Templates/Export/WorkspaceDataWithBRCSpaceSystems.xlsx";
				}
				else
				{
					return "~/Templates/Export/WorkspaceDataSpaceSystems.xlsx";
				}
			}
        }

        public WorkspaceExporterSpaceSystems(
            ICommonDataMapper inICommonDataMapper,
            IPermissionsDTODataLoader inIPermissionsDTOLoader,
            ICustomFieldValueDTODataLoader inICustomFieldValueDTODataLoader,
            IUserDTODataLoader inIUserDTODataLoader,
            IBOEStatusReport inBOEStatusReport,
            WbsExporter inWbsExporter,
            TravelTripCostCalculation inTravelTripCostCalculation,
            ILocationDTODataLoader inLocationDtoDataLoader,
            ICLINExporter inClinExporter)
            : base(inICommonDataMapper,
             inIPermissionsDTOLoader,
             inICustomFieldValueDTODataLoader,
             inIUserDTODataLoader,
             inBOEStatusReport,
             inWbsExporter,
             inTravelTripCostCalculation,
             inLocationDtoDataLoader,
             inClinExporter)
        {

        }

        /// <summary>
        /// Gets the workspace identification sheet export data.
        /// </summary>
        /// <param name="exportInputs">The export inputs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">exportInputs</exception>

        protected override ExcelExportWorksheet GetWorkspaceIdentificationSheetExportData(BOEExportInputs exportInputs)
        {
            if (exportInputs == null)
            {
                throw new ArgumentNullException(nameof(exportInputs));
            }

			ExcelExportWorksheet toReturn = new ExcelExportWorksheet("Workspace Identification");

			Dtos.WorkspaceDTO workspace = exportInputs.Workspace;

            toReturn.Add(this.sEmpty, workspace.WorkspaceName);
            toReturn.Add(this.sEmpty, workspace.Description);
            toReturn.Add(this.sEmpty, workspace.LineOfBusiness.Text);
            toReturn.Add(this.sEmpty, workspace.ProposalClass.Text);
            string contractTypesList = string.Empty;
            if (workspace.SelectedContractTypes != null)
            {
                ICollection<string> contractTypes = exportInputs.ContractTypes.Where(c => workspace.SelectedContractTypes.Contains(c.Id)).Select(ct => ct.Text).ToList();
                contractTypesList = string.Join(",", contractTypes);
            }

            toReturn.Add(this.sEmpty, contractTypesList);
            toReturn.Add(this.sEmpty, this.UserDTODataLoader.GetUserByID(workspace.CostVolumeLeadPricerUserID).DisplayName);
            toReturn.Add(this.sEmpty, workspace.ContractStartDate.ToString("MM/yyyy"));
            toReturn.Add(this.sEmpty, workspace.ContractEndDate.ToString("MM/yyyy"));
            toReturn.Add(this.sEmpty, workspace.ProposalSubmittalDate.HasValue ? workspace.ProposalSubmittalDate.Value.ToString("MM/yyyy") : this.sEmpty);
            toReturn.Add(this.sEmpty, workspace.DecimalPrecision.ToString());
            toReturn.Add(this.sEmpty, workspace.CostDecimalPrecision.ToString());
            toReturn.Add(this.sEmpty, workspace.TrackingNumber);
            if (FullObjectHelper.ShowEquivalentPersonsOption)
            {
                toReturn.Add("Is using Equivalent Person (EP)", workspace.IsUsingEquivalentPerson ? this.sYes : this.sNo);
            }
            toReturn.Add("OCI", workspace.ContainsOCI ? this.sYes : this.sNo);
            toReturn.Add("Proposal Status", this.CommonDataMapper.getProposalStatusTypesDictionary()[(int)workspace.ProposalStatus].ProposalStateType);
            toReturn.Add("Proposal Comments", workspace.StatusComment);
            toReturn.Add("MOQ Template BOEs", workspace.UsingTemplateBOE ? this.sYes : this.sNo);
            toReturn.Add("SAP Connection Enabled", workspace.EnableSAPConnection ? this.sYes : this.sNo);

            if (!FullObjectHelper.ShowEquivalentPersonsOption)
            {
                // Without this, we won't have the right number of rows.. If this is not there, the resulting Excel file exports, but will then throw an error saying it's corrupted
                toReturn.Add(this.sEmpty, this.sEmpty);
            }

            return toReturn;
        }
    }
}
