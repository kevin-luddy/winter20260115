// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common;


    /// <summary>
    /// This class will load the common lookup/reference data from the Loaders
    /// and pass the reults through the CacheDataLoader which will cache
    /// if injected with the Cached data loader.
    /// 
    /// Callers should NOT invoke these methods inside of loops. Instead, grab the entire collection or dictionary outside
    /// of the loop. Inside of the loop, you can reference that variable to either lookup values using the dictionary or 
    /// search for names in the collection. This eliminates multiple calls to the cache to re-create the same objects over
    /// and over again.
    /// 
    /// </summary>
    public class CommonDataMapper : ICommonDataMapper
    {
        private ICommonDataLoader _CommonDataLoader = null;
        private ICacheDataLoader _CacheDataLoader = null;

        /// <summary>
        /// Constructor with Common Data and Cache Data loader
        /// </summary>
        /// <param name="inCommonDataLoader"> the Common data loader</param>
        /// <param name="inCacheDataLoader">the Cache data loader </param>
        public CommonDataMapper(ICommonDataLoader inCommonDataLoader, ICacheDataLoader inCacheDataLoader)
        {
            _CacheDataLoader = inCacheDataLoader;
            _CommonDataLoader = inCommonDataLoader;
        }

        #region SikorskyLegacyResources

        /// <summary>
        /// Get all Sikorsky Legacy Resources and sets up the cache..
        /// </summary>
        /// <returns>Collection of SikorskyLegacyResourceDTO</returns>
        public Collection<SikorskyLegacyResourceDTO> GetSikorskyLegacyResources()
        {
            GetSikorskyLegacyResourcesDelegate theDelegate = new GetSikorskyLegacyResourcesDelegate(_CommonDataLoader.GetSikorskyLegacyResources);
            object toReturn = _CacheDataLoader.GetData(theDelegate, null, CacheConstants.SIKORSKY_LEGACY_RESOURCES, false);
            return toReturn as Collection<SikorskyLegacyResourceDTO>;
        }

        /// <summary>
        /// Get all Sikorsky Legacy Resources as a dictionary with LegacyID as the key.
        /// </summary>
        /// <param name="isProjectMapWorkspace">True, if this is a Project Map Workspace; false otherwise.</param>
        /// <returns>Dictionary of Sikorsky Legacy Resources.</returns>
        public IDictionary<int, SikorskyLegacyResourceDTO> GetSikorskyLegacyResourcesDictionary(bool isProjectMapWorkspace)
        {
            return isProjectMapWorkspace ? this.GetSikorskyLegacyResources().ToDictionary(x => x.LegacyID) : null;
        }

        /// <summary>
        /// Get all Sikorsky Legacy Resources as a dictionary with LegacyResourceID as the key.  Typically used during import processes
        /// where values need to be converted for saving.
        /// </summary>
        /// <returns>Dictionary of Sikorsky Legacy Resources.</returns>
        public IDictionary<string, SikorskyLegacyResourceDTO> GetSikorskyLegacyResourcesConversionDictionary()
        {
            return this.GetSikorskyLegacyResources().ToDictionary(x => x.LegacyResourceID);
        }

        /// <summary>
        /// Returns the Legacy Resource ID based upon the LegacyID from the set of Sikorsky Legacy Resources.
        /// Typically used for lookups or during export processes.
        /// </summary>
        /// <param name="legacyID">The LegacyID (the ID of the entity).</param>
        /// <param name="allSikorskyLegacyResources">Dictionary of LegacyIDs mapping to the corresponding SikorskyLegacyResourceDTOs.</param>
        /// <returns>The ResourceID if found, an empty string otherwise.</returns>
        public string GetSikorskyLegacyResourceID(int? legacyID, IDictionary<int, SikorskyLegacyResourceDTO> allSikorskyLegacyResources)
        {
            if (allSikorskyLegacyResources != null && legacyID.HasValue && allSikorskyLegacyResources.ContainsKey(legacyID.Value))
            {
                return  allSikorskyLegacyResources[legacyID.Value].LegacyResourceID;
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the LegacyID (the entity ID) based upon the LegacyResourceID from the set of Sikorsky Legacy Resources.
        /// Typically used for converting during import processes.
        /// </summary>
        /// <param name="legacyResourceID">The Legacy Resource ID.</param>
        /// <param name="allSikorskyLegacyResources">Dictionary of LegacyResourceIDs mapping to the corresponding SikorskyLegacyResourceDTOs.</param>
        /// <returns>The LegacyID (the ID of the entity) if found, null otherwise.</returns>
        public int? GetSikorskyLegacyID(string legacyResourceID, IDictionary<string, SikorskyLegacyResourceDTO> allSikorskyLegacyResources)
        {
            if (allSikorskyLegacyResources != null && !string.IsNullOrEmpty(legacyResourceID) && allSikorskyLegacyResources.ContainsKey(legacyResourceID.ToUpper()))
            {
                return allSikorskyLegacyResources[legacyResourceID.ToUpper()].LegacyID;
            }

            return null;
        }

        #endregion SikorskyLegacyResources

        #region ElementOfCostTypes
        /// <summary>
        /// Get the ProposalStates and set up cache
        /// </summary>
        /// <returns>Collection of ElementOfCostTypeModelView</returns>
        public virtual Collection<ElementOfCostTypeModelView> GetElementOfCostTypes()
        {
            GetElementOfCostTypesDelegate theDelegate = new GetElementOfCostTypesDelegate(_CommonDataLoader.GetElementOfCostTypes);
            object toReturn = _CacheDataLoader.GetData(theDelegate, null, CacheConstants.ELEMENTS_OF_COST_TYPE, false);
            return toReturn as Collection<ElementOfCostTypeModelView>;
        }
        
        /// <summary>
        /// Get the ProposalStates as dictionary
        /// </summary>
        /// <returns>Dictionary of ElementOfCostTypeModelView</returns>
        public virtual IDictionary<int,ElementOfCostTypeModelView> GetElementOfCostTypesDictionary()
        {
            return this.GetElementOfCostTypes().ToDictionary(x => x.ElementOfCostId);
        }
        #endregion

        #region ProposalStates
        /// <summary>
        /// Get the ProposalStates and set up cache
        /// </summary>
        /// <returns>Collection of ProposalStatusTypeModelView</returns>
        public virtual Collection<ProposalStatusTypeModelView> getProposalStatusTypes()
        {
            GetProposalStatusTypesDelegate theDelegate = new GetProposalStatusTypesDelegate(_CommonDataLoader.GetProposalStatusTypes);
            object toReturn = _CacheDataLoader.GetData(theDelegate, null, CacheConstants.PROPOSAL_STATE_TYPE, false);
            return toReturn as Collection<ProposalStatusTypeModelView>;
        }

        /// <summary>
        /// Get the ProposalStates as a dictionary
        /// </summary>
        /// <returns>Dictionary of ProposalStatusTypeModelView</returns>
        public virtual IDictionary<int,ProposalStatusTypeModelView> getProposalStatusTypesDictionary()
        {
            return this.getProposalStatusTypes().ToDictionary(x => x.ProposalStateID);
        }
        #endregion

        #region BOEStates
        /// <summary>
        /// Get the full string name for a given BOE State
        /// </summary>
        /// <param name="inStateToGetName">the enum state</param>
        /// <returns>the string value for the enum, from the database</returns>
        public virtual string getBOEStateName(BOEState inEnumToGetName)
        {
            return this.getBOEStates().Where(x => x.BOEStateID == (int)inEnumToGetName).Select(x => x.BOEState).FirstOrDefault();
        }

        /// <summary>
        /// Get the BOEStates and set up cache
        /// </summary>
        /// <returns>Collection of BOE states</returns>
        public virtual Collection<BOEStateModelView> getBOEStates()
        {
            GetBOEStateDelegate boeStateDelegate = new GetBOEStateDelegate(_CommonDataLoader.GetBOEStates);
            object toReturn = _CacheDataLoader.GetData(boeStateDelegate, null, CacheConstants.BOE_STATE, false);
            return toReturn as Collection<BOEStateModelView>;
        }

        /// <summary>
        /// Get the BOEStates as a dictionary
        /// </summary>
        /// <returns>Dictionary of BOE states</returns>
        public virtual IDictionary<int,BOEStateModelView> getBOEStatesDictionary()
        {
            return this.getBOEStates().ToDictionary(x => x.BOEStateID);
        }
        #endregion

        #region MOQType
        /// <summary>
        /// Get the full string name for a given MOQ Type
        /// </summary>
        /// <param name="inStateToGetName">the enum state</param>
        /// <returns>the string value for the enum, from the database</returns>
        public virtual string getMOQTypeName(MOQType inEnumToGetName)
        {
            return this.getMOQType().Where(x => x.MOQTypeID == (int)inEnumToGetName).Select(x => x.MOQTypeName).FirstOrDefault();
        }

        /// <summary>
        /// Gets the MOQ Type and sets up the cache
        /// </summary>
        /// <returns>Collection of MOQ Types</returns>
        public virtual Collection<MOQTypeModelView> getMOQType()
        {
            GetMOQTypeDelegate moqTypeDelegate = new GetMOQTypeDelegate(_CommonDataLoader.GetMOQTypes);
            object toReturn = _CacheDataLoader.GetData(moqTypeDelegate, null, CacheConstants.MOQ_TYPE, false);
            return toReturn as Collection<MOQTypeModelView>;
        }

        /// <summary>
        /// Gets the MOQ Type as a dictionary
        /// </summary>
        /// <returns>Dictionary of MOQ Types</returns>
        public virtual IDictionary<int,MOQTypeModelView> getMOQTypeDictionary()
        {
            return this.getMOQType().ToDictionary(x => x.MOQTypeID);
        }
        #endregion

        #region Roles

        /// <summary>
        /// Get the Roles and set up cache
        /// </summary>
        /// <returns>Collection of Roles</returns>
        public virtual Collection<RoleModelView> getRoles()
        {
            GetRoleDelegate roleDelegate = new GetRoleDelegate(_CommonDataLoader.GetRoles);
            object toReturn = _CacheDataLoader.GetData(roleDelegate, null, CacheConstants.ROLES, false);
            return toReturn as Collection<RoleModelView>;
        }

        /// <summary>
        /// Get the Roles as a dictionary
        /// </summary>
        /// <returns>Dictionary of Roles</returns>
        public virtual IDictionary<int,RoleModelView> getRolesDictionary()
        {
            return this.getRoles().ToDictionary(x => x.RoleID);
        }
        #endregion

        #region SpreadCurve
        /// <summary>
        /// get the spread curve and set up cache
        /// </summary>
        /// <returns>Collection of spread curves</returns>
        public virtual Collection<SpreadCurveModelView> getSpreadCurve()
        {
            GetSpreadCurveDelegate spreadCurveDelegate = new GetSpreadCurveDelegate(_CommonDataLoader.GetSpreadCurve);
            object toReturn = _CacheDataLoader.GetData(spreadCurveDelegate, null, CacheConstants.SPREAD_CURVE, false);
            return toReturn as Collection<SpreadCurveModelView>;
        }

        /// <summary>
        /// get the spread curve as a dictionary
        /// </summary>
        /// <returns>Dictionary of spread curves</returns>
        public virtual IDictionary<int,SpreadCurveModelView> getSpreadCurveDictionary()
        {
            return this.getSpreadCurve().ToDictionary(x => (int)x.SpreadCurveID);
        }

        /// <summary>
        /// get the Project Map spread curve and set up cache
        /// </summary>
        /// <returns>Collection of spread curves</returns>
        public virtual Collection<SpreadCurveModelView> getProjectMapSpreadCurve()
        {
            GetSpreadCurveDelegate spreadCurveDelegate = new GetSpreadCurveDelegate(_CommonDataLoader.GetProjectMapSpreadCurve);
            object toReturn = _CacheDataLoader.GetData(spreadCurveDelegate, null, CacheConstants.PROJECT_MAP_SPREAD_CURVE, false);
            return toReturn as Collection<SpreadCurveModelView>;
        }

        /// <summary>
        /// get the Project Map spread curve as a dictionary
        /// </summary>
        /// <returns>Dictionary of spread curves</returns>
        public virtual IDictionary<int, SpreadCurveModelView> getProjectMapSpreadCurveDictionary()
        {
            return this.getProjectMapSpreadCurve().ToDictionary(x => (int) x.SpreadCurveID);
        }
        #endregion

        #region WorkspaceStates
        /// <summary>
        /// Get the full string name for a given workspace state
        /// </summary>
        /// <param name="inStateToGetName">the enum state</param>
        /// <returns>the string value for the enum, from the database</returns>
        public virtual string getWorkspaceStateName(WorkspaceState inStateToGetName)
        {
            return this.getWorkspaceStatesDictionary()[(int)inStateToGetName].WorkspaceState;
        }

        /// <summary>
        /// Get the WorkspaceStates and set up cache
        /// </summary>
        /// <returns>Collection of WorkSpace states</returns>
        public virtual Collection<WorkspaceStateModelView> getWorkspaceStates()
        {
            GetWorkspaceStateDelegate workspaceStateDelegate = new GetWorkspaceStateDelegate(_CommonDataLoader.GetWorkspaceStates);
            object toReturn = _CacheDataLoader.GetData(workspaceStateDelegate, null, CacheConstants.WORKSPACE_STATE, false);
            return toReturn as Collection<WorkspaceStateModelView>;
        }

        /// <summary>
        /// Get the WorkspaceStates as a dictionary
        /// </summary>
        /// <returns>Dictionary of WorkSpace states</returns>
        public virtual IDictionary<int,WorkspaceStateModelView> getWorkspaceStatesDictionary()
        {
            return this.getWorkspaceStates().ToDictionary(x => x.WorkspaceStateID);
        }
        #endregion

        #region FieldTypes
        /// <summary>
        /// Get the FieldTypes and set up cache
        /// </summary>
        /// <returns>Collection of FieldType states</returns>
        public virtual Collection<FieldTypeModelView> getFieldTypes()
        {
            GetFieldTypeDelegate fieldTypeDelegate = new GetFieldTypeDelegate(_CommonDataLoader.GetFieldTypes);
            object toReturn = _CacheDataLoader.GetData(fieldTypeDelegate, null, CacheConstants.FIELD_TYPE, false);
            return toReturn as Collection<FieldTypeModelView>;
        }

        /// <summary>
        /// Get the FieldTypes as a dictionary
        /// </summary>
        /// <returns>Dictionary of FieldType states</returns>
        public virtual IDictionary<int,FieldTypeModelView> getFieldTypesDictionary()
        {
            return this.getFieldTypes().ToDictionary(x => x.FieldTypeID);
        }
        #endregion

        #region ProPricerFields

        /// <summary>
        /// Get all ProPricer Fields
        /// </summary>
        /// <param name="wsProjectMapType"></param>
        /// <returns>Collection of fields</returns>
        public virtual Collection<EnumTypeModelView> GetProPricerFields(bool isProjectMapType)
        {
            GetProPricerFieldsDelegate getProPricerFieldsDelegate = this._CommonDataLoader.GetProPricerFields;
            object toReturn;
            if (isProjectMapType)
            {
                toReturn = this._CacheDataLoader.GetData(getProPricerFieldsDelegate, new[] { (object)isProjectMapType }, CacheConstants.PROJMAP_PROPRICER_FIELDS, false);
            }
            else
            {
                toReturn = this._CacheDataLoader.GetData(getProPricerFieldsDelegate, new[] { (object)isProjectMapType }, CacheConstants.PROPRICER_FIELDS, false);
            }
            return toReturn as Collection<EnumTypeModelView>;
        }

        /// <summary>
        /// Get all ProPricer Fields as a dictionary
        /// </summary>
        /// <param name="wsProjectMapType"></param>
        /// <returns>DIctionary of fields</returns>
        public virtual IDictionary<int, EnumTypeModelView> GetProPricerFieldsDictionary(bool isProjectMapType)
        {
            return this.GetProPricerFields(isProjectMapType).ToDictionary(x => x.EnumTypeID);
        }
        #endregion

        #region RateTypes
        /// <summary>
        /// Get all Rate Types
        /// </summary>
        /// <returns>Collection of rate types</returns>
        public virtual Collection<RateTypeModelView> GetRateTypes()
        {
            GetRateTypesDelegate rateTypesDelegate = new GetRateTypesDelegate(_CommonDataLoader.GetRateTypes);
            object toReturn = _CacheDataLoader.GetData(rateTypesDelegate, null, CacheConstants.RATE_TYPES, false);
            return toReturn as Collection<RateTypeModelView>;
        }

        /// <summary>
        /// Get all Rate Types as a dictionary
        /// </summary>
        /// <returns>Dictionary of rate types</returns>
        public virtual IDictionary<int,RateTypeModelView> GetRateTypesDictionary()
        {
            return this.GetRateTypes().ToDictionary(x => x.RateTypeID);
        }
        #endregion

        #region OdcSpreadCurve
        /// <summary>
        /// get the spread cruve and set up cache
        /// </summary>
        /// <returns>Collection of spread curves</returns>
        public virtual Collection<OtherDirectCostSpreadCurveModelView> getOdcSpreadCurve()
        {
            GetODCSpreadCurveDelegate OdcSpreadCurveDelegate = new GetODCSpreadCurveDelegate(_CommonDataLoader.GetODCSpreadCurve);
            object toReturn = _CacheDataLoader.GetData(OdcSpreadCurveDelegate, null, CacheConstants.ODC_SPREAD_CURVE, false);
            return toReturn as Collection<OtherDirectCostSpreadCurveModelView>;
        }

        /// <summary>
        /// get the spread cruve as dictionary
        /// </summary>
        /// <returns>Dictionary of spread curves</returns>
        public virtual IDictionary<int,OtherDirectCostSpreadCurveModelView> getOdcSpreadCurveDictionary()
        {
            return this.getOdcSpreadCurve().ToDictionary(x => (int)x.SpreadCurveID);
        }
        #endregion

        #region Emails

        /// <summary>
        /// Gets the emails and sets up the cache
        /// </summary>
        /// <returns>Emails for the system</returns>
        public virtual ICollection<EmailModelDomain> GetEmails()
        {
            GetEmailDelegate emailDelegate = this._CommonDataLoader.GetEmails;
            object toReturn = this._CacheDataLoader.GetData(emailDelegate, null, CacheConstants.EMAILS, false);
            return toReturn as Collection<EmailModelDomain>;
        }

        /// <summary>
        /// Saves the system emails.
        /// </summary>
        /// <param name="emails">The emails.</param>
        public virtual void SaveSystemEmails(ICollection<EmailModelDomain> emails)
        {
            if (emails == null)
            {
                throw new ArgumentNullException(nameof(emails));
            }
            
            foreach (EmailModelDomain email in emails)
            {
                this._CommonDataLoader.SaveSystemEmail(email);
            }

            // Remove System email cache
            this._CacheDataLoader.Remove(CacheConstants.EMAILS);
        }

        #endregion Emails

        /// <summary>
        /// Gets all the report types from the DB
        /// </summary>
        /// <returns></returns>
        public virtual Collection<ReportDTO> getReports()
        {
            GetReportsDelegate reportsDelegate = new GetReportsDelegate(_CommonDataLoader.GetReports);
            object toReturn = _CacheDataLoader.GetData(reportsDelegate, null, CacheConstants.REPORTS, false);
            return toReturn as Collection<ReportDTO>;
        }

        /// <summary>
        /// Get the SortBy
        /// </summary>
        /// <returns>Collection of SortByModelView</returns>
        public virtual Collection<SortByModelView> getSortBy()
        {
            GetSortByDelegate SortByDelegate = new GetSortByDelegate(_CommonDataLoader.GetSortBy);
            object toReturn = _CacheDataLoader.GetData(SortByDelegate, null, CacheConstants.SORTBY, false);
            return toReturn as Collection<SortByModelView>;
        }

        /// <summary>
        /// Get all Sum Variable Resource Types
        /// </summary>
        /// <returns>Collection of SumVariableResourceTypeModelView</returns>
        public virtual Collection<SumVariableResourceTypeModelView> GetSumVariableResourceTypes()
        {
            GetSumVariableResourceTypesDelegate SumVarDelegate = new GetSumVariableResourceTypesDelegate(_CommonDataLoader.GetSumVariableResourceTypes);
            object toReturn = _CacheDataLoader.GetData(SumVarDelegate, null, CacheConstants.SUM_VARIABLE_RESOURCE_TYPE, false);
            return toReturn as Collection<SumVariableResourceTypeModelView>;
        }


        #region MSTTravelModes
        /// <summary>
        /// Gets all MSTTravelMode
        /// </summary>
        /// <returns>Collection of EnumTypeModelView</returns>
        public virtual Collection<EnumTypeModelView> GetMSTTravelModes()
        {
            GetEnumDelegate enumDelegate = new GetEnumDelegate(_CommonDataLoader.GetMSTTravelModes);
            object toReturn = _CacheDataLoader.GetData(enumDelegate, null, CacheConstants.HM_USAGE, false);
            return toReturn as Collection<EnumTypeModelView>;
        }

        /// <summary>
        /// Gets MSTTravelModes as a dictionary
        /// </summary>
        /// <returns>Dictionary of MSTTravelModes</returns>
        public virtual IDictionary<int, EnumTypeModelView> GetMSTTravelModesDictionary()
        {
            return this.GetMSTTravelModes().ToDictionary(x => x.EnumTypeID);
        }
        #endregion
    }
}
