// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using IES.Common;
    using GenBOE.Dtos;

    /// <summary>
    /// This class will load the common lookup/reference data from the Loaders
    /// and pass the reults through the CacheDataLoader which will cache
    /// if injected with the Cached data loader.
    /// 
    /// Callers should NOT invoke these methods inside of loops. Instead, grab the entire collection or dictionary outside
    /// of the loop. Inside of the loop, you can either lookup values using the dictionary or search for names in the collection. This prevents impacts to performance.
    /// 
    /// </summary>
    public interface ICommonDataMapper
    {
        #region BOEStates
        /// <summary>
        /// Gets all the BOEStates
        /// </summary>
        /// <returns>Collection of BOEStates</returns>
        Collection<BOEStateModelView> getBOEStates();

        /// <summary>
        /// Get the BOEStates as a dictionary
        /// </summary>
        /// <returns>Dictionary of BOE states</returns>
        IDictionary<int, BOEStateModelView> getBOEStatesDictionary();

        /// <summary>
        /// Gets the Name of the BOEState 
        /// </summary>
        /// <param name="inEnumToGetName"></param>
        /// <returns>BOE State name</returns>
        string getBOEStateName(BOEState inEnumToGetName);
        #endregion 

        #region MOQTypes
        /// <summary>
        /// Gets all the MOQTypes
        /// </summary>
        /// <returns></returns>
        Collection<MOQTypeModelView> getMOQType();

        /// <summary>
        /// Gets all the MOQTypes as a dictionary
        /// </summary>
        /// <returns>Dictionary of MOQ Types</returns>
        IDictionary<int,MOQTypeModelView> getMOQTypeDictionary();

        /// <summary>
        /// Gets the name of the MOQType
        /// </summary>
        /// <param name="inEnumToGetName">MOQType to get the name of</param>
        /// <returns>MOQType name</returns>
        string getMOQTypeName(MOQType inEnumToGetName);
        #endregion
        
        #region Roles
        /// <summary>
        /// Gets all roles
        /// </summary>
        /// <returns>Collection of RoleModelView</returns>
        Collection<RoleModelView> getRoles();

        /// <summary>
        /// Get the Roles as a dictionary
        /// </summary>
        /// <returns>Dictionary of Roles</returns>
        IDictionary<int, RoleModelView> getRolesDictionary();
        #endregion

        #region SpreadCurves
        /// <summary>
        /// Gets all spread curves
        /// </summary>
        /// <returns>Collection of SpreadCurveModelView</returns>
        Collection<SpreadCurveModelView> getSpreadCurve();

        /// <summary>
        /// Gets the spread cruves as a dictionary
        /// </summary>
        /// <returns>Dictionary of spread curves</returns>
        IDictionary<int,SpreadCurveModelView> getSpreadCurveDictionary();

        /// <summary>
        /// get the Project Map spread curve and set up cache
        /// </summary>
        /// <returns>Collection of spread curves</returns>
        Collection<SpreadCurveModelView> getProjectMapSpreadCurve();

        /// <summary>
        /// get the Project Map spread curve as a dictionary
        /// </summary>
        /// <returns>Dictionary of spread curves</returns>
        IDictionary<int, SpreadCurveModelView> getProjectMapSpreadCurveDictionary();
        #endregion

        #region WorkspaceStates
        /// <summary>
        /// Gets all the workspace states
        /// </summary>
        /// <returns>Collection of WorkspaceStateModelView</returns>
        Collection<WorkspaceStateModelView> getWorkspaceStates();

        /// <summary>
        /// Get the WorkspaceStates as a dictionary
        /// </summary>
        /// <returns>Dictionary of WorkSpace states</returns>
        IDictionary<int,WorkspaceStateModelView> getWorkspaceStatesDictionary();

        string getWorkspaceStateName(WorkspaceState inStateToGetName);
        #endregion

        #region FieldTypes
        /// <summary>
        /// Gets all the Field types
        /// </summary>
        /// <returns>Collection of FieldTypeModelView</returns>
        Collection<FieldTypeModelView> getFieldTypes();

        /// <summary>
        /// Get the FieldTypes as a dictionary
        /// </summary>
        /// <returns>Dictionary of FieldType states</returns>
        IDictionary<int, FieldTypeModelView> getFieldTypesDictionary();
        #endregion

        #region ProPricerFields
        /// <summary>
        /// Gets the ProPricer Fields
        /// </summary>
        /// <returns>Collection of EnumTypeModelView</returns>
        Collection<EnumTypeModelView> GetProPricerFields(bool isProjectMapType);

        /// <summary>
        /// Get all ProPricer Fields as a dictionary
        /// </summary>
        /// <param name="wsProjectMapType"></param>
        /// <returns>List of fields</returns>
        IDictionary<int, EnumTypeModelView> GetProPricerFieldsDictionary(bool isProjectMapType, bool enableTaskAuthor);
        #endregion

        #region ProposalStatusTypes
        /// <summary>
        /// Gets all Proposal Status Types
        /// </summary>
        /// <returns>Collection of ProposalStatusTypeModelView</returns>
        Collection<ProposalStatusTypeModelView> getProposalStatusTypes();

        /// <summary>
        /// Get the ProposalStates as a dictionary
        /// </summary>
        /// <returns>Dictionary of ProposalStatusTypeModelView</returns>
        IDictionary<int,ProposalStatusTypeModelView> getProposalStatusTypesDictionary();
        #endregion

        #region ElementOfCostTypes
        /// <summary>
        /// Gets all ElementOfCostTypes
        /// </summary>
        /// <returns>Collection of ElementOfCostTypeModelView</returns>
        Collection<ElementOfCostTypeModelView> GetElementOfCostTypes();

        /// <summary>
        /// Get the ProposalStates as dictionary
        /// </summary>
        /// <returns>Dictionary of ElementOfCostTypeModelView</returns>
        IDictionary<int, ElementOfCostTypeModelView> GetElementOfCostTypesDictionary();
        #endregion

        #region RateTypes
        /// <summary>
        /// Gets all rate types
        /// </summary>
        /// <returns>Collection of RateTypeModelView</returns>
        Collection<RateTypeModelView> GetRateTypes();

        /// <summary>
        /// Get all Rate Types as a dictionary
        /// </summary>
        /// <returns>Dictionary of rate types</returns>
        IDictionary<int, RateTypeModelView> GetRateTypesDictionary();
        #endregion

        #region OdcSpreadCurve
        /// <summary>
        /// Get all ODC Spread Curves
        /// </summary>
        /// <returns>Collection of OtherDirectCostSpreadCurveModelView</returns>
        Collection<OtherDirectCostSpreadCurveModelView> getOdcSpreadCurve();

        /// <summary>
        /// get the spread cruve as dictionary
        /// </summary>
        /// <returns>Dictionary of spread curves</returns>
        IDictionary<int, OtherDirectCostSpreadCurveModelView> getOdcSpreadCurveDictionary();
        #endregion

        #region SikorskyLegacyResources

        /// <summary>
        /// Get all Sikorsky Legacy Resources.
        /// </summary>
        /// <returns>Collection of SikorskyLegacyResourceDTO</returns>
        Collection<SikorskyLegacyResourceDTO> GetSikorskyLegacyResources();

        /// <summary>
        /// Get all Sikorsky Legacy Resources as a dictionary.
        /// </summary>
        /// <param name="isProjectMapWorkspace">True, if this is a Project Map Workspace; false otherwise.</param>
        /// <returns>Dictionary of Sikorsky Legacy Resources.</returns>
        IDictionary<int, SikorskyLegacyResourceDTO> GetSikorskyLegacyResourcesDictionary(bool isProjectMapWorkspace);

        /// <summary>
        /// Get all Sikorsky Legacy Resources as a dictionary with LegacyResourceID as the key.  Typically used during import processes
        /// where values need to be converted for saving.
        /// </summary>
        /// <returns>Dictionary of Sikorsky Legacy Resources.</returns>
        IDictionary<string, SikorskyLegacyResourceDTO> GetSikorskyLegacyResourcesConversionDictionary();

        /// <summary>
        /// Returns the Legacy Resource ID based upon the LegacyID from the set of Sikorsky Legacy Resources.
        /// Typically used for lookups or during export processes.
        /// </summary>
        /// <param name="legacyID">The LegacyID (the ID of the entity).</param>
        /// <param name="allSikorskyLegacyResources">Dictionary of LegacyIDs mapping to the corresponding SikorskyLegacyResourceDTOs.</param>
        /// <returns>The ResourceID if found, an empty string otherwise.</returns>
        string GetSikorskyLegacyResourceID(int? legacyID, IDictionary<int, SikorskyLegacyResourceDTO> allSikorskyLegacyResources);

        /// <summary>
        /// Returns the LegacyID (the entity ID) based upon the LegacyResourceID from the set of Sikorsky Legacy Resources.
        /// Typically used for converting during import processes.
        /// </summary>
        /// <param name="legacyResourceID">The Legacy Resource ID.</param>
        /// <param name="allSikorskyLegacyResources">Dictionary of LegacyResourceIDs mapping to the corresponding SikorskyLegacyResourceDTOs.</param>
        /// <returns>The LegacyID (the ID of the entity) if found, null otherwise.</returns>
        int? GetSikorskyLegacyID(string legacyResourceID, IDictionary<string, SikorskyLegacyResourceDTO> allSikorskyLegacyResources);

        #endregion SikorskyLegacyResources

        /// <summary>
        /// Gets all EmailModelDomains
        /// </summary>
        /// <returns>Collection of EmailModelDomains</returns>
        ICollection<EmailModelDomain> GetEmails();

        /// <summary>
        /// Saves the system emails.
        /// </summary>
        /// <param name="emails">The emails.</param>
        void SaveSystemEmails(ICollection<EmailModelDomain> emails);

        /// <summary>
        /// Gets all reports
        /// </summary>
        /// <returns>Collection ReportDTO</returns>
        Collection<ReportDTO> getReports();

        /// <summary>
        /// Get the SortBy and set up cache
        /// </summary>
        /// <returns>list of SortBy</returns>
        Collection<SortByModelView> getSortBy();

        /// <summary>
        /// Gets all SumVariableResourceTypes
        /// </summary>
        /// <returns>Collection of SumVariableResourceTypeModelView</returns>
        Collection<SumVariableResourceTypeModelView> GetSumVariableResourceTypes();

        #region MSTTravelModes
        /// <summary>
        /// Gets all MSTTravelMode
        /// </summary>
        /// <returns>Collection of EnumTypeModelView</returns>
        Collection<EnumTypeModelView> GetMSTTravelModes();

        /// <summary>
        /// Gets MSTTravelModes as a dictionary
        /// </summary>
        /// <returns>Dictionary of MSTTravelModes</returns>
        IDictionary<int, EnumTypeModelView> GetMSTTravelModesDictionary();
        #endregion


    }
}
