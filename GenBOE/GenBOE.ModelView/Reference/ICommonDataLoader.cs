// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common.PickList;

    public interface ICommonDataLoader
    {
        Collection<BOEStateModelView> GetBOEStates();
        Collection<ElementOfCostTypeModelView> GetElementOfCostTypes();
        Collection<EmailModelDomain> GetEmails();
        Collection<FieldTypeModelView> GetFieldTypes();
        Collection<EnumTypeModelView> GetMSTTravelModes();
        Collection<MOQTypeModelView> GetMOQTypes();
        Collection<OtherDirectCostSpreadCurveModelView> GetODCSpreadCurve();
        Collection<ProposalStatusTypeModelView> GetProposalStatusTypes();
        ICollection<PickListDto> GetSelectedContractTypes(int inWorkspaceId);
        Collection<EnumTypeModelView> GetProPricerFields(bool isProjectMapType);
        Collection<RateTypeModelView> GetRateTypes();
        Collection<ReportDTO> GetReports();
        string getResourceName(int inResourceID);
        Collection<RoleModelView> GetRoles();
        Collection<SegmentTypeModelView> GetSegmentTypes();
        Collection<SortByModelView> GetSortBy();
        Collection<SpreadCurveModelView> GetSpreadCurve();
        Collection<SpreadCurveModelView> GetProjectMapSpreadCurve();
        Collection<SumVariableResourceTypeModelView> GetSumVariableResourceTypes();
        Collection<WorkspaceStateModelView> GetWorkspaceStates();
        Collection<SikorskyLegacyResourceDTO> GetSikorskyLegacyResources();
        /// <summary>
        /// Saves the system email.
        /// </summary>
        /// <param name="email">The email to save.</param>
        void SaveSystemEmail(EmailModelDomain email);
    }
}
