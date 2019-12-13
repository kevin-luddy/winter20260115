// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;

    /// <summary>
    /// Used to get a collection of IDs based on the ID being passed in
    /// </summary>
    /// <param name="id">Id by which to find the collection</param>
    /// <returns>result</returns>
    public delegate ICollection<int> GetCollectionOfIdsById(int id);

    //Common Reference Types
    public delegate Collection<RoleModelView> GetRoleDelegate();
    public delegate Collection<SortByModelView> GetSortByDelegate();
    public delegate Collection<WorkspaceStateModelView> GetWorkspaceStateDelegate();
    public delegate Collection<ProposalStatusTypeModelView> GetProposalStatusTypesDelegate();
    public delegate Collection<ElementOfCostTypeModelView> GetElementOfCostTypesDelegate();
    public delegate Collection<BOEStateModelView> GetBOEStateDelegate();
    public delegate Collection<EnumTypeModelView> GetEnumDelegate();
    public delegate Collection<SpreadCurveModelView> GetSpreadCurveDelegate();
    public delegate Collection<MOQTypeModelView> GetMOQTypeDelegate();
    public delegate Collection<EmailModelDomain> GetEmailDelegate();
    public delegate Collection<ReportDTO> GetReportsDelegate();
    public delegate Collection<FieldTypeModelView> GetFieldTypeDelegate();
    public delegate Collection<EnumTypeModelView> GetProPricerFieldsDelegate(bool isProjectMapType);
    public delegate Collection<SegmentTypeModelView> GetSegmentTypesDelegate();
    public delegate Collection<RateTypeModelView> GetRateTypesDelegate();
    public delegate Collection<OtherDirectCostSpreadCurveModelView> GetODCSpreadCurveDelegate();
    public delegate Collection<SumVariableResourceTypeModelView> GetSumVariableResourceTypesDelegate();
    public delegate Collection<SikorskyLegacyResourceDTO> GetSikorskyLegacyResourcesDelegate();
}
